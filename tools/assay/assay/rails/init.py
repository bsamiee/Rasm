"""Mint blessed workspace projects and prove the member census.

``python-lib`` and ``python-app`` write one project kind's minimal file set — the member manifest, its
package seat, and (for libs) the suite conftest — and an apps project appends its explicit workspace
member row, because a glob over polyglot ``apps/`` trees hard-fails on non-Python directories. ``check``
is the census gate closing uv's silent-orphan hole: a manifest under a governed tree that no member row
admits never reaches the lock, so this rail refuses it instead of letting it vanish. C# and TypeScript
projects mint by hand — an identity-only csproj joined via ``dotnet sln add`` under the parity and shape
guards, and the TypeScript two-file set the root tsconfig presets compose — so this rail owns the Python
kinds alone.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from pathlib import Path
import re
import tomllib
from typing import ClassVar, Final, override

from expression import Error, Ok, Result
import tomlkit
from tomlkit.items import Array

from assay.composition.settings import AssaySettings
from assay.composition.store import ArtifactScope
from assay.core.exec import Executor
from assay.core.model import BaseParams, Claim, Completed, Fault, RailStatus, receipt, Report
from assay.diagnostics import fold


# --- [CONSTANTS] ------------------------------------------------------------------------

_APPS: Final[str] = "apps"
_LIBS: Final[str] = "libs/python"
_NAME: Final[re.Pattern[str]] = re.compile(r"[a-z][a-z0-9-]*")
_SUITE: Final[str] = "tests/python/libs"
_TOOLS: Final[str] = "tools"
_VERSION: Final[str] = "0.1.0"

_LIB_MANIFEST: Final[str] = """\
# --- [PROJECT] --------------------------------------------------------------------------

[project]
name = "rasm-{name}"
version = "{version}"
description = "{description}"
readme = "README.md"
requires-python = ">=3.15"
dependencies = []

# --- [BUILD] ----------------------------------------------------------------------------

[build-system]
requires = ["uv_build>=0.12.5,<0.13"]
build-backend = "uv_build"

[tool.uv.build-backend]
# Flat layout, no src/: the package sits at the project root, and scoped sys.path entries keep the editable install unshadowed.
module-root = ""
module-name = "rasm.{module}"
namespace = true
"""

_APP_MANIFEST: Final[str] = """\
# --- [PROJECT] --------------------------------------------------------------------------

[project]
name = "{name}"
version = "{version}"
description = "{description}"
requires-python = ">=3.15"
dependencies = []

# --- [BUILD] ----------------------------------------------------------------------------

[build-system]
requires = ["uv_build>=0.12.5,<0.13"]
build-backend = "uv_build"

