namespace Lab.F24;

internal static class SharedState {
    public static int Increment(Atom<int> counter) => counter.Swap(static n => n + 1);

    public static int Capped(Atom<int> counter, int limit) => counter.SwapMaybe(n => n < limit ? Some(n + 1) : Option<int>.None);

    public static Unit Register(AtomHashMap<string, int> registry, string key, int value) => registry.TryAdd(key, value);

    public static Unit Bump(AtomHashMap<string, int> registry, string key) => registry.SwapKey(key, static n => n + 1);

    public static Unit BumpOrStart(AtomHashMap<string, int> registry, string key) => registry.SwapKey(key, static n => n.Map(static v => v + 1) | Some(1));

    public static Option<int> Read(AtomHashMap<string, int> registry, string key) => registry.Find(key);

    public static int ReadOrRegister(AtomHashMap<string, int> registry, string key, int value) => registry.FindOrAdd(key, value);

    public static decimal Move(Ref<decimal> source, Ref<decimal> target, decimal amount) =>
        atomic(() => {
            _ = swap(source, balance => balance - amount);
            return commute(target, balance => balance + amount);
        }, Isolation.Serialisable);

    public static TrackingHashMap<string, int> Tracked(TrackingHashMap<string, int> stock) => stock.Add("a", 1).SetItem("a", 2).Add("b", 3).Remove("b");

    public static int Logged(TrackingHashMap<string, int> stock) => stock.Changes.Count;

    public static TrackingHashMap<string, int> Cleared(TrackingHashMap<string, int> stock) => stock.Snapshot();

    public static Func<int, int> Squares(Atom<int> calls) =>
        memo((int x) => {
            _ = calls.Swap(static n => n + 1);
            return x * x;
        });

    public static Memo<int> Once(Atom<int> calls) => memo(() => calls.Swap(static n => n + 1));

    public static Memo<IO, int> Built(Atom<int> builds, Atom<int> runs) =>
        memoK<IO, int>(() => {
            _ = builds.Swap(static n => n + 1);
            return IO.lift(() => runs.Swap(static n => n + 1));
        });
}
