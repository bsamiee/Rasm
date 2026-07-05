# w1 — Rasm.Persistence/Element batch b0 grounding dossier

Batch pages: `Element/graph.md` [improve], `Element/codec.md` [improve], `Element/identity.md` [rebuild], `Element/authority.md` [new].
VERIFIED PRIMARY EXTRACTS ONLY. Every claim carries a `file:line` anchor. Downstream spot-verifies these.

## [A] — INVENTORIES (real `ls`)

### Doctrine root `docs/stacks/csharp/`
```
algorithms.md  boundaries.md  domain/  language.md  rails-and-effects.md
README.md  shapes.md  surfaces-and-dispatch.md  system-apis.md
```
Domain shards `docs/stacks/csharp/domain/`: compute.md concurrency.md data-interchange.md diagnostics.md durability.md interaction.md persistence.md postgres.md README.md resilience.md runtime.md transport.md validation.md visuals.md.

### Shared substrate `libs/csharp/.api/` (rails layered onto folder tier)
api-languageext.md · api-thinktecture-runtime-extensions.md · api-thinktecture-json.md · api-thinktecture-messagepack.md · api-generator-equals.md · api-quikgraph.md · api-mapperly.md · api-nodatime.md · api-hashing.md · api-highperformance.md · api-mathnet-numerics.md · api-csparse.md (+others).

### Folder tier `libs/csharp/Rasm.Persistence/.api/` (77 catalogs). Element-relevant:
- graph: api-marten.md · api-npgsql.md · api-nodatime.md
- codec: api-messagepack.md · api-cbor.md · api-thinktecture-json.md · api-thinktecture-messagepack.md · api-thinktecture-serialization.md · api-nts-io.md · api-nodatime-stj.md · api-lz4.md · api-zstd.md · api-hashing.md · api-fastcdc.md
- identity: api-marten.md · api-npgsql-ef.md · api-pgvector-ef.md · api-h3.md · api-h3-pg.md · api-nts-ef.md · api-thinktecture-ef.md · api-ef-naming.md · api-ef-sqlite.md · api-ef-design.md · api-linq2db-ef.md · api-aws-kms.md · api-azure-keyvault.md · api-google-kms.md
- authority: api-thinktecture-runtime-extensions.md (shared) · api-generator-equals.md (shared)

### Element folder on disk (`loc libs/csharp/Rasm.Persistence/.planning/Element/`)
`identity.md` 582 CODE (Read reports 652 total lines) · `codec.md` 401 · `graph.md` 290. No `authority.md` on disk (the `new` target).

## [B] — CENTRAL MANIFEST PINS (`Directory.Packages.props`)
```
:27  Generator.Equals                                        4.0.1
:141 pocketken.H3                                            4.0.0
:260 AWSSDK.KeyManagementService                             4.0.100.2
:262 Azure.Security.KeyVault.Keys                            4.10.0
:279 EFCore.NamingConventions                               10.0.1
:282 Google.Cloud.Kms.V1                                     3.24.0
:301 Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite 10.0.2
:317 Thinktecture.Runtime.Extensions.EntityFrameworkCore10  10.4.0
```

