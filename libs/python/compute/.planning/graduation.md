# [PY_COMPUTE_GRADUATION]

The C# graduation receipt. `GraduationReceipt` moves useful Python evidence into the C# owner system: source-pkg/target-C#-owner/subject/evidence-bundle/residual-limits/decision-route. `HandoffAxis` is the Literal-discriminated union of handoff kinds; the geometry case carries registration-transform/reconstructed-mesh/topology-graph/form-finding subjects so geometry-package evidence reaches the C# owner system through the one graduation rail. Graduation is a Python-branch-only concept; no C# graduation page exists, so the geometry case is added freely. The receipt describes handoff; it never mints C# receipts or authorizes product runtime behavior.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                                                 |
| :-----: | :--------- | :------------------------------------------------------------------------------------- |
|   [1]   | GRADUATION | the graduation receipt, the handoff axis, cross-owner rules                            |
|   [2]   | INFERENCE  | the Bayesian inference owner over a prior/likelihood/posterior axis emitting a receipt |
|   [3]   | CODEGEN    | the typed-stub generator consuming the C# graduation-evidence bundle shape             |

## [2]-[GRADUATION]

- Owner: `GraduationReceipt` — the source-pkg/target-C#-owner/subject/evidence-bundle/residual-limits/decision-route record; `HandoffAxis` the Literal-discriminated union of handoff kinds; wired through runtime `ReceiptContributor`.
- Cases: `HandoffAxis` literals `solver` · `symbolic` · `model-asset` · `array-layout` · `unit-law` · `uncertainty-law` · `geometry` — the geometry case carries the `GeometrySubject` literal `registration-transform` · `reconstructed-mesh` · `topology-graph` · `form-finding`.
- Entry: `GraduationReceipt.of` builds the receipt from a study receipt, artifact bundle, data exchange bundle, model-asset manifest, or geometry evidence and returns the C# handoff record; `GraduationReceipt.route` resolves the target C# owner from the axis.
- Auto: solver/symbolic evidence routes to `Rasm.Compute`; model-asset evidence routes to the C# model lane only after the manifest validation passes; geometry evidence routes to the C# owner system carrying its `GeometrySubject`; the residual-limits field gates the decision route.
- Receipt: the graduation contributes a `Receipt.Planned` row through `ReceiptContributor` (it is a handoff proposal, never an emitted product receipt); the planned facts carry the resolved route and target owner so the row records the routing verdict.
- Packages: `msgspec`, runtime (`ReceiptContributor`/`ContentKey`).
- Growth: a new handoff kind is one `HandoffAxis` literal; a new geometry subject is one `GeometrySubject` literal; zero new surface.
- Boundary: no C# `ComputeReceipt`, benchmark claim, or source generation; a handoff record claiming production readiness, a graduation without data/artifact evidence, a Python-only benchmark conclusion, and a C# source-shape claim absent from the C# owner planning pages are the deleted forms; this owner is FINALIZED (no external-package member, only the runtime port and msgspec).

```python signature
from typing import Literal

from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.observability import Receipt


type HandoffAxis = Literal["solver", "symbolic", "model-asset", "array-layout", "unit-law", "uncertainty-law", "geometry"]
type GeometrySubject = Literal["registration-transform", "reconstructed-mesh", "topology-graph", "form-finding"]


class GraduationReceipt(Struct, frozen=True):
    source_package: str
    target_csharp_owner: str
    axis: HandoffAxis
    subject: str
    evidence_key: ContentKey
    residual_limits: dict[str, float]
    decision_route: str

    def contribute(self) -> Receipt:
        return Receipt.Planned(
            self.source_package,
            self.target_csharp_owner,
            {"axis": self.axis, "route": self.decision_route, "target": self.target_csharp_owner},
        )

    @staticmethod
    def route(axis: HandoffAxis) -> str:
        match axis:
            case "geometry":
                return "Rasm.Compute#interchange"
            case "model-asset":
                return "Rasm.Compute#model-lane"
            case _:
                return "Rasm.Compute"
```

## [3]-[CROSS_OWNER_RULES]

