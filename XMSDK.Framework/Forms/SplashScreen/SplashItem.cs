using System;

namespace XMSDK.Framework.Forms.SplashScreen;

/// <summary>
/// 单个启动项。
/// </summary>
public class SplashItem
{
    public string Description { get; }
    public int Weight { get; }
    public Action<SplashContext> Action { get; }

    public SplashItem(string description, int weight, Action<SplashContext> action)
    {
        if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight));
        Description = description;
        Weight = weight;
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }
    
    public SplashItem(int weight, Action<SplashContext> action)
    {
        if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight));
        Description = string.Empty;
        Weight = weight;
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }
    public override string ToString() => Description;
}