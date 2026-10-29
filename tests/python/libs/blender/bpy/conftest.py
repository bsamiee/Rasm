"""Session fixtures running the background Blender session over the fixture extension once and reviving its observations into results and options."""

from collections.abc import Mapping
lazy from functools import reduce
lazy from pathlib import Path

import msgspec
import pytest
lazy from expression import downcast, Error, Nothing, Ok, Option, Result, Some

lazy from eng.python.fitout.__main__ import app

# --- [TYPES] ----------------------------------------------------------------------------

type Step = str | int

# --- [MODELS] ---------------------------------------------------------------------------


class Observations(msgspec.Struct, frozen=True):
    """Probe observations keyed `<area>.<probe>`, read by key and the steps into its value."""

    values: Mapping[str, object]

    def __getitem__(self, path: str | tuple[str, *tuple[Step, ...]]) -> object:
        """Returns the observation at a key, or at the steps into the observation under its key."""

        def stepped(node: object, step: Step) -> object:
            match node, step:
                case Result(tag="ok", ok=value), "ok":
                    return value
                case Option(tag="some", some=value), "some":
                    return value
                case Mapping(), str():
                    return node[step]
                case tuple(), int():
                    return node[step]
                case _:
                    raise KeyError(step)

        key, *steps = (path,) if isinstance(path, str) else path
        return reduce(stepped, steps, self.values[key])

    def of[T](self, kind: type[T], path: str | tuple[str, *tuple[Step, ...]]) -> T:
        """Returns the observation at a path narrowed to a kind, failing the asserting test for another kind."""
        return downcast(kind, self[path])


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


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="session")
def session(tmp_path_factory: pytest.TempPathFactory, worker_id: str) -> Mapping[str, Observations]:
    """Runs one headless factory-settings Blender over the session script through the fitout `run` command, decoding the observations of each phase."""
    target = tmp_path_factory.mktemp("bpy") / "observed.json"
    script = Path(__file__).parent / "background" / "session.py"
    command, bound, _ = app.parse_args(["run", f"bpy-{worker_id}", "--", "-b", "--factory-startup", "--python-exit-code", "1", "--python", str(script), "--", str(target)])
    assert command(*bound.args, **bound.kwargs) == Ok(0)
    phases = msgspec.json.decode(target.read_bytes(), type=dict[str, dict[str, object]])
    return {phase: Observations({key: revived(value) for key, value in values.items()}) for phase, values in phases.items()}


@pytest.fixture(scope="session")
def observed(session: Mapping[str, Observations]) -> Observations:
    """Observations read while the fixture extension is enabled."""
    return session["enabled"]


@pytest.fixture(scope="session")
def released(session: Mapping[str, Observations]) -> Observations:
    """Observations read after the fixture extension is disabled."""
    return session["released"]
