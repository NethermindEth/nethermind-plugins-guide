# Nethermind Plugins Guide

```sh
git clone https://github.com/NethermindEth/nethermind-plugins-guide.git --recursive --depth 1
```

### About the client

Nethermind application is an execution client for Ethereum, which means it is used:
- to confirm correctness of blocks appending to the chain: it runs transactions of a block one by one confirming that it leads to the same changes in state and balance
- to gossip transactions and blocks to other peers
- to provide RPC and HTTP API for other clients and users

The client also allows to monitor its state and performance, and to interact with it using the console. 

### Writing a plugin
    
The structure is modular and allows you to extend the client in different ways

| Module             | Description                                                                                                                                                   |
|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|
| RPC                | Adding additional RPC modules to the client that have full access to the internal Nethermind APIs and can extend capabilities of the node when integrating with your infrastructure / systems. |
| Block Tree Visitors| Code allowing you to analyze entire block tree from genesis to the head block and execute aggregated calculations and checks.                                  |
| Devp2p             | Allows you to create additional devp2p network protocol for your nodes to communicate over TCP/IP. You can also build custom products that will run attached to Nethermind nodes.          |
| State Visitors     | Allow you to run aggregated analysis on the entire raw format state (or just some accounts storages).                                                         |
| Config             | You can add additional configuration categories to our config files and then use them in env variables, json files or command line to configure behaviour of your plugins.                 |
| TxPool             | TxPool behaviours and listeners.                                                                                                                               |
| Tracers            | Custom, powerful EVM tracers capable of extracting elements of EVM execution in real time.                                                                     |
| CLI                | Additional modules for Nethermind CLI that can allow you build some quick scratchpad style JavaScript based behaviors.                                        |


**Note:** When writing a plugin be carefull about exceptions you throw. Especially if you are hooking up event handlers on some core objects like BlockProcessor or BlockTree. Those exceptions can bring the node down. This is by design. Its responsibility of plugin writer to correctly handle those exceptions.

## Examples

### 01 Hello Ethereum, Blockchain Processor, Lifetime of a plugin, NUnit

Make a plugin, start debugging, check incoming blocks, log something, close properly


### 02 Useful types tools and stuff

Tools:
EthereumEscda
Hash256
Address
UInt256
Hex
Rlp

Block
Transaction
TimerFactory
ReleaseSpec
TimeStamper


### 02 TxPool, Transaction Processor, State, Evm


### 03 Rpc, cli, networking protocol


### 01 Packaging and distribution of your plugin 


## Need help?

Docs: https://docs.nethermind.io/developers/plugins/

Discord: #nethermind-plugins https://discord.gg/DH2evNGNeA

