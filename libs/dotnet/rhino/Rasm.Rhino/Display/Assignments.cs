using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.Runtime;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DisplayModeRule(Func<RhinoObject, bool> Applies, Option<ViewportTarget> Viewport, Guid Mode);

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct DisplayMaterialRef(Guid ViewportId, Guid DisplayMaterialId);

// --- [SERVICES] ------------------------------------------------------------------------
internal static class DisplayMaterialRefs {
    // --- [EXPORTS]
    private static readonly nint OpenNurbs = OperatingSystem.IsWindows() ? NativeLibrary.Load("opennurbs") : NativeLibrary.GetMainProgramHandle();

    private static readonly unsafe delegate* unmanaged<nint, DisplayMaterialRef*, DisplayMaterialRef*, byte> FindDisplayMaterialRefExport =
        (delegate* unmanaged<nint, DisplayMaterialRef*, DisplayMaterialRef*, byte>)Export(
            "?FindDisplayMaterialRef@ON_3dmObjectAttributes@@QEBA_NAEBVON_DisplayMaterialRef@@PEAV2@@Z",
            "_ZNK22ON_3dmObjectAttributes22FindDisplayMaterialRefERK21ON_DisplayMaterialRefPS0_");

    private static readonly unsafe delegate* unmanaged<nint, DisplayMaterialRef, byte> AddDisplayMaterialRefExport =
        (delegate* unmanaged<nint, DisplayMaterialRef, byte>)Export(
            "?AddDisplayMaterialRef@ON_3dmObjectAttributes@@QEAA_NVON_DisplayMaterialRef@@@Z",
            "_ZN22ON_3dmObjectAttributes21AddDisplayMaterialRefE21ON_DisplayMaterialRef");

    private static readonly unsafe delegate* unmanaged<nint, Guid, Guid, byte> RemoveDisplayMaterialRefExport =
        (delegate* unmanaged<nint, Guid, Guid, byte>)Export(
            "?RemoveDisplayMaterialRef@ON_3dmObjectAttributes@@QEAA_NUON_UUID_struct@@0@Z",
            "_ZN22ON_3dmObjectAttributes24RemoveDisplayMaterialRefE14ON_UUID_structS0_");

    private static nint Export(string microsoft, string itanium) => NativeLibrary.GetExport(OpenNurbs, OperatingSystem.IsWindows() ? microsoft : itanium);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ConstPointer")]
    private static extern nint ConstPointer(CommonObject target);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "NonConstPointer")]
    private static extern nint NonConstPointer(ObjectAttributes target);

    // --- [CALLS]
    private static unsafe Option<DisplayMaterialRef> FindDisplayMaterialRef(ObjectAttributes attributes, DisplayMaterialRef search) {
        DisplayMaterialRef found = default;
        bool held = FindDisplayMaterialRefExport(ConstPointer(attributes), &search, &found) is not 0;
        GC.KeepAlive(attributes);
        return Conversions.Found(held, found);
    }

    private static unsafe bool AddDisplayMaterialRef(ObjectAttributes attributes, DisplayMaterialRef displayMaterial) {
        bool added = AddDisplayMaterialRefExport(NonConstPointer(attributes), displayMaterial) is not 0;
        GC.KeepAlive(attributes);
        return added;
    }

    private static unsafe bool RemoveDisplayMaterialRef(ObjectAttributes attributes, Guid viewportId, Guid displayMaterialId) {
        bool removed = RemoveDisplayMaterialRefExport(NonConstPointer(attributes), viewportId, displayMaterialId) is not 0;
        GC.KeepAlive(attributes);
        return removed;
    }

    // --- [SLOTS]
    internal static Option<Guid> DisplayMaterialRef(ObjectAttributes attributes, Guid viewport) =>
        FindDisplayMaterialRef(attributes, new DisplayMaterialRef(viewport, Guid.Empty))
            .Filter(found => found.ViewportId == viewport)
            .Map(static found => found.DisplayMaterialId);

    internal static void AllViewportsRemoved(ObjectAttributes staged, Guid key) {
        _ = AddDisplayMaterialRef(staged, new DisplayMaterialRef(Guid.Empty, key));
        _ = RemoveDisplayMaterialRef(staged, Guid.Empty, key);
    }
}

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
            slot => DisplayMaterialRefs.DisplayMaterialRef(found.Attributes, slot)))
        select Conversions.Found(!ruled.Writes.IsEmpty || ruled.Mark != marked, (found.Id, ruled.Writes, ruled.Mark));

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
        from marked in mark.IsEmpty ? ArchivableDictionaries.Remove(owner, Mark) : ArchivableDictionaries.Write(owner, Mark, mark.AsIterable().ToImmutableDictionary(static pair => pair.Key, static pair => pair.Value))
        select unit;

    // --- [ACQUISITION]
    public static Func<IPlugInViews, IO<Unit>> Declared(Seq<DisplayModeRule> rules) =>
        views =>
            from assignable in IO.lift(() => views.Answer(
                Callbacks.Each(rules.Map(static rule => rule.Mode).Distinct().Map(static mode => DisplayModes.With(mode, static description => IO.lift(() => AttributeEdits.Assignable(description)))))
                    .Map(static _ => true),
                failed: false))
            from followed in when(assignable, Mailboxes.Idle<(uint Document, Option<Guid> Object), Unit>(pending => Drained(rules, pending))(views)
                .Bind(post => Touched.Inline(key => post(key, unit), views))).As()
            select unit;

    private static readonly HostEvent<(uint Document, Option<Guid> Object)> Touched = Subscriptions.Merged(
        EventKind.AddRhinoObject.Choose(static args => Object(EventKind.AddRhinoObject.Serial(args), args.TheObject)),
        EventKind.UndeleteRhinoObject.Choose(static args => Object(EventKind.UndeleteRhinoObject.Serial(args), args.TheObject)),
        EventKind.ModifyObjectAttributes.Choose(static args => Object(EventKind.ModifyObjectAttributes.Serial(args), args.RhinoObject)),
        EventKind.LayerTableEvent.Choose(static args => EventKind.LayerTableEvent.Serial(args).Map(static serial => (serial, Option<Guid>.None))),
        EventKind.DocumentArrived.Choose(static serial => Some((serial, Option<Guid>.None))));

    private static Option<(uint Document, Option<Guid> Object)> Object(Option<uint> document, RhinoObject? found) =>
        from serial in document
        from held in Optional(found)
        select (serial, Some(held.Id));

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