## [C] — BRIEF SEAM/RIDER ANCHORS (`RASM-CS-PERSISTENCE-BRIEF.md`)
- `:30` VERIFIED DEFECT: "`identity.md:120` hand-writes a `NodeId` converter while `:82` relies on the Thinktecture auto-converter for `H3Cell` — one of the two violates the package's own prohibition (`ARCHITECTURE.md:113`)".
- `:55` H2/H3 hasher re-anchor law: "every Persistence digest mint composes the landed kernel `ContentHash.Of` seed-zero entry ... directly for the opaque preimages codec today raw-hashes (`SnapshotHeader.Seal` sealed bytes, the `ContentChunker` per-chunk and whole-payload digests ...) ... names itself the demanding consumer of the identity.md Growth row's streaming-identity member (`XxHash128` `Append` + `GetCurrentHashAsUInt128`, seed zero) for the blobstore multipart and chunk folds whose payloads outgrow a one-shot span".
- `:57` verify-or-die: "the seam `ContentAddress.Of(Node, double)` overload `timetravel.md:187` spells against merge's span form (`merge.md:160`) — verify the overload set or unify the spelling, and flag the `OfGraph`-vs-`Of(ElementGraph)` naming split to the Element campaign".
- `:94` V5 split ruling: "`Element/identity.md` extracts `Element/authority.md` — the object-ACL authz vocabulary (`Grant`/`GrantSet`/`AclScope`/`AclEntry`/`ObjectAcl`/`Authority.Admit`+`Effective`+`LapsedFor`, `identity.md:287-341,464-484` — a pure set-algebra with zero `KmsProvider`); identity keeps the relational tier + `IdentityPolicy` + KMS custody + `SchemaVerdict`. The signing+envelope co-location on ONE `KmsProvider` axis is SETTLED ... the split is authz-vs-crypto, never signing-vs-envelope".
- `:98` V6 EF commit: "register `UseValueObjectValueConverter()` (Thinktecture.EF) + `UseSnakeCaseNamingConvention()` (EFCore.NamingConventions) on the identity `DbContext`; the hand `NodeId` converter (`identity.md:120`) and the ~40 hand `HasColumnName` calls die; the LanguageExt-type converters (`Option<Vector>`/`Seq<NodeId>`, `identity.md:87-105`) stay". [NOTE: the spelled member `UseValueObjectValueConverter()` is NOT the verified member — see [G].]
- `:186` KMS trio depth: "`AWSSDK.KeyManagementService`+`Azure.Security.KeyVault.Keys`+`Google.Cloud.Kms.V1` — TWO disjoint arms per provider on the ONE `KmsProvider` axis".

## [D] — DECISION ELEMENT PAGE ROWS (`RASM-CS-PERSISTENCE-DECISION.md` `[04]`, all leg 1)
- `:84` graph.md KEEP·improve: "**HOSTS the `[FAULT_TABLES]` `FaultBand` `[SmartEnum<int>]` registry owner block**"; "**Re-homes `Principal` to the Persistence-owned `StoreActor`** ([A.1]); the `:253` AppHost annotation rewritten"; "**Persists the kernel generational naming lineage**: the `Rasm/Spatial/naming` `NameTable`/`TopoName` reference lineage (`Track(prior, rebuilt)`) ... the REFERENCE axis, distinct from the merge-consumed per-node `NamingHash` CONTENT receipt". Entry re-thread: `GraphStore.Run(session, identity, GraphStoreOp, StoreActor actor, Guid storeId, ProjectionContext frame) : IO<Fin<GraphReceipt>>`. Band Graph 8300.
- `:85` codec.md KEEP·improve: "`[V10]` HashPolicy shrink to `{Identity,Content}` + `HashDomain` forward-compat byte law; kill the `:175` `ByDomainId` prose drift. **THE kernel-hash re-anchor root** (all codec-tier mints compose here, [B])". GeoJsonProjection additionally serves Ingest/geospatial via `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>`. Band Codec 8310.
- `:86` identity.md SPLIT·rebuild: "Relational identity tier (Marten doc), `IdentityPolicy` key axis, KMS custody (`SigningKeyring`+`EnvelopeKeyring` on the one `KmsProvider` axis, V5a — authz-vs-crypto split), `SchemaVerdict` boot fold. `[V6]` EF commit (`UseValueObjectValueConverter()`+`UseSnakeCaseNamingConvention()`; hand `NodeId` converter + the 10 hand `HasColumnName` calls die ...) + the DDL/migration owner (`element_identity`/`node_cell` DDL; `ServerExtension.CreateSql` commits through this rail; `EF.Design` earns its admission here). **`[V13]` spatial + embedded-relational rows**: `UseNetTopologySuite()` joins the V6 provider options; the `Envelope`-derived `geometry(Polygon, 4326)` bounds column + GiST index row ... (`ST_Union`/`ST_Extent` aggregates, `.Distance`/`.Intersects` ...); `StoreProfile.Embedded` binds `UseSqlite` ... Slims 651→~410 LOC. Band Identity 8340".
- `:87` authority.md NEW·new·SPLIT-from `Element/identity.md#[04]-[AUTHORITY]` (`:287-341,465-484`): "Object-ACL authz set-algebra: `Grant`/`GrantSet`/`AclScope`/`AclEntry`/`ObjectAcl`/`Authority.Admit`+`Effective`+`LapsedFor` — pure deny-over-allow set-algebra, zero `KmsProvider`, total. Band **NONE**". Entry `Authority.Admit(GrantSet, AclScope) → Effective` (NONE — pure set-algebra). Seam: "⇄ Rasm.AppHost/Runtime (ObjectAcl identity-store PORT — the ARCH:53 `ObjectAcl` half moves HERE); → Version/commits#Movable (ACL gate on GrantSet)".

