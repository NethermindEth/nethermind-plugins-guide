using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Consensus.Producers;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.JsonRpc;
using Nethermind.JsonRpc.Modules;
using Nethermind.Logging;
using Nethermind.Merge.Plugin;
using Nethermind.Merge.Plugin.Data;
using Org.BouncyCastle.Utilities.Encoders;

namespace Nethermind.Nethermind.TreeVisitor.Plugin;

public class TreeVisitorPlugin : INethermindPlugin
{
    private ILogger _logger;
    private INethermindApi? _api;
    private Core.Timers.ITimer? _timer;

    public string Name => nameof(TreeVisitorPlugin);

    public string Description => "Hello Ethereum";

    public string Author => "Netherminder";

    public ValueTask DisposeAsync()
    {

    }
}
