// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Util
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;

    public static class Hash
    {
        static readonly ThreadLocal<SHA256> SHA256Hasher = new ThreadLocal<SHA256>(() => SHA256.Create());

        static readonly ThreadLocal<SHA1> SHA1Hasher = new ThreadLocal<SHA1>(() => SHA1.Create());

        public static string CreateSha256(this string input)
        {
            byte[] hash = SHA256Hasher.Value.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash);
        }

        public static string CreateSha1AsHex(this string input)
        {
            byte[] hash = SHA1Hasher.Value.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:X2}", b);
            }

            return stringBuilder.ToString();
        }
    }
}
