# [IAC_GUARD]

The CrossGuard gate: `Guard` is one `PolicyPackArgs` value — policies as data rows, pack-level `mandatory` enforcement with per-policy overrides — that every `program/automation` run attaches through `Automation.Options.policyPacks`, so no `up` or `preview` executes ungated. Policies narrow against the exact resource classes the sibling catalogues export through the typed helper family: `validateResourceOfType` gives each validator the resource's unwrapped input props at compile-checked depth, `validateStackResourcesOfType` and raw stack validation own cross-resource invariants over the dependency-aware resource graph, and `remediateResourceOfType` fixes forward before validation judges. The rows encode this folder's own laws as machine pressure — digest-pinned images, no superuser roles, TLS-only ingress, protected data planes, namespace network fences, managed-by stamps — and a remediated credential wraps `policy.Secret` so the engine encrypts it. The module is `iac/src/policy/guard.ts`; a new invariant is one policy row, a compliance frame is one `framework` field, and no validator ever branches — growth is rows, never arms.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                          | [PUBLIC] |
| :-----: | :-------------- | :------------------------------------------------------------------ | :------- |
|  [01]   | `PACK_ASSEMBLY` | the pack value, enforcement vocabulary, and the analyzer-entry law  | `Guard`  |
|  [02]   | `POLICY_ROWS`   | the typed validation, stack-invariant, and remediation rows         | `Guard`  |

## [2]-[PACK_ASSEMBLY]

[PACK_ASSEMBLY]:
- Owner: `Guard`, the `PolicyPackArgs` value — the `policies` array is the whole pack, `enforcementLevel: "mandatory"` is the pack default each row may override (`"advisory"` for stamps, `"remediate"` for fix-forward rows), and metadata (`description`, `severity`, `framework`, `remediationSteps`) rides each row as data the engine surfaces with the violation.
- Law: the pack module is pure and the analyzer entry is a boot edge — this module exports the args value; a one-line entry module (`new PolicyPack("rasm-guard", Guard)`) is the analyzer process's own top level, executed by the engine's policy plugin, and `Automation` attaches the entry's path via `policyPacks`, so the lib stays side-effect-free and the pack still gates every run.
- Law: violations are receipt material — the engine folds `ReportViolation` calls into the run's policy events; a `mandatory` violation fails the run before apply, and the receipt's diagnostics carry the evidence, so gating and reporting are one stream.
- Law: configuration is typed at the row — a policy with knobs declares `configSchema` and reads `args.getConfig<T>()`; a config-less policy declares none, and knob defaults live in the schema, never in validator bodies.
- Growth: one policy row per invariant; a per-app enforcement override is `PolicyPackConfig` data at the entry, never a pack edit.
- Boundary: attachment plumbing is `program/automation.md`'s options row; drift is not policy — `policy/drift.md` reads state divergence, this pack judges desired state.
- Packages: `@pulumi/policy` (`PolicyPackArgs`, `ResourceValidationPolicy`, `StackValidationPolicy`, helper family, `Secret`); `@pulumi/kubernetes`, `@pulumi/postgresql` (the narrowed classes).

## [3]-[POLICY_ROWS]

