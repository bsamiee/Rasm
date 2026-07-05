# [PY_DATA_STORE]

The dense chunked N-D array store over one `TensorBackend` engine axis. `TensorStore` owns the dense `zarr` v3 array — chunk grid plus three-slot codec pipeline plus orthogonal region write — and lifts the same store into a bounded-memory `cubed` plan that streams blockwise within an `allowed_mem` budget and materializes back through `cubed.to_zarr`. `TensorBackend` is the `StrEnum` whose member value IS the engine tag and whose `create`/`write`/`read` delegate selects the engine — `ZARR` the pure-Python sync `zarr` v3 store over a `zarr.storage.LocalStore`, `TENSORSTORE` the high-throughput async `tensorstore` engine opening the IDENTICAL Zarr v3 chunk grid through `tensorstore.open({"driver": "zarr3", "kvstore": ..., "metadata": {"codecs": [...]}})` over a native `KvStore` JSON backend with native concurrency, micro-caching, `oindex`/`vindex` read-selection, and `Transaction`-staged atomic multi-region writes — config as a domain value carrying behavior, never an `engine=` flag set, never a parallel `open_zarr`/`open_tensorstore` reader family. `TensorChunking` carries the chunk grid plus shard tuple; `TensorCodec` the Zarr v3 `(filters, serializer, compressors)` array-to-array filter / array-to-bytes serializer / bytes-to-bytes compressor pipeline; `TensorRegion` the orthogonal slice; `TensorReceipt` the typed write receipt keyed by exactly one runtime `ContentIdentity`; `PlanReceipt` the typed chunked-compute receipt carrying the `allowed_mem` budget and the measured peak memory the `cubed` executor already records. The backend is recovered from the store URL scheme, never a parallel `ZarrStore`/`TensorStoreStore` family per engine; out-of-core is not a backend but the `cubed` plan over either store, and the versioned and ragged dimensions live on their own `gridded/virtual` and `gridded/ragged` owners, never as backend tags here.

## [01]-[INDEX]

- [01]-[STORE]: the `TensorStore` dense chunked N-D store over a `TensorBackend` engine axis — the `zarr` v3 / `tensorstore` create / region-write / read entrypoint, the `TensorChunking` grid, the `TensorCodec` three-slot serializer pipeline, and the `TensorReceipt` content-keyed write receipt.
- [02]-[PLAN]: the bounded-memory `cubed` plan over the same store — `from_zarr` under a `Spec(work_dir, *, allowed_mem=...)`, the one `PlanOp` named-operation lookup over reductions / linalg / blockwise transforms, the `cubed.to_zarr` materialization, and the `PlanReceipt` carrying the memory budget plus the `TaskEndEvent`-measured peak, never a parallel out-of-core backend.

## [02]-[STORE]

