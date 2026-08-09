# [RASM_RHINO_LAYERS]

`Rasm.Rhino.Document` owns the layer tree as a managed domain: full-path nesting topology, per-layer render material, linetype, print width, print color, and section style, visibility and locking with their persistent variants, the per-detail-viewport override family, and every structural mutation — create, graft, reparent, merge, duplicate, delete, purge, revive, current-layer anointment, and ordering.

`Layers.Ask` folds two products off one read window: a detached `LayerTree` under parameterized per-detail probe targets, and a host-free `OrganizationFact` when the caller supplies the federation authority. `Layers.Commit` folds an admitted operation program inside one session capability window through the shared `DocumentCommit` envelope, so every structural change seals one undo record and returns one typed fact stream. `TableKind.Layers` stays the identity and reclamation row this page composes for purge tallies; the tree itself is minted here.

## [01]-[INDEX]

- [02]-[IDENTITY_AND_ADDRESS]: `LayerName`, `LayerPath`, `LayerRef`, and the detached `LayerStamp` anchor.
- [03]-[TREE_SNAPSHOT]: `LayerFace`, `LayerCondition`, `DetailFace`, `LayerNode`, and the `LayerTree` detached topology.
- [04]-[EDITS_AND_OVERRIDES]: `LayerEdit` staged-property program beside the `LayerOverride` per-detail family.
- [05]-[COMMIT_RAIL]: `LayerOp`, `LayerDelta`, the `Layers` entry pair, and the `LayerReceipt` fact stream.
- [06]-[ORGANIZATION_PROJECTION]: `OrganizationFact` host-free egress, its `IOrganizationAuthority` port, and the `rasm.organization.v1` codec.
- [07]-[SURFACE_LEDGER]: page owner map.

## [02]-[IDENTITY_AND_ADDRESS]

