# [PY_DATA_TENSOR]

The chunked N-D tensor store over one `TensorBackend` axis. `TensorStore` owns the dense `zarr` store (chunk grid plus codec pipeline plus region write), the bounded-memory `cubed` plan that streams blockwise within an `allowed_mem` budget, the ragged `awkward` row for variable-length nested arrays, the transactional `icechunk` versioned-store row, and the `virtualizarr` virtual-reference row that makes archival HDF5/NetCDF/GeoTIFF byte ranges queryable as zero-copy virtual zarr stores. `TensorChunking` carries the chunk grid; `TensorCodec` the compression pipeline; `TensorRegion` the slice; `TensorReceipt` the typed write receipt keyed by runtime `ContentIdentity`. The backend is recovered from the source shape and the requested memory bound, never a parallel store class.

## [01]-[INDEX]

- [01]-[TENSOR]: the chunked N-D tensor store over a `TensorBackend` axis — codec/region rows, ragged + cubed, the versioned and virtual-reference dimensions.

## [02]-[TENSOR]

- Owner: `TensorStore` — one chunked N-D tensor store over a `TensorBackend` axis: the dense `zarr` store, the bounded-memory `cubed` plan, the ragged `awkward` row, the transactional `icechunk` versioned-store row, and the `virtualizarr` virtual-reference row. `TensorChunking` carries the chunk grid; `TensorCodec` the compression pipeline; `TensorRegion` the slice; `TensorReceipt` the typed write receipt.
- Entry: `TensorStore.create` opens a `zarr` array/group rooted at a `ResourceRef` with a `TensorChunking` grid and `TensorCodec` pipeline; `TensorStore.write_region` writes a `TensorRegion` slice (orthogonal/block selection) and folds a `TensorReceipt` keyed by `ContentIdentity`; `TensorStore.plan` lifts the same store into a `cubed.Array` under a `Spec(allowed_mem=...)` for bounded-memory blockwise reductions that materialize back through `cubed.to_zarr`; `TensorStore.ragged` admits an `awkward.Array` and round-trips through `ak.to_parquet`/`ak.from_parquet`.
- Auto: the `zarr` v3 pipeline splits into role-typed slots — `serializer=` takes an `ArrayBytesCodec` (`BytesCodec`/`ShardingCodec`), `compressors=` takes `BytesBytesCodec` rows (`BloscCodec`/`ZstdCodec`/`GzipCodec`); `TensorCodec.pipeline` returns the `(serializer, compressors)` pair fed straight into `create_array`, and conflating the slots fails the `ArrayBytesCodec` type check. `zarr`/`cubed`/`awkward` are cp315-clean and import module-top. The `icechunk` row routes `create`/`write_region` through the icechunk session store instead of `LocalStore`, adding git-like snapshot identity on top of the zarr chunk grid without a parallel store owner; `icechunk` alone rides the `python_version<'3.15'` gated band, so its arm imports the dist function-local under `# noqa: PLC0415`, never a module-top import on this cp315-core page. The cp315-clean `virtualizarr` `virtual` row imports module-top and combines byte-range chunk manifests through `xarray` — but `xarray` is on `banned-module-level-imports`, so the `xarray` combine call binds function-local under `# noqa: PLC0415` inside the virtual arm, alongside the gated `icechunk` commit that lands the assembled datacube. `open_virtual_dataset` mandates a per-source `ObjectStoreRegistry` keyed by URL prefix onto `obstore` `from_url` backends — the registry is the virtualizarr-to-obstore seam, never an optional knob, so the virtual arm builds one registry over the source URLs and passes it alongside the `HDFParser` on every `open_virtual_dataset` call.
- Packages: `zarr` (`create_array(serializer=, compressors=)`/`open_array`/`open_group`/`Array.set_orthogonal_selection`/`Array.blocks`/`codecs.{BytesCodec,ShardingCodec,BloscCodec,ZstdCodec,GzipCodec}`/`storage.LocalStore`/`consolidate_metadata`), `cubed` (`from_zarr`/`to_zarr`/`Spec`/`compute`/`sum`/`mean`/`nansum`/`std`/`rechunk`/`map_blocks`), `awkward` (`Array`/`from_iter`/`to_parquet`/`from_parquet`/`num`/`flatten`/`to_regular`), `icechunk` (`Repository`/`Storage`/`Session.commit`, the one `<3.15` gated arm), `virtualizarr` (the cp315-clean byte-range chunk-manifest virtual reference, module-top; `registry.ObjectStoreRegistry` the mandatory URL-to-`obstore` backend map on `open_virtual_dataset`), `obstore` (`store.from_url` backing the per-source registry entries), `xarray` (combining virtual references into one datacube), runtime (`ResourceRef`/`ContentIdentity`/`RuntimeRail`/`ReceiptContributor`).
- Growth: a new codec is one `TensorCodec` case; a new chunk strategy is one `TensorChunking` field; a new backend (`tensorstore`/`n5`) is one `TensorBackend` tag plus one `_open` dispatch arm; the versioned-store dimension is one `icechunk` row and the virtual-reference dimension is one `virtualizarr` row, never a parallel store owner.
- Boundary: no compute-package numeric trio (NumPy/SciPy/labelled-array compute is `compute`), no production tensor session, no durable product store; `data` emits a portable content-addressed chunked store and a bounded-memory plan, not a runtime compute graph. The `icechunk` versioned row rides the `<3.15` gated band and imports function-local; a module-top `icechunk` import on this cp315-core page is the floor-violating form. A parallel `ZarrStore`/`CubedStore`/`AwkwardStore` family, an `xarray` re-derivation of the dense store, and a hand-rolled chunk codec are the deleted forms.

