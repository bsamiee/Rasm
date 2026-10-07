using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PurgeKind {
    public static readonly PurgeKind Linetypes = new(nameof(Linetypes), static doc => doc.Linetypes.PurgeUnused());

    public static readonly PurgeKind Layers = new(nameof(Layers), static doc => doc.Layers.PurgeUnused());

    public static readonly PurgeKind Groups = new(nameof(Groups), static doc => doc.Groups.PurgeUnused());

    public static readonly PurgeKind DimStyles = new(nameof(DimStyles), static doc => doc.DimStyles.PurgeUnused());

    public static readonly PurgeKind HatchPatterns = new(nameof(HatchPatterns), static doc => doc.HatchPatterns.PurgeUnused());

    public static readonly PurgeKind InstanceDefinitions = new(nameof(InstanceDefinitions), static doc => doc.InstanceDefinitions.PurgeUnused());

    [UseDelegateFromConstructor]
    public partial int PurgeUnused(RhinoDoc doc);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransformMode {
    public sealed record Move(bool DeleteOriginal) : TransformMode;

    public sealed record History() : TransformMode;

    public Fin<Guid> Apply(ObjectTable table, Guid id, Transform xform) =>
        Switch(
            (Table: table, Id: id, Xform: xform),
            move: static (state, move) => Answers.Required(state.Table.Transform(state.Id, state.Xform, move.DeleteOriginal), nameof(ObjectTable.Transform)),
            history: static (state, _) => Answers.Required(state.Table.TransformWithHistory(state.Id, state.Xform), nameof(ObjectTable.TransformWithHistory)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ComponentRef {
    public sealed record ById : ComponentRef {
        internal ById(Guid id) => Id = id;

        public Guid Id { get; }

        public static Fin<ComponentRef> From(Guid id) => Answers.Present(id).ToFin(new Invalid(nameof(ById))).Map<ComponentRef>(static valid => new ById(valid));
    }

    public sealed record ByIndex : ComponentRef {
        internal ByIndex(int index) => Index = index;

        public int Index { get; }

        public static Fin<ComponentRef> From(int index) => Limits.AtLeast(0).Check(index, nameof(ByIndex)).Map<ComponentRef>(static valid => new ByIndex(valid));
    }

    public sealed record ByName : ComponentRef {
        private ByName(string name) => Name = name;

        public string Name { get; }

        public static Fin<ComponentRef> From(string name) => Invalid.Unless<ComponentRef>(name.Length > 0, new ByName(name), nameof(ByName));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotWeight {
    public sealed record Default() : PlotWeight;

    public sealed record NoPlot() : PlotWeight;

    public sealed record Millimeters : PlotWeight {
        internal Millimeters(double value) => Value = value;

        public double Value { get; }

        public static Fin<PlotWeight> From(double value) => Limits.Above(0.0).Check(value, nameof(Millimeters)).Map<PlotWeight>(static positive => new Millimeters(positive));
    }

    public static PlotWeight FromHost(double value) =>
        value switch {
            0.0 => new Default(),
            < 0.0 => new NoPlot(),
            _ => new Millimeters(value),
        };

    public static Option<PlotWeight> FromInheritable(double value) =>
        value < -1.0 ? Option<PlotWeight>.None : Some(FromHost(value));

    public static double ToInheritable(Option<PlotWeight> weight) =>
        weight.Match(Some: static some => some.ToHost(), None: static () => -10.0);

    public double ToHost() =>
        Switch(@default: static _ => 0.0, noPlot: static _ => -1.0, millimeters: static millimeters => millimeters.Value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeRef {
    public sealed record Continuous() : LinetypeRef;

    public sealed record Row(ComponentRef Address) : LinetypeRef;

    public static Option<LinetypeRef> FromHost(int index) =>
        index switch {
            -1 => Some<LinetypeRef>(new Continuous()),
            >= 0 => Some<LinetypeRef>(new Row(new ComponentRef.ByIndex(index))),
            _ => Option<LinetypeRef>.None,
        };

    public IO<int> Resolve(RhinoDoc doc) =>
        Switch(
            doc,
            continuous: static (_, _) => IO.pure(-1),
            row: static (document, row) => TableOps.Find(document.Linetypes, row.Address, includeDeleted: false).Map(static found => found.Index));
}

public readonly record struct SelectionOptions(bool SyncHighlight, bool Persistent, bool IgnoreGrips, bool IgnoreLayerLocks, bool IgnoreLayerVisibility) {
    public static SelectionOptions Default { get; } = new(SyncHighlight: true, Persistent: true, IgnoreGrips: true, IgnoreLayerLocks: false, IgnoreLayerVisibility: false);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SelectionEdit {
    public sealed record Set(bool Select) : SelectionEdit;

    public sealed record Replace() : SelectionEdit;

    public int Apply(ObjectTable table, IEnumerable<Guid> ids, SelectionOptions options) =>
        Switch(
            (Table: table, Ids: ids, Options: options),
            set: static (state, set) => state.Table.Select(
                state.Ids, set.Select, state.Options.SyncHighlight, state.Options.Persistent, state.Options.IgnoreGrips, state.Options.IgnoreLayerLocks, state.Options.IgnoreLayerVisibility),
            replace: static (state, _) => state.Table.SetSelectedObjects(
                state.Ids, state.Options.SyncHighlight, state.Options.Persistent, state.Options.IgnoreGrips, state.Options.IgnoreLayerLocks, state.Options.IgnoreLayerVisibility));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VisibilityState {
    public sealed record Hidden(Option<string> HideGroup) : VisibilityState;

    public sealed record Shown() : VisibilityState;

    public sealed record Locked() : VisibilityState;

    public sealed record Unlocked() : VisibilityState;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UndoOp {
    public sealed record Undo() : UndoOp;

    public sealed record Redo() : UndoOp;

    public sealed record ClearUndo(bool PurgeDeleted, Option<uint> Serial) : UndoOp;

    public sealed record ClearRedo() : UndoOp;
}

public sealed record CustomUndo(string Description, EventHandler<CustomUndoEventArgs> Handler, Option<object> Tag);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableOp {
    public sealed record Add(Seq<GeometryPair> Rows, Option<HistoryRecord> History, bool Reference) : TableOp;

    public sealed record Replace(Guid Id, GeometryBase Geometry, bool IgnoreModes) : TableOp;

    public sealed record Delete(ObjectTarget Target, bool Quiet, bool IgnoreModes) : TableOp;

    public sealed record Move(ObjectTarget Target, Transform Xform, TransformMode Mode) : TableOp;

    public sealed record ModifyAttributes(ObjectTarget Target, Func<ObjectAttributes, IO<Unit>> Edit, bool Quiet) : TableOp;

    public sealed record State(ObjectTarget Target, VisibilityState Next, bool IgnoreLayerMode) : TableOp;

    public sealed record Undelete(Seq<uint> Serials) : TableOp;

    public sealed record PointCloud(int X, int Y, int Z, Box Box, Option<ObjectAttributes> Attributes, Option<HistoryRecord> History, bool Reference) : TableOp;

    public sealed record ImportPage(string Path, Guid MainViewportId, string PageName) : TableOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImmediateOp {
    public sealed record Selection(ObjectTarget Target, SelectionEdit Edit, SelectionOptions Options) : ImmediateOp;

    public sealed record ClearSelection(bool IgnorePersistent) : ImmediateOp;

    public sealed record Flash(ObjectTarget Target, bool UseSelectionColor) : ImmediateOp;

    public sealed record Purge(ObjectTarget Target) : ImmediateOp;

    public sealed record PurgeUnused(PurgeKind Kind) : ImmediateOp;
}

public sealed record TableAccessors<T>(RhinoDocCommonTable<T> Table, Func<T, T> Copy, Func<T, int> Add, Func<T, int, bool, bool> Modify, Func<int, bool, bool> Delete) where T : ModelComponent;

public static class TableAccessors {
    public static TableAccessors<Linetype> Linetypes(RhinoDoc doc) =>
        new(doc.Linetypes, static live => new Linetype(live), doc.Linetypes.Add, doc.Linetypes.Modify, doc.Linetypes.Delete);

    public static TableAccessors<SectionStyle> SectionStyles(RhinoDoc doc) =>
        new(doc.SectionStyles, static live => new SectionStyle(live), doc.SectionStyles.Add, doc.SectionStyles.Modify, doc.SectionStyles.Delete);

    public static TableAccessors<HatchPattern> HatchPatterns(RhinoDoc doc) =>
        new(doc.HatchPatterns, static live => new HatchPattern(live), doc.HatchPatterns.Add, doc.HatchPatterns.Modify, doc.HatchPatterns.Delete);

    public static TableAccessors<DimensionStyle> DimStyles(RhinoDoc doc) =>
        new(doc.DimStyles, static live => live.Duplicate(), style => doc.DimStyles.Add(style, reference: false), doc.DimStyles.Modify, doc.DimStyles.Delete);

    public static TableAccessors<Layer> Layers(RhinoDoc doc) =>
        new(
            doc.Layers,
            static live => {
                Layer staged = new();
                staged.CopyAttributesFrom(live);
                return staged;
            },
            doc.Layers.Add,
            doc.Layers.Modify,
            doc.Layers.Delete);
}

public sealed record TableSpec<TSpec>(Seq<TSpec> Rows, bool Exclusive);

public sealed record TableUpsert<TRow, TSpec>(string Name, Option<TRow> Present, TSpec Spec) where TRow : ModelComponent;

public sealed record TablePlan<TRow, TSpec>(Seq<TableUpsert<TRow, TSpec>> Upserts, Seq<TRow> Prunes) where TRow : ModelComponent;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TableOps {
    // --- [RECORDED]
    public static IO<Seq<Guid>> Apply(RhinoDoc doc, TableOp op) =>
        op.Switch(
            doc,
            add: static (document, add) => add.Rows.TraverseM(row => Add(document, row, add.History, add.Reference)).As(),
            replace: static (document, replace) => IO.lift(() =>
                Refused.Unless(document.Objects.Replace(replace.Id, replace.Geometry, replace.IgnoreModes), Seq(replace.Id), nameof(ObjectTable.Replace))),
            delete: static (document, delete) =>
                Touched(document, delete.Target, target => Refused.Unless(document.Objects.Delete(target, delete.Quiet, delete.IgnoreModes), nameof(ObjectTable.Delete))),
            move: static (document, move) => Each(document, move.Target, target => IO.lift(() => move.Mode.Apply(document.Objects, target.Id, move.Xform))),
            modifyAttributes: static (document, modify) => Each(document, modify.Target, target => DisposalOps.Using(
                () => target.Attributes.Duplicate(),
                copy =>
                    from edited in modify.Edit(copy)
                    from accepted in IO.lift(() => Refused.Unless(document.Objects.ModifyAttributes(target.Id, copy, modify.Quiet), nameof(ObjectTable.ModifyAttributes)))
                    select target.Id)),
            state: static (document, state) => Touched(document, state.Target, target => Visibility(document.Objects, target, state.Next, state.IgnoreLayerMode)),
            undelete: static (document, undelete) =>
                Touched(document, new ObjectTarget.Serials(undelete.Serials), target => Refused.Unless(document.Objects.Undelete(target.RuntimeSerialNumber), nameof(ObjectTable.Undelete))),
            pointCloud: static (document, cloud) =>
                from valid in IO.lift(() => Invalid.Unless(cloud.Box.IsValid, nameof(cloud.Box)))
                from id in WithAttributes(document, cloud.Attributes, attributes => IO.lift(() => Answers.Required(
                    document.Objects.AddOrderedPointCloud(cloud.X, cloud.Y, cloud.Z, cloud.Box.GetCorners(), attributes, cloud.History.ValueUnsafe(), cloud.Reference),
                    nameof(ObjectTable.AddOrderedPointCloud))))
                select Seq(id),
            importPage: static (document, page) => IO.lift(() =>
                Refused.Unless(document.Views.ImportPageView(page.Path, page.MainViewportId, page.PageName), Seq<Guid>(), nameof(ViewTable.ImportPageView))));

    public static IO<Seq<Guid>> Recorded(RhinoDoc doc, string name, RedrawPolicy redraw, Seq<CustomUndo> customUndo, Seq<TableOp> ops) =>
        Commits.Commit(
            doc,
            name,
            redraw,
            from registered in customUndo.TraverseM(undo => IO.lift(() => Refused.Unless(doc.AddCustomUndoEvent(undo.Description, undo.Handler, undo.Tag.ValueUnsafe()), nameof(RhinoDoc.AddCustomUndoEvent)))).As()
            from applied in ops.TraverseM(op => Apply(doc, op)).As()
            select applied.Flatten());

    public static IO<Guid> Add(RhinoDoc doc, GeometryPair row, Option<HistoryRecord> history, bool reference) =>
        from clipped in toSeq((row.Geometry as ClippingPlaneSurface)?.ViewportIds()).TraverseM(id => DisposalOps.Using(Viewports.ResolveViewport(doc, new ViewportTarget.Id(id)), static _ => IO.pure(unit))).As()
        from id in WithAttributes(doc, row.Attributes, attributes =>
            IO.lift(() => Answers.Required(doc.Objects.Add(row.Geometry, attributes, history.ValueUnsafe(), reference), nameof(ObjectTable.Add))))
        select id;

    private static IO<Seq<Guid>> Each(RhinoDoc doc, ObjectTarget target, Func<RhinoObject, IO<Guid>> step) =>
        Queries.Evaluate(doc, target).Bind(objects => objects.TraverseM(step).As());

    private static IO<Seq<Guid>> Touched(RhinoDoc doc, ObjectTarget target, Func<RhinoObject, Fin<Unit>> answer) =>
        Each(doc, target, found => IO.lift(() => answer(found).Map(_ => found.Id)));

    private static Fin<Unit> Visibility(ObjectTable table, RhinoObject target, VisibilityState next, bool ignoreLayerMode) =>
        next.Switch(
            (Table: table, Target: target, IgnoreLayerMode: ignoreLayerMode),
            hidden: static (state, hidden) =>
                state.Target.IsHidden ? unit : Refused.Unless(state.Table.Hide(state.Target.Id, state.IgnoreLayerMode, hidden.HideGroup.ValueUnsafe()), nameof(ObjectTable.Hide)),
            shown: static (state, _) =>
                state.Target.IsHidden ? Refused.Unless(state.Table.Show(state.Target.Id, state.IgnoreLayerMode), nameof(ObjectTable.Show)) : unit,
            locked: static (state, _) =>
                state.Target.IsLocked ? unit : Refused.Unless(state.Table.Lock(state.Target.Id, state.IgnoreLayerMode), nameof(ObjectTable.Lock)),
            unlocked: static (state, _) =>
                state.Target.IsLocked ? Refused.Unless(state.Table.Unlock(state.Target.Id, state.IgnoreLayerMode), nameof(ObjectTable.Unlock)) : unit);

    // --- [IMMEDIATE]
    public static IO<int> Immediate(RhinoDoc doc, RedrawPolicy redraw, ImmediateOp op) =>
        Commits.WithinRedraw(doc, redraw, op.Switch(
            doc,
            selection: static (document, selection) =>
                from ids in Queries.Evaluate(document, selection.Target).Map(static objects => objects.Map(static target => target.Id).Strict())
                from count in IO.lift(() => selection.Edit.Apply(document.Objects, ids, selection.Options))
                select count,
            clearSelection: static (document, clear) => IO.lift(() => document.Objects.UnselectAll(clear.IgnorePersistent)),
            flash: static (document, flash) =>
                from objects in Queries.Evaluate(document, flash.Target)
                from flashed in IO.lift(() => document.Views.FlashObjects(objects, flash.UseSelectionColor))
                select objects.Count,
            purge: static (document, purge) =>
                Touched(document, purge.Target, target => Refused.Unless(document.Objects.Purge(target.RuntimeSerialNumber), nameof(ObjectTable.Purge))).Map(static ids => ids.Count),
            purgeUnused: static (document, unused) => IO.lift(() => unused.Kind.PurgeUnused(document))));

    // --- [UNDO]
    public static IO<Unit> Undo(RhinoDoc doc, RedrawPolicy redraw, UndoOp op) =>
        from closed in IO.lift<Unit>(() => doc.UndoRecordingIsActive ? new UndoRecordOpen(doc.CurrentUndoRecordSerialNumber) : unit)
        from applied in Commits.WithinRedraw(doc, redraw, op.Switch(
            doc,
            undo: static (document, _) => IO.lift(() => Refused.Unless(document.Undo(), nameof(RhinoDoc.Undo))),
            redo: static (document, _) => IO.lift(() => Refused.Unless(document.Redo(), nameof(RhinoDoc.Redo))),
            clearUndo: static (document, clear) => IO.lift(() => clear.Serial.Match(
                Some: serial => document.ClearUndoRecords(serial, clear.PurgeDeleted),
                None: () => document.ClearUndoRecords(clear.PurgeDeleted))),
            clearRedo: static (document, _) => IO.lift(document.ClearRedoRecords)))
        select applied;

    // --- [ROWS]
    public static IO<T> Find<T>(RhinoDocCommonTable<T> table, ComponentRef address, bool includeDeleted) where T : ModelComponent =>
        IO.lift(() => address.Switch(
                table,
                byId: static (rows, byId) => Fin.Succ(Optional(rows.Document.Manifest.FindId<T>(byId.Id))),
                byIndex: static (rows, byIndex) => Fin.Succ(Optional(rows.Document.Manifest.FindIndex<T>(byIndex.Index)).Filter(row => row.Index == byIndex.Index)),
                byName: static (rows, byName) => ModelComponent.ModelComponentTypeRequiresUniqueName(rows.ComponentType) && !ModelComponent.ModelComponentTypeIncludesParent(rows.ComponentType)
                    ? Fin.Succ(Optional(rows.Document.Manifest.FindName<T>(byName.Name, Guid.Empty)))
                    : new Invalid(nameof(ComponentRef.ByName)))
            .Bind(found => found.Filter(row => includeDeleted || !row.IsDeleted).ToFin(new MissingComponent(table.ComponentType, address))));

    public static IO<T> Row<T>(RhinoDocCommonTable<T> table, int index) where T : ModelComponent =>
        Find(table, new ComponentRef.ByIndex(index), includeDeleted: false);

    public static IO<int> Index<T>(RhinoDocCommonTable<T> table, Option<ComponentRef> address) where T : ModelComponent =>
        address.Traverse(row => Find(table, row, includeDeleted: false)).As().Map(static found => Answers.Unset(found.Map(static row => row.Index)));

    public static StringComparer Names<T>() where T : ModelComponent =>
        ModelComponent.ModelComponentTypeIgnoresCase(ManifestTable.GetModelComponentTypeFromGenericType<T>()) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    public static IO<Unit> Named<T>(T staged, Option<string> name) where T : ModelComponent =>
        Locked(IO.lift(() => { _ = name.Iter(value => staged.Name = value); }), nameof(ModelComponent.Name));

    public static IO<int> AddRow<T>(TableAccessors<T> accessors, IO<T> create, Func<T, IO<Unit>> edit) where T : ModelComponent =>
        DisposalOps.Using(create, row =>
            from edited in edit(row)
            from index in IO.lift(() => Answers.Required(accessors.Add(row), nameof(TableAccessors<>.Add)))
            select index);

    public static IO<Unit> ModifyRow<T>(TableAccessors<T> accessors, int index, Func<T, IO<Unit>> edit, bool quiet) where T : ModelComponent =>
        from live in Row(accessors.Table, index)
        from modified in DisposalOps.Using(() => accessors.Copy(live), staged => edit(staged).Bind(_ => Modified(accessors, staged, index, quiet)))
        select modified;

    public static IO<Unit> DeleteRows<T>(TableAccessors<T> accessors, Seq<T> rows, bool quiet) where T : ModelComponent =>
        IO.lift(() => rows.Traverse(row => RefusedElement.Unless(accessors.Delete(row.Index, quiet), nameof(TableAccessors<>.Delete), row.Index)).As().Map(static _ => unit));

    public static IO<Seq<TRow>> ReadRows<T, TRow>(Func<T?[]?> read, Func<T, IO<TRow>> project) where T : class, IDisposable =>
        DisposalOps.Using(IO.lift(() => Answers.Present(read())), rows => rows.TraverseM(project).As());

    private static IO<Unit> Modified<T>(TableAccessors<T> accessors, T staged, int index, bool quiet) where T : ModelComponent =>
        IO.lift(() => Refused.Unless(accessors.Modify(staged, index, quiet), nameof(TableAccessors<>.Modify)));

    // --- [RECONCILE]
    public static IO<Seq<T>> Present<T>(RhinoDocCommonTable<T> table) where T : ModelComponent =>
        IO.lift(() => toSeq(table).Filter(static row => !row.IsReference && Answers.Present(row.Name).IsSome).Strict());

    public static Validation<Error, TablePlan<TRow, TSpec>> Plan<TRow, TSpec>(Seq<TRow> present, TableSpec<TSpec> spec, Func<TSpec, Option<string>> name) where TRow : ModelComponent =>
        from named in spec.Rows
            .Map(static (row, position) => (Row: row, Position: position))
            .Traverse(pair => name(pair.Row).Map(key => (Name: key, pair.Row)).ToValidation<Error>(new InvalidElement(nameof(ModelComponent.Name), pair.Position)))
            .As()
        let names = Names<TRow>()
        from unique in Answers.Unique(named.Map(static row => row.Name), names, nameof(ModelComponent.Name))
        select new TablePlan<TRow, TSpec>(
            named.Map(row => new TableUpsert<TRow, TSpec>(row.Name, present.Find(held => names.Equals(held.Name, row.Name)), row.Row)).Strict(),
            spec.Exclusive ? present.Filter(held => !named.Exists(row => names.Equals(held.Name, row.Name))).Strict() : Seq<TRow>());

    public static IO<Seq<(string Name, int Index)>> Upsert<T, TSpec>(TableAccessors<T> accessors, Seq<TableUpsert<T, TSpec>> upserts, Func<T, TSpec, IO<Unit>> write, Func<T, T, IO<bool>> same, bool quiet)
        where T : ModelComponent, new() =>
        upserts.TraverseM(upsert => upsert.Present.Match(
                Some: row => DisposalOps.Using(() => accessors.Copy(row), staged =>
                    from written in write(staged, upsert.Spec)
                    from held in same(row, staged)
                    from accepted in unless(held, Modified(accessors, staged, row.Index, quiet)).As()
                    select (upsert.Name, row.Index)),
                None: () => AddRow(accessors, IO.lift(static () => new T()), staged => write(staged, upsert.Spec)).Map(index => (upsert.Name, index))))
            .As();

    // --- [ATTRIBUTES]
    public static IO<TValue> WithAttributes<TValue>(RhinoDoc doc, Option<ObjectAttributes> attributes, Func<ObjectAttributes, IO<TValue>> body) =>
        attributes.Match(Some: body, None: () => DisposalOps.Using(doc.CreateDefaultAttributes, body));

    public static IO<TValue> WithAttributes<TValue>(RhinoDoc doc, Seq<Option<ObjectAttributes>> attributes, Func<Seq<ObjectAttributes>, IO<TValue>> body) =>
        attributes.Match(
            Empty: () => body(Seq<ObjectAttributes>()),
            Tail: (head, rest) => WithAttributes(doc, head, first => WithAttributes(doc, rest, others => body(first.Cons(others)))));

    // --- [ANSWERS]
    public static IO<TValue> Locked<TValue>(IO<TValue> write, string member) =>
        write | @catch(static error => error.HasException<InvalidOperationException>(), _ => IO.fail<TValue>(new Refused(member)));
}
