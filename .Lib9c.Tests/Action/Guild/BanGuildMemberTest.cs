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

    public class BanGuildMemberTest
    {
        // Expected use-case.
        [Fact]
        public void Ban_By_GuildMaster()
        {
            PrivateKey guildMaster = new (), guildMember = new (), otherGuildMember = new (),
                otherGuildMaster = new ();
            var guildMasterAddress = new AgentAddress(guildMaster.Address);
            var otherGuildMasterAddress = new AgentAddress(otherGuildMaster.Address);
            var guildMemberAddress = new AgentAddress(guildMember.Address);
            var otherGuildMemberAddress = new AgentAddress(otherGuildMember.Address);
            var guildAddress = new GuildAddress(guildMasterAddress);
            var otherGuildAddress = new GuildAddress(otherGuildMasterAddress);

            IWorld world = new World(MockWorldState.CreateModern());
            world = world.SetGuild(guildAddress, new Nekoyume.Model.Guild.Guild(guildMasterAddress))
                .SetGuildParticipant(guildMasterAddress, new GuildParticipant(guildAddress))
                .SetGuildParticipant(guildMemberAddress, new GuildParticipant(guildAddress));
            world = world.SetGuild(otherGuildAddress, new Guild(otherGuildMasterAddress))
                .SetGuildParticipant(otherGuildMasterAddress, new GuildParticipant(otherGuildMasterAddress))
                .SetGuildParticipant(otherGuildMemberAddress, new GuildParticipant(otherGuildMemberAddress));

            // Guild
            Assert.False(world.IsBanned(guildAddress, guildMasterAddress));
            Assert.True(world.IsMemberOfGuild(guildMasterAddress, guildAddress));
            Assert.False(world.IsBanned(guildAddress, guildMemberAddress));
            Assert.True(world.IsMemberOfGuild(guildMemberAddress, guildAddress));
            // Other guild
            Assert.False(world.IsBanned(guildAddress, otherGuildMasterAddress));
            Assert.False(world.IsMemberOfGuild(otherGuildMasterAddress, guildAddress));
            Assert.False(world.IsBanned(guildAddress, otherGuildMemberAddress));
            Assert.False(world.IsMemberOfGuild(otherGuildMemberAddress, guildAddress));

            var action = new BanGuildMember(guildMemberAddress);
            world = action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = guildMasterAddress,
            });

            // Guild
            Assert.False(world.IsBanned(guildAddress, guildMasterAddress));
            Assert.True(world.IsMemberOfGuild(guildMasterAddress, guildAddress));
            Assert.True(world.IsBanned(guildAddress, guildMemberAddress));
            Assert.False(world.IsMemberOfGuild(guildMemberAddress, guildAddress));
            // Other guild
            Assert.False(world.IsBanned(guildAddress, otherGuildMasterAddress));
            Assert.False(world.IsMemberOfGuild(otherGuildMasterAddress, guildAddress));
            Assert.False(world.IsBanned(guildAddress, otherGuildMemberAddress));
            Assert.False(world.IsMemberOfGuild(otherGuildMemberAddress, guildAddress));

            action = new BanGuildMember(otherGuildMasterAddress);
            world = action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = guildMasterAddress,
            });

            // Guild
            Assert.False(world.IsBanned(guildAddress, guildMasterAddress));
            Assert.True(world.IsMemberOfGuild(guildMasterAddress, guildAddress));
            Assert.True(world.IsBanned(guildAddress, guildMemberAddress));
            Assert.False(world.IsMemberOfGuild(guildMemberAddress, guildAddress));
            // Other guild
            Assert.True(world.IsBanned(guildAddress, otherGuildMasterAddress));
            Assert.False(world.IsMemberOfGuild(otherGuildMasterAddress, guildAddress));
            Assert.False(world.IsBanned(guildAddress, otherGuildMemberAddress));
            Assert.False(world.IsMemberOfGuild(otherGuildMemberAddress, guildAddress));

            action = new BanGuildMember(otherGuildMemberAddress);
            world = action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = guildMasterAddress,
            });

            // Guild
            Assert.False(world.IsBanned(guildAddress, guildMasterAddress));
            Assert.True(world.IsMemberOfGuild(guildMasterAddress, guildAddress));
            Assert.True(world.IsBanned(guildAddress, guildMemberAddress));
            Assert.False(world.IsMemberOfGuild(guildMemberAddress, guildAddress));
            // Other guild
            Assert.True(world.IsBanned(guildAddress, otherGuildMasterAddress));
            Assert.False(world.IsMemberOfGuild(otherGuildMasterAddress, guildAddress));
            Assert.True(world.IsBanned(guildAddress, otherGuildMemberAddress));
            Assert.False(world.IsMemberOfGuild(otherGuildMemberAddress, guildAddress));

            action = new BanGuildMember(guildMasterAddress);
            // GuildMaster cannot ban itself.
            Assert.Throws<InvalidOperationException>(() => action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = guildMasterAddress,
            }));
        }

        [Fact]
        public void Serialization()
        {
            var guildMemberAddress = new PrivateKey().Address;
            var action = new BanGuildMember(guildMemberAddress);
            var plainValue = action.PlainValue;

            var actionLoader = new NCActionLoader();
            var loadedRaw = actionLoader.LoadAction(0, plainValue);
            var loadedAction = Assert.IsType<BanGuildMember>(loadedRaw);
            Assert.Equal(guildMemberAddress, loadedAction.Target);
        }

        [Fact]
        public void Ban_By_GuildMember()
        {
            PrivateKey guildMaster = new (),
                other = new (),
                guildMember = new (),
                targetGuildMember = new ();
            var guildMasterAddress = new AgentAddress(guildMaster.Address);
            var guildMemberAddress = new AgentAddress(guildMember.Address);
            var otherAddress = new AgentAddress(other.Address);
            var targetGuildMemberAddress = new AgentAddress(targetGuildMember.Address);
            var guildAddress = new GuildAddress(guildMasterAddress);

            var action = new BanGuildMember(targetGuildMemberAddress);

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
            action = new BanGuildMember(guildMemberAddress);
            Assert.Throws<InvalidOperationException>(() => action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = guildMemberAddress,
            }));

            action = new BanGuildMember(otherAddress);
            // GuildMember tries to ban other not joined to its guild.
            Assert.Throws<InvalidOperationException>(() => action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = guildMemberAddress,
            }));
        }

        [Fact]
        public void Ban_By_Other()
        {
            // NOTE: It assumes 'other' hasn't any guild. If 'other' has its own guild,
            //       it should be assumed as a guild master.
            PrivateKey guildMaster = new (), other = new (), targetGuildMember = new ();
            var guildMasterAddress = new AgentAddress(guildMaster.Address);
            var otherAddress = new AgentAddress(other.Address);
            var targetGuildMemberAddress = new AgentAddress(targetGuildMember.Address);
            var guildAddress = new GuildAddress(guildMasterAddress);

            IWorld world = new World(MockWorldState.CreateModern());
            world = world.SetGuild(guildAddress, new Nekoyume.Model.Guild.Guild(guildMasterAddress))
                .SetGuildParticipant(guildMasterAddress, new GuildParticipant(guildAddress))
                .SetGuildParticipant(targetGuildMemberAddress, new GuildParticipant(guildAddress));

            // Other tries to ban GuildMember.
            var action = new BanGuildMember(targetGuildMemberAddress);
            Assert.Throws<InvalidOperationException>(() => action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = otherAddress,
            }));

            // Other tries to ban GuildMaster.
            action = new BanGuildMember(guildMasterAddress);
            Assert.Throws<InvalidOperationException>(() => action.Execute(new ActionContext
            {
                PreviousState = world,
                Signer = otherAddress,
            }));
        }
    }
}
