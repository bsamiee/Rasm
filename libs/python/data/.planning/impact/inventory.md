# [PY_DATA_INVENTORY]

Brightway project and inventory custodian — the system-of-record leg of the impact plane: `Inventory` owns the `bw2data` project scope, the biosphere/LCIA bootstrap, and the whole LCI ingestion pipeline `bw2io` spells (`extract → apply_strategies → statistics → resolve → write_database`), and `MatrixPackage` owns the `bw_processing` matrix-datapackage substrate the solver consumes. Every operation is project-pinned: the owner carries its project name and every boundary leg re-selects it first, so an ambient current-project global never decides which store an import lands in — two same-process compositions with distinct projects cannot cross-write.

The pipeline's linking quality is a RECEIPT, never a print: `statistics()`'s `(nodes, edges, unlinked, multifunctional)` tuple lands typed on `IngestReceipt`, the residual unlinked set resolves through one closed `Resolution` policy row — match another database, promote to biosphere, or refuse — and `drop_unlinked`'s reckless erasure is the named rejected form. The written `bw2data.Database` is the ONE hand-off to the solve leg; this page assembles no matrix and solves nothing, and the carrier page's demand keys arrive resolved from the project this custodian filled.

## [01]-[INDEX]

- [02]-[INVENTORY]: the `Inventory` project custodian — bootstrap, the `IngestSource` importer axis, the `Resolution` policy, the typed `IngestReceipt`.
- [03]-[PACKAGES]: the `MatrixPackage` owner over the `bw_processing` COO-triple datapackage substrate.

## [02]-[INVENTORY]

