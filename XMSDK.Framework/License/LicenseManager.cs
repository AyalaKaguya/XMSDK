using System;
using System.Text.RegularExpressions;
using XMSDK.Framework.Crypt;

namespace XMSDK.Framework.License
{
    /// <summary>
    /// 简单授权管理器（新版逻辑）：
    /// - licenseKey：开发者私有，用于派生 verifyKey，不下发给客户端。
    /// - verifyKey：由 licenseKey 派生（DeriveVerifyKey），嵌入客户端，仅用于验证。
    /// - 签名规则：Sign = SHA256(machineCode|expireTicks|verifyKey).Substring(0,32).ToUpper。
    /// - 授权码：Base64(machineCode|expireTicks|Sign)。
    /// - 生成授权：需要 licenseKey 与 verifyKey（校验一致性）。
    /// - 验证授权：只需 verifyKey。
    /// </summary>
    public static class LicenseManager
    {
        private static readonly Regex MachineCodeRegex = new Regex("^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$", RegexOptions.Compiled);

        /// <summary>
        /// 由 licenseKey 派生 verifyKey（可内置到客户端）
        /// </summary>
        public static string DeriveVerifyKey(string licenseKey)
        {
            if (string.IsNullOrWhiteSpace(licenseKey)) throw new ArgumentException("licenseKey 不能为空", nameof(licenseKey));
            return licenseKey.Sha256Hash().Substring(0, 32).ToUpperInvariant();
        }

        /// <summary>
        /// 生成稳定机器码（同一机器运行多次结果一致）
        /// </summary>
        public static string GenerateMachineCode()
        {
            var raw = string.Join("|", Environment.MachineName, Environment.ProcessorCount, Environment.OSVersion.VersionString);
            var md5 = raw.Md5Upper32();
            return string.Format("{0}-{1}-{2}-{3}-{4}",
                md5.Substring(0, 8),
                md5.Substring(8, 4),
                md5.Substring(12, 4),
                md5.Substring(16, 4),
                md5.Substring(20, 12));
        }

        /// <summary>
        /// 生成授权码（Base64(machineCode|ticks|sign)）。
        /// 需要同时提供 licenseKey 与 verifyKey，用于校验 verifyKey 是否由该 licenseKey 派生。
        /// </summary>
        public static string GenerateLicense(string machineCode, string licenseKey, string verifyKey, DateTime? expireAt)
        {
            if (string.IsNullOrWhiteSpace(machineCode) || !MachineCodeRegex.IsMatch(machineCode))
                throw new ArgumentException("非法机器码", nameof(machineCode));
            if (string.IsNullOrWhiteSpace(licenseKey)) throw new ArgumentException("licenseKey 不能为空", nameof(licenseKey));
            if (string.IsNullOrWhiteSpace(verifyKey)) throw new ArgumentException("verifyKey 不能为空", nameof(verifyKey));

            // 校验 verifyKey 是否与 licenseKey 匹配
            var derived = DeriveVerifyKey(licenseKey);
            if (!string.Equals(derived, verifyKey, StringComparison.Ordinal))
                throw new ArgumentException("verifyKey 与 licenseKey 不匹配", nameof(verifyKey));

            var ticks = expireAt?.ToUniversalTime().Ticks ?? 0L;
            var sign = CalcSignature(machineCode, ticks, verifyKey);
            var plain = string.Join("|", machineCode, ticks, sign);
            return plain.EncodeBase64();
        }

        /// <summary>
        /// 校验授权码（仅需 verifyKey）。
        /// </summary>
        public static bool TryValidate(string licenseCode, string verifyKey, out LicenseInfo info, out string error)
        {
            info = null;
            error = null;
            if (string.IsNullOrWhiteSpace(licenseCode)) { error = "授权码为空"; return false; }
            if (string.IsNullOrWhiteSpace(verifyKey)) { error = "verifyKey 为空"; return false; }

            string decoded;
            try { decoded = licenseCode.DecodeBase64(); }
            catch { error = "授权码格式错误"; return false; }

            var parts = decoded.Split('|');
            if (parts.Length != 3) { error = "授权码字段数量不正确"; return false; }
            var machineCode = parts[0];
            if (!MachineCodeRegex.IsMatch(machineCode)) { error = "机器码格式错误"; return false; }
            if (!long.TryParse(parts[1], out var ticks)) { error = "过期时间字段错误"; return false; }
            var sign = parts[2];

            var expectSign = CalcSignature(machineCode, ticks, verifyKey);
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
                ExpireAt = expireAt,
                Signature = sign
            };
            return true;
        }

        private static string CalcSignature(string machineCode, long expireTicks, string verifyKey)
        {
            var raw = string.Join("|", machineCode, expireTicks, verifyKey);
            return raw.Sha256Hash().Substring(0, 32).ToUpperInvariant();
        }
    }
}
