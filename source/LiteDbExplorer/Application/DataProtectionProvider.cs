using System;
using System.Security;

namespace LiteDbExplorer
{
    public class DataProtectionProvider
    {
        private static readonly byte[] _entropy = System.Text.Encoding.Unicode.GetBytes("AB410213-FF0A-400D-AA1E-AA93C25F0AEE");

        public static bool TryProtectPassword(string password, out string securePassword)
        {
            securePassword = null;
            try
            {
                var secureString = ToSecureString(password);
                securePassword = EncryptString(secureString);
                if (!string.IsNullOrEmpty(securePassword))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // Ignore
            }
            return false;
        }

        public static bool TryUnprotectPassword(string password, out string insecurePassword)
        {
            insecurePassword = null;
            try
            {
                var decryptString = DecryptString(password);
                insecurePassword = ToInsecureString(decryptString);
                if (!string.IsNullOrEmpty(insecurePassword))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // Ignore
            }

            return false;
        }

        public static string EncryptString(SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                _entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    _entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        public static SecureString ToSecureString(string input)
        {
            var secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ToInsecureString(SecureString input)
        {
            var returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
    }
}