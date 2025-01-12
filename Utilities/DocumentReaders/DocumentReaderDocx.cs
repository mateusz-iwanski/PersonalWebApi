using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PersonalWebApi.Services.Azure;
using System.Text;

namespace PersonalWebApi.Utilities.Utilities.DocumentReaders
{
    /// <summary>
    /// Read docx file.
    /// 
    /// First download form Azure Blob Storage and read it.
    /// </summary>
    public class DocumentReaderDocx : DocumentReaderBase, IDocumentReaderDocx
    {
        public DocumentReaderDocx(IBlobStorageService blobService) : base(blobService)
        {
        }

        /// <summary>
        /// Reads the content of a DOCX file from Azure Blob Storage.
        /// </summary>
        /// <param name="fileUri">The URI of the DOCX file in Azure Blob Storage.</param>
        /// <returns>A task that represents the asynchronous read operation. The task result contains the text content of the DOCX file.</returns>
        /// <remarks>
        /// This method downloads the DOCX file from Azure Blob Storage, opens it using the Open XML SDK, and reads the text content of each paragraph in the document.
        /// </remarks>
        public async Task<string> ReadAsync(Uri fileUri)
        {
            StringBuilder text = new StringBuilder();

            using (var stream = await _blobService.DownloadFileAsync(fileUri))
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, false))
            {
                Body body = wordDoc.MainDocumentPart.Document.Body;
                foreach (var paragraph in body.Elements<Paragraph>())
                {
                    text.AppendLine(paragraph.InnerText);
                }
            }

            return text.ToString();
        }


    }
}
