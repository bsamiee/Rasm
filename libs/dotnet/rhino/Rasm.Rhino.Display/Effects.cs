using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EffectCallbacks(
    Func<PostEffectPipeline, Rectangle, IO<Unit>> Execute,
    Func<string, IO<Option<object>>> GetParam,
    Func<string, object, IO<Unit>> SetParam,
    Func<PostEffectState, IO<Unit>> ReadState,
    Func<PostEffectState, IO<Unit>> WriteState,
    Func<IO<Unit>> Reset,
    Func<PostEffectUI, IO<Unit>> Sections,
    Option<Func<IO<Unit>>> Help,
    Seq<Guid> RequiredChannels,
    Option<Func<PostEffectPipeline, IO<bool>>> CanExecute,
    Action<Error> Reject);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChannelRead<TValue> {
    public sealed record Cpu(Func<RenderWindow.Channel, IO<TValue>> Body) : ChannelRead<TValue>;

    public sealed record Gpu(Func<RenderWindow.ChannelGPU, IO<TValue>> Body) : ChannelRead<TValue>;
}

public sealed record EffectRow(Guid Id, PostEffectType Type, Option<string> Name, bool On, bool Shown, uint Crc);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class CallbackPostEffect(EffectCallbacks callbacks) : PostEffect {
    public sealed override Guid[] RequiredChannels =>
        [.. callbacks.RequiredChannels];

    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        Answers.Succeeded(callbacks.Execute(pipeline, rect), callbacks.Reject);

    public sealed override bool GetParam(string param, ref object v) {
        Option<object> found = Answers.Answer(callbacks.GetParam(param), callbacks.Reject, Option<object>.None);
        v = found.IfNone(v);
        return found.IsSome;
    }

    public sealed override bool SetParam(string param, object v) =>
        Bracketed(callbacks.SetParam(param, v));

    public sealed override bool ReadState(PostEffectState state) =>
        Answers.Succeeded(callbacks.ReadState(state), callbacks.Reject);

    public sealed override bool WriteState(ref PostEffectState state) =>
        Answers.Succeeded(callbacks.WriteState(state), callbacks.Reject);

    public sealed override void ResetToFactoryDefaults() =>
        _ = Bracketed(callbacks.Reset());

    public sealed override void AddUISections(PostEffectUI ui) =>
        _ = Answers.Answer(callbacks.Sections(ui), callbacks.Reject, unit);

    public sealed override bool DisplayHelp() =>
        Answers.Succeeded(callbacks.Help.Map(static help => help()), callbacks.Reject, static () => false);

    public sealed override bool CanExecute(PostEffectPipeline pipeline) =>
        Answers.Answer(callbacks.CanExecute.Map(can => can(pipeline)), callbacks.Reject, refused: false, () => base.CanExecute(pipeline));

    private bool Bracketed(IO<Unit> edit) =>
        Answers.Succeeded(
            Disposal.Bracketed(IO.lift(() => BeginChange(RenderContent.ChangeContexts.Program)), _ => IO.lift(() => Refused.Unless(EndChange(), nameof(EndChange))), _ => edit)
                .Bind(_ => IO.lift(Changed)),
            callbacks.Reject);
}