- Owner: `LayerName` admits one leaf name under the host name rule; `LayerPath` canonicalizes trimmed segments and admits every leaf through that same rule before owning leaf, parent, and child projections. `LayerRef` `[Union]` closes id, index, full-path, and current-layer addressing; `LayerStamp` `[ComplexValueObject]` is the detached identity anchor every receipt row and tree node carries.
- Entry: `LayerRef.ById`/`AtIndex`/`AtPath`/`Current` are the only constructors. Internal `Resolve` admits one live row for every address case, and `Index` projects that row's durable table index without a second lookup.
- Law: a deleted layer is addressable only by id or index with `includeDeleted` — the revive path — so a path address never resolves a dead branch, and every resolution failure is a typed fault, never a `-1` or null leak.
- Boundary: `Layer.PathSeparator`, `GetLeafName`, `GetParentName`, and `IsValidName` are the host path grammar; `LayerPath` composes them once, so no consumer re-derives separator arithmetic or name legality.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// No `using System.Drawing`: `Color` is a host simple name the branch also carries on the kernel colour rail, so
// every colour on this page spells `System.Drawing.Color` in full. `System.Linq` is imported because the sibling
// ordering the tree fold needs has no carrier-side member — `OrderBy`/`ThenBy` are LINQ shapes that leave the
// carrier and re-enter through `toSeq`, and that re-entry is spelled at every use.
using System.Linq;
using Rasm.Domain;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Document;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct LayerName : IDetachedDocumentResult {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = Refusal(value: value);
    }

    internal static ValidationError? Refusal(string value) => value switch {
        "" => new ValidationError(message: "Layer name is blank."),
        var candidate when !Layer.IsValidName(name: candidate) => new ValidationError(message: "Layer name is rejected by the host name rule."),
        var candidate when candidate.Contains(value: Layer.PathSeparator, comparisonType: StringComparison.Ordinal) =>
            new ValidationError(message: "Layer name carries the path separator."),
        _ => null,
    };

    public static Fin<LayerName> Of(string value, Op? key = null) =>
        key.OrDefault().AcceptValidated<LayerName>(candidate: value);
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct LayerPath : IDetachedDocumentResult {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        string raw = value?.Trim() ?? string.Empty;
        string[] segments = raw.Split(separator: Layer.PathSeparator, options: StringSplitOptions.TrimEntries);
        value = string.Join(Layer.PathSeparator, segments);
        validationError = raw.Length is 0
            ? new ValidationError(message: "Layer path is blank.")
            : toSeq(segments).Choose(static segment => Optional(LayerName.Refusal(value: segment))).Head.IfNone(defaultValue: null!);
    }

    public Fin<Seq<LayerName>> Segments(Op? key = null) {
        Op op = key.OrDefault();
        return toSeq(Value.Split(separator: Layer.PathSeparator, options: StringSplitOptions.TrimEntries))
            .Traverse(segment => LayerName.Of(value: segment, key: op).ToValidation())
            .As()
            .ToFin();
    }

    public Fin<LayerName> Leaf(Op? key = null) =>
        LayerName.Of(value: Layer.GetLeafName(fullPath: Value), key: key);

    public Option<LayerPath> Parent =>
        Optional(Layer.GetParentName(fullPath: Value))
            .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
            .Bind(value => Of(value: value).ToOption());

    public Fin<LayerPath> Child(LayerName name, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in guard(name != default, op.InvalidInput()).ToFin().Map(_ => name)
               from path in Of(value: $"{Value}{Layer.PathSeparator}{admitted.Value}", key: op)
               select path;
    }

    public static Fin<LayerPath> Of(string value, Op? key = null) =>
        key.OrDefault().AcceptValidated<LayerPath>(candidate: value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerRef {
    private LayerRef() { }

    private sealed record IdCase(ResourceId Value) : LayerRef;
    private sealed record IndexCase(ResourceIndex Value) : LayerRef;
    private sealed record PathCase(LayerPath Value) : LayerRef;
    private sealed record CurrentCase : LayerRef;

    public static Fin<LayerRef> ById(Guid value, Op? key = null) =>
        ResourceId.Admit(value: value, key: key.OrDefault()).Map(static id => (LayerRef)new IdCase(Value: id));

    public static Fin<LayerRef> AtIndex(int value, Op? key = null) =>
        ResourceIndex.Admit(value: value, key: key.OrDefault()).Map(static index => (LayerRef)new IndexCase(Value: index));

    public static Fin<LayerRef> AtPath(LayerPath value, Op? key = null) =>
        guard(value != default, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (LayerRef)new PathCase(Value: value));

    public static LayerRef Current { get; } = new CurrentCase();

    // The spine's index value object, not a bare `int`: the host's own miss sentinel for this table is `-1`, and a
    // raw int carries it into every consumer that reads a resolved index as a live slot.
    internal Fin<ResourceIndex> Index(RhinoDoc document, bool includeDeleted, Op key) =>
        Resolve(document: document, includeDeleted: includeDeleted, key: key)
            .Bind(row => ResourceIndex.Admit(value: row.LayerIndex, key: key));

    internal Fin<Layer> Resolve(RhinoDoc document, bool includeDeleted, Op key) =>
        Switch(
            state: (Document: document, IncludeDeleted: includeDeleted, Op: key),
            idCase: static (context, address) =>
                from index in context.Op.Catch(() => ResourceIndex.Admit(
                    value: context.Document.Layers.Find(
                        layerId: address.Value.Value,
                        ignoreDeletedLayers: !context.IncludeDeleted),
                    key: context.Op))
                from row in Optional(context.Document.Layers.FindIndex(index: index.Value)).ToFin(Fail: context.Op.MissingContext())
                from admitted in guard(context.IncludeDeleted || !row.IsDeleted, context.Op.MissingContext()).ToFin()
                select row,
            indexCase: static (context, address) =>
                from row in Optional(context.Document.Layers.FindIndex(index: address.Value.Value)).ToFin(Fail: context.Op.MissingContext())
                from admitted in guard(context.IncludeDeleted || !row.IsDeleted, context.Op.MissingContext()).ToFin()
                select row,
            pathCase: static (context, address) =>
                from index in context.Op.Catch(() => ResourceIndex.Admit(
                    value: context.Document.Layers.FindByFullPath(
                        layerPath: address.Value.Value,
                        notFoundReturnValue: NoLayer),
                    key: context.Op))
                from row in Optional(context.Document.Layers.FindIndex(index: index.Value)).ToFin(Fail: context.Op.MissingContext())
                from admitted in guard(!row.IsDeleted, context.Op.MissingContext()).ToFin()
                select row,
            currentCase: static (context, _) => Optional(context.Document.Layers.CurrentLayer)
                .Filter(static row => !row.IsDeleted)
                .ToFin(Fail: context.Op.InvalidResult()));

    // The host's own not-found answer for this table, named once so no lookup re-spells it and `ResourceIndex`
    // refuses it on the same line that reads it.
    private const int NoLayer = -1;
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
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
            ? new ValidationError(message: "Layer stamp identity is incomplete.")
            : null;

    internal static Fin<LayerStamp> Of(Layer layer, Op key) =>
        from source in Optional(layer).ToFin(Fail: key.MissingContext())
        from path in LayerPath.Of(value: source.FullPath, key: key)
        from stamp in Admission.Admitted(
            fault: Validate(source.Id, source.LayerIndex, path, out LayerStamp? admitted),
            value: admitted,
            refusal: key.InvalidResult())
        select stamp;
}
```

## [03]-[TREE_SNAPSHOT]

- Owner: `LayerFace` carries the render/print product — draw color, print color, print width, linetype index, render-material index, section-style index — and `LayerCondition` the state product: visibility, locking, their persistent variants, expansion, currency, deletion, and reference custody. `DetailFace` is one probed per-detail override row; `LayerNode` is one detached tree node; `LayerTree` is the whole topology from one read.
- Entry: `Layers.Ask(session, key, detailViewports)` demands `SessionNeed.Read` and mints the tree inside one callback; probe targets are call data, so the same entry answers the plain topology and any per-detail audit without a second surface.
- Law: the tree is built from one table sweep — non-deleted rows keyed by id, children grouped by `ParentLayerId`, roots at the empty parent, siblings ordered by `SortIndex` then name — so parent/child evidence is structural, never re-derived per consumer from path text.
- Law: the assembly is BOUNDED and never descends. `ParentLayerId` is a raw host id the table does not prove acyclic, so a recursive child walk over a cycle overflows the stack — an uncatchable process death this rail cannot report. Depth is measured by a budgeted parent climb, a cycle and an orphan each refuse typed and named, and the tree assembles deepest-first in one fold; a post-hoc count guard is the deleted form, because it can only run after the walk that would not have returned.
- Law: the host exposes no roster of viewports carrying overrides, so per-detail evidence is probe-parameterized: each requested viewport lands a `DetailFace` only where `HasPerViewportSettings` proves one, and an unprobed override is absent evidence, never a fabricated default.
- Boundary: every node is detached — the live `Layer` handle dies inside the demand window, and `LayerTree` implements `IDetachedDocumentResult` so it crosses out of `Demand` by construction.
- Boundary: persistent visibility and locking are THREE-state on the write side and TWO-state on the read side, and the host closes no probe over the gap. `SetPersistentVisibility`/`UnsetPersistentVisibility` set and clear the explicit setting, but `GetPersistentVisibility` answers a bare `bool` that returns the layer's CURRENT `IsVisible` when nothing was ever set — so an explicit `true` and an unset-and-visible layer read identically, and no managed member distinguishes them. `LayerCondition` therefore reports the host's collapsed answer and names it as such: the column is what the host resolves, never proof that a setting exists. The edit side keeps all three states because it writes through the pair the host does expose.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record LayerFace(
    Color Color,
    Color PrintColor,
    double PrintWidth,
    int LinetypeIndex,
    int RenderMaterialIndex,
    int SectionStyleIndex) : IDetachedDocumentResult {
    internal static LayerFace Of(Layer layer) => new(
        Color: layer.Color,
        PrintColor: layer.PlotColor,
        PrintWidth: layer.PlotWeight,
        LinetypeIndex: layer.LinetypeIndex,
        RenderMaterialIndex: layer.RenderMaterialIndex,
        SectionStyleIndex: layer.SectionStyleIndex);
}

public sealed record LayerCondition(
    bool Visible,
    bool Locked,
    bool PersistentVisibility,
    bool PersistentLocking,
    bool Expanded,
    bool Current,
    bool Deleted,
    bool Reference) : IDetachedDocumentResult {
    internal static LayerCondition Of(Layer layer) => new(
        Visible: layer.IsVisible,
        Locked: layer.IsLocked,
        PersistentVisibility: layer.GetPersistentVisibility(),
        PersistentLocking: layer.GetPersistentLocking(),
        Expanded: layer.IsExpanded,
        Current: layer.IsCurrent,
        Deleted: layer.IsDeleted,
        Reference: layer.IsReference);
}

public sealed record DetailFace(
    Guid Viewport,
    Color Color,
    Color PrintColor,
    double PrintWidth,
    bool Visible,
    bool PersistentVisibility) : IDetachedDocumentResult {
    internal static Option<DetailFace> Probe(Layer layer, Guid viewport) =>
        layer.HasPerViewportSettings(viewportId: viewport)
            ? Some(new DetailFace(
                Viewport: viewport,
                Color: layer.PerViewportColor(viewportId: viewport),
                PrintColor: layer.PerViewportPlotColor(viewportId: viewport),
                PrintWidth: layer.PerViewportPlotWeight(viewportId: viewport),
                Visible: layer.PerViewportIsVisible(viewportId: viewport),
                PersistentVisibility: layer.PerViewportPersistentVisibility(viewportId: viewport)))
            : None;
}

public sealed record LayerNode(
    LayerStamp Identity,
    LayerName Name,
    Option<Guid> Parent,
    LayerFace Face,
    LayerCondition Condition,
    int SortIndex,
    Seq<DetailFace> Details,
    Seq<LayerNode> Children) : IDetachedDocumentResult {
    public Seq<LayerNode> Flatten() => this.Cons(Children.Bind(static child => child.Flatten()));
}

public sealed record LayerTree(Seq<LayerNode> Roots, int Count, Option<LayerStamp> Current) : IDetachedDocumentResult {
    public Seq<LayerNode> Flatten() => Roots.Bind(static root => root.Flatten());

    public Option<LayerNode> Find(LayerRef address) => address.Switch(
        state: this,
        idCase: static (tree, target) => tree.Flatten().Find(node => node.Identity.Id == target.Value.Value),
        indexCase: static (tree, target) => tree.Flatten().Find(node => node.Identity.Index == target.Value.Value),
        pathCase: static (tree, target) => tree.Flatten().Find(node => node.Identity.Path == target.Value),
        currentCase: static (tree, _) => tree.Current.Bind(stamp => tree.Flatten().Find(node => node.Identity.Id == stamp.Id)));

    internal static Fin<LayerTree> Of(RhinoDoc document, Seq<Guid> detailViewports, Op key) => key.Catch(() => {
        Seq<Layer> rows = toSeq(document.Layers.AsIterable()).Filter(static row => !row.IsDeleted).Strict();
        return from nodes in rows
            .Traverse(row => Leaf(layer: row, detailViewports: detailViewports, key: key).ToValidation())
            .As()
            .ToFin()
        from roots in Assembled(nodes: nodes, key: key)
        from current in Optional(document.Layers.CurrentLayer)
            .Traverse(layer => LayerStamp.Of(layer: layer, key: key))
            .As()
        select new LayerTree(Roots: roots, Count: nodes.Count, Current: current);
    });

    private static Fin<LayerNode> Leaf(Layer layer, Seq<Guid> detailViewports, Op key) =>
        from identity in LayerStamp.Of(layer: layer, key: key)
        from name in LayerName.Of(value: layer.Name, key: key)
        select new LayerNode(
            Identity: identity,
            Name: name,
            Parent: Optional(layer.ParentLayerId).Filter(static parent => parent != Guid.Empty),
            Face: LayerFace.Of(layer: layer),
            Condition: LayerCondition.Of(layer: layer),
            SortIndex: layer.SortIndex,
            Details: detailViewports.Choose(viewport => DetailFace.Probe(layer: layer, viewport: viewport)),
            Children: Seq<LayerNode>());

    // `ParentLayerId` is a raw host id with no acyclicity guarantee: a corrupt archive, a worksession merge, or a
    // table read mid-edit can present A parented to B parented to A. A recursive descent over that graph does not
    // fault — it exhausts the stack, which no `Catch` on this rail observes and no receipt can report, and the
    // process dies. The build is therefore an explicit bounded walk: every node's depth is measured by climbing
    // its parent chain under a budget of the node count, so a cycle exhausts the budget as a typed refusal naming
    // the offending path and a parent id matching no row refuses as a typed orphan. Assembly then runs
    // DEEPEST-FIRST over the measured rows, so each node is rebuilt only after every one of its children already
    // is — one ordered fold, no descent, no second pass, and no `Flatten().Count` guard that could only notice
    // the damage after a walk that could not survive it.
    private static Fin<Seq<LayerNode>> Assembled(Seq<LayerNode> nodes, Op key) {
        HashMap<Guid, LayerNode> byId = toHashMap(nodes.Map(static node => (node.Identity.Id, node)));
        HashMap<Guid, Seq<Guid>> childIds = nodes.Fold(
            HashMap<Guid, Seq<Guid>>(),
            static (held, node) => node.Parent.Match(
                Some: parent => held.AddOrUpdate(
                    key: parent,
                    Some: existing => existing.Add(value: node.Identity.Id),
                    None: () => Seq(node.Identity.Id)),
                None: () => held));
        return nodes
            .Traverse(node => Depth(node: node, byId: byId, key: key)
                .Map(depth => (Node: node, Depth: depth))
                .ToValidation())
            .As()
            .ToFin()
            .Map(measured => toSeq(measured.OrderByDescending(static row => row.Depth))
                .Fold(byId, (held, row) => held.AddOrUpdate(
                    key: row.Node.Identity.Id,
                    value: held.Find(row.Node.Identity.Id).IfNone(row.Node) with {
                        Children = Sorted(rows: childIds.Find(row.Node.Identity.Id)
                            .IfNone(Seq<Guid>())
                            .Choose(child => held.Find(child))),
                    })))
            .Map(held => Sorted(rows: toSeq(nodes)
                .Filter(static node => node.Parent.IsNone)
                .Choose(node => held.Find(node.Identity.Id))));
    }

    // Exemption: the parent climb is a bounded statement loop — the budget IS the refusal, so expressing it as a
    // fold would either lose the early exit or need a sentinel state richer than the answer.
    private static Fin<int> Depth(LayerNode node, HashMap<Guid, LayerNode> byId, Op key) {
        Option<Guid> parent = node.Parent;
        int depth = 0;
        while (parent.Case is Guid id) {
            if (depth > byId.Count) {
                return Fin.Fail<int>(error: key.InvalidResult(detail: node.Identity.Path.Value));
            }
            if (byId.Find(id).Case is not LayerNode owner) {
                return Fin.Fail<int>(error: key.MissingContext());
            }
            parent = owner.Parent;
            depth += 1;
        }
        return Fin.Succ(value: depth);
    }

    // Sibling order is the host's own: sort index first, then name under the case-insensitive comparison the layer
    // table itself uses. The ordering leaves the carrier, so it re-enters through `toSeq` at the one site.
    private static Seq<LayerNode> Sorted(Seq<LayerNode> rows) =>
        toSeq(rows
            .OrderBy(static node => node.SortIndex)
            .ThenBy(static node => node.Name.Value, StringComparer.OrdinalIgnoreCase));
}
```

## [04]-[EDITS_AND_OVERRIDES]

- Owner: `LayerOverride` `[Union]` closes the per-detail-viewport family — color, visibility, persistent visibility, print color, print width, the new-detail visibility default, and the whole-viewport purge — with one `Option` per payload discriminating write from clear, so set and delete are one case, never sibling verbs. `LayerEdit` `[Union]` closes every staged property write, and `LayerFlag` rows collapse visible, locked, and expanded assignments into one behavior case.
- Entry: edit factories admit payloads once — finite print width, nonnegative resource indexes, admitted names — and `Apply` runs each case against the staged layer copy inside the commit callback.
- Law: a persistent-visibility or persistent-locking edit carries `Option<bool>`: a value writes `SetPersistent*`, absence runs `UnsetPersistent*`, so the host's three-state persistence is one case rather than a set/unset verb pair.
- Law: section style is two independent axes — the table index and the custom carrier — and the custom axis clears through absence, mirroring the host `SetCustomSectionStyle`/`RemoveCustomSectionStyle` pair as one case.
- Boundary: every override member on `Layer` is a void host write; each arm crosses through `Op.Catch`, and the staged copy never leaves the callback, so a failed edit program leaves the live table untouched until `Modify` lands the whole staged state.
- Boundary: `Layer` inherits `IDisposable` through `ModelComponent`/`CommonObject`, and `Add`/`Modify` copy their argument into the table, so every caller-minted `Layer` — the created row and the staged copy alike — rides `Lease<Layer>.Owned(...).Use(...)`; a live row read back through `FindIndex` or `CurrentLayer` is table-owned and never leased.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerOverride {
    private LayerOverride() { }

    private sealed record ColorCase(Guid Viewport, Option<System.Drawing.Color> Value) : LayerOverride;
    private sealed record VisibleCase(Guid Viewport, Option<bool> Value) : LayerOverride;
    private sealed record PersistentVisibilityCase(Guid Viewport, Option<bool> Value) : LayerOverride;
    private sealed record PrintColorCase(Guid Viewport, Option<System.Drawing.Color> Value) : LayerOverride;
    private sealed record PrintWidthCase(Guid Viewport, Option<double> Value) : LayerOverride;
    private sealed record NewDetailVisibilityCase(bool Value) : LayerOverride;
    private sealed record PurgeCase(Guid Viewport) : LayerOverride;

    public static Fin<LayerOverride> Color(Guid viewport, Option<System.Drawing.Color> value = default, Op? key = null) =>
        Addressed(viewport: viewport, key: key, mint: address => new ColorCase(Viewport: address, Value: value));

    public static Fin<LayerOverride> Visible(Guid viewport, Option<bool> value = default, Op? key = null) =>
        Addressed(viewport: viewport, key: key, mint: address => new VisibleCase(Viewport: address, Value: value));

    public static Fin<LayerOverride> PersistentVisibility(Guid viewport, Option<bool> value = default, Op? key = null) =>
        Addressed(viewport: viewport, key: key, mint: address => new PersistentVisibilityCase(Viewport: address, Value: value));

    public static Fin<LayerOverride> PrintColor(Guid viewport, Option<System.Drawing.Color> value = default, Op? key = null) =>
        Addressed(viewport: viewport, key: key, mint: address => new PrintColorCase(Viewport: address, Value: value));

    public static Fin<LayerOverride> PrintWidth(Guid viewport, Option<double> value = default, Op? key = null) =>
        from admitted in value.Traverse(width => LayerEdit.Width(value: width, op: key.OrDefault())).As()
        from minted in Addressed(
            viewport: viewport,
            key: key,
            mint: address => new PrintWidthCase(Viewport: address, Value: admitted))
        select minted;

    public static LayerOverride NewDetailVisibility(bool value) => new NewDetailVisibilityCase(Value: value);

    public static Fin<LayerOverride> Purge(Guid viewport, Op? key = null) =>
        Addressed(viewport: viewport, key: key, mint: address => new PurgeCase(Viewport: address));

    private static Fin<LayerOverride> Addressed(Guid viewport, Op? key, Func<Guid, LayerOverride> mint) =>
        guard(viewport != Guid.Empty, key.OrDefault().InvalidInput()).ToFin().Map(_ => mint(arg: viewport));

    internal Fin<Unit> Apply(Layer layer, Op key) =>
        Switch(
            state: (Target: layer, Op: key),
            colorCase: static (context, edit) => LayerEdit.Toggle(op: context.Op, value: edit.Value,
                set: value => context.Target.SetPerViewportColor(viewportId: edit.Viewport, color: value),
                clear: () => context.Target.DeletePerViewportColor(viewportId: edit.Viewport)),
            visibleCase: static (context, edit) => LayerEdit.Toggle(op: context.Op, value: edit.Value,
                set: value => context.Target.SetPerViewportVisible(viewportId: edit.Viewport, visible: value),
                clear: () => context.Target.DeletePerViewportVisible(viewportId: edit.Viewport)),
            persistentVisibilityCase: static (context, edit) => LayerEdit.Toggle(op: context.Op, value: edit.Value,
                set: value => context.Target.SetPerViewportPersistentVisibility(viewportId: edit.Viewport, persistentVisibility: value),
                clear: () => context.Target.UnsetPerViewportPersistentVisibility(viewportId: edit.Viewport)),
            printColorCase: static (context, edit) => LayerEdit.Toggle(op: context.Op, value: edit.Value,
                set: value => context.Target.SetPerViewportPlotColor(viewportId: edit.Viewport, color: value),
                clear: () => context.Target.DeletePerViewportPlotColor(viewportId: edit.Viewport)),
            printWidthCase: static (context, edit) => LayerEdit.Toggle(op: context.Op, value: edit.Value,
                set: value => context.Target.SetPerViewportPlotWeight(viewportId: edit.Viewport, plotWeight: value),
                clear: () => context.Target.DeletePerViewportPlotWeight(viewportId: edit.Viewport)),
            newDetailVisibilityCase: static (context, edit) => LayerEdit.Write(op: context.Op, write: () => context.Target.PerViewportIsVisibleInNewDetails = edit.Value),
            purgeCase: static (context, edit) => LayerEdit.Write(op: context.Op, write: () => context.Target.DeletePerViewportSettings(viewportId: edit.Viewport)));
}

[SmartEnum<int>]
internal sealed partial class LayerFlag {
    public static readonly LayerFlag Visible = new(
        key: 0,
        set: static (layer, value) => layer.IsVisible = value);
    public static readonly LayerFlag Locked = new(
        key: 1,
        set: static (layer, value) => layer.IsLocked = value);
    public static readonly LayerFlag Expanded = new(
        key: 2,
        set: static (layer, value) => layer.IsExpanded = value);

    [UseDelegateFromConstructor]
    internal partial void Set(Layer layer, bool value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerEdit {
    private LayerEdit() { }

    private sealed record RenameCase(LayerName Name) : LayerEdit;
    private sealed record ColorCase(System.Drawing.Color Value) : LayerEdit;
    private sealed record PrintColorCase(System.Drawing.Color Value) : LayerEdit;
    private sealed record PrintWidthCase(double Value) : LayerEdit;
    private sealed record LinetypeCase(int Index) : LayerEdit;
    private sealed record RenderMaterialCase(int Index) : LayerEdit;
    private sealed record SectionStyleIndexCase(int Index) : LayerEdit;
    private sealed record CustomSectionStyleCase(Option<SectionStyle> Value) : LayerEdit;
    private sealed record FlagCase(LayerFlag Flag, bool Value) : LayerEdit;
    private sealed record PersistentVisibilityCase(Option<bool> Value) : LayerEdit;
    private sealed record PersistentLockingCase(Option<bool> Value) : LayerEdit;
    private sealed record DescriptionCase(Option<string> Value) : LayerEdit;
    private sealed record IgesLevelCase(int Value) : LayerEdit;
    private sealed record OverrideCase(LayerOverride Value) : LayerEdit;

    public static LayerEdit Rename(LayerName name) => new RenameCase(Name: name);

    public static LayerEdit Recolor(System.Drawing.Color value) => new ColorCase(Value: value);

    public static LayerEdit PrintColor(System.Drawing.Color value) => new PrintColorCase(Value: value);

    public static Fin<LayerEdit> PrintWidth(double value, Op? key = null) =>
        Width(value: value, op: key.OrDefault()).Map(static admitted => (LayerEdit)new PrintWidthCase(Value: admitted));

    public static Fin<LayerEdit> Linetype(int index, Op? key = null) =>
        Indexed(index: index, floor: 0, key: key, mint: static value => new LinetypeCase(Index: value));

    public static Fin<LayerEdit> RenderMaterial(int index, Op? key = null) =>
        Indexed(index: index, floor: Unassigned, key: key, mint: static value => new RenderMaterialCase(Index: value));

    public static Fin<LayerEdit> SectionStyleIndex(int index, Op? key = null) =>
        Indexed(index: index, floor: Unassigned, key: key, mint: static value => new SectionStyleIndexCase(Index: value));

    public static LayerEdit CustomSectionStyle(Option<SectionStyle> value = default) => new CustomSectionStyleCase(Value: value);

    public static LayerEdit Visibility(bool value) => new FlagCase(Flag: LayerFlag.Visible, Value: value);

    public static LayerEdit Locking(bool value) => new FlagCase(Flag: LayerFlag.Locked, Value: value);

    public static LayerEdit PersistentVisibility(Option<bool> value = default) => new PersistentVisibilityCase(Value: value);

    public static LayerEdit PersistentLocking(Option<bool> value = default) => new PersistentLockingCase(Value: value);

    public static LayerEdit Expansion(bool value) => new FlagCase(Flag: LayerFlag.Expanded, Value: value);

    // `AcceptText` refuses blank, so the text factory could never express "clear the description" — a caller
    // wanting an empty description had no admitted path and the only way through was to bypass admission. Clearing
    // is its own construction, and the case carries `Option`: a value writes, absence writes the empty string the
    // host stores for "no description".
    public static Fin<LayerEdit> Description(string value, Op? key = null) =>
        key.OrDefault().AcceptText(value: value)
            .Map(admitted => (LayerEdit)new DescriptionCase(Value: Some(admitted)));

    public static LayerEdit ClearDescription() => new DescriptionCase(Value: Option<string>.None);

    public static Fin<LayerEdit> IgesLevel(int value, Op? key = null) =>
        Indexed(index: value, floor: 0, key: key, mint: static admitted => new IgesLevelCase(Value: admitted));

    public static LayerEdit Override(LayerOverride value) => new OverrideCase(Value: value);

    private static Fin<LayerEdit> Indexed(int index, int floor, Op? key, Func<int, LayerEdit> mint) =>
        guard(index >= floor, key.OrDefault().InvalidInput()).ToFin().Map(_ => mint(arg: index));

    // The host's own "no component assigned" slot for the render-material and section-style columns.
    private const int Unassigned = -1;

    internal Fin<Unit> Apply(Layer staged, Op key) =>
        Switch(
            state: (Staged: staged, Op: key),
            renameCase: static (context, edit) => Write(op: context.Op, write: () => context.Staged.Name = edit.Name.Value),
            colorCase: static (context, edit) => Write(op: context.Op, write: () => context.Staged.Color = edit.Value),
            printColorCase: static (context, edit) => Write(op: context.Op, write: () => context.Staged.PlotColor = edit.Value),
            printWidthCase: static (context, edit) => Write(op: context.Op, write: () => context.Staged.PlotWeight = edit.Value),
            linetypeCase: static (context, edit) => Write(op: context.Op, write: () => context.Staged.LinetypeIndex = edit.Index),
            renderMaterialCase: static (context, edit) => Write(op: context.Op, write: () => context.Staged.RenderMaterialIndex = edit.Index),
            sectionStyleIndexCase: static (context, edit) => Write(op: context.Op, write: () => context.Staged.SectionStyleIndex = edit.Index),
            customSectionStyleCase: static (context, edit) => Toggle(op: context.Op, value: edit.Value,
                set: style => context.Staged.SetCustomSectionStyle(sectionStyle: style),
                clear: context.Staged.RemoveCustomSectionStyle),
            flagCase: static (context, edit) => Write(
                op: context.Op,
                write: () => edit.Flag.Set(context.Staged, edit.Value)),
            persistentVisibilityCase: static (context, edit) => Toggle(op: context.Op, value: edit.Value,
                set: value => context.Staged.SetPersistentVisibility(persistentVisibility: value),
                clear: context.Staged.UnsetPersistentVisibility),
            persistentLockingCase: static (context, edit) => Toggle(op: context.Op, value: edit.Value,
                set: value => context.Staged.SetPersistentLocking(persistentLocking: value),
                clear: context.Staged.UnsetPersistentLocking),
            descriptionCase: static (context, edit) => Write(
                op: context.Op,
                write: () => context.Staged.Description = edit.Value.IfNone(string.Empty)),
            igesLevelCase: static (context, edit) => Write(op: context.Op, write: () => context.Staged.IgesLevel = edit.Value),
            overrideCase: static (context, edit) => edit.Value.Apply(layer: context.Staged, key: context.Op));

    internal static Fin<Unit> Write(Op op, Action write) =>
        op.Catch(() => {
            write();
            return Fin.Succ(value: unit);
        });

    // `Match` over two `Unit`-returning arms; the prior spelling wrapped each side in `fun(...)` and invoked the
    // resulting delegate on the same line, which allocates a closure per call to express what `Match` already is.
    internal static Fin<Unit> Toggle<T>(Op op, Option<T> value, Action<T> set, Action clear) =>
        Write(op: op, write: () => value.Match(
            Some: chosen => set(obj: chosen),
            None: () => clear()));

    internal static Fin<double> Width(double value, Op op) =>
        guard(double.IsFinite(value) && value >= -1.0, op.InvalidInput()).ToFin().Map(_ => value);
}
```

## [05]-[COMMIT_RAIL]

- Owner: `LayerOp` `[Union]` closes the structural mutation family; `LayerArrangement` `[Union]` closes sibling ordering; `LayerDelta` admits one named program with its `RedrawPolicy`; `LayerSlot` `[SmartEnum<int>]` names structural consequences, and `LayerReceipt` is the additive fold over one internal `LayerFact` stream with the sealed undo serial as a fact.
- Entry: `Layers.Ask` is the read window; `Layers.Commit` derives its needs through `SessionNeed.Mutation`, demands once, and commits through `DocumentCommit.Sealed` — suppress, fold every operation into one receipt, seal the record with the serial stamped as an `UndoCase` fact, restore redraw state on every outcome, then repaint after restoration so a suppressing policy still lands its terminal redraw.
- Law: reparent is staged mutation with a cycle guard — the resolved new parent must not be a child of the target — and the root move writes the empty parent id; rename and every face edit ride the same staged-copy-then-`Modify` path, so a failed program never half-writes a live layer.
- Law: merge is object custody before structure, and it composes `DocumentCommit.Compensated` — the slice's ONE compensation algebra — rather than re-deriving it: the residents are the fold's source, each re-home is a landed key, the source-layer delete is the RELEASE step whose refusal unwinds the landed prefix, and the retained attribute snapshots free after the fold settles on both exits, because a release that freed them first would hand the rollback a disposed attribute set. Cleanup faults land beside the tally as `Seq<Error>`, foldable back onto the rail; source equal to target is refused twice — at admission by ADDRESS, where `LayerRef`'s structural equality catches only two identical addresses, and again inside `Apply` by resolved IDENTITY, which is what catches `ById` and `AtPath` naming one layer.
- Law: purge tallies compose `TableKind.Layers.Reclaim` — the vocabulary row stays the one reclamation delegate — and revive addresses the dead row by id or index with deleted resolution, the only path that may see a deleted layer.
- Law: explicit arrangement admits one complete permutation of every active layer before the native sort boundary.
- Law: `Rollback` is a HOST-SIDE undo of one layer's prior modification and it runs INSIDE the sealed record the delta opened. `UndoModify(layerIndex)` with no serial targets the host's current record — which is the record this commit is building — so the arm reverses edits the same envelope just landed and the seal then stamps a serial over a record whose content has already been withdrawn. A rollback therefore carries its own `UndoSerial` for a PRIOR record, and the serial-free overload is admissible only in a delta that lands no other operation on the same layer; a delta mixing `Amend` and serial-free `Rollback` on one target is the deleted form.
- Boundary: layer-table events stay on the events page's `EventFamily.LayerTable` binding, named-layer-state save/restore stays on the presets page, and object relayering by query stays on the tables rail; this page enters `document.Objects` only inside the merge arm's custody move.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerArrangement {
    private LayerArrangement() { }

    private sealed record ByNameCase(bool Ascending) : LayerArrangement;
    private sealed record ExplicitCase(Seq<LayerRef> Order) : LayerArrangement;

    public static LayerArrangement ByName(bool ascending) => new ByNameCase(Ascending: ascending);

    public static Fin<LayerArrangement> Explicit(params ReadOnlySpan<LayerRef> order) {
        Op op = Op.Of();   // an optional before `params` forecloses the positional spread — the key mints at the entry
        return from values in Admission.All(values: order, key: op)
               from _ in guard(!values.IsEmpty, op.InvalidInput()).ToFin()
               select (LayerArrangement)new ExplicitCase(Order: values);
    }

    internal Fin<int> Apply(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            byNameCase: static (context, arrange) => context.Op.Catch(() => {
                context.Document.Layers.SortByLayerName(bAscending: arrange.Ascending);
                return Fin.Succ(value: context.Document.Layers.ActiveCount);
            }),
            explicitCase: static (context, arrange) =>
                from indices in arrange.Order
                    .Traverse(address => address.Index(document: context.Document, includeDeleted: false, key: context.Op)
                        .Map(static index => index.Value)
                        .ToValidation())
                    .As()
                    .ToFin()
                let unique = indices.Distinct()
                from _unique in guard(unique.Count == indices.Count, context.Op.InvalidInput()).ToFin()
                from _complete in guard(unique.Count == context.Document.Layers.ActiveCount, context.Op.InvalidInput()).ToFin()
                from _ in context.Op.Catch(() => {
                    context.Document.Layers.Sort(layerIndices: unique.AsIterable());
                    return Fin.Succ(value: unit);
                })
                select unique.Count);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerOp {
    private LayerOp() { }

    private sealed record CreateCase(LayerName Name, Option<LayerRef> Parent, Seq<LayerEdit> Edits) : LayerOp;
    private sealed record GraftCase(LayerPath Path, Option<System.Drawing.Color> Color) : LayerOp;
    private sealed record AmendCase(LayerRef Target, Seq<LayerEdit> Edits) : LayerOp;
    private sealed record ReparentCase(LayerRef Target, Option<LayerRef> Parent) : LayerOp;
    private sealed record MergeCase(LayerRef Source, LayerRef Target) : LayerOp;
    private sealed record DuplicateCase(LayerRef Target, bool Objects, bool Sublayers) : LayerOp;
    private sealed record DeleteCase(LayerRef Target, HostInteraction Interaction) : LayerOp;
    private sealed record PurgeCase(LayerRef Target, HostInteraction Interaction) : LayerOp;
    private sealed record ReviveCase(LayerRef Target) : LayerOp;
    private sealed record AnointCase(LayerRef Target, HostInteraction Interaction) : LayerOp;
    private sealed record ExposeCase(LayerRef Target) : LayerOp;
    private sealed record ArrangeCase(LayerArrangement Arrangement) : LayerOp;
    private sealed record RollbackCase(LayerRef Target, Option<uint> UndoSerial) : LayerOp;
    private sealed record ReclaimCase : LayerOp;

    public static Fin<LayerOp> Create(LayerName name, Option<LayerRef> parent = default, params ReadOnlySpan<LayerEdit> edits) =>
        // an optional before `params` forecloses the positional spread — the key mints at the entry
        Admission.All(values: edits, key: Op.Of())
            .Map(admitted => (LayerOp)new CreateCase(Name: name, Parent: parent, Edits: admitted));

    public static Fin<LayerOp> Graft(LayerPath path, Option<System.Drawing.Color> color = default, Op? key = null) =>
        guard(path != default, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => (LayerOp)new GraftCase(Path: path, Color: color));

    public static Fin<LayerOp> Amend(LayerRef target, params ReadOnlySpan<LayerEdit> edits) {
        Op op = Op.Of();   // an optional before `params` forecloses the positional spread — the key mints at the entry
        return from address in op.Need(target)
               from admitted in Admission.All(values: edits, key: op)
               from _ in guard(!admitted.IsEmpty, op.InvalidInput()).ToFin()
               select (LayerOp)new AmendCase(Target: address, Edits: admitted);
    }

    public static Fin<LayerOp> Reparent(LayerRef target, Option<LayerRef> parent = default, Op? key = null) =>
        Addressed(target: target, key: key, mint: address => new ReparentCase(Target: address, Parent: parent));

    public static Fin<LayerOp> Merge(LayerRef source, LayerRef target, Op? key = null) {
        Op op = key.OrDefault();
        return from origin in op.Need(source)
               from destination in op.Need(target)
               from _ in guard(origin != destination, op.InvalidInput()).ToFin()
               select (LayerOp)new MergeCase(Source: origin, Target: destination);
    }

    public static Fin<LayerOp> Duplicate(LayerRef target, bool objects, bool sublayers, Op? key = null) =>
        Addressed(target: target, key: key, mint: address => new DuplicateCase(Target: address, Objects: objects, Sublayers: sublayers));

    public static Fin<LayerOp> Delete(LayerRef target, HostInteraction interaction, Op? key = null) =>
        Dialogued(target: target, interaction: interaction, key: key, mint: static (address, dialogue) =>
            new DeleteCase(Target: address, Interaction: dialogue));

    public static Fin<LayerOp> Purge(LayerRef target, HostInteraction interaction, Op? key = null) =>
        Dialogued(target: target, interaction: interaction, key: key, mint: static (address, dialogue) =>
            new PurgeCase(Target: address, Interaction: dialogue));

    public static Fin<LayerOp> Revive(LayerRef target, Op? key = null) =>
        Addressed(target: target, key: key, mint: static address => new ReviveCase(Target: address));

    public static Fin<LayerOp> Anoint(LayerRef target, HostInteraction interaction, Op? key = null) =>
        Dialogued(target: target, interaction: interaction, key: key, mint: static (address, dialogue) =>
            new AnointCase(Target: address, Interaction: dialogue));

    public static Fin<LayerOp> Expose(LayerRef target, Op? key = null) =>
        Addressed(target: target, key: key, mint: static address => new ExposeCase(Target: address));

    public static Fin<LayerOp> Arrange(LayerArrangement arrangement, Op? key = null) =>
        Optional(arrangement).ToFin(Fail: key.OrDefault().InvalidInput()).Map(order => (LayerOp)new ArrangeCase(Arrangement: order));

    public static Fin<LayerOp> Rollback(LayerRef target, Option<uint> undoSerial = default, Op? key = null) {
        Op op = key.OrDefault();
        return (
                op.Need(target).ToValidation(),
                guard(undoSerial.Map(static serial => serial > 0u).IfNone(noneValue: true), op.InvalidInput()).ToFin().ToValidation())
            .Apply(static (address, _) => address)
            .As()
            .ToFin()
            .Map(address => (LayerOp)new RollbackCase(Target: address, UndoSerial: undoSerial));
    }

    public static LayerOp Reclaim { get; } = new ReclaimCase();

    private static Fin<LayerOp> Addressed(LayerRef target, Op? key, Func<LayerRef, LayerOp> mint) =>
        Optional(target).ToFin(Fail: key.OrDefault().InvalidInput()).Map(address => mint(arg: address));

    private static Fin<LayerOp> Dialogued(
        LayerRef target,
        HostInteraction interaction,
        Op? key,
        Func<LayerRef, HostInteraction, LayerOp> mint) {
        Op op = key.OrDefault();
        return (
                op.Need(target).ToValidation(),
                op.Need(interaction).ToValidation())
            .Apply((address, dialogue) => mint(address, dialogue))
            .As()
            .ToFin();
    }

    internal Fin<LayerReceipt> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            createCase: static (context, edit) =>
                from parent in edit.Parent
                    .Traverse(address => address.Resolve(document: context.Document, includeDeleted: false, key: context.Op).Map(static layer => layer.Id))
                    .As()
                from index in context.Op.Catch(() => new Lease<Layer>.Owned(Value: new Layer { Name = edit.Name.Value }).Use(
                    state: (Document: context.Document, Parent: parent),
                    project: static (state, minted) => {
                        state.Parent.IfSome(id => minted.ParentLayerId = id);
                        return Fin.Succ(value: state.Document.Layers.Add(layer: minted));
                    }))
                from _ in guard(index >= 0, context.Op.InvalidResult()).ToFin()
                from receipt in Amended(document: context.Document, index: index, edits: edit.Edits, slot: LayerSlot.Created, op: context.Op)
                select receipt,
            graftCase: static (context, edit) =>
                from index in context.Op.Catch(() => Fin.Succ(value: edit.Color.Match(
                    Some: color => context.Document.Layers.AddPath(layerPath: edit.Path.Value, layerColor: color),
                    None: () => context.Document.Layers.AddPath(layerPath: edit.Path.Value))))
                from _ in guard(index >= 0, context.Op.InvalidResult()).ToFin()
                from stamp in Stamped(document: context.Document, index: index, op: context.Op)
                select LayerReceipt.Node(slot: LayerSlot.Grafted, stamp: stamp),
            amendCase: static (context, edit) =>
                from index in edit.Target.Index(document: context.Document, includeDeleted: false, key: context.Op)
                from receipt in Amended(document: context.Document, index: index.Value, edits: edit.Edits, slot: LayerSlot.Amended, op: context.Op)
                select receipt,
            reparentCase: static (context, edit) =>
                from target in edit.Target.Resolve(document: context.Document, includeDeleted: false, key: context.Op)
                from parent in edit.Parent
                    .Traverse(address => address.Resolve(document: context.Document, includeDeleted: false, key: context.Op))
                    .As()
                from acyclic in guard(
                    parent.Map(candidate => candidate.Id != target.Id && !candidate.IsChildOf(otherlayerId: target.Id)).IfNone(noneValue: true),
                    context.Op.InvalidInput()).ToFin()
                from receipt in Staged(
                    document: context.Document,
                    index: target.LayerIndex,
                    revise: staged => context.Op.Catch(() => {
                        staged.ParentLayerId = parent.Map(static layer => layer.Id).IfNone(Guid.Empty);
                        return Fin.Succ(value: unit);
                    }),
                    slot: LayerSlot.Reparented,
                    op: context.Op)
                select receipt,
            mergeCase: static (context, edit) =>
                from source in edit.Source.Resolve(document: context.Document, includeDeleted: false, key: context.Op)
                from target in edit.Target.Resolve(document: context.Document, includeDeleted: false, key: context.Op)
                from distinct in guard(source.Id != target.Id, context.Op.InvalidInput()).ToFin()
                from sourceStamp in LayerStamp.Of(layer: source, key: context.Op)
                from targetStamp in LayerStamp.Of(layer: target, key: context.Op)
                from merged in Merged(
                    document: context.Document,
                    sourceIndex: source.LayerIndex,
                    targetIndex: target.LayerIndex,
                    op: context.Op)
                select LayerReceipt.Merge(
                    source: sourceStamp,
                    target: targetStamp,
                    relayered: merged.Relayered,
                    cleanupFaults: merged.CleanupFaults),
            duplicateCase: static (context, edit) =>
                from index in edit.Target.Index(document: context.Document, includeDeleted: false, key: context.Op)
                from minted in context.Op.Catch(() => Fin.Succ(value: toSeq(context.Document.Layers.Duplicate(
                    layerIndex: index.Value,
                    duplicateObjects: edit.Objects,
                    duplicateSublayers: edit.Sublayers))))
                from _ in guard(!minted.IsEmpty, context.Op.InvalidResult()).ToFin()
                from stamps in minted
                    .Traverse(row => Stamped(document: context.Document, index: row, op: context.Op).ToValidation())
                    .As()
                    .ToFin()
                select stamps.Fold(LayerReceipt.Empty, (state, stamp) => state + LayerReceipt.Node(slot: LayerSlot.Duplicated, stamp: stamp)),
            deleteCase: static (context, edit) =>
                from target in edit.Target.Resolve(document: context.Document, includeDeleted: false, key: context.Op)
                from stamp in LayerStamp.Of(layer: target, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Layers.Delete(layerIndex: target.LayerIndex, quiet: edit.Interaction.IsQuiet))
                select LayerReceipt.Node(slot: LayerSlot.Deleted, stamp: stamp),
            purgeCase: static (context, edit) =>
                from target in edit.Target.Resolve(document: context.Document, includeDeleted: true, key: context.Op)
                from stamp in LayerStamp.Of(layer: target, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Layers.Purge(layerIndex: target.LayerIndex, quiet: edit.Interaction.IsQuiet))
                select LayerReceipt.Node(slot: LayerSlot.Purged, stamp: stamp),
            reviveCase: static (context, edit) =>
                from index in edit.Target.Index(document: context.Document, includeDeleted: true, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Layers.Undelete(layerIndex: index.Value))
                from stamp in Stamped(document: context.Document, index: index.Value, op: context.Op)
                select LayerReceipt.Node(slot: LayerSlot.Revived, stamp: stamp),
            anointCase: static (context, edit) =>
                from index in edit.Target.Index(document: context.Document, includeDeleted: false, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Layers.SetCurrentLayerIndex(
                    layerIndex: index.Value,
                    quiet: edit.Interaction.IsQuiet))
                from stamp in Stamped(document: context.Document, index: index.Value, op: context.Op)
                select LayerReceipt.Node(slot: LayerSlot.Anointed, stamp: stamp),
            exposeCase: static (context, edit) =>
                from target in edit.Target.Resolve(document: context.Document, includeDeleted: false, key: context.Op)
                from stamp in LayerStamp.Of(layer: target, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Layers.ForceLayerVisible(layerId: target.Id))
                select LayerReceipt.Node(slot: LayerSlot.Exposed, stamp: stamp),
            arrangeCase: static (context, edit) => edit.Arrangement.Apply(document: context.Document, key: context.Op)
                .Map(static count => LayerReceipt.Order(count: count)),
            rollbackCase: static (context, edit) =>
                from index in edit.Target.Index(document: context.Document, includeDeleted: false, key: context.Op)
                from _ in context.Op.Confirm(success: edit.UndoSerial.Match(
                    Some: serial => context.Document.Layers.UndoModify(layerIndex: index.Value, undoRecordSerialNumber: serial),
                    None: () => context.Document.Layers.UndoModify(layerIndex: index.Value)))
                from stamp in Stamped(document: context.Document, index: index.Value, op: context.Op)
                select LayerReceipt.Node(slot: LayerSlot.RolledBack, stamp: stamp),
            reclaimCase: static (context, _) => TableKind.Layers.Reclaim(document: context.Document, key: context.Op)
                .Map(static tally => LayerReceipt.Reclaimed(tally: tally)));

    private static Fin<LayerStamp> Stamped(RhinoDoc document, int index, Op op) =>
        Optional(document.Layers.FindIndex(index: index))
            .ToFin(Fail: op.InvalidResult())
            .Bind(layer => LayerStamp.Of(layer: layer, key: op));

    private static Fin<LayerReceipt> Amended(RhinoDoc document, int index, Seq<LayerEdit> edits, LayerSlot slot, Op op) =>
        edits.IsEmpty
            ? Stamped(document: document, index: index, op: op).Map(stamp => LayerReceipt.Node(slot: slot, stamp: stamp))
            : Staged(
                document: document,
                index: index,
                revise: staged => edits.TraverseM(edit => edit.Apply(staged: staged, key: op)).As().Map(static _ => unit),
                slot: slot,
                op: op);

    private static Fin<LayerReceipt> Staged(RhinoDoc document, int index, Func<Layer, Fin<Unit>> revise, LayerSlot slot, Op op) =>
        from live in Optional(document.Layers.FindIndex(index: index)).ToFin(Fail: op.MissingContext())
        from landed in op.Catch(() => {
            Layer copy = new();
            copy.CopyAttributesFrom(otherLayer: live);
            return new Lease<Layer>.Owned(Value: copy).Use(
                state: (Document: document, Index: index, Revise: revise, Op: op),
                project: static (state, staged) =>
                    from revised in state.Revise(arg: staged)
                    from written in state.Op.Confirm(success: state.Document.Layers.Modify(
                        newSettings: staged,
                        layerIndex: state.Index,
                        quiet: true))
                    select written);
        })
        from stamp in Stamped(document: document, index: index, op: op)
        select LayerReceipt.Node(slot: slot, stamp: stamp);

    private sealed record LayerMove(Guid ObjectId, ObjectAttributes Original);

    private sealed record LayerMerge(int Relayered, Seq<Error> CleanupFaults);

    // The merge IS a compensation fold, so it composes the slice's one compensation algebra rather than re-spelling
    // it: `Compensated` lands each element, rolls every landed key back on the first refusal, and settles source
    // custody through its release policy — and the source-layer delete is exactly that release step, because it
    // runs once the whole fold has landed and its own refusal unwinds the landed prefix like any operation fault.
    // The hand-rolled form re-derived the same landed-then-rollback shape in sixty-five lines beside the owner,
    // which is the deleted form the transaction rail's `Compensated` law already names.
    private static Fin<LayerMerge> Merged(RhinoDoc document, int sourceIndex, int targetIndex, Op op) =>
        from moves in Staged(document: document, sourceIndex: sourceIndex, op: op)
        from merged in DocumentCommit.Compensated(
                source: moves,
                land: move => Move(document: document, move: move, targetIndex: targetIndex, op: op),
                rollback: landed => Restore(
                    document: document,
                    moves: moves.Filter(move => landed.Exists(id => id == move.ObjectId)),
                    op: op),
                release: _ => op.Confirm(success: document.Layers.Delete(layerIndex: sourceIndex, quiet: true)))
            .BiBind(
                // The retained originals free on BOTH exits and only after the fold has settled — a release that
                // freed them before rollback would hand the restore a disposed attribute set.
                Succ: landed => Fin.Succ(value: new LayerMerge(
                    Relayered: landed.Count,
                    CleanupFaults: Release(moves: moves, op: op))),
                Fail: primary => Fin.Fail<LayerMerge>(error: Release(moves: moves, op: op)
                    .Fold(primary, static (fault, cleanup) => fault + cleanup)))
        select merged;

    // One duplicate per resident, retained as the restore payload. The census reads the residents through the
    // spine's own query value, so the merge does not mint a second settings shape beside `QuerySpec`.
    private static Fin<Seq<LayerMove>> Staged(RhinoDoc document, int sourceIndex, Op op) =>
        from index in ResourceIndex.Admit(value: sourceIndex, key: op)
        from spec in QuerySpec.Of(
            hidden: true,
            lights: true,
            layer: index,
            key: op)
        from settings in spec.Build(document: document, key: op)
        from residents in op.Catch(() => Optional(document.Objects.GetObjectList(settings: settings))
            .ToFin(Fail: op.InvalidResult())
            .Map(static values => toSeq(values).Strict()))
        from staged in DocumentCommit.Compensated(
            source: residents,
            land: native => op.Catch(() => Optional(native.Attributes?.Duplicate())
                .ToFin(Fail: op.InvalidResult())
                .Map(original => new LayerMove(ObjectId: native.Id, Original: original))),
            rollback: landed => Fin.Succ(value: Op.Side(() => landed.Iter(static move => move.Original.Dispose()))))
        select staged;

    private static Fin<Guid> Move(RhinoDoc document, LayerMove move, int targetIndex, Op op) =>
        from staged in op.Catch(() => Optional(move.Original.Duplicate()).ToFin(Fail: op.InvalidResult()))
        from _ in new Lease<ObjectAttributes>.Owned(Value: staged).Use(owned => {
            owned.LayerIndex = targetIndex;
            return op.Confirm(success: document.Objects.ModifyAttributes(
                objectId: move.ObjectId,
                newAttributes: owned,
                quiet: true));
        })
        select move.ObjectId;

    private static Fin<Unit> Restore(RhinoDoc document, Seq<LayerMove> moves, Op op) => moves.Rev()
        .Traverse(move => op.Catch(() => op.Confirm(success: document.Objects.ModifyAttributes(
            objectId: move.ObjectId,
            newAttributes: move.Original,
            quiet: true))).ToValidation())
        .As()
        .ToFin()
        .Map(static _ => unit);

    private static Seq<Error> Release(Seq<LayerMove> moves, Op op) => moves
        .Choose(move => op.Catch(() => Fin.Succ(value: Op.Side(move.Original.Dispose))).Match(
            Succ: static _ => Option<Error>.None,
            Fail: static error => Some(error)));
}

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class LayerSlot {
    public static readonly LayerSlot Created = new(key: 0);
    public static readonly LayerSlot Grafted = new(key: 1);
    public static readonly LayerSlot Amended = new(key: 2);
    public static readonly LayerSlot Reparented = new(key: 3);
    public static readonly LayerSlot Duplicated = new(key: 4);
    public static readonly LayerSlot Deleted = new(key: 5);
    public static readonly LayerSlot Purged = new(key: 6);
    public static readonly LayerSlot Revived = new(key: 7);
    public static readonly LayerSlot Anointed = new(key: 8);
    public static readonly LayerSlot Exposed = new(key: 9);
    public static readonly LayerSlot RolledBack = new(key: 10);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record LayerFact {
    private LayerFact() { }
    internal sealed record NodeCase(LayerSlot Slot, LayerStamp Stamp) : LayerFact;
    internal sealed record MergeCase(
        LayerStamp Source,
        LayerStamp Target,
        int Relayered,
        Seq<Error> CleanupFaults) : LayerFact;
    internal sealed record OrderCase(int Count) : LayerFact;
    internal sealed record ReclaimCase(int Tally) : LayerFact;
    internal sealed record UndoCase(uint Serial) : LayerFact;
}

public readonly record struct LayerReceipt : IDetachedDocumentResult {
    private readonly Seq<LayerFact> facts;

    private LayerReceipt(Seq<LayerFact> facts) => this.facts = facts;

    public static LayerReceipt Empty { get; } = new(facts: Seq<LayerFact>());

    public static LayerReceipt operator +(LayerReceipt left, LayerReceipt right) =>
        new(facts: left.facts + right.facts);

    internal static LayerReceipt Node(LayerSlot slot, LayerStamp stamp) => Of(fact: new LayerFact.NodeCase(Slot: slot, Stamp: stamp));

    internal static LayerReceipt Merge(
        LayerStamp source,
        LayerStamp target,
        int relayered,
        Seq<Error> cleanupFaults) => Of(fact: new LayerFact.MergeCase(
            Source: source,
            Target: target,
            Relayered: relayered,
            CleanupFaults: cleanupFaults));

    internal static LayerReceipt Order(int count) => Of(fact: new LayerFact.OrderCase(Count: count));

    internal static LayerReceipt Reclaimed(int tally) => Of(fact: new LayerFact.ReclaimCase(Tally: tally));

    internal static LayerReceipt Undo(uint serial) => Of(fact: new LayerFact.UndoCase(Serial: serial));

    private static LayerReceipt Of(LayerFact fact) => new(facts: Seq(fact));

    public Fin<Seq<LayerStamp>> Stamps(LayerSlot slot, Op? key = null) =>
        Optional(slot).ToFin(Fail: key.OrDefault().InvalidInput()).Map(admitted =>
            facts.Choose(fact => fact is LayerFact.NodeCase { Slot: var factSlot, Stamp: var stamp } && factSlot == admitted
                ? Some(stamp)
                : Option<LayerStamp>.None));

    public Seq<(LayerStamp Source, LayerStamp Target, int Relayered, Seq<Error> CleanupFaults)> Merges =>
        facts.Choose(static fact => fact is LayerFact.MergeCase merge
            ? Some((merge.Source, merge.Target, merge.Relayered, merge.CleanupFaults))
            : Option<(LayerStamp, LayerStamp, int, Seq<Error>)>.None);

    public Seq<int> Arranged =>
        facts.Choose(static fact => fact is LayerFact.OrderCase order ? Some(order.Count) : Option<int>.None);

    public Seq<int> Reclaims =>
        facts.Choose(static fact => fact is LayerFact.ReclaimCase reclaim ? Some(reclaim.Tally) : Option<int>.None);

    public Seq<uint> UndoRecords =>
        facts.Choose(static fact => fact is LayerFact.UndoCase undo ? Some(undo.Serial) : Option<uint>.None);

    public Fin<int> Count(LayerSlot slot, Op? key = null) =>
        Stamps(slot: slot, key: key).Map(static values => values.Count);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public sealed record LayerDelta {
    private LayerDelta(string name, RedrawPolicy redraw, Seq<LayerOp> operations) =>
        (Name, Redraw, Operations) = (name, redraw, operations);

    public string Name { get; }
    public RedrawPolicy Redraw { get; }
    public Seq<LayerOp> Operations { get; }

    public static Fin<LayerDelta> Of(string name, RedrawPolicy redraw, params ReadOnlySpan<LayerOp> operations) {
        Op op = Op.Of();   // an optional before `params` forecloses the positional spread — the key mints at the entry
        return from label in op.AcceptText(value: name)
               from policy in op.Need(redraw)
               from admitted in Admission.All(values: operations, key: op)
               from _ in guard(!admitted.IsEmpty, op.InvalidInput()).ToFin()
               select new LayerDelta(name: label, redraw: policy, operations: admitted);
    }
}

// One entry class across two clusters: `[05]` carries the tree read and the commit rail, `[06]` the host-free
// organization egress, and `Ask` folds its product off the request shape rather than proliferating a second verb.
public static partial class Layers {
    public static Fin<LayerTree> Ask(DocumentSession session, params ReadOnlySpan<Guid> detailViewports) {
        Op op = Op.Of();   // an optional before `params` forecloses the positional spread — the key mints at the entry
        Seq<Guid> probes = toSeq(detailViewports.ToArray());
        return from scope in op.Need(session)
               from admitted in probes
                   .Traverse(viewport => guard(viewport != Guid.Empty, op.InvalidInput()).ToFin().Map(_ => viewport).ToValidation())
                   .As()
                   .ToFin()
               from tree in scope.Demand(
                   use: document => LayerTree.Of(document: document, detailViewports: admitted, key: op),
                   key: op,
                   needs: [SessionNeed.Read])
               select tree;
    }

    public static Fin<LayerReceipt> Commit(DocumentSession session, LayerDelta delta, Op? key = null) {
        Op op = key.OrDefault();
        return from admission in Admission.Pair(first: session, second: delta, key: op)
               from receipt in admission.First.Demand(
                   use: document => Run(document: document, delta: admission.Second, op: op),
                   key: op,
                   needs: SessionNeed.Mutation(undo: true, redraw: admission.Second.Redraw).ToArray())
               select receipt;
    }

    private static Fin<LayerReceipt> Run(RhinoDoc document, LayerDelta delta, Op op) =>
        DocumentCommit.Sealed(
            document: document,
            name: delta.Name,
            recordsUndo: true,
            redraw: delta.Redraw,
            run: () => delta.Operations
                .TraverseM(operation => operation.Apply(document: document, op: op)).As()
                .Map(static receipts => receipts.Fold(LayerReceipt.Empty, static (state, value) => state + value)),
            stamp: static (receipt, serial) => receipt + LayerReceipt.Undo(serial: serial),
            op: op);
}
```

## [06]-[ORGANIZATION_PROJECTION]

- Owner: `OrganizationFact` carries the host-free organization document this boundary emits — `OrganizationEntity` rows with content-keyed organizational address, leaf label, dense sibling ordinal, and resolved visibility and locking; `ContainmentFact` rows carrying nesting and membership as ONE edge family the `ContainmentTarget` `[Union]` discriminates by target key space; `ViewOverrideFact` rows carrying probed per-view presentation evidence. `IOrganizationAuthority` is the federation port a composition root binds, and `OrganizationCodec` lowers the fact onto `rasm.organization.v1` bytes.
- Entry: `Layers.Ask(session, authority, views)` demands `SessionNeed.Read`, mints the tree and projects it inside ONE window, so probe targets stay call data exactly as the tree read carries them and no consumer re-opens the document to answer organization.
- Law: every name on this egress states the HOST-FREE organizational concept and the Rhino layer vocabulary translates HERE. Publishing `LayerStamp` field-for-field binds every peer decode to one host's layer model, which `libs/.planning/ARCHITECTURE.md` `[03]-[UNIVERSAL_VS_CAPTURE]` forecloses, so the host `Guid`, the `-1`-sentinel table index, and the `::`-joined path each stop at this boundary.
- Law: organizational identity is the content key over the count-framed ancestor label chain under `docs/laws/patterns.md` `[PREIMAGE_FRAMING]`, so one organizational address keys identically across source documents and a worksession merge unions them. Folding the source key into that preimage is the rejected form, since it re-scopes a federation address down to one file.
- Law: membership targets are FEDERATION keys the authority issues, never host object ids — the host-object-to-entity binding lives in the element projection, which sits outside this plane's reference set. Residents the authority declines land NO edge, so an unclaimed object reads as absent membership rather than as a key no peer resolves.
- Law: sibling ordinal is POSITIONAL in the snapshot's own sorted sibling sequence. Publishing the raw `SortIndex` pushes the host's case-insensitive tie-break onto every peer, and a peer ordering by codepoint inverts exactly the pairs UTF-16 ordering ranks the other way.
- Law: per-view rows land only where `HasPerViewportSettings` proved settings, so row presence IS the evidence and `visible` is the host's resolved answer under them. Persistent visibility never crosses, because the host collapses its own three write states onto a two-state read and a peer column carrying that collapse carries no defined meaning.
- Boundary: render and print product stays host-side evidence — `LayerFace` colours, print width, linetype, render material, and section style reach no wire field, and `PerceptualColor` riding a detached payload is the crossing the kernel colour rail already forecloses.
- Boundary: `ContainmentTarget` crosses through its generated `Switch`, never a Mapperly derived-type row, because a union arm selecting a oneof slot is dispatch rather than member transcription.
- Packages: Google.Protobuf (`ByteString`/`RepeatedField<T>`/`MessageExtensions.ToByteArray` over the generated `rasm.organization.v1` messages), Grpc.Tools (the `<Protobuf>` MSBuild item, `GrpcServices=None`, `PrivateAssets=all` — build-only, never a runtime surface), BCL inbox (`System.Buffers.ArrayBufferWriter<byte>`, `System.Buffers.Binary.BinaryPrimitives`, `System.Text.Encoding`).
- Growth: one appended field on `EntityWire` beside one column here carries a new organizational axis; one `ContainmentTarget` case beside one oneof arm carries a new containment relation; one appended `ViewOverrideWire` field beside one probe fill carries a new presentation axis.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// Framing and encoding both live at this boundary: `ArrayBufferWriter<byte>` is BCL inbox, so the organizational
// address preimage costs no manifest row, and `Rasm.Organization` is the generated namespace of the corpus source
// rather than a namespace under this host package, because the family it spells is host-free.
using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using Google.Protobuf;
using Rasm.Organization;

// --- [TYPES] ------------------------------------------------------------------------------
// Target KEY SPACE is the whole discriminant: an `Entity` target resolves in this document's own address space and
// a `Member` target in the federation space its authority issues. Two same-typed slots under a relation column
// erase that distinction and hand every decoder a lookup it cannot route, so the union carries it structurally.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContainmentTarget {
    private ContainmentTarget() { }

    private sealed record EntityCase(UInt128 Value) : ContainmentTarget;
    private sealed record MemberCase(string Value) : ContainmentTarget;

    public static ContainmentTarget Entity(UInt128 value) => new EntityCase(Value: value);

    public static Fin<ContainmentTarget> Member(string value, Op? key = null) =>
        key.OrDefault().AcceptText(value: value).Map(static admitted => (ContainmentTarget)new MemberCase(Value: admitted));
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record OrganizationEntity(
    UInt128 Key,
    LayerName Name,
    int Ordinal,
    bool Visible,
    bool Locked) : IDetachedDocumentResult;

public sealed record ContainmentFact(UInt128 Container, ContainmentTarget Target) : IDetachedDocumentResult;

public sealed record ViewOverrideFact(UInt128 Entity, string View, bool Visible) : IDetachedDocumentResult;

public sealed record OrganizationFact(
    UInt128 Source,
    string Authority,
    Seq<OrganizationEntity> Entities,
    Seq<ContainmentFact> Containment,
    Seq<ViewOverrideFact> Overrides,
    Option<UInt128> Current) : IDetachedDocumentResult;

// --- [SERVICES] ---------------------------------------------------------------------------
// Federation vocabulary this boundary cannot mint: host objects bind to graph entities inside the element
// projection, which seats outside the host plane's reference set, so a composition root supplies the binding as
// one declared port. An unbound port refuses at the entry — never a fabricated key and never a crash.
public interface IOrganizationAuthority {
    string Name { get; }
    UInt128 Source { get; }
    Option<string> MemberOf(Guid resident);
    Option<string> ViewOf(Guid viewport);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class Layers {
    public static Fin<OrganizationFact> Ask(
        DocumentSession session,
        IOrganizationAuthority authority,
        params ReadOnlySpan<Guid> views) {
        Op op = Op.Of();   // an optional before `params` forecloses the positional spread — the key mints at the entry
        Seq<Guid> probes = toSeq(views.ToArray());
        return from admission in Admission.Pair(first: session, second: authority, key: op)
               from admitted in probes
                   .Traverse(view => guard(view != Guid.Empty, op.InvalidInput()).ToFin().Map(_ => view).ToValidation())
                   .As()
                   .ToFin()
               from fact in admission.First.Demand(
                   use: document =>
                       from tree in LayerTree.Of(document: document, detailViewports: admitted, key: op)
                       from projected in Projected(document: document, tree: tree, authority: admission.Second, op: op)
                       select projected,
                   key: op,
                   needs: [SessionNeed.Read])
               select fact;
    }

    private static Fin<OrganizationFact> Projected(
        RhinoDoc document,
        LayerTree tree,
        IOrganizationAuthority authority,
        Op op) =>
        from issuer in op.AcceptText(value: authority.Name)
        from residents in Residents(document: document, op: op)
        from rows in Ranked(tree: tree)
            .Traverse(row => Projected(row: row, residents: residents, authority: authority, op: op).ToValidation())
            .As()
            .ToFin()
        from current in tree.Current.Traverse(stamp => Address(path: stamp.Path, op: op)).As()
        select new OrganizationFact(
            Source: authority.Source,
            Authority: issuer,
            Entities: rows.Map(static row => row.Entity),
            Containment: rows.Bind(static row => row.Containment),
            Overrides: rows.Bind(static row => row.Overrides),
            Current: current);

    // Ordinal is POSITIONAL in the sibling sequence `LayerTree` already sorted, so the dense rank inherits the
    // order the snapshot proved and no second comparison re-breaks the host's sort-index ties. `Flatten` supplies
    // every node's child sequence once, so the whole ranking is one owned walk plus one flat bind — no second
    // descent over a parent graph the snapshot already measured.
    private static Seq<(LayerNode Node, int Ordinal)> Ranked(LayerTree tree) =>
        Ranked(siblings: tree.Roots) + tree.Flatten().Bind(static node => Ranked(siblings: node.Children));

    // Indexed projection has no carrier-side member, so this one ordering leaves the carrier and re-enters
    // through `toSeq`, exactly as the snapshot's own sibling sort does.
    private static Seq<(LayerNode Node, int Ordinal)> Ranked(Seq<LayerNode> siblings) =>
        toSeq(siblings.Select(static (node, ordinal) => (Node: node, Ordinal: ordinal)));

    // ONE object census grouped by layer index, never a per-layer query: a thousand-entity document costs one
    // sweep. Filter posture matches the merge arm's own census (hidden and light residents included), so a layer
    // merge and an organization read agree on what a layer holds. An attribute-less native row folds away rather
    // than seeding a phantom index.
    private static Fin<HashMap<int, Seq<Guid>>> Residents(RhinoDoc document, Op op) =>
        from spec in QuerySpec.Of(hidden: true, lights: true, key: op)
        from settings in spec.Build(document: document, key: op)
        from natives in op.Catch(() => Optional(document.Objects.GetObjectList(settings: settings))
            .ToFin(Fail: op.InvalidResult())
            .Map(static values => toSeq(values).Strict()))
        select natives.Fold(
            HashMap<int, Seq<Guid>>(),
            static (held, native) => Optional(native.Attributes).Match(
                Some: attributes => held.AddOrUpdate(
                    key: attributes.LayerIndex,
                    Some: existing => existing.Add(value: native.Id),
                    None: () => Seq(native.Id)),
                None: () => held));

    private static Fin<(OrganizationEntity Entity, Seq<ContainmentFact> Containment, Seq<ViewOverrideFact> Overrides)> Projected(
        (LayerNode Node, int Ordinal) row,
        HashMap<int, Seq<Guid>> residents,
        IOrganizationAuthority authority,
        Op op) =>
        from key in Address(path: row.Node.Identity.Path, op: op)
        from nesting in row.Node.Identity.Path.Parent.Traverse(parent => Address(path: parent, op: op)).As()
        from members in residents.Find(row.Node.Identity.Index)
            .IfNone(Seq<Guid>())
            .Choose(resident => authority.MemberOf(resident: resident))
            .Traverse(external => ContainmentTarget.Member(value: external, key: op).ToValidation())
            .As()
            .ToFin()
        from overrides in row.Node.Details
            .Choose(detail => authority.ViewOf(viewport: detail.Viewport).Map(view => (View: view, detail.Visible)))
            .Traverse(probe => op.AcceptText(value: probe.View)
                .Map(view => new ViewOverrideFact(Entity: key, View: view, Visible: probe.Visible))
                .ToValidation())
            .As()
            .ToFin()
        select (
            Entity: new OrganizationEntity(
                Key: key,
                Name: row.Node.Name,
                Ordinal: row.Ordinal,
                Visible: row.Node.Condition.Visible,
                Locked: row.Node.Condition.Locked),
            Containment: nesting.Match(
                    Some: parent => Seq(new ContainmentFact(Container: parent, Target: ContainmentTarget.Entity(value: key))),
                    None: static () => Seq<ContainmentFact>())
                + members.Map(target => new ContainmentFact(Container: key, Target: target)),
            Overrides: overrides);

    // Organizational address preimage: label count framed first, then every label framed by its UTF-8 byte width,
    // so `A::B` and `B::A` key apart and a label carrying the host path separator shifts no field split. Frame
    // widths are big-endian `int32`, matching every other preimage this boundary writes.
    // Exemption: a measured span write into one pre-sized arena — expressing it as a fold carries a byte-offset
    // accumulator richer than the answer, and the buffer never escapes this body.
    private static Fin<UInt128> Address(LayerPath path, Op op) =>
        from chain in path.Segments(key: op)
        from key in op.Catch(() => {
            ArrayBufferWriter<byte> bytes = new();
            Frame(bytes: bytes, value: chain.Count);
            foreach (LayerName label in chain) {
                int width = Encoding.UTF8.GetByteCount(s: label.Value);
                Frame(bytes: bytes, value: width);
                bytes.Advance(count: Encoding.UTF8.GetBytes(label.Value, bytes.GetSpan(sizeHint: width)));
            }
            return Fin.Succ(value: ContentHash.Of(canonicalBytes: bytes.WrittenSpan));
        })
        select key;

    private static void Frame(ArrayBufferWriter<byte> bytes, int value) {
        BinaryPrimitives.WriteInt32BigEndian(destination: bytes.GetSpan(sizeHint: sizeof(int)), value: value);
        bytes.Advance(count: sizeof(int));
    }
}

// --- [COMPOSITION] ------------------------------------------------------------------------
// Content keys leave as 16 BIG-ENDIAN bytes while `XxHash128` fills its own hash buffer little-endian, so byte
// order normalizes exactly ONCE — here, at the encode — and every peer decodes what it received without reversing
// a second time. Generated message construction is the one imperative seat on this page: protobuf owns mutable
// repeated fields, and each fold lands its rows through `AddRange` at the boundary rather than inside a domain
// transform.
public static class OrganizationCodec {
    public static Fin<ReadOnlyMemory<byte>> Encode(OrganizationFact fact, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(fact).Map(static admitted => (ReadOnlyMemory<byte>)Wire(fact: admitted).ToByteArray());
    }

    private static OrganizationWire Wire(OrganizationFact fact) {
        OrganizationWire wire = new() { SourceKey = Key(value: fact.Source), Authority = fact.Authority };
        fact.Current.IfSome(current => wire.Current = Key(value: current));
        wire.Entities.AddRange(fact.Entities.Map(static entity => new EntityWire {
            Key = Key(value: entity.Key),
            Name = entity.Name.Value,
            Ordinal = (uint)entity.Ordinal,
            Visible = entity.Visible,
            Locked = entity.Locked,
        }));
        wire.Containment.AddRange(fact.Containment.Map(static edge => edge.Target.Switch(
            state: edge,
            entityCase: static (row, target) => new ContainmentWire { Container = Key(value: row.Container), Entity = Key(value: target.Value) },
            memberCase: static (row, target) => new ContainmentWire { Container = Key(value: row.Container), Member = target.Value })));
        wire.Overrides.AddRange(fact.Overrides.Map(static probe => new ViewOverrideWire {
            Entity = Key(value: probe.Entity),
            View = probe.View,
            Visible = probe.Visible,
        }));
        return wire;
    }

    private static ByteString Key(UInt128 value) {
        Span<byte> bytes = stackalloc byte[KeyWidth];
        BinaryPrimitives.WriteUInt128BigEndian(destination: bytes, value: value);
        return ByteString.CopyFrom(bytes: bytes);
    }

    // Content-key width the estate wires everywhere; named once so no frame or span literal re-spells it.
    private const int KeyWidth = 16;
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]              | [OWNER]                      | [FORM]                            | [ENTRY]                                   |
| :-----: | :--------------------- | :--------------------------- | :-------------------------------- | :---------------------------------------- |
|  [01]   | leaf and path identity | `LayerName` / `LayerPath`    | generated host-grammar values     | `Of` / `Segments` / `Child`               |
|  [02]   | layer addressing       | `LayerRef`                   | id/index/path/current union       | `ById` / `AtIndex` / `AtPath` / `Current` |
|  [03]   | detached anchor        | `LayerStamp`                 | id/index/path evidence product    | receipt and node rows                     |
|  [04]   | tree topology          | `LayerTree` / `LayerNode`    | one-read recursive snapshot       | `Layers.Ask` / `Find`                     |
|  [05]   | per-detail overrides   | `LayerOverride`              | option-discriminated write/clear  | `LayerEdit.Override`                      |
|  [06]   | staged property edits  | `LayerEdit`                  | closed staged-write union         | edit factories / `Amend`                  |
|  [07]   | structural mutation    | `LayerOp`                    | admitted total operation union    | operation factories / `Apply`             |
|  [08]   | sibling ordering       | `LayerArrangement`           | by-name/explicit union            | `LayerOp.Arrange`                         |
|  [09]   | commit program         | `LayerDelta`                 | named redraw-scoped program       | `Layers.Commit`                           |
|  [10]   | consequence evidence   | `LayerReceipt` / `LayerSlot` | stamped fact stream + undo serial | typed projections                         |
|  [11]   | host-free organization | `OrganizationFact`           | detached entity + containment set | `Layers.Ask` with an authority            |
|  [12]   | federation vocabulary  | `IOrganizationAuthority`     | composition-root-bound port       | `MemberOf` / `ViewOf`                     |
|  [13]   | organization egress    | `OrganizationCodec`          | `rasm.organization.v1` encode     | `Encode`                                  |

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
