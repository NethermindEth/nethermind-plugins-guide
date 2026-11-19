// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Api.Steps;
using Nethermind.Init.Steps;

namespace Nethermind.HelloPlugin;

/// <summary>
/// Creates the <see cref="HeadLogger"/> during startup, after the block tree exists and before any block is processed.
/// </summary>
/// <remarks>
/// The container creates services lazily, when something first asks for them. A step is the usual way to make that
/// happen at a known point of the startup: the <c>dependencies</c> run before this step, the <c>dependents</c> after it.
/// </remarks>
[RunnerStepDependencies(
    dependencies: [typeof(InitializeBlockchain)],
    dependents: [typeof(StartBlockProcessor)])]
public class StartHeadLogger(HeadLogger headLogger) : IStep
{
    public Task Execute(CancellationToken cancellationToken)
    {
        _ = headLogger;
        return Task.CompletedTask;
    }
}
