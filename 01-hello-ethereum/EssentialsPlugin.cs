using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Consensus.Processing;
using Nethermind.Core;
using Nethermind.Logging;

namespace EssentialsPlugin;

public class EssentialsPlugin : INethermindPlugin
{
    private ILogger _logger;
    private INethermindApi? _api;

    // Name and Author will be displayed on start up
    public string Name => nameof(EssentialsPlugin);
    public string Author => "Netherminder";
    public string Description => "Hello Ethereum";

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    class IBlock : IBlockPreprocessorStep
    {
        public void RecoverData(Block block)
        {
            throw new NotImplementedException();
        }
    }

    public Task Init(INethermindApi api)
    {
        // API does not contain all services initialized on Init, InitRpcModules/InitNetworkProtocol may require more than available right now
        _api = api;

        // Loggers are typed
        _logger = api.LogManager.GetClassLogger();
        // Optional if clause reduces allocations
        if (_logger.IsWarn) _logger.Warn("Hello, Ethereum!");

        // You can schedule a task that will run when Nethermind is free from heavy tasks like block processing
        _api.BackgroundTaskScheduler.ScheduleTask(new { }, () => , TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }
}
