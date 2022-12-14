using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Sequence_Visualizer;

public partial class SequenceVisualizerViewer : UserControl
{
    private IDictionary<string, List<List<DisplayItem>>> _displayItems;
    private string _startingFieldName = "enter";
    private string _endingFieldName = "exit";
    private Dictionary<string, Color> _colorsDictionary;

    public SequenceVisualizerViewer()
    {
        InitializeComponent();
        _displayItems = new Dictionary<string, List<List<DisplayItem>>> {{string.Empty, new List<List<DisplayItem>>()}};
        SetDisplayItems(GetDebugItems());
    }

    public void SetStartingFieldName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _startingFieldName = name;
        }
    }

    public void SetEndingFieldName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _endingFieldName = name;
        }
    }

    public void SetDisplayItems(IDictionary<string, List<List<DisplayItem>>> items)
    {
        _displayItems = items;
        RemoveChildren();
        var displayRange = GetDisplayRange(_displayItems);
        var scale = string.IsNullOrEmpty(Scale.Text) ? 1.0 : double.Parse(Scale.Text);
        foreach (var displayItem in _displayItems)
        {
            ScrollableSequenceViewer.RowDefinitions.Add(new RowDefinition {Height = new GridLength(25)});
            var targetRowId = ScrollableSequenceViewer.RowDefinitions.Count - 1;

            var headerLabel = new Label()
            {
                Content = displayItem.Key,
                VerticalContentAlignment = VerticalAlignment.Center,
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Colors.CornflowerBlue),
                Width = (displayRange.Item2! - displayRange.Item1!).Value.TotalMinutes / scale + 1,
                HorizontalAlignment = HorizontalAlignment.Left,
                ToolTip = displayItem.Key,
                Background = new SolidColorBrush(Colors.CornflowerBlue),
            };
            Grid.SetRow(headerLabel, targetRowId);
            ScrollableSequenceViewer.Children.Add(headerLabel);

            displayItem.Value.ForEach(x =>
                AddRowGrid(x, displayRange, scale));
        }
    }

    private void RemoveChildren()
    {
        ScrollableSequenceViewer.Children.Clear();
        ScrollableSequenceViewer.RowDefinitions.Clear();
    }

    private static (DateTime?, DateTime?) GetDisplayRange(IDictionary<string, List<List<DisplayItem>>> items)
    {
        var dates = (
            items.SelectMany(z => z.Value.SelectMany(x => x.Select(y => y.Start))).ToList(),
            items.SelectMany(z => z.Value.SelectMany(x => x.Select(y => y.End)).ToList()));
        var range = (dates.Item1.Union(dates.Item2).Min(), dates.Item2.Union(dates.Item1).Max());
        range = (
            dates.Item1.Any(x => x == default)
                ? (range.Item1 ?? range.Item2 ?? (DateTime?) DateTime.Now)?.AddHours(-1)
                : range.Item1,
            dates.Item2.Any(x => x == default)
                ? (range.Item2 ?? range.Item1 ?? (DateTime?) DateTime.Now)?.AddHours(1)
                : range.Item2);

        return range;
    }

    private void AddRowGrid(IEnumerable<DisplayItem> items, (DateTime?, DateTime?) range, double scale = 1)
    {
        foreach (var itemList in GetOrderedItems(items))
        {
            ScrollableSequenceViewer.RowDefinitions.Add(new RowDefinition {Height = new GridLength(50)});
            var targetRowId = ScrollableSequenceViewer.RowDefinitions.Count - 1;
            foreach (var item in itemList)
            {
                var label = new Label()
                {
                    Content = item.Id,
                    BorderThickness = new Thickness(item.Start == default ? 0 : 1, 1, item.End == default ? 0 : 1, 1),
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    Margin = new Thickness(((item.Start ?? range.Item1!) - range.Item1!).Value.TotalMinutes / scale, 5,
                        0, 5),
                    Width = ((item.End ?? range.Item2!) - (item.Start ?? range.Item1!)).Value.TotalMinutes / scale,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    ToolTip = item.Content,
                    Background = new SolidColorBrush(item.Color),
                };
                Grid.SetRow(label, targetRowId);
                label.MouseLeftButtonUp += LabelOnMouseLeftUp;
                label.MouseRightButtonUp += LabelOnMouseRightUp;
                ScrollableSequenceViewer.Children.Add(label);
            }
        }
    }

    private void LabelOnMouseLeftUp(object sender, MouseButtonEventArgs e)
    {
        Clipboard.SetText(((Label) sender).Content.ToString()!);
    }

    private void LabelOnMouseRightUp(object sender, MouseButtonEventArgs e)
    {
        Clipboard.SetText(string.Concat(((Label) sender).ToolTip.ToString().Split("\n")));
    }

    private List<List<DisplayItem>> GetOrderedItems(IEnumerable<DisplayItem> items)
    {
        var orderedItems = items.OrderBy(x => x.Start).ToList();
        var splittedItems = new List<List<DisplayItem>> {new()};

        foreach (var item in orderedItems)
        {
            var isAdded = false;
            foreach (var splittedItemList in splittedItems)
            {
                if (!splittedItemList.Any(x => x.End == null || x.End > item.Start))
                {
                    isAdded = true;
                    splittedItemList.Add(item);
                    break;
                }
            }

            if (!isAdded)
            {
                splittedItems.Add(new List<DisplayItem> {item});
            }
        }

        return splittedItems;
    }

    private IDictionary<string, List<List<DisplayItem>>> GetDebugItems()
    {
        var items = new List<List<DisplayItem>>();

        var itemA = JsonDocument.Parse(
            "{\"id\": \"1\",\"enter\": \"2000-01-01T00:00:00\",\"exit\": \"2000-01-02T00:00:00\"}");
        var itemB = JsonDocument.Parse(
            "{\"id\": \"2\",\"enter\": \"2000-01-01T00:00:00\",\"exit\": \"2000-01-01T06:00:00\"}");
        var itemC = JsonDocument.Parse(
            "{\"id\": \"3\",\"enter\": \"2000-01-01T12:00:00\",\"exit\": \"2000-01-02T06:00:00\"}");
        var itemD = JsonDocument.Parse(
            "{\"id\": \"4\",\"enter\": \"2000-01-01T12:00:00\"}");

        items.Add(new List<DisplayItem>
        {
            DisplayItem.Create(itemA, string.Empty, _startingFieldName, _endingFieldName, string.Empty,
                new Dictionary<string, Color>())
        });
        items.Add(new List<DisplayItem>
        {
            DisplayItem.Create(itemB, string.Empty, _startingFieldName, _endingFieldName, string.Empty,
                new Dictionary<string, Color>()),
            DisplayItem.Create(itemC, string.Empty, _startingFieldName, _endingFieldName, string.Empty,
                new Dictionary<string, Color>()),
            DisplayItem.Create(itemD, string.Empty, _startingFieldName, _endingFieldName, string.Empty,
                new Dictionary<string, Color>())
        });

        return new Dictionary<string, List<List<DisplayItem>>> {{string.Empty, items}};
    }

    private static readonly Regex _regex = new Regex("[^0-9.]+");

    private static bool IsTextAllowed(string text)
    {
        return !_regex.IsMatch(text);
    }

    private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !IsTextAllowed(e.Text);
    }

    private void Scale_OnKeyUp(object sender, KeyEventArgs e)
    {
        SetDisplayItems(_displayItems);
    }

    private void UIElement_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        SetDisplayItems(_displayItems);
    }
}