# [IAC_SOURCE]

`Source` owns the bootstrap-axis source-control shell and static-distribution fold. Repository law, deployment environments aligned with `StackSpec.doppler.config`, public deploy keys, webhook configuration, and non-secret Actions variables share one tier. `Source.distribute` converges a built frontend into an arm's object cell and publishes caller-supplied artifact rows as content-addressed `served` outputs on the admission rail — a multi-file artifact publishing every leaf under one digest directory, its rows minted by `Source.decoder` from a decoder slug and a build digest, and by `Source.set` from a baked set's own content key and its leaf roster. `secretssync.GithubActions` alone fills secret values from the canonical config. Module `iac/src/program/source.ts` grows by one environment row, `_FOLDERS` dialect row, `_DECODERS` row, `_TYPE_REPAIR` row, artifact row, or variable entry.

## [01]-[INDEX]

- [02]-[SOURCE_CONTROL]: repo, branch law, environment gates, deploy keys, webhook, variables; `Source`.
- [03]-[DISTRIBUTION]: synced-folder dialects, artifact mints, digest-directory addresses, leaf presence, headers; `Source`.

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
- Packages: `@pulumi/github` (`Provider`, `Repository`, `RepositoryRuleset`, `RepositoryEnvironment`, `RepositoryEnvironmentDeploymentPolicy`, `RepositoryDeployKey`, `RepositoryWebhook`, `ActionsVariable`); `@pulumi/tls` (`PrivateKey`); `@pulumi/pulumi` (`Input`, `ComponentResourceOptions`); `effect` (`Array`, `Option`, `Record`, `Schema`); `./spec.ts` (`StackSpec`, `Tier`).

