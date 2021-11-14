using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NEDev
{
    public static class FileAsync
    {
        public static readonly UTF8Encoding DefaultEncoding = new UTF8Encoding(false);
        public static readonly char[] LineSplitChars = new char[] { '\r', '\n' };
        const int DefaultBufferSize = 4096;
        const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
        

        public static async Task WriteAllTextAsync(string filePath, string text, Encoding encoding = null)
        {
            var currentEncoding = encoding ?? DefaultEncoding;
            var encodedBytes = currentEncoding.GetBytes(text);
            await WriteAllBytesAsync(filePath, encodedBytes);
        }

        public static async Task<string[]> ReadAllLinesAsync(string filePath, Encoding encoding = null)
        {
            var text = await ReadAllTextAsync(filePath, encoding);
            return text.Split(LineSplitChars, StringSplitOptions.RemoveEmptyEntries);
        }

        public static async Task<string> ReadAllTextAsync(string filePath, Encoding encoding = null)
        {
            var currentEncoding = encoding ?? DefaultEncoding;
            using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(sourceStream, currentEncoding))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static async Task WriteAllBytesAsync(string filePath, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            using (FileStream sourceStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, DefaultBufferSize, true))
                await sourceStream.WriteAsync(bytes, 0, bytes.Length);
        }

        public static async Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            {
                var readCount = 0;
                var bytes = new byte[sourceStream.Length];
                while (readCount < bytes.Length)
                    readCount += await sourceStream.ReadAsync(bytes, readCount, bytes.Length - readCount);
                return bytes;
            }
        }
    }
}
