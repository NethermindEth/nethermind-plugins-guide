﻿using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Logging;

namespace _01_hello_ethereum;

public class EssentialsPlugin : INethermindPlugin
{
    private ILogger _logger;
    private INethermindApi? _api;

    public string Name => nameof(EssentialsPlugin);

    public string Description => "Hello Ethereum";

    public string Author => "Netherminder";

    public ValueTask DisposeAsync()
    {
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
        return Task.CompletedTask;
    }
}