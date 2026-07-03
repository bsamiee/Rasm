# [API_CATALOGUE] @pulumi/random

`@pulumi/random` is one parameterized `pulumi.CustomResource` shape instantiated across ten randomness resources (`RandomPassword`, `RandomString`, `RandomId`, `RandomBytes`, `RandomInteger`, `RandomPet`, `RandomShuffle`, `RandomUuid`, `RandomUuid4`, `RandomUuid7`) plus the package `Provider`. Every class shares the same lifecycle: a cryptographic RNG produces the value on create, the value is stored in stack state and re-emitted as a stable `Output<T>`, and the `keepers` map is the recreation discriminant — any change to a keeper (typically an upstream `Output`) forces a new random value, nothing else does. `RandomInteger`/`RandomShuffle` add a `seed` for deterministic repeatability. The load-bearing services use is deploy-time secret and identifier seeding: `RandomPassword.result` (secret-marked) for generated credentials, `RandomId.hex`/`RandomBytes.base64` for resource-name suffixes and key material, `keepers` wired to an upstream digest so a credential rotates exactly when its dependency changes.

- package: `@pulumi/random`
- version: `4.21.0`
- license: `Apache-2.0`
- tier: `node` — deploy-time only, reachable through the `./provisioning` (`iac`) subpath; never on the durable runtime hot path, never browser-reachable.
- rail: deployment

## [01]-[RESOURCE_SHAPE]

One shared shape, not ten. Every `Random*` class extends `pulumi.CustomResource` and carries the identical construction, lookup, and guard surface; only the args, the typed outputs, and the secret marking vary (the [RESOURCE_MATRIX] rows). Documenting per-class constructors would be enumerated-instance noise — the variation is data on this one pattern.

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY]   | [ROLE]                                                                 |
| :-----: | :--------------------------------------------------------------------------- | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `new Random{Kind}(name, args{, opts?})`                                       | resource ctor    | create; generates the value once, tracks it in state                  |
|  [02]   | `Random{Kind}.get(name, id: Input<ID>, state?: Random{Kind}State, opts?)`     | static lookup    | adopt an existing value into state by provider id (the `pulumi import` path) |
|  [03]   | `Random{Kind}.isInstance(obj): obj is Random{Kind}`                           | static guard     | multi-SDK-safe runtime type guard                                    |
|  [04]   | `keepers: Output<{ [k: string]: string } \| undefined>`                       | echoed output    | the recreation discriminant, echoed on every class                    |
|  [05]   | `Random{Kind}Args`                                                            | input interface  | per-kind create inputs (all fields `pulumi.Input<T>`)                 |
|  [06]   | `Random{Kind}State`                                                           | lookup interface | per-kind optional state bag for `.get()`                             |
|  [07]   | `new Provider(name, args?: ProviderArgs, opts?)` · `Provider.terraformConfig(): Output<Provider.TerraformConfigResult>` | provider ctor    | explicit provider handle for `ResourceOptions.provider`; `ProviderArgs` is empty (package-wide config) |

## [02]-[RESOURCE_MATRIX]

The ten kinds as data on the shape above: required args, the typed output surface, and whether the value is secret-marked in state. `overrideSpecial` and the `min*` fields exist on both `RandomPassword` and `RandomString` — the two share one charset-composition arg shape ([ARG_SHAPES]), differing only in that `RandomPassword.result` is secret and adds `bcryptHash`.

| [INDEX] | [KIND]           | [REQUIRED_ARGS]        | [OUTPUTS]                                              | [SECRET]                     |
| :-----: | :--------------- | :--------------------- | :---------------------------------------------------- | :--------------------------- |
|  [01]   | `RandomPassword` | `length`               | `result: Output<string>`, `bcryptHash: Output<string>` | `result` + `bcryptHash` secret |
|  [02]   | `RandomString`   | `length`               | `result: Output<string>`                              | plain (not secret)           |
|  [03]   | `RandomId`       | `byteLength`           | `hex`, `b64Std`, `b64Url`, `dec: Output<string>`       | plain                        |
|  [04]   | `RandomBytes`    | `length`               | `base64`, `hex: Output<string>`                        | `base64` secret (key material) |
|  [05]   | `RandomInteger`  | `min`, `max`           | `result: Output<number>`                              | plain                        |
|  [06]   | `RandomPet`      | — (`length` default 2) | `id: Output<string>` (the pet name)                    | plain                        |
|  [07]   | `RandomShuffle`  | `inputs`               | `results: Output<string[]>`                            | plain                        |
|  [08]   | `RandomUuid`     | — (`keepers` only)     | `result: Output<string>` (v4-style)                    | plain                        |
|  [09]   | `RandomUuid4`    | — (`keepers` only)     | `result: Output<string>` (explicit v4)                 | plain                        |
|  [10]   | `RandomUuid7`    | — (`keepers` only)     | `result: Output<string>` (time-sortable v7)            | plain                        |

## [03]-[ARG_SHAPES]

Every args field is `pulumi.Input<T>`; every kind carries the optional `keepers` map. The charset shape below is shared by `RandomPassword` and `RandomString`; the remaining kinds are narrow.

