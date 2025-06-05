
using Microsoft.Extensions.Configuration;

namespace SftpFileTransfer
{
    public class ConfigurationLoader
    {
        private static IConfigurationRoot _config;
        public static string _sftpHost, _sftpUser, _sftpPassword, _sftpSSH, _uploadScriptPath;
        public static int _sftpPort;

        public static void LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _config = builder.Build();
            _sftpHost = _config["SftpConfig:sftHost"];
            _sftpPort = int.Parse(_config["SftpConfig:sftPort"]);
            _sftpUser = _config["SftpConfig:sftUser"];
            _sftpPassword = EncryptDecrypt.DecryptPsswd(_config["SftpConfig:sftPsswrd"]);
            _sftpSSH = _config["SftpConfig:sftSSH"];
            _uploadScriptPath = _config["SftpConfig:uploadScriptPath"];
        }
    }
}
