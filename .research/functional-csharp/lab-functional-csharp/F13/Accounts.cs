namespace Lab.F13;

internal readonly record struct CurrencyCode(string Code);

internal enum AccountStatus { Requested = 0, Active = 1, Frozen = 2 }

internal sealed record Transaction(string Reference, decimal Amount);

internal sealed record AccountState(CurrencyCode Currency, AccountStatus Status, decimal AllowedOverdraft, Seq<Transaction> Transactions) {
    public static AccountState Requested(CurrencyCode currency) =>
        new(currency, AccountStatus.Requested, 0m, Seq<Transaction>());

    public static AccountState Opened(CurrencyCode currency, IList<Transaction> transactions) =>
        new(currency, AccountStatus.Active, 0m, toSeq(transactions));

    public AccountState With(Option<AccountStatus> status = default, Option<decimal> allowedOverdraft = default) =>
        this with { Status = status.IfNone(Status), AllowedOverdraft = allowedOverdraft.IfNone(AllowedOverdraft) };

    public AccountState Add(Transaction transaction) =>
        this with { Transactions = transaction.Cons(Transactions) };
}

internal static class Transitions {
    public static AccountState Frozen(AccountState active) => active.With(AccountStatus.Frozen);
}
