# 02 Core types

The types every plugin works with, as tests that state one fact each: run them, change them, and watch them fail.

[CoreTypesTests.cs](./Nethermind.CoreTypes.Test/CoreTypesTests.cs) covers:

| Type | Namespace | Notes |
|---|---|---|
| `Hash256`, `ValueHash256`, `Keccak` | `Nethermind.Core.Crypto` | Class and struct forms of a 32-byte hash |
| `Address`, `PrivateKey`, `EthereumEcdsa` | `Nethermind.Core`, `Nethermind.Crypto` | Parsing, EIP-55 checksums, signing and recovery |
| `UInt256` | `Nethermind.Int256` | 256-bit amounts in wei; arithmetic wraps like the EVM's |
| `Bytes`, `ToHexString` | `Nethermind.Core.Extensions` | Hex conversions; `1.Ether`, `1.GWei` units |
| `Rlp`, `TxDecoder` | `Nethermind.Serialization.Rlp` | Ethereum's serialization |
| `Block`, `BlockHeader`, `Transaction` | `Nethermind.Core` | Built with the `Build.A` test builders |
| `ISpecProvider`, `IReleaseSpec` | `Nethermind.Core.Specs` | Which rules are active at a block |

```sh
dotnet test --project Nethermind.CoreTypes.Test -c release
```
