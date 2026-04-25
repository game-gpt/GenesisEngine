using Genesis.World;
using Gnosis.Widget.Element;
using Gnosis.Widget.Layout;
using Gnosis.Widget.Render;

namespace Genesis.Editor.Panels;

public sealed class WorldSettingsPanel
{
    #region 字段

    private readonly ulong _worldSeed;
    private readonly FlexLayout _root;

    #endregion

    #region 属性

    public FlexLayout Root => _root;

    #endregion

    #region 构造函数

    public WorldSettingsPanel(ulong worldSeed)
    {
        _worldSeed = worldSeed;

        _root = new FlexLayout
        {
            Direction = FlexDirection.Column,
            Height = 80,
            CrossAxisAlignment = CrossAxisAlignment.Stretch
        };
        _root.Margin = new EdgeInsets(10, 10, 10, 10);

        BuildPanel();
    }

    #endregion

    #region 私有方法

    private void BuildPanel()
    {
        var dimColor = new Color(0.6f, 0.6f, 0.6f);
        var brightColor = new Color(0.9f, 0.9f, 0.9f);

        _root.AddChild(new TextWidget("World Settings") { FontSize = 14, Foreground = brightColor });
        _root.AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });

        _root.AddChild(new TextWidget($"  Seed: {_worldSeed}") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
        _root.AddChild(new TextWidget($"  Biomes: {BiomeType.Forest}, {BiomeType.Desert}, {BiomeType.Ocean}...") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
    }

    #endregion
}
