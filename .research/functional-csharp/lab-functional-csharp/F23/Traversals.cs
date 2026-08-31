namespace Lab.F23;

internal sealed record Job(string Name, IO<int> Work);

internal static class Traversals {
    public static Validation<Error, Seq<int>> Accumulated(Seq<string> values, Func<string, Validation<Error, int>> check) => values.Traverse(check).As();

    public static Validation<Error, Seq<int>> FirstFailure(Seq<string> values, Func<string, Validation<Error, int>> check) => values.TraverseM(check).As();

    public static IO<Seq<int>> Overlapped(Seq<Job> jobs) => jobs.Traverse(static job => job.Work).As();

    public static IO<Seq<int>> Serial(Seq<Job> jobs) => jobs.TraverseM(static job => job.Work).As();

    public static IO<Seq<int>> Bounded(Seq<Job> jobs, int width) => toSeq(jobs.Chunk(width)).Map(toSeq).TraverseM(Overlapped).As().Map(static groups => groups.Flatten());

    public static IO<(Seq<Error> Fails, Seq<int> Succs)> BestEffort(Seq<Job> jobs) => jobs.Map<K<IO, int>>(static job => job.Work).PartitionFallible().As();

    public static IO<Seq<int>> Completed(Seq<Job> jobs) => jobs.Map<K<IO, int>>(static job => job.Work).Succs().As();

    public static IO<Seq<Error>> Failed(Seq<Job> jobs) => jobs.Map<K<IO, int>>(static job => job.Work).Fails().As();
}
