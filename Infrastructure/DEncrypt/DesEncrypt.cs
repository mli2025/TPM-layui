using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.DEncrypt;

public static class DesEncrypt
{
    private static readonly byte[] DefaultKey = Encoding.UTF8.GetBytes("Dm2024@!");
    private static readonly byte[] DefaultIv = Encoding.UTF8.GetBytes("Dm2024@!");

    public static string Encrypt(string source)
    {
        if (string.IsNullOrEmpty(source)) return string.Empty;
        using var des = DES.Create();
        des.Key = DefaultKey;
        des.IV = DefaultIv;
        var input = Encoding.UTF8.GetBytes(source);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(input, 0, input.Length);
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipher)
    {
        if (string.IsNullOrEmpty(cipher)) return string.Empty;
        using var des = DES.Create();
        des.Key = DefaultKey;
        des.IV = DefaultIv;
        var input = Convert.FromBase64String(cipher);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
        {
            cs.Write(input, 0, input.Length);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    public static string Md5(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
