"""Law decorators, registry records, SUT registration, and coverage gate."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable  # noqa: TC003  # PEP 695 ParamSpec annotations are runtime-evaluated; TYPE_CHECKING guard breaks them
import functools
import importlib
import inspect
from pathlib import Path

from hypothesis import event as hyp_event, given as hyp_given, settings as hyp_settings
import msgspec
import pytest

from tests.python._testkit.runtime import PROFILE_DEFAULT


# --- [MODELS] ---------------------------------------------------------------------------


class LawRecord(msgspec.Struct, frozen=True):
    """Registered law record keyed by subject, law, owner module, and optional subject module."""

    subject: str
    law: str
    module: str
    subject_module: str = ""


# --- [TABLES] ---------------------------------------------------------------------------

MANIFEST: list[LawRecord] = []
SUT_PACKAGES: dict[str, frozenset[str]] = {}

# --- [OPERATIONS] -----------------------------------------------------------------------


def _qualname(subject: object) -> str:
    return getattr(subject, "__qualname__", None) or getattr(subject, "__name__", None) or str(subject)


# The filesystem walk and per-module public-name fold are one import-failure-aware coverage pass.
def _public_surface(package_name: str) -> tuple[frozenset[str], tuple[tuple[str, str], ...]]:  # noqa: PLR0912
    """Collect a package's public symbol names and import failures.

    Returns:
        Public simple-name set and ``(module_name, error)`` import failures.
    """
    root = importlib.import_module(package_name)
    # rglob keeps namespace subpackages visible; pkgutil.walk_packages silently skips them.
    modules = [root]
    failures: list[tuple[str, str]] = []
    for base in getattr(root, "__path__", ()):
        for py in sorted(Path(base).rglob("*.py")):
            parts = py.relative_to(base).with_suffix("").parts
            stem = parts[:-1] if parts[-1] == "__init__" else parts
            mod_name = ".".join((package_name, *stem))
            if mod_name == package_name or any(part.startswith("_") for part in stem):
                continue
            try:
                modules.append(importlib.import_module(mod_name))
            except Exception as exc:  # noqa: BLE001  # accumulated and surfaced by assert_law_coverage, never swallowed
                failures.append((mod_name, repr(exc)))

    surface: set[str] = set()
    for mod in modules:
        all_names: object = getattr(mod, "__all__", None)
        match all_names:
            case list() | tuple() as names:
                surface.update(n for n in names if isinstance(n, str) and not inspect.ismodule(getattr(mod, n, None)))
            case _:
                surface.update(n for n in dir(mod) if not n.startswith("_") and not inspect.ismodule(getattr(mod, n, None)))

    return frozenset(surface), tuple(failures)


def spec[**P](
    subject: object,
    *,
    given: bool = True,
    mutation: bool = False,
    profile: str = PROFILE_DEFAULT,
    markers: tuple[str, ...] = (),
    timeout: float | None = None,
    law: str | None = None,
    events: tuple[Callable[[object], str], ...] = (),
) -> Callable[[Callable[P, None]], Callable[P, None]]:
    """Register a law and optionally wrap it with Hypothesis strategy injection.

    Args:
        subject: Type or callable whose strategy/law coverage is registered.
        given: True injects ``resolve(subject)`` as the rightmost positional argument.
        mutation: True also applies ``pytest.mark.mutation``.
        profile: Registered Hypothesis profile name.
        markers: Extra pytest mark names to apply.
        timeout: Hypothesis deadline in seconds; ``None`` inherits from the profile.
        law: Override law name in ``MANIFEST``; defaults to the decorated function's ``__name__``.
        events: Optional drawn-value event taggers for Hypothesis statistics.

    Returns:
        Decorator that applies the mark/settings stack and emits one ``LawRecord``.
    """

    def _decorator(fn: Callable[P, None]) -> Callable[P, None]:
        if hasattr(fn, "__wrapped__"):
            msg = f"@spec double-decoration detected on {fn!r}; remove the duplicate decorator."
            raise TypeError(msg)

        profile_settings = hyp_settings.get_profile(profile)

        match given:
            case True:
                from tests.python._testkit.strategies import resolve  # noqa: PLC0415  # deferred to break import cycle

                assert isinstance(subject, type), f"@spec given=True requires a type, got {subject!r}"
                # Hypothesis must wrap before settings; wraps preserves the collected signature.
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
                with_settings = hyp_settings(parent=profile_settings, deadline=timeout)(with_given)

        all_marks = (*markers, *(("mutation",) if mutation else ()))
        result = functools.reduce(lambda acc, m: getattr(pytest.mark, m)(acc), all_marks, with_settings)

        fn_name: str = getattr(fn, "__name__", repr(fn))
        MANIFEST.append(
            LawRecord(
                subject=_qualname(subject),
                law=law or fn_name,
                module=getattr(fn, "__module__", "<unknown>"),
                subject_module=getattr(subject, "__module__", "") or "",
            )
        )

        # Keep @given's signature-erased stack so the strategy param does not reappear as a fixture.
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
    MANIFEST.append(LawRecord(subject=_qualname(subject), law=law, module=caller_module, subject_module=getattr(subject, "__module__", "") or ""))


def register_laws(*pairs: tuple[object, tuple[str, ...]]) -> None:
    """Batch-register import-time law families sharing the caller module.

    Args:
        *pairs: Variable-length sequence of ``(subject, (law_name, ...))`` 2-tuples.
    """
    frame = inspect.currentframe()
    caller_module = frame.f_back.f_globals.get("__name__", "<unknown>") if frame and frame.f_back else "<unknown>"
    MANIFEST.extend(
        LawRecord(subject=_qualname(subject), law=law_name, module=caller_module, subject_module=getattr(subject, "__module__", "") or "")
        for subject, law_names in pairs
        for law_name in law_names
    )


def register_sut(package: str, *, exempt: frozenset[str] = frozenset()) -> None:
    """Record a SUT package for law-coverage gating.

    Called once per project conftest. Multiple calls accumulate packages independently; a duplicate
    registration of the same package merges exempt sets (idempotent under repeated conftest import).

    Args:
        package: Fully-qualified package name (e.g. ``"tools.<package>"``).
        exempt: Symbol simple-names that are explicitly exempt from law coverage.
    """
    SUT_PACKAGES[package] = SUT_PACKAGES.get(package, frozenset()) | exempt


def assert_law_coverage() -> None:
    """Assert every registered SUT public symbol is law-covered or explicitly exempt."""
    global_covered = frozenset(rec.subject.rsplit(".", 1)[-1] for rec in MANIFEST if not rec.subject_module)

    for package, exempt in SUT_PACKAGES.items():
        surface, failures = _public_surface(package)
        covered = global_covered | frozenset(
            rec.subject.rsplit(".", 1)[-1] for rec in MANIFEST if rec.subject_module == package or rec.subject_module.startswith(f"{package}.")
        )
        uncovered = surface - covered - exempt
        gaps = [*(f"  - {name}" for name in sorted(uncovered)), *(f"  ! {mod}: {err}" for mod, err in failures)]
        assert not gaps, (
            f"Law coverage gap in '{package}': {len(uncovered)} symbol(s) have no law, {len(failures)} module import failure(s):\n" + "\n".join(gaps)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["spec", "register_law", "register_laws", "register_sut", "assert_law_coverage", "MANIFEST", "LawRecord"]