```python signature
from typing import TYPE_CHECKING, Literal, assert_never

import awkward as ak
import cubed
import virtualizarr as vz
import zarr
from expression import case, tag, tagged_union
from msgspec import Struct
from virtualizarr.parsers import HDFParser
from zarr import codecs as zc

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Sequence

    import numpy as np
    from zarr.abc.codec import ArrayBytesCodec, BytesBytesCodec


type Shape = tuple[int, ...]
type ChunkGrid = tuple[int, ...]
type DType = str
type TensorBackend = Literal["zarr", "cubed", "awkward", "icechunk", "virtual"]


class TensorChunking(Struct, frozen=True):
    chunks: ChunkGrid
    shards: ChunkGrid | None = None


@tagged_union(frozen=True)
class TensorCodec:
    tag: Literal["blosc", "zstd", "gzip", "sharding", "raw"] = tag()
    blosc: tuple[str, int] = case()
    zstd: int = case()
    gzip: int = case()
    sharding: ChunkGrid = case()
    raw: None = case()

    def pipeline(self) -> "tuple[ArrayBytesCodec, tuple[BytesBytesCodec, ...]]":
        match self:
            case TensorCodec(tag="blosc"):
                cname, clevel = self.blosc
                return (zc.BytesCodec(), (zc.BloscCodec(cname=cname, clevel=clevel),))
            case TensorCodec(tag="zstd"):
                return (zc.BytesCodec(), (zc.ZstdCodec(level=self.zstd),))
            case TensorCodec(tag="gzip"):
                return (zc.BytesCodec(), (zc.GzipCodec(level=self.gzip),))
            case TensorCodec(tag="sharding"):
                inner = zc.ShardingCodec(chunk_shape=self.sharding, codecs=(zc.BytesCodec(), zc.ZstdCodec()))
                return (inner, ())
            case TensorCodec(tag="raw"):
                return (zc.BytesCodec(), ())
            case unreachable:
                assert_never(unreachable)


class TensorRegion(Struct, frozen=True):
    bounds: tuple[tuple[int, int], ...]

    def selection(self) -> tuple[slice, ...]:
        return tuple(slice(lo, hi) for lo, hi in self.bounds)


class TensorReceipt(Struct, frozen=True):
    backend: TensorBackend
    shape: Shape
    chunks: ChunkGrid
    dtype: DType
    codec: str
    bytes_stored: int
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted", "tensor", self.backend, {"shape": "x".join(map(str, self.shape)), "codec": self.codec, "stored": str(self.bytes_stored)}
        )


class TensorStore(Struct, frozen=True):
    backend: TensorBackend
    ref: ResourceRef
    shape: Shape
    chunking: TensorChunking
    dtype: DType
    codec: TensorCodec

    @classmethod
    def create(
        cls, ref: ResourceRef, shape: Shape, dtype: DType, chunking: TensorChunking, codec: TensorCodec = TensorCodec(raw=None)
    ) -> "RuntimeRail[TensorStore]":
        return boundary("tensor.create", lambda: _create_zarr(ref, shape, dtype, chunking, codec))

    def write_region(self, region: TensorRegion, data: "np.ndarray") -> "RuntimeRail[TensorReceipt]":
        return boundary("tensor.write_region", lambda: _write_region(self, region, data))

    def plan(self, allowed_mem: str = "2GB") -> "RuntimeRail[cubed.Array]":
        return boundary("tensor.plan", lambda: cubed.from_zarr(str(self.ref.path), spec=cubed.Spec(allowed_mem=allowed_mem)))

    @staticmethod
    def ragged(values: "Sequence[Sequence[object]]") -> "RuntimeRail[ak.Array]":
        return boundary("tensor.ragged", lambda: ak.from_iter(values))

    @staticmethod
    def virtual(sources: "tuple[str, ...]", ref: ResourceRef, *, concat_dim: str = "time") -> "RuntimeRail[TensorReceipt]":
        return boundary("tensor.virtual", lambda: _virtual_cube(sources, ref, concat_dim))


def _create_zarr(ref: ResourceRef, shape: Shape, dtype: DType, chunking: TensorChunking, codec: TensorCodec) -> TensorStore:
    serializer, compressors = codec.pipeline()
    zarr.create_array(
        store=zarr.storage.LocalStore(str(ref.path)),
        shape=shape,
        dtype=dtype,
        chunks=chunking.chunks,
        shards=chunking.shards,
        serializer=serializer,
        compressors=compressors,
        overwrite=True,
    )
    return TensorStore(backend="zarr", ref=ref, shape=shape, chunking=chunking, dtype=dtype, codec=codec)


def _write_region(store: "TensorStore", region: TensorRegion, data: "np.ndarray") -> TensorReceipt:
    arr = zarr.open_array(store=zarr.storage.LocalStore(str(store.ref.path)), mode="r+")
    arr.set_orthogonal_selection(region.selection(), data)
    return TensorReceipt(
        backend="zarr",
        shape=tuple(arr.shape),
        chunks=tuple(arr.chunks),
        dtype=str(arr.dtype),
        codec=store.codec.tag,
        bytes_stored=arr.nbytes_stored(),
        content_key=ContentIdentity.of("tensor", data.tobytes()),
    )


def reduce_plan(plan: "cubed.Array", op: Literal["sum", "mean", "nansum", "std"], axis: int | None) -> "cubed.Array":
    return {"sum": cubed.sum, "mean": cubed.mean, "nansum": cubed.nansum, "std": cubed.std}[op](plan, axis=axis)


def ragged_egress(ref: ResourceRef, array: "ak.Array") -> "RuntimeRail[ContentKey]":
    def _write() -> ContentKey:
        ak.to_parquet(array, str(ref.path))
        return ContentIdentity.of("tensor.ragged", ref.path.read_bytes())

    return boundary("tensor.ragged.egress", _write)


def _virtual_cube(sources: "tuple[str, ...]", ref: ResourceRef, concat_dim: str) -> TensorReceipt:
    import xarray as xr  # noqa: PLC0415
    from icechunk import Repository, local_filesystem_storage  # noqa: PLC0415
    from obstore.store import from_url  # noqa: PLC0415
    from virtualizarr.registry import ObjectStoreRegistry  # noqa: PLC0415

    parser = HDFParser()
    registry = ObjectStoreRegistry({url: from_url(url) for url in sources})
    refs = [vz.open_virtual_dataset(url, parser=parser, registry=registry) for url in sources]
    cube = xr.combine_by_coords(refs, combine_attrs="drop_conflicts") if len(refs) > 1 else refs[0]
    repo = Repository.open_or_create(local_filesystem_storage(str(ref.path)))
    session = repo.writable_session("main")
    cube.virtualize.to_icechunk(session.store)
    snapshot = session.commit("virtual-reference cube")
    return TensorReceipt(
        backend="virtual",
        shape=tuple(cube.sizes.values()),
        chunks=(),
        dtype=str(next(iter(cube.data_vars.values())).dtype),
        codec="manifest",
        bytes_stored=int(cube.virtualize.nbytes),
        content_key=ContentIdentity.of("tensor.virtual", snapshot.encode()),
    )
```

