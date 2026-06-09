"""@spec decorator, law MANIFEST registry, and law-coverage gate.

Provides one polymorphic entrypoint ``spec`` that applies ``@given`` + ``@settings`` + pytest marks
and registers a ``LawRecord`` into the global ``MANIFEST``. ``register_law`` covers multi-arg /
metamorphic / matrix laws that bring their own ``@given``/``@parametrize``. ``register_sut`` records
the SUT package surface; ``assert_law_coverage`` walks it via ``pkgutil.walk_packages`` and asserts
every public symbol has at least one law or explicit exemption.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from builtins import sentinel as _sentinel  # 3.15 PEP 661 builtin; mypy typeshed lag (ty resolves)
from collections.abc import Callable  # noqa: TC003  # PEP 695 [**P] annotations are evaluated at runtime; TYPE_CHECKING guard would break them
import functools
import importlib
import inspect
from pathlib import Path

from hypothesis import event as hyp_event, given as hyp_given, settings as hyp_settings
import msgspec
import pytest


# --- [CONSTANTS] ------------------------------------------------------------------------

_UNSET: object = _sentinel("_UNSET")


# --- [MODELS] ---------------------------------------------------------------------------


class LawRecord(msgspec.Struct, frozen=True):
    """One law entry in the global MANIFEST.

    ``subject`` and ``law`` hold the qualname / function-name captured at import time so coverage
    comparisons are stable across interpreter restarts.
    """

    subject: str
    law: str
    module: str


MANIFEST: list[LawRecord] = []
SUT_PACKAGES: dict[str, frozenset[str]] = {}


# --- [OPERATIONS] -----------------------------------------------------------------------


def _qualname(subject: object) -> str:
    """Return a stable qualified name for a subject type or callable.

    Returns:
        The most specific name available on the subject.
    """
    return getattr(subject, "__qualname__", None) or getattr(subject, "__name__", None) or str(subject)


def _qualname_simple(qualname: str) -> str:
    """Return the simple (last-component) name from a qualname string.

    ``tools.assay.core.model.Envelope`` → ``Envelope``.

    Returns:
        The last dot-delimited segment.
    """
    return qualname.rsplit(".", 1)[-1]


def _public_surface(package_name: str) -> frozenset[str]:  # noqa: PLR0912  # filesystem walk + per-module __all__/dir fork is one cohesive surface-collection pass
    """Walk every leaf module under ``package_name`` and union their public symbols.

    Public surface = union of each module's ``__all__``, or all non-underscore attribute names when
    ``__all__`` is absent. Module objects re-exported as attributes are excluded because they are
    structural namespacing, not callable/type law subjects.

    Returns:
        Frozenset of simple (unqualified) public symbol names across the whole SUT package.
    """
    root = importlib.import_module(package_name)
    modules = [root]  # the root package's own __all__ counts — submodule walk alone misses __init__ exports
    # Filesystem walk over __path__ (NOT pkgutil.walk_packages): the SUT uses namespace subpackages with no
    # __init__.py, which walk_packages silently skips — that would false-pass the gate over the whole subtree.
    for base in getattr(root, "__path__", ()):
        for py in sorted(Path(base).rglob("*.py")):
            parts = py.relative_to(base).with_suffix("").parts
            stem = parts[:-1] if parts[-1] == "__init__" else parts
            mod_name = ".".join((package_name, *stem))
            if mod_name == package_name:
                continue  # root already included
            try:
                modules.append(importlib.import_module(mod_name))
            except Exception:  # noqa: BLE001, S112  # walk must continue past broken/optional modules; silence is intentional
                continue

    surface: set[str] = set()
    for mod in modules:
        all_names: list[str] | None = getattr(mod, "__all__", None)
        match all_names:
            case list() as names:
                surface.update(n for n in names if not inspect.ismodule(getattr(mod, n, None)))
            case _:
                surface.update(n for n in dir(mod) if not n.startswith("_") and not inspect.ismodule(getattr(mod, n, None)))

    return frozenset(surface)


# --- [EXPORTS] --------------------------------------------------------------------------


def spec[**P](
    subject: object,
    *,
    given: bool = True,
    mutation: bool = False,
    profile: str = "rasm",
    markers: tuple[str, ...] = (),
    timeout: float | None = None,
    law: str | None = None,
    events: tuple[Callable[[object], str], ...] = (),
) -> Callable[[Callable[P, None]], Callable[P, None]]:
    """Polymorphic @spec entrypoint: register + optionally drive a property law.

    When ``given=True`` (default), applies ``@hypothesis.given(resolve(subject))`` positionally —
    the strategy parameter must be the LAST param (convention; fixtures precede it). When
    ``given=False``, only registers the ``LawRecord`` and applies marks (for metamorphic / matrix /
    multi-arg laws that supply their own ``@given``/``@parametrize``).

    Applies ``@settings(parent=settings.get_profile(profile))`` in both modes. Applies
    ``deadline=timeout`` only when ``timeout is not None``. Applies one ``pytest.mark.<m>`` per
    entry in ``markers``, plus ``pytest.mark.mutation`` when ``mutation=True``. Double-decoration is
    rejected via ``__wrapped__`` inspection. All hypothesis test functions must return ``None``
    by hypothesis contract — ``R`` is fixed to ``None``.

    When ``events`` is non-empty and ``given=True``, each callable receives the drawn argument and
    its return value is passed to ``hypothesis.event()``. Per-event frequency appears under
    ``--hypothesis-show-statistics``. Default empty tuple = zero overhead; ``given=False`` ignores
    ``events`` (no drawn arg in scope).

    Args:
        subject: The type or callable whose strategy ``resolve`` resolves (strategy registered on
            first call; ``given=False`` skips strategy resolution).
        given: When True, inject the hypothesis strategy as the rightmost positional argument.
        mutation: When True, also apply ``pytest.mark.mutation``.
        profile: Hypothesis settings profile name (must be pre-registered in conftest).
        markers: Extra pytest mark names to apply.
        timeout: Hypothesis deadline in seconds; ``None`` (default) inherits from the profile.
        law: Override law name in ``MANIFEST``; defaults to the decorated function's ``__name__``.
        events: Callables ``(drawn) -> str``; when non-empty and ``given=True``, each tag is
            reported via ``hypothesis.event()`` on every drawn example.

    Returns:
        A decorator that wraps the function, applies the full mark/settings stack, and registers a
        ``LawRecord`` into ``MANIFEST``.
    """
    # Lazy import avoids top-level circular dependency; _strategies is a sibling test-internal module.
    from tests._strategies import resolve  # noqa: PLC0415, PLC2701

    def _decorator(fn: Callable[P, None]) -> Callable[P, None]:
        # Double-decoration guard: @wraps sets __wrapped__; a second @spec sees it and rejects.
        if hasattr(fn, "__wrapped__"):
            msg = f"@spec double-decoration detected on {fn!r}; remove the duplicate decorator."
            raise TypeError(msg)

        # Apply @given BEFORE @settings: hypothesis must wrap the raw function first.
        # subject must be a type for resolve() to register a bounded strategy.
        assert isinstance(subject, type), f"@spec given=True requires a type, got {subject!r}"
        profile_settings = hyp_settings.get_profile(profile)

        match given:
            case True:
                # When events are registered, wrap fn so each drawn arg is reported via hypothesis.event()
                # before delegation. functools.wraps preserves fn's signature so @given injects correctly;
                # __wrapped__ is set on the wrapper (pointing to fn), not on fn itself — the double-decoration
                # guard already passed and only fires on the incoming fn, not our internal wrapper.
                target = (
                    functools.wraps(fn)(lambda *args, **kwargs: ([hyp_event(tag(args[-1])) for tag in events], fn(*args, **kwargs))[-1])
                    if events
                    else fn
                )
                with_given = hyp_given(resolve(subject))(target)
            case _:
                with_given = fn

        match timeout:
            case None:
                with_settings = hyp_settings(parent=profile_settings)(with_given)
            case _:
                # timeout is float | None; non-None branch is always float per the param annotation.
                with_settings = hyp_settings(parent=profile_settings, deadline=timeout)(with_given)

        all_marks = (*markers, *(("mutation",) if mutation else ()))
        result = functools.reduce(lambda acc, m: getattr(pytest.mark, m)(acc), all_marks, with_settings)

        fn_name: str = getattr(fn, "__name__", repr(fn))
        MANIFEST.append(LawRecord(subject=_qualname(subject), law=law or fn_name, module=getattr(fn, "__module__", "<unknown>")))

        # Return the @given/@settings/@mark stack AS-IS: @given already removed the strategy parameter from the
        # collected signature and set __wrapped__ (the double-decoration sentinel). Re-stamping fn's signature here
        # would re-expose the strategy param and pytest would treat it as a missing fixture.
        return result

    return _decorator


def register_law(subject: object, law: str, *, module: str | None = None) -> None:
    """Imperatively register a law for ``subject`` without decorating a function.

    Use inside ``@pytest.mark.parametrize`` loops or for metamorphic / matrix laws that call
    ``assert_*`` oracles directly without ``@spec``.

    Args:
        subject: The type or callable the law covers.
        law: The law name (usually the function or parametrize ID).
        module: Override the module recorded in ``MANIFEST``; defaults to the caller's ``__name__``.
    """
    frame = inspect.currentframe()
    caller_module = module or (frame.f_back.f_globals.get("__name__", "<unknown>") if frame and frame.f_back else "<unknown>")
    MANIFEST.append(LawRecord(subject=_qualname(subject), law=law, module=caller_module))


def register_laws(*pairs: tuple[object, tuple[str, ...]]) -> None:
    """Batch-register multiple law families at import time.

    A greppable import-time helper that registers many ``LawRecord`` entries without
    per-law ``register_law`` call spam. Intended for families of metamorphic, matrix, or
    parametrized laws that share one subject but declare several law names together.

    Each pair is ``(subject, (law_name, ...))`` where ``subject`` is the type or callable
    covered and each ``law_name`` is a distinct law identifier. The caller module is
    captured once via ``inspect.currentframe().f_back`` and shared across all emitted
    records, matching the behaviour of ``register_law``.

    Args:
        *pairs: Variable-length sequence of ``(subject, (law_name, ...))`` 2-tuples.
    """
    frame = inspect.currentframe()
    caller_module = frame.f_back.f_globals.get("__name__", "<unknown>") if frame and frame.f_back else "<unknown>"
    MANIFEST.extend(
        LawRecord(subject=_qualname(subject), law=law_name, module=caller_module) for subject, law_names in pairs for law_name in law_names
    )


def register_sut(package: str, *, exempt: frozenset[str] = frozenset()) -> None:
    """Record a SUT package for law-coverage gating.

    Called once per project conftest. Multiple calls accumulate packages independently.

    Args:
        package: Fully-qualified package name (e.g. ``"tools.assay"``).
        exempt: Symbol simple-names that are explicitly exempt from law coverage.
    """
    SUT_PACKAGES[package] = exempt


def assert_law_coverage() -> None:
    """Assert every public SUT symbol has at least one registered law or is explicitly exempt.

    Walks each package registered via ``register_sut`` using ``pkgutil.walk_packages``, unions the
    public surface across all leaf modules (``__all__`` union; non-underscore fallback; module objects
    excluded), and asserts that the covered set + exempt set spans the full surface.
    """
    covered = frozenset(_qualname_simple(rec.subject) for rec in MANIFEST)

    for package, exempt in SUT_PACKAGES.items():
        surface = _public_surface(package)
        uncovered = surface - covered - exempt
        assert not uncovered, f"Law coverage gap in '{package}': {len(uncovered)} symbol(s) have no law:\n" + "\n".join(
            f"  - {name}" for name in sorted(uncovered)
        )


__all__ = ["spec", "register_law", "register_laws", "register_sut", "assert_law_coverage", "MANIFEST", "LawRecord"]
