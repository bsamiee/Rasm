namespace Lab.F11;

internal sealed record BookTransfer(string Bic, DateOnly Date);

internal static class Codes {
    public const int InvalidBic = 1;
    public const int TransferDateIsPast = 2;
}

internal sealed record InvalidBic() : Expected("The beneficiary BIC is invalid", Codes.InvalidBic);

internal sealed record TransferDateIsPast() : Expected("Transfer date cannot be in the past", Codes.TransferDateIsPast);

internal static class Transfers {
    public static Fin<BookTransfer> ValidateBic(BookTransfer command) =>
        command.Bic.Length is 8 or 11 && command.Bic.All(char.IsLetterOrDigit) ? command : new InvalidBic();

    public static Fin<BookTransfer> ValidateDate(BookTransfer command, DateOnly today) =>
        command.Date > today ? command : new TransferDateIsPast();

    public static Fin<BookTransfer> Validate(BookTransfer command, DateOnly today) =>
        ValidateBic(command).Bind(c => ValidateDate(c, today));
}