```typescript signature
import * as github from "@pulumi/github"
import * as pulumi from "@pulumi/pulumi"
import * as tls from "@pulumi/tls"
import { Array, Option, Record, Schema } from "effect"
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
  // decoder rows own their leaf sets; the digest arrives from the build that copied those bytes, so a
  // caller supplies identity alone and no call site re-spells a filename the viewer resolves by name
  static decoder(slug: Source.DecoderSlug, digest: string): Source.AssetRow {
    return { slug, digest, ..._DECODERS[slug] }
  }
  // served-header rows reach an arm that converges no folder — a static origin uploading its build product
  // out of graph fronts the same addresses and reads these values rather than re-spelling one
  static get edge(): typeof _EDGE_RULES {
    return _EDGE_RULES
  }
  // a baked set's own content key IS its digest directory, so slug and digest are one value and the leaf
  // roster's head takes the bare key; the key enters DECODED (branded lowercase hex) because the peer's
  // X32 wire spelling is uppercase and `_Asset.digest` refuses it rather than forking one set in two
  static set(key: string, leaves: Array.NonEmptyReadonlyArray<string>): Source.AssetRow {
    const [file, ...siblings] = leaves
    return { slug: key, digest: key, file, siblings }
  }
  // admission runs BEFORE the dialect registers — a refused artifact set leaves no converging component
  // behind, and the built directory answers both the collision question and the presence question at once;
  // the minted decoder rows join the caller's own here, so collision and presence prove over the WHOLE
  // published set rather than over the app's half of it
  static distribute(name: string, args: Source.Distribution, opts?: pulumi.ComponentResourceOptions): Source.Distributed {
    const rows = Schema.decodeUnknownSync(_assetsUnder(args.path))([..._decoders(args.decoders), ...(args.assets ?? [])])
    return {
      folder: _FOLDERS[args.arm](name, args, opts),
      served: Record.fromEntries(Array.flatMap(rows, _addressedAll)),
      edge: _EDGE_RULES,
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
- Law: artifact identity enters as caller data admitted once — each `{ slug, digest, file, siblings }` row decodes at `distribute` over one `_SEGMENT` admission (no separators, no traversal, no empty or dot-only form) so a slug can never spell a derived key and a leaf can never escape its directory; `siblings` names every further leaf publishing under the row's own digest, which is what makes a multi-file artifact ONE identity rather than a family of independently-versioned rows.
- Law: a baked set publishes under its OWN content key and `Source.set` is that mint — a `TextureSet`'s `setKey` and an `AssetSetManifest`'s `manifestKey` each name the identity a viewer resolves BY and the digest directory its leaves live UNDER, so slug and digest are one value, the leaf roster enters in document order with its head taking the bare key, and the consuming end's `[setKey, file]` resolver derives the address this fold wrote without a second coordinate crossing. The key enters DECODED — the branded lowercase hex — because the peer's X32 wire spelling is uppercase and `_Asset.digest` refuses it here rather than publishing one set across two directories; a caller lowering a wire key at the join is the deleted direction the admission already forecloses.
- Law: decoder rows are estate-invariant and their digests are build facts, so `distribute` takes `decoders` as one slug-to-digest map and mints the rows itself — `_DECODERS` supplies every leaf name, a caller states one digest per slug it ships, and no roster re-spells a filename the loader resolves by name; the minted rows join the caller's own artifacts BEFORE admission, so collision and presence prove over the whole published set rather than over the app's half of it.
- Law: `assets` takes the ENCODED row, so `siblings` enters optional and the schema fills the empty set — a single-leaf app artifact spells three fields while `Source.decoder` and `Source.set` each fill all four, and one entry type admits every form because a decoded row satisfies its own encoded shape; demanding the decoded row at the entry refuses every caller whose artifact roster predates the sibling column while its own admission accepts them.
- Law: `_Assets` admits on the derived ENTRY set, never on a field — the flattened `_addressedAll` output is injective in both directions, so a duplicate slug, a repeated sibling, a sibling colliding with its own `file`, and two rows sharing one digest all fail as the single collision they are; a per-field uniqueness check passes the two-rows-one-directory case that silently gives one address space two owners.
- Law: serving paths are content-addressed and immutable — `_addressed` is the ONE address projection of `_segments`, the empty leaf spelling the digest directory itself, so the file form and the directory form cannot drift; a byte change mints a new digest hence a new path and a new directory, and both seam ends compute the derivation independently because no free-form path string ever crosses.
- Law: the digest segment is LOWERCASE at admission — a hex key lowers ONCE, at egress-name construction, so `_Asset` admits no uppercase letter and a key handed over un-lowered refuses here instead of publishing `assets/<UPPER>/…` beside the `assets/<lower>/…` the reading end derives from the same key; uppercasing a path segment to match a wire value is the deleted direction, and one set answering to two directories is the address fork this admission forecloses.
- Law: one row publishes three key shapes — the bare slug carries the row's primary leaf, the trailing-slash key carries its digest directory, and each sibling appends its own leaf; a consumer resolving a decoder by directory and a consumer resolving one file read the same row without a second identity, and the primary leaf takes the bare key exactly once so no address answers to two keys.
- Law: `_DECODERS` owns the served decoder leaf sets and `Source.decoder` mints the row from a slug and a build-supplied digest — `draco` publishes `draco_wasm_wrapper.js` beside `draco_decoder.wasm` and the `draco_decoder.js` fallback because the loader resolves all three from the directory it is handed and dropping the fallback strands every runtime without wasm; `ktx2` publishes `basis_transcoder.js` beside `basis_transcoder.wasm`; `meshopt` publishes the UMD decoder build under a `meshopt_decoder.js` leaf alone — its consumer injects a CLASSIC script element, so the ES-module distribution leaf raises a syntax error nothing reports, and the distribution's own `.cjs` spelling stamps `application/node` at the sync (a MIME `nosniff` refuses for scripts), so the build copies the UMD bytes to the `.js` leaf the row names. `msc_basis_transcoder` is deleted vocabulary the current transcoder refuses by name — a row spelling it publishes bytes no consumer loads.
- Law: `_EDGE_RULES` is the estate's one served-header roster and `Source.edge` publishes it to EVERY arm, not only the arms that converge a folder — a static origin whose build product uploads out of graph fronts the same addresses, so reachability tied to a `distribute` call is what forces that arm to invent a second literal. The dialects expose no per-object coordinate at all — `path`, `bucketName`, the managed/hidden/alias policy triple, and the aws row's `acl` are the whole argument surface — so no header this roster carries is settable at the sync, every one lands on whichever edge fronts the bytes, and one `pattern`/`header`/`value` shape carries each.
- Law: `_CACHE_POSTURE` is the roster's immutable-asset row — `assets/*` answers `Cache-Control: public, max-age=31536000, immutable`, because a content-addressed leaf never changes bytes under one path and revalidation buys nothing the address has already proven. The value is TWO-ENDED: the runtime serving plane transcribes it verbatim and selects on the same address before any filename fingerprint (`runtime/serve/route.md` `_cached`), so an unfronted origin and a fronting edge answer identically on one address and a divergence is a two-ended edit — no import crosses the two planes to enforce it, which is why the value carries a stated counterpart rather than a per-arm default.
- Law: `_TYPE_REPAIR` pins the served type for every egress extension the origin cannot be trusted to resolve — the sync's content-type derivation belongs to the provider plugin, versioned outside this estate and unreachable from any argument the dialects expose, so an extension that table misses publishes a type a `nosniff` origin makes fatal for every consumer that types its read. The roster answers by stating the type where the value IS this estate's, at the edge; a row duplicating a type the origin already resolves is inert, so the roster costs nothing when a provider table catches up and holds when it does not. `hdr` and `jxl` carry the rows because radiance and JPEG-XL are the two egress media types no provider table decides for this plane. Rows here state the SERVED type of bytes this plane already publishes and never what a codec means.
- Law: the served address IS the object key — no dialect carries a key prefix, so `assets/<digest>/<leaf>` is equally the published address and the leaf's path relative to `path`, and `_segments` owns that coordinate once: joined on `/` it spells the address, spread into the host join it spells where the build copies the bytes. Two answers for one leaf grow exactly where a second layout literal lands.
- Law: `_assetsUnder` gates presence at graph construction — the sync manages only the files it FINDS, so a declared leaf the build never copied mints no object, raises no drift, and publishes an address that answers 404 until a consumer refuses at runtime; the gate runs BEFORE the dialect registers, names every absent leaf, and leaves no converging component behind a refused artifact set. Per-object drift under `sync.managed` proves a MANAGED object changed and proves nothing about an artifact the roster declared and the directory never held.
- Law: `distribute` returns the synced component beside the `served` slug-to-path record and the edge roster, and the composing arm returns that record through `StackOutputs`.
- Entry: `Source.distribute("frontend", { arm: "aws", path, bucket, assets, decoders }, { providers })` inside the owning arm after its object cell stands; `decoders` is the build's slug-to-digest map and `assets` carries the composing root's own rows — `Source.set(set.setKey, leaves)` per baked texture or environment set beside the app's plain artifacts. The returned `served` record exits through the arm's `served` plane keys, and `Source.edge` reads the header roster from any arm with an edge.
- Growth: a new distribution target is one `_FOLDERS` row carrying its bucket coordinates alone — `_sync` owns the policy triple, so no dialect restates it and no third copy drifts from these defaults; a new decoder is one `_DECODERS` row; a new served-header fact is one `_TYPE_REPAIR` row; a new artifact is one caller row and a new artifact KIND is one mint beside `decoder`/`set`.
- Boundary: fronting DNS/CDN rows stay on the owning arms; this plane declares WHICH bytes publish under one identity and never what a codec means — transcoder wiring, format vocabulary, and capability refusal stay with the consuming folder; the `served` plane's decode is `spec.md`'s.
- Packages: `@pulumi/synced-folder` (`S3BucketFolder`, `GoogleCloudFolder`); `@pulumi/pulumi` (`Input`, `ComponentResource`, `ComponentResourceOptions`); `effect` (`Array`, `Option`, `Record`, `Schema`); `node:fs` (`existsSync`); `node:path` (`join`).

```typescript signature
import * as syncedFolder from "@pulumi/synced-folder"
import { existsSync } from "node:fs"
import { join } from "node:path"

declare namespace Source {
  // entries take the ENCODED row so siblings stay optional for a single-leaf artifact; Source.decoder
  // mints the decoded row every derivation reads, its empty set already filled
  type AssetInput = typeof _Asset.Encoded
  type AssetRow = typeof _Asset.Type
  type DecoderSlug = keyof typeof _DECODERS
  type Distribution = {
    readonly arm: keyof typeof _FOLDERS
    readonly path: string
    readonly bucket: pulumi.Input<string>
    readonly assets?: ReadonlyArray<AssetInput>
    // one digest per decoder slug the build shipped: `_DECODERS` owns every leaf name, so a roster never
    // re-spells a filename the loader resolves by name and an unshipped decoder is an omitted key
    readonly decoders?: Partial<Record.ReadonlyRecord<DecoderSlug, string>>
    readonly sync?: {
      readonly managed?: boolean
      readonly hidden?: boolean
      readonly aliases?: boolean
    }
  }
  type Distributed = {
    readonly folder: pulumi.ComponentResource
    readonly served: Record.ReadonlyRecord<string, string>
    readonly edge: typeof _EDGE_RULES
  }
}

// one relative segment per coordinate — no separators, no traversal, no empty or dot-only spelling — so a
// slug can never forge a derived key and a leaf can never climb out of its digest directory
const _SEGMENT = Schema.String.pipe(Schema.pattern(/^(?!\.{1,2}$)[A-Za-z0-9._-]+$/))

const _Asset = Schema.Struct({
  slug: _SEGMENT,
  // no dot: a digest names a directory, never a file — and no uppercase: a key lowers once at egress
  // construction, so an un-lowered key refuses here instead of forking one set across two directories
  digest: Schema.String.pipe(Schema.pattern(/^[a-z0-9_-]+$/)),
  file: _SEGMENT,
  siblings: Schema.optionalWith(Schema.Array(_SEGMENT), { default: () => [] }),
})

// admission reads the DERIVED entries, so slug collisions, repeated siblings, a sibling equal to its own
// row's file, and two rows sharing one digest directory all fail here as the one collision they are — and
// refusal names every offender, because a wide derived-entry roster makes "some pair collided" a diagnosis
// no operator can act on
const _Assets = Schema.Array(_Asset).pipe(
  Schema.filter((rows) => {
    const entries = Array.flatMap(rows, _addressedAll)
    const keys = Array.map(entries, ([key]) => key)
    const paths = Array.map(entries, ([, path]) => path)
    const collided = Array.dedupe([
      ...Array.filter(keys, (key, rank) => keys.indexOf(key) !== rank),
      ...Array.filter(paths, (path, rank) => paths.indexOf(path) !== rank),
    ])
    return collided.length === 0
      ? undefined
      : `one address answers one key and one digest one row — collided: ${collided.join(", ")}`
  }),
)

// ONE coordinate owner for the pair of questions the address answers: no dialect carries a key prefix, so
// object keys ARE the path relative to the synced root — joined on "/" these segments spell the published
// address, spread into the host join they spell where the build copies the bytes
const _segments = (digest: string, leaf: string): ReadonlyArray<string> => ["assets", digest, leaf]

// sync manages only the files it FINDS: a declared leaf the build never copied mints no object, raises no
// drift, and publishes an address answering 404 until a consumer refuses at runtime — so presence proves
// at graph construction, where the roster and the built directory are both in hand
const _assetsUnder = (root: string) =>
  _Assets.pipe(
    Schema.filter((rows) => {
      const absent = Array.filter(
        Array.flatMap(rows, (row) => Array.map([row.file, ...row.siblings], (leaf) => _segments(row.digest, leaf))),
        (parts) => !existsSync(join(root, ...parts)),
      )
      return absent.length === 0
        ? undefined
        : `every declared leaf ships under the built directory — absent: ${Array.map(absent, (parts) => parts.join("/")).join(", ")}`
    }),
  )

const _addressed = (digest: string, leaf: string): string => _segments(digest, leaf).join("/")

// three key shapes per row — the bare slug carries the primary leaf, the trailing-slash key its digest
// directory, each sibling its own leaf — and no address publishes twice
const _addressedAll = (asset: Source.AssetRow): ReadonlyArray<readonly [string, string]> => [
  [asset.slug, _addressed(asset.digest, asset.file)] as const,
  [`${asset.slug}/`, _addressed(asset.digest, "")] as const,
  ...Array.map(asset.siblings, (leaf) => [`${asset.slug}/${leaf}`, _addressed(asset.digest, leaf)] as const),
]

// leaf spellings transcribe the viewer's own decoder distributions: the draco loader resolves its wrapper,
// wasm, and js fallback from the directory it is handed, and the transcoder resolves its js/wasm pair the
// same way; meshopt alone renames — its consumer injects a classic script element, which the ES-module
// leaf cannot serve, and the UMD build's .cjs spelling stamps application/node at the sync, which nosniff
// refuses — so the build copies the UMD bytes to the .js leaf this row names
const _DECODERS = {
  draco: { file: "draco_wasm_wrapper.js", siblings: ["draco_decoder.wasm", "draco_decoder.js"] },
  ktx2: { file: "basis_transcoder.js", siblings: ["basis_transcoder.wasm"] },
  meshopt: { file: "meshopt_decoder.js", siblings: [] },
} as const

// the roster is the iteration order, so an unshipped slug is an absent digest rather than a caller-ordered
// list — and every minted row travels the same admission the app's own artifacts do
const _decoders = (digests: Source.Distribution["decoders"]): ReadonlyArray<Source.AssetRow> =>
  Array.filterMap(Record.toEntries(_DECODERS), ([slug]) =>
    Option.map(Option.fromNullable(digests?.[slug]), (digest) => Source.decoder(slug, digest)))

// content-addressed leaves never change bytes under one path, so revalidation buys nothing the address
// has not already proven
const _CACHE_POSTURE = {
  pattern: "assets/*",
  header: "Cache-Control",
  value: "public, max-age=31536000, immutable",
} as const

// the sync's type derivation is the provider plugin's own table — versioned outside this estate and
// unreachable from any dialect argument — so the roster pins the served type wherever a miss would make a
// nosniff origin fatal; a pin duplicating a type the origin already resolves is inert, which is what keeps
// the roster true across a provider-side table bump in either direction
const _TYPE_REPAIR = [
  { pattern: "assets/*.hdr", header: "Content-Type", value: "image/vnd.radiance" },
  { pattern: "assets/*.jxl", header: "Content-Type", value: "image/jxl" },
] as const

// ONE roster of the headers the sync cannot set, ordered cache-then-type: an arm that converges a folder
// reads it off Distributed, an arm whose static origin uploads out of graph reads Source.edge, and neither
// re-spells a value — the second literal is how one estate grows two answers for one address
const _EDGE_RULES = [_CACHE_POSTURE, ..._TYPE_REPAIR] as const

// sync policy is ONE fact every dialect shares, so it projects once and each row spreads it — a new
// dialect then carries its bucket coordinates alone and no copy of the triple drifts from these
// defaults, which is the whole difference between a dialect record and parallel constructions
const _sync = (args: Source.Distribution) => ({
  path: args.path,
  managedObjects: args.sync?.managed ?? true,
  includeHiddenFiles: args.sync?.hidden ?? false,
  disableManagedObjectAliases: args.sync?.aliases === false,
})

const _FOLDERS = {
  aws: (name: string, args: Source.Distribution, opts?: pulumi.ComponentResourceOptions): pulumi.ComponentResource =>
    // a fresh BucketV2 defaults Object Ownership to bucket-owner-enforced, which refuses every ACL-bearing
    // PutObject EXCEPT the bucket-owner-full-control canned ACL — the one spelling the required `acl` knob
    // accepts under both ownership postures, so the sync never trips AccessControlListNotSupported
    new syncedFolder.S3BucketFolder(name, { ..._sync(args), bucketName: args.bucket, acl: "bucket-owner-full-control" }, opts),
  gcp: (name: string, args: Source.Distribution, opts?: pulumi.ComponentResourceOptions): pulumi.ComponentResource =>
    new syncedFolder.GoogleCloudFolder(name, { ..._sync(args), bucketName: args.bucket }, opts),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Source }
```

## [04]-[RESEARCH]

(none)
