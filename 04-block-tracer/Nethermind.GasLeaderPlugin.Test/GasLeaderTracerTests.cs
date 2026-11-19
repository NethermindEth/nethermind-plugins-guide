// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Autofac;
using Nethermind.Core;
using Nethermind.Core.Specs;
using Nethermind.Core.Test.Blockchain;
using Nethermind.Core.Test.Builders;
using Nethermind.Evm;
using Nethermind.State;
using NUnit.Framework;

namespace Nethermind.GasLeaderPlugin.Test;

public class GasLeaderTracerTests
{
    [Test]
    public async Task Leader_is_the_transaction_that_used_the_most_gas()
    {
        using BasicTestBlockchain chain = await BasicTestBlockchain.Create(builder => builder
            .AddModule(new GasLeaderPlugin(new GasLeaderConfig { Enabled = true }).Module));
        Block head = chain.BlockTree.Head!;
        ulong nonce = chain.StateReader.GetNonce(head.Header, TestItem.AddressA);
        bool isEip155Enabled = chain.SpecProvider.GetSpec(head.Header).IsEip155Enabled;
        Transaction transfer = Transfer(nonce, gasLimit: GasCostOf.Transaction, data: []);
        // Call data costs gas, so this transaction uses more than the plain transfer.
        Transaction transferWithData = Transfer(nonce + 1, gasLimit: 100_000, data: new byte[256]);

        Block block = await chain.AddBlock(transfer, transferWithData);

        Assert.That(block.Transactions, Has.Length.EqualTo(2), "both transactions should be included");
        GasLeader? leader = chain.Container.Resolve<GasLeaderboard>().Latest;
        Assert.That(leader, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(leader!.BlockNumber, Is.EqualTo(block.Number));
            Assert.That(leader.TransactionHash, Is.EqualTo(transferWithData.Hash));
            Assert.That(leader.GasUsed, Is.GreaterThan(GasCostOf.Transaction));
        }

        Transaction Transfer(ulong txNonce, ulong gasLimit, byte[] data) => Build.A.Transaction
            .WithTo(TestItem.AddressB)
            .WithNonce(txNonce)
            .WithGasLimit(gasLimit)
            .WithGasPrice(head.BaseFeePerGas)
            .WithData(data)
            .SignedAndResolved(TestItem.PrivateKeyA, isEip155Enabled)
            .TestObject;
    }
}
