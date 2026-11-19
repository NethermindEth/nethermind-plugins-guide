// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Autofac;
using Autofac.Core;
using Nethermind.Api.Extensions;
using Nethermind.Config;
using Nethermind.Core;
using Nethermind.Core.Container;
using Nethermind.Evm.Tracing;

namespace Nethermind.GasLeaderPlugin;

[ConfigCategory(Description = "Configuration of the gas leader plugin.")]
public interface IGasLeaderConfig : IConfig
{
    [ConfigItem(Description = "Whether to trace every processed block for its most gas-hungry transaction.", DefaultValue = "false")]
    bool Enabled { get; set; }
}

public class GasLeaderConfig : IGasLeaderConfig
{
    public bool Enabled { get; set; }
}

/// <summary>
/// Reports the transaction that used the most gas in every block the node processes.
/// </summary>
public class GasLeaderPlugin(IGasLeaderConfig config) : INethermindPlugin
{
    public string Name => "GasLeader";
    public string Description => "Reports the most gas-hungry transaction of every processed block.";
    public string Author => "Nethermind";
    // Tracing costs time on every block, so it is opt-in.
    public bool Enabled => config.Enabled;
    public IModule Module => new GasLeaderModule();
}

public class GasLeaderModule : Module
{
    protected override void Load(ContainerBuilder builder) => builder
        // The leaderboard lives in the node's root container, so anything (a JSON-RPC module, a step) can read it.
        .AddSingleton<GasLeaderboard>()
        // The tracer is registered in the main block processing scope only, so it traces the blocks the node imports
        // and not eth_call, simulations or block production, which build processing scopes of their own.
        .AddSingleton<IMainProcessingModule, GasLeaderMainProcessingModule>();

    private class GasLeaderMainProcessingModule : Module, IMainProcessingModule
    {
        // Every IBlockTracer in this scope is combined into the tracer of the main block processor.
        protected override void Load(ContainerBuilder builder) =>
            builder.AddSingleton<IBlockTracer, GasLeaderTracer>();
    }
}
