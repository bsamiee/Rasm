using System.Globalization;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Riok.Mapperly.Abstractions;

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
            despeckle: static () => new Guid("57baba9e-fc02-40d2-9682-5f61b9049eaa"),
            denoise: static () => new Guid("1ec87a84-f61f-43dc-b331-92d8b81ed468"),
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
            FieldTexts<TState>.Items.Choose(field => field.Capture(state).Map(text => (EffectRequest)new Tuning(typeof(TEffect).GUID, EffectCollection.ParameterName(field.Path), text)));

        public static Tuning Amount<TEffect>(Mix mix) where TEffect : PostEffect =>
            new(typeof(TEffect).GUID, EffectStates.Amount.Name, EffectStates.Amount.Text(mix));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class EffectMapper {
    [MapProperty(nameof(PostEffectData.LocalName), nameof(EffectEntry.LocalName), SuppressNullMismatchDiagnostic = true)]
    [MapPropertyFromSource(nameof(EffectEntry.Crc), Use = nameof(Crc))]
    internal static partial EffectEntry.Listed ToListed(PostEffectData data);

    [MapProperty(nameof(PostEffectData.LocalName), nameof(EffectEntry.LocalName), SuppressNullMismatchDiagnostic = true)]
    [MapPropertyFromSource(nameof(EffectEntry.Crc), Use = nameof(Crc))]
    internal static partial EffectEntry.Selectable ToSelectable(PostEffectData data, bool selected);

    internal static EffectEntry ToEntry(PostEffectData data, Guid selected) =>
        data.Type switch {
            PostEffectType.Early or PostEffectType.Late => ToListed(data),
            PostEffectType.ToneMapping => ToSelectable(data, data.Id == selected),
        };

    private static uint Crc(PostEffectData data) => data.DataCRC(0u);
}

public static class EffectCollection {
    // --- [SCOPE]
    public static IO<Seq<EffectEntry>> Apply(RenderSettings settings, Seq<EffectRequest> requests) =>
        from effects in IO.lift(() => settings.PostEffects)
        from held in IO.lift(() => Conversions.Rows(effects))
        from written in IO.lift(() => Callbacks.Each(requests, (request, _) => Applied(effects, held, request)))
        from selected in IO.lift(() => Selected(effects))
        from entries in IO.lift(() => Conversions.Rows(effects).Map(data => EffectMapper.ToEntry(data, selected)).Strict())
        select entries;

    public static IO<TState> State<TEffect, TState>(RenderSettings settings) where TEffect : PostEffect where TState : IStateRecord<TState> =>
        Entry(settings, typeof(TEffect).GUID, static data => FieldTexts.Recalled<TState>(EntryKey.TypeOwner(typeof(TEffect).GUID), path => Stored(data, ParameterName(path))));

    public static IO<Option<Mix>> Amount<TEffect>(RenderSettings settings) where TEffect : PostEffect =>
        Entry(settings, typeof(TEffect).GUID, static data => IO.lift(() => Stored(data, EffectStates.Amount.Name)
            .Traverse(static text => new EntryKey(EntryKey.TypeOwner(typeof(TEffect).GUID), Seq(EffectStates.Amount.Name)).Read(text, EffectStates.Amount.Read))
            .As()));

    private static IO<A> Entry<A>(RenderSettings settings, Guid id, Func<PostEffectData, IO<A>> read) =>
        IO.lift(() => Resolved(Conversions.Rows(settings.PostEffects), id)).Bind(read);

    private static Fin<PostEffectData> Resolved(Seq<PostEffectData> held, Guid id) =>
        held.Find(data => data.Id == id).ToFin(new UnknownEffect(id));

    // --- [PARAMETERS]
    internal static string ParameterName(Seq<string> path) => string.Join('.', path);

    private static Option<string> Stored(PostEffectData data, string name) =>
        Optional(data.GetParameter(name)).Map(static variant => variant.ToString(CultureInfo.InvariantCulture));

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
    private static readonly Type Pending = typeof(EffectCollection);

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(Seq<EffectKind> kinds) =>
        (_, sink) => DisposalOps.AcquireAll(
                Seq(
                    EventKind.DocumentArrived.Choose(static serial => Some((serial, unit))).Through(Subscriptions.Idle<uint, Unit>(serials =>
                        Arrange(kinds, () => toSeq(serials.Keys).Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial))), Mark)), sink),
                    EventKind.ImageFileSaved.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ =>
                        Arrange(kinds, static () => toSeq(RhinoDoc.OpenDocuments()).Filter(static doc => doc.RuntimeData.ContainsKey(Pending)), Write)), sink)),
                DisposalOps.Release)
            .Map(held => DisposalOps.Composite(held, new CallbackSite(sink, typeof(EffectCollection), nameof(Register))));

    private static IO<Unit> Arrange(Seq<EffectKind> kinds, Func<Seq<RhinoDoc>> documents, Func<RhinoDoc, Option<Seq<EffectRequest>>, IO<Unit>> step) =>
        from held in IO.lift(() => documents().Strict())
        from arranged in Callbacks.Each(held.Map(doc => Apply(doc.RenderSettings, []).Bind(entries => step(doc, Arrangement(kinds, entries)))))
        select unit;

    private static IO<Unit> Mark(RhinoDoc doc, Option<Seq<EffectRequest>> arranged) =>
        when(arranged.IsNone, IO.lift(() => { doc.RuntimeData[Pending] = unit; })).As();

    private static IO<Unit> Write(RhinoDoc doc, Option<Seq<EffectRequest>> arranged) =>
        from released in when(arranged.IsSome, IO.lift(() => { doc.RuntimeData.Remove(Pending); })).As()
        from written in arranged.Traverse(requests => Apply(doc.RenderSettings, requests)).As()
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
