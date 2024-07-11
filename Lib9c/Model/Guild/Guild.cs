using System;
using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Action;

namespace Nekoyume.Model.Guild
{
    public class Guild : IEquatable<Guild>, IBencodable
    {
        private const string StateTypeName = "guild";
        private const long StateVersion = 1;

        public readonly Address GuildMaster;

        public Guild(Address guildMaster)
        {
            GuildMaster = guildMaster;
        }

        public Guild(List list) : this(
            new Address(list[2]))
        {
            if (list[0] is not Text text || text != StateTypeName || list[1] is not Integer integer)
            {
                throw new InvalidCastException();
            }

            if (integer > StateVersion)
            {
                throw new FailedLoadStateException("Un-deserializable state.");
            }
        }

        public List Bencoded => List.Empty
            .Add(StateTypeName)
            .Add(StateVersion)
            .Add(GuildMaster.Bencoded);

        IValue IBencodable.Bencoded => Bencoded;

        public bool Equals(Guild other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GuildMaster.Equals(other.GuildMaster);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Guild)obj);
        }

        public override int GetHashCode()
        {
            return GuildMaster.GetHashCode();
        }
    }
}
