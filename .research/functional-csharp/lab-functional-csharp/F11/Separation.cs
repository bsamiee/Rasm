namespace Lab.F11;

internal static class Ledger {
    public static Unit Insert(BookTransfer command) =>
        string.Equals(command.Bic, "DEUTDEFFXXX", StringComparison.Ordinal) ? throw new InvalidOperationException("duplicate transfer") : unit;
}

internal static class Persistence {
    public static IO<Unit> Save(BookTransfer command) =>
        IO.lift(() => Ledger.Insert(command));
}

internal static class Handler {
    public static Validation<Error, BookTransfer> ValidateCommand(BookTransfer command, DateOnly today) =>
        (Transfers.ValidateBic(command).ToValidation(), Transfers.ValidateDate(command, today).ToValidation())
            .Apply(static (_, valid) => valid)
            .As();

    public static IO<Unit> Handle(BookTransfer command, DateOnly today) =>
        from valid in IO.lift(ValidateCommand(command, today).ToFin())
        from _ in Persistence.Save(valid)
        select unit;

    public static int Exit(IO<Unit> handler) =>
        handler.RunSafe().Match(
            Succ: static _ => 200,
            Fail: static error => error.IsExceptional ? 500 : 400);
}
