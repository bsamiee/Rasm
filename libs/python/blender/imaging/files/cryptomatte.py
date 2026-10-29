"""Cryptomatte manifests and rank channels decoded into coverage."""

from collections.abc import Mapping
from copy import replace
from functools import partial
import re
from types import MappingProxyType
from typing import Annotated, Self

from expression import Option
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from blender.imaging.files.multilayer import Layer, Multilayer
from blender.records.faults import accumulated, Fault, Outcome, RecordReason

# --- [TYPES] ----------------------------------------------------------------------------

type _Manifest = dict[str, Annotated[str, msgspec.Meta(pattern="^[0-9a-fA-F]{8}$")]]

# --- [MODELS] ---------------------------------------------------------------------------


class Matte(msgspec.Struct, frozen=True, kw_only=True):
    """Cryptomatte layer with hash per ID and rank layers in order."""

    name: str
    ids: Mapping[str, int]
    ranks: tuple[Layer, ...]
    file: Multilayer

    @classmethod
    def found(cls, file: Multilayer) -> Outcome[tuple[Self, ...]]:
        """Decodes every Cryptomatte layer a file's header names.

        Returns:
            Mattes in header order, or `read` fault per undecodable manifest.
        """
        header = {key: value for part in file.parts for key, value in part.header.items()}
        layers, rgba = sorted(file.layers, key=lambda owner: owner.name), "rgba"

        def decoded(name: str, manifest: str) -> Outcome[Self]:
            ranks = tuple(
                replace(layer, channels=tuple(folded[key] for key in rgba))
                for layer in layers
                if re.fullmatch(f"{re.escape(name)}[0-9]{{2}}", layer.name) and (folded := {channel.name.casefold(): channel for channel in layer.channels}).keys() >= set(rgba)
            )
            return (
                catch(msgspec.json.decode, exception=msgspec.DecodeError)(manifest, type=_Manifest)
                .map_error(partial(Fault.of, RecordReason.UNREADABLE, str(file.path)))
                .map(
                    lambda entries: cls(
                        name=name,
                        ids=MappingProxyType({entity: code & 0x807FFFFF | min(max(code >> 23 & 0xFF, 1), 254) << 23 for entity, digits in entries.items() for code in (int(digits, 16),)}),
                        ranks=ranks,
                        file=file,
                    )
                )
            )

        return accumulated(
            decoded(name, manifest)
            for key, name in header.items()
            if key.startswith("cryptomatte/") and key.endswith("/name")
            for manifest in Option.of_optional(header.get(f"{key.removesuffix('/name')}/manifest")).to_list()
        )

    def coverage(self, names: frozenset[str]) -> Outcome[npt.NDArray[np.float32]]:
        """Sums coverage of named IDs, unknown names adding nothing.

        Returns:
            `(height, width)` coverage, or `read` fault.
        """
        targets = np.fromiter((code for entity, code in self.ids.items() if entity in names), dtype=np.uint32)
        return self._planes().map(lambda planes: np.where(np.isin(planes[0], targets), planes[1], 0.0).sum(axis=-1, dtype=np.float32))

    def areas(self) -> Outcome[dict[str, float]]:
        """Measures frame share each manifest name covers.

        Returns:
            Share per name, 0 for names no pixel holds, or `read` fault.
        """
        part = self.file.parts[0]

        def summed(planes: tuple[npt.NDArray[np.uint32], npt.NDArray[np.float32]]) -> dict[str, float]:
            codes, inverse = np.unique(planes[0], return_inverse=True)
            found = dict(zip(codes.tolist(), (np.bincount(inverse.reshape(-1), weights=planes[1].reshape(-1), minlength=codes.size) / (part.width * part.height)).tolist()))
            return {entity: found.get(code, 0.0) for entity, code in self.ids.items()}

        return self._planes().map(summed)

    def _planes(self) -> Outcome[tuple[npt.NDArray[np.uint32], npt.NDArray[np.float32]]]:
        """ID codes and coverage weights of every rank pair, stacked along the last axis, `(height, width, 2 * ranks)`."""
        part = self.file.parts[0]
        empty = np.zeros((part.height, part.width, 0), dtype=np.float32)
        return (
            accumulated(self.file.plane(rank, rank.channels) for rank in self.ranks)
            .map(lambda ranked: np.concatenate((empty, *ranked), axis=-1))
            .map(lambda stacked: (stacked.view(np.uint32)[..., 0::2], stacked[..., 1::2]))
        )

    def filled(self, names: frozenset[str], color: npt.NDArray[np.float64]) -> Outcome[npt.NDArray[np.float32]]:
        """Fills named ID coverage with a `(3,)` color as associated RGBA.

        Returns:
            `(height, width, 4)` plane with coverage alpha, or `read` fault.
        """
        tint = np.append(color, 1.0).astype(np.float32)
        return self.coverage(names).map(lambda covered: covered[..., None] * tint)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Matte"]
