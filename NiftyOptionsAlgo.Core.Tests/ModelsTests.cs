using NiftyOptionsAlgo.Core;

namespace NiftyOptionsAlgo.Core.Tests;

public class StrategyConfigTests
{
    [Fact]
    public void StrategyConfig_DefaultConstruction_HasExpectedDefaults()
    {
        // Arrange & Act
        var config = new StrategyConfig();

        // Assert
        Assert.Equal(12.0m, config.VixDangerZone);
        Assert.Equal(14.0m, config.VixCautionLow);
        Assert.Equal(14.0m, config.VixSweetSpotLow);
        Assert.Equal(18.0m, config.VixSweetSpotHigh);
        Assert.Equal(22.0m, config.VixElevatedHigh);
        Assert.Equal(28.0m, config.VixCrisisThreshold);
        Assert.Equal(1000000m, config.CapitalTotal);
        Assert.Equal(0.02m, config.MaxRiskPerTradePercent);
        Assert.Equal(0.30m, config.ReservePercent);
        Assert.Equal(25, config.NiftyLotSize);
        Assert.Equal(45, config.MinDteForEntry);
        Assert.Equal(10, config.EntryWindowStartHour);
        Assert.Equal(11, config.EntryWindowEndHour);
    }

    [Fact]
    public void StrategyConfig_PropertyCanBeModified_UpdatesValue()
    {
        // Arrange
        var config = new StrategyConfig();

        // Act
        config.VixDangerZone = 15.0m;
        config.CapitalTotal = 500000m;

        // Assert
        Assert.Equal(15.0m, config.VixDangerZone);
        Assert.Equal(500000m, config.CapitalTotal);
    }

    [Fact]
    public void StrategyConfig_AllPropertiesAssignable_CanSetAndRetrieve()
    {
        // Arrange
        var config = new StrategyConfig
        {
            VixDangerZone = 11.5m,
            MaxRiskPerTradePercent = 0.03m,
            NiftyLotSize = 50,
            MaxAdjustments = 3,
            ProfitTargetPercent = 0.60m
        };

        // Assert
        Assert.Equal(11.5m, config.VixDangerZone);
        Assert.Equal(0.03m, config.MaxRiskPerTradePercent);
        Assert.Equal(50, config.NiftyLotSize);
        Assert.Equal(3, config.MaxAdjustments);
        Assert.Equal(0.60m, config.ProfitTargetPercent);
    }
}

public class StrangleTradeTests
{
    [Fact]
    public void StrangleTrade_DefaultConstruction_HasEmptyCollections()
    {
        // Arrange & Act
        var trade = new StrangleTrade();

        // Assert
        Assert.NotNull(trade.Legs);
        Assert.Empty(trade.Legs);
        Assert.NotNull(trade.Adjustments);
        Assert.Empty(trade.Adjustments);
    }

    [Fact]
    public void StrangleTrade_PropertiesCanBeSet_UpdatesValues()
    {
        // Arrange
        var tradeId = Guid.NewGuid();
        var entryDate = DateTime.Now;
        var expiryDate = DateTime.Now.AddDays(30);

        // Act
        var trade = new StrangleTrade
        {
            Id = tradeId,
            EntryDate = entryDate,
            ExpiryDate = expiryDate,
            Status = TradeStatus.Open,
            Strategy = StrategyType.S1_Strangle,
            Lots = 3,
            NiftySpotAtEntry = 20000m,
            VixAtEntry = 15.5m,
            DteAtEntry = 30,
            TotalPremiumCollected = 5000m
        };

        // Assert
        Assert.Equal(tradeId, trade.Id);
        Assert.Equal(entryDate, trade.EntryDate);
        Assert.Equal(expiryDate, trade.ExpiryDate);
        Assert.Equal(TradeStatus.Open, trade.Status);
        Assert.Equal(StrategyType.S1_Strangle, trade.Strategy);
        Assert.Equal(3, trade.Lots);
        Assert.Equal(20000m, trade.NiftySpotAtEntry);
        Assert.Equal(15.5m, trade.VixAtEntry);
    }

