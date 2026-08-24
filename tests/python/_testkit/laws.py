"""Law registration via ``@spec`` + declarative ``COVERS``, auto-exemption, and the coverage census gate."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable  # PEP 695 ParamSpec annotations are runtime-evaluated; TYPE_CHECKING guard breaks them
from datetime import timedelta
import enum
import functools
import importlib
import inspect
from pathlib import Path
import sys
from typing import get_args, TypeAliasType, TypeForm, TypeIs
import weakref

from hypothesis import event as hyp_event, given as hyp_given, settings as hyp_settings
import msgspec
import pytest
from ruamel.yaml import YAML

from tests.python._testkit.runtime import REPO_ROOT
lazy from tests.python._testkit.strategies import resolve  # strategies imports this module; the lazy binding breaks the cycle at first draw


# --- [CONSTANTS] ------------------------------------------------------------------------

# bench_*.py are measurement sessions, never census members; laws live only in these module shapes.
_LAW_GLOBS: tuple[str, ...] = ("test_*.py", "*_test.py")

# Distinguishes a genuinely-None public value (value-only exempt) from an attribute __all__ promises but the module never defines.
_ABSENT: object = object()

# The estate generation template is the one authority on generated out roots; a tree beneath one is emitted, never authored, so it owes no suite.
_TEMPLATE: Path = REPO_ROOT / "libs" / "contracts" / "buf.gen.yaml"
_OUT_BASE: Path = REPO_ROOT  # template `out` rows are repo-relative, so they resolve here rather than beside the template

# --- [MODELS] ---------------------------------------------------------------------------


class LawRecord(msgspec.Struct, frozen=True):
    """Registered law record keyed by subject, law, owner module, and optional subject module."""

    subject: str
    law: str
    module: str
    subject_module: str = ""


class Sut(msgspec.Struct, frozen=True):
    """SUT registration: explicit exemptions plus the suite directory whose law modules census the package."""

    exempt: frozenset[str] = frozenset()
    suite: Path | None = None


# --- [TABLES] ---------------------------------------------------------------------------

MANIFEST: list[LawRecord] = []
SUT_PACKAGES: dict[str, Sut] = {}
_CONSUMED: set[str] = set()
_STAMPED: weakref.WeakSet[object] = weakref.WeakSet()

# --- [OPERATIONS] -----------------------------------------------------------------------


def _qualname(subject: object) -> str:
    return getattr(subject, "__qualname__", None) or getattr(subject, "__name__", None) or str(subject)


def _resolvable(subject: object) -> TypeIs[TypeForm[object]]:
    """Decide whether ``resolve`` can own the subject: classes, PEP 695 aliases, and parameterized forms.

    Returns:
        True when the subject is a strategy-resolvable type form; bare callables and values refuse.
    """
    return isinstance(subject, type | TypeAliasType) or bool(get_args(subject))


def auto_exempt(subject: object) -> bool:
    """Decide whether a public symbol needs no law: StrEnum, method-free frozen struct, or value-only object.

    Returns:
        True when the census must not demand a law for ``subject``.
    """
    match subject:
        case type() if issubclass(subject, enum.StrEnum):
            return True
        case type() if issubclass(subject, msgspec.Struct):
            # __post_init__ is admission behavior: a frozen struct validating itself still needs a law.
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
            # Value-only symbols: constants, tables, codecs, ContextVars, typing aliases — anything neither class nor callable.
            return type(subject).__module__ == "typing" or not callable(subject)


def generated_roots(source_root: Path) -> frozenset[Path]:
    """Resolve the generation template's ``plugins[].out`` directories lying under ``source_root``.

    Returns:
        Out roots resolved against the out base (template ``out`` rows are repo-relative); empty when no template or no root lies beneath it.
    """
    match YAML(typ="safe").load(_TEMPLATE) if _TEMPLATE.is_file() else None:
        case {"plugins": [*rows]}:
            outs = ((_OUT_BASE / out).resolve() for row in rows if isinstance(row, dict) and isinstance(out := row.get("out"), str))
            return frozenset(root for root in outs if root.is_relative_to(source_root))
        case _:
            return frozenset()


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
        # Generation authors every module beneath an out root, so those symbols owe no law here for the same
        # reason register_tree grants their folder no suite: one template, one authority, both census grains.
        emitted = generated_roots(Path(base).resolve())
        for py in sorted(Path(base).rglob("*.py")):
            parts = py.relative_to(base).with_suffix("").parts
            stem = parts[:-1] if parts[-1] == "__init__" else parts
            mod_name = ".".join((package_name, *stem))
            if mod_name == package_name or any(part.startswith("_") for part in stem):
                continue
            if any(py.resolve().is_relative_to(out) for out in emitted):
                continue
            try:
                modules.append(importlib.import_module(mod_name))
            except Exception as exc:  # ruff:ignore[blind-except]  # accumulated and surfaced by assert_law_coverage, never swallowed
                failures.append((mod_name, repr(exc)))

    surface: dict[str, object] = {}
    for mod in modules:
        all_names: object = getattr(mod, "__all__", None)
        names = (
            [n for n in all_names if isinstance(n, str)] if isinstance(all_names, (list, tuple)) else [n for n in dir(mod) if not n.startswith("_")]
        )
        for name in names:
            member = getattr(mod, name, _ABSENT)
            if member is _ABSENT:
                # A phantom export is a broken public surface, never a silent value-only exemption.
                failures.append((getattr(mod, "__name__", "<module>"), f"__all__ names {name!r} but the module never defines it"))
            elif not inspect.ismodule(member):
                surface.setdefault(name, member)

    return surface, tuple(failures)


def spec[**P](
    subject: object,
    *,
    given: bool = True,
    mutation: bool = False,
    profile: str | None = None,
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
        profile: Registered Hypothesis profile name to pin; ``None`` follows the session-active
            profile, so mutation/CI/stress lanes govern undecorated laws through the CLI profile.
        markers: Extra pytest mark names to apply.
        timeout: Hypothesis deadline in seconds; ``None`` inherits from the governing profile.
        law: Override law name in ``MANIFEST``; defaults to the decorated function's ``__name__``.
        events: Optional drawn-value event taggers for Hypothesis statistics.

    Returns:
        Decorator that applies the mark/settings stack and emits one ``LawRecord``.
    """

    def _decorator(fn: Callable[P, None]) -> Callable[P, None]:
        if fn in _STAMPED:
            msg = f"@spec double-decoration detected on {fn!r}; remove the duplicate decorator."
            raise TypeError(msg)

        match given:
            case True:
                # The registration algebra matches the resolver's: classes, PEP 695 aliases, and
                # parameterized forms (unions, Literal, Annotated) all inject; bare callables refuse.
                if not _resolvable(subject):
                    msg = f"@spec given=True requires a resolvable type form, got {subject!r}"
                    raise TypeError(msg)
                # Hypothesis maps the positional strategy to fn's rightmost parameter and injects it as
                # a KEYWORD argument, so event taggers read that name; wraps preserves the collected signature.
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
                with_given = hyp_given(resolve(subject))(target)
            case _:
                with_given = fn

        # Explicit settings attach only for a named pin or a deadline; hypothesis numeric deadlines are
        # MILLISECONDS, so seconds convert here — and an unpinned law stays governed by the active profile.
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

        all_marks = (*markers, *(("mutation",) if mutation else ()))
        result = functools.reduce(lambda acc, m: getattr(pytest.mark, m)(acc), all_marks, with_settings)
        _STAMPED.add(result)  # identity stamp: the only durable double-decoration witness (@given sets no __wrapped__)

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