[POLICY_ROWS]:
- Law: image provenance is structural — `image-digest-pinned` narrows `k8s.apps.v1.Deployment` and reports every container whose image lacks an `@sha256:` digest; the `kube/workload` digest law compiles to this gate, so a mutable tag cannot reach a cluster even from an app-authored program.
- Law: the data plane cannot escalate or vanish — `role-no-superuser` narrows `postgresql.Role` and rejects `superuser: true`; `data-plane-protected` walks the stack graph for CNPG `Cluster` CRs (matched on the CR's own `apiVersion`/`kind` props, the carrier's stable discriminant) and demands `opts.protect`, so an unprotected database is unshippable.
- Law: traffic is TLS-only — `ingress-tls-required` narrows `k8s.networking.v1.Ingress` and rejects a spec whose `tls` block is empty; the `kube/traffic` sink law becomes machine pressure here.
- Law: workloads carry fences — `namespace-network-fence` is a stack validation demanding a `k8s.networking.v1.NetworkPolicy` wherever a `Deployment` exists, judged over the whole `PolicyResource` graph through `isType` because presence-beside is a cross-resource fact no single-resource validator can see.
- Law: remediation stamps, never invents — `managed-by-stamp` remediates `Deployment` metadata with the `app.kubernetes.io/managed-by` label before validation, returning the corrected prop bag; a remediation that mints credential material wraps it in `new policy.Secret(...)` so the engine encrypts it in state.
- Law: validators read unwrapped props totally — optional chains over the generated arg shapes, `report(message, urn?)` once per finding, `args.notApplicable(reason)` where a policy cannot judge a resource; a validator that throws is a defect, not a verdict.
- Growth: one row per invariant, appended to `_policies`; a prepared-arm invariant (bucket retention, IAM floor) lands as a row narrowing that provider's class when the arm's realizer settles.
- Boundary: the classes narrowed here are the tier pages' constructions; enforcement semantics (`remediate` apply order, `mandatory` abort) are the engine's contract.

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as policy from "@pulumi/policy"
import * as postgresql from "@pulumi/postgresql"

const _digestPinned: policy.ResourceValidationPolicy = {
  name: "image-digest-pinned",
  description: "workload images pin an immutable digest",
  severity: "high",
  validateResource: policy.validateResourceOfType(k8s.apps.v1.Deployment, (deployment, _args, report) =>
    (deployment.spec?.template.spec?.containers ?? [])
      .filter((container) => !(container.image ?? "").includes("@sha256:"))
      .forEach((container) => report(`<mutable-image:${container.name}>`))),
}

const _noSuperuser: policy.ResourceValidationPolicy = {
  name: "role-no-superuser",
  description: "app roles never hold superuser",
  severity: "critical",
  validateResource: policy.validateResourceOfType(postgresql.Role, (role, _args, report) =>
    void (role.superuser === true && report("<superuser-role>"))),
}

const _tlsIngress: policy.ResourceValidationPolicy = {
  name: "ingress-tls-required",
  description: "every ingress carries a tls block",
  severity: "high",
  validateResource: policy.validateResourceOfType(k8s.networking.v1.Ingress, (ingress, _args, report) =>
    void ((ingress.spec?.tls ?? []).length === 0 && report("<ingress-without-tls>"))),
}

const _protectedData: policy.StackValidationPolicy = {
  name: "data-plane-protected",
  description: "database clusters carry protect",
  severity: "critical",
  validateStack: (args, report) =>
    args.resources
      .filter((resource) => resource.props.apiVersion === "postgresql.cnpg.io/v1" && resource.props.kind === "Cluster")
      .filter((resource) => resource.opts.protect !== true)
      .forEach((resource) => report("<unprotected-cluster>", resource.urn)),
}

const _networkFence: policy.StackValidationPolicy = {
  name: "namespace-network-fence",
  description: "deployment-bearing stacks carry a network policy",
  severity: "medium",
  validateStack: (args, report) =>
    void (
      args.resources.some((resource) => resource.isType(k8s.apps.v1.Deployment))
      && !args.resources.some((resource) => resource.isType(k8s.networking.v1.NetworkPolicy))
      && report("<deployments-without-network-policy>")
    ),
}

const _managedBy: policy.ResourceValidationPolicy = {
  name: "managed-by-stamp",
  description: "workloads carry the managed-by label",
  enforcementLevel: "remediate",
  remediateResource: policy.remediateResourceOfType(k8s.apps.v1.Deployment, (deployment) => ({
    ...deployment,
    metadata: {
      ...deployment.metadata,
      labels: { ...deployment.metadata?.labels, "app.kubernetes.io/managed-by": "rasm-iac" },
    },
  })),
}

const _policies: policy.Policies = [_digestPinned, _noSuperuser, _tlsIngress, _protectedData, _networkFence, _managedBy]

const Guard: policy.PolicyPackArgs = {
  policies: _policies,
  enforcementLevel: "mandatory",
  description: "rasm deploy-plane invariants",
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Guard }
```
