# Nethermind plugins guide

Learn to extend the [Nethermind](https://github.com/NethermindEth/nethermind) Ethereum execution client with plugins: small .NET assemblies the node loads at startup that can add JSON-RPC methods, react to blocks, trace execution, export metrics, and change how the node behaves.

Every example is a project that builds and passes its tests against the Nethermind version in the `nethermind` submodule, and CI builds them weekly against the latest Nethermind `master` too.

| Chapter | Shows |
|---|---|
| [01 Hello plugin](./01-hello-plugin) | The plugin contract, config, DI module, startup steps, events, metrics, disposal, tests |
| [02 Core types](./02-core-types) | Hashes, addresses, signatures, `UInt256`, hex, RLP, blocks, transactions and fork specs, as runnable tests |
| [03 JSON-RPC](./03-json-rpc) | A new JSON-RPC namespace that reads the state |
| [04 Block tracer](./04-block-tracer) | A live tracer in the main block processing, safe for parallel execution |
| [05 Invalid chain dump](./05-invalid-chain-dump) | A complete plugin with a third-party dependency (AWS S3), background work, graceful shutdown and fail-fast config validation |

## Quick start

You need the .NET SDK named in [global.json](./global.json) and git.

```sh
git clone https://github.com/NethermindEth/nethermind-plugins-guide.git
cd nethermind-plugins-guide
git submodule update --init
dotnet build NethermindPluginsGuide.slnx -c release
dotnet test --solution NethermindPluginsGuide.slnx -c release --no-build
```

The first build compiles Nethermind itself, which takes a few minutes. Initialize only the guide's own submodule: `--recursive` would also download Nethermind's test fixtures, several gigabytes the guide does not need.

Every plugin project is copied after each build to `artifacts/plugins`, with only the files the node does not ship itself (see [Packaging](#packaging-and-deployment)). Run the node from the submodule with that folder as its plugins folder, here on a local development chain kept in memory:

```sh
dotnet run --project nethermind/src/Nethermind/Nethermind.Runner -c release -- \
  --config spaceneth --Init.DiagnosticMode MemDb \
  --plugins-dir "$PWD/artifacts/plugins" --Hello.Enabled true
```

The startup log lists every plugin and whether it is enabled:

```text
Loading assembly Nethermind.HelloPlugin
...
  Hello by Nethermind            Enabled
...
Hello, block 0 (0xefd537...8f508b)!
```

`--plugins-dir` replaces the default `plugins` folder rather than adding to it, so plugins the node ships only as files there (JsonRpc.TraceStore, OpcodeTracing, ExternalSigner) are not loaded in these runs.

To debug, open `NethermindPluginsGuide.slnx`, which also contains `Nethermind.Runner`, add `--plugins-dir` to its launch arguments, and set breakpoints in your plugin.

## How the node loads a plugin

1. **Discovery.** At startup the node loads every `*.dll` in its plugins folder (`plugins` next to the executable, or `--plugins-dir`; a relative path is relative to the executable, `./` and `../` paths to the working directory) and looks for public classes implementing `INethermindPlugin`. Subfolders are not searched. Config interfaces and `Metrics` classes are found only in assemblies whose name starts with `Nethermind` or that contain a plugin, so keep them in the plugin assembly.
2. **Construction.** The node creates each plugin before it creates itself, so a plugin constructor can only take configs (any `IXConfig`), `ChainSpec` and `IConfigProvider`. Anything else, such as `ILogManager` or `IBlockTree`, belongs in the plugin's services.
3. **Enabling.** A plugin whose `Enabled` is false stops here. Gate optional or costly behavior on a config value.
4. **Wiring.** The `Module` of every enabled plugin is added to the node's [Autofac](https://autofac.org) container, in `Plugin.PluginOrder` order and then alphabetically by class name (a `PluginOrder` entry `Hello` matches a class named `HelloPlugin`). In the module a plugin registers its services, decorates the node's ones and adds startup steps.
5. **Startup.** The node runs its startup steps (`IStep`) in dependency order. A plugin's step declares where it belongs with `[RunnerStepDependencies(dependencies: [...], dependents: [...])]`, and gets services by constructor injection.
6. **Shutdown.** Services implementing `IStoppableService` are stopped, then the container disposes everything it created, including `IDisposable` and `IAsyncDisposable` services.

A plugin only implements `Name`, `Description`, `Author`, `Enabled` and `Module`. `InitTxTypesAndRlpDecoders` is for plugins that add transaction types. The `Init`, `InitRpcModules` and `InitNetworkProtocol` methods of older Nethermind versions are gone, and `MustInitialize` on the plugin has no effect: to make a failure stop the node, throw from a step (steps have `MustInitialize => true` by default), or from the constructor of a service that a step creates during startup. A service created later, such as a JSON-RPC module, only fails the request that created it.

### Configuration

A config is an interface deriving from `IConfig` and a class implementing it, named `IXConfig` and `XConfig`. `X` is the category, and the node derives every way of setting a value from these names:

| Source | Example |
|---|---|
| Command line | `--Hello.Greeting Hi` or `--hello-greeting Hi` |
| Environment | `NETHERMIND_HELLOCONFIG_GREETING=Hi` |
| JSON config file | `{ "Hello": { "Greeting": "Hi" } }` |

The command line wins over the environment, and the environment over the config file. `ConfigItem.DefaultValue` only documents the default for `--help` and the docs; the class property initializer is the actual default. Mark secrets with `IsSensitive = true`. Inject `IXConfig` into any service, step or the plugin constructor.

The node rejects command-line options it does not know, so `--Hello.Enabled` stops the node from starting when the plugin is missing from the plugins folder. Unknown settings in the environment or the config file only log a warning, which makes them the safer way to configure an optional plugin.

## Where a plugin can hook in

In the plugin's Autofac module. The registration methods (`AddSingleton`, `AddDecorator`, `AddStep`, ...) are Nethermind's extensions in `Nethermind.Core`; the [DI rules](https://github.com/NethermindEth/nethermind/blob/master/.agents/rules/di-patterns.md) of the node apply to plugins too.

| To | Use | See |
|---|---|---|
| Run code at a point of the startup | `AddStep(typeof(MyStep))` with `[RunnerStepDependencies]` | [01](./01-hello-plugin), CensorshipDetector |
| React to new blocks, reorgs, finalization | `IBlockTree` events (`NewHeadBlock`, `BlockAddedToMain`, `BlocksFinalized`...) | [01](./01-hello-plugin), StateDiffsWriter |
| React to processed or rejected blocks | `IBlockProcessingQueue` events (`BlockRemoved`, `InvalidBlock`...), `IMainProcessingContext.BranchProcessor` events (`BlockProcessing`, `BlockProcessed`...) | Hive, CensorshipDetector |
| React to transactions | `ITxPool` events (`NewPending`, `RemovedPending`...) | |
| Add JSON-RPC methods | `RegisterSingletonJsonRpcModule<IMyRpcModule, MyRpcModule>()`, enabled with `JsonRpc.EnabledModules` | [03](./03-json-rpc), Flashbots |
| Change existing JSON-RPC methods | `AddDecorator<ITraceRpcModule>(...)` | JsonRpc.TraceStore |
| Trace the execution of every block | `IBlockTracer` registered in an `IMainProcessingModule` | [04](./04-block-tracer), OpcodeTracing |
| Change block processing or validation | `AddDecorator<IBlockProcessor, ...>()`, `IBlockValidationModule` | BalRecorder, AuRa |
| Filter locally sent transactions | `AddDecorator<ITxSender, ...>()` | SendPolicy |
| Add HTTP endpoints, middleware or health checks | `AddSingleton<IJsonRpcServiceConfigurer, ...>()` | PortfolioViewer, HealthChecks |
| Export Prometheus metrics | A public static class named `Metrics` with `[CounterMetric]`/`[GaugeMetric]` fields | [01](./01-hello-plugin) |
| Store data | `AddDatabase("name")` and `[KeyFilter("name")] IDb` | JsonRpc.TraceStore, StateDiffsWriter |
| Run background work | A service started from a step, stopped in `Dispose`, or `IBackgroundTaskScheduler` for low-priority work between blocks | [05](./05-invalid-chain-dump), StateDiffsWriter |
| Add a devp2p protocol | `AddProtocolHandler<T>()`, `AddMessageSerializer<TMsg, TSerializer>()`, an `IP2PCapabilityResolver` | Merge, Xdc |
| Add a consensus engine | `IConsensusPlugin` (exactly one is enabled per chain) | Optimism, Taiko |
| Change another config's values | `Intercept<ITxPoolConfig>(config => ...)` or `AddDecorator<INetworkConfig>(...)` | Taiko, Hive |
| Stop the node | `IProcessExitSource.Exit(code)` | |

The plugins in "See" are in [nethermind/src/Nethermind](https://github.com/NethermindEth/nethermind/tree/master/src/Nethermind) and are the best reference for anything this guide does not cover.

Changing another config this way edits the shared config object when something first resolves it from the container. Code that read it earlier, such as plugin constructors and `Enabled`, sees the original value, so one plugin cannot enable another this way.

Registering a service the node already has replaces it, for every other plugin too; decorate instead, and expect other decorators around yours. A global decorator also runs for `eth_call`, simulations, block production and prewarming; register it in an `IMainProcessingModule` to affect only the node's own block processing.

## Packaging and deployment

A plugin is its assembly plus the dependencies the node does not have. The node loads every `*.dll` in the plugins folder into its default load context, together with its own assemblies; plugins are not isolated from the node or from each other. So:

- Do not ship assemblies the node already has (`Nethermind.*`, Autofac, ASP.NET Core...): the node's copies are used, and a duplicate can fail to load.
- Ship your other dependencies flat next to the plugin; native libraries in `runtimes/` folders are not found.
- Two plugins cannot use different versions of the same dependency, and a plugin gets the node's version of anything the node already has.
- "Failed to load plugin X" in the log for a dependency `X` usually means one of its own dependencies is missing from the folder.

The [Directory.Build.targets](./Directory.Build.targets) of this repository does this for every project with `<NethermindPlugin>true</NethermindPlugin>`: it copies the assembly and the dependencies missing from the node's output to `artifacts/plugins`, and warns about files it cannot ship flat. Its [Directory.Packages.props](./Directory.Packages.props) imports Nethermind's, whose transitive pinning makes a plugin fail to build, rather than fail at runtime, when it needs a different version of a package Nethermind pins. The folder accumulates files across builds; delete it to start clean.

To add a plugin to an installed node, copy the files to its `plugins` folder. In the official Docker image that is `/nethermind/plugins`; mount or copy single files there, as mounting a folder over it hides the plugins the image ships. Use the image of the Nethermind version the plugin was built against (see [Versions](#versions)):

```sh
docker run -v $PWD/artifacts/plugins/Nethermind.HelloPlugin.dll:/nethermind/plugins/Nethermind.HelloPlugin.dll \
  nethermind/nethermind:2.2.2 --config hoodi --Hello.Enabled true
```

### Versions

Nethermind does not promise a stable plugin API: types and members change between versions, and a plugin built against one version can fail with `MissingMethodException` or `TypeLoadException` on another. Build against the exact version you run.

- **From source (this repository).** Check out the release tag in the submodule, `git -C nethermind checkout 2.2.2`, and rebuild. This gives you the test infrastructure (`Nethermind.Core.Test`) and APIs that are not released yet. Chapter 05 needs an API that is not in a release yet; remove it from the solution (`dotnet sln NethermindPluginsGuide.slnx remove 05-invalid-chain-dump/*/*.csproj`) to build against one.
- **From NuGet.** The [Nethermind.ReferenceAssemblies](https://www.nuget.org/packages/Nethermind.ReferenceAssemblies) package has the reference assemblies of every release. It has no dependencies, so reference the packages you use directly (Autofac and `Nethermind.Numerics.Int256`, for example) with the versions of that release and `ExcludeAssets="runtime"`, so they are not shipped with your plugin.

The plugin must also target the node's .NET version (`TargetFramework` in Nethermind's [Directory.Build.props](https://github.com/NethermindEth/nethermind/blob/master/Directory.Build.props)).

## Testing

Reference `Nethermind.Core.Test` from the submodule and add your plugin's module to a test node:

```csharp
using BasicTestBlockchain chain = await BasicTestBlockchain.Create(builder => builder
    .AddSingleton<IHelloConfig>(config)
    .AddModule(new HelloPlugin(config).Module));

Block block = await chain.AddBlock(transaction);
```

`BasicTestBlockchain` runs the real block processing on an in-memory database. Resolve your services from `chain.Container`, override any registration in the builder, and use the `Build.A.Transaction`, `Build.A.Block` and `TestItem` helpers for test data. Every chapter has tests to start from.

## Things that bite

- **Exceptions in event handlers** run on the node's threads and can stop block processing or the node. Catch them.
- **Blocking in event handlers** slows the node down: hand slow work to a background queue, as [05](./05-invalid-chain-dump) does.
- **Parallel execution.** Blocks with a block-level access list (EIP-7928) can run their transactions in parallel. Only tracers implementing `IParallelSafeBlockTracer` are called for those, from several threads at once; see [04](./04-block-tracer).
- **Head versus processing.** The state of a block exists only after it is processed. Use `IStateReader.HasStateForBlock` before reading, and remember a pruning node keeps the state of recent blocks only.
- **Forks.** Ask `ISpecProvider` for the spec of the block at hand instead of assuming the latest fork.

## Need help?

- [Nethermind documentation](https://docs.nethermind.io)
- Discord: #nethermind-plugins on [discord.gg/DH2evNGNeA](https://discord.gg/DH2evNGNeA)
