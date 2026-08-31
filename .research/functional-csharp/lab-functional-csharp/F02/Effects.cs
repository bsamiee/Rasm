namespace Lab.F02;

internal sealed record MakeTransfer(DateTime Date, string Bic);

internal sealed class DateNotPastValidator(DateTime today) {
    public bool IsValid(MakeTransfer command) => today <= command.Date.Date;
}

internal sealed class BicExistsValidator(Seq<string> validCodes) {
    public bool IsValid(MakeTransfer command) =>
        validCodes.Exists(code => string.Equals(code, command.Bic, StringComparison.Ordinal));
}

internal static class Transfers {
    public static IO<bool> BicExists(IO<Seq<string>> loadCodes, MakeTransfer command) =>
        loadCodes.Map(codes => new BicExistsValidator(codes).IsValid(command));
}

internal sealed record Clock(DateTime Today);

internal sealed record Runtime(Clock Clock, ConsoleIO Console) : Has<Eff<Runtime>, Clock>, Has<Eff<Runtime>, ConsoleIO> {
    static K<Eff<Runtime>, Clock> Has<Eff<Runtime>, Clock>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Clock);

    static K<Eff<Runtime>, ConsoleIO> Has<Eff<Runtime>, ConsoleIO>.Ask => Eff.runtime<Runtime>().Map(static rt => rt.Console);
}

internal static class Capabilities {
    public static Eff<RT, bool> DateNotPast<RT>(MakeTransfer command) where RT : Has<Eff<RT>, Clock> =>
        RT.Ask.Map(clock => new DateNotPastValidator(clock.Today).IsValid(command)).As();
}
