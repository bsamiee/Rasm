"""Cryptomatte manifests and rank channels decoded into coverage."""

import functools
import operator
import re
from typing import Annotated, Self

from expression import Option
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from rasm.imaging.faults import accumulated, ImagingReadFault, ImagingResult
from rasm.imaging.files.multilayer import Layer, Multilayer
from rasm.imaging.parameters import Rgb

# --- [MODELS] ---------------------------------------------------------------------------


class Matte(msgspec.Struct, frozen=True, kw_only=True):
    """One Cryptomatte layer: its name, each ID's encoded hash, and its rank layers in order."""

    name: str
    ids: dict[str, int]
    ranks: tuple[Layer, ...]
    file: Multilayer

    @classmethod
    def found(cls, file: Multilayer) -> ImagingResult[tuple[Self, ...]]:
        """Decodes every Cryptomatte layer a file's header names.

        Args:
            file: Indexed multilayer file.

        Returns:
            The mattes in header order; the `read` case holding the path and msgspec's error for each manifest that fails its decode.
        """
        header = {key: value for part in file.parts for key, value in part.header.items()}
        layers, rgba = sorted(file.layers, key=lambda owner: owner.name), "rgba"

        def decoded(name: str, manifest: str) -> ImagingResult[Self]:
            ranks = tuple(
                Layer(part=layer.part, name=layer.name, channels=tuple(folded[key] for key in rgba))
                for layer in layers
                if re.fullmatch(f"{re.escape(name)}[0-9]{{2}}", layer.name) and (folded := {channel.name.casefold(): channel for channel in layer.channels}).keys() >= set(rgba)
            )
            return (
                catch(lambda: msgspec.json.decode(manifest, type=dict[str, Annotated[str, msgspec.Meta(pattern="^[0-9a-fA-F]{8}$")]]), exception=msgspec.DecodeError)()
                .map_error(lambda error: (ImagingReadFault(request=file.path, error=error),))
                .map(
                    lambda entries: cls(
                        name=name,
                        ids={entity: code & 0x807FFFFF | min(max(code >> 23 & 0xFF, 1), 254) << 23 for entity, digits in entries.items() for code in (int(digits, 16),)},
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

    def coverage(self, names: frozenset[str]) -> ImagingResult[npt.NDArray[np.float32]]:
        """Sums the coverage of the named IDs.

        Args:
            names: Manifest names whose coverage sums, a name outside the manifest adding nothing.

        Returns:
            The `(height, width)` coverage; the `read` case holding the path and OpenImageIO's text.
        """
        targets = np.fromiter((code for entity, code in self.ids.items() if entity in names), dtype=np.uint32)
        frame = np.zeros((self.file.parts[0].height, self.file.parts[0].width), dtype=np.float32)
        return accumulated(
            self.file.plane(rank, rank.channels).map(lambda ranked: np.where(np.isin(ranked.view(np.uint32)[..., 0::2], targets), ranked[..., 1::2], 0.0).sum(axis=-1, dtype=np.float32))
            for rank in self.ranks
        ).map(lambda terms: functools.reduce(operator.add, terms, frame))

    def areas(self) -> ImagingResult[dict[str, float]]:
        """Measures each manifest name's coverage as a share of the frame.

        Returns:
            Each name's share, 0 for a name no rank pixel holds; the `read` case holding the path and OpenImageIO's text.
        """

        def counted(ranked: npt.NDArray[np.float32]) -> tuple[npt.NDArray[np.uint32], npt.NDArray[np.float64]]:
            codes, inverse = np.unique(ranked.view(np.uint32)[..., 0::2], return_inverse=True)
            return codes, np.bincount(inverse.reshape(-1), weights=ranked[..., 1::2].reshape(-1))

        def joined(terms: tuple[tuple[npt.NDArray[np.uint32], npt.NDArray[np.float64]], ...]) -> dict[str, float]:
            codes, inverse = np.unique(np.concatenate((np.empty(0, dtype=np.uint32), *(codes for codes, _ in terms))), return_inverse=True)
            sums = np.bincount(inverse, weights=np.concatenate((np.empty(0), *(sums for _, sums in terms))), minlength=codes.size)
            found = dict(zip(codes.tolist(), (sums / (self.file.parts[0].width * self.file.parts[0].height)).tolist(), strict=True))
            return {entity: found.get(code, 0.0) for entity, code in self.ids.items()}

        return accumulated(self.file.plane(rank, rank.channels).map(counted) for rank in self.ranks).map(joined)

    def filled(self, names: frozenset[str], color: Rgb) -> ImagingResult[npt.NDArray[np.float32]]:
        """Fills the named IDs' coverage with one color as associated RGBA.

        Args:
            names: Manifest names the fill covers.
            color: Fill color.

        Returns:
            The `(height, width, 4)` plane, alpha equal to coverage; the `read` case holding the path and OpenImageIO's text.
        """
        tint = np.array((color.red, color.green, color.blue, 1.0), dtype=np.float32)
        return self.coverage(names).map(lambda covered: covered[..., None] * tint)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Matte"]
