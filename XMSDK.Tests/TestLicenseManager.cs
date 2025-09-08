using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using XMSDK.Framework.License;

namespace XMSDK.Tests
{
    [TestFixture]
    public class TestLicenseManager
    {
        private const string LicenseKey = "USER-KEY-001"; // 模拟发给用户/渠道的授权密钥
        private const string VerifyKey = "INTERNAL-VERIFY-SECRET"; // 内置验证密钥，不对外暴露
        private static readonly Regex MachineCodeRegex = new Regex("^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$", RegexOptions.Compiled);

        [Test]
        public void TestGenerateMachineCode_FormatAndStability()
        {
            var mc1 = LicenseManager.GenerateMachineCode();
            var mc2 = LicenseManager.GenerateMachineCode();
            Assert.IsTrue(MachineCodeRegex.IsMatch(mc1));
            Assert.AreEqual(mc1, mc2, "同一环境应生成稳定机器码");
        }

        [Test]
        public void TestGenerateAndValidate_Permanent()
        {
            var machineCode = LicenseManager.GenerateMachineCode();
            var licenseCode = LicenseManager.GenerateLicense(machineCode, LicenseKey, VerifyKey, null);
            var ok = LicenseManager.TryValidate(licenseCode, LicenseKey, VerifyKey, out var info, out var error);
            Assert.IsTrue(ok, error);
            Assert.NotNull(info);
            Assert.IsTrue(info.IsPermanent);
            Assert.IsFalse(info.IsExpired);
            Assert.AreEqual(machineCode, info.MachineCode);
        }

        [Test]
        public void TestGenerateAndValidate_Expired()
        {
            var machineCode = LicenseManager.GenerateMachineCode();
            var expiredAt = DateTime.UtcNow.AddSeconds(-5);
            var licenseCode = LicenseManager.GenerateLicense(machineCode, LicenseKey, VerifyKey, expiredAt);
            var ok = LicenseManager.TryValidate(licenseCode, LicenseKey, VerifyKey, out var info, out var error);
            Assert.IsFalse(ok, "应该过期");
            Assert.IsNull(info);
            Assert.AreEqual("授权已过期", error);
        }

        [Test]
        public void TestValidate_TamperedSignature()
        {
            var machineCode = LicenseManager.GenerateMachineCode();
            var licenseCode = LicenseManager.GenerateLicense(machineCode, LicenseKey, VerifyKey, DateTime.UtcNow.AddMinutes(5));
            // 篡改：解码后修改最后一个字符再编码
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(licenseCode));
            var parts = decoded.Split('|');
            Assert.AreEqual(3, parts.Length);
            // 修改签名首字符（A->B 或 其他->A）
            var sign = parts[2];
            var newFirst = sign[0] == 'A' ? 'B' : 'A';
            parts[2] = newFirst + sign.Substring(1);
            var tampered = string.Join("|", parts);
            var tamperedCode = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tampered));
            var ok = LicenseManager.TryValidate(tamperedCode, LicenseKey, VerifyKey, out var info, out var error);
            Assert.IsFalse(ok);
            Assert.IsNull(info);
            Assert.AreEqual("签名不匹配", error);
        }
    }
}

