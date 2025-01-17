using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TagLib;
using iText.Kernel.Pdf;
using MetadataExtractor;
using DocumentFormat.OpenXml.Packaging;
using PersonalWebApi.Models.FileStorage;
using File = TagLib.File;
using Amazon.Runtime.Internal.Transform;

namespace PersonalWebApi.Utilities.Document
{
    /// <summary>
    /// Provides methods to create metadata for various file types.
    /// </summary>
    /// <remarks>
    /// This class uses the following NuGet packages:
    /// - TagLibSharp: For extracting metadata from audio files.
    /// - itext7: For extracting metadata from PDF files.
    /// - MetadataExtractor: For extracting metadata from various file types.
    /// - DocumentFormat.OpenXml: For extracting metadata from Word and Excel files.
    /// - Microsoft.PowerShell.5.ReferenceAssemblies: For working with PowerShell and digital signatures.
    /// </remarks>
    public static class FileMetadataCreator
    {
        /// <summary>
        /// Creates metadata for the specified file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="fileId">The unique identifier for the file.</param>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="sessionId">The unique identifier for the session.</param>
        /// <returns>A <see cref="FileContentMetadataDto"/> containing the metadata of the file.</returns>
        /// <example>
        /// <code>
        /// var metadata = FileMetadataCreator.CreateMetadata("path/to/file.pdf", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        /// Console.WriteLine(metadata.FileName);
        /// </code>
        /// </example>
        public static FileContentMetadataDto CreateMetadata(string filePath, Guid fileId, Guid conversationUuid, Guid sessionId)
        {
            var fileInfo = new FileInfo(filePath);
            var metadata = new FileContentMetadataDto(conversationUuid, sessionId, fileId)
            {
                FileName = fileInfo.Name,
                ContentType = GetContentType(filePath),
                FileSize = fileInfo.Length,
                StoragePath = filePath,
                Checksum = GetChecksum(filePath),
                UploadedBy = "system", // This should be set to the actual user
                Description = "Automatically generated metadata"
            };

            // Extract additional metadata based on file type
            if (fileInfo.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ExtractPdfMetadata(filePath, metadata);
            }
            else if (fileInfo.Extension.Equals(".docx", StringComparison.OrdinalIgnoreCase) ||
                     fileInfo.Extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ExtractOpenXmlMetadata(filePath, metadata);
            }
            else if (fileInfo.Extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase) ||
                     fileInfo.Extension.Equals(".flac", StringComparison.OrdinalIgnoreCase))
            {
                ExtractAudioMetadata(filePath, metadata);
            }

            // Extract digital signature if available
            metadata.Checksum = GetDigitalSignature(filePath);

            // Extract additional metadata using MetadataExtractor
            ExtractAdditionalMetadata(filePath, metadata);

            return metadata;
        }

        /// <summary>
        /// Creates metadata for a file from a URL.
        /// </summary>
        /// <param name="fileUrl">The URL of the file.</param>
        /// <param name="fileId">The unique identifier for the file.</param>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="sessionUuid">The unique identifier for the session.</param>
        /// <returns>A <see cref="FileContentMetadataDto"/> containing the metadata of the file.</returns>
        /// <example>
        /// <code>
        /// var metadata = await FileMetadataCreator.CreateMetadataFromUrlAsync("https://example.com/file.pdf", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        /// Console.WriteLine(metadata.FileName);
        /// </code>
        /// </example>
        public static async Task<FileContentMetadataDto> CreateMetadataFromUrlAsync(string fileUrl, Guid fileId, Guid conversationUuid, Guid sessionUuid)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(fileUrl);
                response.EnsureSuccessStatusCode();

