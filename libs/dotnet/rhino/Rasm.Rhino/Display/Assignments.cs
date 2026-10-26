using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DisplayModeRule(Func<RhinoObject, bool> Applies, Option<ViewportTarget> Viewport, Guid Mode);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisplayModeRules {
    // --- [MARK]
    private static readonly DictionaryCodec<HashMap<Guid, Guid>> Mark = DictionaryCodec.Json<HashMap<Guid, Guid>>(nameof(DisplayModeRules), 1);

    private static IO<HashMap<Guid, Guid>> Marked(ObjectAttributes attributes) =>
        IO.lift(() => attributes.HasUserData).Bind(held => held
            ? DisposalOps.Within(attributes.Duplicate, static copy => ArchivableDictionaries.Read(DictionaryOwner.Of(IO.pure(copy)), Mark)).Map(static mark => mark.IfNone([]))
            : IO.pure(HashMap<Guid, Guid>()));

    // --- [RECONCILE]
    public static IO<Seq<Guid>> Reconcile(RhinoDoc doc, Seq<DisplayModeRule> rules, Seq<RhinoObject> objects, RedrawPolicy redraw) =>
        Commits.WithinRedraw(doc, redraw,
            from slots in Callbacks.Each(rules.Map(rule => AttributeEdits.Slot(doc, rule.Viewport).Map(slot => (Slot: slot, Rule: rule))))
            from planned in Callbacks.Each(objects.Map(found => Planned(found, slots)))
            from written in Callbacks.Each(planned.Somes().Map(row => TableOps.Apply(doc, new TableOp.ModifyAttributes(
                new ObjectTarget.Ids(Seq(row.Id)),
                staged => Written(staged, row.Writes, row.Mark),
                Quiet: true))))
            select written.Flatten());

    private static IO<Option<(Guid Id, Seq<(Guid Slot, Option<Guid> Mode)> Writes, HashMap<Guid, Guid> Mark)>> Planned(RhinoObject found, Seq<(Guid Slot, DisplayModeRule Rule)> slots) =>
        from marked in Marked(found.Attributes)
        from ruled in IO.lift(() => Ruled(
            slots.Fold(HashMap<Guid, Guid>(), (held, row) => held.ContainsKey(row.Slot) || !row.Rule.Applies(found) ? held : held.Add(row.Slot, row.Rule.Mode)),
            marked,
            slot => AttributeOps.DisplayMaterialRef(found.Attributes, slot)))
        select Callbacks.Found(!ruled.Writes.IsEmpty || ruled.Mark != marked, (found.Id, ruled.Writes, ruled.Mark));

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
        from overridden in Callbacks.Each(writes.Map(write => AttributeEdits.DisplayModeOverride(staged, write.Slot, write.Mode)))
        let owner = DictionaryOwner.Of(IO.pure(staged))
        from marked in mark.IsEmpty ? ArchivableDictionaries.Remove(owner, Mark) : ArchivableDictionaries.Write(owner, Mark, mark)
        select unit;

    // --- [ACQUISITION]
    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> Declared(Seq<DisplayModeRule> rules) =>
        (sink, retain) =>
            from assignable in Callbacks.Each(rules.Map(static rule => rule.Mode).Distinct().Map(static mode => DisplayModes.With(mode, static description => IO.lift(() => AttributeEdits.Assignable(description)))))
                .Map(static _ => true)
                .Catch(error => IO.lift(() => {
                    sink.Report(error, typeof(DisplayModeRules), nameof(Declared));
                    return false;
                }))
                .As()
            from followed in when(assignable, Touched.Through(Subscriptions.Idle<(uint Document, Option<Guid> Object), Unit>(pending => Drained(rules, pending)), sink, retain)).As()
            select unit;

    private static readonly HostEvent<((uint Document, Option<Guid> Object), Unit)> Touched = Subscriptions.Host(typeof(DisplayModeRules), static () => Seq(
        EventKind.AddRhinoObject.Choose(static args => Object(EventKind.AddRhinoObject.DocumentSerial(args), args.TheObject)),
        EventKind.UndeleteRhinoObject.Choose(static args => Object(EventKind.UndeleteRhinoObject.DocumentSerial(args), args.TheObject)),
        EventKind.ModifyObjectAttributes.Choose(static args => Object(EventKind.ModifyObjectAttributes.DocumentSerial(args), args.RhinoObject)),
        EventKind.LayerTableEvent.Choose(static args => EventKind.LayerTableEvent.DocumentSerial(args).Map(static serial => ((serial, Option<Guid>.None), unit))),
        EventKind.DocumentArrived.Choose(static serial => Some(((serial, Option<Guid>.None), unit)))));

    private static Option<((uint Document, Option<Guid> Object), Unit)> Object(Option<uint> document, RhinoObject? found) =>
        from serial in document
        from held in Optional(found)
        select ((serial, Some(held.Id)), unit);

    private static IO<Unit> Drained(Seq<DisplayModeRule> rules, HashMap<(uint Document, Option<Guid> Object), Unit> pending) =>
        Callbacks.Each(toSeq(pending.Keys).Map(static key => key.Document).Distinct()
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
}
