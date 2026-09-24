"""Property-test registration and public-API test coverage accounting."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable, Mapping
import enum
import functools
import importlib
import inspect
from pathlib import Path
import sys
from types import BuiltinFunctionType, ClassMethodDescriptorType, FunctionType, MethodDescriptorType, MethodType, ModuleType, WrapperDescriptorType
from typing import get_args, TypeAliasType, TypeForm, TypeIs

from hypothesis import given as hyp_given
import msgspec
import pytest

from tests.python.support.strategies import strategy_for

# --- [MODELS] ---------------------------------------------------------------------------


class PropertyRecord(msgspec.Struct, frozen=True):
    """Registered property test keyed by subject and test module."""

    subject: str
    property_name: str
    module: str
    subject_module: str | None


class PackageUnderTest(msgspec.Struct, frozen=True):
    """Package registration with explicit exemptions and its test directory."""

    exempt: frozenset[str]
    suite: Path


# --- [STASH] ----------------------------------------------------------------------------

PROPERTY_RECORDS: pytest.StashKey[tuple[PropertyRecord, ...]] = pytest.StashKey()
PACKAGES_UNDER_TEST: pytest.StashKey[frozendict[str, PackageUnderTest]] = pytest.StashKey()

# --- [OPERATIONS] -----------------------------------------------------------------------


def _record(subject: object, property_name: str, module: str) -> PropertyRecord:
    """Build the record of one subject under one property name in one test module, a subject without a defining module renders by ``str``."""
    match subject:
        case type() | TypeAliasType() | FunctionType() | MethodType() | BuiltinFunctionType():
            return PropertyRecord(subject=subject.__qualname__, property_name=property_name, module=module, subject_module=subject.__module__)
        case MethodDescriptorType() | WrapperDescriptorType() | ClassMethodDescriptorType():
            return PropertyRecord(subject=subject.__qualname__, property_name=property_name, module=module, subject_module=subject.__objclass__.__module__)
        case _:
            return PropertyRecord(subject=str(subject), property_name=property_name, module=module, subject_module=None)


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
                callable(member) or isinstance(member, property | classmethod | staticmethod | functools.cached_property)
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
    exports: list[tuple[ModuleType, list[str]]] = []
    failures: list[tuple[str, str]] = []
    for base in root.__path__:
        for py in sorted(Path(base).rglob("*.py")):
            parts = py.relative_to(base).with_suffix("").parts
            stem = parts[:-1] if parts[-1] == "__init__" else parts
            if any(part.startswith("_") for part in stem):
                continue
            mod_name = ".".join((package_name, *stem))
            try:
                module = importlib.import_module(mod_name)
                exports.append((module, module.__all__))
            except Exception as exc:  # ruff:ignore[blind-except]
                failures.append((mod_name, repr(exc)))

    public_api: dict[str, object] = {}
    for mod, names in exports:
        members = dict(inspect.getmembers(mod))
        for name in names:
            if name not in members:
                failures.append((mod.__name__, f"__all__ names {name!r} but the module never defines it"))
            elif not inspect.ismodule(member := members[name]):
                public_api.setdefault(name, member)

    return public_api, tuple(failures)


def property_test[**P](subject: object, *, given: bool = True) -> Callable[[Callable[P, None]], Callable[P, None]]:
    """Register a property test and inject the subject's Hypothesis strategy.

    Args:
        subject: Type or callable covered by the property test.
        given: True injects ``strategy_for(subject)`` as the rightmost positional argument, the subject is then a type form.

    Returns:
        The decorator marking the test with its ``PropertyRecord``, collection records it.
    """

    def _decorator(fn: Callable[P, None]) -> Callable[P, None]:
        if not isinstance(fn, FunctionType):
            raise TypeError(f"@property_test decorates a test function, got {fn!r}")
        record = _record(subject, fn.__qualname__, fn.__module__)
        if not given:
            return pytest.mark.property(record=record)(fn)
        if not _resolvable(subject):
            raise TypeError(f"@property_test given=True requires a resolvable type form, got {subject!r}")
        drawn: Callable[P, None] = hyp_given(strategy_for(subject))(fn)
        return pytest.mark.property(record=record)(drawn)

    return _decorator


def covers(module: str, *subjects: Callable[..., object]) -> list[pytest.MarkDecorator]:
    """Return one property mark per subject for a test module's ``pytestmark``, recording each subject as covered by the module named ``module``."""
    return [pytest.mark.property(record=_record(subject, "covers", module)) for subject in subjects]


