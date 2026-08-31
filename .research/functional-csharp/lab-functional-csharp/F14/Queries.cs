namespace Lab.F14;

internal sealed record StatementRow(string Description, decimal Amount);

internal static class Queries {
    public static Seq<StatementRow> Rows(Seq<Event> history) =>
        history.Choose(static evt => evt.Switch<Option<StatementRow>>(
            createdAccount: static _ => Option<StatementRow>.None,
            depositedCash: static e => Some(new StatementRow("deposit", e.Amount)),
            debitedTransfer: static e => Some(new StatementRow(e.Beneficiary, -e.DebitedAmount)),
            frozeAccount: static _ => Option<StatementRow>.None));

    public static decimal Total(Seq<StatementRow> rows) => rows.Fold(0m, static (total, row) => total + row.Amount);
}
