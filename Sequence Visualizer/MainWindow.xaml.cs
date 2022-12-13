﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Nest;
using Sequence_Visualizer.DataProvider.ElasticSearch;

namespace Sequence_Visualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string _startingFieldName = "enter";
    private string _endingFieldName = "exit";
    private string _type = "placeType";

    private List<List<string>> _typeGroups = new()
    {
        new List<string> {"PORT", "SUBPORT", "ANCHORAGE", "HYDROCARBON"},
        new List<string> {"BERTH"}
    };

    private Dictionary<string, System.Windows.Media.Color> _color = new()
    {
        {"PORT", System.Windows.Media.Color.FromArgb(127, Color.DarkRed.R,Color.DarkRed.G,Color.DarkRed.B)},
        {"SUBPORT", System.Windows.Media.Color.FromArgb(127, Color.IndianRed.R,Color.IndianRed.G,Color.IndianRed.B)},
        {"ANCHORAGE", System.Windows.Media.Color.FromArgb(127, Color.DarkBlue.R,Color.DarkBlue.G,Color.DarkBlue.B)},
        {"HYDROCARBON", System.Windows.Media.Color.FromArgb(127, Color.LightBlue.R,Color.LightBlue.G,Color.LightBlue.B)},
        {"BERTH", System.Windows.Media.Color.FromArgb(127, Color.Orange.R,Color.Orange.G,Color.Orange.B)},
    };

    public MainWindow()
    {
        InitializeComponent();

        GetDataButton.MouseUp += GetDataButtonOnMouseUp;
        GetDataButton.MouseLeftButtonUp += GetDataButtonOnMouseUp;
        GetDataButton.PreviewMouseLeftButtonUp += GetDataButtonOnMouseUp;

        InitDefault();
    }

    private void InitDefault()
    {
        EsClusterName.Text = "";
        EsIndexName.Text = "";
        VesselId.Text = "13692631";
        FromDateCheckBox.IsChecked = true;
        ToDateCheckBox.IsChecked = true;
    }

    private void GetDataButtonOnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (ValidateSettings())
        {
            return;
        }

        var request = new Request
        {
            VesselId = new KeyValuePair<string, string>("vesselId", VesselId.Text),
            Enter =  new KeyValuePair<string, DateTime?>(_startingFieldName ,FromDateCheckBox.IsChecked == true ?FromDate.SelectedDate: null) ,
            Exit = new KeyValuePair<string, DateTime?>(_startingFieldName ,ToDateCheckBox.IsChecked == true ? ToDate.SelectedDate : null)
        };
        
        var settings = new ConnectionSettings(new Uri(EsClusterName.Text)).DefaultIndex(EsIndexName.Text);

        var esProviderResult = new ElasticSearchDataProvider().Provide(settings, request);
        var items = esProviderResult.Select(x =>
            DisplayItem.Create(JsonDocument.Parse(x), _startingFieldName, _endingFieldName, _type, _color)).ToList();
        var groups = GroupItems(items);
        SequenceVisualizerViewerInstance.SetDisplayItems(groups);
    }

    private bool ValidateSettings()
    {
        return string.IsNullOrWhiteSpace(EsClusterName.Text) || string.IsNullOrWhiteSpace(EsIndexName.Text);
    }

    private List<List<DisplayItem>> GroupItems(List<DisplayItem> items)
    {
        var result = new List<List<DisplayItem>>();

        foreach (var typeGroup in _typeGroups)
        {
            var groupItems = new List<DisplayItem>();
            groupItems.AddRange(items.Where(x => typeGroup.Contains(x.Type)));
            items.RemoveAll(x => groupItems.Contains(x));
            result.Add(groupItems);
        }

        if (items.Any())
        {
            result.Add(items);
        }

        return result;
    }

    private List<List<DisplayItem>> SetDebugItems()
    {
        var _displayItems = new List<List<DisplayItem>>();
        var itemA = JsonDocument.Parse(
            "{\"id\": \"1\",\"enter\": \"2001-01-01T00:00:00\",\"exit\": \"2002-02-02T00:00:00\"}");
        var itemB = JsonDocument.Parse(
            "{\"id\": \"2\",\"enter\": \"2000-01-01T00:00:00\",\"exit\": \"2000-01-01T06:00:00\"}");
        var itemC = JsonDocument.Parse(
            "{\"id\": \"3\",\"enter\": \"2000-01-01T12:00:00\",\"exit\": \"2000-01-02T06:00:00\"}");
        var itemD = JsonDocument.Parse(
            "{\"id\": \"4\",\"enter\": \"2000-01-02T12:00:00\"}");

        _displayItems.Add(new List<DisplayItem>
        {
            DisplayItem.Create(itemA, _startingFieldName, _endingFieldName, _type, _color)
        });
        _displayItems.Add(new List<DisplayItem>
        {
            DisplayItem.Create(itemB, _startingFieldName, _endingFieldName, _type, _color),
            DisplayItem.Create(itemC, _startingFieldName, _endingFieldName, _type, _color),
            DisplayItem.Create(itemD, _startingFieldName, _endingFieldName, _type, _color)
        });

        return _displayItems;
    }
}