# [BIM_MODEL_DIFF]

The GlobalId-stable federation diff: one `ModelDiff` change-set folding two `BimModel` snapshots into added/modified/removed/moved arms, joining by `model/elements#ELEMENT_MODEL` `GlobalId` plus the `csharp:Compute/interchange/codecs#CONTENT_ADDRESSING` content-key so an unchanged element dedups by content-key and re-checks only changed elements. The diff consumes two `BimModel` snapshots as settled vocabulary and mints no second element shape — coordination turns a single semantic model into a multi-party federation, the diff is incremental, and the join reuses the Compute content-key rather than a second identity scheme. The page composes the `model/elements#ELEMENT_MODEL` `BimModel` and the `csharp:Compute/interchange/codecs#CONTENT_ADDRESSING` content-key as settled vocabulary. The page is HOST-LOCAL.

## [1]-[INDEX]

- [2]-[MODEL_DIFF]: `ModelDiff` change-set, the `ElementChange` closed arm family, and the GlobalId-plus-content-key join.
- [3]-[TS_PROJECTION]: the `DiffWire` host-free JSON producer the TS UI live-binding decodes — the GlobalId-keyed `ElementChange` change-set over the source-generated `BimWireContext` with the per-leaf `[JsonDerivedType]` union discriminant.

## [2]-[MODEL_DIFF]

- Owner: `ModelDiff` the change-set record carrying the added/modified/removed/moved element-change arms between two `BimModel` snapshots; `ElementChange` `[Union]` the closed change family (Added, Removed, Modified, Moved) each carrying its GlobalId and the changed evidence; `ElementFingerprint` the per-element content-key value the join dedups on.
- Entry: `ModelDiff.Between(BimModel baseline, BimModel revision)` folds the two snapshots into one `ModelDiff` — a GlobalId present in the revision but not the baseline is `Added`, present in the baseline but not the revision is `Removed`, present in both with a differing content-key is `Modified` (or `Moved` when only the placement differs), and present in both with an identical content-key is dedup'd as unchanged; the fold is total, pure, no rail — a diff is one expression over the two element collections, never an imperative accumulation loop.
- Auto: `Between` builds a content-keyed fingerprint per element through the `csharp:Compute/interchange/codecs#CONTENT_ADDRESSING` content-key over the element's class, properties, quantities, materials, classifications, and geometry-handle key, then joins the baseline and revision fingerprint maps by GlobalId — an unchanged element (matching content-key) never enters the change set so a federation re-checks only the changed elements; a `Modified` arm carries the per-field changed evidence (the property/quantity/classification deltas) and a `Moved` arm carries the placement delta when the geometry-handle key changed but every other field matched, so a pure relocation reads as `Moved` rather than `Modified`.
- Receipt: the `ModelDiff` change-set is the incremental federation evidence; a BCF `coordination/issue-exchange#BCF_ARCHIVE` topic anchors on the `Modified`/`Moved` element GlobalIds, and the `exchange/wire#WIRE_PROJECTION` wire carries the diff as one content-keyed payload, never a second diff shape.
- Packages: NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, System.IO.Hashing, Rasm
- Growth: a new change kind is one `ElementChange` union arm; a new fingerprint field is one column folded into the content-key; the diff joins by GlobalId plus content-key so a new identity dimension is a content-key field, never a second identity scheme; never a per-change-kind type.
- Boundary: the diff joins by `model/elements#ELEMENT_MODEL` `GlobalId` plus the `csharp:Compute/interchange/codecs#CONTENT_ADDRESSING` content-key and a second identity scheme is the rejected form — the Compute content-key dedups unchanged elements so the federation re-checks only changed elements; the change family is a closed `ElementChange` union and a per-change-kind class is the deleted form; the fold consumes two `BimModel` snapshots as settled vocabulary and mints no second element shape; the diff is a fold over the two element collections, never an imperative filter loop with mutable accumulation; a `Moved` arm is distinguished from `Modified` by the geometry-handle-key delta so a pure relocation reads correctly; the content-key derivation is owned at `csharp:Compute/interchange/codecs#CONTENT_ADDRESSING` and consumed here, Bim mints no second identity scheme; a dangling GlobalId a `Modified` arm names but the revision graph never declares routes `faults#FAULT_BAND` `BimFault.DanglingReference`.

