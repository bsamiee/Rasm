using Rasm.Imaging.Files;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;
using NativeConstructionPlane = Rhino.DocObjects.ConstructionPlane;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>(SkipIParsable = true)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class StateName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value.Length > 0 ? null : new ValidationError("state name is empty");
}

[SmartEnum]
public sealed partial class NamedRecording {
    public static readonly NamedRecording Batch = new();
    public static readonly NamedRecording Command = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NamedWrite {
    public sealed record Recorded : NamedWrite;

    public sealed record Compensated(IO<Unit> Revert, IO<Unit> Release) : NamedWrite;

    private (IO<Unit> Revert, IO<Unit> Release) Owed =>
        Switch(
            recorded: static _ => (IO.pure(unit), IO.pure(unit)),
            compensated: static compensated => (compensated.Revert, compensated.Release));

    internal static IO<Unit> Reverted(Seq<NamedWrite> writes) =>
        Callbacks.Each(writes.Rev().Map(static write => write.Owed.Revert)).Map(static _ => unit);

    internal static IO<Unit> Released(Seq<NamedWrite> writes) =>
        Callbacks.Each(writes.Map(static write => write.Owed.Release)).Map(static _ => unit);

    internal static IO<Unit> Settled(Seq<NamedWrite> writes) =>
        Callbacks.Each(Seq(Reverted(writes), Released(writes))).Map(static _ => unit);

    internal static NamedWrite Joined(Seq<NamedWrite> writes) =>
        writes.ForAll(static write => write is Recorded) ? new Recorded() : new Compensated(Reverted(writes), Released(writes));
}

public sealed record NamedEntry(NamedTable Table, string Name, Option<Guid> Id) {
    public static ReadModel<Seq<NamedEntry>> Catalog { get; } = new(
        [.. toSeq(NamedTable.Items).Bind(static table => table.Marks), ReadModel.Marks(EventKind.UndoRedo), ReadModel.Marks(EventKind.EndOpenDocument)],
        Read);

