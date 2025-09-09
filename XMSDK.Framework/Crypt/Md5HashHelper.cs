using System.Security.Cryptography;
using System.Text;

namespace XMSDK.Framework.Crypt;

public static class Md5HashHelper
{
    /// <summary>
    /// 32位大写
    /// </summary>
    /// <returns></returns>
    public static string Md5Upper32(this string s)
        => Md5Hash(s, "X2");

    /// <summary>
    /// 32位小写
    /// </summary>
    /// <returns></returns>
    public static string Md5Lower32(this string s)
        => Md5Hash(s, "x2");

    /// <summary>
    /// 16位大写
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string Md5Upper16(this string s)
        => Md5Hash(s, "X2").Substring(8, 16);

    /// <summary>
    /// 16位小写
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string Md5Lower16(this string s)
        => Md5Hash(s, "x2").Substring(8, 16);
        
    /// <summary>
    /// 计算MD5哈希值
    /// </summary>
    /// <param name="s"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    private static string Md5Hash(string s, string format)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(s);
        var hashBytes = md5.ComputeHash(inputBytes);
        var sb = new StringBuilder();
        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString(format));
        }

        return sb.ToString();
    }

}