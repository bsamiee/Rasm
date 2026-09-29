# ty: ignore[invalid-argument-type, invalid-return-type, unresolved-attribute, unresolved-import, unsupported-operator]
# mypy: disable-error-code="import-untyped, import-not-found, no-any-unimported, arg-type, attr-defined, call-overload, no-any-return, operator, return-value"
"""Rhino's display modes as rows, every user mode deleted and each built-in mode's members at their roles, and the point and curve widths previews draw at."""

from collections.abc import Callable, Iterator
from enum import StrEnum
from functools import partial, reduce
from operator import attrgetter

from Eto.Forms import Screen
from Rhino.Display import DisplayModeDescription, DisplayPipelineAttributes, PointStyle
import System
from System import Array
from System.Drawing import Color
from System.Reflection import BindingFlags

from interface.report import Row
from interface.rhino.script.accessors import color, Internal, opened
from interface.roles import Axis, Guide, Ink, Line, POINT_WIDTH, Selection, Status, Surface

# --- [TYPES] ----------------------------------------------------------------------------


class Native(StrEnum):
    """Display attribute family RhinoCommon reaches through its `CDisplayPipelineAttributes_Get<family>` and `_Set<family>` native entries, or the solid fill through `SetFill`."""

    SOLID_COLOR = "SolidColor"
    COLOR = "Color"
    BOOL = "Bool"
    INT = "Int"
    TECHNICAL_USAGE = "TechnicalUsage"
    PER_PIXEL_LIGHTNING = "PerPixelLightning"

    def accessors(self, attributes: DisplayPipelineAttributes, *names: str) -> tuple[Callable[[], object], Callable[[object], object]]:
        """Read and write of the family's attribute the internal enum member names, a color exchanged as its ARGB integer."""

        def entries(*selector: object) -> tuple[Callable[[], object], Callable[[object], object]]:
            return partial(native, f"CDisplayPipelineAttributes_Get{self}", attributes, *selector), partial(native, f"CDisplayPipelineAttributes_Set{self}", attributes, *selector)

        def fill(value: Color) -> None:
            mode = attributes.FillMode
            attributes.SetFill(value)
            attributes.FillMode = mode

        match self:
            case Native.SOLID_COLOR:
                return lambda: attributes.GetFill()[0], fill
            case Native.COLOR:
                get, put = entries(Internal.DISPLAY_ATTRS_COLOR.parsed(*names))
                return lambda: Color.FromArgb(get()), lambda value: put(value.ToArgb())
            case Native.BOOL:
                return entries(Internal.DISPLAY_PIPELINE_ATTRIBUTES_BOOL.parsed(*names))
            case Native.INT:
                return entries(Internal.DISPLAY_ATTRIBUTES_INT.parsed(*names))
            case Native.TECHNICAL_USAGE:
                return entries(System.UInt32(int(Internal.TECHNICAL_MODE_PARAMETER.parsed(*names))))
            case Native.PER_PIXEL_LIGHTNING:
                return entries()


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- [WIDTHS]
def point_width() -> float:
    """Logical width a point draws at to span the declared device pixels on the primary screen."""
    return POINT_WIDTH / Screen.PrimaryScreen.LogicalPixelSize


def curve_width() -> float:
    """Logical width of Rhino's antialiased thickness-1 wire, `(scale - 1) / 2 + 1` device pixels at the primary screen's scale."""
    scale = Screen.PrimaryScreen.LogicalPixelSize
    return (scale + 1) / (2 * scale)


# --- [NATIVE]
def native(entry: str, owner: object, *arguments: object) -> object:
    """Result of the RhinoCommon native entry called on the owner's native pointer and the arguments."""
    pointer = owner.GetType().GetMethod("NonConstPointer", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, None)
    return Internal.UNSAFE_NATIVE_METHODS.type.GetMethod(entry, BindingFlags.Static | BindingFlags.NonPublic).Invoke(None, Array[System.Object]([pointer, *arguments]))


# --- [COMPOSITION] ----------------------------------------------------------------------


