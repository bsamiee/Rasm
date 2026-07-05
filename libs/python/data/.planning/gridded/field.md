# [PY_DATA_FIELD]

The CF-conventioned labelled N-D field owner, narrowed to the CF plane: one polymorphic owner over `xarray` reading and writing CF-metadata field cubes through a `FieldEngine` reader-engine axis, with CF-aware coordinate selection, flox-vectorized grouped/binned/resampled reductions and grouped cumulative scans, and unit/coordinate-reference metadata, materializing to the content-keyed `pyarrow`/Zarr egress the `data:tabular/columnar#SCAN` and `data:gridded/store#STORE` owners already speak. `FieldDataset` is the one frozen field owner; `FieldEngine` the `StrEnum` whose member value IS the `xarray` engine string and whose `open`/`write` delegate selects `open_dataset(engine=)`/`open_zarr` against `to_netcdf(engine=)`/`to_zarr` — config as a domain value carrying behaviour, never an `engine=` flag set — and whose CF-time encode rides the `netcdf4` `date2num`/`num2date` calendar surface and whose write knobs ride the `netcdf4` `createVariable(zlib=, chunksizes=, least_significant_digit=, significant_digits=, quantize_mode=)` compression/quantization axis projected through the `xarray` netcdf4-backend encoding-key vocabulary. `FieldSelection` is the one closed `@tagged_union` CF-selection/reduction axis (`Sel`/`Isel`/`Interp`/`GroupBy`/`GroupByBins`/`Resample`/`Scan` folded by `match`/`case` closed with `assert_never`), the variants are cases not sibling methods; `ReductionPolicy` is the one frozen reduction-behaviour carrier every grouped/binned/resampled/scanned arm threads through one reduction lowering — one `Reduction` literal that IS the registered `flox.aggregations` `func` superset (`std`/`var`/`prod` and their `nan` forms are first-class flox aggregations, never a separate bare set), the `flox.Aggregation`/`flox.Scan` custom escapes on the same `func` slot, plus its `method`/`engine`/`expected_groups`/`reindex`/`skipna`/`min_count`/`fill_value` knobs collapsed onto one value, never a parallel per-reduction case or a bare reduction name, and never two lowering paths for the grouped versus resampled arm. The fallback is the `flox`-absent install band alone — `xarray`'s own `groupby`/`groupby_bins`/`resample`-then-reduction over one `_FALLBACK_CALL` grouper row plus the `_NAN_BASE` nan-prefix strip — never a per-reduction route. The byte-range VIRTUAL-datacube concern lives on `data:gridded/virtual#VIRTUAL` — the sole manifest-cube owner absorbing `FieldVirtual`/`VirtualParser`/`ManifestExport`/`CFDtype` whole — so this page is the pure CF owner and the codemap charters exactly one virtualizarr home. `FieldReceipt` folds content-keyed over the egress, mirroring `data:gridded/store#STORE` `TensorReceipt`, keyed by exactly one runtime `ContentIdentity` and wired through `ReceiptContributor`; the absorbed virtual owner mints the same `FieldReceipt` family downward, one receipt family for the labelled plane. This is the labelled-field counterpart of the dense chunk-grid `data:gridded/store#STORE` store — a distinct owner composing the existing Zarr egress and runtime content key, binding `netcdf4`/`h5netcdf`, never a second labelled-array store inside `store`. `xarray` is on `banned-module-level-imports`, so every `xarray` call binds function-local under `# noqa: PLC0415`; `netcdf4` is an UNGATED Forge source build importing module-top, while `flox` (pure-python core, the former `<3.15` band narrowed on wheel evidence — only the `numba`/`numbagg` `ReductionEngine` rows stay gated on the numba cp315 activation) binds its `flox.xarray.xarray_reduce`/`flox.groupby_scan` lowering function-local under `# noqa: PLC0415` because `flox.xarray` touches the banned-module-level `xarray`, gated by the `policy.vectorizable` probe (the `flox` install conjoined with a string `func` or an `Aggregation` object) and falling back to the catalogued bare-`xarray` reduction only where `flox` is absent — there is no subprocess seam, and the resample grouper `xarray.groupers.TimeResampler` is an `xarray` symbol `xarray_reduce` accepts positionally, never a `flox` export.

## [01]-[INDEX]

