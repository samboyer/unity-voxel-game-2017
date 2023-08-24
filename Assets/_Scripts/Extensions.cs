using UnityEngine;

public static class Extensions
{
    public static byte[] ToArray(this Color32 col)
    {
        return new byte[] { col.r, col.g, col.b, col.a };
    }

    public static Color32 ToColor(this byte[] bytes)
    {
        if (bytes.Length == 4)
            return new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);
        if(bytes.Length==3)
            return new Color32(bytes[0], bytes[1], bytes[2], 255);
        return new Color32{r=0,g=0,b=0,a=0};
    }

    public static string HexStringFromBytes(byte[] bytes)
    {
        string str1 = System.BitConverter.ToString(bytes);
        return str1.Replace("-", string.Empty);
    }
    public static byte[] ColorBytesFromHexString(string hexStr)
    {
        if (hexStr[0] == '#') hexStr = hexStr.Substring(1);
        if(hexStr.Length == 6)
        {
            return new byte[] {byte.Parse(hexStr.Substring(0,2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture),
                byte.Parse(hexStr.Substring(2,2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture),
                byte.Parse(hexStr.Substring(4,2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture)
            };
        }
        if (hexStr.Length == 8)
        {
            return new byte[] {byte.Parse(hexStr.Substring(0,2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture),
                byte.Parse(hexStr.Substring(2,2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture),
                byte.Parse(hexStr.Substring(4,2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture),
                byte.Parse(hexStr.Substring(6,2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture)
            };
        }
        return new byte[] { 0, 0, 0, 0 };
    }

    static string chars = "abdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
    public static string GenerateBase64String(int length)
    {
        string str = "";
        for(int i = 0; i < length; i++)
        {
            str += chars[Random.Range(0, chars.Length)];
        }
        return str;
    }
}