- Owner: `TensorStore` — one frozen dense chunked N-D store carrying the `TensorBackend` engine row, the source `ResourceRef`, the shape, the `TensorChunking` grid, the dtype, and the `TensorCodec` pipeline; `TensorBackend` the `StrEnum` two-engine axis whose member value is the engine tag and whose `create`/`write`/`read` delegate selects the driver (`ZARR` the pure-Python sync `zarr` v3 store over `zarr.storage.LocalStore`, `TENSORSTORE` the async `tensorstore` engine over a `KvStore` backend reading the identical Zarr v3 chunk grid). `TensorChunking` carries the chunk grid plus optional outer `shards` grid and is the SOLE owner of sub-chunk sharding — the native `create_array(shards=)` path wraps the whole inner pipeline, never a second `Serializer` sharding case duplicating the concept; `TensorCodec` the Zarr v3 codec product pairing the orthogonal `filters` array-to-array pre-pipeline with a `Serializer` discriminated union over the `compress`/`raw` array-to-bytes-plus-compressor slot (the compressor-presence axis only — one `BytesCodec` plus a bytes-to-bytes compressor, or the bare `BytesCodec`), so the per-instance `filters` axis never conflates with the serializer-shape discriminant and sharding never splits across two owners; `TensorRegion` the orthogonal slice the region write addresses. The backend is recovered from the store URL scheme through `TensorBackend.for_ref`, never a parallel store class per engine.
- Entry: `TensorStore.create` opens a store rooted at a `ResourceRef` with a `TensorChunking` grid and `TensorCodec` pipeline, lifting the engine's awaitable `create` delegate through `async_boundary` driven by `anyio.run` and folding the recovered shape/chunks/dtype into the frozen owner returned in a `RuntimeRail`; `TensorStore.write_region` absorbs arity over one `writes: Write | Iterable[Write]` parameter normalized once at the head — a lone `(TensorRegion(), array)` pair keeps whole through the closed-owner match arm before the `Iterable` arm can shatter it, an empty snapshot rails as a typed `config` `Error` rather than an `IndexError` escaping the rail, a singular write routes the engine `write` delegate, and a plural write routes `write_many` (`tensorstore` one `Transaction(atomic=True)` staged commit, `zarr` sequential region writes) so the atomic-versus-sequential disposition is the normalized count, never a `*writes` unpack the snapshot caller never asked for and never a flag — and folds one `TensorReceipt` whose `bytes_stored` carries the summed written `data.nbytes` and whose `content_key` folds the `ContentIdentity.of` `stream` modality over every written region's bytes in write order, so a multi-region snapshot keys distinctly rather than collapsing onto the last block alone; `TensorStore.read_region` reads a `TensorRegion` through the engine's selection delegate routed by the region `Indexing` axis (`zarr` `get_orthogonal_selection`/`get_coordinate_selection`, `tensorstore` `await store.oindex[selection].read()`/`store.vindex`) into a NumPy array. One `create`/`write_region`/`read_region` entrypoint family owns all modalities by the `TensorBackend` member the `ResourceRef` scheme recovers and the `Indexing`/arity axes the value carries, never a per-engine reader family and never a per-arm sync portal.
- Growth: a new filter is one `_FILTER` table row plus one `TensorFilter` case carrying its `zarr.codecs` or `zarr.codecs.numcodecs` constructor and its registry codec name; a new compressor is one `_COMPRESSOR` table row under the existing `compress` case, never a parallel arm; a new chunk or shard strategy is one `TensorChunking` field the native `create_array(shards=)`/`sharding_indexed` wrap already threads, never a `Serializer` case; a new read/write selection mode is one `Indexing` literal plus one `_ZARR_WRITE`/`_ZARR_READ` row the `tensorstore` `oindex`/`vindex` views already answer; a new store engine (`n5`, an object-store-backed Zarr) is one `TensorBackend` member plus one `create`/`write`/`write_many`/`read` delegate row; a new `KvStore` cloud backend is one `_KVSTORE_DRIVER` scheme row; a stored-domain resize is one `TensorStore.resize` entry over the catalogued `tensorstore` `resize`/`zarr` `Array.resize`; the bounded-memory plan is the `[2]-[PLAN]` `cubed` row on this same owner; the versioned-store dimension is the `gridded/virtual` owner and the ragged dimension the `gridded/ragged` owner, never a backend tag here.
- Boundary: no compute-package numeric trio (NumPy/SciPy/labelled-array compute is `compute`), no production tensor session, no durable product store; `data` emits a portable content-addressed chunked store, not a runtime compute graph. A parallel `ZarrStore`/`CubedStore`/`TensorStoreStore` family per engine, an `open_zarr`/`open_tensorstore` reader family where one `TensorBackend` delegate dispatches, a `_ts_run` sync portal re-minting the `async_boundary` fault rail per arm, a blocking `Future.result()` inside the async rail where `anyio.run` drives the `async_boundary`, an `obstore` store object passed into the JSON `kvstore` slot where the native `kvstore` JSON spec is the contract, a re-minted `ts.Context()` per open where the `@functools.cache`-memoized `_ts_context` singleton is reused, a `global`-mutated `Any | None` context sentinel where `functools.cache` owns the memoization, an `oindex`-only read dropping the catalogued `vindex` selection, an `xarray` re-derivation of the dense store, a hand-rolled chunk codec / sharding / cache layer `tensorstore` owns, a hand-rolled filter pre-pipeline `zarr.codecs`/`zarr.codecs.numcodecs` already provide, a phantom `zarr.codecs.Delta`/`Quantize`/`LZ4`/`LZMA`/`BZ2`/`Zlib` reference where those names live in `zarr.codecs.numcodecs` (the absorbed live home at `zarr>=3.1.3`; `numcodecs.zarr3` is the DEPRECATED spelling emitting a `DeprecationWarning`, a deleted import), a `zarr.codecs.numcodecs` admission for a name `zarr.codecs` already carries (`BytesCodec`/`ShardingCodec`/`TransposeCodec`/`ScaleOffset`/`BloscCodec`/`ZstdCodec`/`GzipCodec`/`Crc32cCodec`), a `zc.ScaleOffset(dtype=)`/`(astype=)` keyword against its keyword-only `(*, offset, scale)` signature (the dtype-bearing slots belong to `nc.FixedScaleOffset` alone), an `az`/`abfs` tensorstore kvstore row where the source-verified root drivers are exactly file/gcs/http/memory/s3/tsgrpc_kvstore (no azure driver — an azure cube routes through the `zarr` engine's object store or a non-azure ref), a `virtual`/`icechunk`/`awkward` backend tag smuggled onto `TensorBackend`, a `writes[-1].tobytes()` last-region-only content key dropping every prior region from a plural write's identity where the `ContentIdentity.of` `stream` fold keys over all written blocks, a `Serializer.sharding` case duplicating the `TensorChunking.shards` grid where sharding is single-owned by the chunking and wrapped through the native `create_array(shards=)`/`sharding_indexed` projection, a hardcoded-zstd hand-built `ShardingCodec` dropping the inner compressor/filter choice where the native `shards=` wrap keeps the whole inner pipeline, a `shards=`-plus-hand-built-`ShardingCodec` double-sharding where the chunking owns the one wrap, a `*writes` variadic forcing the snapshot caller to unpack where one `Write | Iterable[Write]` parameter normalizes at the head, a `writes[0]` index on an empty call raising `IndexError` past the rail where the empty snapshot is a typed `config` `Error`, and an undecorated `create` admitting a caller `ResourceRef`/`TensorChunking`/`TensorCodec` argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling `interop`/`egress`/`ragged` admission factories share are the deleted forms.

```python signature
import functools
from collections.abc import Awaitable, Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import anyio
import zarr
import zarr.codecs.numcodecs as nc
from beartype import beartype
from expression import Error, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from zarr import codecs as zc

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import OBJECT_STORE_SCHEMES, ResourceRef

if TYPE_CHECKING:
    import numpy as np
    from zarr.abc.codec import ArrayArrayCodec, ArrayBytesCodec, BytesBytesCodec


type Shape = tuple[int, ...]
type ChunkGrid = tuple[int, ...]
type DType = str
type Pipeline = tuple[tuple["ArrayArrayCodec", ...], "ArrayBytesCodec", tuple["BytesBytesCodec", ...]]
type JsonSpec = dict[str, Any]

# `OBJECT_STORE_SCHEMES` is the one `runtime/roots#RESOURCE`-owned scheme vocabulary (`s3`/`gs`/`az`/
# `abfs`), imported not re-declared, so the engine-routing membership test never diverges from the
# `ResourceRef.scheme` authority that mints the refs this owner reads.


class TensorChunking(Struct, frozen=True):
    chunks: ChunkGrid
    shards: ChunkGrid | None = None

    @property
    def grid(self) -> ChunkGrid:
        # the array-level chunk grid: the outer `shards` when sharding, else `chunks`. `tensorstore`'s
        # `chunk_grid` reads this while the `sharding_indexed` inner `chunk_shape` reads `chunks`; the
        # `zarr` native `chunks=`/`shards=` pair encodes the same inner/outer split directly.
        return self.shards or self.chunks


type Compressor = Literal["blosc", "zstd", "gzip", "lz4", "lzma", "bz2", "zlib"]

_COMPRESSOR: "Final[Map[Compressor, tuple[Callable[..., BytesBytesCodec], str, tuple[str, ...]]]]" = Map.of_seq([
    ("blosc", (lambda cname, clevel: zc.BloscCodec(cname=cname, clevel=clevel), "blosc", ("cname", "clevel"))),
    ("zstd", (lambda level: zc.ZstdCodec(level=level), "zstd", ("level",))),
    ("gzip", (lambda level: zc.GzipCodec(level=level), "gzip", ("level",))),
    ("lz4", (lambda level: nc.LZ4(level=level), "numcodecs.lz4", ("level",))),
    ("lzma", (lambda preset: nc.LZMA(preset=preset), "numcodecs.lzma", ("preset",))),
    ("bz2", (lambda level: nc.BZ2(level=level), "numcodecs.bz2", ("level",))),
    ("zlib", (lambda level: nc.Zlib(level=level), "numcodecs.zlib", ("level",))),
])


# the serializer slot is the compressor-presence axis ONLY — `compress` (one `BytesCodec` + one
# bytes->bytes compressor) versus `raw` (the bare `BytesCodec`). Sharding is NOT a serializer case:
# it is the `TensorChunking.shards` grid the native `create_array(shards=)` path wraps the WHOLE
# `(filters, serializer, compressors)` inner pipeline in, so a sharded store keeps every inner
# compressor/filter choice rather than the prior hardcoded-zstd `ShardingCodec` that dropped them.
@tagged_union(frozen=True)
class Serializer:
    tag: Literal["compress", "raw"] = tag()
    compress: "tuple[Compressor, tuple[Any, ...]]" = case()
    raw: None = case()

    def slot(self, pre: "tuple[ArrayArrayCodec, ...]") -> Pipeline:
        match self:
            case Serializer(tag="compress"):
                name, args = self.compress
                build, _, _ = _COMPRESSOR[name]
                return (pre, zc.BytesCodec(), (build(*args),))
            case Serializer(tag="raw"):
                return (pre, zc.BytesCodec(), ())
            case unreachable:
                assert_never(unreachable)

    def slot_json(self, pre: list[JsonSpec]) -> list[JsonSpec]:
        match self:
            case Serializer(tag="compress"):
                name, args = self.compress
                _, codec_name, keys = _COMPRESSOR[name]
                return [*pre, {"name": "bytes"}, {"name": codec_name, "configuration": dict(zip(keys, args, strict=True))}]
            case Serializer(tag="raw"):
                return [*pre, {"name": "bytes"}]
            case unreachable:
                assert_never(unreachable)

    @property
    def name(self) -> str:
        return self.compress[0] if self.tag == "compress" else self.tag


class TensorCodec(Struct, frozen=True):
    serializer: Serializer = Serializer(raw=None)
    filters: "tuple[TensorFilter, ...]" = ()

    def pipeline(self) -> Pipeline:
        # the inner `(filters, serializer, compressors)` triple; the native `create_array(shards=)`
        # path wraps it in the `ShardingCodec` itself when `TensorChunking.shards` is set.
        return self.serializer.slot(tuple(f.codec() for f in self.filters))

    def metadata(self, chunking: "TensorChunking") -> list[JsonSpec]:
        # `tensorstore` carries no native `shards=`, so the `sharding_indexed` wrap is explicit here:
        # a sharded store's outer chunk grid is `shards`, the inner `chunk_shape` is `chunks`.
        inner = self.serializer.slot_json([f.json() for f in self.filters])
        return [{"name": "sharding_indexed", "configuration": {"chunk_shape": list(chunking.chunks), "codecs": inner}}] if chunking.shards else inner

    @property
    def name(self) -> str:
        return self.serializer.name


type Filter = Literal["transpose", "scale_offset", "delta", "fixed_scale_offset", "quantize", "bitround", "packbits"]

# `zc.ScaleOffset` is keyword-only `(*, offset=0, scale=1)` — the `dtype`/`astype` slots belong to
# the `numcodecs` `FixedScaleOffset` row alone, so the native case carries the two floats only.
_FILTER: "Final[Map[Filter, tuple[Callable[..., ArrayArrayCodec], str, tuple[str, ...]]]]" = Map.of_seq([
    ("transpose", (lambda order: zc.TransposeCodec(order=order), "transpose", ("order",))),
    ("scale_offset", (lambda scale, offset: zc.ScaleOffset(offset=offset, scale=scale), "scaleoffset", ("scale", "offset"))),
    ("delta", (lambda dtype: nc.Delta(dtype=dtype), "numcodecs.delta", ("dtype",))),
    (
        "fixed_scale_offset",
        (lambda scale, offset, dtype: nc.FixedScaleOffset(scale=scale, offset=offset, dtype=dtype), "numcodecs.fixedscaleoffset", ("scale", "offset", "dtype")),
    ),
    ("quantize", (lambda digits, dtype: nc.Quantize(digits=digits, dtype=dtype), "numcodecs.quantize", ("digits", "dtype"))),
    ("bitround", (lambda keepbits: nc.BitRound(keepbits=keepbits), "numcodecs.bitround", ("keepbits",))),
    ("packbits", (lambda: nc.PackBits(), "numcodecs.packbits", ())),
])


@tagged_union(frozen=True)
class TensorFilter:
    tag: Filter = tag()
    transpose: ChunkGrid = case()
    scale_offset: tuple[float, float] = case()
    delta: DType = case()
    fixed_scale_offset: tuple[float, float, DType] = case()
    quantize: tuple[int, DType] = case()
    bitround: int = case()
    packbits: None = case()

    def _args(self) -> tuple[Any, ...]:
        match self:
            case TensorFilter(tag="transpose"):
                return (list(self.transpose),)
            case TensorFilter(tag="scale_offset"):
                return self.scale_offset
            case TensorFilter(tag="delta"):
                return (self.delta,)
            case TensorFilter(tag="fixed_scale_offset"):
                return self.fixed_scale_offset
            case TensorFilter(tag="quantize"):
                return self.quantize
            case TensorFilter(tag="bitround"):
                return (self.bitround,)
            case TensorFilter(tag="packbits"):
                return ()
            case unreachable:
                assert_never(unreachable)

    def codec(self) -> "ArrayArrayCodec":
        build, _, _ = _FILTER[self.tag]
        return build(*self._args())

    def json(self) -> JsonSpec:
        _, name, keys = _FILTER[self.tag]
        return {"name": name, "configuration": dict(zip(keys, self._args(), strict=True))} if keys else {"name": name}


type Indexing = Literal["orthogonal", "vectorized"]


class TensorRegion(Struct, frozen=True):
    bounds: tuple[tuple[int, int], ...]
    indexing: Indexing = "orthogonal"

    def selection(self) -> tuple[slice, ...]:
        return tuple(slice(lo, hi) for lo, hi in self.bounds)


type Write = "tuple[TensorRegion, np.ndarray]"


class TensorBackend(StrEnum):
    ZARR = "zarr"
    TENSORSTORE = "tensorstore"

    @staticmethod
    def for_ref(ref: ResourceRef) -> "TensorBackend":
        return TensorBackend.TENSORSTORE if ref.scheme in OBJECT_STORE_SCHEMES else TensorBackend.ZARR

    @property
    def create(self) -> "Callable[[ResourceRef, Shape, DType, TensorChunking, TensorCodec], Awaitable[None]]":
        return _CREATE[self]

    @property
    def write(self) -> "Callable[[ResourceRef, TensorRegion, np.ndarray], Awaitable[int]]":
        return _WRITE[self]

    @property
    def write_many(self) -> "Callable[[ResourceRef, tuple[Write, ...]], Awaitable[int]]":
        return _WRITE_MANY[self]

    @property
    def read(self) -> "Callable[[ResourceRef, TensorRegion], Awaitable[np.ndarray]]":
        return _READ[self]


class TensorReceipt(Struct, frozen=True):
    backend: TensorBackend
    shape: Shape
    chunks: ChunkGrid
    dtype: DType
    codec: str
    filters: tuple[str, ...]
    bytes_stored: int
    content_key: ContentKey
    shards: ChunkGrid | None = None

    def contribute(self) -> Iterable[Receipt]:
        return (
            Receipt.of(
                "tensor",
                (
                    "emitted",
                    self.backend.value,
                    {
                        "shape": "x".join(map(str, self.shape)),
                        "codec": self.codec,
                        "filters": ",".join(self.filters),
                        "stored": self.bytes_stored,
                        **({"shards": "x".join(map(str, self.shards))} if self.shards else {}),
                    },
                ),
            ),
        )


class TensorStore(Struct, frozen=True):
    backend: TensorBackend
    ref: ResourceRef
    shape: Shape
    chunking: TensorChunking
    dtype: DType
    codec: TensorCodec

    @classmethod
    @beartype(conf=FAULT_CONF)
    def create(
        cls, ref: ResourceRef, shape: Shape, dtype: DType, chunking: TensorChunking, codec: TensorCodec = TensorCodec()
    ) -> "RuntimeRail[TensorStore]":
        backend = TensorBackend.for_ref(ref)

        async def _open() -> TensorStore:
            await backend.create(ref, shape, dtype, chunking, codec)
            return TensorStore(backend, ref, shape, chunking, dtype, codec)

        return anyio.run(async_boundary, "tensor.create", _open)

    def write_region(self, writes: "Write | Iterable[Write]") -> "RuntimeRail[TensorReceipt]":
        # one `T | Iterable[T]` parameter the head normalizes once: a lone `(region, array)` pair is
        # a 2-tuple whose `[0]` is a `TensorRegion`, so the `(TensorRegion(), _)` arm keeps it whole
        # before the `Iterable` arm can shatter the pair — the closed-owner analog of the `str`/`bytes`
        # seam. An empty snapshot is a typed `Error`, never the `writes[0]` `IndexError` escaping the rail.
        match writes:
            case (TensorRegion(), _) as lone:
                staged: tuple[Write, ...] = (lone,)
            case Iterable() as many:
                staged = tuple(many)
            case _ as unreachable:
                assert_never(unreachable)
        if not staged:
            return Error(BoundaryFault(config=("tensor.write_region", "empty-writes")))

        async def _write() -> int:
            head = staged[0]
            return await self.backend.write(self.ref, *head) if len(staged) == 1 else await self.backend.write_many(self.ref, staged)

        # the content key folds the `stream` modality over every written region in write order, so a
        # plural snapshot keys distinctly rather than collapsing onto the last block alone; the
        # singular/plural atomic-vs-sequential disposition is the normalized count, never a flag.
        return anyio.run(async_boundary, "tensor.write_region", _write).bind(
            lambda stored: ContentIdentity.of("tensor", tuple(block.tobytes() for _, block in staged)).map(lambda key: _receipt(self, stored, key))
        )

    def read_region(self, region: TensorRegion) -> "RuntimeRail[np.ndarray]":
        return anyio.run(async_boundary, "tensor.read_region", lambda: self.backend.read(self.ref, region))


_ZARR_WRITE: "Final[Map[Indexing, str]]" = Map.of_seq([("orthogonal", "set_orthogonal_selection"), ("vectorized", "set_coordinate_selection")])
_ZARR_READ: "Final[Map[Indexing, str]]" = Map.of_seq([("orthogonal", "get_orthogonal_selection"), ("vectorized", "get_coordinate_selection")])


async def _zarr_create(ref: ResourceRef, shape: Shape, dtype: DType, chunking: TensorChunking, codec: TensorCodec) -> None:
    filters, serializer, compressors = codec.pipeline()
    zarr.create_array(
        store=zarr.storage.LocalStore(str(ref.path)),
        shape=shape,
        dtype=dtype,
        chunks=chunking.chunks,
        shards=chunking.shards,
        filters=filters,
        serializer=serializer,
        compressors=compressors,
        overwrite=True,
    )


async def _zarr_write(ref: ResourceRef, region: TensorRegion, data: "np.ndarray") -> int:
    arr = zarr.open_array(store=zarr.storage.LocalStore(str(ref.path)), mode="r+")
    getattr(arr, _ZARR_WRITE[region.indexing])(region.selection(), data)
    return int(data.nbytes)


async def _zarr_read(ref: ResourceRef, region: TensorRegion) -> "np.ndarray":
    arr = zarr.open_array(store=zarr.storage.LocalStore(str(ref.path)), mode="r")
    return getattr(arr, _ZARR_READ[region.indexing])(region.selection())


async def _zarr_write_many(ref: ResourceRef, regions: "tuple[Write, ...]") -> int:
    arr = zarr.open_array(store=zarr.storage.LocalStore(str(ref.path)), mode="r+")
    for region, data in regions:
        getattr(arr, _ZARR_WRITE[region.indexing])(region.selection(), data)
    return sum(int(data.nbytes) for _, data in regions)


# maps the `runtime/roots#RESOURCE` `OBJECT_STORE_SCHEMES` vocabulary to the `tensorstore` `kvstore`
# driver names; no `gcs` scheme row (`gs` is the one GCS scheme the roots owner mints) and NO azure
# row — the source-verified kvstore root drivers are exactly file/gcs/http/memory/s3/tsgrpc_kvstore,
# so an `az`/`abfs` ref raises the typed reader-absence error the boundary converts, never a phantom.
_KVSTORE_DRIVER: "Final[Map[str, str]]" = Map.of_seq([("s3", "s3"), ("gs", "gcs")])


def _ts_kvstore(ref: ResourceRef) -> JsonSpec:
    if ref.scheme in OBJECT_STORE_SCHEMES:
        driver = _KVSTORE_DRIVER.get(ref.scheme)
        if driver is None:
            raise ValueError(f"tensorstore carries no {ref.scheme} kvstore driver (roots: file/gcs/http/memory/s3/tsgrpc_kvstore)")
        return {"driver": driver, "path": str(ref.path)}
    return {"driver": "file", "path": str(ref.path)}


def _ts_spec(
    ref: ResourceRef, *, codec: TensorCodec | None = None, shape: Shape = (), chunking: TensorChunking | None = None, dtype: DType = ""
) -> JsonSpec:
    # the array-level `chunk_grid` reads `chunking.grid` (the outer `shards` when sharding, else
    # `chunks`); `codec.metadata(chunking)` wraps the inner pipeline in `sharding_indexed` to match.
    # CAPABILITY ASYMMETRY: the tensorstore zarr3 `metadata.codecs` chain admits the transpose/
    # bytes/sharding_indexed/gzip/blosc/zstd/crc32c names; the `numcodecs.<id>`-named rows and the
    # `scaleoffset` name are `zarr`-engine-only, so a TENSORSTORE store selecting one is the typed
    # engine-capability reject, never a silent both-engine claim.
    metadata: JsonSpec = (
        {}
        if codec is None or chunking is None
        else {
            "shape": list(shape),
            "data_type": dtype,
            "chunk_grid": {"name": "regular", "configuration": {"chunk_shape": list(chunking.grid)}},
            "codecs": codec.metadata(chunking),
        }
    )
    return {"driver": "zarr3", "kvstore": _ts_kvstore(ref), **({"metadata": metadata} if metadata else {})}


@functools.cache
def _ts_context() -> "Any":
    import tensorstore as ts  # noqa: PLC0415

    return ts.Context()


async def _ts_open(spec: JsonSpec, *, create: bool) -> "Any":
    import tensorstore as ts  # noqa: PLC0415

    return await ts.open(spec, create=create, delete_existing=create, context=_ts_context())


async def _ts_create(ref: ResourceRef, shape: Shape, dtype: DType, chunking: TensorChunking, codec: TensorCodec) -> None:
    await _ts_open(_ts_spec(ref, codec=codec, shape=shape, chunking=chunking, dtype=dtype), create=True)


def _ts_view(store: "Any", region: TensorRegion) -> "Any":
    return (store.vindex if region.indexing == "vectorized" else store.oindex)[region.selection()]


async def _ts_write(ref: ResourceRef, region: TensorRegion, data: "np.ndarray") -> int:
    store = await _ts_open(_ts_spec(ref), create=False)
    await _ts_view(store, region).write(data).commit
    return int(data.nbytes)


async def _ts_write_atomic(ref: ResourceRef, regions: "tuple[Write, ...]") -> int:
    import tensorstore as ts  # noqa: PLC0415

    txn = ts.Transaction(atomic=True)
    store = await ts.open(_ts_spec(ref), create=False, context=_ts_context(), transaction=txn)
    for region, data in regions:
        await _ts_view(store, region).write(data).commit
    await txn.commit_async()
    return sum(int(data.nbytes) for _, data in regions)


async def _ts_read(ref: ResourceRef, region: TensorRegion) -> "np.ndarray":
    store = await _ts_open(_ts_spec(ref), create=False)
    return await _ts_view(store, region).read()


_CREATE: "Final[Map[TensorBackend, Callable[[ResourceRef, Shape, DType, TensorChunking, TensorCodec], Awaitable[None]]]]" = Map.of_seq([
    (TensorBackend.ZARR, _zarr_create),
    (TensorBackend.TENSORSTORE, _ts_create),
])
_WRITE: "Final[Map[TensorBackend, Callable[[ResourceRef, TensorRegion, np.ndarray], Awaitable[int]]]]" = Map.of_seq([
    (TensorBackend.ZARR, _zarr_write),
    (TensorBackend.TENSORSTORE, _ts_write),
])
_WRITE_MANY: "Final[Map[TensorBackend, Callable[[ResourceRef, tuple[Write, ...]], Awaitable[int]]]]" = Map.of_seq([
    (TensorBackend.ZARR, _zarr_write_many),
    (TensorBackend.TENSORSTORE, _ts_write_atomic),
])
_READ: "Final[Map[TensorBackend, Callable[[ResourceRef, TensorRegion], Awaitable[np.ndarray]]]]" = Map.of_seq([
    (TensorBackend.ZARR, _zarr_read),
    (TensorBackend.TENSORSTORE, _ts_read),
])


def _receipt(store: TensorStore, bytes_stored: int, key: ContentKey) -> TensorReceipt:
    return TensorReceipt(
        backend=store.backend,
        shape=store.shape,
        chunks=store.chunking.chunks,
        dtype=store.dtype,
        codec=store.codec.name,
        filters=tuple(f.tag for f in store.codec.filters),
        bytes_stored=bytes_stored,
        content_key=key,
        shards=store.chunking.shards,
    )
```

```mermaid
flowchart LR
    Ref["ResourceRef scheme"] --> Backend["TensorBackend.for_ref"]
    Backend -->|local| Zarr["zarr awaitable delegate filters/serializer/compressors"]
    Backend -->|cloud| TS["tensorstore.open zarr3 + KvStore + Transaction"]
    Codec["TensorCodec.pipeline / .metadata"] --> Zarr
    Codec --> TS
    Zarr --> Rail["async_boundary driven by anyio.run"]
    TS --> Rail
    Rail --> Receipt["TensorReceipt ContentIdentity.of"]
```

## [03]-[PLAN]

- Owner: the bounded-memory `cubed` plan over the same `TensorStore` module — `plan` lifts the dense Zarr store into a `cubed.Array` under a `Spec(allowed_mem=...)` for blockwise reductions, out-of-core linear algebra, and per-chunk maps that never exceed the per-task memory budget, and `PlanOp` is the one closed named-operation family folded over the lazy graph. The plan is the out-of-core dimension of the store, not a fifth backend tag — one owner module carries the dense store and its bounded-memory plan, never a parallel `CubedStore` class.
- Cases: `PlanOp` collapses every named lazy-graph operation onto one `@tagged_union` `apply` fold — `reduce` (the Array API reduction resolved off `cubed.Array.__array_namespace__()` over an `axis`/`keepdims` pair — the catalogue-settled `nanmean` plus the `Reduction`-literal `sum`/`mean`/`nansum`/`std`/`var`/`prod`/`max`/`min` members the Array API standard mandates, the bounded-memory aggregation), `linalg` (the `cubed.array_api.linalg` out-of-core `matmul`/`svd`/`qr`/`svdvals`/`tensordot`/`outer`/`vecdot`/`matrix_transpose` over an optional operand, the headline TSQR/SVD bounded-memory capability returning a tuple of factors the multi-output materialization persists whole), `blockwise` (one `cubed.map_blocks` per-chunk callable carrying its `dtype`/`drop_axis`/`new_axis`), `gufunc` (one `cubed.apply_gufunc` generalized ufunc carrying its `signature`/`output_dtypes`/`vectorize`/`allow_rechunk`), and `rechunk` (one `cubed.rechunk` boundary realignment carrying its `chunks`/`min_mem` per the catalogued `rechunk(x, chunks, *, min_mem)` arity, the chunk-boundary realignment that recomputes no values) — the operation dimension a case the `apply` fold dispatches by `match`/`case` closed with `assert_never`, never a `sum_plan`/`svd_plan`/`map_blocks_plan`/`rechunk_plan` sibling family.
- Entry: `plan` opens the store as a `cubed.Array` through `cubed.from_zarr(str(store.ref.path), spec=cubed.Spec(str(work_dir.path), allowed_mem=, reserved_mem=, executor_name=))`, the required `work_dir` scratch root a caller `ResourceRef` (the catalogued `Spec(work_dir, *, ...)` first positional that routes intermediate Zarr writes) and the `allowed_mem`/`reserved_mem`/`executor` triple a ONE `PlanBudget` policy value the entrypoint folds into the `Spec`, never three loose scalars the body re-derives, returning the lazy array in a `RuntimeRail` with the executor declared on the `Spec` at the one graph boundary and the `reserved_mem` headroom calibrated per executor by `cubed.measure_reserved_mem` when the budget's `reserved_mem` is `None` rather than a hardcoded literal; `PlanOp.apply` folds the named operation over the lazy graph, never a per-operation method; `materialize` runs `cubed.store` over every output array of the operation (one array for a reduction, the full factor tuple for a `svd`/`qr` so no factor is dropped) against a target `ResourceRef` under a `MemoryProbe` `Callback` whose `on_operation_start`/`on_task_end` accumulate the operation count, task count, and `TaskEndEvent` peak, reading the budget and executor name off the lazy array's own `array.spec` rather than re-passing them, folding one `PlanReceipt` carrying the `allowed_mem`/`reserved_mem` budget plus the observed peak and arity, and the materialized result re-enters through `[1]-[STORE]` `TensorStore.create`/`write_region` as a fresh content-keyed `TensorReceipt`. The operation is one named-operation lookup over `PlanOp`, so a `sum_plan`/`mean_plan`/`svd_plan` family collapses to one closed dispatch.
- Receipt: the plan emits no receipt while lazy — it builds a graph; the `cubed.store` materialization folds one `PlanReceipt` carrying the `allowed_mem`/`reserved_mem` budget, the `PlanOp` tag, the executor name, the summed `npartitions` chunk count over every output, the output `arity`, the `on_operation_start`-counted operation count, the `TaskEndEvent`-counted task count, and the `TaskEndEvent`-measured peak memory as typed chunked-compute evidence, and the materialized store re-enters through `[1]-[STORE]` `TensorStore.create`/`write_region` folding the one `TensorReceipt`, never a parallel plan rail and never a generic reported-value receipt where the budget-versus-peak evidence belongs.
- Packages: `cubed` (`from_zarr`/`to_zarr`/`store`/`Spec(work_dir, *, allowed_mem=, reserved_mem=, executor_name=)`/`measure_reserved_mem`/`compute`/`nanmean`/`map_blocks`/`apply_gufunc`/`rechunk`/`Callback`/`TaskEndEvent`/`npartitions`/`Array.__array_namespace__`/`array_api.linalg.{matmul,svd,qr,svdvals,tensordot,outer,vecdot,matrix_transpose}`), `zarr` (the backing store the plan reads and writes), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `plan` entrypoint so a caller `TensorStore`/budget argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `reliability/faults#FAULT` `CLASSIFY` `api` row folds onto the rail, the shared `FAULT_CONF` the sibling data admission seams bind; `materialize` folds over the `cubed.Array` graph the owner already produced and carries no decorator), runtime (`RuntimeRail`/`boundary`/`FAULT_CONF` the shared beartype violation-redirect config/`Receipt`, the `PlanReceipt` `contribute` stream satisfying the `ReceiptContributor` Protocol structurally).
- Growth: a new bounded-memory reduction is one `Reduction` literal member the Array API namespace already answers; a new out-of-core factorization is one `cubed.array_api.linalg` member on the `linalg` arm; a new blockwise transform is one `PlanOp.blockwise` callable; a new chunk-boundary realignment is one `PlanOp.rechunk` row; a new executor is one `Executor` literal the `PlanBudget` carries; a new execution dimension (`executor_options`, `zarr_compressor`) is one `PlanBudget` field the `plan` `Spec` fold threads, never a fresh signature param; a new measured fact is one field off the `Callback` lifecycle (`on_operation_start`/`on_operation_end`/`on_task_end`); zero new surface and never a `cubed` backend tag on `TensorBackend`.
- Boundary: cubed execution is offline study evidence; production substrate selection stays in the C# `csharp:Rasm.Compute` owner; `data` emits a bounded-memory plan plus its typed peak-memory receipt, not a runtime compute graph. A `CubedStore` parallel class, an eager full-materialization where the lazy graph applies, a hand-rolled chunked execution loop / TSQR / blockwise map cubed owns, a per-operation `*_plan` method family where `PlanOp` dispatches, an in-memory NumPy linalg for an out-of-core payload, a generic ledger over the typed `PlanReceipt`, a loose `allowed_mem`/`reserved_mem`/`executor` scalar tail on `plan` where the one `PlanBudget` policy value carries the execution spec, and an undecorated `plan` entrypoint admitting a caller `TensorStore`/budget argument without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling data admission entrypoints share are the deleted forms.

```python signature
from collections.abc import Callable, Iterable
from typing import TYPE_CHECKING, Final, Literal, assert_never

import cubed
from beartype import beartype
from cubed.array_api import linalg as cla
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    import numpy as np

    from rasm.runtime.roots import ResourceRef

# `ChunkGrid`/`DType`/`TensorStore` are the [02]-[STORE] owners in this same module.

type Op = Literal["reduce", "linalg", "blockwise", "gufunc", "rechunk"]
type Executor = Literal["single-threaded", "threads", "processes", "dask", "lithops", "modal", "coiled", "ray", "spark"]
type Reduction = Literal["nanmean", "sum", "mean", "nansum", "std", "var", "prod", "max", "min"]
type Factorization = Literal["matmul", "svd", "qr", "svdvals", "tensordot", "outer", "vecdot", "matrix_transpose"]


class PlanBudget(Struct, frozen=True):
    # the one behavior-carrying execution policy `plan` folds into `cubed.Spec`, never three loose
    # `allowed_mem`/`reserved_mem`/`executor` scalars the body re-derives — a new `Spec` dimension
    # (`executor_options`) lands as one field here with `plan`'s signature untouched. `reserved_mem`
    # `None` is the genuine "calibrate via `measure_reserved_mem`" value, distinct from a budget int.
    allowed_mem: str = "2GB"
    reserved_mem: str | None = None
    executor: Executor = "single-threaded"


DEFAULT_BUDGET: Final[PlanBudget] = PlanBudget()

# the NaN-aware reductions live at `cubed.<name>` top-level, not the Array-API standard namespace, so
# the `reduce` arm resolves these off the `cubed` module and the rest off `__array_namespace__()`.
_NAN_REDUCTIONS: Final[frozenset[Reduction]] = frozenset({"nanmean", "nansum"})

_LINALG: "Final[Map[Factorization, Callable[..., cubed.Array | tuple[cubed.Array, ...]]]]" = Map.of_seq([
    ("matmul", cla.matmul),
    ("svd", cla.svd),
    ("qr", cla.qr),
    ("svdvals", cla.svdvals),
    ("tensordot", cla.tensordot),
    ("outer", cla.outer),
    ("vecdot", cla.vecdot),
    ("matrix_transpose", cla.matrix_transpose),
])


@tagged_union(frozen=True)
class PlanOp:
    tag: Op = tag()
    reduce: "tuple[Reduction, int | tuple[int, ...] | None, bool]" = case()
    linalg: "tuple[Factorization, cubed.Array | int | tuple[int, ...] | None]" = case()
    blockwise: "tuple[Callable[..., np.ndarray], DType, int | None, int | None]" = case()
    gufunc: "tuple[Callable[..., np.ndarray], str, tuple[DType, ...], bool, bool]" = case()
    rechunk: "tuple[ChunkGrid, str | None]" = case()

    def apply(self, plan: "cubed.Array") -> "cubed.Array | tuple[cubed.Array, ...]":
        match self:
            case PlanOp(tag="reduce"):
                op, axis, keepdims = self.reduce
                # the NaN-aware family is catalogued as `cubed.<name>` top-level (`.api` L88), NOT in the
                # Array-API standard namespace; only the standard reductions (`sum`/`mean`/`std`/`var`/
                # `prod`/`max`/`min`) ride `__array_namespace__()` (the conformance surface, `.api` L112).
                source = cubed if op in _NAN_REDUCTIONS else plan.__array_namespace__()
                return getattr(source, op)(plan, axis=axis, keepdims=keepdims)
            case PlanOp(tag="linalg"):
                op, operand = self.linalg
                return _LINALG[op](plan, operand) if operand is not None else _LINALG[op](plan)
            case PlanOp(tag="blockwise"):
                func, dtype, drop_axis, new_axis = self.blockwise
                return cubed.map_blocks(func, plan, dtype=dtype, drop_axis=drop_axis, new_axis=new_axis)
            case PlanOp(tag="gufunc"):
                func, signature, output_dtypes, vectorize, allow_rechunk = self.gufunc
                return cubed.apply_gufunc(func, signature, plan, output_dtypes=output_dtypes, vectorize=vectorize, allow_rechunk=allow_rechunk)
            case PlanOp(tag="rechunk"):
                chunks, min_mem = self.rechunk
                return cubed.rechunk(plan, chunks, min_mem=min_mem)
            case unreachable:
                assert_never(unreachable)


class MemoryProbe(cubed.Callback):
    def __init__(self) -> None:
        super().__init__()
        self.peak_mem = 0
        self.tasks = 0
        self.operations = 0

    def on_operation_start(self, event: "cubed.OperationStartEvent") -> None:
        self.operations += 1

    def on_task_end(self, event: "cubed.TaskEndEvent") -> None:
        self.peak_mem = max(self.peak_mem, int(event.peak_measured_mem_end or 0))
        self.tasks += 1


class PlanReceipt(Struct, frozen=True):
    op: Op
    executor: Executor
    allowed_mem: int
    reserved_mem: int
    npartitions: int
    arity: int
    operations: int
    tasks: int
    peak_mem: int
    target: str

    def contribute(self) -> Iterable[Receipt]:
        return (
            Receipt.of(
                "tensor",
                (
                    "planned",
                    self.op,
                    {
                        "executor": self.executor,
                        "allowed_mem": self.allowed_mem,
                        "reserved_mem": self.reserved_mem,
                        "peak_mem": self.peak_mem,
                        "npartitions": self.npartitions,
                        "arity": self.arity,
                        "operations": self.operations,
                        "tasks": self.tasks,
                    },
                ),
            ),
        )


@beartype(conf=FAULT_CONF)
def plan(store: "TensorStore", work_dir: "ResourceRef", *, budget: PlanBudget = DEFAULT_BUDGET) -> "RuntimeRail[cubed.Array]":
    def _open() -> "cubed.Array":
        reserved = cubed.measure_reserved_mem(budget.executor) if budget.reserved_mem is None else budget.reserved_mem
        spec = cubed.Spec(str(work_dir.path), allowed_mem=budget.allowed_mem, reserved_mem=reserved, executor_name=budget.executor)
        return cubed.from_zarr(str(store.ref.path), spec=spec)

    return boundary("tensor.plan", _open)


def materialize(graph: "cubed.Array", op: PlanOp, target: "ResourceRef") -> "RuntimeRail[PlanReceipt]":
    def _run() -> PlanReceipt:
        result = op.apply(graph)
        outputs = result if isinstance(result, tuple) else (result,)
        spec = outputs[0].spec
        probe = MemoryProbe()
        targets = [f"{target.path}/{op.tag}.{index}" for index in range(len(outputs))]
        cubed.store(list(outputs), targets, callbacks=[probe])
        return PlanReceipt(
            op=op.tag,
            executor=spec.executor_name,
            allowed_mem=int(spec.allowed_mem),
            reserved_mem=int(spec.reserved_mem),
            npartitions=sum(int(array.npartitions) for array in outputs),
            arity=len(outputs),
            operations=probe.operations,
            tasks=probe.tasks,
            peak_mem=probe.peak_mem,
            target=str(target.path),
        )

    return boundary("tensor.materialize", _run)
```

```mermaid
flowchart LR
    Store["TensorStore"] --> From["cubed.from_zarr Spec + measure_reserved_mem"]
    From --> Op["PlanOp.apply reduce/linalg/blockwise/gufunc/rechunk"]
    Op --> Mat["cubed.store over all factors + MemoryProbe Callback"]
    Mat --> Peak["TaskEndEvent peak + operation/task counts"]
    Peak --> Receipt["PlanReceipt budget vs peak + arity"]
    Mat --> Restore["[1]-[STORE] TensorStore.create"]
```
