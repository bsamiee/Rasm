# [IAC_CLOUD]

The hosted control plane, gated on one spec value: every resource here exists only when `StackSpec.backend` is `cloud` — the composing root reads `spec.hosted` and never constructs this page otherwise — and each is the hosted TWIN of a local owner, subject to the one-clock law: `DriftSchedule` twins `Drift.sweep` (`autoRemediate: false` mirrors evidence-then-deliberate-`refresh`), `TtlSchedule` twins the `Automation.ephemeral` bracket, `DeploymentSchedule.pulumiOperation` speaks the mutating-ledger vocabulary, the `Webhook` drift filter lands beside the Doppler secret-change webhook as one evidence-delivery law with two sources, and review stacks are three data rows (`vcs.previewPullRequests`, `pullRequestTemplate`, a TTL bound), never hand-rolled REST against `api.pulumi.com`. `CloudPlane` is the one tier carrying all of it — deployment settings with VCS triggers, the schedule family, the drift webhook, team RBAC with a stack-scoped token, and the graph-owned ESC `Environment` whose consumers pin through revision tags. `Environments` is the imperative half of the same lane: the `EscApi` client wrapped once onto the Effect rail for processes that are not a Pulumi program — open/read sessions, check-gated writes, revision-tag pinning — with Doppler remaining canonical on every arm: an ESC environment is a projection DAG composing canonical material through dynamic-provider opens, never a second store, and the short-lived OIDC credentials its `fn::open` providers mint are the prepared cloud arms' credential path over static keys. The module is `iac/src/operate/cloud.ts`; a new hosted automation is one resource row, a new environment consumer is one revision-tag pin, a new delivery sink is one webhook row.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                            | [PUBLIC]       |
| :-----: | :------------------ | :------------------------------------------------------------------ | :------------- |
|  [01]   | `CONTROL_PLANE`     | deployment settings, schedules, drift webhook, RBAC, environment   | `CloudPlane`   |
|  [02]   | `ENVIRONMENT_PLANE` | the EscApi Effect rail: open/read, check-gated writes, tag pinning | `Environments` |

## [2]-[CONTROL_PLANE]

