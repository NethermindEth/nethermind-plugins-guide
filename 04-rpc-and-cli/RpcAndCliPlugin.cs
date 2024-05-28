using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Logging;

namespace Nethermind.RpcAndCli.Plugin;

public class RpcAndCliPlugin : INethermindPlugin
{
    private ILogger _logger;
    private INethermindApi? _api;

    public string Name => nameof(RpcAndCliPlugin);

    public string Description => "Hello Ethereum";

    public string Author => "Netherminder";

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
