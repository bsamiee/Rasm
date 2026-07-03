# [TS_IAC_API_PULUMI_COMMAND]

`@pulumi/command` is the imperative escape-hatch provider that runs shell commands as first-class graph resources — the sole owner of the `selfhosted-k8s` cluster-bootstrap row over owned metal/VPS. Two arms share ONE lifecycle keyword schema: `local.Command` runs on the deploy host, `remote.Command` runs over an SSH `ConnectionArgs` (host/user/privateKey, optional bastion `proxy`), and both discriminate work across the `create`/`update`/`delete` CRUD slots with a `triggers` re-run discriminator, captured `stdout`/`stderr` outputs, and a `logging` secrecy control. `remote.CopyToRemote` stages an `Asset`/`Archive` to a remote path; `local.run`/`runOutput` are the unconditional-invoke `Promise`/`Output` mirror pair for read-only shell facts. Every command is a `pulumi.CustomResource` woven into other resources' lifecycles through the universal `opts` seam (`dependsOn`/`parent`), so bootstrap ordering is graph-derived, never scripted.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/command`
- package: `@pulumi/command`
- version: `1.2.1`
- license: Apache-2.0
- import: `@pulumi/command` → `{ local, remote, types, Provider }`
- owner: `iac`
- rail: fabric / cluster-bootstrap
- runtime: Node deploy-host + the `pulumi` CLI-on-PATH (the `program/automation` wrap); remote arm needs SSH reachability to the target
- build-floor: `@pulumi/pulumi` `^3.142.0` (catalog pins `3.250.0`)
- depends-on: `@pulumi/pulumi` (the `CustomResource`/`Input`/`Output`/`asset` model; the Automation-API run drives the CRUD steps)
- namespaces: `command.local` (host `Command` + `run`/`runOutput`), `command.remote` (SSH `Command` + `CopyToRemote` + deprecated `CopyFile`), `command.types.input.remote` (`ConnectionArgs`/`ProxyConnectionArgs`), `command.Provider` (empty-arg provider marker; transport rides each `connection`, so no provider-level credential)
- capability: shell commands as CRUD-lifecycle resources (host + SSH-remote), asset/archive staging to remote paths, unconditional shell invokes with an `Output` mirror, trigger-driven re-runs, captured stdout/stderr with per-stream secrecy
- abi-note: every `Command`/`CopyToRemote` output prop mirrors its `Args` field resolved through `Output<T>`; `stdout`/`stderr` are always-present `Output<string>`, all other props are `Output<T | undefined>`

## [02]-[COMMAND_LIFECYCLE]

[LIFECYCLE_SCOPE]: the one polymorphic command surface
- rail: cluster-bootstrap
- `local.Command` and `remote.Command` are the SAME lifecycle shape; local-vs-remote is the transport axis and the remote arm adds exactly one required field (`connection`). The CRUD slots are independent shell strings, so one resource owns create/update/delete without three resource kinds. `triggers` (any-typed values — a `random.RandomId.hex`, a `tls` fingerprint, a file `Asset`) forces re-run: with `update` present the resource updates, else it replaces via `create`. `logging` gates whether captured output is echoed to the CLI (mark secret outputs and set `none` for credential-bearing steps); capture into `stdout`/`stderr` is unconditional.

