# [IAC_SPEC]

`StackSpec` is the decoded value an app supplies to deploy — the closed arm union, the consumption-axis rosters, coordinate options, and capability profile — and the coordinates-never-material law keeps every spec value loggable, diffable, and receipt-safe. Deployment shape arrives here and nowhere else, so no tier infers a topology, tenant count, or pooling posture. `iac/src/program/spec.ts` is the module; a new cloud is one arm entry `provider.md` realizes, a new axis one profile field with its default, a new plane one `Option` field.

`Connection` owns the SSH coordinate product, and its `ssh` projection is the only spelling of the daemon URL. `Tier` adapts Pulumi's class model with one option fold, one terminal output registration, and the `harden` anchor every pod-constructing tier stamps, `StackOutputs` is the typed exit whose secret-refusal gate holds the raw `OutputMap`, whose `pairsOf` owns the one `<plane>.<field>` flatten, and whose `channels` and `custody` catalogs together own every deploy-to-process variable spelling, and `sharding` is the sole plane crossing back into the runtime graph — the two planes meet at the process boundary and never import each other.

## [01]-[INDEX]

- [02]-[ARM_VOCABULARY]: the closed arm tuple and derived identity union; `StackSpec`.
- [03]-[SPEC_OWNER]: the app-supplied value: coordinates, profile, separation, defaults; `StackSpec`.
- [04]-[TIER_BASE]: the abstract component owner: token scope, option fold, seal, privilege anchor, roster; `Tier`.
- [05]-[OUTPUT_PLANES]: the decoded exit, the secret gate, the channel-flatten owner, the env-key catalogs; `StackOutputs`.

## [02]-[ARM_VOCABULARY]

[ARM_VOCABULARY]:
- Owner: the interior `_arms` key tuple — order, iteration, and the non-empty `Schema.Literal` spread are tuple facts stated once; `StackSpec.Arm` derives on the interior anchor, `StackSpec.arms` rides the class as a static, and the arm roster has one edit site branch-wide.
- Law: arm admission states identity only; realized capability is the construction inventory in `provider.md`'s `_map` and `_ARMS`.
- Growth: a new cloud is one `_arms` entry — the provider record, the equivalence map, and the `Schema.Literal` admission all break at compile time until their rows land.
- Boundary: which resources an arm composes and which program body runs are `provider.md`'s record and map.

## [03]-[SPEC_OWNER]

