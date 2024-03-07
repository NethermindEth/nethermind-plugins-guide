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

namespace Nethermind.FakeCl.Plugin;

public class FakeClPlugin : INethermindPlugin
{
    private ILogger _logger;
    private INethermindApi? _api;
    private Core.Timers.ITimer? _timer;

    public string Name => nameof(FakeClPlugin);

    public string Description => "Hello Ethereum";

    public string Author => "Netherminder";

    public ValueTask DisposeAsync()
    {
        _timer?.Dispose();
        return ValueTask.CompletedTask;
    }

    public Task Init(INethermindApi api)
    {
        _logger = api.LogManager.GetClassLogger();
        _logger.Warn("Hello Ethereum");
        _api = api;

        return Task.CompletedTask;
    }

    public Task InitNetworkProtocol()
    {
        return Task.CompletedTask;
    }

    public Task InitRpcModules()
    {
        if(_api is null || _api.BlockTree is null || _api.RpcModuleProvider is null)
        {
            return Task.CompletedTask;
        }

        _timer = _api.TimerFactory.CreateTimer(TimeSpan.FromSeconds(5));
       
        Hash256 previousBlockHash = _api.BlockTree.Head!.Hash!;

        _timer.Elapsed += async (_, _) =>
        {
            _timer.Enabled = false;
            await ProgressAsync(_api.BlockTree.Head!);

            IRpcModuleProvider rpcModuleProvider = _api.RpcModuleProvider;
            if (rpcModuleProvider is null)
            {
                _logger.Warn($"{nameof(_api.RpcModuleProvider)} is not available");
                return;
            }
            IEngineRpcModule rpc = (await rpcModuleProvider.Rent(nameof(IEngineRpcModule.engine_forkchoiceUpdatedV3), false) 
                as IEngineRpcModule)!;

            ResultWrapper<ForkchoiceUpdatedV1Result> fcu = 
                await rpc.engine_forkchoiceUpdatedV3(new ForkchoiceStateV1(previousBlockHash, previousBlockHash, previousBlockHash),
                   new PayloadAttributes
                   {
                       Withdrawals = [],
                       Timestamp = (ulong)DateTime.UtcNow.Ticks,
                       PrevRandao = new Hash256(new byte[32]),
                       SuggestedFeeRecipient = new Address(new byte[20]),
                       ParentBeaconBlockRoot = new Hash256(new byte[32]),
                   });

            // wait a bit for a new payload build
            await Task.Delay(100);

            GetPayloadV3Result builtPayload = 
                (await rpc.engine_getPayloadV3(Hex.Decode(fcu.Data.PayloadId!.Replace("0x", "")))).Data!;

            previousBlockHash = builtPayload.ExecutionPayload.BlockHash;

            var res3 = await rpc.engine_newPayloadV3(
                builtPayload.ExecutionPayload,
                builtPayload.BlobsBundle.Blobs,
                builtPayload.ExecutionPayload.ParentBeaconBlockRoot);
                       
            rpcModuleProvider.Return(nameof(IEngineRpcModule.engine_forkchoiceUpdatedV3), rpc);
            _timer.Enabled = true;
        };

        _timer.Start();
        return Task.CompletedTask;
    }

    public virtual Task ProgressAsync(Block head)
    {
        _logger.Warn($"New block! {head.Number} {head.Hash}");
        return Task.CompletedTask;
    }
}
