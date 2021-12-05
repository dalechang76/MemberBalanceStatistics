using System;
using MemberBalanceStatistics.Domain.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using static StackExchange.Redis.SocketManager;

namespace MemberBalanceStatistics.Provider.Redis
{
    public class RedisConnectionFactory : IDisposable
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;

        public IServer _Server { get; }
        public IDatabase _Database { get; }

        public RedisConnectionFactory(IOptions<RedisOptions> optionsAccessor)
        {
            var options = optionsAccessor.Value;


            _connectionMultiplexer = ConnectionMultiplexer.Connect(
                new ConfigurationOptions
                {
                    EndPoints = { { options.Address, options.Port } },
                    Password = options.Auth,
                    KeepAlive = 30,
                    SocketManager = new SocketManager(
                        workerCount: 100,
                        options: SocketManagerOptions.UseHighPrioritySocketThreads)
                });

            _Database = _connectionMultiplexer.GetDatabase(options.DbIndex);
            _Server = _connectionMultiplexer.GetServer(options.Address, options.Port);
        }

        #region IDisposable Support
        private bool _disposedValue = false; // 偵測多餘的呼叫

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置 Managed 狀態 (Managed 物件)。
                    _connectionMultiplexer?.Dispose();
                }

                // TODO: 釋放 Unmanaged 資源 (Unmanaged 物件) 並覆寫下方的完成項。
                // TODO: 將大型欄位設為 null。

                _disposedValue = true;
            }
        }

        // TODO: 僅當上方的 Dispose(bool disposing) 具有會釋放 Unmanaged 資源的程式碼時，才覆寫完成項。
        // ~RedisConnectionFactory()
        // {
        //   // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 加入這個程式碼的目的在正確實作可處置的模式。
        public void Dispose()
        {
            // 請勿變更這個程式碼。請將清除程式碼放入上方的 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果上方的完成項已被覆寫，即取消下行的註解狀態。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
