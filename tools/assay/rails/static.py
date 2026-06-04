"""Static rail: five verbs as one ``Mode``-filtered, language-routed fold over ``Claim.STATIC``.

Carries no ``Detail`` variant; every outcome collapses into ``Report``. The rail never builds argv,
spawns, or leases — it ``select``s ``Tool`` rows, ``route``s the change-set, hands ``Check``s to the
Engine, and folds the ``Completed`` outcomes through ``core.model.fold`` (the sole count-derivation
site). The five adapters (``fix``/``report``/``build``/``full``/``plan``) are the ``Handler``s
``REGISTRY`` binds; four delegate to ``thin_rail``, ``plan`` short-circuits before the Engine. Every
adapter is a plain function (returns, never yields), so ``@effect.result`` is deliberately absent.

``route`` resolves one ``Language`` per call: a ``language=None`` request fans every member through its
own ``route``. ``sequence`` collapses both the per-language route rail and the per-slot ``fan_out`` rail
into one ``Result`` — a timeout/spawn ``Fault`` short-circuits to the registry seam, while a non-zero
exit already rode the success channel as ``Completed{status=FAILED}``, so the fold consumes successes
only and never sees a ``Fault`` for it.
"""

from dataclasses import dataclass
from hashlib import sha256
from typing import TYPE_CHECKING

from expression import Result  # noqa: TC002  # unconditional: beartype @checked resolves the handler forward-ref (PEP 649)
from expression.collections import block
from expression.extra.result import sequence
import msgspec

from tools.assay.composition.catalog import select  # intra-package import; tools.assay is the package root
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # unconditional for beartype runtime
from tools.assay.core.engine import fan_out  # intra-package import; tools.assay is the package root
from tools.assay.core.model import (  # intra-package import; tools.assay is the package root
    Artifact,
    ArtifactKind,
    BaseParams,
    Check,
    Claim,
    Fault,  # noqa: TC001  # unconditional: @checked's beartype resolves the -> Result[Report, Fault] forward-ref under PEP 649
    fold,
    Language,
    Mode,
    Report,  # noqa: TC001  # unconditional: see Fault above (same forward-ref resolution)
)
from tools.assay.core.routing import route, Routed  # intra-package import; tools.assay is the package root


if TYPE_CHECKING:
    from expression.collections import Block

    from tools.assay.core.model import Completed  # intra-package import; tools.assay is the package root


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class StaticParams(BaseParams):
    """Static rail CLI params: inherits ``paths``/``language`` from ``BaseParams``, adds nothing.

    Carries no ``strict`` field (only ``api``/``docs`` Params do), so an empty static slice can never be
    hardened into a fault; ``language=None`` selects every slice. ``registry`` flattens these fields onto
    the CLI via ``cyclopts`` ``Parameter(name="*")``.
    """


# --- [OPERATIONS] -----------------------------------------------------------------------


def _languages(selected: Language | None) -> tuple[Language, ...]:
    """Languages to fan over: one explicit member, or every member for the polyglot request.

    ``DOCS`` rides the fan despite its ``Claim`` being ``DOCS``: ``select(Claim.STATIC, …)`` yields no
    ``DOCS`` row, so the empty slice folds to an honest ``EMPTY`` without a per-language guard.
    """
    match selected:
        case None:
            return tuple(Language)
        case language:
            return (language,)


def _checks(routed: Routed, mode: Mode) -> tuple[Check, ...]:
    """Bind every ``Mode``-matching ``Tool`` of the routed language to the routed file scope.

    ``select(Claim.STATIC, language)`` yields the language slice; ``t.mode is mode`` keeps only the rows
    the verb owns. ``paths`` ride as a ``Check`` field; only ``scope``/``routed`` reach the Engine.
    """
    return tuple(Check(tool=t, paths=routed.files) for t in select(Claim.STATIC, routed.language) if t.mode is mode)