public sealed class CallbackExecutionControl(Func<Guid, IO<bool>> ready, Action<Error> reject) : PostEffectExecutionControl {
    public override bool ReadyToExecutePostEffect(Guid pep_id) =>
        Answers.Answer(ready(pep_id), reject, fallback: false);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class RenderPostEffects {
    // --- [REGISTRY]
    public static IO<Seq<Type>> Register(System.Reflection.Assembly assembly, Guid plugInId) =>
        Answers.Registered(PostEffect.RegisterPostEffect, assembly, plugInId, nameof(PostEffect.RegisterPostEffect));

    // --- [DOCUMENT]
    public static IO<Seq<EffectRow>> Rows(RhinoDoc doc) =>
        WithEffects(doc, static effects => TableOps.ReadRows(effects.ToArray, static data => IO.lift(() => EffectMapper.ToRow(data))));

    public static IO<Unit> SetState(RhinoDoc doc, Guid id, Option<bool> enabled, Option<bool> shown) =>
        WithEffect(doc, id, data =>
            from turned in IO.lift(() => enabled.Iter(value => data.On = value))
            from revealed in IO.lift(() => shown.Iter(value => data.Shown = value))
            select unit);

    public static IO<Unit> Reorder(RhinoDoc doc, Guid move, Option<Guid> before) =>
        WithEffects(doc, effects => IO.lift(() => Refused.Unless(effects.MovePostEffectBefore(move, before.IfNone(Guid.Empty)), nameof(PostEffectCollection.MovePostEffectBefore))));

    public static IO<Option<Guid>> Selected(RhinoDoc doc, PostEffectType type) =>
        WithEffects(doc, effects => IO.lift(() => Answers.Found(effects.GetSelectedPostEffect(type, out Guid id), id)));

    public static IO<Unit> Select(RhinoDoc doc, PostEffectType type, Guid id) =>
        WithEffects(doc, effects => IO.lift(() => effects.SetSelectedPostEffect(type, id)));

    public static IO<IConvertible> GetParameter(RhinoDoc doc, Guid id, string name) =>
        WithEffect(doc, id, data => IO.lift(() => Missing.Unless(data.GetParameter(name), nameof(PostEffectData.GetParameter))));

    public static IO<Unit> SetParameter(RhinoDoc doc, Guid id, string name, object value) =>
        WithEffect(doc, id, data => IO.lift(() => Refused.Unless(data.SetParameter(name, value), nameof(PostEffectData.SetParameter))));

    private static IO<TValue> WithEffects<TValue>(RhinoDoc doc, Func<PostEffectCollection, IO<TValue>> body) =>
        Disposal.Using(() => doc.RenderSettings.PostEffects, body);

    private static IO<TValue> WithEffect<TValue>(RhinoDoc doc, Guid id, Func<PostEffectData, IO<TValue>> body) =>
        WithEffects(doc, effects =>
            from ids in TableOps.ReadRows(effects.ToArray, static data => IO.lift(() => data.Id))
            from known in IO.lift(() => ids.Exists(present => present == id) ? Fin.Succ(unit) : new UnknownEffect(id))
            from value in Disposal.Using(() => effects.PostEffectDataFromId(id), body)
            select value);

    // --- [CHANNELS]
    public static IO<TValue> ReadChannel<TValue>(PostEffectPipeline pipeline, Guid channel, ChannelRead<TValue> read) =>
        Disposal.Using(
            IO.lift(() => Missing.Unless(pipeline.GetChannelForRead(channel), nameof(PostEffectPipeline.GetChannelForRead))),
            opened => read.Switch(
                (Pipeline: pipeline, Opened: opened),
                cpu: static (state, cpu) => Disposal.Using(IO.lift(() => Missing.Unless(state.Opened.CPU(), nameof(PostEffectChannel.CPU))), cpu.Body),
                gpu: static (state, gpu) =>
                    from allowed in IO.lift(() => Refused.Unless(state.Pipeline.GPUAllowed, nameof(PostEffectPipeline.GPUAllowed)))
                    from value in Disposal.Using(IO.lift(() => Missing.Unless(state.Opened.GPU(), nameof(PostEffectChannel.GPU))), gpu.Body)
                    select value));

    public static IO<Unit> WriteChannel(PostEffectPipeline pipeline, Guid channel, Func<RenderWindow.Channel, IO<Unit>> body) =>
        Disposal.Using(
            IO.lift(() => Missing.Unless(pipeline.GetChannelForWrite(channel), nameof(PostEffectPipeline.GetChannelForWrite))),
            opened =>
                from written in Disposal.Using(IO.lift(() => Missing.Unless(opened.CPU(), nameof(PostEffectChannel.CPU))), body)
                from committed in IO.lift(opened.Commit)
                select committed);
}

[Mapper]
internal static partial class EffectMapper {
    [MapProperty(nameof(PostEffectData.LocalName), nameof(EffectRow.Name))]
    [MapPropertyFromSource(nameof(EffectRow.Crc), Use = nameof(Crc))]
    internal static partial EffectRow ToRow(PostEffectData data);

    private static uint Crc(PostEffectData data) => data.DataCRC(0u);
}
