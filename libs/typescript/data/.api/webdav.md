# [TS_DATA_API_WEBDAV]

`webdav` is the isomorphic TypeScript WebDAV client that opens the origin class neither S3 nor SSH reaches: Nextcloud/ownCloud, SharePoint/OneDrive over their DAV endpoints, and Apache/nginx `mod_dav` shares. One `createClient(remoteURL, options)` factory mints a `WebDAVClient` whose surface is the complete DAV verb set — directory census with `deep`/`glob`, whole-file and ranged reads, streamed writes, server-side `copyFile`/`moveFile` (a capability flag SFTP lacks), quota, `lock`/`unlock` RFC 4918 lock tokens, and byte-ranged `partialUpdateFileContents` where the server honors it. Auth is a config row, not a fork: `AuthType.Password`/`Digest`/`Token`/`None` with `Auto` detection — basic auth IS `AuthType.Password`, no `Basic` member exists. Every read can return receipts: `details: true` flips the return to `ResponseDataDetailed<T>` carrying `data`/`headers`/`status`/`statusText`, and `FileStat.etag` is the change-detection key the poll-watch row diffs on. It is a remote-origin backend row of the `object/file` plane — origin-addressed, capability-flag dispatched beside the SFTP and FTP rows.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `webdav`
- package: `webdav` (MIT, perry-mitchell/webdav-client)
- module format: ESM only; isomorphic node + browser — `createReadStream`/`createWriteStream` are node-only and THROW in the browser, where `getFileContents`/`putFileContents` carry the transfer
- rail: remote-origin row (`object/file`); the DAV protocol lane
- hostile-input guard: `entityDecoder` limits (`maxTotalExpansions`, `maxExpandedLength`) bound XML entity expansion on parsed multistatus bodies

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client factory, auth rows, and result shapes
- rail: boundaries
- `createClient(remoteURL, options)` mints the `WebDAVClient`; `options` carry the auth row (`authType: AuthType`, `username`/`password`, `token: { access_token, token_type, refresh_token? }`, `ha1`) and the transport row (`headers`, `httpAgent`/`httpsAgent`, `withCredentials`, `maxBodyLength`/`maxContentLength`, `remoteBasePath`, `contactHref`, `entityDecoder`).
- result shapes: `FileStat` (`filename`, `basename`, `lastmod`, `size`, `type: "file" \| "directory"`, `etag`, `mime?`, `props?`), `ResponseDataDetailed<T>` (`data`, `headers`, `status`, `statusText`), `DiskQuota` (`used`, `available: "unknown" \| "unlimited" \| number`), `LockResponse` (`serverTimeout`, `token`).

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CONSUMER]                                                                       |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `createClient` factory    | factory       | one client per origin row, held as a scoped service                              |
|  [02]   | `options` auth row        | auth row      | `AuthType.Auto \| Password \| Digest \| Token \| None` — credential kind is data |
|  [03]   | `options` transport row   | transport     | agent pooling, header policy, LOCK owner contact, XML-expansion bounds           |
|  [04]   | `FileStat`                | stat fact     | the census/diff unit; `etag` is the poll-watch change key                        |
|  [05]   | `ResponseDataDetailed<T>` | receipt       | every read's `details: true` projection                                          |
|  [06]   | `DiskQuota`               | quota fact    | capacity row for transfer admission                                              |
|  [07]   | `LockResponse`            | lock fact     | the RFC 4918 token `unlock` requires                                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: census, transfer, namespace, and locks
- rail: boundaries

| [INDEX] | [SURFACE]                                                                                                   | [ENTRY_FAMILY] |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :------------- |
|  [01]   | `getDirectoryContents(path, { deep?, glob?, includeSelf?, details? })`                                      | census         |
|  [02]   | `stat(path, { details? })` / `exists(path)` / `getQuota({ details?, path? })`                               | attrs          |
|  [03]   | `getFileContents(filename, { format: "binary" \| "text", details?, onDownloadProgress? })`                  | whole read     |
|  [04]   | `createReadStream(filename, { range?: { start, end? }, callback? })` → `Readable`                           | ranged read    |
|  [05]   | `putFileContents(filename, data, { overwrite?, contentLength?, onUploadProgress? }): Promise<boolean>`      | whole write    |
|  [06]   | `createWriteStream(filename, { overwrite? }, callback?)` → `Writable`                                       | streamed write |
|  [07]   | `partialUpdateFileContents(filePath, start, end, data)`                                                     | ranged write   |
|  [08]   | `copyFile(filename, destination, { shallow? })` / `moveFile(filename, destinationFilename, { overwrite? })` | server-side    |
|  [09]   | `deleteFile(filename)` / `createDirectory(path, { recursive? })`                                            | namespace      |
|  [10]   | `lock(path, { refreshToken?, timeout? })` → `LockResponse` / `unlock(path, token)`                          | lock           |
|  [11]   | `search(path, { details? })` / `customRequest(path, requestOptions)` / `getDAVCompliance(path)`             | probe          |
|  [12]   | `getHeaders()` / `setHeaders(headers)` / `getFileDownloadLink(filename)` / `getFileUploadLink(filename)`    | session        |

