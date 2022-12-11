using System;
using System.Text.Json;

namespace Sequence_Visualizer;

public class DisplayItem
{
    public string Id { get; set; }

    public DateTime? Start { get; set; }

    public DateTime? End { get; set; }

    public string? Content { get; set; }

    public static DisplayItem Create(JsonDocument document, string startFieldName, string endFieldName)
    {
        return new DisplayItem
        {
            Id = document.RootElement.GetProperty("id").GetString() ?? string.Empty,
            Start = document.RootElement.TryGetProperty(startFieldName, out var startProperty)
                ? startProperty.TryGetDateTime(out var startDateTime) ? startDateTime : null
                : null,
            End = document.RootElement.TryGetProperty(endFieldName, out var endProperty)
                ? endProperty.TryGetDateTime(out var endDateTime) ? endDateTime : null
                : null,
            Content = document.RootElement.ToString(),
        };
    }
}