using System.Collections.Generic;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace Sequence_Visualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private string _startingFieldName = "enter";
    private string _endingFieldName = "exit";

    public MainWindow()
    {
        InitializeComponent();

        GetDataButton.MouseUp += GetDataButtonOnMouseUp;
        GetDataButton.MouseLeftButtonUp += GetDataButtonOnMouseUp;
        GetDataButton.PreviewMouseLeftButtonUp += GetDataButtonOnMouseUp;
    }

    private void GetDataButtonOnMouseUp(object sender, MouseButtonEventArgs e)
    {
        SequenceVisualizerViewerInstance.SetDisplayItems(SetDebugItems());
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
            DisplayItem.Create(itemA, _startingFieldName, _endingFieldName)
        });
        _displayItems.Add(new List<DisplayItem>
        {
            DisplayItem.Create(itemB, _startingFieldName, _endingFieldName),
            DisplayItem.Create(itemC, _startingFieldName, _endingFieldName),
            DisplayItem.Create(itemD, _startingFieldName, _endingFieldName)
        });

        return _displayItems;
    }
}