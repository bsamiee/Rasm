# [TS_IAC_API_PULUMI_RANDOM]

`@pulumi/random` is the provider-tracked entropy owner: passwords, strings, ids, byte suffixes, integers, seeded shuffles, and versioned UUIDs whose value lives in Pulumi state and regenerates ONLY when its `keepers` recreation-trigger map changes — deterministic across `up` runs, unlike a runtime RNG. `iac` composes it as generated secret material that flows sensitive into the `secret`/`kube`/`data` rows: one `keepers` rotation pattern owns recreation everywhere, and one char-class policy shape drives both `RandomPassword` and `RandomString`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/random`
- package: `@pulumi/random`
- module: `@pulumi/random`
- license: `Apache-2.0`
- asset: provider-tracked random material (passwords, strings, ids, bytes, integers, shuffles, UUIDs)
- runtime: `node` — Terraform-bridge provider plugin auto-downloads on first resource registration; values persist in stack state
- rail: fabric

## [02]-[RESOURCE_SURFACE]

Every resource extends `pulumi.CustomResource`, carries `static get(name, id, state?, opts?)` + `static isInstance(obj)` + `constructor(name, args, opts?)`, and shares the universal `keepers?: Input<{[k]: Input<string>}>` recreation trigger. Generated values surface as `Output<T>`; `RandomPassword`/`RandomString` `result` is state-encrypted sensitive.

[PUBLIC_TYPE_SCOPE]: resource roster
- rail: fabric

| [INDEX] | [SYMBOL] | [REQUIRED_ARGS] | [KEY_OUTPUTS] | [NOTE] |
|:-----: |:--------------- |:---------------------- |:------------------------------------- |:---------------------------------------- |
| [01] | `RandomPassword` | `length` | `result` (secret), `bcryptHash` | cryptographic RNG; sensitive; char-class knobs |
| [02] | `RandomString` | `length` | `result` | same knobs as `RandomPassword`, non-sensitive |
| [03] | `RandomId` | `byteLength` | `hex`, `dec`, `b64Std`, `b64Url` | N random bytes projected to four encodings |
| [04] | `RandomBytes` | `length` | `base64`, `hex` | raw random bytes; sensitive base64 |
| [05] | `RandomInteger` | `min`, `max` | `result` (number) | seeded bounded integer |
| [06] | `RandomShuffle` | `inputs` | `results` (string[]) | `seed`-stable permutation; `resultCount` |
| [07] | `RandomPet` | — | id = pet name (`length`/`prefix`/`separator`) | human-friendly stable resource names |
| [08] | `RandomUuid` | — | `result` (uuid) | retired catalog-bound UUID |
| [09] | `RandomUuid4` | — | `result` (uuid catalog-bound) | explicit catalog-bound |
| [10] | `RandomUuid7` | — | `result` (uuid catalog-bound) | time-ordered UUID (sortable ids) |
| [11] | `Provider` | — | — | explicit provider instance |

## [03]-[MATERIAL_PROJECTIONS]

Three parameterized patterns own the surface; the roster above is seed data feeding them, not eleven independent recipes.

[PATTERN]: char-class policy — ONE shape drives `RandomPassword` + `RandomString`

| [INDEX] | [KNOB] | [ROLE] |
|:-----: |:------------------------------------------------- |:---------------------------------------------- |
| [01] | `length` | total length; `≥ minLower+minUpper+minNumeric+minSpecial` |
| [02] | `lower` / `upper` / `numeric` / `special` | character-class toggles (default `true`) |
| [03] | `minLower` / `minUpper` / `minNumeric` / `minSpecial` | per-class minimums |
| [04] | `overrideSpecial` | replace the default `!@#$%&*()-_=+[]{}<>:?` set |
| [05] | `number` (deprecated) → `numeric` | retired alias; use `numeric` |

The sensitive-vs-plain choice is a mode arm over ONE knob struct: `RandomPassword` (encrypted `result` + `bcryptHash`) vs `RandomString` (plain `result`). Document/drive the policy once; dispatch the resource on a `sensitive` tag.

