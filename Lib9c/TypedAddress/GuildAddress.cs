using Bencodex;
using Bencodex.Types;

namespace Nekoyume.TypedAddress
{
    using Address = Libplanet.Crypto.Address;

    public readonly struct GuildAddress : IBencodable
    {
        private readonly Address _address;

        public GuildAddress(byte[] bytes) : this(new Address(bytes))
        {
        }

        public GuildAddress(IValue value) : this(new Address(value))
        {
        }

        public GuildAddress(Address address)
        {
            _address = address;
        }

        public static implicit operator Address(GuildAddress guildAddress)
        {
            return guildAddress._address;
        }

        public IValue Bencoded => _address.Bencoded;
    }
}
