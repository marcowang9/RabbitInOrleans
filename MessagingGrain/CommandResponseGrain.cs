using System.Threading.Tasks;
using Orleans;
using MessagingGrainInterface;
using System;
using Logging.Orleans;
using System.Diagnostics;
using Common;
using RabbitMQMessaging;
using System.Text;
using Orleans.Concurrency;

namespace MessagingGrain
{
    [Reentrant]
    public class CommandResponseGrain<T> : Grain, ICommandResponseGrain<T> where T : class
    {
        private Guid _correlationId;

        private IOrleansLog _logger;

        private string _commandName;

        private object _resultValue;

        private const int RetryMillSeconds = 30000;

        public CommandResponseGrain() { }

        public override async Task OnActivateAsync()
        {
            _correlationId = this.GetPrimaryKey();
            _logger = OrleansLoggerSingleton.Logger;

            _logger.Information(string.Format("CommandResponseGrain [{0}]>> Grain Activated", _correlationId));
            await TaskDone.Done;
        }

        public async Task<bool> IsCompleteAsync()
        {
            if (_resultValue != null)
            {
                _logger.Information(string.Format("IsCompleteAsync [{0}]>> Polling, DONE", _correlationId));
                return await Task.FromResult(true);
            }
            _logger.Information(string.Format("IsCompleteAsync [{0}]>> Polling, NOT READY", _correlationId));
            return await Task.FromResult(false);
        }

        public async Task SetResponseAsync(string commandName, object commandResultValue)
        {
            _logger.Information(string.Format("SetResponse [{0}]>> Got result for command=[{1}]",
                _correlationId, commandName));

            await Task.Run(() => {
                _commandName = commandName;
                _resultValue = commandResultValue;
            });
        }

        public async Task<T> GetResultAsync()
        {
            _logger.Information(string.Format("GetResultAsync [{0}]>> Request result retrieval", _correlationId));

            using (var myStopWatch = new MyStopwatch())
            {
                while (myStopWatch.ElapsedMilliseconds < RetryMillSeconds)
                {
                    var isComplete = await IsCompleteAsync();
                    if (isComplete)
                    {
                        var message = _resultValue as RabbitReceivedMessage;
                        var result = new CommandResult()
                        {
                            ResponseBody = Encoding.UTF8.GetString(message.GetBody()),
                            Value = message.Header
                        };

                        return result as T;
                    }
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }
         
            _logger.Warning(string.Format("GetResultAsync [{0}]>> Premature retrieval of result!! result NOT READY", _correlationId));
            return default(T);
        }
    }

    internal class MyStopwatch : IDisposable
    {
        private readonly Stopwatch _stopWatch;

        public MyStopwatch()
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        public void Dispose()
        {
            _stopWatch.Stop();
        }

        public long ElapsedMilliseconds
        {
            get { return _stopWatch.ElapsedMilliseconds; }
        }
    }

    //public static class CommandResponseGrainExt
    //{
    //    private const int RetryMillSeconds = 300000;

    //    public static async Task<T> GetResultWithWaitAsync<T>(this ICommandResponseGrain<T> commandResponseGrain)
    //        where T : class
    //    {
    //        using (var myStopWatch = new MyStopwatch())
    //        {
    //            while (myStopWatch.ElapsedMilliseconds < RetryMillSeconds)
    //            {
    //                var isComplete = await commandResponseGrain.IsCompleteAsync();
    //                if (isComplete)
    //                {
    //                    return await commandResponseGrain.GetResultAsync();
    //                }
    //                await Task.Delay(TimeSpan.FromSeconds(1));
    //            }
    //        }
    //        return null;
    //    }
    //}
}
