# [API_CATALOGUE] @pulumi/random

`@pulumi/random` supplies randomness resource classes — `RandomPassword`, `RandomString`, `RandomId`, `RandomBytes`, `RandomInteger`, `RandomPet`, `RandomShuffle`, `RandomUuid`, `RandomUuid4`, `RandomUuid7` — that produce deterministic Pulumi-tracked outputs, regenerating only when `keepers` change, for seeding secrets, generating resource name suffixes, and selecting values from pools.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/random`
- package: `@pulumi/random`
- module: `@pulumi/random`
- asset: random value resource classes with keeper-driven recreation
- rail: deployment

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: random resource family
- rail: deployment

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]  | [RAIL]                                             |
| :-----: | :--------------- | :------------- | :------------------------------------------------- |
|   [1]   | `RandomPassword` | resource class | secret-grade string with charset composition rules |
|   [2]   | `RandomString`   | resource class | plain string with charset composition rules        |
|   [3]   | `RandomId`       | resource class | fixed-byte random ID in hex/base64/decimal forms   |
|   [4]   | `RandomBytes`    | resource class | raw random bytes as base64 and hex outputs         |
|   [5]   | `RandomInteger`  | resource class | integer in `[min, max]` range                      |
|   [6]   | `RandomPet`      | resource class | human-readable random pet name                     |
|   [7]   | `RandomShuffle`  | resource class | shuffled subset of an input string list            |
|   [8]   | `RandomUuid`     | resource class | random UUID v4 string                              |
|   [9]   | `RandomUuid4`    | resource class | explicit UUID v4 string                            |
|  [10]   | `RandomUuid7`    | resource class | time-sortable UUID v7 string                       |

[PUBLIC_TYPE_SCOPE]: args family
- rail: deployment

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [KEY_FIELDS]                                                                                                                                  |
| :-----: | :------------------- | :------------- | :-------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `RandomPasswordArgs` | args interface | `length` (required), `special`, `upper`, `lower`, `numeric`, `minSpecial`, `minUpper`, `minLower`, `minNumeric`, `overrideSpecial`, `keepers` |
|   [2]   | `RandomStringArgs`   | args interface | `length` (required), `special`, `upper`, `lower`, `numeric`, `overrideSpecial`, `keepers`                                                     |
|   [3]   | `RandomIdArgs`       | args interface | `byteLength` (required), `keepers`, `prefix`                                                                                                  |
|   [4]   | `RandomBytesArgs`    | args interface | `length` (required), `keepers`                                                                                                                |
|   [5]   | `RandomIntegerArgs`  | args interface | `min` (required), `max` (required), `keepers`, `seed`                                                                                         |
|   [6]   | `RandomPetArgs`      | args interface | `length`, `prefix`, `separator`, `keepers`                                                                                                    |
|   [7]   | `RandomShuffleArgs`  | args interface | `inputs` (required), `resultCount`, `keepers`, `seed`                                                                                         |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resource constructors
- rail: deployment

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY]  | [OUTPUT_FIELDS]                          |
| :-----: | :-------------------------------------- | :-------------- | :--------------------------------------- |
|   [1]   | `new RandomPassword(name, args, opts?)` | resource create | `result: Output<string>` (secret-marked) |
|   [2]   | `new RandomString(name, args, opts?)`   | resource create | `result: Output<string>`                 |
|   [3]   | `new RandomId(name, args, opts?)`       | resource create | `b64Std`, `b64Url`, `hex`, `dec`         |
|   [4]   | `new RandomBytes(name, args, opts?)`    | resource create | `hex: Output<string>`, `length`          |
|   [5]   | `new RandomInteger(name, args, opts?)`  | resource create | `result: Output<number>`                 |
|   [6]   | `new RandomPet(name, args?, opts?)`     | resource create | `id: Output<string>`                     |
|   [7]   | `new RandomShuffle(name, args, opts?)`  | resource create | `results: Output<string[]>`              |
|   [8]   | `new RandomUuid(name, args?, opts?)`    | resource create | `result: Output<string>`                 |
|   [9]   | `new RandomUuid4(name, args?, opts?)`   | resource create | `result: Output<string>`                 |
|  [10]   | `new RandomUuid7(name, args?, opts?)`   | resource create | `result: Output<string>`                 |

## [4]-[IMPLEMENTATION_LAW]

[RANDOM_TOPOLOGY]:
- all resources extend `pulumi.CustomResource`; inputs accept `Input<T>`, outputs emit `Output<T>`
- `keepers`: map of arbitrary string values; any change to a keeper triggers full resource recreation and new random output — the primary mechanism for controlled regeneration
- `RandomPassword.result` is automatically marked as a Pulumi secret; `RandomString.result` is not
- `RandomId.byteLength` controls entropy; outputs: `hex` (hex string), `b64Std` (standard base64), `b64Url` (URL-safe base64), `dec` (decimal string)
- `RandomPet` default `length` is 2 words; `separator` defaults to `-`; `prefix` prepends before the first word
- `RandomShuffle.resultCount` limits the output to a subset of the shuffled list; omitting it returns all inputs shuffled
- `RandomInteger` and `RandomShuffle` accept a `seed` for deterministic output; when `seed` is set, `keepers` changes do not regenerate unless `seed` changes too

[LOCAL_ADMISSION]:
- Pass `RandomPassword.result` directly as `Input<string>` to other resource args; the `Output<string>` wrapper carries the secret mark end-to-end through `Output.apply`.
- Do not read `result` outside of `apply` or resource arg positions; secret values remain encrypted in Pulumi state.

[RAIL_LAW]:
- Package: `@pulumi/random`
- Owns: deterministic random value generation within Pulumi stacks
- Accept: `keepers` for recreation control; `seed` for deterministic repeatability
- Reject: Node.js `Math.random()` or `crypto.randomBytes()` for stack values that must be tracked, regenerated on demand, or kept secret in state
