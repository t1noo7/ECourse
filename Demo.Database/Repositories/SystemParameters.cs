using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using Demo.Application.Services;
using Demo.Core.Models;

namespace Demo.Database
{
    public class SystemParameters : ISystemParameters
    {
        private readonly IMemoryCache _memoryCache;
        protected readonly IMongoCollection<SystemParamData> _collection;
        public SystemParameters(IMongoDatabase database, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _collection = database.GetCollection<SystemParamData>(nameof(SystemParameters));
        }

        public string? BankInfo => GetValue(nameof(BankInfo)) as string;
        public int PaymentPendingDays => int.Parse(GetValue(nameof(PaymentPendingDays)) as string ?? "3");
        public string? SmtpHost => GetValue(nameof(SmtpHost)) as string;
        public int SmtpPort => int.Parse(GetValue(nameof(SmtpPort)) as string ?? "0");
        public string? SmtpEmail => GetValue(nameof(SmtpEmail)) as string;
        public string? SmtpPassword => GetValue(nameof(SmtpPassword)) as string;

        public string? AccountingEmails => GetValue(nameof(AccountingEmails)) as string;

        public string? Domain => GetValue(nameof(Domain)) as string;
        public List<SystemParamData> GetValues() => _memoryCache.GetOrCreate(nameof(SystemParameters), m => _collection.AsQueryable().ToList());

        public object? GetValue(string name) => GetValues().FirstOrDefault(m => m.DataName == name)?.DataValue;

        public void SetValue(string name, object value)
        {
            var filter_id = Builders<SystemParamData>.Filter.Eq(m => m.DataName, name);
            var model = _collection.Find(filter_id).FirstOrDefault();
            if (model == null)
                _collection.InsertOne(new SystemParamData { DataName = name, DataValue = value });
            else
                _collection.UpdateOne(filter_id, Builders<SystemParamData>.Update.Set(m => m.DataValue, value));
            _memoryCache.Remove(nameof(SystemParameters));
        }
    }
}
