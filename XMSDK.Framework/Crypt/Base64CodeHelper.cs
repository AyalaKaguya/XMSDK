using System;
using System.Text;

namespace XMSDK.Framework.Crypt;

public static class Base64CodeHelper
{
    /// <summary>
    /// 将普通文本转换成Base64编码的文本
    /// </summary>
    /// <param name="plainText">普通文本</param>
    /// <returns></returns>
    public static string EncodeBase64(this string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    /// <summary>
    /// 将Base64编码的文本转换成普通文本
    /// </summary>
    /// <param name="base64EncodedData">Base64编码的文本</param>
    /// <returns></returns>
    public static string DecodeBase64(this string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }


    /// <summary>
    /// 将Byte[]转换成Base64编码文本
    /// </summary>
    /// <param name="binBuffer">Byte[]</param>
    /// <returns></returns>
    public static string EncodeBase64(this byte[] binBuffer)
        => Convert.ToBase64String(binBuffer);

    /// <summary>
    /// 将Base64编码文本转换成Byte[]
    /// </summary>
    /// <param name="base64">Base64编码文本</param>
    /// <returns></returns>
    public static byte[] Base64ToBytes(this string base64)
        => Convert.FromBase64String(base64);
}