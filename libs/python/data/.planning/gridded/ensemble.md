# [PY_DATA_ENSEMBLE]

Scenario-tree owner over the CF field plane: one `ScenarioTree` carries a multi-scenario simulation family — design options, climate years, IAM prospective backgrounds — as a `DataTree` hierarchy whose leaves are the `gridded/field#FIELD` CF cubes, so cross-scenario map, reduce, and difference run group-wise in ONE call instead of N hand-looped cubes. The page COMPOSES the CF owner and mints no second labelled-array store: every leaf is an `xr.Dataset` a `FieldDataset` opened or a producer built, group nodes carry structure alone, and egress lands on the shared `FieldReceipt` family under the `tree` tag so scenario sets key, meter, and reside exactly as every field egress does.

The scenario axis is data twice over: `ScenarioKind` is the closed family-kind vocabulary spelling the tree's first path segment, and the leaf name is the open scenario identity a caller mints — a prospective background leaf name is exactly the registered database name the `impact/scenario#SCENARIO` build wrote, so energy and impact result families cross one tree vocabulary. Reduction collapses the family through one concat over a minted `scenario` dimension, which is what makes a per-scenario delta queryable beside the vector-cube and claims planes: the collapsed cube and the per-leaf frames both lower through the standing field Arrow egress.

## [01]-[INDEX]

- [02]-[ENSEMBLE]: the `ScenarioTree` owner — tree construction, the `TreeOp` group-wise operation family, the `TreeResult` union, content-keyed `FieldReceipt` egress.

## [02]-[ENSEMBLE]

