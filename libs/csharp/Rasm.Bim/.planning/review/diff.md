# [BIM_MODEL_DIFF]

The GlobalId-stable federation diff: one `ModelDiff` change-set folding two `BimModel` snapshots into added/modified/removed/moved arms, joining by `Model/elements#ELEMENT_MODEL` `GlobalId` plus the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` content-key so an unchanged element dedups by content-key and re-checks only changed elements. The diff consumes two `BimModel` snapshots as settled vocabulary and mints no second element shape — coordination turns a single semantic model into a multi-party federation, the diff is incremental, and the join reuses the Compute content-key rather than a second identity scheme. The page composes the `Model/elements#ELEMENT_MODEL` `BimModel` and the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` content-key as settled vocabulary. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[MODEL_DIFF]: `ModelDiff` change-set, the `ElementChange` closed arm family, and the GlobalId-plus-content-key join.
- [02]-[TS_PROJECTION]: the `DiffWire` host-free JSON producer the TS UI live-binding decodes — the GlobalId-keyed `ElementChange` change-set over the source-generated `BimWireContext` with the per-leaf `[JsonDerivedType]` union discriminant.
- [03]-[AUDIT]: the chained content-keyed `AuditEntry` log folding `ModelDiff` change-sets across a version sequence into a tamper-evident per-element mutation trail, and the `AuditTrail.For(GlobalId)` lifecycle query.

## [02]-[MODEL_DIFF]

- Owner: `ModelDiff` the change-set record carrying the added/modified/removed/moved element-change arms between two `BimModel` snapshots; `ElementChange` `[Union]` the closed change family (Added, Removed, Modified, Moved) each carrying its GlobalId and the changed evidence; `ElementFingerprint` the per-element content-key value the join dedups on.
- Entry: `ModelDiff.Between(BimModel baseline, BimModel revision)` folds the two snapshots into one `ModelDiff` — a GlobalId present in the revision but not the baseline is `Added`, present in the baseline but not the revision is `Removed`, present in both with a differing content-key is `Modified` (or `Moved` when only the placement differs), and present in both with an identical content-key is dedup'd as unchanged; the fold is total, pure, no rail — a diff is one expression over the two element collections, never an imperative accumulation loop.
- Auto: `Between` builds a content-keyed fingerprint per element through the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` content-key over the element's class, properties, quantities, materials, classifications, and geometry-handle key, then joins the baseline and revision fingerprint maps by GlobalId — an unchanged element (matching content-key) never enters the change set so a federation re-checks only the changed elements; a `Modified` arm carries the per-field changed evidence (the property/quantity/classification deltas) and a `Moved` arm carries the placement delta when the geometry-handle key changed but every other field matched, so a pure relocation reads as `Moved` rather than `Modified`.
- Receipt: the `ModelDiff` change-set is the incremental federation evidence; a BCF `Review/issues#BCF_ARCHIVE` topic anchors on the `Modified`/`Moved` element GlobalIds, and the `Exchange/wire#WIRE_PROJECTION` wire carries the diff as one content-keyed payload, never a second diff shape.
- Packages: NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, System.IO.Hashing, Rasm
- Growth: a new change kind is one `ElementChange` union arm; a new fingerprint field is one column folded into the content-key; the diff joins by GlobalId plus content-key so a new identity dimension is a content-key field, never a second identity scheme; never a per-change-kind type.
- Boundary: the diff joins by `Model/elements#ELEMENT_MODEL` `GlobalId` plus the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` content-key and a second identity scheme is the rejected form — the Compute content-key dedups unchanged elements so the federation re-checks only changed elements; the change family is a closed `ElementChange` union and a per-change-kind class is the deleted form; the fold consumes two `BimModel` snapshots as settled vocabulary and mints no second element shape; the diff is a fold over the two element collections, never an imperative filter loop with mutable accumulation; a `Moved` arm is distinguished from `Modified` by the geometry-handle-key delta so a pure relocation reads correctly; the content-key derivation is owned at `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` and consumed here, Bim mints no second identity scheme; a dangling GlobalId a `Modified` arm names but the revision graph never declares routes `Model/faults#FAULT_BAND` `BimFault.DanglingReference`.

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

    public static ElementFingerprint Fingerprint(BimElement element) =>
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

