#nullable enable
namespace Lib9c.Tests.Action.ValidatorDelegation;

using Nekoyume.ValidatorDelegation;
using Xunit;

public class ConstantTest
{
    [Fact]
    public void StaticPropertyTest()
    {
        Assert.True(ValidatorDelegatee.ValidatorUnbondingPeriod > 0);
        Assert.True(ValidatorDelegatee.MaxCommissionPercentage < int.MaxValue);
        Assert.True(ValidatorDelegatee.MaxCommissionPercentage >= 0);
    }
}