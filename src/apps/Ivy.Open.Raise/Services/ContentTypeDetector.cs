namespace Ivy.Open.Raise.Services;

public static class ContentTypeDetector
{
    /// <summary>
    /// Detects the content type from a stream and/or filename.
    /// First tries to detect from stream content (magic numbers), then falls back to filename extension.
    /// </summary>
    /// <param name="stream">The stream to inspect (will be reset to position 0 after reading)</param>
    /// <param name="fileName">The filename to use for extension-based detection</param>
    /// <returns>The detected MIME type, or "application/octet-stream" if detection fails</returns>
    public static string Detect(Stream? stream, string? fileName)
    {
        // Try to detect from stream content (magic numbers)
        if (stream is { CanSeek: true })
        {
            var detectedFromStream = DetectFromStream(stream);
            stream.Position = 0; // Reset stream position after reading

            if (!string.IsNullOrEmpty(detectedFromStream))
            {
                return detectedFromStream;
            }
        }

        // Fallback to filename extension
        if (!string.IsNullOrEmpty(fileName))
        {
            var detectedFromExtension = DetectFromFileName(fileName);
            if (!string.IsNullOrEmpty(detectedFromExtension))
            {
                return detectedFromExtension;
            }
        }

        return "application/octet-stream";
    }

    /// <summary>
    /// Detects content type from filename extension using MimeMapping library.
    /// </summary>
    private static string? DetectFromFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        try
        {
            var mimeType = MimeMapping.MimeUtility.GetMimeMapping(fileName);
            // MimeMapping returns "application/octet-stream" when it doesn't know, treat that as null
            return mimeType == "application/octet-stream" ? null : mimeType;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Detects content type by inspecting the first bytes of the stream (magic numbers/file signatures).
    /// </summary>
    private static string? DetectFromStream(Stream stream)
    {
        if (!stream.CanSeek || stream.Length < 4)
            return null;

        var buffer = new byte[Math.Min(512, stream.Length)];
        var bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (bytesRead < 2)
            return null;

        // Check common file signatures (magic numbers)
        return buffer switch
        {
            // Images
            [0xFF, 0xD8, 0xFF, ..] => "image/jpeg",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, ..] => "image/png",
            [0x47, 0x49, 0x46, 0x38, ..] => "image/gif",
            [0x42, 0x4D, ..] => "image/bmp",
            [0x49, 0x49, 0x2A, 0x00, ..] or [0x4D, 0x4D, 0x00, 0x2A, ..] => "image/tiff",
            [0x52, 0x49, 0x46, 0x46, _, _, _, _, 0x57, 0x45, 0x42, 0x50, ..] => "image/webp",
            [0x00, 0x00, 0x01, 0x00, ..] => "image/x-icon",

            // Documents
            [0x25, 0x50, 0x44, 0x46, ..] => "application/pdf",
            [0x50, 0x4B, 0x03, 0x04, ..] => DetectZipBasedFormat(buffer),
            [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, ..] => "application/vnd.ms-office",

            // Video
            [0x00, 0x00, 0x00, _, 0x66, 0x74, 0x79, 0x70, ..] => "video/mp4",
            [0x1A, 0x45, 0xDF, 0xA3, ..] => "video/webm",
            [0x46, 0x4C, 0x56, 0x01, ..] => "video/x-flv",

            // Audio
            [0x49, 0x44, 0x33, ..] => "audio/mpeg",
            [0xFF, 0xFB, ..] or [0xFF, 0xF3, ..] or [0xFF, 0xF2, ..] => "audio/mpeg",
            [0x52, 0x49, 0x46, 0x46, _, _, _, _, 0x57, 0x41, 0x56, 0x45, ..] => "audio/wav",
            [0x4F, 0x67, 0x67, 0x53, ..] => "audio/ogg",

            // Archives
            [0x1F, 0x8B, ..] => "application/gzip",
            [0x42, 0x5A, 0x68, ..] => "application/x-bzip2",
            [0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, ..] => "application/x-rar-compressed",
            [0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C, ..] => "application/x-7z-compressed",

            // Text (UTF-8 BOM)
            [0xEF, 0xBB, 0xBF, ..] => "text/plain",

            _ => null
        };
    }

    private static string DetectZipBasedFormat(byte[] buffer)
    {
        // Check for Office Open XML formats
        if (buffer.Length > 30)
        {
            var bufferStr = System.Text.Encoding.ASCII.GetString(buffer, 0, Math.Min(512, buffer.Length));
            if (bufferStr.Contains("word/")) return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            if (bufferStr.Contains("xl/")) return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            if (bufferStr.Contains("ppt/")) return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        }

        return "application/zip";
    }
}
