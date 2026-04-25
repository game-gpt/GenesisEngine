using System.Text;
using GnosisEcsWorld = Gnosis.ECS.World.World;
using Gnosis.Runtime.VM;

namespace Genesis.Runtime;

public sealed class ScriptRuntime
{
    #region 字段

    private VMState? _state;
    private VMInterpreter? _vm;
    private NativeFunctionRegistry _nativeRegistry;
    private ComponentTypeRegistry _componentRegistry;
    private GnosisEcsWorld? _world;
    private bool _initialized;

    #endregion

    #region 属性

    public VMState? State => _state;
    public VMInterpreter? VM => _vm;
    public bool IsInitialized => _initialized;

    #endregion

    #region 构造函数

    public ScriptRuntime()
    {
        _nativeRegistry = new NativeFunctionRegistry();
        _componentRegistry = new ComponentTypeRegistry();
    }

    #endregion

    #region 初始化

    public void Initialize(GnosisEcsWorld world)
    {
        _world = world;
        _state = new VMState();
        _vm = new VMInterpreter(_state, _nativeRegistry, _componentRegistry, world);
        _initialized = true;
    }

    #endregion

    #region 脚本加载

    public bool LoadScript(string scriptPath)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("ScriptRuntime 未初始化，请先调用 Initialize()");
        }

        if (!File.Exists(scriptPath))
        {
            Console.WriteLine($"[ScriptRuntime] 脚本文件不存在: {scriptPath}");
            return false;
        }

        var source = File.ReadAllText(scriptPath);
        return LoadScriptSource(source, scriptPath);
    }

    public bool LoadScriptDirectory(string directory)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("ScriptRuntime 未初始化，请先调用 Initialize()");
        }

        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"[ScriptRuntime] 脚本目录不存在: {directory}");
            return false;
        }

        var scriptFiles = Directory.GetFiles(directory, "*.script", SearchOption.AllDirectories)
            .OrderBy(f => f)
            .ToList();

        if (scriptFiles.Count == 0)
        {
            Console.WriteLine($"[ScriptRuntime] 目录中未找到 .script 文件: {directory}");
            return false;
        }

        Console.WriteLine($"[ScriptRuntime] 找到 {scriptFiles.Count} 个脚本文件:");
        foreach (var file in scriptFiles)
        {
            Console.WriteLine($"  - {Path.GetRelativePath(directory, file)}");
        }

        Console.WriteLine("[ScriptRuntime] 脚本编译需要 Gnosis.Toolchain（等待 03-Oak-Language-Frontend 团队完成 AST 类型定义）");
        Console.WriteLine("[ScriptRuntime] 当前以 ECS World 模式运行，脚本内容将在 Toolchain 就绪后启用");

        return true;
    }

    public bool LoadScriptSource(string source, string filePath)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("ScriptRuntime 未初始化，请先调用 Initialize()");
        }

        Console.WriteLine($"[ScriptRuntime] 脚本编译需要 Gnosis.Toolchain（等待 03-Oak-Language-Frontend 团队完成 AST 类型定义）");
        Console.WriteLine($"[ScriptRuntime] 跳过编译: {filePath}");

        return true;
    }

    #endregion

    #region 执行

    public void Run()
    {
        if (!_initialized || _vm is null)
        {
            return;
        }

        try
        {
            _vm.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScriptRuntime] VM 执行错误: {ex.Message}");
        }
    }

    public void Tick(float delta)
    {
        if (!_initialized || _vm is null || _world is null)
        {
            return;
        }

        try
        {
            _world.Update(delta);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScriptRuntime] World 更新错误: {ex.Message}");
        }
    }

    #endregion

    #region 关闭

    public void Shutdown()
    {
        _initialized = false;
        _state?.Reset();
    }

    #endregion
}
