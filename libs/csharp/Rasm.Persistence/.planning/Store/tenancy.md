# [PERSISTENCE_TENANCY]

`Rasm.Persistence` owns the multi-tenancy and row-level-security axis on the self-provisioned PostgreSQL 18.4 server tier as ONE closed lifecycle. `TenancyModel` is the `[SmartEnum<string>]` isolation-strategy axis whose row carries its isolation plane and partition behaviour as derived columns. `TenantSlug` is the `[ValueObject<string>]` identifier admitted EXACTLY ONCE so every interpolated DDL identifier rides a closed, validated vocabulary rather than a raw client string. `TenantId` arrives from the AppHost `TenantContext` as a `[ValueObject<UInt128>]`; this page renders it to its canonical `uuid` wire form ONCE through the `TenantId.Uuid` extension projection so the `set_config` ingress, the `ALTER ROLE … SET` default, and the `current_setting('rasm.tenant', true)::uuid` RLS predicate read one settled spelling — never a decimal `UInt128` that a `::uuid` cast rejects. `RlsPolicy` is the `[ComplexValueObject]` carrying the full `CREATE POLICY` product (command, permissive/restrictive composition, grantee role, the `USING` read predicate and the `WITH CHECK` write predicate). `PartitionPolicy` is the `[ComplexValueObject]` carrying the per-tenant `pg_partman` lifecycle (control column, interval, premake horizon, retention, `retention_keep_table`, infinite-time stance). `TenantQuota` is the `[ComplexValueObject]` carrying the per-tenant role-and-GUC governance set the write path and the role constraint enforce. `RlsScope` is the per-transaction `set_config('rasm.tenant', …, is_local => true)` ingress that binds the session GUC the RLS predicate reads. `TenantProvision` is the one owner folding the `TenancyModel × TenantPhase` matrix: `Project` lowers a `(model, phase)` cell to `Validation<TenancyFault, Seq<string>>` DDL and `Provision` runs an `Apply`+`Policy` lifecycle end to end into a `Validation<TenancyFault, TenantReceipt>`. `TenancyFault` is the closed `[Union]` deriving from `Expected`. `TenantReceipt` is the typed lifecycle evidence whose `Facts` projection folds the policy/quota/audit proof onto the page-wide fact stream. PostgreSQL with RLS is the same `Store/profiles#PROFILE_AXIS` `PostgresServer` profile — never a forked store family and never a new engine. The page spine is Npgsql, Thinktecture vocabulary, LanguageExt rails, NodaTime, and the admitted `pg_partman` per-tenant partition maintenance; the deploy-side cluster-GUC verification is the sibling `Store/provisioning#CLUSTER_CONFIG` owner and the per-tenant audit class resolves at `Version/retention#AUDIT_BINDING`.

Wire posture: this page is host-local — every owner emits raw tenant-lifecycle SQL executed server-side. The tenancy primitive it consumes (`TenantContext`) crosses the wire only as `TenantContextWire` owned at `AppHost/runtime-ports#TS_PROJECTION`; this page binds the `rasm.tenant` GUC per transaction through `RlsScope`, reads `TenantId` as the fail-closed `nullif(current_setting('rasm.tenant', true), '')::uuid` RLS predicate, and never mints a client-facing projection.

## [01]-[INDEX]

- [01]-[TENANCY_RLS]: multi-tenancy axis, identifier admission, the canonical tenant-id wire form, the full RLS policy product, the per-tenant partition policy, the per-tenant role/GUC quota, the per-transaction GUC-binding ingress, the lifecycle fold, and the typed fault family.

## [02]-[TENANCY_RLS]

