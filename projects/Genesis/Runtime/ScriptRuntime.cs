using System.Buffers.Binary;
using System.Text;
using Gnosis.ECS.World;
using Gnosis.Runtime.VM;
using Gnosis.Toolchain.ScriptCompiler;

namespace Genesis.Runtime;

public sealed class ScriptRuntime
{
    #region 字段

    private VMState? _state;
    private VMInterpreter? _vm;
    private NativeFunctionRegistry _nativeRegistry;
    private ComponentTypeRegistry _componentRegistry;
    private World? _world;
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

    public void Initialize(World world)
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

        var compiler = new Compiler();
        var macros = new ChannelMacros();

        try
        {
            var result = compiler.Compile(scriptFiles, ArchTarget.X64, macros);

            if (result.Bytecode.Length == 0)
            {
                Console.WriteLine("[ScriptRuntime] 编译结果为空");
                return false;
            }

            var module = DeserializeModule(result.Bytecode);
            if (module is null)
            {
                Console.WriteLine("[ScriptRuntime] 字节码反序列化失败");
                return false;
            }

            _state!.LoadModule(module);
            Console.WriteLine($"[ScriptRuntime] 加载脚本模块: {module.Name}，指令数: {module.Instructions.Count}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScriptRuntime] 编译脚本失败: {ex.Message}");
            return false;
        }
    }

    public bool LoadScriptSource(string source, string filePath)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("ScriptRuntime 未初始化，请先调用 Initialize()");
        }

        var compiler = new Compiler();
        var macros = new ChannelMacros();

        try
        {
            var result = compiler.CompileSource(source, filePath, ArchTarget.X64, macros);

            if (result.Bytecode.Length == 0)
            {
                Console.WriteLine("[ScriptRuntime] 编译结果为空");
                return false;
            }

            var module = DeserializeModule(result.Bytecode);
            if (module is null)
            {
                Console.WriteLine("[ScriptRuntime] 字节码反序列化失败");
                return false;
            }

            _state!.LoadModule(module);
            Console.WriteLine($"[ScriptRuntime] 加载脚本模块: {module.Name}，指令数: {module.Instructions.Count}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScriptRuntime] 编译脚本失败: {ex.Message}");
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

    #region 字节码反序列化

    private static IModule? DeserializeModule(byte[] bytecode)
    {
        using var ms = new MemoryStream(bytecode);
        using var reader = new BinaryReader(ms);

        var magic = reader.ReadUInt32();
        if (magic != 0x474E4F53)
        {
            return null;
        }

        var version = reader.ReadUInt16();

        var nameLength = reader.ReadUInt16();
        var nameBytes = reader.ReadBytes(nameLength);
        var moduleName = Encoding.UTF8.GetString(nameBytes);

        var constantCount = reader.ReadInt32();
        var constants = new Dictionary<string, object?>();

        for (var i = 0; i < constantCount; i++)
        {
            var typeTag = reader.ReadByte();
            switch (typeTag)
            {
                case 0x01:
                    constants[i.ToString()] = reader.ReadString();
                    break;
                case 0x02:
                    constants[i.ToString()] = reader.ReadInt32();
                    break;
                case 0x03:
                    constants[i.ToString()] = reader.ReadSingle();
                    break;
                default:
                    constants[i.ToString()] = null;
                    break;
            }
        }

        var importCount = reader.ReadUInt16();
        var imports = new List<string>();
        for (var i = 0; i < importCount; i++)
        {
            imports.Add(reader.ReadString());
        }

        var exportCount = reader.ReadUInt16();
        var exports = new List<string>();
        for (var i = 0; i < exportCount; i++)
        {
            exports.Add(reader.ReadString());
        }

        var depCount = reader.ReadUInt16();
        for (var i = 0; i < depCount; i++)
        {
            reader.ReadString();
        }

        var instructionLength = reader.ReadInt32();
        var instructions = reader.ReadBytes(instructionLength);

        return new DeserializedModule(moduleName, instructions, constants, imports, exports);
    }

    #endregion

    #region 内部类

    private sealed class DeserializedModule : IModule
    {
        #region 属性

        public string Name { get; }
        public IReadOnlyList<byte> Instructions { get; }
        public IReadOnlyDictionary<string, int> NativeBindings { get; }
        public int EntryPoint => 0;
        public IReadOnlyDictionary<string, object?> Constants { get; }
        public IReadOnlyList<string> ExportedSymbols { get; }
        public IReadOnlyList<string> ImportedSymbols { get; }
        public int Version => 1;
        public bool IsValid => !string.IsNullOrEmpty(Name) && Instructions.Count > 0;
        public IReadOnlyList<ModuleFunctionInfo> Functions { get; }
        public IReadOnlyList<ModuleTypeInfo> Types { get; }

        #endregion

        #region 构造函数

        public DeserializedModule(
            string name,
            byte[] instructions,
            IReadOnlyDictionary<string, object?> constants,
            IReadOnlyList<string> imports,
            IReadOnlyList<string> exports)
        {
            Name = name;
            Instructions = instructions;
            Constants = constants;
            ImportedSymbols = imports;
            ExportedSymbols = exports;
            NativeBindings = new Dictionary<string, int>();
            Functions = [];
            Types = [];
        }

        #endregion
    }

    #endregion
}
