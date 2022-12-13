using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;

namespace Sequence_Visualizer;

public class DisplayItem
{
    public string Id { get; set; }

    public DateTime? Start { get; set; }

    public DateTime? End { get; set; }

    public string? Content { get; set; }

    public string? Type { get; set; }

    public System.Windows.Media.Color Color { get; set; }

    public static DisplayItem Create(JsonDocument document, string startFieldName, string endFieldName, string type, Dictionary<string, System.Windows.Media.Color> colors)
    {
        return new DisplayItem
        {
            Id = document.RootElement.GetProperty("id").GetString() ?? string.Empty,
            Start = document.RootElement.TryGetProperty(startFieldName, out var startProperty)
                ? startProperty.TryGetDateTime(out var startDateTime) ? startDateTime : null
                : null,
            End = document.RootElement.TryGetProperty(endFieldName, out var endProperty)
                ? !string.IsNullOrEmpty(endProperty.GetString()) && endProperty.TryGetDateTime(out var endDateTime) ? endDateTime : null
                : null,
            Type = document.RootElement.TryGetProperty(type, out var typeProperty)
                ? typeProperty.GetString()
                : string.Empty,
            Color = document.RootElement.TryGetProperty(type, out var colorProperty)
                ? colors[colorProperty.GetString()]
                : System.Windows.Media.Color.FromRgb(255, 255, 255),
            Content = SplitByLength(document.RootElement.ToString()),
        };
    }

    private static string SplitByLength(string text)
    {
        var result = string.Join("\n", text.Chunk(200).Select(x=> string.Concat(x)));

        return result;
    }
}