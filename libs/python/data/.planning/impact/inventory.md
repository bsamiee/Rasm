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
- Receipt: `IngestReceipt` carries the `statistics()` quadruple, the written database name, and the source `ContentKey` (the file bytes, or the release coordinate for a network import), contributing under `domain="impact"`/`kind="ingest"` with the lifted `domain`/`kind`/`key` columns every residence row reads.
- Packages: `bw2data` (`projects.set_current`/`projects.create_project`, `databases`, the durable `Database` store), `bw2io` (the importer classes, `bw2setup`, `apply_strategies`/`apply_strategy`/`statistics`/`match_database`/`add_unlinked_flows_to_biosphere_database`/`write_database`, `import_ecoinvent_release`/`useeio20`/`exiobase_monetary`), runtime (`RuntimeRail`/`boundary`/`ContentIdentity`/`scoped`/`RetryClass`/`guarded`/`on_thread`).
- Growth: a new source format is one `IngestSource` case naming its importer; a new linking move is one `Resolution` case; a new receipt fact is one `IngestReceipt` field; a project-specific remap is one strategy function, zero page edits.
- Boundary: no matrix assembly, no solve, no prospective build (`impact/scenario#SCENARIO` owns premise), no EPD parsing (the carrier's declaration arms own wires); backup/restore (`backup_project_directory`) is composition-root operations, not an owner surface; `imp.data` never leaks — the pipeline's interior `list[dict]` stays inside the boundary leg.

```python signature
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct
from opentelemetry import trace

from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded

if TYPE_CHECKING:
    from collections.abc import Callable, Iterable

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.impact.inventory")


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
    database: str
    nodes: int
    edges: int
    unlinked: int
    multifunctional: int
    content_key: ContentKey

    def contribute(self) -> "Iterable[Receipt]":
        Metrics.record({"rasm.impact.ingested": float(self.nodes)}, domain="impact", kind="ingest")
        yield Receipt.of(
            "inventory",
            (
                "emitted",
                self.database,
                {
                    "domain": "impact",
                    "kind": "ingest",
                    "key": self.content_key.hex,
                    "nodes": self.nodes,
                    "edges": self.edges,
                    "unlinked": self.unlinked,
                    "multifunctional": self.multifunctional,
                },
            ),
        )


class Inventory(Struct, frozen=True):
    project: str
    database: str

    def bootstrap(self) -> "RuntimeRail[None]":
        def run() -> None:
            import bw2data as bd  # ruff:ignore[import-outside-top-level] — banded boundary import
            import bw2io  # ruff:ignore[import-outside-top-level]

            bd.projects.set_current(self.project)
            bw2io.bw2setup()  # idempotent on an existing biosphere

        with _TRACER.start_as_current_span("inventory.bootstrap", attributes={"rasm.impact.project": self.project}):
            return boundary("inventory.bootstrap", run)

    async def ingest(
        self, source: IngestSource, resolution: Resolution, strategies: "tuple[Callable[[list], list], ...]" = ()
    ) -> "RuntimeRail[IngestReceipt]":
        # file imports run the blocking pipeline on the band hop; the network one-shots additionally ride the
        # HTTP retry class because a release download is a transient-faulting remote leg.
        def run() -> IngestReceipt:
            import bw2data as bd  # ruff:ignore[import-outside-top-level] — banded boundary import
            import bw2io  # ruff:ignore[import-outside-top-level]

            bd.projects.set_current(self.project)
            match source:
                case IngestSource(tag="ecoinvent_release", ecoinvent_release=(version, system_model)):
                    bw2io.import_ecoinvent_release(version, system_model)
                    return self._release_receipt(f"ecoinvent:{version}:{system_model}")
                case IngestSource(tag="useeio", useeio=name):
                    bw2io.useeio20(name=name)
                    return self._release_receipt(f"useeio:{name}")
                case IngestSource(tag="exiobase", exiobase=(major, minor, patch)):
                    bw2io.exiobase_monetary(version=(major, minor, patch), name=self.database)
                    return self._release_receipt(f"exiobase:{major}.{minor}.{patch}")
                case filed:
                    # every file-backed case routes the one pipeline; the importer name rides the case's own table row.
                    return self._pipeline(bw2io, filed, resolution, strategies)

        remote = source.tag in {"ecoinvent_release", "useeio", "exiobase"}
        with _TRACER.start_as_current_span(f"inventory.ingest.{source.tag}", attributes={"rasm.impact.project": self.project}):
            if remote:
                return await guarded(RetryClass.HTTP, on_thread, run, subject=f"inventory.ingest.{source.tag}")
            railed = await on_thread(lambda: boundary(f"inventory.ingest.{source.tag}", run))
            return railed.bind(lambda rail: rail)

    def _pipeline(
        self, bw2io: object, source: IngestSource, resolution: Resolution, strategies: "tuple[Callable[[list], list], ...]"
    ) -> IngestReceipt:
        # the canonical extract -> strategies -> statistics -> resolve -> write flow, statistics AS the receipt.
        from pathlib import Path  # ruff:ignore[import-outside-top-level]

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
            raise ValueError(f"<unlinked:{unlinked}>")  # the strict row refuses; reckless drop never appears
        imp.write_database()
        key = ContentIdentity.key("impact", Path(path).read_bytes())
        return IngestReceipt(
            database=self.database, nodes=nodes, edges=edges, unlinked=unlinked, multifunctional=multifunctional, content_key=key
        )

    def _release_receipt(self, coordinate: str) -> IngestReceipt:
        import bw2data as bd  # ruff:ignore[import-outside-top-level] — banded boundary import

        key = ContentIdentity.key("impact", coordinate.encode())
        counts = len(bd.Database(self.database)) if self.database in bd.databases else 0
        return IngestReceipt(database=self.database, nodes=counts, edges=0, unlinked=0, multifunctional=0, content_key=key)
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

    def written(self, matrix: Matrix, indices: object, data: object, flip: object | None = None) -> "RuntimeRail[object]":
        # one persistent-vector write onto a fresh package handle: indices ride INDICES_DTYPE rows, data the
        # aligned float vector, flip the sign mask — the exact triple bw2calc mounts; the handle returns for
        # further vectors, and `finalize_serialization` is the caller's terminal on the same handle.
        def build() -> object:
            import bw_processing as bp  # ruff:ignore[import-outside-top-level] — banded boundary import

            package = bp.create_datapackage(name=self.name)
            package.add_persistent_vector(matrix=matrix.value, indices_array=indices, data_array=data, flip_array=flip)
            return package

        with _TRACER.start_as_current_span("inventory.package", attributes={"rasm.impact.matrix": matrix.value}):
            return boundary("inventory.package", build)

    @staticmethod
    def loaded(fs: object) -> "RuntimeRail[object]":
        def read() -> object:
            import bw_processing as bp  # ruff:ignore[import-outside-top-level] — banded boundary import

            return bp.load_datapackage(fs)

        return boundary("inventory.package.load", read)
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
