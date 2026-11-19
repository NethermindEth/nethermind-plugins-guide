# 04 Block tracer

Traces every block the node executes and reports the transaction that used the most gas, in the log and as the `nethermind_last_block_leader_gas_used` metric. A block is traced while it executes, before the node checks the result, so blocks the node then rejects and branches that are later reorganized out are reported too; [GasLeaderboard.cs](./Nethermind.GasLeaderPlugin/GasLeaderboard.cs) says how to report canonical blocks only.

1. [GasLeaderPlugin.cs](./Nethermind.GasLeaderPlugin/GasLeaderPlugin.cs): registering a tracer for the main block processing only, with an `IMainProcessingModule`.
2. [GasLeaderTracer.cs](./Nethermind.GasLeaderPlugin/GasLeaderTracer.cs): the block and transaction tracer.
3. [GasLeaderboard.cs](./Nethermind.GasLeaderPlugin/GasLeaderboard.cs): publishing the result to the rest of the node.
4. [GasLeaderTracerTests.cs](./Nethermind.GasLeaderPlugin.Test/GasLeaderTracerTests.cs): two transactions in a test block.

## Tracing in short

- The block processor calls an `IBlockTracer` at the start and end of every block and asks it for an `ITxTracer` for every transaction.
- A transaction tracer receives only what its `IsTracing*` properties ask for. `IsTracingReceipt` is cheap; instructions, memory or stack slow every block down, so a live tracer should ask for as little as it can. Derive from `TxTracer` to override only what you need.
- A tracer registered in an `IMainProcessingModule` runs for the blocks the node imports. `eth_call`, `debug_trace*` and block production have their own tracers.
- Blocks with a block-level access list (EIP-7928) can execute their transactions in parallel. Only tracers implementing `IParallelSafeBlockTracer` are called for them, concurrently; others see no transactions at all. This tracer gives every transaction its own tracer and combines the results under a lock.

## Try it

The tracer needs blocks to execute: run it on a network node with its consensus client, or start the node of the [quick start](../README.md#quick-start) with `--GasLeader.Enabled true` and send it transactions.

```sh
dotnet build Nethermind.GasLeaderPlugin -c release
dotnet run --project ../nethermind/src/Nethermind/Nethermind.Runner -c release -- \
  --config hoodi --plugins-dir "$PWD/../artifacts/plugins" --GasLeader.Enabled true
```