| [INDEX] | [SYMBOL] | [SLOT] | [SHAPE / BOUNDARY] |
| :-----: | :------- | :----- | :----------------- |
|  [01]   | `local.Command` / `remote.Command` | class | `new Command(name, CommandArgs, opts?)` → `CustomResource`; `static get`, `static isInstance` |
|  [02]   | `CommandArgs.create` / `.update` / `.delete` | CRUD | `Input<string>`; `delete` env carries `PULUMI_COMMAND_STDOUT`/`STDERR` from the prior create/update |
|  [03]   | `CommandArgs.triggers` | discriminator | `Input<any[]>`; value change → update (or replace when no `update`) |
|  [04]   | `CommandArgs.environment` / `.stdin` | input | `Input<{[k]: Input<string>}>` / `Input<string>` |
|  [05]   | `CommandArgs.logging` | secrecy | `Input<Logging>` = `"stdout" \| "stderr" \| "stdoutAndStderr" \| "none"` |
|  [06]   | `CommandArgs.addPreviousOutputInEnv` | chaining | `Input<boolean>` (default `true`); injects prior run stdout/stderr as env for the next |
|  [07]   | `local` extras: `interpreter` / `dir` / `assetPaths` / `archivePaths` | host | `Input<string[]>` shell (default `["/bin/sh","-c"]` / `["cmd","/C"]`), cwd, glob capture → `assets`/`archive` outputs |
|  [08]   | `remote.CommandArgs.connection` | transport | `Input<ConnectionArgs>` (required) |
|  [09]   | `Command.stdout` / `.stderr` | output | `Output<string>` (always present); the bootstrap result threads downstream via `.apply`/`pulumi.all` |

[CONNECTION_SCOPE]: `types.input.remote.ConnectionArgs` — the VPS/metal SSH seam
- rail: cluster-bootstrap
- The remote transport target. `host` is the only required field; `proxy` nests a full `ProxyConnectionArgs` for bastion/jump-host reach. Every credential field is `Input<string>` so it binds a `tls`/`doppler`/`random` `Output`, never a literal.

| [INDEX] | [FIELD] | [TYPE] | [MEANING] |
| :-----: | :------ | :----- | :-------- |
|  [01]   | `host` | `Input<string>` | target address (required) |
|  [02]   | `port` / `user` | `Input<number>` / `Input<string>` | SSH port (default 22), login (default `root`) |
|  [03]   | `privateKey` / `privateKeyPassword` | `Input<string>` | PEM key + optional passphrase; bind `tls.PrivateKey.privateKeyPem` |
|  [04]   | `password` | `Input<string>` | password auth alternative |
|  [05]   | `agentSocketPath` / `hostKey` | `Input<string>` | SSH-agent socket, pinned host public key |
|  [06]   | `perDialTimeout` / `dialErrorLimit` | `Input<number>` | per-dial timeout (s), retry ceiling before failing the step |
|  [07]   | `proxy` | `Input<ProxyConnectionArgs>` | bastion hop (same field set, its own `host`) |

[COPY_SCOPE]: remote asset staging
- rail: cluster-bootstrap
- Stage local files onto the target before/after a `remote.Command`; `dependsOn` threads ordering. `source` is any `pulumi.asset.Asset | Archive` (a rendered config, a `FileArchive` of manifests), so staged content is an `Output`-derived value, not a checked-in path.

| [INDEX] | [SYMBOL] | [SHAPE / BOUNDARY] |
| :-----: | :------- | :----------------- |
|  [01]   | `remote.CopyToRemote` | `new CopyToRemote(name, { connection, source: Asset\|Archive, remotePath, triggers? }, opts?)` → outputs `connection`/`source`/`remotePath` |
|  [02]   | `remote.CopyFile` | DEPRECATED single-file copy — superseded by `CopyToRemote`; never author it |

## [03]-[UNCONDITIONAL_INVOKE]

[INVOKE_SCOPE]: `local.run` / `local.runOutput` — the Promise/Output mirror pair
- rail: fabric
- Unconditional shell reads that run on EVERY preview/up (no CRUD lifecycle, no state) — the read-side complement to the lifecycle `Command`. `run` returns an eager `Promise` for use inside an `async` inline program; `runOutput` returns an `Output<RunResult>` that threads a shell fact through the resource graph (feed `dependsOn`, `triggers`, or a downstream `Input`). `RunArgs.command` is required; the rest mirrors the local `Command` input slots.

