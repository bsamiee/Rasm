"""Declarations preserve family overrides, relative paths, and input fault provenance."""

from pathlib import Path

from expression import Result
import msgspec
import pytest
from ufoLib2.objects.info import WidthClass

from typography.faces.coverage import Coverage
from typography.faces.declaration import Declared, declared, Face, OrnamentFace, Sheared, TextNaskh
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


def _declared(folder: Path, text: str, /) -> Outcome[Declared]:
    path = folder / "set.toml"
    path.write_text(text, encoding="utf-8")
    return declared(path)


def test_family_overrides_and_relative_paths_survive_relocation(tmp_path: Path) -> None:
    """Family values fill every face while face-specific values and all relative output paths keep their meaning."""
    found = _declared(tmp_path, DECLARATION).default_with(lambda faults: pytest.fail(str(faults)))
    regular, italic, ornament = found.set.faces
    assert isinstance(regular, Face)
    assert isinstance(italic, Face)
    assert isinstance(ornament, OrnamentFace)
    assert regular.genre == italic.genre == TextNaskh(word_ratio=3.0)
    assert (regular.values.counter, italic.treatments, italic.typographic, ornament.first) == (0.06, (Sheared(angle=20),), "Satr Outline", 0xE4CF)
    assert (found.original(regular), found.binary(italic), found.source(ornament), found.corpus) == (
        tmp_path / "Naskh/original/Naskh-Regular.ttf",
        tmp_path / "build/Satr/SatrOutline-ExtraBoldItalic.ttf",
        tmp_path / "build/source/Tazhib-Regular.ufo",
        tmp_path / "corpus.toml",
    )
    relocated = tmp_path / "relocated"
    relocated.mkdir()
    moved = _declared(relocated, DECLARATION).default_with(lambda faults: pytest.fail(str(faults)))
    assert moved.set == found.set
    assert moved.binary(italic).relative_to(relocated) == found.binary(italic).relative_to(tmp_path)
    assert moved.original(regular).relative_to(relocated) == found.original(regular).relative_to(tmp_path)


@pytest.mark.parametrize(
    "old,new",
    [
        ('style = "Regular"\noriginal = "Zakharif', 'style = "regular"\noriginal = "Zakharif'),
        ('name = "Tazhib"', 'name = "Satr"'),
        ("weight = 800", "weight = 1200"),
        ('kind = "sheared"', 'kind = "slanted"'),
        ("nuqta_floor", "nuqta_flor"),
        ("weight = 800", "weight = 0"),
        ("weight = 800", "weight = 800\nwidth = 0"),
        ("weight = 800", "weight = 800\nwidth = 10"),
    ],
)
def test_refused_wire_values_keep_the_native_decode_cause(tmp_path: Path, old: str, new: str) -> None:
    """Style grammar, duplicate names, numeric bounds, treatment tags, and unknown keys fail at decoding."""
    match _declared(tmp_path, DECLARATION.replace(old, new)):
        case Result(error=(fault,)):
            assert fault.reason is Reason.INVALID
            assert isinstance(fault.cause, msgspec.DecodeError)
        case outcome:
            pytest.fail(str(outcome))


@pytest.mark.parametrize("width,weight", [(1, 1), (9, 1000)])
def test_native_metric_classes_accept_the_external_endpoints(tmp_path: Path, width: int, weight: int) -> None:
    """Numeric declarations retain the native OpenType width and weight endpoints."""
    found = _declared(tmp_path, DECLARATION.replace("weight = 800", f"weight = {weight}\nwidth = {width}")).default_with(lambda faults: pytest.fail(str(faults)))
    _, face, _ = found.set.faces
    assert isinstance(face, Face)
    assert face.width is WidthClass(width)
    assert face.weight == weight


def test_missing_declarations_keep_the_native_file_cause(tmp_path: Path) -> None:
    """Absent files preserve FileNotFoundError beneath the readable boundary reason."""
    path = tmp_path / "absent.toml"
    match declared(path):
        case Result(error=(fault,)):
            assert (fault.reason, fault.subject) == (Reason.UNREADABLE, str(path))
            assert isinstance(fault.cause, FileNotFoundError)
        case outcome:
            pytest.fail(str(outcome))


def test_coverage_classifies_decompositions_kept_blocks_and_real_gaps() -> None:
    """Presentation forms inherit exclusions while retained Latin and unrecorded Greek stay distinct."""
    coverage = Coverage()
    encoded = frozendict({**dict.fromkeys(coverage.required - {0x0628, 0x06CC}, "glyph"), 0x41: "A", 0x0394: "uni0394", 0xFB67: "uniFB67"})
    assert coverage.exclusion(0xFB67) == coverage.exclusion(0x0679)
    assert coverage.exclusion(0x0679).is_some()
    assert coverage.uncovered(encoded) == (0x0628, 0x06CC)
    assert coverage.unrecorded(encoded) == (0x0394,)