[SPEC_OWNER]:
- Law: coordinates, never material — `Connection` carries host/user/port with the hardening coordinates (`hostKey` is the host's public key pinned against a MITM re-key, `bastion` is the jump-hop's own host/user/port row reusing the same struct) and no key field; the SSH private key, provider tokens, and generated passwords travel the provider material read or the in-graph Doppler fan-in, so a spec value never leaks into state, receipt, or log; the `ssh` getter on `Connection` is the one spelling of the daemon URL every consumer reads.
- Law: `epoch` is the one rotation trigger — it feeds every `@pulumi/random` `keepers` map and every `@pulumi/command` `triggers` list, so bumping one field re-mints credentials and re-runs bootstrap deliberately; per-resource rotation knobs are the named defect.
- Law: the profile is defaults-total — `scale` selects the `kube/workload` sizing row, `compute` selects the cloud-arm workload posture (`serverless` = the managed container cell, `cluster` = the managed-Kubernetes escalation that reuses the whole `kube/*` roster), `capacity` sizes the node pool that escalation stands (the instance type and the EKS-optimized OS family off their providers' own rosters, beside the pool's floor and ceiling), `extensions` names the `data` extension-matrix subset the data tier finalizes (validated against `Pg.rows` at `kube/data.md`, never here), `objectEngine` selects a conditional-put-conforming self-host row (`minio` = the maintained continuation image, `ceph` = the RGW row; the engine that cannot CAS has no literal to select), `exposure` selects the traffic posture (`direct` = the metal-address DNS row, `tunnel` = the Zero-Trust row, `internal` = no edge — the workload stands service-only and no edge coordinate is demanded), `data` carries instance count, storage, backup cron, retention, and the pooling pair, `fanout` carries the NATS replica quorum and stream storage, and `separation` carries the tenant-boundary posture the cluster realizes — every field defaulted at the declaration so `_Profile.make({})` is a complete standard deployment and an app states only its deltas; `objective` alone is a getter rather than a field, because the durability window follows from `topology` and a defaulted second declaration of it forks the pair the runner grades against.
- Law: `topology` is the deployment-shape axis a composition root supplies, not a fact a package infers — `proc/config#ADMISSION_ROWS` owns the closed roster and `ConsumptionProfile.topologies` spreads that one spelling into this schema, `service` is the deploy plane's own default, and a tier serving a proper subset refuses the rest at admission with typed evidence naming the axis and the rejected value; `operate/converge.md` is the first such refusal, `_SERVED` its subset; the same owner supplies the topology-keyed durability window through `ConsumptionProfile.recoveryOf`, so `objective` reads one branch table and this schema restates no window of its own.
- Law: a coordinate naming a provider's own vocabulary types off that vocabulary, never a bare string — `capacity.instanceType` spreads the generated `aws.types.enums.ec2.InstanceType` roster and `capacity.os` the `eks.OperatingSystem` roster into their admission alphabets, so an unspellable capacity refuses at decode where the value is still loggable rather than surfacing as a provider fault mid-apply, and the roster widens with the installed tree so no release literal rides this page; a coordinate whose governing roster is arm-dependent cannot close here at all — `region` reads an AWS roster on one arm and an unpublished Google one on the next — so it stays a plain coordinate and admits inside the arm that knows whose vocabulary governs, which is `provider.md`'s `_vocab` proof.
- Law: `_Profile` is a `Schema.Class` for exactly one reason — a derived `objective` needs a body, and `Schema.Struct` carries none; the class seats `topology` beside that getter, which is what makes `StackSpec.Profile` satisfy `operate/converge` `Converge.Profile` structurally instead of by an adapter the deploy program hand-builds.
- Law: `data` sizes its own cluster — `instances` and `storage` bound the estate and `requests`/`limits` bound each instance, because a cluster stating no resource block schedules BestEffort and the estate's only stateful workload is then the first pod the kubelet evicts under node pressure; both faces spell one quantity pair so a Guaranteed posture is a caller setting them equal rather than a second axis.
- Law: `data.pool` sizes the pooler exactly as `data` sizes the cluster — the replica count, the request/limit pair the bouncer container carries, and the two connection ceilings (`clients` meters browser-side connections, `sessions` the server-side pool width) all enter as defaulted coordinates, because the CRD's own default of one replica and its silence on `resources` together schedule a BestEffort singleton in front of a Guaranteed database; `kube/data.md` `[CNPG_CLUSTER]` is the one consumer and owns both the operator's parameter spellings and the allowlist that proves them, so this owner states magnitudes and never a bouncer key.
- Law: `data.pooling` is a capability input, not a chart knob — the mode selects the PgBouncer posture AND the primitive set that posture voids, `session` is the default because it voids none, `data.primitives` names the pooled-bind primitives the app composes, `kube/data.md` refuses the intersection at admission, and the realized mode publishes on the `data` plane so the runtime capability rail gates on deployment truth instead of assuming a session.
- Law: a wire literal carries a refined brand, never a bare string — `_Quantity` holds the Kubernetes resource-quantity grammar every storage and sizing field spells, `_Cron` holds the six-field seconds-leading dialect the CNPG schedule consumes, and `_Window` holds the retention duration every store row's `retain` projection reads; a value failing its grammar fails at decode, where the coordinate is still loggable, never inside a chart the operator already accepted.
- Law: the observability backend is spec data — `observe.store` selects the metrics-store row (`prometheus` the reference row; `mimir` the fleet escalation whose object-store binding reuses the object plane and whose org-id header scopes the stack; `victoriametrics` the resource-pressure escape), `observe.retention` the store retention window, `observe.profiles` the Pyroscope row, `observe.ingest` the pg-server metrics arm, `observe.costs` the OpenCost pricing row, and `observe.ebpf` the privileged zero-code instrumentation row — every coordinate interpreted by `operate/observe.md`'s row family, never a second program body.
- Law: every fleet escalation resolves to one `_Escalation` value, so arming one is a coordinate flip and never a tier re-design — `observe.sampling` names the traces-pipeline decision tier (`head` rides the SDK parent ratios, `tail` mounts the gateway's already-defined `tail_sampling` processor), `observe.topology` names the collector deployment shape (`gateway` is the one deployment every workload dials, `agent` adds the daemonset tier exporting Arrow onto that same door), and `observe.buffer` names the durability carrier (`file` is the gateway's own disk queue, `broker` adds the paired kafka legs that survive the gateway itself); each value is the arm coordinate its `libs/.planning/ARCHITECTURE.md` `[FLEET_ESCALATION]` row names, and a row whose coordinate no value spells is an escalation nothing can arm.
- Law: an escalation carrying no coordinate is unrepresentable — `_Escalation.buffer` discriminates on `mode` and the broker arm carries its own `brokers` roster, exactly as `_Separation` carries the tenant slugs its escalated arms cannot run without, so selecting a broker leg against an absent broker estate fails at decode rather than rendering a pipeline that connects to nothing.
- Law: `observe.topology` names the COLLECTOR shape and `profile.topology` the CONSUMPTION shape — the two axes share a noun and nothing else, so a gateway estate serving `edge` consumers and an agent estate serving `service` consumers are both ordinary, and folding one onto the other re-mints a deployment assumption the consumption roster exists to delete.
- Law: `observe.analytics` selects the durable residence family (`none` refuses evidence residence outright, `lake` is the default cold tail on the object plane the stack already carries, `clickhouse` is the interactive wide-event residence, `both` runs the pair) — residence selection is the one coordinate deciding whether telemetry survives its metrics retention window, so `none` states an accepted evidence loss rather than an unmade decision.
- Law: the tenant boundary is data, never code paths — `separation.mode` selects the control-plane boundary (`single` = one app one namespace; `namespace` = Capsule-governed namespace-per-tenant; `vcluster` = virtual-control-plane-per-tenant), `separation.pgTier` selects the data-plane escalation (`shared-rls` = one database with `Tenancy.rls` policy rows; `db-per-tenant` = one CNPG cluster with one `Database` CR per tenant; `cluster-per-tenant` = one CNPG `Cluster` per tenant with its own custody envelope), and `separation.tenants` names the tenant slugs the `kube/tenant.md` owner realizes rows for; an escalation is a spec delta interpreted by the owning tiers, never a second program body.
- Law: tenant GOVERNANCE is spec data the `kube/tenant.md` `_MODES` fold reads — `quota` bounds the namespaces a tenant claims (floored at one by the Capsule CRD, ceilinged by this estate), `ownerGroup` is the SUFFIX the owning group's name carries so the fold composes `${tenant}-${ownerGroup}` from one edit site, and `registries` and `ingressClasses` name the pull sources and classes a tenant may claim with an empty roster meaning unrestricted; every column is defaulted, so a standard escalation states none and the defaults reproduce the posture the retired literals bound, and a governance value living as a literal inside an isolation-mode row is estate policy wearing a module constant's clothes — the defect this axis deletes.
- Law: an escalation with no tenant is unrepresentable — `_Separation` is a union discriminated on `mode`, the `single` arm carrying neither `pgTier` nor `tenants` and each escalated arm carrying `pgTier` and the governance columns beside a `NonEmptyArray` of slugs, so a data-plane tier paired with an empty tenant roster fails at decode instead of realizing zero tenant databases in silence, and a governance coordinate is unspellable on the mode that governs nothing; the `pgTier` getter projects the shared tier for `single`, giving every consumer one total read.
- Law: absence is `Option` admitted by `Schema.optionalWith(..., { as: "Option" })` — a cloud arm demanding an absent `region`, or a selfhosted arm demanding an absent `connection`, fails as a typed `DeployFault` inside its provider arm before the `PulumiFn` is entered, never as an `undefined` read and never as a construction-time throw inside a tier.
- Entry: `StackSpec.make(...)` at the app seam; `Schema.decodeUnknown(StackSpec)` where the value arrives as data.
- Growth: a new coordinate is one field with its dialect chosen here; a new profile axis is one `_Profile` field with its default; a new separation tier is one literal with its interpreting row in the owning tier and a new governance axis one `_Governance` field the same tier reads; a new consumption axis is one roster the runtime admission owner mints with one `_Profile` field spreading it.
- Boundary: deploy-host facts (backend URL, passphrase, CLI root) are `automation.md`'s Config surface; extension validation is `kube/data.md`'s; sizing interpretation is `kube/workload.md`'s; tenant realization is `kube/tenant.md`'s; the node group `capacity` sizes is `provider.md`'s aws `cluster` row; backend publication is `operate/converge.md`'s pointer write.
- Packages: `effect` (`Schema`); `@pulumi/aws` (`types.enums.ec2.InstanceType`); `@pulumi/eks` (`OperatingSystem`); `@rasm/ts/core` (`Identity.App`); `@rasm/ts/runtime` (`Consumption`, `Profile`).

