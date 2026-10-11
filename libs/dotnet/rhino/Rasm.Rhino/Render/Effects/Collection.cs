using System.Globalization;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Scenes;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class BuiltinEffect {
    public static readonly BuiltinEffect Despeckle = new();
    public static readonly BuiltinEffect Denoise = new();
    public static readonly BuiltinEffect Clamp = new();
    public static readonly BuiltinEffect Gamma = new();

    public Guid Id =>
        Switch(
            despeckle: static () => new Guid(0x57baba9e, 0xfc02, 0x40d2, 0x96, 0x82, 0x5f, 0x61, 0xb9, 0x04, 0x9e, 0xaa),
            denoise: static () => new Guid(0x1ec87a84, 0xf61f, 0x43dc, 0xb3, 0x31, 0x92, 0xd8, 0xb8, 0x1e, 0xd4, 0x68),
            clamp: static () => PostEffectUuids.ToneMapper_Clamp,
            gamma: static () => PostEffectUuids.Gamma);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BuiltinParameter {
    public static readonly BuiltinParameter Gamma = new("gamma", BuiltinEffect.Gamma);

    public BuiltinEffect Effect { get; }
}

[Union]
public abstract partial record EffectEntry {
    private EffectEntry(Guid id, string localName, uint crc) => (Id, LocalName, Crc) = (id, localName, crc);

    public Guid Id { get; }
    public string LocalName { get; }
    public uint Crc { get; }
    public bool Runs => Switch(listed: static listed => listed.On, selectable: static selectable => selectable.Selected);

    public sealed record Listed(Guid Id, string LocalName, bool On, bool Shown, uint Crc) : EffectEntry(Id, LocalName, Crc);

    public sealed record Selectable(Guid Id, string LocalName, bool Selected, uint Crc) : EffectEntry(Id, LocalName, Crc);
}

[Union]
public abstract partial record EffectRequest {
    public sealed record Listing(EffectEntry.Listed Entry) : EffectRequest;

    public sealed record Ordering(Seq<EffectEntry> Entries) : EffectRequest;

    public sealed record Selecting(EffectEntry.Selectable Entry) : EffectRequest;

    public sealed record Tuning : EffectRequest {
        private Tuning(Guid id, string key, string text) => (Id, Key, Text) = (id, key, text);

        public Guid Id { get; }
        public string Key { get; }
        public string Text { get; }

        public static Seq<EffectRequest> Of<TEffect, TState>(TState state) where TEffect : PostEffect where TState : IStateRecord<TState> =>
            FieldTexts.Items<TState>().Choose(field => field.Capture(state).Map(text => (EffectRequest)new Tuning(typeof(TEffect).GUID, EntryKey.Name(field.Path), text)));

        public static Tuning Amount<TEffect>(Mix mix) where TEffect : PostEffect =>
            new(typeof(TEffect).GUID, EffectStates.Amount.Name, EffectStates.Amount.Text(mix));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class EffectMapper {
    internal static EffectEntry ToEntry(PostEffectData data, Guid selected) =>
        (data.Type, data.Id, data.LocalName, data.DataCRC(0u)) switch {
            (PostEffectType.Early or PostEffectType.Late, var id, var name, var crc) => new EffectEntry.Listed(id, name, data.On, data.Shown, crc),
            (PostEffectType.ToneMapping, var id, var name, var crc) => new EffectEntry.Selectable(id, name, id == selected, crc),
        };
}

public static class EffectRegistry {
    // --- [DEFINITIONS]
    public static Seq<EffectKind> Add<TEffect>(this Seq<EffectKind> chain, Option<BuiltinEffect> replaces = default)
        where TEffect : PostEffect, IEffectDefinition =>
        chain.Add(TEffect.Kind with { Replaces = replaces });

    // --- [SCOPE]
    public static IO<Seq<EffectEntry>> Read(RenderSettings settings) =>
        Within(settings, static (effects, held) => IO.lift(() => {
            Guid selected = Selected(effects);
            return held.Map(data => EffectMapper.ToEntry(data, selected)).Strict();
        }));

    public static IO<Unit> Apply(RenderSettings settings, Seq<EffectRequest> requests) =>
        Within(settings, (effects, held) => IO.lift(() => Callbacks.Each(requests, (request, _) => Applied(effects, held, request))).Map(static _ => unit));

    public static IO<BindingGroup> Bindings(RhinoDoc doc, Seq<EffectKind> kinds) =>
        SceneSources.Read(doc, source => Within(source.Settings, (_, held) => IO.lift(() => {
            LanguageExt.HashSet<Guid> present = toHashSet(held.Map(static data => data.Id));
            return new BindingGroup(kinds.Filter(kind => present.Contains(kind.Id)).Map(kind => kind.Binding(doc)).Strict());
        })));

    public static IO<TState> State<TEffect, TState>(RenderSettings settings, Option<TState> factory = default) where TEffect : PostEffect where TState : IStateRecord<TState> =>
        Entry(settings, typeof(TEffect).GUID, data => FieldTexts.Recalled(EntryKey.TypeOwner(typeof(TEffect).GUID), toSeq(TState.Parameters), path => Stored(data, EntryKey.Name(path)), factory.IfNone(TState.Default)));

    public static IO<Option<Mix>> Amount<TEffect>(RenderSettings settings) where TEffect : PostEffect =>
        Entry(settings, typeof(TEffect).GUID, static data => Stored(data, EffectStates.Amount.Name)
            .Bind(static stored => IO.lift(() => stored
                .Traverse(static text => new EntryKey(EntryKey.TypeOwner(typeof(TEffect).GUID), Seq(EffectStates.Amount.Name)).Read(text, EffectStates.Amount.Read))
                .As())));

    private static IO<A> Entry<A>(RenderSettings settings, Guid id, Func<PostEffectData, IO<A>> read) =>
        Within(settings, (_, held) => IO.lift(() => Resolved(held, id)).Bind(read));

    private static IO<A> Within<A>(RenderSettings settings, Func<PostEffectCollection, Seq<PostEffectData>, IO<A>> body) =>
        DisposalOps.Within(
            () => settings.PostEffects,
            effects => DisposalOps.Within(
                () => Conversions.Rows(effects).Strict(),
                held => body(effects, held),
                DisposalOps.Release),
            static effects => IO.lift(effects.Dispose));

    private static Fin<PostEffectData> Resolved(Seq<PostEffectData> held, Guid id) =>
        held.Find(data => data.Id == id).ToFin(new UnknownEffect(id));

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
            listing: static (scope, listing) => Resolved(scope.Held, listing.Entry.Id).Map(data => {
                data.On = listing.Entry.On;
                data.Shown = listing.Entry.Shown;
                return unit;
            }),
            ordering: static (scope, ordering) => Callbacks.Each(
                ordering.Entries, entry => scope.Effects.MovePostEffectBefore(entry.Id, Guid.Empty), nameof(PostEffectCollection.MovePostEffectBefore)),
            selecting: static (scope, selecting) => Resolved(scope.Held, selecting.Entry.Id).Map(_ => {
                scope.Effects.SetSelectedPostEffect(ToneMappingNode, selecting.Entry.Id);
                return unit;
            }),
            tuning: static (scope, tuning) => Resolved(scope.Held, tuning.Id)
                .Bind(data => Refused.Unless(data.SetParameter(tuning.Key, tuning.Text), nameof(PostEffectData.SetParameter))));

    // --- [ENTRIES]
    private static Guid Selected(PostEffectCollection effects) =>
        Callbacks.Found(effects.GetSelectedPostEffect(ToneMappingNode, out Guid id), id).Bind(Conversions.Present).IfNone(static () => BuiltinEffect.Clamp.Id);

    // --- [ARRANGEMENT]
    private static readonly Type Pending = typeof(EffectRegistry);

    public static Func<PlugIn, IPlugInSink, Action<IDisposable>, IO<Unit>> Register(Seq<EffectKind> kinds) =>
        (_, sink, retain) => DisposalOps.Acquire(
            owned =>
                from arrived in EventKind.DocumentArrived.Choose(static serial => Some((serial, unit))).Through(Subscriptions.Idle<uint, Unit>(serials =>
                    Arrange(kinds, () => toSeq(serials.Keys).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))), Mark)), sink, owned)
                from saved in EventKind.ImageFileSaved.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ =>
                    Arrange(kinds, static () => toSeq(RhinoDoc.OpenDocuments()).Filter(static doc => doc.RuntimeData.ContainsKey(Pending)), Write)), sink, owned)
                select unit,
            (_, lease) => retain(lease));

    private static IO<Unit> Arrange(Seq<EffectKind> kinds, Func<Seq<RhinoDoc>> documents, Func<RhinoDoc, Option<Seq<EffectRequest>>, IO<Unit>> step) =>
        from held in IO.lift(() => documents().Strict())
        from arranged in Callbacks.Each(held.Map(doc => SceneSources.Read(doc, static window => Read(window.Settings)).Bind(entries => step(doc, Arrangement(kinds, entries)))))
        select unit;

    private static IO<Unit> Mark(RhinoDoc doc, Option<Seq<EffectRequest>> arranged) =>
        when(arranged.IsNone, IO.lift(() => { doc.RuntimeData[Pending] = unit; })).As();

    private static IO<Unit> Write(RhinoDoc doc, Option<Seq<EffectRequest>> arranged) =>
        from released in when(arranged.IsSome, IO.lift(() => { _ = doc.RuntimeData.Remove(Pending); })).As()
        from written in arranged.Traverse(requests => SceneSources.Edit(doc, window => Apply(window.Settings, requests))).As()
        select unit;

    private static Option<Seq<EffectRequest>> Arrangement(Seq<EffectKind> kinds, Seq<EffectEntry> entries) =>
        from placed in kinds.Traverse(kind => entries.Find(entry => entry.Id == kind.Id).Map(entry => (Kind: kind, Entry: entry))).As()
        from replaced in kinds.Bind(static kind => kind.Replaces.ToSeq()).Traverse(builtin => entries.Find(entry => entry.Id == builtin.Id)).As()
        let named = toHashSet(placed.Map(static held => held.Entry.Id).Add(BuiltinEffect.Despeckle.Id))
        let staged = fun((PostEffectType stage) => placed.Filter(held => held.Kind.Stage == stage).Map(static held => held.Entry))
        select placed.Filter(static held => held.Kind.Replaces.IsSome).Choose(static held => held.Entry.Switch(
                listed: static entry => Some<EffectRequest>(new EffectRequest.Listing(entry with { On = true, Shown = true })),
                selectable: static _ => Option<EffectRequest>.None))
            + placed.Choose(static held => held.Entry.Switch(
                listed: static _ => Option<EffectRequest>.None,
                selectable: static entry => Some<EffectRequest>(new EffectRequest.Selecting(entry))))
            + replaced.Choose(static entry => entry.Switch(
                listed: static held => Some<EffectRequest>(new EffectRequest.Listing(held with { On = false })),
                selectable: static _ => Option<EffectRequest>.None))
            + Seq<EffectRequest>(new EffectRequest.Ordering(
                staged(PostEffectType.Early)
                + entries.Filter(entry => !named.Contains(entry.Id) && entry.Switch(listed: static _ => true, selectable: static _ => false))
                + staged(PostEffectType.Late)));
}
