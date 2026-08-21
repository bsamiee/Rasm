# [IAC_POLICY]

`Guard`, `Drift`, and `Reconcile` own the policy plane's verdict directions. `Guard` judges desired state before apply through one `PolicyPackArgs` value of policies-as-data rows attached by `Automation.Options.policyPacks`. Typed helpers narrow each policy onto the resource class its tier constructs: digest-pinned images, non-superuser Postgres roles, TLS edges, protected data with scheduled backups, namespace fences, and managed-by stamps. `Drift` projects `Automation.reconcile` receipts into `DriftReport` rows with docker-cell store conformance. `Reconcile` carries typed `Stack` CR rows through the Pulumi Kubernetes Operator. `Evidence` unifies run settlement, drift, rotation, secret-change, and hosted-webhook deliveries under one tagged union and never-failing sink. Growth is one policy row, report field, evidence case, or `Stack` CR row.

## [01]-[INDEX]

- [02]-[PACK_ASSEMBLY]: the pack value, enforcement vocabulary, compliance frames; `Guard`.
- [03]-[POLICY_ROWS]: the typed validation, stack-invariant, and remediation rows; `Guard`.
- [04]-[DRIFT_REPORT]: the report owner: drifted rows, rotation watch, skew evidence; `Drift`.
- [05]-[EVIDENCE_SPINE]: the typed evidence union, the sink contract, file sink, sweep cursor; `Evidence`.
- [06]-[DRIFT_SWEEP]: the reconcile projection, the fleet sweep, the conformance read-back; `Drift`.
- [07]-[RECONCILE_LOOP]: the in-cluster PKO operator and its typed Stack CR rows; `Reconcile`.

## [02]-[PACK_ASSEMBLY]

[PACK_ASSEMBLY]:
- Owner: `Guard`, the `PolicyPackArgs` value — the `policies` array is the whole pack, `enforcementLevel: "mandatory"` is the pack default each row may override (`"advisory"` for stamps, `"remediate"` for fix-forward rows), and metadata (`description`, `severity`, `framework`, `remediationSteps`) rides each row as data the engine surfaces with the violation.
- Law: compliance is a frame on the row — `_CIS` is the one `PolicyComplianceFramework` vocabulary value, stamped on the rows whose invariant realizes a benchmark control, so an auditor reads coverage off the pack value and a compliance mapping is a `framework` field, never a parallel document.
- Law: the pack module is pure and the analyzer entry is a boot edge — this module exports the args value; a one-line entry module (`new PolicyPack("rasm-guard", Guard)`) is the analyzer process's own top level, executed by the engine's policy plugin, and `Automation` attaches the entry's path via `policyPacks`, so the lib stays side-effect-free and the pack still gates every run.
- Law: violations are receipt material — the engine folds `ReportViolation` calls into the run's policy events; a `mandatory` violation fails the run before apply, and the receipt's diagnostics carry the evidence, so gating and reporting are one stream.
- Law: configuration is typed at the row — a policy with knobs declares `configSchema` and reads `args.getConfig<T>()`; a config-less policy declares none, knob defaults live in the schema, never in validator bodies, and a per-app enforcement override is `PolicyPackConfig` data at the entry, never a pack edit.
- Growth: one policy row per invariant; a new benchmark is one frame value stamped on the rows it maps.
- Boundary: attachment plumbing is `program/automation.md`'s options row; the narrowed classes are the tier pages' constructions; enforcement semantics (`remediate` apply order, `mandatory` abort) are the engine's contract.
- Packages: `@pulumi/policy` (`PolicyPackArgs`, `ResourceValidationPolicy`, `StackValidationPolicy`, `PolicyResourceOptions`, the typed helper family, `PolicyComplianceFramework`, `Secret`); `@pulumi/kubernetes`, `@pulumi/postgresql` (the narrowed classes).

## [03]-[POLICY_ROWS]

