namespace Lab.F22;

internal static class Schedules {
    public static Schedule Policy => Schedule.spaced(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(5);

    public static Schedule Backoff => Schedule.exponential(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(3) | Schedule.jitter();

    public static Schedule Capped => Schedule.exponential(TimeSpan.FromMilliseconds(1)) & Schedule.maxDelay(TimeSpan.FromMilliseconds(4));

    public static Schedule Replayed => Schedule.spaced(TimeSpan.FromMilliseconds(1)) | Schedule.recurs(2) | Schedule.repeat(2);

    public static Schedule Union => Schedule.spaced(TimeSpan.FromMilliseconds(1)) | Schedule.spaced(TimeSpan.FromMilliseconds(3));

    public static Schedule Intersection => Schedule.spaced(TimeSpan.FromMilliseconds(1)) & Schedule.spaced(TimeSpan.FromMilliseconds(3));

    public static IO<int> Retried(Atom<int> attempts) =>
        IO.lift(() => attempts.Swap(static n => n + 1))
            .Bind(static n => n < 3 ? IO.fail<int>(new Unavailable()) : IO.pure(7))
            .Retry(Policy);

    public static IO<int> Repeated(Atom<int> ticks) =>
        IO.lift(() => ticks.Swap(static n => n + 1)).Repeat(Schedule.recurs(2));
}
