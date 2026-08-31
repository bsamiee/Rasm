namespace Lab.F09;

internal sealed record AccountState(decimal Balance);

internal sealed record InsufficientFunds() : Expected("insufficient funds", 902);

internal static class Account {
    public static Fin<AccountState> Debit(this AccountState account, decimal amount) =>
        account.Balance < amount
            ? new InsufficientFunds()
            : new AccountState(account.Balance - amount);
}