- [01]-[FIELD]: the `FieldDataset` owner over the `FieldEngine` reader-engine axis — the CF open/read/write entrypoint over `netcdf4`/`h5netcdf`/`zarr` engines binding `xarray` function-local.
- [02]-[SELECT]: the `FieldSelection` CF-aware coordinate-selection and `ReductionPolicy`-threaded grouped/resampled-reduction-and-scan axis — label-indexed `sel`/`isel`/`interp`, flox-vectorized `groupby`/`groupby_bins`/`resample`, and the grouped cumulative `scan` (`cumsum`/`ffill`/`bfill`) folded by `match`/`case` and `assert_never` through one reduction lowering, with `flox.xarray.xarray_reduce` the vectorized reduce and `flox.groupby_scan` the vectorized scan under the `policy.vectorizable` probe, `xarray.groupers.TimeResampler` the resample grouper, one `Reduction` literal that IS the registered `flox` `func` superset (`std`/`var`/`prod` included), and the single `_FALLBACK_CALL` grouper row plus the `_NAN_BASE` nan-strip the bare-`xarray` fallback for the flox-absent install band alone.
- [03]-[EGRESS]: the `FieldReceipt` fold materializing to the content-keyed `pyarrow`/Zarr egress, keyed by one runtime `ContentIdentity`, wired through `ReceiptContributor`.

## [02]-[FIELD]

- Owner: `FieldDataset` — one frozen CF field owner carrying the source `ResourceRef`, the `FieldEngine` reader engine, and the CF dimension/coordinate/unit metadata recovered at open; `FieldEngine` the `StrEnum` whose member value is the `xarray` engine string (`netcdf4`/`h5netcdf`/`zarr`) and whose `open`/`write` delegate selects the IO surface that engine owns. The engine is recovered from the source shape (the `.nc`/`.h5`/`.zarr` suffix), never a parallel `open_netcdf`/`open_hdf`/`open_zarr` reader family.
- Cases: `FieldEngine` rows `NETCDF4` (`open_dataset(engine="netcdf4")` / `to_netcdf(engine="netcdf4")` over the Unidata netCDF-4 C library, the CF-compliant time/compound/group container) · `H5NETCDF` (`open_dataset(engine="h5netcdf")` / `to_netcdf(engine="h5netcdf")` over the pure-`h5py` HDF5 backend, the netCDF-4-as-HDF5 read path) · `ZARR` (`open_zarr` / `to_zarr` over the chunked Zarr store, the cloud-native CF carrier sharing the `gridded/store` Zarr chunk grid), matched by `match`/`case`, each binding the `xarray` IO surface that engine owns.
- Entry: `FieldEngine.from_ref` recovers the engine from the `ResourceRef` suffix; `FieldDataset.open` admits a `ResourceRef`, recovers the engine, opens the CF dataset (decoding CF metadata, `chunks="auto"` opting into the lazy dask path), and folds the recovered dimension/coordinate/unit metadata into the frozen owner returned in a `RuntimeRail`; `FieldDataset.read` re-opens the live `xarray.Dataset` for a selection or egress operation; `FieldDataset.write` emits the live dataset through the engine `write` delegate keyed by `ContentIdentity`. One `open`/`read`/`write` entrypoint owns all modalities by input shape, never a per-engine reader family.
- Packages: `xarray` (`open_dataset(engine=, chunks=)`/`open_zarr`/`to_netcdf(engine=, encoding=)`/`to_zarr`/`Dataset.dims`/`Dataset.coords`/`Dataset.attrs`/`Dataset.variables`, banned-module-level, function-local), `netcdf4` (the `netcdf4` engine plus the mined write surface — `createVariable(zlib=, complevel=, shuffle=, fletcher32=, chunksizes=, least_significant_digit=, significant_digits=, quantize_mode=, fill_value=)` compression/quantization knobs threaded as the `to_netcdf` `encoding` map keyed by the `xarray` netcdf4-backend encoding-key vocabulary, `date2num`/`num2date` CF time, and the `__has_quantization_support__`/`__has_blosc_support__`/`__has_zstandard_support__` module-level build-capability flags gating the non-`zlib` quantization and compressor rows, the first consumer of the admitted-but-unconsumed dist, module-top), `h5py` (the `h5netcdf` HDF5 engine backend; the native virtual-dataset surface is the absorbed `data:gridded/virtual#VIRTUAL` owner, module-top), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `FieldDataset.open` factory so a caller `ResourceRef` argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `runtime/faults#FAULT` `CLASSIFY` `api` row folds onto the rail at the caller's enclosing fence, the shared `FAULT_CONF` the sibling `gridded/store#STORE` admission seams bind; `read` over the owner's own held engine carries no decorator), runtime (`ResourceRef`/`ContentIdentity`/`ContentKey`/`RuntimeRail`/`boundary`/`FAULT_CONF` the shared beartype violation-redirect config/`ReceiptContributor`).
- Growth: a new CF engine (`pydap`/`scipy`/`zarr` v2) is one `FieldEngine` member carrying its `open`/`write` delegate; a new CF metadata facet (CRS/grid-mapping/cell-methods) is one field on `FieldDataset`; a new compression/quantization knob is one field on `FieldEncoding`; zero new surface.
- Boundary: no compute-package numeric trio (NumPy/SciPy labelled-array compute is `compute`), no production field session, no durable product store; `data` emits a portable content-addressed CF field cube and its selection/reduction plan, not a runtime compute graph. A second labelled-array store inside `store` (the `gridded/store#STORE` Growth forbids it), a parallel `NetcdfDataset`/`HdfDataset`/`ZarrDataset` family, an `engine=` flag set, a module-top `xarray` import on this banned-module-level page, an undecorated `FieldDataset.open` admitting a caller `ResourceRef` without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling `gridded/store#STORE` admission factories share, an unencoded `h5netcdf` write that drops the cube's compression where `for_vars(quantize=False)` threads the shared band, a netCDF-C-only `quantize_mode`/`significant_digits` key passed to the h5netcdf backend that rejects it, a module-level mutable `dict` dispatch/correspondence table where the `expression` `Final[Map[...]]` rail owns the row, and a hand-decoded CF time string are the deleted forms.

