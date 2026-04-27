namespace Genesis.HAL;

/// <summary>
/// 文件系统抽象接口
/// 内核通过此接口访问文件，不直接依赖 System.IO
/// HAL 实现可提供沙箱化或平台特定的文件访问
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// 读取文件全部内容
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>文件内容字节数组</returns>
    byte[] ReadAllBytes(string path);

    /// <summary>
    /// 读取文件全部文本
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>文件文本内容</returns>
    string ReadAllText(string path);

    /// <summary>
    /// 写入文件全部内容
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="data">文件内容字节数组</param>
    void WriteAllBytes(string path, byte[] data);

    /// <summary>
    /// 写入文件全部文本
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="text">文件文本内容</param>
    void WriteAllText(string path, string text);

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns>是否存在</returns>
    bool FileExists(string path);

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <returns>是否存在</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// 获取目录下的文件列表
    /// </summary>
    /// <param name="path">目录路径</param>
    /// <param name="searchPattern">搜索模式</param>
    /// <returns>文件路径列表</returns>
    IReadOnlyList<string> GetFiles(string path, string searchPattern = "*");
}