def rows() -> Iterator[Row]:
    """Rows deleting every user display mode, then each built-in mode's members from every table row whose mode set holds it, a later row's value winning, the mode saved after its last row."""
    paper, modeling = (DisplayModeDescription.PenId, DisplayModeDescription.AmbientOcclusionId), (DisplayModeDescription.ShadedId, DisplayModeDescription.XRayId, DisplayModeDescription.GhostedId)
    rendered = (DisplayModeDescription.RenderedId, DisplayModeDescription.RaytracedId)
    custom_material = (*modeling, DisplayModeDescription.AmbientOcclusionId, DisplayModeDescription.MonochromeId)
    shown = (DisplayModeDescription.ShadedId, DisplayModeDescription.WireframeId, DisplayModeDescription.XRayId, DisplayModeDescription.RenderedId, DisplayModeDescription.RaytracedId)

    def modes(*, user: bool) -> tuple[System.Guid, ...]:
        """Ids of the user display modes, ordered at zero or above in the manager list, or of Rhino's own below it."""
        return tuple(mode.Id for mode in DisplayModeDescription.GetDisplayModes() if (native("DisplayAttrsMgrListDesc_Order", mode) >= 0) is user)

    built_in = modes(user=False)
    screen, drawn = (tuple(mode for mode in built_in if mode not in held) for held in (paper, rendered))
    logical = point_width()
    subd_usages = ("DisplayAttributes.SubDSmoothInteriorEdgeColorUsage", "DisplayAttributes.SubDCreaseInteriorEdgeColorUsage", "DisplayAttributes.SubDBoundaryEdgeColorUsage")
    fills = ((Native.COLOR, "GradTopLeft"), (Native.COLOR, "GradBottomLeft"), (Native.COLOR, "GradTopRight"), (Native.COLOR, "GradBottomRight"))
    inked = (
        "DisplayAttributes.CurveColor",
        "DisplayAttributes.SurfaceEdgeColor",
        "DisplayAttributes.SurfaceIsoUVColor",
        "DisplayAttributes.SurfaceIsoUColor",
        "DisplayAttributes.SurfaceIsoVColor",
        "DisplayAttributes.MeshEdgeColor",
        "DisplayAttributes.ClippingEdgeColor",
        (Native.COLOR, "MeshWireColor"),
        (Native.COLOR, "TechnicalLine"),
        (Native.COLOR, "TechnicalEdge"),
        (Native.COLOR, "TechnicalSilhouette"),
        (Native.COLOR, "TechnicalIntersection"),
    )
    lines = {"TECH_HIDDENLINES": True, "TECH_EDGES": False, "TECH_SILHOUETTES": True, "TECH_CREASES": False, "TECH_SEAMS": False, "TECH_INTERSECTIONS": False}
    table = (
        (
            built_in,
            {
                "InMenu": False,
                **dict.fromkeys(("DisplayAttributes.SurfaceNakedEdgeColor", "DisplayAttributes.MeshNakedEdgeColor", "DisplayAttributes.MeshNonmanifoldEdgeColor"), color(Status.ERROR)),
                **dict.fromkeys(("DisplayAttributes.SubDReflectionAxisLineColor", "DisplayAttributes.SubDReflectionPlaneColor"), color(Guide.CONSTRUCTION)),
                "DisplayAttributes.ViewSpecificAttributes.WorldAxisColorX": color(Axis.X),
                "DisplayAttributes.ViewSpecificAttributes.WorldAxisColorY": color(Axis.Y),
                "DisplayAttributes.ViewSpecificAttributes.WorldAxisColorZ": color(Axis.Z),
                **dict.fromkeys(
                    ("DisplayAttributes.AmbientLightingColor", "DisplayAttributes.ShadowColor", "DisplayAttributes.FrontMaterial.Emission", "DisplayAttributes.BackMaterial.Emission"),
                    color(Surface.SHADOW),
                ),
                "DisplayAttributes.ClippingFillColor": color(Surface.SECTION),
                "DisplayAttributes.ClippingShadeColor": color(Selection.BODY),
                "DisplayAttributes.ControlPolygonColor": color(Guide.HANDLE),
                "DisplayAttributes.LockedColor": color(Line.LOCKED),
                "DisplayAttributes.GridPlaneColor": color(Line.GRID),
            },
        ),
        (
            drawn,
            {
                "DisplayAttributes.FillMode": DisplayPipelineAttributes.FrameBufferFillMode.DefaultColor,
                "DisplayAttributes.LinearWorkflowUsage": DisplayPipelineAttributes.LinearWorkflowUsages.Custom,
                **dict.fromkeys(("DisplayAttributes.PreProcessColors", "DisplayAttributes.PreProcessTextures", "DisplayAttributes.PostProcessFrameBuffer"), False),
                "DisplayAttributes.ControlPolygonUseFixedSingleColor": True,
                **dict.fromkeys(("DisplayAttributes.UseSingleCurveColor", "DisplayAttributes.SurfaceIsoSingleColor", "DisplayAttributes.SurfaceIsoColorsUsed"), False),
                "DisplayAttributes.SurfaceEdgeColorUsage": DisplayPipelineAttributes.SurfaceEdgeColorUse.ObjectColor,
                "DisplayAttributes.SurfaceNakedEdgeColorUsage": DisplayPipelineAttributes.SurfaceNakedEdgeColorUse.SingleColorForAll,
                **dict.fromkeys(subd_usages, DisplayPipelineAttributes.SubDEdgeColorUse.ObjectColor),
                "DisplayAttributes.SubDNonManifoldEdgeColorUsage": DisplayPipelineAttributes.SubDEdgeColorUse.SingleColorForAll,
                "DisplayAttributes.ClippingPlaneFillColorUsage": DisplayPipelineAttributes.ClippingPlaneFillColorUse.SolidColor,
                "DisplayAttributes.ClippingEdgeColorUsage": DisplayPipelineAttributes.ClippingEdgeColorUse.SolidColor,
                (Native.SOLID_COLOR,): color(Surface.CANVAS),
                (Native.BOOL, "SingleMeshWireColor"): False,
                **{(Native.TECHNICAL_USAGE, kind): fixed for kind, fixed in lines.items()},
                "DisplayAttributes.SubDNonManifoldEdgeColor": color(Status.ERROR),
            },
        ),
        (screen, dict.fromkeys(inked, color(Ink.SCREEN)) | dict.fromkeys(fills, color(Surface.CANVAS))),
        (
            paper,
            {
                "DisplayAttributes.FillMode": DisplayPipelineAttributes.FrameBufferFillMode.SolidColor,
                **dict.fromkeys(("DisplayAttributes.UseSingleCurveColor", "DisplayAttributes.SurfaceIsoSingleColor"), True),
                "DisplayAttributes.SurfaceEdgeColorUsage": DisplayPipelineAttributes.SurfaceEdgeColorUse.SingleColorForAll,
                **dict.fromkeys(subd_usages, DisplayPipelineAttributes.SubDEdgeColorUse.SingleColorForAll),
                (Native.SOLID_COLOR,): color(Surface.PAPER),
                (Native.BOOL, "SingleMeshWireColor"): True,
                **dict.fromkeys(((Native.TECHNICAL_USAGE, kind) for kind in lines), True),
                **dict.fromkeys(
                    (*inked, "DisplayAttributes.SubDSmoothInteriorEdgeColor", "DisplayAttributes.SubDCreaseInteriorEdgeColor", "DisplayAttributes.SubDBoundaryEdgeColor"), color(Ink.DOCUMENT)
                ),
                **dict.fromkeys(fills, color(Surface.PAPER)),
            },
        ),
        (
            modeling,
            {
                **dict.fromkeys(("DisplayAttributes.ShadingEnabled", "DisplayAttributes.UseCustomObjectMaterial", "DisplayAttributes.UseCustomObjectColor"), True),
                "DisplayAttributes.FrontMaterialShine": 0.0,
                "DisplayAttributes.FrontOverrideObjectTransparency": False,
                "DisplayAttributes.BackfaceDisplayStyle": DisplayPipelineAttributes.BackfaceStyle.UseFrontFaceSettings,
                "DisplayAttributes.LightingScheme": DisplayPipelineAttributes.LightingSchema.DefaultLighting,
                (Native.PER_PIXEL_LIGHTNING,): True,
                "DisplayAttributes.AmbientLightingColor": color(Surface.AMBIENT),
                **dict.fromkeys(
                    (
                        "DisplayAttributes.CastShadows",
                        "DisplayAttributes.ShowIsoCurves",
                        "DisplayAttributes.ShowTangentEdges",
                        "DisplayAttributes.ShowTangentSeams",
                        "DisplayAttributes.ShowSurfaceNakedEdge",
                        "DisplayAttributes.MeshSpecificAttributes.ShowMeshWires",
                        "DisplayAttributes.ShowMeshNakedEdges",
                        "DisplayAttributes.ShowSubDEdges",
                        "DisplayAttributes.ShowSubDNonmanifoldEdges",
                        "DisplayAttributes.ViewSpecificAttributes.DrawZAxis",
                        "DisplayAttributes.UseSectionStyles",
                    ),
                    False,
                ),
                **dict.fromkeys(
                    (
                        "DisplayAttributes.ShowSurfaceEdges",
                        "DisplayAttributes.ShowMeshEdges",
                        "DisplayAttributes.LayersFollowLockUsage",
                        "DisplayAttributes.ControlPolygonUseSolidLines",
                        "DisplayAttributes.ViewSpecificAttributes.DrawGrid",
                        "DisplayAttributes.ViewSpecificAttributes.DrawGridAxes",
                        "DisplayAttributes.ViewSpecificAttributes.DrawWorldAxes",
                    ),
                    True,
                ),
                **dict.fromkeys(
                    (
                        "DisplayAttributes.SurfaceEdgeThicknessScale",
                        "DisplayAttributes.SubDCreaseInteriorEdgeThickness",
                        "DisplayAttributes.SubDBoundaryEdgeThickness",
                        "DisplayAttributes.SubDBoundaryThicknessScale",
                        "DisplayAttributes.SubDReflectionAxisLineThickness",
                    ),
                    1.0,
                ),
                **dict.fromkeys(("DisplayAttributes.MeshEdgeThickness", "DisplayAttributes.MeshNakedEdgeThickness", "DisplayAttributes.ClippingEdgeThickness"), 1),
                **dict.fromkeys(("DisplayAttributes.MeshEdgeColorReduction", "DisplayAttributes.GridTransparency"), 0),
                "DisplayAttributes.ControlPolygonStyle": PointStyle.RoundDot,
                **dict.fromkeys(("DisplayAttributes.PointStyle", "DisplayAttributes.PointCloudStyle"), PointStyle.RoundSimple),
                **dict.fromkeys(("DisplayAttributes.PointRadius", (Native.INT, "PCGripSize")), round((logical - 1) / 2)),
                "DisplayAttributes.PointCloudRadius": round(logical),
            },
        ),
        ((DisplayModeDescription.GhostedId,), {"DisplayAttributes.FrontOverrideObjectTransparency": True}),
        (custom_material, {"DisplayAttributes.FrontDiffuse": color(Surface.SHADED)}),
        (shown, {"InMenu": True}),
    )

    def remove(_: object) -> None:
        """Delete every user display mode and its stored settings, then save the display modes."""
        stored = opened(("Options", "DisplayAttributesManager"))
        for mode in modes(user=True):
            DisplayModeDescription.DeleteDisplayMode(mode)
            stored.DeleteChild(str(mode))
        DisplayModeDescription.SaveDisplayModes()

    def row(description: DisplayModeDescription, member: str | tuple[Native, *tuple[str, ...]], target: object) -> Row:
        """Row of one member of the mode's description, a public member by its dotted path and a native attribute by its family and internal enum member."""
        mode = f'DisplayModeDescription["{description.EnglishName}"]'
        match member:
            case str():
                *owners, name = member.split(".")
                return Row(label=f"{mode}.{member}", read=partial(attrgetter(member), description), write=partial(setattr, reduce(getattr, owners, description), name), target=target)
            case (family, *names):
                read, write = family.accessors(description.DisplayAttributes, *names)
                return Row(label="".join((f"{mode}.DisplayAttributes.{family}", *(f'["{name}"]' for name in names))), read=read, write=write, target=target)

    yield Row(label="DisplayModeDescription.GetDisplayModes", read=partial(modes, user=True), write=remove, target=())
    for mode in built_in:
        description = DisplayModeDescription.GetDisplayMode(mode)
        yield from (row(description, member, target) for member, target in {key: value for modes, mapping in table if mode in modes for key, value in mapping.items()}.items())
        DisplayModeDescription.UpdateDisplayMode(description)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["curve_width", "point_width", "rows"]