```csharp signature
public readonly record struct ElementFingerprint(string GlobalId, UInt128 ContentKey, UInt128 PlacementKey);

[Union]
public partial record ElementChange {
    partial record Added(string GlobalId, IfcClass Class);
    partial record Removed(string GlobalId, IfcClass Class);
    partial record Modified(string GlobalId, UInt128 BaselineKey, UInt128 RevisionKey);
    partial record Moved(string GlobalId, UInt128 BaselinePlacement, UInt128 RevisionPlacement);
}

public sealed record ModelDiff(Seq<ElementChange> Changes, int UnchangedCount) {
    public static ModelDiff Between(BimModel baseline, BimModel revision) {
        var baselineMap = baseline.Elements.Map(Fingerprint).ToMap(static f => f.GlobalId);
        var revisionMap = revision.Elements.Map(Fingerprint).ToMap(static f => f.GlobalId);
        var classMap = baseline.Elements.Append(revision.Elements).ToMap(static e => e.GlobalId, static e => e.Class);
        var added = revisionMap.Keys.Filter(id => !baselineMap.ContainsKey(id))
            .Map(id => (ElementChange)new ElementChange.Added(id, classMap[id]));
        var removed = baselineMap.Keys.Filter(id => !revisionMap.ContainsKey(id))
            .Map(id => (ElementChange)new ElementChange.Removed(id, classMap[id]));
        var common = baselineMap.Keys.Filter(revisionMap.ContainsKey).Map(id => (id, b: baselineMap[id], r: revisionMap[id]));
        var changed = common.Choose(static row =>
            row.b.ContentKey != row.r.ContentKey
                ? Some((ElementChange)new ElementChange.Modified(row.id, row.b.ContentKey, row.r.ContentKey))
            : row.b.PlacementKey != row.r.PlacementKey
                ? Some((ElementChange)new ElementChange.Moved(row.id, row.b.PlacementKey, row.r.PlacementKey))
                : Option<ElementChange>.None);
        int unchanged = common.Count(static row => row.b.ContentKey == row.r.ContentKey && row.b.PlacementKey == row.r.PlacementKey);
        return new ModelDiff(toSeq(added).Append(removed).Append(changed), unchanged);
    }

    static ElementFingerprint Fingerprint(BimElement element) =>
        new(element.GlobalId,
            InterchangeIdentity.Key(element.Class.Key, ContentBytes(element), 0.0, 0.0, 0.0),
            XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(element.Geometry.Key)));

    static byte[] ContentBytes(BimElement element) =>
        Encoding.UTF8.GetBytes(string.Join('|',
            element.Properties.Map(static p => $"{p.SetName}.{p.Name}={p.Value}")
                .Append(element.Quantities.Map(static q => $"{q.SetName}.{q.Name}={q.Value}"))
                .Append(element.Materials)
                .Append(element.Classifications.Map(static c => $"{c.System.Key}:{c.Code.Value}"))));
}
```

## [3]-[TS_PROJECTION]

