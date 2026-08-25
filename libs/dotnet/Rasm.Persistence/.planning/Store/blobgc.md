# [PERSISTENCE_STORE_BLOBGC]

Rasm.Persistence reclaims object bytes through the write-blob-first protocol and the ONE full-history reachability sweep, never a blob-lane-local deletion executor. `BlobCatalogRow` is the content-lineage retention row every blob carries — the same row the snapshot spine has — keyed on the `ArtifactKind` its retention class DERIVES from and carrying the WORM window, the residence form, and the wrapped DEK beside the tenant, extent, tier, lineage, and classification columns. `PendingWrite` is the write-first ledger whose open rows ARE the in-flight fence, so a crash before the catalog commit leaves a collectible orphan rather than a dangling reference. `LifecycleRules` arms the PROVIDER-side half — expiries and cold-tier rungs projected from declared schedule values onto per-class key prefixes, unlocked by the class-leading object name. `BlobGc` owns the write protocol, the in-flight fence it contributes to the sweep's eligibility predicate, the set-shaped WORM-aware evict arrow, and the metadata-only demote, and it owns no second sweeper.

`RetentionClass`, `RetentionFact`, `SweepVerdict`, `SweepTally`, `Hold`, `Reachability`, and `RetentionSweep` compose from `Version/retention`; object placement composes `ObjectStore`, `BlobResidence`, `EraseTally`, and `MultipartTransfer.Upload`; storage form, faults, chunking, classification, and key custody compose from their owning package surfaces.

## [01]-[INDEX]

- [02]-[LIFECYCLE_RULES]: `LifecycleRule` the per-class prefix rule, and `LifecycleRules` deriving its ladder root, projecting expiries and transition rungs from declared schedule values alone, and arming each provider's own rule surface through the union case that IS the dispatch.
- [03]-[BLOB_GC]: `BlobCatalogRow` and `PendingWrite` the two durable ledgers, and `BlobGc` owning the write-blob-first protocol, the in-flight fence, the retention-fact projection, the WORM-gated metadata-only demote, the set-shaped evict arrow, and the reclaim pass that arms the provider half before routing every verdict through the one retention executor.

## [02]-[LIFECYCLE_RULES]

