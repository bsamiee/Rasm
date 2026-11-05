"""Shaping corpus: the texts a font set is shaped over and the letter form, mark, word, and pair runs they derive."""

from typing import Annotated
lazy from itertools import product
lazy from unicodedata import category

import msgspec

# --- [CONSTANTS] ------------------------------------------------------------------------

_JOINS = (("", ""), ("", "\N{ZERO WIDTH JOINER}"), ("\N{ZERO WIDTH JOINER}", "\N{ZERO WIDTH JOINER}"), ("\N{ZERO WIDTH JOINER}", ""))

# --- [MODELS] ---------------------------------------------------------------------------


class Corpus(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """Unmarked prose by BCP 47 language, texts by the check that shapes them, and the letters and mark stacks the mark matrix crosses."""

    prose: frozendict[str, tuple[str, ...]]
    marked: tuple[str, ...]
    annotated: tuple[str, ...]
    deep: tuple[str, ...]
    words: tuple[str, ...]
    digits: tuple[str, ...]
    punctuation: tuple[str, ...]
    joins: tuple[str, ...]
    seam: tuple[str, ...]
    zwnj: tuple[str, ...]
    ligatures: tuple[str, ...]
    letters: tuple[Annotated[int, msgspec.Meta(ge=0, le=0x10FFFF)], ...]
    vowel_stacks: tuple[str, ...]
    stacks: tuple[str, ...]

    @property
    def languages(self) -> tuple[str, ...]:
        """Language of each prose kind and the empty language of the font's default system, the languages every run shapes under."""
        return tuple(dict.fromkeys((*self.prose, "")))

    @property
    def plain(self) -> tuple[str, ...]:
        """Unmarked prose lines of every language."""
        return tuple(*lines for lines in self.prose.values())

    @property
    def lines(self) -> tuple[str, ...]:
        """Every text line of every kind, each once in first-seen order."""
        return tuple(dict.fromkeys((*self.plain, *self.marked, *self.annotated, *self.deep, *self.words, *self.digits, *self.punctuation, *self.joins, *self.seam, *self.zwnj, *self.ligatures)))

    @property
    def matrix(self) -> tuple[str, ...]:
        """Every form of every letter under every vowel stack and mark stack, the runs the mark matrix shapes."""
        return _runs(self.letters, 1, (*self.vowel_stacks, *self.stacks), _JOINS)

    @property
    def word_runs(self) -> tuple[str, ...]:
        """Each word of the marked, annotated, and word lines once, a word of marks alone shaped between its neighbors."""
        return tuple(
            dict.fromkeys(
                " ".join(words[max(index - 1, 0) : index + 2]) if all(category(character) == "Mn" for character in word) else word
                for words in (line.split() for line in (*self.marked, *self.annotated, *self.words))
                for index, word in enumerate(words)
            )
        )

    @staticmethod
    def forms(code: int, marks: str = "", /) -> tuple[str, ...]:
        """Isolated, initial, medial, and final forms of a letter with marks after it, each joining side held open by U+200D.

        Args:
            code: Code point of the letter.
            marks: Mark sequence typed after the letter, empty for unmarked forms.

        Returns:
            The four forms in isolated, initial, medial, final order.
        """
        return _runs((code,), 1, (marks,), _JOINS)

    def pairs(self, around: str = "", marks: tuple[str, ...] = ("",), /) -> tuple[str, ...]:
        """Every ordered pair of corpus letters with each mark sequence on each letter, between two copies of `around`.

        Args:
            around: Letter the pair sits between, empty for an unwrapped pair.
            marks: Mark sequences typed after each letter of the pair, `("",)` for unmarked pairs.

        Returns:
            One run per letter and mark pair, the first letter and its marks outermost.
        """
        return _runs(self.letters, 2, marks, ((around, around),))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _runs(letters: tuple[int, ...], count: int, marks: tuple[str, ...], contexts: tuple[tuple[str, str], ...], /) -> tuple[str, ...]:
    """Every run of `count` letters, each letter followed by each mark sequence, between each context pair, the first letter outermost."""
    clusters = tuple(f"{chr(code)}{mark}" for code in letters for mark in marks)
    return tuple(f"{before}{''.join(chosen)}{after}" for chosen in product(clusters, repeat=count) for before, after in contexts)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Corpus"]
