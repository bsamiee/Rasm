namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Key {
    private const char Mark = '#';

    public string Identifier =>
        Switch(
            named: static named => named.Name.StartsWith(Mark) ? $"{Mark}{named.Name}" : named.Name,
            ordinal: static ordinal => $"{Mark}{ordinal.Index}",
            handle: static _ => $"{Mark}handle");

    public sealed record Named(string Name) : Key;

    public sealed record Ordinal(uint Index) : Key;

    public sealed record Handle : Key;
}

[SmartEnum]
public sealed partial class EditPhase {
    public static readonly EditPhase Begin = new(opens: true, settles: false);
    public static readonly EditPhase Preview = new(opens: false, settles: false);
    public static readonly EditPhase Step = new(opens: false, settles: true);
    public static readonly EditPhase Commit = new(opens: false, settles: true);
    public static readonly EditPhase Cancel = new(opens: false, settles: false);

    public bool Opens { get; }
    public bool Settles { get; }
}

public sealed record Edit<TValue>(EditPhase Phase, Option<TValue> Value) : K<Edit, TValue> {
    public Option<TValue> Applied => Value.Filter(_ => Phase.Settles);
}

public sealed class Edit : Traversable<Edit> {
    public static Edit<T> Begin<T>(T value) => new(EditPhase.Begin, Some(value));

    public static Edit<T> Preview<T>(T value) => new(EditPhase.Preview, Some(value));

    public static Edit<T> Step<T>(T value) => new(EditPhase.Step, Some(value));

    public static Edit<T> Commit<T>(T value) => new(EditPhase.Commit, Some(value));

    public static Edit<T> Cancel<T>() => new(EditPhase.Cancel, None);

    public static K<Edit, B> Map<A, B>(Func<A, B> f, K<Edit, A> ma) {
        Edit<A> edit = ma.As();
        return new Edit<B>(edit.Phase, edit.Value.Map(f));
    }

    public static K<F, K<Edit, B>> Traverse<F, A, B>(Func<A, K<F, B>> f, K<Edit, A> ta) where F : Applicative<F> {
        Edit<A> edit = ta.As();
        return edit.Value.Match(Some: value => f(value).Map(held => (K<Edit, B>)new Edit<B>(edit.Phase, Some(held))), None: () => F.Pure<K<Edit, B>>(new Edit<B>(edit.Phase, None)));
    }

    public static Fold<A, S> FoldStep<A, S>(K<Edit, A> ta, S initialState) =>
        ta.As().Value.Match(Some: value => Fold.Loop(initialState, value, Fold.Done<A, S>), None: () => Fold.Done<A, S>(initialState));

    public static Fold<A, S> FoldStepBack<A, S>(K<Edit, A> ta, S initialState) => FoldStep(ta, initialState);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Selection {
    public Option<Key> Lead => Switch(one: static one => one.Picked, many: static many => many.Focus);

    public bool Holds(Key key) => Switch(key, one: static (held, one) => one.Picked == Some(held), many: static (held, many) => many.Picked.Contains(held));

    public Selection Raised(Seq<Key> ordered, Key hit, KeyModifiers modifiers) =>
        Switch(
            (Ordered: ordered, Hit: hit, Modifiers: modifiers),
            one: static (held, _) => (Selection)new One(Some(held.Hit)),
            many: static (held, many) => (held.Modifiers.Common, held.Modifiers.Shift) switch {
                (true, _) => new Many(many.Picked.SymmetricExcept([held.Hit])) { Anchor = Some(held.Hit), Focus = Some(held.Hit) },
                (_, true) => Spanned(held.Ordered, many.Anchor, held.Hit),
                _ when many.Picked.Contains(held.Hit) => many with { Anchor = Some(held.Hit), Focus = Some(held.Hit) },
                _ => Only(held.Hit),
            });

    public Selection Led(Seq<Key> ordered, Key lead, bool extend) =>
        Switch(
            (Ordered: ordered, Lead: lead, Extend: extend),
            one: static (held, _) => (Selection)new One(Some(held.Lead)),
            many: static (held, many) => held.Extend ? Spanned(held.Ordered, many.Anchor | many.Focus, held.Lead) : Only(held.Lead));

    public Selection All(Seq<Key> ordered) => Switch(ordered, one: static (_, one) => (Selection)one, many: static (keys, many) => many with { Picked = toHashSet(keys) });

    public Selection Swept(Seq<Key> ordered, HashSet<Key> covered, KeyModifiers modifiers) =>
        Switch(
            (Ordered: ordered, Covered: covered, Modifiers: modifiers),
            one: static (held, _) => (Selection)new One(held.Ordered.Find(held.Covered.Contains)),
            many: static (held, many) => {
                Seq<Key> swept = held.Ordered.Filter(held.Covered.Contains);
                return new Many((held.Modifiers.Common, held.Modifiers.Shift) switch {
                    (true, _) => many.Picked.SymmetricExcept(held.Covered),
                    (_, true) => many.Picked.Union(held.Covered),
                    _ => held.Covered,
                }) { Anchor = swept.Head | many.Anchor, Focus = swept.Last | many.Focus };
            });

    internal Selection Renamed(HashMap<Key, Key> renames) =>
        Switch(
            renames,
            one: static (held, one) => (Selection)new One(one.Picked.Map(key => Renamed(held, key))),
            many: static (held, many) => new Many(many.Picked.Map(key => Renamed(held, key))) { Anchor = many.Anchor.Map(key => Renamed(held, key)), Focus = many.Focus.Map(key => Renamed(held, key)) });

    internal static Option<int> Position(Seq<Key> ordered, Key key) =>
        ordered.Map(static (held, index) => (Key: held, Index: index)).Find(entry => entry.Key == key).Map(static entry => entry.Index);

    private static Key Renamed(HashMap<Key, Key> renames, Key key) => renames.Find(key).IfNone(key);

    private static Many Only(Key key) => new([key]) { Anchor = Some(key), Focus = Some(key) };

    private static Many Spanned(Seq<Key> ordered, Option<Key> anchor, Key target) =>
        (from end in Position(ordered, target)
         let found = anchor.Bind(held => Position(ordered, held))
         let start = found.IfNone(end)
         select new Many(toHashSet(ordered.Skip(int.Min(start, end)).Take(int.Abs(start - end) + 1))) { Anchor = found.IsSome ? anchor : Some(target), Focus = Some(target) })
        .IfNone(() => Only(target));

    public sealed record One(Option<Key> Picked) : Selection;

    public sealed record Many(HashSet<Key> Picked) : Selection {
        public Option<Key> Anchor { get; init; }
        public Option<Key> Focus { get; init; }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EditExtensions {
    extension<TValue>(K<Edit, TValue> edit) {
        public Edit<TValue> As() => (Edit<TValue>)edit;
    }
}
