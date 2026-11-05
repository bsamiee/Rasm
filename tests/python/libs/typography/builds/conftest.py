"""Arabic test original with beh forms, alef, a lam-alef ligature and a fatha on mark anchors, its declaration, corpus, and imported source."""

from pathlib import Path

from fontTools.pens.ttGlyphPen import TTGlyphPen
from fontTools.ttLib import TTFont
from fontTools.ttLib.tables._g_l_y_f import Glyph
import pytest
import ufoLib2

from tests.python.libs.typography.fonts import encoded
from typography.builds.pipeline import Builds, Built, closure, constructed
from typography.faces.declaration import Declared, declared as read_declared
from typography.faces.glyphs import Source
from typography.faces.importing import imported
from typography.rules.metrics import Metrics
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def declared(tmp_path: Path) -> Declared:
    """Declaration of one Kufi face over a FontBuilder original in a temporary folder."""
    drawings = {
        ".notdef": (500, 450),
        "space": (250, 0),
        **dict.fromkeys(("beh", "beh.i", "beh.m", "beh.f"), (500, 450)),
        **dict.fromkeys(("lam", "lam.i"), (300, 250)),
        **dict.fromkeys(("alef", "alef.f"), (250, 200)),
        "lamalef": (550, 500),
        "fatha": (0, 200),
    }

    def drawn(width: int, /) -> Glyph:
        pen = TTGlyphPen(None)
        if width:
            pen.moveTo((0, 0))
            pen.lineTo((0, 100))
            pen.qCurveTo((width / 2, 160), (width, 100))
            pen.lineTo((width, 0))
            pen.closePath()
        return pen.glyph()

    features = """
languagesystem arab dflt;
markClass fatha <anchor 100 80> @ABOVE;
feature init { sub beh by beh.i; sub lam by lam.i; } init;
feature medi { sub beh by beh.m; } medi;
feature fina { sub beh by beh.f; sub alef by alef.f; } fina;
feature rlig { sub lam.i alef.f by lamalef; } rlig;
feature mark { pos base [beh beh.i beh.m beh.f lam alef] <anchor 250 600> mark @ABOVE; } mark;
table GDEF { GlyphClassDef [beh beh.i beh.m beh.f lam lam.i alef alef.f], [lamalef], [fatha], ; } GDEF;
"""
    (tmp_path / "original").mkdir()
    (tmp_path / "original" / "Test-Regular.ttf").write_bytes(
        encoded(
            {name: (advance, drawn(width)) for name, (advance, width) in drawings.items()},
            {0x0020: "space", 0x0628: "beh", 0x0644: "lam", 0x0627: "alef", 0x064E: "fatha"},
            features,
            ascent=800,
            descent=-200,
        )
    )
    declaration = tmp_path / "set.toml"
    declaration.write_text(
        'name="Set"\ncorpus="corpus.toml"\n[[families]]\nname="Test"\noriginal="original"\ngenre={kind="kufi"}\n[[families.faces]]\nstyle="Regular"\noriginal="Test-Regular.ttf"\nwidth=3\n',
        encoding="utf-8",
    )
    return read_declared(declaration, tmp_path / "build").default_with(lambda faults: pytest.fail(str(faults)))


@pytest.fixture
def texts() -> Corpus:
    """Corpus over beh, lam, and alef."""
    lines = ("بلا", "ببب")
    return Corpus(
        prose=frozendict(fa=lines, ar=lines),
        marked=("بَب",),
        annotated=(),
        deep=(),
        words=lines,
        digits=(),
        punctuation=(),
        joins=(),
        seam=lines,
        zwnj=(),
        ligatures=("لا",),
        letters=(0x0628, 0x0644, 0x0627),
        vowel_stacks=("َ",),
        stacks=(),
    )


@pytest.fixture
def builds(declared: Declared, texts: Corpus) -> Builds:
    """Owns the input identities and original snapshots for one invocation."""
    return Builds.opened(declared, texts, closure().default_with(lambda faults: pytest.fail(str(faults))))


@pytest.fixture
def original(builds: Builds) -> Binary:
    """Reads one original snapshot shared by each build operation."""
    declared = builds.declaration
    return Binary.read(declared.original(declared.set.faces[0])).default_with(lambda faults: pytest.fail(str(faults)))


@pytest.fixture
def source(declared: Declared, original: Binary) -> Source:
    """Source the declared face imports from its original."""
    face = declared.set.faces[0]
    return imported(original.data, face).default_with(lambda faults: pytest.fail(str(faults)))


@pytest.fixture
def product(source: Source, texts: Corpus) -> tuple[Source, ufoLib2.Font, TTFont]:
    """Constructs native layout once while preserving the imported line pitch."""
    return constructed(source, Metrics(pitch=None, stack=0), texts).default_with(lambda faults: pytest.fail(str(faults)))


@pytest.fixture
def recorded(builds: Builds, original: Binary) -> Built:
    """Builds one face from the same immutable original snapshot its checks consume."""
    return builds.built(builds.declaration.set.faces[0], original).default_with(lambda faults: pytest.fail(str(faults)))
