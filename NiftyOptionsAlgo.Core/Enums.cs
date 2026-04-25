namespace NiftyOptionsAlgo.Core;

public enum VixRegime { DangerZone, Caution, SweetSpot, Elevated, HighRisk, Crisis }
public enum VixDirection { Rising, Falling, Stable }
public enum TradeStatus { Pending, Open, Adjusting, Closed }
public enum ExitReason { ProfitTarget, StopLoss, GttFired, TimestopDte21, ManualExit, EventRisk }
public enum StrategyType { S1_Strangle, S2_IronCondor, S3_Butterfly, S4_Calendar }
public enum OptionType { CE, PE }
public enum LegType { ShortPE, ShortCE, LongPE, LongCE }
public enum LegStatus { Open, Closed, GttFired }
public enum EventCategory { RbiMpc, UsaFomc, IndiaEvent, Weekly, Monthly, Custom }
public enum RiskLevel { Extreme, High, Medium, Low }
