# [TS_IAC_API_PULUMI_SYNCED_FOLDER]

`@pulumi/synced-folder` converges a local folder into an existing bucket as ONE component pattern applied across three storage dialects: `S3BucketFolder`, `GoogleCloudFolder`, and `AzureBlobFolder` are the same `ComponentResource` shape — required `path` plus the dialect's bucket coordinates, with `managedObjects`/`includeHiddenFiles`/`disableManagedObjectAliases` as the shared sync-policy axis. The component owns content convergence only; the bucket itself stays the object cell's product (`aws.s3.BucketV2`, `gcp.storage.Bucket`), so in `iac` this is the static-distribution leg riding the prepared arms — the built frontend folder lands in the arm's object cell and the arm's DNS/CDN rows front it. The package carries no in-process cloud SDK: each class registers a `synced-folder:index:<Dialect>` token the `pulumi-resource-synced-folder` plugin resolves at deploy time, so explicit cloud providers thread through `ComponentResourceOptions.providers`, never a package-level credential.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/synced-folder`
- package: `@pulumi/synced-folder`
- license: Apache-2.0
- import: `@pulumi/synced-folder` → `{ S3BucketFolder, GoogleCloudFolder, AzureBlobFolder, Provider }`
- owner: `iac`
- rail: fabric / static-distribution
- runtime: Node deploy-host; the `pulumi-resource-synced-folder` plugin performs the sync, so the folder `path` must exist on the deploy host at run time
- depends-on: `@pulumi/pulumi` only (multi-language components — the cloud writes happen in the plugin process under the threaded provider)
- capability: local-folder-to-bucket convergence across S3/GCS/Azure-Blob, per-file state-tracked objects or out-of-state bulk sync, hidden-file admission, alias-free replacement control
- abi-note: no class declares an output property — the component's evidence is its child objects in the resource graph; `static isInstance` is the only non-constructor member

## [02]-[FOLDER_DIALECTS]

[DIALECT_SCOPE]: one component pattern, three coordinate sets
- rail: static-distribution
- Every dialect is `new <Dialect>Folder(name, args, opts?: pulumi.ComponentResourceOptions)` over the same policy axis; only the bucket coordinates differ. `acl` exists on the S3 dialect alone (a canned-ACL string the bucket's ownership posture must admit); GCS and Azure inherit access from the bucket/container policy.

| [INDEX] | [SYMBOL] | [REQUIRED_COORDINATES] | [SHARED_POLICY_FIELDS] |
|:-----: |:------- |:--------------------- |:--------------------- |
| [01] | `S3BucketFolder` | `path`, `bucketName`, `acl` (all `Input<string>`) | `managedObjects?`, `includeHiddenFiles?`, `disableManagedObjectAliases?` — each `Input<boolean>` |
| [02] | `GoogleCloudFolder` | `path`, `bucketName` | same triple |
| [03] | `AzureBlobFolder` | `path`, `containerName`, `storageAccountName`, `resourceGroupName` | same triple |
| [04] | `Provider` | empty-args `ProviderResource` marker | cloud credentials ride the threaded aws/gcp/azure provider, never here |

[POLICY_SCOPE]: the sync-policy axis
- `managedObjects` (default true) tracks every file as an individual state-managed object — per-file diffs, per-file deletes, policy visibility; `false` switches to one-shot out-of-state bulk sync through the matching cloud CLI on the deploy host — the large-site row where per-file state is cost, not evidence. Changing the mode replaces the folder's object management wholesale; `disableManagedObjectAliases` opts out of the aliasing that otherwise smooths that transition. `includeHiddenFiles` admits dotfiles the sync skips by default.

## [03]-[IMPLEMENTATION_LAW]

[DISTRIBUTION_TOPOLOGY]:
- cell law: the bucket is the arm's object cell, the folder is its content — `S3BucketFolder.bucketName` binds `aws.s3.BucketV2.bucket` (`.api/pulumi-aws.md`), `GoogleCloudFolder.bucketName` binds `gcp.storage.Bucket.name` (`.api/pulumi-gcp.md`), and the component never creates the bucket; a folder component pointed at an unmanaged bucket name is an adoption defect, not a shortcut.
- provider law: the plugin process inherits no ambient credential posture from this package — the arm's one provider seam threads through `opts.providers`, the same one-seam-per-arm law every arm obeys; a folder component constructed without the arm's provider set silently targets the default account, which is the named defect.
- fronting law: the synced bucket is origin material — the DNS/CDN rows that front it (`cloudflare.DnsRecord` proxied records, the aws/gcp edge cells) stay on their owning arms; no Cloudflare R2 dialect exists here, so the `cloudflare` arm's static distribution remains its own rows and never routes through this package.
- content law: `path` points at a built artifact directory the deploy host already holds — a `docker-build`/bundler product handed in like every pin; the lib hardcodes no path, and rebuilding content re-converges through the component's own diff, no `triggers` channel needed.
- mode law: `managedObjects: true` for sites whose files are policy-visible evidence; `false` only when file count makes per-file state a cost, accepting that drift detection over individual objects goes dark with it.

[RAIL_LAW]:
- Package: `@pulumi/synced-folder`
- Owns: local-folder-to-bucket content convergence for static distribution across the S3/GCS/Azure-Blob dialects
- Accept: the dialect matching the arm's object cell, `bucketName`/`containerName` bound to the cell's outputs, the arm provider threaded via `opts.providers`, explicit `managedObjects` choice per site scale
- Reject: bucket creation through this package, hand-rolled per-file `BucketObject` loops duplicating a dialect, an R2/Cloudflare route through the S3 dialect, ambient default-account syncs, `path` literals baked into the lib
