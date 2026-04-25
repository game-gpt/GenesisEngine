using Genesis.Causal;
using Genesis.Core;
using Genesis.Spacetime;
using Gnosis.Widget.Element;
using Gnosis.Widget.Layout;
using Gnosis.Widget.Render;

namespace Genesis.Editor.Panels;

public sealed class CausalInspectorPanel
{
    #region 字段

    private readonly FlexLayout _root;
    private ISpacetimeNode? _selectedNode;

    #endregion

    #region 属性

    public FlexLayout Root => _root;

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
        _root = new FlexLayout
        {
            Direction = FlexDirection.Column,
            Width = 280,
            CrossAxisAlignment = CrossAxisAlignment.Stretch
        };
        _root.Margin = new EdgeInsets(10, 10, 10, 10);

        BuildEmptyInspector();
    }

    #endregion

    #region 私有方法

    private void BuildEmptyInspector()
    {
        _root.ClearChildren();

        _root.AddChild(new TextWidget("Inspector") { FontSize = 14, Foreground = new Color(0.9f, 0.9f, 0.9f) });
        _root.AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });
        _root.AddChild(new TextWidget("  Select a node to inspect") { FontSize = 11, Foreground = new Color(0.5f, 0.5f, 0.5f), Margin = new EdgeInsets(3, 0, 0, 0) });
    }

    private void RebuildInspector()
    {
        _root.ClearChildren();

        _root.AddChild(new TextWidget("Inspector") { FontSize = 14, Foreground = new Color(0.9f, 0.9f, 0.9f) });
        _root.AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });

        if (_selectedNode is null)
        {
            _root.AddChild(new TextWidget("  Select a node to inspect") { FontSize = 11, Foreground = new Color(0.5f, 0.5f, 0.5f), Margin = new EdgeInsets(3, 0, 0, 0) });
            return;
        }

        BuildSpacetimeNodeInspector(_selectedNode);
    }

    private void BuildSpacetimeNodeInspector(ISpacetimeNode node)
    {
        var dimColor = new Color(0.6f, 0.6f, 0.6f);

        _root.AddChild(new TextWidget("Spacetime Node") { FontSize = 12, Foreground = new Color(0.8f, 0.8f, 0.8f), Margin = new EdgeInsets(3, 0, 0, 0) });
        _root.AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });

        _root.AddChild(new TextWidget($"  Level: {node.Level}") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
        _root.AddChild(new TextWidget($"  Spatial Hash: {node.SpatialHash:X16}") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
        _root.AddChild(new TextWidget($"  History Hash: {node.HistoryHash:X16}") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
        _root.AddChild(new TextWidget($"  Time Scale: {node.TimeScale}x") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
        _root.AddChild(new TextWidget($"  Children: {node.Children.Count}") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });

        var stateColor = node.CollapseState switch
        {
            CollapseState.Collapsed => new Color(0.4f, 0.8f, 0.4f),
            _ => new Color(0.9f, 0.9f, 0.4f)
        };

        _root.AddChild(new TextWidget($"  State: {node.CollapseState}") { FontSize = 11, Foreground = stateColor, Margin = new EdgeInsets(3, 0, 0, 0) });

        _root.AddChild(new SeparatorWidget { Margin = new EdgeInsets(8, 0, 8, 0) });

        _root.AddChild(new TextWidget("Bounds") { FontSize = 12, Foreground = new Color(0.8f, 0.8f, 0.8f), Margin = new EdgeInsets(3, 0, 0, 0) });
        _root.AddChild(new TextWidget($"  Center: ({node.Bounds.Center.X:F1}, {node.Bounds.Center.Y:F1}, {node.Bounds.Center.Z:F1})") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
    }

    #endregion
}
