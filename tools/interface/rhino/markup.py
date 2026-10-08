"""Child lookup and canonical digest of lxml element trees."""

from lxml import etree

from interface.report import digest

# --- [OPERATIONS] -----------------------------------------------------------------------


def element(owner: etree._Element, tag: str, **keys: str) -> etree._Element:
    """Owner's first child with the tag and attributes, created when the owner holds none."""
    match [each for each in owner.iterfind(tag) if all(each.get(name) == value for name, value in keys.items())]:
        case [found, *_]:
            return found
        case _:
            return etree.SubElement(owner, tag, keys)


def canonical(tree: etree._Element) -> str:
    """Short digest of the tree's canonical form, blind to comments and indentation."""
    return digest(etree.tostring(tree, method="c14n2", with_comments=False, strip_text=True))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["canonical", "element"]
