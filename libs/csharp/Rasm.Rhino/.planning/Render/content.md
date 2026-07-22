# [RASM_RHINO_RENDER_CONTENT]

`ContentRef` owns live RDK graph identity across resolution, table routing, scoped mutation, detached topology, replayable hash evidence, and serialized ingress. `ContentKind`, `ContentStyle`, `ProxyKind`, `ChangeReason`, and `HashAxis` translate native discriminants once; every live `RenderContent` remains demand-window bound, and every unattached mint remains an owned `Lease<RenderContent>`.

## [01]-[INDEX]

- [02]-[KIND_AND_REASON]: `ContentKind` — the kind axis with table behavior columns; `ChangeReason` — the change-context vocabulary; `ChangeScope` — the write bracket.
- [03]-[ADDRESS]: `ContentRef` — the one content address union and its resolution fold.
- [04]-[SNAPSHOT_AND_HASH]: `SlotState`, `ContentSnapshot`, and the `HashProbe` render-hash read.
- [05]-[INGRESS]: `ContentIo` — leased XML and file mints.
- [06]-[SURFACE_LEDGER]: page owner table.

## [02]-[KIND_AND_REASON]

- Owner: `ContentKind` rows own kind-specific table behavior, `ContentStyle` validates and decomposes the complete native capability mask, `ProxyKind` classifies proxy topology, `ChangeReason` translates change context, and `ChangeScope` closes every direct mutation bracket. Sibling mutation rails commit through the Document spine's `DocumentCommit.Sealed` envelope (which owns the suppress/restore/flush bracket), and `Seam` carries the folder-wide projections — `Row` (native discriminant onto its vocabulary row), `Leased` (unattached mint into owned custody), `Quantized` (`PerceptualColor` onto the host byte color); a generated multi-member `Validate` outcome lifts through the kernel `Op.AcceptValidated` outcome rows, never a folder-local lifter.
- Law: kind is derived, never asked — each `ContentKind` row carries its runtime carrier type, `ContentKind.Of(RenderContent, Op)` derives from `Items`, and `ContentKind.Of(RenderContentKind, Op)` admits the native discriminant. Null ingress is invalid input; an unmatched live subtype is an invalid host result.
- Law: every direct field, parameter, parameter-binding, texture, rename, or child-slot write rides `ChangeScope.Write` with a named `ChangeReason`; host-owned table, assignment, replacement, grouping, and export verbs retain their own change semantics.
- Law: `ContentKind` columns are the only site naming `RenderMaterials`/`RenderEnvironments`/`RenderTextures`; every content operation reaches a table through its kind row.
- Growth: a new change context is one `ChangeReason` row; a new content kind is one `ContentKind` row whose columns close its table behavior.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ContentKind {
    public static readonly ContentKind Material = new(
        key: (int)RenderContentKind.Material,
        carrier: typeof(RenderMaterial),
        attach: static (document, content) => content is RenderMaterial value && document.RenderMaterials.Add(value),
        detach: static (document, content) => content is RenderMaterial value && document.RenderMaterials.Remove(value),
        open: static (document, reason) => document.RenderMaterials.BeginChange(reason),
        close: static document => document.RenderMaterials.EndChange(),
        roster: static document => toSeq(document.RenderMaterials).Map(static content => (RenderContent)content));
    public static readonly ContentKind Environment = new(
        key: (int)RenderContentKind.Environment,
        carrier: typeof(RenderEnvironment),
        attach: static (document, content) => content is RenderEnvironment value && document.RenderEnvironments.Add(value),
        detach: static (document, content) => content is RenderEnvironment value && document.RenderEnvironments.Remove(value),
        open: static (document, reason) => document.RenderEnvironments.BeginChange(reason),
        close: static document => document.RenderEnvironments.EndChange(),
        roster: static document => toSeq(document.RenderEnvironments).Map(static content => (RenderContent)content));
    public static readonly ContentKind Texture = new(
        key: (int)RenderContentKind.Texture,
        carrier: typeof(RenderTexture),
        attach: static (document, content) => content is RenderTexture value && document.RenderTextures.Add(value),
        detach: static (document, content) => content is RenderTexture value && document.RenderTextures.Remove(value),
        open: static (document, reason) => document.RenderTextures.BeginChange(reason),
        close: static document => document.RenderTextures.EndChange(),
        roster: static document => toSeq(document.RenderTextures).Map(static content => (RenderContent)content));

    internal Type Carrier { get; }

    [UseDelegateFromConstructor]
    internal partial bool Attach(RhinoDoc document, RenderContent content);

    [UseDelegateFromConstructor]
    internal partial bool Detach(RhinoDoc document, RenderContent content);

    [UseDelegateFromConstructor]
    internal partial void Open(RhinoDoc document, RenderContent.ChangeContexts reason);

    [UseDelegateFromConstructor]
    internal partial void Close(RhinoDoc document);

    [UseDelegateFromConstructor]
    internal partial Seq<RenderContent> Roster(RhinoDoc document);

    public static Fin<ContentKind> Of(RenderContent? content, Op key) =>
        Optional(content).ToFin(Fail: key.InvalidInput()).Bind(active =>
            toSeq(Items)
                .Filter(row => row.Carrier.IsInstanceOfType(active))
                .Head
                .ToFin(Fail: key.InvalidResult(detail: active.GetType().Name)));

    internal static Fin<ContentKind> Of(RenderContentKind native, Op key) =>
        TryGet((int)native, out ContentKind? row)
            ? Fin.Succ(value: row!)
            : Fin.Fail<ContentKind>(error: key.InvalidResult(detail: native.ToString()));
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
        TryGet((int)native, out ChangeReason? row)
            ? Fin.Succ(value: row!)
            : Fin.Fail<ChangeReason>(error: key.InvalidResult(detail: native.ToString()));
}

