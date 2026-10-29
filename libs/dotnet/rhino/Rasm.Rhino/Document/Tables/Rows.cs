using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rhino;
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

public sealed record ComponentIdentity(Guid Id, ModelComponentType Type);

public sealed record TableKind<T>(
    Func<RhinoDoc, RhinoDocCommonTable<T>> Table,
    Func<T> Fresh,
    Func<T, T> Copy,
    Func<RhinoDoc, T, int> Add,
    Func<RhinoDoc, T, int, bool> Modify,
    Func<RhinoDoc, string> FreeName) where T : ModelComponent {
    public Func<RhinoDoc, Seq<T>> Live { get; init; } = doc => Conversions.Rows(Table(doc)).Filter(static row => !row.IsDeleted).Strict();

    public Func<T, string> Key { get; init; } = static row => row.Name;

    public IEqualityComparer<string> Names { get; init; } = Rows.Names<T>();

    public Func<T, Option<Guid>> Parent { get; init; } = static _ => None;

    public Func<Seq<string>, Seq<string>> Arrange { get; init; } = static keys => keys;

    public Func<RhinoDoc, Option<T>, string, Fin<Seq<RowEdit<T>>>> Place { get; init; } = static (_, _, key) =>
        RhinoFault.Unless(ModelComponent.IsValidComponentName(key), RhinoReason.Unqualified, key)
            .Map(_ => Seq(Rows.Set<T, string>(static row => row.Name, static (row, name) => row.Name = name, key)));

    public Option<Func<RhinoDoc, T, bool>> Purge { get; init; }

    public Option<Action<RhinoDoc, Seq<int>>> Sort { get; init; }
}

public sealed class RowEdit<T> where T : class {
    internal RowEdit(Step<T> write, Option<Func<T, bool>> holds) => (Write, Holds) = (write, holds);

    public Step<T> Write { get; }

    public Option<Func<T, bool>> Holds { get; }
}

public sealed record RowSpec<T>(string Key, Seq<RowEdit<T>> Edits) where T : class;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RowPlan<TRow, TSpec> {
    public sealed record Create(TSpec Spec) : RowPlan<TRow, TSpec>;

    public sealed record Change(TRow Row, TSpec Spec) : RowPlan<TRow, TSpec>;

    public sealed record Remove(TRow Row, Prune Prune) : RowPlan<TRow, TSpec>;

    public sealed record Order(Seq<string> Keys) : RowPlan<TRow, TSpec>;
}

[SmartEnum]
public sealed partial class Prune {
    public static readonly Prune Keep = new(removes: false);
    public static readonly Prune Delete = new(removes: true);
    public static readonly Prune Purge = new(removes: true);

    public bool Removes { get; }
}

// --- [TABLES] --------------------------------------------------------------------------
public static class TableKinds {
    public static TableKind<Linetype> Linetypes { get; } = new(
        static doc => doc.Linetypes,
        static () => new Linetype(),
        static live => new Linetype(live),
        static (doc, row) => doc.Linetypes.Add(row),
        static (doc, row, index) => doc.Linetypes.Modify(row, index, quiet: true),
        static doc => doc.Linetypes.GetUnusedLinetypeName());

    public static TableKind<DimensionStyle> DimensionStyles { get; } = new(
        static doc => doc.DimStyles,
        static () => new DimensionStyle(),
        static live => live.Duplicate(),
        static (doc, row) => doc.DimStyles.Add(row, reference: false),
        static (doc, row, index) => doc.DimStyles.Modify(row, index, quiet: true),
        static doc => doc.DimStyles.GetUnusedStyleName());

    public static TableKind<HatchPattern> HatchPatterns { get; } = new(
        static doc => doc.HatchPatterns,
        static () => new HatchPattern(),
        static live => new HatchPattern(live),
        static (doc, row) => doc.HatchPatterns.Add(row),
        static (doc, row, index) => doc.HatchPatterns.Modify(row, index, quiet: true),
        static doc => doc.HatchPatterns.GetUnusedHatchPatternName());

