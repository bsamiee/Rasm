namespace Lab.F20;

internal sealed record Overdrawn() : Expected("debit exceeds the overdraft limit", 1001);

internal sealed record AccountState(decimal Balance, decimal OverdraftLimit);

internal sealed record Debited(decimal Amount);

internal sealed record Debit(decimal Amount, Conduit<Fin<AccountState>, Fin<AccountState>> Replies);

internal static class Account {
    public static Fin<(Debited Event, AccountState Next)> Debit(AccountState state, decimal amount) =>
        state.Balance - amount < -state.OverdraftLimit
            ? new Overdrawn()
            : (new Debited(amount), state with { Balance = state.Balance - amount });
}

internal sealed record AccountProcess(Conduit<Debit, Debit> Inbox, ForkIO<AccountState> Running) {
    public static IO<AccountProcess> Start(Func<Debited, IO<Unit>> persist, AccountState initial) {
        Conduit<Debit, Debit> inbox = Agent.Inbox<Debit>();
        return Agent.Start(inbox, initial, (state, command) => Handle(persist, state, command))
            .Map(running => new AccountProcess(inbox, running));
    }

    public IO<AccountState> Debit(decimal amount) {
        Conduit<Fin<AccountState>, Fin<AccountState>> replies = Conduit.make(Buffer<Fin<AccountState>>.Unbounded);
        return
            from _ in Inbox.Post(new Debit(amount, replies))
            from reply in replies.Source.Take(1).Last()
            from next in IO.lift(reply)
            select next;
    }

    private static IO<AccountState> Handle(Func<Debited, IO<Unit>> persist, AccountState state, Debit command) =>
        Account.Debit(state, command.Amount).Match(
            Succ: transition =>
                from _ in persist(transition.Event)
                from __ in command.Replies.Post(transition.Next)
                select transition.Next,
            Fail: error =>
                from _ in command.Replies.Post(error)
                select state);
}
