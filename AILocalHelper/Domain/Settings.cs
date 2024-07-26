using LiteDB;

namespace AILocalHelper.Domain
{
    public class Settings
    {
        [BsonId]
        public int Id { get; set; }
        public string PathToModel { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public List<HistoryRecord> HistoryRecords { get; set; } = [];
    }

    public class HistoryRecord
    {
        public DateTime CreatedDateTime { get; set; }
        public string Actor { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        private HistoryRecord() { }
        public static HistoryRecord CreateRecord(string actor, string message)
        {
            return new HistoryRecord
            {
                CreatedDateTime = DateTime.Now,
                Actor = actor,
                Message = message
            };
        }

        public static HistoryRecord CreateSystemRecord(string message)
        {
            return new HistoryRecord
            {
                CreatedDateTime = DateTime.Now,
                Actor = Domain.Actor.System,
                Message = message
            };
        }

        public static HistoryRecord CreateAIRecord(string message)
        {
            return new HistoryRecord
            {
                CreatedDateTime = DateTime.Now,
                Actor = Domain.Actor.AI,
                Message = message
            };
        }

        public static HistoryRecord CreateUserRecord(string message)
        {
            return new HistoryRecord
            {
                CreatedDateTime = DateTime.Now,
                Actor = Domain.Actor.User,
                Message = message
            };
        }

        public override string ToString()
        {
            return $"{CreatedDateTime.ToShortDateString()} {CreatedDateTime.ToShortTimeString()}: {Actor} - {Message}";
        }
    }
}
