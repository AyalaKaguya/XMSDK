using System;
using System.Text.RegularExpressions;
using XMSDK.Framework.Crypt;

namespace XMSDK.Framework.License
{
    /// <summary>
    /// 简单授权管理器：
    /// 1. 生成机器码（UUID格式，基于环境信息 MD5 后格式化）。
    /// 2. 通过 (machineCode|expireTicks|licenseKey|verifyKey) 计算签名（SHA256 -> 前32位大写）。
    /// 3. 授权码 = Base64( machineCode|expireTicks|signature ).
    /// 4. 验证时用相同规则重算签名并校验过期时间。
    /// </summary>
    public static class LicenseManager
    {
        private static readonly Regex MachineCodeRegex = new Regex("^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$", RegexOptions.Compiled);

        /// <summary>
        /// 生成稳定机器码（同一机器运行多次结果一致）。
        /// </summary>
        public static string GenerateMachineCode()
        {
            // 可按需扩展：加入硬盘序列号 / CPU 信息等。
            var raw = string.Join("|", Environment.MachineName, Environment.ProcessorCount, Environment.OSVersion.VersionString);
            var md5 = raw.Md5Upper32(); // 32位大写
            // 格式化为 UUID: 8-4-4-4-12
            return string.Format("{0}-{1}-{2}-{3}-{4}",
                md5.Substring(0, 8),
                md5.Substring(8, 4),
                md5.Substring(12, 4),
                md5.Substring(16, 4),
                md5.Substring(20, 12));
        }

        /// <summary>
        /// 生成授权码（Base64）。
        /// </summary>
        /// <param name="machineCode">机器码(UUID格式)</param>
        /// <param name="licenseKey">授权密钥（用户/渠道专属）</param>
        /// <param name="verifyKey">验证密钥（内置，不对外公开）</param>
        /// <param name="expireAt">过期时间(UTC，可空，空表示永久)</param>
        public static string GenerateLicense(string machineCode, string licenseKey, string verifyKey, DateTime? expireAt)
        {
            if (string.IsNullOrWhiteSpace(machineCode) || !MachineCodeRegex.IsMatch(machineCode))
                throw new ArgumentException("非法机器码", nameof(machineCode));
            if (string.IsNullOrWhiteSpace(licenseKey)) throw new ArgumentException("授权密钥不能为空", nameof(licenseKey));
            if (string.IsNullOrWhiteSpace(verifyKey)) throw new ArgumentException("验证密钥不能为空", nameof(verifyKey));

            var ticks = expireAt?.ToUniversalTime().Ticks ?? 0L;
            var sign = CalcSignature(machineCode, ticks, licenseKey, verifyKey);
            var plain = string.Join("|", machineCode, ticks, sign);
            return plain.EncodeBase64();
        }

        /// <summary>
        /// 校验授权码。
        /// </summary>
        /// <param name="licenseCode">Base64 授权码</param>
        /// <param name="licenseKey">授权密钥</param>
        /// <param name="verifyKey">验证密钥</param>
        /// <param name="info">成功时返回授权信息</param>
        /// <param name="error">失败原因</param>
        public static bool TryValidate(string licenseCode, string licenseKey, string verifyKey, out LicenseInfo info, out string error)
        {
            info = null;
            error = null;
            if (string.IsNullOrWhiteSpace(licenseCode)) { error = "授权码为空"; return false; }
            if (string.IsNullOrWhiteSpace(licenseKey)) { error = "授权密钥为空"; return false; }
            if (string.IsNullOrWhiteSpace(verifyKey)) { error = "验证密钥为空"; return false; }

            string decoded;
            try { decoded = licenseCode.DecodeBase64(); }
            catch { error = "授权码格式错误"; return false; }

            var parts = decoded.Split('|');
            if (parts.Length != 3) { error = "授权码字段数量不正确"; return false; }
            var machineCode = parts[0];
            if (!MachineCodeRegex.IsMatch(machineCode)) { error = "机器码格式错误"; return false; }
            if (!long.TryParse(parts[1], out var ticks)) { error = "过期时间字段错误"; return false; }
            var sign = parts[2];

            var expectSign = CalcSignature(machineCode, ticks, licenseKey, verifyKey);
            if (!string.Equals(sign, expectSign, StringComparison.Ordinal))
            {
                error = "签名不匹配";
                return false;
            }

            var expireAt = ticks == 0 ? (DateTime?)null : new DateTime(ticks, DateTimeKind.Utc);
            if (expireAt.HasValue && DateTime.UtcNow > expireAt.Value)
            {
                error = "授权已过期";
                return false;
            }

            info = new LicenseInfo
            {
                MachineCode = machineCode,
                LicenseKey = licenseKey,
                ExpireAt = expireAt,
                Signature = sign
            };
            return true;
        }

        private static string CalcSignature(string machineCode, long expireTicks, string licenseKey, string verifyKey)
        {
            var raw = string.Join("|", machineCode, expireTicks, licenseKey, verifyKey);
            // 使用 SHA256，再取前32位（16字节）大写，兼顾长度与碰撞概率
            return raw.Sha256Hash().Substring(0, 32).ToUpperInvariant();
        }
    }
}

