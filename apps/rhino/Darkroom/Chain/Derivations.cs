namespace Darkroom.Chain;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Derivations : IDisposable {
    private readonly Lock sync = new();
    private HashMap<Delegate, (object Key, object Value)> derived;
    private Option<HashMap<Delegate, (object Key, IDisposable Value)>> held = Some(HashMap<Delegate, (object Key, IDisposable Value)>());

    // --- [VALUES]
    public TValue Derived<TKey, TValue>(TKey key, Func<TKey, TValue> derive) where TKey : notnull where TValue : notnull {
        lock (sync) {
            ObjectDisposedException.ThrowIf(held.IsNone, this);
            return derived.Find(derive).Filter(kept => kept.Key.Equals(key)).Map(static kept => (TValue)kept.Value).IfNone(() => {
                TValue value = derive(key);
                derived = derived.AddOrUpdate(derive, (key, value));
                return value;
            });
        }
    }

    // --- [RESOURCES]
    public Fin<TResult> Held<TKey, TResource, TResult>(TKey key, Func<TKey, Fin<TResource>> open, Func<TResource, Fin<TResult>> use)
        where TKey : notnull
        where TResource : IDisposable {
        lock (sync) {
            return held.ToFin(Errors.Cancelled).Bind(resources => resources.Find(open).Filter(kept => kept.Key.Equals(key)).Match(
                Some: kept => Used((TResource)kept.Value, use),
                None: () => Try.lift(() => open(key)).Run().Bind(static Fin<TResource> (opened) => opened).Bind(opened => {
                    Fin<Unit> released = resources.Find(open).Map(static kept => Try.lift(kept.Value.Dispose).Run()).IfNone(unit);
                    held = Some(resources.AddOrUpdate(open, (key, opened)));
                    return released.Bind(_ => Used(opened, use));
                })));
        }
    }

    private static Fin<TResult> Used<TResource, TResult>(TResource resource, Func<TResource, Fin<TResult>> use) =>
        Try.lift(() => use(resource)).Run().Bind(static Fin<TResult> (result) => result);

    // --- [RELEASE]
    public void Dispose() {
        lock (sync) {
            Seq<IDisposable> closed = held.ToSeq().Bind(static resources => toSeq(resources.Values).Map(static kept => kept.Value));
            (held, derived) = (None, HashMap<Delegate, (object Key, object Value)>());
            _ = closed.Map(static resource => Try.lift(resource.Dispose).Run()).Strict().Traverse(static released => released).As().ThrowIfFail();
        }
    }
}
