# ruff: file-ignore[suspicious-xml-etree-import]
"""Child lookup and canonical digest of XML trees over the element API lxml and the standard library's ElementTree both serve."""

from collections.abc import Iterator
from typing import Protocol, Self
import xml.etree.ElementTree as ET

from interface.report import digest

# --- [TYPES] ----------------------------------------------------------------------------


class Element(Protocol):
    """Element members lxml and the standard library's ElementTree both define."""

    def iterfind(self, path: str, /) -> Iterator[Self]:
        """Children the path selects in document order."""

    def get(self, key: str, /) -> str | None:
        """Attribute value, None when the element holds none."""

    def makeelement(self, tag: str, attrib: dict[str, str], /) -> Self:
        """New element of the tree's kind."""

    def append(self, child: Self, /) -> None:
        """Add the child as the last one."""


# --- [OPERATIONS] -----------------------------------------------------------------------


def element[T: Element](owner: T, tag: str, **keys: str) -> T:
    """Owner's first child with the tag and attributes, created when the owner holds none."""
    match [each for each in owner.iterfind(tag) if all(each.get(name) == value for name, value in keys.items())]:
        case [found, *_]:
            return found
        case _:
            made = owner.makeelement(tag, keys)
            owner.append(made)
            return made


def canonical(text: str) -> str:
    """Short digest of an XML text's canonical form, blind to comments and indentation."""
    return digest(ET.canonicalize(text, strip_text=True, with_comments=False).encode())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Element", "canonical", "element"]
