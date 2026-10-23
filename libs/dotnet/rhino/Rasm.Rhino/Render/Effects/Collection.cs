using System.Globalization;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rhino;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class BuiltinEffect {
    public static readonly BuiltinEffect Despeckle = new(new Guid(0x57baba9e, 0xfc02, 0x40d2, 0x96, 0x82, 0x5f, 0x61, 0xb9, 0x04, 0x9e, 0xaa));
    public static readonly BuiltinEffect Denoise = new(new Guid(0x1ec87a84, 0xf61f, 0x43dc, 0xb3, 0x31, 0x92, 0xd8, 0xb8, 0x1e, 0xd4, 0x68));
    public static readonly BuiltinEffect Clamp = new(PostEffectUuids.ToneMapper_Clamp);
    public static readonly BuiltinEffect Gamma = new(PostEffectUuids.Gamma);
    public static readonly BuiltinEffect Dithering = new(PostEffectUuids.Dithering);

    public Guid Id { get; }
}

[Union]
public abstract partial record EffectSource {
    public HostEvent<Unit> Changed =>
        Switch(
            document: static held => ValueStore.Signal(EventKind.DocumentPropertiesChanged.In(held.Doc.RuntimeSerialNumber), static _ => true),
            section: static held => SectionRows.Changed(held.Host));

    public sealed record Document(RhinoDoc Doc) : EffectSource;

    public sealed record Section(EtoPostEffectCollapsibleSection Host) : EffectSource;
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