- Owner: `TenancyModel` the multi-tenancy `[SmartEnum<string>]` axis under the `StoreKeyPolicy` ordinal accessor, each row carrying its isolation plane (`OwnsSchema`/`OwnsRole`/`ClonesDatabase`/`EmitsPolicy`/`Partitions`) as derived behaviour columns; `TenantSlug` the `[ValueObject<string>]` validated identifier admitting the raw slug ONCE so every DDL identifier is closed-vocabulary; `TenantWire` the `TenantId.Uuid` extension projection rendering the `UInt128` to its `uuid` text once at the SQL egress; `RlsPolicy` the `[ComplexValueObject]` `CREATE POLICY` product; `PartitionPolicy` the `[ComplexValueObject]` per-tenant `pg_partman` lifecycle; `TenantQuota` the `[ComplexValueObject]` per-tenant role-and-GUC governance set; `RlsScope` the per-transaction `set_config` GUC ingress; `TenantProvision` the one owner with the `Project` DDL fold and the `Provision` receipt-minting lifecycle over the `TenancyModel × TenantPhase` matrix; `TenancyFault` the closed fault family; `TenantReceipt` the lifecycle evidence with its `Facts` proof-row projection.
- Cases: `TenancyModel` `single` (no isolation, the shared row) | `schema` (per-tenant schema + role + search-path) | `rls` (one shared table, predicate isolation by the session GUC) | `db-per-tenant` (a template-cloned database); `TenantPhase` `Apply` (create the per-model objects) | `Destroy` (idempotent teardown) | `Policy` (the RLS `CREATE/DROP POLICY` set) | `Bind` (the per-transaction `SET LOCAL` GUC ingress) | `Adopt` (a session-default `ALTER ROLE … SET` adoption); `PolicyCommand` `ALL`/`SELECT`/`INSERT`/`UPDATE`/`DELETE` each carrying its derived `CarriesUsing`/`CarriesCheck` clause columns so a policy emits ONLY the read/write predicate its command admits; `TenantQuota` carries the connection cap, the statement/idle-txn/idle-session/lock timeouts, the `temp_file_limit`, the `work_mem` and `maintenance_work_mem` floors, the `TenantDurability` per-tenant `synchronous_commit`/isolation/read-only/deferrable stance, the role `VALID UNTIL` expiry, and the bound `AuditCategory` whose full six-GUC pgaudit product (`pgaudit.log`, `pgaudit.log_relation`, `pgaudit.log_parameter`, `pgaudit.log_catalog`, `pgaudit.log_parameter_max_size`, `pgaudit.log_level`) lowers as role-default GUCs — never the bare `pgaudit.log` class string alone — and the write-byte-rate bound is NOT a column here, it lives wholly on the `Query/rail#BULK_LANE` COPY rail because PostgreSQL exposes no per-role write-rate GUC; `RlsPolicy` carries the command, permissive/restrictive mode, grantee role, and the `USING`/`WITH CHECK` predicates; `PartitionPolicy` carries the parent relation name, control column, interval, premake horizon, retention, retention-keep-table stance, infinite-time stance, and the opt-in back-fill flag.
- Entry: `public static Validation<TenancyFault, Seq<string>> Project(TenancyModel model, TenantPhase phase, TenantContext tenant, TenantQuota quota, PartitionPolicy partition)` is the one DDL-fragment fold and `public static Validation<TenancyFault, TenantReceipt> Provision(TenancyModel model, TenantContext tenant, TenantQuota quota, PartitionPolicy partition, Seq<RlsPolicy> policies, Seq<DataClassification> classifications, CorrelationId correlation, Instant at)` is the receipt-minting lifecycle owner that composes the `Apply` and `Policy` projections applicatively and folds their proof into the typed `TenantReceipt`; the slug admits through `TenantSlug.Validate` BEFORE any identifier interpolation so an unsafe slug accumulates a `TenancyFault.UnsafeSlug` rather than reaching the DDL string, and the `(model, phase)` pair selects its DDL fragment through `TenancyModel.Switch` — `Apply` folds the per-model create DDL (schema/role/GUC/partition steps keyed on the admitted `TenantSlug`, the rendered `TenantId.Uuid`, the `TenantQuota` GUCs, and the `PartitionPolicy`), `Destroy` folds the idempotent `DROP … IF EXISTS` teardown, `Policy` admits EVERY identifier of every policy (table, per-policy name, tenant column, grantee) in one accumulating pass through `RlsPolicy.Admit` then projects the `rls`-row `CREATE POLICY` set (every other row projects the empty sequence), `Bind` projects the single per-transaction `select set_config('rasm.tenant', …, true)` statement, and `Adopt` projects the role-default `ALTER ROLE … SET rasm.tenant`.
- Auto: the `rls`-plane table arms ONCE through `RlsPolicy.Arm` (`ALTER TABLE … ENABLE ROW LEVEL SECURITY` then `ALTER TABLE … FORCE ROW LEVEL SECURITY`, so the table owner is filtered too — the page-level isolation guarantee a bare `ENABLE` does not give) and then projects one `CREATE POLICY` per policy in the admitted set, so the table-level arming and the per-policy creation never duplicate; the permissive base binds `FOR ALL … USING (tenant_id = nullif(current_setting('rasm.tenant', true), '')::uuid) WITH CHECK (…)` so a tenant can neither read nor write another tenant's rows, and a writable table adds `RESTRICTIVE` per-command write conjuncts covering every mutating verb (`FOR INSERT WITH CHECK`, `FOR UPDATE USING … WITH CHECK`, `FOR DELETE USING …`) that compose as an AND over the permissive base for defense-in-depth — each emitting ONLY the clauses its `PolicyCommand` admits rather than both unconditionally, so no write path (including DELETE) is governed by the permissive base alone; the tenant id rides that session GUC which `RlsScope.Bind` sets per transaction through `set_config(slot, id.Uuid, is_local => true)` so the GUC is transaction-scoped and pool-return cannot leak it across acquisitions (the ingress the RLS predicate, the `Sync/coordination#FENCED_CAS` CAS, and every `Query/rail` read depend on — and the two-arg `current_setting('rasm.tenant', true)` with the `nullif(…, '')` lift is the load-bearing fail-closed form: an unbound session reads NULL, `col = NULL` is never true, so the predicate yields zero rows rather than RAISING the `unrecognized configuration parameter` error the one-arg spelling throws). Every create step derives from the resolved `TenantContext`: the role step mints `CREATE ROLE … WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS NOINHERIT NOREPLICATION` so the tenant role can never bypass its own RLS predicate, never inherit a granted role's privileges, and never replicate — the isolation the policy assumes; the `schema` row creates the tenant schema, `REVOKE`s `ALL … FROM PUBLIC` before it `GRANT`s the schema's table privileges to the tenant role, sets the search-path role default, and folds the `PartitionPolicy` into a `pg_partman` `create_parent` plus a `part_config` `UPDATE` (premake/retention/retention-keep-table) when the model partitions; the `rls` row seeds the per-tenant `rasm.tenant` GUC default, the bound `AuditCategory`'s full six-GUC pgaudit role-default set, and the shared partition set; the `db-per-tenant` row emits the `CREATE DATABASE … TEMPLATE rasm_template` clone, then `REVOKE CONNECT … FROM PUBLIC` so no foreign role connects to the per-tenant database. Each model folds the `TenantQuota` GUC rows (`ALTER ROLE … CONNECTION LIMIT`, `ALTER ROLE … SET statement_timeout/idle_in_transaction_session_timeout/idle_session_timeout/lock_timeout/temp_file_limit/work_mem/maintenance_work_mem` plus the full bound `AuditCategory.Guc` six-row pgaudit set and the non-default `TenantDurability` `synchronous_commit/default_transaction_isolation/default_transaction_read_only/default_transaction_deferrable` rows, `ALTER ROLE … VALID UNTIL`) so the quota is a deploy-time role constraint rather than a runtime branch; the schema row's partition step folds an opt-in `partman.partition_data_proc` back-fill when the policy attaches to a non-empty parent; `Destroy` mirrors each create with its idempotent `DROP … IF EXISTS` step. `TenantReceipt.Facts` folds the per-tenant RLS-policy-applied fact, the per-tenant quota-set fact, and the `AuditBinding.Bind`-bound audit category onto the `StoreFact.Maintain` provisioning-verifier stream.
- Receipt: `TenantReceipt(TenantId Tenant, TenantSlug Slug, TenancyModel Model, Seq<RlsPolicy> Policies, TenantQuota Quota, PartitionPolicy Partition, AuditCategory Audit, Seq<string> Applied, CorrelationId Correlation, Instant At)` is the lifecycle evidence `Provision` mints; its `Facts(Duration elapsed)` projection folds the policy/quota/audit proof onto the ONE settled `StoreFact.Maintain` kind the closed `Query/rail#STORE_FACT` vocabulary already carries — the tenant-provisioning variant rides the `Subject` slot (`tenant.policy:…` per armed policy with its `CREATE POLICY` text, `tenant.quota:…` per role-default GUC `(setting, value)` pair, `tenant.audit:…` carrying the per-tenant `AuditCategory` bound through `AuditBinding.Bind` over the tenant's classifications), never a new dotted kind constant the closed `StoreFact` vocabulary does not define — all onto the page-wide stream the provisioning verifier consumes; a tenant-isolation mismatch, an unsafe slug, or a quota-set failure is a typed `TenancyFault`, never silent.
- Packages: Npgsql, Npgsql.EntityFrameworkCore.PostgreSQL, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: one `TenancyModel` row per new isolation strategy (with its behaviour columns); one `TenantQuota` GUC column per new resource bound; one `RlsPolicy` column per new policy dimension; one `PartitionPolicy` column per new `part_config` knob; one `TenantPhase` case per new lifecycle concern; one `TenancyFault` case per new failure cause; zero new surface.
- Boundary: `TenancyModel` is one axis row tying the host profile to an RLS policy, never a forked store family — PostgreSQL with RLS is the same `Store/profiles#PROFILE_AXIS` `PostgresServer` profile, never a new engine, and the package prohibition against admitting a new engine row holds; the tenant id arrives from the AppHost `TenantContext` threading owned at `AppHost/runtime-ports#PORT_RECORDS`, never minted here — `TenantProvision.Project` consumes the resolved `TenantContext.TenantId`/`Slug` as settled input across every `TenantPhase`, and the `TenantId.Uuid` extension projection is the ONE place the `UInt128` tenant id renders to its `uuid` egress form (the AppHost owns the value; this page owns its SQL spelling), so the tenant-id resolution stays the AppHost concern and only the per-model DDL lifecycle, the wire rendering, and the per-transaction GUC binding are owned here; `TenantContext.TenantSlot` (`rasm.tenant`) is the single canonical GUC spelling every owner reads, never a re-spelled literal — the `current_setting`/`set_config` seam, the role default, and the meter tag (`AppHost/runtime-ports`) are one symbol; the slug is admitted ONCE through `TenantSlug.Validate` and every downstream `tenant_{slug}` identifier reads the validated value, so the dynamic-identifier interpolation rides a closed vocabulary per the `Query/rail#TWO_DOOR_ROUTING` identifier law rather than a raw client string — a slug carrying SQL metacharacters or exceeding the role-name length is a `TenancyFault.UnsafeSlug` admission reject, never an interpolated injection; the audit-category consequence — a tenant-id-bearing classification binding to one `AuditCategory` — is resolved at `Version/retention#AUDIT_BINDING` and this page consumes the bound category's full `AuditCategory.Guc` six-GUC product (`pgaudit.log`, `pgaudit.log_relation`, `pgaudit.log_parameter`, `pgaudit.log_catalog`, `pgaudit.log_parameter_max_size`, `pgaudit.log_level`) as role-default GUCs — never the bare `pgaudit.log` string that would drop the other five — so the whole audit obligation provisions with the tenant role rather than at every session, `Provision` overrides `TenantQuota.Audit` with the category `AuditBinding.Bind` derives from the tenant's classifications so the provisioned set cannot diverge from the demanded one, and the category vocabulary stays single; `TenantQuota` is the per-tenant role/GUC governance set on the tenant-lifecycle fold, never a second rate-limiter — the runtime write-path enforcement reads the same bound at `Query/rail#BULK_LANE`, the connection-cap enforcement rides the role `CONNECTION LIMIT`, and the statement/idle/lock-timeout, temp-file, and work-mem bounds ride the role-default GUCs the provisioning emits, so the AppHost resource governor and this server-side bound are one declared set rather than parallel limiters; `RlsScope.Bind` is the BOUNDARY_ADMISSION ingress the `Query/transaction#TXN_SCOPE` `Transactions.Scoped` body runs as its first `ExecuteSqlAsync` statement so every tenant-scoped transaction sets the GUC before any RLS-filtered read or write, and `Adopt` is the session-default fallback for a role pinned to one tenant — the two never duplicated, one transaction-scoped (`is_local => true`) and one role-default; the `schema`/`db-per-tenant` rows project no `Policy`-phase SQL because their isolation is a connection-routing concern resolved by the host placement (the schema/database plane), not a row-level predicate, but they carry full `Apply`/`Destroy` lifecycle DDL because schema and database creation are server-tier concerns; the `db-per-tenant` plane's `CREATE DATABASE`/`DROP DATABASE` are the only statements PostgreSQL forbids inside a transaction block, so the lifecycle runner reads `TenancyModel.RunsAutocommit` (derived from `ClonesDatabase`) off the row and runs that plane's DDL on a fresh connection in autocommit OUTSIDE the `Query/transaction#TXN_SCOPE` `Transactions.Scoped` bracket, while every transactional plane (`single`/`schema`/`rls`) rides the scoped bracket whole — the autocommit boundary is a derived row signal, never a call-site `CREATE DATABASE`-string sniff; the `nullif(current_setting('rasm.tenant', true), '')::uuid` RLS binding and the `ClusterConfig.Preload` companion contract stay intact, owned at `Store/provisioning#CLUSTER_CONFIG`, and the `pg_partman` per-tenant partition provisioning rides the admitted `Store/profiles#PROVISIONING_ROWS` `pg_partman` manifest row, never a second partition owner — the per-tenant `PartitionPolicy` carries the `create_parent`/`part_config` arguments, while `run_maintenance` cadence stays the one preloaded `pg_partman_bgw` maintenance row, and `pg_squeeze` per-schema bloat reclaim stays the provisioning `squeeze.tables` owner, never re-spelled here.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

// The key IS the SQL token, so `.Key` is the rendered clause and no second projection member is needed.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class PolicyMode {
    public static readonly PolicyMode Permissive = new("PERMISSIVE");   // OR-combined base
    public static readonly PolicyMode Restrictive = new("RESTRICTIVE"); // AND-combined defense-in-depth
}

