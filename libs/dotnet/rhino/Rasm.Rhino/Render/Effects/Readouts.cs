using Rasm.Imaging.Pixels;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StageFrame(int Watchers, Option<(PixelFrame Frame, PassContext Context)> Input) {
    public static StageFrame Unwatched { get; } = new(0, None);

    public bool Demanded => Watchers > 0;

    public static IO<Unit> Watch(Atom<StageFrame> stage, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => stage.Swap(static held => held with { Watchers = held.Watchers + 1 })),
            _ => IO.lift(() => stage.Swap(static frame => frame.Watchers > 1 ? frame with { Watchers = frame.Watchers - 1 } : Unwatched)).Map(unit), retain)
            .Map(static _ => unit);

    internal static Unit Fill(Atom<StageFrame> stage, PixelFrame input, PassContext context) =>
        ignore(stage.SwapMaybe(held => held.Demanded ? Some(held with { Input = (input, context) }) : None));
}