## [03]-[AUDIT]

- Owner: `AuditEntry` the immutable mutation-log row carrying the element `GlobalId`, the `ElementChange` kind, the baseline/revision content-key pair, the author, the `Instant`, the version pointer, and the chained `EntryKey` content-key keyed on the prior entry's key so a retroactive edit breaks the chain; `AuditTrail` the append-only log folding the per-version `ModelDiff` change-sets into the chained entry sequence, queryable by element `GlobalId`; the trail is a model-mutation log (who/when/what-changed-semantically), explicitly distinct from the geometry-asset XMP lineage (who-minted-this-GLB).
- Entry: `AuditTrail.Fold(Seq<(AuditVersion Version, ModelDiff Diff)> history)` folds a version sequence of `ModelDiff` change-sets into the chained `AuditTrail` — each `ElementChange` arm in each version's diff projects onto one `AuditEntry` (the `Added`/`Removed`/`Modified`/`Moved` kind, the version's baseline/revision content-keys, the version's author and `Instant`), and the `EntryKey` chains on the prior entry's key through `XxHash128.HashToUInt128(priorKey ++ entryContent)` so the log is tamper-evident — re-folding a tampered history yields a divergent terminal `EntryKey`; the fold is total, pure, no rail — the audit trail is one expression over the version-and-diff sequence, never an imperative append loop. `AuditTrail.For(string globalId)` folds every entry an element underwent into its lifecycle history (its full mutation sequence in chain order), and `AuditTrail.Verify()` re-derives the chain to witness no retroactive edit broke it (the terminal `EntryKey` matching the recomputed chain).
- Auto: `Fold` threads the `(priorKey, entries)` accumulator across the version sequence — for each version it folds the version's `ModelDiff.Changes` onto `AuditEntry` rows in change order (each carrying the element `GlobalId` the change names, the version content-keys, the version author/`Instant`, and the `parent` prior `EntryKey`), computes each entry's `EntryKey` as the hash of the prior key concatenated with the entry's content bytes (`GlobalId|kind|baselineKey|revisionKey|author|instant`), and threads the new key as the next entry's parent so the chain is a content-addressed Merkle-like sequence; `For(globalId)` filters the folded entries to the element and preserves chain order so the element's lifecycle reads add→modify→move→remove; `Verify` re-folds the entry contents recomputing each `EntryKey` from the recorded parent and compares the recomputed terminal against the stored terminal — a single retroactive field edit diverges every downstream key, so the boolean witnesses chain integrity without storing a separate checksum.
- Receipt: the `AuditTrail` chained `Seq<AuditEntry>` is the compliance evidence (who/when/from-what per element) the federation and compliance consumer read, the `AuditTrail.For(globalId)` the per-element lifecycle history anchoring a BCF topic and the `Versioned` merge, and `AuditTrail.Verify()` the tamper-evidence witness; the durable append-only store is the `csharp:Rasm.Persistence/Query/federation#FEDERATION` concern joined at the `Review/diff → csharp:Rasm.Persistence/Query/federation # [CONTENT_KEY]: AuditEntry chained ElementChange mutation log` seam by the content-key, this owner producing the chained log and its content-key identity.
- Packages: NodaTime, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, Rasm
- Growth: a new audit field is one column on `AuditEntry` folded into the `EntryKey` content; a new version-metadata dimension is one column on `AuditVersion`; a new lifecycle query is one fold over the same chained entry sequence; never a per-change-kind audit record, never a second mutation store, and never a checksum beside the chained `EntryKey`.
- Boundary: the audit trail is a model-mutation log keying on the `[2]-[MODEL_DIFF]` `ElementChange` and the version lineage, explicitly distinct from the Wave A tile-XMP geometry-asset provenance — the two stay separate, the audit trail never keying on the export artifact content-key; the chain is the content-addressed `EntryKey` sequence through the `XxHash128.HashToUInt128` idiom so a retroactive edit breaks the chain and `Verify` witnesses it, a separate stored checksum or a mutable sequence-number is the deleted form; the fold consumes the `[2]-[MODEL_DIFF]` `ModelDiff` change-sets as settled vocabulary and mints no second diff or element shape; the trail is a pure fold over the version-and-diff sequence, never an imperative append loop with mutable accumulation; the durable append-only residence is the `csharp:Rasm.Persistence/Query/federation#FEDERATION` concern joined at the content-key seam and a durable store minted here is the named seam violation; a `Modified`/`Moved` arm naming a `GlobalId` the version graph never declares lowers `Model/faults#FAULT_BAND` `BimFault.DanglingReference`.

