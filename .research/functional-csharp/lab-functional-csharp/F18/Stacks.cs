namespace Lab.F18;

internal sealed record AccountState(Guid Id, decimal Balance);

internal sealed record DebitCommand(Guid DebitedAccountId, decimal Amount);

internal sealed record Debited(AccountState NewState, string Event);

internal sealed record UnknownAccountId() : Expected("unknown account id", Codes.UnknownAccount);

internal sealed record InsufficientFunds() : Expected("insufficient funds", Codes.InsufficientFunds);

internal sealed record Runtime;

internal static class Account {
    public static Fin<Debited> Debit(AccountState account, DebitCommand command) =>
        account.Balance >= command.Amount
            ? new Debited(account with { Balance = account.Balance - command.Amount }, "debited")
            : new InsufficientFunds();
}

internal static partial class Stacks {
    public static IO<AccountState> GetAccount(Func<Guid, OptionT<IO, AccountState>> lookup, Guid id) =>
        lookup(id).Run().As().Bind(static option => IO.lift(option.ToFin(new UnknownAccountId())));

    public static OptionT<IO, decimal> Converted(Func<Guid, OptionT<IO, AccountState>> lookup, Guid id, IO<decimal> rate) =>
        from account in lookup(id)
        from factor in OptionT.liftIO<IO, decimal>(rate)
        select account.Balance * factor;

    public static IO<Unit> SaveAndPublish(Func<Task> publish) =>
        IO.liftAsync(async () => {
            await publish().ConfigureAwait(false);
            return unit;
        });
}

internal static partial class Stacks {
    public static IO<AccountState> Debit(
        Func<DebitCommand, Validation<Error, DebitCommand>> validate,
        Func<Guid, OptionT<IO, AccountState>> lookup,
        Func<Task> publish,
        DebitCommand request) =>
        from command in IO.lift(validate(request).ToFin())
        from account in GetAccount(lookup, command.DebitedAccountId)
        from debit in IO.lift(Account.Debit(account, command))
        from _ in SaveAndPublish(publish)
        select debit.NewState;
}

internal static class Host {
    public static int Exit(IO<AccountState> workflow) =>
        workflow.RunSafe().Match(
            Succ: static _ => 0,
            Fail: static error => error.IsExpected ? 4 : 1);

    public static Flight Recover(IO<Flight> flight, Flight substitute) =>
        flight.RunSafe().IfFail(substitute);

    public static Task<Fin<AccountState>> ExitAsync(Eff<Runtime, AccountState> workflow, Runtime runtime) =>
        workflow.RunAsync(runtime);
}
