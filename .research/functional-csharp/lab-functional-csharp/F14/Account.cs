namespace Lab.F14;

internal static partial class Account {
    public static AccountState Create(Event.CreatedAccount evt) => new(AccountStatus.Active, evt.Currency, 0m, 0m);
    public static AccountState Apply(AccountState state, Event evt) =>
        evt.Switch(
            state,
            createdAccount: static (s, _) => s,
            depositedCash: static (s, e) => s.Credit(e.Amount),
            debitedTransfer: static (s, e) => s.Debit(e.DebitedAmount),
            frozeAccount: static (s, _) => s.WithStatus(AccountStatus.Frozen));
}

internal static partial class Account {
    public static Option<AccountState> Rebuild(Seq<Event> history) =>
        history.Head
            .Bind(static head => head is Event.CreatedAccount created ? Some(created) : Option<Event.CreatedAccount>.None)
            .Map(created => history.Tail.Fold(Create(created), Apply));
}

internal static partial class Account {
    public static Fin<(Event Event, AccountState State)> Debit(AccountState account, MakeTransfer command) =>
        from _ in guard<Error>(account.Status == AccountStatus.Active, new AccountNotActive()).ToFin()
        from __ in guard<Error>(account.Balance - command.Amount >= account.AllowedOverdraft, new InsufficientBalance())
        let evt = command.ToEvent()
        select (Event: evt, State: Apply(account, evt));
}
