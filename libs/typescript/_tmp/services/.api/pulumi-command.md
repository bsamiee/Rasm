# [API_CATALOGUE] @pulumi/command

`@pulumi/command` runs shell commands inside the Pulumi resource graph across two symmetric namespaces — `local.*` on the deployment host and `remote.*` on an SSH target. Each namespace carries the same split: a lifecycle-tracked `Command` `CustomResource` (create/update/delete hooks that re-run on `triggers` change) and, for `local`, an unconditional `run`/`runOutput` invoke that fires every `pulumi up`/`preview`. `remote` adds `CopyToRemote`/`CopyFile` file-transfer resources over the same SSH `Connection`. The load-bearing surface for the deploy tier is `local.Command` (idempotent host provisioning steps), `remote.Command` (self-hosted node bootstrap over SSH), and the full `Connection` shape; the invoke functions are the escape hatch for read-time values.

- package: `@pulumi/command`
- version: `1.2.1`
- license: `Apache-2.0`
- tier: `node` — deploy-time only, reachable through the `./provisioning` (`iac`) subpath; never on the durable runtime hot path, never browser-reachable.
- rail: deployment

## [01]-[PACKAGE_SURFACE]

Two namespaces (`local`, `remote`) plus a package-level `Provider`. Every resource is an `Output`-bearing `pulumi.CustomResource`; every invoke returns a bare `Promise` (async) or an `Output`-wrapped mirror. `local.Command`/`remote.Command` are the lifecycle-tracked forms; `local.run`/`runOutput` are the resource-less forms.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]  | [DESCRIPTION]                                              |
| :-----: | :--------------------------------------------------------------- | :-------------- | :-------------------------------------------------------- |
|  [01]   | `new local.Command(name, args?, opts?)`                          | resource ctor   | lifecycle-tracked host command; runs on create/update/delete |
|  [02]   | `local.run(args, opts?): Promise<RunResult>`                     | async invoke    | unconditional host run, every `up`/`preview`              |
|  [03]   | `local.runOutput(args, opts?): Output<RunResult>`                | output invoke   | same, `Output`-wrapped for `apply` chains                 |
|  [04]   | `new remote.Command(name, args, opts?)`                          | resource ctor   | lifecycle-tracked SSH command; `connection` required      |
|  [05]   | `new remote.CopyToRemote(name, args, opts?)`                     | resource ctor   | copy an `Asset`/`Archive` to the remote host              |
|  [06]   | `new remote.CopyFile(name, args, opts?)`                         | resource ctor   | copy a local file path to the remote host (`@deprecated` — use `CopyToRemote`) |
|  [07]   | `new Provider(name, args?, opts?)`                               | provider ctor   | explicit provider handle for `ResourceOptions.provider`   |

`types.input.remote` / `types.output.remote` carry the shared `Connection`/`ProxyConnection` shapes; `enums.local.Logging` / `enums.remote.Logging` carry the log-capture vocabulary.

## [02]-[LOCAL_SURFACE]

[COMMAND]: `local.Command` — the lifecycle-tracked host command. Args and the mirrored outputs (every input echoes as an `Output`, plus the computed `stdout`/`stderr`/`archive`/`assets`):

```ts
interface local.CommandArgs {
  create?:                 pulumi.Input<string>            // shell run on create (and on update when `update` is absent)
  update?:                 pulumi.Input<string>            // shell run on update
  delete?:                 pulumi.Input<string>            // shell run on delete
  triggers?:               pulumi.Input<any[]>             // re-run when any element (incl. upstream `Output<T>`) changes
  environment?:            pulumi.Input<{ [k: string]: string }>
  interpreter?:            pulumi.Input<string[]>          // argv prefix; default ["/bin/sh","-c"] on Linux/macOS
  dir?:                    pulumi.Input<string>            // working directory
  stdin?:                  pulumi.Input<string>            // fed to the process stdin
  archivePaths?:           pulumi.Input<string[]>          // globs collected into `archive` output ('/' seps, '!' excludes, '**' recursive)
  assetPaths?:             pulumi.Input<string[]>          // globs collected into `assets` map
  addPreviousOutputInEnv?: pulumi.Input<boolean>           // default true; injects PULUMI_COMMAND_STDOUT/STDERR into the next update/delete
  logging?:                pulumi.Input<enums.local.Logging>
}
// outputs: stdout: Output<string>, stderr: Output<string>,
//   archive: Output<asset.Archive | undefined>, assets: Output<{[k:string]: Asset|Archive} | undefined>,
//   plus create/update/delete/dir/stdin/environment/interpreter/triggers/logging/addPreviousOutputInEnv echoed as Output<… | undefined>
```