```typescript signature
import * as aws from "@pulumi/aws"
import * as eks from "@pulumi/eks"
import { Identity } from "@rasm/ts/core"
import { type Consumption, Profile as ConsumptionProfile } from "@rasm/ts/runtime"
import { Schema } from "effect"

const _arms = ["selfhosted-k8s", "selfhosted-docker", "aws", "gcp", "cloudflare"] as const

const _Name = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,39}$/), Schema.brand("StackName"))
const _Slug = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9-]{1,30}$/), Schema.brand("TenantSlug"))
// Kubernetes admits a binary suffix (Ki…Ei) or a decimal one (n u m k M G T P E) over a decimal
// mantissa; the scientific form the API also parses is refused here because no estate value spells it.
const _Quantity = Schema.String.pipe(
  Schema.pattern(/^[+-]?(?:\d+(?:\.\d+)?|\.\d+)(?:Ki|Mi|Gi|Ti|Pi|Ei|[numkMGTPE])?$/),
  Schema.brand("K8sQuantity"),
)
// Six fields, seconds leading: the CNPG `ScheduledBackup` dialect, not the five-field crontab one.
const _Cron = Schema.String.pipe(
  Schema.pattern(/^[0-9A-Z*/,?-]+(?: [0-9A-Z*/,?-]+){5}$/),
  Schema.brand("CronExpression"),
)
// The suffix set is the INTERSECTION every store dialect reads identically: victoria-metrics-single's
// `retentionPeriod` reads a bare number as MONTHS and admits `h|d|w|y` alone, so a window spelled `ms`, `s`, or
// `m` silently changes meaning or refuses on exactly one row — the brand refuses those spellings at decode instead.
const _Window = Schema.String.pipe(Schema.pattern(/^\d+(?:h|d|w|y)$/), Schema.brand("RetentionWindow"))
// the PgBouncer posture spelled once: the profile SELECTS it and the `data` output plane publishes what the
// deployment REALIZED, so the two faces of one vocabulary cannot drift into a mode only one side admits
const _Pooling = Schema.Literal("session", "transaction", "statement")

const _Bastion = Schema.Struct({
  host: Schema.NonEmptyString,
  user: Schema.optionalWith(Schema.NonEmptyString, { default: () => "root" }),
  port: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 65535)), { default: () => 22 }),
})

class Connection extends Schema.Class<Connection>("Connection")({
  ..._Bastion.fields,
  hostKey: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  bastion: Schema.optionalWith(_Bastion, { as: "Option" }),
}) {
  get ssh(): string {
    return `ssh://${this.user}@${this.host}:${this.port}`
  }
}

const _Doppler = Schema.Struct({
  project: Schema.NonEmptyString,
  config: Schema.NonEmptyString,
})

// The scheduler reads the pair, so both faces spell one shape: a cluster stating neither lands BestEffort and
// is the first pod the kubelet evicts under node pressure — on the estate's only stateful workload.
const _Compute = Schema.Struct({ cpu: _Quantity, memory: _Quantity })

// `_Capacity` sizes the node pool the `cluster` compute posture stands, and `instanceType` spreads the PROVIDER's
// own generated roster into the admission alphabet: the node-group arg is `Input<string>`, so the SDK closes
// nothing and this coordinate is the only surface that can, while the roster widens with the installed tree
// rather than a literal set this page would chase. `min` doubles as the pool's desired size — a cluster whose
// steady state is its own floor scales up under pressure and parks no capacity the scheduler never claimed.
const _Capacity = Schema.Struct({
  instanceType: Schema.optionalWith(Schema.Literal(...Object.values(aws.types.enums.ec2.InstanceType)), {
    default: () => aws.types.enums.ec2.InstanceType.M7g_Large,
  }),
  // `os` is the pin, not `amiType`: the node-group component resolves the EKS-optimized image from this axis
  // AND the instance types, so an arm64 capacity value picks its own arch, while `amiType` is the raw
  // `Input<string>` twin that supersedes that derivation and forks it. The default names the FAMILY rather
  // than the `RECOMMENDED` alias, whose value AWS moves — an alias pin re-images the pool on a bump that
  // touched nothing this estate stated. Deprecated members ride the provider's own const because an estate
  // never narrows a provider vocabulary, and `RECOMMENDED` collapses onto its family's literal here.
  os: Schema.optionalWith(Schema.Literal(...Object.values(eks.OperatingSystem)), {
    default: () => eks.OperatingSystem.AL2023,
  }),
  min: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 100)), { default: () => 2 }),
  max: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 100)), { default: () => 4 }),
})

// PgBouncer stands in front of the cluster above it, so the pool states its own width and envelope: the CRD
// defaults ONE replica, and a bouncer entry carrying no `resources` schedules BestEffort in front of a
// Guaranteed database. `clients` and `sessions` are the two ceilings the bouncer meters by — the operator's
// parameter spellings and its key-by-key admission allowlist are `kube/data.md`'s, never restated here — and
// both bounds are this estate's own, floored at one connection because a pool admitting none is not a pool.
const _Pool = Schema.Struct({
  instances: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 9)), { default: () => 2 }),
  requests: Schema.optionalWith(_Compute, { default: () => _Compute.make({ cpu: _Quantity.make("100m"), memory: _Quantity.make("128Mi") }) }),
  limits: Schema.optionalWith(_Compute, { default: () => _Compute.make({ cpu: _Quantity.make("500m"), memory: _Quantity.make("256Mi") }) }),
  clients: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 10000)), { default: () => 1000 }),
  sessions: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 1000)), { default: () => 20 }),
})

