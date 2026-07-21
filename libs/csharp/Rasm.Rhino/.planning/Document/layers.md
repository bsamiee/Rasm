# [RASM_RHINO_LAYERS]

`Rasm.Rhino.Document` owns the layer tree as a managed domain: full-path nesting topology, per-layer render material, linetype, print width, print color, and section style, visibility and locking with their persistent variants, the per-detail-viewport override family, and every structural mutation — create, graft, reparent, merge, duplicate, delete, purge, revive, current-layer anointment, and ordering. `Layers.Ask` projects one detached `LayerTree` with parameterized per-detail probe targets; `Layers.Commit` folds an admitted operation program inside one session capability window through the shared `DocumentCommit` envelope, so every structural change seals one undo record and returns one typed fact stream. `TableKind.Layers` stays the identity and reclamation row this page composes for purge tallies; the tree itself is minted here.

## [01]-[INDEX]

- [02]-[IDENTITY_AND_ADDRESS]: `LayerName`, `LayerPath`, `LayerRef`, and the detached `LayerStamp` anchor.
- [03]-[TREE_SNAPSHOT]: `LayerFace`, `LayerCondition`, `DetailFace`, `LayerNode`, and the `LayerTree` detached topology.
- [04]-[EDITS_AND_OVERRIDES]: the `LayerEdit` staged-property program and the `LayerOverride` per-detail family.
- [05]-[COMMIT_RAIL]: `LayerOp`, `LayerDelta`, the `Layers` entry pair, and the `LayerReceipt` fact stream.
- [06]-[SURFACE_LEDGER]: the page owner map.

## [02]-[IDENTITY_AND_ADDRESS]