// The per-command policy scope. `USING` is the read predicate `SELECT`/`UPDATE`/`DELETE`/`ALL` honor;
// `WITH CHECK` is the write predicate `INSERT`/`UPDATE`/`ALL` honor — so `CarriesUsing`/`CarriesCheck` are the
// derived columns the CREATE POLICY projection reads to emit ONLY the clauses the command admits (an `INSERT`
// policy has no `USING`, a `SELECT` policy no `WITH CHECK`), never both unconditionally. A defense-in-depth
// table arms one permissive `ALL` base plus per-write `INSERT`/`UPDATE`/`DELETE` restrictive conjuncts off this axis.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class PolicyCommand {
    public static readonly PolicyCommand All = new("ALL", carriesUsing: true, carriesCheck: true);
    public static readonly PolicyCommand Select = new("SELECT", carriesUsing: true, carriesCheck: false);
    public static readonly PolicyCommand Insert = new("INSERT", carriesUsing: false, carriesCheck: true);
    public static readonly PolicyCommand Update = new("UPDATE", carriesUsing: true, carriesCheck: true);
    public static readonly PolicyCommand Delete = new("DELETE", carriesUsing: true, carriesCheck: false);

    public bool CarriesUsing { get; }   // SELECT/UPDATE/DELETE/ALL read predicate
    public bool CarriesCheck { get; }    // INSERT/UPDATE/ALL write predicate
}

// The per-tenant WAL-flush stance lowered to the `synchronous_commit` role default. `Default` is the
// cluster-default sentinel whose empty key the durability projection filters, so a tenant on the cluster
// default emits no SET; `.Key` IS the PG18 GUC spelling. Keyless because the value is a process-local
// role-default decision, never a wire-keyed identity.
[SmartEnum]
public sealed partial class SynchronousCommit {
    public static readonly SynchronousCommit Default = new("");
    public static readonly SynchronousCommit Off = new("off");
    public static readonly SynchronousCommit Local = new("local");
    public static readonly SynchronousCommit RemoteWrite = new("remote_write");
    public static readonly SynchronousCommit On = new("on");
    public static readonly SynchronousCommit RemoteApply = new("remote_apply");

    public string Key { get; }
    private SynchronousCommit(string key) => Key = key;
}

// The per-tenant transaction-isolation default lowered to the `default_transaction_isolation` role default.
// `Default` is the cluster-default sentinel the durability projection filters. `.Key` IS the PG18 GUC spelling.
[SmartEnum]
public sealed partial class TransactionIsolation {
    public static readonly TransactionIsolation Default = new("");
    public static readonly TransactionIsolation ReadCommitted = new("read committed");
    public static readonly TransactionIsolation RepeatableRead = new("repeatable read");
    public static readonly TransactionIsolation Serializable = new("serializable");

    public string Key { get; }
    private TransactionIsolation(string key) => Key = key;
}

// The isolation strategy axis: ONE vocabulary, no parallel `IsolationPlane` enum modeling the same closed
// set. Each row carries its plane as derived behaviour columns — whether it owns a schema, owns a role, clones
// a database, projects an RLS policy, and partitions — so the lifecycle fold reads behaviour off the row
// rather than re-deciding per call. `.Key` is the canonical wire spelling; `Switch` is the total dispatch the
// fragment fold rides so a new model breaks every site at compile time, never a silent string-tuple arm.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[KeyMemberComparer<StoreKeyPolicy, string>]
public sealed partial class TenancyModel {
    public static readonly TenancyModel Single = new("single", ownsSchema: false, ownsRole: false, clonesDatabase: false, emitsPolicy: false, partitions: false);
    public static readonly TenancyModel Schema = new("schema", ownsSchema: true, ownsRole: true, clonesDatabase: false, emitsPolicy: false, partitions: true);
    public static readonly TenancyModel Rls = new("rls", ownsSchema: false, ownsRole: true, clonesDatabase: false, emitsPolicy: true, partitions: true);
    public static readonly TenancyModel DbPerTenant = new("db-per-tenant", ownsSchema: false, ownsRole: true, clonesDatabase: true, emitsPolicy: false, partitions: false);

    public bool OwnsSchema { get; }       // CREATE SCHEMA + REVOKE FROM PUBLIC + search-path role on Apply
    public bool OwnsRole { get; }         // CREATE ROLE (no-bypassrls/no-inherit) + quota GUCs + table grants
    public bool ClonesDatabase { get; }   // CREATE DATABASE … TEMPLATE + REVOKE CONNECT FROM PUBLIC on Apply
    public bool EmitsPolicy { get; }      // only the predicate plane projects Policy-phase SQL
    public bool Partitions { get; }       // pg_partman per-tenant range partition set on Apply

