"""Wheels a pack vendors: core metadata read from a wheel and a wheel's packages placed in a stage importing each other relative to its root."""

import ast
from pathlib import Path, PurePosixPath
import zipfile

from asttokens import ASTText
from asttokens.util import replace
from packaging.metadata import Metadata

# --- [OPERATIONS] -----------------------------------------------------------------------


def cored(wheel: Path) -> Metadata:
    """Core metadata of the wheel."""
    with zipfile.ZipFile(wheel) as archive:
        (member,) = (name for name in archive.namelist() if PurePosixPath(name).match("*.dist-info/METADATA"))
        return Metadata.from_email(archive.read(member), validate=False)


def copied(wheel: Path, stage: Path) -> None:
    """Place the wheel's packages in the stage with every staged module importing them relative to the stage root."""
    with zipfile.ZipFile(wheel) as archive:
        members = [name for name in archive.namelist() if not PurePosixPath(name).parts[0].endswith(".dist-info")]
        archive.extractall(stage, members)
    tops = frozenset(PurePosixPath(name).parts[0] for name in members)
    for path in stage.rglob("*.py"):
        source = path.read_text(encoding="utf-8", newline="")
        parsed, level = ASTText(source), len(path.relative_to(stage).parent.parts) + 1
        imports = (node for node in ast.walk(parsed.tree) if isinstance(node, ast.ImportFrom) and node.level == 0 and node.module is not None and node.module.partition(".")[0] in tops)
        path.write_text(replace(source, [(*parsed.get_text_range(node, padded=False), ast.unparse(ast.ImportFrom(node.module, node.names, level))) for node in imports]), encoding="utf-8", newline="")


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["copied", "cored"]
