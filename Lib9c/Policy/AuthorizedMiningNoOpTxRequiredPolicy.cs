namespace Nekoyume.BlockChain.Policy
{
    public static class AuthorizedMiningNoOpTxRequiredPolicy
    {
        public static VariableSubPolicy<bool> Default
        {
            get
            {
                return VariableSubPolicy<bool>
                    .Create(false);
            }
        }

        public static VariableSubPolicy<bool> Mainnet
        {
            get
            {
                return VariableSubPolicy<bool>
                    .Create(false)
                    .Add(new SpannedSubPolicy<bool>(
                        startIndex: BlockPolicySource.AuthorizedMiningNoOpTxRequiredStartIndex,
                        endIndex: BlockPolicySource.AuthorizedMinersPolicyEndIndex,
                        predicate: index => index % BlockPolicySource.AuthorizedMinersPolicyInterval == 0,
                        value: true));
            }
        }
    }
}