    // CREATE/DROP DATABASE are the only statements PostgreSQL forbids inside a transaction block, so the
    // lifecycle runner executes the db-clone plane's DDL in autocommit; derived from ClonesDatabase, not a
    // second constructor column, because the database-clone plane is exactly the autocommit-required plane.
    public bool RunsAutocommit => ClonesDatabase;
}

// --- [MODELS] --------------------------------------------------------------------------

// The identifier admitted ONCE: a tenant slug is a Postgres role/schema/database name interpolated into DDL,
// so it rides a closed vocabulary — lowercase ASCII, leading letter, [a-z0-9_] tail, and a length leaving
// room for the `tenant_` prefix under the 63-byte NAMEDATALEN ceiling. A slug failing the gate is a
// TenancyFault.UnsafeSlug admission reject, NEVER an interpolated string, so the Query/rail#TWO_DOOR_ROUTING
// dynamic-identifier law holds without a per-call quote_ident pass. Schema/Role share the `tenant_` projection
// (a per-tenant schema and its owner role are one name); Database is its own `db_` projection so a per-tenant
// database and a per-tenant schema never collide on one identifier.
[ValueObject<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
[ValidationError<TenancyFault>]
public readonly partial struct TenantSlug {
    public const int MaxLength = 56;   // 63-byte NAMEDATALEN minus the longest `tenant_`/`db_` prefix headroom
    internal static readonly SearchValues<char> Allowed = SearchValues.Create("abcdefghijklmnopqrstuvwxyz0123456789_");

    // The slug rule IS the dynamic-identifier rule, so the gate is owned once on TenantIdentifier and the
    // generated factory delegates to it — a bad slug folds to TenancyFault.UnsafeSlug, the value passes by.
    private static Validation<TenancyFault, string> ValidateFactoryArguments(string value) =>
        TenantIdentifier.Admit(value).MapFail(static _ => (TenancyFault)new TenancyFault.UnsafeSlug(value));

    public string Schema => $"tenant_{Value}";
    public string Role => $"tenant_{Value}";
    public string Database => $"db_{Value}";
}

// The per-tenant pg_partman lifecycle as a typed product, not three frozen literals. `create_parent` declares
// the set off Control/Interval/Type; the part_config UPDATE carries Premake (future-partition horizon, the
// scheduler-downtime budget), Retention (the drop policy the Version/retention destructive gate honors),
// RetentionKeepTable (detach-vs-drop), and InfiniteTime (suppress the gap-detection abort). A new part_config
// knob is one column plus one Lower row; the maintenance cadence stays the preloaded pg_partman_bgw worker
// (Store/provisioning#PROVISIONING_ROWS), never re-spelled here.
[ComplexValueObject]
public sealed partial class PartitionPolicy {
    public string Table { get; }                // the unqualified parent relation this policy partitions (e.g. "op_log")
    public string Control { get; }              // p_control — the partition-key column (a time/serial column)
    public string Interval { get; }             // p_interval — e.g. "1 day"
    public string Strategy { get; }             // p_type — "range" | "list"
    public int Premake { get; }                 // part_config.premake — future partitions kept ahead of now
    public Option<string> Retention { get; }    // part_config.retention — e.g. "90 days"; None disables drop
    public bool RetentionKeepTable { get; }     // part_config.retention_keep_table — detach instead of drop
    public bool InfiniteTime { get; }           // part_config.infinite_time_partitions — never abort on gap
    public bool Backfill { get; }               // partition_data_proc back-fill when attaching to a non-empty parent

    public static readonly PartitionPolicy OpLogDaily = Create("op_log", "logged_at", "1 day", "range", 4, Some("90 days"), false, true, false);

    // create_parent declares the set off the policy's OWN parent relation (qualified by the plane's schema),
    // the part_config UPDATE folds the policy columns, and partition_data_proc back-fills an attach to a
    // non-empty parent (Schema/ddl#EXTENSION_DDL `partman.partition_data_proc`), so a per-tenant rollup carries
    // premake + retention + back-fill without a second partition owner and without a frozen `op_log` literal
    // buried in the fold. An empty Retention projects no retention column so the row never drops a partition the
    // policy did not declare; Backfill is opt-in so a fresh parent skips the heavy data move.
    public Seq<string> Apply(string schema) {
        var parent = $"{schema}.{Table}";
        var retention = Retention.Match(Some: r => $", retention = '{r}', retention_keep_table = {(RetentionKeepTable ? "true" : "false")}", None: static () => string.Empty);
        return Seq(
            $"SELECT partman.create_parent(p_parent_table := '{parent}', p_control := '{Control}', p_interval := '{Interval}', p_type := '{Strategy}')",
            $"UPDATE partman.part_config SET premake = {Premake}, infinite_time_partitions = {(InfiniteTime ? "true" : "false")}{retention} WHERE parent_table = '{parent}'")
            + (Backfill ? Seq($"CALL partman.partition_data_proc(p_parent_table := '{parent}')") : Seq<string>());
    }
}

// The full per-tenant role-and-GUC governance set — every bound is a Duration-typed, sized, or category
// quantity the Gucs projection lowers to one `ALTER ROLE … SET …` row, so the quota is a deploy-time role
// constraint the role itself enforces, never a runtime branch. Every column is an ACTUAL PG18 `ALTER ROLE …`
// clause or settable GUC: `temp_file_limit`/`work_mem`/`maintenance_work_mem` bound the per-session resource
// footprint, the four timeouts bound runtime liveness, `synchronous_commit`/`default_transaction_isolation`/
// `default_transaction_read_only`/`default_transaction_deferrable` set the per-tenant durability+isolation
// stance (a read-replica tenant rides `read_only = on`, a serializable tenant rides the isolation GUC), and
// `Audit` is the per-tenant bound `AuditCategory` — Version/retention#AUDIT_BINDING resolves the tenant's
// present classifications to ONE `AuditCategory`, and `Audit.Guc` is the FULL six-GUC pgaudit product
// (`pgaudit.log`, `pgaudit.log_relation`, `pgaudit.log_parameter`, `pgaudit.log_catalog`,
// `pgaudit.log_parameter_max_size`, `pgaudit.log_level`), so TenantProvision lowers the WHOLE bound obligation
// as role-default GUCs — never the bare `pgaudit.log` class string alone, which would silently drop the
// relation/parameter/catalog/max-size/level GUCs the category carries. `Provision` OVERRIDES this field with
// the category `AuditBinding.Bind` derives from the tenant's classifications so the provisioned obligation
// cannot diverge from the demanded one. The write-byte-rate bound is NOT a column here — PostgreSQL has no
// per-role write-rate GUC, so that bound lives wholly on the Query/rail#BULK_LANE COPY rail it gates, never a
// phantom role clause. A new resource bound is one column plus one Gucs row; an empty string-valued GUC
// projects no row, so a tenant without a work-mem floor emits no blank SET, and a default-valued
// isolation/durability stance is filtered the same way.
[ComplexValueObject]
public sealed partial class TenantQuota {
    public int ConnectionLimit { get; }                  // ALTER ROLE … CONNECTION LIMIT
    public Duration StatementTimeout { get; }            // statement_timeout
    public Duration IdleInTxnTimeout { get; }            // idle_in_transaction_session_timeout
    public Duration IdleSessionTimeout { get; }          // idle_session_timeout (idle outside a txn)
    public Duration LockTimeout { get; }                 // lock_timeout
    public long TempFileLimitKb { get; }                 // temp_file_limit (-1 unlimited)
    public string WorkMem { get; }                       // work_mem floor, e.g. "4MB"
    public string MaintenanceWorkMem { get; }            // maintenance_work_mem — index/vacuum footprint, e.g. "64MB"
    public TenantDurability Durability { get; }          // synchronous_commit + isolation/read-only/deferrable stance
    public AuditCategory Audit { get; }                  // full pgaudit GUC product, bound from AuditBinding (Version/retention)
    public Option<Instant> ValidUntil { get; }           // ALTER ROLE … VALID UNTIL role expiry

    static string Ms(Duration d) => ((long)d.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);

    // The role-default GUC product — every row is a (setting, value) pair the lifecycle fold lowers to one
    // `ALTER ROLE {role} SET {setting} = '{value}'`. VALID UNTIL is a role clause, not a SET GUC, so it rides
    // the role-create step instead of this projection; an empty string-valued GUC is filtered so a tenant
    // without a work-mem floor emits no blank SET, the durability stance contributes only its non-default rows
    // through TenantDurability.Gucs, and the FULL bound `AuditCategory.Guc` product (all six pgaudit GUCs) folds
    // in so the per-tenant role carries the complete audit obligation, never a single `pgaudit.log` string.
    public Seq<(string Setting, string Value)> Gucs =>
        (Seq(
            ("statement_timeout", Ms(StatementTimeout)),
            ("idle_in_transaction_session_timeout", Ms(IdleInTxnTimeout)),
            ("idle_session_timeout", Ms(IdleSessionTimeout)),
            ("lock_timeout", Ms(LockTimeout)),
            ("temp_file_limit", TempFileLimitKb.ToString(CultureInfo.InvariantCulture)),
            ("work_mem", WorkMem),
            ("maintenance_work_mem", MaintenanceWorkMem))
        + Durability.Gucs
        + Audit.Guc)
        .Filter(static row => row.Item2.Length > 0);
}

// The per-tenant durability and transaction stance lowered to role-default GUCs. `synchronous_commit` is the
// per-tenant WAL-flush stance (`off`/`local`/`remote_write`/`on`/`remote_apply` — a low-classification tenant
// trades a crash-window for throughput by riding `local`), `Isolation` defaults the tenant's transaction
// isolation (`SERIALIZABLE` for a tenant the federation rail proves needs SSI), and the read-only/deferrable
// pair routes a reporting tenant to a deferrable read-only default. A `default`-keyed member projects no GUC,
// so a tenant on the cluster default emits no SET. The keys ARE the PG18 GUC spelling so `.Key` is the value
// the role-default SET writes with no second projection.
[ComplexValueObject]
public sealed partial class TenantDurability {
    public SynchronousCommit Commit { get; }             // synchronous_commit
    public TransactionIsolation Isolation { get; }       // default_transaction_isolation
    public bool ReadOnly { get; }                        // default_transaction_read_only
    public bool Deferrable { get; }                      // default_transaction_deferrable (serializable read-only)

    public static readonly TenantDurability ClusterDefault = Create(SynchronousCommit.Default, TransactionIsolation.Default, false, false);

    public Seq<(string Setting, string Value)> Gucs => Seq(
            ("synchronous_commit", Commit.Key),
            ("default_transaction_isolation", Isolation.Key),
            ("default_transaction_read_only", ReadOnly ? "on" : string.Empty),
            ("default_transaction_deferrable", Deferrable ? "on" : string.Empty))
        .Filter(static row => row.Item2.Length > 0);
}

// The full CREATE POLICY product, not a bare USING predicate. The shared tenant `Predicate` becomes the USING
// read clause and/or the WITH CHECK write clause per the `Command` axis (a tenant cannot read or write a row
// tagged with another tenant's id), `Mode` composes permissive (OR base) vs restrictive (AND defense-in-depth),
// `Command` scopes the policy per FOR clause, `Grantee` binds TO role. ENABLE/FORCE is a TABLE setting, not a
// per-policy one, so it is emitted ONCE through `Arm`, never per policy. ALL four interpolated identifiers —
// `Table`, the per-policy `Name`, `TenantColumn`, and `Grantee` — cross the closed-vocabulary `Admit` gate
// (the schema/migration owner supplies them, never raw client text), so every identifier rides the same one
// seam the slug rides, none reaching the DDL string unvalidated.
[ComplexValueObject]
public sealed partial class RlsPolicy {
    public string Table { get; }
    public string Name { get; }   // the per-policy name (`{table}_tenant`, `{table}_ins`, `{table}_upd`, `{table}_del`)
    public string TenantColumn { get; }
    public PolicyCommand Command { get; }
    public PolicyMode Mode { get; }
    public string Grantee { get; }

    // The defense-in-depth set: a PERMISSIVE base FOR ALL the tenant's rows, then RESTRICTIVE write conjuncts
    // scoped per write command — `INSERT` (`WITH CHECK`), `UPDATE` (`USING` + `WITH CHECK`), AND `DELETE`
    // (`USING`) — that forbid a row escaping the tenant predicate on any write path even if a later permissive
    // policy is added, so the restrictive layer covers EVERY mutating verb the engine exposes (INSERT/UPDATE/
    // DELETE), never a partial set leaving DELETE governed only by the permissive base. A read-only tenant
    // supplies only the permissive base; a write tenant adds all three restrictive write conjuncts, so the full
    // `PolicyCommand` write axis is load-bearing, never a decorative field carrying an unproduced `Delete` case.
    // Modeled as a Seq because an `rls`-plane table arms several policies, never one policy pretending to be all.
    public static Seq<RlsPolicy> Set(string table, string tenantColumn, string grantee, bool writable) =>
        Seq(Create(table, $"{table}_tenant", tenantColumn, PolicyCommand.All, PolicyMode.Permissive, grantee))
        + (writable
            ? Seq(Create(table, $"{table}_ins", tenantColumn, PolicyCommand.Insert, PolicyMode.Restrictive, grantee),
                  Create(table, $"{table}_upd", tenantColumn, PolicyCommand.Update, PolicyMode.Restrictive, grantee),
                  Create(table, $"{table}_del", tenantColumn, PolicyCommand.Delete, PolicyMode.Restrictive, grantee))
            : Seq<RlsPolicy>());

    // The two-arg `current_setting(name, missing_ok => true)` form is load-bearing: the one-arg form RAISES
    // `unrecognized configuration parameter` when the GUC is unset (an unbound session), so the policy would
    // ERROR rather than filter; `nullif(…, '')::uuid` lifts the unset/empty GUC to SQL NULL, and `col = NULL`
    // is NULL — never true — so an unbound session reads ZERO rows (fail-closed) instead of crashing. This is
    // the canonical RLS tenant predicate; the one-arg spelling is the illusory fail-open form the page rejects.
    string Predicate => $"{TenantColumn} = nullif(current_setting('{TenantContext.TenantSlot}', true), '')::uuid";

    // ENABLE arms the policy machinery and FORCE extends it to the table owner — table-level, emitted once per
    // table by the lifecycle fold, never per policy in the set.
    public static Seq<string> Arm(string table) => Seq(
        $"ALTER TABLE {table} ENABLE ROW LEVEL SECURITY",
        $"ALTER TABLE {table} FORCE ROW LEVEL SECURITY");

    // The CREATE POLICY binds the clauses the COMMAND admits — `USING` for read commands, `WITH CHECK` for
    // write commands — off the command's derived columns, never both unconditionally (an `INSERT` policy with a
    // `USING` clause is a syntax error, a `SELECT` policy with `WITH CHECK` is meaningless), so the projection
    // reads the command axis rather than ignoring it; arming is the table's, not this policy's.
    public string Create() =>
        $"CREATE POLICY {Name} ON {Table} AS {Mode.Key} FOR {Command.Key} TO {Grantee}"
        + (Command.CarriesUsing ? $" USING ({Predicate})" : string.Empty)
        + (Command.CarriesCheck ? $" WITH CHECK ({Predicate})" : string.Empty);

    public string Drop() => $"DROP POLICY IF EXISTS {Name} ON {Table}";

    // Every identifier the CREATE/DROP interpolates — table, the per-policy NAME, the tenant COLUMN, and the
    // grantee role — crosses the same closed-vocabulary admission ONCE, accumulating every unsafe identifier in
    // one applicative pass. v5 `Validation` exposes only the curried single-argument applicative
    // `Apply<F,A,B>(K<Validation<F>,Func<A,B>>, K<Validation<F>,A>) where F : Semigroup<F>`, so the four
    // admissions chain through a `curry`d constructor `Map`ped onto the first then peeled one arg per `Apply` —
    // each `Apply` combines the fail-side through `TenancyFault.Combine`, so a request with a bad NAME AND a bad
    // GRANTEE folds both into one `Aggregate`. The arms are discarded (the value already lives on `this`).
    public Validation<TenancyFault, RlsPolicy> Admit() =>
        TenantIdentifier.Admit(Table)
            .Map(curry((string _, string _, string _, string _) => this))
            .Apply(TenantIdentifier.Admit(Name))
            .Apply(TenantIdentifier.Admit(TenantColumn))
            .Apply(TenantIdentifier.Admit(Grantee))
            .As();
}

// The typed lifecycle evidence — never a generic IReceipt. `Facts` folds the proof rows the provisioning
// verifier consumes onto the ONE settled `StoreFact.Maintain` kind the closed `Query/rail#STORE_FACT`
// vocabulary already carries: the tenant-provisioning variant lands in the `Subject` slot (`policy:…`,
// `quota:…`, `audit:…`), NEVER a new dotted `tenant.policy.applied`/`tenant.quota.applied`/`tenant.audit.bound`
// kind constant the closed `StoreFact` vocabulary does not define — a parallel fact kind is the deleted form
// the `Query/rail#STORE_FACT` and `Query/transaction#TXN_SCOPE` owners reject. One row per armed policy carries
// its `CREATE POLICY` text, one per role-default GUC `(setting, value)` pair, and one carries the bound
// `AuditCategory` GUC product, so a tenant-isolation mismatch, an unsafe slug, or a quota-set failure is a
// typed `TenancyFault` and a SUCCESSFUL provision is reproducible proof rows rather than an opaque ok.
public sealed record TenantReceipt(
    TenantId Tenant, TenantSlug Slug, TenancyModel Model, Seq<RlsPolicy> Policies,
    TenantQuota Quota, PartitionPolicy Partition, AuditCategory Audit,
    Seq<string> Applied, CorrelationId Correlation, Instant At) {

    public Seq<StoreFact> Facts(Duration elapsed) =>
        Policies.Map(p => new StoreFact(StoreFact.Maintain, $"tenant.policy:{p.Create()}", 1L, elapsed, At))
        + Quota.Gucs.Map(g => new StoreFact(StoreFact.Maintain, $"tenant.quota:{g.Setting}={g.Value}", 1L, elapsed, At))
        + Seq(new StoreFact(StoreFact.Maintain, $"tenant.audit:{Audit.Log}", (long)Audit.Guc.Count, elapsed, At));
}

[Union]
public abstract partial record TenantPhase {
    public sealed partial record Apply : TenantPhase;                            // create the per-model objects
    public sealed partial record Destroy : TenantPhase;                          // idempotent DROP … IF EXISTS teardown
    public sealed partial record Policy(Seq<RlsPolicy> Policies) : TenantPhase;  // the RLS CREATE/DROP POLICY set
    public sealed partial record Bind : TenantPhase;                             // per-transaction SET LOCAL GUC ingress
    public sealed partial record Adopt : TenantPhase;                            // role-default ALTER ROLE … SET adoption
}

// --- [ERRORS] --------------------------------------------------------------------------

// A closed fault family deriving from Expected, implementing IValidationError<TenancyFault> for the generated
// [ValidationError] admission AND Semigroup<TenancyFault> because LanguageExt v5 Validation<F,A> resolves the
// F-combination trait ad-hoc at run time and throws TypeLoadException unless the CONCRETE F derives from
// Semigroup<F> (Error's own Monoid<Error> does not satisfy Semigroup<TenancyFault> — the trait is on the
// concrete type). The applicative pass over the slug + every policy identifier therefore accumulates typed
// members under one Aggregate case rather than flattening to a string. Recovery is code-keyed via
// Is/HasCode/IsType<E>, never `==`.
[Union]
public abstract partial record TenancyFault : Expected, IValidationError<TenancyFault>, Semigroup<TenancyFault> {
    private TenancyFault(string detail, int code) : base(detail, code, None) { }
    public static TenancyFault Create(string message) => new Text(message);

    public sealed record Text : TenancyFault { public Text(string detail) : base(detail, 8270) { } }
    public sealed record UnsafeSlug : TenancyFault { public UnsafeSlug(string slug) : base($"<unsafe-tenant-slug:{slug}>", 8271) { Slug = slug; } public string Slug { get; } }
    public sealed record UnsafeIdentifier : TenancyFault { public UnsafeIdentifier(string role, string identifier) : base($"<unsafe-identifier:{role}:{identifier}>", 8272) { Role = role; Identifier = identifier; } public string Role { get; } public string Identifier { get; } }
    public sealed record PolicyAbsent : TenancyFault { public PolicyAbsent(string table) : base($"<rls-policy-absent:{table}>", 8273) { Table = table; } public string Table { get; } }
    public sealed record QuotaUnset : TenancyFault { public QuotaUnset(string setting) : base($"<tenant-quota-unset:{setting}>", 8274) { Setting = setting; } public string Setting { get; } }
    public sealed record Aggregate : TenancyFault { public Aggregate(Seq<TenancyFault> faults) : base($"{faults.Count} tenancy faults", 8299) => Faults = faults; public Seq<TenancyFault> Faults { get; } }

    public TenancyFault Combine(TenancyFault rhs) => (this, rhs) switch {
        (Aggregate l, Aggregate r) => new Aggregate(l.Faults + r.Faults),
        (Aggregate l, _) => new Aggregate(l.Faults.Add(rhs)),
        (_, Aggregate r) => new Aggregate(this.Cons(r.Faults)),
        _ => new Aggregate(Seq(this, rhs)),
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// The canonical tenant-id wire form, an extension member on the AppHost-owned `TenantId` value object rather
// than a one-method static utility class — the projection attaches at THIS stratum without leaking a method
// onto the upstream owner. TenantContext.TenantId is a [ValueObject<UInt128>], but every reader in the package
// — this page's RLS predicate, Query/federation#ENTITY_GRAPH's `Tenant uuid` column, Sync/coordination#FENCED_CAS
// — casts `current_setting('rasm.tenant')::uuid`. A 128-bit value is exactly a 16-byte uuid (the same
// transcription Schema/identity#IDENTITY_AXIS uses for content keys), so the id renders to its uuid text ONCE
// here and the ingress, the role default, and the predicate read one settled spelling — never the decimal
// `UInt128` that a `::uuid` cast rejects. The `stackalloc byte[16]` span kernel is this projection's named
// statement exemption (a measured fixed-width buffer, never a heap allocation per render).
public static class TenantWire {
    extension(TenantId id) {
        public string Uuid {
            get {
                Span<byte> bytes = stackalloc byte[16];
                BinaryPrimitives.WriteUInt128BigEndian(bytes, id.Value);
                return new Guid(bytes, bigEndian: true).ToString();
            }
        }
    }
}

// The dynamic-identifier admission gate: every identifier that interpolates into DDL (the slug, the policy
// table/column/grantee) crosses ONCE through the same closed-vocabulary rule. `Admit` is the one bridge —
// it accumulates EVERY unsafe identifier in one Validation pass rather than aborting on the first, so a
// provisioning request carrying a bad slug AND a bad grantee reports both faults at admission.
public static class TenantIdentifier {
    static readonly SearchValues<char> Allowed = TenantSlug.Allowed;

    public static Validation<TenancyFault, string> Admit(string identifier) =>
        identifier is { Length: > 0 and <= TenantSlug.MaxLength } && char.IsAsciiLetterLower(identifier[0]) && !identifier.AsSpan().ContainsAnyExcept(Allowed)
            ? Success<TenancyFault, string>(identifier)
            : Fail<TenancyFault, string>(new TenancyFault.UnsafeIdentifier("identifier", identifier));
}

public static class TenantProvision {
    // The one lifecycle projection over the total generated `TenantPhase.Switch` — a sixth phase case breaks
    // EVERY arm at compile time, never a silent `_` the page's create/teardown matrix would skip. Each phase
    // admits exactly the identifiers it interpolates: the `policy` arm admits EVERY identifier of EVERY policy
    // (table, per-policy name, tenant column, grantee) in one accumulating `RlsPolicy.Admit` pass, while the four
    // slug-bearing arms route through the shared `Slugged` continuation that admits the slug once through the
    // generated `TenantSlug.Validate` bridge BEFORE any interpolation, so no raw client string reaches the DDL.
    // `policy` needs no slug (it interpolates the policy's own admitted identifiers); on a non-policy plane it is
    // a legitimate empty projection (the schema/db isolation is a connection-routing concern), never a hole.
    public static Validation<TenancyFault, Seq<string>> Project(TenancyModel model, TenantPhase phase, TenantContext tenant, TenantQuota quota, PartitionPolicy partition) =>
        phase.Switch(
            policy: p => model.EmitsPolicy
                ? p.Policies.Traverse(static rls => rls.Admit()).As().Map(static held => Armed(held))
                : Success<TenancyFault, Seq<string>>(Seq<string>()),
            apply: _ => Slugged(tenant, slug => Fragments(model, apply: true, slug, tenant.TenantId, quota, partition)),
            destroy: _ => Slugged(tenant, slug => Fragments(model, apply: false, slug, tenant.TenantId, quota, partition)),
            bind: _ => Slugged(tenant, _ => Seq($"select set_config('{TenantContext.TenantSlot}', '{tenant.TenantId.Uuid}', is_local => true)")),
            adopt: _ => Slugged(tenant, slug => Seq($"ALTER ROLE {slug.Role} SET {TenantContext.TenantSlot} = '{tenant.TenantId.Uuid}'")));

    static Validation<TenancyFault, Seq<string>> Slugged(TenantContext tenant, Func<TenantSlug, Seq<string>> project) =>
        Admit(tenant.Slug).Map(project);

    static Validation<TenancyFault, TenantSlug> Admit(string slug) =>
        TenantSlug.Validate(slug, null, out var owned) is { } fault ? Fail<TenancyFault, TenantSlug>(fault) : Success<TenancyFault, TenantSlug>(owned!);

    // The receipt-minting lifecycle owner — the one entry that runs an Apply provision end to end and folds its
    // applied DDL plus its policy/quota/audit proof into the typed TenantReceipt. The `Apply` DDL, the `Policy`
    // arming, and the slug admit INDEPENDENTLY (a bad slug AND a bad policy identifier accumulate in one pass),
    // so the three projections compose through the v5 curried single-argument applicative `Apply` chain — a
    // `curry`d `TenantReceipt` constructor `Map`ped onto the `Apply` projection then peeled one carrier per
    // `.Apply`, every step combining the fail-side through `TenancyFault.Combine` (the `&` collect-into-`Seq`
    // operator is the wrong shape here: it yields `Validation<F,Seq<A>>`, never the projected receipt). The
    // bound `AuditCategory` is derived ONCE from
    // the tenant's present classifications and OVERRIDES `quota.Audit`, so the role-default pgaudit GUCs the
    // Apply fold emits and the category the receipt records are the SAME value — the provisioned obligation can
    // never diverge from the demanded one, and a caller-supplied audit category is canonically replaced rather
    // than trusted. A consumer that only needs the raw DDL calls `Project`; a consumer provisioning a tenant
    // calls `Provision` and gets reproducible evidence, so the receipt is genuinely minted, never declared-but-
    // unproduced. The governed quota threads through BOTH the Apply role-GUC fold and the receipt.
    public static Validation<TenancyFault, TenantReceipt> Provision(
        TenancyModel model, TenantContext tenant, TenantQuota quota, PartitionPolicy partition,
        Seq<RlsPolicy> policies, Seq<DataClassification> classifications, CorrelationId correlation, Instant at) {
        var bound = AuditBinding.Bind(classifications);
        var governed = TenantQuota.Create(quota.ConnectionLimit, quota.StatementTimeout, quota.IdleInTxnTimeout,
            quota.IdleSessionTimeout, quota.LockTimeout, quota.TempFileLimitKb, quota.WorkMem, quota.MaintenanceWorkMem,
            quota.Durability, bound, quota.ValidUntil);
        return Project(model, new TenantPhase.Apply(), tenant, governed, partition)
            .Map(curry((Seq<string> apply, Seq<string> armed, TenantSlug slug) => new TenantReceipt(
                tenant.TenantId, slug, model, model.EmitsPolicy ? policies : Seq<RlsPolicy>(), governed, partition,
                bound, apply + armed, correlation, at)))
            .Apply(Project(model, new TenantPhase.Policy(policies), tenant, governed, partition))
            .Apply(Admit(tenant.Slug))
            .As();
    }

    // Table-level arming once per distinct table, then one CREATE POLICY per policy in the admitted set — never
    // ENABLE/FORCE per policy.
    static Seq<string> Armed(Seq<RlsPolicy> policies) =>
        policies.Map(static p => p.Table).Distinct().Bind(RlsPolicy.Arm)
            + policies.Map(static p => p.Create());

    // The create/teardown matrix driven by the model's behaviour columns under the generated Switch, never a
    // string-tuple `switch` with a silent `_`. Apply composes the plane-specific objects (schema, role,
    // grants, database, partition set) off the model's columns; Destroy mirrors each with its idempotent DROP.
    // The role is minted with the full deny-attribute set — NOBYPASSRLS is the load-bearing one: a tenant role
    // that could bypass RLS defeats the whole predicate. A LOGIN role with no GRANT is hollow, so the schema
    // step REVOKEs FROM PUBLIC then GRANTs the tenant schema's privileges to the role. The `db-per-tenant`
    // plane's `CREATE DATABASE`/`DROP DATABASE` are the ONLY statements PostgreSQL forbids inside a transaction
    // block, so the lifecycle runner reads `model.RunsAutocommit` off the row and executes those steps in
    // autocommit (a fresh connection outside `Query/transaction#TXN_SCOPE`'s `Transactions.Scoped` bracket);
    // every other plane's DDL is transactional and rides the scoped bracket whole.
    static Seq<string> Fragments(TenancyModel model, bool apply, TenantSlug slug, TenantId id, TenantQuota quota, PartitionPolicy partition) {
        var uuid = id.Uuid;
        var quotaGucs = quota.Gucs.Map(g => $"ALTER ROLE {slug.Role} SET {g.Setting} = '{g.Value}'");
        var validUntil = quota.ValidUntil.Match(Some: at => $" VALID UNTIL '{at}'", None: static () => string.Empty);
        var roleApply = Seq($"CREATE ROLE {slug.Role} WITH LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE NOBYPASSRLS NOINHERIT NOREPLICATION CONNECTION LIMIT {quota.ConnectionLimit}{validUntil}") + quotaGucs;
        // pg_partman parent: the schema-plane parent rides the per-tenant schema, the rls-plane parent the
        // shared `public` schema partitioned by the per-tenant tag's time column. The PartitionPolicy carries
        // the parent relation name AND control/interval/premake/retention, so the fold passes only the plane's
        // schema and the policy resolves its own qualified parent — no frozen `op_log` literal here.
        var partman = partition.Apply(model.OwnsSchema ? slug.Schema : "public");
        return model.Switch(
            single: () => Seq<string>(),
            schema: () => apply
                ? Seq($"CREATE SCHEMA IF NOT EXISTS {slug.Schema}") + roleApply
                    + Seq($"REVOKE ALL ON SCHEMA {slug.Schema} FROM PUBLIC",
                          $"REVOKE ALL ON ALL TABLES IN SCHEMA {slug.Schema} FROM PUBLIC",
                          $"ALTER ROLE {slug.Role} SET search_path = {slug.Schema}",
                          $"GRANT USAGE ON SCHEMA {slug.Schema} TO {slug.Role}",
                          $"GRANT ALL ON ALL TABLES IN SCHEMA {slug.Schema} TO {slug.Role}",
                          $"ALTER DEFAULT PRIVILEGES IN SCHEMA {slug.Schema} GRANT ALL ON TABLES TO {slug.Role}")
                    + partman
                : Seq($"DROP SCHEMA IF EXISTS {slug.Schema} CASCADE", $"DROP ROLE IF EXISTS {slug.Role}"),
            rls: () => apply
                ? roleApply + Seq($"ALTER ROLE {slug.Role} SET {TenantContext.TenantSlot} = '{uuid}'") + partman
                : Seq($"DROP ROLE IF EXISTS {slug.Role}"),
            dbPerTenant: () => apply
                ? roleApply + Seq(
                    $"CREATE DATABASE {slug.Database} TEMPLATE rasm_template OWNER {slug.Role} CONNECTION LIMIT {quota.ConnectionLimit}",
                    $"REVOKE CONNECT ON DATABASE {slug.Database} FROM PUBLIC",
                    $"GRANT CONNECT ON DATABASE {slug.Database} TO {slug.Role}")
                : Seq($"DROP DATABASE IF EXISTS {slug.Database} WITH (FORCE)", $"DROP ROLE IF EXISTS {slug.Role}"));
    }
}

// The per-transaction GUC ingress (BOUNDARY_ADMISSION) and the runtime egress of the one GUC-binding
// statement Project's `Bind`/`Adopt` phases project as deploy-script text. The two lowerings are NOT
// duplicates: the deploy form emits literal script text (a role default or a migration statement that takes no
// bound parameter), the runtime form here binds the TenantSlot name and the rendered uuid as real set_config
// arguments through ExecuteSqlAsync's FormattableString parameterization, so the runtime path never
// literal-interpolates the tenant id. The set_config value is the uuid text (tenant.TenantId.Uuid), so the
// current_setting('rasm.tenant', true)::uuid predicate resolves — a decimal UInt128 here would fail the cast.
// The Query/transaction#TXN_SCOPE Transactions.Scoped body runs Bind as its first statement so
// current_setting('rasm.tenant') resolves for every RLS-filtered read and write in the transaction; the
// `true` third argument is `is_local` (transaction-scoped) so pool-return cannot leak the binding. The
// cancellation token rides the `EnvIO` carrier per the rails law (a `CancellationToken` never sits on a
// domain signature), so the bind composes with any deadline/retry aspect the transaction owner stacks. The
// `ExecuteSqlAsync(FormattableString, …)` overload is the parameterizing form (the GUC name and uuid become
// `$1`/`$2`), NEVER the `ExecuteSqlRawAsync` raw-string form a tenant id must never reach.
public static class RlsScope {
    public static IO<Unit> Bind(DbContext context, TenantContext tenant) =>
        IO.liftVAsync(async (EnvIO env) => {
            _ = await context.Database.ExecuteSqlAsync(
                $"SELECT set_config({TenantContext.TenantSlot}, {tenant.TenantId.Uuid}, true)", env.Token);
            return unit;
        });
}
```

## [03]-[RESEARCH]

- [RLS_POLICY_PROBE]: the live-PG18 `CREATE POLICY … AS PERMISSIVE FOR ALL TO tenant_role USING (tenant_id = nullif(current_setting('rasm.tenant', true), '')::uuid) WITH CHECK (…)` round-trip against a per-transaction `set_config('rasm.tenant', <uuid-text>, true)` binding and a per-role `ALTER ROLE … SET rasm.tenant` default — whether the `USING` predicate filters a foreign tenant's rows on read AND the `WITH CHECK` predicate rejects a cross-tenant write, whether the two-arg `current_setting(name, true)` + `nullif(…, '')::uuid` form reads ZERO rows on an UNBOUND session (fail-closed) rather than RAISING `unrecognized configuration parameter` the one-arg spelling throws, the `ENABLE` + `FORCE ROW LEVEL SECURITY` pair forces the predicate on every read including the table owner, a `NOBYPASSRLS` tenant role cannot escape the predicate, and a `RESTRICTIVE` policy composes as an AND-conjunction over the permissive base, proven against a live PG18 server before the `rls` lifecycle fence pins; the `set_config(..., is_local => true)` transaction-scoped GUC versus the role-default GUC and the pool-return leak boundary confirmed on the same server, and the `TenantId.Uuid` rendering confirmed equal to the `Guid` form Npgsql binds for the `uuid` tenant column.
- [TENANT_QUOTA_APPLY]: the per-tenant `ALTER ROLE … CONNECTION LIMIT … VALID UNTIL` plus the role-default `statement_timeout`/`idle_in_transaction_session_timeout`/`idle_session_timeout`/`lock_timeout`/`temp_file_limit`/`work_mem`/`maintenance_work_mem` GUC application, the full bound `AuditCategory.Guc` six-row pgaudit set (`pgaudit.log`/`pgaudit.log_relation`/`pgaudit.log_parameter`/`pgaudit.log_catalog`/`pgaudit.log_parameter_max_size`/`pgaudit.log_level`) as role defaults, and the `db-per-tenant` `CREATE DATABASE … TEMPLATE rasm_template` clone run in autocommit with its `REVOKE CONNECT … FROM PUBLIC` — whether the role connection cap, the timeout/temp/work-mem/audit GUCs, and the role expiry bind as server-side limits the runtime write path reads rather than runtime branches, whether the deny-attribute role posture (`NOBYPASSRLS NOINHERIT`) holds, whether a `CREATE DATABASE` outside a transaction block succeeds where one inside fails with `25001`, and whether the template clone carries the seeded schema, confirmed against the Forge-provisioned PG18 before the quota fence pins.
- [PARTMAN_PER_TENANT]: the `partman.create_parent(p_parent_table => 'tenant_{slug}.op_log', p_interval => '1 day')` plus the `part_config` `premake`/`retention`/`retention_keep_table`/`infinite_time_partitions` UPDATE for the `schema`/`rls` planes — whether the admitted `pg_partman` bgworker provisions a per-tenant partitioned op-log rollup honoring the `PartitionPolicy` retention, whether `partition_data_proc` back-fills existing rows when the policy attaches to a non-empty parent, and whether `run_maintenance` advances every tenant's partition horizon from the one `Store/profiles#PROVISIONING_ROWS` maintenance row, proven against the preloaded `pg_partman` before the partition step pins.
