# [PY_DATA_INVENTORY]

Brightway project and inventory custodian — the system-of-record leg of the impact plane: `Inventory` owns the `bw2data` project scope, the biosphere/LCIA bootstrap, and the whole LCI ingestion pipeline `bw2io` spells (`extract → apply_strategies → statistics → resolve → write_database`), and `MatrixPackage` owns the `bw_processing` matrix-datapackage substrate the solver consumes. Every operation is project-pinned: the owner carries its project name and every boundary leg re-selects it first, so an ambient current-project global never decides which store an import lands in — two same-process compositions with distinct projects cannot cross-write.

The pipeline returns `IngestResult` with `statistics()`'s `(nodes, edges, unlinked, multifunctional)` measurements beside the written database and source key. The residual unlinked set resolves through one closed `Resolution` policy row — match another database, promote to biosphere, or refuse — and `drop_unlinked`'s reckless erasure is the named rejected form. The written `bw2data.Database` is the ONE hand-off to the solve leg; this page assembles no matrix and solves nothing, and the carrier page's demand keys arrive resolved from the project this custodian filled.

## [01]-[INDEX]

- [02]-[INVENTORY]: the `Inventory` project custodian — bootstrap, the `IngestSource` importer axis, the `Resolution` policy, and `IngestResult`.
- [03]-[PACKAGES]: the `MatrixPackage` owner over the `bw_processing` COO-triple datapackage substrate.

## [02]-[INVENTORY]