- Owner: `LayerName` admits one leaf name under the host name rule; `LayerPath` canonicalizes trimmed segments and admits every leaf through that same rule before owning leaf, parent, and child projections. `LayerRef` `[Union]` closes id, index, full-path, and current-layer addressing; `LayerStamp` `[ComplexValueObject]` is the detached identity anchor every receipt row and tree node carries.
- Entry: `LayerRef.ById`/`AtIndex`/`AtPath`/`Current` are the only constructors. Internal `Resolve` admits one live row for every address case, and `Index` projects that row's durable table index without a second lookup.
- Law: a deleted layer is addressable only by id or index with `includeDeleted` — the revive path — so a path address never resolves a dead branch, and every resolution failure is a typed fault, never a `-1` or null leak.
- Boundary: `Layer.PathSeparator`, `GetLeafName`, `GetParentName`, and `IsValidName` are the host path grammar; `LayerPath` composes them once, so no consumer re-derives separator arithmetic or name legality.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Drawing;
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
            : segments.Select(static segment => LayerName.Refusal(value: segment)).FirstOrDefault(static fault => fault is not null);
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

    private sealed record IdCase(Guid Value) : LayerRef;
    private sealed record IndexCase(int Value) : LayerRef;
    private sealed record PathCase(LayerPath Value) : LayerRef;
    private sealed record CurrentCase : LayerRef;

    public static Fin<LayerRef> ById(Guid value) =>
        guard(value != Guid.Empty, Op.Of().InvalidInput()).ToFin()
            .Map(_ => (LayerRef)new IdCase(Value: value));

    public static Fin<LayerRef> AtIndex(int value) =>
        guard(value >= 0, Op.Of().InvalidInput()).ToFin()
            .Map(_ => (LayerRef)new IndexCase(Value: value));

    public static Fin<LayerRef> AtPath(LayerPath value) =>
        guard(value != default, Op.Of().InvalidInput()).ToFin()
            .Map(_ => (LayerRef)new PathCase(Value: value));

    public static LayerRef Current { get; } = new CurrentCase();

    internal Fin<int> Index(RhinoDoc document, bool includeDeleted, Op key) =>
        Resolve(document: document, includeDeleted: includeDeleted, key: key)
            .Map(static row => row.LayerIndex);

    internal Fin<Layer> Resolve(RhinoDoc document, bool includeDeleted, Op key) =>
        Switch(
            state: (Document: document, IncludeDeleted: includeDeleted, Op: key),
            idCase: static (context, address) =>
                from index in context.Op.Catch(() => Fin.Succ(value: context.Document.Layers.Find(
                    layerId: address.Value,
                    ignoreDeletedLayers: !context.IncludeDeleted)))
                from _ in guard(index >= 0, context.Op.MissingContext()).ToFin()
                from row in Optional(context.Document.Layers.FindIndex(index: index)).ToFin(Fail: context.Op.MissingContext())
                from admitted in guard(context.IncludeDeleted || !row.IsDeleted, context.Op.MissingContext()).ToFin()
                select row,
            indexCase: static (context, address) =>
                from row in Optional(context.Document.Layers.FindIndex(index: address.Value)).ToFin(Fail: context.Op.MissingContext())
                from admitted in guard(context.IncludeDeleted || !row.IsDeleted, context.Op.MissingContext()).ToFin()
                select row,
            pathCase: static (context, address) =>
                from index in context.Op.Catch(() => Fin.Succ(value: context.Document.Layers.FindByFullPath(
                    layerPath: address.Value.Value,
                    notFoundReturnValue: -1)))
                from _ in guard(index >= 0, context.Op.MissingContext()).ToFin()
                from row in Optional(context.Document.Layers.FindIndex(index: index)).ToFin(Fail: context.Op.MissingContext())
                from admitted in guard(!row.IsDeleted, context.Op.MissingContext()).ToFin()
                select row,
            currentCase: static (context, _) => Optional(context.Document.Layers.CurrentLayer)
                .Filter(static row => !row.IsDeleted)
                .ToFin(Fail: context.Op.InvalidResult()));
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
- Law: the host exposes no roster of viewports carrying overrides, so per-detail evidence is probe-parameterized: each requested viewport lands a `DetailFace` only where `HasPerViewportSettings` proves one, and an unprobed override is absent evidence, never a fabricated default.
- Boundary: every node is detached — the live `Layer` handle dies inside the demand window, and `LayerTree` implements `IDetachedDocumentResult` so it crosses out of `Demand` by construction.

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
        idCase: static (tree, target) => tree.Flatten().Find(node => node.Identity.Id == target.Value),
        indexCase: static (tree, target) => tree.Flatten().Find(node => node.Identity.Index == target.Value),
        pathCase: static (tree, target) => tree.Flatten().Find(node => node.Identity.Path == target.Value),
        currentCase: static (tree, _) => tree.Current.Bind(stamp => tree.Flatten().Find(node => node.Identity.Id == stamp.Id)));

    internal static Fin<LayerTree> Of(RhinoDoc document, Seq<Guid> detailViewports, Op key) => key.Catch(() => {
        Seq<Layer> rows = toSeq(document.Layers.AsIterable()).Filter(static row => !row.IsDeleted).Strict();
        return from nodes in rows
            .Traverse(row => Leaf(layer: row, detailViewports: detailViewports, key: key).ToValidation())
            .As()
            .ToFin()
        let byParent = nodes
                .Filter(static entry => entry.Parent.IsSome)
                .GroupBy(static entry => entry.Parent.IfNone(Guid.Empty))
                .AsIterable()
                .ToHashMap(static group => (group.Key, toSeq(group)))
        from current in Optional(document.Layers.CurrentLayer)
            .Traverse(layer => LayerStamp.Of(layer: layer, key: key))
            .As()
        let tree = new LayerTree(
            Roots: Ordered(nodes: nodes.Filter(static entry => entry.Parent.IsNone), byParent: byParent),
            Count: nodes.Count,
            Current: current)
        from _ in guard(tree.Flatten().Count == tree.Count, key.InvalidResult()).ToFin()
        select tree;
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

    private static Seq<LayerNode> Ordered(Seq<LayerNode> nodes, HashMap<Guid, Seq<LayerNode>> byParent) =>
        nodes
            .OrderBy(static node => node.SortIndex)
            .ThenBy(static node => node.Name.Value, StringComparer.OrdinalIgnoreCase)
            .ToSeq()
            .Map(node => node with {
                Children = Ordered(nodes: byParent.Find(node.Identity.Id).IfNone(Seq<LayerNode>()), byParent: byParent),
            });
}
```

## [04]-[EDITS_AND_OVERRIDES]

- Owner: `LayerOverride` `[Union]` closes the per-detail-viewport family — color, visibility, persistent visibility, print color, print width, the new-detail visibility default, and the whole-viewport purge — with one `Option` per payload discriminating write from clear, so set and delete are one case, never sibling verbs. `LayerEdit` `[Union]` closes every staged property write, and `LayerFlag` rows collapse visible, locked, and expanded assignments into one behavior case.
- Entry: edit factories admit payloads once — finite print width, nonnegative resource indexes, admitted names — and `Apply` runs each case against the staged layer copy inside the commit callback.
- Law: a persistent-visibility or persistent-locking edit carries `Option<bool>`: a value writes `SetPersistent*`, absence runs `UnsetPersistent*`, so the host's three-state persistence is one case rather than a set/unset verb pair.
- Law: section style is two independent axes — the table index and the custom carrier — and the custom axis clears through absence, mirroring the host `SetCustomSectionStyle`/`RemoveCustomSectionStyle` pair as one case.
- Boundary: every override member on `Layer` is a void host write; each arm crosses through `Op.Catch`, and the staged copy never leaves the callback, so a failed edit program leaves the live table untouched until `Modify` lands the whole staged state.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerOverride {
    private LayerOverride() { }

    private sealed record ColorCase(Guid Viewport, Option<Color> Value) : LayerOverride;
    private sealed record VisibleCase(Guid Viewport, Option<bool> Value) : LayerOverride;
    private sealed record PersistentVisibilityCase(Guid Viewport, Option<bool> Value) : LayerOverride;
    private sealed record PrintColorCase(Guid Viewport, Option<Color> Value) : LayerOverride;
    private sealed record PrintWidthCase(Guid Viewport, Option<double> Value) : LayerOverride;
    private sealed record NewDetailVisibilityCase(bool Value) : LayerOverride;
    private sealed record PurgeCase(Guid Viewport) : LayerOverride;

    public static Fin<LayerOverride> Color(Guid viewport, Option<Color> value = default) =>
        Addressed(viewport: viewport, mint: address => new ColorCase(Viewport: address, Value: value));

    public static Fin<LayerOverride> Visible(Guid viewport, Option<bool> value = default) =>
        Addressed(viewport: viewport, mint: address => new VisibleCase(Viewport: address, Value: value));

    public static Fin<LayerOverride> PersistentVisibility(Guid viewport, Option<bool> value = default) =>
        Addressed(viewport: viewport, mint: address => new PersistentVisibilityCase(Viewport: address, Value: value));

    public static Fin<LayerOverride> PrintColor(Guid viewport, Option<Color> value = default) =>
        Addressed(viewport: viewport, mint: address => new PrintColorCase(Viewport: address, Value: value));

    public static Fin<LayerOverride> PrintWidth(Guid viewport, Option<double> value = default) =>
        from admitted in value.Traverse(width => LayerEdit.Width(value: width, op: Op.Of())).As()
        from minted in Addressed(viewport: viewport, mint: address => new PrintWidthCase(Viewport: address, Value: admitted))
        select minted;

    public static LayerOverride NewDetailVisibility(bool value) => new NewDetailVisibilityCase(Value: value);

    public static Fin<LayerOverride> Purge(Guid viewport) =>
        Addressed(viewport: viewport, mint: address => new PurgeCase(Viewport: address));

    private static Fin<LayerOverride> Addressed(Guid viewport, Func<Guid, LayerOverride> mint) =>
        guard(viewport != Guid.Empty, Op.Of().InvalidInput()).ToFin().Map(_ => mint(arg: viewport));

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

    internal Action<Layer, bool> Set { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerEdit {
    private LayerEdit() { }

    private sealed record RenameCase(LayerName Name) : LayerEdit;
    private sealed record ColorCase(Color Value) : LayerEdit;
    private sealed record PrintColorCase(Color Value) : LayerEdit;
    private sealed record PrintWidthCase(double Value) : LayerEdit;
    private sealed record LinetypeCase(int Index) : LayerEdit;
    private sealed record RenderMaterialCase(int Index) : LayerEdit;
    private sealed record SectionStyleIndexCase(int Index) : LayerEdit;
    private sealed record CustomSectionStyleCase(Option<SectionStyle> Value) : LayerEdit;
    private sealed record FlagCase(LayerFlag Flag, bool Value) : LayerEdit;
    private sealed record PersistentVisibilityCase(Option<bool> Value) : LayerEdit;
    private sealed record PersistentLockingCase(Option<bool> Value) : LayerEdit;
    private sealed record DescriptionCase(string Value) : LayerEdit;
    private sealed record IgesLevelCase(int Value) : LayerEdit;
    private sealed record OverrideCase(LayerOverride Value) : LayerEdit;

    public static LayerEdit Rename(LayerName name) => new RenameCase(Name: name);

    public static LayerEdit Recolor(Color value) => new ColorCase(Value: value);

    public static LayerEdit PrintColor(Color value) => new PrintColorCase(Value: value);

    public static Fin<LayerEdit> PrintWidth(double value) =>
        Width(value: value, op: Op.Of()).Map(static admitted => (LayerEdit)new PrintWidthCase(Value: admitted));

    public static Fin<LayerEdit> Linetype(int index) => Indexed(index: index, floor: 0, mint: static value => new LinetypeCase(Index: value));

    public static Fin<LayerEdit> RenderMaterial(int index) =>
        Indexed(index: index, floor: -1, mint: static value => new RenderMaterialCase(Index: value));

    public static Fin<LayerEdit> SectionStyleIndex(int index) =>
        Indexed(index: index, floor: -1, mint: static value => new SectionStyleIndexCase(Index: value));

    public static LayerEdit CustomSectionStyle(Option<SectionStyle> value = default) => new CustomSectionStyleCase(Value: value);

    public static LayerEdit Visibility(bool value) => new FlagCase(Flag: LayerFlag.Visible, Value: value);

    public static LayerEdit Locking(bool value) => new FlagCase(Flag: LayerFlag.Locked, Value: value);

    public static LayerEdit PersistentVisibility(Option<bool> value = default) => new PersistentVisibilityCase(Value: value);

    public static LayerEdit PersistentLocking(Option<bool> value = default) => new PersistentLockingCase(Value: value);

    public static LayerEdit Expansion(bool value) => new FlagCase(Flag: LayerFlag.Expanded, Value: value);

    public static Fin<LayerEdit> Description(string value) =>
        Op.Of().AcceptText(value: value).Map(admitted => (LayerEdit)new DescriptionCase(Value: admitted));

    public static Fin<LayerEdit> IgesLevel(int value) =>
        Indexed(index: value, floor: 0, mint: static admitted => new IgesLevelCase(Value: admitted));

    public static LayerEdit Override(LayerOverride value) => new OverrideCase(Value: value);

    private static Fin<LayerEdit> Indexed(int index, int floor, Func<int, LayerEdit> mint) =>
        guard(index >= floor, Op.Of().InvalidInput()).ToFin().Map(_ => mint(arg: index));

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
            descriptionCase: static (context, edit) => Write(op: context.Op, write: () => context.Staged.Description = edit.Value),
            igesLevelCase: static (context, edit) => Write(op: context.Op, write: () => context.Staged.IgesLevel = edit.Value),
            overrideCase: static (context, edit) => edit.Value.Apply(layer: context.Staged, key: context.Op));

    internal static Fin<Unit> Write(Op op, Action write) =>
        op.Catch(() => {
            write();
            return Fin.Succ(value: unit);
        });

    internal static Fin<Unit> Toggle<T>(Op op, Option<T> value, Action<T> set, Action clear) =>
        Write(op: op, write: () => value.Match(Some: chosen => fun(() => set(obj: chosen))(), None: () => fun(clear)()));

    internal static Fin<double> Width(double value, Op op) =>
        guard(double.IsFinite(value) && value >= -1.0, op.InvalidInput()).ToFin().Map(_ => value);
}
```

## [05]-[COMMIT_RAIL]

- Owner: `LayerOp` `[Union]` closes the structural mutation family; `LayerArrangement` `[Union]` closes sibling ordering; `LayerDelta` admits one named program with its `RedrawPolicy`; `LayerSlot` `[SmartEnum<int>]` names structural consequences, and `LayerReceipt` is the additive fold over one internal `LayerFact` stream with the sealed undo serial as a fact.
- Entry: `Layers.Ask` is the read window; `Layers.Commit` derives its needs through `SessionNeed.Mutation`, demands once, and commits through `DocumentCommit.Sealed` — suppress, fold every operation into one receipt, seal the record with the serial stamped as an `UndoCase` fact, restore redraw state on every outcome, then repaint after restoration so a suppressing policy still lands its terminal redraw.
- Law: reparent is staged mutation with a cycle guard — the resolved new parent must not be a child of the target — and the root move writes the empty parent id; rename and every face edit ride the same staged-copy-then-`Modify` path, so a failed program never half-writes a live layer.
- Law: merge is object custody before structure — every object on the source layer re-homes through a retained attribute snapshot, any failed move or source deletion restores the landed prefix, and successful cleanup evidence lands beside the tally; source equal to target is refused at admission.
- Law: purge tallies compose `TableKind.Layers.Reclaim` — the vocabulary row stays the one reclamation delegate — and revive addresses the dead row by id or index with deleted resolution, the only path that may see a deleted layer.
- Law: explicit arrangement admits one complete permutation of every active layer before the native sort boundary.
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
        Op op = Op.Of();
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
                    .Traverse(address => address.Index(document: context.Document, includeDeleted: false, key: context.Op).ToValidation())
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
    private sealed record GraftCase(LayerPath Path, Option<Color> Color) : LayerOp;
    private sealed record AmendCase(LayerRef Target, Seq<LayerEdit> Edits) : LayerOp;
    private sealed record ReparentCase(LayerRef Target, Option<LayerRef> Parent) : LayerOp;
    private sealed record MergeCase(LayerRef Source, LayerRef Target) : LayerOp;
    private sealed record DuplicateCase(LayerRef Target, bool Objects, bool Sublayers) : LayerOp;
    private sealed record DeleteCase(LayerRef Target, Notice Notice) : LayerOp;
    private sealed record PurgeCase(LayerRef Target, Notice Notice) : LayerOp;
    private sealed record ReviveCase(LayerRef Target) : LayerOp;
    private sealed record AnointCase(LayerRef Target, Notice Notice) : LayerOp;
    private sealed record ExposeCase(LayerRef Target) : LayerOp;
    private sealed record ArrangeCase(LayerArrangement Arrangement) : LayerOp;
    private sealed record RollbackCase(LayerRef Target, Option<uint> UndoSerial) : LayerOp;
    private sealed record ReclaimCase : LayerOp;

    public static Fin<LayerOp> Create(LayerName name, Option<LayerRef> parent = default, params ReadOnlySpan<LayerEdit> edits) =>
        Admission.All(values: edits, key: Op.Of())
            .Map(admitted => (LayerOp)new CreateCase(Name: name, Parent: parent, Edits: admitted));

    public static Fin<LayerOp> Graft(LayerPath path, Option<Color> color = default) =>
        guard(path != default, Op.Of().InvalidInput()).ToFin()
            .Map(_ => (LayerOp)new GraftCase(Path: path, Color: color));

    public static Fin<LayerOp> Amend(LayerRef target, params ReadOnlySpan<LayerEdit> edits) {
        Op op = Op.Of();
        return from address in Optional(target).ToFin(Fail: op.InvalidInput())
               from admitted in Admission.All(values: edits, key: op)
               from _ in guard(!admitted.IsEmpty, op.InvalidInput()).ToFin()
               select (LayerOp)new AmendCase(Target: address, Edits: admitted);
    }

    public static Fin<LayerOp> Reparent(LayerRef target, Option<LayerRef> parent = default) =>
        Addressed(target: target, mint: address => new ReparentCase(Target: address, Parent: parent));

    public static Fin<LayerOp> Merge(LayerRef source, LayerRef target) {
        Op op = Op.Of();
        return from origin in Optional(source).ToFin(Fail: op.InvalidInput())
               from destination in Optional(target).ToFin(Fail: op.InvalidInput())
               from _ in guard(origin != destination, op.InvalidInput()).ToFin()
               select (LayerOp)new MergeCase(Source: origin, Target: destination);
    }

    public static Fin<LayerOp> Duplicate(LayerRef target, bool objects, bool sublayers) =>
        Addressed(target: target, mint: address => new DuplicateCase(Target: address, Objects: objects, Sublayers: sublayers));

    public static Fin<LayerOp> Delete(LayerRef target, Notice notice) => Noticed(target: target, notice: notice, mint: static (a, n) => new DeleteCase(Target: a, Notice: n));

    public static Fin<LayerOp> Purge(LayerRef target, Notice notice) => Noticed(target: target, notice: notice, mint: static (a, n) => new PurgeCase(Target: a, Notice: n));

    public static Fin<LayerOp> Revive(LayerRef target) => Addressed(target: target, mint: static address => new ReviveCase(Target: address));

    public static Fin<LayerOp> Anoint(LayerRef target, Notice notice) => Noticed(target: target, notice: notice, mint: static (a, n) => new AnointCase(Target: a, Notice: n));

    public static Fin<LayerOp> Expose(LayerRef target) => Addressed(target: target, mint: static address => new ExposeCase(Target: address));

    public static Fin<LayerOp> Arrange(LayerArrangement arrangement) =>
        Optional(arrangement).ToFin(Fail: Op.Of().InvalidInput()).Map(order => (LayerOp)new ArrangeCase(Arrangement: order));

    public static Fin<LayerOp> Rollback(LayerRef target, Option<uint> undoSerial = default) {
        Op op = Op.Of();
        return (
                Optional(target).ToFin(Fail: op.InvalidInput()).ToValidation(),
                guard(undoSerial.Map(static serial => serial > 0u).IfNone(noneValue: true), op.InvalidInput()).ToFin().ToValidation())
            .Apply(static (address, _) => address)
            .As()
            .ToFin()
            .Map(address => (LayerOp)new RollbackCase(Target: address, UndoSerial: undoSerial));
    }

    public static LayerOp Reclaim { get; } = new ReclaimCase();

    private static Fin<LayerOp> Addressed(LayerRef target, Func<LayerRef, LayerOp> mint) =>
        Optional(target).ToFin(Fail: Op.Of().InvalidInput()).Map(address => mint(arg: address));

    private static Fin<LayerOp> Noticed(LayerRef target, Notice notice, Func<LayerRef, Notice, LayerOp> mint) {
        Op op = Op.Of();
        return (
                Optional(target).ToFin(Fail: op.InvalidInput()).ToValidation(),
                Optional(notice).ToFin(Fail: op.InvalidInput()).ToValidation())
            .Apply((address, reporting) => mint(address, reporting))
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
                from index in context.Op.Catch(() => {
                    Layer minted = new() { Name = edit.Name.Value };
                    parent.IfSome(id => minted.ParentLayerId = id);
                    return Fin.Succ(value: context.Document.Layers.Add(layer: minted));
                })
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
                from receipt in Amended(document: context.Document, index: index, edits: edit.Edits, slot: LayerSlot.Amended, op: context.Op)
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
                    layerIndex: index,
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
                from _ in context.Op.Confirm(success: context.Document.Layers.Delete(layerIndex: target.LayerIndex, quiet: edit.Notice.SuppressesWarnings))
                select LayerReceipt.Node(slot: LayerSlot.Deleted, stamp: stamp),
            purgeCase: static (context, edit) =>
                from target in edit.Target.Resolve(document: context.Document, includeDeleted: true, key: context.Op)
                from stamp in LayerStamp.Of(layer: target, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Layers.Purge(layerIndex: target.LayerIndex, quiet: edit.Notice.SuppressesWarnings))
                select LayerReceipt.Node(slot: LayerSlot.Purged, stamp: stamp),
            reviveCase: static (context, edit) =>
                from index in edit.Target.Index(document: context.Document, includeDeleted: true, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Layers.Undelete(layerIndex: index))
                from stamp in Stamped(document: context.Document, index: index, op: context.Op)
                select LayerReceipt.Node(slot: LayerSlot.Revived, stamp: stamp),
            anointCase: static (context, edit) =>
                from index in edit.Target.Index(document: context.Document, includeDeleted: false, key: context.Op)
                from _ in context.Op.Confirm(success: context.Document.Layers.SetCurrentLayerIndex(
                    layerIndex: index,
                    quiet: edit.Notice.SuppressesWarnings))
                from stamp in Stamped(document: context.Document, index: index, op: context.Op)
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
                    Some: serial => context.Document.Layers.UndoModify(layerIndex: index, undoRecordSerialNumber: serial),
                    None: () => context.Document.Layers.UndoModify(layerIndex: index)))
                from stamp in Stamped(document: context.Document, index: index, op: context.Op)
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
        from staged in op.Catch(() => {
            Layer copy = new();
            copy.CopyAttributesFrom(otherLayer: live);
            return Fin.Succ(value: copy);
        })
        from _ in revise(arg: staged)
        from landed in op.Confirm(success: document.Layers.Modify(newSettings: staged, layerIndex: index, quiet: true))
        from stamp in Stamped(document: document, index: index, op: op)
        select LayerReceipt.Node(slot: slot, stamp: stamp);

    private sealed record LayerMove(Guid ObjectId, ObjectAttributes Original);

    private sealed record LayerMerge(int Relayered, Seq<Error> CleanupFaults);

    private static Fin<LayerMerge> Merged(RhinoDoc document, int sourceIndex, int targetIndex, Op op) =>
        Rehomed(document: document, sourceIndex: sourceIndex, targetIndex: targetIndex, op: op).Bind(moves =>
            op.Confirm(success: document.Layers.Delete(layerIndex: sourceIndex, quiet: true)).Match(
                Succ: _ => Fin.Succ(value: new LayerMerge(
                    Relayered: moves.Count,
                    CleanupFaults: Release(moves: moves, op: op))),
                Fail: primary => Restore(document: document, moves: moves, op: op).Match(
                    Succ: _ => Fin.Fail<LayerMerge>(error: primary),
                    Fail: rollback => Fin.Fail<LayerMerge>(error: primary + rollback))));

    private static Fin<Seq<LayerMove>> Rehomed(RhinoDoc document, int sourceIndex, int targetIndex, Op op) =>
        from residents in op.Catch(() => Fin.Succ(value: toSeq(document.Objects.GetObjectList(settings: new ObjectEnumeratorSettings {
            NormalObjects = true,
            LockedObjects = true,
            HiddenObjects = true,
            IncludeLights = true,
            LayerIndexFilter = sourceIndex,
        })).Strict()))
        from moved in residents.Fold(
            Fin.Succ(value: Seq<LayerMove>()),
            (rail, native) => rail.Bind(held => Move(
                    document: document,
                    native: native,
                    targetIndex: targetIndex,
                    op: op)
                .Map(held.Add)
                .MapFail(primary => Restore(document: document, moves: held, op: op).Match(
                    Succ: _ => primary,
                    Fail: rollback => primary + rollback))))
        select moved;

    private static Fin<LayerMove> Move(RhinoDoc document, RhinoObject native, int targetIndex, Op op) =>
        from original in op.Catch(() => Optional(native.Attributes?.Duplicate()).ToFin(Fail: op.InvalidResult()))
        from staged in op.Catch(() => Optional(native.Attributes?.Duplicate()).ToFin(Fail: op.InvalidResult()))
            .MapFail(primary => Dispose(value: original, op: op).Match(
                Succ: _ => primary,
                Fail: cleanup => primary + cleanup))
        from _ in new Lease<ObjectAttributes>.Owned(Value: staged).Use(owned => {
            owned.LayerIndex = targetIndex;
            return op.Confirm(success: document.Objects.ModifyAttributes(
                objectId: native.Id,
                newAttributes: owned,
                quiet: true));
        }).MapFail(primary => Dispose(value: original, op: op).Match(
            Succ: _ => primary,
            Fail: cleanup => primary + cleanup))
        select new LayerMove(ObjectId: native.Id, Original: original);

    private static Fin<Unit> Restore(RhinoDoc document, Seq<LayerMove> moves, Op op) => moves.Rev()
        .Traverse(move => Restore(document: document, move: move, op: op).ToValidation())
        .As()
        .ToFin()
        .Map(static _ => unit);

    private static Fin<Unit> Restore(RhinoDoc document, LayerMove move, Op op) {
        Fin<Unit> mutation = op.Catch(() => op.Confirm(success: document.Objects.ModifyAttributes(
            objectId: move.ObjectId,
            newAttributes: move.Original,
            quiet: true)));
        Fin<Unit> cleanup = Dispose(value: move.Original, op: op);
        return mutation.Match(
            Succ: _ => cleanup,
            Fail: primary => cleanup.Match(
                Succ: _ => Fin.Fail<Unit>(error: primary),
                Fail: release => Fin.Fail<Unit>(error: primary + release)));
    }

    private static Seq<Error> Release(Seq<LayerMove> moves, Op op) => moves
        .Choose(move => Dispose(value: move.Original, op: op).Match(
            Succ: static _ => Option<Error>.None,
            Fail: static error => Some(error)));

    private static Fin<Unit> Dispose(ObjectAttributes value, Op op) =>
        op.Catch(() => Fin.Succ(value: Op.Side(value.Dispose)));
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
        Seq<string> CleanupFaults) : LayerFact;
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
            CleanupFaults: cleanupFaults.Map(static error => error.Message)));

    internal static LayerReceipt Order(int count) => Of(fact: new LayerFact.OrderCase(Count: count));

    internal static LayerReceipt Reclaimed(int tally) => Of(fact: new LayerFact.ReclaimCase(Tally: tally));

    internal static LayerReceipt Undo(uint serial) => Of(fact: new LayerFact.UndoCase(Serial: serial));

    private static LayerReceipt Of(LayerFact fact) => new(facts: Seq(fact));

    public Fin<Seq<LayerStamp>> Stamps(LayerSlot slot, Op? key = null) =>
        Optional(slot).ToFin(Fail: key.OrDefault().InvalidInput()).Map(admitted =>
            facts.Choose(fact => fact is LayerFact.NodeCase { Slot: var factSlot, Stamp: var stamp } && factSlot == admitted
                ? Some(stamp)
                : Option<LayerStamp>.None));

    public Seq<(LayerStamp Source, LayerStamp Target, int Relayered, Seq<string> CleanupFaults)> Merges =>
        facts.Choose(static fact => fact is LayerFact.MergeCase merge
            ? Some((merge.Source, merge.Target, merge.Relayered, merge.CleanupFaults))
            : Option<(LayerStamp, LayerStamp, int, Seq<string>)>.None);

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
        Op op = Op.Of();
        return from label in op.AcceptText(value: name)
               from policy in Optional(redraw).ToFin(Fail: op.InvalidInput())
               from admitted in Admission.All(values: operations, key: op)
               from _ in guard(!admitted.IsEmpty, op.InvalidInput()).ToFin()
               select new LayerDelta(name: label, redraw: policy, operations: admitted);
    }
}

