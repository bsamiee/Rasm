# [PERSISTENCE_TENANCY]

`Rasm.Persistence` owns the multi-tenancy and row-level-security axis on the self-provisioned PostgreSQL 18.4 server tier as ONE closed lifecycle: `TenancyModel` is the `[SmartEnum<string>]` isolation-strategy axis carrying per-row capability columns (the predicate/schema/database isolation plane and the partition strategy); `TenantSlug` is the `[ValueObject<string>]` identifier admitted EXACTLY ONCE so every interpolated DDL identifier rides a closed, validated vocabulary rather than a raw client string; `RlsPolicy` is the `[ComplexValueObject]` carrying the full `CREATE POLICY` product (command, permissive/restrictive composition, grantee role, the `USING` read predicate and the `WITH CHECK` write predicate, the `FORCE` flag); `TenantQuota` is the `[ComplexValueObject]` carrying the full per-tenant role-and-GUC governance set the write path and the role constraint enforce; `RlsScope` is the per-transaction `set_config('rasm.tenant', …, is_local => true)` ingress that binds the session GUC the RLS predicate reads; `TenantProvision` is the one polymorphic projection folding the `TenancyModel × TenantPhase` matrix through a frozen data table into `Validation<TenancyFault, Seq<string>>`; `TenancyFault` is the closed `[Union]` deriving from `Expected`; and `TenantReceipt` is the typed lifecycle evidence. PostgreSQL with RLS is the same `Store/profiles#PROFILE_AXIS` `PostgresServer` profile — never a forked store family and never a new engine. The page spine is Npgsql, Thinktecture vocabulary, LanguageExt rails, NodaTime, and the admitted `pg_partman` per-tenant partition maintenance; the deploy-side cluster-GUC verification is the sibling `Store/provisioning#CLUSTER_CONFIG` owner.

Wire posture: this page is host-local — every owner emits raw tenant-lifecycle SQL executed server-side. The tenancy primitive it consumes (`TenantContext`) crosses the wire only as `TenantContextWire` owned at `AppHost/runtime-ports#TS_PROJECTION`; this page reads `TenantId` as the `current_setting('rasm.tenant')::uuid` RLS predicate, binds that same GUC per transaction through `RlsScope`, and never mints a client-facing projection.

## [01]-[INDEX]

- [01]-[TENANCY_RLS]: multi-tenancy axis, identifier admission, the full RLS policy product, per-tenant role/GUC quota, the per-transaction GUC-binding ingress, the lifecycle fold, and the typed fault family.

## [02]-[TENANCY_RLS]

