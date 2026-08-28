# [RASM_RHINO_ANNOTATION_STYLE]

`StyleField` is the drafting-schema authority: each row admits one exact payload family, reads and writes one catalogued `DimensionStyle.Field` pairing, and feeds the same patch fold into table styles and per-annotation overrides. `TableGrip`, `TableOp`, `ListEdit`, and `TagEdit` are the namespace's shared component-table machinery — every Annotation table joins by declaring one grip and embedding one case.

Document spine component address `ResourceRef` resolves every Annotation table through its per-table `ResourceLens<T>` row, while `DraftPlan` and `DraftSpine` carry every drafting mutation through the Document grant and the shared `DocumentCommit.Sealed`.

## [01]-[INDEX]

- [02]-[ADDRESS_AND_VOCAB]: the `TableGrip<TComponent, TDef>` revision law and the `TableOp`/`ListEdit`/`TagEdit` verb families over the Document-owned `ResourceRef`/`ResourceLens<T>` address, the `DraftCrossing` detach boundary, the `DraftBorrow` input bracket, the shared drafting scalars, and explicit-value length-display rows.
- [03]-[FIELD_SCHEMA]: `StyleAxis`, `EnumFamily`, `StyleValue`, the `FieldTable<TOwner, THostEnum>` row mechanism, exact-family `StyleField` rows, the `StylePatch` fold that re-derives an override child from its construction parent, and the `DraftStandard` projection of the kernel drawing ladders onto those rows.
- [04]-[STYLE_PIPELINE]: `StyleDef`, `StyleOp`, `DraftPlan<StyleOp>`, and the `Styles.Commit` entry over the shared spine.
- [05]-[ASK_FAMILY]: `StyleAsk`/`StyleAnswer` — snapshot, built-in census, swatch lease, and name minting.
- [06]-[SPINE]: `DraftSpine` provides the shared command entry.
- [07]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[ADDRESS_AND_VOCAB]

- Owner: `TableGrip<TComponent, TDef>` extends the Document spine's `ResourceRef`/`ResourceLens<TComponent>` component address (tables.md) with every verb a host component table publishes — title, index, duplicate, mint, revise, retitle, modify, seat, retire, elect, the tag surface, an optional host-state bracket, and the two independent file halves — and owns the one duplicate-then-`Modify` revision law every component table walks; `TableOp<TComponent, TDef>` is the shared eight-case verb family over that grip; the address family, its `ResourceId`/`ResourceName`/`ResourceIndex` scalars, and the sentinel projectors live on the Document spine, never re-declared here.
- Law: each Annotation table contributes one `ResourceLens<T>` row and one `TableGrip<T, TDef>` — style, linetype, hatch, and section each declare exactly one — and embeds `TableOp` as a single case on its own program union; a page re-spelling `Author`, `Amend`, `Rename`, `Retag`, `Delete`, `SetCurrent`, `Import`, or `Export` as its own arm executes the deleted form, and no owner mints a second address family.
- Law: the grip's verb columns are the host's own members, and the two file halves are INDEPENDENT — a table that reads a `.lin` roster but publishes no writer states `Ingest` and leaves `Emit` absent, so `TableOp.Export` against it refuses typed where a paired read/write record forces a fabricated writer, exactly as `HatchSpec.Lens.ById` states the id lookup its table does not publish.
- Law: `TableGrip.Revised` releases its duplicate on all three paths — revise refusal, `Modify` refusal, and success — because `Modify` copies settings into the table row and leaves the duplicate the caller's; the released-on-refusal-only shape leaks one native per successful amendment. `Scoped` brackets a host STATE toggle around the revise and runs its exit on EVERY leg, so a refused edit can never leave the duplicate in the toggled state.
- Law: `ListEdit<TRow>` is the one index-addressed list-revision vocabulary and `ListSurface<TRow>` the interface it walks — the append, remove, optional in-place write, optional bulk purge, and row floor the host and its table declare — so a list without an in-place setter spells `Replace` as a bounded remove-then-append ONCE, here, and a list that may not empty carries its floor as a column instead of a guard re-spelled per page.
- Law: `TagEdit` is the one component user-string vocabulary — incremental set, delete, clear, and the whole-bag `Replace` whose replay rides `DocumentCommit.Compensated` — and `TagSurface` binds one tagged component's four re-published members; the host keeps that surface `internal` on `CommonObject`, so the surface is the argument and never a reflected delegate receiver.
- Law: `DraftCrossing.Crossed` lands a raw host batch on `GeometryHandle` custody through the Document crossing; disposal and failed-path aggregation remain kernel `Custody`/`Rollback` work.
- Law: `DraftScale` and `DraftAngle` are the namespace's drafting quantity owners — pattern and boundary scales and radian rotations admit once here and compose from every drafting page, so no page re-mints a scalar owner for a host property another already owns. A millimetre PLOT WEIGHT is not one of them: the ISO 128-24 width ladder is the kernel's and the folder's ingress is `Document/layers.md` `PrintPen`, whose three cases carry the host's `0.0` and `-1.0` sentinels as named rows.
- Law: `DraftBorrow` is the one input-custody bracket — a `GeometryHandle` argument projects its live native inside one lease scope through `Typed`, and a handle spread nests one scope per member, so a public drafting payload names custody and never a raw `Curve` or `Brep`.
- Law: `ResourceName` carries the ordinal-ignore-case comparer the host component tables key on, so a name census, a duplicate probe, and an occupancy guard read ONE comparison policy instead of passing `StringComparer.OrdinalIgnoreCase` per call site.
- Law: `TargetResolution.Only<TNative>` owns exactly-one object resolution with the typed cast probe; `LengthDisplayRow` keys each host value explicitly, including the host spelling `Millmeters`.
- Boundary: resolution reads live per call inside the owning operation — tables mutate under commands, so no resolved component is cached on a value.
- Packages: `Document/tables.md` (`ResourceRef`, `ResourceLens<T>`, `ResourceName`, `ResourceIndex`, `TableTarget`, `GeometryHandle`, `GeometryCrossing`, `TagOp.Snapshot`), `Document/commit.md` (`DocumentCommit.Compensated`, `HostInteraction`), `Document/session.md` (`DraftFault`), `Domain/results` (`Lease<T>`, `Custody`); Thinktecture.Runtime.Extensions; LanguageExt.Core; RhinoCommon component tables per `.api/api-rhinocommon-drafting-resources.md`.
- Growth: a component table joins with one lens row and one grip; a new table verb is one `TableOp` case beside one grip column every table already answers.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Specialized;
using System.Globalization;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Interaction;
using Rasm.Numerics;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using Rasm.Rhino.Document;
using Fields = Rasm.Rhino.Annotation.FieldTable<Rhino.DocObjects.DimensionStyle, Rhino.DocObjects.DimensionStyle.Field>;
using PixelSpan = Rasm.Numerics.Dimension;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] ---------------------------------------------------------------------------
public static class DraftCrossing {
    internal static Fin<Seq<GeometryHandle>> Crossed<TGeometry>(Seq<TGeometry> products)
        where TGeometry : GeometryBase =>
        DocumentCommit.Compensated(
            source: products,
            land: product => GeometryCrossing.Cross(source: product, mode: CrossingMode.Detach),
            rollback: landed => Custody.Dispose(held: landed),
            release: sources => Custody.Dispose(held: sources));
}

public readonly record struct TagSurface(
    Func<NameValueCollection> Read,
    Func<string, string, bool> Set,
    Func<string, bool> Drop,
    Action Clear);

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TagEdit {
    private TagEdit() { }
    public sealed record Set(StyleTag Tag) : TagEdit;
    public sealed record Delete(StyleTagKey Key) : TagEdit;
    public sealed record Clear : TagEdit;
    public sealed record Replace(HashMap<string, string> Tags) : TagEdit;

    internal Fin<Unit> Apply(TagSurface owner) => Switch(
        owner,
        set: static (context, edit) =>
            Admit.Confirm(success: context.Set(edit.Tag.Key.Value, edit.Tag.Value)),
        delete: static (context, edit) => Admit.Confirm(success: context.Drop(edit.Key.Value)),
        clear: static (context, _) => Try.lift(context.Clear).Run(),
        replace: static (context, edit) =>
            from admitted in toSeq(edit.Tags.AsIterable()).Traverse(pair =>
                (from name in Acceptance.Text(value: pair.Key)
                 from value in Acceptance.Text(value: pair.Value)
                 select (Name: name, Value: value)).ToValidation()).As().ToFin()
            from original in Try.lift(() => TagOp.Snapshot(context.Read())).Run()
            from _ in Try.lift(context.Clear).Run()
            from __ in DocumentCommit.Compensated(
                source: admitted,
                land: pair => Admit.Confirm(success: context.Set(pair.Name, pair.Value)).Map(_ => pair.Name),
                rollback: _ => Replay(rows: toSeq(original), owner: context))
            select unit);

    private static Fin<Unit> Replay(Seq<KeyValuePair<string, string>> rows, TagSurface owner) =>
        from _ in Try.lift(owner.Clear).Run()
        from __ in rows.Traverse(pair => Admit.Confirm(success: owner.Set(pair.Key, pair.Value)).ToValidation()).As().ToFin()
        select unit;
}

public readonly record struct ListSurface<TRow>(
    Func<int> Count,
    Func<TRow, Fin<Unit>> Append,
    Func<int, Fin<Unit>> Remove,
    Option<Func<int, TRow, Fin<Unit>>> Write,
    Option<Func< Fin<Unit>>> Purge,
    int Floor) where TRow : class;

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ListEdit<TRow> where TRow : class {
    private ListEdit() { }
    public sealed record Append(TRow Row) : ListEdit<TRow>;
    public sealed record Replace(int Index, TRow Row) : ListEdit<TRow>;
    public sealed record Remove(int Index) : ListEdit<TRow>;
    public sealed record Clear : ListEdit<TRow>;

    internal Fin<Unit> Apply(ListSurface<TRow> surface) => Switch(
        surface,
        append: static (context, edit) => context.Append(edit.Row),
        replace: static (context, edit) =>
            from _ in Bounded(surface: context, index: edit.Index)
            from __ in context.Write.Match(
                Some: write => write(edit.Index, edit.Row),
                None: () => from ___ in context.Remove(edit.Index)
                            from ____ in context.Append(edit.Row)
                            select unit)
            select unit,
        remove: static (context, edit) =>
            from _ in Bounded(surface: context, index: edit.Index)
            from __ in guard(context.Count() > context.Floor, new KernelFault.InvalidInput())
            from ___ in context.Remove(edit.Index)
            select unit,
        clear: static (context, _) =>
            from purge in context.Purge.ToFin(new KernelFault.Unsupported(
                valueType: typeof(TRow), OutputType: typeof(Unit)))
            from _ in guard(context.Floor is 0, new KernelFault.InvalidInput())
            from __ in purge()
            select unit);

    private static Fin<Unit> Bounded(ListSurface<TRow> surface, int index) =>
        guard(index >= 0 && index < surface.Count(), new KernelFault.InvalidInput()).ToFin();
}

