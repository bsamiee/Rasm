# [RASM_RHINO_RENDER_CONTENT]

`ContentRef` owns live RDK graph identity across resolution, table routing, scoped mutation, detached topology, replayable hash evidence, and serialized ingress. `ContentKind`, `ContentStyle`, `ContentTrait`, `ProxyKind`, `ChangeReason`, and `HashAxis` translate native discriminants once; every live `RenderContent` remains demand-window bound, and every unattached mint remains an owned `Lease<RenderContent>`.

## [01]-[INDEX]

- [02]-[KIND_AND_REASON]: `ContentKind` — the kind axis with table behavior columns; `ChangeReason` — the change-context vocabulary; `ContentStyle` — the capability mask; `ChangeScope` — the write bracket.
- [03]-[ADDRESS]: `ContentRef` — the one content address union and its resolution fold.
- [04]-[SNAPSHOT_AND_HASH]: `SlotState`, `ContentTrait`, `ContentSnapshot`, and the `HashAxis`/`HashProbe` render-hash read.
- [05]-[INGRESS]: `ContentIo` — leased XML and file mints.
- [06]-[SURFACE_LEDGER]: page owner table.

## [02]-[KIND_AND_REASON]

- Owner: `ContentKind` rows own kind-specific table behavior, `ContentStyle` is the capability vocabulary the native style mask decodes onto, `ProxyKind` classifies proxy topology, `ChangeReason` translates change context, and `ChangeScope` closes every direct mutation bracket. Sibling mutation pipelines commit through the Document spine's `DocumentCommit.Sealed` (which owns the suppress/restore/flush bracket), and `Bridge` carries the two folder-wide projections — `Row` (a host column onto its vocabulary row) and `Minted` (a null-answering host mint into owned custody).
- Entry: `ContentKind.Attach`/`Detach` are the result-typed table writes; `ContentKind.Table` is the ONE table change window, and `Roster` the kind's live census.
- Law: kind is derived, never asked — each `ContentKind` row carries a `Holds` predicate over the live subtype, `ContentKind.Of(RenderContent, Op)` derives from `Items`, and `ContentKind.Of(RenderContentKind, Op)` admits the native discriminant through the kernel host-enum row read, so an undefined ordinal refuses before the roster is scanned. Null ingress is invalid input; an unmatched live subtype is an invalid host result.
- Law: every direct field, parameter, parameter-binding, texture, rename, or child-slot write rides `ChangeScope.Write` with a named `ChangeReason`; host-owned table, assignment, replacement, grouping, and export verbs retain their own change semantics.
- Law: host begin/end windows are CUSTODY, never a `finally` — `ChangeScope` and `TableScope` are `IDisposable` windows carried on `Lease<T>`, so the kernel `Use` fold aggregates an `EndChange`/`EndChange` refusal INTO the body's own fault. Wrapping the same pair in `try/finally` silently replaces the body's fault with the release's, which is the deleted form.
- Law: `ContentKind` columns are the only site naming `RenderMaterials`/`RenderEnvironments`/`RenderTextures`; every content operation reaches a table through its kind row.
- Growth: a new change context is one `ChangeReason` row; a new content kind is one `ContentKind` row whose columns close its table behavior.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderContent`, `RenderContentKind`, `RenderContentStyles`, `ProxyTypes`, `RenderContent.ChangeContexts`, `BeginChange`/`EndChange`, `IRenderContentTable<T>.Add`/`Remove`, `RenderMaterialTable.BeginChange`/`EndChange`/`GetEnumerator`); kernel `Domain/results` (`Op`, `Op.Catch`, `Op.Confirm`, `Lease<T>.Acquire`/`Use`), `Domain/validation` (`ICapability`, `CapabilitySet`, `Op.Row`); LanguageExt.Core (`Fin`, `Seq`, `Option`); Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[UseDelegateFromConstructor]`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using Thinktecture;