```python signature
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from beartype import beartype
from msgspec import Struct
from msgspec.structs import asdict

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Callable

    import xarray as xr


type FieldDims = tuple[str, ...]
type FieldCoords = tuple[str, ...]
type QuantizeMode = Literal["BitGroom", "BitRound", "GranularBitRound"]


# the netcdf4-only encoding keys the h5netcdf backend rejects: lossy quantization rides the netCDF-C
# `createVariable(least_significant_digit=, significant_digits=, quantize_mode=)` surface alone, so a
# `for_vars(quantize=False)` projection strips them and the shared compression band threads to both.
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
        return boundary("field.open", lambda: _open(ref, FieldEngine.from_ref(ref)))

    def read(self) -> "RuntimeRail[xr.Dataset]":
        return boundary("field.read", self.engine.open(str(self.ref.path)))

    def write(self, dataset: "xr.Dataset", target: ResourceRef, encoding: FieldEncoding = FieldEncoding()) -> "RuntimeRail[FieldReceipt]":
        return boundary("field.write", lambda: _write(self, dataset, target, encoding)).bind(lambda railed: railed)


def _open(ref: ResourceRef, engine: FieldEngine) -> FieldDataset:
    dataset = engine.open(str(ref.path))()
    units = {name: str(var.attrs.get("units", "")) for name, var in dataset.variables.items()}
    return FieldDataset(ref=ref, engine=engine, dims=tuple(dataset.dims), coords=tuple(dataset.coords), units=units)
```

## [03]-[SELECT]

