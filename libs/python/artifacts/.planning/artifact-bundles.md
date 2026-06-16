# [PY_ARTIFACTS_BUNDLES]

Artifact bundle ownership gives every generated file set content identity, manifest structure, and caller-owned placement. The package does not invent storage roots or product object identity.

## [1]-[DIGEST_OWNER]

[CONTENT_DIGEST]:
- Owns: algorithm key, digest value, byte count, media type, and source route.
- Output: content identity record used by document, visual, preview, and archive receipts.
- Boundary: product object identity and retention policy stay outside this package.

[FILE_SET]:
- Owns: bundle name, member files, media types, digests, relation labels, and provenance rows.
- Output: `ArtifactBundle` manifest record.
- Boundary: no root scratch output; caller supplies placement through runtime resource roots.

## [2]-[BUNDLE_OWNER]

[ARTIFACT_MANIFEST]:
- Owns: deterministic file list, primary artifact, sidecar files, preview files, source specs, and receipt refs.
- Output: manifest document suitable for AppUi, TypeScript, C#, Assay, or support tools to consume.
- Boundary: no UI state and no product artifact timeline.

[COMPRESSION_ROUTE]:
- Owns: compression algorithm, archive format, member map, chunk policy, and output digest.
- API routes: `.api/api-zstandard.md`, `.api/api-lz4.md`, `.api/api-brotli.md`, `.api/api-py7zr.md`.
- Output: compression receipt.
- Boundary: package staging, release archives, and support capture orchestration remain caller-owned.

## [3]-[RED_TEAM]

- Reject bundle output without content digests.
- Reject hidden temp paths and root scratch files.
- Reject compression code that bypasses package APIs.
- Reject artifact manifests that embed UI state.