    internal static EffectEntry Of(PostEffect effect) =>
        effect.PostEffectType switch {
            PostEffectType.Early or PostEffectType.Late => new Listed(effect.Id, effect.On, effect.Shown),
            PostEffectType.ToneMapping => new Selectable(effect.Id, effect.IsSelected),
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

        public static Seq<Tuning> Of<TEffect, TState>(TState state) where TEffect : PostEffect where TState : IStateRecord<TState> =>
            EffectStates.Captured(state).Map(static entry => new Tuning(typeof(TEffect).GUID, entry.Key, entry.Text));

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

    public static IO<Seq<EffectEntry>> Read(EffectSource source) =>
        source.Switch(
            document: static held => RenderSettingsSources.Read(held.Doc, static scope => Read(scope.Settings)),
            section: static held => IO.lift(() => Hosted(held.Host).Map(EffectEntry.Of)));

    public static IO<Unit> Apply(RenderSettings settings, Seq<EffectRequest> requests) =>
        Within(settings, (effects, held) => IO.lift(() => Callbacks.Each(requests, (request, _) => Applied(effects, held, request))).Map(unit));

    public static IO<Unit> Tune(EffectSource source, Seq<EffectRequest.Tuning> tunings) =>
        source.Switch(
            tunings,
            document: static (requests, held) => RenderSettingsSources.Edit(held.Doc, scope => Apply(scope.Settings, requests.Map(static request => (EffectRequest)request))),
            section: static (requests, held) => SectionModels.Write(held.Host, Provider.RdkRenderingPostEffects, _ => IO.lift(() => Hosted(held.Host) switch {
                var effects => Callbacks.Each(requests, (request, _) => effects.Find(effect => effect.Id == request.Id).ToFin(new NotFound("post effect", $"{request.Id}"))
                    .Bind(effect => Refused.Unless(effect.SetParam(request.Key, request.Text), nameof(PostEffect.SetParam)))),
            }).Map(unit)));

    public static IO<TState> State<TEffect, TState>(RenderSettings settings, Option<TState> factory = default) where TEffect : PostEffect where TState : IStateRecord<TState> =>
        Entry(settings, typeof(TEffect).GUID, data => Recalled<TEffect, TState>(name => Stored(data, name), factory.IfNone(TState.Default)));

    public static IO<TState> State<TEffect, TState>(EffectSource source, TState factory) where TEffect : PostEffect where TState : IStateRecord<TState> =>
        Stored<TEffect, TState>(source, stored => Recalled<TEffect, TState>(stored, factory));

    public static IO<Option<AxisFraction>> Amount<TEffect>(EffectSource source) where TEffect : PostEffect =>
        Stored<TEffect, Option<AxisFraction>>(source, static stored => stored(EffectStates.Amount.Name)
            .Bind(static text => IO.lift(() => text.Traverse(static held => EffectStates.AmountOf(EntryKey.TypeOwner(typeof(TEffect).GUID), held)).As())));

    private static IO<TState> Recalled<TEffect, TState>(Func<string, IO<Option<string>>> stored, TState factory) where TEffect : PostEffect where TState : IStateRecord<TState> =>
        FieldTexts.Recalled(EntryKey.TypeOwner(typeof(TEffect).GUID), TState.Parameters, path => stored(EntryKey.Name(path)), factory);

    private static IO<A> Stored<TEffect, A>(EffectSource source, Func<Func<string, IO<Option<string>>>, IO<A>> read) where TEffect : PostEffect =>
        source.Switch(
            read,
            document: static (body, held) => RenderSettingsSources.Read(held.Doc, scope => Entry(scope.Settings, typeof(TEffect).GUID, data => body(name => Stored(data, name)))),
            section: static (body, held) => IO.lift(() => Hosted(held.Host).Find(static effect => effect.Id == typeof(TEffect).GUID)
                    .ToFin(new NotFound("post effect", $"{typeof(TEffect).GUID}")))
                .Bind(effect => body(name => Parameter(effect, name))));

    private static IO<A> Entry<A>(RenderSettings settings, Guid id, Func<PostEffectData, IO<A>> read) =>
        Within(settings, (_, held) => IO.lift(() => Resolved(held, id)).Bind(read));

    private static IO<A> Within<A>(RenderSettings settings, Func<PostEffectCollection, Seq<PostEffectData>, IO<A>> body) =>
        DisposalOps.Within(() => settings.PostEffects, effects => DisposalOps.Within(
            () => Conversions.Rows(effects).Strict(),
            held => body(effects, held),
            DisposalOps.Release));

    private static Seq<PostEffect> Hosted(EtoPostEffectCollapsibleSection host) =>
        toSeq(Enum.GetValues<PostEffectType>()).Bind(type => toSeq(host.GetPostEffects(type))).Strict();

    private static Fin<PostEffectData> Resolved(Seq<PostEffectData> held, Guid id) =>
        held.Find(data => data.Id == id).ToFin(new NotFound("post effect", $"{id}"));

    // --- [PARAMETERS]
    private static IO<Option<string>> Stored(PostEffectData data, string name) =>
        DisposalOps.Within(
            () => Optional(data.GetParameter(name)),
            static found => IO.lift(() => found.Map(static variant => variant.ToString(CultureInfo.InvariantCulture))),
            static found => DisposalOps.Release(found.ToSeq().Map(static variant => (IDisposable)variant)));

    private static IO<Option<string>> Parameter(PostEffect effect, string name) =>
        IO.lift(() => {
            object? value = null;
            return effect.GetParam(name, ref value) ? Optional(value).Bind(static held => Optional(Convert.ToString(held, CultureInfo.InvariantCulture))) : None;
        });

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

    public static IO<BindingGroup> Bindings(EffectSource source, IPlugInSink sink) =>
        Kinds.Of(sink).Map(kinds => new BindingGroup(kinds.Bind(kind => kind.Binding(source))));

    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> Register(Seq<EffectKind> kinds) =>
        (sink, retain) => DisposalOps.Acquire(
            owned =>
                from published in Kinds.Publish(sink, kinds)
                from arrived in EventKind.DocumentArrived.Choose(static serial => Some((serial, unit))).Through(Subscriptions.Idle<uint, Unit>(serials =>
                    Arrange(kinds, () => toSeq(serials.Keys).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))), static (doc, arranged) =>
                        when(arranged.IsNone, IO.lift(() => { doc.RuntimeData[Pending] = unit; })).As())), sink, owned)
                from saved in EventKind.ImageFileSaved.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ =>
                    Arrange(kinds, static () => toSeq(RhinoDoc.OpenDocuments()).Filter(static doc => doc.RuntimeData.ContainsKey(Pending)), static (doc, arranged) =>
                        arranged.Traverse(requests =>
                            from released in IO.lift(() => doc.RuntimeData.Remove(Pending))
                            from written in RenderSettingsSources.Edit(doc, scope => Apply(scope.Settings, requests))
                            select written).As().Map(unit))), sink, owned)
                select unit,
            retain);

    private static IO<Unit> Arrange(Seq<EffectKind> kinds, Func<Seq<RhinoDoc>> documents, Func<RhinoDoc, Option<Seq<EffectRequest>>, IO<Unit>> step) =>
        from held in IO.lift(() => documents().Strict())
        from arranged in Callbacks.Each(held.Map(doc => RenderSettingsSources.Read(doc, static scope => Read(scope.Settings)).Bind(entries => step(doc, Arrangement(kinds, entries)))))
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
