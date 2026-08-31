namespace Lab.F14;

[Union]
internal abstract partial record Event {
    private Event(Guid accountId) => AccountId = accountId;

    public Guid AccountId { get; }

    internal sealed record CreatedAccount(Guid AccountId, string Currency) : Event(AccountId);
    internal sealed record DepositedCash(Guid AccountId, decimal Amount) : Event(AccountId);
    internal sealed record DebitedTransfer(Guid AccountId, decimal DebitedAmount, string Beneficiary) : Event(AccountId);
    internal sealed record FrozeAccount(Guid AccountId) : Event(AccountId);
}

internal enum AccountStatus {
    Requested = 0,
    Active = 1,
    Frozen = 2,
}

internal sealed record AccountState(AccountStatus Status, string Currency, decimal Balance, decimal AllowedOverdraft) {
    public AccountState Credit(decimal amount) => this with { Balance = Balance + amount };

    public AccountState Debit(decimal amount) => this with { Balance = Balance - amount };

    public AccountState WithStatus(AccountStatus status) => this with { Status = status };
}

internal sealed record MakeTransfer(Guid AccountId, decimal Amount, string Beneficiary) {
    public Event ToEvent() => new Event.DebitedTransfer(AccountId, Amount, Beneficiary);
}

internal sealed record AccountNotActive() : Expected("account is not active", 1401);

internal sealed record InsufficientBalance() : Expected("insufficient balance", 1402);

internal sealed record AccountNotFound() : Expected("account not found", 1403);

internal sealed record InvalidAmount() : Expected("amount is not positive", 1404);