def _routed(languages: tuple[Language, ...], paths: tuple[str, ...]) -> Result[Block[Routed], Fault]:
    """Route every requested language through one ``route`` each, sequencing the per-language rail.

    ``route`` resolves one ``Language`` per call, so the polyglot request fans each member through its own
    ``route`` and ``sequence`` collapses the ``Block[Result[Routed, Fault]]`` into one
    ``Result[Block[Routed], Fault]``: the first routing ``Fault`` (a git/``fd`` spawn failure at the
    ``LOCAL`` boundary) short-circuits the whole fan.
    """
    return sequence(block.of_seq(route(language, paths) for language in languages))


def thin_rail(settings: AssaySettings, scope: ArtifactScope, params: StaticParams, *, claim: Claim, verb: str, mode: Mode) -> Result[Report, Fault]:
    """The shared body the four executing adapters parameterize: route → fan_out → fold.

    A **plain** function that folds a ``Result`` (never yields, so not ``@effect.result``). ``full``
    differs from ``build`` by ``verb="full"`` only — both pass ``mode=Mode.BUILD``; the FULL routing scope
    is a ``CLOSURE``-arm escalation internal to ``routing`` (trigger files → ``Scope.FULL``), never a
    parameter threaded here. A spawn/timeout/lease ``Fault`` is the sole ``Error`` channel; an honest
    no-op slice folds to ``EMPTY``/``SKIP`` on the success channel (there is no static ``strict``
    promotion to harden it).
    """
    return _routed(_languages(params.language), params.paths).bind(
        lambda routed: sequence(routed.collect(lambda r: block.of_seq(_dispatch(r, settings=settings, scope=scope, mode=mode)))).map(
            lambda done: fold(claim, verb, tuple(done))
        )
    )


def _dispatch(routed: Routed, *, settings: AssaySettings, scope: ArtifactScope, mode: Mode) -> tuple[Result[Completed, Fault], ...]:
    """Fan one routed language through the Engine under its OWN ``Routed`` context.

    The Engine threads a single ``routed`` parameter to ``place`` per ``fan_out`` call, so the polyglot
    fan runs one ``fan_out`` *per language* — each language's argv tail is projected from its own
    ``Routed``, never a shared head. The per-slot ``Result``s are returned flat so the caller concatenates
    every language's outcomes into one fold; a language with no ``mode``-matching rows produces an empty
    fan and contributes nothing rather than a phantom ``EMPTY`` slot.
    """
    checks = _checks(routed, mode)
    match checks:
        case ():
            return ()
        case _:
            return fan_out(checks, settings=settings, scope=scope, routed=routed)


# --- [COMPOSITION] ----------------------------------------------------------------------


def fix(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """The ``Mode.WRITE`` adapter: the ``Handler`` ``REGISTRY`` binds for ``static fix``.

    Fans the ``WRITE``-mode rows (``ruff format``/``ruff check --fix``, ``dotnet format``, ``shfmt -w``,
    ``sqlfluff fix``) concurrently under one ``CapacityLimiter``; the formatter twins touch disjoint
    suffix sets so the concurrent fan never races a file.
    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="fix", mode=Mode.WRITE)


def report(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """The ``Mode.CHECK`` adapter: the ``Handler`` ``REGISTRY`` binds for ``static report``.

    Fans the non-mutating ladder (``ruff check``, ``ruff format --check``, ``ty``/``mypy``, ``dotnet
    format --verify-no-changes``, ``shellcheck``, ``sqlfluff lint``, ``biome ci``) as diagnostics — no
    mutation, no build.
    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="report", mode=Mode.CHECK)


