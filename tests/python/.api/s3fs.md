# [PY_TESTS_API_S3FS]

`s3fs` is the `AsyncFileSystem` implementation of the fsspec algebra for S3: an `S3FileSystem` targets any S3-compatible endpoint through `aiobotocore`/`botocore` and exposes both the universal fsspec verbs and the S3-native surfaces — object e-tags and presigned URLs — a memory backend cannot serve. `_testkit`'s `ObjectStore` double provisions one `S3FileSystem` bound by `endpoint_url` to a `ThreadedMotoServer` loopback; `test_env.py` proves it satisfies the same filesystem algebra the `MemoryFileSystem` double does while round-tripping e-tags and presigned GET.

## [01]-[PACKAGE_SURFACE]

- package: `s3fs` · version `2026.4.0` · license `BSD`
- namespace: `import s3fs` · `from s3fs import S3FileSystem, S3File`
- asset: pure-Python wheel; drives `aiobotocore` `3.7.0` over `botocore` `1.43.0`; fsspec base `2026.4.0` (versions lockstep)
- rail: object-store egress — the S3-native view every `ObjectStore` provision yields

## [02]-[PUBLIC_TYPES]

| [INDEX] | [SYMBOL]       | [KIND]                                       | [CAPABILITY]                                                          |
| :-----: | :------------- | :------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `S3FileSystem` | class `AsyncFileSystem` `AbstractFileSystem` | the S3 filesystem; one endpoint, sync facade over an async core       |
|  [02]   | `S3File`       | class `AbstractBufferedFile`                 | an open object handle; `commit`/`discard` finalize a multipart write  |
|  [03]   | `protocol`     | class attr `("s3", "s3a")`                   | the registry keys fsspec/`UPath` resolve `s3://` and `s3a://` through |

```python signature
class S3FileSystem(AsyncFileSystem):
    def __init__(self, anon=False, endpoint_url=None, key=None, secret=None, token=None, use_ssl=True,
                 client_kwargs=None, requester_pays=False, default_block_size=None, default_fill_cache=True,
                 default_cache_type="readahead", version_aware=False, config_kwargs=None, s3_additional_kwargs=None,
                 session=None, username=None, password=None, cache_regions=False, asynchronous=False, loop=None,
                 max_concurrency=10, fixed_upload_size=False, local_expiry_check=False, **kwargs) -> None: ...
    # skip_instance_cache rides **kwargs into the fsspec instance-cache metaclass, never a named parameter
```

## [03]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                   | [KIND]          | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------ | :-------------- | :-------------------------------------------------------------------- |
|  [01]   | `open(path, mode)` · `pipe_file`/`cat_file` | fsspec io       | byte read/write; `pipe`/`cat` for the bulk multi-path variants        |
|  [02]   | `ls`/`find`/`glob`/`exists`/`isdir`/`info`  | fsspec query    | listing/existence/metadata; `info(path)["ETag"]` is the object e-tag  |
|  [03]   | `copy`/`mv`/`rm`/`makedirs`                 | fsspec mutate   | duplicate, move, recursive delete, bucket/prefix creation             |
|  [04]   | `get`/`put`                                 | fsspec transfer | remote↔local file transfer for both directions                        |
|  [05]   | `url(path, expires, client_method)`         | presign         | presigned HTTP URL an anon client GETs; the memory double refuses     |
|  [06]   | `sign(path, expiration=100)`                | presign alias   | fsspec-generic presign hook delegating to `url`                       |
|  [07]   | `call_s3(method, *akw, **kw)` · `_call_s3`  | boto boundary   | typed S3 call escaping fsspec; `create_bucket` + `LocationConstraint` |

```python signature
def url(self, path, expires: int = 3600, client_method: str = "get_object", **kwargs) -> str: ...
def call_s3(self, method, *akwarglist, **kwargs): ...        # _call_s3 is the async core; call_s3 the sync facade
# bucket creation bypasses s3fs.mkdir's unconditional LocationConstraint (S3 rejects us-east-1 as a constraint):
fs.call_s3("create_bucket", Bucket=bucket, **({"CreateBucketConfiguration": {"LocationConstraint": region}} if region != "us-east-1" else {}))
```

## [04]-[IMPLEMENTATION_LAW]

[S3FS_TOPOLOGY]:
- `S3FileSystem` is an `AsyncFileSystem`: every verb has an async core (`_ls`, `_cat_file`, `_call_s3`) with a sync facade wrapping it; `asynchronous=True` exposes the coroutines directly, `asynchronous=False` (the double's mode) drives them through the fsspec event loop.
- fsspec caches instances by constructor args; a reissued endpoint port resurfaces a dead server's `dircache`, so the double sets `skip_instance_cache=True` to force a fresh filesystem per provision.
- `endpoint_url` + `client_kwargs={"region_name": …}` retarget the client at any S3-compatible wire (moto here); `key`/`secret` are static double credentials, never real secrets.
- S3-native surfaces (`url`, `info(...)["ETag"]`) live below the fsspec algebra — they are the capability boundary the memory backend cannot cross, which is why the double resolves to the concrete `S3FileSystem` type, not `AbstractFileSystem`.

[STACKING]:
- `moto`(`.api/moto.md`): `_provision_store` binds an `endpoint_url` to a `ThreadedMotoServer` loopback; the real HTTP wire is what makes e-tags and presigned GET observable in `test_env.py`.
- `fsspec`/`universal-pathlib`: `s3fs` registers `("s3", "s3a")` into the fsspec registry, so a `UPath("s3://…", **storage_options)` resolves this backend; `_provision_fs` uses the sibling `MemoryFileSystem`/`DirFileSystem` for the honest no-presign lane.
- `network` marker: the S3 view speaks real loopback HTTP, so every `ObjectStore` spec carries `network` and stays out of the socket-disabled mutation lane.

[LOCAL_ADMISSION]:
- Admitted at the dev-plane test tier only (`[dependency-groups] dev`, `s3fs`); never a runtime `libs/python` dependency — runtime object-store egress owns `obstore`.
- `ObjectStore` is the sole `s3fs` consumer; specs reach S3 egress through `provision(ObjectStore()).client_factory()`, never a bare `S3FileSystem`.

[RAIL_LAW]:
- Package: `s3fs`
- Owns: the S3-native fsspec view over a moto endpoint — the full fsspec algebra with e-tags and presigned URLs.
- Accept: `S3FileSystem(endpoint_url=…, client_kwargs={"region_name": …}, skip_instance_cache=True)`; `call_s3` for typed bucket ops the fsspec algebra cannot express; `url`/`info(...)["ETag"]` for the S3-native egress assertions.
- Reject: an instance-cached filesystem over an ephemeral endpoint (stale `dircache`); `s3fs.mkdir` for a `us-east-1` bucket (its unconditional `LocationConstraint` fails); any `s3fs` import outside the `_testkit` `env.py` owner.
