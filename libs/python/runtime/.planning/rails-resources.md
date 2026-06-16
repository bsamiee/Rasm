# [PY_RUNTIME_RAILS_RESOURCES]

Runtime rails and resources provide Python-local boundary discipline. They translate provider failures at the edge and keep raw provider handles from leaking into package interiors.

## [1]-[RAIL_OWNER]

[BOUNDARY_FAULT]:
- Owns: config, resource, deadline, API, import, and boundary failure cases.
- Entry: one exception-to-fault boundary conversion surface.
- Packages: `expression`, `msgspec`, `beartype`, `stamina`.
- Output: `Result`/`Option` rails, never nullable sentinel returns.
- Boundary: no C# `Expected` clone and no product receipt minting.

[RETRY]:
- Owns: local retry admission where Python talks to external resources.
- Entry: one policy row per retryable resource class.
- Packages: `stamina`.
- Output: receipt fields for attempt count and terminal outcome.
- Boundary: product outbound resilience, circuit breaking, and hop policy remain AppHost-owned.

## [2]-[RESOURCE_OWNER]

[RESOURCE_ROOT]:
- Owns: local file roots, object-store roots, scratch roots, and safe relative resolution.
- Entry: one root-admission surface and one child-resolution surface.
- Packages: `fsspec`, `s3fs`, `gcsfs`, `universal-pathlib`.
- Output: resource references with scheme, root, relative path, and owner metadata.
- Boundary: no default root creation, root litter, product store root derivation, or bridge staging root ownership.

[TRANSPORT_RESOURCE]:
- Owns: HTTP and SSH/SFTP resource adapters only where file or API acquisition is package-local.
- Entry: one transport request record per resource class.
- Packages: `httpx`, `asyncssh`.
- Output: resource receipts and response metadata.
- Boundary: no service API layer and no companion-control transport.

## [3]-[LANE_OWNER]

[DRAIN_LANE]:
- Owns: bounded AnyIO task groups, cancellation scopes, capacity policies, and drain receipts.
- Entry: one lane-policy record consumed by package-local asynchronous work.
- Packages: `anyio`, `watchfiles`, `aiocron`.
- Output: `DrainReceipt` records with accepted, completed, cancelled, and rejected counts.
- Boundary: no daemon scheduler, app lifecycle hook, or service health contribution.

## [4]-[RED_TEAM]

- Delete wrapper-renames over `anyio`, `fsspec`, `httpx`, or `asyncssh` unless they add policy, admission, or receipt projection.
- Reject custom retry loops when `stamina` can own the retry surface.
- Reject path parsing that bypasses `fsspec` and `universal-pathlib`.
- Reject unbounded task creation or background loops with no drain receipt.