    public static IO<Seq<NamedEntry>> Read(RhinoDoc doc) =>
        toSeq(NamedTable.Items).TraverseM(table => table.Entries(doc)).As().Map(static tables => tables.Flatten());
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NamedTable {
    public static readonly NamedTable LayerStates = new(
        nameof(LayerStates), StringComparer.Ordinal, NamedRecording.Batch,
        marks: [ReadModel.Marks(EventKind.DocumentPropertiesChanged)],
        transform: None,
        import: Some(static (RhinoDoc doc, ArchiveSource source, Option<Seq<StateName>> names, CallbackSite _) => Tables.LayerStates.Import(doc, source, names)),
        archive: None, members: None, current: None,
        entries: Tables.LayerStates.Entries,
        accepts: static _ => unit,
        rename: Tables.LayerStates.Rename,
        delete: static (doc, entry, _) => Tables.LayerStates.Delete(doc, entry),
        hold: Tables.LayerStates.Hold);

    public static readonly NamedTable Views = new(
        nameof(Views), StringComparer.OrdinalIgnoreCase, NamedRecording.Batch,
        marks: [ReadModel.Marks(EventKind.DocumentPropertiesChanged)],
        transform: Some(NamedViews.Transform), import: Some(NamedViews.Import), archive: Some(NamedViews.Archive),
        members: None, current: Some(NamedViews.Current),
        entries: NamedViews.Entries,
        accepts: static _ => unit,
        rename: NamedViews.Rename,
        delete: NamedViews.Delete,
        hold: NamedViews.Hold);

    public static readonly NamedTable ConstructionPlanes = new(
        nameof(ConstructionPlanes), StringComparer.OrdinalIgnoreCase, NamedRecording.Batch,
        marks: [ReadModel.Marks(EventKind.DocumentPropertiesChanged)],
        transform: Some(Viewport.ConstructionPlanes.Transform), import: Some(Viewport.ConstructionPlanes.Import), archive: Some(Viewport.ConstructionPlanes.Archive),
        members: None, current: Some(Viewport.ConstructionPlanes.Current),
        entries: Viewport.ConstructionPlanes.Entries,
        accepts: static _ => unit,
        rename: Viewport.ConstructionPlanes.Rename,
        delete: Viewport.ConstructionPlanes.Delete,
        hold: Viewport.ConstructionPlanes.Hold);

    public static readonly NamedTable Positions = new(
        nameof(Positions), StringComparer.Ordinal, NamedRecording.Batch,
        marks: [ReadModel.Marks(EventKind.DocumentPropertiesChanged)],
        transform: None, import: None, archive: None, members: Some(NamedPositions.Members), current: None,
        entries: NamedPositions.Entries,
        accepts: static _ => unit,
        rename: static (doc, entry, name, _) => NamedPositions.Rename(doc, entry, name),
        delete: static (doc, entry, _) => NamedPositions.Delete(doc, entry),
        hold: static (doc, _) => NamedPositions.Hold(doc));

    public static readonly NamedTable Snapshots = new(
        nameof(Snapshots), StringComparer.OrdinalIgnoreCase, NamedRecording.Command,
        marks: [ReadModel.Marks(EventKind.EndCommand)],
        transform: None,
        import: Some(static (RhinoDoc doc, ArchiveSource source, Option<Seq<StateName>> names, CallbackSite _) => SnapshotChange.Imported(doc, source, names)),
        archive: None, members: None, current: None,
        entries: SnapshotChange.Entries,
        accepts: static name => SnapshotName.Validated(name).Map(static _ => unit),
        rename: static (doc, entry, name, _) => SnapshotChange.Renamed(doc, entry, name),
        delete: static (doc, entry, _) => SnapshotChange.Deleted(doc, entry),
        hold: static (doc, _) => SnapshotChange.Held(doc));

    public static readonly NamedTable Selections = new(
        nameof(Selections), StringComparer.OrdinalIgnoreCase, NamedRecording.Batch,
        marks: [NamedSelections.Refreshed],
        transform: None, import: None, archive: None, members: Some(NamedSelections.Members), current: None,
        entries: NamedSelections.Entries,
        accepts: NamedSelections.Accepts,
        rename: static (doc, entry, name, _) => NamedSelections.Rename(doc, entry, name),
        delete: static (doc, entry, _) => NamedSelections.Delete(doc, entry),
        hold: static (doc, _) => NamedSelections.Hold(doc));

    public static readonly NamedTable Markups = new(
        nameof(Markups), TableOps.Names<Markup>(), NamedRecording.Batch,
        marks: [ReadModel.Marks(EventKind.MarkupTableEvent)],
        transform: None, import: None, archive: None, members: Some(Tables.Markups.Members), current: Some(Tables.Markups.Current),
        entries: Tables.Markups.Entries,
        accepts: static _ => unit,
        rename: static (doc, entry, name, _) => Tables.Markups.Rename(doc, entry, name),
        delete: static (doc, entry, _) => Tables.Markups.Delete(doc, entry),
        hold: static (doc, _) => Tables.Markups.Hold(doc));

    public IEqualityComparer<string> Comparer { get; }
    public NamedRecording Recording { get; }
    public Seq<HostEvent<Option<uint>>> Marks { get; }
    public Option<Func<RhinoDoc, NamedEntry, Transform, CallbackSite, IO<NamedWrite>>> Transform { get; }
    public Option<Func<RhinoDoc, ArchiveSource, Option<Seq<StateName>>, CallbackSite, IO<NamedWrite>>> Import { get; }
    public Option<Func<RhinoDoc, Seq<NamedEntry>, File3dm, IO<Unit>>> Archive { get; }
    public Option<Func<RhinoDoc, NamedEntry, IO<Seq<Guid>>>> Members { get; }
    public Option<Func<RhinoDoc, ViewportTarget, IO<Option<NamedEntry>>>> Current { get; }

    // --- [ROWS]
    [UseDelegateFromConstructor]
    public partial IO<Seq<NamedEntry>> Entries(RhinoDoc doc);

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Accepts(StateName name);

    [UseDelegateFromConstructor]
    public partial IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name, CallbackSite site);

    [UseDelegateFromConstructor]
    public partial IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry, CallbackSite site);

    [UseDelegateFromConstructor]
    public partial IO<NamedWrite> Hold(RhinoDoc doc, Option<ViewportSet> scope);

    public Option<NamedEntry> Find(Seq<NamedEntry> entries, string name) =>
        entries.Find(entry => entry.Table == this && Comparer.Equals(entry.Name, name));

    internal Error Absent(string verb) =>
        Error.New($"{this} rows have no {verb}");