[CONTROL_PLANE]:
- Owner: `CloudPlane` — one tier over `@pulumi/pulumiservice`: `DeploymentSettings` binds the Git source (`sourceContext.git`) and the CURRENT `vcs` trigger block (`previewPullRequests` + `pullRequestTemplate` mint per-PR review stacks; the deprecated `github` block has no spelling), `DriftSchedule` runs hosted detection on the caller's cron with remediation off, `TtlSchedule` bounds review-stack lifetime, the `Webhook` row delivers `DriftDetected`/`UpdateFailed` evidence to the sink that runs `Drift.check` between local cycles, `Team` + `TeamStackPermission` + `TeamAccessToken` scope automation to one stack at the narrowest grant, and the `Environment` resource authors the graph-owned ESC environment (yaml as a `StringAsset`) with an `EnvironmentVersionTag` so consumer rotation is a tag move, never a consumer edit.
- Law: one clock per stack — a hosted `DriftSchedule` and the local `Drift.sweep` never both watch one stack; the estate picks per backend, and this tier existing at all means the hosted clock owns its stacks.
- Law: deploy credentials are federated, never static — `operationContext.oidc` carries the cloud-side identity for arm credentials, ESC dynamic-provider opens (`fn::open::aws-login` and kin) mint the short-lived material, and a static provider key living in a hosted environment definition is the second-source defect; webhook `secret` values are Doppler-generated entries the receiver verifies.
- Law: tokens mint at the narrowest scope — a `TeamAccessToken` behind a `TeamStackPermission` over an org token wherever the consumer is one stack's automation; the `buildStackScopedPermissions`/`buildEnvironmentScopedPermissions` helpers spell finer grants when a token must carry them.
- Entry: `new CloudPlane("cloud", { spec, organization, git, vcs, driftCron, sink, environment }, opts)` from the composing root when `spec.hosted`.
- Growth: a new hosted automation is one resource row; a second review-stack template is one `TemplateSource` row; an agent-pool posture is one `agentPoolId` field when data must not transit shared compute.
- Boundary: the local twins are `operate/policy.md` (`Drift`) and `program/automation.md` (`ephemeral`, `remote`); the Doppler webhook half of the evidence law is `operate/secret.md`'s; stack attachment of environments is `Automation.attach`.
- Packages: `@pulumi/pulumiservice` (`DeploymentSettings`, `DriftSchedule`, `TtlSchedule`, `DeploymentSchedule`, `Webhook`, `Team`, `TeamStackPermission`, `TeamAccessToken`, `Environment`, `EnvironmentVersionTag`); `@pulumi/pulumi` (`asset.StringAsset`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as pulumi from "@pulumi/pulumi"
import * as pulumiservice from "@pulumi/pulumiservice"
import { Tier, type StackSpec } from "../program/spec.ts"

declare namespace CloudPlane {
  type Args = {
    readonly spec: StackSpec
    readonly organization: string
    readonly project: string
    readonly git: { readonly repoUrl: string; readonly branch: string; readonly repoDir?: string }
    readonly vcs: { readonly provider: string; readonly repository: string; readonly pullRequestTemplate: boolean }
    readonly driftCron: string
    readonly sink: { readonly url: pulumi.Input<string>; readonly secret: pulumi.Input<string> }
    readonly environment?: { readonly name: string; readonly yaml: string; readonly tag: string }
    readonly ttl?: { readonly timestamp: string; readonly deleteAfterDestroy: boolean }
  }
}

class CloudPlane extends Tier {
  readonly token: pulumi.Output<string>
  constructor(name: string, args: CloudPlane.Args, opts?: pulumi.ComponentResourceOptions) {
    super("CloudPlane", name, opts)
    const coords = { organization: args.organization, project: args.project, stack: args.spec.name }
    new pulumiservice.DeploymentSettings(name, {
      ...coords,
      sourceContext: {
        git: {
          repoUrl: args.git.repoUrl,
          branch: args.git.branch,
          ...(args.git.repoDir !== undefined && { repoDir: args.git.repoDir }),
        },
      },
      vcs: {
        provider: args.vcs.provider,
        repository: args.vcs.repository,
        deployCommits: true,
        previewPullRequests: true,
        pullRequestTemplate: args.vcs.pullRequestTemplate,
      },
      cacheOptions: { enable: true },
    }, this.child())
    new pulumiservice.DriftSchedule(`${name}-drift`, {
      ...coords,
      scheduleCron: args.driftCron,
      autoRemediate: false,
    }, this.child())
    if (args.ttl !== undefined) {
      new pulumiservice.TtlSchedule(`${name}-ttl`, {
        ...coords,
        timestamp: args.ttl.timestamp,
        deleteAfterDestroy: args.ttl.deleteAfterDestroy,
      }, this.child())
    }
    new pulumiservice.Webhook(`${name}-evidence`, {
      active: true,
      displayName: `${args.spec.name}-drift`,
      organizationName: args.organization,
      projectName: args.project,
      stackName: args.spec.name,
      payloadUrl: args.sink.url,
      secret: args.sink.secret,
      format: "raw",
      filters: ["DriftDetected", "UpdateFailed", "DeploymentFailed"],
    }, this.child())
    const team = new pulumiservice.Team(`${name}-automation`, {
      organizationName: args.organization,
      name: `${args.spec.name}-automation`,
      teamType: "pulumi",
    }, this.child())
    new pulumiservice.TeamStackPermission(`${name}-grant`, {
      organization: args.organization,
      project: args.project,
      stack: args.spec.name,
      team: team.name.apply((held) => held ?? `${args.spec.name}-automation`),
      permission: pulumiservice.TeamStackPermissionScope.Admin,
    }, this.child())
    this.token = new pulumiservice.TeamAccessToken(`${name}-token`, {
      organizationName: args.organization,
      teamName: team.name.apply((held) => held ?? `${args.spec.name}-automation`),
      name: `${args.spec.name}-automation`,
      description: `<automation:${args.spec.name}>`,
    }, this.child()).value
    if (args.environment !== undefined) {
      const environment = new pulumiservice.Environment(args.environment.name, {
        organization: args.organization,
        project: args.project,
        name: args.environment.name,
        yaml: new pulumi.asset.StringAsset(args.environment.yaml),
      }, this.child())
      new pulumiservice.EnvironmentVersionTag(`${args.environment.name}-pin`, {
        organization: args.organization,
        project: args.project,
        environment: environment.name,
        tagName: args.environment.tag,
        revision: environment.revision,
      }, this.child())
    }
    this.seal({ token: this.token })
  }
}
```

## [3]-[ENVIRONMENT_PLANE]

[ENVIRONMENT_PLANE]:
- Owner: `Environments` — the `EscApi` client acquired once from the `PULUMI_ACCESS_TOKEN` redacted read and every method lifted onto the `DeployFault` rail: `read` is the one-shot `openAndReadEnvironment` session (an open is a lease — dynamic providers resolve once per open), `readAt` pins a consumer to a revision tag through `openAndReadEnvironmentAtVersion`, `write` is check-gated — `checkEnvironment` first, diagnostics re-spelled as an `input` fault naming them, only then `updateEnvironment` — and `pin` moves a revision tag so rotation is a tag move observed by every `readAt` consumer.
- Law: ESC is a projection DAG, never a store — `imports` composes environments, the three `values` channels (`pulumiConfig`/`environmentVariables`/`files`) are the projection contract, canonical material arrives through dynamic-provider opens over the stores that own it, and a secret literal authored into a definition is the second-source defect; `decryptEnvironment` never feeds automation.
- Law: one writer per environment — the `CloudPlane.Environment` resource authors graph-owned environments, this client authors out-of-graph ones; both writing one environment is drift by construction.
- Law: the boundary wrap is total — every client method is `Promise`-shaped foreign material lifted through `Effect.tryPromise` with the folder's one triage, `undefined` resolutions lift to `Option`, secret-flagged `Value`s surface `Redacted`-postured to the caller, and a raw client call escaping this rail is the flat-code defect.
- Entry: `Environments.read(org, project, env)` from any deploy-host process; `Environments.write(org, project, env, definition)`; `Automation.attach(stack, name, [env])` binds an environment to a stack — attachment is run data, no workspace-settings field exists.
- Growth: a new environment verb is one member over the client method that carries it; revision history and label reads land the same way.
- Boundary: environment EXISTENCE for graph-owned rows is `[2]`'s resource; stack attachment is `program/automation.md`'s; the SSRF posture escape hatch (`PULUMI_DISABLE_ESC_SSRF_PROTECTION`) is a deploy-host env fact for same-host providers, never lib code.
- Packages: `@pulumi/esc-sdk` (`EscApi`, `Configuration`, `EnvironmentDefinition`); `effect` (`Config`, `Effect`, `Option`, `Redacted`); `../program/automation.ts` (`DeployFault`).

```typescript
import * as esc from "@pulumi/esc-sdk"
import { Config, Effect, Option, Redacted } from "effect"
import { DeployFault } from "../program/automation.ts"

const _client: Effect.Effect<esc.EscApi, DeployFault> = Effect.map(
  Effect.mapError(
    Config.redacted("PULUMI_ACCESS_TOKEN"),
    (issue) => new DeployFault({ reason: "input", stack: "<esc>", detail: String(issue) }),
  ),
  (token) => new esc.EscApi(new esc.Configuration({ accessToken: Redacted.value(token) })),
)

const _call = <A>(env: string, run: (api: esc.EscApi) => Promise<A>): Effect.Effect<Option.Option<NonNullable<A>>, DeployFault> =>
  Effect.flatMap(_client, (api) =>
    Effect.map(
      Effect.tryPromise({ try: () => run(api), catch: DeployFault.triaged(env) }),
      Option.fromNullable,
    ))

const Environments = {
  read: (org: string, project: string, env: string) =>
    _call(env, (api) => api.openAndReadEnvironment(org, project, env)),
  readAt: (org: string, project: string, env: string, tag: string) =>
    _call(env, (api) => api.openAndReadEnvironmentAtVersion(org, project, env, tag)),
  write: (org: string, project: string, env: string, definition: esc.EnvironmentDefinition): Effect.Effect<void, DeployFault> =>
    Effect.flatMap(
      _call(env, (api) => api.checkEnvironment(org, definition)),
      (checked) =>
        Option.match(
          Option.filter(
            Option.flatMapNullable(checked, (verdict) => verdict.diagnostics),
            (diagnostics) => diagnostics.length > 0,
          ),
          {
            onNone: () => Effect.asVoid(_call(env, (api) => api.updateEnvironment(org, project, env, definition))),
            onSome: (diagnostics) => Effect.fail(new DeployFault({ reason: "input", stack: env, detail: JSON.stringify(diagnostics) })),
          },
        ),
    ),
  pin: (org: string, project: string, env: string, tag: string, revision: number): Effect.Effect<void, DeployFault> =>
    Effect.asVoid(_call(env, (api) => api.createEnvironmentRevisionTag(org, project, env, tag, revision))),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { CloudPlane, Environments }
```
