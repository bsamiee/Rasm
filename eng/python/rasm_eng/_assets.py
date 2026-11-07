"""Writes the one folder `pack` overlays on its stage, holding what an extension's declarations make: encoded icons, pinned upstream files, interface themes."""

import os
lazy from collections.abc import Iterable
lazy from functools import partial
lazy from pathlib import Path

import msgspec
lazy import anyio
lazy from coloraide import Color
lazy from cyclopts.types import ResolvedExistingDirectory, ResolvedPath
lazy import pooch  # type: ignore[import-untyped]

lazy from rasm_eng._host import _declared, _Host, _probed, _Pyproject, _root, _site, Blender, scripted
lazy from rasm_eng._icons import _encoded
lazy from rasm_records import accumulated, argument, committed, Error, Fault, folder_made, Ok, Outcome, paired, Reason

# --- [MODELS] ---------------------------------------------------------------------------


class _Pin(msgspec.Struct, frozen=True):
    """File fetched at a pinned URL, checked by sha256, and linked at its target."""

    url: str
    sha256: str
    target: Path


# --- [OPERATIONS] -----------------------------------------------------------------------


def _fetched(base: Path, pins: Iterable[_Pin], /) -> Outcome[tuple[Path, ...]]:
    """Links each pin's file at its target from the repository's content store at `.cache/fetched/<sha256>`, pooch fetching a missing or altered one.

    Args:
        base: Repository root.
        pins: Files to link, their targets absent.

    Returns:
        Targets in input order, or the fault of every failed fetch and link.
    """

    def linked(pin: _Pin) -> Outcome[Path]:
        try:
            stored = pooch.retrieve(pin.url, f"sha256:{pin.sha256}", fname=pin.sha256, path=base / ".cache/fetched")
            pin.target.parent.mkdir(parents=True, exist_ok=True)
            pin.target.hardlink_to(stored)
        except (OSError, ValueError) as error:
            return Error(Fault.of(Reason.FAILED, pin.url, error))
        return Ok(pin.target)

    return accumulated(anyio.run(anyio.amap, lambda pin: anyio.to_thread.run_sync(linked, pin), pins))  # ty: ignore[unsound-return-statement]


def _icon(stage: Path, paint: str, name: Path, master: Path, *, turned: bool) -> Outcome[Path]:
    """Writes one master's preview and geometry icon under the stage's `icons` folder."""
    target = stage / "icons" / name
    try:
        preview, icon = _encoded(master, paint, turned=turned)
    except (OSError, ValueError, SyntaxError) as error:
        return Error(Fault.of(Reason.UNREADABLE, str(master), error))
    try:
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(preview)
        target.with_suffix(".dat").write_bytes(icon)
    except OSError as error:
        return Error(Fault.of(Reason.UNWRITABLE, str(target), error))
    return Ok(target)


def assets(source: ResolvedExistingDirectory, directory: ResolvedPath, /, *, blender: Blender) -> Outcome[tuple[Path, ...]]:
    """Writes what an extension folder declares at its stage-relative path, each part present once its declaration is.

    Every `icons/**/*.svg` master becomes a preview and geometry icon, `currentColor` painting the factory theme's tool text color.
    Every `[tool.rasm-eng.turned]` row names a variant stem and its master, the variant the master turned a quarter clockwise and written beside the masters.
    Every `[tool.rasm-eng.fetch."<folder>"]` row's files reach that folder from the content store.
    Every `[tool.rasm-eng.theme."<name>"]` table becomes `<name>.xml`, the files a `type = "theme"` extension holds, the first by name the one its installer enables.

    Args:
        source: Extension folder.
        directory: Folder replaced in one step.
        blender: Blender executable.

    Returns:
        Files written, or fault of the probe, the folder commit, and every part.
    """
    masters = source / "icons"
    pyproject = source / "pyproject.toml"

    def written(workspace: tuple[Path, _Host], project: _Pyproject, stage: Path) -> Outcome[object]:
        base, host = workspace
        paint = Color("srgb", list(host.text)).to_string(hex=True)
        table = project.tool.rasm_eng
        rows = (
            *((master.relative_to(masters), master, False) for master in sorted(masters.glob("**/*.svg"))),
            *((Path(f"{variant}.svg"), masters / f"{master}.svg", True) for variant, master in table.turned.items()),
        )
        return paired(
            folder_made(stage),
            accumulated(_icon(stage, paint, name, master, turned=turn) for name, master, turn in rows),
            _fetched(base, (_Pin(f"{upstream.base}/{file}", sha256, stage / folder / file) for folder, upstream in table.fetch.items() for file, sha256 in upstream.files.items())),
            _site(base, host, source).bind(
                lambda folder: scripted(
                    blender,
                    "import sys; from rasm_bpy.ui import themes_written; sys.exit(themes_written(*sys.argv[sys.argv.index('--') + 1:]))",
                    argument(table),
                    source,
                    stage,
                    env=os.environ | {"PYTHONUSERBASE": str(folder)},
                )
            )
            if table.theme
            else Ok(None),
        )

    workspace = _root().bind(lambda base: _probed(base, blender).map(lambda host: (base, host)))
    return (
        paired(workspace, _declared(pyproject, _Pyproject))
        .bind(lambda held: committed(directory, partial(written, *held)))
        .map(lambda folder: tuple(sorted(file for file in folder.rglob("*") if file.is_file())))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["assets"]