- Owner: `FieldSelection` — one closed `@tagged_union` CF-selection/reduction-and-scan axis; the variants are cases the `apply` fold dispatches by `match`/`case` closed with `assert_never`, never a `sel`/`isel`/`groupby`/`resample` sibling-method family on `FieldDataset`. The selection runs against the live `xarray.Dataset` the owner re-opens and returns a transformed `xarray.Dataset`, keeping label-vs-position, point-vs-group, and reduce-vs-scan as one axis. `ReductionPolicy` is the one frozen reduction-behaviour carrier every grouped/binned/resampled/scanned arm threads — one `Reduction` literal that IS the registered `flox.aggregations` `func` superset (`all`/`any`/`count`/`sum`/`prod`/`mean`/`std`/`var`/`max`/`min`/`argmax`/`argmin`/`quantile`/`median`/`mode`/`first`/`last` and their `nan` forms, with `std`/`var`/`prod` first-class flox aggregations the kernel answers, never a separate `BareReduction` set), the `flox.Aggregation` custom-reduce escape on the `ReductionFunc` `func` slot and the `flox.Scan` custom-scan escape on the `ScanRail` `scan` slot (the reduce and scan slots are distinct because `xarray_reduce` and `groupby_scan` are distinct kernels), its parallelization `method` (`map-reduce`/`blockwise`/`cohorts`), vectorization `engine` (`flox`/`numpy`/`numba`/`numbagg`), declared `expected_groups`, the `ReindexStrategy(blockwise, array_type)` combine-reindex policy (the `flox` replacement for the bare-`bool` reindex, `array_type` selecting dense `NUMPY` against `SPARSE_COO` for high-cardinality groups), `skipna`, `min_count` sparse floor, `fill_value`, `sort`/`keep_attrs` combine knobs, resample `freq`, and bin-flag collapsed onto one policy value whose `kwargs` projection feeds the `flox.xarray.xarray_reduce` keyword set under the `policy.vectorizable` probe — the `_HAS_FLOX` install probe conjoined with a string `func` (any registered aggregation, including `std`/`var`/`prod`) or an `Aggregation`/`Scan` object — never a per-reduction case and never a bare reduction-name argument. The only fallback is the `flox`-absent install band: one `_FALLBACK_CALL` grouper row (`group`/`bins`/`resample`) carries one `xarray` `groupby`/`groupby_bins`/`resample` call each, the reduction-name then applied through the `_NAN_BASE` map that strips the `nan` prefix off the closed `Reduction` literal's own members (derived by comprehension over the whole literal, never a hand-listed dict that omits a member and never a stringly-typed `getattr(grouper, name.removeprefix("nan"))`), since bare `xarray` reduces through `.mean(skipna=)` rather than a `.nanmean()` member. The `skipna`/`min_count` pair threads only for the `_SKIPNA_BASE` set the bare reduction accepts — `count`/`all`/`any`/`argmax`/`argmin`/`first`/`last` reject it — and `mode`/`nanmode` is the one `flox`-registered aggregation with no `xarray` grouper member, so `policy.base` maps it to `reduce` (the slot the bare grouper rejects) and the `flox`-absent band raises on `mode` explicitly rather than calling a phantom `grouped.mode`; the band is total over every reduction the bare grouper natively exposes, with `mode` the documented flox-only narrowing rather than a silently-broken arm. The scan arm mirrors the reduction nan-strip through `_SCAN_BASE` derived by comprehension over the closed `ScanFunc` literal, so the bare-`xarray` fallback lowers `nancumsum` to the `cumsum(skipna=)` member and `ffill`/`bfill` to their nan-free members rather than calling a phantom `grouped.nancumsum`.
- Cases: `FieldSelection` rows `Sel(indexers, method, tolerance)` (label-indexed `Dataset.sel`, the CF-coordinate label selection — a `time`/`lat`/`lon` label dict, `method="nearest"` the inexact-match policy) · `Isel(indexers)` (positional `Dataset.isel`, integer-axis slicing) · `Interp(coords, method)` (coordinate interpolation `Dataset.interp`, the `"linear"`/`"cubic"` regridding onto new label positions) · `GroupBy(by, policy)` (grouping over one or more coordinates lowered through the one `_reduce(dataset, by, dim, policy, grouper)` lowering) · `GroupByBins(by, bins, policy)` (the same lowering with `isbin=True` and `expected_groups` carrying the bin edges, the binned-coordinate aggregation) · `Resample(indexer, policy)` (the CF-time frequency resample — `{"time": "1D"}` — lowered through the same `_reduce` path with the resample frequency carried on the policy, the `xarray.groupers.TimeResampler(freq)` grouper passed positionally to `xarray_reduce` on the vectorized arm, never a second reduction code path) · `Scan(by, policy)` (the grouped cumulative scan — `cumsum`/`nancumsum`/`ffill`/`bfill` — the one running-total/forward-fill modality the reduce family cannot express, lowered through the raw-array `flox.groupby_scan` lifted onto the cube by `xr.apply_ufunc` on the vectorized arm and the bare-`xarray` `groupby(...).cumsum()`/`.ffill()` fallback, returning one value per input element rather than one per group), matched by `match`/`case`, each binding the lowering that owns it.
- Entry: `apply(selection, dataset)` runs one `FieldSelection` against the live `xarray.Dataset` and returns a `RuntimeRail[xr.Dataset]`; the grouped, binned, and resampled arms route through the one `_reduce(dataset, by, dim, policy, grouper)` lowering and the scan arm through the one `_scan(dataset, by, policy, grouper)` lowering, both keyed by the `ReductionPolicy`, so a `mean`/`sum`/`std`/`cumsum` method family on the selection is collapsed to one frozen policy value and the grouped-versus-resampled-versus-scan split is the `by`/`dim`/`isbin` policy data plus the `_FALLBACK_CALL` grouper tag alone, never four reduction code paths.
- Receipt: the selection itself emits no receipt — it transforms the live dataset; the receipt folds once at egress through `[4]-[EGRESS]` `FieldReceipt`, never a per-selection rail.
- Growth: a new CF-selection intent is one `FieldSelection` case; a new flox reduction is one `Reduction` literal member and the `_NAN_BASE` fallback map derives it automatically through the `nan`-prefix comprehension with zero map edit; a custom reduction is one `flox.Aggregation` object on the `ReductionPolicy.func` slot and a custom scan one `flox.Scan` object on the distinct `ReductionPolicy.scan` slot; a new parallelization strategy or aggregation engine is one `ReductionMethod`/`ReductionEngine` literal member on `ReductionPolicy`; a new reindex intermediate is one `ReindexArrayType` member on the `ReductionPolicy.reindex` policy; a new scan func is one `ScanFunc` literal member the `_SCAN_BASE` fallback map derives automatically through the same `nan`-prefix comprehension; a new inexact-match policy is one field on the `Sel` case; zero new surface and never a `sel_*`/`group_*` method family.
- Boundary: no compute numeric kernel, no durable selection store; a `sel`/`isel`/`groupby`/`resample`/`cumsum` sibling-method family on `FieldDataset`, a per-reduction case, a bare reduction-name argument, two reduction lowering paths for the grouped versus resampled arm, a `FloxReduction`/`BareReduction` split routing `std`/`var`/`prod` to a slow bare path where flox registers them as `func` values, a stringly-typed `getattr(grouper, name.removeprefix("nan"))` reduction dispatch, a hand-listed `_BARE_REDUCE` dict that omits superset members, a `flox`-imported `TimeResampler` where the symbol lives in `xarray.groupers`, an erased `func: object`/`fill_value: object` slot where a precise `Reduction`/`float` type holds, a bare-`bool` `reindex` where `flox.ReindexStrategy` carries the dense/sparse intermediate, a scan smuggled onto `xarray_reduce` where `groupby_scan` is the one cumulative entrypoint, a `Dataset.map(lambda da: groupby_scan(da, *by, ...))` that hands the raw-array kernel a `DataArray` and coordinate-name `by` arguments and expects a `DataArray` back where `apply_ufunc` is the correct raw-kernel lift, an unconditional `skipna`/`min_count` on a bare `count`/`all`/`any`/`argmax`/`first` reduction that rejects the pair, a `mode` fallback calling a phantom `grouped.mode` where `mode` is flox-only and the band raises, a scan fallback calling a phantom `grouped.nancumsum` where `_SCAN_BASE` strips the `nan` prefix to `cumsum(skipna=)`, a `flox.Scan` custom-scan smuggled onto the `func` reduce slot where the distinct `scan` slot carries it, a `ScanFunc`-only `scan` slot that cannot carry the `flox.Scan` escape, a `policy.__dict__` splat over a slotted frozen `Struct`, the slow bare-`groupby` reduction path where `flox` vectorizes, and a positional/label flag are the deleted forms.

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


