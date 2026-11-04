"""Font compilation, snapshot cache identity, cached checks, and publication."""

from datetime import timedelta, UTC
from io import BytesIO
from pathlib import Path

from expression import Result, Some
from fontTools.misc.timeTools import epoch_diff
from fontTools.ttLib import TTFont
from fontTools.ttLib.tables.ttProgram import Program
import msgspec
import pytest
import ufoLib2
from ufoLib2.objects.info import GaspBehavior, GaspRangeRecord

from typography.builds.pipeline import Action, Builds, Built, compiled, constructed, performed, published, written
from typography.faces.declaration import Face, Values
from typography.faces.faults import Ok, Reason
from typography.faces.glyphs import Source
from typography.rules.metrics import Metrics
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary
from typography.verification.requirements import Finding

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_ufo_output_preserves_native_glyphs_anchors_and_metadata(product: tuple[Source, ufoLib2.Font, TTFont], tmp_path: Path) -> None:
    """UFO readers recover glyph order, mark anchors, and font records from the exported source."""
    source, ufo, native = product
    target = written(ufo, tmp_path / "Test-Regular.ufo").default_with(lambda faults: pytest.fail(str(faults)))
    font = ufoLib2.Font.open(target)
    assert font.glyphOrder == [glyph.name.text for glyph in source.glyphs]
    assert tuple(anchor.name for anchor in font["uni0628"].anchors) == ("top",)
    assert (font.info.familyName, font.info.openTypeOS2Type, font.lib["public.openTypeCategories"]["uni064E"]) == (source.face.typographic, [2], "mark")
    assert font["uni0628"].width == source.named["uni0628"].advance
    assert font.info.openTypeGaspRangeRecords == [GaspRangeRecord(rangeMaxPPEM=0xFFFF, rangeGaspBehavior=list(GaspBehavior))]
    assert font.info.openTypeHeadCreated == source.info.created.astimezone(UTC).strftime("%Y/%m/%d %H:%M:%S")
    assembly = font.lib["public.truetype.instructions"]["controlValueProgram"]
    assert "SCANCTRL" in assembly
    assert "SCANTYPE" in assembly
    program = Program()
    program.fromAssembly(assembly)
    assert program.getBytecode() == native["prep"].program.getBytecode()


def test_compilation_preserves_shaping_and_source_time(source: Source, original: Binary, texts: Corpus) -> None:
    """Native compiled tables keep glyph placement and use creation time rather than wall-clock build time."""
    source = msgspec.structs.replace(source, info=msgspec.structs.replace(source.info, created=source.info.created - timedelta(days=1)))
    source, _, native = constructed(source, Metrics(pitch=None, stack=0), texts).default_with(lambda faults: pytest.fail(str(faults)))
    data = compiled(source, native).default_with(lambda faults: pytest.fail(str(faults)))
    binary, font = Binary(data=data), TTFont(BytesIO(data))
    assert {"prep", "gasp", "STAT", "meta", "GDEF", "GPOS", "GSUB"} <= set(font.keys())
    assert font["head"].created == font["head"].modified == round(source.info.created.timestamp()) - epoch_diff  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    assert font["gasp"].gaspRange == {0xFFFF: 0xF}
    assert font["OS/2"].fsSelection & 0x180 == 0x180  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    assert (font["OS/2"].version, font["OS/2"].fsType, font["head"].flags) == (4, 4, 0b11)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    assert font["meta"].data["dlng"] == "Arab"  # type: ignore[call-overload]  # ty: ignore[invalid-argument-type]
    assert isinstance(source.face, Face)
    assert font["OS/2"].usWidthClass == source.face.width  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    assert [axis.AxisTag for axis in font["STAT"].table.DesignAxisRecord.Axis] == ["wght"]
    for text in ("ببب", "بَ", "لا"):
        before, after = original.shaped(text), binary.shaped(text)
        assert tuple((placed.x, placed.y, placed.advance) for placed in after.placed) == tuple((placed.x, placed.y, placed.advance) for placed in before.placed)
    assert compiled(source, native) == Ok(data)


def test_current_build_uses_snapshot_inputs_and_detects_external_binary_changes(builds: Builds, original: Binary, recorded: tuple[Built, Source]) -> None:
    """A supplied original snapshot survives file races while changed declarations, snapshots, and built bytes invalidate the record."""
    done, _ = recorded
    face = builds.declaration.set.faces[0]
    assert builds.current(face, original) == Some(done)
    assert builds.current(msgspec.structs.replace(face, values=Values(counter=0.05)), original).is_none()
    assert builds.current(face, Binary(data=original.data + b"\0")).is_none()
    path = builds.declaration.original(face)
    path.write_bytes(b"replaced after snapshot")
    assert builds.original(path) == Ok(original)
    assert Builds(declaration=builds.declaration, texts=builds.texts).original(path).is_error()
    assert builds.current(face, original) == Some(done)
    done.path.write_bytes(b"changed")
    assert builds.current(face, original).is_none()


def test_run_checks_a_current_build_and_persists_findings(builds: Builds, original: Binary, recorded: tuple[Built, Source]) -> None:
    """A cached build still receives a fresh report when run requests verification."""
    done, _ = recorded
    declared = builds.declaration
    declared.corpus.write_bytes(msgspec.toml.encode(builds.texts))
    reports = performed(Action.RUN, declared.root / "set.toml").default_with(lambda faults: pytest.fail(str(faults)))
    findings = msgspec.json.decode(declared.check(done.face).read_bytes(), type=tuple[Finding, ...])
    assert reports[0].current
    assert reports[0].findings == len(findings) > 0
    assert builds.current(done.face, original).default_with(lambda: pytest.fail("Current build was lost after checking")).findings == findings


def test_publication_creates_folders_and_keeps_native_font_tables(recorded: tuple[Built, Source], tmp_path: Path) -> None:
    """New publication directories receive an exact TrueType copy and equivalent WOFF2 mappings and metrics."""
    done, _ = recorded
    destination = tmp_path / "new" / "published"
    ttf, woff2 = published(done, destination).default_with(lambda faults: pytest.fail(str(faults)))
    assert (ttf, woff2) == (destination / done.path.name, (destination / done.path.name).with_suffix(".woff2"))
    assert ttf.read_bytes() == done.path.read_bytes()
    with TTFont(ttf) as plain, TTFont(woff2) as packed:
        assert packed.flavor == "woff2"
        assert packed.getBestCmap() == plain.getBestCmap()
        assert packed["hmtx"].metrics == plain["hmtx"].metrics


def test_publication_reports_a_real_unwritable_parent(recorded: tuple[Built, Source], tmp_path: Path) -> None:
    """A file occupying the destination folder returns the native write fault."""
    done, _ = recorded
    destination = tmp_path / "occupied"
    destination.write_bytes(b"file")
    match published(done, destination):
        case Result(error=faults):
            assert tuple(fault.reason for fault in faults) == (Reason.UNWRITABLE,)
            assert isinstance(faults[0].cause, OSError)
        case outcome:
            pytest.fail(str(outcome))