```csharp signature
public sealed record AuditVersion(string VersionId, string Author, Instant At);

public readonly record struct AuditEntry(
    string GlobalId,
    string ChangeKind,
    UInt128 BaselineKey,
    UInt128 RevisionKey,
    string Author,
    Instant At,
    UInt128 ParentKey,
    UInt128 EntryKey);

public sealed record AuditTrail(Seq<AuditEntry> Entries) {
    public static readonly AuditTrail Empty = new(Seq<AuditEntry>());

    public static AuditTrail Fold(Seq<(AuditVersion Version, ModelDiff Diff)> history) =>
        new(history.Fold((Prior: UInt128.Zero, Rows: Seq<AuditEntry>()), static (state, step) =>
                step.Diff.Changes.Fold(state, (acc, change) => {
                    var entry = Chain(acc.Prior, change, step.Version);
                    return (entry.EntryKey, acc.Rows.Add(entry));
                })).Rows);

    // The version author/instant ride the closure; ParentKey chains on the prior EntryKey so a tampered
    // row diverges every downstream key. Content = GlobalId|kind|baseline|revision|author|instant.
    static AuditEntry Chain(UInt128 prior, ElementChange change, AuditVersion version) {
        var (globalId, kind, baseline, revision) = Decompose(change);
        var content = $"{globalId}|{kind}|{baseline}|{revision}|{version.Author}|{version.At.ToUnixTimeTicks()}";
        var key = XxHash128.HashToUInt128(Encoding.UTF8.GetBytes($"{prior}:{content}"));
        return new AuditEntry(globalId, kind, baseline, revision, version.Author, version.At, prior, key);
    }

    static (string GlobalId, string Kind, UInt128 Baseline, UInt128 Revision) Decompose(ElementChange change) => change.Switch(
        added:    static c => (c.GlobalId, "added", UInt128.Zero, UInt128.Zero),
        removed:  static c => (c.GlobalId, "removed", UInt128.Zero, UInt128.Zero),
        modified: static c => (c.GlobalId, "modified", c.BaselineKey, c.RevisionKey),
        moved:    static c => (c.GlobalId, "moved", c.BaselinePlacement, c.RevisionPlacement));

    public Seq<AuditEntry> For(string globalId) => Entries.Filter(e => e.GlobalId == globalId);

    public bool Verify() =>
        Entries.Fold((Prior: UInt128.Zero, Ok: true), static (state, entry) => {
            var content = $"{entry.GlobalId}|{entry.ChangeKind}|{entry.BaselineKey}|{entry.RevisionKey}|{entry.Author}|{entry.At.ToUnixTimeTicks()}";
            var recomputed = XxHash128.HashToUInt128(Encoding.UTF8.GetBytes($"{state.Prior}:{content}"));
            return (entry.EntryKey, state.Ok && recomputed == entry.EntryKey && entry.ParentKey == state.Prior);
        }).Ok;
}
```

## [04]-[TS_PROJECTION]

