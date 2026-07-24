# [TS_IAC_API_PULUMI_COMMAND]

`@pulumi/command` runs shell commands as first-class graph resources, owning the `selfhosted-k8s` cluster-bootstrap row over owned metal and VPS.

`local.Command` and `remote.Command` share one CRUD-slot lifecycle; the remote arm dials an SSH `ConnectionArgs`, `CopyToRemote` stages assets to a remote path, and `local.run`/`runOutput` mirror read-only shell facts as `Promise`/`Output` — every command woven into sibling lifecycles through `opts`, so ordering is graph-derived.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/command`
- package: `@pulumi/command` (Apache-2.0)
- module: `@pulumi/command` → `{ local, remote, types, Provider }`
- runtime: Node deploy-host + `pulumi` CLI on PATH; the remote arm needs SSH reachability to the target
- rail: fabric / cluster-bootstrap
- namespaces: `local` (host `Command`, `run`/`runOutput`), `remote` (SSH `Command`, `CopyToRemote`), `types.input.remote` (`ConnectionArgs`/`ProxyConnectionArgs`), `Provider` (empty-arg marker; transport rides each `connection`)
- depends: `@pulumi/pulumi` `CustomResource`/`Input`/`Output`/`asset` model; the Automation-API run drives the CRUD steps
- abi: every output prop mirrors its `Args` field through `Output<T>`; `stdout`/`stderr` are always-present `Output<string>`, all other props `Output<T | undefined>`

## [02]-[COMMAND_LIFECYCLE]

[LIFECYCLE_SCOPE]: the one polymorphic command surface

`local.Command` and `remote.Command` are one lifecycle shape — the remote arm adds only the required `connection`, so transport is an axis, not a second resource kind. Independent shell strings fill the CRUD slots, so one resource owns create/update/delete. A `triggers` change updates when `update` is set, else replaces via `create`; `logging` gates the CLI echo while `stdout`/`stderr` capture unconditionally.

| [INDEX] | [SYMBOL]                                      | [SLOT]        | [SHAPE_BOUNDARY]                                                         |
| :-----: | :-------------------------------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `local.Command` / `remote.Command`            | class         | `new Command(name, CommandArgs, opts?)` → `CustomResource`               |
|  [02]   | `CommandArgs.create` / `.update` / `.delete`  | CRUD          | `Input<string>`; `delete` env carries `PULUMI_COMMAND_STDOUT`/`STDERR`   |
|  [03]   | `CommandArgs.triggers`                        | discriminator | `Input<any[]>`; value change → update (or replace when no `update`)      |
|  [04]   | `CommandArgs.environment` / `.stdin`          | input         | `Input<{[k]: Input<string>}>` / `Input<string>`                          |
|  [05]   | `CommandArgs.logging`                         | secrecy       | `Input<Logging>` = `"stdout" \| "stderr" \| "stdoutAndStderr" \| "none"` |
|  [06]   | `CommandArgs.addPreviousOutputInEnv`          | chaining      | `Input<boolean>` (default `true`); injects prior stdout/stderr as env    |
|  [07]   | `local` `interpreter`                         | host          | `Input<string[]>` shell, default `["/bin/sh","-c"]` / `["cmd","/C"]`     |
|  [08]   | `local` `dir` / `assetPaths` / `archivePaths` | host          | cwd for the process; glob capture → `assets` / `archive` outputs         |
|  [09]   | `remote.CommandArgs.connection`               | transport     | `Input<ConnectionArgs>` (required)                                       |
|  [10]   | `Command.stdout` / `.stderr`                  | output        | `Output<string>` always present; threads via `.apply`/`pulumi.all`       |

[CONNECTION_SCOPE]: `types.input.remote.ConnectionArgs` — the VPS/metal SSH seam

`host` is the only required field; `proxy` nests a full `ProxyConnectionArgs` for a bastion/jump-host hop. Every credential field is `Input<string>`, so it binds a `tls`/`doppler`/`random` `Output`, never a literal.

| [INDEX] | [FIELD]                             | [TYPE]                            | [MEANING]                                                   |
| :-----: | :---------------------------------- | :-------------------------------- | :---------------------------------------------------------- |
|  [01]   | `host`                              | `Input<string>`                   | target address (required)                                   |
|  [02]   | `port` / `user`                     | `Input<number>` / `Input<string>` | SSH port (default 22), login (default `root`)               |
|  [03]   | `privateKey` / `privateKeyPassword` | `Input<string>`                   | PEM key + passphrase; bind `tls.PrivateKey.privateKeyPem`   |
|  [04]   | `password`                          | `Input<string>`                   | password auth alternative                                   |
|  [05]   | `agentSocketPath` / `hostKey`       | `Input<string>`                   | SSH-agent socket, pinned host public key                    |
|  [06]   | `perDialTimeout` / `dialErrorLimit` | `Input<number>`                   | per-dial timeout (s), retry ceiling before failing the step |
|  [07]   | `proxy`                             | `Input<ProxyConnectionArgs>`      | bastion hop (same field set, its own `host`)                |

[COPY_SCOPE]: `remote.CopyToRemote` — remote asset staging

`new CopyToRemote(name, { connection, source: Asset|Archive, remotePath, triggers? }, opts?)` stages a `pulumi.asset.Asset | Archive` onto the target — a rendered config, a `FileArchive` of manifests — and `opts.dependsOn` threads it before or after a `remote.Command`. Outputs mirror the args: `connection`/`source`/`remotePath`.

## [03]-[UNCONDITIONAL_INVOKE]

[INVOKE_SCOPE]: `local.run` / `local.runOutput` — the Promise/Output mirror pair

Unconditional shell reads run on every preview/up with no CRUD lifecycle or state — the read-side complement to `Command`. `run` returns an eager `Promise` for an `async` inline program; `runOutput` returns an `Output<RunResult>` threading a shell fact through the graph (feed `dependsOn`, `triggers`, or a downstream `Input`). `RunArgs.command` is required; the rest mirrors the local `Command` inputs.

| [INDEX] | [SURFACE]                                              | [SHAPE_BOUNDARY]                                                              |
| :-----: | :----------------------------------------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `local.run(RunArgs, InvokeOptions?)`                   | `Promise<RunResult>`; eager read, `command` required (rest mirrors `Command`) |
|  [02]   | `local.runOutput(RunOutputArgs, InvokeOutputOptions?)` | `Output<RunResult>`; graph-threaded read (Input-typed args)                   |
|  [03]   | `RunResult.stdout` / `.stderr`                         | `string`; both always present, plus resolved `assets`/`archive`               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `remote.Command` over `ConnectionArgs` boots the `selfhosted-k8s` row: its `create` installs the control plane (k3s/kubeadm) and its `stdout` is the kubeconfig; ordering is graph-derived through `opts.dependsOn`/`parent`.
- An unconditional shell read is `local.runOutput` when its result threads the graph, `local.run` for an eager `Promise` in an `async` body; a lifecycle side effect is `Command`.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): the bootstrap `stdout` kubeconfig threads via `.apply`/`pulumi.interpolate` into `Provider.kubeconfig`; `command` provisions the metal and the k8s provider owns every workload thereafter.
- `@pulumi/tls`(`.api/pulumi-tls.md`): `PrivateKey.privateKeyPem` binds `ConnectionArgs.privateKey`.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): `Secret.value` and `getSecrets` map refs bind `ConnectionArgs.password`/`hostKey`; `logging: "none"` gates any credential-echoing step.
- `@pulumi/random`(`.api/pulumi-random.md`): `RandomId.hex` in `triggers` is the replacement discriminator whose change re-bootstraps.
- `effect`(`libs/typescript/.api/effect.md`): a `Match.exhaustive` arm builds the connection from a `Schema`-decoded `StackSpec` and folds a non-zero CRUD exit into the `@pulumi/pulumi` `automation.UpResult` receipt (`.api/pulumi-pulumi.md`).
- within-lib: `provider/surface` cluster-bootstrap and the `provider/dispatch` `selfhosted-k8s` arm compose `remote.Command` + `CopyToRemote`, staging a `FileArchive` of rendered install artifacts with `tls` cert material and `doppler` secrets.

[LOCAL_ADMISSION]:
- `command` is the escape hatch, admitted only where no typed provider owns the concern — bare-metal control-plane install, one-shot host mutation; a literal key or inline password in `ConnectionArgs` is rejected for an Output-bound ref. A shell command duplicating a `@pulumi/kubernetes`, `@pulumi/docker`, or cloud-provider resource is rejected for the typed resource.

[RAIL_LAW]:
- Package: `@pulumi/command`
- Owns: shell commands as CRUD-lifecycle graph resources (host + SSH-remote), remote asset staging, unconditional shell invokes with an `Output` mirror, the cluster-bootstrap row
- Accept: `remote.Command` bootstrap over `ConnectionArgs`, `CopyToRemote` staging, `local`/`remote.Command` host mutation, `local.run`/`runOutput` reads, `triggers`-driven re-runs, Output-bound credentials
- Reject: shell duplicating a typed provider resource, literal keys/passwords in `ConnectionArgs`, hand-sequenced ordering over `dependsOn`, credential echo without `logging: "none"`
