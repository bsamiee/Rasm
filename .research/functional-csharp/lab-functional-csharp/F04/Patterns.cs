namespace Lab.F04;

internal abstract record StandardBankAccount(decimal Balance, decimal InterestRate);

internal sealed record PremiumBankAccount(decimal Balance, decimal InterestRate, decimal BonusInterestRate)
    : StandardBankAccount(Balance, InterestRate);

internal sealed record MillionairesBankAccount(decimal Balance, decimal InterestRate, decimal OverflowBalance)
    : StandardBankAccount(Balance, InterestRate);

internal sealed record Player(string FirstName, string LastName);

internal sealed record MonopolyPlayersBankAccount(
    decimal Balance,
    decimal InterestRate,
    Player Player,
    string CurrSquare,
    decimal PassingGoBonus)
    : StandardBankAccount(Balance, InterestRate);

internal sealed record ClosedBankAccount(decimal Balance, decimal InterestRate)
    : StandardBankAccount(Balance, InterestRate);

internal static class Interest {
    public static decimal CalculateInterest(StandardBankAccount account) =>
        account switch {
            PremiumBankAccount { Balance: > 20_000m } p =>
                p.Balance * (p.InterestRate + (p.BonusInterestRate * 1.25m)),

            PremiumBankAccount { Balance: > 10_000m and <= 20_000m } p =>
                p.Balance * (p.InterestRate + p.BonusInterestRate),

            MillionairesBankAccount m =>
                (m.Balance * m.InterestRate) +
                (m.OverflowBalance * m.InterestRate),

            MonopolyPlayersBankAccount { CurrSquare: not "InJail" } m =>
                (m.Balance * m.InterestRate) + m.PassingGoBonus,

            ClosedBankAccount => 0m,

            _ => account.Balance * account.InterestRate,
        };
}

internal static class Recognition {
    public static bool IsSimon(StandardBankAccount account) =>
        account switch {
            MonopolyPlayersBankAccount { Player.FirstName: "Simon" } => true,
            _ => false,
        };
}
