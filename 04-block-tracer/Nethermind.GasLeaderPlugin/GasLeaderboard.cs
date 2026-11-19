// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System.ComponentModel;
using Nethermind.Core.Attributes;
using Nethermind.Core.Crypto;
using Nethermind.Logging;

namespace Nethermind.GasLeaderPlugin;

/// <summary>
/// The most gas-hungry transaction of a block; <paramref name="TransactionHash"/> is null for a block without transactions.
/// </summary>
public record GasLeader(ulong BlockNumber, Hash256? BlockHash, Hash256? TransactionHash, ulong GasUsed);

/// <summary>
/// Keeps the gas leader of the last executed block.
/// </summary>
/// <remarks>
/// <para>Written on the block processing thread and readable from any thread.</para>
/// <para>
/// A block is traced while it executes, before the node validates the result, so this also reports blocks the node then
/// rejects (a wrong state root, for example) and branches that are reorganized out later. To report canonical blocks
/// only, keep the results per block hash and publish them on <c>IBlockTree.BlockAddedToMain</c>.
/// </para>
/// </remarks>
public sealed class GasLeaderboard(ILogManager logManager)
{
    private readonly ILogger _logger = logManager.GetClassLogger<GasLeaderboard>();
    private GasLeader? _latest;

    public GasLeader? Latest => Volatile.Read(ref _latest);

    internal void Record(GasLeader leader)
    {
        Volatile.Write(ref _latest, leader);
        Metrics.LastBlockLeaderGasUsed = leader.GasUsed;
        if (leader.TransactionHash is not null && _logger.IsInfo)
        {
            _logger.Info($"Block {leader.BlockNumber}: transaction {leader.TransactionHash} used the most gas, {leader.GasUsed:N0}");
        }
    }
}

public static class Metrics
{
    [GaugeMetric]
    [Description("Gas used by the most gas-hungry transaction of the last processed block.")]
    public static ulong LastBlockLeaderGasUsed;
}
