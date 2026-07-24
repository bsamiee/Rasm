# [TS_DATA_API_CHOKIDAR]

`chokidar` mints an `FSWatcher`, a typed `EventEmitter<FSWatcherEventMap>` folding a watched tree into one admission stream whose `all` listener carries every `(event, path, stats?)` row; selection is the `ignored` predicate algebra over literal paths, never globs, `awaitWriteFinish` settles half-written files before intake, and `close()` is the awaited release arm. It is the local watch row of `object/file` — SSH, DAV, and FTP origins own the remote halves.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `chokidar`
- package: `chokidar` (MIT)
- module: ESM only (`type: module`); named `watch` (factory) + `FSWatcher` (class), default export bags both
- runtime: node only; one dependency `readdirp`
- rail: local watch row (`object/file`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the watcher, its typed event map, and the `ignored` matcher algebra where a string is an exact path, never a glob

| [INDEX] | [SYMBOL]                                             | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :--------------------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `FSWatcher`                                          | class         | scoped resource; `add`/`unwatch` return `this`           |
|  [02]   | `add` / `addDir` / `change` / `unlink` / `unlinkDir` | event rows    | `(path, stats?)`; `stats` rides under `alwaysStat`       |
|  [03]   | `all` — `(event, path, stats?)`                      | aggregate     | the one-listener fold over every event                   |
|  [04]   | `ready` / `error` / `raw`                            | lifecycle     | census barrier, fault row, opaque backend tap            |
|  [05]   | `ignored: Matcher \| Matcher[]`                      | union         | `string` / `RegExp` / predicate / `{ path, recursive? }` |
|  [06]   | `getWatched(): Record<string, string[]>`             | census        | the dir → entries snapshot for reconciliation            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the watch acquire/release bracket

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :------------------------------------ | :------- | :---------------------------------------------------------- |
|  [01]   | `watch(paths, options?) -> FSWatcher` | factory  | acquire arm; `paths` literal `string \| string[]`, no globs |
|  [02]   | `watcher.close() -> Promise<void>`    | instance | awaited release arm; removes every listener                 |

[OPTION_LEVERS]: intake-correctness knobs on `ChokidarOptions`, each carrying its default

| [INDEX] | [OPTION]                                     | [CAPABILITY]                                                                      |
| :-----: | :------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `ignoreInitial`                              | default `false` replays the tree as `add` before `ready`                          |
|  [02]   | `awaitWriteFinish`                           | settles `add`/`change` writes — `{ stabilityThreshold: 2000, pollInterval: 100 }` |
|  [03]   | `atomic`                                     | off-polling delay `100`; absorbs editor rename-swap artifacts                     |
|  [04]   | `usePolling` / `interval` / `binaryInterval` | `100` / `300` network-FS degrade row where native events lie                      |
|  [05]   | `depth` / `cwd`                              | recursion bound; cwd-relative path emission                                       |
|  [06]   | `followSymlinks` / `alwaysStat`              | link-follow (default `true`); force `stats` on every event                        |
|  [07]   | `ignorePermissionErrors` / `persistent`      | skip EACCES entries; keep the process alive (default `true`)                      |

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `effect` (`libs/typescript/.api/effect.md`): the watcher acquires under `Effect.acquireRelease(watch(...), (w) => Effect.promise(() => w.close()))`; the `all` listener lifts through `Stream.asyncPush` into one typed admission stream, `error` joins the failure channel, and `ready` resolves the initial-census barrier.
- `object/file` intake: an `add`/`change` row post-settle feeds the content-addressed fold — stream, digest, conditional put; the watcher supplies admission events, never bytes.
- `ssh2` / `webdav` / `basic-ftp` (`.api/ssh2.md`, `.api/webdav.md`, `.api/basic-ftp.md`): the remote halves of the watch strategy — exec-push where the host carries a notify tool, `Schedule`-driven poll diffs elsewhere; the strategy row dispatches on origin kind and this package is the `file:` arm.
- `@effect/platform` `FileSystem` (`libs/typescript/.api/effect-platform.md`): stat and read follow-ups on an admission event ride the platform capability, not `node:fs` beside the watcher.

[LOCAL_ADMISSION]:
- Await `close()` inside the release arm; an unawaited close leaks native handles across the scope boundary.
- Gate intake on `awaitWriteFinish` for any directory receiving whole-file writes; digesting an unsettled file is the named defect.
- Express selection as `ignored` predicate rows; a glob string matches nothing but its literal self.
- Configure `usePolling` per-origin for network mounts and containers, never as a global default.

[RAIL_LAW]:
- Package: `chokidar`
- Owns: local filesystem watching — the `FSWatcher` lifecycle, the typed event map, the matcher algebra, write-settle and atomic guards, polling degrade, initial-census replay
- Accept: scoped `watch`/awaited-`close` brackets, one `all`-listener lift into a typed admission stream, `ignoreInitial: false` + `ready` as the census barrier, predicate `ignored` rows, per-origin polling config
- Reject: glob strings in paths or matchers, unawaited `close()`, raw multi-listener consumption where the `all` fold suffices, intake without a settle guard, a remote origin forced through this local row
