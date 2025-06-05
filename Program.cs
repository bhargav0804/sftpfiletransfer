using SftpFileTransfer;

class Program
{
    public static void Main()
    {
        try
        {
            Console.WriteLine("Enter SFTP Password: ");
            string plainPassword = Console.ReadLine();

            if (string.IsNullOrEmpty(plainPassword))
            {
                Console.WriteLine("Error: Password cannot be empty");
                return;
            }

            string encryptPassword = EncryptDecrypt.EncryptPsswd(plainPassword);
            Console.WriteLine($"Encrypted password: {encryptPassword}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}