# [PY_DATA_STORE]

The chunked N-D tensor store over one `TensorBackend` axis. `TensorStore` owns the dense `zarr` store (chunk grid plus codec pipeline plus region write), the bounded-memory `cubed` plan that streams blockwise within an `allowed_mem` budget, the ragged `awkward` row for variable-length nested arrays, the transactional `icechunk` versioned-store row, and the `virtualizarr` virtual-reference row that makes archival HDF5/NetCDF/GeoTIFF byte ranges queryable as zero-copy virtual zarr stores. `TensorChunking` carries the chunk grid; `TensorCodec` the compression pipeline; `TensorRegion` the slice; `TensorReceipt` the typed write receipt keyed by runtime `ContentIdentity`. The backend is recovered from the source shape and the requested memory bound, never a parallel store class.

## [1]-[INDEX]

- `[2]-[TENSOR]`: the chunked N-D tensor store over a `TensorBackend` axis — codec/region rows, ragged + cubed, the versioned and virtual-reference dimensions.

## [2]-[TENSOR]

- Owner: `TensorStore` — one chunked N-D tensor store over a `TensorBackend` axis: the dense `zarr` store, the bounded-memory `cubed` plan, the ragged `awkward` row, the transactional `icechunk` versioned-store row, and the `virtualizarr` virtual-reference row. `TensorChunking` carries the chunk grid; `TensorCodec` the compression pipeline; `TensorRegion` the slice; `TensorReceipt` the typed write receipt.
- Entry: `TensorStore.create` opens a `zarr` array/group rooted at a `ResourceRef` with a `TensorChunking` grid and `TensorCodec` pipeline; `TensorStore.write_region` writes a `TensorRegion` slice (orthogonal/block selection) and folds a `TensorReceipt` keyed by `ContentIdentity`; `TensorStore.plan` lifts the same store into a `cubed.Array` under a `Spec(allowed_mem=...)` for bounded-memory blockwise reductions that materialize back through `cubed.to_zarr`; `TensorStore.ragged` admits an `awkward.Array` and round-trips through `ak.to_parquet`/`ak.from_parquet`.
- Auto: the `zarr` v3 pipeline splits into role-typed slots — `serializer=` takes an `ArrayBytesCodec` (`BytesCodec`/`ShardingCodec`), `compressors=` takes `BytesBytesCodec` rows (`BloscCodec`/`ZstdCodec`/`GzipCodec`); `TensorCodec.pipeline` returns the `(serializer, compressors)` pair fed straight into `create_array`, and conflating the slots fails the `ArrayBytesCodec` type check. The `icechunk` row routes `create`/`write_region` through the icechunk session store instead of `LocalStore`, adding git-like snapshot identity on top of the zarr chunk grid without a parallel store owner.
- Packages: `zarr` (`create_array(serializer=, compressors=)`/`open_array`/`open_group`/`Array.set_orthogonal_selection`/`Array.blocks`/`codecs.{BytesCodec,ShardingCodec,BloscCodec,ZstdCodec,GzipCodec}`/`storage.LocalStore`/`consolidate_metadata`), `cubed` (`from_zarr`/`to_zarr`/`Spec`/`compute`/`sum`/`mean`/`nansum`/`std`/`rechunk`/`map_blocks`), `awkward` (`Array`/`from_iter`/`to_parquet`/`from_parquet`/`num`/`flatten`/`to_regular`), `icechunk` (`Repository`/`Storage`/`Session.commit`), `virtualizarr` (the byte-range chunk-manifest virtual reference), `xarray` (combining virtual references into one datacube), runtime (`ResourceRef`/`ContentIdentity`/`RuntimeRail`/`ReceiptContributor`).
- Growth: a new codec is one `TensorCodec` case; a new chunk strategy is one `TensorChunking` field; a new backend (`tensorstore`/`n5`) is one `TensorBackend` tag plus one `_open` dispatch arm; the versioned-store dimension is one `icechunk` row and the virtual-reference dimension is one `virtualizarr` row, never a parallel store owner.
- Boundary: no compute-package numeric trio (NumPy/SciPy/labelled-array compute is `compute`), no production tensor session, no durable product store; `data` emits a portable content-addressed chunked store and a bounded-memory plan, not a runtime compute graph. A parallel `ZarrStore`/`CubedStore`/`AwkwardStore` family, an `xarray` re-derivation of the dense store, and a hand-rolled chunk codec are the deleted forms.

