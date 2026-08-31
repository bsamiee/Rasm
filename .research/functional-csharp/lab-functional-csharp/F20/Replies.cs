namespace Lab.F20;

internal sealed record Increment(int Amount, Conduit<int, int> Replies);

internal static class Counting {
    public static IO<int> Process(int state, Increment message) =>
        message.Replies.Post(state + message.Amount).Map(_ => state + message.Amount);
}

internal sealed class Counter(Conduit<Increment, Increment> inbox) {
    public IO<int> IncrementBy(int amount) {
        Conduit<int, int> replies = Conduit.make(Buffer<int>.Unbounded);
        return
            from _ in inbox.Post(new Increment(amount, replies))
            from reply in replies.Source.Take(1).Last()
            select reply;
    }
}
