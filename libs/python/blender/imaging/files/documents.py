"""Layered TIFF documents with Photoshop layer data and Linear Raw DNG frames through tifffile."""

import fractions
import functools
import struct

from expression import Nothing, Option
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import psdtags
import PyOpenColorIO
import tifffile

from blender.imaging.color.processors import basis
from blender.imaging.files.images import profile, written
from blender.records.faults import Fault, Outcome, paired, RecordReason
from blender.records.fields import Finite, Positive

# --- [TYPES] ----------------------------------------------------------------------------

type DocumentLayers[D: np.uint8 | np.uint16 | np.float32] = tuple[PixelLayer[D] | AdjustmentLayer[D] | LayerGroup[D], ...]


# --- [MODELS] ---------------------------------------------------------------------------


class PixelLayer[D: np.uint8 | np.uint16 | np.float32](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Pixel layer in unassociated color with alpha."""

    name: str
    color: npt.NDArray[D]
    alpha: npt.NDArray[D]
    mask: Option[npt.NDArray[D]] = Nothing
    mode: psdtags.PsdBlendMode = psdtags.PsdBlendMode.NORMAL
    opacity: float = 1.0
    visible: bool = True


class AdjustmentLayer[D: np.uint8 | np.uint16 | np.float32](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Adjustment layer as rasterized result in unassociated color."""

    name: str
    color: npt.NDArray[D]
    alpha: npt.NDArray[D]
    visible: bool = True


class LayerGroup[D: np.uint8 | np.uint16 | np.float32](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Layer group, layers bottom first."""

    name: str
    layers: DocumentLayers[D]
    mode: Option[psdtags.PsdBlendMode] = Nothing
    opacity: float = 1.0
    visible: bool = True


class Document[D: np.uint8 | np.uint16 | np.float32](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Layers bottom first and density in pixels per meter."""

    layers: DocumentLayers[D]
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

        def records(node: PixelLayer[D] | AdjustmentLayer[D] | LayerGroup[D]) -> tuple[psdtags.PsdLayer, ...]:
            match node:
                case PixelLayer():
                    channels = (*zip(ids, (*(node.color[..., index] for index in range(3)), node.alpha)), *((psdtags.PsdChannelId.USER_LAYER_MASK, mask) for mask in node.mask.to_list()))
                    mask = node.mask.map(lambda _: psdtags.PsdLayerMask(rectangle=canvas)).to_optional()
                    return (record(node.name, blend=(node.mode, node.opacity), visible=node.visible, channels=channels, rectangle=canvas, mask=mask, info=()),)
                case AdjustmentLayer():
                    channels = tuple(zip(ids, (*(node.color[..., index] for index in range(3)), node.alpha)))
                    return (record(node.name, blend=(psdtags.PsdBlendMode.NORMAL, 1.0), visible=node.visible, channels=channels, rectangle=canvas, mask=None, info=()),)
                case LayerGroup():
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
        tagged = catch(source.tifftag, exception=struct.error)(compression=psdtags.PsdCompressionType.ZIP, maxworkers=tifffile.TIFF.MAXWORKERS).map_error(
            lambda error: (Fault(reason=RecordReason.UNWRITABLE, subject=path, cause=error),)
        )
        return paired(tagged, profile(config, self.colorspace)).bind(
            lambda pair: written(
                path,
                functools.partial(
                    tifffile.imwrite,
                    data=self.composite,
                    photometric=tifffile.PHOTOMETRIC.RGB,
                    planarconfig=tifffile.PLANARCONFIG.CONTIG,
                    extrasamples=(tifffile.EXTRASAMPLE.UNASSALPHA,),
                    compression=tifffile.COMPRESSION.ADOBE_DEFLATE,
                    predictor=tifffile.PREDICTOR.HORIZONTAL if np.issubdtype(self.composite.dtype, np.integer) else tifffile.PREDICTOR.NONE,
                    iccprofile=pair[1].to_optional(),
                    resolution=(self.density * 0.0254, self.density * 0.0254),
                    resolutionunit=tifffile.RESUNIT.INCH,
                    byteorder=source.byteorder,
                    metadata=None,
                    software=False,
                    bigtiff=False,
                    extratags=[pair[0]],
                ),
            )
        )


class Optics(msgspec.Struct, frozen=True, kw_only=True):
    """Source camera focal length in meters and f-number."""

    focal_length: Positive
    f_number: Positive


class LinearRaw(msgspec.Struct, frozen=True, kw_only=True):
    """Camera and exposure tags of Linear Raw DNG."""

    make: str
    model: str
    exposure: Finite = 0.0
    optics: Option[Optics] = Nothing

    def written(self, path: str, frame: npt.NDArray[np.float32], config: PyOpenColorIO.Config) -> Outcome[str]:
        """Writes scene-linear frame as Linear Raw DNG.

        Args:
            path: File to write.
            frame: C-contiguous `(height, width, 3)` scene-linear frame.
            config: Config defining scene-linear space.

        Returns:
            Path, or `color`, `numeric`, or `write` fault.
        """

        def signed(*values: float) -> tuple[tuple[int, int], ...]:
            limits = np.iinfo(np.int32)
            bounded = (
                fraction.limit_denominator(min(limits.max, (limits.max if fraction > 0 else -limits.min) * fraction.denominator // abs(fraction.numerator))) if fraction else fraction
                for fraction in map(fractions.Fraction, values)
            )
            return tuple(fraction.as_integer_ratio() for fraction in bounded)

        def tags(matrix: npt.NDArray[np.inexact]) -> list[tuple[int, int, int, object, bool]]:
            rows = (
                ("Make", tifffile.DATATYPE.ASCII, 0, self.make),
                ("Model", tifffile.DATATYPE.ASCII, 0, self.model),
                ("UniqueCameraModel", tifffile.DATATYPE.ASCII, 0, f"{self.make} {self.model}"),
                ("DNGVersion", tifffile.DATATYPE.BYTE, 4, (1, 4, 0, 0)),
                ("DNGBackwardVersion", tifffile.DATATYPE.BYTE, 4, (1, 4, 0, 0)),
                ("ColorMatrix1", tifffile.DATATYPE.SRATIONAL, 9, signed(*matrix.reshape(-1).tolist())),
                ("CalibrationIlluminant1", tifffile.DATATYPE.SHORT, 1, 21),
                ("AsShotNeutral", tifffile.DATATYPE.RATIONAL, 3, (tifffile.rational(1.0),) * 3),
                ("BaselineExposure", tifffile.DATATYPE.SRATIONAL, 1, signed(self.exposure)),
                *(
                    row
                    for optics in self.optics.to_list()
                    for row in (
                        ("FocalLength", tifffile.DATATYPE.RATIONAL, 1, tifffile.rational(optics.focal_length * 1000.0)),
                        ("FNumber", tifffile.DATATYPE.RATIONAL, 1, tifffile.rational(optics.f_number)),
                    )
                ),
            )
            return [(tifffile.TIFF.TAGS[name], kind, count, value, True) for name, kind, count, value in rows]

        return (
            basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR)
            .bind(lambda matrix: catch(np.linalg.inv, exception=np.linalg.LinAlgError)(matrix).map_error(lambda error: (Fault(reason=RecordReason.NUMERIC, subject=str(matrix), cause=error),)))
            .bind(
                lambda inverse: written(
                    path,
                    functools.partial(
                        tifffile.imwrite,
                        data=frame,
                        photometric=tifffile.PHOTOMETRIC.LINEAR_RAW,
                        planarconfig=tifffile.PLANARCONFIG.CONTIG,
                        subfiletype=tifffile.FILETYPE.UNDEFINED,
                        metadata=None,
                        software=False,
                        bigtiff=False,
                        extratags=tags(inverse),
                    ),
                )
            )
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AdjustmentLayer", "Document", "DocumentLayers", "LayerGroup", "LinearRaw", "Optics", "PixelLayer"]