def build(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """The ``Mode.BUILD`` adapter: the ``Handler`` ``REGISTRY`` binds for ``static build``.

    Fans the ``BUILD``-mode rows (``dotnet build``/``tsc``) under the closure-leased artifact ``scope``
    the Engine splices into the dotnet argv.
    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="build", mode=Mode.BUILD)


def full(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """The ``.slnx``-parity adapter: ``build`` at full scope; the ``Handler`` for ``static full``.

    Differs from ``build`` by ``verb="full"`` only — it passes ``mode=Mode.BUILD`` like ``build``. The
    FULL routing scope is the ``CLOSURE`` arm's trigger-file escalation internal to ``routing``, so a
    ``Directory.Packages.props``/``Workspace.slnx``/analyzer-tree edit routes ``Scope.FULL`` and ``place``
    reads the whole ``SOLUTION`` without a scope-forcing parameter threaded here.
    """
    return thin_rail(settings, scope, params, claim=Claim.STATIC, verb="full", mode=Mode.BUILD)


def plan(settings: AssaySettings, scope: ArtifactScope, params: StaticParams) -> Result[Report, Fault]:
    """The zero-run adapter: owners/triggers/closure-sha into ``notes``/``artifacts``, no Engine.

    The lone short-circuit before the Engine: it routes per requested language, sequences the rows, and
    folds them into a ``Report`` whose ``artifacts``/``notes`` carry the routing projection — never
    reaching ``fan_out``. The closure sha is the same ``sha256(sorted-projects)[:16]`` recipe
    ``ArtifactScope.build`` keys its warm tree on, so an operator can correlate a planned
    ``build-<closure>`` tree with the lease a subsequent ``build`` takes.
    """
    _ = scope  # plan runs zero checks: no artifact scope is spliced
    return _routed(_languages(params.language), params.paths).map(
        lambda routed: msgspec.structs.replace(
            fold(Claim.STATIC, "plan", ()),  # empty outcomes seed EMPTY status + zero counts via the sole deriver
            artifacts=_plan_artifacts(tuple(routed), settings),
            notes=_plan_notes(tuple(routed)),
        )
    )


def _plan_artifacts(routed: tuple[Routed, ...], settings: AssaySettings) -> tuple[Artifact, ...]:
    """Project the planned warm-tree path per routed closure: the ``build-<closure>`` correlate.

    Only a slice that resolved a non-empty project closure plans a warm tree; the artifact path is the
    path-fold equivalent of ``ArtifactScope.build`` so the planned path is byte-identical to the one
    ``build`` leases. A glob-only language (no ``projects``) plans no warm tree and contributes no row.
    """
    return tuple(
        Artifact(id=f"build-{(sha := _closure_sha(r))}", kind=ArtifactKind.SCOPE, path=ArtifactScope.build(settings, sha).path)
        for r in routed
        if r.projects
    )


def _plan_notes(routed: tuple[Routed, ...]) -> tuple[str, ...]:
    """Fold the routing projection into operator notes: owners, full-triggers, and the closure sha.

    Surfaces the closure fingerprint verbatim so the planned ``build-<closure>`` lock path can be
    correlated against a subsequent ``build`` lease. A glob-only language with no projects and no triggers
    folds to its bare routed-file count, never a phantom closure note.
    """
    return tuple(note for r in routed for note in _routed_notes(r))


def _routed_notes(routed: Routed) -> tuple[str, ...]:
    """The note rows of one routed slice: scope + closure-sha for C#, file count for a glob language."""
    match routed:
        case Routed(projects=projects) if projects:
            return (f"{routed.language.value}: scope={routed.scope.value} closure={_closure_sha(routed)} projects={len(projects)}",)
        case Routed(full_triggers=triggers) if triggers:
            return (
                f"{routed.language.value}: scope={routed.scope.value} files={len(routed.files)}",
                *(f"{routed.language.value}: full-trigger {t}" for t in triggers),
            )
        case _:
            return (f"{routed.language.value}: scope={routed.scope.value} files={len(routed.files)}",)


def _closure_sha(routed: Routed) -> str:
    """The ``sha256(sorted-projects)[:16]`` closure fingerprint: ``ArtifactScope.build``'s key.

    Computed from ``Routed.projects`` (already sorted by ``routing._resolve``) joined on newline so the
    planned ``build-<closure>`` path matches the warm tree a subsequent ``build`` leases bit-for-bit; an
    empty closure yields the sha of the empty string, which no artifact row references (``_plan_artifacts``
    gates on ``r.projects``).
    """
    return sha256("\n".join(routed.projects).encode()).hexdigest()[:16]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__: list[str] = ["StaticParams", "build", "fix", "full", "plan", "report", "thin_rail"]
