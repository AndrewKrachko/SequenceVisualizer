using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Nest;
using Newtonsoft.Json;

namespace Sequence_Visualizer.DataProvider.ElasticSearch;

public class ElasticSearchDataProvider
{
    public List<string> Provide(ConnectionSettings settings, Request request)
    {
        var client = new ElasticClient(settings);

        var idQuery =
            new QueryContainerDescriptor<object>().Terms(
                t => t.Field(request.VesselId.Key).Terms(request.VesselId.Value.Split(",").Select(x => x.Trim())));
        var enterWithinQuery = new QueryContainerDescriptor<object>().DateRange(r =>
        {
            var field = r.Field(request.Enter?.Key);
            if (request.Enter?.Value != null)
                field.GreaterThanOrEquals(DateMath.Anchored((DateTime) request.Enter?.Value.Value!));
            if (request.Exit?.Value != null)
                field.LessThanOrEquals(DateMath.Anchored((DateTime) request.Exit?.Value.Value!));

            return field;
        });
        var exitWithinQuery = new QueryContainerDescriptor<object>().DateRange(r =>
        {
            var field = r.Field(request.Exit?.Key);
            if (request.Enter?.Value != null)
                field.GreaterThanOrEquals(DateMath.Anchored((DateTime) request.Enter?.Value.Value!));
            if (request.Exit?.Value != null)
                field.LessThanOrEquals(DateMath.Anchored((DateTime) request.Exit?.Value.Value!));

            return field;
        });
        var openQuery = new QueryContainerDescriptor<object>().Bool(b => b
            .MustNot(
                n => n.Exists(e => e.Field(request.Exit?.Key))
            )
            .Must(
                m => m.DateRange(r =>
                {
                    var field = r.Field(request.Enter?.Key);
                    if (request.Exit?.Value != null)
                        field.LessThanOrEquals(DateMath.Anchored((DateTime) request.Exit?.Value.Value!));

                    return field;
                }))
        );
        var coveringQuery = new QueryContainerDescriptor<object>().Bool(b => b
            .Must(
                m => m.DateRange(r =>
                {
                    var field = r.Field(request.Enter?.Key);
                    if (request.Enter?.Value != null)
                        field.LessThanOrEquals(DateMath.Anchored((DateTime) request.Enter?.Value.Value!));
                    if (request.Exit?.Value != null)
                        field.GreaterThanOrEquals(DateMath.Anchored((DateTime) request.Exit?.Value.Value!));

                    return field;
                }))
        );

        var withinQuery = new QueryContainerDescriptor<object>().Bool(b => b
            .Should(
                _ => enterWithinQuery,
                _ => exitWithinQuery,
                _ => openQuery,
                _ => coveringQuery)
        );

        var boolQuery = new BoolQueryDescriptor<object>().Must(
            m => idQuery,
            m => withinQuery
        );

        var searchResponse = client.Search<object>(s => s
            .From(0)
            .Size(10000)
            .Query(q =>
                q.Bool(
                    b => boolQuery
                )
            ));

        var results = searchResponse.Documents;

        return results.Select(JsonConvert.SerializeObject).ToList();
    }
}