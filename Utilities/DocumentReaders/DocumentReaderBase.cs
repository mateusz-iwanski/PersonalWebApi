using DocumentFormat.OpenXml.Packaging;
using PersonalWebApi.Services.Azure;

namespace PersonalWebApi.Utilities.Utilities.DocumentReaders
{
    public class DocumentReaderBase
    {
        protected readonly IBlobStorageService _blobService;

        public DocumentReaderBase(IBlobStorageService blobService)
        {
            _blobService = blobService;
        }

        public static string GetAuthorNameFromDocument(IFormFile document)
        {
            using (var stream = document.OpenReadStream())
            {
                using (var wordDocument = WordprocessingDocument.Open(stream, false))
                {
                    var coreProperties = wordDocument.PackageProperties;
                    return coreProperties.Creator;
                }
            }
        }
    }
}
