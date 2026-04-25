using NiftyOptionsAlgo.Core;

namespace NiftyOptionsAlgo.Core.Tests;

public class EnumTests
{
    [Fact]
    public void VixRegimeEnum_AllValuesExist()
    {
        Assert.Equal(VixRegime.DangerZone, VixRegime.DangerZone);
        Assert.Equal(VixRegime.Caution, VixRegime.Caution);
        Assert.Equal(VixRegime.SweetSpot, VixRegime.SweetSpot);
        Assert.Equal(VixRegime.Elevated, VixRegime.Elevated);
        Assert.Equal(VixRegime.HighRisk, VixRegime.HighRisk);
        Assert.Equal(VixRegime.Crisis, VixRegime.Crisis);
    }

    [Fact]
    public void TradeStatusEnum_AllValuesExist()
    {
        Assert.Equal(TradeStatus.Pending, TradeStatus.Pending);
        Assert.Equal(TradeStatus.Open, TradeStatus.Open);
        Assert.Equal(TradeStatus.Adjusting, TradeStatus.Adjusting);
        Assert.Equal(TradeStatus.Closed, TradeStatus.Closed);
    }

    [Fact]
    public void OptionTypeEnum_AllValuesExist()
    {
        Assert.Equal(OptionType.CE, OptionType.CE);
        Assert.Equal(OptionType.PE, OptionType.PE);
    }

    [Fact]
    public void LegTypeEnum_AllValuesExist()
    {
        Assert.Equal(LegType.ShortPE, LegType.ShortPE);
        Assert.Equal(LegType.ShortCE, LegType.ShortCE);
        Assert.Equal(LegType.LongPE, LegType.LongPE);
        Assert.Equal(LegType.LongCE, LegType.LongCE);
    }

    [Fact]
    public void StrategyTypeEnum_AllValuesExist()
    {
        Assert.Equal(StrategyType.S1_Strangle, StrategyType.S1_Strangle);
        Assert.Equal(StrategyType.S2_IronCondor, StrategyType.S2_IronCondor);
        Assert.Equal(StrategyType.S3_Butterfly, StrategyType.S3_Butterfly);
        Assert.Equal(StrategyType.S4_Calendar, StrategyType.S4_Calendar);
    }

    [Fact]
    public void ExitReasonEnum_AllValuesExist()
    {
        Assert.Equal(ExitReason.ProfitTarget, ExitReason.ProfitTarget);
        Assert.Equal(ExitReason.StopLoss, ExitReason.StopLoss);
        Assert.Equal(ExitReason.GttFired, ExitReason.GttFired);
        Assert.Equal(ExitReason.TimestopDte21, ExitReason.TimestopDte21);
        Assert.Equal(ExitReason.ManualExit, ExitReason.ManualExit);
        Assert.Equal(ExitReason.EventRisk, ExitReason.EventRisk);
    }

    [Fact]
    public void VixDirectionEnum_AllValuesExist()
    {
        Assert.Equal(VixDirection.Rising, VixDirection.Rising);
        Assert.Equal(VixDirection.Falling, VixDirection.Falling);
        Assert.Equal(VixDirection.Stable, VixDirection.Stable);
    }
}
