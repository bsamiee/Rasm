using Rasm.Imaging.Pixels;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StageFrame(int Watchers, Option<(PixelFrame Frame, PassContext Context)> Input) {
    public static StageFrame Unwatched { get; } = new(0, None);

    public bool Demanded => Watchers > 0;

    public static IO<IO<Option<(PixelFrame Frame, PassContext Context)>>> Watch(Atom<StageFrame> stage, Action<IDisposable> retain) =>
        DisposalOps.Acquire(
                IO.lift(() => {
                    _ = stage.Swap(static held => held with { Watchers = held.Watchers + 1 });
                    return new Disposal<Atom<StageFrame>>(stage, static held => _ = held.Swap(static frame => frame.Watchers > 1 ? frame with { Watchers = frame.Watchers - 1 } : Unwatched));
                }),
                static lease => IO.lift(lease.Dispose), retain)
            .Map(_ => IO.lift(() => stage.Value.Input));

    internal static Unit Fill(Atom<StageFrame> stage, PixelFrame input, PassContext context) =>
        ignore(stage.SwapMaybe(held => held.Demanded ? Some(held with { Input = (input, context) }) : None));
}