- Owner: `LifecycleRule` the declared per-class rule carrying its expiry and its transition schedule; `LifecycleRules` owning the ladder-root derivation, the `Project` fold from declared schedule values, the `Descent` ladder walk, and the per-provider `Arm` dispatch.
- Law: rows derive from DECLARED schedule values alone; a class that expires does so at its own age bound, and a never-evict class demotes down the retention ceiling ladder at cumulative multiples of that SAME bound, rung k at k times the bound — one declared value, zero new knobs.
- Law: the count and size stages NEVER project; a prefix rule reads neither a live count nor a running byte total, so those two stages stay the sweep's and the provider half can never contradict a verdict it never computes; a class whose age bound never lapses arms nothing, because a rule with no reachable deadline only pretends to govern.
- Entry: `Project` folds the class roster into rules; `Arm` installs them once per bucket, the union case being the dispatch exactly as every other per-provider variance is, with each arm's shape mirroring the `StorageTier` per-provider columns row for row.
- Boundary: the class-leading object name is what unlocks the provider half at all — a rule targets a key PREFIX, so one rule per class stem hands the provider's own engine the expiries and transitions the sweep otherwise pays one request per object to effect. Azure's lifecycle policy is an ARM MANAGEMENT-plane resource outside the admitted data-plane package, so its arm declares NO lifecycle surface and the demote path stays its whole mechanism — the same declared-none form the Minio storage-class column already takes; the presigned row holds no bucket to arm. Minio's ILM rule carries ONE transition and its row states no storage-class column, so it arms the expiries alone. Every SDK call lifts at the ONE `ObjectIo.Bound` boundary, never a second fold.
- Packages: AWSSDK.S3 (`PutLifecycleConfigurationAsync` + the `LifecycleConfiguration`/`LifecycleRule`/`LifecycleFilter`/`LifecycleRuleExpiration`/`LifecycleTransition` shapes), Google.Cloud.Storage.V1 (`PatchBucketAsync` over the `Bucket.LifecycleData` resource), Minio (`SetBucketLifecycleAsync` + `Minio.DataModel.ILM`), LanguageExt.Core, NodaTime.
- Growth: a new retention class arms its own prefix rule with zero edits, the schedule columns already carrying the deadline; a new cold rung re-roots the projection from the retention ceiling table alone; a provider gaining a lifecycle surface is one `Arm` case; a per-class knob beside the declared schedule, a hand-asserted ladder root, or a count or size condition on a prefix rule is the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Persistence.Element;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct LifecycleRule(RetentionClass Class, Option<Duration> Expire, Seq<(StorageTier To, Duration After)> Transitions);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LifecycleRules {
    static readonly Seq<StorageTier> Rungs = toSeq(StorageTier.Items)
        .Find(static tier => toSeq(StorageTier.Items).ForAll(other => RetentionCeiling.Demote(other) != Some(tier)))
        .Match(Some: Descent, None: static () => Seq<StorageTier>());

    public static Seq<LifecycleRule> Project(Seq<RetentionClass> classes) =>
        classes.Filter(static cls => cls.Lane == StorageLane.ObjectStore && cls.Schedule.AgeBound < Duration.MaxValue)
            .Map(static cls => cls.Collects
                ? new LifecycleRule(cls, Some(cls.Schedule.AgeBound), Seq<(StorageTier To, Duration After)>())
                : new LifecycleRule(cls, None, toSeq(Rungs.Select((rung, index) => (To: rung, After: cls.Schedule.AgeBound * (index + 1))))));

    static Seq<StorageTier> Descent(StorageTier from) =>
        RetentionCeiling.Demote(from).Match(Some: colder => colder.Cons(Descent(colder)), None: static () => Seq<StorageTier>());

    public static IO<Unit> Arm(ObjectClient client, Seq<RetentionClass> classes) {
        Seq<LifecycleRule> rules = Project(classes);
        return rules.IsEmpty ? IO.pure(unit) : client.Map(
            s3: r => ObjectIo.Bound(r, "s3", ObjectVerb.Lifecycle, default, () => r.Client.PutLifecycleConfigurationAsync(r.Bucket, new Amazon.S3.Model.LifecycleConfiguration {
                Rules = rules.Map(rule => new Amazon.S3.Model.LifecycleRule {
                    Id = BlobName.Prefix(r.Tenant, rule.Class),
                    Status = LifecycleRuleStatus.Enabled,
                    Filter = new LifecycleFilter { Prefix = BlobName.Prefix(r.Tenant, rule.Class) },
                    Expiration = rule.Expire.Match(Some: static after => new LifecycleRuleExpiration { Days = (int)after.TotalDays }, None: static () => null),
                    Transitions = rule.Transitions.Map(static t => new LifecycleTransition { Days = (int)t.After.TotalDays, StorageClass = t.To.S3Class }).ToList(),
                }).ToList(),
            })).Map(static _ => unit),
            azure: static _ => IO.pure(unit),
            gcs: r => ObjectIo.Bound(r, "gcs", ObjectVerb.Lifecycle, default, () => r.Client.PatchBucketAsync(new Google.Apis.Storage.v1.Data.Bucket {
                Name = r.Bucket,
                Lifecycle = new Google.Apis.Storage.v1.Data.Bucket.LifecycleData { Rule = rules.Bind(rule => Gcs(rule, r.Tenant)).ToList() },
            })).Map(static _ => unit),
            minio: r => rules.Choose(static rule => rule.Expire.Map(after => (rule.Class, After: after))).Match(
                Empty: static () => IO.pure(unit),
                More: expiring => ObjectIo.Bound(r, "minio", ObjectVerb.Lifecycle, default, async () => {
                    await r.Client.SetBucketLifecycleAsync(new SetBucketLifecycleArgs()
                        .WithBucket(r.Bucket)
                        .WithLifecycleConfiguration(new Minio.DataModel.ILM.LifecycleConfiguration(expiring.Map(row => new Minio.DataModel.ILM.LifecycleRule {
                            ID = BlobName.Prefix(r.Tenant, row.Class),
                            Status = Minio.DataModel.ILM.LifecycleRule.LifecycleRuleStatusEnabled,
                            Filter = new RuleFilter { Prefix = BlobName.Prefix(r.Tenant, row.Class) },
                            Expiration = new Expiration { Days = row.After.TotalDays },
                        }).ToList()))).ConfigureAwait(false);
                    return unit;
                })),
            presigned: static _ => IO.pure(unit));
    }

    static Seq<Google.Apis.Storage.v1.Data.Bucket.LifecycleData.RuleData> Gcs(LifecycleRule rule, TenantId tenant) =>
        rule.Expire.Map(after => Gcs("Delete", null, after, rule.Class, tenant)).ToSeq() +
        rule.Transitions.Map(t => Gcs("SetStorageClass", t.To.GcsClass, t.After, rule.Class, tenant));

    static Google.Apis.Storage.v1.Data.Bucket.LifecycleData.RuleData Gcs(string action, string? storageClass, Duration after, RetentionClass cls, TenantId tenant) => new() {
        Action = new Google.Apis.Storage.v1.Data.Bucket.LifecycleData.RuleData.ActionData { Type = action, StorageClass = storageClass },
        Condition = new Google.Apis.Storage.v1.Data.Bucket.LifecycleData.RuleData.ConditionData { Age = (int)after.TotalDays, MatchesPrefix = [BlobName.Prefix(tenant, cls)] },
    };
}
```

## [03]-[BLOB_GC]

- Owner: `BlobCatalogRow` the content-lineage retention row, `PendingWrite` the kind-bearing write-first ledger row, and `BlobGc` the static surface owning the write-blob-first protocol, the in-flight fence, the retention-fact projection, the WORM-gated demote, the set-shaped evict arrow, and the reclaim pass — never a second deletion executor.
- Law: one sweep pass handles one retention class, so its schedule and `SweepTally` govern one inventory.
- Law: the catalog IS the authoritative inventory; a listing-then-filter sweep is the deleted parallel executor, and artifact GC over head alone is forbidden — the retention mark folds every AS-OF cut, so a blob a historical version references survives.
- Law: identity and event share the ONE Marten session transaction; the blob is write-first and referenced-after, with no two-ORM atomicity dance.
- Law: the demote gate reads a REALIZED rung alone; a provider stating no storage class returns the row's own declared default, so a gate folding observation and assumption into one value skips a transition that never happened — `Rung.Realized` is what makes the observation a proof.
- Entry: `WriteBlobFirst` takes the facts only the admitting caller holds — the kind, the payload family's codec, and the `BlobAdmission` carrying the settled classification and lineage — and carries `open → blob → catalog → close` as the THREE durable marks one `BlobLedger` composes, so a write path holding two of the three is unrepresentable; `InFlightFence` derives each key's grace from that key's OWN class orphan age; `ToFact` projects a catalog row to the retention fact the executor budgets on; `Demote` rewrites a storage-class header behind an observation gate and a WORM gate; `WormEvict` partitions a verdict group against the catalog's WORM index in one pass; `Sweep` groups by derived class and routes every group through the one executor; `Pass` is the reclaim entry the host schedules, arming the provider-side rules before the sweep so the two halves of retention install and run as one fold.
- Auto: a crash before catalog commit leaves a pending collectible orphan. Upload and catalog share one WORM instant, and a provider-realized colder rung completes as a no-op.
- Boundary: the artifact blob carries the SAME content-lineage and retention-catalog row the snapshot spine has and registers in the object-store retention class, so ONE full-history reachability GC governs both — this lane contributing only its fact projection, its in-flight and WORM fence, and its tier transition. Eviction leaves as a SET so the verdict group goes out through the row's own erase paging, held keys landing on the tally as per-key refusals rather than a rail failure costing the whole pass one compliance window; the eligibility predicate ALSO holds a locked key, so the arrow is the defense-in-depth second gate against a window that landed after the verdict was computed; the catalog row's tenant column stamps the frame tenant the first write step proves equal to the client tenant, and the catalog the caller hands the sweep is RLS-filtered at its query, so a cross-tenant reclaim is unrepresentable end to end; the SSE key MATERIAL is a key-id string on the encryption case and the DEK-wrapping lifecycle is Authority's; a blob-lane-local KMS wrap is the deleted form.
- Packages: System.IO.Hashing, NodaTime (`Instant`/`Duration` the WORM window), LanguageExt.Core (`Seq`/`Choose`/`Partition`/`TraverseM`/`IO.fail`), System.Collections.Frozen (`FrozenDictionary` the WORM index), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new catalog column is one field on the row; a new WORM stance is one `ObjectLock` case both the write and the evict arrow read with zero new surface; a head-only blob GC, a lane-local list-then-filter sweep, a payload re-PUT standing in for a storage-class change, a same-transaction blob write, or a lane-local retention executor re-deciding eviction beside the one sweep is the deleted form.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record BlobCatalogRow(ContentAddress Key, ArtifactKind Kind, Extent Extent, StorageTier Tier, ObjectCodec Codec, Option<ContentAddress> Lineage, TenantId Tenant, DataClassification Classification, Option<Instant> WormUntil, Option<WrappedKey> Dek, Instant At) {
    public RetentionClass Class => Kind.Retention;
}

public readonly record struct PendingWrite(ContentAddress Key, ArtifactKind Kind, long Bytes, Instant Started, Option<string> Session);

public readonly record struct BlobLedger(
    Func<PendingWrite, IO<Unit>> Open,
    Func<BlobCatalogRow, IO<Unit>> Catalog,
    Func<ContentAddress, IO<Unit>> Close);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BlobGc {
    public static IO<BlobResidence> WriteBlobFirst(ObjectStore store, ObjectClient client, BlobAdmission admitted, ArtifactKind kind, ObjectCodec codec, ReadOnlySequence<byte> source, BlobLedger ledger, ProjectionContext frame) =>
        from _t in frame.Tenant.TenantId == client.Tenant ? IO.pure(unit) : IO.fail<Unit>(new RemoteStoreFault.Denied(admitted.Key, store.Key, "tenant-mismatch"))
        let key = admitted.Key
        let session = admitted.Session
        let handle = BlobName.Handle(key, client.Tenant, kind.Retention, codec, source.Length)
        from _o in ledger.Open(new PendingWrite(key, kind, source.Length, frame.Now(), session))
        from formed in store.Encode(codec, key, source)
        from resident in MultipartTransfer.Upload(store, client, handle, BlobResidence.From(key, new Extent(formed.Bytes.Length, source.Length), new Rung.Assumed(store.Tier), codec) with { ConditionToken = session }, ContentChunker.Chunk(store.Chunking, formed.Bytes), formed.Bytes, frame)
        from _c in ledger.Catalog(new BlobCatalogRow(key, kind, resident.Extent, store.Tier, codec, admitted.Lineage, frame.Tenant.TenantId, admitted.Classification, resident.WormUntil, formed.Dek, frame.Now()))
        from _x in ledger.Close(key)
        select resident;

    public static Func<ContentAddress, bool> InFlightFence(Seq<PendingWrite> pending, Instant now) =>
        key => pending.Find(w => w.Key == key).Match(Some: w => now - w.Started >= w.Kind.Retention.Schedule.OrphanAge, None: () => true);

    static RetentionFact ToFact(BlobCatalogRow row) => new(row.Class, row.Key, row.Extent.Stored, row.Tier, row.At);

    static IO<Unit> Demote(ObjectStore store, ObjectClient client, RetentionClass cls, ContentAddress key, StorageTier colder, FrozenDictionary<ContentAddress, (string Mode, Instant Until)> worm, Instant now) {
        BlobHandle handle = BlobName.Handle(key, client.Tenant, cls, ObjectCodec.Identity, 0L);
        return worm.TryGetValue(key, out (string Mode, Instant Until) held) && now < held.Until
            ? IO.fail<Unit>(new RemoteStoreFault.Locked(key, held.Mode, held.Until))
            : store.Head(client, handle).Bind(present => present.Match(Some: resident => resident.Tier is Rung.Realized { Tier: var realized } && realized == colder, None: static () => false)
                ? IO.pure(unit)
                : store.Transition(client, handle, colder, now));
    }

    static Func<Seq<ContentAddress>, IO<EraseTally>> WormEvict(ObjectStore store, ObjectClient client, RetentionClass cls, FrozenDictionary<ContentAddress, (string Mode, Instant Until)> worm, Instant now) =>
        keys => {
            (Seq<ContentAddress> Held, Seq<ContentAddress> Free) split =
                keys.Partition(key => worm.TryGetValue(key, out (string Mode, Instant Until) w) && now < w.Until);
            EraseTally refused = new(split.Held.Count, split.Held.Map(static key => (Key: key, Code: nameof(RemoteStoreFault.Locked))));
            return split.Free.IsEmpty
                ? IO.pure(refused)
                : store.EraseMany(client, split.Free.Map(key => BlobName.Handle(key, client.Tenant, cls, ObjectCodec.Identity, 0L))).Map(page => refused + page);
        };

    public static IO<Seq<SweepTally>> Sweep(ObjectStore store, ObjectClient client, Seq<BlobCatalogRow> catalog, Seq<PendingWrite> pending, Reachability reachable, Seq<Hold> holds, ProjectionContext frame) {
        FrozenDictionary<ContentAddress, (string Mode, Instant Until)> worm = catalog.Choose(static r => r.WormUntil.Map(u => (r.Key, (Mode: "worm", Until: u)))).ToFrozenDictionary(static t => t.Key, static t => t.Item2);
        Instant now = frame.Now();
        Func<ContentAddress, bool> fence = InFlightFence(pending, now);
        Func<ContentAddress, bool> eligible = key => fence(key) && !(worm.TryGetValue(key, out (string Mode, Instant Until) w) && now < w.Until);
        return toSeq(catalog.GroupBy(static row => row.Class)).TraverseM(group =>
            RetentionSweep.Execute(
                group.Key,
                RetentionSweep.Run(group.Key, toSeq(group).Map(ToFact), holds, reachable, eligible, now, frame.Correlation).Verdicts,
                WormEvict(store, client, group.Key, worm, now),
                (key, tier) => Demote(store, client, group.Key, key, tier, worm, now),
                frame)).As();
    }

    public static IO<Seq<SweepTally>> Pass(ObjectStore store, ObjectClient client, Seq<BlobCatalogRow> catalog, Seq<PendingWrite> pending, Seq<RetentionClass> classes, Reachability reachable, Seq<Hold> holds, ProjectionContext frame) =>
        from _armed in LifecycleRules.Arm(client, classes)
        from tallies in Sweep(store, client, catalog, pending, reachable, holds, frame)
        select tallies;
}
```