- Owner: `DiffWire` the host-free JSON wire producer of the `[2]-[MODEL_DIFF]` change-set — `DiffWire.Changes` the projected `Seq<ElementChange>` payload the `ts:ui/bcf-anchor` live-binding decodes to highlight the added/removed/modified/moved elements between two federated `BimModel` snapshots, plus the `UnchangedCount` dedup evidence; `DiffWire.Encode`/`Decode` the `Exchange/wire#WIRE_PROJECTION` `BimWireOptions.Json`-bound codec so the diff payload rides the same source-generated `BimWireContext` and `ThinktectureJsonConverterFactory` machinery the model snapshot and the BCF topics ride, never a second serializer.
- Entry: `DiffWire.Encode(ModelDiff diff)` projects the change-set onto the wire payload and `DiffWire.Decode(ReadOnlyMemory<byte> json)` admits it back — `Fin<T>` aborts on a malformed payload or a `Modified`/`Moved` arm naming a dangling GlobalId (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`/`BimFault.DanglingReference`) lowered with `.ToError()` at the `Boundary` funnel; the `ElementChange` `[Union]` serializes by the per-leaf `[JsonDerivedType]` discriminant the closed-union projection carries so a TS decode switches on the case name (`added`/`removed`/`modified`/`moved`) rather than a positional case index, and the `UInt128` baseline/revision content-keys and placement-keys serialize as the `Model/elements#ELEMENT_MODEL` GlobalId-anchored change evidence the live-binding re-checks only on a changed element.
- Auto: `Encode` serializes through `BimWireOptions.Json` — each `ElementChange` arm carries its case discriminant through the generated `[JsonDerivedType]` polymorphic projection (`Added`/`Removed` carry the GlobalId and the `IfcClass` string key through the Thinktecture smart-enum converter; `Modified` carries the baseline/revision content-keys; `Moved` carries the baseline/revision placement-keys), so a federation client decodes exactly which elements changed and re-fetches only the changed `BimWire` element rows; `Decode` re-admits the `IfcClass` key through the converter's static `Validate` so a malformed entity-class key faults at admission rather than minting a half-built arm.
- Receipt: the `DiffWire.Changes` payload is the one incremental-federation cross-runtime contract — the `ts:ui/bcf-anchor` live-binding decodes the same `ElementChange` vocabulary the C# branch mints and highlights the changed GlobalIds against the `Exchange/wire#WIRE_PROJECTION` `BimWire` element rows it already holds, never re-minting a parallel diff shape; a BCF `Review/issues#TS_PROJECTION` topic anchors on the `Modified`/`Moved` element GlobalIds this diff names so the issue panel and the change highlight carry one element identity.
- Packages: Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions, NodaTime, LanguageExt.Core, System.IO.Hashing, Rasm, BCL `System.Text.Json`
- Growth: a new change kind on the wire is one `ElementChange` `[Union]` arm carrying its `[JsonDerivedType]` discriminant and one `[JsonSerializable]` row on the `Exchange/wire#WIRE_PROJECTION` `BimWireContext`; the TS live-binding decodes the new case with no second wire vocabulary; never a per-change-kind wire record and never a second serializer beside `BimWireOptions.Json`.
- Boundary: `DiffWire` is HOST-FREE — it carries no RhinoCommon type and no host-bound geometry, only the GlobalId-keyed change evidence and the `UInt128` content/placement keys; it rides the `Exchange/wire#WIRE_PROJECTION` `BimWireOptions.Json` and the source-generated `BimWireContext`, and a second `JsonSerializerOptions` or a hand-authored DTO mirror is the deleted form — the `ElementChange` arms are wire-capable only through the per-leaf `[JsonDerivedType]` discriminator the closed `[Union]` carries, never a positional case index; the change-set joins by `Model/elements#ELEMENT_MODEL` `GlobalId` and the diff is owned at `[2]-[MODEL_DIFF]` and projected here, Bim mints no second diff shape; the `BimWire` model snapshot and this diff carry one element-GlobalId identity so the live-binding highlights the diff against the snapshot it already holds — a TS-side parallel diff shape is the named cross-language drift defect.

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

## [05]-[RESEARCH]

- [CONTENT_KEY_JOIN]: the `csharp:Compute/Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` content-key derivation owned at Compute and consumed here for the `ElementFingerprint.ContentKey` so the diff dedups unchanged elements by the same content-key the export artifact addresses — Bim mints no second identity scheme; the public signature confirms against the Compute `InterchangeIdentity` owner at cross-folder alignment, and the `XxHash128.HashToUInt128` placement-key over the geometry-handle key is BCL `System.IO.Hashing` inbox and settled.
- [GEOMETRY_HANDLE_KEY]: the `Model/elements#ELEMENT_MODEL` `GeometryHandle.Key` member the `Fingerprint` placement-key reads confirms against the `[GEOMETRY_HANDLE]` kernel-geometry binding at cross-folder alignment so a pure relocation reads as `Moved` by the geometry-handle-key delta rather than `Modified`; the handle key is the by-reference kernel-geometry identity the tessellation bridge re-imports, never a host-bound geometry type.