[tool.uv.build-backend]
# Flat layout, no src/: the package sits at the project root, and scoped sys.path entries keep the editable install unshadowed.
module-root = ""
module-name = "{module}"
"""

_SUITE_SEAT: Final[str] = '"""Package-scoped seams for libs/python/{name} specs; composes the shared kit, never redeclares it."""\n'


# --- [MODELS] ---------------------------------------------------------------------------


class InitParams(BaseParams):
    """Parameters for the init claim: one positional target path for the minting verbs, none for check."""

    SLOTS: ClassVar[dict[str, str]] = {"": "<PATH>", "check": ""}

    @override
    def _arity(self, verb: str) -> int | None:
        return 0 if verb == "check" else 1


# --- [OPERATIONS] -----------------------------------------------------------------------


def _member_entries(root: Path) -> Result[tuple[str, ...], Fault]:
    try:
        manifest = tomllib.loads((root / "pyproject.toml").read_text(encoding="utf-8"))
    except (OSError, tomllib.TOMLDecodeError) as exc:
        return Error(Fault(("init", "workspace"), message=f"root pyproject.toml unreadable: {exc}"))
    rows = manifest.get("tool", {}).get("uv", {}).get("workspace", {}).get("members", [])
    match rows:
        case list() as members if all(isinstance(row, str) for row in members):
            return Ok(tuple(members))
        case _:
            return Error(Fault(("init", "workspace"), message="tool.uv.workspace.members is not a string array"))


def _declared_dirs(root: Path, members: tuple[str, ...]) -> frozenset[Path]:
    return frozenset(
        path.resolve() for row in members for path in (root.glob(row) if any(ch in row for ch in "*?[") else (root / row,)) if path.is_dir()
    )


def _disk_manifests(root: Path) -> tuple[Path, ...]:
    shallow = (candidate for tree in (_LIBS, _TOOLS) for candidate in sorted((root / tree).glob("*/pyproject.toml")))
    nested = sorted((root / _APPS).rglob("pyproject.toml")) if (root / _APPS).is_dir() else []
    return (*shallow, *nested)


def _census_rows(root: Path, members: tuple[str, ...]) -> tuple[Completed, ...]:
    declared = _declared_dirs(root, members)
    orphans = tuple(m for m in _disk_manifests(root) if m.parent.resolve() not in declared)
    ghosts = tuple(row for row in members if not any(ch in row for ch in "*?[") and not (root / row / "pyproject.toml").is_file())
    findings = (
        *(
            receipt(
                ("init", "census", str(m.parent.relative_to(root))),
                1,
                status=RailStatus.FAILED,
                notes=(f"{m.relative_to(root)} is not a declared workspace member; add its row to tool.uv.workspace.members",),
            )
            for m in orphans
        ),
        *(
            receipt(
                ("init", "census", row),
                1,
                status=RailStatus.FAILED,
                notes=(f"member row {row!r} resolves to no pyproject.toml on disk; repair or remove the row",),
            )
            for row in ghosts
        ),
    )
    covered = receipt(
        ("init", "census"),
        0,
        status=RailStatus.OK,
        notes=(f"{len(_disk_manifests(root))} manifests across {_LIBS}, {_TOOLS}, {_APPS} all resolve as workspace members",),
    )
    return findings or (covered,)


def _write_new(root: Path, target: str, *, app: bool) -> Result[tuple[Completed, ...], Fault]:
    relative = Path(target)
    tree, leaf = relative.parts[0] if relative.parts else "", relative.name
    lawful_tree = (tree == _APPS and len(relative.parts) >= 2) if app else (str(relative.parent) == _LIBS and len(relative.parts) == 3)
    match (lawful_tree, _NAME.fullmatch(leaf), (root / relative).exists()):
        case (False, _, _):
            where = f"{_APPS}/<app>/..." if app else f"{_LIBS}/<name>"
            return Error(Fault(("init", target), message=f"target must sit under {where}"))
        case (_, None, _):
            return Error(Fault(("init", target), message=f"project name {leaf!r} must match {_NAME.pattern}"))
        case (_, _, True):
            return Error(Fault(("init", target), message="target already exists; init never overwrites"))
        case _:
            pass
    module = leaf.replace("-", "_")
    body = _APP_MANIFEST if app else _LIB_MANIFEST
    description = f"{leaf} application project." if app else f"rasm.{module} capability for the Rasm estate."
    package = (root / relative / module) if app else (root / relative / "rasm" / module)
    writes: tuple[tuple[Path, str], ...] = (
        (root / relative / "pyproject.toml", body.format(name=leaf, module=module, version=_VERSION, description=description)),
        (package / "__init__.py", f'"""{description}"""\n'),
        *(() if app else ((root / _SUITE / leaf / "conftest.py", _SUITE_SEAT.format(name=leaf)),)),
    )
    try:
        for destination, content in writes:
            destination.parent.mkdir(parents=True, exist_ok=True)
            destination.write_text(content, encoding="utf-8")
        if app:
            _append_member(root, str(relative))
    except OSError as exc:
        return Error(Fault(("init", target), message=f"write failed: {exc}"))
    written = (*(str(destination.relative_to(root)) for destination, _ in writes), *(("pyproject.toml [tool.uv.workspace].members",) if app else ()))
    return Ok((receipt(("init", target), 0, status=RailStatus.OK, notes=tuple(f"wrote {path}" for path in written)),))


def _append_member(root: Path, row: str) -> None:
    manifest = tomlkit.parse((root / "pyproject.toml").read_text(encoding="utf-8"))
    members = manifest["tool"]["uv"]["workspace"]["members"]
    if isinstance(members, Array) and row not in {str(item) for item in members}:
        members.append(row)
        (root / "pyproject.toml").write_text(tomlkit.dumps(manifest), encoding="utf-8")


def _minted(settings: AssaySettings, verb: str, target: str, *, app: bool) -> Result[Report, Fault]:
    root = Path(str(settings.local_root)).resolve()
    return _write_new(root, target, app=app).map(lambda rows: fold(Claim.INIT, verb, rows, promote_empty=True))


# --- [COMPOSITION] ----------------------------------------------------------------------


def python_lib(settings: AssaySettings, scope: ArtifactScope, params: InitParams, executor: Executor) -> Result[Report, Fault]:
    """Mint a libs/python member: manifest, rasm namespace seat, and its suite conftest.

    Returns:
        Report naming every written file, or the refusal fault.
    """
    _ = scope, executor
    return _minted(settings, "python-lib", params.paths[0] if params.paths else "", app=False)


def python_app(settings: AssaySettings, scope: ArtifactScope, params: InitParams, executor: Executor) -> Result[Report, Fault]:
    """Mint an apps python project: manifest, package seat, and its explicit workspace member row.

    Returns:
        Report naming every written file, or the refusal fault.
    """
    _ = scope, executor
    return _minted(settings, "python-app", params.paths[0] if params.paths else "", app=True)


def check(settings: AssaySettings, scope: ArtifactScope, params: InitParams, executor: Executor) -> Result[Report, Fault]:
    """Prove the workspace member census over the governed trees.

    Returns:
        Report whose rows refuse undeclared on-disk manifests and dangling member rows.
    """
    _ = scope, params, executor
    root = Path(str(settings.local_root)).resolve()
    return _member_entries(root).map(lambda members: fold(Claim.INIT, "check", _census_rows(root, members), promote_empty=True))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["InitParams", "check", "python_app", "python_lib"]
