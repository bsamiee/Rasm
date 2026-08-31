namespace Lab.F07;

internal sealed record TransferDateIsPast() : Expected("The transfer date is in the past", 100);

internal sealed record BookTransfer(Guid OwnerId, DateTime Date, decimal Amount);

internal static class Validators {
    public static Func<BookTransfer, Validation<Error, BookTransfer>> DateNotPast(Func<DateTime> clock) =>
        command => command.Date.Date < clock().Date ? new TransferDateIsPast() : command;
}
