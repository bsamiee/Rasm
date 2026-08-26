# [TS_IAC_API_PULUMI_SYNCED_FOLDER]

`@pulumi/synced-folder` converges a local folder into an existing bucket as one `ComponentResource` pattern across three storage dialects — required `path` with the dialect's bucket coordinates over a shared sync-policy axis — owning content convergence alone: the bucket stays the object cell's product, so in `iac` it is the static-distribution leg riding a prepared arm whose DNS/CDN rows front the synced origin.

## [01]-[FOLDER_DIALECTS]

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

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- cell split: the bucket is the arm's object cell and the folder its content; the component converges content only and never creates the bucket, so a dialect pointed at an unmanaged bucket name is an adoption defect.
- provider isolation: the plugin process inherits no ambient credential — the arm's one provider threads through `opts.providers`, and a dialect built without it silently targets the default account.
- content source: `path` resolves a built-artifact directory the deploy host already holds, handed in like any pin; the lib hardcodes no path, and rebuilt content re-converges through the component's own diff with no `triggers` channel.
- key derivation: no dialect declares a key prefix — `path`, the bucket coordinates, and the policy triple are the whole arg surface — so an object's key IS its path relative to the synced root, and a publishing plane's served address and the build's output location are one fact stated twice.
- presence: the sync creates one managed object per file it FINDS under `path`, so a leaf a caller declared and the build never wrote mints no resource and reports no drift; absence is invisible to this component and proves at the composing plane instead.
- content type: each object stamps once from its leaf extension — `mime.getType(path) || "text/plain"` inside the provider — with no override coordinate, so a served extension outside the mime table is a wrong-MIME publish nothing raises; `.cjs` stamps `application/node`, which `nosniff` refuses for classic scripts, so a script leaf publishes under a `.js` spelling.

[STACKING]:
- `@pulumi/aws`(`.api/pulumi-aws.md`): `S3BucketFolder.bucketName` binds `aws.s3.BucketV2.bucket`, the arm's object cell feeding the folder's sync target.
- `@pulumi/gcp`(`.api/pulumi-gcp.md`): `GoogleCloudFolder.bucketName` binds `gcp.storage.Bucket.name`, the same object-cell-to-content binding on the GCS arm.
- within-lib: the built frontend folder lands in the prepared arm's object cell, and the static-distribution leg composes the dialect matching that cell.
