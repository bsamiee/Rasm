using Rasm.Rhino;

namespace Darkroom.Chain;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Derivations {
    private readonly Derivations root;
    private readonly Option<Guid> owner;
    private readonly Lock sync;
    private HashMap<(Option<Guid> Owner, Delegate Derive), (object Key, object Value)> derived;
    private HashMap<(Option<Guid> Owner, Delegate Open), (object Key, IDisposable Value)> held;

    public Derivations() => (root, owner, sync) = (this, None, new Lock());

    private Derivations(Derivations root, Guid owner) => (this.root, this.owner, sync) = (root, Some(owner), root.sync);

    // --- [SCOPES]
    public Derivations Owned(Guid kind) => new(root, kind);

    // --- [VALUES]
    public TValue Derived<TKey, TValue>(TKey key, Func<TKey, TValue> derive) where TKey : notnull where TValue : notnull {
        lock (sync) {
            return root.derived.Find((owner, derive)).Filter(kept => kept.Key.Equals(key)).Map(static kept => (TValue)kept.Value).IfNone(() => {
                TValue value = derive(key);
                root.derived = root.derived.AddOrUpdate((owner, derive), (key, value));
                return value;
            });
        }
    }

    // --- [RESOURCES]
    public Fin<TResult> Held<TKey, TResource, TResult>(TKey key, Func<TKey, Fin<TResource>> open, Func<TResource, Fin<TResult>> run)
        where TKey : notnull
        where TResource : IDisposable {
        lock (sync) {
            return root.held.Find((owner, open)) switch {
                var kept => kept.Filter(found => found.Key.Equals(key)).Match(
                    Some: found => run((TResource)found.Value),
                    None: () => {
                        Fin<TResource> opened = Acquired(kept, open, key);
                        root.held = opened.Match(Succ: fresh => root.held.AddOrUpdate((owner, open), (key, fresh)), Fail: _ => root.held.Remove((owner, open)));
                        return opened.Bind(run);
                    }),
            };
        }
    }

    private static Fin<TResource> Acquired<TKey, TResource>(Option<(object Key, IDisposable Value)> stale, Func<TKey, Fin<TResource>> open, TKey key) where TResource : IDisposable =>
        (from released in stale.Traverse(static found => release(found.Value)).As()
         from opened in IO.lift(() => open(key)).Bind(static opened => use(() => opened))
         select opened).Scoped().RunSafe();
}
