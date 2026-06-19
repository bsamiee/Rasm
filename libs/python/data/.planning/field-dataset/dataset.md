# [PY_DATA_FIELD_DATASET]

The CF-conventioned labelled N-D field-dataset owner: one polymorphic owner over `xarray` reading and writing CF-metadata field cubes through a `FieldEngine` reader-engine axis, with CF-aware coordinate selection, label-indexed slicing, grouped and resampled reductions, and unit/coordinate-reference metadata, materializing to the content-keyed `pyarrow`/Zarr egress the `data:columnar/dataset#SCAN` and `data:tensor/store#TENSOR` owners already speak. `FieldDataset` is the one frozen field owner; `FieldEngine` the `StrEnum` whose member value IS the `xarray` engine string and whose `open`/`write` delegate selects `open_dataset(engine=)`/`open_zarr` against `to_netcdf(engine=)`/`to_zarr` — config as a domain value carrying behaviour, never an `engine=` flag set. `FieldSelection` is the one closed `@tagged_union` CF-selection/reduction axis (`Sel`/`Isel`/`Interp`/`GroupBy`/`GroupByBins`/`Resample` folded by `match`/`case` closed with `assert_never`), the variants are cases not sibling methods. `FieldReceipt` folds content-keyed over the egress, mirroring `data:tensor/store#TENSOR` `TensorReceipt`, keyed by exactly one runtime `ContentIdentity` and wired through `ReceiptContributor`. This is the labelled-field counterpart of the dense chunk-grid `data:tensor/store#TENSOR` store — a distinct owner composing the existing Zarr egress and runtime content key, binding the admitted-but-unconsumed `netcdf4`, never a second labelled-array store inside `tensor`. `xarray` is on `banned-module-level-imports`, so every `xarray` call binds function-local under `# noqa: PLC0415`; there is no gated dependency on this path and no subprocess seam — the function-local import is the only floor discipline.

## [1]-[INDEX]

- `[2]-[FIELD]`: the `FieldDataset` owner over the `FieldEngine` reader-engine axis — the CF open/read/write entrypoint over `netcdf4`/`h5netcdf`/`zarr` engines binding `xarray` function-local.
- `[3]-[SELECT]`: the `FieldSelection` CF-aware coordinate-selection and grouped/resampled-reduction axis — label-indexed `sel`/`isel`/`interp`/`groupby`/`groupby_bins`/`resample` folded by `match`/`case` and `assert_never`.
- `[4]-[EGRESS]`: the `FieldReceipt` fold materializing to the content-keyed `pyarrow`/Zarr egress, keyed by one runtime `ContentIdentity`, wired through `ReceiptContributor`.
- `[5]-[RESEARCH]`: the open keyword-arity spellings (`combine_attrs`/`decode_cf`) pending live-distribution confirmation, and the `xarray` banned-module-level note.

## [2]-[FIELD]

