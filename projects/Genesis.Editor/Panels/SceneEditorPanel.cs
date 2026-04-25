using Gnosis.Scene.Graph;
using Gnosis.Scene.Stream;
using Gnosis.Widget.Element;
using Gnosis.Widget.Layout;
using Gnosis.Widget.Render;

namespace Genesis.Editor.Panels;

public sealed class SceneEditorPanel : VBox
{
    #region 字段

    private readonly SceneGraph _sceneGraph;
    private readonly SceneStreamer _sceneStreamer;
    private readonly SceneTransitionController _transitionController;
    private ISceneNode? _selectedSceneNode;

    #endregion

    #region 属性

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

        Background = new WidgetColor(0.15f, 0.15f, 0.17f);
        Width = 220;
        Padding = new EdgeInsets(10, 10, 10, 10);
        CrossAxisAlignment = CrossAxisAlignment.Stretch;

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
        Children.Clear();

        AddChild(new TextWidget("Scene Hierarchy") { FontSize = 14, Foreground = new WidgetColor(0.9f, 0.9f, 0.9f) });
        AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });

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
            ? new WidgetColor(0.3f, 0.6f, 0.9f)
            : new WidgetColor(0.7f, 0.7f, 0.7f);

        var text = $"{indent}{icon} {node.Name}";

        AddChild(new TextWidget(text) { FontSize = 11, Foreground = color, Margin = new EdgeInsets(3, 0, 0, 0) });

        foreach (var child in node.Children)
        {
            BuildNodeTree(child, depth + 1);
        }
    }

    #endregion
}
