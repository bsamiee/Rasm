using Rasm.UI.Input;
using Rasm.UI.Styling;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum]
public sealed partial class EditPhase {
    public static readonly EditPhase Begin = new(applies: false);
    public static readonly EditPhase Preview = new(applies: false);
    public static readonly EditPhase Step = new(applies: true);
    public static readonly EditPhase Commit = new(applies: true);
    public static readonly EditPhase Cancel = new(applies: false);

    public bool Applies { get; }
}

public sealed record Edit<TValue>(EditPhase Phase, Option<TValue> Value) : K<Edit, TValue> {
    public Option<TValue> Applied => Value.Filter(_ => Phase.Applies);

    public Edit<TResult> Choose<TResult>(Func<TValue, Option<TResult>> chosen) => new(Phase, Value.Bind(chosen));
}

public sealed class Edit : Traversable<Edit> {
    public static Edit<T> Of<T>(EditPhase phase, T value) => new(phase, Some(value));

    public static Edit<T> Cancel<T>() => new(EditPhase.Cancel, None);

    public static K<Edit, B> Map<A, B>(Func<A, B> f, K<Edit, A> ma) => ma.As() switch { var edit => new Edit<B>(edit.Phase, edit.Value.Map(f)) };

    public static K<F, K<Edit, B>> Traverse<F, A, B>(Func<A, K<F, B>> f, K<Edit, A> ta) where F : Applicative<F> =>
        ta.As() switch { var edit => edit.Value.Traverse(f).Map(value => (K<Edit, B>)new Edit<B>(edit.Phase, value)) };

    public static Fold<A, S> FoldStep<A, S>(K<Edit, A> ta, S initialState) =>
        ta.As().Value.Match(Some: value => Fold.Loop(initialState, value, Fold.Done<A, S>), None: () => Fold.Done<A, S>(initialState));

    public static Fold<A, S> FoldStepBack<A, S>(K<Edit, A> ta, S initialState) => FoldStep(ta, initialState);

    internal static Option<T> Restoring<T>(Option<T> rest, Option<T> value) => rest.Filter(held => Some(held) != value);

    internal static HashSet<Condition> Marked<T>(Option<T> rest, Option<T> value) => Restoring(rest, value).IsSome ? [Condition.Modified] : [];

    internal static IO<Unit> Typed<T>(Edit<string> edit, Func<string, Fin<T>> parsed, Func<Edit<T>, IO<Unit>> edited) =>
        Traverse(parsed, edit).As().Match(Succ: typed => edited(typed.As()), Fail: error => when(edit.Phase == EditPhase.Commit, IO.fail<Unit>(error)).As());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Selection {
    public static Selection Empty { get; } = new Many([]);

    public Option<Key> Lead => Switch(one: static one => one.Selected, many: static many => many.Focus);

    public bool Holds(Key key) => Switch(key, one: static (held, one) => one.Selected == Some(held), many: static (held, many) => many.Selected.Contains(held));

    public Selection Pressed(Seq<Key> ordered, Key hit, KeyModifiers modifiers, bool clicked) =>
        Switch(
            (Ordered: ordered, Hit: hit, Modifiers: modifiers, Clicked: clicked),
            one: static (held, _) => (Selection)new One(Some(held.Hit)),
            many: static (held, many) => (held.Modifiers.Primary, held.Modifiers.Shift) switch {
                (true, _) => new Many(many.Selected.SymmetricExcept([held.Hit])) { Anchor = Some(held.Hit), Focus = Some(held.Hit) },
                (_, true) => Spanned(held.Ordered, many.Anchor, held.Hit),
                _ when !held.Clicked && many.Selected.Contains(held.Hit) => many with { Anchor = Some(held.Hit), Focus = Some(held.Hit) },
                _ => Only(held.Hit),
            });

    public Selection Navigated(Seq<Key> ordered, Key lead, bool extend) =>
        Switch(
            (Ordered: ordered, Lead: lead, Extend: extend),
            one: static (held, _) => (Selection)new One(Some(held.Lead)),
            many: static (held, many) => held.Extend ? Spanned(held.Ordered, many.Anchor | many.Focus, held.Lead) : Only(held.Lead));

    public Selection All(Seq<Key> ordered) => Switch(ordered, one: static (_, one) => (Selection)one, many: static (keys, many) => many with { Selected = toHashSet(keys) });

    public Selection Enclosed(Seq<Key> ordered, HashSet<Key> covered, KeyModifiers modifiers) =>
        Switch(
            (Ordered: ordered, Covered: covered, Modifiers: modifiers),
            one: static (held, _) => (Selection)new One(held.Ordered.Find(held.Covered.Contains)),
            many: static (held, many) => held.Ordered.Filter(held.Covered.Contains) switch {
                var enclosed => new Many((held.Modifiers.Primary, held.Modifiers.Shift) switch {
                    (true, _) => many.Selected.SymmetricExcept(held.Covered),
                    (_, true) => many.Selected.Union(held.Covered),
                    _ => held.Covered,
                }) { Anchor = enclosed.Head | many.Anchor, Focus = enclosed.Last | many.Focus },
            });

    internal Selection Renamed(HashMap<Key, Option<Key>> renames) =>
        Switch(
            renames,
            one: static (held, one) => (Selection)new One(one.Selected.Bind(key => Renamed(held, key))),
            many: static (held, many) => new Many(toHashSet(toSeq(many.Selected).Choose(key => Renamed(held, key)))) { Anchor = many.Anchor.Bind(key => Renamed(held, key)), Focus = many.Focus.Bind(key => Renamed(held, key)) });

    private static Option<Key> Renamed(HashMap<Key, Option<Key>> renames, Key key) => renames.Find(key).IfNone(Some(key));

    private static Many Only(Key key) => new([key]) { Anchor = Some(key), Focus = Some(key) };

    private static Many Spanned(Seq<Key> ordered, Option<Key> anchor, Key target) =>
        (from end in ordered.Position(target)
         let found = anchor.Bind(held => ordered.Position(held))
         let start = found.IfNone(end)
         select new Many(toHashSet(ordered.Take(int.Min(start, end)..(int.Max(start, end) + 1)))) { Anchor = found.IsSome ? anchor : Some(target), Focus = Some(target) })
        .IfNone(() => Only(target));

    public sealed record One(Option<Key> Selected) : Selection;

    public sealed record Many(HashSet<Key> Selected) : Selection {
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
