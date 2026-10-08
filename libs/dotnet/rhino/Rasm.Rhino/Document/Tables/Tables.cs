using Rasm.Drafting;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ComponentRef<T> {
    public sealed record ById(Guid Id) : ComponentRef<T>;

    public sealed record ByIndex(int Index) : ComponentRef<T>;

    public sealed record ByName(string Name) : ComponentRef<T>;
}

public readonly record struct ComponentIdentity(Guid Id, ModelComponentType Type);

public sealed record TableKind<T>(
    Func<RhinoDoc, RhinoDocCommonTable<T>> Table,
    Func<T, T> Copy,
    Func<RhinoDoc, T, int> Add,
    Func<RhinoDoc, T, int, bool> Modify,
    Func<RhinoDoc, string> FreeName) where T : ModelComponent, new();

[SmartEnum<ModelComponentType>(SkipIComparable = true)]
[ValidationError<InvalidRhinoValue>]
public sealed partial class PurgeKind {
    public static readonly PurgeKind Linetypes = new(ModelComponentType.LinePattern, static doc => doc.Linetypes.PurgeUnused());

    public static readonly PurgeKind Layers = new(ModelComponentType.Layer, static doc => doc.Layers.PurgeUnused());

    public static readonly PurgeKind Groups = new(ModelComponentType.Group, static doc => doc.Groups.PurgeUnused());

    public static readonly PurgeKind DimensionStyles = new(ModelComponentType.DimStyle, static doc => doc.DimStyles.PurgeUnused());

    public static readonly PurgeKind HatchPatterns = new(ModelComponentType.HatchPattern, static doc => doc.HatchPatterns.PurgeUnused());

    public static readonly PurgeKind InstanceDefinitions = new(ModelComponentType.InstanceDefinition, static doc => doc.InstanceDefinitions.PurgeUnused());