# the full registered flox.aggregations func superset: std/var/prod (and nan forms) ARE
# first-class flox aggregations, so there is one Reduction set, never a bare-vs-flox split.
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
# the nan-prefix strip derived once over each closed scan/reduction literal: the bare-`xarray`
# grouper exposes `cumsum`/`ffill`/`bfill` and `mean(skipna=)`, never a `nan`-prefixed member,
# so the flox-only nan forms lower to the base member threading `skipna` on the fallback band.
_NAN_BASE: "Final[Map[str, str]]" = Map.of_seq((name, name.removeprefix("nan")) for name in Reduction.__value__.__args__)
_SCAN_BASE: "Final[Map[str, str]]" = Map.of_seq((name, name.removeprefix("nan")) for name in ScanFunc.__value__.__args__)
# the skipna/min_count-bearing bare reductions: count/all/any/argmax/argmin/first/last reject the
# pair, so the bare path threads it only for this set. `mode` has NO xarray grouper member and is
# flox-only — `policy.base` maps it to `reduce` so the flox-absent band raises explicitly here.
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
        # `mode`/`nanmode` are flox-only — no bare `xarray` grouper member — so they map to the
        # `reduce` slot the bare grouper rejects, making the flox-absent band raise explicitly
        # rather than calling a phantom `grouped.mode`.
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

        # `groupby_scan` is the raw-array kernel (`np.ndarray`/`DaskArray` in and out, raw `by` code
        # arrays, no xarray-aware mirror), so `apply_ufunc` lifts it onto the labelled cube over the
        # `by[0]` scan dim: the data and each group-code coord enter as core-dim raw arrays and the
        # one-value-per-element result re-wraps with the cube's coords — never `Dataset.map`, whose
        # mapper must return a `DataArray` the bare-ndarray kernel does not.
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