[SmartEnum<int>]
public sealed partial class ContentStyle {
    public static readonly ContentStyle TextureSummary = new(key: (int)RenderContentStyles.TextureSummary);
    public static readonly ContentStyle QuickPreview = new(key: (int)RenderContentStyles.QuickPreview);
    public static readonly ContentStyle PreviewCache = new(key: (int)RenderContentStyles.PreviewCache);
    public static readonly ContentStyle ProgressivePreview = new(key: (int)RenderContentStyles.ProgressivePreview);
    public static readonly ContentStyle LocalTextureMapping = new(key: (int)RenderContentStyles.LocalTextureMapping);
    public static readonly ContentStyle GraphDisplay = new(key: (int)RenderContentStyles.GraphDisplay);
    public static readonly ContentStyle Adjustment = new(key: (int)RenderContentStyles.Adjustment);
    public static readonly ContentStyle Fields = new(key: (int)RenderContentStyles.Fields);
    public static readonly ContentStyle ModalEditing = new(key: (int)RenderContentStyles.ModalEditing);
    public static readonly ContentStyle DynamicFields = new(key: (int)RenderContentStyles.DynamicFields);
    public static readonly ContentStyle NameTypeSection = new(key: (int)RenderContentStyles.NameTypeSection);

    internal RenderContentStyles Native => (RenderContentStyles)Key;

    internal static Fin<Seq<ContentStyle>> Of(RenderContentStyles native, Op key) {
        int mask = toSeq(Items).Fold(0, static (known, row) => known | row.Key);
        int value = (int)native;
        return (value & ~mask) == 0
            ? Fin.Succ(value: toSeq(Items).Filter(row => (value & row.Key) == row.Key))
            : Fin.Fail<Seq<ContentStyle>>(error: key.InvalidResult(detail: $"unknown RenderContentStyles bits: 0x{value & ~mask:X}"));
    }
}

[SmartEnum<int>]
public sealed partial class ProxyKind {
    public static readonly ProxyKind None = new(key: (int)ProxyTypes.None);
    public static readonly ProxyKind Single = new(key: (int)ProxyTypes.Single);
    public static readonly ProxyKind Multi = new(key: (int)ProxyTypes.Multi);
    public static readonly ProxyKind Texture = new(key: (int)ProxyTypes.Texture);

