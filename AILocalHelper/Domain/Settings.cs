using LiteDB;

namespace AILocalHelper.Domain
{
    public class Settings
    {
        [BsonId]
        public int Id { get; set; }
        public bool IsLocked { get; set; } = false;
        public string PathToModel { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public List<HistoryRecord> HistoryRecords { get; set; } = [];
    }

    public class HistoryRecord
    {
        public DateTime CreatedDateTime { get; set; }
        public string Actor { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
