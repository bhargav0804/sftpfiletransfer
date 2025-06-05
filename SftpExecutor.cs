
using Serilog;
using WinSCP;

namespace SftpFileTransfer
{
    public class SftpExecutor
    {
        public static void ExecuteSftpScript()
        {
            Log.Information($"Executing upload script: {ConfigurationLoader._uploadScriptPath}");
            using (Session session = new Session())
            {
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = ConfigurationLoader._sftpHost,
                    PortNumber = ConfigurationLoader._sftpPort,
                    UserName = ConfigurationLoader._sftpUser,
                    Password = ConfigurationLoader._sftpPassword,
                    SshHostKeyFingerprint = ConfigurationLoader._sftpSSH
                };

                session.Open(sessionOptions);
                Log.Information($"Connected to SFTP: {ConfigurationLoader._sftpHost}");

                string[] scriptFile = File.ReadAllLines(ConfigurationLoader._uploadScriptPath);
                HashSet<string> processedPaths = new HashSet<string>();

                foreach (string file in scriptFile)
                {
                    if (file.StartsWith("put", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = file.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3)
                        {
                            string sourcePath = parts[3];
                            string remotePath = parts[5];

                            if (processedPaths.Contains(sourcePath))
                            {
                                Log.Warning($"Skipping already processed paths: {sourcePath}");
                                continue;
                            }
                            processedPaths.Add(sourcePath);

                            if (Directory.Exists(sourcePath))
                            {
                                Log.Information($"Processing files from: {sourcePath}");
                                string[] files = Directory.GetFiles(sourcePath, "*.*")
                                                          .Where(f => f.EndsWith(".txt") || f.EndsWith(".csv") || f.EndsWith(".dat"))
                                                          .ToArray();
                                Log.Information($"Total files found in {sourcePath}: {files.Length}");

                                if (files.Length == 0)
                                {
                                    Log.Warning($"No .txt, .csv, or .dat files found in {sourcePath}. Skipping upload.");
                                }
                                else
                                {
                                    List<string> missingFiles = new List<string>();

                                    foreach (string fileToUpload in files)
                                    {
                                        try
                                        {
                                            string localfileToUpload = Path.GetFullPath(fileToUpload);
                                            Log.Information($"Checking local file: {localfileToUpload}");
                                            if (!File.Exists(fileToUpload))
                                            {
                                                missingFiles.Add(Path.GetFileName(localfileToUpload));
                                                Log.Warning($"Files does not exist locally: {localfileToUpload}");
                                                continue;
                                            }
                                            string fileName = Path.GetFileName(localfileToUpload);
                                            string remotefilePath = remotePath.TrimEnd('/') + "/" + fileName;
                                            Log.Information($"Checking if file exists on remote server: {remotefilePath}");
                                            if (session.FileExists(remotefilePath))
                                            {
                                                Log.Information($"Remote file exists. Deleting: {remotefilePath}");
                                                try
                                                {
                                                    session.RemoveFiles(remotefilePath).Check();
                                                    Log.Information($"Deleted existing remote file: {remotefilePath}");
                                                }
                                                catch (Exception ex)
                                                {
                                                    Log.Warning($"Failed to delete remote file {remotefilePath}: {ex.Message}");
                                                    continue;
                                                }
                                            }
                                            //session.CreateDirectory(remotefilePath);
                                            Log.Information($"Uploading {localfileToUpload} to {remotefilePath}");

                                            TransferOptions transferOptions = new TransferOptions
                                            {
                                                TransferMode = TransferMode.Binary
                                            };

                                            session.PutFiles(localfileToUpload, remotefilePath, false, transferOptions).Check();

                                            Log.Information($"Upload successful: {localfileToUpload} -> {remotePath}");
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Error($"Error uploading file {sourcePath}: {ex.Message}");
                                        }
                                    }

                                    if (missingFiles.Count > 0)
                                    {
                                        Log.Warning($"The following files were expected but not found: {string.Join(", ", missingFiles)}");
                                    }
                                }
                            }
                            else
                            {
                                Log.Error($"Source directory does not exist: {sourcePath}");
                            }
                        }
                    }
                }
                Log.Information("All directories processed. Closing session.");
                session.Close();
            }
        }
    }
}