                var tempFilePath = Path.GetTempFileName();
                await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                return CreateMetadata(tempFilePath, fileId, conversationUuid, sessionUuid);
            }
        }

        /// <summary>
        /// Creates metadata for a file from an <see cref="IFormFile"/>.
        /// </summary>
        /// <param name="formFile">The form file.</param>
        /// <param name="fileId">The unique identifier for the file.</param>
        /// <param name="conversationUuid">The unique identifier for the conversation.</param>
        /// <param name="sessionUuid">The unique identifier for the session.</param>
        /// <returns>A <see cref="FileContentMetadataDto"/> containing the metadata of the file.</returns>
        /// <example>
        /// <code>
        /// var metadata = await FileMetadataCreator.CreateMetadataFromFormFileAsync(formFile, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        /// Console.WriteLine(metadata.FileName);
        /// </code>
        /// </example>
        public static async Task<FileContentMetadataDto> CreateMetadataFromFormFileAsync(IFormFile formFile, Guid fileId, Guid conversationUuid, Guid sessionUuid)
        {
            var tempFilePath = Path.GetTempFileName();
            await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                await formFile.CopyToAsync(fileStream);
            }

            return CreateMetadata(tempFilePath, fileId, conversationUuid, sessionUuid);
        }

        /// <summary>
        /// Determines the MIME type of the specified file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>The MIME type of the file.</returns>
        private static string GetContentType(string filePath)
        {
            // Implement logic to determine MIME type
            return "application/octet-stream";
        }

        /// <summary>
        /// Computes the SHA-256 checksum of the specified file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>The SHA-256 checksum of the file.</returns>
        private static string GetChecksum(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    var hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// Extracts metadata from a PDF file.
        /// </summary>
        /// <param name="filePath">The path to the PDF file.</param>
        /// <param name="metadata">The metadata object to populate.</param>
        private static void ExtractPdfMetadata(string filePath, FileContentMetadataDto metadata)
        {
            using (var pdfReader = new PdfReader(filePath))
            {
                var pdfDocument = new PdfDocument(pdfReader);
                var info = pdfDocument.GetDocumentInfo();
                metadata.Description = info.GetMoreInfo("Description");
                metadata.PageCount = pdfDocument.GetNumberOfPages();
            }
        }

        /// <summary>
        /// Extracts metadata from an OpenXML document (Word or Excel).
        /// </summary>
        /// <param name="filePath">The path to the OpenXML document.</param>
        /// <param name="metadata">The metadata object to populate.</param>
        private static void ExtractOpenXmlMetadata(string filePath, FileContentMetadataDto metadata)
        {
            using (var document = WordprocessingDocument.Open(filePath, false))
            {
                var props = document.PackageProperties;
                metadata.Description = props.Description;
                metadata.Category = props.Category;
            }
        }

        /// <summary>
        /// Extracts metadata from an audio file.
        /// </summary>
        /// <param name="filePath">The path to the audio file.</param>
        /// <param name="metadata">The metadata object to populate.</param>
        private static void ExtractAudioMetadata(string filePath, FileContentMetadataDto metadata)
        {
            var file = TagLib.File.Create(filePath);
            metadata.Description = file.Tag.Title;
            metadata.Category = file.Tag.Album;
        }

        /// <summary>
        /// Extracts the digital signature from the specified file, if available.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>The thumbprint of the digital signature, or null if not available.</returns>
        private static string GetDigitalSignature(string filePath)
        {
            try
            {
                var cert = new X509Certificate2(X509Certificate.CreateFromSignedFile(filePath));
                return cert.Thumbprint;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts additional metadata using MetadataExtractor.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="metadata">The metadata object to populate.</param>
        private static void ExtractAdditionalMetadata(string filePath, FileContentMetadataDto metadata)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);
                foreach (var directory in directories)
                {
                    foreach (var tag in directory.Tags)
                    {
                        metadata.Metadata.Add($"{directory.Name} - {tag.Name}", tag.Description);
                    }
                }
            }
            catch (MetadataExtractor.ImageProcessingException ex)
            {
                // Log the exception and file details
                Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                // Optionally, add more error handling logic here
            }
        }
    }
}
