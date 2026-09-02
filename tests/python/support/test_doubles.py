"""Tests for call recording, virtual time, fixture writers, and NDJSON decoding."""

# --- [IMPORTS] --------------------------------------------------------------------------

import time
from types import SimpleNamespace
from typing import TYPE_CHECKING

import anyio
import msgspec
import pytest

from tests.python.support.doubles import Async, autojump_backend, Batch, CallSpy, Factory, NdjsonOracle, Sync, VariantWriter

if TYPE_CHECKING:
    from pathlib import Path


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CALL_RECORDING]


def test_sync_stub_records_arguments_projects_and_returns(monkeypatch: pytest.MonkeyPatch) -> None:
    """The synchronous stub returns its fixed value, records the call, and applies the projection."""
    owner = SimpleNamespace(op=None)
    spy: CallSpy[int] = CallSpy(project=lambda args: [item for item in args if isinstance(item, int)])
    spy.install(monkeypatch, owner, "op", Sync(7))
    assert owner.op(3, key="v") == 7
    assert spy.calls == [("op", (3,), {"key": "v"})]
    assert spy.captured == [3]


@pytest.mark.anyio
async def test_async_stub_is_awaitable_and_records(monkeypatch: pytest.MonkeyPatch) -> None:
    """The Async double returns its value through await and records the call."""
    owner = SimpleNamespace(op=None)
    spy: CallSpy[object] = CallSpy()
    spy.install(monkeypatch, owner, "op", Async("done"))
    assert await owner.op(1) == "done"
    assert spy.calls == [("op", (1,), {})]


@pytest.mark.anyio
@pytest.mark.parametrize("anyio_backend", [pytest.param(autojump_backend(), id="autojump")])
async def test_autojump_backend_collapses_virtual_time() -> None:
    """Hour-long virtual sleeps and a deadline finish within 5 wall-clock seconds under the autojumping clock."""
    start = time.perf_counter()
    await anyio.sleep(3600)
    with anyio.move_on_after(300) as scope:
        await anyio.sleep(600)
    assert scope.cancelled_caught, "the virtual deadline never fired"
    assert time.perf_counter() - start < 5.0, "virtual-time advancement exceeded the wall-time limit"


def test_batch_stub_records_items_as_sole_positional(monkeypatch: pytest.MonkeyPatch) -> None:
    """The batch stub returns its fixed results and records the item collection as its argument."""
    owner = SimpleNamespace(op=None)
    spy: CallSpy[object] = CallSpy()
    spy.install(monkeypatch, owner, "op", Batch((10, 20)))
    assert owner.op(["a", "b"], flag=True) == (10, 20)
    assert spy.calls == [("op", (["a", "b"],), {"flag": True})]


def test_factory_stub_records_factory_and_returned_function_calls(monkeypatch: pytest.MonkeyPatch) -> None:
    """The Factory double records construction once, then records every call to the returned function."""
    owner = SimpleNamespace(op=None)
    spy: CallSpy[object] = CallSpy()
    spy.install(monkeypatch, owner, "op", Factory(9, inner_label="<f>.run"))
    runner = owner.op("cfg", mode="m")
    assert (runner(5), runner(6)) == (9, 9)
    assert spy.calls == [("op", ("cfg",), {"mode": "m"}), ("<f>.run", (5,), {}), ("<f>.run", (6,), {})]
    assert spy.projected(lambda call: [call[0]] if call[0] == "<f>.run" else []) == ["<f>.run", "<f>.run"]


# --- [FIXTURE_WRITERS]


def test_variant_writer_writes_raw_encodes_objects_and_skips_absent(tmp_path: Path) -> None:
    """Raw bytes are written unchanged, objects encode through the codec, and absent variants create no files."""
    writer: VariantWriter[str] = VariantWriter(
        directory=tmp_path / "variants",
        names={"raw": "raw.bin", "obj": "obj.json", "gone": "gone.json"},
        contents={"raw": b"\x00\x01", "obj": {"key": 1}},
        absent=frozenset({"gone"}),
    )
    paths = writer.write_all()
    assert paths["raw"].read_bytes() == b"\x00\x01", "raw bytes were re-encoded instead of written verbatim"
    assert msgspec.json.decode(paths["obj"].read_bytes()) == {"key": 1}, "object content did not encode through the codec"
    assert not paths["gone"].exists(), "an absent variant was materialized"


# --- [DECODE_ORACLES]


def test_ndjson_decoder_decodes_every_row_and_checks_the_exact_count() -> None:
    """Multiline decoders preserve row order, check the count, and reject ``one()`` for many rows."""
    stream: NdjsonOracle[dict[str, int]] = NdjsonOracle(msgspec.json.Decoder(dict[str, int]), expect_lines=2)
    assert stream.rows(b'{"a":1}\n{"a":2}\n') == ({"a": 1}, {"a": 2})
    with pytest.raises(AssertionError, match="expected exactly 2"):
        stream.rows(b'{"a":1}\n')
    with pytest.raises(AssertionError, match="single-write"):
        stream.one(b'{"a":1}\n{"a":2}\n')


def test_ndjson_one_write_contract_fails_on_double_write(capsys: pytest.CaptureFixture[str]) -> None:
    """The default oracle expects a single write, an extra NDJSON line fails, and captured output decodes."""
    oracle: NdjsonOracle[dict[str, int]] = NdjsonOracle(msgspec.json.Decoder(dict[str, int]))
    assert oracle.one(b'{"a":1}\n') == {"a": 1}
    with pytest.raises(AssertionError, match="expected exactly 1"):
        oracle.one(b'{"a":1}\n{"a":2}\n')
    print('{"a":3}')  # ruff:ignore[print]
    assert oracle.from_capture(capsys) == {"a": 3}