- Owner: `TenancyModel` the multi-tenancy `[SmartEnum<string>]` axis under the `StoreKeyPolicy` ordinal comparer accessor, each row carrying its isolation plane (`IsolationPlane.Predicate`/`Schema`/`Database`) and partition strategy; `TenantSlug` the `[ValueObject<string>]` validated identifier admitting the raw slug ONCE so every DDL identifier is closed-vocabulary; `RlsPolicy` the `[ComplexValueObject]` `CREATE POLICY` product; `TenantQuota` the `[ComplexValueObject]` per-tenant role-and-GUC governance set; `RlsScope` the per-transaction `set_config` GUC ingress; `TenantProvision` the one polymorphic projection over the `TenancyModel × TenantPhase` matrix; `TenancyFault` the closed fault family; `TenantReceipt` the lifecycle evidence.
- Cases: `TenancyModel` `single` (no isolation, the shared row) | `schema` (per-tenant schema + role + search-path) | `rls` (one shared table, predicate isolation by the session GUC) | `db-per-tenant` (a template-cloned database); `TenantPhase` `Apply` (create the per-model objects) | `Destroy` (idempotent teardown) | `Policy` (the RLS `CREATE/DROP POLICY` pair) | `Bind` (the per-transaction `SET LOCAL` GUC ingress) | `Adopt` (a session-default `ALTER ROLE … SET` adoption); `TenantQuota` carries the connection cap, the statement/idle/lock timeouts, the `temp_file_limit`, the `work_mem` floor, the role `VALID UNTIL` expiry, and the write-byte-rate bound; `RlsPolicy` carries the command, permissive/restrictive mode, grantee role, the `USING` and `WITH CHECK` predicates, and the `FORCE` flag.
- Entry: `public static Validation<TenancyFault, Seq<string>> Project(TenancyModel model, TenantPhase phase, TenantContext tenant, TenantQuota quota)` is the one lifecycle fold; the slug admits through `TenantSlug.Validate` BEFORE any identifier interpolation so an unsafe slug accumulates a `TenancyFault.UnsafeSlug` rather than reaching the DDL string, and the `(model, phase)` pair selects its DDL fragment from the frozen `Fragments` table — `Apply` folds the per-model create DDL (schema/role/GUC/partition steps keyed on the admitted `TenantSlug` and the `TenantId` and the `TenantQuota` GUCs), `Destroy` folds the idempotent `DROP … IF EXISTS` teardown, `Policy` projects the `rls`-row `CREATE POLICY` pair (every other row projects the empty sequence), `Bind` projects the single per-transaction `select set_config('rasm.tenant', …, true)` statement, and `Adopt` projects the role-default `ALTER ROLE … SET rasm.tenant`.
- Auto: the `rls` row emits `ALTER TABLE … ENABLE ROW LEVEL SECURITY`, `ALTER TABLE … FORCE ROW LEVEL SECURITY` (so the table owner is filtered too — the page-level isolation guarantee a bare `ENABLE` does not give), and a `CREATE POLICY … AS PERMISSIVE FOR ALL TO tenant_role USING (tenant_id = current_setting('rasm.tenant')::uuid) WITH CHECK (tenant_id = current_setting('rasm.tenant')::uuid)` so the read predicate AND the write predicate are both bound — a tenant can neither read nor write another tenant's rows, and a `RESTRICTIVE` policy composes as an AND-conjunction over a permissive base for the defense-in-depth row; the tenant id rides the `current_setting('rasm.tenant')::uuid` session GUC which `RlsScope.Bind` sets per transaction through `set_config(slot, id, is_local => true)` so the GUC is transaction-scoped and pool-return cannot leak it across acquisitions (the ingress the RLS predicate the `Sync/coordination#FENCED_CAS` CAS and every `Query/rail` read depend on — without it `current_setting('rasm.tenant', true)` is null and the policy fails closed for every session); `Project` derives every create step from the resolved `TenantContext` — the `schema` row creates the tenant schema, the search-path role, and a `pg_partman` `create_parent` per-tenant range partition set when the model partitions, the `rls` row seeds the policy and the per-tenant GUC default, the `db-per-tenant` row emits the `CREATE DATABASE … TEMPLATE rasm_template` clone, and each row folds the `TenantQuota` GUC rows (`ALTER ROLE … CONNECTION LIMIT`, `ALTER ROLE … SET statement_timeout/idle_in_transaction_session_timeout/lock_timeout/temp_file_limit/work_mem`, `ALTER ROLE … VALID UNTIL`) so the quota is a deploy-time role constraint rather than a runtime branch; `Destroy` mirrors each create with its idempotent `DROP … IF EXISTS` step; the per-tenant RLS-policy-applied fact, the per-tenant quota-set fact, and the `AuditBinding.BindTenant` pgaudit-category binding fact fold into the provisioning verifier.
- Receipt: `TenantReceipt(TenantId Tenant, TenantSlug Slug, TenancyModel Model, RlsPolicy Policy, TenantQuota Quota, Seq<string> Applied, CorrelationId Correlation, Instant At)` is the lifecycle evidence; a per-tenant RLS-policy-applied `StoreFact` row carrying the `USING`/`WITH CHECK`/`FORCE` triple, a `tenant.quota.applied` fact carrying every GUC `(setting, value)` pair, and the `AuditBinding.BindTenant` result fact fold into the open receipt's proof rows; a tenant-isolation mismatch, an unsafe slug, or a quota-set failure is a typed `TenancyFault`, never silent.
- Packages: Npgsql, Npgsql.EntityFrameworkCore.PostgreSQL, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.AppHost (project)
- Growth: one `TenancyModel` row per new isolation strategy (with its `IsolationPlane` and partition strategy columns); one `TenantQuota` GUC column per new resource bound; one `RlsPolicy` column per new policy dimension; one `TenantPhase` case or one `Fragments` table row per new lifecycle concern; one `TenancyFault` case per new failure cause; zero new surface.
- Boundary: `TenancyModel` is one axis row tying the host profile to an RLS policy, never a forked store family — PostgreSQL with RLS is the same `Store/profiles#PROFILE_AXIS` `PostgresServer` profile, never a new engine, and the package prohibition against admitting a new engine row holds; the tenant id arrives from the AppHost `TenantContext` threading owned at `AppHost/runtime-ports#PORT_RECORDS`, never minted here — `TenantProvision.Project` consumes the resolved `TenantContext.TenantId`/`Slug` as settled input across every `TenantPhase`, so the tenant-id resolution stays the AppHost concern and only the per-model DDL lifecycle plus the per-transaction GUC binding are owned here; `TenantContext.TenantSlot` (`rasm.tenant`) is the single canonical GUC spelling every owner reads, never a re-spelled literal — the `current_setting`/`set_config` seam, the role default, and the meter tag (`AppHost/runtime-ports`) are one symbol; the slug is admitted ONCE through `TenantSlug.Validate` and every downstream `tenant_{slug}` identifier reads the validated value, so the dynamic-identifier interpolation rides a closed vocabulary per the `Query/rail#TWO_DOOR_ROUTING` identifier law rather than a raw client string — a slug carrying SQL metacharacters or exceeding the role-name length is a `TenancyFault.UnsafeSlug` admission reject, never an interpolated injection; the audit-category consequence — a tenant-id-bearing classification binding to one pgaudit category — is owned at `Version/retention#AUDIT_BINDING`, and this cluster owns only the RLS-policy, quota, GUC-ingress, and lifecycle mechanics; `TenantQuota` is the per-tenant role/GUC governance set on the tenant-lifecycle fold, never a second rate-limiter — the runtime write-path enforcement reads the same bound at `Query/rail#BULK_LANE`, the connection-cap enforcement rides the role `CONNECTION LIMIT`, and the statement/idle/lock-timeout, temp-file, and work-mem bounds ride the role-default GUCs the provisioning emits, so the AppHost resource governor and this server-side bound are one declared set rather than parallel limiters; `RlsScope.Bind` is the BOUNDARY_ADMISSION ingress the `Query/transaction#TXN_SCOPE` `Transactions.Scoped` body runs as its first `ExecuteSqlRawAsync` statement so every tenant-scoped transaction sets the GUC before any RLS-filtered read or write, and `Adopt` is the session-default fallback for a role pinned to one tenant — the two never duplicated, one transaction-scoped (`is_local => true`) and one role-default; the `schema`/`db-per-tenant` rows project no `Policy`-phase SQL because their isolation is a connection-routing concern resolved by the host placement (`IsolationPlane.Schema`/`Database`), not a row-level predicate, but they carry full `Apply`/`Destroy` lifecycle DDL because schema and database creation are server-tier concerns; the `current_setting('rasm.tenant')` RLS binding and the `ClusterConfig.Preload` companion contract stay intact, owned at `Store/provisioning#CLUSTER_CONFIG`, and the `pg_partman` per-tenant partition provisioning rides the admitted `Store/profiles#PROVISIONING_ROWS` `pg_partman` manifest row, never a second partition owner.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