    // --- [HOLDS]
    public static IO<T> Within<T>(RhinoDoc doc, Seq<NamedTable> tables, Option<ViewportSet> scope, IO<T> body) =>
        DisposalOps.Within(
            tables.Fold(IO.pure(Seq<NamedWrite>()), (held, table) =>
                from writes in held
                from write in DisposalOps.OnFailure(table.Hold(doc, scope), NamedWrite.Settled(writes))
                select writes.Add(write)),
            _ => body,
            NamedWrite.Settled);

    // --- [EXCHANGE]
    public static IO<StepOutcome> Export(RhinoDoc doc, Seq<NamedEntry> entries, OutputPath path) =>
        from rows in IO.lift(() => toSeq(entries.GroupBy(static entry => entry.Table))
            .Traverse(static held => held.Key.Archive.ToFin(held.Key.Absent(nameof(Archive))).Map(archive => (Archive: archive, Entries: toSeq(held))))
            .As())
        from outcome in DisposalOps.Within(static () => new File3dm(), archive =>
            from units in IO.lift(() => { archive.Settings.ModelUnits = doc.ModelUnits; })
            from archived in rows.TraverseM(row => row.Archive(doc, row.Entries, archive)).As()
            from persisted in Archives.Persist(archive, path, new File3dmWriteOptions())
            select persisted)
        select outcome;

    internal IO<NamedWrite> Imported<T>(RhinoDoc doc, ArchiveSource source, Option<Seq<StateName>> names, Func<File3dm, IEnumerable<T>> rows, Func<T, string> name, Func<T, Transform, IO<NamedWrite>> write) =>
        Archives.Read(source, read =>
            from stored in IO.lift(() => Conversions.Rows(rows(read.Model)))
            from chosen in IO.lift(() => names.Match(
                Some: wanted => wanted.Traverse(held => stored.Find(row => Comparer.Equals(name(row), held)).ToFin(new NotFound($"{this}", held))).As(),
                None: () => stored))
            from scaling in Archives.Scaling(read.Model, doc)
            from written in chosen.TraverseM(row => write(row, scaling)).As()
            select NamedWrite.Joined(written));

