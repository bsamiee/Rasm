# ruff: file-ignore[import-private-name, private-member-access]
# ty: ignore[invalid-argument-type, not-iterable, redundant-condition, unresolved-attribute, unresolved-import]
# mypy: disable-error-code="arg-type, attr-defined, func-returns-value, import-not-found, no-any-return, union-attr, unreachable"
"""Blender's add-on packages: the declared remote repositories, the loaded modules by package id, and the rows installing each declared package and removing each undeclared one."""

from functools import partial, reduce
from importlib import metadata
from pathlib import Path, PurePosixPath
import sys
import tomllib
from types import ModuleType
import zipfile
import zlib

from _bpy_internal.assets.remote_library.listing_asset_catalogs import parse_catalogs
import addon_utils
from bl_pkg.cli.blender_ext import (
    pkg_is_legacy_addon,
    PKG_MANIFEST_FILENAME_TOML,
    PKG_REPO_LIST_FILENAME,
    pkg_zipfile_detect_subdir_or_none,
    platform_from_this_system,
    platform_machine_replace,
    platform_system_replace_for_wheels,
    REPO_LOCAL_PRIVATE_DIR,
)
import bpy

from interface.blender.rows import Archive, Installation, JSON, Listed, Local, Repository, stamp
from interface.report import converged, Kind, line, Row, subscript

# --- [TYPES] ----------------------------------------------------------------------------

type Declaration = Local | Listed | None

# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [MODULES]
def identity(module: ModuleType) -> str:
    """Id a package row keys the module on, an extension's manifest id or another add-on's `bl_info` name."""
    return module.__name__.rpartition(".")[2] if addon_utils.check_extension(module.__name__) else module.bl_info["name"]


def core_addon(module: ModuleType) -> bool:
    """Whether the module is one of Blender's bundled add-ons."""
    return Path(module.__file__).is_relative_to(bpy.utils.system_resource("SCRIPTS", path="addons_core"))


def enabled(name: str) -> bool:
    """Whether the add-on is recorded in the preferences and loaded."""
    return all(addon_utils.check(name))


def installed(name: str) -> tuple[ModuleType, ...]:
    """Installed modules of the package id."""
    return tuple(module for module in addon_utils.modules() if identity(module) == name)


def loaded() -> dict[str, str]:
    """Module name of every loaded add-on by its package id."""
    return {identity(module): module.__name__ for module in addon_utils.modules() if addon_utils.check(module.__name__)[1]}


def checksum(path: Path) -> tuple[int, int] | None:
    """CRC-32 and size of a file, the pair a zip member records, None for a path the install lacks."""
    try:
        with path.open("rb") as handle:
            return reduce(lambda total, chunk: zlib.crc32(chunk, total), iter(partial(handle.read, 1 << 20), b""), 0), path.stat().st_size
    except FileNotFoundError:
        return None


def stamped(module: ModuleType, archive: str) -> str:
    """Stamp of the module's installed files at the archive's member paths, an extension's under its folder and a legacy add-on's under the add-ons folder, a member the install lacks left out and a file the add-on writes beside them its own."""
    file = Path(module.__file__)
    root = file.parent.parent if file.name == "__init__.py" and not addon_utils.check_extension(module.__name__) else file.parent
    with zipfile.ZipFile(archive) as packed:
        members = [info.filename for info in packed.infolist() if not info.is_dir()]
    return stamp({name: pair for name in members if (pair := checksum(root / name)) is not None})


def build(row: Declaration, module: ModuleType) -> str | None:
    """Build a declaration compares an installed module by: a listed package's manifest version, a staged archive's file stamp, none for a bundled or undeclared add-on."""
    match row:
        case Listed():
            return str(tomllib.loads(Path(module.__file_manifest__).read_text(encoding="utf-8"))["version"])
        case Local(archive=Archive(path=path)):
            return stamped(module, path)
        case Local() | None:
            return None


