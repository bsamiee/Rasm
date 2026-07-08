"""Falsification laws for the contracts-corpus reader, audit fold, and round-trip oracle."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

import pytest

from tests.python._testkit.corpus import assert_corpus_roundtrip, CorpusEntry, CorpusManifest, load_manifest
from tests.python._testkit.runtime import REPO_ROOT


if TYPE_CHECKING:
    from collections.abc import Callable
    from pathlib import Path

    type _Builder = Callable[[Path], tuple[CorpusManifest, Path]]


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (CorpusEntry, CorpusManifest, load_manifest, assert_corpus_roundtrip)

_HEADER = "| [INDEX] | [FIXTURE] | [SEAM] | [PRODUCER] | [PAYLOAD] | [PIN] |\n| :-: | :-- | :-- | :-- | :-- | :-- |"

# --- [OPERATIONS] -----------------------------------------------------------------------


def _corpus(
    tmp_path: Path,
    *,
    pin: str = "REAL",
    blocker: str = "",
    expectation: str = "the frozen bytes",
    payload: str = "`wire-bytes` + `digest`",
    producer: str = "`csharp:Demo/Page/one#CLUSTER`",
    ledger_row: str | None = None,
    page: bool = True,
    asset: bytes | None = None,
) -> tuple[CorpusManifest, Path]:
    """Materialize a one-fixture synthetic corpus tree and its libs planning root.

    Returns:
        The decoded manifest and the synthetic ``libs`` root for audit calls.
    """
    root = tmp_path / "contracts"
    root.mkdir(parents=True, exist_ok=True)
    row = ledger_row if ledger_row is not None else f"| [01] | DEMO_FIX | `demo-seam` | {producer} | {payload} | {pin} |"
    entry_fields = [
        "- Seam: `demo-seam`",
        f"- Producer: {producer}",
        "- Consumers: `python:kit`",
        f"- Payload: {payload}",
        f"- Pin: {pin}",
        *([f"- Blocker: {blocker}"] if blocker else []),
        "- Shape: demo bytes",
        *([f"- Expectation: {expectation}"] if expectation else []),
        "- Regenerate when: the demo contract changes",
    ]
    body = f"# [DEMO_MANIFEST]\n\n## [01]-[LEDGER]\n\n{_HEADER}\n{row}\n\n## [02]-[ENTRIES]\n\n### [02.1]-[DEMO_FIX]\n"
    (root / "MANIFEST.md").write_text(body + "\n".join(entry_fields) + "\n", encoding="utf-8")
    libs = tmp_path / "libs"
    if page:
        target = libs / "csharp" / "Demo" / ".planning" / "Page" / "one.md"
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text("# demo\n", encoding="utf-8")
    if asset is not None:
        (root / "demo-seam").mkdir(exist_ok=True)
        (root / "demo-seam" / "demo.bin").write_bytes(asset)
    return load_manifest(root), libs


# --- [LIVE_CORPUS_GATE]


def test_live_contracts_manifest_parses_and_audits_clean() -> None:
    """The committed corpus registry decodes to typed entries and folds zero honesty defects."""
    manifest = load_manifest(REPO_ROOT / "tests" / "contracts")
    assert manifest.entries, "the live manifest decoded to zero fixture entries"
    assert {e.fixture for e in manifest.entries} == {row[0] for row in manifest.ledger}, "ledger and entry fixture sets drifted"
    defects = manifest.audit(REPO_ROOT / "libs")
    assert defects == (), "live corpus audit found honesty defects:\n" + "\n".join(defects)


# --- [AUDIT_FALSIFICATION]


@pytest.mark.parametrize(
    "build, fragment",
    [
        pytest.param(_corpus, None, id="clean"),
        pytest.param(lambda t: _corpus(t, pin="PINNED"), "outside", id="unknown-pin"),
        pytest.param(lambda t: _corpus(t, expectation=""), "missing Expectation", id="real-without-expectation"),
        pytest.param(lambda t: _corpus(t, blocker="gap"), "carries Blocker", id="real-with-blocker"),
        pytest.param(lambda t: _corpus(t, pin="DESIGN-PIN", expectation=""), "missing Blocker", id="design-pin-without-blocker"),
        pytest.param(lambda t: _corpus(t, pin="DESIGN-PIN", blocker="gap"), "carries Expectation", id="design-pin-with-expectation"),
        pytest.param(lambda t: _corpus(t, payload="`wire-bytes` + `parquet`"), "closed vocabulary", id="payload-outside-vocabulary"),
        pytest.param(lambda t: _corpus(t, payload="none"), "no payload kind", id="payload-empty"),
        pytest.param(lambda t: _corpus(t, producer="`csharp:Demo/Page/absent#CLUSTER`"), "resolves to no planning page", id="dangling-anchor"),
        pytest.param(lambda t: _corpus(t, page=False), "resolves to no planning page", id="planning-page-missing"),
        pytest.param(
            lambda t: _corpus(t, ledger_row="| [01] | OTHER_FIX | `demo-seam` | x | `wire-bytes` | REAL |"), "no H3 entry", id="ledger-entry-drift"
        ),
        pytest.param(
            lambda t: _corpus(t, ledger_row="| [01] | DEMO_FIX | `other-seam` | x | `wire-bytes` | REAL |"), "!= entry seam", id="ledger-seam-drift"
        ),
        pytest.param(
            lambda t: _corpus(t, ledger_row="| [01] | DEMO_FIX | `demo-seam` | x | `wire-bytes` | DESIGN-PIN |"),
            "!= entry pin",
            id="ledger-pin-drift",
        ),
    ],
)
def test_audit_fires_each_honesty_defect_by_name(tmp_path: Path, build: _Builder, fragment: str | None) -> None:
    """Every corpus-law defect class fires with its named fragment; the clean tree folds empty."""
    manifest, libs = build(tmp_path)
    defects = manifest.audit(libs)
    if fragment is None:
        assert defects == (), f"clean synthetic corpus reported defects: {defects}"
    else:
        assert any(fragment in d for d in defects), f"expected a defect naming {fragment!r}, got {defects}"


def test_audit_flags_unregistered_and_design_pin_asset_directories(tmp_path: Path) -> None:
    """A seam directory without an entry and a DESIGN-PIN seam with assets both fail by name."""
    manifest, libs = _corpus(tmp_path, asset=b"payload")
    (manifest.root / "rogue-seam").mkdir()
    (manifest.root / "rogue-seam" / "x.bin").write_bytes(b"x")
    defects = manifest.audit(libs)
    assert any("rogue-seam" in d and "no manifest entry" in d for d in defects), f"unregistered directory not flagged: {defects}"

    pinned, libs2 = _corpus(tmp_path / "pin", pin="DESIGN-PIN", expectation="", blocker="gap", asset=b"fabricated")
    assert any("DESIGN-PIN seam carries assets" in d for d in pinned.audit(libs2)), "fabricated DESIGN-PIN asset not flagged"


# --- [ROUNDTRIP_ORACLE]


def test_roundtrip_proves_counts_skips_and_refutes(tmp_path: Path) -> None:
    """Byte-identical assets prove and count; a lossy encoder fails; DESIGN-PIN skips by blocker; unknown fixtures refuse."""
    manifest, _ = _corpus(tmp_path, asset=b"\x03\x00wire")
    assert assert_corpus_roundtrip(manifest, "DEMO_FIX", bytes, bytes) == 1
    with pytest.raises(AssertionError, match="byte-identical"):
        assert_corpus_roundtrip(manifest, "DEMO_FIX", bytes, lambda raw: raw + b"!")

    unemitted, _ = _corpus(tmp_path / "empty")
    assert assert_corpus_roundtrip(unemitted, "DEMO_FIX", bytes, bytes) == 0, "an un-emitted REAL seam proves zero assets, never fails"

    pinned, _ = _corpus(tmp_path / "pin", pin="DESIGN-PIN", expectation="", blocker="producer gap")
    with pytest.raises(pytest.skip.Exception, match="producer gap"):
        assert_corpus_roundtrip(pinned, "DEMO_FIX", bytes, bytes)

    with pytest.raises(KeyError, match="GHOST"):
        manifest.entry("GHOST")
