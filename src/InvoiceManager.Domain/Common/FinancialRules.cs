namespace InvoiceManager.Domain.Common;

public static class FinancialRules
{
    public const int MoneyDecimalPlaces = 2;

    public static decimal RoundMoney(decimal value)
    {
        return decimal.Round(value, MoneyDecimalPlaces, MidpointRounding.AwayFromZero);
    }
}