```python signature
from typing import TYPE_CHECKING, Literal, assert_never

import awkward as ak
import cubed
import zarr
from expression import case, tag, tagged_union
from msgspec import Struct
from zarr import codecs as zc

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability.receipts import Receipt
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Sequence

    import numpy as np
    from zarr.abc.codec import ArrayBytesCodec, BytesBytesCodec


# --- [TYPES] ----------------------------------------------------------------------------
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


# --- [MODELS] ---------------------------------------------------------------------------
class TensorReceipt(Struct, frozen=True):
    backend: TensorBackend
    shape: Shape
    chunks: ChunkGrid
    dtype: DType
    codec: str
    bytes_stored: int
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.Emitted(
            "tensor", self.backend,
            {"shape": "x".join(map(str, self.shape)), "codec": self.codec, "stored": str(self.bytes_stored)},
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
        cls, ref: ResourceRef, shape: Shape, dtype: DType,
        chunking: TensorChunking, codec: TensorCodec = TensorCodec(raw=None),
    ) -> "RuntimeRail[TensorStore]":
        return boundary("tensor.create", lambda: _create_zarr(ref, shape, dtype, chunking, codec))

    def write_region(self, region: TensorRegion, data: "np.ndarray") -> "RuntimeRail[TensorReceipt]":
        return boundary("tensor.write_region", lambda: _write_region(self, region, data))

    def plan(self, allowed_mem: str = "2GB") -> "RuntimeRail[cubed.Array]":
        return boundary(
            "tensor.plan",
            lambda: cubed.from_zarr(str(self.ref.path), spec=cubed.Spec(allowed_mem=allowed_mem)),
        )

    @staticmethod
    def ragged(values: "Sequence[Sequence[object]]") -> "RuntimeRail[ak.Array]":
        return boundary("tensor.ragged", lambda: ak.from_iter(values))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _create_zarr(
    ref: ResourceRef, shape: Shape, dtype: DType, chunking: TensorChunking, codec: TensorCodec,
) -> TensorStore:
    serializer, compressors = codec.pipeline()
    zarr.create_array(
        store=zarr.storage.LocalStore(str(ref.path)),
        shape=shape, dtype=dtype, chunks=chunking.chunks, shards=chunking.shards,
        serializer=serializer, compressors=compressors, overwrite=True,
    )
    return TensorStore(backend="zarr", ref=ref, shape=shape, chunking=chunking, dtype=dtype, codec=codec)


def _write_region(store: "TensorStore", region: TensorRegion, data: "np.ndarray") -> TensorReceipt:
    arr = zarr.open_array(store=zarr.storage.LocalStore(str(store.ref.path)), mode="r+")
    arr.set_orthogonal_selection(region.selection(), data)
    return TensorReceipt(
        backend="zarr", shape=tuple(arr.shape), chunks=tuple(arr.chunks),
        dtype=str(arr.dtype), codec=store.codec.tag, bytes_stored=arr.nbytes_stored(),
        content_key=ContentIdentity.key("tensor", data.tobytes()),
    )


def reduce_plan(plan: "cubed.Array", op: Literal["sum", "mean", "nansum", "std"], axis: int | None) -> "cubed.Array":
    return {"sum": cubed.sum, "mean": cubed.mean, "nansum": cubed.nansum, "std": cubed.std}[op](plan, axis=axis)


def ragged_egress(ref: ResourceRef, array: "ak.Array") -> "RuntimeRail[ContentKey]":
    def _write() -> ContentKey:
        ak.to_parquet(array, str(ref.path))
        return ContentIdentity.key("tensor.ragged", ref.path.read_bytes())
    return boundary("tensor.ragged.egress", _write)
```
