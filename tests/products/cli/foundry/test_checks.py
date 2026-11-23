"""Fontspector reports over built faces, read through the private `inspected` as a build reaches it only after compiling a declared set from its originals."""

from pathlib import Path

from expression import Error, Ok
from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen
import pytest

from rasm.foundry._checks import inspected
from rasm.records import Reason

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def program(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> Path:
    """Returns the path of the only `fontspector` on `PATH`, a shell script the case writes."""
    written = tmp_path / "bin" / "fontspector"
    written.parent.mkdir()
    monkeypatch.setenv("PATH", str(written.parent))
    return written


# --- [OPERATIONS] -----------------------------------------------------------------------


def test_each_face_holds_its_file_checks_then_its_folder_checks(tmp_path: Path, program: Path) -> None:
    """A report keyed by each face, by their folder in two sections, and by an unrelated path gives each face its own checks followed by its folder's, and nothing of the unrelated path."""
    faces = (tmp_path / "fonts" / "Probe-Regular.ttf", tmp_path / "fonts" / "Probe-Bold.ttf")
    program.write_text(
        "#!/bin/sh\n"
        'printf \'{"results":{"%s":{"Universal":[{"check_id":"regular"}]},"%s":{"Universal":[{"check_id":"bold"}]},'
        '"%s":{"Universal":[{"check_id":"family"}],"OpenType":[{"check_id":"names"}]},"/elsewhere":{"Universal":[{"check_id":"stranger"}]}}}\' '
        '"$5" "$6" "${5%/*}"\n',
        encoding="utf-8",
    )
    program.chmod(0o755)
    routed = inspected(faces).map(lambda checks: {face: tuple(check.check_id for check in held) for face, held in checks.items()})
    assert routed == Ok({faces[0]: ("regular", "family", "names"), faces[1]: ("bold", "family", "names")})


def test_a_truncated_report_is_refused_as_invalid(tmp_path: Path, program: Path) -> None:
    """A fontspector printing a report cut off mid-record refuses the run as `INVALID` of `fontspector`."""
    program.write_text('#!/bin/sh\nprintf \'{"results": {"/x": {"Universal": [{"check_id": "a", "subres\'\n', encoding="utf-8")
    program.chmod(0o755)
    refused = inspected((tmp_path / "Probe-Regular.ttf",)).map_error(lambda faults: tuple((fault.reason, fault.subject) for fault in faults))
    assert refused == Error(((Reason.INVALID, "fontspector"),))


def test_the_installed_fontspector_report_decodes_for_every_face(tmp_path: Path) -> None:
    """The installed fontspector's report over a Regular and a Bold of one family decodes, each face holding checks of its own file and the `opentype/family/` checks of the folder they share."""
    faces = (tmp_path / "Probe-Regular.ttf", tmp_path / "Probe-Bold.ttf")
    for face in faces:
        pen = TTGlyphPen(None)
        pen.moveTo((100, 0))
        pen.lineTo((300, 700))
        pen.lineTo((500, 0))
        pen.closePath()
        builder = FontBuilder(1000, isTTF=True)
        builder.setupGlyphOrder([".notdef", "A"])
        builder.setupCharacterMap({0x41: "A"})
        builder.setupGlyf({".notdef": TTGlyphPen(None).glyph(), "A": pen.glyph()})
        builder.setupHorizontalMetrics({".notdef": (500, 0), "A": (600, 100)})
        builder.setupHorizontalHeader(ascent=800, descent=-200)
        builder.setupNameTable({"familyName": "Probe", "styleName": face.stem.removeprefix("Probe-")})
        builder.setupOS2()
        builder.setupPost()
        builder.save(face)
    family = inspected(faces).map(lambda checks: {face: frozenset(check.check_id.startswith("opentype/family/") for check in held) for face, held in checks.items()})
    assert family == Ok(dict.fromkeys(faces, frozenset({True, False})))
