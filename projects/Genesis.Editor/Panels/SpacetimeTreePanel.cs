using Genesis.Core;
using Genesis.Spacetime;
using Gnosis.Widget.Element;
using Gnosis.Widget.Layout;
using Gnosis.Widget.Render;

namespace Genesis.Editor.Panels;

public sealed class SpacetimeTreePanel
{
    #region 字段

    private readonly SpacetimeTree? _spacetimeTree;
    private readonly FlexLayout _root;
    private readonly List<TextWidget> _nodeWidgets = [];

    #endregion

    #region 属性

    public ISpacetimeNode? SelectedNode { get; private set; }

    public FlexLayout Root => _root;

    #endregion

    #region 构造函数

    public SpacetimeTreePanel(SpacetimeTree? spacetimeTree)
    {
        _spacetimeTree = spacetimeTree;

        _root = new FlexLayout
        {
            Direction = FlexDirection.Column,
            Width = 220,
            CrossAxisAlignment = CrossAxisAlignment.Stretch
        };
        _root.Margin = new EdgeInsets(10, 10, 10, 10);

        _root.AddChild(new TextWidget("Spacetime Tree") { FontSize = 14, Foreground = new Color(0.9f, 0.9f, 0.9f) });
        _root.AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });

        if (_spacetimeTree?.Root is not null)
        {
            BuildTreeNodes(_spacetimeTree.Root, 0);
        }
        else
        {
            _root.AddChild(new TextWidget("  (empty)") { FontSize = 11, Foreground = new Color(0.5f, 0.5f, 0.5f), Margin = new EdgeInsets(3, 0, 0, 0) });
        }
    }

    #endregion

    #region 公开方法

    public void Update()
    {
        if (_spacetimeTree?.Root is null)
        {
            return;
        }

        foreach (var widget in _nodeWidgets)
        {
            _root.RemoveChild(widget);
        }

        _nodeWidgets.Clear();
        BuildTreeNodes(_spacetimeTree.Root, 0);
    }

    public void SelectNode(ISpacetimeNode? node)
    {
        SelectedNode = node;
    }

    #endregion

    #region 私有方法

    private void BuildTreeNodes(ISpacetimeNode node, int depth)
    {
        var indent = new string(' ', depth * 2);
        var stateIcon = node.CollapseState == CollapseState.Collapsed ? "●" : "○";
        var levelTag = node.Level.ToString();

        var text = $"{indent}{stateIcon} [{levelTag}] Hash:{node.SpatialHash:X8}";

        var color = node.CollapseState switch
        {
            CollapseState.Collapsed => new Color(0.4f, 0.8f, 0.4f),
            _ => new Color(0.7f, 0.7f, 0.7f)
        };

        var widget = new TextWidget(text) { FontSize = 11, Foreground = color, Margin = new EdgeInsets(3, 0, 0, 0) };
        _root.AddChild(widget);
        _nodeWidgets.Add(widget);

        foreach (var child in node.Children)
        {
            BuildTreeNodes(child, depth + 1);
        }
    }

    #endregion
}
