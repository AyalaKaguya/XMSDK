using System;

namespace XMSDK.Framework.License
{
    /// <summary>
    /// 授权信息对象
    /// </summary>
    public class LicenseInfo
    {
        public string MachineCode { get; set; }
        public string LicenseKey { get; set; }
        public DateTime? ExpireAt { get; set; }
        public string Signature { get; set; }
        public bool IsPermanent => !ExpireAt.HasValue;
        public bool IsExpired => ExpireAt.HasValue && DateTime.UtcNow > ExpireAt.Value;
    }
}