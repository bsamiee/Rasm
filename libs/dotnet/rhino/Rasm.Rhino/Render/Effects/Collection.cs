using System.Text.Json;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.Render.Views;
using Rhino;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record EffectSource {
    public sealed record Document(RhinoDoc Doc) : EffectSource {
        public HostEvent<Unit> Changed =>
            Documents.PropertiesChanged.Documents().In(Doc.RuntimeSerialNumber).Choose(static _ => Some(unit));
    }

    public sealed record Section(EtoPostEffectCollapsibleSection Host) : EffectSource;
}

[Union]
public abstract partial record EffectEntry {
    private EffectEntry(Guid id) => Id = id;

    public Guid Id { get; }
    public bool Runs => Switch(listed: static listed => listed.On && listed.Shown, selectable: static selectable => selectable.Selected);

    public sealed record Listed(Guid Id, bool On, bool Shown) : EffectEntry(Id);

    public sealed record Selectable(Guid Id, bool Selected) : EffectEntry(Id);

    internal static EffectEntry Of(PostEffectData data, Guid selected) =>
        data.Type is PostEffectType.ToneMapping ? new Selectable(data.Id, data.Id == selected) : new Listed(data.Id, data.On, data.Shown);

    internal static EffectEntry Of(PostEffect effect) =>
        effect.PostEffectType is PostEffectType.ToneMapping ? new Selectable(effect.Id, effect.IsSelected) : new Listed(effect.Id, effect.On, effect.Shown);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectList {
    // --- [ENTRIES]
    public const string StateKey = "state";

    public static IO<Seq<EffectEntry>> Read(RenderSettings settings) =>
        Within(settings, static (effects, held) => IO.lift(() => ToneMapper(Conversions.Present(effects.GetSelectedPostEffect(Internals.ToneMappingStage, out Guid id), id)) switch {
            var selected => held.Map(data => EffectEntry.Of(data, selected)).Strict(),
        }));

    public static IO<Seq<EffectEntry>> Read(EffectSource source) =>
        source.Switch(
            document: static held => RenderSettingsSources.Read(held.Doc, static scope => Read(scope.Settings)),
            section: static held => IO.lift(() => Listed(held.Host).Map(EffectEntry.Of).Strict()));

    // --- [STATE]
    public static IO<TState> State<TState>(RenderSettings settings, Guid effect, TState @default) where TState : notnull =>
        Within(settings, (_, held) => held.Find(data => data.Id == effect).Match(
            Some: data => Recalled(Internals.Unwrapped<string>(() => data.GetParameter(StateKey), StateKey), @default),
            None: () => IO.pure(@default)));

    public static IO<TState> State<TState>(EffectSource source, Guid effect, TState @default) where TState : notnull =>
        source.Switch(
            (Effect: effect, Default: @default),
            document: static (wanted, held) => RenderSettingsSources.Read(held.Doc, scope => State(scope.Settings, wanted.Effect, wanted.Default)),
            section: static (wanted, held) => IO.lift(() => Listed(held.Host).Find(listed => listed.Id == wanted.Effect).ToFin(Missing(wanted.Effect)))
                .Bind(listed => Recalled(IO.lift(() => {
                    object? value = null;
                    return listed.GetParam(StateKey, ref value) ? Optional(value as string) : None;
                }), wanted.Default)));

    public static Fin<TState> State<TState>(Rimage image, Guid effect, TState @default) where TState : notnull =>
        image.State.Find(effect).Bind(static fields => fields.Find(StateKey)).Match(Some: Decoded<TState>, None: () => @default);

    public static IO<Unit> Tune<TState>(EffectSource source, Guid effect, TState state) where TState : notnull =>
        source.Switch(
            (Effect: effect, Text: Encoded(state)),
            document: static (tuned, held) => RenderSettingsSources.Edit(held.Doc, scope => Edit(scope.Settings, [Tuned(tuned.Effect, tuned.Text)])),
            section: static (tuned, held) => SectionModels.Write(held.Host, Provider.RdkRenderingPostEffects, _ =>
                IO.lift(() => Listed(held.Host).Find(listed => listed.Id == tuned.Effect).ToFin(Missing(tuned.Effect)))
                    .Bind(listed => Products.Accepted(() => listed.SetParam(StateKey, tuned.Text)))));

    // --- [EDITS]
    public static IO<Unit> Edit(RenderSettings settings, Seq<Step<PostEffectCollection>> steps) =>
        (from effects in Products.Of(() => settings.PostEffects)
         from edited in Edits.Apply(Stages.InPlace<PostEffectCollection>(), effects, steps)
         select unit).Bracket();

    public static Step<PostEffectCollection> Shown(Guid effect, bool on, bool shown) =>
        new(effects => Found(effects, effect).Map(data => {
            data.On = on;
            data.Shown = shown;
            return effects;
        }), $"shown {effect} = ({on}, {shown})");

    public static Step<PostEffectCollection> Selected(Guid effect) =>
        new(effects => Found(effects, effect).Map(_ => {
            effects.SetSelectedPostEffect(Internals.ToneMappingStage, effect);
            return effects;
        }), $"selected {effect}");

    public static Step<PostEffectCollection> Ordered(Seq<Guid> ids) =>
        new(effects => IO.lift(() => Products.Each(ids, (id, index) => RhinoFault.Unless(effects.MovePostEffectBefore(id, Guid.Empty), RhinoReason.Declined, id, index))).Map(_ => effects),
            $"ordered {ids}");

    public static Step<PostEffectCollection> Tuned<TState>(Guid effect, TState state) where TState : notnull => Tuned(effect, Encoded(state));

    private static Step<PostEffectCollection> Tuned(Guid effect, string text) =>
        new(effects => Found(effects, effect).Bind(data => Products.Accepted(() => data.SetParameter(StateKey, text))).Map(_ => effects), $"tuned {effect}");

    // --- [RUNNING]
    private static readonly AtomHashMap<int, (PostEffect Effect, Func<string, bool> Holds)> Running = AtomHashMap<int, (PostEffect Effect, Func<string, bool> Holds)>();

    internal static IO<Unit> Following =>
        Documents.PropertiesChanged.Attach(static args => Synced(args.Document));

    internal static Unit Held(PostEffect effect, Func<string, bool> holds) => Running.AddOrUpdate(effect.SerialNumber, (effect, holds));

    internal static Unit Released(PostEffect effect) => Running.Remove(effect.SerialNumber);

    private static IO<Unit> Synced(RhinoDoc changed) =>
        from running in IO.lift(static () => toSeq(Running.Values).Strict())
        from tuned in when(!running.IsEmpty,
            from stored in IO.lift(static () => Conversions.Rows(RhinoDoc.OpenDocuments())).Bind(static documents => Products.Each(documents.Map(static doc =>
                RenderSettingsSources.Read(doc, static scope => Within(scope.Settings, static (_, held) =>
                        held.Traverse(static data => Internals.Unwrapped<string>(() => data.GetParameter(StateKey), StateKey).Map(text => text.Map(stored => (data.Id, stored)))).As()
                            .Map(static rows => toHashMap(rows.Somes()))))
                    .Map(states => (doc.RuntimeSerialNumber, States: states)))))
            let current = toHashMap(stored)
            from synced in Products.Each(current.Find(changed.RuntimeSerialNumber).ToSeq().Bind(states => running.Choose(instance =>
                from text in states.Find(instance.Effect.Id)
                where !current.Values.Exists(other => other.Find(instance.Effect.Id).Exists(instance.Holds))
                select Products.Accepted(() => instance.Effect.SetParam(StateKey, text)))))
            select unit).As()
        select unit;

    // --- [CODEC]
    internal static string Encoded<TState>(TState state) where TState : notnull => JsonSerializer.Serialize(state, ArchivableDictionaries.Options);

    internal static Fin<TState> Decoded<TState>(string text) where TState : notnull =>
        ArchivableDictionaries.Decoded(typeof(TState).Name, () => JsonSerializer.Deserialize<TState>(text, ArchivableDictionaries.Options));

    internal static Guid ToneMapper(Option<Guid> chosen) => chosen.Bind(Conversions.Present).IfNone(static () => PostEffectUuids.ToneMapper_Clamp);

    private static IO<TState> Recalled<TState>(IO<Option<string>> stored, TState @default) where TState : notnull =>
        stored.Bind(text => IO.lift(text.Match(Some: Decoded<TState>, None: () => (Fin<TState>)@default)));

    // --- [SCOPE]
    private static Seq<PostEffect> Listed(EtoPostEffectCollapsibleSection host) =>
        toSeq(Enum.GetValues<PostEffectType>()).Bind(type => toSeq(host.GetPostEffects(type))).Strict();

    private static IO<A> Within<A>(RenderSettings settings, Func<PostEffectCollection, Seq<PostEffectData>, IO<A>> body) =>
        (from effects in Products.Of(() => settings.PostEffects)
         from held in Products.Many(() => effects, required: false)
         from value in body(effects, held)
         select value).Bracket();

    private static IO<PostEffectData> Found(PostEffectCollection effects, Guid id) =>
        Products.Many(() => effects, required: false).Bind(held => IO.lift(held.Find(data => data.Id == id).ToFin(Missing(id))));

    private static RhinoFault Missing(Guid id) => new($"post effect {id}", RhinoReason.Missing);
}
