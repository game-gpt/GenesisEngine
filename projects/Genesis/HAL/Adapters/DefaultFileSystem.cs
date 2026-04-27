namespace Genesis.HAL.Adapters;

/// <summary>
/// IFileSystem 的默认文件系统实现
/// 直接使用 System.IO
/// </summary>
public sealed class DefaultFileSystem : IFileSystem
{
    #region IFileSystem 实现

    /// <summary>
    /// 读取文件全部内容
    /// </summary>
    public byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    /// <summary>
    /// 读取文件全部文本
    /// </summary>
    public string ReadAllText(string path)
    {
        return File.ReadAllText(path);
    }

    /// <summary>
    /// 写入文件全部内容
    /// </summary>
    public void WriteAllBytes(string path, byte[] data)
    {
        File.WriteAllBytes(path, data);
    }

    /// <summary>
    /// 写入文件全部文本
    /// </summary>
    public void WriteAllText(string path, string text)
    {
        File.WriteAllText(path, text);
    }

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    /// 获取目录下的文件列表
    /// </summary>
    public IReadOnlyList<string> GetFiles(string path, string searchPattern = "*")
    {
        return Directory.GetFiles(path, searchPattern);
    }

    #endregion
}
