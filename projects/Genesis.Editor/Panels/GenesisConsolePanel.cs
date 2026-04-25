using Gnosis.Widget.Element;
using Gnosis.Widget.Layout;
using Gnosis.Widget.Render;

namespace Genesis.Editor.Panels;

public sealed class GenesisConsolePanel : VBox
{
    #region 日志条目

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public sealed class LogEntry
    {
        public required string Message { get; init; }
        public required LogLevel Level { get; init; }
        public required DateTime Timestamp { get; init; }
    }

    #endregion

    #region 字段

    private readonly List<LogEntry> _entries = [];
    private readonly List<TextWidget> _entryWidgets = [];

    #endregion

    #region 属性

    public int MaxEntries { get; set; } = 1000;

    public int VisibleLines { get; set; } = 8;

    #endregion

    #region 构造函数

    public GenesisConsolePanel()
    {
        Background = new WidgetColor(0.12f, 0.12f, 0.14f);
        Height = 150;
        Padding = new EdgeInsets(10, 10, 10, 10);
        CrossAxisAlignment = CrossAxisAlignment.Stretch;

        AddChild(new TextWidget("Console") { FontSize = 14, Foreground = new WidgetColor(0.9f, 0.9f, 0.9f) });
        AddChild(new SeparatorWidget { Margin = new EdgeInsets(5, 0, 5, 0) });
    }

    #endregion

    #region 公开方法

    public void LogInfo(string message)
    {
        AddEntry(message, LogLevel.Info);
    }

    public void LogWarning(string message)
    {
        AddEntry(message, LogLevel.Warning);
    }

    public void LogError(string message)
    {
        AddEntry(message, LogLevel.Error);
    }

    #endregion

    #region 私有方法

    private void AddEntry(string message, LogLevel level)
    {
        var entry = new LogEntry
        {
            Message = message,
            Level = level,
            Timestamp = DateTime.Now
        };

        _entries.Add(entry);

        if (_entries.Count > MaxEntries)
        {
            _entries.RemoveAt(0);
        }

        if (_entryWidgets.Count >= VisibleLines && _entryWidgets.Count > 0)
        {
            var oldest = _entryWidgets[0];
            Children.Remove(oldest);
            _entryWidgets.RemoveAt(0);
        }

        var color = level switch
        {
            LogLevel.Warning => new WidgetColor(0.9f, 0.75f, 0.20f),
            LogLevel.Error => new WidgetColor(0.9f, 0.30f, 0.30f),
            _ => new WidgetColor(0.4f, 0.8f, 0.4f)
        };

        var prefix = level switch
        {
            LogLevel.Warning => "[WARN] ",
            LogLevel.Error => "[ERROR] ",
            _ => "[INFO] "
        };

        var widget = new TextWidget($"{prefix}{message}") { FontSize = 11, Foreground = color, Margin = new EdgeInsets(3, 0, 0, 0) };
        AddChild(widget);
        _entryWidgets.Add(widget);
    }

    #endregion
}