const _Data = Schema.Struct({
  instances: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 9)), { default: () => 2 }),
  storage: Schema.optionalWith(_Quantity, { default: () => _Quantity.make("20Gi") }),
  requests: Schema.optionalWith(_Compute, { default: () => _Compute.make({ cpu: _Quantity.make("500m"), memory: _Quantity.make("2Gi") }) }),
  limits: Schema.optionalWith(_Compute, { default: () => _Compute.make({ cpu: _Quantity.make("2"), memory: _Quantity.make("4Gi") }) }),
  backupCron: Schema.optionalWith(_Cron, { default: () => _Cron.make("0 0 3 * * *") }),
  retention: Schema.optionalWith(_Window, { default: () => _Window.make("30d") }),
  pooling: Schema.optionalWith(_Pooling, { default: () => "session" as const }),
  pool: Schema.optionalWith(_Pool, { default: () => _Pool.make({}) }),
  primitives: Schema.optionalWith(Schema.Array(Schema.NonEmptyString), { default: () => [] }),
})

const _Fanout = Schema.Struct({
  replicas: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 5)), { default: () => 3 }),
  storage: Schema.optionalWith(_Quantity, { default: () => _Quantity.make("2Gi") }),
})

// Governance columns the isolation tier READS: a namespace quota, the owning group's spelling, the pull
// registries a tenant may reach, and the ingress classes it may claim. Each is estate policy, so it enters
// here as a defaulted coordinate and the owning tier renders it — a literal inside a mode row is the same
// policy wearing a module constant's clothes, which is what forced a per-estate quota into lib code.
const _Governance = Schema.Struct({
  // the Capsule CRD floors the namespace quota at one; the ceiling is this estate's own bound
  quota: Schema.optionalWith(Schema.Int.pipe(Schema.between(1, 100)), { default: () => 5 }),
  // the SUFFIX the owning group's name carries — `_MODES` composes `${tenant}-${ownerGroup}` — so the
  // convention has one edit site and the default preserves the spelling the retired literal already bound
  ownerGroup: Schema.optionalWith(Schema.NonEmptyString, { default: () => "admin" }),
  // empty means unrestricted on both rows: a governance axis states its restriction or declines to hold one
  registries: Schema.optionalWith(Schema.Array(Schema.NonEmptyString), { default: () => [] }),
  ingressClasses: Schema.optionalWith(Schema.Array(Schema.NonEmptyString), { default: () => [] }),
})

const _Isolated = Schema.Struct({
  pgTier: Schema.optionalWith(Schema.Literal("shared-rls", "db-per-tenant", "cluster-per-tenant"), { default: () => "shared-rls" as const }),
  tenants: Schema.NonEmptyArray(_Slug),
  ..._Governance.fields,
})

// How the control plane separates tenants, which is a Kubernetes boundary posture rather than the consumption
// tenancy axis: the two share no vocabulary, and `_Profile.topology` is where this spec spells a consumption axis.
const _Separation = Schema.Union(
  Schema.Struct({ mode: Schema.Literal("single") }),
  Schema.Struct({ mode: Schema.Literal("namespace"), ..._Isolated.fields }),
  Schema.Struct({ mode: Schema.Literal("vcluster"), ..._Isolated.fields }),
)

// Every escalation arm the estate rules OFF resolves to one literal here, so `libs/.planning/ARCHITECTURE.md`
// `[FLEET_ESCALATION]` names a spec value per row and arming stays a coordinate flip.
const _Escalation = {
  sampling: Schema.Literal("head", "tail"),
  topology: Schema.Literal("gateway", "agent"),
  // an escalation with no coordinate is unrepresentable: the broker arm carries the addresses it cannot run without,
  // so selecting it against an absent broker estate fails at decode instead of rendering a pipeline that never connects
  buffer: Schema.Union(
    Schema.Struct({ mode: Schema.Literal("file") }),
    Schema.Struct({ mode: Schema.Literal("broker"), brokers: Schema.NonEmptyArray(Schema.NonEmptyString) }),
  ),
} as const

const _Observe = Schema.Struct({
  store: Schema.optionalWith(Schema.Literal("prometheus", "mimir", "victoriametrics"), { default: () => "prometheus" as const }),
  retention: Schema.optionalWith(_Window, { default: () => _Window.make("30d") }),
  profiles: Schema.optionalWith(Schema.Boolean, { default: () => true }),
  ingest: Schema.optionalWith(Schema.Literal("scrape", "native"), { default: () => "scrape" as const }),
  costs: Schema.optionalWith(Schema.Boolean, { default: () => false }),
  ebpf: Schema.optionalWith(Schema.Boolean, { default: () => false }),
  // residence selection stays metrics-independent: a store retention window bounds series, never evidence
  analytics: Schema.optionalWith(Schema.Literal("none", "lake", "clickhouse", "both"), { default: () => "lake" as const }),
  sampling: Schema.optionalWith(_Escalation.sampling, { default: () => "head" as const }),
  topology: Schema.optionalWith(_Escalation.topology, { default: () => "gateway" as const }),
  buffer: Schema.optionalWith(_Escalation.buffer, { default: () => ({ mode: "file" as const }) }),
})