[RUN]: `local.run`/`runOutput` — the resource-less invoke. `RunArgs` mirrors `CommandArgs` minus the lifecycle hooks (`command` is the single required run string); `RunOutputArgs` lifts every field to `pulumi.Input<T>` for `Output.apply` chains; `RunResult` returns the resolved `stdout`/`stderr`/`archive`/`assets`:

```ts
interface local.RunArgs {          // command: string (required); dir/environment/interpreter/stdin/archivePaths/assetPaths/addPreviousOutputInEnv/logging optional
  command: string; dir?: string; environment?: {[k:string]: string}; interpreter?: string[]
  stdin?: string; archivePaths?: string[]; assetPaths?: string[]; addPreviousOutputInEnv?: boolean; logging?: enums.local.Logging
}
interface local.RunResult { stdout: string; stderr: string; archive?: asset.Archive; assets?: {[k:string]: Asset|Archive}; /* + echoed inputs */ }
```

## [03]-[REMOTE_SURFACE]

[COMMAND]: `remote.Command` — the SSH-executed command. Args drop the host-only fields (`interpreter`/`dir`/`archivePaths`/`assetPaths`) and require a `connection`:

```ts
interface remote.CommandArgs {
  connection:              pulumi.Input<remote.ConnectionArgs>   // required SSH target (see below)
  create?:                 pulumi.Input<string>
  update?:                 pulumi.Input<string>
  delete?:                 pulumi.Input<string>
  triggers?:               pulumi.Input<any[]>
  environment?:            pulumi.Input<{ [k: string]: string }>
  stdin?:                  pulumi.Input<string>
  addPreviousOutputInEnv?: pulumi.Input<boolean>
  logging?:                pulumi.Input<enums.remote.Logging>
}
// outputs: stdout/stderr: Output<string>, connection: Output<remote.Connection>, plus echoed create/update/delete/environment/stdin/triggers/logging
```

[COPY]: file transfer over the same `Connection`. `CopyToRemote` (Asset/Archive source) is the current form; `CopyFile` (local path source) is `@deprecated`:

```ts
interface remote.CopyToRemoteArgs { connection: Input<ConnectionArgs>; source: Input<asset.Asset | asset.Archive>; remotePath: Input<string>; triggers?: Input<any[]> }
interface remote.CopyFileArgs     { connection: Input<ConnectionArgs>; localPath: Input<string>;                    remotePath: Input<string>; triggers?: Input<any[]> }  // @deprecated
```

[CONNECTION]: `types.input.remote.ConnectionArgs` — the full SSH surface (the current design pins only `host`/`user`/`privateKey`, but the shape carries proxy-jump, agent-forwarding, host-key pinning, and dial-retry tuning). `ProxyConnectionArgs` is the same shape minus `proxy` (no nested jump). Wrap `password`/`privateKey`/`privateKeyPassword` with `pulumi.secret()`:

```ts
interface remote.ConnectionArgs {
  host:                Input<string>                       // required
  port?:               Input<number>                       // default 22
  user?:               Input<string>                       // default "root"
  password?:           Input<string>
  privateKey?:         Input<string>                        // PEM key material
  privateKeyPassword?: Input<string>
  agentSocketPath?:    Input<string>                        // SSH agent forwarding
  hostKey?:            Input<string>                        // pinned host key for verification
  dialErrorLimit?:     Input<number>                        // default 10 dial attempts
  perDialTimeout?:     Input<number>                        // seconds; default 15
  proxy?:              Input<remote.ProxyConnectionArgs>    // jump host (no nested proxy)
}
```

