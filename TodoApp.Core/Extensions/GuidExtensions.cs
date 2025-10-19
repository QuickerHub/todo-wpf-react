using System;

namespace TodoApp.Core.Extensions;

/// <summary>
/// Guid 扩展方法
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    /// 将 Guid 转换为 Base62 字符串
    /// </summary>
    /// <param name="guid">要转换的 Guid</param>
    /// <returns>Base62 字符串</returns>
    public static string ToBase62(this Guid guid)
    {
        const string base62 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var bytes = guid.ToByteArray();
        var result = new System.Text.StringBuilder();
        
        // 将字节数组转换为 Base62
        var value = new System.Numerics.BigInteger(bytes);
        if (value == 0) return "0";
        
        while (value > 0)
        {
            result.Insert(0, base62[(int)(value % 62)]);
            value /= 62;
        }
        
        return result.ToString();
    }
}
