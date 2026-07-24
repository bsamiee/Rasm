# [TS_DATA_API_BASIC_FTP]

`basic-ftp` owns the FTP/FTPS protocol lane as a remote-origin row on the `object/file` plane: one `Client` folds connect, TLS, login, and defaults into a single `access` dial, transfers a Node stream or a local path either direction, and resumes by byte offset.

One transfer rides the control connection at a time, so concurrency is a client pool; a protocol refusal keeps the connection usable while a transport loss re-dials on the next `access`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `basic-ftp`
- package: `basic-ftp` (MIT)
- module: CJS + ESM dual
- runtime: node only (`net`/`tls` sockets), zero dependencies
- rail: remote-origin row on `object/file`; the FTP/FTPS protocol lane

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client, dial options, and census facts

- `new Client(timeout?, { allowSeparateTransferHost?, maxListingBytes? })` mints one control connection; field rosters ride the token lines below.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------ | :------------ | :--------------------------------------- |
|  [01]   | `Client`                                                | client        | one control connection per instance      |
|  [02]   | `AccessOptions`                                         | dial row      | the whole dial in one options row        |
|  [03]   | `FileInfo` / `FileType`                                 | census fact   | per-entry type, size, mtime, permissions |
|  [04]   | `FTPResponse` (`code`, `message`) / `FTPError` (`code`) | receipt/fault | server disposition; refusal stays usable |
|  [05]   | `ProgressInfo`                                          | progress fact | the `trackProgress` stream unit          |
|  [06]   | `client.ftp` (`FTPContext`)                             | context       | log capture and wire tuning              |
|  [07]   | `client.parseList` (`RawListParser` override)           | parser slot   | exotic listing formats as policy         |

[FILE_TYPE]: `FileType = Unknown | File | Directory | SymbolicLink`
[ACCESS_OPTIONS]: `host` `port` `user` `password` `secure: boolean|"implicit"` `secureOptions` (`tls.connect` options)
[FILE_INFO]: `name` `type: FileType` `size` `rawModifiedAt` `modifiedAt` `permissions` `hardLinkCount` `link` `group` `user` `uniqueID` `isFile` `isDirectory` `isSymbolicLink`
[PROGRESS_INFO]: `name` `type: "upload"|"download"|"list"` `bytes` `bytesOverall`
[FTPCONTEXT]: `verbose` `encoding` `tlsOptions` `ipFamily` `hasTLS` `log(message)`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: dial, transfer, resume, and census

- Every entry is a `client` instance member; `access` folds connect, TLS, login, and defaults into one dial, and each transfer takes a Node stream or a local path either side.

| [INDEX] | [SURFACE]                                                               | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `access(options): Promise<FTPResponse>`                                 | the composed dial                              |
|  [02]   | `uploadFrom(source, toRemotePath, options?)`                            | upload; `localStart`/`localEndInclusive` slice |
|  [03]   | `appendFrom(source, toRemotePath, options?)`                            | append to continue an interrupted upload       |
|  [04]   | `downloadTo(destination, fromRemotePath, startAt?)`                     | download; `startAt` byte-offset resume         |
|  [05]   | `list(path?)` / `size(path)` / `lastMod(path)`                          | listing walk; size and mtime poll              |
|  [06]   | `cd` / `cdup` / `pwd` / `ensureDir` / `clearWorkingDir`                 | working-directory discipline                   |
|  [07]   | `uploadFromDir` / `downloadToDir` `(localDirPath, remoteDirPath?)`      | recursive directory mirror                     |
|  [08]   | `rename(srcPath, destPath)` / `remove` / `removeDir` / `removeEmptyDir` | staged rename and removal                      |
|  [09]   | `trackProgress(handler?)`                                               | one handler across all transfers               |
|  [10]   | `send(command)` / `features()`                                          | capability probe and escape-hatch verbs        |
|  [11]   | `close()` / `closed`                                                    | scoped release arm                             |

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `effect` (`.api/effect.md`): each promise member lifts through `Effect.tryPromise`; the client acquires under `Effect.acquireRelease` with `close()` as release; `trackProgress` becomes a fact `Stream` via `Stream.asyncPush`; an `FTPError` folds to a typed refusal keeping the row alive, while a closed-context fault rides the re-`access` under a `Schedule`.
- `@effect/platform-node` (`.api/effect-platform-node.md`): `NodeStream.toReadable`/`NodeSink.fromWritable` bridge an Effect `Stream` body into `uploadFrom` and drain `downloadTo` into a `Sink`; the path-string arms serve whole-file moves the platform `FileSystem` already staged.
- `ssh2` / `webdav` (`.api/ssh2.md`, `.api/webdav.md`): sibling remote-origin rows on the one origin-addressed `object/file` surface; capability flags dispatch, so a missing server-side copy or lock degrades to download-then-upload on the origin that owns coordination.
- `chokidar` (`.api/chokidar.md`): the local half of the watch row; the FTP half polls, diffing `Schedule`-driven `list`/`size`/`lastMod` snapshots on size and mtime.

[LOCAL_ADMISSION]:
- Dial through `access`; the split `connect`/`useTLS`/`login` members serve capability probes, never standing acquisition.
- One transfer per client; concurrency is a bounded pool of scoped clients keyed by origin.
- `ClientOptions.allowSeparateTransferHost` stays `false` — a data connection to a non-control host is refused unless ruled.
- Resume by arithmetic: `size` the remote, then `downloadTo(..., startAt)` or `appendFrom` with `localStart`.
- Stage an upload to a temp name and `rename` into place.

[RAIL_LAW]:
- Package: `basic-ftp`
- Owns: the FTP/FTPS protocol lane — the composed dial with explicit and implicit TLS, streamed and path transfer, byte-offset resume, directory mirroring, listing census with parser override, progress tracking, the usable-after-refusal error split
- Accept: scoped pooled clients dialed through `access`, `NodeStream`/`NodeSink` body seams, offset-resume arithmetic, rename-into-place staging, poll-diff watching
- Reject: interleaved transfers on one client, credential literals, full re-transfers where resume applies, `secure: false` against TLS-capable origins, a second FTP wrapper over this surface