- Owner: `FieldDataset` — one frozen CF field owner carrying the source `ResourceRef`, the `FieldEngine` reader engine, and the CF dimension/coordinate/unit metadata recovered at open; `FieldEngine` the `StrEnum` whose member value is the `xarray` engine string (`netcdf4`/`h5netcdf`/`zarr`) and whose `open`/`write` delegate selects the IO surface that engine owns. The engine is recovered from the source shape (the `.nc`/`.h5`/`.zarr` suffix), never a parallel `open_netcdf`/`open_hdf`/`open_zarr` reader family.
- Cases: `FieldEngine` rows `NETCDF4` (`open_dataset(engine="netcdf4")` / `to_netcdf(engine="netcdf4")` over the Unidata netCDF-4 C library, the CF-compliant time/compound/group container) · `H5NETCDF` (`open_dataset(engine="h5netcdf")` / `to_netcdf(engine="h5netcdf")` over the pure-`h5py` HDF5 backend, the netCDF-4-as-HDF5 read path) · `ZARR` (`open_zarr` / `to_zarr` over the chunked Zarr store, the cloud-native CF carrier sharing the `tensor/store` Zarr chunk grid), matched by `match`/`case`, each binding the `xarray` IO surface that engine owns.
- Entry: `FieldEngine.from_ref` recovers the engine from the `ResourceRef` suffix; `FieldDataset.open` admits a `ResourceRef`, recovers the engine, opens the CF dataset (decoding CF metadata, `chunks="auto"` opting into the lazy dask path), and folds the recovered dimension/coordinate/unit metadata into the frozen owner returned in a `RuntimeRail`; `FieldDataset.read` re-opens the live `xarray.Dataset` for a selection or egress operation; `FieldDataset.write` emits the live dataset through the engine `write` delegate keyed by `ContentIdentity`. One `open`/`read`/`write` entrypoint owns all modalities by input shape, never a per-engine reader family.
- Auto: the `FieldEngine` member value IS the `xarray` `engine=` string, so `open` is one `engine.open(path)` delegate call and `write` one `engine.write(dataset, path)` delegate call — the conflated `engine=` flag set is the deleted form; `netcdf4`/`h5py` are UNGATED Forge source-builds and `zarr` is cp315-clean, so the gate is `xarray`'s banned-module-level import alone, bound function-local under `# noqa: PLC0415` exactly as the `tensor/store#TENSOR` virtual arm binds its `xarray` combine; CF time/units/calendar decode rides `xarray`'s `decode_cf=True` default through `open_dataset`/`open_zarr`, never a hand-decoded CF time string; the recovered metadata (`dims`/`coords`/`attrs` unit/CRS) folds into the frozen owner so the interior never re-opens for a metadata accessor.
- Packages: `xarray` (`open_dataset(engine=, chunks=)`/`open_zarr`/`to_netcdf(engine=)`/`to_zarr`/`Dataset.dims`/`Dataset.coords`/`Dataset.attrs`, banned-module-level, function-local), `netcdf4` (the `netcdf4` engine — first consumer of the admitted-but-unconsumed dist, the CF-compliant C-library container), `h5py` (the `h5netcdf` HDF5 engine backend), runtime (`ResourceRef`/`ContentIdentity`/`ContentKey`/`RuntimeRail`/`boundary`/`ReceiptContributor`).
- Growth: a new CF engine (`pydap`/`scipy`/`zarr` v2) is one `FieldEngine` member carrying its `open`/`write` delegate; a new CF metadata facet (CRS/grid-mapping/cell-methods) is one field on `FieldDataset`; zero new surface.
- Boundary: no compute-package numeric trio (NumPy/SciPy labelled-array compute is `compute`), no production field session, no durable product store; `data` emits a portable content-addressed CF field cube and its selection/reduction plan, not a runtime compute graph. A second labelled-array store inside `tensor` (the `tensor/store#TENSOR` Growth forbids it), a parallel `NetcdfDataset`/`HdfDataset`/`ZarrDataset` family, an `engine=` flag set, a module-top `xarray` import on this banned-module-level page, and a hand-decoded CF time string are the deleted forms.

```python signature
from enum import StrEnum
from typing import TYPE_CHECKING, assert_never

from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Callable

    import xarray as xr


type FieldDims = tuple[str, ...]
type FieldCoords = tuple[str, ...]


class FieldEngine(StrEnum):
    NETCDF4 = "netcdf4"
    H5NETCDF = "h5netcdf"
    ZARR = "zarr"

    @classmethod
    def from_ref(cls, ref: ResourceRef) -> "FieldEngine":
        suffix = ref.path.suffix.lower()
        match suffix:
            case ".zarr":
                return cls.ZARR
            case ".h5" | ".hdf5":
                return cls.H5NETCDF
            case ".nc" | ".nc4" | ".cdf":
                return cls.NETCDF4
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

    def write(self, dataset: "xr.Dataset", path: str) -> "Callable[[], None]":
        match self:
            case FieldEngine.ZARR:
                return lambda: dataset.to_zarr(path, mode="w")
            case FieldEngine.NETCDF4 | FieldEngine.H5NETCDF:
                return lambda: dataset.to_netcdf(path, engine=self.value)
            case unreachable:
                assert_never(unreachable)


class FieldDataset(Struct, frozen=True):
    ref: ResourceRef
    engine: FieldEngine
    dims: FieldDims
    coords: FieldCoords
    units: dict[str, str]

    @classmethod
    def open(cls, ref: ResourceRef) -> "RuntimeRail[FieldDataset]":
        engine = FieldEngine.from_ref(ref)
        return boundary("field.open", lambda: _open(ref, engine))

    def read(self) -> "RuntimeRail[xr.Dataset]":
        return boundary("field.read", self.engine.open(str(self.ref.path)))

    def write(self, dataset: "xr.Dataset", target: ResourceRef) -> "RuntimeRail[FieldReceipt]":
        return boundary("field.write", lambda: _write(self, dataset, target))


def _open(ref: ResourceRef, engine: FieldEngine) -> FieldDataset:
    dataset = engine.open(str(ref.path))()
    units = {name: str(var.attrs.get("units", "")) for name, var in dataset.variables.items()}
    return FieldDataset(
        ref=ref, engine=engine,
        dims=tuple(dataset.dims), coords=tuple(dataset.coords), units=units,
    )
```

