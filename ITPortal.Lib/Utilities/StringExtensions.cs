﻿namespace ITPortal.Lib.Utilities;

public static class StringExtensions
{
    public static string EncodeBase64(this string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }
        byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

        return Convert.ToBase64String(plainTextBytes);
    }

    public static string DecodeBase64(this string base64EncodedData)
    {
        if (string.IsNullOrEmpty(base64EncodedData))
        {
            return string.Empty;
        }
        byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static string FirstCharToUpper(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }
        return $"{char.ToUpper(input[0])}{input[1..]}";
    }

    public static string NormalizeLength(this string input, int maxLength)
    {
        return input.Length <= maxLength ? input : input.Substring(0, maxLength);
    }

    public static string AsSystemMessage(this string message)
    {
        return "[SYSTEM]: " + message;
    }
}
