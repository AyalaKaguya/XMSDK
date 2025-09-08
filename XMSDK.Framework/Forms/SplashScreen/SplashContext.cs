using System;

namespace XMSDK.Framework.Forms.SplashScreen;

/// <summary>
/// 用于控制 SplashWindow 内容的上下文。
/// </summary>
public class SplashContext
{
    private readonly Action<string> _setDetailLabel;

    public SplashContext(Action<string> setDetailLabel)
    {
        _setDetailLabel = setDetailLabel;
    }

    /// <summary>
    /// 设置底部详细信息 label 文本。
    /// </summary>
    public void SetDetail(string? text)
    {
        _setDetailLabel?.Invoke(text ?? string.Empty);
    }
}