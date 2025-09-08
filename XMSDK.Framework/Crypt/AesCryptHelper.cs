using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XMSDK.Framework.Crypt;

/// <summary>
/// 简易的AES加密解密帮助类，
/// 密钥和偏移采用MD5加密后的前16位字符。
/// AES加密后为byte[]，可以使用Base64编码转换为字符串。
/// </summary>
public static class AesCryptHelper
{
    /// <summary>
    /// 加密数据
    /// </summary>
    /// <param name="text">待加密文本</param>
    /// <param name="key">加密密钥</param>
    /// <returns>加密后的文本</returns>
    public static string EncryptAes(this string text, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key.Md5Upper32().Substring(0, 16));
        return EncryptStringToBytes_Aes(text, keyBytes, keyBytes).EncodeBase64();
    }

    /// <summary>
    /// 解密数据
    /// </summary>
    /// <param name="text">待解密文本</param>
    /// <param name="key">解密密钥</param>
    /// <returns>解密后的文本</returns>
    public static string DecryptAes(this string text, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key.Md5Upper32().Substring(0, 16));
        var cipherText = text.Base64ToBytes();
        return DecryptStringFromBytes_Aes(cipherText, keyBytes, keyBytes);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="key"></param>
    /// <param name="iv"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
    {
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException(nameof(plainText));
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException(nameof(key));
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException(nameof(iv));
            
        byte[] encrypted;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                } // StreamWriter和CryptoStream在这里被关闭，确保数据被刷新到MemoryStream
                    
                encrypted = msEncrypt.ToArray(); // 现在获取完整的加密数据
            }
        }

        return encrypted;
    }

    public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
    {
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException(nameof(cipherText));
        if (key == null || key.Length <= 0)
            throw new ArgumentNullException(nameof(key));
        if (iv == null || iv.Length <= 0)
            throw new ArgumentNullException(nameof(iv));

        string plaintext = null;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var msDecrypt = new MemoryStream(cipherText))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var srDecrypt = new StreamReader(csDecrypt))
            {
                plaintext = srDecrypt.ReadToEnd();
            }
        }

        return plaintext;
    }
}