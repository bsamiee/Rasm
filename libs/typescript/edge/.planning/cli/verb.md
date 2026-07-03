# [EDGE_VERB]

`cli/verb.ts` is the terminal entry family under the one assembly law: a verb family is a `Command` VALUE a domain contributes as data, the APP folds selected families through `Command.withSubcommands` into exactly one root — the same posture as the `HttpApi` value, so the god-CLI has no lib-side existence — and `Verb.main` is the run rail that turns the root into an argv arrow with `--help`/`--version` folded to clean exits instead of failures. The flag-config bridge is law here: a flag and its environment variable are one declaration (`Options.withFallbackConfig`), a flag decodes into a branded value at the terminal boundary (`Options.withSchema`) exactly as the wire and route boundaries decode-once, and `ConfigFile.layer` folds a config file into the same provider chain. `Ops` ships the lib verb family — `doctor`, `replay`, `inspect` — runbooks as code executing over `host/exec`, never documents.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                            | [PUBLIC] |
| :-----: | :--------------- | :------------------------------------------------------------------- | :------- |
|  [01]   | [ASSEMBLY_LAW]   | the contribution shape, the one-root fold, the clean-exit run rail    | `Verb`   |
|  [02]   | [OPS_FAMILY]     | the doctor/replay/inspect lib verbs over their capability sources     | `Ops`    |

## [2]-[ASSEMBLY_LAW]

[ASSEMBLY_LAW]:
- Owner: `Verb.main` — the run rail the lib genuinely adds: `ValidationError.isHelpRequested` folds to a clean exit (help and version are outcomes, not faults), every other `ValidationError` propagates for the boot edge to report. The FOLD itself is app code by law — `Command.make(name).pipe(Command.withSubcommands([familyA, familyB, Ops.family(sources)]), Command.run({ name, version }))` — because the package's own two combinators ARE the assembly surface and a lib member re-wrapping them would be the one-hop forward this corpus deletes; an app's CLI entry is `runMain(Verb.main(built)(argv))` and nothing else.
- Law: a verb family is contributed data — a `Command` value whose config rows are `Options`/`Args` declarations and whose handler is one `Effect`; a capability shipped both as an RPC procedure and a verb reuses ONE handler effect, with the verb owning only the terminal decode of its inputs.
- Law: the bridge rows are the boundary decode — `Options.withFallbackConfig(config)` unifies a flag with its `host/config` provider value in one declaration (flag wins, config fills, so an env var and a flag are never two sources), `Options.withSchema(schema)` decodes a flag into a kernel-branded value at parse time, and `Args.fileSchema(schema)` admits a file's content through a schema — the terminal boundary decodes once, and a handler never re-validates its inputs.
- Law: subtree capability is scoped provision — `Command.provide(family, layer)` scopes a Layer to one verb family so the ops family carries its exec runtime without leaking it to app verbs; the parser policy is one root value (`CliConfig.layer`), and `ConfigFile.layer(name)` mounts file-resolved flags at the same root.
- Boundary: `Command.run` demands the platform `Environment` (`FileSystem | Path | Terminal`) the runtime row satisfies at the boot edge — the same node/bun selection the serve rows make, so one runtime choice covers server and CLI.
- Growth: a new app verb family is contributed data (zero lib edits); a new lib runbook is one `Ops` row.
- Packages: `@effect/cli` (`Command`, `Options`, `Args`, `ValidationError`, `CliConfig`, `ConfigFile`); `effect` (`Effect`, `Layer`).

```typescript
import { Args, Command, Options, ValidationError } from "@effect/cli"
import { FileSystem } from "@effect/platform"
import { Config, Data, Effect, Option, Schema } from "effect"
import { Hook, HookIngress } from "../hook/admit.ts"
import { Render } from "./render.ts"

const _main = <E, R>(
  built: (args: ReadonlyArray<string>) => Effect.Effect<void, E | ValidationError.ValidationError, R>,
) =>
  (args: ReadonlyArray<string>): Effect.Effect<void, E | ValidationError.ValidationError, R> =>
    built(args).pipe(
      Effect.catchIf(
        (fault): fault is ValidationError.HelpRequested =>
          ValidationError.isValidationError(fault) && ValidationError.isHelpRequested(fault),
        () => Effect.void,
      ),
    )

const Verb: {
  readonly main: typeof _main
} = { main: _main }
```

