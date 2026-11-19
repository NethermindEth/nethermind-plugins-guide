// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Blockchain;
using Nethermind.Core;
using Nethermind.Logging;

namespace Nethermind.HelloPlugin;

/// <summary>
/// Logs and counts every new head block.
/// </summary>
/// <remarks>
/// A service gets its dependencies by constructor injection from the node's container. The container disposes the
/// singletons it created on shutdown, which is where event handlers are removed.
/// </remarks>
public sealed class HeadLogger : IDisposable
{
    private readonly IBlockTree _blockTree;
    private readonly IHelloConfig _config;
    private readonly ILogger _logger;

    public HeadLogger(IBlockTree blockTree, IHelloConfig config, ILogManager logManager)
    {
        _blockTree = blockTree;
        _config = config;
        _logger = logManager.GetClassLogger<HeadLogger>();
        _blockTree.NewHeadBlock += OnNewHeadBlock;
    }

    private void OnNewHeadBlock(object? sender, BlockEventArgs e)
    {
        // Event handlers run on the node's own threads: keep them short, and catch anything that can throw, as an
        // exception escaping here reaches the node.
        Metrics.HeadBlocksSeen++;
        Metrics.LastHeadBlockNumber = e.Block.Number;

        // Checking the level first skips building a message that would not be logged.
        if (_logger.IsInfo) _logger.Info($"{_config.Greeting}, block {e.Block.ToString(Block.Format.Short)}!");
    }

    public void Dispose() => _blockTree.NewHeadBlock -= OnNewHeadBlock;
}
