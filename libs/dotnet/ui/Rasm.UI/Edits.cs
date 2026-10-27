using System.Numerics;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Key {
    private const char Prefix = '#';

    public string Identifier =>
        Switch(
            named: static named => named.Name.StartsWith(Prefix) ? $"{Prefix}{named.Name}" : named.Name,
            ordinal: static ordinal => $"{Prefix}{ordinal.Index}",
            handle: static _ => $"{Prefix}handle");

    public sealed record Named(string Name) : Key;

    public sealed record Ordinal(uint Index) : Key;

    public sealed record Handle : Key;
}

[SmartEnum]
public sealed partial class EditPhase {
    public static readonly EditPhase Begin = new(begins: true, applies: false);
    public static readonly EditPhase Preview = new(begins: false, applies: false);
    public static readonly EditPhase Step = new(begins: false, applies: true);
    public static readonly EditPhase Commit = new(begins: false, applies: true);
    public static readonly EditPhase Cancel = new(begins: false, applies: false);

    public bool Begins { get; }
    public bool Applies { get; }
}

public sealed record Edit<TValue>(EditPhase Phase, Option<TValue> Value) : K<Edit, TValue> {
    public Option<TValue> Applied => Value.Filter(_ => Phase.Applies);
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
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Selection {
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

    internal Selection Renamed(HashMap<Key, Key> renames) =>
        Switch(
            renames,
            one: static (held, one) => (Selection)new One(one.Selected.Map(key => Renamed(held, key))),
            many: static (held, many) => new Many(many.Selected.Map(key => Renamed(held, key))) { Anchor = many.Anchor.Map(key => Renamed(held, key)), Focus = many.Focus.Map(key => Renamed(held, key)) });

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

    public sealed record One(Option<Key> Selected) : Selection;

    public sealed record Many(HashSet<Key> Selected) : Selection {
        public Option<Key> Anchor { get; init; }
        public Option<Key> Focus { get; init; }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Transfer {
    public sealed record Keys(Seq<Seq<Key>> Paths) : Transfer;

    public sealed record Files(Seq<string> Paths) : Transfer;

    public sealed record Text(string Value) : Transfer;

    public sealed record Color(SKColorF Value) : Transfer;
}

[SmartEnum]
public sealed partial class Placement {
    public static readonly Placement Before = new();
    public static readonly Placement Inside = new();
    public static readonly Placement After = new();

    internal static Placement At(Fraction along, bool nests) =>
        (nests ? toSeq(Items) : [Before, After]) switch { var bands => bands[int.Min((int)((float)along * bands.Count), bands.Count - 1)] };
}

public sealed record Lift(Func<Seq<Key>, IO<Transfer>> Payload) {
    public Option<Func<Vector2, Node>> Preview { get; init; }
}

public sealed record Drop(Func<Transfer, Option<Key>, Placement, Option<IO<Unit>>> Accepted) {
    public bool Nests { get; init; }

    public static Drop Of<TCase>(Func<TCase, Option<Key>, Placement, IO<Unit>> accepted) where TCase : Transfer =>
        new((transfer, target, placement) => Optional(transfer as TCase).Map(held => accepted(held, target, placement)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EditExtensions {
    extension<TValue>(K<Edit, TValue> edit) {
        public Edit<TValue> As() => (Edit<TValue>)edit;
    }
}
