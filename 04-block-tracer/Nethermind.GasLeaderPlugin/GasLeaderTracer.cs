// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Evm.Tracing;
using Nethermind.Evm.TransactionProcessing;
using Nethermind.Int256;

namespace Nethermind.GasLeaderPlugin;

/// <summary>
/// Finds the transaction that used the most gas in each processed block and reports it to the <see cref="GasLeaderboard"/>.
/// </summary>
/// <remarks>
/// <para>
/// The block processor calls a block tracer for every block (<see cref="StartNewBlockTrace"/>,
/// <see cref="EndBlockTrace"/>) and asks it for a transaction tracer for every transaction. A transaction tracer only
/// receives what its <c>IsTracing*</c> properties ask for; each one adds cost, so ask for as little as possible.
/// </para>
/// <para>
/// Blocks with a block-level access list (EIP-7928) can execute their transactions in parallel. Then only tracers marked
/// <see cref="IParallelSafeBlockTracer"/> are called, from several threads at once; any other tracer silently sees no
/// transactions of such blocks. This one is safe: every transaction gets its own tracer, and they only meet in
/// <see cref="BlockLeader"/>, under a lock.
/// </para>
/// </remarks>
internal sealed class GasLeaderTracer(GasLeaderboard leaderboard) : IParallelSafeBlockTracer
{
    private BlockLeader? _block;

    public bool IsTracingRewards => false;

    public void ReportReward(Address author, string rewardType, UInt256 rewardValue) { }

    public void StartNewBlockTrace(Block block) => Volatile.Write(ref _block, new BlockLeader(block));

    public ITxTracer StartNewTxTrace(Transaction? tx) =>
        tx is not null && Volatile.Read(ref _block) is { } block ? new GasTxTracer(block, tx) : NullTxTracer.Instance;

    public void EndTxTrace() { }

    public void EndBlockTrace()
    {
        if (Interlocked.Exchange(ref _block, null) is { } block) leaderboard.Record(block.ToResult());
    }

    private sealed class GasTxTracer(BlockLeader block, Transaction tx) : TxTracer
    {
        // The receipt is the cheapest thing to trace: it reports the gas once, when the transaction ends.
        public override bool IsTracingReceipt => true;

        public override void MarkAsSuccess(Address recipient, in GasConsumed gasSpent, byte[] output, LogEntry[] logs, Hash256? stateRoot = null) =>
            block.Report(tx, gasSpent.SpentGas);

        public override void MarkAsFailed(Address recipient, in GasConsumed gasSpent, byte[] output, string? error, Hash256? stateRoot = null) =>
            block.Report(tx, gasSpent.SpentGas);
    }

    private sealed class BlockLeader(Block block)
    {
        private readonly Lock _lock = new();
        private Hash256? _transactionHash;
        private ulong _gasUsed;

        public void Report(Transaction tx, ulong gasUsed)
        {
            lock (_lock)
            {
                if (_transactionHash is not null && gasUsed <= _gasUsed) return;
                _transactionHash = tx.Hash;
                _gasUsed = gasUsed;
            }
        }

        public GasLeader ToResult()
        {
            lock (_lock) return new GasLeader(block.Number, block.Hash, _transactionHash, _gasUsed);
        }
    }
}
