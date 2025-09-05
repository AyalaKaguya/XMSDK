using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace XMSDK.Framework.Config.Singleton
{
    public static class LocalFileBinder
    {
        // 存储已绑定的类型和文件路径的映射
        private static readonly Dictionary<Type, string> BoundTypes = new Dictionary<Type, string>();

        /// <summary>
        /// 扫描所有已加载的程序集，查找带有 BindLocalFileAttribute 的静态类，并绑定到对应的本地文件。
        /// 如果文件不存在，则创建一个默认的配置文件。
        /// 如果文件存在，则从文件中加载配置到静态类的属性中。
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void BindAll()
        {
            // 扫描所有已加载的程序集，而不仅仅是当前执行的程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies
                .SelectMany(assembly => 
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch
                    {
                        // 忽略无法加载的程序集
                        return Type.EmptyTypes;
                    }
                })
                .Where(t => t.IsClass && t.IsAbstract && t.IsSealed && t.GetCustomAttribute<BindLocalFileAttribute>() != null);
            
            foreach (var type in types)
            {
                Bind(type);
            }
        }

        /// <summary>
        /// 从文件读取配置并绑定到静态类的属性。
        /// 如果文件不存在，则创建一个默认的配置文件。
        /// 如果类型没有 BindLocalFileAttribute，则抛出异常。
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void Bind(Type type)
        {
            var attr = type.GetCustomAttribute<BindLocalFileAttribute>();
            if (attr == null)
            {
                throw new ArgumentException($"Type {type.Name} is not decorated with BindLocalFileAttribute.");
            }
            var filePath = Path.Combine(AppContext.BaseDirectory, attr.FileName);
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!type.IsClass || !type.IsAbstract || !type.IsSealed)
            {
                throw new ArgumentException("Only static classes can be bound.");
            }
            
            BoundTypes[type] = filePath;
            
            if (File.Exists(filePath))
            {
                LoadFromFile(type, filePath);
            }
            else
            {
                SaveToFile(type, filePath);
            }
        }
        
        public static void Bind<T>() where T : class
        {
            Bind(typeof(T));
        }

        /// <summary>
        /// 保存指定类型的配置到文件
        /// </summary>
        public static void Save(Type configType)
        {
            if (BoundTypes.TryGetValue(configType, out var filePath))
            {
                SaveToFile(configType, filePath);
            }
            else
            {
                throw new InvalidOperationException($"Type {configType.Name} is not bound to any file. Call BindAll() first.");
            }
        }

        /// <summary>
        /// 保存指定配置类到文件（通过 typeof() 调用）
        /// </summary>
        public static void Save<T>() where T : class
        {
            Save(typeof(T));
        }

        /// <summary>
        /// 保存所有已绑定的配置类型到对应文件
        /// </summary>
        public static void SaveAll()
        {
            foreach (var kvp in BoundTypes)
            {
                SaveToFile(kvp.Key, kvp.Value);
                Trace.WriteLine($"Saved {kvp.Key.Name} to {Path.GetFileName(kvp.Value)}");
            }
        }

        private static void LoadFromFile(Type type, string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var jsonDoc = JsonDocument.Parse(json);
                LoadPropertiesFromJson(type, jsonDoc.RootElement);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error loading {type.Name} from {filePath}: {ex.Message}");
                // 如果加载失败，保存当前默认值
                SaveToFile(type, filePath);
            }
        }

        private static void LoadPropertiesFromJson(Type type, JsonElement jsonElement)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
            
            foreach (var prop in properties)
            {
                if (prop.CanWrite && jsonElement.TryGetProperty(prop.Name, out var value))
                {
                    try
                    {
                        var convertedValue = ConvertJsonValue(value, prop.PropertyType);
                        prop.SetValue(null, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Error setting property {prop.Name}: {ex.Message}");
                    }
                }
            }

            // 处理嵌套静态类
            var nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);
            foreach (var nestedType in nestedTypes)
            {
                if (nestedType.IsClass && nestedType.IsAbstract && nestedType.IsSealed)
                {
                    if (jsonElement.TryGetProperty(nestedType.Name, out var nestedValue))
                    {
                        LoadPropertiesFromJson(nestedType, nestedValue);
                    }
                }
            }
        }

        private static object ConvertJsonValue(JsonElement value, Type targetType)
        {
            if (targetType == typeof(string))
                return value.GetString();
            if (targetType == typeof(int) || targetType == typeof(int?))
                return value.GetInt32();
            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return value.GetBoolean();
            if (targetType == typeof(double) || targetType == typeof(double?))
                return value.GetDouble();
            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                return value.GetDecimal();
            
            // 对于其他类型，尝试使用 JsonSerializer
            return JsonSerializer.Deserialize(value.GetRawText(), targetType);
        }

        private static void SaveToFile(Type type, string filePath)
        {
            try
            {
                var jsonObject = BuildJsonObject(type);
                var json = JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = null
                });
                
                // 确保目录存在
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(filePath, json);
                // 只在初始创建时显示消息，保存时不显示
                if (!File.Exists(filePath))
                {
                    Trace.WriteLine($"Created default config file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error saving {type.Name} to {filePath}: {ex.Message}");
            }
        }

        private static Dictionary<string, object> BuildJsonObject(Type type)
        {
            var result = new Dictionary<string, object>();
            
            // 获取所有静态属性
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var prop in properties)
            {
                if (prop.CanRead)
                {
                    var value = prop.GetValue(null);
                    result[prop.Name] = value;
                }
            }
            
            // 处理嵌套静态类
            var nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static);
            foreach (var nestedType in nestedTypes)
            {
                if (nestedType.IsClass && nestedType.IsAbstract && nestedType.IsSealed)
                {
                    result[nestedType.Name] = BuildJsonObject(nestedType);
                }
            }
            
            return result;
        }
    }
}