// `objective` DERIVES and stores nothing: an operator declares topology, and the durability window follows from it
// through the one `proc/config#ADMISSION_ROWS` table, so this plane and the process it deploys grade against one
// pair. Storing it forks that answer the moment an operator moves topology and leaves the window defaulted.
// Deriving also makes `StackSpec.Profile` satisfy `operate/converge` `Converge.Profile` structurally, so that tier
// takes a profile row rather than a `StackSpec` and any composition root outside this estate converges identically
// off the same two members.
class _Profile extends Schema.Class<_Profile>("StackSpec.Profile")({
  scale: Schema.optionalWith(Schema.Literal("dev", "standard", "fleet"), { default: () => "standard" as const }),
  topology: Schema.optionalWith(Schema.Literal(...ConsumptionProfile.topologies), { default: () => "service" as const }),
  compute: Schema.optionalWith(Schema.Literal("serverless", "cluster"), { default: () => "serverless" as const }),
  capacity: Schema.optionalWith(_Capacity, { default: () => _Capacity.make({}) }),
  extensions: Schema.optionalWith(Schema.Array(Schema.NonEmptyString), { default: () => [] }),
  objectEngine: Schema.optionalWith(Schema.Literal("minio", "ceph"), { default: () => "minio" as const }),
  exposure: Schema.optionalWith(Schema.Literal("direct", "tunnel", "internal"), { default: () => "direct" as const }),
  data: Schema.optionalWith(_Data, { default: () => _Data.make({}) }),
  fanout: Schema.optionalWith(_Fanout, { default: () => _Fanout.make({}) }),
  observe: Schema.optionalWith(_Observe, { default: () => _Observe.make({}) }),
  separation: Schema.optionalWith(_Separation, { default: () => ({ mode: "single" as const }) }),
}) {
  get objective(): Consumption.Objective {
    return ConsumptionProfile.recoveryOf(this.topology)
  }
}

class StackSpec extends Schema.Class<StackSpec>("StackSpec")({
  name: _Name,
  app: Identity.App.fields.app,
  target: Schema.Literal(..._arms),
  backend: Schema.optionalWith(Schema.Literal("self-managed", "cloud"), { default: () => "self-managed" as const }),
  region: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  domain: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  zone: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  project: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  account: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  connection: Schema.optionalWith(Connection, { as: "Option" }),
  image: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  doppler: _Doppler,
  epoch: Schema.optionalWith(Schema.NonEmptyString, { default: () => "0" }),
  profile: Schema.optionalWith(_Profile, { default: () => _Profile.make({}) }),
}) {
  static readonly arms: StackSpec.Arms = _arms
  static readonly topologies: StackSpec.Topologies = ConsumptionProfile.topologies
  get hosted(): boolean {
    return this.backend === "cloud"
  }
  get tenants(): ReadonlyArray<StackSpec.Slug> {
    return this.profile.separation.mode === "single" ? [] : this.profile.separation.tenants
  }
  // `single` carries no tier field, so this projection seats the shared tier and every data-plane
  // consumer reads one total value instead of re-deriving the mode-to-tier correspondence.
  get pgTier(): StackSpec.PgTier {
    return this.profile.separation.mode === "single" ? "shared-rls" : this.profile.separation.pgTier
  }
}

declare namespace StackSpec {
  type Arms = typeof _arms
  type Arm = (typeof _arms)[number]
  type Topologies = Consumption.Topologies
  type Topology = Consumption.Topology
  type Slug = typeof _Slug.Type
  type Quantity = typeof _Quantity.Type
  type Window = typeof _Window.Type
  type Connection = InstanceType<typeof Connection>
  type Capacity = typeof _Capacity.Type
  type Data = typeof _Data.Type
  type Pooling = Data["pooling"]
  type Pool = typeof _Pool.Type
  type Observe = typeof _Observe.Type
  type Residence = Observe["analytics"]
  type Profile = InstanceType<typeof _Profile>
  type Separation = typeof _Separation.Type
  type Governance = typeof _Governance.Type
  type PgTier = (typeof _Isolated.Type)["pgTier"]
  type _Keys<K extends Arm = (typeof _arms)[number]> = K
  type _Axes<K extends Topology = Profile["topology"]> = K
}
```

## [04]-[TIER_BASE]

[TIER_BASE]:
- Owner: `Tier`, the abstract `pulumi.ComponentResource` subclass every grouped concern extends — the constructor stamps the type token `rasm:iac:<Kind>`, `child(overrides?)` folds `{ parent: this }` into per-resource overrides through `pulumi.mergeOptions` so ownership is inherited and never restated, `hooked(rows, overrides?)` folds the named lifecycle-hook binding onto the same channel, and `seal(outputs)` is the mandatory terminal `registerOutputs` call.
- Law: the constructor is the platform seam — Pulumi's model is class heritage with field assignment, so `super(...)`, child construction, and readonly field assignment are the exemption's whole extent; a tier member beyond the constructor is an expression-shaped projection over already-constructed outputs.
- Law: options are algebra, not assembly — `child()` is the only way a child receives options: `parent` rides the fold, an explicit `provider`/`providers` set at tier construction flows down the chain, `dependsOn` states only genuine extra-graph edges (an `Output` reference already is one), `protect: true` marks tiers owning irreplaceable state, `aliases` accompany a rename so state survives it, and `ignoreChanges` quarantines fields an operator mutates out-of-band — the fold is the single channel every option class travels.
- Law: lifecycle interception is registry data — `hooked(rows, overrides?)` is the one hook spelling: each `_HOOKS` point row binds a named `ResourceHook` as `rasm.iac.<tier>.<point>`, `onError` binds the named `ErrorHook` the same way, and the assembled `ResourceHookBinding` rides the same `child()` fold as every option class, so a tier earning interception states rows, never callbacks at call sites; the engine demands named instances for the delete points and error hooks, which the registry grammar satisfies for every point uniformly, and a `before<Point>` row that rejects fails the action while an `after<Point>` row only warns — posture is the engine's, the name is the registry's.
- Law: `Tier.harden` is the estate's ONE privilege posture and it seats here because three tiers on two strata stamp it — `kube/workload`'s Deployment and CronJob pods, `kube/traffic`'s tunnel connector, and `operate/converge`'s runner Jobs — and the lowest stratum every one of them reaches is the base they all extend; a posture declared on whichever tier happened to need it first forces an upward import from `operate` or a second literal at every other construction site, and two privilege postures under one estate is the divergence a mandatory hardening gate then reports as a violation of itself. The anchor carries the pod block (non-root identity, the unprivileged uid/gid pair, the `RuntimeDefault` seccomp filter every container inherits) beside the container block (no privilege escalation, a read-only root filesystem, every capability dropped, the same filter restated where the container level outranks the pod level) and the scratch `emptyDir` pair the read-only root owes — `doppler run --` and every temp write need a writable directory and an image layer is not one, so the pair rides the same anchor as the refusal that makes it necessary and a hardened pod cannot ship without it. `operate/policy.md`'s `workload-hardened` row asserts this stamp, so a new refusal is one field here and one roster entry there.
- Law: `seal` closes every constructor — an unsealed tier reports no outputs and its dependents race construction; the sealed record is the tier's public evidence and mirrors the readonly fields the class exposes.
- Law: adoption is not composition — a `ComponentResource` has no `static get`; a pre-existing cloud object adopts through its own resource class `get` or `opts.import` inside the owning tier, and the tier remains the sole author thereafter.
- Law: the tier tree is closed and page-owned — `Bootstrap` (`provider.md`), `Source` (`program/source.md`), `Secrets`/`Certs` (`operate/secret.md`), `ObjectStore`/`Nats`/`Postgres` (`kube/data.md`), `Workload` (`kube/workload.md`), `Traffic` (`kube/traffic.md`), `Tenants` (`kube/tenant.md`), `Lgtm`/`Boards` (`operate/observe.md`), `Reconcile` (`operate/policy.md`), `CloudPlane` (`operate/cloud.md`) — each a subclass whose declaration and invariants live on its owning page; a concern with no tier row composes inside an existing tier before a new subclass is minted, and a rename travels as an `aliases` row, never a silent replacement.
- Growth: a new tier is one subclass row on its owning page with its roster mention here; a new interception point is one `_HOOKS` entry; a new privilege refusal is one `_HARDEN` field; the base never grows knobs.
- Packages: `@pulumi/pulumi` (`ComponentResource`, `ComponentResourceOptions`, `CustomResourceOptions`, `mergeOptions`, `Inputs`, `ResourceHook`, `ErrorHook`, `ResourceHookBinding`, `ResourceHookFunction`, `ErrorHookFunction`); `@pulumi/kubernetes` (`types.input.core.v1.{PodSecurityContext,SecurityContext,Volume,VolumeMount}`); `effect` (`Record`).

```typescript signature
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Record } from "effect"

