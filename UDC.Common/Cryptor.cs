using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UDC.Common
{
    public static class Cryptor
    {
        private static int _iterations = 2;
        private static int _keySize = 256;

        private static string _hash = "SHA1";
        private static string _salt = "mae3fy157ghdqb84"; // Random
        private static string _vector = "ykrvui1sqnzg5ae4"; // Random

        public static string Encrypt(string value, string password)
        {
            return Encrypt<AesManaged>(value, password);
        }
        public static string Encrypt<T>(string value, string password)
            where T : SymmetricAlgorithm, new()
        {

            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            byte[] vectorBytes = Encoding.ASCII.GetBytes(_vector);
            byte[] saltBytes = Encoding.ASCII.GetBytes(_salt);
            byte[] encrypted;

            // Use the generic SymmetricAlgorithm type (T, which is likely TripleDESCryptoServiceProvider)
            using (T cipher = new T())
            {
                // Derive the key using PasswordDeriveBytes
                // Ensure _hash and _iterations match the encryption side.
                PasswordDeriveBytes _passwordBytes =  new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8); // Get key bytes of correct size

                // Explicitly set CipherMode and PaddingMode for consistency and security
                cipher.Mode = CipherMode.CBC;
                cipher.Padding = PaddingMode.PKCS7; // Standard padding

                // Set the generated key and IV for the cipher
                cipher.Key = keyBytes;
                cipher.IV = vectorBytes;

                // Create the encryptor transform
                using (ICryptoTransform encryptor = cipher.CreateEncryptor()) // No need to pass keyBytes, vectorBytes here if set on cipher object
                {
                    // Use MemoryStream to hold the encrypted output bytes
                    using (MemoryStream to = new MemoryStream())
                    {
                        // Use CryptoStream to perform the encryption as data is written
                        using (CryptoStream cryptoStream = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                        {
                            // Write the plaintext bytes to the CryptoStream
                            cryptoStream.Write(valueBytes, 0, valueBytes.Length);
                            // Flush any final blocks and apply padding
                            cryptoStream.FlushFinalBlock();
                            // Get the encrypted bytes from the MemoryStream
                            encrypted = to.ToArray();
                        }
                    }
                }

            }
            // Convert the encrypted bytes to a Base64 string for storage/transmission
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string value, string password)
        {
            return Decrypt<AesManaged>(value, password);
        }
        public static string Decrypt<T>(string value, string password) where T : SymmetricAlgorithm, new()
        {
            byte[] vectorBytes = ASCIIEncoding.ASCII.GetBytes(_vector);
            byte[] saltBytes = ASCIIEncoding.ASCII.GetBytes(_salt);

            byte[] valueBytes = Convert.FromBase64String(value);

            byte[] decrypted;
            int decryptedByteCount = 0;

            using (T cipher = new T())
            {
                // Problem 2: PasswordDeriveBytes for key derivation
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(password, saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8); // This generates the key

                cipher.Mode = CipherMode.CBC;
                cipher.Padding = PaddingMode.PKCS7; // Explicitly set this if not already

                try
                {
                    using (ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes))
                    {
                        using (MemoryStream from = new MemoryStream(valueBytes))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                            {
                                using (MemoryStream to = new MemoryStream())
                                {
                                    cryptoStream.CopyTo(to); // This is the correct way to read
                                    decrypted = to.ToArray();
                                    decryptedByteCount = decrypted.Length;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) // Still recommend logging 'ex' here
                {
                    return String.Empty;
                }

                cipher.Clear();
            }
            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }
    }
}