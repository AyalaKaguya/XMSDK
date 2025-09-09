using System;

namespace XMSDK.Framework.Forms.SplashScreen;

/// <summary>
/// 用于控制 SplashWindow 内容的上下文。
/// </summary>
public class SplashContext(Action<string> setDetailLabel)
{
    /// <summary>
    /// 设置底部详细信息 label 文本。
    /// </summary>
    public void SetDetail(string? text)
    {
        setDetailLabel?.Invoke(text ?? string.Empty);
    }
}