namespace Rasm.Rhino.Render;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ContentKind {
    public static readonly ContentKind Material = new(
        key: (int)RenderContentKind.Material,
        holds: static content => content is RenderMaterial,
        added: static (document, content) => content is RenderMaterial value && document.RenderMaterials.Add(value),
        removed: static (document, content) => content is RenderMaterial value && document.RenderMaterials.Remove(value),
        opened: static (document, reason) => document.RenderMaterials.BeginChange(reason),
        closed: static document => document.RenderMaterials.EndChange(),
        roster: static document => toSeq(document.RenderMaterials).Map(static content => (RenderContent)content));
    public static readonly ContentKind Environment = new(
        key: (int)RenderContentKind.Environment,
        holds: static content => content is RenderEnvironment,
        added: static (document, content) => content is RenderEnvironment value && document.RenderEnvironments.Add(value),
        removed: static (document, content) => content is RenderEnvironment value && document.RenderEnvironments.Remove(value),
        opened: static (document, reason) => document.RenderEnvironments.BeginChange(reason),
        closed: static document => document.RenderEnvironments.EndChange(),
        roster: static document => toSeq(document.RenderEnvironments).Map(static content => (RenderContent)content));
    public static readonly ContentKind Texture = new(
        key: (int)RenderContentKind.Texture,
        holds: static content => content is RenderTexture,
        added: static (document, content) => content is RenderTexture value && document.RenderTextures.Add(value),
        removed: static (document, content) => content is RenderTexture value && document.RenderTextures.Remove(value),
        opened: static (document, reason) => document.RenderTextures.BeginChange(reason),
        closed: static document => document.RenderTextures.EndChange(),
        roster: static document => toSeq(document.RenderTextures).Map(static content => (RenderContent)content));

    [UseDelegateFromConstructor]
    internal partial bool Holds(RenderContent content);

    [UseDelegateFromConstructor]
    private partial bool Added(RhinoDoc document, RenderContent content);

    [UseDelegateFromConstructor]
    private partial bool Removed(RhinoDoc document, RenderContent content);

    [UseDelegateFromConstructor]
    internal partial void Opened(RhinoDoc document, RenderContent.ChangeContexts reason);

    [UseDelegateFromConstructor]
    internal partial void Closed(RhinoDoc document);

    [UseDelegateFromConstructor]
    internal partial Seq<RenderContent> Roster(RhinoDoc document);

    internal Fin<Unit> Attach(RhinoDoc document, RenderContent content, Op key) =>
        key.Catch(() => key.Confirm(success: Added(document: document, content: content)));

    internal Fin<Unit> Detach(RhinoDoc document, RenderContent content, Op key) =>
        key.Catch(() => key.Confirm(success: Removed(document: document, content: content)));

    internal Fin<TOut> Table<TOut>(RhinoDoc document, ChangeReason reason, Func<RhinoDoc, Fin<TOut>> body, Op key) =>
        Lease<TableScope>.Acquire(mint: () => new TableScope(kind: this, document: document, reason: reason), key: key)
            .Bind(scope => scope.Use(body: _ => body(arg: document), key: key));

    public static Fin<ContentKind> Of(RenderContent? content, Op key) =>
        key.Need(content).Bind(active =>
            toSeq(Items)
                .Filter(row => row.Holds(content: active))
                .Head
                .ToFin(Fail: key.InvalidResult(detail: active.GetType().Name)));

    internal static Fin<ContentKind> Of(RenderContentKind native, Op key) =>
        key.Row<RenderContentKind, ContentKind>(native, static value => (int)value);
}

[SmartEnum<int>]
public sealed partial class ChangeReason {
    public static readonly ChangeReason Ui = new(key: (int)RenderContent.ChangeContexts.UI);
    public static readonly ChangeReason Drop = new(key: (int)RenderContent.ChangeContexts.Drop);
    public static readonly ChangeReason Program = new(key: (int)RenderContent.ChangeContexts.Program);
    public static readonly ChangeReason Ignore = new(key: (int)RenderContent.ChangeContexts.Ignore);
    public static readonly ChangeReason Tree = new(key: (int)RenderContent.ChangeContexts.Tree);
    public static readonly ChangeReason Undo = new(key: (int)RenderContent.ChangeContexts.Undo);
    public static readonly ChangeReason FieldInit = new(key: (int)RenderContent.ChangeContexts.FieldInit);
    public static readonly ChangeReason Serialize = new(key: (int)RenderContent.ChangeContexts.Serialize);
    public static readonly ChangeReason RealTimeUi = new(key: (int)RenderContent.ChangeContexts.RealTimeUI);
    public static readonly ChangeReason Script = new(key: (int)RenderContent.ChangeContexts.Script);

