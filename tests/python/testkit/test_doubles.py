"""Test-double falsification laws: call-shape recording, virtual time, fixture writers, decode oracles."""

# --- [IMPORTS] --------------------------------------------------------------------------

import time
from types import SimpleNamespace
from typing import TYPE_CHECKING

import anyio
import msgspec
import pytest

from tests.python.testkit.doubles import Async, autojump_backend, Factory, FanOut, NdjsonOracle, CallProbe, Sync, VariantWriter

if TYPE_CHECKING:
    from pathlib import Path


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CALL_SHAPES]


def test_sync_shape_records_args_projects_and_returns(monkeypatch: pytest.MonkeyPatch) -> None:
    """The Sync double returns its canned value, records the exact call, and feeds the projection."""
    owner = SimpleNamespace(op=None)
    probe: CallProbe[int] = CallProbe(project=lambda args: [item for item in args if isinstance(item, int)])
    probe.install(monkeypatch, owner, "op", Sync(7))
    assert owner.op(3, key="v") == 7
    assert probe.calls == [("op", (3,), {"key": "v"})]
    assert probe.captured == [3]


@pytest.mark.anyio
async def test_async_shape_is_awaitable_and_records(monkeypatch: pytest.MonkeyPatch) -> None:
    """The Async double yields its value only through await and records the call."""
    owner = SimpleNamespace(op=None)
    probe: CallProbe[object] = CallProbe()
    probe.install(monkeypatch, owner, "op", Async("done"))
    assert await owner.op(1) == "done"
    assert probe.calls == [("op", (1,), {})]


@pytest.mark.anyio
@pytest.mark.parametrize("anyio_backend", [pytest.param(autojump_backend(), id="autojump")])
async def test_autojump_backend_collapses_virtual_time() -> None:
    """An hour of sleeps and a fired deadline both prove in wall-milliseconds under the autojumping clock."""
    start = time.perf_counter()
    await anyio.sleep(3600)
    with anyio.move_on_after(300) as scope:
        await anyio.sleep(600)
    assert scope.cancelled_caught, "the virtual deadline never fired"
    assert time.perf_counter() - start < 5.0, "virtual time leaked into wall time"


def test_fanout_shape_records_items_as_sole_positional(monkeypatch: pytest.MonkeyPatch) -> None:
    """The FanOut double returns the canned batch and records the items collection as one positional."""
    owner = SimpleNamespace(op=None)
    probe: CallProbe[object] = CallProbe()
    probe.install(monkeypatch, owner, "op", FanOut((10, 20)))
    assert owner.op(["a", "b"], flag=True) == (10, 20)
    assert probe.calls == [("op", (["a", "b"],), {"flag": True})]


def test_factory_shape_records_bind_layer_then_logs_inner_calls(monkeypatch: pytest.MonkeyPatch) -> None:
    """The Factory double records the bind call once, then logs every inner call under its label."""
    owner = SimpleNamespace(op=None)
    probe: CallProbe[object] = CallProbe()
    probe.install(monkeypatch, owner, "op", Factory(9, inner_label="<f>.run"))
    runner = owner.op("cfg", mode="m")
    assert (runner(5), runner(6)) == (9, 9)
    assert probe.calls == [("op", ("cfg",), {"mode": "m"}), ("<f>.run", (5,), {}), ("<f>.run", (6,), {})]
    assert probe.projected(lambda call: [call[0]] if call[0] == "<f>.run" else []) == ["<f>.run", "<f>.run"]


# --- [FIXTURE_WRITERS]


def test_variant_writer_emits_raw_encodes_objects_and_withholds_absent(tmp_path: Path) -> None:
    """Raw bytes land verbatim, objects encode through the codec, and absent variants never touch disk."""
    writer: VariantWriter[str] = VariantWriter(directory=tmp_path / "variants", names={"raw": "raw.bin", "obj": "obj.json", "gone": "gone.json"}, payloads={"raw": b"\x00\x01", "obj": {"key": 1}}, absent=frozenset({"gone"}))
    paths = writer.write_all()
    assert paths["raw"].read_bytes() == b"\x00\x01", "raw bytes were re-encoded instead of written verbatim"
    assert msgspec.json.decode(paths["obj"].read_bytes()) == {"key": 1}, "object payload did not encode through the codec"
    assert not paths["gone"].exists(), "an absent variant was materialized"


# --- [DECODE_ORACLES]


def test_ndjson_oracle_decodes_every_row_and_gates_the_exact_count() -> None:
    """A multi-line oracle decodes all rows in order; a count drift fails, and one() refuses multi-line oracles."""
    stream: NdjsonOracle[dict[str, int]] = NdjsonOracle(msgspec.json.Decoder(dict[str, int]), expect_lines=2)
    assert stream.rows(b'{"a":1}\n{"a":2}\n') == ({"a": 1}, {"a": 2})
    with pytest.raises(AssertionError, match="expected exactly 2"):
        stream.rows(b'{"a":1}\n')
    with pytest.raises(AssertionError, match="single-write"):
        stream.one(b'{"a":1}\n{"a":2}\n')


def test_ndjson_one_write_contract_reds_on_double_write(capsys: pytest.CaptureFixture[str]) -> None:
    """The default oracle is the one-write contract: a second NDJSON line is a failure, and capture decodes."""
    oracle: NdjsonOracle[dict[str, int]] = NdjsonOracle(msgspec.json.Decoder(dict[str, int]))
    assert oracle.one(b'{"a":1}\n') == {"a": 1}
    with pytest.raises(AssertionError, match="expected exactly 1"):
        oracle.one(b'{"a":1}\n{"a":2}\n')
    print('{"a":3}')  # ruff:ignore[print]
    assert oracle.from_capture(capsys) == {"a": 3}