[PATTERN]: entropy encoding — ONE random source, multiple projections
- `RandomId(byteLength)` projects the same bytes to `hex` / `dec` / `b64Std` / `b64Url`; `RandomBytes(length)` exposes `base64` / `hex` — pick the encoding a consumer needs (DNS-safe `hex`, URL-safe `b64Url`) rather than post-processing.

[PATTERN]: `keepers` rotation — ONE recreation trigger on every resource
- `keepers` is an arbitrary `{[k]: string}` map; changing any value forces recreation (new material) on the next `up`. Rotate a credential by bumping a keeper (`{ epoch: "<n>" }`), never by deleting the resource. This is the single rotation mechanism across all eleven resources.

## [04]-[INTEGRATION]

Generated material is a sensitive `Output` that stacks into the secret + workload rows; `effect` owns the policy shape and the rotation epoch.

[RAIL]: `random → effect + sibling providers`

| [INDEX] | [RANDOM_SEAM] | [STACKS_WITH] | [COMPOSED_RAIL] |
|:-----: |:------------------------------- |:----------------------------------------- |:---------------------------------------------------------- |
| [01] | char-class knob struct | `Schema.Struct` (from `StackSpec`) | ONE decoded `PasswordPolicy` value → `RandomPassword` args |
| [02] | `RandomPassword.result` (secret) | `@pulumiverse/doppler` `Secret({value})` | `pulumi.secret(result)` → Doppler stores it canonically |
| [03] | `RandomPassword.result` | `@pulumi/kubernetes` `Secret.stringData` / `@pulumi/postgresql` role password | generated credential feeds workload + DB rows |
| [04] | `RandomId.hex` / `RandomBytes` | `ComponentResource` child names | collision-free bucket/db/stack suffixes under a tier |
| [05] | `RandomShuffle.results` (seeded) | `kube` workload placement | seed-stable AZ / replica spreading, stable across `up` |
| [06] | `keepers` | `Redacted` / `Config` rotation epoch | `keepers = { epoch }` from an Effect `Config` value |
| [07] | `RandomUuid7.result` | typed `StackOutputs` | time-ordered ids surfaced as outputs |

```ts contract
// iac/secret — generate → mark secret → store canonically, one policy shape
const dbPassword = new random.RandomPassword("db-password", {
  length: policy.length, special: policy.special,
  minSpecial: policy.minSpecial, overrideSpecial: policy.overrideSpecial,
  keepers: { epoch },                                    // bump epoch to rotate
}, { parent })
new doppler.Secret("db-password", {
  project, config, name: "DB_PASSWORD",
  value: pulumi.secret(dbPassword.result),               // sensitive Output → canonical store
}, { parent })
```

## [05]-[IMPLEMENTATION_LAW]

[MATERIAL_TOPOLOGY]:
- Prefer the provider-tracked resource over an inline random value: state persistence makes the material stable across `up` and diffable, and `keepers` makes rotation explicit and audited.
- `RandomPassword`/`RandomBytes` `result`/`base64` are sensitive; cross a resource boundary only as `pulumi.secret`-tracked `Output`. `RandomString` is plain — never use it for credentials.
- Choose the id encoding at the source (`hex` for DNS/label safety, `b64Url` for URL tokens); do not post-process an `Output<string>`.

[RAIL_LAW]:
- Package: `@pulumi/random`
- Owns: provider-tracked passwords, strings, ids, bytes, integers, seeded shuffles, versioned UUIDs
- Accept: one `Schema`-decoded char-class policy for `RandomPassword`/`RandomString`; `keepers` epoch from an Effect `Config` value; the source encoding a consumer needs
- Reject: `RandomString` for secrets; per-resource rotation logic where one `keepers` bump suffices; post-processing an id `Output` where a native projection exists; enumerating password recipes where one policy shape dispatches