    internal RenderContent.ChangeContexts Native => (RenderContent.ChangeContexts)Key;

    internal static Fin<ChangeReason> Of(RenderContent.ChangeContexts native, Op key) =>
        key.Row<RenderContent.ChangeContexts, ChangeReason>(native, static value => (int)value);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContentStyle : ICapability<ContentStyle> {
    public static readonly ContentStyle TextureSummary = new(key: "texture-summary", bit: (int)RenderContentStyles.TextureSummary);
    public static readonly ContentStyle QuickPreview = new(key: "quick-preview", bit: (int)RenderContentStyles.QuickPreview);
    public static readonly ContentStyle PreviewCache = new(key: "preview-cache", bit: (int)RenderContentStyles.PreviewCache);
    public static readonly ContentStyle ProgressivePreview = new(key: "progressive-preview", bit: (int)RenderContentStyles.ProgressivePreview);
    public static readonly ContentStyle LocalTextureMapping = new(key: "local-texture-mapping", bit: (int)RenderContentStyles.LocalTextureMapping);
    public static readonly ContentStyle GraphDisplay = new(key: "graph-display", bit: (int)RenderContentStyles.GraphDisplay);
    public static readonly ContentStyle Adjustment = new(key: "adjustment", bit: (int)RenderContentStyles.Adjustment);
    public static readonly ContentStyle Fields = new(key: "fields", bit: (int)RenderContentStyles.Fields);
    public static readonly ContentStyle ModalEditing = new(key: "modal-editing", bit: (int)RenderContentStyles.ModalEditing);
    public static readonly ContentStyle DynamicFields = new(key: "dynamic-fields", bit: (int)RenderContentStyles.DynamicFields);
    public static readonly ContentStyle NameTypeSection = new(key: "name-type-section", bit: (int)RenderContentStyles.NameTypeSection);

    internal int Bit { get; }

    internal static Fin<CapabilitySet<ContentStyle>> Of(RenderContentStyles native, Op key) =>
        CapabilitySet<ContentStyle>.OfMask(mask: (int)native, bit: static row => row.Bit, key: key);
}

[SmartEnum<int>]
public sealed partial class ProxyKind {
    public static readonly ProxyKind None = new(key: (int)ProxyTypes.None);
    public static readonly ProxyKind Single = new(key: (int)ProxyTypes.Single);
    public static readonly ProxyKind Multi = new(key: (int)ProxyTypes.Multi);
    public static readonly ProxyKind Texture = new(key: (int)ProxyTypes.Texture);

    internal static Fin<ProxyKind> Of(ProxyTypes native, Op key) =>
        key.Row<ProxyTypes, ProxyKind>(native, static value => (int)value);
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ChangeScope : IDisposable {
    private readonly RenderContent content;

    private ChangeScope(RenderContent content, ChangeReason reason) {
        this.content = content;
        content.BeginChange(reason.Native);
    }

    public void Dispose() => content.EndChange();

    internal static Fin<TOut> Write<TOut>(RenderContent content, ChangeReason reason, Func<RenderContent, Fin<TOut>> body, Op key) =>
        Lease<ChangeScope>.Acquire(mint: () => new ChangeScope(content: content, reason: reason), key: key)
            .Bind(scope => scope.Use(body: _ => body(arg: content), key: key));
}

internal sealed class TableScope : IDisposable {
    private readonly ContentKind kind;
    private readonly RhinoDoc document;

    internal TableScope(ContentKind kind, RhinoDoc document, ChangeReason reason) {
        (this.kind, this.document) = (kind, document);
        kind.Opened(document: document, reason: reason.Native);
    }