public sealed record TableGrip<TComponent, TDef>(
    ResourceLens<TComponent> Lens,
    Func<TDef, ResourceName> Named,
    Func<TComponent, Fin<ResourceName>> Title,
    Func<TComponent, int> Index,
    Func<TComponent, TComponent> Duplicate,
    Func<TComponent, TagSurface> Tags,
    Func<RhinoDoc, TDef, Fin<TComponent>> Mint,
    Func<RhinoDoc, TComponent, TDef, Fin<Unit>> Revise,
    Func<TComponent, ResourceName, Fin<Unit>> Retitle,
    Func<RhinoDoc, TComponent, int, HostInteraction, Fin<Unit>> Modify,
    Func<RhinoDoc, TComponent, Fin<ResourceIndex>> Seat,
    Func<RhinoDoc, Seq<int>, HostInteraction, Fin<Unit>> Retire,
    Func<RhinoDoc, int, HostInteraction, Fin<Unit>> Elect,
    Option<Func<TComponent, Fin<Func< Fin<Unit>>>>> Scoped = default,
    Option<Func<DraftPath, HostInteraction, Fin<Seq<TComponent>>>> Ingest = default,
    Option<Func<DraftPath, Seq<TComponent>, Fin<Unit>>> Emit = default) where TComponent : class, IDisposable {
    internal Fin<Unit> Revised(
        ResourceRef target, RhinoDoc document, HostInteraction interaction,
        Func<TComponent, Fin<Unit>> revise) =>
        from live in target.Resolve(document: document, lens: Lens)
        let index = Index(live)
        from copy in Try.lift(() => Duplicate(live)).Run()
        from _ in Bracketed(copy: copy, revise: revise)
            .Rollback(release: () => Custody.Dispose(held: Seq(copy)))
        from __ in Modify(document, copy, index, interaction)
            .Rollback(release: () => Custody.Dispose(held: Seq(copy)))
        from ___ in Custody.Dispose(held: Seq(copy))
        select unit;

    internal bool Occupied(RhinoDoc document, ResourceName name) => Lens.ByName(document, name.Value) is not null;

    private Fin<Unit> Bracketed(TComponent copy, Func<TComponent, Fin<Unit>> revise) => Scoped.Match(
        Some: enter =>
            from exit in enter(copy)
            from outcome in revise(copy).Match(
                Succ: _ => exit(),
                Fail: primary => exit().Match(
                    Succ: _ => Fin.Fail<Unit>(error: primary),
                    Fail: restore => Fin.Fail<Unit>(error: primary + restore)))
            select outcome,
        None: () => revise(copy));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableOp<TComponent, TDef> where TComponent : class, IDisposable {
    private TableOp() { }
    public sealed record Author(TDef Def, HostInteraction Interaction) : TableOp<TComponent, TDef>;
    public sealed record Amend(ResourceRef Target, TDef Def, HostInteraction Interaction) : TableOp<TComponent, TDef>;
    public sealed record Rename(ResourceRef Target, ResourceName Name, HostInteraction Interaction) : TableOp<TComponent, TDef>;
    public sealed record Retag(ResourceRef Target, TagEdit Edit, HostInteraction Interaction) : TableOp<TComponent, TDef>;
    public sealed record Delete(Seq<ResourceRef> Targets, HostInteraction Interaction) : TableOp<TComponent, TDef>;
    public sealed record SetCurrent(ResourceRef Target, HostInteraction Interaction) : TableOp<TComponent, TDef>;
    public sealed record Import(DraftPath Path, HostInteraction Interaction) : TableOp<TComponent, TDef>;
    public sealed record Export(DraftPath Path, Seq<ResourceRef> Targets) : TableOp<TComponent, TDef>;

    internal Fin<Unit> Apply(TableGrip<TComponent, TDef> grip, RhinoDoc document) => Switch(
        (Grip: grip, Document: document),
        author: static (context, edit) =>
            from _ in guard(!context.Grip.Occupied(context.Document, context.Grip.Named(edit.Def)),
                new KernelFault.InvalidInput()).ToFin()
            from minted in context.Grip.Mint(context.Document, edit.Def)
            from __ in new Lease<TComponent>.Owned(Value: minted).Use(owned =>
                context.Grip.Seat(context.Document, owned).Map(static _ => unit))
            select unit,
        amend: static (context, edit) => context.Grip.Revised(
            target: edit.Target, document: context.Document, interaction: edit.Interaction, revise: (copy, key) => context.Grip.Revise(context.Document, copy, edit.Def)),
        rename: static (context, edit) => context.Grip.Revised(
            target: edit.Target, document: context.Document, interaction: edit.Interaction, revise: (copy, key) => context.Grip.Retitle(copy, edit.Name)),
        retag: static (context, edit) => context.Grip.Revised(
            target: edit.Target, document: context.Document, interaction: edit.Interaction, revise: (copy, key) => edit.Edit.Apply(owner: context.Grip.Tags(copy))),
        delete: static (context, edit) =>
            from _ in guard(!edit.Targets.IsEmpty, new KernelFault.InvalidInput()).ToFin()
            from rows in edit.Targets.TraverseM(target => target.Resolve(
                document: context.Document, lens: context.Grip.Lens)).As()
            let indices = rows.Map(row => context.Grip.Index(row))
            from __ in guard(indices.Distinct().Count == indices.Count, new KernelFault.InvalidInput())
            from ___ in context.Grip.Retire(context.Document, indices, edit.Interaction)
            select unit,
        setCurrent: static (context, edit) =>
            from row in edit.Target.Resolve(document: context.Document, lens: context.Grip.Lens)
            let index = context.Grip.Index(row)
            from _ in context.Grip.Elect(context.Document, index, edit.Interaction)
            select unit,
        import: static (context, edit) =>
            from ingest in context.Grip.Ingest.ToFin(new KernelFault.Unsupported(
                valueType: typeof(DraftPath), OutputType: typeof(Seq<TComponent>)))
            from read in ingest(edit.Path, edit.Interaction)
            from titles in read.TraverseM(native => context.Grip.Title(native)).As()
                .Rollback(release: () => Custody.Dispose(held: read))
            from _ in guard(
                !read.IsEmpty
                && titles.Distinct().Count == titles.Count
                && !titles.Exists(title => context.Grip.Occupied(context.Document, title)),
                new KernelFault.InvalidInput())
                .Rollback(release: () => Custody.Dispose(held: read))
            from __ in DocumentCommit.Compensated(
                source: read,
                land: native => context.Grip.Seat(context.Document, native),
                rollback: landed => context.Grip.Retire(
                    context.Document, landed.Map(static index => index.Value), HostInteraction.Silent),
                release: sources => Custody.Dispose(held: sources))
            select unit,
        export: static (context, edit) =>
            from emit in context.Grip.Emit.ToFin(new KernelFault.Unsupported(
                valueType: typeof(Seq<TComponent>), OutputType: typeof(DraftPath)))
            from rows in edit.Targets.TraverseM(target => target.Resolve(
                document: context.Document, lens: context.Grip.Lens)).As()
            from _ in guard(!rows.IsEmpty, new KernelFault.InvalidInput())
            from __ in emit(edit.Path, rows)
            select unit);
}

public static class TargetResolution {
    extension(TableTarget target) {
        internal Fin<(Guid Id, TNative Native)> Only<TNative>(RhinoDoc document) where TNative : RhinoObject =>
            from ids in target.Resolve(document: document)
            from id in ids switch { [Guid only] => Fin.Succ(value: only), _ => Fin.Fail<Guid>(error: new KernelFault.InvalidInput()) }
            from native in Optional(document.Objects.FindId(id)).ToFin(Fail: new KernelFault.MissingContext())
            from typed in Admit.Need(native as TNative)
            select (id, typed);
    }
}

public static class DraftBorrow {
    extension(GeometryHandle handle) {
        internal Fin<TResult> Typed<TNative, TResult>(Func<TNative, Fin<TResult>> project)
            where TNative : GeometryBase =>
            handle.With(project: native => Optional(native as TNative)
                .ToFin(Fail: new KernelFault.InvalidInput())
                .Bind(project));
    }

    extension(Seq<GeometryHandle> handles) {
        internal Fin<TResult> Typed<TNative, TResult>(Func<Seq<TNative>, Fin<TResult>> project)
            where TNative : GeometryBase =>
            handles.Head.Match(
                Some: head => head.Typed<TNative, TResult>(project: native =>
                    handles.Tail.Typed<TNative, TResult>(project: rest => project(Seq(native) + rest))),
                None: () => project(Seq<TNative>()));
    }
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class DraftScale {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0.0
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(DraftScale), value, "a finite positive drafting scale" }));
}

[ValueObject<double>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class DraftAngle {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = double.IsFinite(value)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(DraftAngle), value, "a finite radian rotation" }));
}

[SmartEnum<int>]
public sealed partial class LengthDisplayRow {
    public static readonly LengthDisplayRow ModelUnits = new(key: (int)DimensionStyle.LengthDisplay.ModelUnits);
    public static readonly LengthDisplayRow InchesFractional = new(key: (int)DimensionStyle.LengthDisplay.InchesFractional);
    public static readonly LengthDisplayRow FeetAndInches = new(key: (int)DimensionStyle.LengthDisplay.FeetAndInches);
    public static readonly LengthDisplayRow Millimeters = new(key: (int)DimensionStyle.LengthDisplay.Millmeters);
    public static readonly LengthDisplayRow Centimeters = new(key: (int)DimensionStyle.LengthDisplay.Centimeters);
    public static readonly LengthDisplayRow Meters = new(key: (int)DimensionStyle.LengthDisplay.Meters);
    public static readonly LengthDisplayRow Kilometers = new(key: (int)DimensionStyle.LengthDisplay.Kilometers);
    public static readonly LengthDisplayRow InchesDecimal = new(key: (int)DimensionStyle.LengthDisplay.InchesDecimal);
    public static readonly LengthDisplayRow FeetDecimal = new(key: (int)DimensionStyle.LengthDisplay.FeetDecimal);
    public static readonly LengthDisplayRow Miles = new(key: (int)DimensionStyle.LengthDisplay.Miles);