## [3]-[SELECT]

- Owner: `FieldSelection` — one closed `@tagged_union` CF-selection/reduction axis; the variants are cases the `apply` fold dispatches by `match`/`case` closed with `assert_never`, never a `sel`/`isel`/`groupby`/`resample` sibling-method family on `FieldDataset`. The selection runs against the live `xarray.Dataset` the owner re-opens and returns a transformed `xarray.Dataset`, keeping label-vs-position and point-vs-group as one axis.
- Cases: `FieldSelection` rows `Sel(indexers, method, tolerance)` (label-indexed `Dataset.sel`, the CF-coordinate label selection — a `time`/`lat`/`lon` label dict, `method="nearest"` the inexact-match policy) · `Isel(indexers)` (positional `Dataset.isel`, integer-axis slicing) · `Interp(coords, method)` (coordinate interpolation `Dataset.interp`, the `"linear"`/`"cubic"` regridding onto new label positions) · `GroupBy(group, reduction)` (`Dataset.groupby(group)` then the named reduction — `mean`/`sum`/`std`/`max` — over the group) · `GroupByBins(group, bins, reduction)` (`Dataset.groupby_bins(group, bins)` then the reduction, the binned-coordinate aggregation) · `Resample(indexer, reduction)` (`Dataset.resample(**indexer)` then the reduction, the CF-time frequency resample — `{"time": "1D"}`), matched by `match`/`case`, each binding the `xarray` member that owns it.
- Entry: `apply(selection, dataset)` runs one `FieldSelection` against the live `xarray.Dataset` and returns a `RuntimeRail[xr.Dataset]`; the reduction on the grouped/resampled arms is one named-reduction lookup (`_REDUCE`), never a per-reduction case, so a `mean`/`sum`/`std` family on the selection is collapsed to one frozen row.
- Auto: the label-vs-position split is the `Sel`/`Isel` case, never an `exact=`/`positional=` flag; `Sel` carries `method`/`tolerance` so the inexact CF-coordinate match is one knob on the case, not a parallel `sel_nearest` method; the grouped/binned/resampled arms share the one `_REDUCE` named-reduction table so adding a reduction is one frozen-literal row; `xarray` is banned-module-level so the `apply` body binds `xarray`-typed members through the dataset handle the owner already holds, and the `_REDUCE` table calls the bound method on the `GroupBy`/`Resample` accessor the case produces; every arm returns a transformed `xarray.Dataset` the `[4]-[EGRESS]` fold then materializes.
- Receipt: the selection itself emits no receipt — it transforms the live dataset; the receipt folds once at egress through `[4]-[EGRESS]` `FieldReceipt`, never a per-selection rail.
- Packages: `xarray` (`Dataset.sel(method=, tolerance=)`/`isel`/`interp`/`groupby`/`groupby_bins`/`resample` plus the `mean`/`sum`/`std`/`max`/`min` reductions on the grouped accessor), runtime (`RuntimeRail`/`boundary`).
- Growth: a new CF-selection intent is one `FieldSelection` case; a new reduction is one `_REDUCE` row; a new inexact-match policy is one field on the `Sel` case; zero new surface and never a `sel_*`/`group_*` method family.
- Boundary: no compute numeric kernel, no durable selection store; a `sel`/`isel`/`groupby`/`resample` sibling-method family on `FieldDataset`, a per-reduction case, and a positional/label flag are the deleted forms.