### DECISION band registry (`[03]`)
- `:132` "ONE `FaultBand` `[SmartEnum<int>]` sited on `Element/graph.md#[FAULT_TABLES]` ... a duplicate band integer FAILS at type initialization."
- `:145` `GraphFault` `Element/graph` 8300-8302 "register as-is (keeps the simple name; cypher renames) + registry HOST".
- `:146` `CodecFault` `Element/codec` 8310/8320/8330 "register as-is (legal 831x-833x multi-decade stride)".
- `:147` `IdentityFault` `Element/identity` 834x "register as-is (authority composes it, no new band)".
- `:160` "the ruling is FOLD into `GraphFault` 8300 — a concurrency conflict raised by the folded rail registers as the `GraphFault` sub-band row 8303, NEVER a loose 7001 integer."
- `:162` No-band pages list includes `Element/authority`.

### DECISION seam ledger (`[04]`)
- `:175` "ARCH:53 identity KMS PORT | SPLIT — `ObjectAcl` store → `Element/authority`; TenantId RLS + KMS keyrings → `Element/identity` (APPHOST:72 unchanged)".
- `:176` "ARCH:54 frame ingredients | re-express as INJECTED port-input shapes Persistence DEFINES (ClockPolicy/CorrelationId/TenantContext + the Persistence-owned `Principal` value-object ...); the csproj:10 AppHost ProjectReference is DELETED".

## [E] — CURRENT-DISK FENCE ANCHORS (the pages under edit)