- Owner: `Inventory` — one frozen custodian per `(project, database)` target; `IngestSource` the closed importer axis whose case names the `bw2io` importer class it binds (`ecospold2`/`ecospold1`/`simapro_csv`/`excel`/`csv` the file importers, `ecoinvent_release`/`useeio`/`exiobase` the one-shot network imports); `Resolution` the unlinked-residual policy row.
- Law: every leg is project-pinned — `bd.projects.set_current(self.project)` opens each boundary body, idempotent by the provider's own contract — so the process-global current project is re-asserted per operation, never trusted across an await or a sibling composition's switch; this is the per-composition binding law applied to the one provider whose scope has no handle form.
- Law: `bootstrap` runs `bw2io.bw2setup()` once per project — biosphere3, bundled LCIA packs, core migrations — idempotent on an existing biosphere, so ingest never guards on a remembered out-of-band setup; the network one-shot imports ride the HTTP retry class on the banded thread hop because a release download is a transient-faulting remote leg.
- Law: the residual unlinked set resolves by POLICY — `Resolution.matched(db, fields)` links against a sibling database, `Resolution.promoted(biosphere)` admits unlinked flows as new biosphere records, `Resolution.strict()` refuses with the unlinked count on the fault — and `drop_unlinked(i_am_reckless=True)` never appears: silently erasing exchanges is the data-loss arm the policy vocabulary forecloses. A custom project linker is one `list[dict] -> list[dict]` strategy handed to `apply_strategy`, never an importer subclass.
- Law: `IngestResult` carries the `statistics()` quadruple as four `Option` slots, the written database name, and the source `ContentKey` over file bytes or the release coordinate. Only the file pipeline runs `statistics()`, so a release import leaves those slots absent; zero remains a measured empty value rather than standing in for an unmeasured source.
- Packages: `bw2data` (`projects.set_current`/`projects.create_project`, `databases`, the durable `Database` store, `errors.BW2Exception` the store family's root), `bw2io` (the importer classes, `bw2setup`, `apply_strategies`/`apply_strategy`/`statistics`/`match_database`/`add_unlinked_flows_to_biosphere_database`/`write_database`, `import_ecoinvent_release`/`useeio20`/`exiobase_monetary`, `errors.StrategyError`/`MultiprocessingError`), `bw_processing` (`errors.BrightwayProcessingError`), runtime (`RuntimeResult`/`boundary`/`Catch`/`FaultRow`/`ContentIdentity`/`scoped`/`RetryClass`/`guarded`/`on_thread`). Every provider binds `lazy`, so each raise set resolves at its call rather than as a module-scope tuple that would import the whole project stack to name an exception.
- Growth: a new source format is one `IngestSource` case naming its importer; a new linking move is one `Resolution` case; a new caller-required ingest measurement is one `IngestResult` field, `Option`-shaped wherever a source leg can leave it unmeasured; a new refusal law is one `FaultRow` row on this module's `RAISES` table; a project-specific remap is one strategy function, zero page edits.
- Boundary: no matrix assembly, no solve, no prospective build (`impact/scenario#SCENARIO` owns premise), no EPD parsing (the carrier's declaration arms own wires); backup/restore (`backup_project_directory`) is composition-root operations, not an owner surface; `imp.data` never leaks — the pipeline's interior `list[dict]` stays inside the boundary leg.

```python
from enum import StrEnum
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from opentelemetry import trace

lazy import bw2data as bd
lazy import bw2io
lazy import bw_processing as bp

from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, Catch, FaultRow, RuntimeResult, boundary, rostered, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.resilience import RetryClass, guarded

if TYPE_CHECKING:
    from collections.abc import Callable

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.impact.inventory")


# --- [CONSTANTS] ------------------------------------------------------------------------

def _project_raises() -> Catch:
    return (bd.errors.BW2Exception, KeyError, TypeError, ValueError, OSError)


def _ingest_raises() -> Catch:
    return (bw2io.errors.StrategyError, bw2io.errors.MultiprocessingError, *_project_raises())


def _package_raises() -> Catch:
    return (bp.errors.BrightwayProcessingError, KeyError, TypeError, ValueError, OSError)


INVENTORY_BOOTSTRAP: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INVENTORY, point="bootstrap", arm="boundary", defect="project-setup", retriability=TRANSIENT
)
INVENTORY_RELEASE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INVENTORY, point="ingest.release", arm="boundary", defect="release-import", retriability=TRANSIENT
)
INVENTORY_PIPELINE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INVENTORY, point="ingest.pipeline", arm="boundary", defect="ingest-pipeline", retriability=TERMINAL
)
INVENTORY_UNLINKED: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INVENTORY, point="ingest.unlinked", arm="config", defect="unlinked-residual", retriability=TERMINAL, slots=("count",)
)
PACKAGE_WRITE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INVENTORY, point="package", arm="boundary", defect="package-write", retriability=TERMINAL
)
PACKAGE_LOAD: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.INVENTORY, point="package.load", arm="boundary", defect="package-load", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    INVENTORY_BOOTSTRAP,
    INVENTORY_RELEASE,
    INVENTORY_PIPELINE,
    INVENTORY_UNLINKED,
    PACKAGE_WRITE,
    PACKAGE_LOAD,
]))


@tagged_union(frozen=True)
class IngestSource:
    tag: Literal["ecospold2", "ecospold1", "simapro_csv", "excel", "csv", "ecoinvent_release", "useeio", "exiobase"] = tag()
    ecospold2: str = case()
    ecospold1: str = case()
    simapro_csv: str = case()
    excel: str = case()
    csv: str = case()
    ecoinvent_release: tuple[str, str] = case()
    useeio: str = case()
    exiobase: tuple[int, int, int] = case()

    @property
    def importer(self) -> str:
        return _IMPORTER[self.tag]


_IMPORTER: Final[dict[str, str]] = {
    "ecospold2": "SingleOutputEcospold2Importer",
    "ecospold1": "SingleOutputEcospold1Importer",
    "simapro_csv": "SimaProCSVImporter",
    "excel": "ExcelImporter",
    "csv": "CSVImporter",
}


@tagged_union(frozen=True)
class Resolution:
    tag: Literal["matched", "promoted", "strict"] = tag()
    matched: tuple[str, tuple[str, ...]] = case()
    promoted: str = case()
    strict: None = case()


class IngestResult(Struct, frozen=True, gc=False):
    database: str
    nodes: Option[int]
    edges: Option[int]
    unlinked: Option[int]
    multifunctional: Option[int]
    content_key: ContentKey

class Inventory(Struct, frozen=True):
    project: str
    database: str

    def bootstrap(self) -> "RuntimeResult[None]":
        def run() -> None:
            bd.projects.set_current(self.project)
            bw2io.bw2setup()

        with _TRACER.start_as_current_span("inventory.bootstrap", attributes={"rasm.impact.project": self.project}):
            return boundary(INVENTORY_BOOTSTRAP, run, catch=_ingest_raises())

    async def ingest(
        self, source: IngestSource, resolution: Resolution, strategies: "tuple[Callable[[list], list], ...]" = ()
    ) -> "RuntimeResult[IngestResult]":
        def run() -> "RuntimeResult[IngestResult]":
            bd.projects.set_current(self.project)
            match source:
                case IngestSource(tag="ecoinvent_release", ecoinvent_release=(version, system_model)):
                    bw2io.import_ecoinvent_release(version, system_model)
                    return Ok(self._release_result(f"ecoinvent:{version}:{system_model}"))
                case IngestSource(tag="useeio", useeio=name):
                    bw2io.useeio20(name=name)
                    return Ok(self._release_result(f"useeio:{name}"))
                case IngestSource(tag="exiobase", exiobase=(major, minor, patch)):
                    bw2io.exiobase_monetary(version=(major, minor, patch), name=self.database)
                    return Ok(self._release_result(f"exiobase:{major}.{minor}.{patch}"))
                case filed:
                    return self._pipeline(filed, resolution, strategies)

        remote = source.tag in {"ecoinvent_release", "useeio", "exiobase"}
        with _TRACER.start_as_current_span(f"inventory.ingest.{source.tag}", attributes={"rasm.impact.project": self.project}):
            if remote:
                outcome = await guarded(RetryClass.HTTP, on_thread, run, at=INVENTORY_RELEASE, on=Some(source.tag))
                return outcome.bind(lambda held: held)
            fenced = await on_thread(lambda: boundary(INVENTORY_PIPELINE, run, catch=_ingest_raises()))
            return fenced.bind(lambda fence: fence).bind(lambda body: body)

    def _pipeline(
        self, source: IngestSource, resolution: Resolution, strategies: "tuple[Callable[[list], list], ...]"
    ) -> "RuntimeResult[IngestResult]":
        path = getattr(source, source.tag)
        imp = getattr(bw2io, source.importer)(path, self.database)
        imp.apply_strategies(verbose=False)
        for strategy in strategies:
            imp.apply_strategy(strategy)
        match resolution:
            case Resolution(tag="matched", matched=(db_name, fields)):
                imp.match_database(db_name, fields=list(fields))
            case Resolution(tag="promoted", promoted=biosphere):
                imp.add_unlinked_flows_to_biosphere_database(biosphere)
            case Resolution(tag="strict"):
                pass
            case unreachable:
                assert_never(unreachable)
        nodes, edges, unlinked, multifunctional = imp.statistics(print_stats=False)
        if unlinked and resolution.tag == "strict":
            return Error(INVENTORY_UNLINKED.raised(str(unlinked)))
        imp.write_database()
        key = ContentIdentity.key("impact", Path(path).read_bytes())
        return Ok(IngestResult(
            database=self.database, nodes=Some(nodes), edges=Some(edges), unlinked=Some(unlinked),
            multifunctional=Some(multifunctional), content_key=key,
        ))

    def _release_result(self, coordinate: str) -> IngestResult:
        key = ContentIdentity.key("impact", coordinate.encode())
        counted = Some(len(bd.Database(self.database))) if self.database in bd.databases else Nothing
        return IngestResult(
            database=self.database, nodes=counted, edges=Nothing, unlinked=Nothing, multifunctional=Nothing, content_key=key
        )
```

## [03]-[PACKAGES]

- Owner: `MatrixPackage` — the `bw_processing` datapackage custodian: one owner over the COO-triple substrate (`indices_array` under `INDICES_DTYPE`, `data_array`, `flip_array`, `distributions_array` under `UNCERTAINTY_DTYPE`) the `bw2calc` solver mounts as `data_objs`. It writes persistent vectors onto one `create_datapackage` handle and reads a stored package back through `load_datapackage`; the scenario-overlay splice (`merge_datapackages_with_mask`) rides here because splicing future coefficients onto a baseline background is datapackage algebra, not a build step.
- Law: matrix names are the solver's own vocabulary (`technosphere_matrix`, `biosphere_matrix`, `characterization_matrix`) spelled once as the `Matrix` StrEnum whose value IS the provider string; a hand-spelled matrix literal at a call site is the deleted form.
- Growth: a new resource kind is one `add_persistent_vector` call shape on the same handle; a new matrix name is one `Matrix` member; zero new surface.
- Boundary: no solve (the arrays hand to `bw2calc` as `data_objs`), no ingestion (the pipeline above writes databases, not packages), no premise build (the superstructure datapackage premise emits arrives as a stored package this owner merely loads).

```python
class Matrix(StrEnum):
    TECHNOSPHERE = "technosphere_matrix"
    BIOSPHERE = "biosphere_matrix"
    CHARACTERIZATION = "characterization_matrix"


class MatrixPackage(Struct, frozen=True):
    name: str

    def written(self, matrix: Matrix, indices: object, data: object, flip: Option[object] = Nothing) -> "RuntimeResult[object]":
        def build() -> object:
            package = bp.create_datapackage(name=self.name)
            package.add_persistent_vector(
                matrix=matrix.value, indices_array=indices, data_array=data, flip_array=flip.default_value(None)
            )
            return package

        with _TRACER.start_as_current_span("inventory.package", attributes={"rasm.impact.matrix": matrix.value}):
            return boundary(PACKAGE_WRITE, build, catch=_package_raises())

    @staticmethod
    def loaded(fs: object) -> "RuntimeResult[object]":
        def read() -> object:
            return bp.load_datapackage(fs)

        return boundary(PACKAGE_LOAD, read, catch=_package_raises())
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
