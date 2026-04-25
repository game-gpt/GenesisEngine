using Gnosis.Scene.Graph;
using Gnosis.Scene.Stream;
using Gnosis.Widget.Element;
using Gnosis.Widget.Layout;
using Gnosis.Widget.Render;

namespace Genesis.Editor.Panels;

public sealed class SceneEditorPanel
{
    #region 字段

    private readonly SceneGraph _sceneGraph;
    private readonly SceneStreamer _sceneStreamer;
    private readonly SceneTransitionController _transitionController;
    private readonly FlexLayout _root;
    private ISceneNode? _selectedSceneNode;

    #endregion

    #region 属性

    public FlexLayout Root => _root;

    public SceneGraph SceneGraph => _sceneGraph;

    public SceneStreamer SceneStreamer => _sceneStreamer;

    public ISceneNode? SelectedSceneNode
    {
        get => _selectedSceneNode;
        set
        {
            _selectedSceneNode = value;
            RebuildHierarchy();
        }
    }

    #endregion

    #region 构造函数

    public SceneEditorPanel() : this(new SceneGraph("GenesisScene"))
    {
    }

    public SceneEditorPanel(SceneGraph sceneGraph)
    {
        _sceneGraph = sceneGraph;
        _sceneStreamer = new SceneStreamer();
        _transitionController = new SceneTransitionController();

        _root = new FlexLayout
        {
            Direction = FlexDirection.Column,
            Width = 220,
            CrossAxisAlignment = CrossAxisAlignment.Stretch
        };
        _root.Margin = new EdgeInsets(10, 10, 10, 10);

        BuildPanel();
    }

    #endregion

    #region 公开方法

    public ISceneNode CreateNode(string name, System.Numerics.Vector2 position)
    {
        var node = new SceneNode(name) { Position = position };
        _sceneGraph.Root.AddChild(node);
        RebuildHierarchy();
        return node;
    }

    public void RemoveNode(string name)
    {
        _sceneGraph.Root.RemoveChild(name);
        RebuildHierarchy();
    }

    public void LoadScene(string scenePath)
    {
        _sceneStreamer.LoadScene(scenePath);
    }

    public void UnloadScene(string scenePath)
    {
        _sceneStreamer.UnloadScene(scenePath);
    }

    public void Update(float deltaTime)
    {
        _transitionController.Update(deltaTime);
    }

    #endregion

    #region 私有方法

    private void BuildPanel()
    {
        _root.ClearChildren();

        _root.AddChild(new TextWidget("Scene Hierarchy") { FontSize = 14, Foreground = new Color(0.9f, 0.9f, 0.9f) });
        _root.AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });

        BuildNodeTree(_sceneGraph.Root, 0);
    }

    private void RebuildHierarchy()
    {
        BuildPanel();
    }

    private void BuildNodeTree(ISceneNode node, int depth)
    {
        var indent = new string(' ', depth * 2);
        var icon = node.Children.Count > 0 ? "▸" : "·";

        var isSelected = node == _selectedSceneNode;
        var color = isSelected
            ? new Color(0.3f, 0.6f, 0.9f)
            : new Color(0.7f, 0.7f, 0.7f);

        var text = $"{indent}{icon} {node.Name}";

        _root.AddChild(new TextWidget(text) { FontSize = 11, Foreground = color, Margin = new EdgeInsets(3, 0, 0, 0) });

        foreach (var child in node.Children)
        {
            BuildNodeTree(child, depth + 1);
        }
    }

    #endregion
}