- Solver evidence routes to `Rasm.Compute` unless it belongs in the RhinoCommon-aware kernel.
- Symbolic derivation routes to C# source owners only after the derivation is stable and reproducible (the sympy codegen handoff).
- Model-asset evidence routes to the C# model lane only after the `ModelAssetManifest` validation passes.
- Unit and uncertainty evidence routes to the C# quantity/unit owners as policy rows only.
- Geometry evidence (registration transform, reconstructed mesh, topology graph, form-finding) routes to the C# owner system through the geometry `HandoffAxis` case, the single rail geometry-package evidence crosses; CATALOGUE_PENDING: each `GeometrySubject` literal must map to an actual C# interchange owner row at the seam before the geometry case finalizes, or geometry graduates evidence to a target that cannot receive it — the `route("geometry")` target `Rasm.Compute#interchange` is the provisional row pending that owner-contract confirmation.
- Inference evidence (a posterior with passing convergence diagnostics) routes to the C# quantity/uncertainty owners through the `uncertainty-law` `HandoffAxis` case as a policy row; the inference receipt's diagnostic fields gate the residual-limits check, and a posterior failing the `r_hat`/`ess` bar is an admission rejection, never a graduated handoff.

## [4]-[INFERENCE]

- Owner: `Inference` — the one Bayesian owner over a prior/likelihood/posterior axis. `InferenceSpec` is the frozen request carrying the observed `ArrayPayload`, the `Prior` family per latent, the `Likelihood` case, and the `SamplerPlan`; `Inference.run` builds the `pymc.Model`, draws the posterior with `pymc.sample`, scores it with `arviz` diagnostics, and returns an `InferenceReceipt`. One owner, one rail — adding a distribution family is a `Prior`/`Likelihood` case, never a parallel sampler surface.
- Cases: `Prior` literals `normal` · `half_normal` · `beta` · `gamma` · `student_t` · `uniform` — each maps to the matching `pymc` distribution class; `Likelihood` literals `normal` · `bernoulli` · `poisson` · `binomial` · `student_t` select the observed-node distribution; `SamplerKind` literals `nuts` · `metropolis` select the step method (the `nuts` default is the gradient sampler `pymc.sample` chooses).
- Entry: `Inference.run(spec)` returns `RuntimeRail[InferenceReceipt]` through one `boundary`; `InferenceReceipt.graduates` is the hard convergence gate — it returns `Ok(GraduationReceipt(...))` on the `uncertainty-law` axis when `converged` is true and `Error(BoundaryFault.Boundary(...))` when it is false, so a non-converged posterior is an admission rejection on the rail, never a graduated handoff carrying a sentinel route.
- Diagnostics: `arviz.rhat` and `arviz.ess` over the `arviz.InferenceData` trace fill the receipt's `r_hat`/`ess_bulk`/`ess_tail` fields; `converged` is the three-term fold `max(r_hat) < rhat_ceiling and min(ess_bulk) > ess_floor and divergences == 0`, the published convergence bar — a divergent NUTS run is not graduable — the receipt carries the evidence, the boolean carries the decision.
- Receipt: `InferenceReceipt` is the typed posterior-evidence carrier (posterior-mean/posterior-sd per latent, the diagnostic fields, the `divergences` count, the model `ContentKey`); it contributes a `Receipt.Planned` row through `ReceiptContributor`. The `model_key` is `ContentIdentity.key` over the full study payload — observed-data bytes, sorted latent specs, likelihood, and `SamplerPlan` — so two studies with the same latent names but different data or plan key distinctly and never collide to one cache hit.
- Packages: `pymc` (model/sample), `arviz` (diagnostics), `numpy`, `msgspec`, runtime (`RuntimeRail`/`boundary`/`ContentKey`/`Receipt`). `pymc` and `arviz` are DEPLOY-ASSET-GATED on cp315: `pymc` pulls `pytensor`->`numba`->`llvmlite` (no cp315 wheel) and `arviz` pulls `arviz-stats`->`scipy` (no cp315 wheel); both fences are documented-API transcriptions verified-by-stability on the marker floor (suite TASKLOG `PY_INFER_001`), the same deploy-asset-gate posture as the `numba`/`jax`/`onnx` accelerator rows. CATALOGUE_PENDING: `api-pymc.md`/`api-arviz.md` are deferred until the cp315 wheels resolve; at gate-lift the re-reflection confirms the `arviz.InferenceData` group accessor is reachable as `trace.sample_stats` (not `trace["sample_stats"]`), the single PyMC<->ArviZ spelling most likely to drift across the cp315 wheel rebuild.
- Boundary: classical Bayesian inference only — conjugate, hierarchical, and GLM-class models via MCMC. Variational/normalizing-flow/neural posterior estimation and any deep generative model are OUT of scope and never enter this owner; the charter amendment draws the line exactly at gradient-MCMC over an explicit prior/likelihood graph. Inference is offline evidence; production uncertainty propagation stays in the C# quantity/unit owners, reached only through the `uncertainty-law` graduation row.

