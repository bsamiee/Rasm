using System.Globalization;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Settings;
using Rhino;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class BuiltinEffect {
    public static readonly BuiltinEffect Despeckle = new(new Guid(0x57baba9e, 0xfc02, 0x40d2, 0x96, 0x82, 0x5f, 0x61, 0xb9, 0x04, 0x9e, 0xaa));
    public static readonly BuiltinEffect Denoise = new(new Guid(0x1ec87a84, 0xf61f, 0x43dc, 0xb3, 0x31, 0x92, 0xd8, 0xb8, 0x1e, 0xd4, 0x68));
    public static readonly BuiltinEffect Clamp = new(PostEffectUuids.ToneMapper_Clamp);
    public static readonly BuiltinEffect Gamma = new(PostEffectUuids.Gamma);

    public Guid Id { get; }
}

[Union]
public abstract partial record EffectEntry {
    private EffectEntry(Guid id) => Id = id;

    public Guid Id { get; }
    public bool Runs => Switch(listed: static listed => listed.On, selectable: static selectable => selectable.Selected);

    public sealed record Listed(Guid Id, bool On, bool Shown) : EffectEntry(Id);

    public sealed record Selectable(Guid Id, bool Selected) : EffectEntry(Id);

    internal static EffectEntry Of(PostEffectData data, Guid selected) =>
        data.Type switch {
            PostEffectType.Early or PostEffectType.Late => new Listed(data.Id, data.On, data.Shown),
            PostEffectType.ToneMapping => new Selectable(data.Id, data.Id == selected),
        };
}

[Union]
public abstract partial record EffectRequest {
    public sealed record Placing(EffectEntry Entry) : EffectRequest;

    public sealed record Ordering(Seq<Guid> Ids) : EffectRequest;

    public sealed record Tuning : EffectRequest {
        private Tuning(Guid id, string key, string text) => (Id, Key, Text) = (id, key, text);

        public Guid Id { get; }
        public string Key { get; }
        public string Text { get; }

        public static Seq<EffectRequest> Of<TEffect, TState>(TState state) where TEffect : PostEffect where TState : IStateRecord<TState> =>
            EffectStates.Captured(state).Map(static entry => (EffectRequest)new Tuning(typeof(TEffect).GUID, entry.Key, entry.Text));

