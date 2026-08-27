# [RASM_RHINO_LAYERS]

`Rasm.Rhino.Document` owns the layer tree as a managed domain: full-path nesting topology, per-layer render material, linetype, print pen, print color, and section style, visibility and locking with their persistent variants, the per-detail-viewport override family, and every structural mutation — create, graft, reparent, merge, duplicate, delete, purge, revive, current-layer anointment, and ordering. The drawing-standards half composes the kernel `Rasm.Drawing` module: a standards-issued layer name projects into the host `::` path through `HostLayerScheme.RhinoPath` and re-admits through the same rows, a plot width is a `LineWidth` ladder rung with the host's two sentinel postures as named cases, and the plot product projects as a kernel `PlotStyle` for the CAD egress — no standards grammar, width table, or pen mapping is re-derived here.

`Layers.Ask` folds two products off one read window: a detached `LayerTree` under parameterized per-detail probe targets, and a host-free `OrganizationFact` when the caller supplies the federation authority. `Layers.Commit` folds an admitted operation program inside one session capability window through `DocumentCommit.Sealed`. `TableKind.Layers` stays the identity and reclamation row this page composes for purge; the tree itself is minted here.

## [01]-[INDEX]

- [02]-[IDENTITY_AND_ADDRESS]: `LeafName`, `LayerPath`, `StandardLayers`, `Liveness`, `LayerRef`, and the detached `LayerStamp` anchor.
- [03]-[TREE_SNAPSHOT]: `LayerTrait`, `PrintPen`, `LayerFace`, `DetailTrait`, `DetailFace`, `LayerNode`, and the `LayerTree` detached topology.
- [04]-[EDITS_AND_OVERRIDES]: `LayerEdit` staged-property program on its slot rosters beside the `LayerOverride` per-detail family.
- [05]-[COMMIT_PIPELINE]: `LayerOp`, `LayerDelta`, and the `Layers` entry pair.
- [07]-[SURFACE_LEDGER]: page owner map.

## [02]-[IDENTITY_AND_ADDRESS]

- Owner: `LeafName` admits one leaf name under the HOST name rule — the host's own legality probe plus separator freedom, no field structure — because a Rhino document's layers are arbitrary user text no published grammar governs; `LayerPath` canonicalizes trimmed segments and admits every leaf through that same rule before owning leaf, parent, and child projections. `StandardLayers` is the standards crossing: both directions of the kernel `Rasm.Drawing.LayerName` ↔ host-path correspondence, composed through `HostLayerScheme.RhinoPath` and nothing local. `Liveness` closes the deleted-row resolution axis as rows. `LayerRef` `[Union]` closes id, index, full-path, and current-layer addressing; `LayerStamp` `[ComplexValueObject]` is the detached identity anchor every fact row and tree node carries.
- Entry: `LayerRef.ById`/`AtIndex`/`AtPath`/`Current` are the only constructors. Internal `Resolve` admits one live row for every address case under a `Liveness` row, and `Index` projects that row's durable table index without a second lookup. `StandardLayers.Path(name)` projects a standards name into an admitted host path; `StandardLayers.Name(standard, path)` re-admits a host path under a declared standard. `StandardLayers` is a PUBLIC altitude entry under the folder census ruling — the `apps/<app>/` plugin shell's issued-set import composes it, so its zero in-corpus caller count proves altitude, not death.
- Law: the HOST leaf rule and the STANDARDS grammar are two facts with two owners, and the discriminant is stated here: `LeafName` admits what the HOST accepts (any legal layer text), while the kernel `Rasm.Drawing.LayerName` admits what a STANDARD publishes — a set issued under NCS, ISO 13567, BS 1192, or the house scheme crosses through `StandardLayers`, and a document following no standard never fabricates one. The prior local `LayerName` value object shadowed the kernel owner's simple name inside one assembly and carried no standards structure; the rename resolves the shadow and the standards half composes down.
- Law: the `::` path is a PROJECTION of the standards name, never the storage form — `HostLayerScheme.RhinoPath.Path` spells the segments and `Unproject` re-admits through the standard's OWN `LayerName.Parse` (the kernel member is reached through the scheme row, which re-joins the host separator onto the standard's delimiter before parsing), so a consumer that stored `Parent::Child` re-enters through `StandardLayers.Name` and no local code splits a standards path; the five `Layer.PathSeparator` sites on this page are the HOST-grammar owners' own interior (`LeafName` refusal, `LayerPath` canonicalize/segment/append) under the Boundary row's host-grammar law, never consumers.
- Law: a deleted layer is addressable only by id or index under `Liveness.IncludeDeleted` — the revive path — so a path address never resolves a dead branch, and every resolution failure is a typed fault, never a `-1` or null leak; the liveness ROW replaces the boolean whose negation each arm re-spelled.
- Law: an optional before `params` forecloses the positional spread, so every `params`-bearing factory on this page mints its key at the entry — stated once here, spelled nowhere else.
- Packages: `RhinoCommon` layer-table surface (`Rasm.Rhino/.api/api-rhinocommon-document.md` — `Layer.IsValidName`, `Layer.PathSeparator`, `GetLeafName`, `GetParentName`, `FindByFullPath`, `FindIndex`); kernel `Rasm.Drawing` (`LayerName`, `LayerStandard`, `HostLayerScheme` — `libs/dotnet/Rasm/.planning/Drawing/sheet.md#[07]-[LAYER]`); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`); `LanguageExt.Core` (`libs/dotnet/.api/api-languageext.md`).
- Boundary: `Layer.PathSeparator`, `GetLeafName`, `GetParentName`, and `IsValidName` are the host path grammar; `LayerPath` composes them once, so no consumer re-derives separator arithmetic or name legality.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Linq;
using Rasm.Domain;
using Rasm.Drawing;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct LeafName : IDetachedDocumentResult {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = Refusal(value: value);
    }

    internal static ValidationError? Refusal(string value) => value switch {
        "" => new ValidationError("LeafName requires a non-blank value."),
        var candidate when !Layer.IsValidName(name: candidate) => new ValidationError($"Rhino rejects layer name '{candidate}'."),
        var candidate when candidate.Contains(value: Layer.PathSeparator, comparisonType: StringComparison.Ordinal) =>
            new ValidationError("LeafName must not contain the host path separator."),
        _ => null,
    };

    public static Fin<LeafName> Of(string value) =>
        key.OrDefault().AcceptValidated<LeafName>(candidate: value);
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct LayerPath : IDetachedDocumentResult {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        string raw = value?.Trim() ?? string.Empty;
        string[] segments = raw.Split(separator: Layer.PathSeparator, options: StringSplitOptions.TrimEntries);
        value = string.Join(Layer.PathSeparator, segments);
        validationError = raw.Length is 0
            ? new ValidationError(string.Join(" | ", new object?[] { nameof(LayerPath) }))
            : toSeq(segments).Choose(static segment => Optional(LeafName.Refusal(value: segment))).Head.IfNone(default(ValidationError));
    }

    public Fin<Seq<LeafName>> Segments() {
        return toSeq(Value.Split(separator: Layer.PathSeparator, options: StringSplitOptions.TrimEntries))
            .Traverse(segment => LeafName.Of(value: segment).ToValidation())
            .As()
            .ToFin();
    }

    public Fin<LeafName> Leaf() =>
        LeafName.Of(value: Layer.GetLeafName(fullPath: Value));

    public Option<LayerPath> Parent =>
        Optional(Layer.GetParentName(fullPath: Value))
            .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
            .Bind(value => Of(value: value).ToOption());

    public Fin<LayerPath> Child(LeafName name) {
        return from admitted in guard(name != default, new KernelFault.InvalidInput()).ToFin().Map(_ => name)
               from path in Of(value: $"{Value}{Layer.PathSeparator}{admitted.Value}")
               select path;
    }

    public static Fin<LayerPath> Of(string value) =>
        key.OrDefault().AcceptValidated<LayerPath>(candidate: value);
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class StandardLayers {
    public static Fin<LayerPath> Path(Rasm.Drawing.LayerName name) {
        return LayerPath.Of(value: HostLayerScheme.RhinoPath.Path(name: name));
    }

    public static Fin<Rasm.Drawing.LayerName> Name(LayerStandard standard, LayerPath path) =>
        HostLayerScheme.RhinoPath.Unproject(standard: standard, path: path.Value);
}

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class Liveness {
    public static readonly Liveness ActiveOnly = new(key: false);
    public static readonly Liveness IncludeDeleted = new(key: true);

    internal bool Admits(Layer row) => Key || !row.IsDeleted;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerRef {
    private LayerRef() { }

    private sealed record IdCase(ResourceId Value) : LayerRef;
    private sealed record IndexCase(ResourceIndex Value) : LayerRef;
    private sealed record PathCase(LayerPath Value) : LayerRef;
    private sealed record CurrentCase : LayerRef;

    public static Fin<LayerRef> ById(Guid value) =>
        ResourceId.Admit(value: value).Map(static id => (LayerRef)new IdCase(Value: id));

    public static Fin<LayerRef> AtIndex(int value) =>
        ResourceIndex.Admit(value: value).Map(static index => (LayerRef)new IndexCase(Value: index));

    public static Fin<LayerRef> AtPath(LayerPath value) =>
        guard(value != default, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (LayerRef)new PathCase(Value: value));

    public static LayerRef Current { get; } = new CurrentCase();

    internal Fin<ResourceIndex> Index(RhinoDoc document, Liveness liveness) =>
        Resolve(document: document, liveness: liveness)
            .Bind(row => ResourceIndex.Admit(value: row.LayerIndex));

    internal Fin<Layer> Resolve(RhinoDoc document, Liveness liveness) =>
        Switch(
            state: (Document: document, Liveness: liveness),
            idCase: static (context, address) =>
                from index in Try.lift(() => ResourceIndex.Admit(
                    value: context.Document.Layers.Find(
                        layerId: address.Value.Value,
                        ignoreDeletedLayers: !context.Liveness.Key))).Run().Bind(static inner => inner)
                from row in Optional(context.Document.Layers.FindIndex(index: index.Value)).ToFin(Fail: new KernelFault.MissingContext())
                from admitted in guard(context.Liveness.Admits(row: row), new KernelFault.MissingContext())
                select row,
            indexCase: static (context, address) =>
                from row in Optional(context.Document.Layers.FindIndex(index: address.Value.Value)).ToFin(Fail: new KernelFault.MissingContext())
                from admitted in guard(context.Liveness.Admits(row: row), new KernelFault.MissingContext())
                select row,
            pathCase: static (context, address) =>
                from index in Try.lift(() => ResourceIndex.Admit(
                    value: context.Document.Layers.FindByFullPath(
                        layerPath: address.Value.Value,
                        notFoundReturnValue: NoLayer))).Run().Bind(static inner => inner)
                from row in Optional(context.Document.Layers.FindIndex(index: index.Value)).ToFin(Fail: new KernelFault.MissingContext())
                from admitted in guard(Liveness.ActiveOnly.Admits(row: row), new KernelFault.MissingContext())
                select row,
            currentCase: static (context, _) => Optional(context.Document.Layers.CurrentLayer)
                .Filter(static row => !row.IsDeleted)
                .ToFin(Fail: new KernelFault.InvalidResult()));

    private const int NoLayer = -1;
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError]
public sealed partial class LayerStamp : IDetachedDocumentResult {
    public Guid Id { get; }
    public int Index { get; }
    public LayerPath Path { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Guid id,
        ref int index,
        ref LayerPath path) =>
        validationError = id == Guid.Empty || index < 0 || path == default
            ? new ValidationError(string.Join(" | ", new object?[] { nameof(LayerStamp) }))
            : null;

    internal static Fin<LayerStamp> Of(Layer layer) =>
        from source in Optional(layer).ToFin(Fail: new KernelFault.MissingContext())
        from path in LayerPath.Of(value: source.FullPath)
        from stamp in FactoryBridge.Accept<LayerStamp>(
            fault: Validate(source.Id, source.LayerIndex, path, out LayerStamp? admitted),
            admitted: admitted)
        select stamp;
}
```