## [03]-[RESEARCH]

- [ZARR_PIPELINE]: the `zarr` v3 `create_array(store=, shape=, dtype=, chunks=, shards=, serializer=, compressors=)` slot arity, the `codecs.{BytesCodec,ShardingCodec,BloscCodec,ZstdCodec,GzipCodec}` constructor names, the `Array.set_orthogonal_selection`/`storage.LocalStore`/`open_array` surface are catalogue-confirmed against the folder `zarr` `.api`; the one open seam is the `Array.nbytes_stored()` byte-count method and the `ArrayBytesCodec`/`BytesBytesCodec` ABC split the `pipeline()` return annotates — the catalogue captures the codec types but not the `nbytes_stored` accessor, confirmed against the live v3 distribution.
- [CUBED_AWKWARD]: the `cubed` `from_zarr`/`to_zarr`/`Spec(allowed_mem=...)`/`sum`/`mean`/`nansum`/`std` bounded-plan surface and the `awkward` `from_iter`/`to_parquet`/`Array` ragged surface are catalogue-confirmed against the folder `cubed`/`awkward` `.api`; both are settled fence code.
- [VIRTUAL_REFERENCE]: the `virtualizarr` `open_virtual_dataset(url, registry=, parser=)`/`parsers.HDFParser`/`VirtualiZarrDatasetAccessor.{to_icechunk,nbytes}` surface, the `xarray` `combine_by_coords(data_objects=[], ...)` entrypoint, and the `icechunk` `Repository.open_or_create`/`local_filesystem_storage`/`writable_session`/`Session.{store,commit}` transactional surface are catalogue-confirmed against the folder `virtualizarr`/`xarray`/`icechunk` `.api`. `open_virtual_dataset` requires the `registry` positional (an `ObjectStoreRegistry` mapping URL prefix to an `obstore` `from_url` backend) — a `registry`-less call is the catalogue-violating form the virtual arm never emits. The open seams confirmed against the live distribution before the receipt settles: the `ObjectStoreRegistry` import module (the live spelling is `virtualizarr.registry`, version-sensitive — `obspec_utils.registry` on the obspec-split build), the `xr.combine_by_coords` `combine_attrs=` keyword and its `"drop_conflicts"` value (the `xarray` catalogue enumerates `combine_by_coords` but truncates its keyword arity), and the `cube.virtualize.nbytes` accessor return shape. `virtualizarr` is cp315-clean and imports module-top; the `<3.15`-gated `icechunk` arm, the banned-module-level `xarray` combine, and the `obstore`/`ObjectStoreRegistry` registry build bind function-local under `# noqa: PLC0415`.
