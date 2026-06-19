# [PY_ARTIFACTS_REPORT_PLAN]

The reproducible-report composition layer binding data and visual outputs into a document tree. `ReportPlan` is ONE owner spanning two report kinds on one axis: jinja2 template composition (parameterized sections, figure binding, TOC) and parameterized-notebook execution (papermill/nbclient headless run into an executed-notebook archive). The executed notebook is the reproducibility artifact; the rendered tree feeds `documents/document-plan#DOCUMENT`. It owns neither the visual nor the codec — it binds the figures keyed by content key into a template tree and hands the result to the document axis.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[REPORT]`, the report-composition axis over the template and notebook kinds.

## [2]-[REPORT]

- Owner: `ReportPlan` the one composition axis discriminating report kind; `ReportKind` the closed `StrEnum` over `TEMPLATE` (jinja2) and `NOTEBOOK` (papermill/nbclient); the bind layer between visual outputs and document inputs.
- Cases: `ReportKind.TEMPLATE` renders a jinja2 environment over section data and figure content keys into a document string; `ReportKind.NOTEBOOK` injects parameters into a parameters-tagged notebook, executes it headlessly through papermill on the nbclient engine, and yields the executed-notebook bytes as the audit artifact plus the rendered output feeding the document axis.
- Entry: `ReportPlan.render` dispatches the kind and returns a `RuntimeRail[ContentKey]`; the template environment is one `jinja2.Environment` with the section loader; the notebook run is one `papermill.execute_notebook` call on the configured nbclient kernel.
- Auto: template binding folds the sections and the figure content keys into `Environment.from_string(...).render`; notebook execution folds the parameter map through papermill into nbclient `NotebookClient.execute`, capturing the executed notebook as the reproducibility receipt.
- Receipt: each render contributes `receipt/artifact-receipt#RECEIPT` `ArtifactReceipt.Report` keyed by the content key over the rendered tree; the notebook kind additionally keys the executed-notebook archive.
- Packages: `jinja2` (`Environment`/`Template`/`select_autoescape`), `papermill` (`execute_notebook`), `nbclient` (`NotebookClient`), runtime (`content_identity.ContentKey`/`ContentIdentity`, `faults.RuntimeRail`/`boundary`).
- Growth: a new section kind is one template macro; a new report kind is one `ReportKind` row plus one acceptor arm; zero new surface.
- Boundary: a hand-rolled section-composition loop is the deleted form; the report binds figure content keys produced by `charts/chart-spec#CHART`, `tables/table-plan#TABLE`, and `scene3d/scene#SCENE`, never re-rendering them. Notebook conversion to PDF/HTML hands the rendered tree to the document axis; no live kernel server, no UI.

```python signature
from enum import StrEnum
from io import BytesIO
from typing import assert_never

from jinja2 import Environment, select_autoescape
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary


class ReportKind(StrEnum):
    TEMPLATE = "template"
    NOTEBOOK = "notebook"


class ReportPlan(Struct, frozen=True):
    kind: ReportKind
    source: str
    sections: tuple[dict[str, object], ...]
    figures: tuple[ContentKey, ...]
    parameters: dict[str, object]

    def render(self, env: Environment) -> RuntimeRail[ContentKey]:
        return boundary(f"report.{self.kind}", lambda: self._compose(env))

    def _compose(self, env: Environment) -> ContentKey:
        match self.kind:
            case ReportKind.TEMPLATE:
                rendered = env.from_string(self.source).render(sections=self.sections, figures=self.figures)
                return ContentIdentity.of("report-template", rendered.encode())
            case ReportKind.NOTEBOOK:
                executed = _execute_notebook(self.source, self.parameters)
                return ContentIdentity.of("report-notebook", executed)
            case _:
                assert_never(self.kind)


def report_env() -> Environment:
    return Environment(autoescape=select_autoescape(("html", "xml")), trim_blocks=True, lstrip_blocks=True)


def _execute_notebook(source: str, parameters: dict[str, object]) -> bytes:
    import nbformat
    import papermill

    executed = papermill.execute_notebook(
        source, None, parameters=parameters, engine_name=None, progress_bar=False, log_output=False
    )
    sink = BytesIO()
    nbformat.write(executed, sink, version=nbformat.NO_CONVERT)
    return sink.getvalue()
```

## [3]-[RESEARCH]

No open items. The papermill `execute_notebook(input_path, output_path, parameters, engine_name, progress_bar, log_output)` signature returns the mutated `NotebookNode` and defaults to the nbclient `NBClientEngine` when `engine_name=None`; both verify against the folder `.api` catalogues for `papermill` and `nbclient`. The executed `NotebookNode` serializes to the audit-artifact bytes through `nbformat.write(version=NO_CONVERT)` on the cp315 core; `output_path=None` runs the notebook without writing a side file, so the in-memory mutated node `execute_notebook` returns is the only emitted artifact and `nbformat.write` keys it under the content owner.