    [UseDelegateFromConstructor]
    public partial int PurgeUnused(RhinoDoc doc);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlotWeight {
    public sealed record Default() : PlotWeight;

    public sealed record NoPlot() : PlotWeight;

    public sealed record Width : PlotWeight {
        internal Width(Length value) => Value = value;

        public Length Value { get; }
    }

    public static PlotWeight Of(LineWidth width) => new Width(width.Width);

    public static PlotWeight FromHost(double millimeters) =>
        millimeters switch {
            > 0.0 => new Width(Length.FromMillimeters(millimeters)),
            < 0.0 => new NoPlot(),
            _ => new Default(),
        };

    public static Option<PlotWeight> FromInheritable(double millimeters) =>
        millimeters < -1.0 ? None : Some(FromHost(millimeters));

    public static double ToInheritable(Option<PlotWeight> weight) =>
        weight.Match(Some: static some => some.ToHost(), None: static () => -10.0);

    public double ToHost() =>
        Switch(@default: static _ => 0.0, noPlot: static _ => -1.0, width: static width => width.Value.Millimeters.ToDouble());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeRef {
    public sealed record Continuous() : LinetypeRef;

    public sealed record Row(ComponentRef<Linetype> Address) : LinetypeRef;

    public static Option<LinetypeRef> FromHost(int index) =>
        index switch {
            -1 => Some<LinetypeRef>(new Continuous()),
            >= 0 => Some<LinetypeRef>(new Row(new ComponentRef<Linetype>.ByIndex(index))),
            _ => Option<LinetypeRef>.None,
        };

    public IO<int> Resolve(RhinoDoc doc) =>
        Switch(
            doc,
            continuous: static (_, _) => IO.pure(-1),
            row: static (document, row) => TableOps.Find(document.Linetypes, row.Address, includeDeleted: false).Map(static found => found.Index));
}

public readonly record struct SelectionOptions(bool SyncHighlight, bool Persistent, bool IgnoreGrips, bool IgnoreLayerLocks, bool IgnoreLayerVisibility);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SelectionEdit {
    public sealed record Set(bool Select) : SelectionEdit;

    public sealed record Replace() : SelectionEdit;

    public int Apply(ObjectTable table, Seq<Guid> ids, SelectionOptions options) =>
        Switch(
            (Table: table, Ids: ids, Options: options),
            set: static (state, set) => state.Table.Select(
                state.Ids, set.Select, state.Options.SyncHighlight, state.Options.Persistent, state.Options.IgnoreGrips, state.Options.IgnoreLayerLocks, state.Options.IgnoreLayerVisibility),
            replace: static (state, _) => state.Table.SetSelectedObjects(
                state.Ids, state.Options.SyncHighlight, state.Options.Persistent, state.Options.IgnoreGrips, state.Options.IgnoreLayerLocks, state.Options.IgnoreLayerVisibility));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VisibilityState {
    public sealed record Hidden(Option<string> Group) : VisibilityState;

    public sealed record Shown() : VisibilityState;

    public sealed record Locked() : VisibilityState;

    public sealed record Unlocked() : VisibilityState;

    public Fin<Unit> Apply(ObjectTable table, RhinoObject target, bool ignoreLayerMode, int index) =>
        Switch(
            (Table: table, Target: target, IgnoreLayerMode: ignoreLayerMode, Index: index),
            hidden: static (state, hidden) => state.Target.IsHidden ? unit : RefusedElement.Unless(
                hidden.Group.Match(Some: group => state.Table.Hide(state.Target, state.IgnoreLayerMode, group), None: () => state.Table.Hide(state.Target, state.IgnoreLayerMode)),
                nameof(ObjectTable.Hide), state.Index),
            shown: static (state, _) => state.Target.IsHidden ? RefusedElement.Unless(state.Table.Show(state.Target, state.IgnoreLayerMode), nameof(ObjectTable.Show), state.Index) : unit,
            locked: static (state, _) => state.Target.IsLocked ? unit : RefusedElement.Unless(state.Table.Lock(state.Target, state.IgnoreLayerMode), nameof(ObjectTable.Lock), state.Index),
            unlocked: static (state, _) => state.Target.IsLocked ? RefusedElement.Unless(state.Table.Unlock(state.Target, state.IgnoreLayerMode), nameof(ObjectTable.Unlock), state.Index) : unit);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TransformMode {
    public sealed record Move(bool DeleteOriginal) : TransformMode;

    public sealed record History() : TransformMode;

    public Fin<Guid> Apply(ObjectTable table, RhinoObject target, Transform xform, int index) =>
        Switch(
            (Table: table, Target: target, Xform: xform, Index: index),
            move: static (state, move) => Conversions.Present(state.Table.Transform(state.Target, state.Xform, move.DeleteOriginal))
                .ToFin(new RefusedElement(nameof(ObjectTable.Transform), state.Index)),
            history: static (state, _) => Conversions.Present(state.Table.TransformWithHistory(state.Target, state.Xform))
                .ToFin(new RefusedElement(nameof(ObjectTable.TransformWithHistory), state.Index)));
}

public sealed record GeometryPair(GeometryBase Geometry, Option<ObjectAttributes> Attributes);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableOp {
    public sealed record Add(Seq<GeometryPair> Rows, Option<HistoryRecord> History, bool Reference) : TableOp;

    public sealed record Replace(Guid Id, GeometryBase Geometry, bool IgnoreModes) : TableOp;

    public sealed record Delete(ObjectTarget Target, bool Quiet, bool IgnoreModes) : TableOp;

    public sealed record Move(ObjectTarget Target, Transform Xform, TransformMode Mode) : TableOp;

    public sealed record ModifyAttributes(ObjectTarget Target, Func<ObjectAttributes, IO<Unit>> Edit, bool Quiet) : TableOp;

    public sealed record State(ObjectTarget Target, VisibilityState Next, bool IgnoreLayerMode) : TableOp;

    public sealed record Undelete(ObjectTarget Target) : TableOp;

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

[Union]
public abstract partial record UndoOp {
    public sealed record Undo() : UndoOp;

    public sealed record Redo() : UndoOp;

    public sealed record ClearUndo(bool PurgeDeleted, Option<uint> Serial) : UndoOp;

    public sealed record ClearRedo() : UndoOp;
}

public sealed record CustomUndo(string Description, IO<Unit> Undo, IO<Unit> Redo, CallbackSite Site);

public sealed record TableSpec<TSpec>(Seq<(string Name, TSpec Spec)> Rows, bool Exclusive);

public sealed record TableUpsert<T, TSpec>(string Name, Option<T> Existing, TSpec Spec) where T : ModelComponent;

public sealed record TablePlan<T, TSpec>(Seq<TableUpsert<T, TSpec>> Upserts, Seq<T> Prunes) where T : ModelComponent;

// --- [TABLES] --------------------------------------------------------------------------
public static class TableKinds {
    public static TableKind<Layer> Layers { get; } = new(
        static doc => doc.Layers,
        static live => {
            Layer staged = new();
            staged.CopyAttributesFrom(live);
            return staged;
        },
        static (doc, row) => doc.Layers.Add(row),
        static (doc, row, index) => doc.Layers.Modify(row, index, quiet: true),
        static doc => doc.Layers.GetUnusedLayerName());

    public static TableKind<Linetype> Linetypes { get; } = new(
        static doc => doc.Linetypes,
        static live => new Linetype(live),
        static (doc, row) => doc.Linetypes.Add(row),
        static (doc, row, index) => doc.Linetypes.Modify(row, index, quiet: true),
        static doc => doc.Linetypes.GetUnusedLinetypeName());

    public static TableKind<DimensionStyle> DimensionStyles { get; } = new(
        static doc => doc.DimStyles,
        static live => live.Duplicate(),
        static (doc, row) => doc.DimStyles.Add(row, reference: false),
        static (doc, row, index) => doc.DimStyles.Modify(row, index, quiet: true),
        static doc => doc.DimStyles.GetUnusedStyleName());

    public static TableKind<HatchPattern> HatchPatterns { get; } = new(
        static doc => doc.HatchPatterns,
        static live => new HatchPattern(live),
        static (doc, row) => doc.HatchPatterns.Add(row),
        static (doc, row, index) => doc.HatchPatterns.Modify(row, index, quiet: true),
        static doc => doc.HatchPatterns.GetUnusedHatchPatternName());

    public static TableKind<SectionStyle> SectionStyles { get; } = new(
        static doc => doc.SectionStyles,
        static live => new SectionStyle(live),
        static (doc, row) => doc.SectionStyles.Add(row),
        static (doc, row, index) => doc.SectionStyles.Modify(row, index, quiet: true),
        static doc => doc.SectionStyles.GetUnusedSectionStyleName());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TableOps {
    // --- [ADDRESSES]
    public static IO<T> Find<T>(RhinoDocCommonTable<T> table, ComponentRef<T> address, bool includeDeleted) where T : ModelComponent =>
        IO.lift(() => address.Switch(
                table,
                byId: static (rows, byId) => Fin.Succ(Optional(rows.Document.Manifest.FindId<T>(byId.Id))),
                byIndex: static (rows, byIndex) => Fin.Succ(Optional(rows.Document.Manifest.FindIndex<T>(byIndex.Index))),
                byName: static (rows, byName) =>
                    ModelComponent.ModelComponentTypeRequiresUniqueName(rows.ComponentType) && !ModelComponent.ModelComponentTypeIncludesParent(rows.ComponentType)
                        ? Fin.Succ(Optional(rows.Document.Manifest.FindName<T>(byName.Name, Guid.Empty)))
                        : new NonUniqueName(rows.ComponentType))
            .Bind(found => found.Filter(row => includeDeleted || !row.IsDeleted).ToFin(new MissingComponent<T>(address))));

    public static IO<int> Index<T>(RhinoDocCommonTable<T> table, Option<ComponentRef<T>> address) where T : ModelComponent =>
        address.Traverse(row => Find(table, row, includeDeleted: false)).As().Map(static found => Conversions.Unset(found.Map(static row => row.Index)));

    public static IEqualityComparer<string> Names<T>() where T : ModelComponent =>
        EqualityComparer<string>.Create(static (left, right) => NameKey<T>(left) == NameKey<T>(right), static name => NameKey<T>(name).GetHashCode());

    public static IO<Unit> Named<T>(T staged, Option<string> name) where T : ModelComponent =>
        IO.lift(() => Callbacks.Thrown<InvalidOperationException, Unit>(() => name.Iter(value => staged.Name = value), nameof(ModelComponent.Name)));

    private static NameHash NameKey<T>(string? name) where T : ModelComponent => new(name, Guid.Empty, ManifestTable.GetModelComponentTypeFromGenericType<T>());

    // --- [ROWS]
    public static IO<Seq<T>> Rows<T>(RhinoDocCommonTable<T> table) where T : ModelComponent =>
        IO.lift(() => Conversions.Rows(table).Filter(static row => !row.IsReference && Conversions.Present(row.Name).IsSome).Strict());

    public static IO<int> AddRow<T>(RhinoDoc doc, TableKind<T> kind, Func<T, IO<Unit>> edit) where T : ModelComponent, new() =>
        (from staged in use(static () => new T())
         from edited in edit(staged)
         from index in IO.lift(() => Conversions.Required(kind.Add(doc, staged), nameof(TableKind<>.Add)))
         select index).Bracket();

    public static IO<Unit> ModifyRow<T>(RhinoDoc doc, TableKind<T> kind, ComponentRef<T> address, Func<T, IO<Unit>> edit) where T : ModelComponent, new() =>
        from live in Find(kind.Table(doc), address, includeDeleted: false)
        from modified in (
            from staged in use(() => kind.Copy(live))
            from edited in edit(staged)
            from landed in Modified(doc, kind, staged, live.Index)
            select landed).Bracket()
        select modified;

    public static Validation<Error, TablePlan<T, TSpec>> Plan<T, TSpec>(Seq<T> rows, TableSpec<TSpec> spec) where T : ModelComponent =>
        from names in Success<Error, IEqualityComparer<string>>(Names<T>())
        from named in Callbacks.Unique(spec.Rows, static row => row.Name, nameof(ModelComponent.Name), names)
        let held = rows.ToLookup(static row => row.Name, names)
        let wanted = named.ToLookup(static row => row.Name, names)
        select new TablePlan<T, TSpec>(
            named.Map(row => new TableUpsert<T, TSpec>(row.Name, toSeq(held[row.Name]).Head, row.Spec)).Strict(),
            spec.Exclusive ? rows.Filter(row => !wanted.Contains(row.Name)).Strict() : Seq<T>());

    public static IO<Seq<(string Name, int Index)>> Upsert<T, TSpec>(RhinoDoc doc, TableKind<T> kind, Seq<TableUpsert<T, TSpec>> upserts, Func<T, TSpec, IO<Unit>> write, Func<T, T, IO<bool>> unchanged)
        where T : ModelComponent, new() =>
        upserts.TraverseM(upsert => upsert.Existing.Match(
                Some: row =>
                    (from staged in use(() => kind.Copy(row))
                     from named in Named(staged, Some(upsert.Name))
                     from written in write(staged, upsert.Spec)
                     from same in unchanged(row, staged)
                     from modified in unless(same && string.Equals(row.Name, upsert.Name, StringComparison.Ordinal), Modified(doc, kind, staged, row.Index)).As()
                     select (upsert.Name, row.Index)).Bracket(),
                None: () => AddRow(doc, kind, staged => Named(staged, Some(upsert.Name)).Bind(_ => write(staged, upsert.Spec))).Map(index => (upsert.Name, index))))
            .As();

    private static IO<Unit> Modified<T>(RhinoDoc doc, TableKind<T> kind, T staged, int index) where T : ModelComponent, new() =>
        IO.lift(() => Refused.Unless(kind.Modify(doc, staged, index), nameof(TableKind<>.Modify)));

    // --- [ATTRIBUTES]
    public static IO<T> WithAttributes<T>(Func<ObjectAttributes> defaults, Option<ObjectAttributes> attributes, Func<ObjectAttributes, IO<T>> body) =>
        attributes.Match(Some: body, None: () => use(defaults).Bind(body).Bracket());

    public static IO<T> WithAttributes<T>(Func<ObjectAttributes> defaults, Seq<GeometryPair> members, Func<Seq<GeometryBase>, Seq<ObjectAttributes>, IO<T>> body) =>
        members.FoldBack(
            (Seq<ObjectAttributes> rows) => body(members.Map(static member => member.Geometry), rows),
            (next, member) => rows => WithAttributes(defaults, member.Attributes, value => next(rows.Add(value))))(Seq<ObjectAttributes>());

    // --- [RECORDED]
    public static IO<Seq<Guid>> Apply(RhinoDoc doc, TableOp op) =>
        op.Switch(
            doc,
            add: static (document, add) => add.Rows.TraverseM(row => Add(document, row, add.History, add.Reference)).As(),
            replace: static (document, replace) => IO.lift(() =>
                Refused.Unless(document.Objects.Replace(replace.Id, replace.Geometry, replace.IgnoreModes), Seq(replace.Id), nameof(ObjectTable.Replace))),
            delete: static (document, delete) => Each(document, delete.Target, found => document.Objects.Delete(found, delete.Quiet, delete.IgnoreModes), nameof(ObjectTable.Delete)),
            move: static (document, move) => Each(document, move.Target, (found, _, index) => IO.lift(() => move.Mode.Apply(document.Objects, found, move.Xform, index))),
            modifyAttributes: static (document, modify) => Each(document, modify.Target, (found, id, index) =>
                (from copy in use(() => found.Attributes.Duplicate())
                 from edited in modify.Edit(copy)
                 from landed in IO.lift(() => RefusedElement.Unless(document.Objects.ModifyAttributes(found, copy, modify.Quiet), id, nameof(ObjectTable.ModifyAttributes), index))
                 select landed).Bracket()),
            state: static (document, state) => Each(document, state.Target, (found, id, index) => IO.lift(() => state.Next.Apply(document.Objects, found, state.IgnoreLayerMode, index).Map(_ => id))),
            undelete: static (document, undelete) => Each(document, undelete.Target, document.Objects.Undelete, nameof(ObjectTable.Undelete)),
            pointCloud: static (document, cloud) => IO.lift(() => Conversions.Required(
                    document.Objects.AddOrderedPointCloud(cloud.X, cloud.Y, cloud.Z, cloud.Box.GetCorners(), cloud.Attributes.ValueUnsafe(), cloud.History.ValueUnsafe(), cloud.Reference),
                    nameof(ObjectTable.AddOrderedPointCloud))
                .Map(static id => Seq(id))),
            importPage: static (document, page) => IO.lift(() =>
                Refused.Unless(document.Views.ImportPageView(page.Path, page.MainViewportId, page.PageName), Seq<Guid>(), nameof(ViewTable.ImportPageView))));

    public static IO<Guid> Add(RhinoDoc doc, GeometryPair row, Option<HistoryRecord> history, bool reference) =>
        IO.lift(() => Conversions.Required(doc.Objects.Add(row.Geometry, row.Attributes.ValueUnsafe(), history.ValueUnsafe(), reference), nameof(ObjectTable.Add)));

    public static IO<Seq<Guid>> Place<T>(RhinoDoc doc, IO<Seq<T>> created, Option<ObjectAttributes> attributes, Option<HistoryRecord> history, bool reference) where T : GeometryBase =>
        created.Bracket(
            Use: products => products.TraverseM(product => Add(doc, new GeometryPair(product, attributes), history, reference)).As(),
            Fin: DisposalOps.Release);

    public static IO<Committed<Seq<Guid>>> Recorded(RhinoDoc doc, string name, RedrawPolicy redraw, Seq<CustomUndo> custom, Seq<TableOp> steps) =>
        Commits.Commit(doc, name, redraw,
            from registered in custom.TraverseM(undo => Register(doc, undo)).As()
            from touched in steps.TraverseM(step => Apply(doc, step)).As()
            select touched.Flatten());

    public static IO<Unit> Register(RhinoDoc doc, CustomUndo undo) =>
        IO.lift(() => Refused.Unless(
            doc.AddCustomUndoEvent(undo.Description, Callbacks.Handler<CustomUndoEventArgs>(args => undo.Undo.Bind(_ => Register(args.Document, undo with { Undo = undo.Redo, Redo = undo.Undo })), undo.Site)),
            nameof(RhinoDoc.AddCustomUndoEvent)));

    private static IO<Seq<Guid>> Each(RhinoDoc doc, ObjectTarget target, Func<RhinoObject, Guid, int, IO<Guid>> element) =>
        target.Objects(doc).Bind(objects => Callbacks.Each(objects.Map((found, index) => element(found, found.Id, index)).Strict()));

    private static IO<Seq<Guid>> Each(RhinoDoc doc, ObjectTarget target, Func<RhinoObject, bool> call, string member) =>
        Each(doc, target, (found, id, index) => IO.lift(() => RefusedElement.Unless(call(found), id, member, index)));

    // --- [IMMEDIATE]
    public static IO<int> Immediate(RhinoDoc doc, RedrawPolicy redraw, ImmediateOp op) =>
        Commits.WithinRedraw(doc, redraw, op.Switch(
            doc,
            selection: static (document, selection) =>
                from objects in selection.Target.Objects(document)
                from count in IO.lift(() => selection.Edit.Apply(document.Objects, objects.Map(static found => found.Id).Strict(), selection.Options))
                select count,
            clearSelection: static (document, clear) => IO.lift(() => document.Objects.UnselectAll(clear.IgnorePersistent)),
            flash: static (document, flash) =>
                from objects in flash.Target.Objects(document)
                from flashed in IO.lift(() => document.Views.FlashObjects(objects, flash.UseSelectionColor))
                select objects.Count,
            purge: static (document, purge) => Each(document, purge.Target, document.Objects.Purge, nameof(ObjectTable.Purge)).Map(static ids => ids.Count),
            purgeUnused: static (document, unused) => IO.lift(() => unused.Kind.PurgeUnused(document))));

    // --- [UNDO]
    public static IO<Unit> Undo(RhinoDoc doc, RedrawPolicy redraw, UndoOp op) =>
        from closed in IO.lift(() => UndoRecordOpen.Unless(Conversions.Present(doc.CurrentUndoRecordSerialNumber)))
        from stepped in Commits.WithinRedraw(doc, redraw, op.Switch(
            doc,
            undo: static (document, _) => Commits.Step(document, document.Undo, nameof(RhinoDoc.Undo)),
            redo: static (document, _) => Commits.Step(document, document.Redo, nameof(RhinoDoc.Redo)),
            clearUndo: static (document, clear) => IO.lift(() => clear.Serial.Match(
                Some: serial => document.ClearUndoRecords(serial, clear.PurgeDeleted),
                None: () => document.ClearUndoRecords(clear.PurgeDeleted))),
            clearRedo: static (document, _) => IO.lift(document.ClearRedoRecords)))
        select stepped;

    public static IO<(Seq<UndoRecordEntry> Undo, Seq<UndoRecordEntry> Redo)> UndoRecords(RhinoDoc doc) =>
        IO.lift(() => (Conversions.Rows(doc.GetUndoRecords()), Conversions.Rows(doc.GetRedoRecords())));
}
