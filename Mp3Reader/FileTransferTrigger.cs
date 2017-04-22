using System;
using System.IO;
using System.Net.Http;
using log4net;

namespace Mp3Reader
{
    public class FileTransferTrigger
    {
        private readonly HttpClient _client;
        private readonly string _apiBaseUri;
        private readonly string _outpuDirectory;

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FileTransferTrigger(string apiBaseUri, string outputDirectory)
        {
            _client = new HttpClient();
            _apiBaseUri = apiBaseUri;
            _outpuDirectory = outputDirectory;
        }

        public void SweepOutputDirectory()
        {
            var directory = new DirectoryInfo(_outpuDirectory);

            foreach (var fileInfo in directory.GetFiles("*.wav"))
            {
                var requestUri = string.Concat(_apiBaseUri, "ping.php", $"?filename={fileInfo.Name}");
                var apiCall = _client.GetAsync(requestUri);
                apiCall.ContinueWith(t =>
                {
                    try
                    {
                        Log.Info(t.Result);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                });
            }
        }
    }
}
