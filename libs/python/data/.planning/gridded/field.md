# [PY_DATA_FIELD]

The CF-conventioned labelled N-D field owner, narrowed to the CF plane: `FieldDataset` reads and writes CF-metadata field cubes over the `FieldEngine` axis, `FieldSelection` owns CF-aware selection and flox-vectorized grouped/binned/resampled reduction and grouped cumulative scan as one closed axis, and `FieldReceipt` folds the content-keyed egress. This is the labelled-field counterpart of the dense `data:gridded/store#STORE` chunk-grid — a distinct owner, never a second labelled-array store inside `store` — and the byte-range virtual-datacube concern lives whole on `data:gridded/virtual#VIRTUAL`, so this page is the pure CF owner.

Import gating is tri-state: `xarray` is banned-module-level, so every call binds function-local under `# noqa: PLC0415`; `netcdf4` is an ungated Forge source build importing module-top; `flox` binds its lowering function-local because `flox.xarray` touches the banned `xarray`, and only the `numba`/`numbagg` `ReductionEngine` rows stay gated on the numba cp315 activation. Egress materializes to the content-keyed `pyarrow`/Zarr surfaces `data:tabular/columnar#SCAN` and `data:gridded/store#STORE` speak, and the absorbed virtual owner mints this same `FieldReceipt` family downward — one receipt family for the labelled plane.

## [01]-[INDEX]

- [01]-[FIELD]: the `FieldDataset` owner over the `FieldEngine` axis — one CF open/read/write entrypoint.
- [02]-[SELECT]: the `FieldSelection` selection/reduction/scan axis threaded by one `ReductionPolicy` through one lowering per kernel.
- [03]-[EGRESS]: the `FieldReceipt` content-keyed `pyarrow`/Zarr egress fold.

## [02]-[FIELD]

- Owner: `FieldDataset` — one frozen CF owner whose `FieldEngine` is recovered from the source shape (the `.nc`/`.h5`/`.zarr` suffix), config as a domain value carrying its `open`/`write` behaviour, never a parallel `open_netcdf`/`open_hdf`/`open_zarr` reader family and never an `engine=` flag set. One `open`/`read`/`write` entrypoint owns all modalities by input shape.
- Packages: `netcdf4` is the first consumer of the admitted-but-unconsumed dist — its `createVariable` compression/quantization knobs thread as the `to_netcdf` `encoding` map, and its `__has_quantization_support__`/`__has_blosc_support__`/`__has_zstandard_support__` build-capability flags gate the non-`zlib` rows; `h5py` backs the `h5netcdf` engine, its native virtual-dataset surface owned by the absorbed `data:gridded/virtual#VIRTUAL` owner.
- Growth: a new CF engine (`pydap`/`scipy`) is one `FieldEngine` member carrying its `open`/`write` delegate; a new CF metadata facet is one field on `FieldDataset`; a new compression/quantization knob is one field on `FieldEncoding`; zero new surface.
- Boundary: no compute-package numeric trio (labelled-array compute is `compute`), no production field session, no durable product store — `data` emits a portable content-addressed CF field cube and its selection plan, never a runtime compute graph.