public static class Layers {
    public static Fin<LayerTree> Ask(DocumentSession session, Op? key = null, params ReadOnlySpan<Guid> detailViewports) {
        Op op = key.OrDefault();
        Seq<Guid> probes = toSeq(detailViewports.ToArray());
        return from scope in Optional(session).ToFin(Fail: op.InvalidInput())
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

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]              | [OWNER]                     | [FORM]                             | [ENTRY]                       |
| :-----: | :--------------------- | :-------------------------- | :--------------------------------- | :---------------------------- |
|  [01]   | leaf and path identity | `LayerName` / `LayerPath`   | generated host-grammar values      | `Of` / `Segments` / `Child`   |
|  [02]   | layer addressing       | `LayerRef`                  | id/index/path/current union        | `ById` / `AtIndex` / `AtPath` / `Current` |
|  [03]   | detached anchor        | `LayerStamp`                | id/index/path evidence product     | receipt and node rows         |
|  [04]   | tree topology          | `LayerTree` / `LayerNode`   | one-read recursive snapshot        | `Layers.Ask` / `Find`         |
|  [05]   | per-detail overrides   | `LayerOverride`             | option-discriminated write/clear   | `LayerEdit.Override`          |
|  [06]   | staged property edits  | `LayerEdit`                 | closed staged-write union          | edit factories / `Amend`      |
|  [07]   | structural mutation    | `LayerOp`                   | admitted total operation union     | operation factories / `Apply` |
|  [08]   | sibling ordering       | `LayerArrangement`          | by-name/explicit union             | `LayerOp.Arrange`             |
|  [09]   | commit program         | `LayerDelta`                | named redraw-scoped program        | `Layers.Commit`               |
|  [10]   | consequence evidence   | `LayerReceipt` / `LayerSlot`| stamped fact stream + undo serial  | typed projections             |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
