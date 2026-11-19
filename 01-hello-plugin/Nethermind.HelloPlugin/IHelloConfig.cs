// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Config;

namespace Nethermind.HelloPlugin;

/// <summary>
/// The configuration of the hello plugin, in the "Hello" category.
/// </summary>
/// <remarks>
/// The category is the interface name without the leading "I" and the trailing "Config". The values can be set in a
/// JSON config (<c>{"Hello": {"Enabled": true}}</c>), on the command line (<c>--Hello.Enabled true</c>) or in the
/// environment (<c>NETHERMIND_HELLOCONFIG_ENABLED=true</c>).
/// </remarks>
[ConfigCategory(Description = "Configuration of the hello plugin.")]
public interface IHelloConfig : IConfig
{
    // DefaultValue only documents the default for --help and the docs; the real default is set in HelloConfig.
    [ConfigItem(Description = "Whether to enable the hello plugin.", DefaultValue = "false")]
    bool Enabled { get; set; }

    [ConfigItem(Description = "The greeting logged for every new head block.", DefaultValue = "Hello")]
    string Greeting { get; set; }
}
