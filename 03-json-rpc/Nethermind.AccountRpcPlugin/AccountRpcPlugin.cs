// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Autofac;
using Autofac.Core;
using Nethermind.Api.Extensions;
using Nethermind.JsonRpc.Modules;

namespace Nethermind.AccountRpcPlugin;

/// <summary>
/// Adds the <c>account</c> JSON-RPC namespace.
/// </summary>
/// <remarks>
/// The plugin is always enabled: whether the namespace is served is already the operator's choice, made with
/// <c>JsonRpc.EnabledModules</c>, so it needs no config of its own.
/// </remarks>
public class AccountRpcPlugin : INethermindPlugin
{
    public string Name => "AccountRpc";
    public string Description => "Adds the account JSON-RPC namespace.";
    public string Author => "Nethermind";
    public bool Enabled => true;
    public IModule Module => new AccountRpcModuleRegistration();
}

public class AccountRpcModuleRegistration : Module
{
    // A singleton module serves all requests with one instance, so it must be thread-safe. A module that is not can be
    // pooled instead, with RegisterBoundedJsonRpcModule and a factory.
    protected override void Load(ContainerBuilder builder) =>
        builder.RegisterSingletonJsonRpcModule<IAccountRpcModule, AccountRpcModule>();
}
