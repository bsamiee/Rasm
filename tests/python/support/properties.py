"""Property-test registration and public-API test coverage accounting."""

from collections.abc import Mapping
from copy import replace
import enum
import functools
import importlib
import inspect
from pathlib import Path
import sys
from types import BuiltinFunctionType, ClassMethodDescriptorType, FunctionType, MethodDescriptorType, MethodType, WrapperDescriptorType
from typing import TypeAliasType

import msgspec
import pytest

# --- [MODELS] ---------------------------------------------------------------------------


class PropertyRecord(msgspec.Struct, frozen=True):
    """Public subject covered by a property mark."""

    subject: str
    subject_module: str | None


class PackageUnderTest(msgspec.Struct, frozen=True):
    """Package registration with explicit exemptions and its test directory."""

    exempt: frozenset[str]
    suite: Path


# --- [STASH] ----------------------------------------------------------------------------

PROPERTY_RECORDS: pytest.StashKey[tuple[PropertyRecord, ...]] = pytest.StashKey()
PACKAGES_UNDER_TEST: pytest.StashKey[frozendict[str, PackageUnderTest]] = pytest.StashKey()

# --- [OPERATIONS] -----------------------------------------------------------------------


def is_automatically_exempt(subject: object) -> bool:
    """Return whether public-API test coverage excludes a value-only symbol."""
    match subject:
        case type() if issubclass(subject, enum.StrEnum):
            return True
        case type() if issubclass(subject, msgspec.Struct):
            return subject.__struct_config__.frozen and not any(
                callable(member) or isinstance(member, property | classmethod | functools.cached_property)
                for klass in subject.__mro__
                for name, member in vars(klass).items()
                if name == "__post_init__" or not name.startswith("__")
            )
        case type():
            return False
        case TypeAliasType():
            return True
        case _:
            return type(subject).__module__ == "typing" or not callable(subject)


def property_test(subject: object) -> pytest.MarkDecorator:
    """Return a property mark recording the covered subject's public-API identity."""
    match subject:
        case type() | TypeAliasType() | FunctionType() | MethodType() | BuiltinFunctionType():
            record = PropertyRecord(subject=subject.__qualname__, subject_module=subject.__module__)
        case MethodDescriptorType() | WrapperDescriptorType() | ClassMethodDescriptorType():
            record = PropertyRecord(subject=subject.__qualname__, subject_module=subject.__objclass__.__module__)
        case _:
            record = PropertyRecord(subject=str(subject), subject_module=None)
    return pytest.mark.property(record=record)


def covers(*subjects: object) -> list[pytest.MarkDecorator]:
    """Return property marks for module-level ``pytestmark`` coverage."""
    return [property_test(subject) for subject in subjects]


def register_package(stash: pytest.Stash, package: str, *, suite: Path, exempt: frozenset[str] = frozenset()) -> None:
    """Register a package for public-API test coverage on the session stash, a repeat call merging exemptions and keeping the first test directory."""
    packages = stash.get(PACKAGES_UNDER_TEST, frozendict())
    prior = packages.get(package, PackageUnderTest(frozenset(), suite))
    stash[PACKAGES_UNDER_TEST] = packages | {package: replace(prior, exempt=prior.exempt | exempt)}


def register_package_tree(stash: pytest.Stash, module_root: Path, suite_root: Path) -> None:
    """Register each directory beneath the import root ``module_root`` that holds Python source under its import name, the directory name, with the same-named ``suite_root`` folder as test directory."""
    for package in (child for child in sorted(module_root.iterdir()) if child.is_dir() and any(child.rglob("*.py"))):
        register_package(stash, package.name, suite=suite_root / package.name)


def uncollected_test_modules(config: pytest.Config, packages: Mapping[str, PackageUnderTest]) -> dict[str, tuple[str, ...]]:
    """Return the package test modules pytest did not import during collection."""
    names = {
        package: tuple(sorted(".".join(py.relative_to(config.rootpath).with_suffix("").parts) for pattern in config.getini("python_files") for py in registration.suite.rglob(pattern)))
        for package, registration in packages.items()
    }
    return {package: missing for package, modules in names.items() if (missing := tuple(module for module in modules if module not in sys.modules))}


def assert_property_coverage(records: tuple[PropertyRecord, ...], packages: Mapping[str, PackageUnderTest]) -> None:
    """Assert every public API of each registered package has a property test or an exemption, and every public module imports and states ``__all__``."""

    def surveyed(module: str) -> tuple[dict[str, object], tuple[str, ...]]:
        try:
            members = dict(inspect.getmembers(loaded := importlib.import_module(module)))
        except (ImportError, SyntaxError) as error:
            return {}, (repr(error),)
        match vars(loaded).get("__all__"):
            case None:
                return {}, ("the module defines no __all__",)
            case names:
                return {name: members[name] for name in names if name in members and not inspect.ismodule(members[name])}, tuple(
                    f"__all__ names {name!r} but the module never defines it" for name in names if name not in members
                )

    for package, registration in packages.items():
        modules = (
            ".".join((package, *py.relative_to(base).with_suffix("").parts)).removesuffix(".__init__") for base in importlib.import_module(package).__path__ for py in sorted(Path(base).rglob("*.py"))
        )
        surveys = {module: surveyed(module) for module in modules if not any(part.startswith("_") for part in module.removeprefix(package).split("."))}
        failures = [f"  ! {module}: {failure}" for module, (_, faults) in surveys.items() for failure in faults]
        covered = registration.exempt | {record.subject.rsplit(".", 1)[-1] for record in records if record.subject_module is None or f"{record.subject_module}.".startswith(f"{package}.")}
        uncovered = sorted({f"  - {name}" for exports, _ in surveys.values() for name, member in exports.items() if name not in covered and not is_automatically_exempt(member)})
        gaps = uncovered + failures
        assert not gaps, f"Property-test coverage gap in '{package}': {len(uncovered)} untested public symbol(s) and {len(failures)} module fault(s):\n{'\n'.join(gaps)}"


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "PACKAGES_UNDER_TEST",
    "PROPERTY_RECORDS",
    "PackageUnderTest",
    "PropertyRecord",
    "assert_property_coverage",
    "covers",
    "is_automatically_exempt",
    "property_test",
    "register_package",
    "register_package_tree",
    "uncollected_test_modules",
]
