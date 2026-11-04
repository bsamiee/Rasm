"""Corpus decoding, language coverage, contextual marks, and complete text products."""

from pathlib import Path
lazy import re

from expression import Result
import msgspec
import pytest

from typography.faces.declaration import declared
from typography.faces.faults import Ok, Reason
from typography.shaping.corpus import Corpus, corpus

# --- [CONSTANTS] ------------------------------------------------------------------------

TOML = """
prose = { fa = ["بب"], ar = ["ببب"] }
marked = ["بَ"]
annotated = []
deep = []
words = ["ب ب"]
digits = []
punctuation = []
joins = ["ببب"]
seam = ["ببب"]
zwnj = []
ligatures = []
letters = [0x0628]
vowel_stacks = ["\\u064E"]
stacks = []
"""

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_toml_decoding_preserves_unicode_and_language_order(tmp_path: Path, sample: Corpus) -> None:
    """Hex code points and escaped marks decode without losing declared languages or the default system."""
    path = tmp_path / "corpus.toml"
    path.write_text(TOML, encoding="utf-8")
    assert corpus(path) == Ok(sample)
    assert sample.languages == ("fa", "ar", "")
    assert msgspec.structs.replace(sample, prose=frozendict()).languages == ("",)
    assert msgspec.structs.replace(sample, prose=frozendict({"": ("بب",), "fa": ("ب",)})).languages == ("", "fa")


def test_declared_asset_corpus_reaches_its_native_consumer(pytestconfig: pytest.Config) -> None:
    """The actual font-set row resolves a readable corpus with its declared languages and text."""
    sample = declared(pytestconfig.rootpath / "assets" / "fonts" / "rasm.toml").bind(lambda found: corpus(found.corpus)).default_with(lambda faults: pytest.fail(str(faults)))
    assert sample.lines
    assert {"fa", "ar", ""} <= set(sample.languages)


@pytest.mark.parametrize("text,reason", [(None, Reason.UNREADABLE), (f"{TOML}extra = []\n", Reason.INVALID), (TOML.replace("0x0628", "-1"), Reason.INVALID)])
def test_input_faults_distinguish_read_failure_from_refused_content(tmp_path: Path, text: str | None, reason: Reason) -> None:
    """Missing files, unknown fields, and invalid code points keep their distinct boundary reasons."""
    path = tmp_path / "corpus.toml"
    if text is not None:
        path.write_text(text, encoding="utf-8")
    match corpus(path):
        case Result(error=(fault,)):
            assert fault.reason is reason
            assert isinstance(fault.cause, OSError if text is None else msgspec.DecodeError)
        case outcome:
            pytest.fail(str(outcome))


def test_lines_deduplicate_across_kinds_without_reordering(sample: Corpus) -> None:
    """Repeated prose and seam text appears once while first-seen order remains stable."""
    assert sample.plain == ("بب", "ببب")
    assert sample.lines == ("بب", "ببب", "بَ", "ب ب")
    assert msgspec.structs.replace(sample, marked=(*sample.marked, *sample.plain), seam=sample.lines).lines == sample.lines


@pytest.mark.parametrize("line,context", [("ب ۛ ب", "ب ۛ ب"), ("ۛ ب", "ۛ ب"), ("ب ۛ", "ب ۛ"), (" \tب  ۛ\nب \t", "ب ۛ ب")])
def test_lone_mark_words_retain_available_neighbor_context(sample: Corpus, line: str, context: str) -> None:
    """Standalone combining marks stay with their neighbors at middle and edge positions."""
    runs = msgspec.structs.replace(sample, marked=(), annotated=(), words=(line,)).word_runs
    assert context in runs
    assert "ۛ" not in runs
    assert "" not in runs
    assert len(runs) == len(set(runs))


def test_forms_place_marks_inside_the_joining_context(sample: Corpus) -> None:
    """Marks follow the letter inside the four Unicode joiner contexts."""
    joiner = "\N{ZERO WIDTH JOINER}"
    assert sample.forms(0x0628, "َ") == sample.matrix == ("بَ", f"بَ{joiner}", f"{joiner}بَ{joiner}", f"{joiner}بَ")
    assert tuple(form.replace("َ", "") for form in sample.matrix) == sample.forms(0x0628)


def test_pair_products_are_complete_unique_and_well_formed(sample: Corpus) -> None:
    """Each letter and optional mark appears at both positions under the requested outer context."""
    letters, marks = (0x0628, 0x0644), ("", "َ")
    selected = msgspec.structs.replace(sample, letters=letters)
    runs = selected.pairs("ب", marks)
    assert len(runs) == len(set(runs)) == (len(letters) * len(marks)) ** 2
    assert all(re.fullmatch(r"ب(?:[بل]َ?){2}ب", run) for run in runs)
    assert (runs[0], runs[-1]) == ("بببب", "بلَلَب")
    assert msgspec.structs.replace(selected, letters=()).pairs() == ()
