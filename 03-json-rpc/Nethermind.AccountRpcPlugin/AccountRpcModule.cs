// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Blockchain.Find;
using Nethermind.Core;
using Nethermind.JsonRpc;
using Nethermind.State;

namespace Nethermind.AccountRpcPlugin;

/// <summary>
/// Reads accounts from the state of the head block.
/// </summary>
/// <remarks>
/// <see cref="IStateReader"/> reads the state as of any block whose state the node still has; it never changes it.
/// A pruning node keeps the state of recent blocks only, so <see cref="IStateReader.HasStateForBlock"/> guards the
/// read.
/// </remarks>
public class AccountRpcModule(IBlockFinder blockFinder, IStateReader stateReader) : IAccountRpcModule
{
    public ResultWrapper<AccountSummary> account_summary(Address address)
    {
        BlockHeader? head = blockFinder.Head?.Header;
        if (head is null || !stateReader.HasStateForBlock(head))
        {
            return ResultWrapper<AccountSummary>.Fail("The state of the head block is not available.", ErrorCodes.ResourceUnavailable);
        }

        stateReader.TryGetAccount(head, address, out AccountStruct account);
        return ResultWrapper<AccountSummary>.Success(new AccountSummary(
            head.Number,
            account.Balance,
            account.Nonce,
            stateReader.GetCode(account.CodeHash)?.Length ?? 0));
    }
}
