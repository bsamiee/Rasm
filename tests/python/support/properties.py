"""Property-test registration and public-API test coverage accounting."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable, Mapping
from datetime import timedelta
import enum
import functools
import importlib
import inspect
from pathlib import Path
import sys
from typing import get_args, TypeAliasType, TypeForm, TypeIs

from hypothesis import event as hyp_event, given as hyp_given, settings as hyp_settings
import msgspec
import pytest

from tests.python.support.runtime import REPO_ROOT
lazy from tests.python.support.strategies import strategy_for

# --- [CONSTANTS] ------------------------------------------------------------------------

_TEST_FILE_GLOBS: tuple[str, ...] = ("test_*.py", "*_test.py")
_IMPORT_ROOTS: frozenset[Path] = frozenset({REPO_ROOT, REPO_ROOT / "libs" / "python"})  # The pythonpath rows of the root pyproject.toml

# --- [MODELS] ---------------------------------------------------------------------------


class PropertyRecord(msgspec.Struct, frozen=True):
    """Registered property test keyed by subject and test module."""

    subject: str
    property_name: str
    module: str
    subject_module: str = ""


class PackageUnderTest(msgspec.Struct, frozen=True):
    """Package registration with explicit exemptions and its test directory."""

    exempt: frozenset[str] = frozenset()
    suite: Path | None = None


# --- [STASH] ----------------------------------------------------------------------------

PROPERTY_RECORDS: pytest.StashKey[tuple[PropertyRecord, ...]] = (
    pytest.StashKey()
)  # Written once at collection from the property marks and the COVERS tuples
PACKAGES_UNDER_TEST: pytest.StashKey[frozendict[str, PackageUnderTest]] = pytest.StashKey()  # Written by register_package at configure

# --- [OPERATIONS] -----------------------------------------------------------------------


def _qualname(subject: object) -> str:
    return getattr(subject, "__qualname__", None) or getattr(subject, "__name__", None) or str(subject)


def _resolvable(subject: object) -> TypeIs[TypeForm[object]]:
    """Return whether Hypothesis can construct a strategy for the subject."""
    return isinstance(subject, type | TypeAliasType) or bool(get_args(subject))


def is_automatically_exempt(subject: object) -> bool:
    """Return whether public-API test coverage excludes a value-only symbol."""
    match subject:
        case type() if issubclass(subject, enum.StrEnum):
            return True
        case type() if issubclass(subject, msgspec.Struct):
            declared = any(
                callable(member) or isinstance(member, (property, classmethod, staticmethod, functools.cached_property))
                for klass in subject.__mro__
                if klass not in {msgspec.Struct, object}
                for name, member in vars(klass).items()
                if name == "__post_init__" or not name.startswith("__")
            )
            return bool(subject.__struct_config__.frozen) and not declared
        case type():
            return False
        case TypeAliasType():
            return True
        case _:
            return type(subject).__module__ == "typing" or not callable(subject)


def _public_api(package_name: str) -> tuple[dict[str, object], tuple[tuple[str, str], ...]]:
    """Collect public names and module-import failures for a package."""
    root = importlib.import_module(package_name)
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
            except Exception as exc:  # ruff:ignore[blind-except]
                failures.append((mod_name, repr(exc)))

    public_api: dict[str, object] = {}
    for mod in modules:
        all_names: object = getattr(mod, "__all__", None)
        names = (
            [n for n in all_names if isinstance(n, str)] if isinstance(all_names, (list, tuple)) else [n for n in dir(mod) if not n.startswith("_")]
        )
        for name in names:
            if not hasattr(mod, name):
                failures.append((getattr(mod, "__name__", "<module>"), f"__all__ names {name!r} but the module never defines it"))
            elif not inspect.ismodule(member := getattr(mod, name)):
                public_api.setdefault(name, member)

    return public_api, tuple(failures)


def property_test[**P](
    subject: object,
    *,
    given: bool = True,
    profile: str | None = None,
    markers: tuple[str, ...] = (),
    timeout: float | None = None,
    property_name: str | None = None,
    events: tuple[Callable[[object], str], ...] = (),
) -> Callable[[Callable[P, None]], Callable[P, None]]:
    """Register a property test and optionally inject a Hypothesis strategy.

    Args:
        subject: Type or callable covered by the property test.
        given: True injects ``strategy_for(subject)`` as the rightmost positional argument.
        profile: Registered Hypothesis profile name to pin, ``None`` follows the session-active profile.
        markers: Extra pytest mark names to apply.
        timeout: Hypothesis deadline in seconds, ``None`` inherits from the active profile.
        property_name: Recorded property name, ``None`` uses the function name.
        events: Drawn-value event taggers for Hypothesis statistics.

    Returns:
        The decorator marking the test with its ``PropertyRecord``, collection records it.
    """

    def _decorator(fn: Callable[P, None]) -> Callable[P, None]:
        if any(mark.name == "property" and "record" in mark.kwargs for mark in getattr(fn, "pytestmark", ())):
            raise TypeError(f"@property_test applied twice to {fn!r}, remove the duplicate decorator")

        match given:
            case True:
                if not _resolvable(subject):
                    raise TypeError(f"@property_test given=True requires a resolvable type form, got {subject!r}")
                drawn = next(reversed(inspect.signature(fn).parameters), "")
                target = (
                    functools.wraps(fn)(
                        lambda *args, **kwargs: (
                            [hyp_event(tag(kwargs[drawn] if drawn in kwargs else args[-1])) for tag in events],
                            fn(*args, **kwargs),
                        )[-1]
                    )
                    if events
                    else fn
                )
                with_given = hyp_given(strategy_for(subject))(target)
            case _:
                with_given = fn

        pinned = hyp_settings.get_profile(profile) if profile is not None else None
        deadline = timedelta(seconds=timeout) if timeout is not None else None
        match (pinned, deadline):
            case (None, None):
                with_settings = with_given
            case (None, ceiling):
                with_settings = hyp_settings(deadline=ceiling)(with_given)
            case (parent, None):
                with_settings = hyp_settings(parent=parent)(with_given)
            case (parent, ceiling):
                with_settings = hyp_settings(parent=parent, deadline=ceiling)(with_given)

        record = PropertyRecord(
            subject=_qualname(subject),
            property_name=property_name or getattr(fn, "__name__", repr(fn)),
            module=getattr(fn, "__module__", "<unknown>"),
            subject_module=getattr(subject, "__module__", "") or "",
        )
        marked: Callable[P, None] = functools.reduce(lambda acc, m: getattr(pytest.mark, m)(acc), markers, with_settings)
        return pytest.mark.property(record=record)(marked)

    return _decorator


def record_coverage_declarations(module: object) -> tuple[PropertyRecord, ...]:
    """Return the records a test module's declarative ``COVERS`` tuple declares.

    Raises:
        TypeError: A ``COVERS`` entry is neither a type nor a callable.
    """
    name: str = getattr(module, "__name__", "")
    covers: tuple[object, ...] = getattr(module, "COVERS", ()) if name else ()
    match [subject for subject in covers if not (isinstance(subject, type) or inspect.isroutine(subject))]:
        case [value, *_]:
            raise TypeError(f"COVERS in {name} lists {value!r}: entries must be types or callables")
        case _:
            return tuple(
                PropertyRecord(
                    subject=_qualname(subject), property_name="covers", module=name, subject_module=getattr(subject, "__module__", "") or ""
                )
                for subject in covers
            )


def register_package(stash: pytest.Stash, package: str, *, suite: Path, exempt: frozenset[str] = frozenset()) -> None:
    """Register a package for public-API test coverage on the session stash, repeat calls merge exemptions and keep the first test directory.

    Args:
        stash: Session stash the ``PACKAGES_UNDER_TEST`` key lives on.
        package: Fully-qualified package name.
        suite: Package test directory.
        exempt: Public names explicitly exempt from the coverage requirement.
    """
    packages = stash.get(PACKAGES_UNDER_TEST, frozendict())
    prior = packages.get(package)
    registration = PackageUnderTest(
        exempt=(prior.exempt if prior is not None else frozenset()) | exempt, suite=suite if prior is None or prior.suite is None else prior.suite
    )
    stash[PACKAGES_UNDER_TEST] = packages | {package: registration}


def _importable(folder: Path) -> str:
    """Return the import name of a package directory, its name under an import root and its repository-relative dotted path elsewhere."""
    if folder.parent in _IMPORT_ROOTS or not folder.is_relative_to(REPO_ROOT):
        return folder.name
    return ".".join(folder.relative_to(REPO_ROOT).parts)


def register_package_tree(stash: pytest.Stash, source_root: Path, suite_root: Path) -> tuple[str, ...]:
    """Register each Python package directly beneath ``source_root`` under the name its modules import by, with the same-named folder under ``suite_root`` as the test directory.

    Returns:
        The registered names, a directory without Python source does not register.
    """
    children = sorted(p for p in source_root.iterdir() if p.is_dir()) if source_root.is_dir() else []
    authored = tuple(child for child in children if any(child.rglob("*.py")))
    names = tuple(_importable(child) for child in authored)
    for name, child in zip(names, authored, strict=True):
        register_package(stash, name, suite=suite_root / child.name)
    return names


def _module_name(py: Path) -> str:
    """Return the dotted name pytest importlib mode assigns a test module."""
    return ".".join((py.relative_to(REPO_ROOT) if py.is_relative_to(REPO_ROOT) else py).with_suffix("").parts)


def _test_modules(suite: Path) -> frozenset[str]:
    return frozenset(_module_name(py) for pattern in _TEST_FILE_GLOBS for py in suite.rglob(pattern))


def uncollected_test_modules(packages: Mapping[str, PackageUnderTest]) -> dict[str, tuple[str, ...]]:
    """Return package test modules that pytest did not import during collection, their coverage declarations were not recorded.

    Collection imports every selected test module, a dotted name absent from ``sys.modules`` marks an uncollected module.
    """
    gaps = {
        package: tuple(sorted(name for name in _test_modules(registration.suite) if name not in sys.modules))
        for package, registration in packages.items()
        if registration.suite is not None
    }
    return {package: missing for package, missing in gaps.items() if missing}


def assert_property_coverage(
    records: tuple[PropertyRecord, ...], packages: Mapping[str, PackageUnderTest], *, only: frozenset[str] | None = None
) -> None:
    """Assert every registered public API has a property test or an explicit exemption.

    Args:
        records: Property records collection recorded, the ``PROPERTY_RECORDS`` stash value.
        packages: Registered packages, the ``PACKAGES_UNDER_TEST`` stash value.
        only: Packages to inspect, ``None`` inspects every registration.
    """
    global_covered = frozenset(record.subject.rsplit(".", 1)[-1] for record in records if not record.subject_module)

    for package, registration in packages.items():
        if only is not None and package not in only:
            continue
        public_api, failures = _public_api(package)
        covered = global_covered | frozenset(
            record.subject.rsplit(".", 1)[-1]
            for record in records
            if record.subject_module == package or record.subject_module.startswith(f"{package}.")
        )
        uncovered = frozenset(
            name
            for name, member in public_api.items()
            if name not in covered and name not in registration.exempt and not is_automatically_exempt(member)
        )
        gaps = [*(f"  - {name}" for name in sorted(uncovered)), *(f"  ! {mod}: {err}" for mod, err in failures)]
        assert not gaps, (
            f"Property-test coverage gap in '{package}': {len(uncovered)} public symbol(s) are untested and {len(failures)} module(s) failed to import:\n"
            + "\n".join(gaps)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "property_test",
    "record_coverage_declarations",
    "register_package",
    "register_package_tree",
    "assert_property_coverage",
    "is_automatically_exempt",
    "uncollected_test_modules",
    "PROPERTY_RECORDS",
    "PACKAGES_UNDER_TEST",
    "PropertyRecord",
    "PackageUnderTest",
]
