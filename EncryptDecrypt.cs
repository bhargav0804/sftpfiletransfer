using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SftpFileTransfer
{
    public static class EncryptDecrypt
    {
        public static string EncryptPsswd(string plainText)
        {
            byte[] encryptBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(plainText), null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptBytes);
        }

        public static string DecryptPsswd(string encrptText)
        {
            byte[] decryptBytes = ProtectedData.Unprotect(Convert.FromBase64String(encrptText), null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptBytes);
        }
    }
}
