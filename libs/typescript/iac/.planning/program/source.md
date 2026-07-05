# [IAC_SOURCE]

The bootstrap-axis legs the estate stands on before and after a deploy: `Source` provisions the source-control shell — repository, branch law, the deployment-environment gates whose names align with the `StackSpec.doppler.config` axis so the Doppler mirror, the GitHub gate, and the stack speak ONE environment vocabulary, deploy keys whose private halves never leave the `tls` owner, the webhook seam a deploy-triggering endpoint subscribes through, and non-secret Actions variables — and `Source.distribute` converges a built static frontend into the arm's object cell through the synced-folder dialect record, so the UI folder's build product lands behind the arm's DNS/CDN rows with zero new plumbing. Secret VALUES never route through this page: the `secretssync.GithubActions` mirror (`operate/secret.md` `_MIRRORS`) writes FROM the canonical config INTO the `RepositoryEnvironment` shells this page mints — this page owns the shells, the mirror owns the filling — and `ActionsVariable` rows carry only configuration that is not secret material. One branch-law owner per repo: `RepositoryRuleset` is the rules-engine spelling, and a legacy-protection twin beside it is the split the family forbids. The module is `iac/src/program/source.ts`; a new gated environment is one row sharing the doppler-config spelling, a new distribution target is one `_FOLDERS` dialect row, a new pipeline variable is one `variables` entry.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                             | [PUBLIC] |
| :-----: | :--------------- | :------------------------------------------------------------------ | :------- |
|  [01]   | `SOURCE_CONTROL` | repo, branch law, environment gates, deploy keys, webhook, variables | `Source` |
|  [02]   | `DISTRIBUTION`   | the synced-folder dialect record over the arms' object cells        | `Source` |

## [2]-[SOURCE_CONTROL]