```python signature
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from beartype import beartype
from msgspec import Struct
from msgspec.structs import asdict
from opentelemetry import trace

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Callable

    import xarray as xr


_TRACER: Final = trace.get_tracer("rasm.data.gridded.field")

type FieldDims = tuple[str, ...]
type FieldCoords = tuple[str, ...]
type QuantizeMode = Literal["BitGroom", "BitRound", "GranularBitRound"]


# the netcdf4-only encoding keys the h5netcdf backend rejects: lossy quantization rides the netCDF-C surface alone, so
# `for_vars(quantize=False)` strips them and the shared compression band threads to both backends.
_QUANTIZE_KEYS: "Final[frozenset[str]]" = frozenset({"least_significant_digit", "significant_digits", "quantize_mode"})


class FieldEncoding(Struct, frozen=True):
    zlib: bool = True
    complevel: int = 4
    shuffle: bool = True
    fletcher32: bool = False
    chunksizes: tuple[int, ...] | None = None
    least_significant_digit: int | None = None
    significant_digits: int | None = None
    quantize_mode: QuantizeMode = "BitGroom"
    dtype: str | None = None
    fill_value: float | None = None

    def for_vars(self, names: "tuple[str, ...]", *, quantize: bool = True) -> dict[str, dict[str, object]]:
        row = {key: value for key, value in asdict(self).items() if value is not None and (quantize or key not in _QUANTIZE_KEYS)}
        return {name: row for name in names}


class FieldEngine(StrEnum):
    NETCDF4 = "netcdf4"
    H5NETCDF = "h5netcdf"
    ZARR = "zarr"

    @classmethod
    def from_ref(cls, ref: ResourceRef) -> "FieldEngine":
        match ref.path.suffix.lower():
            case ".zarr":
                return cls.ZARR
            case ".h5" | ".hdf5":
                return cls.H5NETCDF
            case _:
                return cls.NETCDF4

    def open(self, path: str) -> "Callable[[], xr.Dataset]":
        import xarray as xr  # noqa: PLC0415

        match self:
            case FieldEngine.ZARR:
                return lambda: xr.open_zarr(path, decode_cf=True)
            case FieldEngine.NETCDF4 | FieldEngine.H5NETCDF:
                return lambda: xr.open_dataset(path, engine=self.value, chunks="auto", decode_cf=True)
            case unreachable:
                assert_never(unreachable)

    def write(self, dataset: "xr.Dataset", path: str, encoding: FieldEncoding) -> "Callable[[], None]":
        names = tuple(dataset.data_vars)
        match self:
            case FieldEngine.ZARR:
                return lambda: dataset.to_zarr(path, mode="w")
            case FieldEngine.NETCDF4:
                return lambda: dataset.to_netcdf(path, engine="netcdf4", encoding=encoding.for_vars(names))
            case FieldEngine.H5NETCDF:
                return lambda: dataset.to_netcdf(path, engine="h5netcdf", encoding=encoding.for_vars(names, quantize=False))
            case unreachable:
                assert_never(unreachable)


class FieldDataset(Struct, frozen=True):
    ref: ResourceRef
    engine: FieldEngine
    dims: FieldDims
    coords: FieldCoords
    units: dict[str, str]

    @classmethod
    @beartype(conf=FAULT_CONF)
    def open(cls, ref: ResourceRef) -> "RuntimeRail[FieldDataset]":
        # CF cube I/O carries a span per leg — trace parity with the gridded plane; the fence inside marks a failed leg's span.
        with _TRACER.start_as_current_span("field.open", attributes={"rasm.field.engine": FieldEngine.from_ref(ref).value}):
            return boundary("field.open", lambda: _open(ref, FieldEngine.from_ref(ref)))

    def read(self) -> "RuntimeRail[xr.Dataset]":
        with _TRACER.start_as_current_span("field.read", attributes={"rasm.field.engine": self.engine.value}):
            return boundary("field.read", self.engine.open(str(self.ref.path)))

    def write(self, dataset: "xr.Dataset", target: ResourceRef, encoding: FieldEncoding = FieldEncoding()) -> "RuntimeRail[FieldReceipt]":
        with _TRACER.start_as_current_span("field.write", attributes={"rasm.field.engine": self.engine.value}):
            return boundary("field.write", lambda: _write(self, dataset, target, encoding)).bind(lambda railed: railed)


def _open(ref: ResourceRef, engine: FieldEngine) -> FieldDataset:
    dataset = engine.open(str(ref.path))()
    units = {name: str(var.attrs.get("units", "")) for name, var in dataset.variables.items()}
    return FieldDataset(ref=ref, engine=engine, dims=tuple(dataset.dims), coords=tuple(dataset.coords), units=units)
```

## [03]-[SELECT]