```python signature
from typing import Literal

import arviz
import msgspec
import numpy as np
import pymc
from expression import Error, Ok
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.observability import Receipt
from rasm.runtime.rails_resilience import BoundaryFault, RuntimeRail, boundary


type Prior = Literal["normal", "half_normal", "beta", "gamma", "student_t", "uniform"]
type Likelihood = Literal["normal", "bernoulli", "poisson", "binomial", "student_t"]
type SamplerKind = Literal["nuts", "metropolis"]

_PRIOR_CLS: dict[Prior, type] = {
    "normal": pymc.Normal,
    "half_normal": pymc.HalfNormal,
    "beta": pymc.Beta,
    "gamma": pymc.Gamma,
    "student_t": pymc.StudentT,
    "uniform": pymc.Uniform,
}
_LIKELIHOOD_CLS: dict[Likelihood, type] = {
    "normal": pymc.Normal,
    "bernoulli": pymc.Bernoulli,
    "poisson": pymc.Poisson,
    "binomial": pymc.Binomial,
    "student_t": pymc.StudentT,
}


class LatentPrior(Struct, frozen=True):
    name: str
    prior: Prior
    params: dict[str, float]


class SamplerPlan(Struct, frozen=True):
    kind: SamplerKind = "nuts"
    draws: int = 2000
    tune: int = 1000
    chains: int = 4
    target_accept: float = 0.9
    seed: int = 0
    ess_floor: float = 400.0
    rhat_ceiling: float = 1.01


class InferenceSpec(Struct, frozen=True):
    observed: np.ndarray
    latents: tuple[LatentPrior, ...]
    likelihood: Likelihood
    mean_latent: str
    plan: SamplerPlan = SamplerPlan()


class InferenceReceipt(Struct, frozen=True):
    likelihood: Likelihood
    sampler: SamplerKind
    posterior_mean: dict[str, float]
    posterior_sd: dict[str, float]
    r_hat: dict[str, float]
    ess_bulk: dict[str, float]
    ess_tail: dict[str, float]
    divergences: int
    draws: int
    converged: bool
    model_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.Planned(
            "compute.inference",
            self.mean_subject(),
            {"likelihood": self.likelihood, "converged": str(self.converged), "divergences": str(self.divergences)},
        )

    def mean_subject(self) -> str:
        return next(iter(self.posterior_mean), "<empty>")

    def graduates(self) -> RuntimeRail["GraduationReceipt"]:
        if not self.converged:
            return Error(
                BoundaryFault.Boundary(
                    "inference.graduate",
                    f"non-converged posterior: max_rhat={max(self.r_hat.values())}, "
                    f"min_ess_bulk={min(self.ess_bulk.values())}, divergences={self.divergences}",
                )
            )
        return Ok(
            GraduationReceipt(
                source_package="compute",
                target_csharp_owner="Rasm.Compute",
                axis="uncertainty-law",
                subject=self.mean_subject(),
                evidence_key=self.model_key,
                residual_limits={"max_rhat": max(self.r_hat.values()), "min_ess_bulk": min(self.ess_bulk.values())},
                decision_route="Rasm.Compute",
            )
        )


class Inference:
    @staticmethod
    def run(spec: InferenceSpec) -> RuntimeRail[InferenceReceipt]:
        return boundary(f"inference.{spec.likelihood}", lambda: Inference._fit(spec))

    @staticmethod
    def _fit(spec: InferenceSpec) -> InferenceReceipt:
        with pymc.Model() as model:
            nodes = {lat.name: _PRIOR_CLS[lat.prior](lat.name, **lat.params) for lat in spec.latents}
            mu = nodes[spec.mean_latent]
            _LIKELIHOOD_CLS[spec.likelihood]("observation", mu, observed=spec.observed)
            trace = pymc.sample(
                draws=spec.plan.draws,
                tune=spec.plan.tune,
                chains=spec.plan.chains,
                target_accept=spec.plan.target_accept,
                random_seed=spec.plan.seed,
                step=_step(spec.plan.kind, model),
                progressbar=False,
                return_inferencedata=True,
            )
        return Inference._score(spec, trace)

    @staticmethod
    def _score(spec: InferenceSpec, trace: "arviz.InferenceData") -> InferenceReceipt:
        names = [lat.name for lat in spec.latents]
        posterior = trace.posterior
        rhat = arviz.rhat(trace, var_names=names)
        ess_bulk = arviz.ess(trace, var_names=names, method="bulk")
        ess_tail = arviz.ess(trace, var_names=names, method="tail")
        r_hat = {n: float(rhat[n].to_numpy()) for n in names}
        ebk = {n: float(ess_bulk[n].to_numpy()) for n in names}
        plan = spec.plan
        divergences = int(trace.sample_stats["diverging"].to_numpy().sum())
        return InferenceReceipt(
            likelihood=spec.likelihood,
            sampler=plan.kind,
            posterior_mean={n: float(posterior[n].mean().to_numpy()) for n in names},
            posterior_sd={n: float(posterior[n].std().to_numpy()) for n in names},
            r_hat=r_hat,
            ess_bulk=ebk,
            ess_tail={n: float(ess_tail[n].to_numpy()) for n in names},
            divergences=divergences,
            draws=plan.draws * plan.chains,
            converged=(max(r_hat.values()) < plan.rhat_ceiling and min(ebk.values()) > plan.ess_floor and divergences == 0),
            model_key=ContentIdentity.key("pymc-model", _study_payload(spec)),
        )


def _step(kind: SamplerKind, model: "pymc.Model") -> object:
    match kind:
        case "nuts":
            return pymc.NUTS(model=model)
        case "metropolis":
            return pymc.Metropolis(model=model)


def _study_payload(spec: InferenceSpec) -> bytes:
    latents = msgspec.json.encode(sorted(spec.latents, key=lambda lat: lat.name))
    plan = msgspec.json.encode(spec.plan)
    observed = np.ascontiguousarray(spec.observed)
    shape = repr((observed.dtype.str, observed.shape)).encode()
    return b"\x00".join((spec.likelihood.encode(), latents, plan, shape, observed.tobytes()))
```