    internal DimensionStyle.LengthDisplay Host => (DimensionStyle.LengthDisplay)Key;
}
```

## [03]-[FIELD_SCHEMA]

- Owner: `FieldTable<TOwner, THostEnum>` is the row MECHANISM — the nine payload adapters, the one `Of<T>` seat, and the host-enum row admission — parameterized over the owner the delegates read and the host enum the rows key on, so a second drafting schema over a different host enum instantiates it instead of re-spelling nine adapters; `FieldSeat<TOwner>` is the row payload the mechanism mints; `StyleField` is that mechanism keyed to `DimensionStyle.Field`, each row carrying its axis with exact read, admission, and write delegates, while `StyleEdit` is the sole admitted field/payload pair.
- Owner: `EnumFamily` is the one CLR-enum admission memo: the `[Flags]` question, the composite mask, and the membership probe resolve ONCE per enum family at first touch and the family rides on the value, so a payload carries an admission WITNESS rather than a `Type` every reader re-tests and no admission runs reflection per call.
- Law: enum payloads carry their admitted family beside the value; each `Pick<TEnum>` row accepts only its exact family and a declared member before any host cast, and `[Flags]` rows admit composites `Enum.IsDefined` refuses through the family's own mask-subset test.
- Law: the write path is RESULT-TYPED end to end — a payload's host projection is a `Fin<T>`, so a colour leaving through the gamut boundary refuses at the field rather than inside a statement block the fold cannot see.
- Law: `StylePatch.Of` accumulates its edit admission while `Apply` stops on the first refused host write, and `Overlay` mints annotation overrides — the two folds answer different questions, so they carry different failure algebras.
- Law: color/plot-source `Field` cases from `ExtLineColorSource` through `DimLinePlotWeight_mm`, with `MaskFlags`, `SignedOrdinate`, and `UnitSystem`, carry no CLR property on `DimensionStyle`; `Name` and `Index` cannot inherit from a parent. `StyleField` excludes every non-property case, and the override census reports schema rows alone.
- Law: each host setter marks its own override field, while `MaskOffset` binds `Field.MaskBorder`.
- Law: `Field.LeaderContentAngle` is a shared slot — `LeaderContentAngleType` reads it as `GetInt` and `LeaderTextRotationRadians`/`LeaderTextRotationDegrees` as `GetDouble`, while `Field.LeaderContentAngleStyle` binds no accessor — so the schema carries exactly one row for the field, the angle-style enum, and the rotation double stays off-schema; a second row keyed on the same field value is a duplicate-key fault at vocabulary materialization.
- Law: `ToleranceZeroSuppress` is an inert host stub — its getter returns the constant `ZeroSuppression.None`, its setter body is empty, and no `Field` case backs it — so the tolerance axis excludes it and no patch can claim tolerance zero suppression.
- Law: `Overlay` duplicates a nil-id child against the annotation's bound style, applies the patch, and attaches through `SetOverrideDimStyle`.
- Law: `StyleAxis` is a READ axis, not a label — `StyleField.On(axis)` is the memoized roster every axis-scoped census, override clear, and standards patch reads, so an axis column no consumer selects on reads as decorative and an axis-scoped request never re-derives the grouping.
- Law: `DraftStandard` seats the ONE route a drafting standard reaches this schema by: the kernel `Drawing/sheet` ladders own every proportion — the ISO 3098-1 lettering height for the sheet extent, the `DraftingMetrics` derivations off `h` and `d`, the ISO 129-1 terminator size off the line group's wide rung, and the `DrawingPrecision` resolution off the scale and the sheet's declared units — and this page projects them onto its own rows. A free millimetre or a hand-typed decimal count anywhere in a `StylePatch` executes the deleted form; the schema rows carry no defaults of their own precisely because the standard owns them.
- Packages: `Rasm.Drawing` (`TextHeight.For`, `LetteringForm.Metrics`, `DraftingMetrics`, `LineGroup.For`, `Terminator.Size`, `DrawingScale`, `DrawingUnits.For`, `DrawingPrecision`, `DrawingPrecisionForm`, `SheetSize`), `Numerics/atoms` (`PerceptualColor.OfHost`/`ToDrawing`), `Domain/validation` (`FactoryBridge.Row`, `FactoryBridge.Accept`); Thinktecture.Runtime.Extensions; LanguageExt.Core; RhinoCommon `DimensionStyle` per `.api/api-rhinocommon-annotation.md`.
- Growth: a catalog-proven host config pairing is one row minted through its payload adapter; a second drafting schema is one `FieldTable` instantiation; a standards clause is one `DraftStandard` edit row. Every patch, snapshot, and census gains each without another operation surface, and each lands beside the rest.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class StyleAxis {
    public static readonly StyleAxis Arrow = new(key: 0);
    public static readonly StyleAxis Text = new(key: 1);
    public static readonly StyleAxis Length = new(key: 2);
    public static readonly StyleAxis Tolerance = new(key: 3);
    public static readonly StyleAxis Mask = new(key: 4);
    public static readonly StyleAxis Layout = new(key: 5);
    public static readonly StyleAxis Leader = new(key: 6);
}

public sealed record EnumFamily {
    private EnumFamily(string name, bool combinable, ulong mask, Func<Enum, bool> defined) {
        Name = name;
        Combinable = combinable;
        Mask = mask;
        Defined = defined;
    }

    public string Name { get; }
    public bool Combinable { get; }
    private ulong Mask { get; }
    private Func<Enum, bool> Defined { get; }

    public bool Admits(Enum member) => Combinable ? (Bits(member) & ~Mask) is 0UL : Defined(member);

    public static EnumFamily For<TEnum>() where TEnum : struct, Enum => Seated<TEnum>.Row;

    private static ulong Bits(Enum member) => Convert.ToUInt64(member, CultureInfo.InvariantCulture);

    private static class Seated<TEnum> where TEnum : struct, Enum {
        internal static readonly EnumFamily Row = new(
            name: typeof(TEnum).FullName ?? typeof(TEnum).Name,
            combinable: typeof(TEnum).IsDefined(typeof(FlagsAttribute), inherit: false),
            mask: Enum.GetValues<TEnum>().Aggregate(0UL, static (bits, member) => bits | Bits(member)),
            defined: static member => member is TEnum typed && Enum.IsDefined(typed));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleValue {
    private StyleValue() { }
    public sealed record Real(double Value) : StyleValue;
    public sealed record Whole(int Value) : StyleValue;
    public sealed record Choice : StyleValue {
        internal Choice(Enum value, EnumFamily family) {
            Value = value;
            Family = family;
        }

        public Enum Value { get; }
        public EnumFamily Family { get; }
    }
    public sealed record Flag(bool Value) : StyleValue;
    public sealed record Script(string Value) : StyleValue;
    public sealed record Tint(PerceptualColor Value) : StyleValue;
    public sealed record Anchor(Option<ResourceId> Value) : StyleValue;
    public sealed record Face(Font Value) : StyleValue;
    public sealed record Glyph(char Value) : StyleValue;

    public static StyleValue Of<TEnum>(TEnum value) where TEnum : struct, Enum =>
        new Choice(value: value, family: EnumFamily.For<TEnum>());
}

public readonly record struct FieldSeat<TOwner>(
    Func<StyleValue, bool> Accepts,
    Func<TOwner, Fin<StyleValue>> Read,
    Func<TOwner, StyleValue, Fin<Unit>> Write) where TOwner : class;

public static class FieldTable<TOwner, THostEnum>
    where TOwner : class
    where THostEnum : struct, Enum {
    public static FieldSeat<TOwner> Real(Func<TOwner, double> get, Action<TOwner, double> set) =>
        Of(get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Real(Value: value)),
            static (value, _) => Fin.Succ(value: ((StyleValue.Real)value).Value),
            static value => value is StyleValue.Real scalar && double.IsFinite(scalar.Value));

    public static FieldSeat<TOwner> Whole(Func<TOwner, int> get, Action<TOwner, int> set) =>
        Of(get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Whole(Value: value)),
            static (value, _) => Fin.Succ(value: ((StyleValue.Whole)value).Value),
            static value => value is StyleValue.Whole);

    public static FieldSeat<TOwner> Pick<TEnum>(Func<TOwner, TEnum> get, Action<TOwner, TEnum> set)
        where TEnum : struct, Enum =>
        Of(get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: StyleValue.Of(value)),
            static (value, key) => value is StyleValue.Choice { Value: TEnum member }
                ? Fin.Succ(value: member)
                : Fin.Fail<TEnum>(error: new KernelFault.InvalidInput()),
            static value => value is StyleValue.Choice { Value: TEnum member } choice
                && choice.Family == EnumFamily.For<TEnum>()
                && choice.Family.Admits(member));

    public static FieldSeat<TOwner> Flag(Func<TOwner, bool> get, Action<TOwner, bool> set) =>
        Of(get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Flag(Value: value)),
            static (value, _) => Fin.Succ(value: ((StyleValue.Flag)value).Value),
            static value => value is StyleValue.Flag);

    public static FieldSeat<TOwner> Script(Func<TOwner, string> get, Action<TOwner, string> set) =>
        Of(get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Script(Value: value)),
            static (value, _) => Fin.Succ(value: ((StyleValue.Script)value).Value),
            static value => value is StyleValue.Script { Value: not null });

    public static FieldSeat<TOwner> Tint(
        Func<TOwner, System.Drawing.Color> get, Action<TOwner, System.Drawing.Color> set) =>
        Of(get, set,
            static (value, key) => PerceptualColor.OfHost(host: value)
                .Map(static color => (StyleValue)new StyleValue.Tint(Value: color)),
            static (value, key) => ((StyleValue.Tint)value).Value.ToDrawing(),
            static value => value is StyleValue.Tint);

    public static FieldSeat<TOwner> Anchor(Func<TOwner, Guid> get, Action<TOwner, Guid> set) =>
        Of(get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Anchor(
                Value: Optional(value).Filter(static id => id != Guid.Empty).Map(ResourceId.Create))),
            static (value, _) => Fin.Succ(
                value: ((StyleValue.Anchor)value).Value.Map(static id => id.Value).IfNone(Guid.Empty)),
            static value => value is StyleValue.Anchor);

    public static FieldSeat<TOwner> Face(Func<TOwner, Font> get, Action<TOwner, Font> set) =>
        Of(get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Face(Value: value)),
            static (value, _) => Fin.Succ(value: ((StyleValue.Face)value).Value),
            static value => value is StyleValue.Face { Value: not null });

    public static FieldSeat<TOwner> Glyph(Func<TOwner, char> get, Action<TOwner, char> set) =>
        Of(get, set,
            static (value, _) => Fin.Succ<StyleValue>(value: new StyleValue.Glyph(Value: value)),
            static (value, _) => Fin.Succ(value: ((StyleValue.Glyph)value).Value),
            static value => value is StyleValue.Glyph);

    public static FieldSeat<TOwner> Of<T>(
        Func<TOwner, T> get,
        Action<TOwner, T> set,
        Func<T, Fin<StyleValue>> wrap,
        Func<StyleValue, Fin<T>> unwrap,
        Func<StyleValue, bool> accepts) =>
        new(
            Accepts: accepts,
            Read: (owner, key) =>
                from value in Try.lift(() => wrap(get(owner))).Run().Bind(static inner => inner)
                from _ in guard(accepts(value), new KernelFault.InvalidResult())
                select value,
            Write: (owner, value, key) =>
                from typed in unwrap(value, key)
                from _ in Try.lift(() => HostEdge.Side(() => set(owner, typed))).Run()
                select unit);

    public static Fin<TRow> Row<TRow>(THostEnum field)
        where TRow : class, ISmartEnum<int, TRow, ValidationError> =>
        FactoryBridge.Row<THostEnum, TRow>(
            candidate: field, ordinal: static value => Convert.ToInt32(value, CultureInfo.InvariantCulture));
}

[SmartEnum<int>]
public sealed partial class StyleField {
    // --- [ARROW]
    public static readonly StyleField ArrowType1 = Pick(DimensionStyle.Field.ArrowType1, StyleAxis.Arrow, static s => s.ArrowType1, static (s, v) => s.ArrowType1 = v);
    public static readonly StyleField ArrowType2 = Pick(DimensionStyle.Field.ArrowType2, StyleAxis.Arrow, static s => s.ArrowType2, static (s, v) => s.ArrowType2 = v);
    public static readonly StyleField LeaderArrowType = Pick(DimensionStyle.Field.LeaderArrowType, StyleAxis.Arrow, static s => s.LeaderArrowType, static (s, v) => s.LeaderArrowType = v);
    public static readonly StyleField ArrowLength = Real(DimensionStyle.Field.Arrowsize, StyleAxis.Arrow, static s => s.ArrowLength, static (s, v) => s.ArrowLength = v);
    public static readonly StyleField LeaderArrowLength = Real(DimensionStyle.Field.LeaderArrowsize, StyleAxis.Arrow, static s => s.LeaderArrowLength, static (s, v) => s.LeaderArrowLength = v);
    public static readonly StyleField ArrowBlockId1 = Anchor(DimensionStyle.Field.ArrowBlockId1, StyleAxis.Arrow, static s => s.ArrowBlockId1, static (s, v) => s.ArrowBlockId1 = v);
    public static readonly StyleField ArrowBlockId2 = Anchor(DimensionStyle.Field.ArrowBlockId2, StyleAxis.Arrow, static s => s.ArrowBlockId2, static (s, v) => s.ArrowBlockId2 = v);
    public static readonly StyleField LeaderArrowBlockId = Anchor(DimensionStyle.Field.LeaderArrowBlock, StyleAxis.Arrow, static s => s.LeaderArrowBlockId, static (s, v) => s.LeaderArrowBlockId = v);
    public static readonly StyleField ClippingArrowType1 = Pick(DimensionStyle.Field.ClippingArrowType1, StyleAxis.Arrow, static s => s.ClippingArrowType1, static (s, v) => s.ClippingArrowType1 = v);
    public static readonly StyleField ClippingArrowType2 = Pick(DimensionStyle.Field.ClippingArrowType2, StyleAxis.Arrow, static s => s.ClippingArrowType2, static (s, v) => s.ClippingArrowType2 = v);
    public static readonly StyleField ClippingArrowLength = Real(DimensionStyle.Field.ClippingArrowSize, StyleAxis.Arrow, static s => s.ClippingArrowLength, static (s, v) => s.ClippingArrowLength = v);
    public static readonly StyleField FitArrow = Pick(DimensionStyle.Field.ArrowFit, StyleAxis.Arrow, static s => s.FitArrow, static (s, v) => s.FitArrow = v);
    public static readonly StyleField SuppressArrow1 = Flag(DimensionStyle.Field.SuppressArrow1, StyleAxis.Arrow, static s => s.SuppressArrow1, static (s, v) => s.SuppressArrow1 = v);
    public static readonly StyleField SuppressArrow2 = Flag(DimensionStyle.Field.SuppressArrow2, StyleAxis.Arrow, static s => s.SuppressArrow2, static (s, v) => s.SuppressArrow2 = v);
    // --- [TEXT]
    public static readonly StyleField TextHeight = Real(DimensionStyle.Field.TextHeight, StyleAxis.Text, static s => s.TextHeight, static (s, v) => s.TextHeight = v);
    public static readonly StyleField TextGap = Real(DimensionStyle.Field.TextGap, StyleAxis.Text, static s => s.TextGap, static (s, v) => s.TextGap = v);
    public static readonly StyleField TextRotation = Real(DimensionStyle.Field.TextRotation, StyleAxis.Text, static s => s.TextRotation, static (s, v) => s.TextRotation = v);
    public static readonly StyleField TypeFace = Face(DimensionStyle.Field.Font, StyleAxis.Text, static s => s.Font, static (s, v) => s.Font = v);
    public static readonly StyleField TextVerticalAlignment = Pick(DimensionStyle.Field.TextVerticalAlignment, StyleAxis.Text, static s => s.TextVerticalAlignment, static (s, v) => s.TextVerticalAlignment = v);
    public static readonly StyleField TextHorizontalAlignment = Pick(DimensionStyle.Field.TextHorizontalAlignment, StyleAxis.Text, static s => s.TextHorizontalAlignment, static (s, v) => s.TextHorizontalAlignment = v);
    public static readonly StyleField TextOrientation = Pick(DimensionStyle.Field.TextOrientation, StyleAxis.Text, static s => s.TextOrientation, static (s, v) => s.TextOrientation = v);
    public static readonly StyleField LeaderTextOrientation = Pick(DimensionStyle.Field.LeaderTextOrientation, StyleAxis.Text, static s => s.LeaderTextOrientation, static (s, v) => s.LeaderTextOrientation = v);
    public static readonly StyleField DimTextOrientation = Pick(DimensionStyle.Field.DimTextOrientation, StyleAxis.Text, static s => s.DimTextOrientation, static (s, v) => s.DimTextOrientation = v);
    public static readonly StyleField DimRadialTextOrientation = Pick(DimensionStyle.Field.DimRadialTextOrientation, StyleAxis.Text, static s => s.DimRadialTextOrientation, static (s, v) => s.DimRadialTextOrientation = v);
    public static readonly StyleField DimTextLocation = Pick(DimensionStyle.Field.DimTextLocation, StyleAxis.Text, static s => s.DimTextLocation, static (s, v) => s.DimTextLocation = v);
    public static readonly StyleField DimRadialTextLocation = Pick(DimensionStyle.Field.DimRadialTextLocation, StyleAxis.Text, static s => s.DimRadialTextLocation, static (s, v) => s.DimRadialTextLocation = v);
    public static readonly StyleField DimTextAngleType = Pick(DimensionStyle.Field.DimTextAngleStyle, StyleAxis.Text, static s => s.DimTextAngleType, static (s, v) => s.DimTextAngleType = v);
    public static readonly StyleField DimRadialTextAngleType = Pick(DimensionStyle.Field.DimRadialTextAngleStyle, StyleAxis.Text, static s => s.DimRadialTextAngleType, static (s, v) => s.DimRadialTextAngleType = v);
    public static readonly StyleField FitText = Pick(DimensionStyle.Field.TextFit, StyleAxis.Text, static s => s.FitText, static (s, v) => s.FitText = v);
    public static readonly StyleField UseKerning = Flag(DimensionStyle.Field.Kerning, StyleAxis.Text, static s => s.UseKerning, static (s, v) => s.UseKerning = v);
    public static readonly StyleField TextUnderlined = Flag(DimensionStyle.Field.TextUnderlined, StyleAxis.Text, static s => s.TextUnderlined, static (s, v) => s.TextUnderlined = v);
    public static readonly StyleField LineSpaceScale = Real(DimensionStyle.Field.LineSpaceScale, StyleAxis.Text, static s => s.LineSpaceScale, static (s, v) => s.LineSpaceScale = v);
    public static readonly StyleField DrawForward = Flag(DimensionStyle.Field.DrawForward, StyleAxis.Text, static s => s.DrawForward, static (s, v) => s.DrawForward = v);
    public static readonly StyleField DecimalSeparator = Glyph(DimensionStyle.Field.DecimalSeparator, StyleAxis.Text, static s => s.DecimalSeparator, static (s, v) => s.DecimalSeparator = v);
    // --- [LENGTH]
    public static readonly StyleField LengthFactor = Real(DimensionStyle.Field.LengthFactor, StyleAxis.Length, static s => s.LengthFactor, static (s, v) => s.LengthFactor = v);
    public static readonly StyleField AlternateLengthFactor = Real(DimensionStyle.Field.AlternateLengthFactor, StyleAxis.Length, static s => s.AlternateLengthFactor, static (s, v) => s.AlternateLengthFactor = v);
    public static readonly StyleField LengthResolution = Whole(DimensionStyle.Field.LengthResolution, StyleAxis.Length, static s => s.LengthResolution, static (s, v) => s.LengthResolution = v);
    public static readonly StyleField AlternateLengthResolution = Whole(DimensionStyle.Field.AlternateLengthResolution, StyleAxis.Length, static s => s.AlternateLengthResolution, static (s, v) => s.AlternateLengthResolution = v);
    public static readonly StyleField AngleResolution = Whole(DimensionStyle.Field.AngleResolution, StyleAxis.Length, static s => s.AngleResolution, static (s, v) => s.AngleResolution = v);
    public static readonly StyleField DimensionLengthDisplay = Pick(DimensionStyle.Field.DimensionLengthDisplay, StyleAxis.Length, static s => s.DimensionLengthDisplay, static (s, v) => s.DimensionLengthDisplay = v);
    public static readonly StyleField AlternateDimensionLengthDisplay = Pick(DimensionStyle.Field.AlternateDimensionLengthDisplay, StyleAxis.Length, static s => s.AlternateDimensionLengthDisplay, static (s, v) => s.AlternateDimensionLengthDisplay = v);
    public static readonly StyleField AngleFormat = Pick(DimensionStyle.Field.AngleFormat, StyleAxis.Length, static s => s.AngleFormat, static (s, v) => s.AngleFormat = v);
    public static readonly StyleField Roundoff = Real(DimensionStyle.Field.Round, StyleAxis.Length, static s => s.Roundoff, static (s, v) => s.Roundoff = v);
    public static readonly StyleField AlternateRoundoff = Real(DimensionStyle.Field.AltRound, StyleAxis.Length, static s => s.AlternateRoundoff, static (s, v) => s.AlternateRoundoff = v);
    public static readonly StyleField AngularRoundoff = Real(DimensionStyle.Field.AngularRound, StyleAxis.Length, static s => s.AngularRoundoff, static (s, v) => s.AngularRoundoff = v);
    public static readonly StyleField ZeroSuppress = Pick(DimensionStyle.Field.ZeroSuppress, StyleAxis.Length, static s => s.ZeroSuppress, static (s, v) => s.ZeroSuppress = v);
    public static readonly StyleField AlternateZeroSuppress = Pick(DimensionStyle.Field.AltZeroSuppress, StyleAxis.Length, static s => s.AlternateZeroSuppress, static (s, v) => s.AlternateZeroSuppress = v);
    public static readonly StyleField AngleZeroSuppress = Pick(DimensionStyle.Field.AngleZeroSuppress, StyleAxis.Length, static s => s.AngleZeroSuppress, static (s, v) => s.AngleZeroSuppress = v);
    public static readonly StyleField Prefix = Script(DimensionStyle.Field.Prefix, StyleAxis.Length, static s => s.Prefix, static (s, v) => s.Prefix = v);
    public static readonly StyleField Suffix = Script(DimensionStyle.Field.Suffix, StyleAxis.Length, static s => s.Suffix, static (s, v) => s.Suffix = v);
    public static readonly StyleField AlternatePrefix = Script(DimensionStyle.Field.AlternatePrefix, StyleAxis.Length, static s => s.AlternatePrefix, static (s, v) => s.AlternatePrefix = v);
    public static readonly StyleField AlternateSuffix = Script(DimensionStyle.Field.AlternateSuffix, StyleAxis.Length, static s => s.AlternateSuffix, static (s, v) => s.AlternateSuffix = v);
    public static readonly StyleField StackFractionFormat = Pick(DimensionStyle.Field.StackFormat, StyleAxis.Length, static s => s.StackFractionFormat, static (s, v) => s.StackFractionFormat = v);
    public static readonly StyleField StackHeightScale = Real(DimensionStyle.Field.StackTextheightScale, StyleAxis.Length, static s => s.StackHeightScale, static (s, v) => s.StackHeightScale = v);
    public static readonly StyleField AlternateUnitsDisplay = Flag(DimensionStyle.Field.Alternate, StyleAxis.Length, static s => s.AlternateUnitsDisplay, static (s, v) => s.AlternateUnitsDisplay = v);
    public static readonly StyleField AlternateBelowLine = Flag(DimensionStyle.Field.AltBelow, StyleAxis.Length, static s => s.AlternateBelowLine, static (s, v) => s.AlternateBelowLine = v);
    // --- [TOLERANCE]
    public static readonly StyleField ToleranceFormat = Pick(DimensionStyle.Field.ToleranceFormat, StyleAxis.Tolerance, static s => s.ToleranceFormat, static (s, v) => s.ToleranceFormat = v);
    public static readonly StyleField ToleranceResolution = Whole(DimensionStyle.Field.ToleranceResolution, StyleAxis.Tolerance, static s => s.ToleranceResolution, static (s, v) => s.ToleranceResolution = v);
    public static readonly StyleField AlternateToleranceResolution = Whole(DimensionStyle.Field.AltToleranceResolution, StyleAxis.Tolerance, static s => s.AlternateToleranceResolution, static (s, v) => s.AlternateToleranceResolution = v);
    public static readonly StyleField ToleranceHeightScale = Real(DimensionStyle.Field.ToleranceHeightScale, StyleAxis.Tolerance, static s => s.ToleranceHeightScale, static (s, v) => s.ToleranceHeightScale = v);
    public static readonly StyleField ToleranceUpperValue = Real(DimensionStyle.Field.ToleranceUpperValue, StyleAxis.Tolerance, static s => s.ToleranceUpperValue, static (s, v) => s.ToleranceUpperValue = v);
    public static readonly StyleField ToleranceLowerValue = Real(DimensionStyle.Field.ToleranceLowerValue, StyleAxis.Tolerance, static s => s.ToleranceLowerValue, static (s, v) => s.ToleranceLowerValue = v);
    // --- [MASK]
    public static readonly StyleField DrawTextMask = Flag(DimensionStyle.Field.DrawMask, StyleAxis.Mask, static s => s.DrawTextMask, static (s, v) => s.DrawTextMask = v);
    public static readonly StyleField MaskColor = Tint(DimensionStyle.Field.MaskColor, StyleAxis.Mask, static s => s.MaskColor, static (s, v) => s.MaskColor = v);
    public static readonly StyleField MaskColorSource = Pick(DimensionStyle.Field.MaskColorSource, StyleAxis.Mask, static s => s.MaskColorSource, static (s, v) => s.MaskColorSource = v);
    public static readonly StyleField MaskFrameType = Pick(DimensionStyle.Field.MaskFrameType, StyleAxis.Mask, static s => s.MaskFrameType, static (s, v) => s.MaskFrameType = v);
    public static readonly StyleField MaskOffset = Real(DimensionStyle.Field.MaskBorder, StyleAxis.Mask, static s => s.MaskOffset, static (s, v) => s.MaskOffset = v);
    // --- [LAYOUT]
    public static readonly StyleField BaselineSpacing = Real(DimensionStyle.Field.BaselineSpacing, StyleAxis.Layout, static s => s.BaselineSpacing, static (s, v) => s.BaselineSpacing = v);
    public static readonly StyleField DimensionScale = Real(DimensionStyle.Field.DimensionScale, StyleAxis.Layout, static s => s.DimensionScale, static (s, v) => s.DimensionScale = v);
    public static readonly StyleField CentermarkSize = Real(DimensionStyle.Field.Centermark, StyleAxis.Layout, static s => s.CentermarkSize, static (s, v) => s.CentermarkSize = v);
    public static readonly StyleField CenterMarkType = Pick(DimensionStyle.Field.CentermarkStyle, StyleAxis.Layout, static s => s.CenterMarkType, static (s, v) => s.CenterMarkType = v);
    public static readonly StyleField ExtensionLineExtension = Real(DimensionStyle.Field.ExtensionLineExtension, StyleAxis.Layout, static s => s.ExtensionLineExtension, static (s, v) => s.ExtensionLineExtension = v);
    public static readonly StyleField ExtensionLineOffset = Real(DimensionStyle.Field.ExtensionLineOffset, StyleAxis.Layout, static s => s.ExtensionLineOffset, static (s, v) => s.ExtensionLineOffset = v);
    public static readonly StyleField DimensionLineExtension = Real(DimensionStyle.Field.DimensionLineExtension, StyleAxis.Layout, static s => s.DimensionLineExtension, static (s, v) => s.DimensionLineExtension = v);
    public static readonly StyleField SuppressExtension1 = Flag(DimensionStyle.Field.SuppressExtension1, StyleAxis.Layout, static s => s.SuppressExtension1, static (s, v) => s.SuppressExtension1 = v);
    public static readonly StyleField SuppressExtension2 = Flag(DimensionStyle.Field.SuppressExtension2, StyleAxis.Layout, static s => s.SuppressExtension2, static (s, v) => s.SuppressExtension2 = v);
    public static readonly StyleField FixedExtensionOn = Flag(DimensionStyle.Field.FixedExtensionOn, StyleAxis.Layout, static s => s.FixedExtensionOn, static (s, v) => s.FixedExtensionOn = v);
    public static readonly StyleField FixedExtensionLength = Real(DimensionStyle.Field.FixedExtensionLength, StyleAxis.Layout, static s => s.FixedExtensionLength, static (s, v) => s.FixedExtensionLength = v);
    public static readonly StyleField ForceDimensionLineBetweenExtensionLines = Flag(DimensionStyle.Field.ForceDimLine, StyleAxis.Layout, static s => s.ForceDimensionLineBetweenExtensionLines, static (s, v) => s.ForceDimensionLineBetweenExtensionLines = v);
    public static readonly StyleField TextMoveLeader = Whole(DimensionStyle.Field.TextmoveLeader, StyleAxis.Layout, static s => s.TextMoveLeader, static (s, v) => s.TextMoveLeader = v);
    public static readonly StyleField ArcLengthSymbol = Whole(DimensionStyle.Field.ArclengthSymbol, StyleAxis.Layout, static s => s.ArcLengthSymbol, static (s, v) => s.ArcLengthSymbol = v);
    // --- [LEADER]
    public static readonly StyleField LeaderHasLanding = Flag(DimensionStyle.Field.LeaderHasLanding, StyleAxis.Leader, static s => s.LeaderHasLanding, static (s, v) => s.LeaderHasLanding = v);
    public static readonly StyleField LeaderLandingLength = Real(DimensionStyle.Field.LeaderLandingLength, StyleAxis.Leader, static s => s.LeaderLandingLength, static (s, v) => s.LeaderLandingLength = v);
    public static readonly StyleField LeaderContentAngleType = Pick(DimensionStyle.Field.LeaderContentAngle, StyleAxis.Leader, static s => s.LeaderContentAngleType, static (s, v) => s.LeaderContentAngleType = v);
    public static readonly StyleField LeaderCurveType = Pick(DimensionStyle.Field.LeaderCurveType, StyleAxis.Leader, static s => s.LeaderCurveType, static (s, v) => s.LeaderCurveType = v);
    public static readonly StyleField LeaderTextVerticalAlignment = Pick(DimensionStyle.Field.LeaderTextVerticalAlignment, StyleAxis.Leader, static s => s.LeaderTextVerticalAlignment, static (s, v) => s.LeaderTextVerticalAlignment = v);
    public static readonly StyleField LeaderTextHorizontalAlignment = Pick(DimensionStyle.Field.LeaderTextHorizontalAlignment, StyleAxis.Leader, static s => s.LeaderTextHorizontalAlignment, static (s, v) => s.LeaderTextHorizontalAlignment = v);

    public StyleAxis Axis { get; }

    internal DimensionStyle.Field Host => (DimensionStyle.Field)Key;

    [UseDelegateFromConstructor]
    internal partial bool Accepts(StyleValue value);

    [UseDelegateFromConstructor]
    internal partial Fin<StyleValue> Read(DimensionStyle style);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Write(DimensionStyle style, StyleValue value);

    public static Seq<StyleField> On(StyleAxis axis) => ByAxis.Value[axis];

    private static readonly Lazy<FrozenDictionary<StyleAxis, Seq<StyleField>>> ByAxis = new(static () =>
        toSeq(Items).GroupBy(static row => row.Axis)
            .ToFrozenDictionary(static group => group.Key, static group => toSeq(group).Strict()));

    private static StyleField Real(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, double> get, Action<DimensionStyle, double> set) =>
        new(key: (int)field, axis: axis, seat: Fields.Real(get, set));

    private static StyleField Whole(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, int> get, Action<DimensionStyle, int> set) =>
        new(key: (int)field, axis: axis, seat: Fields.Whole(get, set));

    private static StyleField Pick<TEnum>(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, TEnum> get, Action<DimensionStyle, TEnum> set)
        where TEnum : struct, Enum =>
        new(key: (int)field, axis: axis, seat: Fields.Pick(get, set));

    private static StyleField Flag(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, bool> get, Action<DimensionStyle, bool> set) =>
        new(key: (int)field, axis: axis, seat: Fields.Flag(get, set));

    private static StyleField Script(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, string> get, Action<DimensionStyle, string> set) =>
        new(key: (int)field, axis: axis, seat: Fields.Script(get, set));

    private static StyleField Tint(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, System.Drawing.Color> get, Action<DimensionStyle, System.Drawing.Color> set) =>
        new(key: (int)field, axis: axis, seat: Fields.Tint(get, set));

    private static StyleField Anchor(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, Guid> get, Action<DimensionStyle, Guid> set) =>
        new(key: (int)field, axis: axis, seat: Fields.Anchor(get, set));

    private static StyleField Face(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, Font> get, Action<DimensionStyle, Font> set) =>
        new(key: (int)field, axis: axis, seat: Fields.Face(get, set));

    private static StyleField Glyph(DimensionStyle.Field field, StyleAxis axis, Func<DimensionStyle, char> get, Action<DimensionStyle, char> set) =>
        new(key: (int)field, axis: axis, seat: Fields.Glyph(get, set));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StyleEdit {
    private StyleEdit(StyleField field, StyleValue value) {
        Field = field;
        Value = value;
    }

    public StyleField Field { get; }
    public StyleValue Value { get; }

    public static Fin<StyleEdit> Of(StyleField field, StyleValue value) {
        return guard(field.Accepts(value: value), new KernelFault.InvalidInput()).ToFin()
            .Map(_ => new StyleEdit(field: field, value: value));
    }
}

public sealed record StylePatch {
    private StylePatch(Seq<StyleEdit> edits) => Edits = edits;

    public Seq<StyleEdit> Edits { get; }

    public static Fin<StylePatch> Of(params ReadOnlySpan<StyleEdit> edits) =>
        Of(run: LanguageExt.Iterable<StyleEdit>.FromSpan(edits).ToSeq());

    public static Fin<StylePatch> Of(Seq<StyleEdit> run) {
        return from admitted in run.Traverse(edit => Admit.Value(value: edit).ToValidation()).As().ToFin()
               from _ in guard(!admitted.IsEmpty, new KernelFault.InvalidInput())
               select new StylePatch(edits: admitted);
    }

    public Fin<StylePatch> Within(StyleAxis axis) =>
        Of(run: Edits.Filter(edit => edit.Field.Axis == axis));

    internal Fin<Unit> Apply(DimensionStyle style) =>
        Edits.TraverseM(edit => edit.Field.Write(style: style, value: edit.Value)).As().Map(static _ => unit);

    internal Fin<DimensionStyle> Overlay(AnnotationBase annotation) =>
        from parent in Optional(annotation.ParentDimensionStyle).ToFin(Fail: new KernelFault.MissingContext())
        from child in Try.lift(() => parent.Duplicate(
            newName: string.Empty, newId: Guid.Empty, newParentId: annotation.DimensionStyleId)).Run()
        from _ in Apply(style: child)
            .Rollback(release: () => Custody.Dispose(held: Seq(child)))
        from attached in Admit.Confirm(success: annotation.SetOverrideDimStyle(overrideStyle: child))
            .Rollback(release: () => Custody.Dispose(held: Seq(child)))
        select child;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DraftStandard {
    public static Fin<StylePatch> Patch(SheetSize size, LetteringForm form, DrawingScale scale) {
        return from height in TextHeight.For(size: size)
               from group in LineGroup.For(size: size)
               let resolution = new DrawingPrecision(Scale: scale, Units: DrawingUnits.For(standard: size.Standard)).Form()
               let metrics = form.Metrics(height: height)
               let arrow = Terminator.ClosedArrow.Size(width: group.Wide)
               let places = resolution.Switch(
                   places: static count => count,
                   fraction: static denominator => (int)Math.Log2(denominator))
               from edits in Seq(
                   (Field: StyleField.TextHeight, Value: (StyleValue)new StyleValue.Real(metrics.Height.Height.Millimeters)),
                   (StyleField.TextGap, new StyleValue.Real(metrics.CharacterSpacing.Millimeters)),
                   (StyleField.LineSpaceScale, new StyleValue.Real(form.PitchRatio)),
                   (StyleField.StackHeightScale, new StyleValue.Real(form.LowerCaseRatio)),
                   (StyleField.BaselineSpacing, new StyleValue.Real(metrics.LinePitch.Millimeters)),
                   (StyleField.MaskOffset, new StyleValue.Real(metrics.FramePad.Millimeters)),
                   (StyleField.ArrowLength, new StyleValue.Real(arrow.Millimeters)),
                   (StyleField.LeaderArrowLength, new StyleValue.Real(arrow.Millimeters)),
                   (StyleField.ClippingArrowLength, new StyleValue.Real(arrow.Millimeters)),
                   (StyleField.ExtensionLineOffset, new StyleValue.Real(metrics.ExtensionGap.Millimeters)),
                   (StyleField.ExtensionLineExtension, new StyleValue.Real(metrics.ExtensionOvershoot.Millimeters)),
                   (StyleField.CentermarkSize, new StyleValue.Real(metrics.CentreMarkGap.Millimeters)),
                   (StyleField.LeaderLandingLength, new StyleValue.Real(metrics.WordSpacing.Millimeters)),
                   (StyleField.DimensionScale, new StyleValue.Real(scale.Ratio)),
                   (StyleField.LengthResolution, new StyleValue.Whole(places)),
                   (StyleField.AlternateLengthResolution, new StyleValue.Whole(places)),
                   (StyleField.AngleResolution, new StyleValue.Whole(places)))
                   .Traverse(row => StyleEdit.Of(field: row.Field, value: row.Value).ToValidation()).As().ToFin()
               from patch in StylePatch.Of(run: edits)
               select patch;
    }
}
```