```python signature
from typing import TYPE_CHECKING, Literal, assert_never

from expression import case, tag, tagged_union

from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    from collections.abc import Callable

    import xarray as xr


type Reduction = Literal["mean", "sum", "std", "max", "min"]


@tagged_union(frozen=True)
class FieldSelection:
    tag: Literal["sel", "isel", "interp", "groupby", "groupby_bins", "resample"] = tag()
    sel: tuple[dict[str, object], str | None, float | None] = case()
    isel: dict[str, int | slice] = case()
    interp: tuple[dict[str, object], str] = case()
    groupby: tuple[str, Reduction] = case()
    groupby_bins: tuple[str, tuple[float, ...], Reduction] = case()
    resample: tuple[dict[str, str], Reduction] = case()

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
    def GroupBy(group: str, reduction: Reduction = "mean") -> "FieldSelection":
        return FieldSelection(groupby=(group, reduction))

    @staticmethod
    def GroupByBins(group: str, bins: tuple[float, ...], reduction: Reduction = "mean") -> "FieldSelection":
        return FieldSelection(groupby_bins=(group, bins, reduction))

    @staticmethod
    def Resample(indexer: dict[str, str], reduction: Reduction = "mean") -> "FieldSelection":
        return FieldSelection(resample=(indexer, reduction))


def _reduce(accessor: object, reduction: Reduction) -> "xr.Dataset":
    return getattr(accessor, reduction)()


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
        case FieldSelection(tag="groupby", groupby=(group, reduction)):
            return _reduce(dataset.groupby(group), reduction)
        case FieldSelection(tag="groupby_bins", groupby_bins=(group, bins, reduction)):
            return _reduce(dataset.groupby_bins(group, list(bins)), reduction)
        case FieldSelection(tag="resample", resample=(indexer, reduction)):
            return _reduce(dataset.resample(**indexer), reduction)
        case unreachable:
            assert_never(unreachable)
```

## [4]-[EGRESS]

- Owner: `FieldReceipt` — one content-keyed typed field receipt mirroring `data:tensor/store#TENSOR` `TensorReceipt`, keyed by exactly one runtime `ContentIdentity` and wired through `ReceiptContributor`; `_write` the egress fold that emits the live `xarray.Dataset` through the `FieldEngine` `write` delegate (CF `to_netcdf`/`to_zarr`) and the `to_arrow` path that lowers a CF table slice into the same `pyarrow.Table` the `columnar`/`query` owners consume. The egress reuses the engine `write` delegate the owner already holds, never a second per-format writer family.
- Entry: `FieldDataset.write` emits the live dataset through `engine.write` keyed by `ContentIdentity` over the written bytes and folds one `FieldReceipt` over engine/dims/variable-count/byte size; `FieldReceipt.to_arrow` lowers the flattened dataset (`Dataset.to_dataframe().reset_index()` to a `pyarrow.Table`) so a CF field slice round-trips into the content-keyed `columnar/dataset#SCAN` egress, keyed by the same canonical `ContentIdentity.of`.
- Auto: the egress byte path is the `FieldEngine` `write` delegate, so a Zarr field cube shares the `tensor/store` Zarr chunk grid and a netCDF cube rides the CF C-library writer — one delegate call, never a `write_netcdf`/`write_zarr` branch family; the content key derives from the written bytes through exactly one canonical `ContentIdentity.of` over the egress payload (the Zarr v3 store directory hashed through its `zarr.json` root-metadata bytes — `zarr_format=3` is the default the `tensor/store` Zarr grid writes, never the v2 `.zmetadata` consolidated file — the netCDF file hashed through its on-disk bytes); the Arrow lowering keys by `ContentIdentity.of` over the Arrow record-batch bytes so the field slice carries the same content identity the `columnar` egress expects; `xarray` is banned-module-level so the Arrow lowering binds `xarray`/`pyarrow` function-local.
- Receipt: the egress folds exactly one `FieldReceipt` and contributes an emitted-phase `Receipt.of` row through `ReceiptContributor`, never a generic receipt or a parallel rail; this is the one receipt family for the package.
- Packages: `xarray` (`Dataset.to_netcdf`/`to_zarr`/`to_dataframe`/`sizes`/`data_vars`/`nbytes`, banned-module-level, function-local), `pyarrow` (`Table.from_pandas`/`Table.nbytes`/`RecordBatch.serialize` the Arrow lowering target), runtime (`ContentIdentity`/`ContentKey`/`RuntimeRail`/`boundary`/`Receipt`/`ReceiptContributor`).
- Growth: a new egress engine is one `FieldEngine` `write` delegate; the Arrow lowering target is the existing `columnar` egress with zero change here; a new receipt fact is one entry on the `FieldReceipt` fact dict; zero new surface.
- Boundary: composes the `tensor/store#TENSOR` Zarr egress and the `columnar/dataset#SCAN` Arrow egress, never re-minting either; a per-format writer family, a parallel `FieldEgress` tagged union restating the engine tag, a generic receipt abstraction, and a second `ContentIdentity` mint are the deleted forms.