[SOURCE_CONTROL]:
- Owner: `Source` — one `github.Provider` per owner scope constructed from the `GITHUB_TOKEN` fan-in read, one `Repository` (or an adopted pre-existing one through the class `get`), one `RepositoryRuleset` carrying the branch law, one `RepositoryEnvironment` per environments row with reviewer and self-review gates plus a branch-pattern `RepositoryEnvironmentDeploymentPolicy`, one `RepositoryDeployKey` binding a `tls.PrivateKey.publicKeyOpenssh` (read-only posture by default; the private half stays in the entropy owner), one `RepositoryWebhook` whose `configuration.secret` binds a Doppler-generated entry the receiving endpoint verifies, and `ActionsVariable` rows for non-secret pipeline configuration.
- Law: environments align three surfaces — an environments row's `name` is a `StackSpec.doppler.config` spelling, so the `secretssync.GithubActions` mirror targeting that environment, the gate reviewers protecting it, and the stack deploying under it read one vocabulary; an environment named outside the doppler axis is the split-brain this alignment law forbids.
- Law: material splits by kind — secret values arrive ONLY through the Doppler mirror into the shells; `ActionsVariable` carries non-secret configuration; a credential authored through an Actions secret beside the mirror is the second-source defect, and the deploy key's `key` field accepts only the public half.
- Law: one branch-law owner per repo — `RepositoryRuleset` for this estate; `BranchProtection` survives only when adopting a classic-protected repo, never beside the ruleset.
- Law: rate posture is provider data — retries, delays, and parallelism ride the `Provider` knobs, never per-resource retry wrappers.
- Entry: `new Source("source", { spec, owner, token, repository, environments, webhook, variables }, opts)` from the composing root, `token` the `GITHUB_TOKEN` fan-in read; `source.deployKey.privateKeyOpenssh` stays graph-interior; `RepositoryEnvironment` names feed the `_MIRRORS` githubActions rows.
- Growth: a new gated environment is one `environments` row; a new org-level posture (`Team`, `TeamRepository`, `OrganizationRuleset`) is one row when the estate grows an org.
- Boundary: the mirror mechanics are `operate/secret.md`'s; the CI workflows that run inside the shells are app repo material, never lib code; `appAuth` (GitHub-App identity) supersedes the PAT when the estate earns a durable machine identity.
- Packages: `@pulumi/github` (`Provider`, `Repository`, `RepositoryRuleset`, `RepositoryEnvironment`, `RepositoryEnvironmentDeploymentPolicy`, `RepositoryDeployKey`, `RepositoryWebhook`, `ActionsVariable`); `@pulumi/tls` (`PrivateKey`); `effect` (`Array`, `Record`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as github from "@pulumi/github"
import * as pulumi from "@pulumi/pulumi"
import * as tls from "@pulumi/tls"
import { Array, Record } from "effect"
import { Tier, type StackSpec } from "./spec.ts"

declare namespace Source {
  type Args = {
    readonly spec: StackSpec
    readonly owner: pulumi.Input<string>
    readonly token: pulumi.Input<string>
    readonly repository: { readonly name: string; readonly visibility: "private" | "public" }
    readonly environments: ReadonlyArray<{
      readonly name: string
      readonly reviewers?: ReadonlyArray<number>
      readonly branchPattern: string
    }>
    readonly webhook?: { readonly url: pulumi.Input<string>; readonly secret: pulumi.Input<string>; readonly events: ReadonlyArray<string> }
    readonly variables?: Record.ReadonlyRecord<string, pulumi.Input<string>>
  }
}

class Source extends Tier {
  static distribute(name: string, args: Source.Distribution, opts?: pulumi.ComponentResourceOptions): pulumi.ComponentResource {
    return _FOLDERS[args.arm](name, args, opts)
  }
  readonly deployKey: tls.PrivateKey
  constructor(name: string, args: Source.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Source", name, opts)
    const provider = new github.Provider(name, { token: args.token, owner: args.owner }, { parent: this })
    const child = this.child({ provider })
    const repo = new github.Repository(args.repository.name, {
      name: args.repository.name,
      visibility: args.repository.visibility,
      vulnerabilityAlerts: true,
    }, child)
    new github.RepositoryRuleset(`${name}-law`, {
      repository: repo.name,
      name: "branch-law",
      target: "branch",
      enforcement: "active",
      conditions: { refName: { includes: ["~DEFAULT_BRANCH"], excludes: [] } },
      rules: { deletion: true, nonFastForward: true, requiredLinearHistory: true },
    }, child)
    Array.map(args.environments, (row) => {
      const gate = new github.RepositoryEnvironment(row.name, {
        repository: repo.name,
        environment: row.name,
        preventSelfReview: true,
        ...(row.reviewers !== undefined && { reviewers: [{ users: [...row.reviewers] }] }),
        deploymentBranchPolicy: { protectedBranches: false, customBranchPolicies: true },
      }, child)
      return new github.RepositoryEnvironmentDeploymentPolicy(`${row.name}-branches`, {
        repository: repo.name,
        environment: gate.environment,
        branchPattern: row.branchPattern,
      }, child)
    })
    this.deployKey = new tls.PrivateKey(`${name}-deploy-key`, { algorithm: "ED25519" }, this.child())
    new github.RepositoryDeployKey(`${name}-deploy-key`, {
      repository: repo.name,
      title: `${args.spec.name}-deploy`,
      key: this.deployKey.publicKeyOpenssh,
      readOnly: true,
    }, child)
    if (args.webhook !== undefined) {
      new github.RepositoryWebhook(`${name}-events`, {
        repository: repo.name,
        active: true,
        events: [...args.webhook.events],
        configuration: { url: args.webhook.url, contentType: "json", secret: args.webhook.secret },
      }, child)
    }
    Array.map(Record.toEntries(args.variables ?? {}), ([key, value]) =>
      new github.ActionsVariable(key, {
        repository: repo.name,
        variableName: key,
        value,
      }, child))
    this.seal({ repository: repo.name })
  }
}
```

## [3]-[DISTRIBUTION]

[DISTRIBUTION]:
- Law: the bucket is the arm's object cell, the folder is its content — `_FOLDERS` is the dialect record keyed by the arms whose object cells the synced-folder component reaches: the `aws` row converges onto `aws.s3.BucketV2.bucket`, the `gcp` row onto `gcp.storage.Bucket.name`; the component never creates a bucket, cloud credentials thread through the arm's one provider seam via `opts.providers`, and the `cloudflare` arm's static distribution stays its own `PagesProject`/R2 rows — no R2 dialect exists here and none is faked through the S3 row.
- Law: the sync mode is an evidence decision — `managedObjects: true` tracks every file as a state-managed object (per-file diffs, policy visibility) and is the default; `false` is the large-site row where per-file state is cost, accepting that per-object drift detection goes dark; `path` points at a built artifact directory handed in like every pin — the UI folder's build product, never a lib literal.
- Entry: `Source.distribute("frontend", { arm: "aws", path, bucket }, { providers })` inside the owning arm after its object cell stands.
- Growth: a new distribution target is one `_FOLDERS` row when a new arm's dialect ships.
- Boundary: fronting DNS/CDN rows stay on the owning arms; build mechanics are the UI folder's.
- Packages: `@pulumi/synced-folder` (`S3BucketFolder`, `GoogleCloudFolder`).

```typescript
import * as syncedFolder from "@pulumi/synced-folder"

declare namespace Source {
  type Distribution = {
    readonly arm: keyof typeof _FOLDERS
    readonly path: string
    readonly bucket: pulumi.Input<string>
    readonly managed?: boolean
  }
}

const _FOLDERS = {
  aws: (name: string, args: Source.Distribution, opts?: pulumi.ComponentResourceOptions): pulumi.ComponentResource =>
    new syncedFolder.S3BucketFolder(name, {
      path: args.path,
      bucketName: args.bucket,
      acl: "private",
      managedObjects: args.managed ?? true,
    }, opts),
  gcp: (name: string, args: Source.Distribution, opts?: pulumi.ComponentResourceOptions): pulumi.ComponentResource =>
    new syncedFolder.GoogleCloudFolder(name, {
      path: args.path,
      bucketName: args.bucket,
      managedObjects: args.managed ?? true,
    }, opts),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Source }
```
