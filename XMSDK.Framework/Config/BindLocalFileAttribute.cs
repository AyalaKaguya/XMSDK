using System;

namespace XMSDK.Framework.Config
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BindLocalFileAttribute : Attribute
    {
        public string FileName { get; }
        public BindLocalFileAttribute(string fileName)
        {
            FileName = fileName;
        }
    }
}

