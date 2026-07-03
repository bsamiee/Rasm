"""Seam-substrate falsification laws: call-shape recording, fixture writers, decode oracles, process doubles."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from types import SimpleNamespace
from typing import TYPE_CHECKING

import msgspec
import psutil
import pytest

from tests.python._testkit.seams import (
    Async,
    autospec_proc,
    Factory,
    FanOut,
    install_module_attr,
    NdjsonOracle,
    psutil_module_double,
    SeamProbe,
    Sync,
    tmp_root,
    VariantWriter,
)


if TYPE_CHECKING:
    from pathlib import Path


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CALL_SHAPES]


def test_sync_shape_records_args_projects_and_returns(monkeypatch: pytest.MonkeyPatch) -> None:
    """The Sync seam returns its canned value, records the exact call, and feeds the projection."""
    owner = SimpleNamespace(op=None)
    probe: SeamProbe[int] = SeamProbe(project=lambda args: [item for item in args if isinstance(item, int)])
    probe.install(monkeypatch, owner, "op", Sync(7))
    assert owner.op(3, key="v") == 7
    assert probe.calls == [("op", (3,), {"key": "v"})]
    assert probe.captured == [3]


@pytest.mark.anyio
async def test_async_shape_is_awaitable_and_records(monkeypatch: pytest.MonkeyPatch) -> None:
    """The Async seam yields its value only through await and records the call."""
    owner = SimpleNamespace(op=None)
    probe: SeamProbe[object] = SeamProbe()
    probe.install(monkeypatch, owner, "op", Async("done"))
    assert await owner.op(1) == "done"
    assert probe.calls == [("op", (1,), {})]


def test_fanout_shape_records_items_as_sole_positional(monkeypatch: pytest.MonkeyPatch) -> None:
    """The FanOut seam returns the canned batch and records the items collection as one positional."""
    owner = SimpleNamespace(op=None)
    probe: SeamProbe[object] = SeamProbe()
    probe.install(monkeypatch, owner, "op", FanOut((10, 20)))
    assert owner.op(["a", "b"], flag=True) == (10, 20)
    assert probe.calls == [("op", (["a", "b"],), {"flag": True})]


def test_factory_shape_records_bind_layer_then_logs_inner_calls(monkeypatch: pytest.MonkeyPatch) -> None:
    """The Factory seam records the bind call once, then logs every inner call under its label."""
    owner = SimpleNamespace(op=None)
    probe: SeamProbe[object] = SeamProbe()
    probe.install(monkeypatch, owner, "op", Factory(9, inner_label="<f>.run"))
    runner = owner.op("cfg", mode="m")
    assert (runner(5), runner(6)) == (9, 9)
    assert probe.calls == [("op", ("cfg",), {"mode": "m"}), ("<f>.run", (5,), {}), ("<f>.run", (6,), {})]
    assert probe.projected(lambda call: [call[0]] if call[0] == "<f>.run" else []) == ["<f>.run", "<f>.run"]


# --- [FIXTURE_WRITERS]


def test_variant_writer_emits_raw_encodes_objects_and_withholds_absent(tmp_path: Path) -> None:
    """Raw bytes land verbatim, objects encode through the codec, and absent variants never touch disk."""
    writer: VariantWriter[str] = VariantWriter(
        directory=tmp_path / "variants",
        names={"raw": "raw.bin", "obj": "obj.json", "gone": "gone.json"},
        payloads={"raw": b"\x00\x01", "obj": {"key": 1}},
        absent=frozenset({"gone"}),
    )
    paths = writer.write_all()
    assert paths["raw"].read_bytes() == b"\x00\x01", "raw bytes were re-encoded instead of written verbatim"
    assert msgspec.json.decode(paths["obj"].read_bytes()) == {"key": 1}, "object payload did not encode through the codec"
    assert not paths["gone"].exists(), "an absent variant was materialized"


def test_tmp_root_writes_nested_text_and_applies_mode(tmp_path: Path) -> None:
    """The TmpRoot write primitive creates parents, writes UTF-8 text, and applies the requested mode."""
    kit = tmp_root(tmp_path, lambda root: root / "settings.toml")
    assert kit.settings == tmp_path / "settings.toml", "settings projection lost the injected factory"
    written = kit.write("nest/run.sh", "payload", mode=0o755)
    assert written.read_text(encoding="utf-8") == "payload"
    assert written.stat().st_mode & 0o777 == 0o755, "mode was not applied"


# --- [DECODE_ORACLES]


def test_ndjson_oracle_decodes_every_row_and_gates_the_exact_count() -> None:
    """A multi-line oracle decodes all rows in order; a count drift fails, and one() refuses multi-line oracles."""
    stream: NdjsonOracle[dict] = NdjsonOracle(msgspec.json.Decoder(dict), expect_lines=2)
    assert stream.rows(b'{"a":1}\n{"a":2}\n') == ({"a": 1}, {"a": 2})
    with pytest.raises(AssertionError, match="expected exactly 2"):
        stream.rows(b'{"a":1}\n')
    with pytest.raises(AssertionError, match="single-write"):
        stream.one(b'{"a":1}\n{"a":2}\n')


def test_ndjson_one_write_contract_reds_on_double_write(capsys: pytest.CaptureFixture[str]) -> None:
    """The default oracle is the one-write contract: a second NDJSON line is a failure, and capture decodes."""
    oracle: NdjsonOracle[dict] = NdjsonOracle(msgspec.json.Decoder(dict))
    assert oracle.one(b'{"a":1}\n') == {"a": 1}
    with pytest.raises(AssertionError, match="expected exactly 1"):
        oracle.one(b'{"a":1}\n{"a":2}\n')
    print('{"a":3}')  # noqa: T201  # the capture arm needs a real stdout write
    assert oracle.from_capture(capsys) == {"a": 3}


# --- [PROCESS_DOUBLES]


def test_process_doubles_dispatch_by_pid_and_raise_on_dead_or_unknown() -> None:
    """autospec doubles carry fields, method returns, and death; the module double routes pids and raises not_found."""
    live = autospec_proc(psutil.Process, fields={"pid": 42}, methods={"cpu_percent": 12.5})
    dead = autospec_proc(psutil.Process, fields={"pid": 7}, dead=True)
    assert (live.pid, live.cpu_percent()) == (42, 12.5)
    fake = psutil_module_double(psutil, {42: live, 7: dead}, not_found=psutil.NoSuchProcess, extra={"boot_time": lambda: 0.0})
    assert fake.Process(42) is live
    assert fake.boot_time() == 0.0
    with pytest.raises(psutil.NoSuchProcess):
        fake.Process(7)
    with pytest.raises(psutil.NoSuchProcess):
        fake.Process(999)


def test_install_module_attr_pins_and_returns_the_double(monkeypatch: pytest.MonkeyPatch) -> None:
    """The pinned double is returned for fluent post-configuration and owns the attribute for the test."""
    owner = SimpleNamespace(attr=None)
    double = install_module_attr(monkeypatch, owner, "attr", SimpleNamespace(cpu=1))
    assert owner.attr is double
    double.cpu = 2
    assert owner.attr.cpu == 2
