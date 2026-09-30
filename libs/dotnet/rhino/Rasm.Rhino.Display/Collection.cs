using Rasm.Rhino.Document;
using Rhino;
using Rhino.Render.PostEffects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EffectRow(Guid Id, PostEffectType Type, Option<string> Name, bool On, bool Shown, uint Crc);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class EffectMapper {
    [MapProperty(nameof(PostEffectData.LocalName), nameof(EffectRow.Name))]
    [MapPropertyFromSource(nameof(EffectRow.Crc), Use = nameof(Crc))]
    internal static partial EffectRow ToRow(PostEffectData data);

    private static uint Crc(PostEffectData data) => data.DataCRC(0u);
}

public static class RenderPostEffects {
    // --- [STYLES]
    public const PostEffectStyles Listed = PostEffectStyles.ExecuteForProductionRendering | PostEffectStyles.ExecuteForRealtimeRendering | PostEffectStyles.DefaultShown | PostEffectStyles.DefaultOn;

    // --- [DOCUMENT]
    public static IO<Seq<EffectRow>> Rows(RhinoDoc doc) =>
        WithEffects(doc, static effects => TableOps.ReadRows(effects.ToArray, static data => IO.lift(() => EffectMapper.ToRow(data))));

    public static IO<Unit> SetState(RhinoDoc doc, Guid id, Option<bool> enabled, Option<bool> shown) =>
        WithEffect(doc, id, data =>
            from turned in IO.lift(() => enabled.Iter(value => data.On = value))
            from revealed in IO.lift(() => shown.Iter(value => data.Shown = value))
            select unit);

    public static IO<Unit> Reorder(RhinoDoc doc, Seq<Guid> order) =>
        WithEffects(doc, effects => order.TraverseM(id => IO.lift(() => Refused.Unless(effects.MovePostEffectBefore(id, Guid.Empty), nameof(PostEffectCollection.MovePostEffectBefore)))).As().Map(static _ => unit));

    public static IO<Option<Guid>> Selected(RhinoDoc doc, PostEffectType type) =>
        WithEffects(doc, effects => IO.lift(() => Answers.Found(effects.GetSelectedPostEffect(SelectionNode(type), out Guid id), id).Bind(Answers.Present)));

    public static IO<Unit> Select(RhinoDoc doc, PostEffectType type, Guid id) =>
        WithEffects(doc, effects => IO.lift(() => effects.SetSelectedPostEffect(SelectionNode(type), id)));

    public static IO<IConvertible> GetParameter(RhinoDoc doc, Guid id, string name) =>
        WithEffect(doc, id, data => IO.lift(() => Missing.Unless(data.GetParameter(name), nameof(PostEffectData.GetParameter))));

    private static IO<TValue> WithEffects<TValue>(RhinoDoc doc, Func<PostEffectCollection, IO<TValue>> body) =>
        IO.lift(() => doc.RenderSettings)
            .Map(static settings => (Settings: settings, Effects: settings.PostEffects))
            .Bracket(Use: held => body(held.Effects), Fin: static held => DisposalOps.Release(Seq<IDisposable>(held.Settings, held.Effects)));

    private static IO<TValue> WithEffect<TValue>(RhinoDoc doc, Guid id, Func<PostEffectData, IO<TValue>> body) =>
        WithEffects(doc, effects => DisposalOps.Using(
            IO.lift(() => Answers.Present(effects)),
            rows => IO.lift(rows.Find(row => row.Id == id).ToFin(new UnknownEffect(id))).Bind(body)));

    private static PostEffectType SelectionNode(PostEffectType type) =>
        (PostEffectType)((int)type + 1);
}
