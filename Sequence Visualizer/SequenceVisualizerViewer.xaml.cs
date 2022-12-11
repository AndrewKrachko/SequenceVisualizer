using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Sequence_Visualizer;

public partial class SequenceVisualizerViewer : UserControl
{
    private List<List<DisplayItem>> _displayItems;
    private string _startingFieldName = "enter";
    private string _endingFieldName = "exit";
    private Dictionary<string, Color> _colorsDictionary; 

    public SequenceVisualizerViewer()
    {
        InitializeComponent();
        _displayItems = new List<List<DisplayItem>>();
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

    public void SetDisplayItems(List<List<DisplayItem>> items)
    {
        _displayItems = items;
        RemoveChildren();
        var displayRange = GetDisplayRange(_displayItems);
        _displayItems.ForEach(x => AddRowGrid(x, displayRange));
    }

    private void RemoveChildren()
    {
        ScrollableSequenceViewer.Children.Clear();
    }

    private static (DateTime?, DateTime?) GetDisplayRange(List<List<DisplayItem>> items)
    {
        var dates = (
            items.SelectMany(x => x.Select(y => y.Start)).ToList(),
            items.SelectMany(x => x.Select(y => y.End)).ToList());
        var range = (dates.Item1.Union(dates.Item2).Min(), dates.Item2.Union(dates.Item1).Max());
        range = (
            dates.Item1.Any(x => x == default)
                ? (range.Item1 ?? range.Item2 ?? (DateTime?)DateTime.Now)?.AddHours(-1)
                : range.Item1,
            dates.Item2.Any(x => x == default)
                ? (range.Item2 ?? range.Item1 ?? (DateTime?)DateTime.Now)?.AddHours(1)
                : range.Item2);

        return range;
    }

    private void AddRowGrid(List<DisplayItem> items, (DateTime?, DateTime?) range)
    {
        foreach (var itemList in GetOrderedItems(items))
        {
            ScrollableSequenceViewer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            var targetRowId = ScrollableSequenceViewer.RowDefinitions.Count - 1;
            foreach (var item in itemList)
            {
                var label = new Label()
                {
                    Content = item.Id,
                    BorderThickness = new Thickness(item.Start == default ? 0 : 1, 1, item.End == default ? 0 : 1, 1),
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    Margin = new Thickness(((item.Start ?? range.Item1!) - range.Item1!).Value.TotalMinutes, 5, 0, 5),
                    Width = ((item.End ?? range.Item2!) - (item.Start ?? range.Item1!)).Value.TotalMinutes,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    ToolTip = item.Content,
                };
                Grid.SetRow(label, targetRowId);
                ScrollableSequenceViewer.Children.Add(label);
            }
        }
    }

    private List<List<DisplayItem>> GetOrderedItems(IEnumerable<DisplayItem> items)
    {
        var orderedItems = items.OrderBy(x => x.Start).ToList();
        var splittedItems = new List<List<DisplayItem>> { new() };

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
                splittedItems.Add(new List<DisplayItem> { item });
            }
        }

        return splittedItems;
    }

    private List<List<DisplayItem>> GetDebugItems()
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
            DisplayItem.Create(itemA, _startingFieldName, _endingFieldName)
        });
        items.Add(new List<DisplayItem>
        {
            DisplayItem.Create(itemB, _startingFieldName, _endingFieldName),
            DisplayItem.Create(itemC, _startingFieldName, _endingFieldName),
            DisplayItem.Create(itemD, _startingFieldName, _endingFieldName)
        });

        return items;
    }
}