## [04]-[STYLE_PIPELINE]

- Owner: `AnnotationStyleOp` is the per-annotation override pair — overlay a patch, or clear every property override — shared verbatim by the dimension and text programs; `StyleDef` is the authorable dimension-style aggregate — name, patch, and optional parentage — the grip mints and revises; `StyleOp` `[Union]` carries the shared table verbs as ONE `Table` case beside the six verbs no other component table has: whole-setting copy, override clearing, reverse absorption, reparenting, length scaling, and the paper/model scale faces; `DraftPlan<StyleOp>` is the admitted commit plan; `Styles` the `Commit`/`Ask` entry pair.
- Law: an amendment never mutates the resolved live component — every write duplicates it, applies its change to the copy, and lands through `DimStyleTable.Modify` by index inside the shared undo bracket, the `TableGrip` law this page owns.
- Law: `Author` refuses an occupied name, shapes the detached style through the grip's mint row, and performs one terminal `Add`; a parent payload makes the authored style a child whose patch-marked fields alone override the parent through `ParentId`.
- Law: `DraftPlan<TOp>.Of` admits its mode and every operation before the shared commit spine can enter a document grant; `DraftMode` carries the redraw posture and the undo CUSTODY row the session's need table reads, so a commit never states its undo axis as a loose bool beside a policy.
- Law: every plural ADMISSION fold in the namespace accumulates — `Traverse` onto `Validation`, then back to `Fin` — so a rejected batch reports its whole refusal set; the fail-fast `TraverseM` shape is reserved for plural HOST WRITES, where a later write must never run after an earlier one refused.
- Law: `Absorb` is the one reverse projection — `DimStyleTable.Modify(style, annotation)` folds a live annotation's per-instance overrides back onto the style, accepting `Modify` and `Override` while `NotSaved` is a typed refusal.
- Law: `Copy` projects every source setting through `DimensionStyle.CopyFrom` while preserving the target name, id, and index; `ClearOverrides` names its scope as the field run itself, and `StyleField.On(axis)` is how a caller spells a whole axis without a second request shape.
- Law: reclamation is not a case — unused-style reclaim is the document pipeline's `TableOp.Reclaim(TableKind.DimStyles)` row, and re-spelling it here splits one host member across two owners.
- Law: the write posture is the spine's `HostInteraction`, carried by every drafting op case in the namespace — style, linetype, hatch, and section alike. The axis is exactly quiet-versus-interactive-versus-designed-silence and the spine already owns it, so a folder-local vocabulary over the same host `quiet` boolean held one concept under two names that drift apart.
- Law: every scale a case carries is a `DraftScale`, admitted at its own gate — a positivity guard inside an arm re-decides at the write what the owner already decided at admission.
- Packages: `Document/commit.md` (`DocumentCommit.Sealed`, `RedrawPolicy`, `HostInteraction`), `Document/session.md` (`SessionNeed.Mutation`, `UndoCustody`, `DocumentSession.Demand`); RhinoCommon `DimStyleTable` per `.api/api-rhinocommon-annotation.md`.
- Growth: a style-only verb is one case with its arm; a verb every component table shares is one `TableOp` case; the spine and consumers stay unchanged.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class StyleDef {
    public ResourceName Name { get; }
    public StylePatch Patch { get; }
    public Option<ResourceId> Parent { get; }

    internal Fin<Unit> Apply(DimensionStyle style) =>
        from _ in Try.lift(() => HostEdge.Side(() => style.Name = Name.Value)).Run()
        from __ in Try.lift(() => HostEdge.Side(() =>
            style.ParentId = Parent.Map(static parent => parent.Value).IfNone(noneValue: Guid.Empty))).Run()
        from ___ in Patch.Apply(style: style)
        select unit;
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleOp {
    private StyleOp() { }
    public sealed record Table(TableOp<DimensionStyle, StyleDef> Verb) : StyleOp;
    public sealed record Copy(ResourceRef Target, ResourceRef Source, HostInteraction Interaction) : StyleOp;
    public sealed record ClearOverrides(ResourceRef Target, Seq<StyleField> Fields, HostInteraction Interaction) : StyleOp;
    public sealed record Absorb(ResourceRef Target, TableTarget Annotation) : StyleOp;
    public sealed record Reparent(ResourceRef Target, Option<ResourceId> Parent, HostInteraction Interaction) : StyleOp;
    public sealed record ScaleLengths(ResourceRef Target, DraftScale Factor, HostInteraction Interaction) : StyleOp;
    public sealed record PageScale(ResourceRef Target, DraftScale Left, DraftScale Right, HostInteraction Interaction) : StyleOp;

    internal static readonly ResourceLens<DimensionStyle> Lens = new(
        ById: static (document, id) => document.DimStyles.Find(styleId: id, ignoreDeleted: true),
        ByName: static (document, name) => document.DimStyles.FindName(name: name),
        ByIndex: static (document, index) => document.DimStyles.FindIndex(index: index));

    internal static readonly TableGrip<DimensionStyle, StyleDef> Grip = new(
        Lens,
        Named: static def => def.Name,
        Title: static (style, key) => FactoryBridge.Accept<ResourceName>(candidate: style.Name),
        Index: static style => style.Index,
        Duplicate: static style => style.Duplicate(),
        Tags: static style => new TagSurface(
            style.GetUserStrings, style.SetUserString, style.DeleteUserString, style.DeleteAllUserStrings),
        Mint: static (_, def, key) =>
            from shaped in Try.lift(() => new DimensionStyle()).Run()
            from _ in def.Apply(style: shaped)
                .Rollback(release: () => Custody.Dispose(held: Seq(shaped)))
            select shaped,
        Revise: static (_, copy, def, key) => def.Apply(style: copy),
        Retitle: static (copy, name, key) => Try.lift(() => HostEdge.Side(() => copy.Name = name.Value)).Run(),
        Modify: static (document, copy, index, interaction, key) => Admit.Confirm(success: document.DimStyles.Modify(
            newSettings: copy, dimstyleIndex: index, quiet: interaction.IsQuiet)),
        Seat: static (document, style, key) => Try.lift(() => ResourceIndex.Admit(
            document.DimStyles.Add(dimstyle: style, reference: false))).Run().Bind(static inner => inner),
        Retire: static (document, indices, interaction, key) => indices
            .TraverseM(index => Admit.Confirm(success: document.DimStyles.Delete(index: index, quiet: interaction.IsQuiet)))
            .As().Map(static _ => unit),
        Elect: static (document, index, interaction, key) => Admit.Confirm(success: document.DimStyles.SetCurrent(
            index: index, quiet: interaction.IsQuiet)));

    internal Fin<Unit> Apply(RhinoDoc document) =>
        Switch(
            document,
            table: static (context, edit) => edit.Verb.Apply(grip: Grip, document: context),
            copy: static (context, edit) =>
                from source in edit.Source.Resolve(document: context, lens: Lens)
                from _ in Grip.Revised(target: edit.Target, document: context,
                    interaction: edit.Interaction,
                    revise: (style, key) => Try.lift(() => HostEdge.Side(() => style.CopyFrom(source))).Run())
                select unit,
            clearOverrides: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context,
                    interaction: edit.Interaction,
                    revise: (style) => edit.Fields.IsEmpty
                        ? Try.lift(() => HostEdge.Side(style.ClearAllFieldOverrides)).Run()
                        : edit.Fields.TraverseM(field => Try.lift(() => HostEdge.Side(
                            () => style.ClearFieldOverride(field: field.Host))).Run()).As().Map(static _ => unit)),
            absorb: static (context, edit) =>
                from style in edit.Target.Resolve(document: context, lens: Lens)
                from row in edit.Annotation.Only<AnnotationObjectBase>(document: context)
                from annotation in Admit.Need(row.Native.AnnotationGeometry)
                from _ in Try.lift(() => context.DimStyles.Modify(dimstyle: style, annotation: annotation) switch {
                    ModifyType.Modify or ModifyType.Override => Fin.Succ(value: unit),
                    var refused => Fin.Fail<Unit>(error: new KernelFault.InvalidResult(Detail: Some(refused.ToString()))),
                }).Run().Bind(static inner => inner)
                select unit,
            reparent: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context,
                    interaction: edit.Interaction,
                    revise: (style, key) => Try.lift(() => HostEdge.Side(() =>
                        style.ParentId = edit.Parent.Map(static parent => parent.Value).IfNone(noneValue: Guid.Empty))).Run()),
            scaleLengths: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context,
                    interaction: edit.Interaction,
                    revise: (style, key) => Try.lift(() => HostEdge.Side(
                        () => style.ScaleLengthValues(scale: edit.Factor.Value))).Run()),
            pageScale: static (context, edit) =>
                Grip.Revised(target: edit.Target, document: context,
                    interaction: edit.Interaction,
                    revise: (style, key) => Try.lift(() => HostEdge.Side(() => {
                        style.ScaleLeftLengthMillimeters = edit.Left.Value;
                        style.ScaleRightLengthMillimeters = edit.Right.Value;
                    })).Run()));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnnotationStyleOp {
    private AnnotationStyleOp() { }
    public sealed record Restyle(StylePatch Patch) : AnnotationStyleOp;
    public sealed record Unstyle : AnnotationStyleOp;

    internal Fin<Unit> Apply(AnnotationBase annotation) => Switch(
        annotation,
        restyle: static (context, edit) =>
            edit.Patch.Overlay(annotation: context).Map(static _ => unit),
        unstyle: static (context, _) => Admit.Confirm(success: context.ClearPropertyOverrides()));
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DraftMode {
    public static readonly DraftMode Recorded = new(key: 0, redraw: RedrawPolicy.Deferred, custody: UndoCustody.Recorded);
    public static readonly DraftMode Immediate = new(key: 1, redraw: RedrawPolicy.Immediate, custody: UndoCustody.Recorded);
    public static readonly DraftMode Unrecorded = new(key: 2, redraw: RedrawPolicy.Deferred, custody: UndoCustody.Unrecorded);

    internal RedrawPolicy Redraw { get; }
    internal UndoCustody Custody { get; }
}

public sealed record DraftPlan<TOp> where TOp : class {
    private DraftPlan(string name, DraftMode mode, Seq<TOp> operations) { Name = name; Mode = mode; Operations = operations; }

    public string Name { get; }
    public DraftMode Mode { get; }
    public Seq<TOp> Operations { get; }

    public static Fin<DraftPlan<TOp>> Of(string name, DraftMode mode, params ReadOnlySpan<TOp> operations) {
        return from label in Acceptance.Text(value: name)
               from admittedMode in Admit.Value(value: mode)
               from admittedRun in Acceptance.Rows(values: operations)
               from _ in guard(!admittedRun.IsEmpty, new KernelFault.InvalidInput())
               select new DraftPlan<TOp>(name: label, mode: admittedMode, operations: admittedRun);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Styles {
    public static Fin<Unit> Commit(DocumentSession session, DraftPlan<StyleOp> plan) =>
        DraftSpine.Commit(session: session, plan: plan,
            apply: static (document, operation, key) => operation.Apply(document: document));

    public static Fin<StyleAnswer> Ask(DocumentSession session, StyleAsk request) {
        return from admitted in Admit.Value(value: request)
               from answer in session.Demand(
                   use: document => admitted.Answer(document: document), needs: [SessionNeed.Read])
               select answer;
    }
}
```

## [05]-[ASK_FAMILY]

- Owner: `StyleAsk` `[Union]` — the catalog-backed read requests: whole-state snapshot under an optional axis scope, built-in-style census, swatch render, and default-or-rooted name minting; `StyleAnswer` `[Union]` — one typed result case per request; `StyleSetting` — one `(field, value)` read fact; `StyleTag` — one admitted user-string fact; `StyleSnapshot` — the one-pass definition read: identity, parentage, override census over schema rows, config projection, current-selection state, rendered length units, and user strings.
- Law: the snapshot's config projection is the schema fold — every `StyleField` row in the asked scope answers one `StyleSetting` through its own `Read` delegate, so a consumer never re-reads those host properties and an axis-scoped ask costs the rows of that axis alone.
- Law: the swatch crosses as an owned lease — `CreatePreviewBitmap` acquires a native bitmap, the answer wraps it in `Lease<Bitmap>.Owned`, and the caller's disposal is the only release; a bare bitmap field is the deleted form.
- Boundary: `CreatePreviewBitmap` renders through the host and reaches this page only inside `Styles.Ask`'s `DocumentSession.Demand`, which resolves every body on the command thread — so the preview needs no second crossing and none is spelled, exactly as the block-preview path is bound. A preview reached outside a demand has no affinity at all, and this page publishes no such route.
- Law: the preview extent is the kernel's `AssetExtent` — both scaled edges are measured against a DECLARED raster ceiling and the pixel product is proved inside `long` before any allocation — so this page carries no budget owner of its own and no ceiling literal a caller disagrees with.
- Law: the override census reads `IsFieldOverriden` (host single-`d` spelling) per schema row and `Overridden` IS the census, so a presence bool beside it mirrors `Overridden.IsEmpty` and drifts from it; `HasFieldOverrides` still answers presence before the per-row sweep, so an unoverridden style costs one probe.
- Packages: `Interaction/asset.md` (`AssetExtent`), `Numerics/atoms` (`PositiveMagnitude`, `Dimension`), `Domain/results` (`Lease<T>`); RhinoCommon `DimensionStyle.CreatePreviewBitmap`/`BuiltInStyles`/`GetUnusedStyleName` per `.api/api-rhinocommon-annotation.md`.
- Growth: a read is one `StyleAsk` case with its `StyleAnswer` twin; the scope, the lease law, and the extent ceiling come free.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleAsk {
    private StyleAsk() { }
    public sealed record Snapshot(ResourceRef Target, Option<StyleAxis> Axis = default) : StyleAsk;
    public sealed record BuiltIns : StyleAsk;
    public sealed record Swatch(ResourceRef Target, PreviewSpec Preview) : StyleAsk;
    public sealed record MintName(Option<ResourceName> Root = default) : StyleAsk;

    internal Fin<StyleAnswer> Answer(RhinoDoc document) =>
        Switch(
            context: document,
            snapshot: static (ctx, ask) =>
                from style in ask.Target.Resolve(document: ctx, lens: StyleOp.Lens)
                from state in StyleSnapshot.Of(style: style, document: ctx, axis: ask.Axis)
                select (StyleAnswer)new StyleAnswer.State(Snapshot: state),
            builtIns: static (ctx, _) => Try.lift(() => Fin.Succ<StyleAnswer>(value: new StyleAnswer.Rows(
                Styles: toSeq(ctx.DimStyles.BuiltInStyles)
                    .Map(static style => new DimStyleRow(
                        Key: ResourceId.Create(style.Id),
                        Name: ResourceName.Create(style.Name),
                        Index: ResourceIndex.Create(style.Index))),
                CurrentId: ResourceId.Create(ctx.DimStyles.CurrentId)))).Run().Bind(static inner => inner),
            swatch: static (ctx, ask) =>
                from style in ask.Target.Resolve(document: ctx, lens: StyleOp.Lens)
                from bitmap in Try.lift(() => Optional(style.CreatePreviewBitmap(
                        width: ask.Preview.Extent.PixelWidth,
                        height: ask.Preview.Extent.PixelHeight,
                        transparent: ask.Preview.Surface.UsesTransparency))
                    .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
                select (StyleAnswer)new StyleAnswer.Rendered(Swatch: new Lease<System.Drawing.Bitmap>.Owned(Value: bitmap)),
            mintName: static (ctx, ask) =>
                from minted in Try.lift(() => Acceptance.Text(value: ask.Root.Match(
                    Some: root => ctx.DimStyles.GetUnusedStyleName(rootName: root.Value),
                    None: () => ctx.DimStyles.GetUnusedStyleName()))).Run().Bind(static inner => inner)
                select (StyleAnswer)new StyleAnswer.Minted(Name: ResourceName.Create(minted)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StyleAnswer : IDetachedDocumentResult {
    private StyleAnswer() { }
    public sealed record State(StyleSnapshot Snapshot) : StyleAnswer;
    public sealed record Rows(Seq<DimStyleRow> Styles, ResourceId CurrentId) : StyleAnswer;
    public sealed record Rendered(Lease<System.Drawing.Bitmap> Swatch) : StyleAnswer;
    public sealed record Minted(ResourceName Name) : StyleAnswer;
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class PreviewSurface {
    public static readonly PreviewSurface Opaque = new(usesTransparency: false);
    public static readonly PreviewSurface Transparent = new(usesTransparency: true);
    internal bool UsesTransparency { get; }
}

[ComplexValueObject]
public sealed partial class PreviewSpec {
    public AssetExtent Extent { get; }
    public PreviewSurface Surface { get; }

    public static Fin<PreviewSpec> Of(
        PixelSpan width, PixelSpan height, PreviewSurface surface,
        Option<PixelSpan> ceiling = default) =>
        from extent in AssetExtent.Of(
            width: width, height: height, scale: PositiveMagnitude.Create(1.0), max: ceiling)
        select Create(extent: extent, surface: surface);
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class StyleTagKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(StyleTagKey) }));
    }
}

[ComplexValueObject]
public sealed partial class StyleTag {
    public StyleTagKey Key { get; }
    public string Value { get; }
}

public readonly record struct StyleSetting(StyleField Field, StyleValue Value);
public readonly record struct DimStyleRow(ResourceId Key, ResourceName Name, ResourceIndex Index);

public sealed record StyleSnapshot(
    ResourceId Key,
    ResourceIndex Index,
    ResourceName Name,
    Option<ResourceId> Parent,
    ResourceId Root,
    bool IsChild,
    Seq<StyleField> Overridden,
    Seq<StyleSetting> Settings,
    Seq<StyleTag> Tags,
    int TagCount,
    bool Current,
    double ScaleValue,
    ModelUnit LengthUnit,
    ModelUnit AlternateLengthUnit) : IDetachedDocumentResult {
    public static Fin<StyleSnapshot> Of(
        DimensionStyle style, RhinoDoc document, Option<StyleAxis> axis) =>
        from active in Admit.Need(style)
        let scope = axis.Match(Some: StyleField.On, None: static () => toSeq(StyleField.Items))
        from settings in scope
            .TraverseM(row => row.Read(style: active)
                .Map(value => new StyleSetting(Field: row, Value: value)))
            .As()
        from root in Try.lift(() => Optional(document.DimStyles.FindRoot(styleId: active.Id, ignoreDeleted: true))
            .ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
        from lengthUnit in ModelUnit.Of(
            value: active.DimensionLengthDisplayUnit(modelSerialNumber: document.RuntimeSerialNumber))
        from alternateLengthUnit in ModelUnit.Of(
            value: active.AlternateDimensionLengthDisplayUnit(modelSerialNumber: document.RuntimeSerialNumber))
        from snapshot in Try.lift(() => new StyleSnapshot(
            Key: ResourceId.Create(active.Id),
            Index: ResourceIndex.Create(active.Index),
            Name: ResourceName.Create(active.Name),
            Parent: ResourceId.Maybe(active.ParentId),
            Root: ResourceId.Create(root.Id),
            IsChild: active.IsChild,
            Overridden: active.HasFieldOverrides
                ? scope.Filter(row => active.IsFieldOverriden(field: row.Host))
                : Seq<StyleField>(),
            Settings: settings,
            Tags: toSeq(TagOp.Snapshot(active.GetUserStrings())).Map(static pair =>
                StyleTag.Create(key: StyleTagKey.Create(pair.Key), value: pair.Value)),
            TagCount: active.UserStringCount,
            Current: document.DimStyles.CurrentId == active.Id,
            ScaleValue: active.DimensionScaleValue,
            LengthUnit: lengthUnit,
            AlternateLengthUnit: alternateLengthUnit)).Run()
        select snapshot;
}
```

## [06]-[SPINE]

- Owner: `DraftSpine` is the one Annotation commit entry: it derives its needs through `SessionNeed.Mutation(custody:, redraw:)`, demands once, and commits the command-only program through `DocumentCommit.Sealed`.
- Law: the spine is the one commit entry for the namespace — style, text, dimension, hatch, linetype, and section commits share it verbatim, so undo, redraw, and grant semantics cannot drift between drafting programs; a page re-spelling the demand-and-seal sequence, or opening `UndoBracket.Begin` beside `Sealed`, is the deleted form.
- Law: `DocumentCommit.Compensated` is the one compensating-transaction fold — land each element, roll back every landed key on the first refusal, settle source custody through its release policy on every outcome, preserve the initiating fault, and append rollback and release faults in order; a page re-typing this fold or spelling a caller-local release cascade beside it is the deleted form.
- Packages: `Document/commit.md` (`DocumentCommit.Sealed`, `RedrawPolicy`), `Document/session.md` (`SessionNeed.Mutation`, `UndoCustody`).

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class DraftCount {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) =>
        validationError = value >= 0
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(DraftCount), value, "a non-negative tally" }));
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public sealed partial class DraftPath {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(DraftPath) }));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DraftSpine {
    internal static Fin<Unit> Commit<TOp>(
        DocumentSession session, DraftPlan<TOp> plan,
        Func<RhinoDoc, TOp, Fin<Unit>> apply) where TOp : class =>
        session.Demand(
            use: document => DocumentCommit.Sealed(
                document: document,
                name: plan.Name,
                recordsUndo: plan.Mode.Custody.Records,
                redraw: plan.Mode.Redraw,
                run: () => plan.Operations.TraverseM(operation => apply(document, operation)).As()
                    .Map(static _ => unit),
                project: Fin.Succ),
            needs: SessionNeed.Mutation(custody: plan.Mode.Custody, redraw: plan.Mode.Redraw).ToArray());
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]        | [OWNER]                         | [FORM]                                          | [ENTRY]                    |
| :-----: | :--------------- | :------------------------------ | :---------------------------------------------- | :------------------------- |
|  [01]   | native crossing  | `DraftCrossing`                 | detached handle crossing                        | `Crossed`                  |
|  [02]   | table revision   | `TableGrip<T, TDef>`            | Document lens + the host verb column roster     | `Revised(target, ...)`     |
|  [03]   | table verbs      | `TableOp<T, TDef>`              | eight cases every component table shares        | `Apply(grip, ...)`         |
|  [04]   | list revision    | `ListEdit<TRow>`/`ListSurface`  | index-addressed edits over a declared interface | `Apply(surface)`       |
|  [05]   | user-string bag  | `TagEdit`/`TagSurface`          | incremental edits + compensated whole-bag       | `Apply(owner)`         |
|  [06]   | object singleton | `TargetResolution`              | exactly-one id + typed cast on `TableTarget`    | `Only<TNative>`            |
|  [07]   | input custody    | `DraftBorrow`                   | nested lease scopes over a handle or a run      | `Typed<TNative, TOut>`     |
|  [08]   | drafting scalars | `DraftScale`/`DraftAngle`       | positive scale and radian owners                | `Create` / `Value`         |
|  [09]   | unit vocabulary  | `LengthDisplayRow`              | rows keyed on host values                       | `Host` projection          |
|  [10]   | enum admission   | `EnumFamily`                    | per-family flags mask and membership memo       | `For<TEnum>` / `Admits`    |
|  [11]   | schema mechanism | `FieldTable<TOwner, THostEnum>` | nine payload adapters + one seat + row read     | `Real` … `Glyph` / `Row`   |
|  [12]   | config schema    | `StyleField`                    | one row per proven property/`Field` pairing     | `Read` / `Write` / `On`    |
|  [13]   | edit currency    | `StylePatch`                    | exact-family run, table and override folds      | `Apply` / `Overlay`        |
|  [14]   | drawing standard | `DraftStandard`                 | kernel sheet ladders projected onto the rows    | `Patch(size, form, scale)` |
|  [15]   | annotation style | `AnnotationStyleOp`             | overlay / clear a per-annotation override       | `Apply(annotation)`    |
|  [16]   | style mutations  | `StyleOp`                       | shared verbs plus six style-only cases          | `Styles.Commit`            |
|  [17]   | style reads      | `StyleAsk`                      | closed request/answer family, axis-scoped       | `Styles.Ask`               |
|  [18]   | commit entry     | `DraftSpine`                    | sealed command fold                             | `Commit`                   |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
