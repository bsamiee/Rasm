namespace Lab.F22;

internal static class Concurrency {
    public static IO<int> Forked =>
        from left in IO.pure(1).Fork()
        from right in IO.pure(2).Fork()
        from a in left.Await
        from b in right.Await
        select a + b;

    public static IO<Seq<int>> All(Seq<IO<int>> jobs) => awaitAll(jobs);

    public static IO<int> First(Seq<IO<int>> jobs) => awaitAny(jobs);

    public static IO<int> Deadline(IO<int> job) => timeout(TimeSpan.FromSeconds(1), job);

    public static IO<int> Masked => IO.pure(3).Uninterruptible();

    public static IO<Seq<int>> Chunked(Seq<int> items, int width, Func<int, IO<int>> work) =>
        toSeq(items.Chunk(width))
            .TraverseM(chunk => toSeq(chunk).Traverse(work).As())
            .As()
            .Map(static chunks => chunks.Flatten());

    public static IO<int> Drained(Buffer<int> buffer, Seq<int> items) {
        Conduit<int, int> queue = Conduit.make(buffer);
        return
            from running in queue.Reduce(0, static (total, item) => Reduced.ContinueIO(total + item)).Fork()
            from _ in items.TraverseM(queue.Post).As()
            from __ in queue.Complete()
            from total in running.Await
            select total;
    }
}
