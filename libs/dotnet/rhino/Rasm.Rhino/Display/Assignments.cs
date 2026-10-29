using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DisplayModeRule(Func<RhinoObject, bool> Applies, Option<ViewportTarget> Viewport, Guid Mode);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisplayModeRules {
    // --- [MARK]
    private static readonly DictionaryCodec<ImmutableDictionary<Guid, Guid>> Mark = ArchivableDictionaries.Json<ImmutableDictionary<Guid, Guid>>(nameof(DisplayModeRules), 1);

    private static IO<HashMap<Guid, Guid>> Marked(ObjectAttributes attributes) =>
        IO.lift(() => attributes.HasUserData).Bind(held => held
            ? (from copy in use(() => attributes.Duplicate())
               from mark in ArchivableDictionaries.Read(DictionaryOwner.Of(IO.pure(copy)), Mark)
               select mark.Match(Some: static stored => toHashMap(stored.Select(static pair => (pair.Key, pair.Value))), None: static () => HashMap<Guid, Guid>())).Bracket()
            : IO.pure(HashMap<Guid, Guid>()));

    // --- [RECONCILE]
    public static IO<Seq<Guid>> Reconcile(RhinoDoc doc, Seq<DisplayModeRule> rules, Seq<RhinoObject> objects, RedrawPolicy redraw) =>
        Commits.WithinRedraw(doc, redraw,
            from slots in Products.Each(rules.Map(rule => Slot(doc, rule.Viewport).Map(slot => (Slot: slot, Rule: rule))))
            from planned in Products.Each(objects.Map(found => Planned(found, slots)))
            from written in Products.Each(planned.Somes().Map(row => new ObjectTarget.Ids(Seq(row.Id)).Objects(doc).Bind(found => AttributeEdits.Modify(
                doc, found, _ => [Rows.Write<ObjectAttributes>(staged => Written(staged, row.Writes, row.Mark)).Write], quiet: true))))
            select written.Flatten());

    private static IO<Option<(Guid Id, Seq<(Guid Slot, Option<Guid> Mode)> Writes, HashMap<Guid, Guid> Mark)>> Planned(RhinoObject found, Seq<(Guid Slot, DisplayModeRule Rule)> slots) =>
        from marked in Marked(found.Attributes)
        from ruled in IO.lift(() => Ruled(
            slots.Fold(HashMap<Guid, Guid>(), (held, row) => held.ContainsKey(row.Slot) || !row.Rule.Applies(found) ? held : held.Add(row.Slot, row.Rule.Mode)),
            marked,
            slot => DisplayMaterialRefs.DisplayMaterialRef(found.Attributes, slot)))
        select Conversions.Present(!ruled.Writes.IsEmpty || ruled.Mark != marked, (found.Id, ruled.Writes, ruled.Mark));

    private static (Seq<(Guid Slot, Option<Guid> Mode)> Writes, HashMap<Guid, Guid> Mark) Ruled(HashMap<Guid, Guid> desired, HashMap<Guid, Guid> marked, Func<Guid, Option<Guid>> held) =>
        toSeq(toHashSet(desired.Keys.Concat(marked.Keys))).Fold(
            (Writes: Seq<(Guid Slot, Option<Guid> Mode)>(), Mark: HashMap<Guid, Guid>()),
            (state, slot) => (Wanted: desired.Find(slot), Current: held(slot)) switch {
                var (wanted, current) when current.IsNone || current == marked.Find(slot) => wanted.Match(
                    Some: mode => (current == Some(mode) ? state.Writes : state.Writes.Add((slot, Some(mode))), state.Mark.Add(slot, mode)),
                    None: () => (current.IsNone ? state.Writes : state.Writes.Add((slot, Option<Guid>.None)), state.Mark)),
                _ => state,
            });

    private static IO<Unit> Written(ObjectAttributes staged, Seq<(Guid Slot, Option<Guid> Mode)> writes, HashMap<Guid, Guid> mark) =>
        from overridden in Products.Each(writes.Map(write => DisplayModeOverride(staged, write.Slot, write.Mode)))
        let owner = DictionaryOwner.Of(IO.pure(staged))
        from marked in mark.IsEmpty
            ? ArchivableDictionaries.Remove(owner, Mark)
            : ArchivableDictionaries.Write(owner, Mark, mark.AsIterable().ToImmutableDictionary(static pair => pair.Key, static pair => pair.Value))
        select unit;

    // --- [ACQUISITION]
    public static IO<Unit> Declared(Seq<DisplayModeRule> rules) =>
        from assignable in Products.Each(rules.Map(static rule => rule.Mode).Distinct().Map(static mode => DisplayModes.With(mode, static description => IO.lift(() => Assignable(description)))))
        from post in Mailboxes.Idle<(uint Document, Option<Guid> Object), Unit>(pending => Drained(rules, pending))
        from attached in Touched.Attach(key => post(key, unit))
        select unit;

    private static HostEvent<(uint Document, Option<Guid> Object)> Touched =>
        Subscriptions.Merged(
            Touches(Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.AddRhinoObject += h).Documents(static args => args.TheObject?.Document),
                static args => Optional(args.TheObject).Map(static found => Some(found.Id))),
            Touches(Subscriptions.Watched<RhinoObjectEventArgs>(static (watcher, h) => watcher.UndeleteRhinoObject += h).Documents(static args => args.TheObject?.Document),
                static args => Optional(args.TheObject).Map(static found => Some(found.Id))),
            Touches(Subscriptions.On<RhinoModifyObjectAttributesEventArgs>(static h => RhinoDoc.ModifyObjectAttributes += h, static h => RhinoDoc.ModifyObjectAttributes -= h)
                    .Documents(static args => args.DocumentSerialNumber),
                static args => Optional(args.RhinoObject).Map(static found => Some(found.Id))),
            Touches(Subscriptions.On<LayerTableEventArgs>(static h => RhinoDoc.LayerTableEvent += h, static h => RhinoDoc.LayerTableEvent -= h).Documents(static args => args.Document),
                static _ => Some(Option<Guid>.None)),
            Documents.Arrived.Choose(static serial => Some((serial, Option<Guid>.None))));

    private static HostEvent<(uint Document, Option<Guid> Object)> Touches<TArgs>(DocumentEvent<TArgs> row, Func<TArgs, Option<Option<Guid>>> touched) =>
        row.Choose(args => from serial in row.Serial(args) from found in touched(args) select (serial, found));

    private static IO<Unit> Drained(Seq<DisplayModeRule> rules, HashMap<(uint Document, Option<Guid> Object), Unit> pending) =>
        Products.Each(toSeq(pending.Keys).Map(static key => key.Document).Distinct()
            .Choose(static serial => Optional(RhinoDoc.FromRuntimeSerialNumber(serial)))
            .Map(doc => IO.lift(() => Candidates(doc, toSeq(pending.Keys).Filter(key => key.Document == doc.RuntimeSerialNumber).Map(static key => key.Object)))
                .Bind(objects => Reconcile(doc, rules, objects, new RedrawPolicy.AllViews(Deferred: true)))))
            .Map(unit);

    private static Seq<RhinoObject> Candidates(RhinoDoc doc, Seq<Option<Guid>> touched) =>
        (touched.Exists(static held => held.IsNone)
            ? Conversions.Rows(doc.Objects.GetObjectList(new ObjectEnumeratorSettings { HiddenObjects = true }))
            : touched.Somes().Choose(id => Optional(doc.Objects.FindId(id))))
        .Filter(static found => !found.IsDeleted && !found.IsReference && !found.IsInstanceDefinitionGeometry)
        .Strict();

    // --- [OVERRIDES]
    internal static IO<Unit> DisplayModeOverride(ObjectAttributes staged, Guid slot, Option<Guid> mode) =>
        mode.Match(
            Some: id => DisplayModes.With(id, description =>
                from allowed in IO.lift(() => Assignable(description))
                from written in IO.lift(() => { _ = staged.SetDisplayModeOverride(description, slot); })
                select written),
            None: () => slot == Guid.Empty
                ? IO.lift(() => DisplayMaterialRefs.AllViewportsRemoved(staged, Guid.NewGuid()))
                : IO.lift(() => staged.RemoveDisplayModeOverride(slot)));

    internal static IO<Guid> Slot(RhinoDoc doc, Option<ViewportTarget> viewport) =>
        viewport.Match(Some: target => Viewports.With(doc, target, static row => IO.pure(row.Viewport.Id)), None: static () => IO.pure(Guid.Empty));

    internal static Fin<Unit> Assignable(DisplayModeDescription mode) => RhinoFault.Unless(mode.AllowObjectAssignment, RhinoReason.Unsupported, mode.EnglishName);
}