    internal static Fin<ProxyKind> Of(ProxyTypes native, Op key) =>
        TryGet((int)native, out ProxyKind? proxy)
            ? Fin.Succ(value: proxy)
            : Fin.Fail<ProxyKind>(error: key.InvalidResult(detail: native.ToString()));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static class ChangeScope {
    internal static Fin<TOut> Write<TOut>(RenderContent content, ChangeReason reason, Func<RenderContent, Fin<TOut>> body, Op key) =>
        key.Catch(() => {
            content.BeginChange(reason.Native);
            try {
                return body(content);
            } finally {
                content.EndChange();
            }
        });
}

internal static class Seam {
    internal static Fin<TRow> Row<TRow, TNative>(this Op key, IEnumerable<TRow> rows, TNative native, Func<TRow, TNative> project)
        where TRow : class where TNative : notnull =>
        Optional(rows.FirstOrDefault(row => EqualityComparer<TNative>.Default.Equals(project(row), native)))
            .ToFin(Fail: key.InvalidResult(detail: native.ToString() ?? string.Empty));

    internal static Fin<Lease<RenderContent>> Leased(this Fin<RenderContent> minted) =>
        minted.Map(static value => (Lease<RenderContent>)new Lease<RenderContent>.Owned(Value: value));

    internal static System.Drawing.Color Quantized(this PerceptualColor color) =>
        color.ToRgb() switch {
            var (red, green, blue, alpha) => System.Drawing.Color.FromArgb(alpha, red, green, blue),
        };
}
```

## [03]-[ADDRESS]

- Owner: `ContentRef` `[Union]` — `ById` over the content instance guid, `AtSlot` over a root guid plus a child-slot-name path; one `Resolve` fold answers the live `RenderContent` and every arm treats missing content or a broken path as absent.
- Law: `ContentRef` is the package's content identity — receipts, event facts, and settings bindings carry the guid; a slot path addresses a child without a consumer walking `FirstChild`/`NextSibling`.
- Law: resolution reads live per call — the content graph mutates under UI edits, undo, and linked events, so no resolved handle is cached on a value; a consumer holding a `ContentRef` re-resolves at each use inside the owning operation.
- Boundary: `Resolve` is the only site naming `RenderContent.FromId` and `FindChild`; every sibling page addresses through this union.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentRef {
    private ContentRef() { }
    private sealed record ById(Guid Value) : ContentRef;
    private sealed record AtSlot(Guid Root, Seq<string> Path) : ContentRef;

    public static Fin<ContentRef> Of(Guid id) =>
        id != Guid.Empty
            ? Fin.Succ<ContentRef>(value: new ById(Value: id))
            : Fin.Fail<ContentRef>(error: Op.Of(name: nameof(ContentRef)).InvalidInput());

    public static Fin<ContentRef> Of(Guid root, params ReadOnlySpan<string> path) {
        Op op = Op.Of(name: nameof(ContentRef));
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

- Owner: `SlotState` carries one occupied child-slot fact. `ContentSnapshot` carries detached identity, metadata, ownership serials, native discriminants, state predicates, tree position, slot roster, and usage. `HashProbe` admits whole or exclusion-aware host hash reads; `HashWitness` detaches the read value beside the posture that produced it.
- Law: `RenderContentStyles` and `ProxyTypes` stop at `ContentSnapshot.Of`; the detached snapshot carries admitted `ContentStyle` rows and one `ProxyKind`, so downstream branches never decode host discriminants again. `LengthUnit` remains native value evidence.
- Law: `ContentSnapshot.Of` walks `FirstChild`/`NextSibling` once and reads `ChildSlotOn`/`ChildSlotAmount` during that visit.
- Law: `HashProbe.Whole` reads `RenderHash`; every other probe calls `RenderHashExclude`, whose parameter roster is joined with the host-documented semicolon delimiter. Content migration remains `MatchData` on the operation rail.
- Law: the `DocumentWorkflow` posture selects the `LinearWorkflow` hash overload, and the consuming read resolves the document's own workflow inside its demand window — a live `LinearWorkflow` never crosses into a probe value.
- Growth: a content fact is one `ContentSnapshot` field read; an exclusion posture enters through `HashProbe.Excluding`, a workflow correction through `with { DocumentWorkflow = true }`.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct SlotState(string Name, string DisplayName, Guid Child, bool On, double Amount);

public readonly record struct HashWitness(
    CrcRenderHashFlags Flags, Seq<string> Excluded, bool DocumentWorkflow, uint Value) : IDetachedDocumentResult;

public sealed record HashProbe {
    private HashProbe(CrcRenderHashFlags flags, Seq<string> excludedParameters) =>
        (Flags, ExcludedParameters) = (flags, excludedParameters);

    public static HashProbe Whole { get; } = new(CrcRenderHashFlags.Normal, Seq<string>());
    public static HashProbe ForSimulation { get; } = new(CrcRenderHashFlags.ForSimulation, Seq<string>());
    public static HashProbe DocumentFree { get; } = new(CrcRenderHashFlags.ExcludeDocumentEffects, Seq<string>());

    public CrcRenderHashFlags Flags { get; }
    public Seq<string> ExcludedParameters { get; }
    public bool DocumentWorkflow { get; init; }

    public static Fin<HashProbe> Excluding(CrcRenderHashFlags flags, params ReadOnlySpan<string> parameters) {
        Op op = Op.Of(name: nameof(HashProbe));
        return toSeq(parameters.ToArray())
            .TraverseM(parameter => op.AcceptText(value: parameter))
            .As()
            .Map(excluded => new HashProbe(flags: flags, excludedParameters: excluded.Distinct()));
    }

    internal Fin<uint> Read(RenderContent content, Op key) {
        HashProbe self = this;
        return key.Catch(() => Fin.Succ(value:
            self.Flags == CrcRenderHashFlags.Normal && self.ExcludedParameters.IsEmpty
                ? content.RenderHash
                : content.RenderHashExclude(
                    flags: self.Flags,
                    excludeParameterNames: string.Join(separator: ";", values: self.ExcludedParameters))));
    }

    internal Fin<uint> Read(RenderContent content, LinearWorkflow workflow, Op key) {
        HashProbe self = this;
        return key.Catch(() => Fin.Succ(value: content.RenderHashExclude(
            self.Flags, string.Join(separator: ";", values: self.ExcludedParameters), workflow)));
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
    Seq<ContentStyle> Styles,
    ProxyKind Proxy,
    LengthUnit Units,
    bool TopLevel,
    bool Hidden,
    bool Private,
    bool Locked,
    bool Editable,
    bool DefaultInstance,
    bool HiddenByAutoDelete,
    bool Reference,
    Option<uint> DocumentOwner,
    Option<uint> DocumentAssociation,
    Option<Guid> Parent,
    Option<string> SlotInParent,
    Seq<SlotState> Slots,
    int UseCount) : IDetachedDocumentResult {
    public static Fin<ContentSnapshot> Of(RenderContent content, Op key) =>
        Optional(content).ToFin(Fail: key.InvalidInput()).Bind(active => key.Catch(() =>
            from kind in ContentKind.Of(content: active, key: key)
            from styles in ContentStyle.Of(native: active.Styles, key: key)
            from proxy in ProxyKind.Of(native: active.ProxyType, key: key)
            select new ContentSnapshot(
                Key: active.Id,
                TypeId: active.TypeId,
                GroupId: active.GroupId,
                Kind: kind,
                Name: active.Name,
                DisplayName: active.DisplayName,
                TypeName: active.TypeName,
                TypeDescription: active.TypeDescription,
                Notes: Optional(active.Notes).Filter(static text => text.Length > 0),
                Tags: Optional(active.Tags).Filter(static text => text.Length > 0),
                Category: Optional(active.Category).Filter(static text => text.Length > 0),
                Styles: styles,
                Proxy: proxy,
                Units: active.ModelUnits,
                TopLevel: active.TopLevel,
                Hidden: active.Hidden,
                Private: active.Private,
                Locked: active.IsLocked,
                Editable: active.CanBeEdited,
                DefaultInstance: active.IsDefaultInstance,
                HiddenByAutoDelete: active.IsHiddenByAutoDelete,
                Reference: active.IsReference(),
                DocumentOwner: Optional(active.DocumentOwner).Map(static document => document.RuntimeSerialNumber),
                DocumentAssociation: Optional(active.DocumentAssoc).Map(static document => document.RuntimeSerialNumber),
                Parent: Optional(active.Parent).Map(static parent => parent.Id),
                SlotInParent: Optional(active.ChildSlotName).Filter(static slot => slot.Length > 0),
                Slots: SlotsOf(parent: active),
                UseCount: active.UseCount())));

    private static Seq<SlotState> SlotsOf(RenderContent parent) {
        Seq<SlotState> Read(Option<RenderContent> cursor, Seq<SlotState> state) =>
            cursor.Case switch {
                RenderContent child => Read(
                    cursor: Optional(child.NextSibling),
                    state: state.Add(value: new SlotState(
                        Name: child.ChildSlotName,
                        DisplayName: child.ChildSlotDisplayName,
                        Child: child.Id,
                        On: parent.ChildSlotOn(child.ChildSlotName),
                        Amount: parent.ChildSlotAmount(child.ChildSlotName)))),
                _ => state,
            };
        return Read(cursor: Optional(parent.FirstChild), state: Seq<SlotState>());
    }
}
```

## [05]-[INGRESS]

- Owner: `ContentIo` `[Union]` admits serialized XML or an archive path once; `Mint` returns the resulting unattached native as an owned `Lease<RenderContent>`.
- Law: `Lease<RenderContent>` disposes every untransferred mint; successful table attachment transfers custody to the document.
- Law: XML and archive cases preserve the host's two serialized ingress routes; XML/file egress and embedded-file evidence belong to registry programs because those operations start from addressed live content.
- Boundary: factory-registry minting (`RenderContent.Create` by type id) is the registry page's; this union owns only the serialized-form ingress.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentIo {
    private ContentIo() { }
    private sealed record XmlCase(string Value) : ContentIo;
    private sealed record ArchiveCase(string Path) : ContentIo;