### identity.md (the split source)
- `:56-67` `ElementIdentity` sealed record: `ModelId Model, UInt128 Tenant, Seq<NodeId> Roots, HashMap<NodeId,string> GlobalIds, H3Cell Cell, Option<Pgvector.Vector> Embedding, ObjectAcl Acl, DataClassification Classification, Instant At`.
- `:76-110` `IdentityShape : IEntityTypeConfiguration<ElementIdentity>` — hand `HasColumnName` on Tenant/Cell/Classification/At, hand `ValueConverter` on Embedding (`:87-93`), Roots (`:94-99`), GlobalIds (`:100-105`); `ComplexProperty(...ToJson("acl"))` (`:106`).
- `:120` `NodeCellShape` hand `NodeId` converter: `node.Property(static n => n.Node)...HasConversion(static n => n.Value, static s => NodeId.Create(s))` — the E7/`:30` defect vs `:82` auto-converter reliance.
- `:184-270` IDENTITY_POLICY: `IdentityPolicy` `[SmartEnum<string>]` (`:229`), `StoreKey` `[Union]` (`:202`), `Collision` `[SmartEnum<string>]` (`:194`); `Mint`/`Decode` generated `Switch` (`:242`,`:250`).
- `:273-524` AUTHORITY section (the split boundary). SPLIT-TO-authority set (per DECISION `:87`): `Grant` `[SmartEnum]` (`:287-299`), `GrantSet` `[Equatable]`+`[property: SetEquality] FrozenSet<Grant>` (`:306-315`; `Owner`/`Admits`/`Union`/`Without`), `AclScope` `[SmartEnum<string>]` self-`Parent` (`:317-326`), `AclEntry` record `Live`/`Lapsed` (`:329-332`), `ObjectAcl` record `LadderValid` (`:337-341`), and the authz folds `Authority.Effective` (`:465-473`), `Authority.LapsedFor` (`:475-478`), `Authority.Admit` (`:480-484`).
- KMS-CUSTODY set that STAYS on identity (per DECISION `:86`, `:175`): `KmsProvider` `[SmartEnum<string>]` `Signs`/`NativeWrap` (`:355-365`), `KeyState` (`:371-381`), `SigningAlgorithm` (`:383-399`), `OpDigest` `[ValueObject<byte[]>]` (`:401-406`), `SigningKeyring` record (`:408`), `SignedAuthorship` record (`:409`), `EnvelopeAad` `[ComplexValueObject]` (`:416-422`), `WrappedKey` record struct (`:427`), `EnvelopeKeyring` record `Mint`/`Unwrap`/`Rewrap`/`Probe` quartet (`:435-440`), and the crypto folds `Authority.Attest` (`:486-489`), `Verify` (`:491-494`), `Wrap` (`:500-505`), `Unwrap` (`:510-511`), `Rewrap` (`:517-522`).
- `:443-461` `AuthDecision` `[Union]` — FUSED authz+crypto: authz cases `Granted`(`:445`)/`Denied`(`:449`)/`ScopeMismatch`(`:450`)/`Expired`(`:451`) go to authority.md; crypto cases `Attested`/`Authentic`/`Unsigned`/`Unauthored`/`Forged`/`DigestWidth`/`Wrapped`/`Unwrapped`/`KeyUnusable` (`:446-460`) stay identity. The one union splits into two.
- `:526-643` SCHEMA_VERDICT: `Placement` `[SmartEnum<string>]` (`:543`), `SchemaVerdict` `[Union]` (`:555`), `IdentityFault` `[Union]:Expected` 834x (`:579-606`), `SchemaGate.Admit`/`AdmitMarten` (`:614`,`:638`) over EF `GetMigrations`/`GetAppliedMigrations`/`Migrate`/`HasPendingModelChanges` + Marten `ApplyAllConfiguredChangesToDatabaseAsync`.

### graph.md (FaultBand host + StoreActor re-thread)
- `:153-177` `GraphFault` `[Union]:Expected` cases DeltaRejected 8300 / StreamVersionConflict 8301 / ModelAbsent 8302 (`:161-164`). No `FaultBand` `[SmartEnum<int>]` type present anywhere — the V4 host block is unbuilt.
- `:232-242` `GraphStoreOp` `[Union]`: Open/Commit/Retire/Read/ReadAsOf/State.
- `:256` `GraphStore.Run(IDocumentSession session, ElementIdentity identity, GraphStoreOp op, Principal actor, Guid storeId, ClockPolicy clocks, CorrelationId correlation)` — the pre-[A.1] shape (loose `Principal`/`ClockPolicy`/`CorrelationId`, not `StoreActor`/`ProjectionContext frame`).
- `:287-288` `session.SetHeader("actor", actor.Subject)`/`SetHeader("origin", storeId.ToString())` — the blame-write; `actor` typed `Principal` today.
- `:321-327` `Lift` converts `Marten.Exceptions.ConcurrentUpdateException`/`JasperFx.Events.EventStreamUnexpectedMaxEventIdException` → `GraphFault.StreamVersionConflict`; all else → `DeltaRejected`.

### codec.md (kernel-hash re-anchor + HashPolicy shrink)
- `:216-220` `HashPolicy` `[SmartEnum<string>]` 5 rows: `Content`(XxHash3,`:216`)/`Identity`(XxHash128,`:217`)/`Frame`(Crc32,`:218`)/`Wide`(XxHash64,`:219`)/`FrameWide`(Crc64,`:220`) — the V10 shrink-to-`{Identity,Content}` target.
- Raw `XxHash128.HashToUInt128` direct call sites (the [B]/H2-H3 re-anchor targets): `SnapshotHeader.Seal` `:318`, `Snapshots.Verify` `:356`, `ContentChunker.Chunk` per-chunk `:437` + whole-payload `:439`, `Reassemble` `:448`. Also `EnvelopeAad.Of` `XxHash128.HashToUInt128` (identity.md `:421`).
- `:151-158` CONTENT_ADDRESS composes seam `ContentAddress.Of`/`OfGraph`/`Of(UInt128)`; `:157` "a Persistence-local `NodeHash`/`GraphHash` forwarding owner ... is the deleted form" (V9 incremental `OfGraph(prior,delta)` consumer contract records here).
- `:53,134-136` `GeoJsonProjection` record + `.Default.Factory` composed into `ElementJson.Options`.
- `:279-302` `CodecFault` `[Union]:Expected` NoMutualCodec 8310 / SnapshotRejected `8320+Tier.Rank` / ReassemblyDrift 8330.