```ts contract
// @pulumi/random — shared charset composition (RandomPasswordArgs ≡ RandomStringArgs)
interface RandomStringArgs {
  length:           pulumi.Input<number>                                   // required
  special?:         pulumi.Input<boolean>                                  // include punctuation; default true
  upper?:           pulumi.Input<boolean>
  lower?:           pulumi.Input<boolean>
  numeric?:         pulumi.Input<boolean>                                  // `number` is the deprecated alias
  minSpecial?:      pulumi.Input<number>                                   // floor per class; default 0
  minUpper?:        pulumi.Input<number>
  minLower?:        pulumi.Input<number>
  minNumeric?:      pulumi.Input<number>
  overrideSpecial?: pulumi.Input<string>                                   // exact special-char set to draw from
  keepers?:         pulumi.Input<{ [k: string]: pulumi.Input<string> }>
}
// RandomPasswordArgs is identical; RandomPassword.result is secret and adds the bcryptHash output.

interface RandomIdArgs      { byteLength: pulumi.Input<number>; prefix?: pulumi.Input<string>; keepers?: Keepers }
interface RandomBytesArgs   { length: pulumi.Input<number>; keepers?: Keepers }
interface RandomIntegerArgs { min: pulumi.Input<number>; max: pulumi.Input<number>; seed?: pulumi.Input<string>; keepers?: Keepers }
interface RandomShuffleArgs { inputs: pulumi.Input<pulumi.Input<string>[]>; resultCount?: pulumi.Input<number>; seed?: pulumi.Input<string>; keepers?: Keepers }
interface RandomPetArgs     { length?: pulumi.Input<number>; prefix?: pulumi.Input<string>; separator?: pulumi.Input<string>; keepers?: Keepers }
interface RandomUuidArgs    { keepers?: Keepers }   // RandomUuid4Args / RandomUuid7Args are identical
type Keepers = pulumi.Input<{ [k: string]: pulumi.Input<string> }>
```

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every class extends `pulumi.CustomResource`; inputs accept `Input<T>`, outputs emit `Output<T>`. The value is generated once by a cryptographic RNG (`RandomId`/`RandomBytes` explicitly documented so) and is stable across updates.
- `keepers` is the SOLE recreation discriminant: change any keeper value → full recreation and a new random value; change nothing else → the value persists. Wire an upstream `Output` (a digest, an AMI id) into `keepers` to rotate exactly on that dependency's change, computed through `Output.apply`/`pulumi.all`, never a plain string compare.
- `RandomInteger`/`RandomShuffle` accept a `seed`: with `seed` set, the value is deterministic and `keepers` changes alone do NOT regenerate — only a `seed` change does. Reach for `seed` only when reproducibility outranks unpredictability.
- `RandomPassword.result` and `RandomBytes.base64` are secret-marked in state; `RandomString.result` and `RandomId.*` are not. The secret mark rides the `Output<string>` end-to-end through `Output.apply` and downstream resource args.
- `.get(name, id, state?, opts?)` adopts an out-of-band value into state (the `pulumi import` path); `.isInstance` is the multi-copy-safe guard.

[DEPLOY_STACK]: how `provisioning/contract#PROVISIONING` composes this onto `@pulumi/pulumi` (`pulumi-pulumi.md`) core and the Effect rails.
- Ownership: instantiate a `Random*` under its `TierStack` `ComponentResource` (`{ parent: this }`) so it joins the component URN tree and `registerOutputs` surface; a generated credential is a child of the data tier, a name suffix a child of the resource it disambiguates.
- Secret flow: `RandomPassword.result` (already secret) feeds a resource arg or the `SecretResolver` path directly as `Input<string>` — it stays redacted in state, so a generated DB password never appears in a diff. Prefer it over `Config.requireSecret` when the secret is machine-generated rather than operator-supplied.
- Rotation trigger: feed an upstream `Output` (an `Image.repoDigest` from `pulumi-docker.md`, a config epoch) into `keepers` so the credential/suffix rotates exactly when the dependency changes; the `@pulumi/command` (`pulumi-command.md`) `triggers` tuple consumes the same `RandomPassword.result`/`RandomId` outputs to re-run a provisioning step on rotation.

[SIBLING_STACK]:
- `@pulumi/pulumi` core owns the `Output`/`Input` algebra every arg accepts, the `CustomResourceOptions` (`parent`/`dependsOn`/`provider`/`protect`) the constructors take, and the secret-marking machinery `RandomPassword`/`RandomBytes` ride.
- `@pulumi/command` (`pulumi-command.md`) is the primary `triggers`/env consumer — a `Random*` output seeds a `Command`'s trigger tuple or environment so a shell step re-runs on regeneration.
- `effect` (`libs/typescript/.api/effect.md`) owns the `Match.exhaustive` `DeployMode` dispatch and the `Config`/`Redacted` rails the `SecretResolver` folds the generated secret into.

[RAIL_LAW]:
- Package: `@pulumi/random`
- Owns: deterministic, state-tracked random value generation within Pulumi stacks — secrets, name suffixes, key material, pool selection.
- Accept: `keepers` wired to an upstream `Output` for controlled regeneration; `seed` only when deterministic repeatability is required; `RandomPassword.result`/`RandomBytes.base64` as secret `Input<string>` flowing through `Output.apply`.
- Reject: `Math.random()`/`crypto.randomBytes()` for stack values that must be tracked, regenerated on demand, or kept secret in state; reading a secret `result` outside an `apply`/arg position; a plain-string `keepers` compare where an `Output` tuple belongs.
