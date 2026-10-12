"""In-Blender half of `evaluate.py`, writing one Python literal row per check or extension step to the rows file."""

from collections.abc import Callable
import ctypes
from functools import partial
import math
import os
from pathlib import Path
import runpy
import sys
import tempfile
import traceback
from typing import NotRequired, Protocol, TypedDict, Unpack
import warnings

import addon_utils
import bpy

# --- [TYPES] ----------------------------------------------------------------------------


class Check(Protocol):
    """Check function, called with no argument and with its flip input."""

    def __call__(self, *flip: object) -> object:
        """Value the check reads from Blender."""
        ...


class Declared(TypedDict):
    """Expected value or exception class of a check, and the input that flips its verdict."""

    expect: NotRequired[object]
    flip: NotRequired[object]


class Fields(TypedDict):
    """Returned value or raised exception of one call, with its warnings and every line it printed."""

    returned: object
    raised: str | None
    warnings: list[str]
    printed: list[str]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [VALUES]


def plain(value: object) -> object:
    """Value as a Python literal, any value no finite literal holds as its `repr`."""
    match value:
        case bool() | int() | str() | None:
            return value
        case float():
            return value if math.isfinite(value) else repr(value)
        case dict():
            return {str(key): plain(item) for key, item in value.items()}
        case list() | tuple() | set() | frozenset():
            return [plain(item) for item in value]
        case _:
            return repr(value)


def fault(printed: list[str]) -> str | None:
    """First `TypeError` line printed before Blender's `could not register` line, the property definition registration refused."""
    failing = next((index for index, line in enumerate(printed) if "could not register" in line), len(printed))
    return next((line for line in printed[:failing] if line.startswith("TypeError")), None)


def held(declared: Declared, fields: Fields, raised: Exception | None) -> bool:
    """Verdict of a call against its declared expectation, a return without a raise when none is declared."""
    match declared, raised:
        case {"expect": type() as kind}, _ if issubclass(kind, BaseException):
            return isinstance(raised, kind)
        case _, Exception():
            return False
        case {"expect": expect}, None:
            return fields["returned"] == plain(expect)
        case _:
            return True


# --- [CALLS]


def observed(call: Callable[[], object]) -> tuple[Fields, Exception | None]:
    """Fields of one call with every write to file descriptors 1 and 2 captured, and the exception it raised."""
    libc = ctypes.CDLL(None)
    saved = (os.dup(1), os.dup(2))
    with tempfile.TemporaryFile() as sink, warnings.catch_warnings(record=True) as caught:
        warnings.simplefilter("always")
        sys.stdout.flush()
        sys.stderr.flush()
        libc.fflush(None)
        os.dup2(sink.fileno(), 1)
        os.dup2(sink.fileno(), 2)
        try:
            returned, raised = call(), None
        except (ArithmeticError, AttributeError, ImportError, LookupError, OSError, ReferenceError, RuntimeError, SystemError, TypeError, ValueError) as error:
            returned, raised = None, error
        sys.stdout.flush()
        sys.stderr.flush()
        libc.fflush(None)
        os.dup2(saved[0], 1)
        os.dup2(saved[1], 2)
        sink.seek(0)
        printed = sink.read().decode("utf-8", "replace").splitlines()
    os.close(saved[0])
    os.close(saved[1])
    fields = Fields(
        returned=plain(returned),
        raised=None if raised is None else "".join(traceback.format_exception_only(raised)).strip(),
        warnings=[f"{caught_warning.category.__name__}: {caught_warning.message}" for caught_warning in caught],
        printed=printed,
    )
    return fields, raised


def written(raw: Path, row: dict[str, object]) -> None:
    """Append one row as a Python literal line."""
    with raw.open("a", encoding="utf-8") as stream:
        stream.write(f"{row!r}\n")


def declared(raw: Path, checks: list[str]) -> None:
    """Write the header row naming this process's build, argv, and declared check ids."""
    written(raw, {"kind": "blender", "version": bpy.app.version_string, "build_hash": bpy.app.build_hash.decode(), "argv": sys.argv, "checks": checks})


def registered(path: Path) -> list[tuple[str, Check, Declared]]:
    """Checks a checks file declares through the `check` decorator it receives as a global, in file order."""
    found: list[tuple[str, Check, Declared]] = []

    def check(identifier: str, **declaration: Unpack[Declared]) -> Callable[[Check], Check]:
        def kept(fn: Check) -> Check:
            found.append((identifier, fn, declaration))
            return fn

        return kept

    runpy.run_path(str(path), init_globals={"check": check})
    return found


def evaluated(raw: Path, found: list[tuple[str, Check, Declared]], falsify: bool, selected: list[str]) -> None:
    """Run the selected checks from factory settings, every check when none is selected."""
    declared(raw, [identifier for identifier, _, _ in found])
    for identifier, fn, declaration in (entry for entry in found if entry[0] in selected or not selected):
        bpy.ops.wm.read_factory_settings()
        fields, raised = observed(fn)
        verdict = held(declaration, fields, raised)
        match falsify, declaration:
            case True, {"flip": flip}:
                bpy.ops.wm.read_factory_settings()
                flipped: bool | None = held(declaration, *observed(partial(fn, flip))) != verdict
            case True, _:
                flipped = False
            case _:
                flipped = None
        written(raw, {"kind": "check", "id": identifier, **fields, "verdict": verdict and flipped is not False, "flipped": flipped})


def cycled(raw: Path, module: str) -> None:
    """Disable, enable, and unregister an installed extension, reading its enabled state and first registration fault at each step."""
    enable = partial(addon_utils.enable, module, default_set=True)
    disable = partial(addon_utils.disable, module, default_set=True)
    steps = (
        ("installed", lambda: None, True),
        ("disable", disable, False),
        ("enable", enable, True),
        ("disable-again", disable, False),
        ("enable-again", enable, True),
        ("unregister", disable, False),
    )
    declared(raw, [f"extension-{step}" for step, _, _ in steps])
    for step, act, expect in steps:
        fields, raised = observed(act)
        enabled, loaded = addon_utils.check(module)
        verdict = raised is None and enabled == loaded == expect
        written(raw, {"kind": "check", "id": f"extension-{step}", **fields, "returned": {"enabled": enabled, "loaded": loaded, "fault": fault(fields["printed"])}, "verdict": verdict, "flipped": None})


# --- [COMPOSITION] ----------------------------------------------------------------------


def main(argv: list[str]) -> None:
    """Run the mode `evaluate.py` passes after `--`."""
    match argv:
        case ["declare", raw, path]:
            declared(Path(raw), [identifier for identifier, _, _ in registered(Path(path))])
        case ["run", raw, path, mode, *selected]:
            evaluated(Path(raw), registered(Path(path)), falsify=mode == "falsify", selected=selected)
        case ["cycle", raw, module]:
            cycled(Path(raw), module)
        case _:
            sys.exit(f"in_blender.py received {argv!r} after --, pass declare, run, or cycle arguments")


if __name__ == "__main__":
    main(sys.argv[sys.argv.index("--") + 1 :])

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
