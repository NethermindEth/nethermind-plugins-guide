// SPDX-FileCopyrightText: 2026 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;
using Nethermind.Core.Specs;
using Nethermind.Core.Test.Builders;
using Nethermind.Crypto;
using Nethermind.Int256;
using Nethermind.Serialization.Rlp;
using Nethermind.Serialization.Rlp.TxDecoders;
using Nethermind.Specs;
using NUnit.Framework;

namespace Nethermind.CoreTypes.Test;

/// <summary>
/// The core types every plugin works with, one fact per test.
/// </summary>
public class CoreTypesTests
{
    [Test]
    public void Hashes_are_keccak256()
    {
        // Hash256 is an immutable class; ValueHash256 is its struct twin for hot paths that must not allocate.
        Hash256 hash = Keccak.Compute("");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(hash, Is.EqualTo(Keccak.OfAnEmptyString));
            Assert.That(hash.ToString(), Is.EqualTo("0xc5d2460186f7233c927e7db2dcc703c0e500b653ca82273b7bfad8045d85a470"));
            Assert.That(hash.ValueHash256, Is.EqualTo(ValueKeccak.OfAnEmptyString));
        }
    }

    [Test]
    public void Addresses_parse_from_hex_and_derive_from_keys()
    {
        Address parsed = new("0x7e5f4552091a69125d5dfcb7b8c2659029395bdf");
        PrivateKey key = new("0000000000000000000000000000000000000000000000000000000000000001");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(key.Address, Is.EqualTo(parsed));
            // Addresses print in lower case; ToString(true, true) gives the EIP-55 checksummed form.
            Assert.That(parsed.ToString(withZeroX: true, withEip55Checksum: true), Is.EqualTo("0x7E5F4552091A69125d5DfCb7b8C2659029395Bdf"));
        }
    }

    [Test]
    public void Signatures_recover_the_signing_address()
    {
        EthereumEcdsa ecdsa = new(BlockchainIds.Mainnet);
        Hash256 message = Keccak.Compute("hello");

        Signature signature = ecdsa.Sign(TestItem.PrivateKeyA, message);

        Assert.That(ecdsa.RecoverAddress(signature, message), Is.EqualTo(TestItem.AddressA));
    }

    [Test]
    public void Amounts_are_uint256_in_wei()
    {
        UInt256 oneEther = 1.Ether;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(oneEther, Is.EqualTo(UInt256.Parse("1000000000000000000")));
            Assert.That(1.GWei * 1_000_000_000, Is.EqualTo(oneEther));
            // UInt256 arithmetic wraps around like the EVM does; use the checked helpers when that matters.
            Assert.That(UInt256.MaxValue + UInt256.One, Is.EqualTo(UInt256.Zero));
            Assert.That(UInt256.AddOverflow(UInt256.MaxValue, UInt256.One, out _), Is.True);
        }
    }

    [Test]
    public void Bytes_convert_to_and_from_hex()
    {
        byte[] bytes = Bytes.FromHexString("0xdeadbeef");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(bytes, Is.EqualTo(new byte[] { 0xde, 0xad, 0xbe, 0xef }));
            Assert.That(bytes.ToHexString(withZeroX: true), Is.EqualTo("0xdeadbeef"));
        }
    }

    [Test]
    public void Rlp_encodes_numbers_and_round_trips_transactions()
    {
        // Small numbers encode as themselves; larger ones get a length prefix.
        Assert.That(Rlp.Encode(127UL).Bytes, Is.EqualTo(new byte[] { 0x7f }));
        Assert.That(Rlp.Encode(1024UL).Bytes, Is.EqualTo(new byte[] { 0x82, 0x04, 0x00 }));

        // The test builders make valid, signed objects without boilerplate.
        Transaction tx = Build.A.Transaction.WithTo(TestItem.AddressB).WithValue(1.Ether).SignedAndResolved(TestItem.PrivateKeyA).TestObject;
        Rlp rlp = TxDecoder.Instance.Encode(tx);
        RlpReader reader = new(rlp.Bytes);
        Transaction decoded = TxDecoder.Instance.Decode(ref reader)!;

        using (Assert.EnterMultipleScope())
        {
            // A transaction hash is the keccak of its encoding.
            Assert.That(Keccak.Compute(rlp.Bytes), Is.EqualTo(tx.Hash));
            Assert.That(decoded.To, Is.EqualTo(TestItem.AddressB));
            Assert.That(decoded.Value, Is.EqualTo(1.Ether));
        }
    }

    [Test]
    public void Blocks_carry_a_header_and_a_body()
    {
        Transaction tx = Build.A.Transaction.SignedAndResolved(TestItem.PrivateKeyA).TestObject;
        Block block = Build.A.Block.WithNumber(10).WithTransactions(tx).TestObject;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(block.Number, Is.EqualTo(block.Header.Number));
            Assert.That(block.Transactions, Is.EqualTo(new[] { tx }));
            Assert.That(block.Hash, Is.EqualTo(block.Header.Hash));
        }
    }

    [Test]
    public void Fork_rules_come_from_the_spec_provider()
    {
        // An IReleaseSpec answers "is this rule active?" for one fork. Ask the node's ISpecProvider for the spec of the
        // block being handled rather than assuming the latest fork.
        ISpecProvider specProvider = MainnetSpecProvider.Instance;
        IReleaseSpec london = specProvider.GetSpec(new ForkActivation(MainnetSpecProvider.LondonBlockNumber));
        IReleaseSpec berlin = specProvider.GetSpec(new ForkActivation(MainnetSpecProvider.LondonBlockNumber - 1));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(london.IsEip1559Enabled, Is.True);
            Assert.That(berlin.IsEip1559Enabled, Is.False);
            Assert.That(specProvider.ChainId, Is.EqualTo(BlockchainIds.Mainnet));
        }
    }
}
