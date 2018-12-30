using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using System;
using System.Linq;

namespace NBitcoin.Altcoins
{
    public class Reex : NetworkSetBase
    {
        public static Reex Instance => new Reex();

        public override string CryptoCode => "REEX";

        public Reex()
        {

        }

        public class ReexConsensusFactory : ConsensusFactory
        {
            private ReexConsensusFactory()
            {
            }

            public static ReexConsensusFactory Instance { get; } = new ReexConsensusFactory();

            public override BlockHeader CreateBlockHeader()
            {
                return new ReexBlockHeader();
            }
            public override Block CreateBlock()
            {
                return new ReexBlock(new ReexBlockHeader());
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public class ReexBlockHeader : BlockHeader
        {
            // https://github.com/dashpay/dash/blob/e596762ca22d703a79c6880a9d3edb1c7c972fd3/src/primitives/block.cpp#L13
            static byte[] CalculateHash(byte[] data, int offset, int count)
            {
                return new NBitcoin.Altcoins.HashX11.X11().ComputeBytes(data.Skip(offset).Take(count).ToArray());
            }

            protected override HashStreamBase CreateHashStream()
            {
                return BufferedHashStream.CreateFrom(CalculateHash);
            }
        }

        public class ReexBlock : Block
        {
#pragma warning disable CS0612 // Type or member is obsolete
            public ReexBlock(ReexBlockHeader h) : base(h)
#pragma warning restore CS0612 // Type or member is obsolete
            {

            }
            public override ConsensusFactory GetConsensusFactory()
            {
                return Reex.Instance.Mainnet.Consensus.ConsensusFactory;
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        protected override void PostInit()
        {
            RegisterDefaultCookiePath("ReexCore");
        }

        static uint256 GetPoWHash(BlockHeader header)
        {
            var headerBytes = header.ToBytes();
            var h = SCrypt.ComputeDerivedKey(headerBytes, headerBytes, 1024, 1, 1, null, 32);
            return new uint256(h);
        }

        protected override NetworkBuilder CreateMainnet()
        {
            var builder = new NetworkBuilder();
            builder.SetConsensus(new Consensus()
            {
                SubsidyHalvingInterval = 210240,
                MajorityEnforceBlockUpgrade = 750,
                MajorityRejectBlockOutdated = 950,
                MajorityWindow = 1000,
                BIP34Hash = new uint256("0x000007d91d1254d60e2dd1ae580383070a4ddffa4c64c2eeb4a2f9ecc0414343"),
                PowLimit = new Target(new uint256("0x000007d91d1254d60e2dd1ae580383070a4ddffa4c64c2eeb4a2f9ecc0414343")),
                MinimumChainWork = new uint256("0x000000000000000000000000000000000000000000000100a308553b4863b755"),
                PowTargetTimespan = TimeSpan.FromSeconds(20 * 60),
                PowTargetSpacing = TimeSpan.FromSeconds(2 * 60),
                PowAllowMinDifficultyBlocks = false,
                CoinbaseMaturity = 10,
                PowNoRetargeting = false,
                RuleChangeActivationThreshold = 1916,
                MinerConfirmationWindow = 2016,
                ConsensusFactory = ReexConsensusFactory.Instance,
                SupportSegwit = true
            })
            .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 61 })
            .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 122 })
            .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 189 })
            .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x88, 0xB2, 0x1E })
            .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x88, 0xAD, 0xE4 })
            .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("reex"))
            .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("reex"))
            .SetMagic(0xBD6B0CBF)
            .SetPort(43210)
            .SetRPCPort(9998)
            .SetMaxP2PVersion(70208)
            .SetName("reex-main")
            .AddAlias("reex-mainnet")
            .AddDNSSeeds(new[]
            {
                new DNSSeedData("173.249.1.107", "173.249.1.107"),
                new DNSSeedData("35.237.166.130", "35.237.166.130"),
                new DNSSeedData("35.196.193.43", "35.196.193.43"),
                new DNSSeedData("35.231.79.133", "35.231.79.133")
            })
            .AddSeeds(new NetworkAddress[0])
            .SetGenesis("0000047d24635e347be3aaaeb66c26be94901a2f962feccd4f95090191f208c1");
            return builder;
        }

        protected override NetworkBuilder CreateTestnet()
        {
            var builder = new NetworkBuilder();
            var res = builder.SetConsensus(new Consensus()
            {
                SubsidyHalvingInterval = 210240,
                MajorityEnforceBlockUpgrade = 51,
                MajorityRejectBlockOutdated = 75,
                MajorityWindow = 100,
                BIP34Hash = new uint256("0x0000047d24635e347be3aaaeb66c26be94901a2f962feccd4f95090191f208c1"),
                PowLimit = new Target(new uint256("0x00000fffff000000000000000000000000000000000000000000000000000000")),
                MinimumChainWork = new uint256("0x000000000000000000000000000000000000000000000000000924e924a21715"),
                PowTargetTimespan = TimeSpan.FromSeconds(1 * 60),
                PowTargetSpacing = TimeSpan.FromSeconds(2 * 60),
                PowAllowMinDifficultyBlocks = true,
                CoinbaseMaturity = 15,
                PowNoRetargeting = false,
                RuleChangeActivationThreshold = 1512,
                MinerConfirmationWindow = 2016,
                ConsensusFactory = ReexConsensusFactory.Instance,
                SupportSegwit = true
            })
            .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 48 })
            .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 12 })
            .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 108 })
            .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
            .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
            .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("treex"))
            .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("treex"))
            .SetMagic(0xFFCAE2CE)
            .SetPort(15551)
            .SetRPCPort(19998)
            .SetMaxP2PVersion(70208)
           .SetName("reex-test")
           .AddAlias("reex-testnet")
           .AddDNSSeeds(new[]
           {
                new DNSSeedData("173.249.1.107",  "173.249.1.107"),
                //new DNSSeedData("masternode.io", "test.dnsseed.masternode.io")
           })
           .AddSeeds(new NetworkAddress[0])
           .SetGenesis("0000047d24635e347be3aaaeb66c26be94901a2f962feccd4f95090191f208c1");
            return builder;
        }

        protected override NetworkBuilder CreateRegtest()
        {
            var builder = new NetworkBuilder();
            builder.SetConsensus(new Consensus()
            {
                SubsidyHalvingInterval = 150,
                MajorityEnforceBlockUpgrade = 750,
                MajorityRejectBlockOutdated = 950,
                MajorityWindow = 1000,
                BIP34Hash = new uint256(),
                PowLimit = new Target(new uint256("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                MinimumChainWork = new uint256("0x000000000000000000000000000000000000000000000000000924e924a21715"),
                PowTargetTimespan = TimeSpan.FromSeconds(24 * 60 * 60),
                PowTargetSpacing = TimeSpan.FromSeconds(2.5 * 60),
                PowAllowMinDifficultyBlocks = true,
                CoinbaseMaturity = 100,
                PowNoRetargeting = true,
                RuleChangeActivationThreshold = 108,
                MinerConfirmationWindow = 144,
                ConsensusFactory = ReexConsensusFactory.Instance,
                SupportSegwit = false
            })
            .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 140 })
            .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 19 })
            .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 239 })
            .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
            .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
            .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("treex"))
            .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("treex"))
            .SetMagic(0xDCB7C1FC)
            .SetPort(19994)
            .SetRPCPort(19993)
            .SetMaxP2PVersion(70208)
            .SetName("reex-reg")
            .AddAlias("reex-regtest")
            .AddDNSSeeds(new DNSSeedData[0])
            .AddSeeds(new NetworkAddress[0])
            .SetGenesis("0000047d24635e347be3aaaeb66c26be94901a2f962feccd4f95090191f208c1");
            return builder;
        }
    }
}
