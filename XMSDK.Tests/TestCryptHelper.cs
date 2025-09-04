using NUnit.Framework;
using XMSDK.Framework.Crypt;

namespace XMSDK.Tests
{
    [TestFixture]
    public class TestCryptHelper
    {
        private const string OriginalText = "Hello, World!";
        private const string Key = "MySecretKey";

        [Test]
        public void TestMd5Hash()
        {
            var upper32 = OriginalText.Md5Upper32();
            var lower32 = OriginalText.Md5Lower32();
            var upper16 = OriginalText.Md5Upper16();
            var lower16 = OriginalText.Md5Lower16();
            Assert.AreEqual(32, upper32.Length);
            Assert.AreEqual(32, lower32.Length);
            Assert.AreEqual(16, upper16.Length);
            Assert.AreEqual(16, lower16.Length);
            Assert.IsTrue(upper32.ToUpper() == upper32);
            Assert.IsTrue(lower32.ToLower() == lower32);
            Assert.IsTrue(upper16.ToUpper() == upper16);
            Assert.IsTrue(lower16.ToLower() == lower16);
        }

        [Test]
        public void TestBase64EncodeDecode()
        {
            var encoded = OriginalText.EncodeBase64();
            var decoded = encoded.DecodeBase64();
            Assert.AreNotEqual(OriginalText, encoded);
            Assert.AreEqual(OriginalText, decoded);
        }

        [Test]
        public void TestDesEncryptDecrypt()
        {
            // DES密钥需8字节，取MD5前8位
            var desKey = Key.Md5Upper32().Substring(0, 8);
            var encrypted = OriginalText.EncryptDes(desKey);
            var decrypted = encrypted.DecryptDes(desKey);
            Assert.AreNotEqual(OriginalText, encrypted);
            Assert.AreEqual(OriginalText, decrypted);
        }

        [Test]
        public void TestAesEncryptDecrypt()
        {
            // AES密钥需16字节，取MD5前16位
            var aesKey = Key.Md5Upper32().Substring(0, 16);
            var encrypted = OriginalText.EncryptAes(aesKey);
            var decrypted = encrypted.DecryptAes(aesKey);
            Assert.AreNotEqual(OriginalText, encrypted);
            Assert.AreEqual(OriginalText, decrypted);
        }
    }
}

