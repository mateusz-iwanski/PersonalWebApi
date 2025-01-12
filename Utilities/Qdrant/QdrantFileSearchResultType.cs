namespace PersonalWebApi.Utilities.Utilities.Qdrant
{
    public class QdrantFileSearchResultType
    {
        public string Id { get; set; }
        public QdrantFilePayloadType Payload { get; set; }
        public float Score { get; set; }
        public string Version { get; set; }
    }

}
