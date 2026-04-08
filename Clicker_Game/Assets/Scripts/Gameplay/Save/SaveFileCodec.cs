using System;
using System.Security.Cryptography;
using System.Text;

public class SaveFileCodec
{
    private const string XorKey = "StockClicker_SaveKey_v1";

    public string Encode(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText)) return string.Empty;
        
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encodedBytes = ApplyXor(plainBytes);
        
        return Convert.ToBase64String(encodedBytes);
    }

    public bool TryDecode(string encodedText, out string plainText)
    {
        plainText = string.Empty;
        
        if (string.IsNullOrWhiteSpace(encodedText)) return false;

        try
        {
            byte[] encodedBytes = Convert.FromBase64String(encodedText);
            byte[] plainBytes = ApplyXor(encodedBytes);
            plainText = Encoding.UTF8.GetString(plainBytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string ComputeChecksum(string encodedPayload)
    {
        if (encodedPayload == null) encodedPayload = string.Empty;
        
        byte[] bytes = Encoding.UTF8.GetBytes(encodedPayload);
        
        using SHA256 sha256 = SHA256.Create();
        byte[] hashBytes = sha256.ComputeHash(bytes);
        
        return Convert.ToBase64String(hashBytes);
    }
    
    public bool ValidateChecksum(string encodedPayload, string checksum)
    {
        if (string.IsNullOrWhiteSpace(checksum)) return false;
        
        return ComputeChecksum(encodedPayload) == checksum;
    }
    
    private byte[] ApplyXor(byte[] sourceBytes)
    {
        byte[] result = new byte[sourceBytes.Length];
        byte[] keyBytes = Encoding.UTF8.GetBytes(XorKey);

        for (int i = 0; i < sourceBytes.Length; i++)
        {
            result[i] = (byte)(sourceBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }
        
        return result;
    }
    
}