- Owner: `DiffWire` the host-free JSON wire producer of the `[2]-[MODEL_DIFF]` change-set — `DiffWire.Changes` the projected `Seq<ElementChange>` payload the `ts:ui/bcf-anchor` live-binding decodes to highlight the added/removed/modified/moved elements between two federated `BimModel` snapshots, plus the `UnchangedCount` dedup evidence; `DiffWire.Encode`/`Decode` the `exchange/wire#WIRE_PROJECTION` `BimWireOptions.Json`-bound codec so the diff payload rides the same source-generated `BimWireContext` and `ThinktectureJsonConverterFactory` machinery the model snapshot and the BCF topics ride, never a second serializer.
- Entry: `DiffWire.Encode(ModelDiff diff)` projects the change-set onto the wire payload and `DiffWire.Decode(ReadOnlyMemory<byte> json)` admits it back — `Fin<T>` aborts on a malformed payload or a `Modified`/`Moved` arm naming a dangling GlobalId (`faults#FAULT_BAND` `BimFault.ModelRejected`/`BimFault.DanglingReference`) lowered with `.ToError()` at the `Boundary` funnel; the `ElementChange` `[Union]` serializes by the per-leaf `[JsonDerivedType]` discriminant the closed-union projection carries so a TS decode switches on the case name (`added`/`removed`/`modified`/`moved`) rather than a positional case index, and the `UInt128` baseline/revision content-keys and placement-keys serialize as the `model/elements#ELEMENT_MODEL` GlobalId-anchored change evidence the live-binding re-checks only on a changed element.
- Auto: `Encode` serializes through `BimWireOptions.Json` — each `ElementChange` arm carries its case discriminant through the generated `[JsonDerivedType]` polymorphic projection (`Added`/`Removed` carry the GlobalId and the `IfcClass` string key through the Thinktecture smart-enum converter; `Modified` carries the baseline/revision content-keys; `Moved` carries the baseline/revision placement-keys), so a federation client decodes exactly which elements changed and re-fetches only the changed `BimWire` element rows; `Decode` re-admits the `IfcClass` key through the converter's static `Validate` so a malformed entity-class key faults at admission rather than minting a half-built arm.
- Receipt: the `DiffWire.Changes` payload is the one incremental-federation cross-runtime contract — the `ts:ui/bcf-anchor` live-binding decodes the same `ElementChange` vocabulary the C# branch mints and highlights the changed GlobalIds against the `exchange/wire#WIRE_PROJECTION` `BimWire` element rows it already holds, never re-minting a parallel diff shape; a BCF `coordination/issue-exchange#TS_PROJECTION` topic anchors on the `Modified`/`Moved` element GlobalIds this diff names so the issue panel and the change highlight carry one element identity.
- Packages: Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, System.IO.Hashing, Rasm, BCL `System.Text.Json`
- Growth: a new change kind on the wire is one `ElementChange` `[Union]` arm carrying its `[JsonDerivedType]` discriminant and one `[JsonSerializable]` row on the `exchange/wire#WIRE_PROJECTION` `BimWireContext`; the TS live-binding decodes the new case with no second wire vocabulary; never a per-change-kind wire record and never a second serializer beside `BimWireOptions.Json`.
- Boundary: `DiffWire` is HOST-FREE — it carries no RhinoCommon type and no host-bound geometry, only the GlobalId-keyed change evidence and the `UInt128` content/placement keys; it rides the `exchange/wire#WIRE_PROJECTION` `BimWireOptions.Json` and the source-generated `BimWireContext`, and a second `JsonSerializerOptions` or a hand-authored DTO mirror is the deleted form — the `ElementChange` arms are wire-capable only through the per-leaf `[JsonDerivedType]` discriminator the closed `[Union]` carries, never a positional case index; the change-set joins by `model/elements#ELEMENT_MODEL` `GlobalId` and the diff is owned at `[2]-[MODEL_DIFF]` and projected here, Bim mints no second diff shape; the `BimWire` model snapshot and this diff carry one element-GlobalId identity so the live-binding highlights the diff against the snapshot it already holds — a TS-side parallel diff shape is the named cross-language drift defect.

```csharp signature
public sealed record DiffWire(Seq<ElementChange> Changes, int UnchangedCount) {
    public static Fin<byte[]> Encode(ModelDiff diff) =>
        Try.lift(() => JsonSerializer.SerializeToUtf8Bytes(new DiffWire(diff.Changes, diff.UnchangedCount), BimWireOptions.Json)).Run()
            .MapFail(static error => new BimFault.ModelRejected($"diff-wire-encode:{error.Message}").ToError());

    public static Fin<DiffWire> Decode(ReadOnlyMemory<byte> json) =>
        Try.lift(() => JsonSerializer.Deserialize<DiffWire>(json.Span, BimWireOptions.Json)!).Run()
            .MapFail(static error => new BimFault.ModelRejected($"diff-wire-decode:{error.Message}").ToError());
}
```

## [4]-[RESEARCH]

- [CONTENT_KEY_JOIN]: the `csharp:Compute/interchange/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` content-key derivation owned at Compute and consumed here for the `ElementFingerprint.ContentKey` so the diff dedups unchanged elements by the same content-key the export artifact addresses — Bim mints no second identity scheme; the public signature confirms against the Compute `InterchangeIdentity` owner at cross-folder alignment, and the `XxHash128.HashToUInt128` placement-key over the geometry-handle key is BCL `System.IO.Hashing` inbox and settled.
- [GEOMETRY_HANDLE_KEY]: the `model/elements#ELEMENT_MODEL` `GeometryHandle.Key` member the `Fingerprint` placement-key reads confirms against the `[GEOMETRY_HANDLE]` kernel-geometry binding at cross-folder alignment so a pure relocation reads as `Moved` by the geometry-handle-key delta rather than `Modified`; the handle key is the by-reference kernel-geometry identity the tessellation bridge re-imports, never a host-bound geometry type.
