"""Session fixture running the background Blender session over the fixture extension once, its observations read by tuple paths, and the case table parametrization every bpy test module takes."""

from collections.abc import Iterable, Mapping
lazy from functools import reduce
lazy from pathlib import Path

import msgspec
import pytest
lazy from expression import Error, Nothing, Ok, Some

lazy from eng.python.fitout.__main__ import app

# --- [TYPES] ----------------------------------------------------------------------------

type Steps = tuple[str | int, ...]

# --- [MODELS] ---------------------------------------------------------------------------


class Observations(msgspec.Struct, frozen=True):
    """Probe observations as decoded JSON nested by phase, fixture area, and probe, read by the steps into a revived value."""

    values: Mapping[str, object]

    def __getitem__(self, path: Steps, /) -> object:
        """Returns the observation the steps reach, keys into objects, `ok` and `some` into one-key result and option objects, indices into arrays."""

        def stepped(node: object, step: str | int) -> object:
            match node, step:
                case Mapping(), str():
                    return node[step]
                case list(), int():
                    return node[step]
                case _:
                    raise KeyError(step)

        root: object = self.values
        return revived(reduce(stepped, path, root))


# --- [OPERATIONS] -----------------------------------------------------------------------


def revived(node: object) -> object:
    """Returns a JSON observation with one-key result and option objects as `Ok`, `Error`, `Some`, and `Nothing`, and arrays as tuples."""
    match node:
        case {"ok": value, **rest} if not rest:
            return Ok(revived(value))
        case {"error": value, **rest} if not rest:
            return Error(revived(value))
        case {"some": value, **rest} if not rest:
            return Some(revived(value))
        case {"nothing": None, **rest} if not rest:
            return Nothing
        case dict():
            return {key: revived(value) for key, value in node.items()}
        case list():
            return tuple(map(revived, node))
        case _:
            return node


def observing(cases: Iterable[tuple[Steps, object]], /) -> pytest.MarkDecorator:
    """Parametrizes `path` and `expected` over a case table, each case identified by its path."""
    return pytest.mark.parametrize("path,expected", [pytest.param(path, expected, id="/".join(map(str, path))) for path, expected in cases])


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="session")
def observed(tmp_path_factory: pytest.TempPathFactory, worker_id: str) -> Observations:
    """Runs one headless factory-settings Blender over the session script through the fitout `run` command, decoding the observations of both phases."""
    target = tmp_path_factory.mktemp("bpy") / "observed.json"
    script = Path(__file__).parent / "background" / "session.py"
    command, bound, _ = app.parse_args(["run", f"bpy-{worker_id}", "--", "-b", "--factory-startup", "--python-exit-code", "1", "--python", str(script), "--", str(target)])
    assert command(*bound.args, **bound.kwargs) == Ok(0)
    return Observations(msgspec.json.decode(target.read_bytes(), type=dict[str, object]))