## [F] — .API MEMBER BLOCKS (quoted, anchored) FOR CITED MEMBERS

### api-thinktecture-ef.md (identity V6 — the VERIFIED member vs the DECISION mis-spelling)
- `:73` "`UseThinktectureValueConverters()` — installs the plugin with `Configuration.Default`".
- `:74` "`UseThinktectureValueConverters(Configuration configuration)`".
- `:47/:96` "`ThinktectureValueConverterFactory.Create<T, TKey>(useConstructorForRead) -> ValueConverter<T, TKey>`".
- `:108` LOCAL_ADMISSION: "`Element/identity#ELEMENT_IDENTITY` mounts `.UseThinktectureValueConverters(Configuration.Default)` on the one `ConverterRail.Compose(DbContextOptionsBuilder)` row, so every `[ValueObject]`, `[SmartEnum]`, and keyed `[Union]` column converts with zero hand-written converter classes".
- `:117` Reject: "manual `HasConversion` for Thinktecture types".

### api-generator-equals.md (authority GrantSet)
- `:27` "`SetEqualityAttribute` | property / field | set equality (`ISet<T>.SetEquals` fast path; hashing always 0)".
- `:41` "`SetEqualityComparer<T>` | ... | `ISet<T>.SetEquals` fast path; `GetHashCode` returns 0".
- `:132` "hashing exclusions: `[PrecisionEquality]` and `[SetEquality]` members contribute nothing to `GetHashCode` (... set hashing returns 0) — equality and hashing intentionally diverge".
- `:50-53` `Inequality`/`MemberPath`/`MemberPathSegment`/`MemberPathSegmentKind` (`Property/Field/Index/Key/Added/Removed`) — the member-level structured-diff family.
- `:9` "analyzer-only package ... Assay resolves zero public runtime types for this package key" (so verified via catalog, not reflection).

### api-aws-kms.md (identity KMS custody — anchor drift evidence)
- `:7-10` "the DISJOINT asymmetric signing surface (`Sign`/`Verify`/`GetPublicKey` + `SigningAlgorithmSpec`/`MessageType`) the `Element/identity#AUTHORITY` `SigningKeyring` binds. The package serves TWO orthogonal Persistence concerns: the ENVELOPE arm ... and the SIGNING arm (the `SignedAuthorship` blame attestation `Element/identity#AUTHORITY` owns)." — ANCHOR `Element/identity#AUTHORITY` (drifts under the split: `#AUTHORITY` moves to authority.md = ACL-only; KMS custody keeps a renamed identity section).
- `:90/:154` envelope+signing entries: `GenerateDataKeyAsync`, `EncryptAsync`, `DecryptAsync`, `ReEncryptAsync`, `DescribeKeyAsync` (`:96` `KeyState` probe), `SignAsync(SignRequest{MessageType.DIGEST})` (`:154`), `VerifyAsync` (`:155`).
- `:79` `SigningAlgorithmSpec` constants; `:80` `MessageType` `DIGEST`/`RAW`/`EXTERNAL_MU`.
- `:91/:175` `GenerateDataKeyWithoutPlaintextAsync` — wrapped-only DEK for a minting node that never encrypts locally (an unexploited read-path arm).
- `:187` STACK_INTEGRATION: "the `KmsProvider` `[SmartEnum<string>]` provider axis (AWS/Azure/GCP/none) is owned by `Element/identity#AUTHORITY` ... a per-provider keying class is the deleted form".