- Owner: `Inventory` — one frozen custodian per `(project, database)` target; `IngestSource` the closed importer axis whose case names the `bw2io` importer class it binds (`ecospold2`/`ecospold1`/`simapro_csv`/`excel`/`csv` the file importers, `ecoinvent_release`/`useeio`/`exiobase` the one-shot network imports); `Resolution` the unlinked-residual policy row.
- Law: every leg is project-pinned — `bd.projects.set_current(self.project)` opens each boundary body, idempotent by the provider's own contract — so the process-global current project is re-asserted per operation, never trusted across an await or a sibling composition's switch; this is the per-composition binding law applied to the one provider whose scope has no handle form.
- Law: `bootstrap` runs `bw2io.bw2setup()` once per project — biosphere3, bundled LCIA packs, core migrations — idempotent on an existing biosphere, so ingest never guards on a remembered out-of-band setup; the network one-shot imports ride the HTTP retry class on the banded thread hop because a release download is a transient-faulting remote leg.
- Law: the residual unlinked set resolves by POLICY — `Resolution.matched(db, fields)` links against a sibling database, `Resolution.promoted(biosphere)` admits unlinked flows as new biosphere records, `Resolution.strict()` refuses with the unlinked count on the fault — and `drop_unlinked(i_am_reckless=True)` never appears: silently erasing exchanges is the data-loss arm the policy vocabulary forecloses. A custom project linker is one `list[dict] -> list[dict]` strategy handed to `apply_strategy`, never an importer subclass.
- Receipt: `IngestReceipt` carries the `statistics()` quadruple as four `Option` slots, the written database name, and the source `ContentKey` (the file bytes, or the release coordinate for a network import), contributing under `domain="impact"`/`kind="ingest"` with the lifted `domain`/`kind`/`key` columns every residence row reads. Only the FILE pipeline runs `statistics()`, so a release import declares those slots ABSENT and the metric it never measured lands on no series — a zero there reads as a perfectly-linked empty import and grades the plane on a fact nothing computed.
- Packages: `bw2data` (`projects.set_current`/`projects.create_project`, `databases`, the durable `Database` store, `errors.BW2Exception` the store family's root), `bw2io` (the importer classes, `bw2setup`, `apply_strategies`/`apply_strategy`/`statistics`/`match_database`/`add_unlinked_flows_to_biosphere_database`/`write_database`, `import_ecoinvent_release`/`useeio20`/`exiobase_monetary`, `errors.StrategyError`/`MultiprocessingError`), `bw_processing` (`errors.BrightwayProcessingError`), runtime (`RuntimeRail`/`boundary`/`Catch`/`FaultRow`/`ContentIdentity`/`scoped`/`RetryClass`/`guarded`/`on_thread`). Every provider binds `lazy`, so each raise set resolves at its call rather than as a module-scope tuple that would import the whole project stack to name an exception.
- Growth: a new source format is one `IngestSource` case naming its importer; a new linking move is one `Resolution` case; a new receipt fact is one `IngestReceipt` field, `Option`-shaped wherever a source leg can leave it unmeasured; a new refusal law is one `FaultRow` row on this module's `RAISES` table; a project-specific remap is one strategy function, zero page edits.
- Boundary: no matrix assembly, no solve, no prospective build (`impact/scenario#SCENARIO` owns premise), no EPD parsing (the carrier's declaration arms own wires); backup/restore (`backup_project_directory`) is composition-root operations, not an owner surface; `imp.data` never leaks — the pipeline's interior `list[dict]` stays inside the boundary leg.

```python signature
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
from rasm.runtime.faults import TERMINAL, TRANSIENT, Catch, FaultRow, RuntimeRail, boundary, rostered, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded

if TYPE_CHECKING:
    from collections.abc import Callable, Iterable

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.impact.inventory")


# --- [CONSTANTS] ------------------------------------------------------------------------

# every Brightway provider binds `lazy`, so each raise set resolves at the CALL: a module-scope `Final[Catch]`
# naming `bd.errors.BW2Exception` would import the whole project stack at module load and undo the deferral this
# page's package law states. `gridded/field#FIELD`'s `_arrow_raises()` is the landed shape.
def _project_raises() -> Catch:
    # `bw2data.errors.BW2Exception` roots the store's own family (`ValidityError`/`PickleError` under it); a project
    # or database name the registry never held answers `KeyError`, and the on-disk store answers `OSError`.
    return (bd.errors.BW2Exception, KeyError, TypeError, ValueError, OSError)


def _ingest_raises() -> Catch:
    # the pipeline adds `bw2io`'s two flat rows — `StrategyError` for a link against an absent database or an
    # invalid linking config, `MultiprocessingError` for the parallel extract — over the store family the write
    # leg reaches; the extract itself reads a file, so `OSError` stays.
    return (bw2io.errors.StrategyError, bw2io.errors.MultiprocessingError, *_project_raises())


def _package_raises() -> Catch:
    # `bw_processing.errors.BrightwayProcessingError` roots the datapackage family (`FileIntegrityError` under it);
    # the array arguments are numpy buffers, so a mis-shaped vector answers `TypeError`/`ValueError`.
    return (bp.errors.BrightwayProcessingError, KeyError, TypeError, ValueError, OSError)


# this module's raise roster under its one `DataLeg` member. The ingest legs split on the law they hold rather than
# on the source tag: a RELEASE download is a remote hop a re-issue may clear and declares TRANSIENT — it is the leg
# the HTTP retry class already wraps — while the FILE pipeline folds a local document and refuses identically on
# every re-read, so it declares TERMINAL. `bootstrap` fetches the bundled LCIA packs and is transient with them.
# The unlinked-residual row is this owner's OWN refusal and carries its count through `raised`; every other row is
# a fence anchor and declares no `slots`, a converted provider raise filling its detail from the cause.
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
    ecoinvent_release: tuple[str, str] = case()  # (version, system_model) — needs ecoinvent_interface credentials
    useeio: str = case()  # database name
    exiobase: tuple[int, int, int] = case()  # version triple

    @property
    def importer(self) -> str:
        # member value -> bw2io importer name, resolved at the call seam off ONE table.
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
    matched: tuple[str, tuple[str, ...]] = case()  # (database, match fields)
    promoted: str = case()  # biosphere database receiving unlinked flows
    strict: None = case()


class IngestReceipt(Struct, frozen=True, gc=False):
    # the four linking-quality slots ride `Option` because only the FILE pipeline runs `statistics()`: a network
    # one-shot import never computes them, and the zeros that used to stand in published a perfectly-linked import
    # indistinguishable from a measured one — `libs/.planning/RULINGS.md` `[02]`, unmeasured instruments read
    # UNMEASURED. `nodes` is optional for the same reason on the other side: a release whose database the registry
    # does not hold answered `0` for both "no such database" and "an empty one".
    database: str
    nodes: Option[int]
    edges: Option[int]
    unlinked: Option[int]
    multifunctional: Option[int]
    content_key: ContentKey

    def contribute(self) -> "Iterable[Receipt]":
        # the metric lands only for a MEASURED node count: a series that reads zero for every release import grades
        # the import as empty, and the absence is the fact the receipt carries instead.
        match self.nodes:
            case Option(tag="some", some=counted):
                Metrics.record({"rasm.impact.ingested": float(counted)}, domain="impact", kind="ingest")
            case Option(tag="none"):
                pass
        # an unmeasured slot OMITS its key rather than rendering a placeholder, so the evidence dict carries exactly
        # the facts this leg proved and a reader tells an absent statistic from a measured zero by key presence.
        measured = {
            name: held
            for name, slot in (
                ("nodes", self.nodes),
                ("edges", self.edges),
                ("unlinked", self.unlinked),
                ("multifunctional", self.multifunctional),
            )
            for held in slot.to_list()
        }
        yield Receipt.of(
            "inventory",
            ("emitted", self.database, {"domain": "impact", "kind": "ingest", "key": self.content_key.hex} | measured),
        )


class Inventory(Struct, frozen=True):
    project: str
    database: str

    def bootstrap(self) -> "RuntimeRail[None]":
        def run() -> None:
            bd.projects.set_current(self.project)
            bw2io.bw2setup()  # idempotent on an existing biosphere

        with _TRACER.start_as_current_span("inventory.bootstrap", attributes={"rasm.impact.project": self.project}):
            return boundary(INVENTORY_BOOTSTRAP, run, catch=_ingest_raises())

    async def ingest(
        self, source: IngestSource, resolution: Resolution, strategies: "tuple[Callable[[list], list], ...]" = ()
    ) -> "RuntimeRail[IngestReceipt]":
        # file imports run the blocking pipeline on the band hop; the network one-shots additionally ride the
        # HTTP retry class because a release download is a transient-faulting remote leg. The fenced body answers a
        # RAIL rather than a value, because the strict-resolution refusal is this owner's own and must not arrive
        # as a converted raise; each seam self-flattens once.
        def run() -> "RuntimeRail[IngestReceipt]":
            bd.projects.set_current(self.project)
            match source:
                case IngestSource(tag="ecoinvent_release", ecoinvent_release=(version, system_model)):
                    bw2io.import_ecoinvent_release(version, system_model)
                    return Ok(self._release_receipt(f"ecoinvent:{version}:{system_model}"))
                case IngestSource(tag="useeio", useeio=name):
                    bw2io.useeio20(name=name)
                    return Ok(self._release_receipt(f"useeio:{name}"))
                case IngestSource(tag="exiobase", exiobase=(major, minor, patch)):
                    bw2io.exiobase_monetary(version=(major, minor, patch), name=self.database)
                    return Ok(self._release_receipt(f"exiobase:{major}.{minor}.{patch}"))
                case filed:
                    # every file-backed case routes the one pipeline; the importer name rides the case's own table row.
                    return self._pipeline(filed, resolution, strategies)

        remote = source.tag in {"ecoinvent_release", "useeio", "exiobase"}
        with _TRACER.start_as_current_span(f"inventory.ingest.{source.tag}", attributes={"rasm.impact.project": self.project}):
            if remote:
                # TWO coordinates, answering different questions: `at` names WHICH CALL raised and `on` WHICH PEER it
                # reached. `RetryClass.HTTP` carries both a `CIRCUIT` and a `RATES` row, so an unstated peer refuses
                # `config` here — and rightly: the three release legs dial three distinct ORIGINS, and one arc over
                # the lot would trip on an ecoinvent outage and shed every healthy exiobase caller behind it. The
                # source tag IS that origin, so it keys the breaker arc and the rate bucket per destination.
                railed = await guarded(RetryClass.HTTP, on_thread, run, at=INVENTORY_RELEASE, on=Some(source.tag))
                return railed.bind(lambda rail: rail)
            fenced = await on_thread(lambda: boundary(INVENTORY_PIPELINE, run, catch=_ingest_raises()))
            # three rails stack on this leg — the band hop's, the fence's, and the body's own refusal rail — so two
            # self-flattens drop exactly the two wrappers and a strict refusal reaches the caller as itself rather
            # than nested inside a fence verdict that succeeded.
            return fenced.bind(lambda fence: fence).bind(lambda body: body)

    def _pipeline(
        self, source: IngestSource, resolution: Resolution, strategies: "tuple[Callable[[list], list], ...]"
    ) -> "RuntimeRail[IngestReceipt]":
        # the canonical extract -> strategies -> statistics -> resolve -> write flow, statistics AS the receipt.
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
            # the strict row refuses on the RAIL carrying its residual count as a named coordinate; reckless drop
            # never appears, and no write runs past this point.
            return Error(INVENTORY_UNLINKED.raised(str(unlinked)))
        imp.write_database()
        key = ContentIdentity.key("impact", Path(path).read_bytes())
        # this leg RAN `statistics()`, so all four slots are measured and every one declares.
        return Ok(IngestReceipt(
            database=self.database, nodes=Some(nodes), edges=Some(edges), unlinked=Some(unlinked),
            multifunctional=Some(multifunctional), content_key=key,
        ))

    def _release_receipt(self, coordinate: str) -> IngestReceipt:
        # a one-shot release import runs NO `statistics()` pass, so the three linking-quality slots are absent
        # rather than zero — a release is not a perfectly-linked import and the receipt must not read as one. The
        # node count is the registry's own len where the database registered and absent where it did not, which is
        # the state a bare `0` fused with an empty database.
        key = ContentIdentity.key("impact", coordinate.encode())
        counted = Some(len(bd.Database(self.database))) if self.database in bd.databases else Nothing
        return IngestReceipt(
            database=self.database, nodes=counted, edges=Nothing, unlinked=Nothing, multifunctional=Nothing, content_key=key
        )
```

## [03]-[PACKAGES]

- Owner: `MatrixPackage` — the `bw_processing` datapackage custodian: one owner over the COO-triple substrate (`indices_array` under `INDICES_DTYPE`, `data_array`, `flip_array`, `distributions_array` under `UNCERTAINTY_DTYPE`) the `bw2calc` solver mounts as `data_objs`. It writes persistent vectors onto one `create_datapackage` handle and reads a stored package back through `load_datapackage`; the scenario-overlay splice (`merge_datapackages_with_mask`) rides here because splicing future coefficients onto a baseline background is datapackage algebra, not a build step.
- Law: matrix names are the solver's own vocabulary (`technosphere_matrix`, `biosphere_matrix`, `characterization_matrix`) spelled once as the `Matrix` StrEnum whose value IS the provider string; a hand-spelled matrix literal at a call site is the deleted form.
- Growth: a new resource kind is one `add_persistent_vector` call shape on the same handle; a new matrix name is one `Matrix` member; zero new surface.
- Boundary: no solve (the arrays hand to `bw2calc` as `data_objs`), no ingestion (the pipeline above writes databases, not packages), no premise build (the superstructure datapackage premise emits arrives as a stored package this owner merely loads).

```python signature
class Matrix(StrEnum):
    # member value IS the bw2calc matrix name.
    TECHNOSPHERE = "technosphere_matrix"
    BIOSPHERE = "biosphere_matrix"
    CHARACTERIZATION = "characterization_matrix"


class MatrixPackage(Struct, frozen=True):
    name: str

    def written(self, matrix: Matrix, indices: object, data: object, flip: Option[object] = Nothing) -> "RuntimeRail[object]":
        # one persistent-vector write onto a fresh package handle: indices ride INDICES_DTYPE rows, data the
        # aligned float vector, flip the sign mask — the exact triple bw2calc mounts; the handle returns for
        # further vectors, and `finalize_serialization` is the caller's terminal on the same handle. A vector with
        # no sign mask carries `Nothing`, and the provider kwarg is the ONE site that absence lowers back to `None`.
        def build() -> object:
            package = bp.create_datapackage(name=self.name)
            package.add_persistent_vector(
                matrix=matrix.value, indices_array=indices, data_array=data, flip_array=flip.default_value(None)
            )
            return package

        with _TRACER.start_as_current_span("inventory.package", attributes={"rasm.impact.matrix": matrix.value}):
            return boundary(PACKAGE_WRITE, build, catch=_package_raises())

    @staticmethod
    def loaded(fs: object) -> "RuntimeRail[object]":
        def read() -> object:
            return bp.load_datapackage(fs)

        return boundary(PACKAGE_LOAD, read, catch=_package_raises())
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