## [5]-[CODEGEN]

- Owner: `StubCodegen` — the one generator that consumes the C# graduation-evidence bundle SHAPE (the offline seam the C# side seals) and emits typed Python stubs so downstream Python composes against the C# owner row by import, not by re-typing it. The bundle shape is CONSUMED at the boundary, decoded once with `msgspec`, never re-minted: this owner reads `EvidenceBundle`/`OwnerDescriptor`/`FieldDescriptor` (the shape the C# graduation evidence already carries) and projects each descriptor into a `msgspec.Struct` stub via the stdlib `ast` builder.
- Cases: one `FieldDescriptor.kind` literal per primitive the C# seam emits — `i32` · `i64` · `f64` · `bool` · `string` · `key` · `array` · `nested` — folded by `match` to the Python annotation (`int`/`float`/`bool`/`str`/`ContentKey`/`tuple[..., ...]`/the nested stub name); a new C# field kind is one literal and one match arm, never a parallel emitter.
- Entry: `StubCodegen.emit(bundle)` returns `RuntimeRail[GeneratedModule]` through one `boundary`; it decodes the bundle with the module `_BUNDLE_DECODER`, folds the owners into `ast.ClassDef` nodes, and renders the module source with `ast.unparse`. `GeneratedModule` carries the rendered source, the owner count, and the bundle `ContentKey` so the generation is itself receiptable.
- Seam: `EvidenceBundle` is the DECODED projection of the C# graduation-evidence bundle (`owners`, the C# `schema_version`, the `ContentKey` the C# side sealed it under); the Python owner names the same fields the C# bundle carries and adds zero shape. This is the only place compute reads the C# evidence shape; it imports nothing from a C# interior and re-mints nothing — the seam crosses once, offline, as bytes.
- Receipt: `StubCodegen.emit` does not graduate (it consumes a graduated artifact); the caller threads `GeneratedModule.bundle_key` into a study/model receipt when the generated stubs feed a downstream pipeline.
- Packages: stdlib `ast` (stub synthesis), `msgspec` (bundle decode), runtime (`RuntimeRail`/`boundary`/`ContentKey`). No external dist and no cp315 gate — `ast` and `msgspec` are both reflectable on the active env.
- Boundary: codegen emits TYPE stubs (`.pyi`-shaped `msgspec.Struct` declarations) only; it never emits runtime behavior, never re-derives the evidence shape, and never authors a C# source shape. The C# graduation-evidence bundle is the upstream authority; this owner is a one-directional offline consumer of that authority.

