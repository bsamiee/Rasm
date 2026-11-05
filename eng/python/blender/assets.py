"""Writes the one folder `pack` overlays on its stage, holding what an extension's declarations make: encoded icons, pinned upstream files, interface themes."""

lazy from collections.abc import Iterable
lazy import os
lazy from pathlib import Path

import msgspec
lazy import anyio
lazy from coloraide import Color
lazy from cyclopts.types import ResolvedExistingDirectory, ResolvedPath
lazy import pooch  # type: ignore[import-untyped]

lazy from blender.records.faults import accumulated, Error, Fault, Ok, Outcome, paired, Reason
lazy from blender.records.stores import committed, folder_made
lazy from eng.python.blender.host import Blender, declared, Host, probed, Pyproject, root, scripted, site
lazy from eng.python.blender.icons import encoded

# --- [MODELS] ---------------------------------------------------------------------------


class Pin(msgspec.Struct, frozen=True):
    """File fetched at a pinned URL, checked by sha256, and linked at its target."""

    url: str
    sha256: str
    target: Path


# --- [OPERATIONS] -----------------------------------------------------------------------


def fetched(base: Path, pins: Iterable[Pin], /) -> Outcome[tuple[Path, ...]]:
    """Links each pin's file at its target from the workspace's content store at `.cache/fetched/<sha256>`, pooch fetching a missing or altered one.

    Args:
        base: Workspace root.
        pins: Files to link, their targets absent.

    Returns:
        Targets in input order, or the fault of every failed fetch and link.
    """

    def linked(pin: Pin) -> Outcome[Path]:
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
        preview, icon = encoded(master, paint, turned=turned)
    except (OSError, ValueError, SyntaxError) as error:
        return Error(Fault.of(Reason.UNREADABLE, str(master), error))
    try:
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(preview)
        target.with_suffix(".dat").write_bytes(icon)
    except OSError as error:
        return Error(Fault.of(Reason.UNWRITABLE, str(target), error))
    return Ok(target)


def assets(source: ResolvedExistingDirectory, directory: ResolvedPath, /, *, blender: Blender) -> Outcome[int]:
    """Writes what an extension folder declares at its stage-relative path, each part present once its declaration is.

    Every `icons/**/*.svg` master becomes a preview and geometry icon, `currentColor` painting the factory theme's tool text color.
    Every `[tool.blender.turned]` row names a variant stem and its master, the variant the master turned a quarter clockwise and written beside the masters.
    Every `[tool.blender.fetch."<folder>"]` row's files reach that folder from the content store.
    Every `[tool.blender.theme."<name>"]` table becomes `<name>.xml`, the files a `type = "theme"` extension holds, the first by name the one its installer enables.

    Args:
        source: Extension folder.
        directory: Folder replaced in one step.
        blender: Blender executable.

    Returns:
        Zero, or fault of the probe, the folder commit, and every part.
    """
    masters = source / "icons"
    pyproject = source / "pyproject.toml"

    def themed(stage: Path) -> Outcome[object]:
        return site(blender=blender, package="blender").bind(lambda folder: scripted(blender, Path(__file__).with_name("theme.py"), pyproject, stage, env=os.environ | {"PYTHONUSERBASE": str(folder)}))

    def written(stage: Path, base: Path, host: Host, project: Pyproject) -> Outcome[object]:
        paint = Color("srgb", list(host.text)).to_string(hex=True)
        table = project.tool.blender
        rows = (
            *((master.relative_to(masters), master, False) for master in sorted(masters.glob("**/*.svg"))),
            *((Path(f"{variant}.svg"), masters / f"{master}.svg", True) for variant, master in table.turned.items()),
        )
        return paired(
            folder_made(stage),
            accumulated(_icon(stage, paint, name, master, turned=turn) for name, master, turn in rows),
            fetched(base, (Pin(f"{upstream.base}/{file}", sha256, stage / folder / file) for folder, upstream in table.fetch.items() for file, sha256 in upstream.files.items())),
            themed(stage) if table.theme else Ok(None),
        )

    return paired(root(), probed(blender), declared(pyproject, Pyproject)).bind(lambda held: committed(directory, lambda stage: written(stage, *held))).map(lambda _: 0)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Pin", "assets", "fetched"]
