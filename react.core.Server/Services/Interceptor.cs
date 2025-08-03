namespace duoword.admin.Server.Services
{
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Caching.Distributed;
    using ProtoBuf;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;



    public class ProtobufCacheInterceptor : DbCommandInterceptor
    {
        private readonly IDistributedCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

        public ProtobufCacheInterceptor(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Override for SYNC query interception
        public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            string cacheKey = GenerateCacheKey(command);
            byte[] cachedData = _cache.Get(cacheKey);

            //if (cachedData != null)
            //{
            //    Console.WriteLine($"[CACHE HIT] Sync Query: {command.CommandText}");
            //    var cachedResult = DeserializeFromCache(cachedData);  // Deserialize to List<Dictionary<string, object>>
            //    return new CachedDbDataReader(cachedResult);  // This can now work as expected
            //}

            var data = ReadDataSync(result);
            byte[] serializedData = SerializeForCache(data);
            _cache.Set(cacheKey, serializedData, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheExpiration });

            return result;
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            string cacheKey = GenerateCacheKey(command);
            byte[] cachedData = await _cache.GetAsync(cacheKey, cancellationToken);

            //if (cachedData != null)
            //{
            //    Console.WriteLine($"[CACHE HIT] Async Query: {command.CommandText}");
            //    var cachedResult = DeserializeFromCache(cachedData);
            //    return new CachedDbDataReader(cachedResult);
            //}

            var data = await ReadDataAsync(result);
            byte[] serializedData = SerializeForCache(data);
            await _cache.SetAsync(cacheKey, serializedData, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheExpiration }, cancellationToken);

            return result;
        }

        // Helper: Serialize any object dynamically
        private byte[] SerializeForCache(object data)
        {
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, data);  // Protobuf's generic serialization
            return ms.ToArray();
        }

        // Helper: Deserialize to any object dynamically
        private object DeserializeFromCache(byte[] data)
        {
            using var ms = new MemoryStream(data);
            var deserialized = Serializer.Deserialize<object>(ms);  // Protobuf's generic deserialization
            return deserialized;
        }

        // Helper: Generate cache key based on the SQL query
        private string GenerateCacheKey(DbCommand command)
        {
            return $"SQL_CACHE:{command.CommandText.GetHashCode()}";
        }

        // Helper: Convert DbDataReader to a generic list of objects
        private List<object> ReadDataSync(DbDataReader reader)
        {
            var results = new List<object>();

            while (reader.Read())
            {
                var obj = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    obj[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(obj);  // Add as a dictionary or any object, depending on your needs
            }

            return results;
        }

        private async Task<List<object>> ReadDataAsync(DbDataReader reader)
        {
            var results = new List<object>();

            while (await reader.ReadAsync())
            {
                var obj = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    obj[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(obj);
            }

            return results;
        }
    }

    public class CachedDbDataReader : DbDataReader
    {
        private readonly List<Dictionary<string, object>> _cachedData;
        private int _currentRow = -1;

        public CachedDbDataReader(List<Dictionary<string, object>> cachedData)
        {
            _cachedData = cachedData ?? throw new ArgumentNullException(nameof(cachedData));
        }

        // Moves to the next row in the cached data
        public override bool Read()
        {
            _currentRow++;
            return _currentRow < _cachedData.Count;
        }

        // Retrieves the value of a specific column by ordinal
        public override object GetValue(int ordinal)
        {
            var columnName = GetName(ordinal);
            return _cachedData[_currentRow].TryGetValue(columnName, out var value) ? value : DBNull.Value;
        }

        public override int FieldCount => _cachedData.FirstOrDefault()?.Count ?? 0;

        // Gets the column name by its ordinal position
        public override string GetName(int ordinal)
        {
            return _cachedData.FirstOrDefault()?.Keys.ElementAt(ordinal) ?? string.Empty;
        }

        // Indicates if the reader has rows (cached data available)
        public override bool HasRows => _cachedData.Count > 0;

        // Checks if the value at the given ordinal is DBNull
        public override bool IsDBNull(int ordinal) => GetValue(ordinal) == DBNull.Value;

        // Gets the ordinal (position) of a column by its name
        public override int GetOrdinal(string name)
        {
            return _cachedData.FirstOrDefault()?.Keys.ToList().IndexOf(name) ?? -1;
        }

        // Accesses the value by column index
        public override object this[int ordinal] => GetValue(ordinal);

        // Accesses the value by column name
        public override object this[string name] => _cachedData[_currentRow][name];

        // Required overrides for DbDataReader (but will return default values)
        public override bool NextResult() => false;
        public override int RecordsAffected => _cachedData.Count;

        public override int Depth => 0;
        public override bool IsClosed => false;

        public override int VisibleFieldCount => base.VisibleFieldCount;

        public override void Close() { }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string? ToString()
        {
            return base.ToString();
        }

        //public override object InitializeLifetimeService()
        //{
        //    return base.InitializeLifetimeService();
        //}

        public override Task CloseAsync()
        {
            return base.CloseAsync();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            return base.DisposeAsync();
        }

        public override bool GetBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            return base.GetDbDataReader(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        public override Type GetFieldType(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            return base.GetFieldValueAsync<T>(ordinal, cancellationToken);
        }

        public override T GetFieldValue<T>(int ordinal)
        {
            return base.GetFieldValue<T>(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            return base.GetProviderSpecificFieldType(ordinal);
        }

        public override object GetProviderSpecificValue(int ordinal)
        {
            return base.GetProviderSpecificValue(ordinal);
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            return base.GetProviderSpecificValues(values);
        }

        public override DataTable? GetSchemaTable()
        {
            return base.GetSchemaTable();
        }

        public override Task<DataTable?> GetSchemaTableAsync(CancellationToken cancellationToken = default)
        {
            return base.GetSchemaTableAsync(cancellationToken);
        }

        public override Task<ReadOnlyCollection<DbColumn>> GetColumnSchemaAsync(CancellationToken cancellationToken = default)
        {
            return base.GetColumnSchemaAsync(cancellationToken);
        }

        public override Stream GetStream(int ordinal)
        {
            return base.GetStream(ordinal);
        }

        public override string GetString(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override TextReader GetTextReader(int ordinal)
        {
            return base.GetTextReader(ordinal);
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            return base.IsDBNullAsync(ordinal, cancellationToken);
        }

        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            return base.NextResultAsync(cancellationToken);
        }

        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return base.ReadAsync(cancellationToken);
        }
    }

}