```python signature
import ast
from typing import Literal

import msgspec
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.rails_resilience import RuntimeRail, boundary


type FieldKind = Literal["i32", "i64", "f64", "bool", "string", "key", "array", "nested"]

_ANNOTATION: dict[FieldKind, str] = {
    "i32": "int",
    "i64": "int",
    "f64": "float",
    "bool": "bool",
    "string": "str",
    "key": "ContentKey",
}


class FieldDescriptor(Struct, frozen=True):
    name: str
    kind: FieldKind
    element: "FieldDescriptor | None" = None
    nested: str | None = None


class OwnerDescriptor(Struct, frozen=True):
    name: str
    fields: tuple[FieldDescriptor, ...]


class EvidenceBundle(Struct, frozen=True):
    schema_version: str
    owners: tuple[OwnerDescriptor, ...]
    bundle_key: ContentKey


class GeneratedModule(Struct, frozen=True):
    source: str
    owner_count: int
    bundle_key: ContentKey


_BUNDLE_DECODER = msgspec.json.Decoder(type=EvidenceBundle)


class StubCodegen:
    @staticmethod
    def emit(raw: bytes) -> RuntimeRail[GeneratedModule]:
        return boundary("codegen.stub", lambda: StubCodegen._render(_BUNDLE_DECODER.decode(raw)))

    @staticmethod
    def _render(bundle: EvidenceBundle) -> GeneratedModule:
        body: list[ast.stmt] = [StubCodegen._class(owner) for owner in bundle.owners]
        module = ast.Module(body=body, type_ignores=[])
        ast.fix_missing_locations(module)
        return GeneratedModule(source=ast.unparse(module), owner_count=len(bundle.owners), bundle_key=bundle.bundle_key)

    @staticmethod
    def _class(owner: OwnerDescriptor) -> ast.ClassDef:
        fields = [StubCodegen._field(f) for f in owner.fields]
        return ast.ClassDef(
            name=owner.name,
            bases=[ast.Name(id="Struct", ctx=ast.Load())],
            keywords=[ast.keyword(arg="frozen", value=ast.Constant(value=True))],
            body=fields or [ast.Pass()],
            decorator_list=[],
            type_params=[],
        )

    @staticmethod
    def _field(field: FieldDescriptor) -> ast.AnnAssign:
        return ast.AnnAssign(
            target=ast.Name(id=field.name, ctx=ast.Store()),
            annotation=ast.parse(StubCodegen._annotation(field), mode="eval").body,
            value=None,
            simple=1,
        )

    @staticmethod
    def _annotation(field: FieldDescriptor) -> str:
        match field:
            case FieldDescriptor(kind="array", element=FieldDescriptor() as inner):
                return f"tuple[{StubCodegen._annotation(inner)}, ...]"
            case FieldDescriptor(kind="nested", nested=str() as ref):
                return ref
            case FieldDescriptor(kind=kind):
                return _ANNOTATION[kind]
```
