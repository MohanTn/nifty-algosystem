namespace NiftyOptionsAlgo.Core;
using System;

public class BlackScholesGreeksCalculator : IGreeksCalculator
{
    private static readonly double SQRT2PI = Math.Sqrt(2 * Math.PI);

    public Greeks Calculate(decimal spotPrice, decimal strikePrice, decimal riskFreeRate,
        decimal impliedVolatility, decimal daysToExpiry, OptionType optionType)
    {
        double S = (double)spotPrice;
        double K = (double)strikePrice;
        double r = (double)riskFreeRate;
        double sigma = (double)impliedVolatility;
        double T = (double)daysToExpiry / 365.0;

        if (T <= 0 || sigma <= 0) return new Greeks();

        double d1 = (Math.Log(S / K) + (r + 0.5 * sigma * sigma) * T) / (sigma * Math.Sqrt(T));
        double d2 = d1 - sigma * Math.Sqrt(T);

        double Nd1 = NormalCDF(d1);
        double Nd2 = NormalCDF(d2);
        double nd1 = NormalPDF(d1);
        double nd2 = NormalPDF(d2);

        var greeks = new Greeks();

        if (optionType == OptionType.CE)
        {
            greeks.Delta = (decimal)Nd1;
            greeks.Gamma = (decimal)(nd1 / (S * sigma * Math.Sqrt(T)));
            greeks.Theta = (decimal)((-S * nd1 * sigma / (2 * Math.Sqrt(T)) - r * K * Math.Exp(-r * T) * Nd2) / 365);
            greeks.Vega = (decimal)(S * nd1 * Math.Sqrt(T) / 100);
            greeks.Rho = (decimal)(K * T * Math.Exp(-r * T) * Nd2 / 100);
        }
        else // PE
        {
            greeks.Delta = (decimal)(Nd1 - 1);
            greeks.Gamma = (decimal)(nd1 / (S * sigma * Math.Sqrt(T)));
            greeks.Theta = (decimal)((-S * nd1 * sigma / (2 * Math.Sqrt(T)) + r * K * Math.Exp(-r * T) * (1 - Nd2)) / 365);
            greeks.Vega = (decimal)(S * nd1 * Math.Sqrt(T) / 100);
            greeks.Rho = (decimal)(-K * T * Math.Exp(-r * T) * (1 - Nd2) / 100);
        }

        return greeks;
    }

    public decimal ComputeImpliedVolatility(decimal marketPrice, decimal spotPrice, decimal strikePrice,
        decimal riskFreeRate, decimal daysToExpiry, OptionType optionType)
    {
        double price = (double)marketPrice;
        double S = (double)spotPrice;
        double K = (double)strikePrice;
        double r = (double)riskFreeRate;
        double T = (double)daysToExpiry / 365.0;

        double low = 0.001, high = 5.0;
        for (int i = 0; i < 10; i++)
        {
            double mid = (low + high) / 2;
            var greeks = Calculate(spotPrice, strikePrice, riskFreeRate, (decimal)mid, daysToExpiry, optionType);
            double callPrice = BlackScholesPrice(S, K, r, mid, T, optionType == OptionType.CE);

            if (callPrice < price) low = mid;
            else high = mid;
        }

        return (decimal)((low + high) / 2);
    }

    public decimal ComputePortfolioDelta(List<TradeLeg> legs, decimal spotPrice)
    {
        decimal totalDelta = 0;
        foreach (var leg in legs)
        {
            if (leg.Status == LegStatus.Open)
            {
                totalDelta += leg.EntryDelta * (leg.Type == LegType.ShortPE || leg.Type == LegType.ShortCE ? -1 : 1);
            }
        }
        return totalDelta;
    }

    private double BlackScholesPrice(double S, double K, double r, double sigma, double T, bool isCall)
    {
        double d1 = (Math.Log(S / K) + (r + 0.5 * sigma * sigma) * T) / (sigma * Math.Sqrt(T));
        double d2 = d1 - sigma * Math.Sqrt(T);

        if (isCall)
            return S * NormalCDF(d1) - K * Math.Exp(-r * T) * NormalCDF(d2);
        else
            return K * Math.Exp(-r * T) * NormalCDF(-d2) - S * NormalCDF(-d1);
    }

    private double NormalCDF(double x)
    {
        // Approximation using Hart approximation (good enough for options)
        const double a1 = 0.254829592;
        const double a2 = -0.284496736;
        const double a3 = 1.421413741;
        const double a4 = -1.453152027;
        const double a5 = 1.061405429;
        const double p = 0.3275911;

        int sign = x < 0 ? -1 : 1;
        x = Math.Abs(x) / Math.Sqrt(2);

        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

        return 0.5 * (1.0 + sign * y);
    }

    private double NormalPDF(double x) => Math.Exp(-x * x / 2) / SQRT2PI;
}
