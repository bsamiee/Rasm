using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Queue;
using Rasm.Rhino.Render.Slots;

namespace Rasm.Rhino.Render;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPlugInRendering : IPlugInSink {
    public Seq<EffectKind> Effects { get; }
    public Atom<Option<RenderHistory>> History { get; }
    public Atom<Option<PreviewTiles>> Tiles { get; }
    public Atom<Option<SlotComparison>> Comparison { get; }
    public Atom<Option<RenderSets>> Sets { get; }
    public Atom<Option<NoticeSender>> Notices { get; }
    public Atom<Option<QueueContext>> QueueContext { get; }

    public static Fin<T> Holding<T>(Atom<Option<T>> cell, string member) => cell.Value.ToFin(new Missing(member));

    public static IO<T> Served<T>(Atom<Option<T>> cell, string member) => IO.lift(() => Holding(cell, member));
}