        public static Tuning Amount<TEffect>(AxisFraction mix) where TEffect : PostEffect =>
            new(typeof(TEffect).GUID, EffectStates.Amount.Name, EffectStates.Amount.Text(mix));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectRegistry {
    // --- [DEFINITIONS]
    extension(Seq<EffectKind> chain) {
        public Seq<EffectKind> Add<TEffect>(Option<BuiltinEffect> replaces = default)
            where TEffect : PostEffect, IEffectDefinition =>
            chain.Add(TEffect.Kind with { Replaces = replaces });
    }

    // --- [SCOPE]
    public static IO<Seq<EffectEntry>> Read(RenderSettings settings) =>
        Within(settings, static (effects, held) => IO.lift(() => {
            Guid selected = Selected(effects);
            return held.Map(data => EffectEntry.Of(data, selected)).Strict();
        }));

    public static IO<Unit> Apply(RenderSettings settings, Seq<EffectRequest> requests) =>
        Within(settings, (effects, held) => IO.lift(() => Callbacks.Each(requests, (request, _) => Applied(effects, held, request))).Map(unit));

    public static IO<TState> State<TEffect, TState>(RenderSettings settings, Option<TState> factory = default) where TEffect : PostEffect where TState : IStateRecord<TState> =>
        Entry(settings, typeof(TEffect).GUID, data => FieldTexts.Recalled(EntryKey.TypeOwner(typeof(TEffect).GUID), TState.Parameters, path => Stored(data, EntryKey.Name(path)), factory.IfNone(TState.Default)));

    public static IO<Option<AxisFraction>> Amount<TEffect>(RenderSettings settings) where TEffect : PostEffect =>
        Entry(settings, typeof(TEffect).GUID, static data => Stored(data, EffectStates.Amount.Name)
            .Bind(static stored => IO.lift(() => stored.Traverse(static text => EffectStates.AmountOf(EntryKey.TypeOwner(typeof(TEffect).GUID), text)).As())));

    private static IO<A> Entry<A>(RenderSettings settings, Guid id, Func<PostEffectData, IO<A>> read) =>
        Within(settings, (_, held) => IO.lift(() => Resolved(held, id)).Bind(read));

    private static IO<A> Within<A>(RenderSettings settings, Func<PostEffectCollection, Seq<PostEffectData>, IO<A>> body) =>
        DisposalOps.Within(() => settings.PostEffects, effects => DisposalOps.Within(
            () => Conversions.Rows(effects).Strict(),
            held => body(effects, held),
            DisposalOps.Release));

    private static Fin<PostEffectData> Resolved(Seq<PostEffectData> held, Guid id) =>
        held.Find(data => data.Id == id).ToFin(new NotFound("post effect", $"{id}"));

    // --- [PARAMETERS]
    private static IO<Option<string>> Stored(PostEffectData data, string name) =>
        DisposalOps.Within(
            () => Optional(data.GetParameter(name)),
            static found => IO.lift(() => found.Map(static variant => variant.ToString(CultureInfo.InvariantCulture))),
            static found => DisposalOps.Release(found.ToSeq().Map(static variant => (IDisposable)variant)));

    // --- [REQUESTS]
    private const PostEffectType ToneMappingNode = PostEffectType.ToneMapping + 1;

    private static Fin<Unit> Applied(PostEffectCollection effects, Seq<PostEffectData> held, EffectRequest request) =>
        request.Switch(
            (Effects: effects, Held: held),
            placing: static (scope, placing) => Resolved(scope.Held, placing.Entry.Id).Map(data => placing.Entry.Switch(
                (scope.Effects, Data: data),
                listed: static (target, listed) => {
                    target.Data.On = listed.On;
                    target.Data.Shown = listed.Shown;
                    return unit;
                },
                selectable: static (target, selectable) => {
                    if (selectable.Selected) target.Effects.SetSelectedPostEffect(ToneMappingNode, selectable.Id);
                    return unit;
                })),
            ordering: static (scope, ordering) => Callbacks.Each(
                ordering.Ids, id => scope.Effects.MovePostEffectBefore(id, Guid.Empty), nameof(PostEffectCollection.MovePostEffectBefore)),
            tuning: static (scope, tuning) => Resolved(scope.Held, tuning.Id)
                .Bind(data => Refused.Unless(data.SetParameter(tuning.Key, tuning.Text), nameof(PostEffectData.SetParameter))));

    // --- [ENTRIES]
    private static Guid Selected(PostEffectCollection effects) =>
        Callbacks.Found(effects.GetSelectedPostEffect(ToneMappingNode, out Guid id), id).Bind(Conversions.Present).IfNone(static () => BuiltinEffect.Clamp.Id);

    // --- [ARRANGEMENT]
    private static readonly Type Pending = typeof(EffectRegistry);

    public static PlugInSlot<Seq<EffectKind>> Kinds { get; } = new();

    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> Register(Seq<EffectKind> kinds) =>
        (sink, retain) => DisposalOps.Acquire(
            owned =>
                from published in Kinds.Publish(sink, kinds)
                from arrived in EventKind.DocumentArrived.Choose(static serial => Some((serial, unit))).Through(Subscriptions.Idle<uint, Unit>(serials =>
                    Arrange(kinds, () => toSeq(serials.Keys).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))), Mark)), sink, owned)
                from saved in EventKind.ImageFileSaved.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ =>
                    Arrange(kinds, static () => toSeq(RhinoDoc.OpenDocuments()).Filter(static doc => doc.RuntimeData.ContainsKey(Pending)), Write)), sink, owned)
                select unit,
            retain);

    private static IO<Unit> Arrange(Seq<EffectKind> kinds, Func<Seq<RhinoDoc>> documents, Func<RhinoDoc, Option<Seq<EffectRequest>>, IO<Unit>> step) =>
        from held in IO.lift(() => documents().Strict())
        from arranged in Callbacks.Each(held.Map(doc => RenderSettingsSources.Read(doc, static scope => Read(scope.Settings)).Bind(entries => step(doc, Arrangement(kinds, entries)))))
        select unit;

    private static IO<Unit> Mark(RhinoDoc doc, Option<Seq<EffectRequest>> arranged) =>
        when(arranged.IsNone, IO.lift(() => { doc.RuntimeData[Pending] = unit; })).As();

    private static IO<Unit> Write(RhinoDoc doc, Option<Seq<EffectRequest>> arranged) =>
        from released in when(arranged.IsSome, IO.lift(() => { _ = doc.RuntimeData.Remove(Pending); })).As()
        from written in arranged.Traverse(requests => RenderSettingsSources.Edit(doc, scope => Apply(scope.Settings, requests))).As()
        select unit;

    private static Option<Seq<EffectRequest>> Arrangement(Seq<EffectKind> kinds, Seq<EffectEntry> entries) =>
        toHashMap(entries.Map(static entry => (entry.Id, entry))) switch {
            var listed =>
                from placed in kinds.Traverse(kind => listed.Find(kind.Id).Map(entry => (Kind: kind, Entry: entry))).As()
                from replaced in kinds.Bind(static kind => kind.Replaces.ToSeq()).Traverse(builtin => listed.Find(builtin.Id)).As()
                let named = toHashSet(placed.Map(static held => held.Entry.Id).Add(BuiltinEffect.Despeckle.Id))
                let staged = fun((PostEffectType stage) => placed.Filter(held => held.Kind.Stage == stage).Map(static held => held.Entry))
                select placed.Choose(static held => held.Entry.Switch(
                        held.Kind.Replaces.IsSome,
                        listed: static (replacing, entry) => replacing ? Some<EffectRequest>(new EffectRequest.Placing(entry with { On = true, Shown = true })) : None,
                        selectable: static (_, entry) => Some<EffectRequest>(new EffectRequest.Placing(entry with { Selected = true }))))
                    + replaced.Choose(static entry => entry.Switch(
                        listed: static held => Some<EffectRequest>(new EffectRequest.Placing(held with { On = false })),
                        selectable: static _ => Option<EffectRequest>.None))
                    + Seq<EffectRequest>(new EffectRequest.Ordering(
                        (staged(PostEffectType.Early)
                        + entries.Filter(entry => !named.Contains(entry.Id) && entry.Switch(listed: static _ => true, selectable: static _ => false))
                        + staged(PostEffectType.Late)).Map(static entry => entry.Id))),
        };
}