// The key IS the SQL token, so `.Key` is the rendered clause and no second projection member is needed.
[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class PolicyMode {
    public static readonly PolicyMode Permissive = new("PERMISSIVE");   // OR-combined base
    public static readonly PolicyMode Restrictive = new("RESTRICTIVE"); // AND-combined defense-in-depth
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<StoreKeyPolicy, string>]
public sealed partial class PolicyCommand {
    public static readonly PolicyCommand All = new("ALL");
    public static readonly PolicyCommand Select = new("SELECT");
    public static readonly PolicyCommand Insert = new("INSERT");
    public static readonly PolicyCommand Update = new("UPDATE");
    public static readonly PolicyCommand Delete = new("DELETE");
}

// The isolation strategy axis: ONE vocabulary, no parallel `IsolationPlane` enum modeling the same closed
// set. Each row carries its plane as derived behavior columns — whether it owns a schema, owns a role, clones
// a database, projects an RLS policy, and partitions — so the lifecycle fold reads behavior off the row
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

    public bool OwnsSchema { get; }       // CREATE SCHEMA + search-path role on Apply
    public bool OwnsRole { get; }         // CREATE ROLE WITH LOGIN + quota GUCs + table grants
    public bool ClonesDatabase { get; }   // CREATE DATABASE … TEMPLATE on Apply
    public bool EmitsPolicy { get; }      // only the predicate plane projects Policy-phase SQL
    public bool Partitions { get; }       // pg_partman per-tenant range partition set on Apply
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

// The full per-tenant role-and-GUC governance set — every bound is a Duration-typed, sized, or category
// quantity the Gucs projection lowers to one `ALTER ROLE … SET …` row, so the quota is a deploy-time role
// constraint the Query/rail#BULK_LANE write path and the role itself enforce, never a runtime branch.
// `AuditCategory` is the per-tenant `pgaudit.log` binding (Version/retention#AUDIT_BINDING resolves the
// classification → category, this carries the resolved value as a role-default GUC so a tenant's audit class
// is provisioned with the role, not bound at every session). A new resource bound is one column plus one
// Gucs row; an empty `WorkMem`/`AuditCategory` projects no GUC, so a row is never emitted with a blank value.
[ComplexValueObject]
public sealed partial class TenantQuota {
    public int ConnectionLimit { get; }            // ALTER ROLE … CONNECTION LIMIT
    public Duration StatementTimeout { get; }      // statement_timeout
    public Duration IdleInTxnTimeout { get; }      // idle_in_transaction_session_timeout
    public Duration IdleSessionTimeout { get; }    // idle_session_timeout (idle outside a txn)
    public Duration LockTimeout { get; }           // lock_timeout
    public long TempFileLimitKb { get; }           // temp_file_limit (-1 unlimited)
    public string WorkMem { get; }                 // work_mem floor, e.g. "4MB"
    public string AuditCategory { get; }           // pgaudit.log, e.g. "write, ddl, role" (Version/retention)
    public Option<Instant> ValidUntil { get; }     // ALTER ROLE … VALID UNTIL role expiry
    public long WriteBytesPerSecond { get; }       // the Query/rail#BULK_LANE write-rate bound

    static string Ms(Duration d) => ((long)d.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);

    // The role-default GUC product — every row is a (setting, value) pair the lifecycle fold lowers to one
    // `ALTER ROLE {role} SET {setting} = '{value}'`. VALID UNTIL is a CONNECTION-LIMIT-adjacent role clause,
    // not a SET GUC, so it rides the role-create step instead of this projection; an empty string-valued GUC
    // is filtered so a tenant without an audit class or work-mem floor emits no blank SET.
    public Seq<(string Setting, string Value)> Gucs => Seq(
            ("statement_timeout", Ms(StatementTimeout)),
            ("idle_in_transaction_session_timeout", Ms(IdleInTxnTimeout)),
            ("idle_session_timeout", Ms(IdleSessionTimeout)),
            ("lock_timeout", Ms(LockTimeout)),
            ("temp_file_limit", TempFileLimitKb.ToString(CultureInfo.InvariantCulture)),
            ("work_mem", WorkMem),
            ("pgaudit.log", AuditCategory))
        .Filter(static row => row.Item2.Length > 0);
}

// The full CREATE POLICY product, not a bare USING predicate. USING is the read/visibility predicate,
// WithCheck the write/insertability predicate (so a tenant cannot write a row tagged with another tenant's
// id), Force adds the table-owner filter a bare ENABLE omits, Mode composes permissive (OR base) vs
// restrictive (AND defense-in-depth), Command scopes the policy per FOR clause, Grantee binds TO role. The
// `Table`/`TenantColumn`/`Grantee` identifiers are admitted closed-vocabulary (the schema/migration owner
// supplies them, never raw client text), so they interpolate at the same one seam the slug rides.
[ComplexValueObject]
public sealed partial class RlsPolicy {
    public string Table { get; }
    public string Name { get; }   // the per-policy name (`{table}_tenant`, `{table}_restrict`)
    public string TenantColumn { get; }
    public PolicyCommand Command { get; }
    public PolicyMode Mode { get; }
    public string Grantee { get; }
    public bool Force { get; }

    // The defense-in-depth pair: a PERMISSIVE base FOR ALL the tenant rows, AND a RESTRICTIVE conjunct that
    // forbids a row escaping the tenant predicate even if a later permissive policy is added — the two
    // policies an `rls`-plane table arms, modeled as a Seq rather than one policy pretending to be both.
    public static Seq<RlsPolicy> Pair(string table, string tenantColumn, string grantee) => Seq(
        Create(table, $"{table}_tenant", tenantColumn, PolicyCommand.All, PolicyMode.Permissive, grantee, force: true),
        Create(table, $"{table}_restrict", tenantColumn, PolicyCommand.All, PolicyMode.Restrictive, grantee, force: true));

    string Predicate => $"{TenantColumn} = current_setting('{TenantContext.TenantSlot}')::uuid";

    // ENABLE arms the policy machinery, FORCE extends it to the table owner, the CREATE POLICY binds both
    // the USING read predicate and the WITH CHECK write predicate under the mode/command/grantee product.
    public Seq<string> Apply() => Seq(
        $"ALTER TABLE {Table} ENABLE ROW LEVEL SECURITY",
        Force ? $"ALTER TABLE {Table} FORCE ROW LEVEL SECURITY" : $"ALTER TABLE {Table} NO FORCE ROW LEVEL SECURITY",
        $"CREATE POLICY {Name} ON {Table} AS {Mode.Key} FOR {Command.Key} TO {Grantee} USING ({Predicate}) WITH CHECK ({Predicate})");

    public Seq<string> Destroy() => Seq(
        $"DROP POLICY IF EXISTS {Name} ON {Table}",
        $"ALTER TABLE {Table} DISABLE ROW LEVEL SECURITY");
}

public sealed record TenantReceipt(
    TenantId Tenant, TenantSlug Slug, TenancyModel Model, Seq<RlsPolicy> Policies,
    TenantQuota Quota, Seq<string> Applied, CorrelationId Correlation, Instant At);

[Union]
public abstract partial record TenantPhase {
    public sealed partial record Apply : TenantPhase;                       // create the per-model objects
    public sealed partial record Destroy : TenantPhase;                     // idempotent DROP … IF EXISTS teardown
    public sealed partial record Policy(Seq<RlsPolicy> Policies) : TenantPhase;  // the RLS CREATE/DROP POLICY set
    public sealed partial record Bind : TenantPhase;                        // per-transaction SET LOCAL GUC ingress
    public sealed partial record Adopt : TenantPhase;                       // role-default ALTER ROLE … SET adoption
}

// --- [ERRORS] --------------------------------------------------------------------------

// A closed fault family deriving from Expected, satisfying Semigroup<TenancyFault> so the applicative
// Validation pass over the slug + every policy identifier accumulates faults as typed members under one
// Aggregate case rather than flattening to a string (the doctrine FAULT_FAMILIES law: a Semigroup aggregate
// preserves typed members). Recovery is code-keyed via Is/HasCode/IsType<E>, never `==`.
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
    // The one lifecycle projection. Each phase admits exactly the identifiers it interpolates, and admission
    // is applicative where the faults are independent: the `Policy` phase accumulates EVERY bad policy
    // identifier in one pass (`.Traverse(...).As()`), the slug-bearing phases admit the slug once through the
    // generated TenantSlug.Validate bridge. Once every identifier is closed-vocabulary, the model drives the
    // fragment fold through TenancyModel.Switch — a total generated dispatch, never a string-tuple language
    // switch with a silent default arm — so no raw client string reaches an interpolation. `Policy` needs no
    // slug (it interpolates the policy's own admitted table/grantee), so it is not gated behind slug admission.
    public static Validation<TenancyFault, Seq<string>> Project(TenancyModel model, TenantPhase phase, TenantContext tenant, TenantQuota quota) =>
        phase switch {
            TenantPhase.Policy p => model.EmitsPolicy
                ? p.Policies.Traverse(static rls =>
                        TenantIdentifier.Admit(rls.Table).Apply(static (_, _) => unit, TenantIdentifier.Admit(rls.Grantee)).As().Map(_ => rls))
                    .As().Map(static held => held.Fold(Seq<string>(), static (sql, rls) => sql + rls.Apply()))
                : Success<TenancyFault, Seq<string>>(Seq<string>()),
            _ => Admit(tenant.Slug).Map(slug => phase switch {
                TenantPhase.Bind => Seq($"select set_config('{TenantContext.TenantSlot}', '{tenant.TenantId.Value}', is_local => true)"),
                TenantPhase.Adopt => Seq($"ALTER ROLE {slug.Role} SET {TenantContext.TenantSlot} = '{tenant.TenantId.Value}'"),
                _ => Fragments(model, phase, slug, tenant.TenantId, quota),
            }),
        };

    static Validation<TenancyFault, TenantSlug> Admit(string slug) =>
        TenantSlug.Validate(slug, null, out var owned) is { } fault ? Fail<TenancyFault, TenantSlug>(fault) : Success<TenancyFault, TenantSlug>(owned!);

    // The create/teardown matrix driven by the model's behavior columns under the generated Switch, never a
    // string-tuple `switch` with a silent `_`. Apply composes the plane-specific objects (schema, role,
    // grants, database, partition set) off the model's columns; Destroy mirrors each with its idempotent DROP.
    // A LOGIN role with no GRANT is hollow, so the role step grants the tenant schema's table privileges to
    // the role rather than minting a role that can connect but read nothing.
    static Seq<string> Fragments(TenancyModel model, TenantPhase phase, TenantSlug slug, TenantId id, TenantQuota quota) {
        var apply = phase is TenantPhase.Apply;
        var quotaGucs = quota.Gucs.Map(g => $"ALTER ROLE {slug.Role} SET {g.Setting} = '{g.Value}'");
        var validUntil = quota.ValidUntil.Match(Some: at => $" VALID UNTIL '{at}'", None: static () => string.Empty);
        var roleApply = Seq($"CREATE ROLE {slug.Role} WITH LOGIN CONNECTION LIMIT {quota.ConnectionLimit}{validUntil}") + quotaGucs;
        // pg_partman: p_control is the PARTITION-KEY COLUMN (a time/serial column), p_type the range/list
        // strategy — never the literal `physical`. The schema-plane parent is the per-tenant op-log rollup the
        // Version/retention partition-drop policy honors; the rls-plane parent is the shared op-log partitioned
        // by the per-tenant tag's time column.
        var partman = Seq($"SELECT partman.create_parent(p_parent_table := '{(model.OwnsSchema ? slug.Schema : "public")}.op_log', p_control := 'logged_at', p_interval := '1 day', p_type := 'range')");
        return model.Switch(
            single: () => Seq<string>(),
            schema: () => apply
                ? Seq($"CREATE SCHEMA IF NOT EXISTS {slug.Schema}") + roleApply
                    + Seq($"ALTER ROLE {slug.Role} SET search_path = {slug.Schema}",
                          $"GRANT ALL ON ALL TABLES IN SCHEMA {slug.Schema} TO {slug.Role}",
                          $"ALTER DEFAULT PRIVILEGES IN SCHEMA {slug.Schema} GRANT ALL ON TABLES TO {slug.Role}")
                    + partman
                : Seq($"DROP SCHEMA IF EXISTS {slug.Schema} CASCADE", $"DROP ROLE IF EXISTS {slug.Role}"),
            rls: () => apply
                ? roleApply + Seq($"ALTER ROLE {slug.Role} SET {TenantContext.TenantSlot} = '{id.Value}'") + partman
                : Seq($"DROP ROLE IF EXISTS {slug.Role}"),
            dbPerTenant: () => apply
                ? roleApply + Seq($"CREATE DATABASE {slug.Database} TEMPLATE rasm_template OWNER {slug.Role} CONNECTION LIMIT {quota.ConnectionLimit}")
                : Seq($"DROP DATABASE IF EXISTS {slug.Database} WITH (FORCE)", $"DROP ROLE IF EXISTS {slug.Role}"));
    }
}

