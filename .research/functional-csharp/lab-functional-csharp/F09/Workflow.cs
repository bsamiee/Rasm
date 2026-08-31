namespace Lab.F09;

internal sealed record MakeTransfer(Guid DebitedAccountId, string Bic, decimal Amount);

internal sealed record InvalidTransfer() : Expected("transfer is invalid", 901);

internal static class Workflow {
    public static Fin<AccountState> MakeTransfer(MakeTransfer transfer, AccountState account) =>
        from normalized in Pure(Normalize(transfer)).ToFin()
        from _ in guard<Error>(IsValid(normalized), new InvalidTransfer())
        from debited in account.Debit(normalized.Amount)
        select debited;

    public static MakeTransfer Normalize(MakeTransfer transfer) => transfer with { Bic = transfer.Bic.Trim().ToUpperInvariant() };

    public static bool IsValid(MakeTransfer transfer) => transfer.Amount > 0 && transfer.Bic.Length == 8;
}
