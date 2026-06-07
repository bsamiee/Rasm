"""Root pytest policy laws for marker hooks and collection guards."""

from __future__ import annotations

from typing import TYPE_CHECKING

from hypothesis import given, strategies as st

from tests.conftest import pytest_collection_modifyitems
from tools.assay.composition.catalog import BENCHMARK_STORAGE_URI


if TYPE_CHECKING:
    from _pytest.mark.structures import Mark
    import pytest


class _HookItem:
    def __init__(self, *, fixturenames: tuple[str, ...] = (), function: object = None) -> None:
        self.fixturenames = fixturenames
        self.function = function
        self.markers: list[Mark] = []

    def add_marker(self, marker: Mark, *, append: bool = False) -> None:
        _ = append
        self.markers.append(marker)


@given(st.integers())
def _hypothesis_probe(_value: int) -> None:
    """Module-level Hypothesis function for collection-hook law."""


def test_collection_hook_marks_socket_enabled_requests_as_network() -> None:
    """socket_enabled fixture requests lift --disable-socket per test and gain the network marker."""
    item = _HookItem(fixturenames=("assay_root", "socket_enabled"))

    pytest_collection_modifyitems([item])  # type: ignore[list-item]  # ty: ignore[invalid-argument-type]

    assert any(marker.name == "network" for marker in item.markers)


def test_collection_hook_marks_hypothesis_tests_as_property() -> None:
    """Hypothesis-backed tests are auto-marked property for -m property selection."""
    item = _HookItem(function=_hypothesis_probe)

    pytest_collection_modifyitems([item])  # type: ignore[list-item]  # ty: ignore[invalid-argument-type]

    assert any(marker.name == "property" for marker in item.markers)


def test_benchmark_storage_resolves_to_artifact_owner(pytestconfig: pytest.Config) -> None:
    """pytest-benchmark storage resolves to the Assay-owned artifact path."""
    assert pytestconfig.getoption("benchmark_storage") == BENCHMARK_STORAGE_URI
