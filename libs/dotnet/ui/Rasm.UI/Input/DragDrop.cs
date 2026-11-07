using System.Numerics;
using SkiaSharp;

namespace Rasm.UI.Input;

// --- [MODELS] --------------------------------------------------------------------------

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
    public Option<Func<Vector2, View>> Preview { get; init; }

    public static Lift Of(Func<Seq<Key>, Transfer> payload) => new(keys => IO.pure(payload(keys)));

    public static Lift Text(Func<Key, Option<string>> text) => Of(keys => new Transfer.Text(string.Join('\n', keys.Choose(text))));
}

public sealed record Drop(Func<Transfer, Option<Key>, Placement, Option<IO<Unit>>> Accepted) {
    public bool Nests { get; init; }

    public static Drop Of<TCase>(Func<TCase, Option<Key>, Placement, IO<Unit>> accepted) where TCase : Transfer =>
        new((transfer, target, placement) => Optional(transfer as TCase).Map(held => accepted(held, target, placement)));
}

public sealed record Lifted(IO<Transfer> Transfer, SKPoint At);
