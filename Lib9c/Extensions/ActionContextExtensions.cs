using Libplanet.Action;
using Nekoyume.TypedAddress;

namespace Nekoyume.Extensions
{
    public static class ActionContextExtensions
    {
        public static GuildAddress GetGuildAddress(this IActionContext context)
        {
            return new GuildAddress(context.Signer);
        }

        public static AgentAddress GetAgentAddress(this IActionContext context)
        {
            return new AgentAddress(context.Signer);
        }
    }
}
