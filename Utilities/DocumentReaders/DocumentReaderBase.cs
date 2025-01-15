using DocumentFormat.OpenXml.Packaging;
using PersonalWebApi.Services.FileStorage;

namespace PersonalWebApi.Utilities.Utilities.DocumentReaders
{
    public class DocumentReaderBase
    {
        protected readonly IFileStorageService _blobService;

        public DocumentReaderBase(IFileStorageService blobService)
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
