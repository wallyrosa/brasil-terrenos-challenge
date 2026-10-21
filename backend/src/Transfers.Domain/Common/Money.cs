namespace Transfers.Domain.Common;

public static class Money
{
    public const int Scale = 2;

    public static void EnsurePositive(decimal amount)
    {
        if (amount <= 0)
            throw DomainErrors.AmountMustBePositive();

        EnsureScale(amount);
    }

    public static void EnsureNonNegative(decimal amount)
    {
        if (amount < 0)
            throw DomainErrors.AmountMustNotBeNegative();

        EnsureScale(amount);
    }

    private static void EnsureScale(decimal amount)
    {
        if (decimal.Round(amount, Scale) != amount)
            throw DomainErrors.AmountHasInvalidScale(Scale);
    }
}
