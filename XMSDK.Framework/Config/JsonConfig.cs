using System;
using System.IO;
using Newtonsoft.Json;

namespace XMSDK.Framework.Config
{
    /// <summary>
    /// 通用 JSON 配置加载工具类，适配 Newtonsoft.Json
    /// </summary>
    /// <typeparam name="T">配置原型类型，需可序列化</typeparam>
    public class JsonConfig<T> where T : class, new()
    {
        /// <summary>
        /// 配置实例
        /// </summary>
        public T Instance { get; private set; }

        /// <summary>
        /// 配置文件路径
        /// </summary>
        public string ConfigPath { get; }

        /// <summary>
        /// Json 序列化设置
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; } = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// 构造函数，加载配置
        /// </summary>
        /// <param name="path">配置文件路径</param>
        /// <param name="settings">自定义序列化设置</param>
        public JsonConfig(string path, JsonSerializerSettings settings = null)
        {
            var type = typeof(T);
            // 检查是否有 [Serializable] 或 [DataClass] 特性
            var isSerializable = Attribute.IsDefined(type, typeof(SerializableAttribute));
            var isDataClass = Attribute.IsDefined(type, typeof(DataClassAttribute));
            if (!isSerializable && !isDataClass)
            {
                throw new InvalidOperationException($"类型 {type.FullName} 必须标记为 [Serializable] 或 [DataClass] 才能作为配置原型类使用。");
            }
            ConfigPath = path;
            if (settings != null)
                SerializerSettings = settings;
            Load();
        }

        /// <summary>
        /// 获取原型类的默认值实例（优先使用静态成员，其次属性注解）
        /// </summary>
        private static T GetDefaultInstance()
        {
            var type = typeof(T);
            // 优先查找 public static T GetDefault() 方法
            var getDefaultMethod = type.GetMethod("GetDefault", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (getDefaultMethod != null && getDefaultMethod.ReturnType == type)
            {
                return (T)getDefaultMethod.Invoke(null, null);
            }
            // 查找 public static T Default 属性
            var defaultProp = type.GetProperty("Default", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (defaultProp != null && defaultProp.PropertyType == type)
            {
                return (T)defaultProp.GetValue(null);
            }
            // 用属性注解赋值
            var instance = new T();
            var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttributes(typeof(DefaultValueAttribute), false);
                if (attr.Length > 0 && prop.CanWrite)
                {
                    var value = ((DefaultValueAttribute)attr[0]).Value;
                    // 类型转换，支持基础类型
                    if (value != null && prop.PropertyType != value.GetType())
                    {
                        try
                        {
                            value = Convert.ChangeType(value, prop.PropertyType);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    prop.SetValue(instance, value);
                }
            }
            return instance;
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void Load()
        {
            if (!File.Exists(ConfigPath))
            {
                Instance = GetDefaultInstance();
                Save();
                return;
            }
            var json = File.ReadAllText(ConfigPath);
            Instance = JsonConvert.DeserializeObject<T>(json, SerializerSettings) ?? GetDefaultInstance();
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public void Save()
        {
            var json = JsonConvert.SerializeObject(Instance, SerializerSettings);
            File.WriteAllText(ConfigPath, json);
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public void Reload()
        {
            Load();
        }
    }
}
