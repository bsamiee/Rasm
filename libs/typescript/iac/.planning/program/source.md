# [IAC_SOURCE]

`Source` owns the bootstrap-axis source-control shell and static-distribution fold. Repository law, deployment environments aligned with `StackSpec.doppler.config`, public deploy keys, webhook configuration, and non-secret Actions variables share one tier. `Source.distribute` converges a built frontend into an arm's object cell and publishes caller-supplied artifact rows as content-addressed `served` outputs. `secretssync.GithubActions` alone fills secret values from the canonical config. Module `iac/src/program/source.ts` grows by one environment row, `_FOLDERS` dialect row, artifact row, or variable entry.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                               | [PUBLIC] |
| :-----: | :--------------- | :------------------------------------------------------------------- | :------- |
|  [01]   | `SOURCE_CONTROL` | repo, branch law, environment gates, deploy keys, webhook, variables | `Source` |
|  [02]   | `DISTRIBUTION`   | the synced-folder dialect record and roster-derived serving rows     | `Source` |

## [02]-[SOURCE_CONTROL]

[SOURCE_CONTROL]:
- Owner: `Source` — one `github.Provider` per owner scope constructed from the `GITHUB_TOKEN` fan-in read, one `Repository` (a pre-existing repo adopts through the `import` resource option so its settings become plan-managed; the class `get` is a read-only reference, never the management path), one `RepositoryRuleset` carrying the branch law, one `RepositoryEnvironment` per environments row with reviewer and self-review gates and a branch-pattern `RepositoryEnvironmentDeploymentPolicy`, one `RepositoryDeployKey` binding a `tls.PrivateKey.publicKeyOpenssh` (read-only posture by default; the private half stays in the entropy owner), one `RepositoryWebhook` whose `configuration.secret` binds a Doppler-generated entry the receiving endpoint verifies, and `ActionsVariable` rows for non-secret pipeline configuration.
- Law: environments align three surfaces — an environments row's `name` is a `StackSpec.doppler.config` spelling, so the `secretssync.GithubActions` mirror targeting that environment, the gate reviewers protecting it, and the stack deploying under it read one vocabulary; an environment named outside the doppler axis is the split-brain this alignment law forbids.
- Law: material splits by kind — secret values arrive ONLY through the Doppler mirror into the shells; `ActionsVariable` carries non-secret configuration; a credential authored through an Actions secret beside the mirror is the second-source defect, and the deploy key's `key` field accepts only the public half.
- Law: one branch-law owner per repo — `RepositoryRuleset` for this estate; `BranchProtection` survives only when adopting a classic-protected repo, never beside the ruleset.
- Law: merge hygiene is settings-as-code — the `Repository` row carries the whole merge posture (`allowMergeCommit: false`, `allowSquashMerge: true`, `allowRebaseMerge: true`, `deleteBranchOnMerge: true`, `hasWiki: false`), so a dashboard toggle is an out-of-band edit the drift fold surfaces and repo posture never lives in operator memory.
- Law: rate posture is provider data — retries, delays, and parallelism ride the `Provider` knobs, never per-resource retry wrappers.
- Entry: `new Source("source", { spec, owner, token, repository, environments, webhook, variables }, opts)` from the composing root, `token` the `GITHUB_TOKEN` fan-in read; `source.deployKey.privateKeyOpenssh` stays graph-interior; `RepositoryEnvironment` names feed the `_MIRRORS` githubActions rows.
- Growth: a new gated environment is one `environments` row; a new org-level posture (`Team`, `TeamRepository`, `OrganizationRuleset`) is one row when the estate grows an org.
- Boundary: the mirror mechanics are `operate/secret.md`'s; the CI workflows that run inside the shells are app repo material, never lib code; `appAuth` (GitHub-App identity) supersedes the PAT when the estate earns a durable machine identity.
- Packages: `@pulumi/github` (`Provider`, `Repository`, `RepositoryRuleset`, `RepositoryEnvironment`, `RepositoryEnvironmentDeploymentPolicy`, `RepositoryDeployKey`, `RepositoryWebhook`, `ActionsVariable`); `@pulumi/tls` (`PrivateKey`); `effect` (`Array`, `Record`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as github from "@pulumi/github"
import * as pulumi from "@pulumi/pulumi"
import * as tls from "@pulumi/tls"
import { Array, Record, Schema } from "effect"
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
  static distribute(name: string, args: Source.Distribution, opts?: pulumi.ComponentResourceOptions): Source.Distributed {
    return {
      folder: _FOLDERS[args.arm](name, args, opts),
      served: Record.fromEntries(Array.map(Schema.decodeUnknownSync(_Assets)(args.assets ?? []), _addressed)),
    }
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
      allowMergeCommit: false,
      allowSquashMerge: true,
      allowRebaseMerge: true,
      deleteBranchOnMerge: true,
      hasWiki: false,
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

## [03]-[DISTRIBUTION]

[DISTRIBUTION]:
- Law: the bucket is the arm's object cell, the folder is its content — `_FOLDERS` is the dialect record keyed by the arms whose object cells the synced-folder component reaches: the `aws` row converges onto `aws.s3.BucketV2.bucket`, the `gcp` row onto `gcp.storage.Bucket.name`; the component never creates a bucket, cloud credentials thread through the arm's one provider seam via `opts.providers`, and the `cloudflare` arm's static distribution stays its own `PagesProject`/R2 rows — no R2 dialect exists here and none is faked through the S3 row.
- Law: the sync policy is one row — `sync.managed` (default true) tracks every file as a state-managed object (per-file diffs, policy visibility, drift evidence) with `false` the large-site row where per-file state is cost; `sync.hidden` admits the dotfiles (`.well-known`) the sync skips by default; `sync.aliases: false` opts out of the aliasing that smooths a managed-mode flip; `path` points at a built artifact directory handed in like every pin — the UI folder's build product, never a lib literal.
- Law: artifact identity enters as caller data admitted once — each `{ slug, digest, file }` row decodes through the `_Assets` schema at `distribute`: path-safe single-segment `digest` and `file` spellings (no separators, no traversal, no empty form) and slug-unique rows, so a malformed or colliding row fails the deploy loud and never overwrites a served entry; the plane addresses rows without interpreting the slug, and no UI codec vocabulary is declared here.
- Law: serving paths are content-addressed and immutable — `_addressed` derives `assets/<digest>/<file>` from the row alone, both seam ends compute the identical derivation so no free-form path string ever crosses, a byte change mints a new digest hence a new path, and every published path is cache-forever; edge cache posture over `assets/*` rides the owning arm's CDN rows, never per-file metadata writes.
- Law: `distribute` returns the synced component beside the `served` slug-to-path record, and the composing arm returns that record through `StackOutputs`; under `sync.managed`, a declared artifact absent from the built directory surfaces as per-object drift.
- Entry: `Source.distribute("frontend", { arm: "aws", path, bucket, assets }, { providers })` inside the owning arm after its object cell stands; the returned `served` record exits through the arm's `served` plane keys.
- Growth: a new distribution target is one `_FOLDERS` row when a new arm's dialect ships; a new artifact is one caller row.
- Boundary: fronting DNS/CDN rows stay on the owning arms; artifact-vocabulary minting stays with the consuming folder; the `served` plane's decode is `spec.md`'s.
- Packages: `@pulumi/synced-folder` (`S3BucketFolder`, `GoogleCloudFolder`); `effect` (`Array`, `Record`, `Schema`).

```typescript
import * as syncedFolder from "@pulumi/synced-folder"

declare namespace Source {
  type AssetRow = typeof _Asset.Type
  type Distribution = {
    readonly arm: keyof typeof _FOLDERS
    readonly path: string
    readonly bucket: pulumi.Input<string>
    readonly assets?: ReadonlyArray<AssetRow>
    readonly sync?: {
      readonly managed?: boolean
      readonly hidden?: boolean
      readonly aliases?: boolean
    }
  }
  type Distributed = {
    readonly folder: pulumi.ComponentResource
    readonly served: Record.ReadonlyRecord<string, string>
  }
}

// path admission: one relative segment per coordinate — no separators, no traversal, no empty spelling —
// so the derived assets/<digest>/<file> address is safe by construction, and slug-unique rows keep
// Record.fromEntries collision-free
const _Asset = Schema.Struct({
  slug: Schema.NonEmptyString,
  digest: Schema.String.pipe(Schema.pattern(/^[A-Za-z0-9_-]+$/)),
  file: Schema.String.pipe(Schema.pattern(/^(?!\.{1,2}$)[A-Za-z0-9._-]+$/)),
})

const _Assets = Schema.Array(_Asset).pipe(
  Schema.filter((rows) => Array.dedupe(Array.map(rows, (row) => row.slug)).length === rows.length, {
    message: () => "asset slugs are unique — a later row never silently overwrites a served entry",
  }),
)

const _addressed = (asset: Source.AssetRow): readonly [string, string] =>
  [asset.slug, `assets/${asset.digest}/${asset.file}`] as const

const _FOLDERS = {
  aws: (name: string, args: Source.Distribution, opts?: pulumi.ComponentResourceOptions): pulumi.ComponentResource =>
    new syncedFolder.S3BucketFolder(name, {
      path: args.path,
      bucketName: args.bucket,
      acl: "private",
      managedObjects: args.sync?.managed ?? true,
      includeHiddenFiles: args.sync?.hidden ?? false,
      disableManagedObjectAliases: args.sync?.aliases === false,
    }, opts),
  gcp: (name: string, args: Source.Distribution, opts?: pulumi.ComponentResourceOptions): pulumi.ComponentResource =>
    new syncedFolder.GoogleCloudFolder(name, {
      path: args.path,
      bucketName: args.bucket,
      managedObjects: args.sync?.managed ?? true,
      includeHiddenFiles: args.sync?.hidden ?? false,
      disableManagedObjectAliases: args.sync?.aliases === false,
    }, opts),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Source }
```

## [04]-[RESEARCH]

- [ASSET_IDENTITY_ROSTER]-[BLOCKED]: Which UI-owned encoded roster binds codec identity to `{ slug, digest, file }` and arms `codec-absent` from the sealed `served` plane?; route through `libs/typescript/ui/.planning/viewer/scene.md`, `libs/typescript/ui/IDEAS.md` `[ASSET_IDENTITY]`, and `libs/typescript/ui/TASKLOG.md` `[ASSET_IDENTITY_ROSTER]`; replace `Source.AssetRow` only after that owner exists.
