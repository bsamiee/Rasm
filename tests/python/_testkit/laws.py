"""Law registration via ``@spec`` + declarative ``COVERS``, auto-exemption, and the coverage census gate."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable  # noqa: TC003  # PEP 695 ParamSpec annotations are runtime-evaluated; TYPE_CHECKING guard breaks them
import enum
import functools
import importlib
import inspect
from pathlib import Path
from typing import TypeAliasType

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
_CONSUMED: set[str] = set()

# --- [OPERATIONS] -----------------------------------------------------------------------


def _qualname(subject: object) -> str:
    return getattr(subject, "__qualname__", None) or getattr(subject, "__name__", None) or str(subject)


def auto_exempt(subject: object) -> bool:
    """Decide whether a public symbol needs no law: StrEnum, method-free frozen struct, or value-only object.

    Returns:
        True when the census must not demand a law for ``subject``.
    """
    match subject:
        case type() if issubclass(subject, enum.StrEnum):
            return True
        case type() if issubclass(subject, msgspec.Struct):
            declared = any(
                callable(member) or isinstance(member, (property, classmethod, staticmethod, functools.cached_property))
                for klass in subject.__mro__
                if klass not in {msgspec.Struct, object}
                for name, member in vars(klass).items()
                if not name.startswith("__")
            )
            return bool(subject.__struct_config__.frozen) and not declared
        case type():
            return False
        case TypeAliasType():
            return True
        case _:
            # Value-only symbols: constants, tables, codecs, ContextVars, typing aliases — anything neither class nor callable.
            return type(subject).__module__ == "typing" or not callable(subject)


# The filesystem walk and per-module public-name fold are one import-failure-aware coverage pass.
def _public_surface(package_name: str) -> tuple[dict[str, object], tuple[tuple[str, str], ...]]:
    """Collect a package's public symbols and import failures.

    Returns:
        Public simple-name → object mapping and ``(module_name, error)`` import failures.
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

    surface: dict[str, object] = {}
    for mod in modules:
        all_names: object = getattr(mod, "__all__", None)
        names = (
            [n for n in all_names if isinstance(n, str)] if isinstance(all_names, (list, tuple)) else [n for n in dir(mod) if not n.startswith("_")]
        )
        for name in names:
            member = getattr(mod, name, None)
            surface.setdefault(name, member) if not inspect.ismodule(member) else None

    return surface, tuple(failures)


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


def consume_covers(module: object) -> None:
    """Fold a test module's declarative ``COVERS`` tuple into the manifest, once per module.

    The runtime plugin calls this for every collected test module.

    Raises:
        TypeError: When a ``COVERS`` entry is neither a type nor a callable — value-only symbols
            are auto-exempt and never need census credit.
    """
    name: str = getattr(module, "__name__", "")
    covers = getattr(module, "COVERS", None)
    if not name or name in _CONSUMED or covers is None:
        return
    _CONSUMED.add(name)
    for subject in covers:
        if not (isinstance(subject, type) or inspect.isroutine(subject)):
            msg = f"COVERS in {name} lists {subject!r}: entries must be types or callables; value-only symbols are auto-exempt"
            raise TypeError(msg)
        MANIFEST.append(LawRecord(subject=_qualname(subject), law="covers", module=name, subject_module=getattr(subject, "__module__", "") or ""))


def register_sut(package: str, *, exempt: frozenset[str] = frozenset()) -> None:
    """Record a SUT package for law-coverage gating.

    Called once per project conftest. Multiple calls accumulate packages independently; a duplicate
    registration of the same package merges exempt sets (idempotent under repeated conftest import).

    Args:
        package: Fully-qualified package name (e.g. ``"tools.<package>"``).
        exempt: Symbol simple-names that are explicitly exempt from law coverage, each with a
            standing justification at the call site; auto-exempt symbols never need listing.
    """
    SUT_PACKAGES[package] = SUT_PACKAGES.get(package, frozenset()) | exempt


def assert_law_coverage() -> None:
    """Assert every registered SUT public symbol is law-covered, auto-exempt, or explicitly exempt."""
    global_covered = frozenset(rec.subject.rsplit(".", 1)[-1] for rec in MANIFEST if not rec.subject_module)

    for package, exempt in SUT_PACKAGES.items():
        surface, failures = _public_surface(package)
        covered = global_covered | frozenset(
            rec.subject.rsplit(".", 1)[-1] for rec in MANIFEST if rec.subject_module == package or rec.subject_module.startswith(f"{package}.")
        )
        uncovered = frozenset(name for name, member in surface.items() if name not in covered and name not in exempt and not auto_exempt(member))
        gaps = [*(f"  - {name}" for name in sorted(uncovered)), *(f"  ! {mod}: {err}" for mod, err in failures)]
        assert not gaps, (
            f"Law coverage gap in '{package}': {len(uncovered)} symbol(s) have no law, {len(failures)} module import failure(s):\n" + "\n".join(gaps)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["spec", "consume_covers", "register_sut", "assert_law_coverage", "auto_exempt", "MANIFEST", "LawRecord"]
