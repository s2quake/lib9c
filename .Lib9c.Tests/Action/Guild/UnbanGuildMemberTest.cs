namespace Lib9c.Tests.Action.Guild
{
    using System;
    using Libplanet.Action.State;
    using Libplanet.Crypto;
    using Libplanet.Mocks;
    using Nekoyume.Action.Guild;
    using Nekoyume.Action.Loader;
    using Nekoyume.Model.Guild;
    using Nekoyume.Module.Guild;
    using Nekoyume.TypedAddress;
    using Xunit;

    public class UnbanGuildMemberTest
    {
        [Fact]
        public void Serialization()
        {
            var guildMemberAddress = new PrivateKey().Address;
            var action = new UnbanGuildMember(guildMemberAddress);
            var plainValue = action.PlainValue;

            var actionLoader = new NCActionLoader();
            var loadedRaw = actionLoader.LoadAction(0, plainValue);
            var loadedAction = Assert.IsType<UnbanGuildMember>(loadedRaw);
            Assert.Equal(guildMemberAddress, loadedAction.Target);
        }

        [Fact]
        public void Unban_By_GuildMember()
        {
            PrivateKey guildMaster = new (), guildMember = new (), targetGuildMember = new ();
            var guildMasterAddress = new AgentAddress(guildMaster.Address);
            var guildMemberAddress = new AgentAddress(guildMember.Address);
            var targetGuildMemberAddress = new AgentAddress(targetGuildMember.Address);
            var guildAddress = new GuildAddress(guildMasterAddress);

            var action = new UnbanGuildMember(targetGuildMemberAddress);

            IWorld world = new World(MockWorldState.CreateModern());
            world = world.SetGuild(guildAddress, new Nekoyume.Model.Guild.Guild(guildMasterAddress))
                .SetGuildParticipant(guildMasterAddress, new GuildParticipant(guildAddress))
                .SetGuildParticipant(guildMemberAddress, new GuildParticipant(guildAddress))
                .SetGuildParticipant(targetGuildMemberAddress, new GuildParticipant(guildAddress));

            // GuildMember tries to ban other guild member.
            Assert.Throws<InvalidOperationException>(() => action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = guildMemberAddress,
            }));

            // GuildMember tries to ban itself.
            Assert.Throws<InvalidOperationException>(() => action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = targetGuildMemberAddress,
            }));
        }

        [Fact]
        public void Unban_By_GuildMaster()
        {
            PrivateKey guildMaster = new (), targetGuildMember = new ();
            var guildMasterAddress = new AgentAddress(guildMaster.Address);
            var targetGuildMemberAddress = new AgentAddress(targetGuildMember.Address);
            var guildAddress = new GuildAddress(guildMasterAddress);

            var action = new UnbanGuildMember(targetGuildMemberAddress);

            IWorld world = new World(MockWorldState.CreateModern());
            world = world.SetGuild(guildAddress, new Nekoyume.Model.Guild.Guild(guildMasterAddress))
                .SetGuildParticipant(guildMasterAddress, new GuildParticipant(guildAddress))
                .Ban(guildAddress, targetGuildMemberAddress);

            Assert.True(world.IsBanned(guildAddress, targetGuildMemberAddress));
            Assert.False(world.IsMemberOfGuild(targetGuildMemberAddress, guildAddress));

            world = action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = guildMasterAddress,
            });

            Assert.False(world.IsBanned(guildAddress, targetGuildMemberAddress));
            Assert.False(world.IsMemberOfGuild(targetGuildMemberAddress, guildAddress));
        }
    }
}
