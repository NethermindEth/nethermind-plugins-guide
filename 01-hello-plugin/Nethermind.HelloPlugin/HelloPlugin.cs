// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Autofac;
using Autofac.Core;
using Nethermind.Api.Extensions;
using Nethermind.Api.Steps;
using Nethermind.Core;

namespace Nethermind.HelloPlugin;

/// <summary>
/// The entry point the node discovers in its plugins folder.
/// </summary>
/// <remarks>
/// A plugin is created before the node is: its constructor can only take configs (any <c>IXConfig</c>),
/// <c>ChainSpec</c> and <c>IConfigProvider</c>. Everything else goes into <see cref="Module"/>, which the node adds to
/// its DI container only when <see cref="Enabled"/> is true.
/// </remarks>
public class HelloPlugin(IHelloConfig config) : INethermindPlugin
{
    public string Name => "Hello";
    public string Description => "Logs every new head block.";
    public string Author => "Nethermind";
    public bool Enabled => config.Enabled;
    public IModule Module => new HelloModule();
}

/// <summary>
/// Registers the plugin's services and startup step in the node's container.
/// </summary>
public class HelloModule : Module
{
    protected override void Load(ContainerBuilder builder) => builder
        .AddSingleton<HeadLogger>()
        .AddStep(typeof(StartHeadLogger));
}