const _HOOKS = ["beforeCreate", "afterCreate", "beforeUpdate", "afterUpdate", "beforeDelete", "afterDelete"] as const

const _SCRATCH = { name: "scratch", path: "/tmp" } as const

// Deploy-owned posture, never a runtime mirror: the pod block owns identity and the seccomp filter every
// container inherits, the container block owns the refusals a container-level setting outranks the pod on,
// and the scratch pair is what makes the read-only root survive `doppler run --` and every temp write. It
// seats on the base because the workload, traffic, and converge tiers all stamp it and only the base sits
// below all three — a second copy is a second estate posture the mandatory gate then reports against itself.
const _HARDEN: Tier.Harden = {
  pod: { runAsNonRoot: true, runAsUser: 65532, runAsGroup: 65532, fsGroup: 65532, seccompProfile: { type: "RuntimeDefault" } },
  container: {
    runAsNonRoot: true,
    allowPrivilegeEscalation: false,
    readOnlyRootFilesystem: true,
    capabilities: { drop: ["ALL"] },
    seccompProfile: { type: "RuntimeDefault" },
  },
  volumes: [{ name: _SCRATCH.name, emptyDir: {} }],
  mounts: [{ name: _SCRATCH.name, mountPath: _SCRATCH.path }],
}

declare namespace Tier {
  type Point = (typeof _HOOKS)[number]
  type Hooks = { readonly [P in Point]?: pulumi.ResourceHookFunction } & { readonly onError?: pulumi.ErrorHookFunction }
  type Harden = {
    readonly pod: k8s.types.input.core.v1.PodSecurityContext
    readonly container: k8s.types.input.core.v1.SecurityContext
    readonly volumes: ReadonlyArray<k8s.types.input.core.v1.Volume>
    readonly mounts: ReadonlyArray<k8s.types.input.core.v1.VolumeMount>
  }
}

