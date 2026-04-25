using Gnosis.Asset.Format;
using Xunit;

namespace Genesis.Toolchain.Tests;

public class ScriptFormatHandlerTests
{
    #region CanHandle 测试

    [Theory]
    [InlineData("test.script")]
    [InlineData("test.ggscript")]
    [InlineData("test.gnosis-script")]
    [InlineData("path/to/my_system.script")]
    public void CanHandle_ScriptExtensions_ReturnsTrue(string path)
    {
        var handler = new ScriptFormatHandler();
        Assert.True(handler.CanHandle(path));
    }

    [Theory]
    [InlineData("test.cs")]
    [InlineData("test.txt")]
    [InlineData("test.shader")]
    [InlineData("test.gon")]
    public void CanHandle_NonScriptExtensions_ReturnsFalse(string path)
    {
        var handler = new ScriptFormatHandler();
        Assert.False(handler.CanHandle(path));
    }

    [Fact]
    public void SupportedFormat_IsScript()
    {
        var handler = new ScriptFormatHandler();
        Assert.Equal(FormatType.Script, handler.SupportedFormat);
    }

    #endregion

    #region ValidateAsync 测试

    [Fact]
    public async Task ValidateAsync_NonExistentFile_ReturnsFalse()
    {
        var handler = new ScriptFormatHandler();
        var result = await handler.ValidateAsync("/nonexistent/test.script");
        Assert.False(result);
    }

    #endregion

    #region ScriptData 测试

    [Fact]
    public void ScriptData_DefaultValues_AreCorrect()
    {
        var data = new ScriptData();
        Assert.Equal(string.Empty, data.Name);
        Assert.Equal(string.Empty, data.Source);
        Assert.Equal(string.Empty, data.SystemName);
        Assert.Equal(string.Empty, data.ComponentName);
        Assert.Empty(data.Dependencies);
    }

    [Fact]
    public void ScriptData_WithValues_Preserved()
    {
        var data = new ScriptData
        {
            Name = "TestSystem",
            Source = "system TestSystem { }",
            SourcePath = "/test.script",
            SystemName = "TestSystem",
            Dependencies = ["config/items.gon"]
        };
        Assert.Equal("TestSystem", data.Name);
        Assert.Equal("system TestSystem { }", data.Source);
        Assert.Single(data.Dependencies);
    }

    #endregion
}

public class ShaderFormatHandlerTests
{
    #region CanHandle 测试

    [Theory]
    [InlineData("test.shader")]
    [InlineData("test.ggshader")]
    [InlineData("test.gnosis-shader")]
    [InlineData("path/to/belt.shader")]
    public void CanHandle_ShaderExtensions_ReturnsTrue(string path)
    {
        var handler = new ShaderFormatHandler();
        Assert.True(handler.CanHandle(path));
    }

    [Theory]
    [InlineData("test.cs")]
    [InlineData("test.script")]
    [InlineData("test.gon")]
    [InlineData("test.png")]
    public void CanHandle_NonShaderExtensions_ReturnsFalse(string path)
    {
        var handler = new ShaderFormatHandler();
        Assert.False(handler.CanHandle(path));
    }

    [Fact]
    public void SupportedFormat_IsShader()
    {
        var handler = new ShaderFormatHandler();
        Assert.Equal(FormatType.Shader, handler.SupportedFormat);
    }

    #endregion

    #region ValidateAsync 测试

    [Fact]
    public async Task ValidateAsync_NonExistentFile_ReturnsFalse()
    {
        var handler = new ShaderFormatHandler();
        var result = await handler.ValidateAsync("/nonexistent/test.shader");
        Assert.False(result);
    }

    #endregion

    #region ShaderData 测试

    [Fact]
    public void ShaderData_DefaultValues_AreCorrect()
    {
        var data = new ShaderData();
        Assert.Equal(string.Empty, data.Name);
        Assert.Equal(string.Empty, data.Source);
        Assert.Equal(string.Empty, data.VertexInputStruct);
        Assert.Equal(string.Empty, data.VertexOutputStruct);
        Assert.Empty(data.Dependencies);
    }

    [Fact]
    public void ShaderData_WithValues_Preserved()
    {
        var data = new ShaderData
        {
            Name = "BlockShader",
            Source = "struct BlockVertexInput { }",
            SourcePath = "/block.shader",
            VertexInputStruct = "BlockVertexInput",
            Dependencies = ["config/block_colors.gon"]
        };
        Assert.Equal("BlockShader", data.Name);
        Assert.Equal("BlockVertexInput", data.VertexInputStruct);
        Assert.Single(data.Dependencies);
    }

    #endregion
}

public class FormatRegistryIntegrationTests
{
    [Fact]
    public void RegisterScriptAndShaderHandlers_BothDiscoverable()
    {
        var registry = new FormatRegistry();
        registry.RegisterHandler(new ScriptFormatHandler());
        registry.RegisterHandler(new ShaderFormatHandler());

        Assert.True(registry.HasHandler(FormatType.Script));
        Assert.True(registry.HasHandler(FormatType.Shader));
    }

    [Fact]
    public void GetHandler_ByPath_ScriptFile_ReturnsScriptHandler()
    {
        var registry = new FormatRegistry();
        registry.RegisterHandler(new ScriptFormatHandler());
        registry.RegisterHandler(new ShaderFormatHandler());

        var handler = registry.GetHandler("systems/movement.script");
        Assert.NotNull(handler);
        Assert.Equal(FormatType.Script, handler.SupportedFormat);
    }

    [Fact]
    public void GetHandler_ByPath_ShaderFile_ReturnsShaderHandler()
    {
        var registry = new FormatRegistry();
        registry.RegisterHandler(new ScriptFormatHandler());
        registry.RegisterHandler(new ShaderFormatHandler());

        var handler = registry.GetHandler("shaders/belt.shader");
        Assert.NotNull(handler);
        Assert.Equal(FormatType.Shader, handler.SupportedFormat);
    }

    [Fact]
    public void GetHandler_ByPath_GonFile_NoMatchInScriptOrShader()
    {
        var registry = new FormatRegistry();
        registry.RegisterHandler(new ScriptFormatHandler());
        registry.RegisterHandler(new ShaderFormatHandler());

        var handler = registry.GetHandler("config/world.gon");
        Assert.Null(handler);
    }

    [Fact]
    public void RegisterAllGenesisHandlers_AllDiscoverable()
    {
        var registry = new FormatRegistry();
        registry.RegisterHandler(new ScriptFormatHandler());
        registry.RegisterHandler(new ShaderFormatHandler());
        registry.RegisterHandler(new ConfigFormatHandler());
        registry.RegisterHandler(new TextureFormatHandler());
        registry.RegisterHandler(new AudioFormatHandler());

        Assert.True(registry.HasHandler(FormatType.Script));
        Assert.True(registry.HasHandler(FormatType.Shader));
        Assert.True(registry.HasHandler(FormatType.Config));
        Assert.True(registry.HasHandler(FormatType.Texture));
        Assert.True(registry.HasHandler(FormatType.Audio));
    }
}
