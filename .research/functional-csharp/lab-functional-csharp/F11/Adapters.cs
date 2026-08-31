namespace Lab.F11;

internal static class Adapters {
    public static Fin<BookTransfer> WithContext(Fin<BookTransfer> result) =>
        result.MapFail(static error => Error.New("transfer rejected", error));

    public static Fin<string> Describe(Fin<BookTransfer> result) =>
        result.BiMap(
            Succ: static command => command.Bic,
            Fail: static error => Error.New("transfer rejected", error));

    public static Fin<BookTransfer> Recover(Fin<BookTransfer> result, BookTransfer fallback) =>
        result.Catch(Codes.InvalidBic, _ => fallback).As();
}