| [INDEX] | [POLICY]        | [VALUE]                                          | [BINDING]                                                          |
| :-----: | :-------------- | :----------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | write protocol  | open-pending -> blob -> catalog -> close-pending | crash leaves a pending-fenced orphan, never a dangling reference   |
|  [02]   | txn owner       | identity and event in the Marten session         | blob is write-first; no two-ORM atomicity                          |
|  [03]   | GC executor     | the ONE retention sweep                          | this lane projects facts and fences; no parallel sweeper           |
|  [04]   | GC reachability | mark over EVERY AS-OF cut                        | full-history; head-only GC is forbidden                            |
|  [05]   | lineage catalog | same row the snapshot spine has                  | one GC governs both                                                |
|  [06]   | WORM window     | catalog column plus the evict arrow              | eligibility fence plus typed evict; no provider 403 leak           |
|  [07]   | tenancy         | tenant column plus RLS-filtered catalog          | tenant name segment; cross-tenant reclaim unrepresentable          |
|  [08]   | asset class     | kind column; retention DERIVES from it           | one axis both catalogs share                                       |
|  [09]   | sweep partition | one pass and tally per class present             | per-class budgets; a mixed inventory never rides one ceiling       |
|  [10]   | admitted stamps | kind, classification, lineage from the caller    | absence of evidence is not clearance                               |
|  [11]   | class segment   | name LEADS with the retention class              | one prefix rule per class over every tenant; membership immutable  |
|  [12]   | lifecycle rules | projection over declared schedule values         | expiry and rungs from the age bound; count and size stay the sweep |
|  [13]   | cold-tier move  | header rewrite through the transition slot       | the payload re-PUT is deleted                                      |
|  [14]   | demote gate     | realized rung beside the WORM gate               | an assumed rung can no longer suppress a transition                |
|  [15]   | residence form  | `Extent` and codec beside the class on the row   | budget on stored bytes; a reader still knows the plaintext extent  |
|  [16]   | evict grain     | set-shaped arrow over the row's erase page       | held keys refuse on the tally; a transport failure kills the pass  |
|  [17]   | reclaim entry   | `Pass` arms the provider half, then sweeps       | neither half schedules without the other                           |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
