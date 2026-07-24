# [TS_DATA_API_WEBDAV]

`webdav` is the isomorphic WebDAV client reaching the origin class neither S3 nor SSH does: Nextcloud/ownCloud, SharePoint/OneDrive DAV endpoints, Apache/nginx `mod_dav` shares. One `createClient(remoteURL, options)` factory mints a `WebDAVClient` over the whole DAV verb set; auth is an `AuthType` config row, credential kind as data, never a fork.

It is a remote-origin row on the `object/file` plane — origin-addressed, capability-flag dispatched beside the SFTP and FTP rows, carrying server-side `copyFile`/`moveFile` and RFC 4918 locks those rows lack.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `webdav`
- package: `webdav` (MIT)
- module: ESM only
- runtime: isomorphic node + browser; `createReadStream`/`createWriteStream` are node-only and throw in the browser, where `getFileContents`/`putFileContents` carry the transfer
- rail: remote-origin row on `object/file`; the DAV protocol lane

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the client factory, auth rows, and result shapes

- `createClient(remoteURL, options)` mints the `WebDAVClient`; auth and transport field rosters ride the token lines below.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------------------ | :------------ | :--------------------------------------------------------------- |
|  [01]   | `createClient`                  | factory       | one client per origin, held as a scoped service                  |
|  [02]   | `WebDAVClientOptions` auth      | auth row      | `AuthType.Auto \| Password \| Digest \| Token \| None` as data   |
|  [03]   | `WebDAVClientOptions` transport | transport     | agent pooling, header policy, LOCK contact, XML-expansion bounds |
|  [04]   | `FileStat`                      | stat fact     | census/diff unit; `etag` is the poll-watch change key            |
|  [05]   | `ResponseDataDetailed<T>`       | receipt       | every read's `details: true` projection                          |
|  [06]   | `DiskQuota`                     | quota fact    | capacity row for transfer admission                              |
|  [07]   | `LockResponse`                  | lock fact     | the RFC 4918 token `unlock` requires                             |

[AUTH_ROW]: `authType: AuthType` `username` `password` `token: {access_token, token_type, refresh_token?}` `ha1`
[TRANSPORT_ROW]: `headers` `httpAgent` `httpsAgent` `withCredentials` `maxBodyLength` `maxContentLength` `remoteBasePath` `contactHref` `entityDecoder`
[FILE_STAT]: `filename` `basename` `lastmod` `size` `type: "file"|"directory"` `etag: string|null` `mime?` `props?`
[RESPONSE_DETAILED]: `data` `headers: Headers` `status` `statusText`
[DISK_QUOTA]: `used` `available: "unknown"|"unlimited"|number`
[LOCK_RESPONSE]: `serverTimeout` `token`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: census, transfer, namespace, and locks

- Every surface is a `WebDAVClient` instance member; the browser stubs the two stream members.

| [INDEX] | [SURFACE]                                                                                       | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `getDirectoryContents(path, { deep?, glob?, includeSelf?, details? })`                          | recursive PROPFIND census          |
|  [02]   | `stat(path, { details? })` / `exists(path)` / `getQuota({ details?, path? })`                   | point facts; `etag`/`size` diff    |
|  [03]   | `getFileContents(filename, { format: "binary" \| "text", details?, onDownloadProgress? })`      | browser-safe whole read            |
|  [04]   | `createReadStream(filename, { range?: { start, end? }, callback? })` → `Readable`               | node stream; `range.start` resumes |
|  [05]   | `putFileContents(filename, data, { overwrite?, contentLength?, onUploadProgress? })`            | write; `false` = 412 refused       |
|  [06]   | `createWriteStream(filename, { overwrite? }, callback?)` → `Writable`                           | node sink; 3rd-arg callback        |
|  [07]   | `partialUpdateFileContents(filePath, start, end, data)`                                         | byte patch; degrade to full put    |
|  [08]   | `copyFile(filename, destination, { shallow? })`                                                 | zero-transfer copy; SFTP lacks     |
|  [09]   | `moveFile(filename, destination, { overwrite? })`                                               | zero-transfer move; SFTP lacks     |
|  [10]   | `deleteFile(filename)` / `createDirectory(path, { recursive? })`                                | namespace maintenance              |
|  [11]   | `lock(path, { refreshToken?, timeout? })` → `LockResponse` / `unlock(path, token)`              | RFC 4918 exclusive-write lock      |
|  [12]   | `search(path, { details? })` / `customRequest(path, requestOptions)` / `getDAVCompliance(path)` | capability probe; escape-hatch     |
|  [13]   | `getHeaders()` / `setHeaders(headers)`                                                          | header-policy swap                 |
|  [14]   | `getFileDownloadLink(filename)` / `getFileUploadLink(filename)`                                 | presigned basic-auth links         |
|  [15]   | `registerTagParser(parser)` / `registerAttributeParser(parser)`                                 | custom DAV-prop parser             |

## [04]-[IMPLEMENTATION_LAW]

[STACKING]:
- `effect` (`.api/effect.md`): promise members lift through `Effect.tryPromise`; `createReadStream` through `NodeStream.fromReadable` and `createWriteStream` through `NodeSink.fromWritable` (`.api/effect-platform-node.md`), so transfers are backpressured `Stream`/`Sink`; a `putFileContents` `false` folds to the typed precondition-refused fact.
- `ssh2` / `basic-ftp` (`.api/ssh2.md`, `.api/basic-ftp.md`): sibling remote-origin rows on the one origin-addressed `object/file` surface; DAV carries server-side copy/move and locks but no exec, and a flag the origin lacks degrades by row (no server-side move ⇒ copy + delete).
- `chokidar` (`.api/chokidar.md`): the local half of the watch row; the DAV half polls `Schedule`-driven `getDirectoryContents`/`stat` snapshots diffed on `etag`/`size`/`lastmod`, since no DAV push lane exists.
- `object/file` intake: a DAV read stream feeds the same content-addressed intake fold as every origin — digest-while-streaming into the conditional put, never growing its own addressing vocabulary.

[LOCAL_ADMISSION]:
- Mint one scoped client per origin; credentials and `authType` are config rows, never literals.
- Select transfer lane by runtime and size: streams on node, `getFileContents`/`putFileContents` in the browser, `range`/`partialUpdateFileContents` for resume where the capability flag proves support.
- Read with `details: true` wherever a receipt (status, headers, etag) feeds evidence; a bare body read that later re-stats is the double-round-trip defect.
- Bound hostile XML through `entityDecoder` (`maxTotalExpansions`/`maxExpandedLength`) on untrusted origins; probe unknown servers with `getDAVCompliance` before relying on lock or search rows.

[RAIL_LAW]:
- Package: `webdav`
- Owns: the DAV protocol lane — client factory with auth/transport rows, directory census, whole/ranged/streamed transfer, server-side copy/move, quota, RFC 4918 locks, partial updates, capability probing
- Accept: scoped per-origin clients, `AuthType` rows over auth forks, `NodeStream`/`NodeSink` lifts on node, etag-diff poll watching, `details: true` receipts, capability-flag degrade paths
- Reject: browser calls into the stubbed stream members, credential literals, a second DAV wrapper, polling bodies where an etag diff suffices, treating a missing capability as an error instead of a degrade row