    [Fact]
    public void StrangleTrade_LegsCanBeAdded_CollectionUpdates()
    {
        // Arrange
        var trade = new StrangleTrade();
        var leg = new TradeLeg { Id = Guid.NewGuid(), Status = LegStatus.Open };

        // Act
        trade.Legs.Add(leg);

        // Assert
        Assert.Single(trade.Legs);
        Assert.Equal(leg.Id, trade.Legs[0].Id);
    }

    [Fact]
    public void StrangleTrade_AdjustmentsCanBeAdded_CollectionUpdates()
    {
        // Arrange
        var trade = new StrangleTrade();
        var adjustment = new TradeAdjustment { Id = Guid.NewGuid(), ClosedStrike = 20000, NewStrike = 20100 };

        // Act
        trade.Adjustments.Add(adjustment);

        // Assert
        Assert.Single(trade.Adjustments);
        Assert.Equal(20000, trade.Adjustments[0].ClosedStrike);
        Assert.Equal(20100, trade.Adjustments[0].NewStrike);
    }

    [Fact]
    public void StrangleTrade_PnlProperties_CanBeSet()
    {
        // Arrange & Act
        var trade = new StrangleTrade
        {
            RealizedPnl = 5000m,
            UnrealizedPnl = 2500m
        };

        // Assert
        Assert.Equal(5000m, trade.RealizedPnl);
        Assert.Equal(2500m, trade.UnrealizedPnl);
    }
}

public class TradeLegTests
{
    [Fact]
    public void TradeLeg_DefaultConstruction_HasEmptySymbol()
    {
        // Arrange & Act
        var leg = new TradeLeg();

        // Assert
        Assert.Empty(leg.TradingSymbol);
        Assert.Null(leg.ExitPrice);
        Assert.Null(leg.ZerodhaGttOrderId);
    }

    [Fact]
    public void TradeLeg_PropertiesCanBeSet_UpdatesValues()
    {
        // Arrange & Act
        var legId = Guid.NewGuid();
        var tradeId = Guid.NewGuid();
        var leg = new TradeLeg
        {
            Id = legId,
            TradeId = tradeId,
            Type = LegType.ShortPE,
            TradingSymbol = "NIFTY23FEB20000PE",
            Strike = 20000,
            OptionType = OptionType.PE,
            Quantity = 75,
            EntryPrice = 150.50m,
            ExitPrice = 100.25m,
            EntryDelta = -0.30m,
            GttTriggerPrice = 225m,
            Status = LegStatus.Open
        };

        // Assert
        Assert.Equal(legId, leg.Id);
        Assert.Equal(tradeId, leg.TradeId);
        Assert.Equal(LegType.ShortPE, leg.Type);
        Assert.Equal("NIFTY23FEB20000PE", leg.TradingSymbol);
        Assert.Equal(20000, leg.Strike);
        Assert.Equal(OptionType.PE, leg.OptionType);
        Assert.Equal(75, leg.Quantity);
        Assert.Equal(150.50m, leg.EntryPrice);
        Assert.Equal(100.25m, leg.ExitPrice);
        Assert.Equal(-0.30m, leg.EntryDelta);
        Assert.Equal(225m, leg.GttTriggerPrice);
    }

    [Fact]
    public void TradeLeg_PnlProperties_CanBeSet()
    {
        // Arrange & Act
        var leg = new TradeLeg
        {
            UnrealizedPnl = 3000m,
            RealizedPnl = 5000m
        };

        // Assert
        Assert.Equal(3000m, leg.UnrealizedPnl);
        Assert.Equal(5000m, leg.RealizedPnl);
    }

    [Fact]
    public void TradeLeg_ShortAndLongTypes_CanBeSet()
    {
        // Arrange & Act
        var shortCE = new TradeLeg { Type = LegType.ShortCE };
        var longPE = new TradeLeg { Type = LegType.LongPE };
        var longCE = new TradeLeg { Type = LegType.LongCE };

        // Assert
        Assert.Equal(LegType.ShortCE, shortCE.Type);
        Assert.Equal(LegType.LongPE, longPE.Type);
        Assert.Equal(LegType.LongCE, longCE.Type);
    }
}

