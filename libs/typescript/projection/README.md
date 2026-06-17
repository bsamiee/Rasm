# [PROJECTION]

`projection` is the unified key-discriminated transport-free fold algebra of the TypeScript branch — every store folds a wire-vocabulary discriminant verbatim over a `Stream` or receipt sequence into an immutable `SubscriptionRef`-backed keyed map, parameterized by one `StreamPolicy`. Zero consumers exist; implementation is full-capability with no holding back; `.planning/` pages are transcribed, never re-designed. The domain collapses the prior state pages into one fold-algebra owner because envelope folds and availability are identical in kind to the stream folds — co-located by altitude, not payload. It depends only on the decoded `interchange` `Schema` shapes, dials nothing, and the transport-dial import ban in the monorepo's centralized config is the sole mechanical guard keeping the fold interior transport-free. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                          | [OWNS]                                                                       |
| :-----: | :------------------------------------------------------------- | :-------------------------------------------------------------------------- |
|   [1]   | [fold-algebra](.planning/fold-algebra.md)                     | one StreamPolicy, the key-discriminated fold combinator, the five stream stores |
|   [2]   | [envelope-and-evidence](.planning/envelope-and-evidence.md)   | receipt/evidence/availability folds, the envelope carrier, the SkewBand HLC fold |

## [2]-[ADMISSIONS_RECORD]

Each package maps to its consuming page, central catalogue at `libs/typescript/.api/`, and admission status. Concrete coordinates live in the workspace catalog (`pnpm-workspace.yaml` `catalog:`); this table never carries a pin.

| [INDEX] | [PACKAGE]   | [PAGE]     | [CATALOGUE]               | [STATUS] |
| :-----: | :---------- | :--------- | :------------------------ | :------- |
|   [1]   | effect      | both pages | `.api/api-effect.md`      | admitted |
|   [2]   | interchange | both pages | (intra-package)           | admitted |

## [3]-[PROOF_GATES]

`[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]          | [RAIL]                            | [EVIDENCE]                                          |
| :-----: | :-------------- | :-------------------------------- | :------------------------------------------------- |
|  [G1]   | catalog resolve | `pnpm` install/restore            | `catalogMode` strict resolves `@rasm/ts`            |
|  [G2]   | typecheck       | `tsgo` typecheck                  | zero diagnostics over the domain                    |
|  [G3]   | import ban      | centralized lint config            | zero transport-dial imports under the domain        |
|  [G4]   | unit-pbt        | `vitest` project `projection`     | fold associativity/idempotence/monotonicity pass    |
|  [G5]   | page render     | local mermaid-cli                 | page diagrams render through the local renderer      |
