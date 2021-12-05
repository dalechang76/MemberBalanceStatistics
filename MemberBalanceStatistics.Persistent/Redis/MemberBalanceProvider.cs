using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemberBalanceStatistics.Domain.Dto;
using MemberBalanceStatistics.Domain.Options;
using MemberBalanceStatistics.Domain.Provider;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace MemberBalanceStatistics.Provider.Redis
{
    public class MemberBalanceProvider : IMemberBalanceProvider
    {
        private readonly string HISTORY_RESULT_KEY = "HistoryResult";
        private readonly string EXTREME_RESULT_KEY = "ExtremeResult";
        private readonly string MEMBER_BALANCE_KEY = "MemberBalance";
        private readonly string LAST_EXCUTE_TIME_KEY = "LastExcuteTime";
        private readonly string MEMBER_BALANCE_STATISTICS_KEY = "MemberBalanceStatistics";

        private readonly IServer _server;
        private readonly IDatabase _database;
        private readonly RedisOptions _options;

        public MemberBalanceProvider(IOptions<RedisOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value;

            var redisConnectionFactory = new RedisConnectionFactory(optionsAccessor);
            _server = redisConnectionFactory._Server;
            _database = redisConnectionFactory._Database;
        }

        public IEnumerable<TotalResultDto> GetToalResult(int categoryID)
        {
            var batch = _database.CreateBatch();
            var historyResults = new Dictionary<string, Task<HashEntry[]>>();
            var tasks = new List<Task<HashEntry[]>>();

            var keys = GetHistoryResultKeys();

            foreach (var key in keys)
            {
                var result = batch.HashGetAllAsync(key);
                tasks.Add(result);

                var memberID = key.ToString().Replace($"{_options.AffixKey}:{HISTORY_RESULT_KEY}:", "");
                historyResults.Add(memberID, result);
            }

            batch.Execute();
            Task.WaitAll(tasks.ToArray());

            return historyResults.Select(x => new TotalResultDto
            {
                MemberID = x.Key,
                TotalResult = (long)x.Value.Result.Where(entry => entry.Name == $"{categoryID}_DayResult").Select(entry => entry.Value).FirstOrDefault()
            });
        }

        public IEnumerable<ExtremeResultDto> GetExtremeResult(int categoryID, IEnumerable<string> memberIDs)
        {
            var batch = _database.CreateBatch();
            var extremeResults = new Dictionary<string, Task<HashEntry[]>>();
            var tasks = new List<Task<HashEntry[]>>();

            var keys = GenerateExtremeResultKeys(memberIDs);

            foreach (var key in keys)
            {
                var result = batch.HashGetAllAsync(key);
                tasks.Add(result);

                var memberID = key.ToString().Replace($"{_options.AffixKey}:{MEMBER_BALANCE_STATISTICS_KEY}:{EXTREME_RESULT_KEY}:", "");
                extremeResults.Add(memberID, result);
            }

            batch.Execute();
            Task.WaitAll(tasks.ToArray());

            return extremeResults.Select(x => new ExtremeResultDto
            {
                MemberID = x.Key,
                ResultMin = (long)x.Value.Result.Where(entry => entry.Name == $"{categoryID}_ResultMin").Select(entry => entry.Value).FirstOrDefault(),
                ResultMax = (long)x.Value.Result.Where(entry => entry.Name == $"{categoryID}_Resultx").Select(entry => entry.Value).FirstOrDefault()
            });
        }

        public bool UpdateExtremeResult(int categoryID, IEnumerable<ExtremeResultDto> dtos)
        {
            var batch = _database.CreateBatch();

            foreach (var data in dtos)
            {
                var values = new HashEntry[]
                {
                        new HashEntry($"MemberID", data.MemberID),
                        new HashEntry($"{categoryID}_ResultMax", data.ResultMin),
                        new HashEntry($"{categoryID}_ResultMin", data.ResultMax),
                };

                batch.HashSetAsync($"{_options.AffixKey}:{MEMBER_BALANCE_STATISTICS_KEY}:{EXTREME_RESULT_KEY}:{data.MemberID}", values);
            }

            batch.Execute();
            return true;
        }

        public bool UpdateMemberBalance(int categoryID, IEnumerable<MemberBalanceDto> memberBalances)
        {
            var batch = _database.CreateBatch();

            foreach (var data in memberBalances)
            {
                var values = new HashEntry[]
                {
                        new HashEntry($"MemberID", data.MemberID),
                        new HashEntry($"{categoryID}_Balance", data.Balance)
                };

                batch.HashSetAsync($"{_options.AffixKey}:{MEMBER_BALANCE_STATISTICS_KEY}:{MEMBER_BALANCE_KEY}:{data.MemberID}", values);
            }

            batch.Execute();
            return true;
        }

        private IEnumerable<RedisKey> GetHistoryResultKeys()
            => _server.Keys(_options.DbIndex, pattern: $"{_options.AffixKey}:{HISTORY_RESULT_KEY}:*");

        private IEnumerable<RedisKey> GetExtremeResultKeys()
            => _server.Keys(_options.DbIndex, pattern: $"{_options.AffixKey}:{MEMBER_BALANCE_STATISTICS_KEY}:{EXTREME_RESULT_KEY}:*");

        private IEnumerable<RedisKey> GetMemberBalanceKeys()
            => _server.Keys(_options.DbIndex, pattern: $"{_options.AffixKey}:{MEMBER_BALANCE_STATISTICS_KEY}:{MEMBER_BALANCE_KEY}:*");

        private IEnumerable<RedisKey> GenerateExtremeResultKeys(IEnumerable<string> memberIDs)
            => memberIDs.Select(x => new RedisKey($"{_options.AffixKey}:{MEMBER_BALANCE_STATISTICS_KEY}:{EXTREME_RESULT_KEY}:{x}"));

        public DateTime? GetLastExcuteTime()
        {
            var key = new RedisKey($"{_options.AffixKey}:{MEMBER_BALANCE_STATISTICS_KEY}:{LAST_EXCUTE_TIME_KEY}");
            var time =  _database.StringGet(key);
            if (time.IsNullOrEmpty)
            {
                return null;
            }

            return Convert.ToDateTime(time.ToString());
        }

        public bool SetLastExcuteTime()
        {
            var value = DateTime.UtcNow.AddHours(8).ToString();
            var key = new RedisKey($"{_options.AffixKey}:{MEMBER_BALANCE_STATISTICS_KEY}:{LAST_EXCUTE_TIME_KEY}");
            _database.StringSet(key, value);
            return true;
        }

        public bool ClearExtremeResult()
        {
            var batch = _database.CreateBatch();
            var tasks = new List<Task<bool>>();

            var keys = GetExtremeResultKeys();

            foreach (var key in keys)
            {
                var result = batch.KeyDeleteAsync(key);
                tasks.Add(result);
            }

            batch.Execute();
            Task.WaitAll(tasks.ToArray());

            return true;
        }

        public bool CleareMemberBalance()
        {
            var batch = _database.CreateBatch();
            var tasks = new List<Task<bool>>();

            var keys = GetMemberBalanceKeys();

            foreach (var key in keys)
            {
                var result = batch.KeyDeleteAsync(key);
                tasks.Add(result);
            }

            batch.Execute();
            Task.WaitAll(tasks.ToArray());

            return true;
        }
    }
}
