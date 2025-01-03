using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using PersonalWebApi.Services.Azure;
using System.Text;

namespace PersonalWebApi.Services.Services.DocumentReaders
{
    public class DocumentReaderDocx : IDocumentReaderDocx
    {
        private readonly IBlobStorageService _blobService;

        public DocumentReaderDocx(IBlobStorageService blobService)
        {
            _blobService = blobService;
        }

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