def state(name: str, row: Declaration) -> tuple[tuple[str, str | None, bool], ...]:
    """Each installed module of the package with its build and whether it is enabled."""
    return tuple(sorted((module.__name__, build(row, module), enabled(module.__name__)) for module in installed(name)))


# --- [REPOSITORIES]
def user_repository(preferences: bpy.types.Preferences) -> bpy.types.UserExtensionRepo:
    """User repository without a remote, where archive extensions install."""
    return next(repo for repo in preferences.extensions.repos if repo.source == "USER" and not repo.use_remote_url)


def module_repository(preferences: bpy.types.Preferences, module: str) -> bpy.types.UserExtensionRepo | None:
    """Repository with the module name, None while the preferences hold none."""
    return next((repo for repo in preferences.extensions.repos if repo.module == module), None)


def remote(preferences: bpy.types.Preferences, declared: Repository) -> Row:
    """Row holding the declared remote repository enabled at its address, added where the preferences hold none."""

    def read() -> dict[str, object] | None:
        match module_repository(preferences, declared.module):
            case None:
                return None
            case repo:
                return {"enabled": repo.enabled, "use_remote_url": repo.use_remote_url, "remote_url": repo.remote_url}

    def write(target: dict[str, object]) -> None:
        repo = module_repository(preferences, declared.module) or preferences.extensions.repos.new(name=declared.name, module=declared.module)
        for name, value in target.items():
            setattr(repo, name, value)

    return Row(label=subscript("extensions.repos", declared.module), read=read, write=write, target={"enabled": True, "use_remote_url": True, "remote_url": declared.url})


def archived(repository: str, name: str, archive: Archive) -> tuple[tuple[str, str | None, bool], ...] | str:
    """Enabled module a staged archive installs at its stamp, an extension of the user repository when bl_pkg finds its manifest and else the legacy add-on of its one top entry, or why neither holds."""
    with zipfile.ZipFile(archive.path) as packed:
        subdir, tops = pkg_zipfile_detect_subdir_or_none(packed), sorted({PurePosixPath(each).parts[0] for each in packed.namelist()})
    match tops:
        case _ if subdir is not None:
            return ((f"bl_ext.{repository}.{name}", archive.stamp, True),)
        case [top] if pkg_is_legacy_addon(archive.path):
            return ((PurePosixPath(top).stem, archive.stamp, True),)
        case _:
            return f"archive {archive.path} holds {', '.join(tops)} at its root, neither an extension nor one legacy add-on"


def core_module(name: str) -> tuple[tuple[str, str | None, bool], ...] | str:
    """Enabled bundled module of the package id, or why no one bundled module holds it."""
    match [module.__name__ for module in installed(name) if core_addon(module)]:
        case [module]:
            return ((module, None, True),)
        case found:
            return f"names {len(found)} bundled add-ons {found} where one is declared"


def target_modules(repository: str, name: str, row: Declaration) -> tuple[tuple[str, str | None, bool], ...] | str:
    """Modules the package holds with their builds and enabled states: the declared one enabled at its build, an undeclared package's bundled modules disabled and every other removed, or why no one module follows from the declaration."""
    match row:
        case None:
            return tuple(sorted((module.__name__, None, False) for module in installed(name) if core_addon(module)))
        case Listed(repository=listed, version=version):
            return ((f"bl_ext.{listed}.{name}", version, True),)
        case Local(archive=Archive() as archive):
            return archived(repository, name, archive)
        case Local():
            return core_module(name)


# --- [WRITES]
def removed(window: bpy.types.Window, preferences: bpy.types.Preferences, module: ModuleType) -> None:
    """Uninstall an extension, disable a bundled add-on, or delete a legacy add-on."""
    match module.__name__.split("."):
        case ["bl_ext", repository, package_id]:
            bpy.ops.extensions.package_uninstall(repo_directory=module_repository(preferences, repository).directory, pkg_id=package_id)
        case _ if core_addon(module):
            addon_utils.disable(module.__name__, default_set=True)
        case _:
            with bpy.context.temp_override(window=window, screen=window.screen, area=window.screen.areas[0]):
                bpy.ops.preferences.addon_remove(module=module.__name__)