## [03]-[TREE_SNAPSHOT]

- Owner: `LayerTrait` is the eight-row condition vocabulary and every node carries ONE `CapabilitySet<LayerTrait>` under a `CapabilityLaw` barring the deleted-and-current corner; `PrintPen` `[Union]` closes the host plot-weight axis — the ISO 128-24 ladder rung beside the host's two sentinel postures as NAMED cases; `LayerFace` carries the render/print product with the pen typed and a kernel `PlotStyle` projection for the CAD egress; `DetailTrait` and `DetailFace` carry one probed per-detail override row; `LayerNode` is one detached tree node; `LayerTree` is the whole topology from one read, its flat index built once and its depth measured at mint.
- Entry: `Layers.Ask(session, key, detailViewports)` demands `SessionNeed.Read` and mints the tree inside one callback; probe targets are call data, so the same entry answers the plain topology and any per-detail audit without a second surface.
- Law: the eight condition booleans ride ONE set — membership is the host `true` — and the law bars `{Deleted, Current}` together, the corner the host itself cannot produce and a fabricated snapshot could; `Reference` barring mutation is a COMMIT-side admission reading the set, because a cross-operation clause is the pipeline's fact, not the snapshot's. NAMED LOSS: the per-column property names — `node.Conditions.Admits(LayerTrait.Locked)` reads what `Condition.Locked` read — bought back by the set's printable `Wire` and one-row growth.
- Law: the plot-weight axis is THREE-state and each state is a case, never a sentinel: `Pen` carries the ladder rung a free millimetre SNAPS onto through `LineWidth.For` (an off-ladder authored width plots at its nearest rung — the ladder is the authority, and that snap is the point), `HostDefault` is the host's `0.0` "use the application default", and `NoPlot` its `-1.0` "do not plot" — both host facts a bare double smuggled past every reader. `OfHost` is the one ingress and `ToHost` the one egress, so no consumer compares a magic weight again.
- Law: the tree is built from one table sweep — non-deleted rows keyed by id, children grouped by `ParentLayerId`, roots at the empty parent, siblings ordered by `SortIndex` then name — so parent/child evidence is structural, never re-derived per consumer from path text.
- Law: the parent graph is proved acyclic ONCE and ordered by the container's own fact, never a hand walk. `ParentLayerId` is a raw host id the table does not prove acyclic, so the rows build a `BidirectionalGraph`, `IsDirectedAcyclicGraph` is the witness — a cycle refuses typed naming an offending path, an orphan parent refuses as a typed miss — and `SourceFirstTopologicalSort` hands the parents-first order whose REVERSE assembles children-before-parents in one fold. The budgeted parent climb this replaces measured depth per node beside a graph library already admitted; `Depth` now derives from the same order.
- Law: the host exposes no roster of viewports carrying overrides, so per-detail evidence is probe-parameterized: each requested viewport lands a `DetailFace` only where `HasPerViewportSettings` proves one, and an unprobed override is absent evidence, never a fabricated default.
- Packages: `QuikGraph` (`libs/dotnet/.api/api-quikgraph.md` — `ToBidirectionalGraph`, `IsDirectedAcyclicGraph`, `SourceFirstTopologicalSort`); kernel `Rasm.Drawing` (`LineWidth`, `PlotStyle`, `PlotStyleKey`, `AciIndex`); `Numerics/atoms` (`PerceptualColor.OfHost`, `UnitInterval`); `RhinoCommon` layer members per the `.api` catalog.
- Boundary: every node is detached — the live `Layer` handle dies inside the demand window, and `LayerTree` implements `IDetachedDocumentResult` so it crosses out of `Demand` by construction.
- Boundary: persistent visibility and locking are THREE-state on the write side and TWO-state on the read side, and the host closes no probe over the gap. `GetPersistentVisibility` answers the layer's CURRENT `IsVisible` when nothing was ever set, so an explicit `true` and an unset-and-visible layer read identically. The trait rows therefore report the host's collapsed answer under names that state the collapse; the edit side keeps all three states because it writes through the pair the host does expose.
- Boundary: the screen draw colour stays the host's own `System.Drawing.Color` evidence — a snapshot column, never a public payload crossing — and the PLOT product leaves only through the `PlotStyle` projection, where the colour admits into `PerceptualColor` once.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerTrait : ICapability<LayerTrait> {
    public static readonly LayerTrait Visible = new(key: "visible");
    public static readonly LayerTrait Locked = new(key: "locked");
    public static readonly LayerTrait PersistentVisibility = new(key: "persistent-visibility");
    public static readonly LayerTrait PersistentLocking = new(key: "persistent-locking");
    public static readonly LayerTrait Expanded = new(key: "expanded");
    public static readonly LayerTrait Current = new(key: "current");
    public static readonly LayerTrait Deleted = new(key: "deleted");
    public static readonly LayerTrait Reference = new(key: "reference");

    public static CapabilityLaw<LayerTrait> Law => law.Value;
    private static readonly Lazy<CapabilityLaw<LayerTrait>> law =
        new(static () => CapabilityLaw<LayerTrait>.Forbidden(Seq(CapabilitySet<LayerTrait>.Of(Deleted, Current))));

    internal static CapabilitySet<LayerTrait> Of(Layer layer) {
        CapabilitySet<LayerTrait> held = CapabilitySet<LayerTrait>.Of();
        held = layer.IsVisible ? held.With(Visible) : held;
        held = layer.IsLocked ? held.With(Locked) : held;
        held = layer.GetPersistentVisibility() ? held.With(PersistentVisibility) : held;
        held = layer.GetPersistentLocking() ? held.With(PersistentLocking) : held;
        held = layer.IsExpanded ? held.With(Expanded) : held;
        held = layer.IsCurrent ? held.With(Current) : held;
        held = layer.IsDeleted ? held.With(Deleted) : held;
        held = layer.IsReference ? held.With(Reference) : held;
        return held;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PrintPen {
    private PrintPen() { }

    public sealed record HostDefaultCase : PrintPen;
    public sealed record NoPlotCase : PrintPen;
    public sealed record PenCase(Rasm.Drawing.LineWidth Width) : PrintPen;

    public static PrintPen HostDefault { get; } = new HostDefaultCase();
    public static PrintPen NoPlot { get; } = new NoPlotCase();

    public static Fin<PrintPen> Pen(Rasm.Drawing.LineWidth width) =>
        key.OrDefault().Need(value: width).Map(static rung => (PrintPen)new PenCase(Width: rung));

    internal static Fin<PrintPen> OfHost(double weight) => weight switch {
        0.0 => Fin.Succ<PrintPen>(new HostDefaultCase()),
        -1.0 => Fin.Succ<PrintPen>(new NoPlotCase()),
        var value when double.IsFinite(value) && value > 0.0 =>
            Rasm.Drawing.LineWidth.For(width: UnitsNet.Length.FromMillimeters(value))
                .Map(static rung => (PrintPen)new PenCase(Width: rung)),
        _ => Fin.Fail<PrintPen>(new KernelFault.OutOfRange(Label: nameof(PrintPen), Scalar: weight, Requirement: "0.0, -1.0, or a positive finite millimetre width")),
    };

    internal double ToHost() => Switch(
        hostDefaultCase: static _ => 0.0,
        noPlotCase: static _ => -1.0,
        penCase: static pen => pen.Width.Width.Millimeters);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LayerFace(
    System.Drawing.Color Color,
    System.Drawing.Color PrintColor,
    PrintPen Print,
    int LinetypeIndex,
    int RenderMaterialIndex,
    int SectionStyleIndex) : IDetachedDocumentResult {
    internal static Fin<LayerFace> Of(Layer layer) =>
        PrintPen.OfHost(weight: layer.PlotWeight).Map(pen => new LayerFace(
            Color: layer.Color,
            PrintColor: layer.PlotColor,
            Print: pen,
            LinetypeIndex: layer.LinetypeIndex,
            RenderMaterialIndex: layer.RenderMaterialIndex,
            SectionStyleIndex: layer.SectionStyleIndex));

    public Fin<Option<Rasm.Drawing.PlotStyle>> PlotOf(Rasm.Drawing.AciIndex seat, Rasm.Drawing.LineWidth hostDefault) {
        return Print.Switch(
            state: (Face: this, Seat: seat, Default: hostDefault),
            hostDefaultCase: static (context, _) => Styled(context: context, width: context.Default),
            noPlotCase: static (_, _) => Fin.Succ(Option<Rasm.Drawing.PlotStyle>.None),
            penCase: static (context, pen) => Styled(context: context, width: pen.Width));

        static Fin<Option<Rasm.Drawing.PlotStyle>> Styled(
            (LayerFace Face, Rasm.Drawing.AciIndex Seat, Rasm.Drawing.LineWidth Default) context,
            Rasm.Drawing.LineWidth width) =>
            from ink in PerceptualColor.OfHost(host: context.Face.PrintColor)
            from style in Rasm.Drawing.PlotStyle.Of(
                key: new Rasm.Drawing.PlotStyleKey.Indexed(index: context.Seat),
                width: width,
                screening: UnitInterval.Create(value: 1.0),
                colour: Some(ink))
            select Some(style);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DetailTrait : ICapability<DetailTrait> {
    public static readonly DetailTrait Visible = new(key: "visible");
    public static readonly DetailTrait PersistentVisibility = new(key: "persistent-visibility");
}

public sealed record DetailFace(
    Guid Viewport,
    System.Drawing.Color Color,
    System.Drawing.Color PrintColor,
    PrintPen Print,
    CapabilitySet<DetailTrait> Conditions) : IDetachedDocumentResult {
    internal static Fin<Option<DetailFace>> Probe(Layer layer, Guid viewport) =>
        layer.HasPerViewportSettings(viewportId: viewport)
            ? PrintPen.OfHost(weight: layer.PerViewportPlotWeight(viewportId: viewport))
                .Map(pen => Some(new DetailFace(
                    Viewport: viewport,
                    Color: layer.PerViewportColor(viewportId: viewport),
                    PrintColor: layer.PerViewportPlotColor(viewportId: viewport),
                    Print: pen,
                    Conditions: Held(layer: layer, viewport: viewport))))
            : Fin.Succ(Option<DetailFace>.None);

    private static CapabilitySet<DetailTrait> Held(Layer layer, Guid viewport) {
        CapabilitySet<DetailTrait> held = CapabilitySet<DetailTrait>.Of();
        held = layer.PerViewportIsVisible(viewportId: viewport) ? held.With(DetailTrait.Visible) : held;
        held = layer.PerViewportPersistentVisibility(viewportId: viewport) ? held.With(DetailTrait.PersistentVisibility) : held;
        return held;
    }
}

public sealed record LayerNode(
    LayerStamp Identity,
    LeafName Name,
    Option<Guid> Parent,
    LayerFace Face,
    CapabilitySet<LayerTrait> Conditions,
    int SortIndex,
    Seq<DetailFace> Details,
    Seq<LayerNode> Children) : IDetachedDocumentResult {
    public Seq<LayerNode> Flatten() => this.Cons(Children.Bind(static child => child.Flatten()));
}

public sealed class LayerTree : IDetachedDocumentResult {
    private readonly Lazy<Seq<LayerNode>> flat;
    private readonly Lazy<HashMap<Guid, LayerNode>> byId;

    internal LayerTree(Seq<LayerNode> roots, int count, int depth, Option<LayerStamp> current) {
        (Roots, Count, Depth, Current) = (roots, count, depth, current);
        flat = new Lazy<Seq<LayerNode>>(() => Roots.Bind(static root => root.Flatten()).Strict());
        byId = new Lazy<HashMap<Guid, LayerNode>>(() => toHashMap(flat.Value.Map(static node => (node.Identity.Id, node))));
    }

    public Seq<LayerNode> Roots { get; }
    public int Count { get; }
    public int Depth { get; }
    public Option<LayerStamp> Current { get; }

    public Seq<LayerNode> Flatten() => flat.Value;

    public Option<LayerNode> Find(LayerRef address) => address.Switch(
        state: this,
        idCase: static (tree, target) => tree.byId.Value.Find(target.Value.Value),
        indexCase: static (tree, target) => tree.Flatten().Find(node => node.Identity.Index == target.Value.Value),
        pathCase: static (tree, target) => tree.Flatten().Find(node => node.Identity.Path == target.Value),
        currentCase: static (tree, _) => tree.Current.Bind(stamp => tree.byId.Value.Find(stamp.Id)));

    internal static Fin<LayerTree> Of(RhinoDoc document, Seq<Guid> detailViewports) => Try.lift(() => {
        Seq<Layer> rows = toSeq(document.Layers.AsIterable()).Filter(static row => !row.IsDeleted).Strict();
        return from nodes in rows
            .Traverse(row => Leaf(layer: row, detailViewports: detailViewports).ToValidation())
            .As()
            .ToFin()
        from assembled in Assembled(nodes: nodes)
        from current in Optional(document.Layers.CurrentLayer)
            .Traverse(layer => LayerStamp.Of(layer: layer))
            .As()
        select new LayerTree(roots: assembled.Roots, count: nodes.Count, depth: assembled.Depth, current: current);
    }).Run().Bind(static inner => inner);

    private static Fin<LayerNode> Leaf(Layer layer, Seq<Guid> detailViewports) =>
        from identity in LayerStamp.Of(layer: layer)
        from name in LeafName.Of(value: layer.Name)
        from face in LayerFace.Of(layer: layer)
        from details in detailViewports
            .Traverse(viewport => DetailFace.Probe(layer: layer, viewport: viewport).ToValidation())
            .As()
            .ToFin()
        select new LayerNode(
            Identity: identity,
            Name: name,
            Parent: Optional(layer.ParentLayerId).Filter(static parent => parent != Guid.Empty),
            Face: face,
            Conditions: LayerTrait.Of(layer: layer),
            SortIndex: layer.SortIndex,
            Details: details.Somes(),
            Children: Seq<LayerNode>());

    private static Fin<(Seq<LayerNode> Roots, int Depth)> Assembled(Seq<LayerNode> nodes) {
        HashMap<Guid, LayerNode> byId = toHashMap(nodes.Map(static node => (node.Identity.Id, node)));
        Option<LayerNode> orphan = nodes.Find(node => node.Parent.Exists(parent => byId.Find(parent).IsNone));
        if (orphan.Case is LayerNode lost) {
            return Fin.Fail<(Seq<LayerNode>, int)>(error: key.MissingContext(detail: lost.Identity.Path.Value));
        }
        BidirectionalGraph<Guid, SEdge<Guid>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(vertices: nodes.Map(static node => node.Identity.Id));
        graph.AddEdgeRange(edges: nodes.Choose(static node =>
            node.Parent.Map(parent => new SEdge<Guid>(source: parent, target: node.Identity.Id))));
        return Try.lift(() => {
            if (!graph.IsDirectedAcyclicGraph()) {
                Option<LayerNode> witness = nodes.Find(static node => node.Parent.IsSome);
                return Fin.Fail<(Seq<LayerNode>, int)>(error: new KernelFault.InvalidResult(Detail: Some(witness.Map(static node => node.Identity.Path.Value).IfNone("parent cycle"))));
            }
            Seq<Guid> order = toSeq(graph.SourceFirstTopologicalSort());
            HashMap<Guid, Seq<Guid>> childIds = nodes.Fold(
                HashMap<Guid, Seq<Guid>>(),
                static (held, node) => node.Parent.Match(
                    Some: parent => held.AddOrUpdate(
                        key: parent,
                        Some: existing => existing.Add(value: node.Identity.Id),
                        None: () => Seq(node.Identity.Id)),
                    None: () => held));
            HashMap<Guid, int> depth = order.Fold(
                HashMap<Guid, int>(),
                (held, id) => held.Add(id, byId.Find(id)
                    .Bind(static node => node.Parent)
                    .Bind(parent => held.Find(parent))
                    .Map(static parentDepth => parentDepth + 1)
                    .IfNone(0)));
            HashMap<Guid, LayerNode> built = order.Rev().Fold(byId, (held, id) => held.AddOrUpdate(
                key: id,
                value: held.Find(id).IfNone(() => byId[id]) with {
                    Children = Sorted(rows: childIds.Find(id).IfNone(Seq<Guid>()).Choose(child => held.Find(child))),
                }));
            return Fin.Succ((
                Sorted(rows: nodes.Filter(static node => node.Parent.IsNone).Choose(node => built.Find(node.Identity.Id))),
                depth.Values.Fold(0, Math.Max)));
        }).Run().Bind(static inner => inner);
    }

    private static Seq<LayerNode> Sorted(Seq<LayerNode> rows) =>
        toSeq(rows
            .OrderBy(static node => node.SortIndex)
            .ThenBy(static node => node.Name.Value, StringComparer.OrdinalIgnoreCase));
}
```

## [04]-[EDITS_AND_OVERRIDES]

- Owner: `DetailPaint` and `DetailToggle` are the per-detail slot rosters — each row carries its host SET and CLEAR members as one delegate pair, so write-versus-clear is the `Option` on the case and the member pair is row data; `LayerOverride` `[Union]` closes the per-detail-viewport family at five cases — the two paint slots, the two toggle slots, the typed pen, the new-detail default, and the whole-viewport purge; `PaintColumn`, `SeatColumn`, `PersistSlot`, and `LayerFlag` are the staged-write rosters; `LayerEdit` `[Union]` closes every staged property write at nine cases.
- Entry: edit factories admit payloads once — a pen is an admitted `PrintPen` (never a raw millimetre), indexes admit against their row's own floor, names against the host leaf rule — and `Apply` runs each case against the staged layer copy inside the commit callback.
- Law: the five former `(Guid, Option<T>)` twin cases were one shape five times — the payload TYPE and the host member pair were the only variation — so the pair moves onto a row and the case count halves: a sixth per-detail slot is one roster row, never a sixth case, and the `Option` write/clear law is stated once on the case rather than five times. NAMED LOSS: the per-slot case names (`LayerOverride.Visible(...)` reads as `Toggle(viewport, DetailToggle.Visible, ...)`); bought back by named convenience factories that keep every call site literate.
- Law: the per-detail print width is the SAME three-state axis the face reads — an override pen rides `PrintPen`, so D39/D40's bare doubles are gone at the write edge too, and the `-1.0`/`0.0` host sentinels cannot be spelled as an override value at all.
- Law: a persistent-visibility or persistent-locking edit carries `Option<bool>`: a value writes `SetPersistent*`, absence runs `UnsetPersistent*`, so the host's three-state persistence is one case rather than a set/unset verb pair — and the same rule holds per-detail on the `DetailToggle.PersistentVisibility` row.
- Law: section style is two independent axes — the table index rides the `SeatColumn.SectionStyle` row and the custom carrier clears through absence, mirroring the host `SetCustomSectionStyle`/`RemoveCustomSectionStyle` pair as one case.
- Law: index floors are ROW DATA — linetype and IGES level floor at zero, render material and section style admit the host's `-1` "unassigned" — so one admission serves four columns and a fifth indexed column is a row naming its own floor.
- Boundary: every override member on `Layer` is a void host write; each arm crosses through `Try.lift`, and the staged copy never leaves the callback, so a failed edit program leaves the live table untouched until `Modify` lands the whole staged state.
- Boundary: `Layer` inherits `IDisposable` through `ModelComponent`/`CommonObject`, and `Add`/`Modify` copy their argument into the table, so every caller-minted `Layer` — the created row and the staged copy alike — rides `Lease<Layer>.Owned(...).Use(...)`; a live row read back through `FindIndex` or `CurrentLayer` is table-owned and never leased.
- Packages: `RhinoCommon` per-viewport override family and staged-modify members (`Rasm.Rhino/.api/api-rhinocommon-document.md` — `SetPerViewport*`/`DeletePerViewport*`/`UnsetPerViewportPersistentVisibility`, `Layer.PlotWeight`, `Layers.Modify`); kernel `Rasm.Drawing.LineWidth` behind `PrintPen`; `Thinktecture.Runtime.Extensions` delegate-column rosters.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
internal sealed partial class DetailPaint {
    internal static readonly DetailPaint Color = new(
        key: 0,
        set: static (layer, viewport, value) => layer.SetPerViewportColor(viewportId: viewport, color: value),
        clear: static (layer, viewport) => layer.DeletePerViewportColor(viewportId: viewport));
    internal static readonly DetailPaint PrintColor = new(
        key: 1,
        set: static (layer, viewport, value) => layer.SetPerViewportPlotColor(viewportId: viewport, color: value),
        clear: static (layer, viewport) => layer.DeletePerViewportPlotColor(viewportId: viewport));

    [UseDelegateFromConstructor]
    internal partial void Set(Layer layer, Guid viewport, System.Drawing.Color value);

    [UseDelegateFromConstructor]
    internal partial void Clear(Layer layer, Guid viewport);
}

[SmartEnum<int>]
internal sealed partial class DetailToggle {
    internal static readonly DetailToggle Visible = new(
        key: 0,
        set: static (layer, viewport, value) => layer.SetPerViewportVisible(viewportId: viewport, visible: value),
        clear: static (layer, viewport) => layer.DeletePerViewportVisible(viewportId: viewport));
    internal static readonly DetailToggle PersistentVisibility = new(
        key: 1,
        set: static (layer, viewport, value) => layer.SetPerViewportPersistentVisibility(viewportId: viewport, persistentVisibility: value),
        clear: static (layer, viewport) => layer.UnsetPerViewportPersistentVisibility(viewportId: viewport));

    [UseDelegateFromConstructor]
    internal partial void Set(Layer layer, Guid viewport, bool value);

    [UseDelegateFromConstructor]
    internal partial void Clear(Layer layer, Guid viewport);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerOverride {
    private LayerOverride() { }

    private sealed record PaintCase(Guid Viewport, DetailPaint Slot, Option<System.Drawing.Color> Value) : LayerOverride;
    private sealed record ToggleCase(Guid Viewport, DetailToggle Slot, Option<bool> Value) : LayerOverride;
    private sealed record PenCase(Guid Viewport, Option<PrintPen> Value) : LayerOverride;
    private sealed record NewDetailVisibilityCase(bool Value) : LayerOverride;
    private sealed record PurgeCase(Guid Viewport) : LayerOverride;

    public static Fin<LayerOverride> Color(Guid viewport, Option<System.Drawing.Color> value = default) =>
        Addressed(viewport: viewport, mint: address => new PaintCase(Viewport: address, Slot: DetailPaint.Color, Value: value));

    public static Fin<LayerOverride> PrintColor(Guid viewport, Option<System.Drawing.Color> value = default) =>
        Addressed(viewport: viewport, mint: address => new PaintCase(Viewport: address, Slot: DetailPaint.PrintColor, Value: value));

    public static Fin<LayerOverride> Visible(Guid viewport, Option<bool> value = default) =>
        Addressed(viewport: viewport, mint: address => new ToggleCase(Viewport: address, Slot: DetailToggle.Visible, Value: value));

    public static Fin<LayerOverride> PersistentVisibility(Guid viewport, Option<bool> value = default) =>
        Addressed(viewport: viewport, mint: address => new ToggleCase(Viewport: address, Slot: DetailToggle.PersistentVisibility, Value: value));

    public static Fin<LayerOverride> Pen(Guid viewport, Option<PrintPen> value = default) =>
        Addressed(viewport: viewport, mint: address => new PenCase(Viewport: address, Value: value));

    public static LayerOverride NewDetailVisibility(bool value) => new NewDetailVisibilityCase(Value: value);

    public static Fin<LayerOverride> Purge(Guid viewport) =>
        Addressed(viewport: viewport, mint: address => new PurgeCase(Viewport: address));

    private static Fin<LayerOverride> Addressed(Guid viewport, Func<Guid, LayerOverride> mint) =>
        guard(viewport != Guid.Empty, key.OrDefault().InvalidInput()).ToFin().Map(_ => mint(arg: viewport));

    internal Fin<Unit> Apply(Layer layer) =>
        Switch(
            state: layer,
            paintCase: static (context, edit) => LayerEdit.Toggle(value: edit.Value,
                set: value => edit.Slot.Set(context, edit.Viewport, value),
                clear: () => edit.Slot.Clear(context, edit.Viewport)),
            toggleCase: static (context, edit) => LayerEdit.Toggle(value: edit.Value,
                set: value => edit.Slot.Set(context, edit.Viewport, value),
                clear: () => edit.Slot.Clear(context, edit.Viewport)),
            penCase: static (context, edit) => LayerEdit.Toggle(value: edit.Value,
                set: pen => context.SetPerViewportPlotWeight(viewportId: edit.Viewport, plotWeight: pen.ToHost()),
                clear: () => context.DeletePerViewportPlotWeight(viewportId: edit.Viewport)),
            newDetailVisibilityCase: static (context, edit) => LayerEdit.Write(write: () => context.PerViewportIsVisibleInNewDetails = edit.Value),
            purgeCase: static (context, edit) => LayerEdit.Write(write: () => context.DeletePerViewportSettings(viewportId: edit.Viewport)));
}

// --- [SUBSECTION]
[SmartEnum<int>]
internal sealed partial class PaintColumn {
    internal static readonly PaintColumn Color = new(key: 0, write: static (layer, value) => layer.Color = value);
    internal static readonly PaintColumn PrintColor = new(key: 1, write: static (layer, value) => layer.PlotColor = value);

    [UseDelegateFromConstructor]
    internal partial void Write(Layer layer, System.Drawing.Color value);
}

[SmartEnum<int>]
internal sealed partial class SeatColumn {
    internal static readonly SeatColumn Linetype = new(key: 0, floor: 0, write: static (layer, value) => layer.LinetypeIndex = value);
    internal static readonly SeatColumn RenderMaterial = new(key: 1, floor: -1, write: static (layer, value) => layer.RenderMaterialIndex = value);
    internal static readonly SeatColumn SectionStyle = new(key: 2, floor: -1, write: static (layer, value) => layer.SectionStyleIndex = value);
    internal static readonly SeatColumn IgesLevel = new(key: 3, floor: 0, write: static (layer, value) => layer.IgesLevel = value);

    internal int Floor { get; }

    [UseDelegateFromConstructor]
    internal partial void Write(Layer layer, int value);
}

[SmartEnum<int>]
internal sealed partial class PersistSlot {
    internal static readonly PersistSlot Visibility = new(
        key: 0,
        set: static (layer, value) => layer.SetPersistentVisibility(persistentVisibility: value),
        clear: static layer => layer.UnsetPersistentVisibility());
    internal static readonly PersistSlot Locking = new(
        key: 1,
        set: static (layer, value) => layer.SetPersistentLocking(persistentLocking: value),
        clear: static layer => layer.UnsetPersistentLocking());

    [UseDelegateFromConstructor]
    internal partial void Set(Layer layer, bool value);

    [UseDelegateFromConstructor]
    internal partial void Clear(Layer layer);
}

[SmartEnum<int>]
internal sealed partial class LayerFlag {
    internal static readonly LayerFlag Visible = new(key: 0, set: static (layer, value) => layer.IsVisible = value);
    internal static readonly LayerFlag Locked = new(key: 1, set: static (layer, value) => layer.IsLocked = value);
    internal static readonly LayerFlag Expanded = new(key: 2, set: static (layer, value) => layer.IsExpanded = value);

    [UseDelegateFromConstructor]
    internal partial void Set(Layer layer, bool value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerEdit {
    private LayerEdit() { }

    private sealed record RenameCase(LeafName Name) : LayerEdit;
    private sealed record PaintCase(PaintColumn Column, System.Drawing.Color Value) : LayerEdit;
    private sealed record PenCase(PrintPen Value) : LayerEdit;
    private sealed record SeatCase(SeatColumn Column, int Index) : LayerEdit;
    private sealed record CustomSectionStyleCase(Option<SectionStyle> Value) : LayerEdit;
    private sealed record FlagCase(LayerFlag Flag, bool Value) : LayerEdit;
    private sealed record PersistCase(PersistSlot Slot, Option<bool> Value) : LayerEdit;
    private sealed record DescriptionCase(Option<string> Value) : LayerEdit;
    private sealed record OverrideCase(LayerOverride Value) : LayerEdit;

    public static LayerEdit Rename(LeafName name) => new RenameCase(Name: name);

    public static LayerEdit Recolor(System.Drawing.Color value) => new PaintCase(Column: PaintColumn.Color, Value: value);

    public static LayerEdit PrintColor(System.Drawing.Color value) => new PaintCase(Column: PaintColumn.PrintColor, Value: value);

    public static Fin<LayerEdit> Pen(PrintPen value) =>
        key.OrDefault().Need(value: value).Map(static pen => (LayerEdit)new PenCase(Value: pen));

    public static Fin<LayerEdit> Linetype(int index) =>
        Seated(column: SeatColumn.Linetype, index: index);

    public static Fin<LayerEdit> RenderMaterial(int index) =>
        Seated(column: SeatColumn.RenderMaterial, index: index);

    public static Fin<LayerEdit> SectionStyleIndex(int index) =>
        Seated(column: SeatColumn.SectionStyle, index: index);

    public static Fin<LayerEdit> IgesLevel(int value) =>
        Seated(column: SeatColumn.IgesLevel, index: value);

    public static LayerEdit CustomSectionStyle(Option<SectionStyle> value = default) => new CustomSectionStyleCase(Value: value);

    public static LayerEdit Visibility(bool value) => new FlagCase(Flag: LayerFlag.Visible, Value: value);

    public static LayerEdit Locking(bool value) => new FlagCase(Flag: LayerFlag.Locked, Value: value);

    public static LayerEdit Expansion(bool value) => new FlagCase(Flag: LayerFlag.Expanded, Value: value);

    public static LayerEdit PersistentVisibility(Option<bool> value = default) => new PersistCase(Slot: PersistSlot.Visibility, Value: value);

    public static LayerEdit PersistentLocking(Option<bool> value = default) => new PersistCase(Slot: PersistSlot.Locking, Value: value);

    public static Fin<LayerEdit> Description(string value) =>
        key.OrDefault().AcceptText(value: value)
            .Map(admitted => (LayerEdit)new DescriptionCase(Value: Some(admitted)));

    public static LayerEdit ClearDescription() => new DescriptionCase(Value: Option<string>.None);

    public static LayerEdit Override(LayerOverride value) => new OverrideCase(Value: value);

    private static Fin<LayerEdit> Seated(SeatColumn column, int index) =>
        guard(index >= column.Floor, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (LayerEdit)new SeatCase(Column: column, Index: index));

    internal Fin<Unit> Apply(Layer staged) =>
        Switch(
            state: staged,
            renameCase: static (context, edit) => Write(write: () => context.Name = edit.Name.Value),
            paintCase: static (context, edit) => Write(write: () => edit.Column.Write(context, edit.Value)),
            penCase: static (context, edit) => Write(write: () => context.PlotWeight = edit.Value.ToHost()),
            seatCase: static (context, edit) => Write(write: () => edit.Column.Write(context, edit.Index)),
            customSectionStyleCase: static (context, edit) => Toggle(value: edit.Value,
                set: style => context.SetCustomSectionStyle(sectionStyle: style),
                clear: context.RemoveCustomSectionStyle),
            flagCase: static (context, edit) => Write(write: () => edit.Flag.Set(context, edit.Value)),
            persistCase: static (context, edit) => Toggle(value: edit.Value,
                set: value => edit.Slot.Set(context, value),
                clear: () => edit.Slot.Clear(context)),
            descriptionCase: static (context, edit) => Write(write: () => context.Description = edit.Value.IfNone(string.Empty)),
            overrideCase: static (context, edit) => edit.Value.Apply(layer: context));

    internal static Fin<Unit> Write(Action write) =>
        Try.lift(() => {
            write();
            return Fin.Succ(value: unit);
        }).Run().Bind(static inner => inner);

    internal static Fin<Unit> Toggle<T>(Option<T> value, Action<T> set, Action clear) =>
        Write(write: () => value.Match(
            Some: chosen => set(obj: chosen),
            None: () => clear()));
}
```

## [05]-[COMMIT_PIPELINE]

- Owner: `SortSense` and `DuplicateScope` close the two ordering/copy knob pairs as rows; `LayerArrangement` `[Union]` closes sibling ordering; `LayerOp` `[Union]` closes the structural mutation family; `LayerDelta` admits one named program with its `RedrawPolicy`.
- Entry: `Layers.Ask` is the read window; `Layers.Commit` derives its needs through `SessionNeed.Mutation(custody:, redraw:)`, demands once, and commits through `DocumentCommit.Sealed` — the envelope owner is `Document/commit.md` and this pipeline composes it whole.
- Law: reparent is staged mutation with a cycle guard — the resolved new parent must not be a child of the target — and the root move writes the empty parent id; rename and every face edit ride the same staged-copy-then-`Modify` path, so a failed program never half-writes a live layer.
- Law: merge is object custody before structure and composes `DocumentCommit.Compensated` — the slice's ONE compensation algebra: residents are the fold's source, each re-home a landed key, the source-layer delete the RELEASE step whose refusal unwinds the landed prefix, and the retained attribute snapshots free after the fold settles on BOTH exits, because a release that freed them first would hand the rollback a disposed attribute set. Source equal to target refuses twice — at admission by ADDRESS and inside `Apply` by resolved IDENTITY, which is what catches `ById` and `AtPath` naming one layer.
- Law: purge tallies compose `TableKind.Layers.Reclaim` — the vocabulary row stays the one reclamation delegate — and revive addresses the dead row by id or index under `Liveness.IncludeDeleted`, the only path that may see a deleted layer.
- Law: explicit arrangement admits one complete permutation of every active layer before the native sort boundary.
- Law: `Rollback` is a HOST-SIDE undo of one layer's prior modification and runs INSIDE the sealed record the delta opened. `UndoModify(layerIndex)` with no serial targets the host's CURRENT record — the record this commit is building — so the serial-free arm reverses edits the enclosing commit just landed. A rollback therefore carries its own admitted `UndoSerial` for a PRIOR record, and the serial-free overload is admissible only in a delta landing no other operation on the same layer; a delta mixing `Amend` and serial-free `Rollback` on one target is the deleted form.
- Boundary: layer-table events stay on the events page's `EventFamily` binding, named-layer-state save/restore stays on the presets page, and object relayering by query stays on the tables pipeline; this page enters `document.Objects` only inside the merge arm's custody move.
- Packages: `Document/commit.md` (`DocumentCommit.Sealed`/`Compensated`, `RedrawPolicy`, `HostInteraction`, `UndoSerial`), `Document/session.md` (`SessionNeed.Mutation`, `UndoCustody`, `DraftFault`); `RhinoCommon` layer-table mutation members per the `.api` catalog.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class SortSense {
    public static readonly SortSense Ascending = new(key: true);
    public static readonly SortSense Descending = new(key: false);
}

[SmartEnum<int>]
public sealed partial class DuplicateScope {
    public static readonly DuplicateScope LayerOnly = new(key: 0, objects: false, sublayers: false);
    public static readonly DuplicateScope WithObjects = new(key: 1, objects: true, sublayers: false);
    public static readonly DuplicateScope WithSublayers = new(key: 2, objects: false, sublayers: true);
    public static readonly DuplicateScope Whole = new(key: 3, objects: true, sublayers: true);

    internal bool Objects { get; }
    internal bool Sublayers { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerArrangement {
    private LayerArrangement() { }

    private sealed record ByNameCase(SortSense Sense) : LayerArrangement;
    private sealed record ExplicitCase(Seq<LayerRef> Order) : LayerArrangement;

    public static LayerArrangement ByName(SortSense sense) => new ByNameCase(Sense: sense);

    public static Fin<LayerArrangement> Explicit(params ReadOnlySpan<LayerRef> order) {
        return from values in Admission.All(values: order)
               from _ in guard(!values.IsEmpty, new KernelFault.InvalidInput())
               select (LayerArrangement)new ExplicitCase(Order: values);
    }

    internal Fin<int> Apply(RhinoDoc document) =>
        Switch(
            state: document,
            byNameCase: static (context, arrange) => Try.lift(() => {
                context.Layers.SortByLayerName(bAscending: arrange.Sense.Key);
                return Fin.Succ(value: context.Layers.ActiveCount);
            }).Run().Bind(static inner => inner),
            explicitCase: static (context, arrange) =>
                from indices in arrange.Order
                    .Traverse(address => address.Index(document: context, liveness: Liveness.ActiveOnly)
                        .Map(static index => index.Value)
                        .ToValidation())
                    .As()
                    .ToFin()
                let unique = indices.Distinct()
                from _unique in guard(unique.Count == indices.Count, new KernelFault.InvalidInput())
                from _complete in guard(unique.Count == context.Layers.ActiveCount, new KernelFault.InvalidInput())
                from _ in Try.lift(() => {
                    context.Layers.Sort(layerIndices: unique.AsIterable());
                    return Fin.Succ(value: unit);
                }).Run().Bind(static inner => inner)
                select unique.Count);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerOp {
    private LayerOp() { }

    private sealed record CreateCase(LeafName Name, Option<LayerRef> Parent, Seq<LayerEdit> Edits) : LayerOp;
    private sealed record GraftCase(LayerPath Path, Option<System.Drawing.Color> Color) : LayerOp;
    private sealed record AmendCase(LayerRef Target, Seq<LayerEdit> Edits) : LayerOp;
    private sealed record ReparentCase(LayerRef Target, Option<LayerRef> Parent) : LayerOp;
    private sealed record MergeCase(LayerRef Source, LayerRef Target) : LayerOp;
    private sealed record DuplicateCase(LayerRef Target, DuplicateScope Scope) : LayerOp;
    private sealed record DeleteCase(LayerRef Target, HostInteraction Interaction) : LayerOp;
    private sealed record PurgeCase(LayerRef Target, HostInteraction Interaction) : LayerOp;
    private sealed record ReviveCase(LayerRef Target) : LayerOp;
    private sealed record AnointCase(LayerRef Target, HostInteraction Interaction) : LayerOp;
    private sealed record ExposeCase(LayerRef Target) : LayerOp;
    private sealed record ArrangeCase(LayerArrangement Arrangement) : LayerOp;
    private sealed record RollbackCase(LayerRef Target, Option<UndoSerial> Serial) : LayerOp;
    private sealed record ReclaimCase : LayerOp;

    public static Fin<LayerOp> Create(LeafName name, Option<LayerRef> parent = default, params ReadOnlySpan<LayerEdit> edits) =>
        Admission.All(values: edits)
            .Map(admitted => (LayerOp)new CreateCase(Name: name, Parent: parent, Edits: admitted));

    public static Fin<LayerOp> Graft(LayerPath path, Option<System.Drawing.Color> color = default) =>
        guard(path != default, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (LayerOp)new GraftCase(Path: path, Color: color));

    public static Fin<LayerOp> Amend(LayerRef target, params ReadOnlySpan<LayerEdit> edits) {
        return from address in Admit.Need(target)
               from admitted in Admission.All(values: edits)
               from _ in guard(!admitted.IsEmpty, new KernelFault.InvalidInput())
               select (LayerOp)new AmendCase(Target: address, Edits: admitted);
    }

    public static Fin<LayerOp> Reparent(LayerRef target, Option<LayerRef> parent = default) =>
        Addressed(target: target, mint: address => new ReparentCase(Target: address, Parent: parent));

    public static Fin<LayerOp> Merge(LayerRef source, LayerRef target) {
        return from origin in Admit.Need(source)
               from destination in Admit.Need(target)
               from _ in guard(origin != destination, new KernelFault.InvalidInput())
               select (LayerOp)new MergeCase(Source: origin, Target: destination);
    }

    public static Fin<LayerOp> Duplicate(LayerRef target, DuplicateScope scope) {
        return (Admit.Need(target).ToValidation(), Admit.Need(scope).ToValidation())
            .Apply(static (address, admitted) => (LayerOp)new DuplicateCase(Target: address, Scope: admitted))
            .As()
            .ToFin();
    }

    public static Fin<LayerOp> Delete(LayerRef target, HostInteraction interaction) =>
        Dialogued(target: target, interaction: interaction, mint: static (address, dialogue) =>
            new DeleteCase(Target: address, Interaction: dialogue));

    public static Fin<LayerOp> Purge(LayerRef target, HostInteraction interaction) =>
        Dialogued(target: target, interaction: interaction, mint: static (address, dialogue) =>
            new PurgeCase(Target: address, Interaction: dialogue));

    public static Fin<LayerOp> Revive(LayerRef target) =>
        Addressed(target: target, mint: static address => new ReviveCase(Target: address));

    public static Fin<LayerOp> Anoint(LayerRef target, HostInteraction interaction) =>
        Dialogued(target: target, interaction: interaction, mint: static (address, dialogue) =>
            new AnointCase(Target: address, Interaction: dialogue));

    public static Fin<LayerOp> Expose(LayerRef target) =>
        Addressed(target: target, mint: static address => new ExposeCase(Target: address));

    public static Fin<LayerOp> Arrange(LayerArrangement arrangement) =>
        Optional(arrangement).ToFin(Fail: key.OrDefault().InvalidInput()).Map(order => (LayerOp)new ArrangeCase(Arrangement: order));

    public static Fin<LayerOp> Rollback(LayerRef target, Option<UndoSerial> serial = default) =>
        Addressed(target: target, mint: address => new RollbackCase(Target: address, Serial: serial));

    public static LayerOp Reclaim { get; } = new ReclaimCase();

    private static Fin<LayerOp> Addressed(LayerRef target, Func<LayerRef, LayerOp> mint) =>
        Optional(target).ToFin(Fail: key.OrDefault().InvalidInput()).Map(address => mint(arg: address));

    private static Fin<LayerOp> Dialogued(
        LayerRef target,
        HostInteraction interaction,
        Func<LayerRef, HostInteraction, LayerOp> mint) {
        return (
                Admit.Need(target).ToValidation(),
                Admit.Need(interaction).ToValidation())
            .Apply((address, dialogue) => mint(address, dialogue))
            .As()
            .ToFin();
    }

    internal Fin<Unit> Apply(RhinoDoc document) =>
        Switch(
            document,
            createCase: static (context, edit) =>
                from parent in edit.Parent
                    .Traverse(address => address.Resolve(document: context, liveness: Liveness.ActiveOnly).Map(static layer => layer.Id))
                    .As()
                from index in Try.lift(() => new Lease<Layer>.Owned(Value: new Layer { Name = edit.Name.Value }).Use(
                    state: (Document: context, Parent: parent),
                    project: static (state, minted) => {
                        state.Parent.IfSome(id => minted.ParentLayerId = id);
                        return Fin.Succ(value: state.Layers.Add(layer: minted));
                    })).Run().Bind(static inner => inner)
                from _ in guard(index >= 0, new KernelFault.InvalidResult())
                from _edited in Amended(document: context, index: index, edits: edit.Edits)
                select unit,
            graftCase: static (context, edit) =>
                from index in Try.lift(() => Fin.Succ(value: edit.Color.Match(
                    Some: color => context.Layers.AddPath(layerPath: edit.Path.Value, layerColor: color),
                    None: () => context.Layers.AddPath(layerPath: edit.Path.Value)))).Run().Bind(static inner => inner)
                from _ in guard(index >= 0, new KernelFault.InvalidResult())
                from _stamped in Stamped(document: context, index: index)
                select unit,
            amendCase: static (context, edit) =>
                from index in edit.Target.Index(document: context, liveness: Liveness.ActiveOnly)
                from _edited in Amended(document: context, index: index.Value, edits: edit.Edits)
                select unit,
            reparentCase: static (context, edit) =>
                from target in edit.Target.Resolve(document: context, liveness: Liveness.ActiveOnly)
                from parent in edit.Parent
                    .Traverse(address => address.Resolve(document: context, liveness: Liveness.ActiveOnly))
                    .As()
                from acyclic in guard(
                    parent.Map(candidate => candidate.Id != target.Id && !candidate.IsChildOf(otherlayerId: target.Id)).IfNone(noneValue: true),
                    new KernelFault.InvalidInput())
                from _written in Staged(
                    document: context,
                    index: target.LayerIndex,
                    revise: staged => Try.lift(() => {
                        staged.ParentLayerId = parent.Map(static layer => layer.Id).IfNone(Guid.Empty);
                        return Fin.Succ(value: unit);
                    }).Run().Bind(static inner => inner))
                select unit,
            mergeCase: static (context, edit) =>
                from source in edit.Source.Resolve(document: context, liveness: Liveness.ActiveOnly)
                from target in edit.Target.Resolve(document: context, liveness: Liveness.ActiveOnly)
                from distinct in guard(source.Id != target.Id, new KernelFault.InvalidInput())
                from _merged in Merged(
                    document: context,
                    sourceIndex: source.LayerIndex,
                    targetIndex: target.LayerIndex)
                select unit,
            duplicateCase: static (context, edit) =>
                from index in edit.Target.Index(document: context, liveness: Liveness.ActiveOnly)
                from minted in Try.lift(() => Fin.Succ(value: toSeq(context.Layers.Duplicate(
                    layerIndex: index.Value,
                    duplicateObjects: edit.Scope.Objects,
                    duplicateSublayers: edit.Scope.Sublayers)))).Run().Bind(static inner => inner)
                from _ in guard(!minted.IsEmpty, new KernelFault.InvalidResult())
                from _stamped in minted
                    .Traverse(row => Stamped(document: context, index: row).ToValidation())
                    .As()
                    .ToFin()
                select unit,
            deleteCase: static (context, edit) =>
                from target in edit.Target.Resolve(document: context, liveness: Liveness.ActiveOnly)
                from _ in Admit.Confirm(success: context.Layers.Delete(layerIndex: target.LayerIndex, quiet: edit.Interaction.IsQuiet))
                select unit,
            purgeCase: static (context, edit) =>
                from target in edit.Target.Resolve(document: context, liveness: Liveness.IncludeDeleted)
                from _ in Admit.Confirm(success: context.Layers.Purge(layerIndex: target.LayerIndex, quiet: edit.Interaction.IsQuiet))
                select unit,
            reviveCase: static (context, edit) =>
                from index in edit.Target.Index(document: context, liveness: Liveness.IncludeDeleted)
                from _ in Admit.Confirm(success: context.Layers.Undelete(layerIndex: index.Value))
                from _stamped in Stamped(document: context, index: index.Value)
                select unit,
            anointCase: static (context, edit) =>
                from index in edit.Target.Index(document: context, liveness: Liveness.ActiveOnly)
                from _ in Admit.Confirm(success: context.Layers.SetCurrentLayerIndex(
                    layerIndex: index.Value,
                    quiet: edit.Interaction.IsQuiet))
                from _stamped in Stamped(document: context, index: index.Value)
                select unit,
            exposeCase: static (context, edit) =>
                from target in edit.Target.Resolve(document: context, liveness: Liveness.ActiveOnly)
                from _ in Admit.Confirm(success: context.Layers.ForceLayerVisible(layerId: target.Id))
                select unit,
            arrangeCase: static (context, edit) => edit.Arrangement.Apply(document: context).Map(static _ => unit),
            rollbackCase: static (context, edit) =>
                from index in edit.Target.Index(document: context, liveness: Liveness.ActiveOnly)
                from _ in Admit.Confirm(success: edit.Serial.Match(
                    Some: serial => context.Layers.UndoModify(layerIndex: index.Value, undoRecordSerialNumber: serial.Value),
                    None: () => context.Layers.UndoModify(layerIndex: index.Value)))
                from _stamped in Stamped(document: context, index: index.Value)
                select unit,
            reclaimCase: static (context, _) => TableKind.Layers.Reclaim(document: context)
                .Map(static _ => unit));

    private static Fin<LayerStamp> Stamped(RhinoDoc document, int index) =>
        Optional(document.Layers.FindIndex(index: index))
            .ToFin(Fail: new KernelFault.InvalidResult())
            .Bind(layer => LayerStamp.Of(layer: layer));

    private static Fin<Unit> Amended(RhinoDoc document, int index, Seq<LayerEdit> edits) =>
        edits.IsEmpty
            ? Stamped(document: document, index: index).Map(static _ => unit)
            : Staged(
                document: document,
                index: index,
                revise: staged => edits.TraverseM(edit => edit.Apply(staged: staged)).As().Map(static _ => unit));

    private static Fin<Unit> Staged(RhinoDoc document, int index, Func<Layer, Fin<Unit>> revise) =>
        from live in Optional(document.Layers.FindIndex(index: index)).ToFin(Fail: new KernelFault.MissingContext())
        from landed in Try.lift(() => {
            Layer copy = new();
            copy.CopyAttributesFrom(otherLayer: live);
            return new Lease<Layer>.Owned(Value: copy).Use(
                state: (Document: document, Index: index, Revise: revise),
                project: static (state, staged) =>
                    from revised in state.Revise(arg: staged)
                    from written in Admit.Confirm(success: state.Document.Layers.Modify(
                        newSettings: staged,
                        layerIndex: state.Index,
                        quiet: true))
                    select written);
        }).Run().Bind(static inner => inner)
        from _stamped in Stamped(document: document, index: index)
        select unit;

    private sealed record LayerMove(Guid ObjectId, ObjectAttributes Original);

    private static Fin<Unit> Merged(RhinoDoc document, int sourceIndex, int targetIndex) =>
        from moves in StagedMoves(document: document, sourceIndex: sourceIndex)
        from merged in DocumentCommit.Compensated(
                source: moves,
                land: move => Move(document: document, move: move, targetIndex: targetIndex),
                rollback: landed => Restore(
                    document: document,
                    moves: moves.Filter(move => landed.Exists(id => id == move.ObjectId))),
                release: _ => Admit.Confirm(success: document.Layers.Delete(layerIndex: sourceIndex, quiet: true)))
            .Settled(
                release: () => Custody.Dispose(moves.Map(static move => move.Original)))
            .Map(static _ => unit)
        select merged;

    private static Fin<Seq<LayerMove>> StagedMoves(RhinoDoc document, int sourceIndex) =>
        from index in ResourceIndex.Admit(value: sourceIndex)
        from spec in QuerySpec.Of(
            axes: Some(QueryAxis.Baseline.With(QueryAxis.Hidden).With(QueryAxis.Lights)),
            layer: Some(index))
        from settings in spec.Build(document: document)
        from residents in Try.lift(() => Optional(document.Objects.GetObjectList(settings: settings))
            .ToFin(Fail: new KernelFault.InvalidResult())
            .Map(static values => toSeq(values).Strict())).Run().Bind(static inner => inner)
        from staged in DocumentCommit.Compensated(
            source: residents,
            land: native => Try.lift(() => Optional(native.Attributes?.Duplicate())
                .ToFin(Fail: new KernelFault.InvalidResult())
                .Map(original => new LayerMove(ObjectId: native.Id, Original: original))).Run().Bind(static inner => inner),
            rollback: landed => Fin.Succ(value: HostEdge.Side(() => landed.Iter(static move => move.Original.Dispose()))))
        select staged;

    private static Fin<Guid> Move(RhinoDoc document, LayerMove move, int targetIndex) =>
        from staged in Try.lift(() => Optional(move.Original.Duplicate()).ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
        from _ in new Lease<ObjectAttributes>.Owned(Value: staged).Use(owned => {
            owned.LayerIndex = targetIndex;
            return Admit.Confirm(success: document.Objects.ModifyAttributes(
                objectId: move.ObjectId,
                newAttributes: owned,
                quiet: true));
        })
        select move.ObjectId;

    private static Fin<Unit> Restore(RhinoDoc document, Seq<LayerMove> moves) => moves.Rev()
        .Traverse(move => Try.lift(() => Admit.Confirm(success: document.Objects.ModifyAttributes(
            objectId: move.ObjectId,
            newAttributes: move.Original,
            quiet: true))).Run().Bind(static inner => inner).ToValidation())
        .As()
        .ToFin()
        .Map(static _ => unit);

}
```

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record LayerDelta {
    private LayerDelta(Seq<LayerOp> operations, Option<string> recordName, RedrawPolicy redraw) {
        Operations = operations;
        RecordName = recordName;
        Redraw = redraw;
    }

    public Seq<LayerOp> Operations { get; }
    public Option<string> RecordName { get; }
    public RedrawPolicy Redraw { get; }

    public static Fin<LayerDelta> Of(RedrawPolicy redraw, Option<string> recordName = default, params ReadOnlySpan<LayerOp> operations) {
        return from admitted in Admission.All(values: operations)
               from _ in guard(!admitted.IsEmpty, new KernelFault.InvalidInput())
               select new LayerDelta(Operations: admitted, RecordName: recordName, Redraw: redraw);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class Layers {
    public static Fin<LayerTree> Ask(DocumentSession session, Option<Seq<Guid>> detailViewports = default) {
        return session.Demand(
            use: document => LayerTree.Of(document: document, detailViewports: detailViewports.IfNone(Seq<Guid>())),
            needs: [SessionNeed.Read]);
    }

    public static Fin<Unit> Commit(DocumentSession session, LayerDelta delta) {
        return session.Demand(
            use: document => DocumentCommit.Sealed(
                document: document,
                name: delta.RecordName.IfNone(nameof(Layers)),
                recordsUndo: true,
                redraw: delta.Redraw,
                run: () => delta.Operations
                    .TraverseM(operation => operation.Apply(document: document))
                    .As()
                    .Map(static _ => unit),
                project: Fin.Succ),
            needs: SessionNeed.Mutation(custody: UndoCustody.Recorded, redraw: delta.Redraw).ToArray());
    }
}
```

## [06]-[ORGANIZATION_PROJECTION]

- Entry: `Layers.Ask(session, authority, views)` projects one admitted host-free forest inside the read window; `OrganizationCodec.Encode` is its sole proto-binary producer boundary.
- Law: every name on this egress states the HOST-FREE organizational concept and the Rhino layer vocabulary translates HERE. Publishing `LayerStamp` field-for-field binds every peer decode to one host's layer model, which `libs/.planning/ARCHITECTURE.md` `[03]-[UNIVERSAL_VS_CAPTURE]` forecloses, so the host `Guid`, the `-1`-sentinel table index, and the `::`-joined path each stop at this boundary.
- Law: organizational identity is the content key over the count-framed ancestor label chain, minted through the kernel `CanonicalWriter` — `Rows` count-frames the chain and `String` length-frames each label — so one organizational address keys identically across source documents and a worksession merge unions them. Folding the source key into that preimage is the rejected form, since it re-scopes a federation address down to one file. NAMED LOSS: the prior hand framer wrote its int32 frames big-endian; the kernel writer frames little-endian, so the organizational address RE-KEYS ONCE at this landing — stated here, never re-derived per consumer, and the wire's own field roster, numbers, and 16-byte big-endian key emission are untouched.
- Law: the label chain is LABELS, never a joined path and never a re-rendered standards name. `HostLayerScheme.RhinoPath` already spelled a standards name's fields as the segments `[02]` admits, so the ancestor chain IS the standard's field sequence in order; re-rendering `Rasm.Drawing.LayerName.Text` at this boundary would fold discipline, major, and minor back into ONE label and destroy exactly the framing a peer walks.
- Law: membership targets are FEDERATION keys the authority issues, never host object ids — the host-object-to-entity binding lives in the element projection, which sits outside this plane's reference set. Residents the authority declines land NO edge, so an unclaimed object reads as absent membership rather than as a key no peer resolves.
- Law: sibling order is the recursive repeated-field order itself. Publishing an ordinal beside that list creates two authorities that can disagree; publishing raw `SortIndex` pushes the host's tie-break onto every peer.
- Law: per-view rows land only where `HasPerViewportSettings` proved settings, so row presence IS the evidence and `visible` is the host's resolved answer under them, read off the probed `DetailTrait` set. Persistent visibility never crosses, because the host collapses its own three write states onto a two-state read and a peer column carrying that collapse carries no defined meaning.
- Law: roots and every child list leave in the snapshot's published sibling order; members and view rows remain nested under their owning entity. A detached edge or override table, dictionary enumeration, and last-write collapse are unrepresentable.
- Law: the producer admits the generated rules and the schema-inexpressible forest laws once before bytes leave: at most 65,536 total entities, depth at most 64, globally unique entity keys, unique member and view keys per entity, and an optional current path that resolves exactly. The recursive message makes containment single-parent and acyclic by structure.
- Boundary: render and print product stays host-side evidence — `LayerFace` colours, the `PrintPen` rung, linetype, render material, and section style reach no wire field, and `PerceptualColor` riding a detached payload is the crossing the kernel colour owner already forecloses. The plot product leaves through `LayerFace.PlotOf` onto the CAD egress instead.
- Boundary: Mapperly transcribes the recursive owner; generated Protovalidate proves field/repeated constraints and `OrganizationAdmit` proves only cross-node uniqueness, total/depth, and current resolution. No compatibility arm or second graph admission survives.
- Growth: one appended entity field beside one domain column carries a new axis; every containment, member, and view relation stays nested under its owner.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers.Binary;
using Celly.Protovalidate;
using Google.Protobuf;
// Contracts are retired from this logic.
using Riok.Mapperly.Abstractions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OrganizationEntity(
    UInt128 Key,
    LeafName Name,
    bool Visible,
    bool Locked,
    Seq<OrganizationEntity> Children,
    Seq<string> Members,
    Seq<ViewOverrideFact> Overrides) : IDetachedDocumentResult;

public sealed record ViewOverrideFact(string View, bool Visible) : IDetachedDocumentResult;
public readonly record struct EntityPath(Seq<uint> Indexes);

public sealed record OrganizationFact(
    UInt128 Source,
    string Authority,
    Seq<OrganizationEntity> Roots,
    Option<EntityPath> Current) : IDetachedDocumentResult;

// --- [SERVICES] ------------------------------------------------------------------------
public interface IOrganizationAuthority {
    string Name { get; }
    UInt128 Source { get; }
    Option<string> MemberOf(Guid resident);
    Option<string> ViewOf(Guid viewport);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class Layers {
    public static Fin<OrganizationFact> Ask(
        DocumentSession session,
        IOrganizationAuthority authority,
        Option<Seq<Guid>> views = default) {
        return from admission in Admission.Pair(first: session, second: authority)
               from probes in views.IfNone(Seq<Guid>())
                   .Traverse(view => guard(view != Guid.Empty, new KernelFault.InvalidInput()).ToFin().Map(_ => view).ToValidation())
                   .As()
                   .ToFin()
               from fact in admission.First.Demand(
                   use: document =>
                       from tree in LayerTree.Of(document: document, detailViewports: probes)
                       from projected in Projected(document: document, tree: tree, authority: admission.Second)
                       select projected,
                   needs: [SessionNeed.Read])
               select fact;
    }

    private sealed record ProjectedNode(OrganizationEntity Entity, Option<EntityPath> Current);

    private static Fin<OrganizationFact> Projected(
        RhinoDoc document,
        LayerTree tree,
        IOrganizationAuthority authority) =>
        from issuer in Acceptance.Text(value: authority.Name)
        from residents in Residents(document: document)
        from currentKey in tree.Current.Traverse(stamp => Address(path: stamp.Path)).As()
        from projected in Branches(
            siblings: tree.Roots,
            prefix: Seq<uint>(),
            currentKey: currentKey,
            residents: residents,
            authority: authority)
        from current in currentKey
            .TraverseM(_ => projected.Choose(static row => row.Current).Head.ToFin(new KernelFault.InvalidResult()))
            .As()
        from fact in OrganizationAdmit.Admit(new OrganizationFact(
            Source: authority.Source,
            Authority: issuer,
            Roots: projected.Map(static row => row.Entity),
            Current: current))
        select fact;

    private static Fin<Seq<ProjectedNode>> Branches(
        Seq<LayerNode> siblings,
        Seq<uint> prefix,
        Option<UInt128> currentKey,
        HashMap<int, Seq<Guid>> residents,
        IOrganizationAuthority authority) =>
        toSeq(siblings.Select(static (node, index) => (Node: node, Index: checked((uint)index))))
            .Traverse(row => Branch(
                node: row.Node,
                path: prefix.Add(row.Index),
                currentKey: currentKey,
                residents: residents,
                authority: authority).ToValidation())
            .As()
            .ToFin();

    private static Fin<HashMap<int, Seq<Guid>>> Residents(RhinoDoc document) =>
        from spec in QuerySpec.Of(
            axes: Some(QueryAxis.Baseline.With(QueryAxis.Hidden).With(QueryAxis.Lights)))
        from settings in spec.Build(document: document)
        from natives in Try.lift(() => Optional(document.Objects.GetObjectList(settings: settings))
            .ToFin(Fail: new KernelFault.InvalidResult())
            .Map(static values => toSeq(values).Strict())).Run().Bind(static inner => inner)
        select natives.Fold(
            HashMap<int, Seq<Guid>>(),
            static (held, native) => Optional(native.Attributes).Match(
                Some: attributes => held.AddOrUpdate(
                    key: attributes.LayerIndex,
                    Some: existing => existing.Add(value: native.Id),
                    None: () => Seq(native.Id)),
                None: () => held));

    private static Fin<ProjectedNode> Branch(
        LayerNode node,
        Seq<uint> path,
        Option<UInt128> currentKey,
        HashMap<int, Seq<Guid>> residents,
        IOrganizationAuthority authority) =>
        from key in Address(path: node.Identity.Path)
        from members in residents.Find(node.Identity.Index)
            .IfNone(Seq<Guid>())
            .Choose(resident => authority.MemberOf(resident: resident))
            .Traverse(external => Acceptance.Text(value: external).ToValidation())
            .As()
            .ToFin()
        from overrides in node.Details
            .Choose(detail => authority.ViewOf(viewport: detail.Viewport)
                .Map(view => (View: view, Visible: detail.Conditions.Admits(capability: DetailTrait.Visible))))
            .Traverse(probe => Acceptance.Text(value: probe.View)
                .Map(view => new ViewOverrideFact(View: view, Visible: probe.Visible))
                .ToValidation())
            .As()
            .ToFin()
        from children in Branches(node.Children, path, currentKey, residents, authority)
        select new ProjectedNode(
            Entity: new OrganizationEntity(Name: node.Name,
                Visible: node.Conditions.Admits(capability: LayerTrait.Visible),
                Locked: node.Conditions.Admits(capability: LayerTrait.Locked),
                Children: children.Map(static child => child.Entity),
                Members: members,
                Overrides: overrides),
            Current: currentKey.Filter(candidate => candidate == key).Map(_ => new EntityPath(path))
                .OrElse(children.Choose(static child => child.Current).Head));

    private static Fin<UInt128> Address(LayerPath path) =>
        from chain in path.Segments()
        from key in Try.lift(() => Fin.Succ(value: ContentHash.Of(
            state: chain,
            chunks: static (labels, writer) =>
                _ = writer.Rows(rows: labels, field: static (label, rows) => _ = rows.String(value: label.Value))))).Run().Bind(static inner => inner)
        select key;
}

public static class OrganizationAdmit {
    public const int DepthLimit = 64;
    public const int NodeLimit = 65_536;

    private sealed record Census(HashSet<UInt128> Keys, int Total);

    public static Fin<OrganizationFact> Admit(OrganizationFact fact) =>
        from census in Walk(fact.Roots, depth: 1, new Census(HashSet<UInt128>(), 0))
        from _ in fact.Current.Traverse(path => Resolve(fact.Roots, path)).As()
        select fact;

    private static Fin<Census> Walk(Seq<OrganizationEntity> nodes, int depth, Census held) =>
        nodes.IsEmpty
            ? Fin.Succ(held)
            : depth > DepthLimit
            ? Fin.Fail<Census>(key.OutOfRange(nameof(depth)))
            : nodes.Fold(Fin.Succ(held), (result, node) => result.Bind(census =>
                census.Total >= NodeLimit || census.Keys.Contains(node.Key)
                    ? Fin.Fail<Census>(new KernelFault.InvalidInput(Axis: Some(nameof(OrganizationEntity.Key))))
                    : Walk(
                        node.Children,
                        depth + 1,
                        new Census(census.Keys.Add(node.Key), census.Total + 1))));

    private static Fin<OrganizationEntity> Resolve(Seq<OrganizationEntity> roots, EntityPath path) {
        Seq<OrganizationEntity> level = roots;
        Option<OrganizationEntity> selected = None;
        foreach (uint index in path.Indexes) {
            if (index >= level.Count) return Fin.Fail<OrganizationEntity>(new KernelFault.InvalidInput(Axis: Some(nameof(EntityPath))));
            OrganizationEntity node = level[(int)index];
            selected = Some(node);
            level = node.Children;
        }
        return selected.ToFin(new KernelFault.InvalidInput(Axis: Some(nameof(EntityPath))));
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
public static partial class OrganizationCodec {
    private static readonly Validator Rules = new([OrganizationReflection.Descriptor]);

    public static Fin<ReadOnlyMemory<byte>> Encode(OrganizationFact fact) {
        return from offered in Admit.Need(fact)
               from admitted in OrganizationAdmit.Admit(offered)
               from wire in Try.lift(() => Fin.Succ(Sealed(admitted))).Run().Bind(static inner => inner)
               from _ in Rules.Validate(wire).Count == 0
                   ? Fin.Succ(unit)
                   : Fin.Fail<Unit>(new KernelFault.InvalidInput(Axis: Some(nameof(Organization))))
               from bytes in Try.lift(() => Fin.Succ(value: (ReadOnlyMemory<byte>)wire.ToByteArray())).Run().Bind(static inner => inner)
               select bytes;
    }

    private static Organization Sealed(OrganizationFact fact) {
        Organization wire = Wire(fact: fact);
        fact.Current.IfSome(current => wire.Current = Path(current));
        return wire;
    }

    [MapperIgnoreTarget(nameof(Organization.Current))]
    [MapProperty(nameof(OrganizationFact.Source), nameof(Organization.SourceKey))]
    private static partial Organization Wire(OrganizationFact fact);

    [MapProperty([nameof(OrganizationEntity.Name), nameof(LeafName.Value)], [nameof(Entity.Name)])]
    private static partial Entity Entity(OrganizationEntity entity);

    private static partial Rasm.Contracts.Organization.EntityPath Path(EntityPath path);
    private static partial ViewOverride Override(ViewOverrideFact value);

    [UserMapping]
    private static ByteString Key(UInt128 value) {
        Span<byte> bytes = stackalloc byte[KeyWidth];
        BinaryPrimitives.WriteUInt128BigEndian(destination: bytes, value: value);
        return ByteString.CopyFrom(bytes: bytes);
    }

    private const int KeyWidth = 16;
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]              | [OWNER]                      | [FORM]                             | [ENTRY]                                   |
| :-----: | :--------------------- | :--------------------------- | :--------------------------------- | :---------------------------------------- |
|  [01]   | leaf and path identity | `LeafName` / `LayerPath`     | generated host-grammar values      | `Of` / `Segments` / `Child`               |
|  [02]   | standards crossing     | `StandardLayers`             | both directions of one scheme row  | `Path` / `Name`                           |
|  [03]   | layer addressing       | `LayerRef` / `Liveness`      | address union under a liveness row | `ById` / `AtIndex` / `AtPath` / `Current` |
|  [04]   | detached anchor        | `LayerStamp`                 | id/index/path evidence product     | `Of` / tree nodes                         |
|  [05]   | condition vocabulary   | `LayerTrait` / `DetailTrait` | capability sets under one law      | `Conditions.Admits`                       |
|  [06]   | plot weight            | `PrintPen`                   | ladder rung beside named sentinels | `OfHost` / `Pen` / `LayerFace.PlotOf`     |
|  [07]   | tree topology          | `LayerTree` / `LayerNode`    | one-read acyclic-proved snapshot   | `Layers.Ask` / `Find`                     |
|  [08]   | per-detail overrides   | `LayerOverride`              | slot-rostered write/clear union    | `LayerEdit.Override`                      |
|  [09]   | staged property edits  | `LayerEdit`                  | closed staged-write union          | edit factories / `Amend`                  |
|  [10]   | structural mutation    | `LayerOp`                    | admitted total operation union     | operation factories / `Apply`             |
|  [11]   | sibling ordering       | `LayerArrangement`           | by-name/explicit union             | `LayerOp.Arrange`                         |
|  [12]   | commit program         | `LayerDelta`                 | named redraw-scoped program        | `Layers.Commit`                           |
|  [13]   | host-free organization | `OrganizationFact`           | recursive ordered forest           | `Layers.Ask` with an authority            |
|  [14]   | federation vocabulary  | `IOrganizationAuthority`     | composition-root-bound port        | `MemberOf` / `ViewOf`                     |
|  [15]   | organization egress    | `OrganizationCodec`          | generated organization wire        | `Encode`                                  |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
