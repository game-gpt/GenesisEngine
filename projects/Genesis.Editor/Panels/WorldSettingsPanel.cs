using Genesis.World;
using Gnosis.Widget.Element;
using Gnosis.Widget.Layout;
using Gnosis.Widget.Render;

namespace Genesis.Editor.Panels;

public sealed class WorldSettingsPanel : VBox
{
    #region 字段

    private readonly ulong _worldSeed;

    #endregion

    #region 构造函数

    public WorldSettingsPanel(ulong worldSeed)
    {
        _worldSeed = worldSeed;

        Background = new WidgetColor(0.15f, 0.15f, 0.17f);
        Height = 80;
        Padding = new EdgeInsets(10, 10, 10, 10);
        CrossAxisAlignment = CrossAxisAlignment.Stretch;

        BuildPanel();
    }

    #endregion

    #region 私有方法

    private void BuildPanel()
    {
        var dimColor = new WidgetColor(0.6f, 0.6f, 0.6f);
        var brightColor = new WidgetColor(0.9f, 0.9f, 0.9f);

        AddChild(new TextWidget("World Settings") { FontSize = 14, Foreground = brightColor });
        AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });

        AddChild(new TextWidget($"  Seed: {_worldSeed}") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
        AddChild(new TextWidget($"  Biomes: {BiomeType.Forest}, {BiomeType.Desert}, {BiomeType.Ocean}...") { FontSize = 11, Foreground = dimColor, Margin = new EdgeInsets(3, 0, 0, 0) });
    }

    #endregion
}
