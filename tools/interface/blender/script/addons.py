# ruff: file-ignore[import-private-name, private-member-access]
# ty: ignore[invalid-argument-type, not-iterable, redundant-condition, unresolved-attribute, unresolved-import]
# mypy: disable-error-code="arg-type, attr-defined, func-returns-value, import-not-found, no-any-return, union-attr, unreachable"
"""Blender add-on reconciliation, repository declarations, loaded package identities, and bundled interpreter facts."""

from collections.abc import Callable, Set as AbstractSet
from functools import partial
from importlib import metadata
from pathlib import Path, PurePosixPath
import platform
import sys
import sysconfig
import tomllib
from types import ModuleType
from unittest.mock import patch
import zipfile
import zlib

from _bpy_internal.assets.remote_library.listing_asset_catalogs import parse_catalogs
import addon_utils
from attrs import frozen
from bl_pkg import bl_extension_ops
from bl_pkg.cli.blender_ext import (
    pkg_is_legacy_addon,
    PKG_MANIFEST_FILENAME_TOML,
    PKG_REPO_LIST_FILENAME,
    platform_from_this_system,
    platform_machine_replace,
    platform_system_replace_for_wheels,
    REPO_LOCAL_PRIVATE_DIR,
)
import bpy
import OpenImageIO
from packaging.utils import canonicalize_name
import PyOpenColorIO

from interface.blender.rows import Archived, Bundled, Install, Installation, Interpreter, JSON, Listed, Repository, stamp
from interface.report import converged, Error, Item, Refused, Row, subscript

# --- [TYPES] ----------------------------------------------------------------------------

type Declaration = Install | None

# --- [MODELS] ---------------------------------------------------------------------------


@frozen
class State:
    """Installed build and Blender's saved and loaded enablement states."""

    build: str | None
    enabled: tuple[bool, bool]


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [MODULES]
def identity(module: ModuleType) -> str:
    """Id a package row keys the module on, an extension's manifest id or another add-on's `bl_info` name."""
    return module.__name__.rpartition(".")[2] if addon_utils.check_extension(module.__name__) else module.bl_info["name"]


def core_addon(module: ModuleType) -> bool:
    """Whether the module is one of Blender's bundled add-ons."""
    return Path(module.__file__).is_relative_to(bpy.utils.system_resource("SCRIPTS", path="addons_core"))


def installed(name: str) -> tuple[ModuleType, ...]:
    """Installed modules of the package id."""
    return tuple(module for module in addon_utils.modules() if identity(module) == name)


def loaded() -> dict[str, str]:
    """Module name of every loaded add-on by its package id."""
    return {identity(module): module.__name__ for module in addon_utils.modules() if addon_utils.check(module.__name__)[1]}


def stamped(path: Path, root: Path | None = None) -> str:
    """Digest the CRC-32 and size of every installed file under the path, keyed relative to the root, the folder itself or a file's parent by default."""
    directory = path.is_dir()
    base = root if root is not None else path if directory else path.parent
    return stamp({file.relative_to(base).as_posix(): (zlib.crc32(data := file.read_bytes()), len(data)) for file in (path.rglob("*") if directory else (path,)) if file.is_file()})


def build(row: Declaration, module: ModuleType) -> str | None:
    """Return a listed package's manifest version, an archived package's installed file stamp, or None for bundled and undeclared add-ons."""
    match row:
        case Listed() if addon_utils.check_extension(module.__name__):
            return str(tomllib.loads(Path(module.__file_manifest__).read_text(encoding="utf-8"))["version"])
        case Archived():
            file = Path(module.__file__)
            owned = file.parent if file.name == "__init__.py" else file
            return stamped(owned, None if addon_utils.check_extension(module.__name__) else owned.parent)
        case Bundled() | Listed() | None:
            return None


def state(name: str, row: Declaration) -> dict[str, State]:
    """Installed builds and enablement states by module name."""
    return {module.__name__: State(build(row, module), addon_utils.check(module.__name__)) for module in installed(name)}


# --- [REPOSITORIES]
def user_repository(preferences: bpy.types.Preferences) -> bpy.types.UserExtensionRepo:
    """User repository without a remote, where archive extensions install."""
    return next(repo for repo in preferences.extensions.repos if repo.source == "USER" and not repo.use_remote_url)


