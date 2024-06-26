using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Action;

namespace Nekoyume.Model.State
{
    public class RuneState : IState
    {
        public static Address DeriveAddress(Address avatarAddress, int runeId) =>
            avatarAddress.Derive($"{runeId}");

        public int RuneId { get; }
        public int Level { get; private set; }

        public RuneState(int runeId)
        {
            RuneId = runeId;
        }

        public RuneState(int runeId, int level)
        {
            RuneId = runeId;
            Level = level;
        }

        public RuneState(List serialized)
        {
            RuneId = serialized[0].ToInteger();
            Level = serialized[1].ToInteger();
        }

        public IValue Serialize()
        {
            var result = List.Empty
                .Add(RuneId.Serialize())
                .Add(Level.Serialize());
            return result;
        }

        public void LevelUp(int level = 1)
        {
            Level += level;
        }
    }
}
