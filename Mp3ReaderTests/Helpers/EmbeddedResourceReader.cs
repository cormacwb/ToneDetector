using System.IO;
using System.Reflection;

namespace Mp3ReaderTests.Helpers
{
    public static class EmbeddedResourceReader
    {
        public static Stream GetStream(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }
    }
}
