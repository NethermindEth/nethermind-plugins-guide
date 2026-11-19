// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Autofac;
using Nethermind.Core;
using Nethermind.Core.Test.Blockchain;
using NUnit.Framework;

namespace Nethermind.HelloPlugin.Test;

public class HelloPluginTests
{
    [Test]
    public void Plugin_is_enabled_by_config([Values] bool enabled) =>
        Assert.That(new HelloPlugin(new HelloConfig { Enabled = enabled }).Enabled, Is.EqualTo(enabled));

    [Test]
    public async Task Head_logger_sees_every_new_head_block()
    {
        HelloConfig config = new() { Enabled = true };

        // BasicTestBlockchain runs the node's real block processing in memory. The plugin's module is added to its
        // container the same way the node adds it.
        using BasicTestBlockchain chain = await BasicTestBlockchain.Create(builder => builder
            .AddSingleton<IHelloConfig>(config)
            .AddModule(new HelloPlugin(config).Module));
        chain.Container.Resolve<HeadLogger>();
        long seenBefore = Metrics.HeadBlocksSeen;

        Block block = await chain.AddBlock();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Metrics.HeadBlocksSeen, Is.EqualTo(seenBefore + 1));
            Assert.That(Metrics.LastHeadBlockNumber, Is.EqualTo(block.Number));
        }
    }
}