### ASSAY VERIFICATION STATE
- `assay api query` resolves the rhino-common host set; the KMS packages (`AWSSDK.KeyManagementService`, `Azure.Security.KeyVault.Keys`, `Google.Cloud.Kms.V1`) and `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` are NOT in the restored assembly cache (query returns `unsupported`/`no source`; nearest-name candidates only). Per fallback law these members are verified against the decompile-derived `.api` catalogs above + the central manifest pins in [B]. `Generator.Equals` is analyzer-only by its own catalog (`:9`), so catalog is the authoritative surface there too.

## [G] — CROSS-PAGE / FOLDER-CONTEXT ANCHORS
- ARCH domain map pre-split: `libs/csharp/Rasm.Persistence/ARCHITECTURE.md:5` "`Identity.cs` # ElementIdentity relational tier ... IdentityPolicy key axis, Grant/GrantSet/ObjectAcl/Auth..." — the ARCH map still fuses authz into Identity.cs; the split adds an `Authority.cs` row (tail/index obligation, DECISION `[STRUCTURAL_AUTHORITY]` `BRIEF:40`).
- ARCH seam `libs/csharp/Rasm.Persistence/ARCHITECTURE.md:53` "`Element/identity ⇄ csharp:Rasm.AppHost/Runtime # [PORT]: ObjectAcl identity store, TenantId RLS, KMS SigningKeyring + EnvelopeKeyring KMS-...`" — the ARCH:53 row DECISION-`:175` SPLITs (`ObjectAcl`→authority; RLS+KMS→identity).
- graph.md `:3` STORE_RAIL composes seam `GraphDelta.ReplayOnto` (one materializer) and co-commits `Element/identity#ELEMENT_IDENTITY` `IdentityStore.Stamp` — the identity co-transaction the split must preserve (identity stays the Marten-doc stamp; authority carries no store op).
- codec.md `:3` composes seam `Projection/address#CONTENT_ADDRESS` `ContentAddress` + `#CANONICAL_WRITER`; identity.md `:17,:103` value-converts `GlobalIds` through `Element/codec#CODEC_AXIS` `ElementJson.Options` — codec is the STJ owner both identity and graph read.

## [H] — DEFECT / NAMING FLAGS (VERIFIED, anchored — buildout targets, never removals)
1. `RASM-CS-PERSISTENCE-DECISION.md:86`/`:98` spell the V6 member `UseValueObjectValueConverter()`; the VERIFIED member (`api-thinktecture-ef.md:73,:108`) is `UseThinktectureValueConverters(Configuration.Default)`. The rebuild composes the verified spelling.
2. `identity.md:120` hand `NodeId` converter vs `:82` `H3Cell` auto-converter reliance = the E7/`BRIEF:30` prohibition breach (`ARCHITECTURE.md:113`); V6 collapses both to the ONE convention.
3. `codec.md:318,356,437,439,448` + `identity.md:421` call `System.IO.Hashing.XxHash128.HashToUInt128` DIRECTLY — the per-call-site invocation `BRIEF:55` re-anchors to kernel `ContentHash.Of` (value-unchanged; call-path collapse).
4. `AuthDecision` `[Union]` (identity `:443-461`) FUSES authz + crypto verdicts; the split fissions it into an authz union (authority.md) + a crypto union (identity.md), and the `Authority` static class (identity `:464`) fissions into an authz `Authority.Admit`/`Effective`/`LapsedFor` (authority.md) + a renamed crypto owner holding `Attest`/`Verify`/`Wrap`/`Unwrap`/`Rewrap` (identity.md).
5. `api-aws-kms.md:7,:10,:187` (and the sibling `api-azure-keyvault.md`/`api-google-kms.md`) anchor the KMS keyrings to `Element/identity#AUTHORITY`; after the split `#AUTHORITY` = ACL-only on authority.md, so the KMS catalogs re-anchor to the renamed identity KMS-custody section (folder-wide catalog-hygiene obligation routed to identity).
