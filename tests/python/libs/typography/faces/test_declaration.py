"""Set declaration decode, face filling, paths, refusals, and coverage gaps."""

lazy from pathlib import Path

from expression import Result
import pytest

from typography.faces.coverage import Coverage
from typography.faces.declaration import Declared, declared, Diwani, DotMatrix, Face, Kufi, OrnamentFace, Sheared, TextNaskh
from typography.faces.faults import Outcome, Reason

# --- [CONSTANTS] ------------------------------------------------------------------------

DECLARATION = """
name = "Test"
corpus = "corpus.toml"
output = "build"

[[families]]
name = "Satr"
original = "Naskh/original"
genre = { kind = "textnaskh", word_ratio = 3.0 }

[[families.faces]]
style = "Regular"
original = "Naskh-Regular.ttf"
values = { nuqta_floor = 0.05, counter = 0.06 }

[[families.faces]]
style = "ExtraBold Italic"
weight = 800
suffix = "Outline"
original = "Naskh-Regular.ttf"
treatments = [{ kind = "sheared", angle = 20.0 }]

[[ornaments]]
name = "Tazhib"
original = "Ornaments/Zakharif/original"

[[ornaments.faces]]
style = "Regular"
original = "Zakharif-Symbols.ttf"
first = 0xE4CF
"""

# --- [OPERATIONS] -----------------------------------------------------------------------


def _declared(tmp_path: Path, text: str, /) -> Outcome[Declared]:
    (path := tmp_path / "set.toml").write_text(text, encoding="utf-8")
    return declared(path)


def test_declaration_fills_faces_and_paths(tmp_path: Path) -> None:
    """Faces take family name and genre, and paths resolve against the declaration folder by face stem."""
    match _declared(tmp_path, DECLARATION):
        case Result(tag="ok", ok=found):
            regular, italic, ornament = found.set.faces
            assert isinstance(regular, Face)
            assert isinstance(italic, Face)
            assert isinstance(ornament, OrnamentFace)
            assert (regular.genre, regular.values.counter, italic.treatments, italic.typographic, ornament.first) == (TextNaskh(word_ratio=3.0), 0.06, (Sheared(angle=20.0),), "Satr Outline", 0xE4CF)
            assert (found.original(regular), found.binary(italic), found.source(ornament), found.corpus) == (
                tmp_path / "Naskh/original/Naskh-Regular.ttf",
                tmp_path / "build/Satr/SatrOutline-ExtraBoldItalic.ttf",
                tmp_path / "build/source/Tazhib-Regular.ufo",
                tmp_path / "corpus.toml",
            )
        case outcome:
            raise AssertionError(outcome)


@pytest.mark.parametrize(
    "old, new",
    [
        ('style = "Regular"\noriginal = "Zakharif', 'style = "regular"\noriginal = "Zakharif'),
        ('name = "Tazhib"', 'name = "Satr"'),
        ("weight = 800", "weight = 1200"),
        ('kind = "sheared"', 'kind = "slanted"'),
        ("nuqta_floor", "nuqta_flor"),
    ],
)
def test_declaration_refuses_invalid_sets(tmp_path: Path, old: str, new: str) -> None:
    """Bad style, repeated family name, weight past its bound, unknown treatment, and unknown value key each fail with one `INVALID` fault."""
    match _declared(tmp_path, DECLARATION.replace(old, new)):
        case Result(error=(fault,)):
            assert fault.reason is Reason.INVALID
        case outcome:
            raise AssertionError(outcome)


def test_missing_declaration_is_unreadable(tmp_path: Path) -> None:
    """Absent file fails with one `UNREADABLE` fault."""
    match declared(tmp_path / "absent.toml"):
        case Result(error=(fault,)):
            assert fault.reason is Reason.UNREADABLE
        case outcome:
            raise AssertionError(outcome)


def test_genres_state_class_defaults() -> None:
    """Kufi and dot matrix keep their drawn size and narrower mark band, Diwani its close word ratio, text Naskh holds the mark floor and cuts spurs."""
    assert (DotMatrix().alif, DotMatrix().mark, Diwani().word_ratio, TextNaskh().mark_floor, Kufi().mark_floor, TextNaskh().spurs, Kufi().spurs) == (None, (0.30, 0.42), 1.88, 0.05, None, True, False)


def test_coverage_gaps() -> None:
    """Exclusion reaches presentation forms of excluded letters, and unmapped required and unrecorded mapped code points are listed."""
    coverage = Coverage()
    encoded = frozendict({**dict.fromkeys(coverage.required - {0x0628, 0x06CC}, "glyph"), 0x0041: "A", 0x0394: "uni0394", 0xFB67: "uniFB67"})
    assert (coverage.exclusion(0xFB67).filter(lambda excluded: 0x0679 in excluded.codes).is_some(), coverage.uncovered(encoded), coverage.unrecorded(encoded)) == (True, (0x0628, 0x06CC), (0x0394,))
