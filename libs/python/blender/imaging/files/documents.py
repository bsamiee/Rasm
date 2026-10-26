"""Layered TIFF documents with Photoshop layer data."""

import functools
import struct
from typing import IO

from expression import Nothing, Option
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import psdtags
import PyOpenColorIO
import tifffile

from blender.imaging.files.images import profile, written
from blender.records.faults import Fault, Outcome, paired, RecordReason

# --- [TYPES] ----------------------------------------------------------------------------

type Layers[D: np.uint8 | np.uint16 | np.float32] = tuple[Raster[D] | Adjustment[D] | Group[D], ...]


# --- [MODELS] ---------------------------------------------------------------------------


class Raster[D: np.uint8 | np.uint16 | np.float32](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Pixel layer in unassociated color with alpha."""

    name: str
    color: npt.NDArray[D]
    alpha: npt.NDArray[D]
    mask: Option[npt.NDArray[D]] = Nothing
    mode: psdtags.PsdBlendMode = psdtags.PsdBlendMode.NORMAL
    opacity: float = 1.0
    visible: bool = True


class Adjustment[D: np.uint8 | np.uint16 | np.float32](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Adjustment layer as rasterized result in unassociated color."""

    name: str
    color: npt.NDArray[D]
    alpha: npt.NDArray[D]
    visible: bool = True


class Group[D: np.uint8 | np.uint16 | np.float32](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Layer group, layers bottom first."""

    name: str
    layers: Layers[D]
    mode: Option[psdtags.PsdBlendMode] = Nothing
    opacity: float = 1.0
    visible: bool = True


class Document[D: np.uint8 | np.uint16 | np.float32](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Layers bottom first and density in pixels per meter."""

    layers: Layers[D]
    composite: npt.NDArray[D]
    colorspace: str
    density: float

    def written(self, path: str, config: PyOpenColorIO.Config) -> Outcome[str]:
        """Writes layered TIFF with colorspace ICC profile.

        Args:
            path: File to write.
            config: Config the colorspace resolves in.

        Returns:
            Path, `read` fault for profile IO, or `write` fault.
        """
        height, width = self.composite.shape[:2]
        canvas, void = psdtags.PsdRectangle(0, 0, height, width), psdtags.PsdRectangle(0, 0, 0, 0)
        ids = (*(psdtags.PsdChannelId(index) for index in range(3)), psdtags.PsdChannelId.TRANSPARENCY_MASK)
        empty = tuple(zip(ids, (np.zeros((0, 0), dtype=self.composite.dtype),) * 4))

        def record(
            name: str,
            *,
            blend: tuple[psdtags.PsdBlendMode, float],
            visible: bool,
            channels: tuple[tuple[psdtags.PsdChannelId, npt.NDArray[D]], ...],
            rectangle: psdtags.PsdRectangle,
            mask: psdtags.PsdLayerMask | None,
            info: tuple[psdtags.PsdKeyABC, ...],
        ) -> psdtags.PsdLayer:
            return psdtags.PsdLayer(
                name=name.encode("macroman", "replace").decode("macroman"),
                channels=[psdtags.PsdChannel(channel, data=data) for channel, data in channels],
                rectangle=rectangle,
                mask=mask,
                opacity=round(blend[1] * 255),
                blendmode=blend[0],
                flags=psdtags.PsdLayerFlag.PHOTOSHOP5 | (psdtags.PsdLayerFlag.BASE if visible else psdtags.PsdLayerFlag.VISIBLE),
                info=[psdtags.PsdString(psdtags.PsdKey.UNICODE_LAYER_NAME, name), *info],
            )

        def records(node: Raster[D] | Adjustment[D] | Group[D]) -> tuple[psdtags.PsdLayer, ...]:
            match node:
                case Raster():
                    channels = (*zip(ids, (*(node.color[..., index] for index in range(3)), node.alpha)), *((psdtags.PsdChannelId.USER_LAYER_MASK, mask) for mask in node.mask.to_list()))
                    mask = node.mask.map(lambda _: psdtags.PsdLayerMask(rectangle=canvas)).to_optional()
                    return (record(node.name, blend=(node.mode, node.opacity), visible=node.visible, channels=channels, rectangle=canvas, mask=mask, info=()),)
                case Adjustment():
                    channels = tuple(zip(ids, (*(node.color[..., index] for index in range(3)), node.alpha)))
                    return (record(node.name, blend=(psdtags.PsdBlendMode.NORMAL, 1.0), visible=node.visible, channels=channels, rectangle=canvas, mask=None, info=()),)
                case Group():
                    blend = (node.mode.default_value(psdtags.PsdBlendMode.PASS_THROUGH), node.opacity)
                    bounding = psdtags.PsdSectionDividerSetting(psdtags.PsdSectionDividerType.BOUNDING_SECTION_DIVIDER)
                    section = psdtags.PsdSectionDividerSetting(psdtags.PsdSectionDividerType.OPEN_FOLDER, blend[0])
                    return (
                        record("</Layer group>", blend=(psdtags.PsdBlendMode.NORMAL, 1.0), visible=True, channels=empty, rectangle=void, mask=None, info=(bounding,)),
                        *(layer for child in node.layers for layer in records(child)),
                        record(node.name, blend=blend, visible=node.visible, channels=empty, rectangle=void, mask=None, info=(section,)),
                    )

        source = psdtags.TiffImageSourceData(
            psdformat=psdtags.PsdFormat.LE32BIT,
            layers=psdtags.PsdLayers(
                key=next(key for key, char in psdtags.PsdLayers.TYPES.items() if np.dtype(char) == self.composite.dtype),
                layers=[layer for node in self.layers for layer in records(node)],
                has_transparency=True,
            ),
            usermask=psdtags.PsdUserMask(),
            info=[psdtags.PsdEmpty(psdtags.PsdKey.SAVING_MERGED_TRANSPARENCY2)],
        )

        def tiff(tag: tuple[int, int, int, bytes, bool], icc: Option[bytes], output: IO[bytes]) -> None:
            tifffile.imwrite(
                output,
                self.composite,
                photometric=tifffile.PHOTOMETRIC.RGB,
                planarconfig=tifffile.PLANARCONFIG.CONTIG,
                extrasamples=(tifffile.EXTRASAMPLE.UNASSALPHA,),
                compression=tifffile.COMPRESSION.ADOBE_DEFLATE,
                predictor=tifffile.PREDICTOR.HORIZONTAL if np.issubdtype(self.composite.dtype, np.integer) else tifffile.PREDICTOR.NONE,
                iccprofile=icc.to_optional(),
                resolution=(self.density * 0.0254, self.density * 0.0254),
                resolutionunit=tifffile.RESUNIT.INCH,
                byteorder=source.byteorder,
                metadata=None,
                software=False,
                bigtiff=False,
                extratags=[tag],
            )

        tagged = catch(lambda: source.tifftag(compression=psdtags.PsdCompressionType.ZIP, maxworkers=tifffile.TIFF.MAXWORKERS), exception=struct.error)().map_error(
            lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=path, cause=error),)
        )
        return paired(tagged, profile(config, self.colorspace)).bind(lambda pair: written(path, functools.partial(tiff, *pair)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Adjustment", "Document", "Group", "Layers", "Raster"]