def module_repository(preferences: bpy.types.Preferences, module: str) -> bpy.types.UserExtensionRepo | None:
    """Repository with the module name, None while the preferences hold none."""
    return next((repo for repo in preferences.extensions.repos if repo.module == module), None)


def remote(preferences: bpy.types.Preferences, declared: Repository) -> Row:
    """Row holding the declared remote repository enabled at its address, added where the preferences hold none."""
    target: dict[str, object] = {"enabled": True, "use_remote_url": True, "remote_url": declared.url}

    def read() -> dict[str, object] | None:
        repo = module_repository(preferences, declared.module)
        return None if repo is None else {name: getattr(repo, name) for name in target}

    def write(values: dict[str, object]) -> None:
        repo = module_repository(preferences, declared.module) or preferences.extensions.repos.new(name=declared.name, module=declared.module)
        for name, value in values.items():
            setattr(repo, name, value)

    return Row(label=subscript("extensions.repos", declared.module), read=read, write=write, target=target)


def target_modules(repository: str, name: str, row: Declaration) -> dict[str, State] | str:
    """Declared modules and builds, disabled undeclared core modules, or an ambiguous package error."""
    match row:
        case None:
            return {module.__name__: State(None, (False, False)) for module in installed(name) if core_addon(module)}
        case Listed(repository=listed, version=version):
            return {f"bl_ext.{listed}.{name}": State(version, (True, True))}
        case Archived(archive=archive) if not pkg_is_legacy_addon(archive.path):
            return {f"bl_ext.{repository}.{name}": State(archive.stamp, (True, True))}
        case Archived(archive=archive):
            with zipfile.ZipFile(archive.path) as packed:
                tops = sorted({PurePosixPath(each).parts[0] for each in packed.namelist()})
            match tops:
                case [top]:
                    return {PurePosixPath(top).stem: State(archive.stamp, (True, True))}
                case _:
                    return f"legacy add-on archive {archive.path} holds {', '.join(tops)} at its root where Blender installs one module"
        case Bundled():
            match [module.__name__ for module in installed(name) if core_addon(module)]:
                case [module]:
                    return {module: State(None, (True, True))}
                case found:
                    return f"names {len(found)} bundled add-ons {found} where one is declared"


# --- [WRITES]
def extension_command(action: Callable[[], AbstractSet[str]]) -> Refused | None:
    """Run Blender's extension lifecycle and return the refusal holding the failures its blocking operator status omits, from the batch's report calls and the status log, or None."""
    start = len(bl_extension_ops.repo_status_text.log)
    with patch.object(bl_extension_ops, "_report", wraps=bl_extension_ops._report) as report:
        status = action()
    reports = (*(call.args for call in report.call_args_list), *bl_extension_ops.repo_status_text.log[start:])
    failures = (*(message for kind, message in reports if kind in {"ERROR", "FATAL_ERROR"}), *(("extension operation cancelled",) if "CANCELLED" in status else ()))
    return Refused("; ".join(failures)) if failures else None


def removed(preferences: bpy.types.Preferences, module: ModuleType) -> Refused | None:
    """Uninstall extensions, disable bundled add-ons, or remove legacy add-ons in the first window's first area."""
    match module.__name__.split("."):
        case ["bl_ext", repository, package_id]:
            return extension_command(partial(bpy.ops.extensions.package_uninstall, repo_directory=module_repository(preferences, repository).directory, pkg_id=package_id))
        case _ if core_addon(module):
            bpy.ops.preferences.addon_disable(module=module.__name__)
        case _:
            window = bpy.context.window_manager.windows[0]
            with bpy.context.temp_override(window=window, screen=window.screen, area=window.screen.areas[0]):
                bpy.ops.preferences.addon_remove(module=module.__name__)
    return None