def register_sut(package: str, *, exempt: frozenset[str] = frozenset(), suite: Path | None = None) -> None:
    """Record a SUT package for law-coverage gating.

    Called once per project conftest. A duplicate registration merges exempt sets and keeps the
    first resolved suite (idempotent under repeated conftest import).

    Args:
        package: Fully-qualified package name (e.g. ``"tools.<package>"``).
        exempt: Symbol simple-names that are explicitly exempt from law coverage, each with a
            standing justification at the call site; auto-exempt symbols never need listing.
        suite: Directory whose law modules carry this package's census; ``None`` derives the
            calling conftest's directory, so subset-collection detection needs no caller edits.
    """
    frame = inspect.currentframe()
    caller_file = frame.f_back.f_globals.get("__file__") if frame is not None and frame.f_back is not None else None
    derived = suite if suite is not None else (Path(caller_file).resolve().parent if isinstance(caller_file, str) else None)
    prior = SUT_PACKAGES.get(package)
    SUT_PACKAGES[package] = Sut(
        exempt=(prior.exempt if prior is not None else frozenset()) | exempt,
        suite=prior.suite if prior is not None and prior.suite is not None else derived,
    )


def _importable(folder: Path, /) -> str:
    """Name the package a source folder installs, from disk shape alone.

    A workspace member installs the shallowest ``__init__.py`` package beneath its module root — the
    member folder itself under the estate's flat no-``src`` law (``module-root = ""``), or ``src`` where
    a foreign checkout still carries one — so the dotted name is that package's path under the root, the
    one name its modules already carry once installed. Registering the folder path instead imports the
    whole tree a second time under a shadow name, minting duplicate classes, descriptors, and limiters
    beside the live ones. A manifest-less flat folder keeps its repo-relative dotted path, which is what
    a ``sys.path``-prepended root already resolves.

    Returns:
        The dotted name under which this folder's modules import.
    """
    src = folder / "src"
    base = src if src.is_dir() else (folder if (folder / "pyproject.toml").is_file() else None)
    if base is not None:
        installed = sorted((py.parent for py in base.rglob("__init__.py")), key=lambda root: len(root.parts))
        if installed:
            return ".".join(installed[0].relative_to(base).parts)
    return ".".join(folder.relative_to(REPO_ROOT).parts) if folder.is_relative_to(REPO_ROOT) else folder.name