```python signature
from typing import TYPE_CHECKING

from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    import pyarrow as pa
    import xarray as xr


class FieldReceipt(Struct, frozen=True):
    engine: str
    dims: tuple[str, ...]
    variables: int
    bytes_stored: int
    content_key: ContentKey

    def to_arrow(self, dataset: "xr.Dataset") -> "RuntimeRail[pa.Table]":
        return boundary("field.to_arrow", lambda: _to_arrow(dataset))

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted", "field", self.engine,
            {"dims": ",".join(self.dims), "variables": str(self.variables), "stored": str(self.bytes_stored)},
        )


def _write(field: "FieldDataset", dataset: "xr.Dataset", target: ResourceRef) -> FieldReceipt:
    path = str(target.path)
    field.engine.write(dataset, path)()
    payload = (
        ContentIdentity.of("field", target.path.read_bytes())
        if target.path.is_file()
        else ContentIdentity.of("field", (target.path / "zarr.json").read_bytes())
    )
    return FieldReceipt(
        engine=field.engine.value,
        dims=tuple(dataset.sizes),
        variables=len(dataset.data_vars),
        bytes_stored=int(dataset.nbytes),
        content_key=payload,
    )


def _to_arrow(dataset: "xr.Dataset") -> "pa.Table":
    import pyarrow as pa  # noqa: PLC0415

    frame = dataset.to_dataframe().reset_index()
    table = pa.Table.from_pandas(frame)
    _ = ContentIdentity.of("field.arrow", table.to_batches()[0].serialize() if table.num_rows else b"")
    return table
```

## [5]-[RESEARCH]

- [CF_IO_SURFACE]: the `xarray` `open_dataset(engine=, chunks=, decode_cf=)`/`open_zarr(decode_cf=)`/`to_netcdf(engine=)`/`to_zarr(mode=)` IO arity and the `Dataset.sel(method=, tolerance=)`/`isel`/`interp`/`groupby`/`groupby_bins`/`resample` selection/reduction surface the owner transcribes are catalogue-confirmed against the folder `xarray` `.api` (`open_dataset`/`open_zarr`/`to_netcdf`/`to_zarr` L50-57, the grouping family `groupby`/`groupby_bins`/`resample` L37, `sel`/`isel` L34, `interp` L39); the `netcdf4` engine binds the folder `netcdf4` `.api` as its first consumer (the CF-compliant container and `date2num`/`num2date` time decode), and `h5py` backs the `h5netcdf` engine. `xarray` is on `banned-module-level-imports`, so every `xarray` call binds function-local under `# noqa: PLC0415`; `netcdf4`/`h5py` are UNGATED Forge source-builds and `zarr` is cp315-clean, so there is no gated dependency and no subprocess seam on this path — the function-local `xarray` import is the only floor discipline, and a module-top `xarray` import is the floor-violating deleted form.
- [CF_KEYWORD_ARITY]: the inexact-match `Dataset.sel(method="nearest", tolerance=...)` keyword pair, the `Dataset.interp(method="linear"|"cubic")` regridding-method spelling, the `groupby_bins(group, bins)` bin-edge argument shape, and the `resample(**{"time": "1D"})` frequency-indexer spelling confirm against the live `xarray` distribution before the selection arms treat them as settled — the folder `xarray` `.api` enumerates `sel`/`isel`/`interp`/`groupby`/`groupby_bins`/`resample` (L34/37/39) but truncates their keyword arity. The `decode_cf=True` default and its `combine_attrs="drop_conflicts"` companion on any multi-file `open_mfdataset` aggregation (the `xarray` `.api` lists `open_mfdataset` L52 and `combine_by_coords` L66 but truncates the `combine_attrs` value vocabulary) confirm against the live distribution, mirroring the `tensor/store#TENSOR` virtual-arm `combine_attrs` open seam.
- [ARROW_LOWERING]: the `Dataset.to_dataframe().reset_index()` flatten and the `pyarrow.Table.from_pandas` lowering the `_to_arrow` egress uses are stdlib-Arrow/pandas members the catalogue carries (`to_dataframe`/`to_pandas` `xarray` `.api` L41, `pyarrow.Table.from_pandas`); the one open seam is the `Dataset.nbytes` byte-count accessor the `FieldReceipt` reads, confirmed against the live `xarray` distribution before the receipt settles; the directory content-key hashes the Zarr v3 `zarr.json` root metadata (`zarr_format=3` default per the folder `zarr` `.api`), not the v2 `.zmetadata`.
