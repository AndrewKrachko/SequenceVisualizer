using System;
using System.Collections;
using System.Collections.Generic;

namespace Sequence_Visualizer.DataProvider.ElasticSearch;

public class Request
{
    public string Id { get; set; }

    public KeyValuePair<string, string>  VesselId { get; set; }

    public KeyValuePair<string, DateTime?>? Enter { get; set; }

    public KeyValuePair<string, DateTime?>? Exit { get; set; }
}