def register_tree(source_root: Path, suite_root: Path) -> tuple[str, ...]:
    """Register every authored-source package folder under ``source_root`` from disk shape.

    A child folder registers only when it carries Python source outside every generated out root
    the generation template declares; it registers under the name its modules import by, and its
    suite is the same-named folder under ``suite_root``. A sourceless folder never registers, so a
    planning-only tree adds zero census obligations until code lands; a folder whose source is the
    generator's emission alone is no stratum — generation is its author, so it owes no suite and no
    census.

    Returns:
        Registered dotted package names in folder order; empty when no folder carries authored source.
    """
    children = sorted(p for p in source_root.iterdir() if p.is_dir()) if source_root.is_dir() else []
    roots = generated_roots(source_root.resolve())
    authored = tuple(child for child in children if any(not any(py.resolve().is_relative_to(root) for root in roots) for py in child.rglob("*.py")))
    names = tuple(_importable(child) for child in authored)
    for name, child in zip(names, authored, strict=True):
        register_sut(name, suite=suite_root / child.name)
    return names


def _module_name(py: Path) -> str:
    """Dotted name pytest's importlib mode assigns a repo law module.

    Returns:
        Repo-relative dotted module name (absolute parts when the file lives outside the repo).
    """
    return ".".join((py.relative_to(REPO_ROOT) if py.is_relative_to(REPO_ROOT) else py).with_suffix("").parts)


def _law_modules(suite: Path) -> frozenset[str]:
    """Dotted names of every on-disk law module under ``suite``.

    Returns:
        Frozen set of dotted module names.
    """
    return frozenset(_module_name(py) for pattern in _LAW_GLOBS for py in suite.rglob(pattern))


def uncollected_laws() -> dict[str, tuple[str, ...]]:
    """Report per-package law modules on disk but never imported by this session's collection.

    Collection imports every selected law module, so a dotted name absent from ``sys.modules``
    means that module's laws never reached the manifest and the package census would report
    false gaps.

    Returns:
        Package name to sorted missing dotted module names; empty when every census is whole.
    """
    gaps = {
        package: tuple(sorted(name for name in _law_modules(sut.suite) if name not in sys.modules))
        for package, sut in SUT_PACKAGES.items()
        if sut.suite is not None
    }
    return {package: missing for package, missing in gaps.items() if missing}


def assert_law_coverage(*, only: frozenset[str] | None = None) -> None:
    """Assert every registered SUT public symbol is law-covered, auto-exempt, or explicitly exempt.

    Args:
        only: Packages to census; ``None`` censuses every registration.
    """
    global_covered = frozenset(rec.subject.rsplit(".", 1)[-1] for rec in MANIFEST if not rec.subject_module)

    for package, sut in SUT_PACKAGES.items():
        if only is not None and package not in only:
            continue
        surface, failures = _public_surface(package)
        covered = global_covered | frozenset(
            rec.subject.rsplit(".", 1)[-1] for rec in MANIFEST if rec.subject_module == package or rec.subject_module.startswith(f"{package}.")
        )
        uncovered = frozenset(name for name, member in surface.items() if name not in covered and name not in sut.exempt and not auto_exempt(member))
        gaps = [*(f"  - {name}" for name in sorted(uncovered)), *(f"  ! {mod}: {err}" for mod, err in failures)]
        assert not gaps, (
            f"Law coverage gap in '{package}': {len(uncovered)} symbol(s) have no law, {len(failures)} module import failure(s):\n" + "\n".join(gaps)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "spec",
    "consume_covers",
    "register_sut",
    "register_tree",
    "generated_roots",
    "assert_law_coverage",
    "auto_exempt",
    "uncollected_laws",
    "MANIFEST",
    "LawRecord",
    "Sut",
]