| [INDEX] | [SURFACE] | [SHAPE / BOUNDARY] |
| :-----: | :-------- | :----------------- |
|  [01]   | `local.run(RunArgs, InvokeOptions?)` | `Promise<RunResult>`; eager read, `command` required + `dir`/`environment`/`interpreter`/`logging`/`stdin`/`asset*` |
|  [02]   | `local.runOutput(RunOutputArgs, InvokeOutputOptions?)` | `Output<RunResult>`; graph-threaded read (Input-typed args) |
|  [03]   | `RunResult.stdout` / `.stderr` | `string`; `stdout`/`stderr` always present, plus resolved `assets`/`archive` |

## [04]-[IMPLEMENTATION_LAW]

[BOOTSTRAP_TOPOLOGY]:
- bootstrap law: the `selfhosted-k8s` cluster-bootstrap row is a `remote.Command` over `ConnectionArgs`; the CRUD `create` installs the control plane (k3s/kubeadm) and its `stdout` is the kubeconfig. Ordering is graph-derived through `opts.dependsOn`/`parent`, never a hand-sequenced script.
- takeover law: the bootstrap `stdout` kubeconfig threads via `.apply`/`pulumi.interpolate` into a `@pulumi/kubernetes` `Provider` (`.api/pulumi-kubernetes.md`); `command` provisions the metal, `@pulumi/kubernetes` owns every workload thereafter — no shell command re-implements a typed resource the k8s provider already owns.
- credential law: `ConnectionArgs.privateKey`/`password`/`hostKey` bind Outputs from `@pulumi/tls` (`PrivateKey.privateKeyPem`, `.api/pulumi-tls.md`) and `@pulumiverse/doppler` secret refs (`.api/pulumiverse-doppler.md`); a literal key or inline password is rejected. `logging: "none"` on any credential-echoing step.
- re-run law: `triggers` carries the values whose change must re-bootstrap — a `@pulumi/random` `RandomId.hex` (`.api/pulumi-random.md`), a manifest `Asset`, or an upstream resource fingerprint; the trigger set is the replacement discriminator, never a manual `--replace`.
- invoke law: an unconditional shell read is `local.runOutput` when its result threads the graph (`Output`-consumed), `local.run` when an eager `Promise` fact is needed in an `async` program body; a lifecycle side effect is `Command`, never `run`.

[LOCAL_ADMISSION]:
- The `provider/surface` cluster-bootstrap row and the `provider/dispatch` `selfhosted-k8s` arm compose `remote.Command` + `CopyToRemote`; the `effect` `Match.exhaustive` arm (`libs/typescript/.api/effect.md`) constructs the connection from a `Schema`-decoded `StackSpec` (host/user/key ref) and folds a non-zero exit (a failed CRUD step) into the `program/automation` typed run receipt (`@pulumi/pulumi` `automation.UpResult`, `.api/pulumi-pulumi.md`).
- `remote.CopyToRemote.source` is a `pulumi.asset.FileArchive`/`FileAsset` of rendered install artifacts; combine with `@pulumi/tls` cert material and `@pulumiverse/doppler` secrets so nothing crosses as a literal.
- canonical-spelling law: `CopyToRemote` over deprecated `CopyFile`; `runOutput` for graph-threaded reads, `run` for eager reads; one `Command` owning create/update/delete over three single-slot resources.
- boundary law: `command` is the escape hatch, admitted ONLY where no typed provider owns the concern (bare-metal control-plane install, one-shot host mutation). A shell command that duplicates a `@pulumi/kubernetes`, `@pulumi/docker`, or cloud-provider resource is rejected in favor of the typed resource.

[RAIL_LAW]:
- Package: `@pulumi/command`
- Owns: shell commands as CRUD-lifecycle graph resources (host + SSH-remote), remote asset staging, unconditional shell invokes with an `Output` mirror, the cluster-bootstrap row
- Accept: `remote.Command` bootstrap over `ConnectionArgs`, `CopyToRemote` staging, `local`/`remote.Command` host mutation, `local.run`/`runOutput` reads, `triggers`-driven re-runs, Output-bound credentials
- Reject: shell duplicating a typed provider resource, literal keys/passwords in `ConnectionArgs`, deprecated `CopyFile`, hand-sequenced ordering over `dependsOn`, credential echo without `logging: "none"`