    public static TableKind<SectionStyle> SectionStyles { get; } = new(
        static doc => doc.SectionStyles,
        static () => new SectionStyle(),
        static live => new SectionStyle(live),
        static (doc, row) => doc.SectionStyles.Add(row),
        static (doc, row, index) => doc.SectionStyles.Modify(row, index, quiet: true),
        static doc => doc.SectionStyles.GetUnusedSectionStyleName());

    public static TableKind<Markup> Markups { get; } = new(
        static doc => doc.Markups,
        static () => new Markup(),
        static live => new Markup(live),
        static (doc, row) => doc.Markups.Add(row),
        static (doc, row, index) => doc.Markups.Modify(row, index, quiet: true),
        static doc => doc.Markups.GetUnusedMarkupName());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Rows {
    // --- [ADDRESSES]
    public static IO<T> Find<T>(RhinoDocCommonTable<T> table, ComponentRef<T> address, bool includeDeleted = false) where T : ModelComponent =>
        IO.lift(() => address.Switch(
                table,
                byId: static (rows, byId) => Optional(rows.Document.Manifest.FindId<T>(byId.Id)),
                byIndex: static (rows, byIndex) => Optional(rows.Document.Manifest.FindIndex<T>(byIndex.Index)),
                byName: static Fin<Option<T>> (rows, byName) =>
                    RhinoFault.Unless(
                            ModelComponent.ModelComponentTypeRequiresUniqueName(rows.ComponentType) && !ModelComponent.ModelComponentTypeIncludesParent(rows.ComponentType),
                            RhinoReason.Unsupported, rows.ComponentType)
                        .Map(_ => Optional(rows.Document.Manifest.FindName<T>(byName.Name, Guid.Empty))))
            .Bind(found => found.Filter(row => includeDeleted || !row.IsDeleted)
                .ToFin(new RhinoFault($"{address} in {table.ComponentType}", RhinoReason.Missing))));

    public static IO<int> Index<T>(RhinoDocCommonTable<T> table, Option<ComponentRef<T>> address) where T : ModelComponent =>
        address.Traverse(row => Find(table, row)).As().Map(static found => Conversions.Unset(found.Map(static row => row.Index)));

    public static IEqualityComparer<string> Names<T>() where T : ModelComponent =>
        EqualityComparer<string>.Create(static (left, right) => NameKey<T>(left) == NameKey<T>(right), static name => NameKey<T>(name).GetHashCode());

    public static IO<Seq<T>> Listed<T>(RhinoDocCommonTable<T> table) where T : ModelComponent =>
        IO.lift(() => Conversions.Rows(table).Filter(static row => !row.IsReference && Conversions.Present(row.Name).IsSome).Strict());

    private static NameHash NameKey<T>(string? name) where T : ModelComponent => new(name, Guid.Empty, ManifestTable.GetModelComponentTypeFromGenericType<T>());

    // --- [EDITS]
    public static RowEdit<T> Set<T, TValue>(
        Func<T, TValue> read, Action<T, TValue> write, TValue value, [CallerArgumentExpression(nameof(read))] string member = "")
        where T : class =>
        new(new Step<T>(row => IO.lift(() => {
            write(row, value);
            return row;
        }), member), Some<Func<T, bool>>(row => EqualityComparer<TValue>.Default.Equals(read(row), value)));

    public static RowEdit<T> Write<T>(Func<T, IO<Unit>> write, [CallerArgumentExpression(nameof(write))] string member = "") where T : class =>
        new(new Step<T>(row => write(row).Map(_ => row), member), None);

    public static IO<T> Edit<T>(RhinoDoc doc, TableKind<T> kind, ComponentRef<T> address, Seq<RowEdit<T>> edits) where T : ModelComponent =>
        (from live in Find(kind.Table(doc), address)
         from edited in Edits.Apply(Staged(doc, kind, edits), live, edits.Map(static edit => edit.Write))
         select edited).Bracket();

    public static IO<T> Add<T>(RhinoDoc doc, TableKind<T> kind, Option<string> key, Seq<RowEdit<T>> edits) where T : ModelComponent =>
        (from fresh in use(kind.Fresh)
         from placed in IO.lift(() => kind.Place(doc, None, key.IfNone(() => kind.FreeName(doc))))
         from added in Edits.Apply(Added(doc, kind, placed + edits), fresh, (placed + edits).Map(static edit => edit.Write))
         select added).Bracket();

    // --- [STAGES]
    public static Stage<T> Staged<T>(RhinoDoc doc, TableKind<T> kind, Seq<RowEdit<T>> edits) where T : ModelComponent =>
        new(
            Copy: live => use(() => kind.Copy(live)),
            Unchanged: (live, _) => Satisfied(edits, live),
            Commit: (live, staged) => Products.Accepted(() => kind.Modify(doc, staged, live.Index)) >> Landed(kind.Table(doc), new ComponentRef<T>.ById(live.Id), edits));

    private static Stage<T> Added<T>(RhinoDoc doc, TableKind<T> kind, Seq<RowEdit<T>> edits) where T : ModelComponent =>
        new(
            Copy: static fresh => IO.pure(fresh),
            Unchanged: static (_, _) => false,
            Commit: (_, staged) => IO.lift(() => Conversions.Required(Conversions.Present(kind.Add(doc, staged))))
                .Bind(index => Landed(kind.Table(doc), new ComponentRef<T>.ByIndex(index), edits)));

    private static bool Satisfied<T>(Seq<RowEdit<T>> edits, T row) where T : class =>
        edits.ForAll(edit => edit.Holds.Exists(holds => holds(row)));

    private static IO<T> Landed<T>(RhinoDocCommonTable<T> table, ComponentRef<T> address, Seq<RowEdit<T>> edits) where T : ModelComponent =>
        Find(table, address).Bind(live => IO.lift(() => Products.Each(edits, (edit, index) =>
            edit.Holds.ForAll(holds => holds(live)) ? Fin.Succ(unit) : new RhinoFault(edit.Write.Member, RhinoReason.Declined, index)).Map(_ => live)));

    // --- [PLANS]
    public static Fin<Seq<RowPlan<TRow, TSpec>>> Plan<TRow, TSpec>(
        Seq<TRow> rows, Seq<TSpec> specs, Func<TRow, string> rowKey, Func<TSpec, string> key, IEqualityComparer<string> names, Prune prune) =>
        from unique in Conversions.Unique(specs, key, names)
        let held = rows.ToLookup(rowKey, names)
        let wanted = unique.Map(key).ToHashSet(names)
        select unique.Map(spec => toSeq(held[key(spec)]).Head.Match<RowPlan<TRow, TSpec>>(
                Some: row => new RowPlan<TRow, TSpec>.Change(row, spec),
                None: () => new RowPlan<TRow, TSpec>.Create(spec)))
            + (prune.Removes ? rows.Filter(row => !wanted.Contains(rowKey(row))).Map<RowPlan<TRow, TSpec>>(row => new RowPlan<TRow, TSpec>.Remove(row, prune)) : []);

    public static IO<Seq<RowPlan<T, RowSpec<T>>>> Plan<T>(RhinoDoc doc, TableKind<T> kind, Seq<RowSpec<T>> specs, Prune prune) where T : ModelComponent =>
        from rows in IO.lift(() => kind.Live(doc))
        let owned = rows.Filter(static row => !row.IsReference)
        let declared = toHashMap(specs.Map(static spec => (spec.Key, spec)))
        let arranged = kind.Arrange(specs.Map(static spec => spec.Key)).Map(key => declared.Find(key).IfNone(() => new RowSpec<T>(key, [])))
        from matched in IO.lift(() => Plan(owned, arranged, kind.Key, static spec => spec.Key, kind.Names, prune))
        let removed = toHashSet(matched.Choose(static step => step.Switch(
            create: static _ => Option<Guid>.None,
            change: static _ => None,
            remove: static remove => Some(remove.Row.Id),
            order: static _ => None)))
        let pending = matched.Filter(step => step.Switch(
            (Document: doc, Kind: kind, Removed: removed),
            create: static (_, _) => true,
            change: static (state, change) => !state.Kind.Place(state.Document, Some(change.Row), change.Spec.Key).ToOption().Exists(placed => Satisfied(placed + change.Spec.Edits, change.Row)),
            remove: static (state, remove) => !state.Kind.Parent(remove.Row).Exists(state.Removed.Contains),
            order: static (_, _) => true))
        let order = arranged.Map(static spec => spec.Key) + (prune.Removes ? [] : owned.Map(kind.Key).Filter(key => !arranged.Exists(spec => kind.Names.Equals(spec.Key, key))))
        let sorted = kind.Sort.IsSome && !order.SequenceEqual(owned.Map(kind.Key), kind.Names)
        select sorted ? pending.Add(new RowPlan<T, RowSpec<T>>.Order(order)) : pending;

    public static IO<Seq<T>> Apply<T>(RhinoDoc doc, TableKind<T> kind, Seq<RowPlan<T, RowSpec<T>>> plan) where T : ModelComponent =>
        plan.TraverseM(step => step.Switch(
                (Document: doc, Kind: kind),
                create: static (state, create) => Add(state.Document, state.Kind, Some(create.Spec.Key), create.Spec.Edits).Map(Optional),
                change: static (state, change) =>
                    from placed in IO.lift(() => state.Kind.Place(state.Document, Some(change.Row), change.Spec.Key))
                    from edited in Edit(state.Document, state.Kind, new ComponentRef<T>.ById(change.Row.Id), placed + change.Spec.Edits)
                    select Optional(edited),
                remove: static (state, remove) => Remove(state.Document, state.Kind, remove.Row, remove.Prune).Map(static _ => Option<T>.None),
                order: static (state, order) => Sorted(state.Document, state.Kind, order.Keys).Map(static _ => Option<T>.None)))
            .As()
            .Map(static touched => touched.Somes().Strict());

    public static IO<Committed<Seq<RowPlan<T, RowSpec<T>>>>> Reconcile<T>(RhinoDoc doc, TableKind<T> kind, Seq<RowSpec<T>> specs, Prune prune, string name, RedrawPolicy redraw)
        where T : ModelComponent =>
        Commits.Commit(doc, name, redraw,
            from plan in Plan(doc, kind, specs, prune)
            from applied in Apply(doc, kind, plan)
            select plan);

    public static IO<Unit> Remove<T>(RhinoDoc doc, TableKind<T> kind, T row, Prune prune) where T : ModelComponent =>
        from removal in IO.lift(() => prune == Prune.Purge
            ? kind.Purge.ToFin(new RhinoFault($"{nameof(kind.Purge)} of {row.ComponentType}", RhinoReason.Unsupported)).Map(purge => purge(doc, row))
            : kind.Table(doc).Delete(row))
        from gone in IO.lift(() => RhinoFault.Unless(Optional(kind.Table(doc).FindId(row.Id)).ForAll(static held => held.IsDeleted), RhinoReason.Declined, kind.Key(row)))
        select unit;

    private static IO<Unit> Sorted<T>(RhinoDoc doc, TableKind<T> kind, Seq<string> keys) where T : ModelComponent =>
        from rows in IO.lift(() => kind.Live(doc))
        from sort in IO.lift(() => kind.Sort.ToFin(new RhinoFault($"order of {kind.Table(doc).ComponentType}", RhinoReason.Unsupported)))
        let listed = keys.Choose(key => rows.Find(row => !row.IsReference && kind.Names.Equals(kind.Key(row), key)))
        let ids = toHashSet(listed.Map(static row => row.Id))
        from sorted in IO.lift(() => sort(doc, (listed + rows.Filter(row => !ids.Contains(row.Id))).Map(static row => row.Index)))
        select unit;
}
