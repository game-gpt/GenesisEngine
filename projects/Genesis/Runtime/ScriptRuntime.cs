using Genesis.Attention;
using Genesis.Causal;
using Genesis.Rules;
using Genesis.Spacetime;
using Gnosis.Runtime.VM;
using Gnosis.Toolchain.ScriptCompiler;
using GnosisEcsWorld = Gnosis.ECS.World.World;

namespace Genesis.Runtime;

public sealed class ScriptRuntime
{
    #region 字段

    private VMState? _state;
    private VMInterpreter? _vm;
    private NativeFunctionRegistry _nativeRegistry;
    private ComponentTypeRegistry _componentRegistry;
    private GnosisEcsWorld? _world;
    private Compiler _compiler;
    private bool _initialized;
    private int _loadedModuleCount;
    private EmergentNativeBridge? _emergentBridge;

    #endregion

    #region 属性

    public VMState? State => _state;
    public VMInterpreter? VM => _vm;
    public bool IsInitialized => _initialized;
    public int LoadedModuleCount => _loadedModuleCount;
    public EmergentNativeBridge? EmergentBridge => _emergentBridge;

    #endregion

    #region 构造函数

    public ScriptRuntime()
    {
        _nativeRegistry = new NativeFunctionRegistry();
        _componentRegistry = new ComponentTypeRegistry();
        _compiler = new Compiler();
    }

    #endregion

    #region 初始化

    public void Initialize(GnosisEcsWorld world)
    {
        _world = world;
        _state = new VMState();
        _vm = new VMInterpreter(_state, _nativeRegistry, _componentRegistry, world);
        _loadedModuleCount = 0;

        _emergentBridge = new EmergentNativeBridge(_nativeRegistry);
        _emergentBridge.RegisterAll();

        _initialized = true;

        Console.WriteLine("[ScriptRuntime] 初始化完成 - 涌现叙事原生函数已注册");
    }

    #endregion

    #region 涌现叙事引擎绑定

    public void BindSpacetimeTree(SpacetimeTree spacetimeTree)
    {
        if (_emergentBridge is null)
        {
            throw new InvalidOperationException("ScriptRuntime 未初始化");
        }

        _emergentBridge.SpacetimeTree = spacetimeTree;
        Console.WriteLine("[ScriptRuntime] SpacetimeTree 已绑定到脚本运行时");
    }

    public void BindCausalGraph(CausalGraph causalGraph)
    {
        if (_emergentBridge is null)
        {
            throw new InvalidOperationException("ScriptRuntime 未初始化");
        }

        _emergentBridge.CausalGraph = causalGraph;
        Console.WriteLine("[ScriptRuntime] CausalGraph 已绑定到脚本运行时");
    }

    public void BindAttentionManager(SimpleAttentionManager attentionManager)
    {
        if (_emergentBridge is null)
        {
            throw new InvalidOperationException("ScriptRuntime 未初始化");
        }

        _emergentBridge.AttentionManager = attentionManager;
        Console.WriteLine("[ScriptRuntime] AttentionManager 已绑定到脚本运行时");
    }

    public void BindRuleEngine(SimpleRuleEngine ruleEngine)
    {
        if (_emergentBridge is null)
        {
            throw new InvalidOperationException("ScriptRuntime 未初始化");
        }

        _emergentBridge.RuleEngine = ruleEngine;
        Console.WriteLine("[ScriptRuntime] RuleEngine 已绑定到脚本运行时");
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

        var allSuccess = true;
        foreach (var file in scriptFiles)
        {
            var source = File.ReadAllText(file);
            var success = LoadScriptSource(source, file);
            if (!success)
            {
                allSuccess = false;
            }
        }

        return allSuccess;
    }

    public bool LoadScriptSource(string source, string filePath)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("ScriptRuntime 未初始化，请先调用 Initialize()");
        }

        var moduleName = Path.GetFileNameWithoutExtension(filePath);

        try
        {
            var macros = new ChannelMacros();
            var result = _compiler.CompileSource(
                source,
                filePath,
                ArchTarget.X64,
                macros);

            if (result.Bytecode.Length == 0)
            {
                Console.WriteLine($"[ScriptRuntime] 编译失败（空字节码）: {filePath}");
                return false;
            }

            var module = new RawBytecodeModule(moduleName, result.Bytecode);
            _state!.LoadModule(module);
            _loadedModuleCount++;

            Console.WriteLine($"[ScriptRuntime] 编译并加载成功: {moduleName} ({result.Bytecode.Length} 字节)");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScriptRuntime] 编译错误 [{filePath}]: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region 执行

    public void Run()
    {
        if (!_initialized || _vm is null)
        {
            return;
        }

        if (_loadedModuleCount == 0)
        {
            Console.WriteLine("[ScriptRuntime] 未加载任何模块，跳过 VM 执行");
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
        if (!_initialized || _world is null)
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
        _loadedModuleCount = 0;
        _emergentBridge = null;
        _state?.Reset();
    }

    #endregion

    #region 内部类型

    private sealed class RawBytecodeModule : IModule
    {
        public string Name { get; }
        public IReadOnlyList<byte> Instructions { get; }
        public IReadOnlyDictionary<string, int> NativeBindings { get; } = new Dictionary<string, int>();
        public int EntryPoint => 0;
        public IReadOnlyDictionary<string, object?> Constants { get; } = new Dictionary<string, object?>();
        public IReadOnlyList<string> ExportedSymbols { get; } = new List<string>();
        public IReadOnlyList<string> ImportedSymbols { get; } = new List<string>();
        public int Version => 1;
        public bool IsValid => Instructions.Count > 0;
        public IReadOnlyList<ModuleFunctionInfo> Functions { get; } = new List<ModuleFunctionInfo>();
        public IReadOnlyList<ModuleTypeInfo> Types { get; } = new List<ModuleTypeInfo>();

        public RawBytecodeModule(string name, byte[] bytecode)
        {
            Name = name;
            Instructions = bytecode;
        }
    }

    #endregion
}
