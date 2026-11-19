// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System.ComponentModel;
using Nethermind.Core.Attributes;

namespace Nethermind.HelloPlugin;

/// <summary>
/// The plugin's Prometheus metrics.
/// </summary>
/// <remarks>
/// A public static class named exactly <c>Metrics</c> in a plugin assembly is exported automatically, here as
/// <c>nethermind_head_blocks_seen</c> and <c>nethermind_last_head_block_number</c>.
/// </remarks>
public static class Metrics
{
    [CounterMetric]
    [Description("Number of new head blocks seen by the hello plugin.")]
    public static long HeadBlocksSeen;

    [GaugeMetric]
    [Description("Number of the last head block seen by the hello plugin.")]
    public static ulong LastHeadBlockNumber;
}
