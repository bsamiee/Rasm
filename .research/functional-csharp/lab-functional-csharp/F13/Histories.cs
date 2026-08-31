namespace Lab.F13;

internal static class Histories {
    public static Seq<Transaction> Prepend(Transaction transaction, Seq<Transaction> history) =>
        transaction.Cons(history);

    public static Option<Transaction> Newest(Seq<Transaction> history) => history.Head;

    public static Seq<Transaction> Older(Seq<Transaction> history) => history.Tail;

    public static decimal Balance(Seq<Transaction> history) =>
        history.Match(
            Empty: static () => 0m,
            Tail: static (head, tail) => head.Amount + Balance(tail));

    public static Lst<Transaction> Correct(Lst<Transaction> ledger, int index, Transaction corrected) =>
        ledger.SetItem(index, corrected);
}