def register_package(stash: pytest.Stash, package: str, *, suite: Path, exempt: frozenset[str] = frozenset()) -> None:
    """Register a package for public-API test coverage on the session stash, repeat calls merge exemptions and keep the first test directory.

    Args:
        stash: Session stash the ``PACKAGES_UNDER_TEST`` key lives on.
        package: Fully-qualified package name.
        suite: Package test directory.
        exempt: Public names explicitly exempt from the coverage requirement.
    """
    packages = stash.setdefault(PACKAGES_UNDER_TEST, frozendict())
    registration = PackageUnderTest(exempt | prior.exempt, prior.suite) if (prior := packages.get(package)) is not None else PackageUnderTest(exempt, suite)
    stash[PACKAGES_UNDER_TEST] = packages | {package: registration}


def register_package_tree(config: pytest.Config, source_root: Path, suite_root: Path) -> tuple[str, ...]:
    """Register each Python package directly beneath ``source_root`` under the name its modules import by, with the same-named folder under ``suite_root`` as the test directory.

    A package under a ``pythonpath`` root or outside the rootdir imports by its folder name, any other by its rootdir-relative dotted path.

    Returns:
        The registered names, a directory without Python source does not register.
    """
    authored = tuple(child for child in sorted(source_root.iterdir()) if child.is_dir() and any(child.rglob("*.py"))) if source_root.is_dir() else ()
    names = tuple(child.name if child.parent in config.getini("pythonpath") or not child.is_relative_to(config.rootpath) else ".".join(child.relative_to(config.rootpath).parts) for child in authored)
    for name, child in zip(names, authored, strict=True):
        register_package(config.stash, name, suite=suite_root / child.name)
    return names


def uncollected_test_modules(config: pytest.Config, packages: Mapping[str, PackageUnderTest]) -> dict[str, tuple[str, ...]]:
    """Return package test modules that pytest did not import during collection, their coverage declarations were not recorded.

    Collection imports every selected test module under the dotted name pytest's importlib mode assigns it, a name absent from ``sys.modules`` marks an uncollected module.
    """
    gaps = {
        package: tuple(
            sorted(
                name
                for pattern in config.getini("python_files")
                for py in registration.suite.rglob(pattern)
                if (name := ".".join((py.relative_to(config.rootpath) if py.is_relative_to(config.rootpath) else py).with_suffix("").parts)) not in sys.modules
            )
        )
        for package, registration in packages.items()
    }
    return {package: missing for package, missing in gaps.items() if missing}


def assert_property_coverage(records: tuple[PropertyRecord, ...], packages: Mapping[str, PackageUnderTest], *, only: frozenset[str] | None = None) -> None:
    """Assert every registered public API has a property test or an explicit exemption.

    Args:
        records: Property records collection recorded, the ``PROPERTY_RECORDS`` stash value.
        packages: Registered packages, the ``PACKAGES_UNDER_TEST`` stash value.
        only: Packages to inspect, ``None`` inspects every registration.
    """
    global_covered = frozenset(record.subject.rsplit(".", 1)[-1] for record in records if record.subject_module is None)

    for package, registration in packages.items():
        if only is not None and package not in only:
            continue
        public_api, failures = _public_api(package)
        covered = global_covered | frozenset(
            record.subject.rsplit(".", 1)[-1] for record in records if record.subject_module is not None and (record.subject_module == package or record.subject_module.startswith(f"{package}."))
        )
        uncovered = frozenset(name for name, member in public_api.items() if name not in covered and name not in registration.exempt and not is_automatically_exempt(member))
        gaps = [*(f"  - {name}" for name in sorted(uncovered)), *(f"  ! {mod}: {err}" for mod, err in failures)]
        assert not gaps, f"Property-test coverage gap in '{package}': {len(uncovered)} public symbol(s) are untested and {len(failures)} module(s) failed to import:\n" + "\n".join(gaps)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "property_test",
    "covers",
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