public class TradeAdjustmentTests
{
    [Fact]
    public void TradeAdjustment_PropertiesCanBeSet_UpdatesValues()
    {
        // Arrange
        var adjId = Guid.NewGuid();
        var tradeId = Guid.NewGuid();
        var adjDate = DateTime.Now;

        // Act
        var adjustment = new TradeAdjustment
        {
            Id = adjId,
            TradeId = tradeId,
            AdjustmentDate = adjDate,
            ClosedStrike = 20000,
            NewStrike = 20100,
            ClosingPrice = 150m,
            EntryPrice = 125m
        };

        // Assert
        Assert.Equal(adjId, adjustment.Id);
        Assert.Equal(tradeId, adjustment.TradeId);
        Assert.Equal(adjDate, adjustment.AdjustmentDate);
        Assert.Equal(20000, adjustment.ClosedStrike);
        Assert.Equal(20100, adjustment.NewStrike);
        Assert.Equal(150m, adjustment.ClosingPrice);
        Assert.Equal(125m, adjustment.EntryPrice);
    }

    [Fact]
    public void TradeAdjustment_StrikeChange_CanRepresentRollUp()
    {
        // Arrange & Act
        var adjustment = new TradeAdjustment
        {
            ClosedStrike = 20000,
            NewStrike = 20100,
            ClosingPrice = 160m,
            EntryPrice = 140m
        };

        // Assert
        Assert.Equal(20000, adjustment.ClosedStrike);
        Assert.Equal(20100, adjustment.NewStrike);
        Assert.Equal(100, adjustment.NewStrike - adjustment.ClosedStrike);
    }

    [Fact]
    public void TradeAdjustment_StrikeChange_CanRepresentRollDown()
    {
        // Arrange & Act
        var adjustment = new TradeAdjustment
        {
            ClosedStrike = 20100,
            NewStrike = 20000
        };

        // Assert
        Assert.Equal(20100, adjustment.ClosedStrike);
        Assert.Equal(20000, adjustment.NewStrike);
        Assert.Equal(-100, adjustment.NewStrike - adjustment.ClosedStrike);
    }
}

public class VixSnapshotTests
{
    [Fact]
    public void VixSnapshot_PropertiesCanBeSet_UpdatesValues()
    {
        // Arrange
        var snapshotId = Guid.NewGuid();
        var timestamp = DateTime.Now;

        // Act
        var snapshot = new VixSnapshot
        {
            Id = snapshotId,
            Timestamp = timestamp,
            Value = 16.75m,
            Regime = VixRegime.SweetSpot,
            Direction = VixDirection.Stable,
            FiveDayMA = 15.50m
        };

        // Assert
        Assert.Equal(snapshotId, snapshot.Id);
        Assert.Equal(timestamp, snapshot.Timestamp);
        Assert.Equal(16.75m, snapshot.Value);
        Assert.Equal(VixRegime.SweetSpot, snapshot.Regime);
        Assert.Equal(VixDirection.Stable, snapshot.Direction);
        Assert.Equal(15.50m, snapshot.FiveDayMA);
    }

    [Fact]
    public void VixSnapshot_DifferentRegimes_CanBeSet()
    {
        // Arrange & Act
        var dangerZone = new VixSnapshot { Regime = VixRegime.DangerZone, Value = 10m };
        var crisis = new VixSnapshot { Regime = VixRegime.Crisis, Value = 30m };
        var elevated = new VixSnapshot { Regime = VixRegime.Elevated, Value = 22m };

        // Assert
        Assert.Equal(VixRegime.DangerZone, dangerZone.Regime);
        Assert.Equal(VixRegime.Crisis, crisis.Regime);
        Assert.Equal(VixRegime.Elevated, elevated.Regime);
    }

    [Fact]
    public void VixSnapshot_DifferentDirections_CanBeSet()
    {
        // Arrange & Act
        var rising = new VixSnapshot { Direction = VixDirection.Rising };
        var falling = new VixSnapshot { Direction = VixDirection.Falling };
        var stable = new VixSnapshot { Direction = VixDirection.Stable };

        // Assert
        Assert.Equal(VixDirection.Rising, rising.Direction);
        Assert.Equal(VixDirection.Falling, falling.Direction);
        Assert.Equal(VixDirection.Stable, stable.Direction);
    }
}
