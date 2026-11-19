# 01 Hello plugin

The smallest plugin that has every part a real one has: it logs and counts every new head block.

Read the files in this order:

1. [HelloPlugin.cs](./Nethermind.HelloPlugin/HelloPlugin.cs): the plugin class the node discovers, and its DI module.
2. [IHelloConfig.cs](./Nethermind.HelloPlugin/IHelloConfig.cs) and [HelloConfig.cs](./Nethermind.HelloPlugin/HelloConfig.cs): a config category, and why the names matter.
3. [HeadLogger.cs](./Nethermind.HelloPlugin/HeadLogger.cs): a service with injected dependencies that subscribes to `IBlockTree.NewHeadBlock` and unsubscribes on disposal.
4. [StartHeadLogger.cs](./Nethermind.HelloPlugin/StartHeadLogger.cs): a startup step that creates the service before the first block is processed.
5. [Metrics.cs](./Nethermind.HelloPlugin/Metrics.cs): Prometheus metrics without any registration code.
6. [HelloPluginTests.cs](./Nethermind.HelloPlugin.Test/HelloPluginTests.cs): the plugin in an in-memory test node.

## Try it

```sh
dotnet build Nethermind.HelloPlugin -c release
dotnet run --project ../nethermind/src/Nethermind/Nethermind.Runner -c release -- \
  --config spaceneth --Init.DiagnosticMode MemDb \
  --plugins-dir "$PWD/../artifacts/plugins" --Hello.Enabled true --Hello.Greeting Hi
```

Then set the greeting with `NETHERMIND_HELLOCONFIG_GREETING=Hi` instead, or add `--Metrics.Enabled true --Metrics.ExposePort 9091` and look for `nethermind_head_blocks_seen` at `http://localhost:9091/metrics`.
