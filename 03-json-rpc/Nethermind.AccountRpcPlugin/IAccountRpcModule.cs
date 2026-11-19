// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core;
using Nethermind.Int256;
using Nethermind.JsonRpc;
using Nethermind.JsonRpc.Modules;

namespace Nethermind.AccountRpcPlugin;

/// <summary>
/// The <c>account</c> JSON-RPC namespace.
/// </summary>
/// <remarks>
/// The attribute names the namespace, which an operator enables like a built-in one:
/// <c>--JsonRpc.EnabledModules eth,net,web3,account</c>. Every method's name is its JSON-RPC method name, so it
/// starts with the namespace.
/// </remarks>
[RpcModule("Account")]
public interface IAccountRpcModule : IRpcModule
{
    [JsonRpcMethod(
        Description = "Returns the balance, nonce and code size of an account in the state of the head block.",
        ExampleResponse = """{"blockNumber":"0x1","balance":"0xde0b6b3a7640000","nonce":"0x1","codeSize":0}""")]
    ResultWrapper<AccountSummary> account_summary(Address address);
}

/// <summary>
/// An account in the state of the block <paramref name="BlockNumber"/>.
/// </summary>
/// <remarks>The node's serializer writes numeric Ethereum types as hex quantities, as the <c>eth</c> namespace does.</remarks>
public record AccountSummary(ulong BlockNumber, UInt256 Balance, UInt256 Nonce, int CodeSize);
