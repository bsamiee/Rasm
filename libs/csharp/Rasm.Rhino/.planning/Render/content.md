# [RASM_RHINO_RENDER_CONTENT]

RDK content (`Rasm.Rhino.Render`) resolves one `ContentRef` address, routes all document-table behavior through `ContentKind`, brackets direct state writes through `ChangeScope`, snapshots detached identity and topology through `ContentSnapshot`, reads host-derived hashes through `HashProbe`, and mints serialized content through `ContentIo`. Native discriminants stop at this edge, live `RenderContent` values stay inside their demand window, and every unattached mint remains an owned `Lease<RenderContent>` until attachment transfers custody or disposal releases it.

## [01]-[INDEX]

- [02]-[KIND_AND_REASON]: `ContentKind` — the kind axis with table behavior columns; `ChangeReason` — the change-context vocabulary; `ChangeScope` — the write bracket.
- [03]-[ADDRESS]: `ContentRef` — the one content address union and its resolution fold.
- [04]-[SNAPSHOT_AND_HASH]: `SlotState`, `ContentSnapshot`, and the `HashProbe` render-hash read.
- [05]-[INGRESS]: `ContentIo` — leased XML and file mints.
- [06]-[SURFACE_LEDGER]: page owner table.

## [02]-[KIND_AND_REASON]

- Owner: `ContentKind` `[SmartEnum<int>]` — the material/environment/texture axis whose key IS the native `RenderContentKind` value and whose columns are each kind's document table behavior: attach, detach, find, change-scope open/close, and roster; the three `public sealed` host tables share the internal `IRenderContentTable<T>` shape, so one vocabulary row per kind replaces three parallel table wrappers. `ChangeReason` `[SmartEnum<int>]` — the host `RenderContent.ChangeContexts` roster as named rows carrying the native value; `Of` recovers the row from a delivered native context so event projection stays total. `ChangeScope` — the internal write bracket: `BeginChange(reason)` opens, the body runs trapped, `EndChange` closes on every exit.
- Law: kind is derived, never asked — `ContentKind.Of(RenderContent)` classifies by the subclass the instance already is, so no consumer re-tests `is RenderMaterial` beside the vocabulary, and a fourth content kind is one row with its table columns.
- Law: every direct field, parameter, texture, rename, or child-slot write rides `ChangeScope.Write` with a named `ChangeReason`; host-owned table, assignment, replacement, grouping, and export verbs retain their own change semantics.
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
        attach: static (document, content) => content is RenderMaterial value && document.RenderMaterials.Add(value),
        detach: static (document, content) => content is RenderMaterial value && document.RenderMaterials.Remove(value),
        find: static (document, id) => Optional((RenderContent)document.RenderMaterials.Find(id: id)),
        open: static (document, reason) => document.RenderMaterials.BeginChange(reason),
        close: static document => document.RenderMaterials.EndChange(),
        roster: static document => toSeq(document.RenderMaterials).Map(static content => (RenderContent)content));
    public static readonly ContentKind Environment = new(
        key: (int)RenderContentKind.Environment,
        attach: static (document, content) => content is RenderEnvironment value && document.RenderEnvironments.Add(value),
        detach: static (document, content) => content is RenderEnvironment value && document.RenderEnvironments.Remove(value),
        find: static (document, id) => Optional((RenderContent)document.RenderEnvironments.Find(id: id)),
        open: static (document, reason) => document.RenderEnvironments.BeginChange(reason),
        close: static document => document.RenderEnvironments.EndChange(),
        roster: static document => toSeq(document.RenderEnvironments).Map(static content => (RenderContent)content));
    public static readonly ContentKind Texture = new(
        key: (int)RenderContentKind.Texture,
        attach: static (document, content) => content is RenderTexture value && document.RenderTextures.Add(value),
        detach: static (document, content) => content is RenderTexture value && document.RenderTextures.Remove(value),
        find: static (document, id) => Optional((RenderContent)document.RenderTextures.Find(id: id)),
        open: static (document, reason) => document.RenderTextures.BeginChange(reason),
        close: static document => document.RenderTextures.EndChange(),
        roster: static document => toSeq(document.RenderTextures).Map(static content => (RenderContent)content));

    [UseDelegateFromConstructor]
    internal partial bool Attach(RhinoDoc document, RenderContent content);

    [UseDelegateFromConstructor]
    internal partial bool Detach(RhinoDoc document, RenderContent content);

    [UseDelegateFromConstructor]
    internal partial Option<RenderContent> Find(RhinoDoc document, Guid id);

    [UseDelegateFromConstructor]
    internal partial void Open(RhinoDoc document, RenderContent.ChangeContexts reason);

    [UseDelegateFromConstructor]
    internal partial void Close(RhinoDoc document);

    [UseDelegateFromConstructor]
    internal partial Seq<RenderContent> Roster(RhinoDoc document);

    public static Option<ContentKind> Of(RenderContent content) =>
        Optional(content).Bind(static value => value switch {
            RenderMaterial => Some(Material),
            RenderEnvironment => Some(Environment),
            RenderTexture => Some(Texture),
            _ => Option<ContentKind>.None,
        });
}

