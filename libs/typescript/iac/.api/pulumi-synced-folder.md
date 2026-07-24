# [TS_IAC_API_PULUMI_SYNCED_FOLDER]

`@pulumi/synced-folder` converges a local folder into an existing bucket as one `ComponentResource` pattern across three storage dialects — required `path` with the dialect's bucket coordinates over a shared sync-policy axis — owning content convergence alone: the bucket stays the object cell's product, so in `iac` it is the static-distribution leg riding a prepared arm whose DNS/CDN rows front the synced origin.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/synced-folder`
- package: `@pulumi/synced-folder` (Apache-2.0)
- module: `@pulumi/synced-folder`
- runtime: Node deploy host; the `pulumi-resource-synced-folder` plugin runs the sync, so `path` must resolve on the deploy host at run time
- rail: fabric / static-distribution
- abi: no dialect declares an output property — child objects in the resource graph are the evidence, and `static isInstance` is the only non-constructor member

## [02]-[FOLDER_DIALECTS]

[DIALECT_SCOPE]: one `ComponentResource` pattern, three coordinate sets — every dialect constructs as `new <Dialect>Folder(name, args, opts?)` over the shared `managedObjects`/`includeHiddenFiles`/`disableManagedObjectAliases` triple, each coordinate an `Input<string>`; only the bucket coordinates below differ, and `acl` (S3 alone) is a canned-ACL string the bucket's ownership posture must admit.

| [INDEX] | [SYMBOL]            | [REQUIRED_COORDINATES]                                          |
| :-----: | :------------------ | :-------------------------------------------------------------- |
|  [01]   | `S3BucketFolder`    | `path` `bucketName` `acl`                                       |
|  [02]   | `GoogleCloudFolder` | `path` `bucketName`                                             |
|  [03]   | `AzureBlobFolder`   | `path` `containerName` `storageAccountName` `resourceGroupName` |
|  [04]   | `Provider`          | empty-args provider marker                                      |

[POLICY_SCOPE]: the sync-policy axis
- `managedObjects` (default true) tracks every file as an individual state object — per-file diff, delete, and policy visibility; `false` runs one-shot out-of-state bulk sync through the matching cloud CLI, trading per-file state for scale and going dark on per-object drift.
- `disableManagedObjectAliases` drops the alias option that otherwise smooths a wholesale mode switch.
- `includeHiddenFiles` (default false) admits the dotfiles the sync skips.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- cell split: the bucket is the arm's object cell and the folder its content; the component converges content only and never creates the bucket, so a dialect pointed at an unmanaged bucket name is an adoption defect.
- provider isolation: the plugin process inherits no ambient credential — the arm's one provider threads through `opts.providers`, and a dialect built without it silently targets the default account.
- content source: `path` resolves a built-artifact directory the deploy host already holds, handed in like any pin; the lib hardcodes no path, and rebuilt content re-converges through the component's own diff with no `triggers` channel.

[STACKING]:
- `@pulumi/aws`(`.api/pulumi-aws.md`): `S3BucketFolder.bucketName` binds `aws.s3.BucketV2.bucket`, the arm's object cell feeding the folder's sync target.
- `@pulumi/gcp`(`.api/pulumi-gcp.md`): `GoogleCloudFolder.bucketName` binds `gcp.storage.Bucket.name`, the same object-cell-to-content seam on the GCS arm.
- within-lib: the built frontend folder lands in the prepared arm's object cell, and the static-distribution leg composes the dialect matching that cell.

[RAIL_LAW]:
- Package: `@pulumi/synced-folder`
- Owns: local-folder-to-bucket content convergence for static distribution across the S3, GCS, and Azure-Blob dialects
- Accept: the dialect matching the arm's object cell, `bucketName`/`containerName` bound to the cell's outputs, the arm provider threaded via `opts.providers`, an explicit `managedObjects` choice per site scale
- Reject: bucket creation through this package, hand-rolled per-file `BucketObject` loops duplicating a dialect, an R2 route through the S3 dialect, ambient default-account syncs, `path` literals baked into the lib
