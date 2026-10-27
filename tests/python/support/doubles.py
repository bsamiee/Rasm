"""Mock installs logging every call in order, fixture file writer, and the refurb findings fixture type."""

from typing import Protocol
lazy from collections.abc import Callable, Mapping
lazy from pathlib import Path
lazy from unittest.mock import Mock

import msgspec.json
lazy import pytest
lazy from refurb.error import Error

# --- [TYPES] ----------------------------------------------------------------------------

type Findings = Callable[[type[Error], str], list[tuple[int, str]]]


class _Member(Protocol):
    __name__: str


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [RECORDING]


def install(monkeypatch: pytest.MonkeyPatch, target: object, stubs: Mapping[_Member, Mock]) -> Mock:
    """Replace each member of ``target`` with its mock and return the parent whose ``mock_calls`` log every call in order."""
    log = Mock()
    for member, stub in stubs.items():
        log.attach_mock(stub, member.__name__)
        monkeypatch.setattr(target, member.__name__, stub)
    return log


# --- [FIXTURES]


def write_fixtures(directory: Path, files: Mapping[str, object], encode: Callable[[object], bytes] = msgspec.json.encode) -> frozendict[str, Path]:
    """Write each file under ``directory``, non-bytes content through ``encode``, and return paths by name."""
    paths = frozendict({name: directory / name for name in files})
    for name, path in paths.items():
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(content if isinstance(content := files[name], bytes) else encode(content))
    return paths


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Findings", "install", "write_fixtures"]
