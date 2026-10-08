using Rasm.Imaging.Pixels;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StageFrame {
    public abstract Option<PixelFrame> Input { get; }

    public abstract bool Demanded { get; }

    public static IO<IO<Option<PixelFrame>>> Watch(Atom<StageFrame> stage) =>
        IO.lift(() => stage.SwapMaybe(static held => held.Demanded ? None : Some<StageFrame>(new Awaited())))
            .Map(_ => IO.lift(() => stage.Value.Input));

    internal static Unit Fill(Atom<StageFrame> stage, PixelFrame input) =>
        ignore(stage.SwapMaybe(held => held.Demanded ? Some<StageFrame>(new Held(input)) : None));

    public sealed record Unread : StageFrame {
        public override Option<PixelFrame> Input => None;
        public override bool Demanded => false;
    }

    public sealed record Awaited : StageFrame {
        public override Option<PixelFrame> Input => None;
        public override bool Demanded => true;
    }

    public sealed record Held(PixelFrame Frame) : StageFrame {
        public override Option<PixelFrame> Input => Frame;
        public override bool Demanded => true;
    }
}