def install(preferences: bpy.types.Preferences, row: Declaration, module: str) -> Refused | None:
    """Install through Blender, a legacy add-on disabled and its modules evicted so the enable imports the new files."""
    match row:
        case Listed():
            return extension_command(partial(bpy.ops.extensions.package_install, repo_directory=module_repository(preferences, row.repository).directory, pkg_id=row.identity, enable_on_install=False))
        case Archived(archive=archive) if addon_utils.check_extension(module):
            return extension_command(partial(bpy.ops.extensions.package_install_files, filepath=archive.path, repo=user_repository(preferences).module, enable_on_install=False))
        case Archived(archive=archive):
            if any(addon_utils.check(module)):
                bpy.ops.preferences.addon_disable(module=module)
            for key in [key for key in sys.modules if key == module or key.startswith(f"{module}.")]:
                del sys.modules[key]
            bpy.ops.preferences.addon_install(filepath=archive.path)
            return None
        case Bundled() | None:
            return None


def provisioned(preferences: bpy.types.Preferences, name: str, row: Declaration, target: dict[str, State]) -> Refused | None:
    """Remove each installed module of the package the target omits, install each target module whose installed build differs, and set its enabled state."""
    modules = {each.__name__: each for each in installed(name)}
    for stale in modules.keys() - target.keys():
        if refused := removed(preferences, modules[stale]):
            return refused
    for module, wanted in target.items():
        before = addon_utils.check(module)
        if (replaced := module not in modules or build(row, modules[module]) != wanted.build) and (refused := install(preferences, row, module)):
            return refused
        if before != wanted.enabled or (replaced and not addon_utils.check_extension(module)):
            (bpy.ops.preferences.addon_enable if all(wanted.enabled) else bpy.ops.preferences.addon_disable)(module=module)
    return None


# --- [ROWS]
def package(preferences: bpy.types.Preferences, repository: str, name: str, row: Declaration) -> Item:
    """Row converging the package to its target modules, or the error line naming why no one module follows from its declaration."""
    label = subscript("packages", name)
    match target_modules(repository, name, row):
        case str() as error:
            return Error(f"{label} {error}")
        case wanted:
            return Row(label=label, read=partial(state, name, row), write=partial(provisioned, preferences, name, row), target=wanted)


def packages(preferences: bpy.types.Preferences, declared: tuple[Install, ...]) -> tuple[Item, ...]:
    """Row per undeclared package, hidden core add-ons aside, then a row or error line per declared package."""
    repository, declarations = user_repository(preferences).module, {row.identity: row for row in declared}
    undeclared = sorted({identity(module) for module in addon_utils.modules() if module.__name__ not in addon_utils._addons_hidden_core} - declarations.keys())
    return tuple(package(preferences, repository, name, row) for name, row in (*((name, None) for name in undeclared), *declarations.items()))


# --- [INTERPRETER]
def interpreter() -> Interpreter:
    """Read Python and purelib distribution versions by canonical name, adding OpenColorIO and OpenImageIO module versions absent from distribution metadata."""
    bundled = {canonicalize_name(each.name): each.version for each in metadata.distributions(path=[sysconfig.get_path("purelib")])}
    return Interpreter(sys.executable, platform.python_version(), bundled | {"opencolorio": PyOpenColorIO.__version__, "openimageio": OpenImageIO.__version__})


# --- [COMPOSITION] ----------------------------------------------------------------------


def installation(path: str, declared: str) -> None:
    """Converge declared repositories, save changed preferences, and write installation facts as JSON."""
    preferences = bpy.context.preferences
    if report := "".join(f"{entry}\n" for row in JSON.loads(declared, tuple[Repository, ...]) for entry in converged(remote(preferences, row))):
        bpy.ops.wm.save_userpref()
    facts = Installation(
        report=report,
        version=bpy.app.version_string,
        repositories={repo.module: str(Path(repo.directory, REPO_LOCAL_PRIVATE_DIR, PKG_REPO_LIST_FILENAME)) for repo in preferences.extensions.repos if repo.enabled and repo.use_remote_url},
        core=frozenset(identity(module) for module in addon_utils.modules() if core_addon(module)),
        essentials=frozenset(catalog.path for catalog in parse_catalogs(Path(bpy.utils.system_resource("DATAFILES", path="assets")))),
        manifest_filename=PKG_MANIFEST_FILENAME_TOML,
        platform=platform_from_this_system(),
        machines=platform_machine_replace,
        systems=frozenset(platform_system_replace_for_wheels.values()),
        interpreter=interpreter(),
    )
    Path(path).write_text(JSON.dumps(facts), encoding="utf-8")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["extension_command", "installation", "interpreter", "loaded", "package", "packages", "stamped", "user_repository"]
