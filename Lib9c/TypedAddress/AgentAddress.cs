using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;

namespace Nekoyume.TypedAddress
{
    public readonly struct AgentAddress : IBencodable
    {
        private readonly Address _address;

        public AgentAddress(byte[] bytes) : this(new Address(bytes))
        {
        }

        public AgentAddress(Address address)
        {
            _address = address;
        }

        public static implicit operator Address(AgentAddress agentAddress)
        {
            return agentAddress._address;
        }

        public IValue Bencoded => _address.Bencoded;
    }
}