- Owner: `FieldSelection` — one closed axis keeping label-vs-position, point-vs-group, and reduce-vs-scan as cases, never a `sel`/`isel`/`groupby`/`resample` sibling-method family on `FieldDataset`. `ReductionPolicy` is the one frozen behaviour carrier every grouped/binned/resampled/scanned arm threads; the reduce `func` and scan `scan` slots are distinct because `xarray_reduce` and `groupby_scan` are distinct kernels, and `ReindexStrategy` replaces the bare-`bool` reindex so `array_type` selects a dense against `SPARSE_COO` intermediate for high-cardinality groups. The fallback is the `flox`-absent install band alone — one `_FALLBACK_CALL` grouper row per kind plus the nan-strip maps — never a per-reduction route; the band is total over every reduction the bare grouper natively exposes, with `mode` the documented flox-only narrowing rather than a silently-broken arm.
- Cases: `Scan` is the one running-total/forward-fill modality the reduce family cannot express, returning one value per input element rather than one per group; `Resample` rides the same `_reduce` lowering with the frequency on the policy, never a second reduction code path.
- Receipt: the selection emits no receipt — it transforms the live dataset; the receipt folds once at egress.
- Growth: a new selection intent is one `FieldSelection` case; a new flox reduction or scan func is one literal member the nan-strip maps derive automatically with zero map edit; a custom reduction is one `flox.Aggregation` on the `func` slot, a custom scan one `flox.Scan` on the `scan` slot; a new parallelization strategy, engine, or reindex intermediate is one literal member on `ReductionPolicy`; zero new surface.
- Boundary: no compute numeric kernel, no durable selection store; never a `FloxReduction`/`BareReduction` split routing `std`/`var`/`prod` to a slow bare path where flox registers them as `func` values, and never two lowering paths for the grouped versus resampled arm.

