# [TS_IAC_API_PULUMI_RANDOM]

`@pulumi/random` mints provider-tracked entropy: its value persists in Pulumi state and regenerates only when a resource's `keepers` map changes, so material is deterministic across `up` runs where a runtime RNG is not.

`iac` composes it as sensitive `Output` material feeding the secret, kube, and data rows: one `keepers` bump rotates any credential, and one char-class policy shape drives both `RandomPassword` and `RandomString`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/random`
- package: `@pulumi/random` (Apache-2.0)
- module: `@pulumi/random` — flat resource-class exports over the Terraform-bridge provider plugin
- runtime: `node`; the provider plugin auto-downloads on first resource registration and values persist in stack state
- rail: fabric

## [02]-[RESOURCE_SURFACE]

Every resource extends `pulumi.CustomResource`, carries `static get(name, id, state?, opts?)` + `static isInstance(obj)` + `constructor(name, args, opts?)`, and shares the universal `keepers?: Input<{[k]: Input<string>}>` recreation trigger. Generated values surface as `Output<T>`; `RandomPassword`/`RandomString` `result` is state-encrypted sensitive.

[PUBLIC_TYPE_SCOPE]: resource roster

| [INDEX] | [SYMBOL]         | [REQUIRED_ARGS] | [KEY_OUTPUTS]                                 | [NOTE]                     |
| :-----: | :--------------- | :-------------- | :-------------------------------------------- | :------------------------- |
|  [01]   | `RandomPassword` | `length`        | `result` (secret), `bcryptHash`               | cryptographic RNG          |
|  [02]   | `RandomString`   | `length`        | `result`                                      | same char-class knobs      |
|  [03]   | `RandomId`       | `byteLength`    | `hex`, `dec`, `b64Std`, `b64Url`              | bytes to four encodings    |
|  [04]   | `RandomBytes`    | `length`        | `base64` (secret), `hex`                      | raw random bytes           |
|  [05]   | `RandomInteger`  | `min`, `max`    | `result` (number)                             | seeded bounded integer     |
|  [06]   | `RandomShuffle`  | `inputs`        | `results`, `resultCount`                      | `seed`-stable permutation  |
|  [07]   | `RandomPet`      | —               | id = pet name (`length`/`prefix`/`separator`) | human-friendly names       |
|  [08]   | `RandomUuid`     | —               | `result` (uuid)                               | random UUID v4             |
|  [09]   | `RandomUuid4`    | —               | `result` (uuid)                               | explicit UUIDv4            |
|  [10]   | `RandomUuid7`    | —               | `result` (uuid)                               | time-ordered sortable ids  |
|  [11]   | `Provider`       | —               | —                                             | explicit provider instance |

## [03]-[MATERIAL_PROJECTIONS]

Three parameterized patterns own the surface; the roster above is seed data feeding them, not one recipe per resource.

[PATTERN]: char-class policy — ONE shape drives `RandomPassword` + `RandomString`

| [INDEX] | [KNOB]                                                | [ROLE]                                                    |
| :-----: | :---------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `length`                                              | total length; `≥ minLower+minUpper+minNumeric+minSpecial` |
|  [02]   | `lower` / `upper` / `numeric` / `special`             | character-class toggles (default `true`)                  |
|  [03]   | `minLower` / `minUpper` / `minNumeric` / `minSpecial` | per-class minimums                                        |
|  [04]   | `overrideSpecial`                                     | replace the default special-character set                 |

`RandomPassword` (encrypted `result` + `bcryptHash`) and `RandomString` (plain `result`) are one knob struct under a sensitivity arm; the `iac` dispatch picks the resource on a `sensitive` tag.

[PATTERN]: entropy encoding — ONE random source, multiple projections
- `RandomId(byteLength)` projects the same bytes to `hex` / `dec` / `b64Std` / `b64Url` and `RandomBytes(length)` to `base64` / `hex`; pick the encoding a consumer needs (DNS-safe `hex`, URL-safe `b64Url`) at the source.

[PATTERN]: `keepers` rotation — ONE recreation trigger on every resource
- `keepers` is an arbitrary `{[k]: string}` map; changing any value forces recreation on the next `up`. Rotate a credential by bumping a keeper (`{ epoch: "<n>" }`), never by deleting the resource.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Provider-tracked resources own random material: state persistence keeps it stable and diffable across `up`, and `keepers` makes rotation explicit and audited.
- `RandomPassword`/`RandomBytes` `result`/`base64` are sensitive and cross a boundary only as `pulumi.secret`-tracked `Output`; `RandomString` is plain, never a credential.
- Choose the id encoding at the source (`hex` for DNS/label safety, `b64Url` for URL tokens); never post-process an `Output<string>`.

[STACKING]:
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): `pulumi.secret(RandomPassword.result)` binds `Secret.value`, storing the credential canonically.
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `RandomPassword.result` feeds `core.v1.Secret.stringData`; seeded `RandomShuffle.results` spreads AZ/replica placement stable across `up`.
- `@pulumi/postgresql`(`.api/pulumi-postgresql.md`): `RandomPassword.result` binds a `Role.password`.
- `effect`(`libs/typescript/.api/effect.md`): a `Schema.Struct`-decoded char-class policy drives `RandomPassword`/`RandomString` args, and `keepers = { epoch }` reads an Effect `Config`/`Redacted` value.
- within-lib: `RandomId.hex`/`RandomBytes.base64` name collision-free `ComponentResource` children under a tier, and `RandomUuid7.result` surfaces as a time-ordered `StackOutputs` id.

[LOCAL_ADMISSION]:
- Admitted wherever generated material must persist and diff in state and rotate under audit; an inline runtime RNG value is rejected for that role.

[RAIL_LAW]:
- Package: `@pulumi/random`
- Owns: provider-tracked passwords, strings, ids, bytes, integers, seeded shuffles, versioned UUIDs
- Accept: one `Schema`-decoded char-class policy for `RandomPassword`/`RandomString`; a `keepers` epoch from an Effect `Config` value; the source encoding a consumer needs
- Reject: `RandomString` for secrets; per-resource rotation logic where one `keepers` bump suffices; post-processing an id `Output` where a native projection exists; a password recipe enumerated per resource where one policy shape dispatches
