// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

namespace Nethermind.HelloPlugin;

// The implementation must be named like the interface without the leading "I": the command line and the environment
// variables are derived from this name.
public class HelloConfig : IHelloConfig
{
    public bool Enabled { get; set; }
    public string Greeting { get; set; } = "Hello";
}
