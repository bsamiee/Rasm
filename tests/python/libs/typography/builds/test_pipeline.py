"""UFO write, compile tables and reproducibility, current builds, checks, and publication."""

from io import BytesIO
from pathlib import Path

from expression import Result
from fontTools.ttLib import TTFont
import msgspec
import ufoLib2

from typography.builds.pipeline import Built, built, checked, compiled, current, published, written
from typography.faces.declaration import Declared, Values
from typography.faces.faults import Outcome
from typography.faces.glyphs import Source
from typography.shaping.corpus import Corpus

# --- [OPERATIONS] -----------------------------------------------------------------------


def _ok[T](outcome: Outcome[T], /) -> T:
    match outcome:
        case Result(tag="ok", ok=value):
            return value
        case Result(error=faults):
            raise AssertionError(faults)


def _recorded(declared: Declared, texts: Corpus, /) -> Built:
    done, _ = _ok(built(declared, declared.set.faces[0], texts))
    return done


def test_written_ufo_reads_back(source: Source, tmp_path: Path) -> None:
    """UFO keeps glyph order, categories, anchors, records, gasp, creation date, and the dropout program."""
    font = ufoLib2.Font.open(_ok(written(source, tmp_path / "Test-Regular.ufo")))
    assert (font.glyphOrder, font.lib["public.openTypeCategories"]["uni064E"], [anchor.name for anchor in font["uni0628"].anchors], font.info.familyName, font.info.openTypeOS2Type) == (
        [glyph.name.text for glyph in source.glyphs],
        "mark",
        ["top"],
        "Test",
        [2],
    )
    assert (font.info.openTypeGaspRangeRecords is not None, font.info.openTypeHeadCreated is not None, "controlValueProgram" in font.lib["public.truetype.instructions"]) == (True, True, True)


def test_compiled_tables_and_records(source: Source) -> None:
    """Binary carries prep, gasp, STAT, meta, and OS/2 v4 with typo metrics and WWS, and compiles to equal bytes twice."""
    data = _ok(compiled(source))
    font = TTFont(BytesIO(data))
    os2, gasp, head, meta = (vars(font[tag]) for tag in ("OS/2", "gasp", "head", "meta"))
    assert {"prep", "gasp", "STAT", "meta", "GDEF", "GPOS", "GSUB"} <= set(font.keys())
    assert (os2["version"], os2["fsType"], os2["fsSelection"] & 0x180, gasp["gaspRange"], head["flags"], meta["data"]["dlng"]) == (4, 4, 0x180, {0xFFFF: 0x000F}, 0b11, "Arab")
    assert (head["created"] == head["modified"], [axis.AxisTag for axis in font["STAT"].table.DesignAxisRecord.Axis]) == (True, ["wght"])
    assert _ok(compiled(source)) == data


def test_current_follows_inputs_and_bytes(declared: Declared, texts: Corpus) -> None:
    """Recorded build is current until a declared value or its binary changes."""
    done, face = _recorded(declared, texts), declared.set.faces[0]
    kept, moved = current(declared, face, texts), current(declared, msgspec.structs.replace(face, values=Values(counter=0.05)), texts)
    done.path.write_bytes(b"changed")
    assert (kept.is_some() and kept.value == done, moved.is_none(), current(declared, face, texts).is_none()) == (True, True, True)


def test_checked_records_findings(declared: Declared, source: Source, texts: Corpus) -> None:
    """Check writes its findings as the face's report and into the build record, the file's hash held to the record's."""
    done = _recorded(declared, texts)
    verdict = _ok(checked(declared, done, source, texts))
    assert (declared.check(done.face).is_file(), any(finding.requirement == "reproducible" for finding in verdict.findings)) == (True, False)


def test_published_copies_ttf_and_woff2(declared: Declared, texts: Corpus, tmp_path: Path) -> None:
    """Publication writes the TrueType copy and a WOFF2 of it into the destination."""
    done = _recorded(declared, texts)
    (destination := tmp_path / "published").mkdir()
    ttf, woff2 = _ok(published(done, destination))
    assert (ttf.read_bytes() == done.path.read_bytes(), TTFont(woff2).flavor, woff2.name) == (True, "woff2", "Test-Regular.woff2")
