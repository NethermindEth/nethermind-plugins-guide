# 03 JSON-RPC

Adds an `account` namespace with one method, `account_summary`, which returns the balance, nonce and code size of an account in the state of the head block.

1. [IAccountRpcModule.cs](./Nethermind.AccountRpcPlugin/IAccountRpcModule.cs): the namespace, the method and its result. Method names are the JSON-RPC names; the result is serialized like the `eth` namespace's, with hex quantities.
2. [AccountRpcModule.cs](./Nethermind.AccountRpcPlugin/AccountRpcModule.cs): reading the state with `IStateReader`, and returning an error.
3. [AccountRpcPlugin.cs](./Nethermind.AccountRpcPlugin/AccountRpcPlugin.cs): registering the module.
4. [AccountRpcModuleTests.cs](./Nethermind.AccountRpcPlugin.Test/AccountRpcModuleTests.cs): a transfer in a test node, checked through the module.

## Try it

The namespace is served only when the operator enables it, like any built-in one:

```sh
dotnet build Nethermind.AccountRpcPlugin -c release
dotnet run --project ../nethermind/src/Nethermind/Nethermind.Runner -c release -- \
  --config spaceneth --Init.DiagnosticMode MemDb --plugins-dir "$PWD/../artifacts/plugins" \
  --JsonRpc.Enabled true --JsonRpc.EnabledModules eth,account

curl -s -X POST -H 'content-type: application/json' http://localhost:8545 \
  --data '{"jsonrpc":"2.0","id":1,"method":"account_summary","params":["0x0000000000000000000000000000000000000000"]}'
```

```json
{"jsonrpc":"2.0","result":{"blockNumber":"0x0","balance":"0x1","nonce":"0x0","codeSize":0},"id":1}
```

The node checks the parameters before calling the method: `"params":["not-an-address"]` returns the standard `-32602` invalid-argument error.