    public static Fin<ContentIo> Xml(string value) =>
        Op.Of(name: nameof(ContentIo)).AcceptText(value: value).Map(static admitted => (ContentIo)new XmlCase(Value: admitted));

    public static Fin<ContentIo> Archive(string path) =>
        Op.Of(name: nameof(ContentIo)).AcceptText(value: path).Map(static admitted => (ContentIo)new ArchiveCase(Path: admitted));

    internal Fin<Lease<RenderContent>> Mint(RhinoDoc document, Op key) =>
        Switch(
            state: (Document: document, Op: key),
            xmlCase: static (ctx, source) => ctx.Op.Catch(() =>
                Optional(RenderContent.FromXml(xml: source.Value, doc: ctx.Document)).ToFin(Fail: ctx.Op.InvalidResult()).Leased()),
            archiveCase: static (ctx, source) => ctx.Op.Catch(() =>
                Optional(RenderContent.LoadFromFile(filename: source.Path)).ToFin(Fail: ctx.Op.InvalidResult()).Leased()));
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]          | [OWNER]           | [FORM]                                                 | [ENTRY]                       |
| :-----: | :----------------- | :---------------- | :----------------------------------------------------- | :---------------------------- |
|  [01]   | kind axis          | `ContentKind`     | rows whose key is native, table behavior as columns    | `Of` / table columns          |
|  [02]   | change vocabulary  | `ChangeReason`    | rows carrying the native `ChangeContexts` value        | `Of(native, key)` / `Native`  |
|  [03]   | write bracket      | `ChangeScope`     | begin/body/end on every exit                           | `Write(content, reason, ...)` |
|  [04]   | commit envelope    | `DocumentCommit`  | Document-owned suppress/seal/restore/flush composition | `Sealed(document, ...)`       |
|  [05]   | shared projections | `Seam`            | row/lease/color folds onto the rail                    | `Row`/`Leased`/`Quantized`    |
|  [06]   | content address    | `ContentRef`      | one union: id, slot path                               | `Of` / `Resolve`              |
|  [07]   | content state      | `ContentSnapshot` | one-pass identity and topology read                    | `Of(content, key)`            |
|  [08]   | render-hash read   | `HashProbe`       | admitted exclusions, workflow posture, `HashWitness`   | `Excluding` / `Read`          |
|  [09]   | serialized ingress | `ContentIo`       | admitted XML/file mint leased until custody transfer   | `Xml` / `Archive` / `Mint`    |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
