"""A shared native font, its imported source, and the corpus used by geometric checks."""

from pathlib import Path

import pytest

from typography.faces.declaration import Face
from typography.faces.glyphs import Source
from typography.faces.importing import imported
from typography.shaping.corpus import Corpus
from typography.shaping.runs import Binary

from .font import built

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture(scope="session")
def binary() -> Binary:
    """Returns the unperturbed native font."""
    return built()


@pytest.fixture(scope="session")
def path(tmp_path_factory: pytest.TempPathFactory, binary: Binary) -> Path:
    """Writes the unperturbed font for external process checks."""
    written = tmp_path_factory.mktemp("fonts") / "Verification-Regular.ttf"
    written.write_bytes(binary.data)
    return written


@pytest.fixture(scope="session")
def source(path: Path, binary: Binary) -> Source:
    """Imports the same byte snapshot measured by verification."""
    return imported(binary.data, Face(style="Regular", original=path.name, family="Verification")).default_with(lambda faults: pytest.fail(str(faults)))


@pytest.fixture(scope="session")
def sample() -> Corpus:
    """Returns lines crossing joining letters, ligatures, marks, and digits."""
    return Corpus(
        prose=frozendict(fa=("ببب لا",), ar=("بَب",)),
        marked=("بَبُب",),
        annotated=(),
        deep=(),
        words=("بَ بُ",),
        digits=("۱۲",),
        punctuation=(),
        joins=(),
        seam=("ببب",),
        zwnj=(),
        ligatures=("لا",),
        letters=(0x0627, 0x0628),
        vowel_stacks=("َ", "ُ"),
        stacks=(),
    )
