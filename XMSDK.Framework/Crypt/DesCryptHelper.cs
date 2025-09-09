using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XMSDK.Framework.Crypt;

/// <summary>
/// 简易的DES加密解密帮助类，
/// 密钥和偏移采用MD5加密后的前8位字符。
/// 注意：DES加密算法已被认为不够安全，建议使用更强的加密算法如AES。
/// 该类仅用于学习和兼容旧系统。
/// </summary>
public static class DesCryptHelper
{
    /// <summary>
    /// 加密数据
    /// </summary>
    /// <param name="text"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string EncryptDes(this string text, string key)
    {
        var des = new DESCryptoServiceProvider();
        var inputBytes = Encoding.Default.GetBytes(text);
        var keyBytes = Encoding.ASCII.GetBytes(key.Md5Upper32().Substring(0, 8));
        des.Key = des.IV = keyBytes;

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(inputBytes, 0, inputBytes.Length);
        cs.FlushFinalBlock();
        return BitConverter.ToString(ms.ToArray()).Replace("-", "");
    }

    /// <summary>
    /// 解密数据
    /// </summary>
    /// <param name="text"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string DecryptDes(this string text, string key)
    {
        var des = new DESCryptoServiceProvider();
        var inputBytes = new byte[text.Length / 2];
        for (var i = 0; i < inputBytes.Length; i++)
        {
            inputBytes[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
        }
        var keyBytes = Encoding.ASCII.GetBytes(key.Md5Upper32().Substring(0, 8));
        des.Key = des.IV = keyBytes;

        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
        cs.Write(inputBytes, 0, inputBytes.Length);
        cs.FlushFinalBlock();
        return Encoding.Default.GetString(ms.ToArray());
    }
}