abstract class Tier extends pulumi.ComponentResource {
  static readonly harden: Tier.Harden = _HARDEN
  readonly #kind: string
  constructor(kind: string, name: string, opts?: pulumi.ComponentResourceOptions) {
    super(`rasm:iac:${kind}`, name, {}, opts)
    this.#kind = kind
  }
  protected child(overrides?: pulumi.CustomResourceOptions): pulumi.CustomResourceOptions {
    return pulumi.mergeOptions({ parent: this }, overrides)
  }
  protected hooked(rows: Tier.Hooks, overrides?: pulumi.CustomResourceOptions): pulumi.CustomResourceOptions {
    return this.child(pulumi.mergeOptions({
      hooks: {
        // every point mints a NAMED instance under the registry grammar, satisfying the engine's named-hook demand on delete and error points uniformly
        ...Record.fromEntries(_HOOKS.flatMap((point) =>
          rows[point] === undefined
            ? []
            : [[point, [new pulumi.ResourceHook(`rasm.iac.${this.#kind}.${point}`, rows[point])]] as const])),
        ...(rows.onError !== undefined && { onError: [new pulumi.ErrorHook(`rasm.iac.${this.#kind}.onError`, rows.onError)] }),
      },
    }, overrides))
  }
  protected seal(outputs: pulumi.Inputs): void {
    this.registerOutputs(outputs)
  }
}
```

## [05]-[OUTPUT_PLANES]

[OUTPUT_PLANES]:
- Owner: `StackOutputs`, one `Schema.Class` of `Option`-carried plane records — `ingress` (public hostname), `data` (host, port, database, role, realized pooling mode), `object` (endpoint, bucket), `fanout` (the NATS websocket origin), `otlp` (collector ingest endpoint), `grafana` (board URL), `analytics` (realized residence beside its query door and catalog name), `sharding` (runner endpoint), `served` (caller-owned artifact slug to content-addressed path), `deploy` (the time-ordered `RandomUuid7` deployment identity) — each an inline `Schema.Struct` block because no plane has a second consumer shape, while `served` is an open `Schema.Record` because its key vocabulary belongs to its caller; the arm that realizes a plane returns its keys from the `PulumiFn`, and absence means the arm did not realize it.
- Law: `analytics` publishes the REALIZED residence row a query end binds, never the selection and never the ingest path — `observe.analytics: "none"` realizes no plane at all, so a runtime reading evidence resolves absence as the refusal it is instead of dialing a door that answers nothing; the `lake` arm's door is the object plane's own endpoint under a catalog prefix, and `both` publishes the interactive residence here while the cold tail stays readable off `object`, which is why the row key and not the spec value crosses — a reader handed `both` holds a coordinate its residence family cannot resolve.
- Law: `StackOutputs.channels` is the deploy-to-process seam's one key catalog and it is TOTAL over the derived channel union — `Channel` projects `<plane>.<field>` off the ENCODED plane record, so a new plane field with no key, or a key naming a field the class lost, is a compile error at the catalog rather than a variable that quietly stops being written; the `kube/workload.md` env derivation reads this owner and holds no map of its own, and `served` owns no row because its key vocabulary belongs to its caller and it exits as a plane.
- Law: `StackOutputs.custody` and `StackOutputs.backend` complete that one catalog — custody variables reach their mint and stamp, while the backend root and pointer variables reach every workload realizer. Each deploy writer reads this record rather than spelling a literal a sibling must match by hand; the runtime owner retains the same exact names at its import-free boundary.
- Law: a key is the READING contract's spelling, never the writer's convenience — the runtime `Setting` groups resolve `<GROUP>_<ROW>` through `Config.nested`, so `otlp.endpoint` publishes `RUNTIME_OTEL_ORIGIN` and the OpenTelemetry SDK's ambient `OTEL_EXPORTER_OTLP_ENDPOINT` is the deleted spelling: this estate's export lanes read `Setting.otel`, so a deploy plane writing the SDK's name publishes a variable the app's provider chain never opens, and the collector coordinate arrives nowhere while both ends look correct in isolation.
- Law: `pairsOf(planes, render)` is the one channel-flatten — the `<plane>.<field>` spelling and the plane iteration exist exactly here, parameterized on the value renderer; the decoded `pairs` getter rides it with `String`, the in-program live assembly rides it with `pulumi.output(value).apply(String)`, and the plane set feeding the getter derives from the class's own field record through `Record.getSomes`, so no hand-listed plane tuple exists, a new field cannot be silently dropped, and the two modalities cannot drift.
- Law: `read(stack, name)` is the one exit from the engine's `OutputMap` — `stack.outputs()` converts at this seam with the `DeployFault` triage, one entries scan yields both the secret-refusal verdict and the leaked-key evidence (the gate refuses any `{ secret: true }` entry naming the keys in the fault detail), the `{ value, secret }` envelope strips to plain values, and the record decodes through the class; the `Object` reads sit inside the boundary because the map is FFI material, and no decoded value is re-checked downstream.
- Law: coordinates, never material — a role name, host, port, origin, or URL is publishable; a password, token, or key is not, and the fix for a refused output is moving the value into the Doppler store, never widening the gate.
- Law: decode failure is admission evidence — the configured decode (`errors: "all"`, `onExcessProperty: "error"`) makes an output key no field admits, or a malformed plane record, fail loudly and re-spell the `ParseError` as an `input` fault, because the program and this owner are two spellings of one contract and drift between them is a defect at the seam.
- Law: `sharding` is the sole value crossing back to the runtime graph — `work`'s `ShardingConfig.layerFromEnv` consumes the env rows the sharding channels populate, deployment topology stays plane-distinct, and no runtime import exists in either direction; every other plane serves the app's own boot config through the same env assembly, and the variable a channel lands on is `channels`' row here, so the spelling lives beside the plane declaration that mints it rather than in whichever tier happens to render the container.
- Law: the projection is total over presence — absent planes contribute zero rows, values render through the injected renderer at this seam exactly once, and a consumer never re-derives a pair from the decoded owner; the widened `Record<string, string | number>` view on the fold is the type-seam bracket posture, since every plane record is flat scalars by construction.
- Entry: `StackOutputs.read(stack, spec.name)` after any `up`; the plane records project by field access; `outputs.pairs` into the workload env assembly; `StackOutputs.pairsOf(record, render)` inside a program body over live `Output`s.
- Growth: a new plane is one `Option` field, its arm return keys, and one `_CHANNELS` row per field; a custody or backend variable is one row in its sibling catalog reaching every deploy writer.
- Boundary: which keys each arm returns is `provider.md`'s program body; how a channel row becomes a container `EnvVar` is `kube/workload.md`'s rendering; which custody variables a cell holds is `operate/secret.md`'s mint; the reading side's group nesting is the runtime `Setting` owner's; receipt evidence is `automation.md`'s — outputs and receipts never merge.
- Packages: `effect` (`Effect`, `Schema`, `Option`, `Array`, `Record`); `@pulumi/pulumi` (`Output`); `@pulumi/pulumi/automation` (`Stack`); `@rasm/ts/core` (`Shape.Record`); `./automation.ts` (`DeployFault`).

```typescript signature
import type { Stack } from "@pulumi/pulumi/automation"
import { Array, Effect, Option, Record, Schema } from "effect"
import { Shape } from "@rasm/ts/core"
import { DeployFault } from "./automation.ts"

const _Port = Schema.Int.pipe(Schema.between(1, 65535))

// The deploy-to-process seam's ONE key catalog, total over the derived channel union: a plane field with no
// key, or a key naming a field the class lost, is a compile error here rather than a variable that silently
// stops being written. Spellings are the RUNTIME contract's own — `Config.nested` composes group and row
// into `<GROUP>_<ROW>` — because the reading process is what must resolve the value; the OTLP row is that
// law's proof, since the SDK's ambient `OTEL_EXPORTER_OTLP_ENDPOINT` is a name this estate's provider chain
// never reads. `served` owns no row: its key vocabulary belongs to its caller, so it exits as a plane.
const _CHANNELS: { readonly [C in StackOutputs.Channel]: string } = {
  "ingress.hostname": "IAC_INGRESS_HOSTNAME",
  "data.host": "DATA_PG_HOST",
  "data.port": "DATA_PG_PORT",
  "data.database": "DATA_PG_DATABASE",
  "data.role": "DATA_PG_ROLE",
  "data.pooling": "DATA_PG_POOLING",
  "object.endpoint": "OBJECT_ENDPOINT",
  "object.bucket": "OBJECT_BUCKET",
  "fanout.origin": "RUNTIME_FANOUT_ORIGIN",
  "otlp.endpoint": "RUNTIME_OTEL_ORIGIN",
  "grafana.url": "IAC_GRAFANA_URL",
  "analytics.residence": "IAC_ANALYTICS_RESIDENCE",
  "analytics.endpoint": "IAC_ANALYTICS_ENDPOINT",
  "analytics.database": "IAC_ANALYTICS_DATABASE",
  "sharding.host": "IAC_SHARDING_HOST",
  "sharding.port": "IAC_SHARDING_PORT",
  "deploy.id": "IAC_DEPLOY_ID",
} as const