[POLICY_ROWS]:
- Law: authorship is the narrowing every estate-invariant row states first — `helm.v4.Chart` renders server-side and hands EVERY manifest to the provider, so the capsule, CNPG, barman, external-dns, PKO, and object-plane controller Deployments reach this analyzer exactly as the tier-authored ones do, and a mandatory row asserting an estate stamp over all of them fails every run of the arm that installs them. `_authored` is that discriminant and it reads the one fact no upstream chart can carry: every resource a tier constructs receives `parent: this` through the `Tier` option fold, so its parent URN spells the `rasm:iac:<Kind>` type token while a chart-rendered object's parent is the chart component. A row judging what this estate DECLARES states the predicate; a row judging what any workload must SATISFY regardless of author (a wildcard IAM grant, a superuser role, a TLS-less edge) does not, and the split is the difference between an invariant and an audit of somebody else's chart.
- Law: image provenance is structural — `image-digest-pinned` narrows `k8s.apps.v1.Deployment` under `_authored` and reports every container whose image lacks an `@sha256:` digest, with a typed `configSchema` allowlisting registries a proof stack may pull mutable; the `kube/workload` digest law compiles to this gate, so a mutable tag cannot reach a cluster even from an app-authored program while an upstream chart's own tag stays the chart pin's business.
- Law: the data plane cannot escalate on ANY arm — `role-no-superuser` narrows the bridged `postgresql.Role` class (the docker cell's spelling), and `managed-role-no-superuser` walks the stack for CNPG `Cluster` CRs (matched on the CR's own `apiVersion`/`kind` props, the carrier's stable discriminant) and reports any `spec.managed.roles[]` row carrying `superuser: true` — the primary arm's managed roles are guarded by the same law the bridged class row enforces, one invariant, two carrier spellings.
- Law: the data plane cannot vanish or run unarchived — `data-plane-protected` demands `opts.protect` on every CNPG `Cluster`, and `backup-beside-cluster` is the dependency-aware cross-resource row: every `Cluster` CR must have a `ScheduledBackup` CR whose `spec.cluster.name` references it, so an unprotected or unarchived database is unshippable.
- Law: traffic is TLS-only at both edges — `ingress-tls-required` narrows the legacy `k8s.networking.v1.Ingress` and rejects a spec whose `tls` block is empty; `gateway-tls-required` walks Gateway API `Gateway` CRs and rejects a listener set with no `HTTPS`-terminating member carrying `certificateRefs`; the `kube/traffic` sink law becomes machine pressure on whichever edge row the estate selects.
- Law: privilege posture compiles to a gate — `workload-hardened` narrows `k8s.apps.v1.Deployment` under `_authored` and asserts the `Tier.harden` stamp landed: the pod block's non-root identity and `RuntimeDefault` seccomp filter, and every container's dropped capabilities, refused escalation, read-only root, own non-root claim, and own filter restatement; the two rosters stay separate because a container-level setting outranks the pod's, so a compliant pod proves nothing about the container beneath it, and a `Tier.harden` field added upstream is one roster entry here — the container roster carries the seccomp row for exactly that reason, since the anchor restates the filter at the level that outranks and a roster silent on it proves only the level that does not. This is the pattern the digest row already sets — the tier stamps the invariant, the pack proves it held even on an app-authored program.
- Law: workloads carry fences — `namespace-network-fence` narrows the stack to the `Deployment` class through `validateStackResourcesOfType` and demands a `k8s.networking.v1.NetworkPolicy` beside any member, judged over the dependency-aware `PolicyResource` graph because presence-beside is a cross-resource fact no single-resource validator can see.
- Law: remediation and validation are one callback — `managed-by-stamp` rides `validateRemediateResourceOfType` under `_authored`, whose single callback yields both halves spread into the row: the returned prop bag fixes forward under `"remediate"`, and the same callback judges under stricter levels; the narrowing is load-bearing rather than tidy, because remediation runs BEFORE validation and an unnarrowed stamp writes the estate's ownership label onto every chart-rendered controller in the graph, which is both a foreign object's field this pack never authored and the erasure of the only label an ownership question could have read.
- Law: cloud arms gate at the same bar — `bucket-versioned-aws` is the dependency-aware durability row (every `aws.s3.BucketV2` must have a `BucketVersioningV2` beside it, the same presence-beside shape as the backup row), `bucket-versioned-gcp` narrows `gcp.storage.Bucket` and rejects a spec whose `versioning.enabled` is not true, `iam-floor` admits string and object policy documents through one `_IamPolicy` schema, normalizes singular and array statement forms, marks malformed documents not applicable, and rejects an admitted granting statement whose string action set carries a wildcard — an explicit `Deny` wildcard is the hardening posture and passes; `tenant-fence` is the tenancy presence-beside row (every Capsule `Tenant` must have a `GlobalTenantResource` whose tenant selector names it, since the fence rides a replication rather than the CR's own deprecated block), and `tenant-no-deprecated-spec` refuses a tenant composing any superseded governance block — `networkPolicies`, `containerRegistries`, `limitRanges`, `imagePullPolicies` — because the operator still serves each and drops them without a diff to warn on.
- Law: preview-unknowns are engine-guarded — the policy host wraps every validator's deserialized props in its own unknown-checking proxy and converts the raised unknown-value signal into the advisory verdict, so a validator body never guards a possibly-unknown read and never throws one; a hand guard around prop reads restates the engine's own seam.
- Law: validators read unwrapped props totally — optional chains over the generated arg shapes, `report(message, urn?)` once per finding, `args.notApplicable(reason)` where a policy cannot judge a resource; a validator that throws is a defect, not a verdict; the validator bodies are boundary-framework kernels over foreign prop bags, the one place native iteration is ruled.
- Growth: one row per invariant appended to `_policies`; a new cloud-arm invariant is one row narrowing that provider's class; a new tenancy governance axis is one row beside `tenant-fence`; a new privilege refusal is one `_POD_HELD` or `_CONTAINER_HELD` entry, never a second policy; a new estate-authoring tier inherits `_authored` with no edit, because the predicate reads the base's own type token rather than a roster of tier names.

```typescript
import * as aws from "@pulumi/aws"
import * as gcp from "@pulumi/gcp"
import * as k8s from "@pulumi/kubernetes"
import * as policy from "@pulumi/policy"
import * as postgresql from "@pulumi/postgresql"
import { Either, Schema } from "effect"

type _DigestConfig = { readonly allowRegistries?: ReadonlyArray<string> }

const _CIS: policy.PolicyComplianceFramework = {
  name: "CIS-Kubernetes",
  version: "1.9",
  reference: "https://www.cisecurity.org/benchmark/kubernetes",
  specification: "workload and data-plane hardening controls",
}

// Every resource a tier constructs takes `parent: this` through the `Tier` option fold, so its parent URN
// carries the `rasm:iac:<Kind>` type token; a chart-rendered object's parent is the chart component instead.
// This is the one authorship fact no upstream chart can forge and no remediation can overwrite, which is why
// the rows asserting an ESTATE stamp read it and the rows judging any workload's own posture do not.
const _TIER_URN = "::rasm:iac:"

const _authored = (opts: policy.PolicyResourceOptions): boolean => (opts.parent ?? "").includes(_TIER_URN)

const _digestPinned: policy.ResourceValidationPolicy = {
  name: "image-digest-pinned",
  description: "estate-authored workload images pin an immutable digest",
  severity: "high",
  framework: _CIS,
  configSchema: { properties: { allowRegistries: { type: "array", items: { type: "string" } } } },
  validateResource: policy.validateResourceOfType(k8s.apps.v1.Deployment, (deployment, args, report) => {
    if (!_authored(args.opts)) return args.notApplicable("<upstream-chart-workload>")
    const allowed = args.getConfig<_DigestConfig>().allowRegistries ?? []
    return (deployment.spec?.template.spec?.containers ?? [])
      .filter((container) => !(container.image ?? "").includes("@sha256:"))
      .filter((container) => !allowed.some((registry) => (container.image ?? "").startsWith(registry)))
      .forEach((container) => report(`<mutable-image:${container.name}>`))
  }),
}

const _noSuperuser: policy.ResourceValidationPolicy = {
  name: "role-no-superuser",
  description: "app roles never hold superuser",
  severity: "critical",
  framework: _CIS,
  validateResource: policy.validateResourceOfType(postgresql.Role, (role, _args, report) =>
    void (role.superuser === true && report("<superuser-role>"))),
}

const _cnpg = (resource: policy.PolicyResource): boolean =>
  resource.props.apiVersion === "postgresql.cnpg.io/v1" && resource.props.kind === "Cluster"

const _noManagedSuperuser: policy.StackValidationPolicy = {
  name: "managed-role-no-superuser",
  description: "cnpg managed roles never hold superuser",
  severity: "critical",
  framework: _CIS,
  validateStack: (args, report) =>
    args.resources
      .filter(_cnpg)
      .filter((resource) => (resource.props.spec?.managed?.roles ?? []).some((role: { superuser?: boolean }) => role.superuser === true))
      .forEach((resource) => report("<superuser-managed-role>", resource.urn)),
}

const _protectedData: policy.StackValidationPolicy = {
  name: "data-plane-protected",
  description: "database clusters carry protect",
  severity: "critical",
  validateStack: (args, report) =>
    args.resources
      .filter(_cnpg)
      .filter((resource) => resource.opts.protect !== true)
      .forEach((resource) => report("<unprotected-cluster>", resource.urn)),
}

const _backupBeside: policy.StackValidationPolicy = {
  name: "backup-beside-cluster",
  description: "every database cluster has a scheduled backup referencing it",
  severity: "high",
  validateStack: (args, report) => {
    const archived = new Set(
      args.resources
        .filter((resource) => resource.props.apiVersion === "postgresql.cnpg.io/v1" && resource.props.kind === "ScheduledBackup")
        .map((resource) => resource.props.spec?.cluster?.name),
    )
    return args.resources
      .filter(_cnpg)
      .filter((resource) => !archived.has(resource.props.metadata?.name ?? resource.name))
      .forEach((resource) => report("<cluster-without-scheduled-backup>", resource.urn))
  },
}

const _tlsIngress: policy.ResourceValidationPolicy = {
  name: "ingress-tls-required",
  description: "every legacy ingress carries a tls block",
  severity: "high",
  validateResource: policy.validateResourceOfType(k8s.networking.v1.Ingress, (ingress, _args, report) =>
    void ((ingress.spec?.tls ?? []).length === 0 && report("<ingress-without-tls>"))),
}

const _tlsGateway: policy.StackValidationPolicy = {
  name: "gateway-tls-required",
  description: "every gateway terminates tls on a certificate",
  severity: "high",
  validateStack: (args, report) =>
    args.resources
      .filter((resource) => resource.props.kind === "Gateway" && String(resource.props.apiVersion ?? "").startsWith("gateway.networking.k8s.io"))
      .filter((resource) =>
        !(resource.props.spec?.listeners ?? []).some((listener: { protocol?: string; tls?: { certificateRefs?: ReadonlyArray<unknown> } }) =>
          listener.protocol === "HTTPS" && (listener.tls?.certificateRefs ?? []).length > 0))
      .forEach((resource) => report("<gateway-without-tls>", resource.urn)),
}

const _networkFence: policy.StackValidationPolicy = {
  name: "namespace-network-fence",
  description: "deployment-bearing stacks carry a network policy",
  severity: "medium",
  validateStack: policy.validateStackResourcesOfType(k8s.apps.v1.Deployment, (deployments, args, report) =>
    void (
      deployments.length > 0
      && !args.resources.some((resource) => resource.isType(k8s.networking.v1.NetworkPolicy))
      && report("<deployments-without-network-policy>")
    )),
}

const _managedBy: policy.ResourceValidationPolicy = {
  name: "managed-by-stamp",
  description: "estate-authored workloads carry the managed-by label",
  enforcementLevel: "remediate",
  // Remediation runs BEFORE validation, so an unnarrowed stamp would write this estate's ownership label onto
  // every chart-rendered controller in the graph — a foreign object's field this pack never authored.
  ...policy.validateRemediateResourceOfType(k8s.apps.v1.Deployment, (deployment, args) =>
    _authored(args.opts)
      ? {
          ...deployment,
          metadata: {
            ...deployment.metadata,
            labels: { ...deployment.metadata?.labels, "app.kubernetes.io/managed-by": "rasm-iac" },
          },
        }
      : undefined),
}

const _versionedAws: policy.StackValidationPolicy = {
  name: "bucket-versioned-aws",
  description: "every s3 bucket has a versioning row beside it",
  severity: "high",
  validateStack: (args, report) => {
    const versioned = new Set(
      args.resources
        .filter((resource) => resource.isType(aws.s3.BucketVersioningV2))
        .map((resource) => resource.props.bucket),
    )
    return args.resources
      .filter((resource) => resource.isType(aws.s3.BucketV2))
      .filter((resource) => !versioned.has(resource.props.bucket))
      .forEach((resource) => report("<bucket-without-versioning>", resource.urn))
  },
}

const _versionedGcp: policy.ResourceValidationPolicy = {
  name: "bucket-versioned-gcp",
  description: "gcs buckets keep versioning enabled",
  severity: "high",
  validateResource: policy.validateResourceOfType(gcp.storage.Bucket, (bucket, _args, report) =>
    void (bucket.versioning?.enabled !== true && report("<bucket-without-versioning>"))),
}

const _IamStatement = Schema.Struct({
  Effect: Schema.optional(Schema.Literal("Allow", "Deny")),
  Action: Schema.optional(Schema.Union(Schema.String, Schema.Array(Schema.String))),
})

const _IamPolicy = Schema.Struct({
  Statement: Schema.optional(Schema.Union(_IamStatement, Schema.Array(_IamStatement))),
})

const _iamJson = Schema.decodeUnknownEither(Schema.parseJson(_IamPolicy))
const _iamValue = Schema.decodeUnknownEither(_IamPolicy)

const _iamFloor: policy.ResourceValidationPolicy = {
  name: "iam-floor",
  description: "iam policies never grant wildcard actions",
  severity: "critical",
  framework: _CIS,
  validateResource: policy.validateResourceOfType(aws.iam.Policy, (row, args, report) => {
    const parsed = typeof row.policy === "string" ? _iamJson(row.policy) : _iamValue(row.policy ?? {})
    return Either.match(parsed, {
      onLeft: () => args.notApplicable("<invalid-iam-policy>"),
      onRight: (document) => (document.Statement === undefined
        ? []
        : Array.isArray(document.Statement) ? document.Statement : [document.Statement])
        .filter((statement) => {
          // an explicit Deny legitimately wildcards; only a granting statement trips the floor
          const actions = typeof statement.Action === "string" ? [statement.Action] : (statement.Action ?? [])
          return statement.Effect !== "Deny" && actions.some((action) => action.includes("*"))
        })
        .forEach(() => report("<wildcard-iam-action>")),
    })
  }),
}

const _capsule = (resource: policy.PolicyResource, kind: string): boolean =>
  String(resource.props.apiVersion ?? "").startsWith("capsule.clastix.io") && resource.props.kind === kind

// The tenant fence rides a `GlobalTenantResource` replication because the Tenant CR's own `networkPolicies` block
// is deprecated, so presence-beside is the shape this row judges — the same dependency-aware form the backup row
// takes — and the replication's own tenant selector is what names which tenant it fences.
const _tenantFence: policy.StackValidationPolicy = {
  name: "tenant-fence",
  description: "every capsule tenant has a replicated ingress fence beside it",
  severity: "high",
  validateStack: (args, report) => {
    const fenced = new Set(
      args.resources
        .filter((resource) => _capsule(resource, "GlobalTenantResource"))
        .flatMap((resource) => Object.values(resource.props.spec?.tenantSelector?.matchLabels ?? {}) as ReadonlyArray<string>),
    )
    return args.resources
      .filter((resource) => _capsule(resource, "Tenant"))
      .filter((resource) => !fenced.has(resource.props.metadata?.name ?? resource.name))
      .forEach((resource) => report("<tenant-without-replicated-fence>", resource.urn))
  },
}

// The operator still SERVES each of these and has superseded every one, so a tenant composing one is a row the
// next operator bump deletes with no diff to warn on; naming the whole deprecated set here is what keeps the
// governance CR on its stable spellings rather than on whichever block an author found first.
const _DEPRECATED_TENANT = ["networkPolicies", "containerRegistries", "limitRanges", "imagePullPolicies"] as const

const _tenantCurrent: policy.StackValidationPolicy = {
  name: "tenant-no-deprecated-spec",
  description: "tenants compose no deprecated governance block",
  severity: "medium",
  validateStack: (args, report) =>
    args.resources
      .filter((resource) => _capsule(resource, "Tenant"))
      .flatMap((resource) =>
        _DEPRECATED_TENANT
          .filter((block) => resource.props.spec?.[block] !== undefined)
          .map((block) => ({ block, urn: resource.urn })))
      .forEach(({ block, urn }) => report(`<deprecated-tenant-block:${block}>`, urn)),
}

// Every estate tier stamps ONE `Tier.harden` anchor at the pod and at every container it constructs, so this
// row asserts the stamp rather than re-deriving a posture. Pod and container rosters stay separate because a
// container-level setting outranks the pod's, so a compliant pod proves nothing about the container beneath it
// — the anchor restates the seccomp filter at the container level for exactly that reason, and the roster
// carries the row that proves it — and a new `Tier.harden` refusal is one entry on the roster its level owns.
type _Guarded = {
  readonly runAsNonRoot?: boolean
  readonly seccompProfile?: { readonly type?: string }
}
type _Pod = { readonly securityContext?: _Guarded }
type _Container = {
  readonly name?: string
  readonly securityContext?: _Guarded & {
    readonly allowPrivilegeEscalation?: boolean
    readonly readOnlyRootFilesystem?: boolean
    readonly capabilities?: { readonly drop?: ReadonlyArray<string> }
  }
}

const _POD_HELD = [
  ["<pod-may-run-root>", (pod: _Pod) => pod.securityContext?.runAsNonRoot === true],
  ["<pod-without-seccomp>", (pod: _Pod) => pod.securityContext?.seccompProfile?.type === "RuntimeDefault"],
] as const

const _CONTAINER_HELD = [
  ["<container-may-run-root>", (row: _Container) => row.securityContext?.runAsNonRoot === true],
  ["<container-may-escalate>", (row: _Container) => row.securityContext?.allowPrivilegeEscalation === false],
  ["<container-writable-root>", (row: _Container) => row.securityContext?.readOnlyRootFilesystem === true],
  ["<container-keeps-capabilities>", (row: _Container) => (row.securityContext?.capabilities?.drop ?? []).includes("ALL")],
  // the container level outranks the pod's, so the anchor restates the filter here and the roster proves it
  ["<container-without-seccomp>", (row: _Container) => row.securityContext?.seccompProfile?.type === "RuntimeDefault"],
] as const

const _hardened: policy.ResourceValidationPolicy = {
  name: "workload-hardened",
  description: "every estate-authored pod and container carries the deploy-owned privilege posture",
  severity: "critical",
  framework: _CIS,
  validateResource: policy.validateResourceOfType(k8s.apps.v1.Deployment, (deployment, args, report) => {
    if (!_authored(args.opts)) return args.notApplicable("<upstream-chart-workload>")
    const pod = deployment.spec?.template.spec ?? {}
    return [
      ..._POD_HELD.filter(([, held]) => !held(pod)).map(([reason]) => reason),
      ...(pod.containers ?? []).flatMap((container) =>
        _CONTAINER_HELD.filter(([, held]) => !held(container)).map(([reason]) => `${reason}:${container.name ?? ""}`)),
    ].forEach((reason) => report(reason))
  }),
}

const _policies: policy.Policies = [
  _digestPinned, _hardened, _noSuperuser, _noManagedSuperuser, _protectedData,
  _backupBeside, _tlsIngress, _tlsGateway, _networkFence, _managedBy,
  _versionedAws, _versionedGcp, _iamFloor, _tenantFence, _tenantCurrent,
]

const Guard: policy.PolicyPackArgs = {
  policies: _policies,
  enforcementLevel: "mandatory",
  description: "rasm deploy-plane invariants",
}
```

## [04]-[DRIFT_REPORT]

[DRIFT_REPORT]:
- Owner: `DriftReport`, one `Schema.Class` reusing the automation owner's field schemas — `summary` and `drifted` are `RunReceipt.fields.summary` and `RunReceipt.fields.steps` composed directly, so the drift vocabulary cannot fork from the receipt vocabulary — with `rotations` (the urns of certificate resources whose reissue window is open) and the `Option`-carried `skew` pair.
- Law: `clean` is a projection — no drifted row and no open rotation; a report is evidence, and acting on it (re-running `up`, bumping an epoch) is the caller's decision over data.
- Law: rotation is type-token matched — a `tls:`-prefixed step whose op is not `same` is a certificate moving through its renewal window, the deploy-plane read of the `Certs` `earlyRenewalHours`/`readyForRenewal` law; the ACME lane's ARI-window reissue surfaces through the same prefix watch on its own type token, so one channel covers both lanes.
- Law: skew is fold-audit evidence — the engine's change summary and the event-folded buckets must agree; a disagreement ships as the `skew` pair rather than a silent preference, because a fold that quietly trusts one source cannot detect its own decode drift.
- Growth: a new watch family is one prefix row in the rotation filter or one projection field.
- Boundary: step and summary shapes are `program/automation.md`'s; what a drifted board means is `operate/observe.md`'s content-hash law; in-cluster DDL divergence on the k8s arm is the runtime's fail-closed capability probe, ruled there — the `conform` read-back below exists ONLY for the docker cell whose daemon host the deploy plane can reach.
- Packages: `effect` (`Schema`, `Option`); `../program/automation.ts` (`RunReceipt`).

```typescript
import type { Stack } from "@pulumi/pulumi/automation"
import { Array, Effect, Option, pipe, Record, Schema, type Schedule } from "effect"
import { Automation, DeployFault, RunReceipt } from "../program/automation.ts"
import type { StackSpec } from "../program/spec.ts"

class DriftReport extends Schema.Class<DriftReport>("DriftReport")({
  stack: Schema.NonEmptyString,
  summary: RunReceipt.fields.summary,
  drifted: RunReceipt.fields.steps,
  rotations: Schema.Array(Schema.String),
  skew: Schema.optionalWith(
    Schema.Struct({
      expected: RunReceipt.fields.summary,
      observed: RunReceipt.fields.summary,
    }),
    { as: "Option" },
  ),
}) {
  get clean(): boolean {
    return this.drifted.length === 0 && this.rotations.length === 0
  }
}
```

## [05]-[EVIDENCE_SPINE]

[EVIDENCE_SPINE]:
- Owner: `Evidence` — the deploy plane's one delivery vocabulary: `rows` is the tagged `Schema.Union` (`Run` carries a settled `RunReceipt`, `Drift` a projected `DriftReport`, `Fault` a projected per-stack failure, `Rotation` the open reissue windows, `SecretChange` the Doppler webhook delivery, `Hosted` the Pulumi Cloud webhook delivery), `wire` is the fused JSON codec every webhook sink decodes through, `ofVerdict` folds a sweep verdict into its rows, and `file` is the `FileSystem`-backed NDJSON sink — one vocabulary, so run, drift, rotation, and both webhook sources land on any sink interchangeably.
- Law: a sink never fails — `Evidence.Sink<R>` types the error channel `never`, delivery failure logs through `Effect.ignoreLogged` as the ruled discard, and the sweep proceeds; evidence delivery is a tap, and a tap that can halt its source is the inversion this contract forecloses.
- Law: the sink rides the branch platform contracts — `FileSystem.FileSystem` and `Path.Path` Tags carry the file sink, the sweep cursor persists through the `KeyValueStore.layerSchema` store read via `Effect.serviceOption` so an unwired root simply skips the checkpoint — Tags in domain code, Layers at the root, presence as data.
- Law: webhook deliveries decode here — the Doppler secret-change delivery (`operate/secret.md`'s `Secrets.webhook` row) and the hosted `DriftDetected`/`UpdateFailed`/`DeploymentFailed` delivery (`operate/cloud.md`'s `Webhook` row) enter their receiving sink through `Evidence.wire` as the `SecretChange` and `Hosted` rows, so the two-source evidence-delivery law shares one decode and one dispatch.
- Law: rotation is its own row — a report whose `rotations` set is inhabited yields a `Rotation` row beside its `Drift` row, so a certificate-window watcher routes on the tag and never re-scans report interiors.
- Entry: `Evidence.file(directory)` as the standing sweep sink; `Evidence.wire` at any webhook receiver; `Evidence.ofVerdict(verdict)` wherever a check verdict becomes delivery material.
- Growth: a new evidence source is one union row; a new delivery surface is one sink constructor over the platform Tag that carries it.
- Boundary: run settle rows are minted by `program/automation.md` callers; the webhook resources that deliver into this vocabulary are `operate/secret.md`'s and `operate/cloud.md`'s rows; the boot edge that provides `NodeContext.layer` beneath these Tags is `program/automation.md`'s composition-root law.
- Packages: `@effect/platform` (`FileSystem`, `KeyValueStore`, `Path`); `effect` (`Schema`, `Effect`, `Either`, `Array`, `Option`, `DateTime`); `../program/automation.ts` (`RunReceipt`, `DeployFault`).

```typescript
import { FileSystem, KeyValueStore, Path } from "@effect/platform"
import { DateTime, Either } from "effect"

const _hostedKinds = ["DriftDetected", "UpdateFailed", "DeploymentFailed"] as const

const _Run = Schema.TaggedStruct("Run", { receipt: RunReceipt })
const _Drifted = Schema.TaggedStruct("Drift", { report: DriftReport })
const _Faulted = Schema.TaggedStruct("Fault", { stack: Schema.NonEmptyString, reason: Schema.NonEmptyString, detail: Schema.String })
const _Rotation = Schema.TaggedStruct("Rotation", { stack: Schema.NonEmptyString, urns: Schema.Array(Schema.String) })
const _SecretChange = Schema.TaggedStruct("SecretChange", { project: Schema.NonEmptyString, config: Schema.NonEmptyString })
const _Hosted = Schema.TaggedStruct("Hosted", { stack: Schema.NonEmptyString, kind: Schema.Literal(..._hostedKinds) })

const _Evidence = Schema.Union(_Run, _Drifted, _Faulted, _Rotation, _SecretChange, _Hosted)

const _CURSOR = KeyValueStore.layerSchema(Schema.Struct({ cycle: Schema.Int, at: Schema.DateTimeUtc }), "iac/DriftCursor")

declare namespace Evidence {
  type Row = typeof _Evidence.Type
  type Kind = Row["_tag"]
  type Sink<R = never> = (rows: ReadonlyArray<Row>) => Effect.Effect<void, never, R>
}

const Evidence = {
  rows: _Evidence,
  wire: Schema.parseJson(_Evidence),
  run: (receipt: RunReceipt): Evidence.Row => _Run.make({ receipt }),
  ofVerdict: (verdict: Either.Either<DriftReport, DeployFault>): ReadonlyArray<Evidence.Row> =>
    Either.match(verdict, {
      // The rendered message IS the detail: each family row renders its OWN subject, and the two reasons carrying
      // no foreign message would leave a free `detail` read unspellable, so the row reads what the family already
      // composed rather than a column only some arms hold.
      onLeft: (fault) => [_Faulted.make({ stack: fault.case.stack, reason: fault.case.reason, detail: fault.message })],
      onRight: (report) => [
        _Drifted.make({ report }),
        ...(report.rotations.length === 0 ? [] : [_Rotation.make({ stack: report.stack, urns: report.rotations })]),
      ],
    }),
  file: (directory: string): Evidence.Sink<FileSystem.FileSystem | Path.Path> =>
    (rows) =>
      Effect.gen(function* () {
        const fs = yield* FileSystem.FileSystem
        const path = yield* Path.Path
        const at = yield* DateTime.now
        const lines = yield* Effect.forEach(rows, Schema.encode(Evidence.wire))
        yield* fs.writeFileString(path.join(directory, `${DateTime.formatIso(at)}.ndjson`), Array.join(lines, "\n"))
      }).pipe(Effect.ignoreLogged), // the ruled discard: delivery failure logs and the source proceeds
} as const
```

## [06]-[DRIFT_SWEEP]

[DRIFT_SWEEP]:
- Owner: `Drift` — `check(stack, name)` composes `Automation.reconcile` (the driver's read-only leg) and projects the receipt through `_report`; `sweep(fleet, cadence, sink)` repeats the fleet check under the caller's `Schedule` at the fiber's inherited concurrency budget, and each stack's failure is isolated through `Effect.either` so one faulted stack never starves the rest of the fleet cycle — the sweep folds every cycle's verdicts, faults included, through `Evidence.ofVerdict` into the sink, then advances the `_CURSOR` checkpoint where the root provides the store; `conform(database, expected)` is the docker-cell store read-back over the `postgresql.getTables` and `postgresql.getSequences` pair, returning the relations and sequences the expected roster names that the live store does not carry — sequence-level drift is first-class evidence beside table drift; `cursor` is the `KeyValueStore`-backed checkpoint Layer the composing root merges when sweep progress must survive a restart.
- Law: the leg never mutates — `reconcile` is the engine's non-mutating previewRefresh; the mutating `refresh` stays a ledger op a human or workflow chooses after reading a report; the event-shaped triggers between sweep cycles are the two webhooks of one evidence-delivery law — the Doppler secret-change delivery (`operate/secret.md`) and the Pulumi Cloud `DriftDetected` filter (`operate/cloud.md`, when the backend is hosted) — both decoding through `Evidence.wire` at a sink that runs `check`.
- Law: observed buckets fold from steps — group by op, count, compare per `OpType` against the receipt summary with absent buckets read as zero; the comparison is total over the anchored vocabulary, so a new engine op is a compile-time event here, never a silent bucket.
- Law: the projection is expression-shaped end to end — the callback seam lives inside the driver's one stream bridge; this page folds decoded values only.
- Entry: `Drift.check(stack, spec.name)` ad hoc or webhook-triggered; `Drift.sweep(fleet, Schedule.cron("0 4 * * *"), Evidence.file(directory))` as the standing watch with `Drift.cursor` merged at the root; `Drift.conform(database, roster)` on the docker cell beside the sweep.
- Growth: a per-arm drift posture (ignore rows an operator owns) is one filter parameter over the drifted rows, defaulted permissive.
- Boundary: the cadence value and its composition are the rails law consumed as a parameter; reports persist wherever the caller's sink writes them; `conform` is valid only where the daemon host is deploy-reachable — the k8s arm's conformance is the runtime probe, never this read.
- Packages: `effect` (`Effect`, `Array`, `Either`, `Option`, `pipe`, `Record`, `Schedule`, `DateTime`); `@pulumi/pulumi/automation` (`Stack`); `@pulumi/postgresql` (`getTables`); `../program/automation.ts` (`Automation`, `DeployFault`, `RunReceipt`).

```typescript
import * as postgresql from "@pulumi/postgresql"

const _observed = (steps: RunReceipt["steps"]): Record.ReadonlyRecord<string, number> =>
  pipe(
    Array.groupBy(steps, (step) => step.op),
    Record.map((rows) => rows.length),
  )

const _skewed = (
  expected: RunReceipt["summary"],
  observed: Record.ReadonlyRecord<string, number>,
): Option.Option<{ readonly expected: RunReceipt["summary"]; readonly observed: Record.ReadonlyRecord<string, number> }> =>
  Array.every(RunReceipt.opTypes, (op) => (expected[op] ?? 0) === (observed[op] ?? 0))
    ? Option.none()
    : Option.some({ expected, observed })

const _report = (receipt: RunReceipt): unknown => {
  const drifted = Array.filter(receipt.steps, (step) => step.op !== "same")
  const observed = _observed(receipt.steps)
  return {
    stack: receipt.stack,
    summary: receipt.summary,
    drifted,
    rotations: Array.map(
      Array.filter(drifted, (step) => step.type.startsWith("tls:")),
      (step) => step.urn,
    ),
    ...Option.match(_skewed(receipt.summary, observed), {
      onNone: () => ({}),
      onSome: (skew) => ({ skew }),
    }),
  }
}

const Drift = {
  check: (stack: Stack, name: string): Effect.Effect<DriftReport, DeployFault> =>
    Automation.reconcile(stack, name).pipe(
      Effect.flatMap((receipt) =>
        Effect.mapError(
          Schema.decodeUnknown(DriftReport)(_report(receipt)),
          (parse) => new DeployFault({ case: { reason: "alien", stack: name, detail: parse.message } }),
        )),
    ),
  conform: (
    database: string,
    expected: { readonly tables: ReadonlyArray<string>; readonly sequences: ReadonlyArray<string> },
  ): Effect.Effect<{ readonly tables: ReadonlyArray<string>; readonly sequences: ReadonlyArray<string> }, DeployFault> =>
    Effect.map(
      Effect.all({
        tables: Effect.tryPromise({ try: () => postgresql.getTables({ database }), catch: DeployFault.triaged(database) }),
        sequences: Effect.tryPromise({ try: () => postgresql.getSequences({ database }), catch: DeployFault.triaged(database) }),
      }, { concurrency: 2 }),
      ({ tables, sequences }) => ({
        tables: Array.difference(expected.tables, Array.map(tables.tables, (table) => table.objectName)),
        sequences: Array.difference(expected.sequences, Array.map(sequences.sequences, (sequence) => sequence.objectName)),
      }),
    ),
  sweep: <R>(
    fleet: ReadonlyArray<readonly [StackSpec, Stack]>,
    cadence: Schedule.Schedule<unknown>,
    sink: Evidence.Sink<R>,
  ): Effect.Effect<void, never, R> =>
    Effect.repeat(
      Effect.flatMap(
        Effect.forEach(fleet, ([spec, stack]) => Effect.either(Drift.check(stack, spec.name)), { concurrency: "inherit" }),
        (verdicts) =>
          Effect.zipRight(
            sink(Array.flatMap(verdicts, Evidence.ofVerdict)),
            Effect.flatMap(Effect.serviceOption(_CURSOR.tag), Option.match({
              // presence as data: an unwired root skips the checkpoint, a provided store survives restarts
              onNone: () => Effect.void,
              onSome: (store) =>
                Effect.gen(function* () {
                  const held = yield* store.get("sweep")
                  yield* store.set("sweep", {
                    cycle: Option.match(held, { onNone: () => 1, onSome: (cursor) => cursor.cycle + 1 }),
                    at: yield* DateTime.now,
                  })
                }).pipe(Effect.ignoreLogged),
            })),
          ),
      ),
      { schedule: cadence },
    ).pipe(Effect.asVoid),
  cursor: _CURSOR.layer,
} as const
```

## [07]-[RECONCILE_LOOP]

[RECONCILE_LOOP]:
- Owner: `Reconcile`, the in-cluster continuous-reconciliation tier — the Pulumi Kubernetes Operator installs as one `helm.v4.Chart` row, and each reconciled estate is one typed `Stack` CR (committed `crd2pulumi` classes from `../crds/pko`): `spec.stack` names the target, `spec.projectRepo`/`branch` bind the Git source of the desired-state program, `spec.refresh: true` re-reads provider state each cycle, `spec.continueResyncOnCommitMatch` + `spec.resyncFrequencySeconds` make the loop continuous rather than commit-edge-triggered, and `spec.envRefs` bind the workspace facts from the ONE workspace `Secret` this tier mints from its `workspace` args — the same facts `_host` reads on the deploy host, one vocabulary, two execution planes, and a CR referencing a secret nothing minted is the phantom this owner closes.
- Law: two clocks never watch one stack — the deploy-host `Drift.sweep` and an in-cluster `Stack` CR are alternative reconcilers; an estate under PKO drops out of the local fleet roster, so evidence has one producer per stack and remediation posture stays deliberate on both paths.
- Law: tenant-triggered provisioning rides the operator — a tenant-submitted CR (the `Program` CR carrying an inline desired-state program, or a `Stack` CR referencing a tenant repo) is reconciled by PKO inside the tenant's own RBAC envelope, so multi-tenant self-service provisioning needs no deploy-host actor and the Capsule/vcluster boundary from `kube/tenant.md` scopes what the tenant's CR may reach.
- Law: the operator's reach is RBAC shape, never a watch scope — the chart publishes no namespace-watch value at all, so the install lands in the estate namespace while `rbac.createClusterRole` (default TRUE) decides what the controller may reach cluster-wide; narrowing is `createClusterRole: false` plus `createRole: true`, and reading the namespace row as the scope leaves a namespaced install holding cluster-wide grants. The workload identity binds through the same `ServiceAccount` cell `kube/workload.md` realizes.
- Law: the chart's CRDs stay current because the carrier renders — `crds/` carries the `Stack`, `Program`, `Workspace`, and `Update` schemas, which `helm upgrade` plants once and never revisits, while `helm.v4.Chart` hands each to the provider as a managed resource that diffs on every version bump; the generated `crds/pko` module regenerates against that same pin, so the cluster schema and the typed classes move together and neither needs an out-of-band apply.
- Entry: `new Reconcile("reconcile", { spec, namespace, version, source, frequencySeconds, workspace }, opts)` inside the k8s arm when the estate earns the in-cluster loop, `workspace` carrying the backend URL and the passphrase read the composing arm resolves.
- Growth: a second reconciled estate is one more `Stack` CR row; an inline-program subject is one `Program` CR row.
- Boundary: the operator chart's values drift with its pin; the generated `crds/pko` module regenerates on operator bumps; hosted drift schedules are `operate/cloud.md`'s twin, subject to the same one-clock law.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `../crds/pko` (`v1.Stack`, `v1.Program` — crd2pulumi); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pko from "../crds/pko"
import { Tier, type StackSpec } from "../program/spec.ts"

declare namespace Reconcile {
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly version: pulumi.Input<string>
    readonly source: { readonly repo: string; readonly branch: string; readonly dir?: string }
    readonly frequencySeconds: number
    readonly workspace: { readonly backend: pulumi.Input<string>; readonly passphrase: pulumi.Input<string> }
  }
}

class Reconcile extends Tier {
  constructor(name: string, args: Reconcile.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Reconcile", name, opts)
    const operator = new k8s.helm.v4.Chart(name, {
      // OCI is the chart's ONLY published route: the GitHub Pages host serves no `index.yaml` and redirects to a
      // 404, so a `repositoryOpts.repo` row here resolves nothing. The reference carries the registry inline.
      chart: "oci://ghcr.io/pulumi/helm-charts/pulumi-kubernetes-operator",
      version: args.version,
      namespace: args.namespace,
      // The chart's four CRDs ship in `crds/`, which `helm upgrade` installs once and never touches again. This
      // row escapes that: `helm.v4.Chart` RENDERS and hands every object to the provider, so the CRDs are ordinary
      // Pulumi resources a version bump diffs and updates — no out-of-band apply, and no silently stale schema.
      skipCrds: false,
    }, this.child())
    const workspace = new k8s.core.v1.Secret(`${name}-workspace`, {
      metadata: { namespace: args.namespace },
      stringData: {
        PULUMI_BACKEND_URL: args.workspace.backend,
        PULUMI_CONFIG_PASSPHRASE: args.workspace.passphrase,
      },
    }, this.child())
    new pko.v1.Stack(args.spec.name, {
      metadata: { namespace: args.namespace },
      spec: {
        stack: args.spec.name,
        projectRepo: args.source.repo,
        branch: args.source.branch,
        ...(args.source.dir !== undefined && { repoDir: args.source.dir }),
        refresh: true,
        continueResyncOnCommitMatch: true,
        resyncFrequencySeconds: args.frequencySeconds,
        envRefs: {
          PULUMI_BACKEND_URL: { type: "Secret", secret: { name: workspace.metadata.name, key: "PULUMI_BACKEND_URL" } },
          PULUMI_CONFIG_PASSPHRASE: { type: "Secret", secret: { name: workspace.metadata.name, key: "PULUMI_CONFIG_PASSPHRASE" } },
        },
      },
    }, this.child({ dependsOn: [operator] }))
    this.seal({})
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Drift, DriftReport, Evidence, Guard, Reconcile }
```

## [08]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