    internal static Fin<double> Dilation(Transform xform) =>
        Refused.UnlessValid(
            xform.DecomposeSimilarity(out _, out double dilation, out _, RhinoMath.SqrtEpsilon) is not TransformSimilarityType.NotSimilarity,
            Math.Abs(dilation),
            nameof(xform.DecomposeSimilarity));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NamedState {
    public abstract NamedTable Table { get; }

    internal abstract string Key { get; }

    public sealed record LayerState(StateName Name, Option<ViewportTarget> Scope, Seq<LayerOp.Modify> Edits) : NamedState {
        public override NamedTable Table => NamedTable.LayerStates;

        internal override string Key => Name;
    }

    public sealed record View(StateName Name, ViewportTarget Source, Option<Func<RhinoViewport, ViewInfo, IO<Unit>>> Stage) : NamedState {
        public override NamedTable Table => NamedTable.Views;

        internal override string Key => Name;
    }

    public sealed record ConstructionPlane(StateName Name, Option<ViewportTarget> Source, Option<Func<NativeConstructionPlane, IO<Unit>>> Stage) : NamedState {
        public override NamedTable Table => NamedTable.ConstructionPlanes;

        internal override string Key => Name;
    }

    public sealed record Position(StateName Name, ObjectTarget Objects) : NamedState {
        public override NamedTable Table => NamedTable.Positions;

        internal override string Key => Name;
    }

    public sealed record Snapshot(SnapshotName Name) : NamedState {
        public override NamedTable Table => NamedTable.Snapshots;

        internal override string Key => Name;
    }

    public sealed record Selection(StateName Name, ObjectTarget Objects) : NamedState {
        public override NamedTable Table => NamedTable.Selections;

        internal override string Key => Name;
    }

    public sealed record Markup(StateName Name, ViewportTarget Source, Seq<GeometryPair> Geometry) : NamedState {
        public override NamedTable Table => NamedTable.Markups;

        internal override string Key => Name;
    }

    internal IO<NamedWrite> Run(RhinoDoc doc, CallbackSite site) =>
        Switch(
            (Document: doc, Site: site),
            layerState: static (at, state) => LayerStates.Save(at.Document, state),
            view: static (at, state) => NamedViews.Save(at.Document, state, at.Site),
            constructionPlane: static (at, state) => ConstructionPlanes.Save(at.Document, state, at.Site),
            position: static (at, state) => NamedPositions.Save(at.Document, state),
            snapshot: static (at, state) => SnapshotChange.Saved(at.Document, state.Name),
            selection: static (at, state) => NamedSelections.Save(at.Document, state),
            markup: static (at, state) => Markups.Save(at.Document, state));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NamedRestore {
    public abstract NamedTable Table { get; }

    internal abstract string Key { get; }

    public sealed record LayerState(StateName Name, Option<LayerStateRestore> Mask, Option<ViewportSet> Scope) : NamedRestore {
        public override NamedTable Table => NamedTable.LayerStates;

        internal override string Key => Name;
    }

    public sealed record View(StateName Name, ViewportSet Viewports, RestorePacing Pacing) : NamedRestore {
        public override NamedTable Table => NamedTable.Views;

        internal override string Key => Name;
    }

    public sealed record ConstructionPlane(StateName Name, ViewportSet Viewports) : NamedRestore {
        public override NamedTable Table => NamedTable.ConstructionPlanes;

        internal override string Key => Name;
    }

    public sealed record Position(StateName Name) : NamedRestore {
        public override NamedTable Table => NamedTable.Positions;

        internal override string Key => Name;
    }

    public sealed record Snapshot(SnapshotName Name) : NamedRestore {
        public override NamedTable Table => NamedTable.Snapshots;

        internal override string Key => Name;
    }

    public sealed record Selection(StateName Name, SelectionEdit Edit) : NamedRestore {
        public override NamedTable Table => NamedTable.Selections;

        internal override string Key => Name;
    }

    public sealed record Markup(StateName Name) : NamedRestore {
        public override NamedTable Table => NamedTable.Markups;

        internal override string Key => Name;
    }

    internal IO<NamedWrite> Run(RhinoDoc doc) =>
        Switch(
            doc,
            layerState: static (document, target) => LayerStates.Restore(document, target),
            view: static (document, target) => NamedViews.Restore(document, target),
            constructionPlane: static (document, target) => ConstructionPlanes.Restore(document, target),
            position: static (document, target) => NamedPositions.Restore(document, target),
            snapshot: static (document, target) => SnapshotChange.Restored(document, target.Name),
            selection: static (document, target) => NamedSelections.Restore(document, target),
            markup: static (document, target) => Markups.Restore(document, target));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NamedChange {
    public abstract NamedTable Table { get; }

    public abstract string Label { get; }

    public sealed record Save(NamedState State) : NamedChange {
        public override NamedTable Table => State.Table;

        public override string Label => $"{nameof(Save)} {State.Key}";
    }

    public sealed record Restore(NamedRestore Target) : NamedChange {
        public override NamedTable Table => Target.Table;

        public override string Label => $"{nameof(Restore)} {Target.Key}";
    }

    public sealed record Rename(NamedEntry Entry, StateName Name) : NamedChange {
        public override NamedTable Table => Entry.Table;

        public override string Label => $"{nameof(Rename)} {Entry.Name}";
    }

    public sealed record Delete(NamedEntry Entry) : NamedChange {
        public override NamedTable Table => Entry.Table;

        public override string Label => $"{nameof(Delete)} {Entry.Name}";
    }

    public sealed record Transform(NamedEntry Entry, global::Rhino.Geometry.Transform Xform) : NamedChange {
        public override NamedTable Table => Entry.Table;

        public override string Label => $"{nameof(Transform)} {Entry.Name}";
    }

    public sealed record Import(NamedTable Table, ArchiveSource Source, Option<Seq<StateName>> Names) : NamedChange {
        public override NamedTable Table { get; } = Table;

        public override string Label => $"{nameof(Import)} {Table}";
    }

    // --- [PLANNING]
    public static Fin<Seq<NamedChange>> Plan(Seq<NamedEntry> held, Seq<NamedState> desired, LanguageExt.HashSet<NamedTable> prune) =>
        toSeq(
                from table in NamedTable.Items
                join entry in held on table equals entry.Table into entries
                join state in desired on table equals state.Table into states
                select (Table: table, Entries: toSeq(entries), States: toSeq(states)))
            .Traverse(row => TableOps.Plan(
                row.Entries,
                new TableSpec<NamedState>(row.States.Map(static state => (Name: state.Key, Spec: state)), prune.Contains(row.Table)),
                static entry => entry.Name,
                row.Table.Comparer))
            .As()
            .Map(static plans => plans.Bind(static plan => plan.Prunes).Map<NamedChange>(static entry => new Delete(entry))
                + plans.Bind(static plan => plan.Upserts).Map<NamedChange>(static upsert => new Save(upsert.Spec)));

    // --- [APPLY]
    public static IO<Seq<Committed<Seq<NamedEvent>>>> Apply(RhinoDoc doc, Seq<NamedChange> changes, CallbackSite site) =>
        from held in NamedEntry.Read(doc)
        from valid in IO.lift(() => changes.Fold(Fin.Succ(held), static (entries, change) => entries.Bind(change.Landed)))
        from landed in IO.lift(static () => (Writes: Atom(Seq<NamedWrite>()), Records: Atom(Seq<Option<uint>>())))
        from committed in DisposalOps.OnFailure(
                Segments(changes).TraverseM(segment => Segment(doc, segment, site, landed.Writes, landed.Records)).As(),
                Callbacks.Each(Seq(
                    IO.lift(() => landed.Writes.Value).Bind(NamedWrite.Reverted),
                    IO.lift(() => landed.Records.Value.Rev().Somes()).Bind(records => records.TraverseM(_ => Commits.Step(doc, doc.Undo, nameof(RhinoDoc.Undo))).As()).Map(static _ => unit)))
                    .Map(static _ => unit))
            .Finally(IO.lift(() => landed.Writes.Value).Bind(NamedWrite.Released))
        select committed;

    private static Seq<Seq<NamedChange>> Segments(Seq<NamedChange> changes) =>
        changes.Fold(Seq<Seq<NamedChange>>(), static (segments, change) =>
            segments.Last.Filter(open => Batched(open) && Batched([change])).Match(
                Some: open => segments.Init.Add(open.Add(change)),
                None: () => segments.Add([change])));

    private static bool Batched(Seq<NamedChange> segment) =>
        segment.ForAll(static change => change.Table.Recording == NamedRecording.Batch);

    private static IO<Committed<Seq<NamedEvent>>> Segment(RhinoDoc doc, Seq<NamedChange> segment, CallbackSite site, Atom<Seq<NamedWrite>> writes, Atom<Seq<Option<uint>>> records) =>
        from before in NamedEntry.Read(doc)
        from record in Batched(segment)
            ? Commits.Commit(
                    doc,
                    string.Join(", ", segment.Map(static change => change.Label)),
                    segment.Exists(static change => change is Restore or Transform) ? new RedrawPolicy.AllViews(Deferred: true) : new RedrawPolicy.Silent(),
                    segment.TraverseM(change => Ran(doc, change, site, writes)).As())
                .Map(static committed => committed.Record)
            : Commanded(doc, segment, site, writes)
        from after in NamedEntry.Read(doc)
        from noted in IO.lift(() => records.Swap(held => held.Add(record)))
        select new Committed<Seq<NamedEvent>>(NamedEvent.Between(before, after), record);

    private static IO<Option<uint>> Commanded(RhinoDoc doc, Seq<NamedChange> segment, CallbackSite site, Atom<Seq<NamedWrite>> writes) =>
        from prior in IO.lift(() => toHashSet(doc.GetUndoRecords().Select(static entry => entry.SerialNumber)))
        from ran in segment.TraverseM(change => Ran(doc, change, site, writes)).As()
        from record in IO.lift(() => toSeq(doc.GetUndoRecords()).Map(static entry => entry.SerialNumber).Filter(serial => !prior.Contains(serial))
            .Fold(Option<uint>.None, static (newest, serial) => Some(newest.Map(held => Math.Max(held, serial)).IfNone(serial))))
        select record;

    private static IO<Unit> Ran(RhinoDoc doc, NamedChange change, CallbackSite site, Atom<Seq<NamedWrite>> writes) =>
        change.Run(doc, site).Bind(write => IO.lift(() => writes.Swap(held => held.Add(write)))).Map(static _ => unit);

    private IO<NamedWrite> Run(RhinoDoc doc, CallbackSite site) =>
        Switch(
            (Document: doc, Site: site),
            save: static (at, save) => save.State.Run(at.Document, at.Site),
            restore: static (at, restore) => restore.Target.Run(at.Document),
            rename: static (at, rename) => rename.Entry.Table.Rename(at.Document, rename.Entry, rename.Name, at.Site),
            delete: static (at, delete) => delete.Entry.Table.Delete(at.Document, delete.Entry, at.Site),
            transform: static (at, transform) => transform.Entry.Table.Transform.Match(
                Some: run => run(at.Document, transform.Entry, transform.Xform, at.Site),
                None: () => IO.fail<NamedWrite>(transform.Entry.Table.Absent(nameof(Transform)))),
            import: static (at, import) => import.Table.Import.Match(
                Some: run => run(at.Document, import.Source, import.Names, at.Site),
                None: () => IO.fail<NamedWrite>(import.Table.Absent(nameof(Import)))));

    private Fin<Seq<NamedEntry>> Landed(Seq<NamedEntry> entries) =>
        Switch(
            entries,
            save: static (held, save) =>
                from name in StateName.Validated(save.State.Key)
                from accepted in save.State.Table.Accepts(name)
                select save.State.Table.Find(held, name).IsSome ? held : held.Add(new NamedEntry(save.State.Table, name, None)),
            restore: static (held, restore) => Present(held, restore.Target.Table, restore.Target.Key).Map(_ => held),
            rename: static (held, rename) =>
                from found in Present(held, rename.Entry.Table, rename.Entry.Name)
                from accepted in rename.Entry.Table.Accepts(rename.Name)
                from free in guard(rename.Entry.Table.Find(held, rename.Name).ForAll(taken => taken == found), Error.New($"{rename.Entry.Table} already holds {rename.Name}"))
                select held.Map(entry => entry == found ? entry with { Name = rename.Name } : entry),
            delete: static (held, delete) => Present(held, delete.Entry.Table, delete.Entry.Name).Map(found => held.Filter(entry => entry != found)),
            transform: static (held, transform) =>
                from found in Present(held, transform.Entry.Table, transform.Entry.Name)
                from verb in transform.Entry.Table.Transform.ToFin(transform.Entry.Table.Absent(nameof(Transform)))
                select held,
            import: static (held, import) =>
                from verb in import.Table.Import.ToFin(import.Table.Absent(nameof(Import)))
                select import.Names.Match(
                    Some: names => names.Fold(held, (entries, name) => import.Table.Find(entries, name).IsSome ? entries : entries.Add(new NamedEntry(import.Table, name, None))),
                    None: () => held));

    private static Fin<NamedEntry> Present(Seq<NamedEntry> entries, NamedTable table, string name) =>
        table.Find(entries, name).ToFin(new NotFound($"{table}", name));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NamedEvent {
    public abstract NamedTable Table { get; }

    public sealed record Added(NamedEntry Entry) : NamedEvent {
        public override NamedTable Table => Entry.Table;
    }

    public sealed record Removed(NamedEntry Entry) : NamedEvent {
        public override NamedTable Table => Entry.Table;
    }

    public sealed record Renamed(NamedEntry Before, NamedEntry After) : NamedEvent {
        public override NamedTable Table => After.Table;
    }

    public static Seq<NamedEvent> Between(Seq<NamedEntry> before, Seq<NamedEntry> after) =>
        before.Bind(prior => Paired(after, prior).Match(
                Some: next => string.Equals(next.Name, prior.Name, StringComparison.Ordinal) ? Seq<NamedEvent>() : Seq<NamedEvent>(new Renamed(prior, next)),
                None: () => Seq<NamedEvent>(new Removed(prior))))
            + after.Filter(next => Paired(before, next).IsNone).Map<NamedEvent>(static next => new Added(next));

    public static HostEvent<Seq<NamedEvent>> Changed(RhinoDoc doc) =>
        NamedEntry.Catalog.Changed(doc).Choose(static pair => Some(Between(pair.Before, pair.After)).Filter(static events => !events.IsEmpty));

    private static Option<NamedEntry> Paired(Seq<NamedEntry> entries, NamedEntry entry) =>
        entry.Id.Match(
            Some: id => entries.Find(row => row.Table == entry.Table && row.Id == Some(id)),
            None: () => entry.Table.Find(entries, entry.Name));
}
