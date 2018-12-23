using NBitcoin;
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

        public override string CryptoCode => "DASH";

        public Reex()
        {

        }

        public class DashConsensusFactory : ConsensusFactory
        {
            private DashConsensusFactory()
            {
            }

            public static DashConsensusFactory Instance { get; } = new DashConsensusFactory();

            public override BlockHeader CreateBlockHeader()
            {
                return new DashBlockHeader();
            }
            public override Block CreateBlock()
            {
                return new ReexBlock(new DashBlockHeader());
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public class DashBlockHeader : BlockHeader
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
            public ReexBlock(DashBlockHeader h) : base(h)
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
            RegisterDefaultCookiePath("DashCore");
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
                ConsensusFactory = DashConsensusFactory.Instance,
                SupportSegwit = true
            })
            .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 61 })
            .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 122 })
            .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 189 })
            .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x88, 0xB2, 0x1E })
            .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x88, 0xAD, 0xE4 })
            .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("dash"))
            .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("dash"))
            .SetMagic(0xBD6B0CBF)
            .SetPort(43210)
            .SetRPCPort(9998)
            .SetMaxP2PVersion(70208)
            .SetName("dash-main")
            .AddAlias("dash-mainnet")
            .AddDNSSeeds(new[]
            {
                new DNSSeedData("173.249.1.107", "173.249.1.107"),
                new DNSSeedData("35.237.166.130", "35.237.166.130"),
                new DNSSeedData("35.196.193.43", "35.196.193.43"),
                new DNSSeedData("35.231.79.133", "35.231.79.133")
            })
            .AddSeeds(new NetworkAddress[0])
            .SetGenesis("0x00000c1b8abb8755561c46ea298cf725c940ca71409f7024bc3ad82fdb1bdc7f");
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
                ConsensusFactory = DashConsensusFactory.Instance,
                SupportSegwit = true
            })
            .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 48 })
            .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 12 })
            .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 108 })
            .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
            .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
            .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("tdash"))
            .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("tdash"))
            .SetMagic(0xFFCAE2CE)
            .SetPort(15551)
            .SetRPCPort(19998)
            .SetMaxP2PVersion(70208)
           .SetName("dash-test")
           .AddAlias("dash-testnet")
           .AddDNSSeeds(new[]
           {
                new DNSSeedData("173.249.1.107",  "173.249.1.107"),
                //new DNSSeedData("masternode.io", "test.dnsseed.masternode.io")
           })
           .AddSeeds(new NetworkAddress[0])
           .SetGenesis("0x06b942b8b7f0e05ea38e7871a0db70f71592cfb016ee0ae7d5988f9fea840201");
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
                ConsensusFactory = DashConsensusFactory.Instance,
                SupportSegwit = false
            })
            .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 140 })
            .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 19 })
            .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 239 })
            .SetBase58Bytes(Base58Type.EXT_PUBLIC_KEY, new byte[] { 0x04, 0x35, 0x87, 0xCF })
            .SetBase58Bytes(Base58Type.EXT_SECRET_KEY, new byte[] { 0x04, 0x35, 0x83, 0x94 })
            .SetBech32(Bech32Type.WITNESS_PUBKEY_ADDRESS, Encoders.Bech32("tdash"))
            .SetBech32(Bech32Type.WITNESS_SCRIPT_ADDRESS, Encoders.Bech32("tdash"))
            .SetMagic(0xDCB7C1FC)
            .SetPort(19994)
            .SetRPCPort(19993)
            .SetMaxP2PVersion(70208)
            .SetName("dash-reg")
            .AddAlias("dash-regtest")
            .AddDNSSeeds(new DNSSeedData[0])
            .AddSeeds(new NetworkAddress[0])
            .SetGenesis("010000000000000000000000000000000000000000000000000000000000000000000000c762a6567f3cc092f0684bb62b7e00a84890b990f07cc71a6bb58d64b98e02e0b9968054ffff7f20ffba10000101000000010000000000000000000000000000000000000000000000000000000000000000ffffffff6204ffff001d01044c5957697265642030392f4a616e2f3230313420546865204772616e64204578706572696d656e7420476f6573204c6976653a204f76657273746f636b2e636f6d204973204e6f7720416363657074696e6720426974636f696e73ffffffff0100f2052a010000004341040184710fa689ad5023690c80f3a49c8f13f8d45b8c857fbcbc8bc4a8e4d3eb4b10f4d4604fa08dce601aaf0f470216fe1b51850b4acf21b179c45070ac7b03a9ac00000000");
            return builder;
        }
    }
}
