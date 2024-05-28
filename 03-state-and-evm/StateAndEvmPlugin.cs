using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Logging;

namespace Nethermind.StateAndEvm.Plugin;

public class StateAndEvmPlugin : INethermindPlugin
{
    private ILogger _logger;
    private INethermindApi? _api;

    public string Name => nameof(StateAndEvmPlugin);

    public string Description => "Hello Ethereum";

    public string Author => "Netherminder";

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

}