[SmartEnum<int>]
public sealed partial class ChangeReason {
    public static readonly ChangeReason Ui = new(key: 0, native: RenderContent.ChangeContexts.UI);
    public static readonly ChangeReason Drop = new(key: 1, native: RenderContent.ChangeContexts.Drop);
    public static readonly ChangeReason Program = new(key: 2, native: RenderContent.ChangeContexts.Program);
    public static readonly ChangeReason Ignore = new(key: 3, native: RenderContent.ChangeContexts.Ignore);
    public static readonly ChangeReason Tree = new(key: 4, native: RenderContent.ChangeContexts.Tree);
    public static readonly ChangeReason Undo = new(key: 5, native: RenderContent.ChangeContexts.Undo);
    public static readonly ChangeReason FieldInit = new(key: 6, native: RenderContent.ChangeContexts.FieldInit);
    public static readonly ChangeReason Serialize = new(key: 7, native: RenderContent.ChangeContexts.Serialize);
    public static readonly ChangeReason RealTimeUi = new(key: 8, native: RenderContent.ChangeContexts.RealTimeUI);
    public static readonly ChangeReason Script = new(key: 9, native: RenderContent.ChangeContexts.Script);

    internal RenderContent.ChangeContexts Native { get; }

    internal static Fin<ChangeReason> Of(RenderContent.ChangeContexts native, Op key) =>
        toSeq(Items).Find(row => row.Native == native).ToFin(key.InvalidResult(detail: native.ToString()));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static class ChangeScope {
    internal static Fin<TOut> Write<TOut>(RenderContent content, ChangeReason reason, Func<RenderContent, Fin<TOut>> body, Op key) =>
        key.Catch(() => {
            content.BeginChange(reason.Native);
            try {
                return key.Catch(() => body(content));
            } finally {
                content.EndChange();
            }
        });
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
        return from _ in guard(root != Guid.Empty, op.InvalidInput()).ToFin()
               from slots in toSeq(path.ToArray()).TraverseM(slot => op.AcceptText(value: slot)).As()
               from address in guard(!slots.IsEmpty, op.InvalidInput()).ToFin()
                   .Map(_ => (ContentRef)new AtSlot(Root: root, Path: slots))
               select address;
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

- Owner: `SlotState` carries one occupied child-slot fact. `ContentSnapshot` carries detached identity, metadata, ownership serials, native discriminants, state predicates, tree position, slot roster, and usage. `HashProbe` admits whole or exclusion-aware host hash reads, including the `LinearWorkflow` overload.
- Law: `RenderContentStyles`, `ProxyTypes`, and `LengthUnit` stop at `ContentSnapshot`; downstream branches consume named snapshot predicates instead of decoding host masks again.
- Law: `ContentSnapshot.Of` walks `FirstChild`/`NextSibling` once and reads `ChildSlotOn`/`ChildSlotAmount` during that visit.
- Law: `HashProbe.Whole` reads `RenderHash`; every other probe calls `RenderHashExclude`, whose parameter roster is joined with the host-documented semicolon delimiter. Content migration remains `MatchData` on the operation rail.
- Growth: a content fact is one `ContentSnapshot` field read; an exclusion posture enters through `HashProbe.Excluding`.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct SlotState(string Name, string DisplayName, Guid Child, bool On, double Amount);

public sealed record HashProbe {
    private HashProbe(CrcRenderHashFlags flags, Seq<string> excludedParameters) =>
        (Flags, ExcludedParameters) = (flags, excludedParameters);

    public static HashProbe Whole { get; } = new(CrcRenderHashFlags.Normal, Seq<string>());
    public static HashProbe ForSimulation { get; } = new(CrcRenderHashFlags.ForSimulation, Seq<string>());
    public static HashProbe DocumentFree { get; } = new(CrcRenderHashFlags.ExcludeDocumentEffects, Seq<string>());

    public CrcRenderHashFlags Flags { get; }
    public Seq<string> ExcludedParameters { get; }

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
    RenderContentStyles Styles,
    ProxyTypes Proxy,
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
            ContentKind.Of(active).ToFin(key.InvalidInput()).Map(kind => new ContentSnapshot(
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
                Styles: active.Styles,
                Proxy: active.ProxyType,
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
                UseCount: active.UseCount()))));

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
                Optional(RenderContent.FromXml(xml: source.Value, doc: ctx.Document))
                    .ToFin(Fail: ctx.Op.InvalidResult())
                    .Map(static minted => (Lease<RenderContent>)new Lease<RenderContent>.Owned(Value: minted))),
            archiveCase: static (ctx, source) => ctx.Op.Catch(() =>
                Optional(RenderContent.LoadFromFile(filename: source.Path))
                    .ToFin(Fail: ctx.Op.InvalidResult())
                    .Map(static minted => (Lease<RenderContent>)new Lease<RenderContent>.Owned(Value: minted))));
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]          | [OWNER]           | [FORM]                                               | [ENTRY]                       |
| :-----: | :----------------- | :---------------- | :--------------------------------------------------- | :---------------------------- |
|  [01]   | kind axis          | `ContentKind`     | rows whose key is native, table behavior as columns  | `Of` / table columns          |
|  [02]   | change vocabulary  | `ChangeReason`    | rows carrying the native `ChangeContexts` value      | `Of(native, key)` / `Native`  |
|  [03]   | write bracket      | `ChangeScope`     | begin/body/end on every exit                         | `Write(content, reason, ...)` |
|  [04]   | content address    | `ContentRef`      | one union: id, slot path                             | `Of` / `Resolve`              |
|  [05]   | content state      | `ContentSnapshot` | one-pass identity and topology read                  | `Of(content, key)`            |
|  [06]   | render-hash read   | `HashProbe`       | admitted exclusions and workflow-corrected overload  | `Excluding` / `Read`          |
|  [07]   | serialized ingress | `ContentIo`       | admitted XML/file mint leased until custody transfer | `Xml` / `Archive` / `Mint`    |
