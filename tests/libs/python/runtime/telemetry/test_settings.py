"""Log settings read from arguments and `LOG_*` variables: level names as stdlib levels, unknown names reported, and bounds refused at construction."""

from datetime import timedelta

import pydantic
import pytest

from rasm.telemetry import LogSettings

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("name,level", [(" DeBug ", 10), ("WARNING", 30), ("critical", 50)])
def test_a_level_name_reads_in_any_case_with_space_around_it(name: str, level: int) -> None:
    """A structlog level name, any case and surrounding space ignored, reads as its stdlib level number."""
    assert LogSettings(level=name, filter={"httpx": name}).severity == level
    assert LogSettings(level=name, filter={"httpx": name}).overrides == {"httpx": level}


def test_unknown_level_names_read_as_notset_and_are_reported() -> None:
    """Names no stdlib level holds read as `NOTSET` (0), each reported with its setting and the names accepted."""
    settings = LogSettings(level="loud", filter={"httpx": "chatty", "anyio": "debug"})
    assert (settings.severity, settings.overrides) == (0, {"httpx": 0, "anyio": 10})
    assert [(setting, value) for setting, value, _ in settings.unrecognized] == [("level", "loud"), ("filter", "httpx=chatty")]
    assert all({"debug", "warning", "critical"} <= set(accepted.split()) for _, _, accepted in settings.unrecognized)


def test_a_variable_supplies_each_field_an_argument_leaves_out(monkeypatch: pytest.MonkeyPatch) -> None:
    """`LOG_<FIELD>` variables fill the fields no argument names, and an argument wins over its variable."""
    monkeypatch.setenv("LOG_LEVEL", "error")
    monkeypatch.setenv("LOG_FILES", "3")
    monkeypatch.setenv("LOG_FILTER", '{"httpx": "debug"}')
    settings = LogSettings(files=5)
    assert (settings.severity, settings.files, settings.overrides) == (40, 5, {"httpx": 10})


def test_values_at_their_bounds_construct() -> None:
    """A one-second age, a one-byte size, and one kept file are the least values accepted."""
    settings = LogSettings(age=timedelta(seconds=1), size=1, files=1)
    assert (settings.age, settings.size, settings.files) == (timedelta(seconds=1), 1, 1)


@pytest.mark.parametrize(
    "field,value,refusal",
    [
        pytest.param("age", timedelta(milliseconds=999), "greater_than_equal", id="age-under-one-second"),
        pytest.param("size", 0, "greater_than", id="size-zero"),
        pytest.param("files", 0, "greater_than", id="files-zero"),
        pytest.param("color", True, "extra_forbidden", id="unknown-field"),
    ],
)
def test_a_value_outside_its_bounds_is_refused_at_construction(field: str, value: object, refusal: str) -> None:
    """A rotation age under one second, a size or count under one, and a field the settings lack each raise one `ValidationError` naming the field and its bound."""
    with pytest.raises(pydantic.ValidationError) as refused:
        LogSettings.model_validate({field: value})
    assert [(error["loc"], error["type"]) for error in refused.value.errors()] == [((field,), refusal)]