// The per-transaction GUC ingress (BOUNDARY_ADMISSION) and the runtime egress of the one GUC-binding
// statement shape Project's `Bind`/`Adopt` phases project as deploy-script text. The two lowerings are NOT
// duplicates: the deploy form emits literal script text (a `postgresql.conf`-adjacent role default or a
// migration statement that takes no bound parameter), the runtime form here binds the TenantSlot name and the
// TenantId value as real `set_config` arguments through ExecuteSqlAsync's FormattableString parameterization,
// so the runtime path never literal-interpolates the tenant id. The Query/transaction#TXN_SCOPE
// Transactions.Scoped body runs Bind as its first statement so current_setting('rasm.tenant') resolves for
// every RLS-filtered read and write in the transaction; the `is_local => true` third argument scopes the
// binding to the transaction so pool-return cannot leak it across acquisitions.
public static class RlsScope {
    public static IO<Unit> Bind(DbContext context, TenantContext tenant, CancellationToken token) =>
        IO.liftVAsync(async _ => {
            _ = await context.Database.ExecuteSqlAsync(
                $"SELECT set_config({TenantContext.TenantSlot}, {tenant.TenantId.Value}, true)", token);
            return unit;
        });
}
```

## [03]-[RESEARCH]

- [RLS_POLICY_PROBE]: the live-PG18 `CREATE POLICY … AS PERMISSIVE FOR ALL TO tenant_role USING (tenant_id = current_setting('rasm.tenant')::uuid) WITH CHECK (…)` round-trip against a per-transaction `set_config('rasm.tenant', …, true)` binding and a per-role `ALTER ROLE … SET rasm.tenant` default — whether the `USING` predicate filters a foreign tenant's rows on read AND the `WITH CHECK` predicate rejects a cross-tenant write, the `ENABLE` + `FORCE ROW LEVEL SECURITY` pair forces the predicate on every read including the table owner, and a `RESTRICTIVE` policy composes as an AND-conjunction over the permissive base, proven against a live PG18 server before the `rls` lifecycle fence pins; the `set_config(..., is_local => true)` transaction-scoped GUC versus the role-default GUC and the pool-return leak boundary confirmed on the same server.
- [TENANT_QUOTA_APPLY]: the per-tenant `ALTER ROLE … CONNECTION LIMIT … VALID UNTIL` plus the role-default `statement_timeout`/`idle_in_transaction_session_timeout`/`lock_timeout`/`temp_file_limit`/`work_mem` GUC application and the `db-per-tenant` `CREATE DATABASE … TEMPLATE rasm_template` clone — whether the role connection cap, the timeout/temp/work-mem GUCs, and the role expiry bind as server-side limits the runtime write path reads rather than runtime branches, and whether the template clone carries the seeded schema, confirmed against the Forge-provisioned PG18 before the quota fence pins.
- [PARTMAN_PER_TENANT]: the `partman.create_parent(p_parent_table => 'tenant_{slug}.op_log', p_interval => '1 day')` per-tenant range-partition provisioning for the `schema`/`rls` planes — whether the admitted `pg_partman` bgworker provisions a per-tenant partitioned op-log rollup without a second partition owner and whether `run_maintenance` advances every tenant's partition horizon from the one `Store/profiles#PROVISIONING_ROWS` maintenance row, proven against the preloaded `pg_partman` before the partition step pins.
