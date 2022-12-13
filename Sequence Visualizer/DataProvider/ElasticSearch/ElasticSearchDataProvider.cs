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

        var vesselId = new Field("VesselId");

        var boolQuery = new BoolQueryDescriptor<object>().Must(m =>
            m.Term(t => t.Field(request.VesselId.Key).Value(request.VesselId.Value)),
            m => request.Enter?.Value != null ? m.DateRange(r => r.Field(request.Enter?.Key).GreaterThanOrEquals(DateMath.Anchored((DateTime)request.Enter?.Value.Value))) : null,
            m => request.Exit?.Value != null ? m.DateRange(r => r.Field(request.Exit?.Key).LessThanOrEquals(DateMath.Anchored((DateTime)request.Exit?.Value))) : null
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