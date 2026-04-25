using Genesis.Causal;
using Genesis.Core;
using Genesis.Spacetime;
using Gnosis.Widget.Element;
using Gnosis.Widget.Layout;
using Gnosis.Widget.Render;

namespace Genesis.Editor.Panels;

public sealed class CausalInspectorPanel : VBox
{
    #region 字段

    private ISpacetimeNode? _selectedNode;

    #endregion

    #region 属性

    public ISpacetimeNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            _selectedNode = value;
            RebuildInspector();
        }
    }

    #endregion

    #region 构造函数

    public CausalInspectorPanel()
    {
        Background = new WidgetColor(0.15f, 0.15f, 0.17f);
        Width = 280;
        Padding = new EdgeInsets(10, 10, 10, 10);
        CrossAxisAlignment = CrossAxisAlignment.Stretch;

        BuildEmptyInspector();
    }

    #endregion

    #region 私有方法

    private void BuildEmptyInspector()
    {
        Children.Clear();

        AddChild(new TextWidget("Inspector") { FontSize = 14, Foreground = new WidgetColor(0.9f, 0.9f, 0.9f) });
        AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });
        AddChild(new TextWidget("  Select a node to inspect") { FontSize = 11, Foreground = new WidgetColor(0.5f, 0.5f, 0.5f), Margin = new EdgeInsets(3, 0, 0, 0) });
    }

    private void RebuildInspector()
    {
        Children.Clear();

        AddChild(new TextWidget("Inspector") { FontSize = 14, Foreground = new WidgetColor(0.9f, 0.9f, 0.9f) });
        AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });

        if (_selectedNode is null)
        {
            AddChild(new TextWidget("  Select a node to inspect") { FontSize = 11, Foreground = new WidgetColor(0.5f, 0.5f, 0.5f), Margin = new EdgeInsets(3, 0, 0, 0) });
            return;
        }

        BuildSpacetimeNodeInspector(_selectedNode);
    }

    private void BuildSpacetimeNodeInspector(ISpacetimeNode node)
    {
        AddChild(new TextWidget("Spacetime Node") { FontSize = 12, Foreground = new WidgetColor(0.8f, 0.8f, 0.8f), Margin = new EdgeInsets(3, 0, 0, 0) });
        AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });

        var dimColor = new WidgetColor(0.6f, 0.6f, 0.6f);

        AddChild(new TextWidget($"  Level: {node.Level}") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
        AddChild(new TextWidget($"  Spatial Hash: {node.SpatialHash:X16}") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
        AddChild(new TextWidget($"  History Hash: {node.HistoryHash:X16}") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
        AddChild(new TextWidget($"  Time Scale: {node.TimeScale}x") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
        AddChild(new TextWidget($"  Children: {node.Children.Count}") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });

        var stateColor = node.CollapseState switch
        {
            CollapseState.Collapsed => new WidgetColor(0.4f, 0.8f, 0.4f),
            _ => new WidgetColor(0.9f, 0.9f, 0.4f)
        };

        AddChild(new TextWidget($"  State: {node.CollapseState}") { FontSize = 11, Foreground = stateColor, Margin = new EdgeInsets(3, 0, 0, 0) });

        AddChild(new SeparatorWidget { Margin = new EdgeInsets(8, 0, 8, 0) });

        AddChild(new TextWidget("Bounds") { FontSize = 12, Foreground = new WidgetColor(0.8f, 0.8f, 0.8f), Margin = new EdgeInsets(3, 0, 0, 0) });
        AddChild(new TextWidget($"  Center: ({node.Bounds.Center.X:F1}, {node.Bounds.Center.Y:F1}, {node.Bounds.Center.Z:F1})") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
    }

    #endregion
}
