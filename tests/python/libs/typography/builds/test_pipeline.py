"""Font compilation, snapshot cache identity, cached checks, and publication."""

from datetime import timedelta, UTC
from functools import partial
from inspect import getfile
from io import BytesIO
from pathlib import Path

import anyio
from expression import Result, Some
from fontTools.misc.timeTools import epoch_diff
from fontTools.ttLib import TTFont
from fontTools.ttLib.tables.ttProgram import Program
import msgspec
import pytest
import ufoLib2
from ufoLib2.objects.info import GaspBehavior, GaspRangeRecord

from typography.builds.pipeline import Action, Builds, Built, compiled, constructed, performed, published, Verdict, written
from typography.faces.declaration import Face, Values
from typography.faces.faults import Ok, Reason
from typography.faces.glyphs import Source
from typography.rules.metrics import Metrics
from typography.shaping.corpus import Corpus
from typography.shaping.expectations import expected
from typography.shaping.runs import Binary
from typography.verification.requirements import requirements

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


def test_current_build_uses_snapshot_inputs_and_detects_external_binary_changes(builds: Builds, original: Binary, recorded: Built) -> None:
    """A supplied original snapshot survives file races while changed declarations, snapshots, and built bytes invalidate the record."""
    done = recorded
    face = builds.declaration.set.faces[0]
    assert builds.current(face, original) == Some(done)
    assert builds.current(msgspec.structs.replace(face, values=Values(counter=0.05)), original).is_none()
    assert builds.current(face, Binary(data=original.data + b"\0")).is_none()
    builds.declaration.original(face).write_bytes(b"replaced after snapshot")
    assert builds.current(face, original) == Some(done)
    done.path.write_bytes(b"changed")
    assert builds.current(face, original).is_none()


def test_run_checks_a_current_build_and_answers_unmet_with_its_report(builds: Builds, original: Binary, recorded: Built) -> None:
    """A cached build still receives a fresh report when run requests verification, findings answering in one fault naming that report."""
    done = recorded
    declared = builds.declaration
    declared.corpus.write_bytes(msgspec.toml.encode(builds.texts))
    match anyio.run(partial(performed, Action.RUN, declared.root / "set.toml", output=declared.output)):
        case Result(error=(fault,)):
            assert (fault.reason, fault.subject, fault.cause) == (Reason.UNMET, done.face.stem, declared.check(done.face))
        case outcome:
            pytest.fail(str(outcome))
    assert msgspec.json.decode(declared.check(done.face).read_bytes(), type=Verdict).findings
    assert builds.current(done.face, original) == Some(done)
    assert msgspec.json.decode(declared.stamp(done.face).read_bytes(), type=tuple[str, str]) == (done.inputs, done.hash)


def test_input_identity_follows_build_modules_and_ignores_check_only_modules(builds: Builds, original: Binary, monkeypatch: pytest.MonkeyPatch) -> None:
    """A check-only module edit keeps every build current while a build module edit invalidates it."""
    face, native = builds.declaration.set.faces[0], Path.read_bytes

    def identity(edited: str, /) -> str:
        with monkeypatch.context() as patch:
            patch.setattr(Path, "read_bytes", lambda path: native(path) + b"\n" * (path == Path(edited)))
            return Builds.opened(builds.declaration, builds.texts, b"").inputs(original, face)

    base = identity("")
    assert {identity(getfile(requirements)), identity(getfile(expected))} == {base}
    assert identity(getfile(compiled)) != base


def test_approval_writes_a_formatted_file_for_a_face_requiring_shaping(builds: Builds, recorded: Built) -> None:
    """A text face's approval lands in the set's shaping folder ending as the format pass leaves it."""
    done = recorded
    declared = builds.declaration
    declared.corpus.write_bytes(msgspec.toml.encode(builds.texts))
    assert anyio.run(partial(performed, Action.APPROVE, declared.root / "set.toml", output=declared.output)).is_ok()
    assert declared.approved(done.face).read_bytes().endswith(b"}\n")


def test_approval_refuses_a_face_no_shaping_requirement_reads(builds: Builds) -> None:
    """An ornament face holds no shaping requirement, so it writes no approval and names what it lacks."""
    declared = builds.declaration
    declared.corpus.write_bytes(msgspec.toml.encode(builds.texts))
    path = declared.root / "ornaments.toml"
    path.write_text(
        'name="Set"\ncorpus="corpus.toml"\nfamilies=[]\n[[ornaments]]\nname="Marks"\noriginal="original"\n[[ornaments.faces]]\nstyle="Regular"\noriginal="Test-Regular.ttf"\n', encoding="utf-8"
    )
    match anyio.run(partial(performed, Action.APPROVE, path, output=declared.output)):
        case Result(error=(fault,)):
            assert (fault.reason, fault.member) == (Reason.ABSENT, "shaping requirement")
        case outcome:
            pytest.fail(str(outcome))
    assert not declared.approvals.exists()


def test_publication_creates_folders_and_keeps_native_font_tables(recorded: Built, tmp_path: Path) -> None:
    """New publication directories receive an exact TrueType copy and equivalent WOFF2 mappings and metrics."""
    done = recorded
    destination = tmp_path / "new" / "published"
    ttf, woff2 = published(done, destination).default_with(lambda faults: pytest.fail(str(faults)))
    assert (ttf, woff2) == (destination / done.path.name, (destination / done.path.name).with_suffix(".woff2"))
    assert ttf.read_bytes() == done.path.read_bytes()
    with TTFont(ttf) as plain, TTFont(woff2) as packed:
        assert packed.flavor == "woff2"
        assert packed.getBestCmap() == plain.getBestCmap()
        assert packed["hmtx"].metrics == plain["hmtx"].metrics


def test_publication_reports_a_real_unwritable_parent(recorded: Built, tmp_path: Path) -> None:
    """A file occupying the destination folder returns the native write fault."""
    done = recorded
    destination = tmp_path / "occupied"
    destination.write_bytes(b"file")
    match published(done, destination):
        case Result(error=faults):
            assert tuple(fault.reason for fault in faults) == (Reason.UNWRITABLE,)
            assert isinstance(faults[0].cause, OSError)
        case outcome:
            pytest.fail(str(outcome))