## [3]-[OPS_FAMILY]

[OPS_FAMILY]:
- Owner: `Ops.family(sources)` — the lib runbook family built over app-supplied capability sources so the verbs stay composition-free: `checks` (the doctor probe rows — each a named `Effect` verdict over config resolution, engine reachability, dependency versions via `Proc.run`), `artifact` (the canonical OpenAPI emission from `api/emit.ts`), and the intake's `HookIngress` requirement for replay — one record, three verbs, every handler rendering through `cli/render.ts`.
- Law: `doctor` folds the check rows with `Effect.partition` — every probe runs, failures accumulate instead of aborting at the first, and the rendered table shows the whole verdict surface in one pass; the exit is non-zero when any probe failed, which is what makes it a CI gate and not a narration.
- Law: `replay` re-drives a captured delivery — `Args.fileSchema(Schema.parseJson(Hook))` admits the capture file through the `Hook` schema at the argv boundary, and the handler enqueues straight through `HookIngress.enqueue`, bypassing signature verification deliberately: the capture IS the already-verified hook, and re-verifying would demand the provider secret a replay operator must not need.
- Law: `inspect` emits derivations — the OpenAPI artifact to a path (or stdout under `--stdout`) — so the served contract's canonical bytes are one verb away for diffing; the `--out` flag falls back to the `INSPECT_OUT` config row through the bridge, the page's own demonstration of the flag-config law.
- Law: runbooks are code — a new runbook is one `Command` row in this family with its probe/effect, never a document; the ops family is `Command.provide`-scoped with its exec Layer by the app when it needs elevated capability.
- Boundary: process execution mechanics are `host/exec`'s (`Proc.run` with capture modality); what checks exist beyond the shipped rows is app data through `sources.checks`.
- Packages: `@effect/cli` (`Command`, `Options`, `Args`); `host/exec` (`Proc`); `api/emit` (`Emit`); `hook/admit` (`Hook`, `HookIngress`); `cli/render` (`Render`).

```typescript
class OpsFault extends Data.TaggedError("OpsFault")<{ readonly verb: string; readonly failed: number }> {
  get class(): "breached" {
    return "breached"
  }
}

declare namespace Ops {
  type Check = { readonly name: string; readonly run: Effect.Effect<string, string> }
  type Sources = {
    readonly artifact: Effect.Effect<string>
    readonly checks: ReadonlyArray<Check>
  }
}

const _out = Options.text("out").pipe(
  Options.withAlias("o"),
  Options.withFallbackConfig(Config.string("INSPECT_OUT")),
  Options.optional,
)

const _doctor = (sources: Ops.Sources) =>
  Command.make("doctor", {}, () =>
    Effect.gen(function* () {
      const [failed, passed] = yield* Effect.partition(sources.checks, (check) =>
        check.run.pipe(
          Effect.map((detail) => [check.name, detail] as const),
          Effect.mapError((detail) => [check.name, detail] as const),
        ))
      yield* Render.out(Render.verdicts({ passed, failed }))
      return yield* Effect.when(
        Effect.fail(new OpsFault({ verb: "doctor", failed: failed.length })),
        () => failed.length > 0,
      )
    }))

const _replay = Command.make("replay", { capture: Args.fileSchema(Schema.parseJson(Hook), { name: "capture" }) }, ({ capture }) =>
  Effect.gen(function* () {
    const ingress = yield* HookIngress
    const receipt = yield* ingress.enqueue(capture)
    yield* Render.out(Render.kv([["id", receipt.id], ["source", receipt.source], ["lane", receipt.lane]]))
  }))

const _inspect = (sources: Ops.Sources) =>
  Command.make("inspect", { out: _out }, ({ out }) =>
    Effect.gen(function* () {
      const artifact = yield* sources.artifact
      const fs = yield* FileSystem.FileSystem
      yield* Option.match(out, {
        onNone: () => Render.out(Render.raw(artifact)),
        onSome: (target) => fs.writeFileString(target, artifact),
      })
    }))

const _family = (sources: Ops.Sources) =>
  Command.make("ops").pipe(
    Command.withDescription("lib runbooks: doctor | replay | inspect"),
    Command.withSubcommands([_doctor(sources), _replay, _inspect(sources)]),
  )

const Ops: {
  readonly family: typeof _family
} = { family: _family }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Ops, OpsFault, Verb }
```
