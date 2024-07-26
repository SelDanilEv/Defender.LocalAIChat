using AILocalHelper.Domain;
using LiteDB;

namespace AILocalHelper.DB
{
    public class LiteDBService : IDisposable
    {
        private readonly string _databasePath = @"settings.db";
        private readonly string _collectionName = @"configs";
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<Settings> _collection;

        public LiteDBService()
        {
            _db = new LiteDatabase(_databasePath);
            _collection = _db.GetCollection<Settings>(_collectionName);
        }

        public Settings GetConfig()
        {
            var config = _collection.FindById(1);

            if (config == null)
            {
                return CreateConfig();
            }

            return config;
        }

        public void SetPathToModel(string pathToModel)
        {
            var config = GetConfig();

            config.PathToModel = pathToModel;
            _collection.Update(config);
        }

        public string UpdateContext(string newContext)
        {
            var config = GetConfig();

            config.Context = newContext;
            _collection.Update(config);

            return config.Context;
        }

        public void ClearContext()
        {
            var config = GetConfig();
            config.Context = String.Empty;
            _collection.Update(config);
        }

        public List<HistoryRecord> AddToHistory(HistoryRecord record)
        {
            var config = GetConfig();

            config.HistoryRecords.Add(record);

            while (config.HistoryRecords.Count > 10)
            {
                config.HistoryRecords.RemoveAt(0);
            }

            _collection.Update(config);

            return config.HistoryRecords;
        }

        public List<HistoryRecord> GetHistoryRecords()
        {
            var config = GetConfig();

            return config.HistoryRecords;
        }

        public Settings ResetConfig()
        {
            var config = GetConfig();

            config.Context = String.Empty;

            _collection.Update(config);

            return config;
        }


        private Settings CreateConfig()
        {
            var config = new Settings
            {
                Id = 1,
                PathToModel = "",
                Context = "",
                HistoryRecords = []
            };

            _collection.Insert(config);

            return config;
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            _db.Dispose();
        }
    }
}