```python signature
from importlib.util import find_spec
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec.structs import replace

from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    from collections.abc import Callable

    import xarray as xr
    from flox import Aggregation, ReindexStrategy, Scan


# the full registered flox.aggregations func superset: std/var/prod (and nan forms) ARE first-class flox aggregations,
# so there is one Reduction set, never a bare-vs-flox split.
type Reduction = Literal[
    "all",
    "any",
    "count",
    "sum",
    "nansum",
    "prod",
    "nanprod",
    "mean",
    "nanmean",
    "std",
    "nanstd",
    "var",
    "nanvar",
    "max",
    "nanmax",
    "min",
    "nanmin",
    "argmax",
    "nanargmax",
    "argmin",
    "nanargmin",
    "quantile",
    "nanquantile",
    "median",
    "nanmedian",
    "mode",
    "nanmode",
    "first",
    "nanfirst",
    "last",
    "nanlast",
]
type ScanFunc = Literal["cumsum", "nancumsum", "ffill", "bfill"]
type ReductionMethod = Literal["map-reduce", "blockwise", "cohorts"]
type ReductionEngine = Literal["flox", "numpy", "numba", "numbagg"]
type GrouperKind = Literal["group", "bins", "resample"]
type ReductionFunc = "Reduction | Aggregation"
type ScanRail = "ScanFunc | Scan"
type ReindexPolicy = "ReindexStrategy | bool | None"

_HAS_FLOX = find_spec("flox") is not None
# derived by comprehension over each closed literal: the bare-`xarray` grouper exposes `mean(skipna=)`/`cumsum`, never a
# `nan`-prefixed member, so the flox-only nan forms lower to the base member on the fallback band.
_NAN_BASE: "Final[Map[str, str]]" = Map.of_seq((name, name.removeprefix("nan")) for name in Reduction.__value__.__args__)
_SCAN_BASE: "Final[Map[str, str]]" = Map.of_seq((name, name.removeprefix("nan")) for name in ScanFunc.__value__.__args__)
# count/all/any/argmax/argmin/first/last reject the skipna/min_count pair, so the bare path threads it only for this set;
# `mode` has NO xarray grouper member — `policy.base` maps it to `reduce` so the flox-absent band raises explicitly.
_SKIPNA_BASE: "Final[frozenset[str]]" = frozenset({"sum", "prod", "mean", "std", "var", "max", "min", "median", "quantile"})
_FALLBACK_CALL: "Final[Map[GrouperKind, Callable[[xr.Dataset, tuple[object, ...]], object]]]" = Map.of_seq([
    ("group", lambda ds, p: ds.groupby(list(p[0]))),
    ("bins", lambda ds, p: ds.groupby_bins(p[0], list(p[1]))),
    ("resample", lambda ds, p: ds.resample({p[0]: p[1]})),
])


class ReductionPolicy(Struct, frozen=True):
    func: ReductionFunc = "mean"
    scan: ScanRail = "cumsum"
    method: ReductionMethod = "map-reduce"
    engine: ReductionEngine = "flox"
    expected_groups: tuple[object, ...] | None = None
    isbin: bool = False
    min_count: int | None = None
    fill_value: float | None = None
    sort: bool = True
    reindex: ReindexPolicy = None
    keep_attrs: bool = True
    freq: str | None = None
    skipna: bool = True

    def kwargs(self) -> dict[str, object]:
        row = {
            "func": self.func,
            "expected_groups": self.expected_groups,
            "isbin": self.isbin,
            "skipna": self.skipna,
            "method": self.method,
            "engine": self.engine,
            "min_count": self.min_count,
            "fill_value": self.fill_value,
            "sort": self.sort,
            "reindex": self.reindex,
            "keep_attrs": self.keep_attrs,
        }
        return {key: value for key, value in row.items() if value is not None}

    @property
    def vectorizable(self) -> bool:
        return _HAS_FLOX

    @property
    def base(self) -> str:
        if not isinstance(self.func, str):
            return "reduce"
        return "reduce" if _NAN_BASE[self.func] == "mode" else _NAN_BASE[self.func]

    def fallback_kwargs(self) -> dict[str, object]:
        if self.base not in _SKIPNA_BASE:
            return {}
        return {"skipna": self.skipna} | ({"min_count": self.min_count} if self.min_count is not None else {})


@tagged_union(frozen=True)
class FieldSelection:
    tag: Literal["sel", "isel", "interp", "groupby", "groupby_bins", "resample", "scan"] = tag()
    sel: tuple[dict[str, object], str | None, float | None] = case()
    isel: dict[str, int | slice] = case()
    interp: tuple[dict[str, object], str] = case()
    groupby: tuple[tuple[str, ...], ReductionPolicy] = case()
    groupby_bins: tuple[tuple[str, ...], tuple[object, ...], ReductionPolicy] = case()
    resample: tuple[dict[str, str], ReductionPolicy] = case()
    scan: tuple[tuple[str, ...], ReductionPolicy] = case()

    @staticmethod
    def Sel(indexers: dict[str, object], method: str | None = None, tolerance: float | None = None) -> "FieldSelection":
        return FieldSelection(sel=(indexers, method, tolerance))

    @staticmethod
    def Isel(indexers: dict[str, int | slice]) -> "FieldSelection":
        return FieldSelection(isel=indexers)

    @staticmethod
    def Interp(coords: dict[str, object], method: str = "linear") -> "FieldSelection":
        return FieldSelection(interp=(coords, method))

    @staticmethod
    def GroupBy(*by: str, policy: ReductionPolicy = ReductionPolicy()) -> "FieldSelection":
        return FieldSelection(groupby=(by, policy))

    @staticmethod
    def GroupByBins(*by: str, bins: tuple[object, ...], policy: ReductionPolicy = ReductionPolicy()) -> "FieldSelection":
        return FieldSelection(groupby_bins=(by, bins, replace(policy, isbin=True, expected_groups=bins)))

    @staticmethod
    def Resample(indexer: dict[str, str], policy: ReductionPolicy = ReductionPolicy()) -> "FieldSelection":
        ((dim, freq),) = indexer.items()
        return FieldSelection(resample=({dim: freq}, replace(policy, freq=freq)))

    @staticmethod
    def Scan(*by: str, func: "ScanRail" = "cumsum", policy: ReductionPolicy = ReductionPolicy()) -> "FieldSelection":
        return FieldSelection(scan=(by, replace(policy, scan=func)))


def apply(selection: FieldSelection, dataset: "xr.Dataset") -> "RuntimeRail[xr.Dataset]":
    return boundary(f"field.select.{selection.tag}", lambda: _select(selection, dataset))


def _select(selection: FieldSelection, dataset: "xr.Dataset") -> "xr.Dataset":
    match selection:
        case FieldSelection(tag="sel", sel=(indexers, method, tolerance)):
            return dataset.sel(indexers, method=method, tolerance=tolerance)
        case FieldSelection(tag="isel", isel=indexers):
            return dataset.isel(indexers)
        case FieldSelection(tag="interp", interp=(coords, method)):
            return dataset.interp(coords, method=method)
        case FieldSelection(tag="groupby", groupby=(by, policy)):
            return _reduce(dataset, by, by, policy, ("group", (by,)))
        case FieldSelection(tag="groupby_bins", groupby_bins=(by, edges, policy)):
            return _reduce(dataset, by, by, policy, ("bins", (by[0], edges)))
        case FieldSelection(tag="resample", resample=(indexer, policy)):
            ((dim, freq),) = indexer.items()
            return _reduce(dataset, (dim,), (dim,), policy, ("resample", (dim, freq)))
        case FieldSelection(tag="scan", scan=(by, policy)):
            return _scan(dataset, by, policy, ("group", (by,)))
        case unreachable:
            assert_never(unreachable)


def _grouper(by: tuple[str, ...], dim: tuple[str, ...], policy: ReductionPolicy) -> "tuple[object, ...]":
    from xarray.groupers import TimeResampler  # noqa: PLC0415

    return tuple(TimeResampler(policy.freq) if name == dim[0] and policy.freq else name for name in by)


def _reduce(
    dataset: "xr.Dataset", by: tuple[str, ...], dim: tuple[str, ...], policy: ReductionPolicy, fallback: "tuple[GrouperKind, tuple[object, ...]]"
) -> "xr.Dataset":
    if policy.vectorizable:
        from flox.xarray import xarray_reduce  # noqa: PLC0415

        return xarray_reduce(dataset, *_grouper(by, dim, policy), dim=dim, **policy.kwargs())
    grouped = _FALLBACK_CALL[fallback[0]](dataset, fallback[1])
    return getattr(grouped, policy.base)(**policy.fallback_kwargs())


def _scan(dataset: "xr.Dataset", by: tuple[str, ...], policy: ReductionPolicy, fallback: "tuple[GrouperKind, tuple[object, ...]]") -> "xr.Dataset":
    if policy.vectorizable:
        import xarray as xr  # noqa: PLC0415
        from flox import groupby_scan  # noqa: PLC0415

        # `groupby_scan` is a raw-array kernel with no xarray-aware mirror, so `apply_ufunc` lifts it onto the labelled cube —
        # never `Dataset.map`, whose mapper must return a `DataArray` the bare-ndarray kernel does not.
        return xr.apply_ufunc(
            lambda arr, *codes: groupby_scan(arr, *codes, func=policy.scan, axis=-1, method=policy.method, engine=policy.engine),
            dataset,
            *(dataset[name] for name in by),
            input_core_dims=[[by[0]], *([by[0]] for _ in by)],
            output_core_dims=[[by[0]]],
            dask="parallelized",
        )
    grouped = _FALLBACK_CALL[fallback[0]](dataset, fallback[1])
    name = _SCAN_BASE[policy.scan] if isinstance(policy.scan, str) else "cumsum"
    return getattr(grouped, name)(skipna=policy.skipna) if name == "cumsum" else getattr(grouped, name)()
```