[CONSUMER_NOTES]:
- [01]-[CENSUS]: recursive walk in one PROPFIND; the poll-watch snapshot source.
- [02]-[ATTRS]: point facts; `etag`/`size`/`lastmod` diff inputs.
- [03]-[WHOLE_READ]: browser-safe read lane.
- [04]-[RANGED_READ]: node stream lane; `range.start` is the download-resume arm.
- [05]-[WHOLE_WRITE]: browser-safe write lane; `false` = 412 refused, a typed fact.
- [06]-[STREAMED_WRITE]: node sink lane; the completion callback is the third positional arg.
- [07]-[RANGED_WRITE]: byte-offset patch — a server capability flag, degrade to full put.
- [08]-[SERVER_SIDE]: zero-byte-transfer copy/move — the capability flag SFTP rows lack.
- [09]-[NAMESPACE]: tree maintenance.
- [10]-[LOCK]: exclusive-write coordination against concurrent DAV writers.
- [11]-[PROBE]: server capability probing and escape-hatch verbs.
- [12]-[SESSION]: header policy swap; presigned-style basic-auth links.

## [04]-[IMPLEMENTATION_LAW]

[STACKS_WITH]:
- `effect` (`libs/typescript/.api/effect.md`): promise members convert through `Effect.tryPromise` at the seam; `createReadStream` lifts through `NodeStream.fromReadable` and `createWriteStream` through `NodeSink.fromWritable` (`.api/effect-platform-node.md`), so transfers are backpressured `Stream`/`Sink` values; a `putFileContents` `false` folds to the typed precondition-refused fact.
- `ssh2` / `basic-ftp` (`.api/ssh2.md`, `.api/basic-ftp.md`): sibling remote-origin rows on the one origin-addressed `object/file` surface — capability flags dispatch: DAV carries server-side copy/move and locks but no exec; a flag the origin lacks degrades by row (no server-side move ⇒ copy + delete), never by fork.
- `chokidar` (`.api/chokidar.md`): the local half of the watch strategy row; the DAV half is the poll row — `Schedule`-driven `getDirectoryContents`/`stat` snapshots diffed on `etag`/`size`/`lastmod`, since no DAV push-notification lane exists.
- `object/file` intake: a DAV read stream feeds the same content-addressed intake fold as every origin — digest-while-streaming into the conditional put; the origin row never grows its own addressing vocabulary.

[LOCAL_ADMISSION]:
- Mint one scoped client per origin row; credentials and `authType` are config rows, never literals.
- Select transfer lane by runtime and size: streams on node, `getFileContents`/`putFileContents` in the browser, `range`/`partialUpdateFileContents` for resume where the server's capability flag proves support.
- Read with `details: true` wherever a receipt (status, headers, etag) feeds evidence; a bare body read that later re-stats is the named double-round-trip defect.
- Bound hostile XML through `entityDecoder` limits on untrusted origins; probe unknown servers with `getDAVCompliance` before relying on lock or search rows.

[RAIL_LAW]:
- Package: `webdav`
- Owns: the DAV protocol lane — client factory with auth/transport rows, directory census, whole/ranged/streamed transfer, server-side copy/move, quota, RFC 4918 locks, partial updates, capability probing
- Accept: scoped per-origin clients, `AuthType` rows over auth forks, `NodeStream`/`NodeSink` lifts on node, etag-diff poll watching, `details: true` receipts, capability-flag degrade paths
- Reject: browser calls into the stubbed stream members, credential literals, a second DAV wrapper over this surface, polling bodies where an etag diff suffices, treating a missing server capability as an error instead of a degrade row