def install(preferences: bpy.types.Preferences, row: Declaration, module: str) -> None:
    """Install the module's package from its remote repository or staged archive, which bl_pkg routes to the user repository or the legacy add-on folder, a loaded module disabled and evicted first so its modules load from the new files."""
    match row:
        case Listed(identity=name, repository=repository):
            bpy.ops.extensions.package_install(repo_directory=module_repository(preferences, repository).directory, pkg_id=name, enable_on_install=False)
        case Local(archive=Archive(path=path)):
            if addon_utils.check(module)[1]:
                addon_utils.disable(module)
            for key in [key for key in sys.modules if key == module or key.startswith(f"{module}.")]:
                del sys.modules[key]
            bpy.ops.extensions.package_install_files(filepath=path, repo=user_repository(preferences).module, enable_on_install=False)
        case Local() | None:
            pass


def provisioned(window: bpy.types.Window, preferences: bpy.types.Preferences, name: str, row: Declaration, target: tuple[tuple[str, str | None, bool], ...]) -> None:
    """Remove each installed module of the package the target omits, install each target module whose installed build differs, and set its enabled state."""
    for stale in [each for each in installed(name) if each.__name__ not in {kept for kept, _, _ in target}]:
        removed(window, preferences, stale)
    for module, wanted, on in target:
        if not any(each.__name__ == module and build(row, each) == wanted for each in installed(name)):
            install(preferences, row, module)
        if enabled(module) != on:
            (addon_utils.enable if on else addon_utils.disable)(module, default_set=True)


# --- [ROWS]
def package(window: bpy.types.Window, preferences: bpy.types.Preferences, repository: str, name: str, row: Declaration) -> Row | str:
    """Row converging the package to its target modules, or the error line naming why no one module follows from its declaration."""
    label = subscript("packages", name)
    match target_modules(repository, name, row):
        case str() as error:
            return line(Kind.ERROR, f"{label} {error}")
        case wanted:
            return Row(label=label, read=partial(state, name, row), write=partial(provisioned, window, preferences, name, row), target=wanted)


def packages(window: bpy.types.Window, preferences: bpy.types.Preferences, declared: tuple[Local | Listed, ...]) -> tuple[Row | str, ...]:
    """Row per undeclared package, hidden core add-ons aside, then a row or error line per declared package."""
    repository, declarations = user_repository(preferences).module, {row.identity: row for row in declared}
    undeclared = sorted({identity(module) for module in addon_utils.modules() if module.__name__ not in addon_utils._addons_hidden_core} - declarations.keys())
    return tuple(package(window, preferences, repository, name, row) for name, row in (*((name, None) for name in undeclared), *declarations.items()))


def unloaded_packages(declared: tuple[Local | Listed, ...]) -> tuple[str, ...]:
    """Error line of each declared package that holds no loaded module after its row's enable."""
    modules = loaded()
    return tuple(
        line(Kind.ERROR, f"{subscript('packages', row.identity)} holds no loaded module after its enable, its installed modules {tuple(module.__name__ for module in installed(row.identity))}")
        for row in declared
        if row.identity not in modules
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def installation(path: str, declared: str) -> None:
    """Converge the declared repositories, saving the preferences once one changed, then write the installation facts as JSON at the path: change lines, version, index files, core ids, catalogs, manifest name, platform, machines, systems, interpreter, and numpy."""
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
        python=sys.executable,
        numpy=metadata.version("numpy"),
    )
    Path(path).write_text(JSON.dumps(facts), encoding="utf-8")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["core_addon", "identity", "installation", "loaded", "packages", "unloaded_packages"]