[LOGGING]: `enums.local.Logging` / `enums.remote.Logging` — the const-enum vocabulary controlling deploy-log capture: `Stdout` `"stdout"`, `Stderr` `"stderr"`, `StdoutAndStderr` `"stdoutAndStderr"`, `None` `"none"`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `local.Command`/`remote.Command`/`CopyToRemote`/`CopyFile` are `pulumi.CustomResource` instances tracked in stack state; they re-run when `triggers` values change or an input changes. `local.run`/`runOutput` are invokes — they fire every `pulumi up` AND `pulumi preview`, so a side-effecting step MUST be a `Command`, never a `run`.
- When `update` is absent, `create` runs on both create and update operations. `delete` runs on destroy.
- `addPreviousOutputInEnv` (default `true`) injects `PULUMI_COMMAND_STDOUT`/`PULUMI_COMMAND_STDERR` from the prior step into the next `update`/`delete` process env, so a delete can read what create produced.
- `remote.Command` requires `connection.host` plus one of `privateKey`/`password`; `agentSocketPath` and `proxy` cover agent-forwarding and jump-host topologies without a second resource.

[DEPLOY_STACK]: how the `provisioning/contract#PROVISIONING` tier stacks this onto the `@pulumi/pulumi` (`pulumi-pulumi.md`) core and the Effect rails.
- Ownership: a `Command` is instantiated as a child of a `TierStack` `ComponentResource` (`{ parent: this }` in `ComponentResourceOptions`) so it joins the component's URN tree and `registerOutputs` surface; the bootstrap sequence orders steps with `ResourceOptions.dependsOn` rather than shell chaining.
- Change propagation: `triggers` accepts `Input<any[]>` — feed an upstream `Output` (e.g. an `Image.repoDigest` from `pulumi-docker.md`, a `random.RandomPassword.result`) so the command re-runs exactly when the dependency changes; `Output.apply`/`pulumi.all` build the trigger tuple, never a plain string compare.
- Effect boundary: the `AutomationDriver` drives these through the `@pulumi/pulumi/automation` `Stack` inside `Effect.tryPromise`/`Effect.async` (the drift/deploy fold in `pulumi-pulumi.md`); `local.runOutput` composed under `pulumi.output(...).apply` is the read-time path when a value must flow into another resource arg. Secrets in `environment`/`Connection` arrive as `Config.requireSecret`/`pulumi.secret()` `Output`s so they stay redacted in state.

[SIBLING_STACK]:
- `@pulumi/pulumi` core owns the `Output`/`Input` algebra every arg accepts, the `CustomResourceOptions` (`parent`/`dependsOn`/`provider`/`protect`/`customTimeouts`) these constructors take, and the Automation API the deploy driver folds.
- `@pulumi/random` (`pulumi-random.md`) is the standard `triggers`/secret companion — `RandomPassword`/`RandomId` outputs seed a `Command`'s env or trigger tuple.
- `@pulumi/docker` (`pulumi-docker.md`) self-hosted tier and `@pulumi/kubernetes` (`pulumi-kubernetes.md`) cloud tier consume `Command` for the imperative provisioning steps their declarative resources cannot express (ACME cert bootstrap, one-shot migrations); `@effect/cli` binds the enclosing lifecycle verbs and `effect` owns the `Match.exhaustive` `DeployMode` dispatch selecting host-vs-SSH command placement.

[RAIL_LAW]:
- Package: `@pulumi/command`
- Owns: shell command execution and file copy inside the Pulumi lifecycle, on the deploy host (`local`) and over SSH (`remote`).
- Accept: `local.Command`/`remote.Command` for change-triggered side effects with `triggers` wired to upstream `Output`s; `local.runOutput` for unconditional read-time values inside `apply`; the full `Connection` shape (`proxy`/`agentSocketPath`/`hostKey`) for real SSH topologies.
- Reject: `local.run` when conditional-on-change execution is the intent (it fires every preview); a bare `string` compare where a `triggers` `Output` tuple belongs; plaintext `password`/`privateKey` not wrapped in `pulumi.secret()`.