    public void Dispose() => kind.Closed(document: document);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Bridge {
    internal static Fin<TRow> Row<TRow, TNative>(this Op key, IEnumerable<TRow> rows, TNative native, Func<TRow, TNative> project)
        where TRow : class where TNative : notnull =>
        Optional(rows.FirstOrDefault(row => EqualityComparer<TNative>.Default.Equals(project(row), native)))
            .ToFin(Fail: key.InvalidResult(detail: native.ToString() ?? string.Empty));

    internal static Fin<Lease<RenderContent>> Minted(Func<RenderContent?> mint, Op key) =>
        key.Catch(() => Optional(mint()).ToFin(Fail: key.InvalidResult()))
            .Map(static value => (Lease<RenderContent>)new Lease<RenderContent>.Owned(Value: value));
}
```

## [03]-[ADDRESS]

- Owner: `ContentRef` `[Union]` — `ById` over the content instance guid, `AtSlot` over a root guid and a child-slot-name path; one `Resolve` fold answers the live `RenderContent` and every arm treats missing content or a broken path as absent.
- Law: `ContentRef` is the package's content identity — facts, event facts, and settings bindings carry the guid; a slot path addresses a child without a consumer walking `FirstChild`/`NextSibling`.
- Law: resolution reads live per call — the content graph mutates under UI edits, undo, and linked events, so no resolved handle is cached on a value; a consumer holding a `ContentRef` re-resolves at each use inside the owning operation.
- Law: every public factory threads the caller's `Op` — a key minted inside the owner names the owner instead of the operation that asked, so the fault loses the call site it came from.
- Boundary: `Resolve` is the only site naming `RenderContent.FromId` and `FindChild`; every sibling page addresses through this union.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderContent.FromId`, `FindChild`); kernel `Domain/results` (`Op.OrDefault`, `Op.AcceptText`, `Op.MissingContext`); LanguageExt.Core (`Fin`, `Seq`, `guard`, `TraverseM`); Thinktecture.Runtime.Extensions (`[Union]`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentRef {
    private ContentRef() { }
    private sealed record ById(Guid Value) : ContentRef;
    private sealed record AtSlot(Guid Root, Seq<string> Path) : ContentRef;

    public static Fin<ContentRef> Of(Guid id, Op? key = null) {
        Op op = key.OrDefault();
        return id != Guid.Empty
            ? Fin.Succ<ContentRef>(value: new ById(Value: id))
            : Fin.Fail<ContentRef>(error: op.InvalidInput());
    }

    public static Fin<ContentRef> Of(Guid root, Op? key, params ReadOnlySpan<string> path) {
        Op op = key.OrDefault();
        return from _ in guard(root != Guid.Empty, op.InvalidInput())
               from slots in toSeq(path.ToArray()).TraverseM(slot => op.AcceptText(value: slot)).As()
               from __ in guard(!slots.IsEmpty, op.InvalidInput())
               select (ContentRef)new AtSlot(Root: root, Path: slots);
    }

    internal Fin<RenderContent> Resolve(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            byId: static (ctx, address) =>
                Optional(RenderContent.FromId(document: ctx.Document, id: address.Value)).ToFin(Fail: ctx.Op.MissingContext()),
            atSlot: static (ctx, address) =>
                Optional(RenderContent.FromId(document: ctx.Document, id: address.Root)).ToFin(Fail: ctx.Op.MissingContext())
                    .Bind(root => address.Path.Fold(
                        Fin.Succ(value: root),
                        (state, slot) => state.Bind(parent =>
                            Optional(parent.FindChild(childSlotName: slot)).ToFin(Fail: ctx.Op.MissingContext())))));
}
```

## [04]-[SNAPSHOT_AND_HASH]

- Owner: `SlotState` carries one occupied child-slot fact. `ContentTrait` is the content state vocabulary. `ContentSnapshot` carries detached identity, metadata, ownership serials, native discriminants, held traits, tree position, slot roster, and usage. `HashAxis` is the exclusion vocabulary the host flag word decodes onto, and `HashProbe.Read` answers the `HashWitness` naming the axes, the exclusions, the scope, and the value together.
- Law: `RenderContentStyles`, `ProxyTypes`, `LengthUnit`, and `CrcRenderHashFlags` stop at this section; the detached snapshot carries a `CapabilitySet<ContentStyle>`, one `ProxyKind`, the kernel `ModelUnit`, and a `CapabilitySet<ContentTrait>`, so downstream branches never decode host discriminants again.
- Law: the snapshot carries the kernel `ModelUnit` because a content's own magnitudes — texture repeat, offset, and slot amount — are authored in that regime, and it is the only carrier publishing it; the raw `LengthUnit` lives inside the host read alone.
- Law: `ContentSnapshot.Of` walks `FirstChild`/`NextSibling` once as one `List.unfold` over the host cursor and reads `ChildSlotOn`/`ChildSlotAmount` during that visit.
- Law: `HashAxis` rows carry the host's ATOMIC exclusion bits; `ForSimulation` and `ExcludeDocumentEffects` are a host ALIAS and a host COMPOSITE of those bits, so both derive as sets rather than entering as peer rows a mask fold double-counts. Postures the host names no member for — local mapping beside units — compose as sets with no new vocabulary.
- Law: the read owns its own witness — `Read` mints the `HashWitness`, so the workflow scope recorded on the witness is the scope the read took. Storing a posture flag on the probe beside a caller-chosen overload lets the witness disagree with the call, which is the deleted form.
- Law: a live `LinearWorkflow` never reaches a stored value — it enters `Read` as an argument the caller resolved inside its own demand window, and only the `HashScope` row it selects crosses onto the witness.
- Growth: a content fact is one `ContentTrait` row with its predicate; an exclusion axis is one `HashAxis` row with its bit.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderHash`, `RenderHashExclude` both arities, `Styles`, `ProxyType`, `ModelUnits`, `TopLevel`/`Hidden`/`Private`/`IsLocked`/`CanBeEdited`/`IsDefaultInstance`/`IsHiddenByAutoDelete`, `IsReference`, `UseCount`, `DocumentOwner`/`DocumentAssoc`, `FirstChild`/`NextSibling`/`ChildSlotName`/`ChildSlotDisplayName`, `ChildSlotOn`/`ChildSlotAmount`); `api-rhinocommon-document.md` (`LengthUnit`); kernel `Domain/context` (`ModelUnit.Of(LengthUnit, Op)`), `Domain/validation` (`ICapability`, `CapabilitySet.Of`/`OfMask`/`Mask`/`Wire`); LanguageExt.Core (`List.unfold`, `Seq`, `Option`); Thinktecture.Runtime.Extensions (`[SmartEnum]`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ContentTrait : ICapability<ContentTrait> {
    public static readonly ContentTrait TopLevel = new(key: "top-level", holds: static content => content.TopLevel);
    public static readonly ContentTrait Hidden = new(key: "hidden", holds: static content => content.Hidden);
    public static readonly ContentTrait Private = new(key: "private", holds: static content => content.Private);
    public static readonly ContentTrait Locked = new(key: "locked", holds: static content => content.IsLocked);
    public static readonly ContentTrait Editable = new(key: "editable", holds: static content => content.CanBeEdited);
    public static readonly ContentTrait DefaultInstance = new(key: "default-instance", holds: static content => content.IsDefaultInstance);
    public static readonly ContentTrait AutoDeleteHidden = new(key: "auto-delete-hidden", holds: static content => content.IsHiddenByAutoDelete);
    public static readonly ContentTrait Reference = new(key: "reference", holds: static content => content.IsReference());

    [UseDelegateFromConstructor]
    private partial bool Holds(RenderContent content);

    internal static CapabilitySet<ContentTrait> Of(RenderContent content) =>
        CapabilitySet<ContentTrait>.Of(Items.Where(row => row.Holds(content: content)).ToArray());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HashAxis : ICapability<HashAxis> {
    public static readonly HashAxis LinearWorkflow = new(key: "linear-workflow", bit: (int)CrcRenderHashFlags.ExcludeLinearWorkflow);
    public static readonly HashAxis LocalMapping = new(key: "local-mapping", bit: (int)CrcRenderHashFlags.ExcludeLocalMapping);
    public static readonly HashAxis Units = new(key: "units", bit: (int)CrcRenderHashFlags.ExcludeUnits);
    public static readonly HashAxis DocumentEffects = new(key: "document-effects",
        bit: (int)CrcRenderHashFlags.ExcludeDocumentEffects
            & ~(int)(CrcRenderHashFlags.ExcludeLinearWorkflow | CrcRenderHashFlags.ExcludeUnits));

    internal int Bit { get; }

    internal static CrcRenderHashFlags Flags(CapabilitySet<HashAxis> axes) =>
        (CrcRenderHashFlags)axes.Mask(bit: static row => row.Bit);
}

[SmartEnum<bool>]
public sealed partial class HashScope {
    public static readonly HashScope Free = new(key: false);
    public static readonly HashScope Documented = new(key: true);

    internal static HashScope Of(bool documented) => documented ? Documented : Free;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SlotState(string Name, string DisplayName, Guid Child, bool On, double Amount);

public readonly record struct HashWitness(
    CapabilitySet<HashAxis> Axes, Seq<string> Excluded, HashScope Scope, uint Value) : IDetachedDocumentResult;

public sealed record HashProbe {
    private HashProbe(CapabilitySet<HashAxis> axes, Seq<string> excludedParameters) =>
        (Axes, ExcludedParameters) = (axes, excludedParameters);

    public static HashProbe Whole { get; } = new(CapabilitySet<HashAxis>.None, Seq<string>());
    public static HashProbe ForSimulation { get; } = new(CapabilitySet<HashAxis>.Of(HashAxis.LinearWorkflow), Seq<string>());
    public static HashProbe DocumentFree { get; } = new(
        CapabilitySet<HashAxis>.Of(HashAxis.LinearWorkflow, HashAxis.Units, HashAxis.DocumentEffects), Seq<string>());

    public CapabilitySet<HashAxis> Axes { get; }
    public Seq<string> ExcludedParameters { get; }

    public static Fin<HashProbe> Excluding(CapabilitySet<HashAxis> axes, Op? key, params ReadOnlySpan<string> parameters) {
        Op op = key.OrDefault();
        return toSeq(parameters.ToArray())
            .TraverseM(parameter => op.AcceptText(value: parameter))
            .As()
            .Map(excluded => new HashProbe(axes: axes, excludedParameters: excluded.Distinct()));
    }

    internal Fin<HashWitness> Read(RenderContent content, Option<LinearWorkflow> workflow, Op key) {
        HashProbe self = this;
        return key.Catch(() => Fin.Succ(value: new HashWitness(
            Axes: self.Axes,
            Excluded: self.ExcludedParameters,
            Scope: HashScope.Of(documented: workflow.IsSome),
            Value: (workflow.Case, self.Axes.Held.Count, self.ExcludedParameters.IsEmpty) switch {
                (LinearWorkflow live, _, _) => content.RenderHashExclude(
                    HashAxis.Flags(axes: self.Axes), string.Join(separator: ';', values: self.ExcludedParameters), live),
                (_, 0, true) => content.RenderHash,
                _ => content.RenderHashExclude(
                    flags: HashAxis.Flags(axes: self.Axes),
                    excludeParameterNames: string.Join(separator: ';', values: self.ExcludedParameters)),
            })));
    }
}

public sealed record ContentSnapshot(
    Guid Key,
    Guid TypeId,
    Guid GroupId,
    ContentKind Kind,
    string Name,
    string DisplayName,
    string TypeName,
    string TypeDescription,
    Option<string> Notes,
    Option<string> Tags,
    Option<string> Category,
    CapabilitySet<ContentStyle> Styles,
    ProxyKind Proxy,
    ModelUnit Units,
    CapabilitySet<ContentTrait> Traits,
    Option<uint> DocumentOwner,
    Option<uint> DocumentAssociation,
    Option<Guid> Parent,
    Option<string> SlotInParent,
    Seq<SlotState> Slots,
    int UseCount) : IDetachedDocumentResult {
    public static Fin<ContentSnapshot> Of(RenderContent content, Op key) =>
        key.Need(content).Bind(active => key.Catch(() =>
            from kind in ContentKind.Of(content: active, key: key)
            from styles in ContentStyle.Of(native: active.Styles, key: key)
            from proxy in ProxyKind.Of(native: active.ProxyType, key: key)
            from units in ModelUnit.Of(value: active.ModelUnits, key: key)
            select new ContentSnapshot(
                Key: active.Id,
                TypeId: active.TypeId,
                GroupId: active.GroupId,
                Kind: kind,
                Name: active.Name,
                DisplayName: active.DisplayName,
                TypeName: active.TypeName,
                TypeDescription: active.TypeDescription,
                Notes: Op.Text(active.Notes),
                Tags: Op.Text(active.Tags),
                Category: Op.Text(active.Category),
                Styles: styles,
                Proxy: proxy,
                Units: units,
                Traits: ContentTrait.Of(content: active),
                DocumentOwner: Optional(active.DocumentOwner).Map(static document => document.RuntimeSerialNumber),
                DocumentAssociation: Optional(active.DocumentAssoc).Map(static document => document.RuntimeSerialNumber),
                Parent: Optional(active.Parent).Map(static parent => parent.Id),
                SlotInParent: Op.Text(active.ChildSlotName),
                Slots: SlotsOf(parent: active),
                UseCount: active.UseCount())));

    private static Seq<SlotState> SlotsOf(RenderContent parent) =>
        toSeq(LanguageExt.List.unfold(
                Optional(parent.FirstChild),
                static cursor => cursor.Map(child => (child, Optional(child.NextSibling)))))
            .Map(child => new SlotState(
                Name: child.ChildSlotName,
                DisplayName: child.ChildSlotDisplayName,
                Child: child.Id,
                On: parent.ChildSlotOn(child.ChildSlotName),
                Amount: parent.ChildSlotAmount(child.ChildSlotName)))
            .Strict();
}
```

## [05]-[INGRESS]

- Owner: `ContentIo` `[Union]` admits serialized XML or an archive path once; `Mint` returns the resulting unattached native as an owned `Lease<RenderContent>`.
- Law: `Lease<RenderContent>` disposes every untransferred mint; successful table attachment transfers custody to the document.
- Law: XML and archive cases preserve the host's two serialized ingress routes; XML/file egress and embedded-file evidence belong to registry programs because those operations start from addressed live content.
- Boundary: factory-registry minting (`RenderContent.Create` by type id) is the registry page's; this union owns only the serialized-form ingress.
- Packages: `api-rhinocommon-rendercontent.md` (`RenderContent.FromXml`, `RenderContent.LoadFromFile`); kernel `Domain/results` (`Lease<T>.Owned`, `Op.AcceptText`, `Op.OrDefault`); Thinktecture.Runtime.Extensions (`[Union]`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentIo {
    private ContentIo() { }
    private sealed record XmlCase(string Value) : ContentIo;
    private sealed record ArchiveCase(string Path) : ContentIo;

    public static Fin<ContentIo> Xml(string value, Op? key = null) =>
        key.OrDefault().AcceptText(value: value).Map(static admitted => (ContentIo)new XmlCase(Value: admitted));

    public static Fin<ContentIo> Archive(string path, Op? key = null) =>
        key.OrDefault().AcceptText(value: path).Map(static admitted => (ContentIo)new ArchiveCase(Path: admitted));

    internal Fin<Lease<RenderContent>> Mint(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            xmlCase: static (ctx, source) =>
                Bridge.Minted(mint: () => RenderContent.FromXml(xml: source.Value, doc: ctx.Document), key: ctx.Op),
            archiveCase: static (ctx, source) =>
                Bridge.Minted(mint: () => RenderContent.LoadFromFile(filename: source.Path), key: ctx.Op));
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]          | [OWNER]           | [FORM]                                                    | [ENTRY]                       |
| :-----: | :----------------- | :---------------- | :-------------------------------------------------------- | :---------------------------- |
|  [01]   | kind axis          | `ContentKind`     | rows whose key is native, table behavior as columns       | `Of` / `Attach` / `Table`     |
|  [02]   | change vocabulary  | `ChangeReason`    | rows carrying the native `ChangeContexts` value           | `Of(native, key)` / `Native`  |
|  [03]   | style capabilities | `ContentStyle`    | `ICapability` rows carrying the native mask bit           | `Of(native, key)`             |
|  [04]   | write bracket      | `ChangeScope`     | host window on `Lease` custody, release aggregated        | `Write(content, reason, ...)` |
|  [05]   | shared projections | `Bridge`          | column row read and null-answering mint custody           | `Row` / `Minted`              |
|  [06]   | content address    | `ContentRef`      | one union: id, slot path                                  | `Of` / `Resolve`              |
|  [07]   | content state      | `ContentTrait`    | `ICapability` rows over the host state predicates         | `Of(content)`                 |
|  [08]   | content snapshot   | `ContentSnapshot` | one-pass identity and topology read                       | `Of(content, key)`            |
|  [09]   | hash exclusions    | `HashAxis`        | atomic host bits; alias and composite derive as sets      | `Flags(axes)`                 |
|  [10]   | render-hash read   | `HashProbe`       | admitted exclusions answering a self-minted `HashWitness` | `Excluding` / `Read`          |
|  [11]   | serialized ingress | `ContentIo`       | admitted XML/file mint leased until custody transfer      | `Xml` / `Archive` / `Mint`    |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