## [04]-[EGRESS]

- Owner: `FieldReceipt` — one content-keyed receipt mirroring `data:gridded/store#STORE` `TensorReceipt`: the `engine` slot is the closed `FieldEngine | EgressTag` family exactly as `TensorReceipt.backend` is the typed `TensorBackend` enum, never a bare `str`. The egress reuses the engine `write` delegate the owner already holds, never a second per-format writer family.
- Auto: the content key derives from the written bytes — a Zarr cube hashes its `zarr.json` root-metadata bytes (`zarr_format=3` is the default the `gridded/store` grid writes, never the v2 `.zmetadata` consolidated file), a netCDF cube its on-disk bytes, the Arrow lowering the coalesced record-batch payload — so the field slice carries the same content identity the `columnar` egress expects.
- Receipt: one `FieldReceipt` per egress, never a per-selection rail; the family is shared by the `[02]-[FIELD]` write path and the `data:gridded/virtual#VIRTUAL` absorbed aggregation.
- Growth: a new egress engine is one `FieldEngine` `write` delegate; a new receipt fact is one entry on the fact dict; zero new surface.
- Boundary: composes the `gridded/store#STORE` Zarr egress and the `tabular/columnar#SCAN` Arrow egress, never re-minting either.

```python signature
from collections.abc import Iterable
from typing import TYPE_CHECKING, Literal

from beartype import beartype
from msgspec import Struct

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    import pyarrow as pa
    import xarray as xr


# the egress-source tags beside the real `FieldEngine` members: the receipt engine slot stays a closed family.
type EgressTag = Literal["virtual", "arrow"]


class FieldReceipt(Struct, frozen=True):
    engine: "FieldEngine | EgressTag"
    dims: tuple[str, ...]
    variables: int
    bytes_stored: int
    content_key: ContentKey

    @beartype(conf=FAULT_CONF)
    def to_arrow(self, dataset: "xr.Dataset") -> "RuntimeRail[tuple[pa.Table, FieldReceipt]]":
        return boundary("field.to_arrow", lambda: _to_arrow(dataset)).bind(lambda lowered: _arrow_receipt(*lowered))

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of(
            "field", ("emitted", str(self.engine), {"dims": ",".join(self.dims), "variables": self.variables, "stored": self.bytes_stored})
        )


def _write(field: "FieldDataset", dataset: "xr.Dataset", target: ResourceRef, encoding: "FieldEncoding") -> "RuntimeRail[FieldReceipt]":
    path = str(target.path)
    field.engine.write(dataset, path, encoding)()
    source = target.path.read_bytes() if target.path.is_file() else (target.path / "zarr.json").read_bytes()
    return ContentIdentity.of("field", source).map(
        lambda key: FieldReceipt(
            engine=field.engine, dims=tuple(dataset.sizes), variables=len(dataset.data_vars), bytes_stored=int(dataset.nbytes), content_key=key
        )
    )


def _to_arrow(dataset: "xr.Dataset") -> "tuple[pa.Table, tuple[str, ...], int, bytes]":
    import pyarrow as pa  # noqa: PLC0415

    table = pa.Table.from_pandas(dataset.to_dataframe().reset_index())
    # `combine_chunks().to_batches()[0]` coalesces every chunk so the content key folds a chunk-boundary-stable byte span —
    # never a per-`to_batches()` digest whose partition drifts; an empty table keys off `b""` per the catalogue contract.
    payload = bytes(table.combine_chunks().to_batches()[0].serialize()) if table.num_rows else b""
    return table, tuple(dataset.sizes), len(dataset.data_vars), payload


def _arrow_receipt(table: "pa.Table", dims: tuple[str, ...], variables: int, payload: bytes) -> "RuntimeRail[tuple[pa.Table, FieldReceipt]]":
    return ContentIdentity.of("field.arrow", payload).map(
        lambda key: (table, FieldReceipt(engine="arrow", dims=dims, variables=variables, bytes_stored=len(payload), content_key=key))
    )
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
