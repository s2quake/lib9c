namespace Lib9c.Tests.Model.Guild
{
    using System.Threading.Tasks;
    using Libplanet.Crypto;
    using VerifyTests;
    using VerifyXunit;
    using Xunit;

    [UsesVerify]
    public class GuildTest
    {
        public GuildTest()
        {
            VerifierSettings.SortPropertiesAlphabetically();
        }

        [Fact]
        public Task Snapshot()
        {
            var guild = new Nekoyume.Model.Guild.Guild(
                new Address("0xd928ae87311dead490c986c24cc23c37eff892f2"));

            return Verifier.Verify(guild.Bencoded);
        }

        [Fact]
        public void Serialization()
        {
            var guild = new Nekoyume.Model.Guild.Guild(
                new PrivateKey().Address);
            var newGuild = new Nekoyume.Model.Guild.Guild(guild.Bencoded);

            Assert.Equal(guild.GuildMaster, newGuild.GuildMaster);
        }
    }
}