- Owner: `ScenarioTree` — one frozen owner carrying the built `DataTree` beside its `ScenarioKind` and leaf roster, so the analyzed family never decouples from its admitted scenarios; `TreeOp` the group-wise operation family (map, verb reduction, quantile reduction, baseline difference) folded under one total `match`; `TreeResult` the closed output union — `tree` for shape-preserving operations, `cube` for the family-collapsing reductions.
- Law: leaves are CF cubes and group nodes are structure — `map_over_datasets` visits every node, so the mapped step guards the empty group dataset and transforms leaves alone; reduction concats the leaf cubes over a minted `scenario` dimension named by leaf identity, so the collapsed cube carries scenario provenance as a coordinate a query selects on, never a positional index.
- Law: `difference` names its baseline by leaf identity and refuses typed on an absent one — a silent empty delta over a mistyped baseline is the wire-damage class the refusal forecloses; the delta tree drops the baseline leaf, because a zero self-delta is a fabricated row no consumer asked for.
- Entry: `ScenarioTree.of(kind, cubes)` builds `/{kind}/{name}` paths through `DataTree.from_dict`; `apply(op)` is the one operation entrypoint returning the `TreeResult` union; `write(target)` lands the whole hierarchy as one Zarr store and mints the page's one `FieldReceipt` under the `tree` tag, keyed off the store's `zarr.json` root-metadata bytes exactly as the field Zarr egress keys.
- Receipt: one `FieldReceipt` per egress on the shared family — `engine="tree"` partitions scenario-set writes apart from the CF engines on the one receipt column, and `contribute` rides the family's own `domain="field"` projection with zero new receipt surface.
- Packages: `xarray` (`DataTree.from_dict`, `map_over_datasets`, `leaves`, `to_zarr`, `concat`), `pandas` (the named `scenario` index the concat dimension rides), `msgspec` (the frozen owner), runtime (`RuntimeRail`/`boundary`/`FaultRow`/`ContentIdentity`/`scoped`), `gridded/field#EGRESS` (`FieldReceipt`, the shared receipt family), `tabular/interop#INTEROP` (`DataLeg`, the folder's one raise-leg roster).
- Growth: a new scenario family kind is one `ScenarioKind` member; a new group-wise operation is one `TreeOp` case plus one `apply` arm; a new reduction verb is one `ReduceVerb` literal; a new receipt fact is one entry on the family's fact dict; a new fenced leg or refusal law is one `FaultRow` row under `DataLeg.ENSEMBLE` in this module's one `RAISES` table; zero new surface.
- Boundary: composes the CF owner and the Zarr surface, never a second labelled-array store, no scenario GENERATION (design-option authoring is compute's, prospective builds are `impact/scenario#SCENARIO`'s), no UQ replicate container — the `gridded/field#ENSEMBLE` `EnsembleCorpus` owns replicate-chunked response matrices, a disjoint concern sharing only the receipt family.

```python signature
from collections.abc import Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Any, Final, Literal, assert_never

import pandas as pd
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from opentelemetry import trace

lazy import xarray as xr

from rasm.data.gridded.field import FieldReceipt
from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, Catch, FaultRow, RuntimeRail, boundary, rostered, scoped
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Callable

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.gridded.ensemble")

type ReduceVerb = Literal["mean", "std", "min", "max", "median", "sum"]


def _tree_raises() -> Catch:
    return (xr.InvalidTreeError, KeyError, TypeError, ValueError)


def _store_raises() -> Catch:
    return (*_tree_raises(), IndexError, OSError)


TREE_BUILD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.ENSEMBLE, point="build", arm="boundary", defect="tree-build", retriability=TERMINAL
)
TREE_APPLY: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.ENSEMBLE, point="apply", arm="boundary", defect="group-fold", retriability=TERMINAL
)
TREE_BASELINE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.ENSEMBLE, point="baseline", arm="config", defect="absent-baseline", retriability=TERMINAL, slots=("scenario",)
)
TREE_WRITE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.ENSEMBLE, point="write", arm="boundary", defect="tree-write", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([TREE_BUILD, TREE_APPLY, TREE_BASELINE, TREE_WRITE]))


class ScenarioKind(StrEnum):
    OPTION = "option"
    CLIMATE = "climate"
    BACKGROUND = "background"


@tagged_union(frozen=True)
class TreeOp:
    tag: Literal["mapped", "reduced", "quantile", "difference"] = tag()
    mapped: "Callable[[xr.Dataset], xr.Dataset]" = case()
    reduced: ReduceVerb = case()
    quantile: float = case()
    difference: str = case()


@tagged_union(frozen=True)
class TreeResult:
    tag: Literal["tree", "cube"] = tag()
    tree: "ScenarioTree" = case()
    cube: Any = case()


class ScenarioTree(Struct, frozen=True):
    tree: Any
    kind: ScenarioKind
    scenarios: tuple[str, ...]

    @classmethod
    def of(cls, kind: ScenarioKind, cubes: "dict[str, xr.Dataset]") -> "RuntimeRail[ScenarioTree]":
        def build() -> "ScenarioTree":
            tree = xr.DataTree.from_dict({f"/{kind.value}/{name}": cube for name, cube in cubes.items()})
            return cls(tree=tree, kind=kind, scenarios=tuple(sorted(cubes)))

        return boundary(TREE_BUILD, build, catch=_tree_raises())

    def apply(self, op: TreeOp) -> "RuntimeRail[TreeResult]":
        with _TRACER.start_as_current_span(
            f"ensemble.{op.tag}", attributes={"rasm.field.kind": self.kind.value, "rasm.field.scenarios": len(self.scenarios)}
        ):
            return boundary(TREE_APPLY, lambda: self._apply(op), catch=_tree_raises()).bind(lambda railed: railed)

    def _apply(self, op: TreeOp) -> "RuntimeRail[TreeResult]":
        match op:
            case TreeOp(tag="mapped", mapped=step):
                mapped = self.tree.map_over_datasets(lambda ds: step(ds) if ds else ds)
                return Ok(TreeResult(tree=ScenarioTree(tree=mapped, kind=self.kind, scenarios=self.scenarios)))
            case TreeOp(tag="reduced", reduced=verb):
                return Ok(TreeResult(cube=getattr(self._stacked(), verb)("scenario")))
            case TreeOp(tag="quantile", quantile=q):
                return Ok(TreeResult(cube=self._stacked().quantile(q, dim="scenario")))
            case TreeOp(tag="difference", difference=baseline) if baseline not in self.scenarios:
                return Error(TREE_BASELINE.raised(baseline))
            case TreeOp(tag="difference", difference=baseline):
                base = self.tree[f"/{self.kind.value}/{baseline}"].dataset
                deltas = xr.DataTree.from_dict({
                    node.path: node.dataset - base for node in self.tree.leaves if node.path.split("/")[-1] != baseline
                })
                survivors = tuple(name for name in self.scenarios if name != baseline)
                return Ok(TreeResult(tree=ScenarioTree(tree=deltas, kind=self.kind, scenarios=survivors)))
            case unreachable:
                assert_never(unreachable)

    def _stacked(self) -> Any:
        return xr.concat([node.dataset for node in self.tree.leaves], dim=pd.Index(self.scenarios, name="scenario"))

    def write(self, target: ResourceRef) -> "RuntimeRail[FieldReceipt]":
        def emit() -> "RuntimeRail[FieldReceipt]":
            self.tree.to_zarr(str(target.path))
            source = (target.path / "zarr.json").read_bytes()
            return ContentIdentity.of("field", source).map(
                lambda key: FieldReceipt(
                    engine="tree",
                    dims=(self.kind.value, "scenario"),
                    variables=sum(len(node.dataset.data_vars) for node in self.tree.leaves),
                    bytes_stored=sum(int(node.dataset.nbytes) for node in self.tree.leaves),
                    content_key=key,
                )
            )

        with _TRACER.start_as_current_span("ensemble.write", attributes={"rasm.field.kind": self.kind.value}):
            return boundary(TREE_WRITE, emit, catch=_store_raises()).bind(lambda rail: rail)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
