// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Autofac;
using Nethermind.Core;
using Nethermind.Core.Specs;
using Nethermind.Core.Test.Blockchain;
using Nethermind.Core.Test.Builders;
using Nethermind.Evm;
using Nethermind.State;
using Nethermind.Int256;
using Nethermind.JsonRpc;
using Nethermind.JsonRpc.Modules;
using NUnit.Framework;

namespace Nethermind.AccountRpcPlugin.Test;

public class AccountRpcModuleTests
{
    [Test]
    public async Task Summary_reflects_the_head_block_state()
    {
        using BasicTestBlockchain chain = await BasicTestBlockchain.Create(builder => builder
            .AddModule(new AccountRpcPlugin().Module));
        IAccountRpcModule rpc = chain.Container.Resolve<IAccountRpcModule>();
        Address recipient = TestItem.GetRandomAddress();
        UInt256 value = 1_000_000;

        Block head = chain.BlockTree.Head!;
        ulong senderNonce = chain.StateReader.GetNonce(head.Header, TestItem.AddressA);
        Block block = await chain.AddBlock(Build.A.Transaction
            .WithTo(recipient)
            .WithValue(value)
            .WithNonce(senderNonce)
            .WithGasLimit(GasCostOf.Transaction)
            .WithGasPrice(head.BaseFeePerGas)
            .SignedAndResolved(TestItem.PrivateKeyA, chain.SpecProvider.GetSpec(head.Header).IsEip155Enabled)
            .TestObject);
        Assert.That(block.Transactions, Has.Length.EqualTo(1), "the transfer should be included");

        ResultWrapper<AccountSummary> sender = rpc.account_summary(TestItem.AddressA);
        ResultWrapper<AccountSummary> received = rpc.account_summary(recipient);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(received.Data, Is.EqualTo(new AccountSummary(block.Number, value, UInt256.Zero, 0)));
            Assert.That(sender.Data.Nonce, Is.EqualTo((UInt256)(senderNonce + 1)));
        }
    }

    [Test]
    public async Task Namespace_is_registered_with_the_node()
    {
        using BasicTestBlockchain chain = await BasicTestBlockchain.Create(builder => builder
            .AddModule(new AccountRpcPlugin().Module));

        Assert.That(chain.Container.Resolve<IRpcModuleProvider>().All, Does.Contain("Account"));
    }
}
