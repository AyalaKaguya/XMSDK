using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XMSDK.Framework.Forms;

namespace XMSDK.Framework.Logger;

/// <summary>
/// 扩展方法，方便注册ListView日志提供者
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// 添加ListView日志提供者
    /// </summary>
    /// <param name="builder">日志构建器</param>
    /// <param name="loggerList">ColorfulLoggerList控件实例</param>
    /// <returns></returns>
    public static ILoggingBuilder AddListView(this ILoggingBuilder builder, LoggerList loggerList)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new ListViewLoggerProvider(loggerList));
        return builder;
    }
}