// The catalog's second half: the two variables a namespace custody cell may carry — the config-scoped token
// every process resolves through, and the encoded lease boundary the security custodian decodes its whole
// scope out of. `operate/secret.md` mints the cell against this roster and `kube/workload.md` stamps rows off
// it, so a cell states which of them it holds in DATA and no flag decides the stamping at either end.
const _CUSTODY = { token: "DOPPLER_TOKEN", lease: "SECURITY_LEASE_SPEC" } as const
const _BACKEND = {
  root: "RASM_BACKEND_CONTRACT_ROOT",
  pointer: "RASM_BACKEND_POINTER_PATH",
} as const

class StackOutputs extends Schema.Class<StackOutputs>("StackOutputs")({
  ingress: Schema.optionalWith(Schema.Struct({ hostname: Schema.NonEmptyString }), { as: "Option" }),
  data: Schema.optionalWith(Schema.Struct({
    host: Schema.NonEmptyString,
    port: _Port,
    database: Schema.NonEmptyString,
    role: Schema.NonEmptyString,
    pooling: _Pooling,
  }), { as: "Option" }),
  object: Schema.optionalWith(Schema.Struct({ endpoint: Schema.NonEmptyString, bucket: Schema.NonEmptyString }), { as: "Option" }),
  fanout: Schema.optionalWith(Schema.Struct({ origin: Schema.NonEmptyString }), { as: "Option" }),
  otlp: Schema.optionalWith(Schema.Struct({ endpoint: Schema.NonEmptyString }), { as: "Option" }),
  grafana: Schema.optionalWith(Schema.Struct({ url: Schema.NonEmptyString }), { as: "Option" }),
  analytics: Schema.optionalWith(Schema.Struct({
    // Doors publish their REALIZED residence row, never the spec selection: `both` names two planes and no
    // residence family holds it as a key, so admitting it here types a query end against a row it can never resolve
    residence: Schema.Literal("lake", "clickhouse"),
    endpoint: Schema.NonEmptyString,
    database: Schema.NonEmptyString,
  }), { as: "Option" }),
  sharding: Schema.optionalWith(Schema.Struct({ host: Schema.NonEmptyString, port: _Port }), { as: "Option" }),
  served: Schema.optionalWith(Shape.Record(Schema.NonEmptyString, Schema.NonEmptyString), { as: "Option" }),
  deploy: Schema.optionalWith(Schema.Struct({ id: Schema.UUID }), { as: "Option" }),
}) {
  static readonly channels: StackOutputs.Channels = _CHANNELS
  static readonly custody: StackOutputs.Custody = _CUSTODY
  static readonly backend: StackOutputs.Backend = _BACKEND
  static readonly pairsOf = <V, R>(
    planes: Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, V>>,
    render: (value: V) => R,
  ): ReadonlyArray<readonly [channel: string, value: R]> =>
    Array.flatMap(Record.toEntries(planes), ([plane, held]) =>
      Array.map(Record.toEntries(held), ([field, value]) => [`${plane}.${field}`, render(value)] as const))
  static readonly read = (stack: Stack, name: string): Effect.Effect<StackOutputs, DeployFault> =>
    Effect.tryPromise({ try: () => stack.outputs(), catch: DeployFault.triaged(name) }).pipe(
      Effect.flatMap((outputs) => {
        const entries = Object.entries(outputs)
        const leaked = entries.filter(([, entry]) => entry.secret === true).map(([key]) => key)
        return leaked.length === 0
          ? Effect.succeed(Object.fromEntries(entries.map(([key, entry]) => [key, entry.value])))
          : Effect.fail(new DeployFault({ case: { reason: "input", stack: name, detail: leaked.join(",") } }))
      }),
      Effect.flatMap((record) =>
        Effect.mapError(
          Schema.decodeUnknown(StackOutputs, { errors: "all", onExcessProperty: "error" })(record),
          (parse) => new DeployFault({ case: { reason: "input", stack: name, detail: parse.message } }),
        )),
    )
  get pairs(): ReadonlyArray<StackOutputs.Pair> {
    const held: Record.ReadonlyRecord<string, Option.Option<Record.ReadonlyRecord<string, string | number>>> =
      Record.map(StackOutputs.fields, (_, plane) => this[plane])
    return StackOutputs.pairsOf(Record.getSomes(held), String)
  }
}

declare namespace StackOutputs {
  type Pair = readonly [channel: string, value: string]
  // the channel union DERIVES off the encoded plane record, so `Option` unwraps to an optional key and every
  // field of every fixed-shape plane appears exactly once — no hand-listed plane tuple, no missed field
  type Planed = Exclude<keyof typeof StackOutputs.Encoded, "served">
  type Channel = {
    readonly [P in Planed]: `${P & string}.${keyof NonNullable<(typeof StackOutputs.Encoded)[P]> & string}`
  }[Planed]
  type Channels = typeof _CHANNELS
  type Backend = typeof _BACKEND
  type Custody = typeof _CUSTODY
  type Held = keyof Custody
  // What a workload mounts: one namespace cell beside the roster of variables it carries, so the minting tier
  // and the stamping tier pass ONE shape and neither re-declares the roster as a hand-written literal union.
  type Cell = { readonly secret: pulumi.Output<string>; readonly carries: ReadonlyArray<Held> }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Connection, StackOutputs, StackSpec, Tier }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
