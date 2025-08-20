using System;
using System.Diagnostics;
using System.Threading.Tasks;
using XMSDK.Framework.Communication;
using XMSDK.Framework.Logger;
using XMSDK.Framework.Config;
using XMSDK.Framework.Crypt;

namespace ConsoleApplication1
{
    [BindLocalFile("test-config.json")]
    public static class TestConfig
    {
        public static string DatabaseUrl { get; set; } = "localhost:1433";
        public static int MaxConnections { get; set; } = 100;
        public static bool EnableLogging { get; set; } = true;
        public static int ServerPort { get; set; } = 8088;

        public static class Advanced
        {
            public static int Timeout { get; set; } = 30;
            public static string Protocol { get; set; } = "TCP";
        }
    }

    internal static class Program
    {
        public static void Main(string[] args)
        {
            // 测试配置文件
            Console.WriteLine("=== 测试配置文件绑定 ===");
            LocalFileBinder.BindAll();

            Console.WriteLine("初始配置值");
            Console.WriteLine($"DatabaseUrl: {TestConfig.DatabaseUrl}");
            Console.WriteLine($"MaxConnections: {TestConfig.MaxConnections}");
            Console.WriteLine($"EnableLogging: {TestConfig.EnableLogging}");
            Console.WriteLine($"Advanced.Timeout: {TestConfig.Advanced.Timeout}");
            Console.WriteLine($"Advanced.Protocol: {TestConfig.Advanced.Protocol}");

            Console.WriteLine("\n修改配置值");
            TestConfig.MaxConnections = 700;
            TestConfig.DatabaseUrl = "server:5432";
            TestConfig.Advanced.Timeout = 60;

            Console.WriteLine($"修改后 MaxConnections: {TestConfig.MaxConnections}");
            Console.WriteLine($"修改后 DatabaseUrl: {TestConfig.DatabaseUrl}");
            Console.WriteLine($"修改后 Advanced.Timeout: {TestConfig.Advanced.Timeout}");

            Console.WriteLine("\n保存配置到文件");
            LocalFileBinder.Save(typeof(TestConfig));


            // 测试日志记录
            Console.WriteLine("\n=== 测试 Trace 和 Debug 输出 ===");
            new TraceMessageReceiver()
                // .AddToDebug() // 只输出到Debug
                .AddToTrace() // 同时输出到Trace和Debug
                .AddProcesser(new LogToConsoleProcesser())
                .AddProcesser(new LogToFolderProcesser(@"D:\logs"));

            Trace.WriteLine("This is a test message for Trace.");
            Debug.WriteLine("This is a test message for Debug."); // Release模式下不会输出
            
            
            TransmissionTest.TestTransmissionCreate();
            TransmissionTest.TestSignalTransmission();
            TransmissionTest.TestCommandTransmission();
            
            
            Console.WriteLine("\n=== 测试加密 ===");
            
            const string originalText = "Hello, World!";
            
            Console.WriteLine(originalText);
            Console.WriteLine("MD5 哈希值：");
            Console.WriteLine(originalText.Md5Upper32());
            Console.WriteLine(originalText.Md5Lower32());
            Console.WriteLine(originalText.Md5Upper16());
            Console.WriteLine(originalText.Md5Lower16());
            
            Console.WriteLine("Base64 编码：");
            Console.WriteLine(originalText.EncodeBase64());
            Console.WriteLine(originalText.EncodeBase64().DecodeBase64());
            
            Console.WriteLine("DES 加密：");
            // 注意：DES加密需要8字节的密钥，这里使用MD5哈希值的前8个字符作为密钥
            const string key = "MySecretKey";
            Console.WriteLine(originalText.EncryptDes(key));
            Console.WriteLine(originalText.EncryptDes(key).DecryptDes(key));
            
            Console.WriteLine("AES 加密：");
            // 注意：AES加密需要16字节的密钥，这里使用MD5哈希值的前16个字符作为密钥
            Console.WriteLine(originalText.EncryptAes(key));
            Console.WriteLine(originalText.EncryptAes(key).DecryptAes(key));
            
            Console.WriteLine("=== 测试完成 ===");
        }
        
        
    }
}