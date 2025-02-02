
namespace IntegrationTests.Common.Utils
{
    internal static class FilesTestsUtils
    {
        private const string BASE_PATH = "Common/Resources";


        internal static FileStream GetTestImage(out string contentType)
        {
            contentType = "image/png";
            var fileStream = ReadFile("testImage.png", 1024);
            return fileStream;
        }

        internal static FileStream GetTestTextFile(out string contentType)
        {
            contentType = "text/plain";
            var fileStream = ReadFile("testText.txt", 1024);
            return fileStream;
        }

        private static FileStream ReadFile(string fileName, int streamLengthInB)
        {
            var fileStream = File.Open($"{BASE_PATH}/{fileName}", FileMode.Open);
            fileStream.SetLength(streamLengthInB);

            return fileStream;
        }
    }
}