- Owner: `FieldReceipt` — one content-keyed typed field receipt mirroring `data:gridded/store#STORE` `TensorReceipt` (the `engine` slot the closed `FieldEngine | EgressTag` family exactly as `TensorReceipt.backend` is the typed `TensorBackend` enum, never a bare `str`), keyed by exactly one runtime `ContentIdentity` and wired through `ReceiptContributor`; `_write` the egress fold that emits the live `xarray.Dataset` through the `FieldEngine` `write` delegate threaded with the `FieldEncoding` policy (CF `to_netcdf(encoding=)`/`to_zarr`) and `to_arrow` the path that lowers a CF table slice into the same `pyarrow.Table` the `columnar`/`query` owners consume paired with a keyed `FieldReceipt`. The egress reuses the engine `write` delegate the owner already holds, and the absorbed `data:gridded/virtual#VIRTUAL` manifest owner imports and mints this same `FieldReceipt` family downward, never a second per-format or per-arm writer family.
- Entry: `FieldDataset.write` emits the live dataset through `engine.write` threaded with the `FieldEncoding` policy and keyed by `ContentIdentity` over the written bytes, folding one `FieldReceipt` over engine/dims/variable-count/byte size; `FieldReceipt.to_arrow` lowers the flattened dataset (`Dataset.to_dataframe().reset_index()` to a `pyarrow.Table` through `Table.from_pandas`) and returns the `pyarrow.Table` paired with a keyed `FieldReceipt` carrying the `engine="arrow"` row, so a CF field slice round-trips into the content-keyed `tabular/columnar#SCAN` egress with the canonical `ContentIdentity.of` retained, never computed-and-discarded.
- Auto: the egress byte path is the `FieldEngine` `write` delegate, so a Zarr field cube shares the `gridded/store` Zarr chunk grid and a netCDF cube rides the CF C-library writer threaded with the `FieldEncoding` quantization map — one delegate call, never a `write_netcdf`/`write_zarr` branch family; the content key derives from the written bytes through exactly one canonical `ContentIdentity.of` over the egress payload (the Zarr v3 store directory hashed through its `zarr.json` root-metadata bytes — `zarr_format=3` is the default the `gridded/store` Zarr grid writes, never the v2 `.zmetadata` consolidated file — the netCDF file hashed through its on-disk bytes); the Arrow lowering keys by `ContentIdentity.of` over the Arrow record-batch bytes and threads that key onto the returned `FieldReceipt` so the field slice carries the same content identity the `columnar` egress expects; `ContentIdentity.of` returns `RuntimeRail[ContentKey]` (the railed digest the `runtime/evidence/identity#IDENTITY` owner mints, not a bare key), so every receipt closes its `content_key` inside the `ContentIdentity.of(...).map(lambda key: FieldReceipt(..., content_key=key))` thread exactly as the `tabular/columnar#SCAN` `QueryReceipt.railed` fold does — never a `content_key=ContentIdentity.of(...)` direct assign of a rail into the `ContentKey` slot — and the write path's `boundary(...).bind(...)` flattens the resulting nested rail; the Arrow `boundary("field.to_arrow", ...)` fences the `to_dataframe`/`from_pandas` lowering then `.bind`s the `ContentIdentity.of` content-key rail onto the `(table, FieldReceipt)` pair; `xarray` is banned-module-level so the Arrow lowering binds `xarray`/`pyarrow` function-local.
- Receipt: the egress folds exactly one `FieldReceipt` and contributes an emitted-phase `Receipt.of` row through `ReceiptContributor`, never a generic receipt or a parallel rail; this is the one receipt family for the labelled plane, shared by the `[1]-[FIELD]` write path and the `data:gridded/virtual#VIRTUAL` absorbed aggregation.
- Packages: `xarray` (`Dataset.to_netcdf(encoding=)`/`to_zarr`/`to_dataframe`/`sizes`/`data_vars`/`nbytes`, banned-module-level, function-local), `pyarrow` (`Table.from_pandas`/`Table.combine_chunks`/`Table.to_batches`/`RecordBatch.serialize`/`bytes(Buffer)` the Arrow lowering target — the content key and byte span read the one `bytes(combine_chunks().to_batches()[0].serialize())` coalesced-batch payload the folder `pyarrow` `.api` names the canonical whole-table source, an empty table keying off the `b""` contract the same row fixes, not an unverified `Table.nbytes` and not a chunk-boundary-unstable per-`to_batches()` stream), `beartype` (`@beartype(conf=FAULT_CONF)` the public domain-admission contract on the `FieldReceipt.to_arrow` Arrow-lowering seam so a caller `xr.Dataset` argument that violates the in-process annotation raises the canonical `BeartypeCallHintViolation` root the `runtime/faults#FAULT` `CLASSIFY` `api` row folds onto the rail, the shared `FAULT_CONF` the sibling `gridded/store#STORE` egress seams bind; `contribute` over the receipt's own produced facts carries no decorator), runtime (`ContentIdentity`/`ContentKey`/`RuntimeRail`/`boundary`/`FAULT_CONF` the shared beartype violation-redirect config/`Receipt`/`ReceiptContributor`).
- Growth: a new egress engine is one `FieldEngine` `write` delegate; the Arrow lowering target is the existing `columnar` egress with zero change here; a new receipt fact is one entry on the `FieldReceipt` fact dict; zero new surface.
- Boundary: composes the `gridded/store#STORE` Zarr egress and the `tabular/columnar#SCAN` Arrow egress, never re-minting either; a per-format writer family, a parallel `FieldEgress` tagged union restating the engine tag, a generic receipt abstraction, a `content_key=ContentIdentity.of(...)` direct assign of a `RuntimeRail[ContentKey]` into the `ContentKey` slot where the `.map`/`.bind` thread unwraps the railed key, a four-positional `Receipt.of(phase, owner, subject, facts)` where the owner exposes the two-argument `Receipt.of(owner, (phase, subject, facts))` shape, a single-`Receipt` `contribute` where the `ReceiptContributor` Protocol yields an `Iterable[Receipt]`, a `str()`-coerced fact map where the receipts `Encoder(enc_hook=repr)` carries native `int`, an undecorated `to_arrow` admitting a caller `xr.Dataset` without the `@beartype(conf=FAULT_CONF)` public-seam contract the sibling `gridded/store#STORE` egress factories share, a bare-`str` `FieldReceipt.engine` slot where the closed `FieldEngine | EgressTag` family discriminates the three real engines from the two egress-source tags, a chunk-boundary-unstable per-`to_batches()` Arrow content-key stream where the catalogued `combine_chunks().to_batches()[0].serialize()` coalesced-batch payload is the canonical chunk-stable whole-table source, a phantom `Table.nbytes`/`allocate_buffer` member absent from the folder `pyarrow` `.api`, and a second `ContentIdentity` mint are the deleted forms.

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


# the two egress-source tags beside the three real module-scoped `FieldEngine` members: the receipt
# engine slot is the closed `FieldEngine | EgressTag` family, never a bare `str`.
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
    # `combine_chunks().to_batches()[0]` coalesces every chunk into one contiguous batch so the
    # content key folds a chunk-boundary-stable byte span — the canonical whole-table source, never
    # a per-`to_batches()` digest whose partition drifts across xarray/pandas versions for one cube;
    # an empty table keys off `b""` per the catalogue contract.
    payload = bytes(table.combine_chunks().to_batches()[0].serialize()) if table.num_rows else b""
    return table, tuple(dataset.sizes), len(dataset.data_vars), payload


def _arrow_receipt(table: "pa.Table", dims: tuple[str, ...], variables: int, payload: bytes) -> "RuntimeRail[tuple[pa.Table, FieldReceipt]]":
    return ContentIdentity.of("field.arrow", payload).map(
        lambda key: (table, FieldReceipt(engine="arrow", dims=dims, variables=variables, bytes_stored=len(payload), content_key=key))
    )
```
