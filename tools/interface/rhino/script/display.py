# ty: ignore[invalid-argument-type, invalid-return-type, unknown-argument, unresolved-attribute, unresolved-import, unsupported-operator]
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
from interface.rhino.script.accessors import action, color, Internal, opened, preference
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
    """Rows deleting every user display mode, then each built-in mode's menu listing, shown modes alone listed, and its display attributes from every table row whose mode set holds it, a later row's value winning, a written mode updated after its last row and saved once."""
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
    subd_usages = ("SubDSmoothInteriorEdgeColorUsage", "SubDCreaseInteriorEdgeColorUsage", "SubDBoundaryEdgeColorUsage")
    fills = ((Native.COLOR, "GradTopLeft"), (Native.COLOR, "GradBottomLeft"), (Native.COLOR, "GradTopRight"), (Native.COLOR, "GradBottomRight"))
    inked = (
        "CurveColor",
        "SurfaceEdgeColor",
        "SurfaceIsoUVColor",
        "SurfaceIsoUColor",
        "SurfaceIsoVColor",
        "MeshEdgeColor",
        "ClippingEdgeColor",
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
                **dict.fromkeys(("SurfaceNakedEdgeColor", "MeshNakedEdgeColor", "MeshNonmanifoldEdgeColor"), color(Status.ERROR)),
                **dict.fromkeys(("SubDReflectionAxisLineColor", "SubDReflectionPlaneColor"), color(Guide.CONSTRUCTION)),
                "ViewSpecificAttributes.WorldAxisColorX": color(Axis.X),
                "ViewSpecificAttributes.WorldAxisColorY": color(Axis.Y),
                "ViewSpecificAttributes.WorldAxisColorZ": color(Axis.Z),
                **dict.fromkeys(("AmbientLightingColor", "ShadowColor", "FrontMaterial.Emission", "BackMaterial.Emission"), color(Surface.SHADOW)),
                "ClippingFillColor": color(Surface.SECTION),
                "ClippingShadeColor": color(Selection.BODY),
                "ControlPolygonColor": color(Guide.HANDLE),
                "LockedColor": color(Line.LOCKED),
                "GridPlaneColor": color(Line.GRID),
            },
        ),
        (
            drawn,
            {
                "FillMode": DisplayPipelineAttributes.FrameBufferFillMode.DefaultColor,
                "LinearWorkflowUsage": DisplayPipelineAttributes.LinearWorkflowUsages.Custom,
                **dict.fromkeys(("PreProcessColors", "PreProcessTextures", "PostProcessFrameBuffer"), False),
                "ControlPolygonUseFixedSingleColor": True,
                **dict.fromkeys(("UseSingleCurveColor", "SurfaceIsoSingleColor", "SurfaceIsoColorsUsed"), False),
                "SurfaceEdgeColorUsage": DisplayPipelineAttributes.SurfaceEdgeColorUse.ObjectColor,
                "SurfaceNakedEdgeColorUsage": DisplayPipelineAttributes.SurfaceNakedEdgeColorUse.SingleColorForAll,
                **dict.fromkeys(subd_usages, DisplayPipelineAttributes.SubDEdgeColorUse.ObjectColor),
                "SubDNonManifoldEdgeColorUsage": DisplayPipelineAttributes.SubDEdgeColorUse.SingleColorForAll,
                "ClippingPlaneFillColorUsage": DisplayPipelineAttributes.ClippingPlaneFillColorUse.SolidColor,
                "ClippingEdgeColorUsage": DisplayPipelineAttributes.ClippingEdgeColorUse.SolidColor,
                (Native.SOLID_COLOR,): color(Surface.CANVAS),
                (Native.BOOL, "SingleMeshWireColor"): False,
                **{(Native.TECHNICAL_USAGE, kind): fixed for kind, fixed in lines.items()},
                "SubDNonManifoldEdgeColor": color(Status.ERROR),
            },
        ),
        (screen, dict.fromkeys(inked, color(Ink.SCREEN)) | dict.fromkeys(fills, color(Surface.CANVAS))),
        (
            paper,
            {
                "FillMode": DisplayPipelineAttributes.FrameBufferFillMode.SolidColor,
                **dict.fromkeys(("UseSingleCurveColor", "SurfaceIsoSingleColor"), True),
                "SurfaceEdgeColorUsage": DisplayPipelineAttributes.SurfaceEdgeColorUse.SingleColorForAll,
                **dict.fromkeys(subd_usages, DisplayPipelineAttributes.SubDEdgeColorUse.SingleColorForAll),
                (Native.SOLID_COLOR,): color(Surface.PAPER),
                (Native.BOOL, "SingleMeshWireColor"): True,
                **dict.fromkeys(((Native.TECHNICAL_USAGE, kind) for kind in lines), True),
                **dict.fromkeys((*inked, "SubDSmoothInteriorEdgeColor", "SubDCreaseInteriorEdgeColor", "SubDBoundaryEdgeColor"), color(Ink.DOCUMENT)),
                **dict.fromkeys(fills, color(Surface.PAPER)),
            },
        ),
        (
            modeling,
            {
                **dict.fromkeys(("ShadingEnabled", "UseCustomObjectMaterial", "UseCustomObjectColor"), True),
                "FrontMaterialShine": 0.0,
                "FrontOverrideObjectTransparency": False,
                "BackfaceDisplayStyle": DisplayPipelineAttributes.BackfaceStyle.UseFrontFaceSettings,
                "LightingScheme": DisplayPipelineAttributes.LightingSchema.DefaultLighting,
                (Native.PER_PIXEL_LIGHTNING,): True,
                "AmbientLightingColor": color(Surface.AMBIENT),
                **dict.fromkeys(
                    (
                        "CastShadows",
                        "ShowIsoCurves",
                        "ShowTangentEdges",
                        "ShowTangentSeams",
                        "ShowSurfaceNakedEdge",
                        "MeshSpecificAttributes.ShowMeshWires",
                        "ShowMeshNakedEdges",
                        "ShowSubDEdges",
                        "ShowSubDNonmanifoldEdges",
                        "ViewSpecificAttributes.DrawZAxis",
                        "UseSectionStyles",
                    ),
                    False,
                ),
                **dict.fromkeys(
                    (
                        "ShowSurfaceEdges",
                        "ShowMeshEdges",
                        "LayersFollowLockUsage",
                        "ControlPolygonUseSolidLines",
                        "ViewSpecificAttributes.DrawGrid",
                        "ViewSpecificAttributes.DrawGridAxes",
                        "ViewSpecificAttributes.DrawWorldAxes",
                    ),
                    True,
                ),
                **dict.fromkeys(("SurfaceEdgeThicknessScale", "SubDCreaseInteriorEdgeThickness", "SubDBoundaryEdgeThickness", "SubDBoundaryThicknessScale", "SubDReflectionAxisLineThickness"), 1.0),
                **dict.fromkeys(("MeshEdgeThickness", "MeshNakedEdgeThickness", "ClippingEdgeThickness"), 1),
                **dict.fromkeys(("MeshEdgeColorReduction", "GridTransparency"), 0),
                "ControlPolygonStyle": PointStyle.RoundDot,
                **dict.fromkeys(("PointStyle", "PointCloudStyle"), PointStyle.RoundSimple),
                **dict.fromkeys(("PointRadius", (Native.INT, "PCGripSize")), round((logical - 1) / 2)),
                "PointCloudRadius": round(logical),
            },
        ),
        ((*modeling, DisplayModeDescription.WireframeId), {"ViewSpecificAttributes.GridFade": 0.0078125, "ViewSpecificAttributes.GridCornerRadius": 0.9921875}),
        ((DisplayModeDescription.GhostedId,), {"FrontOverrideObjectTransparency": True}),
        (custom_material, {"FrontDiffuse": color(Surface.SHADED)}),
    )

    def remove() -> None:
        """Delete every user display mode and its stored settings, then save the display modes."""
        stored = opened(("Options", "DisplayAttributesManager"))
        for mode in modes(user=True):
            DisplayModeDescription.DeleteDisplayMode(mode)
            stored.DeleteChild(str(mode))
        DisplayModeDescription.SaveDisplayModes()

    written = set[System.Guid]()

    def row(mode: System.Guid, owner: object, label: str, member: str | tuple[Native, *tuple[str, ...]], target: object) -> Row:
        """Row of one member of a mode's description or display attributes under the owner's label, a public member by its dotted path and a native attribute by its family and internal enum member, its write marking the mode written."""
        read: Callable[[], object]
        write: Callable[[object], object]
        match member:
            case str():
                *owners, name = member.split(".")
                label, read, write = f"{label}.{member}", partial(attrgetter(member), owner), partial(setattr, reduce(getattr, owners, owner), name)
            case (family, *names):
                (read, write), label = family.accessors(owner, *names), "".join((f"{label}.{family}", *(f'["{name}"]' for name in names)))

        def marked(value: object) -> object:
            written.add(mode)
            return write(value)

        return preference(label=label, read=read, write=marked, target=target)

    yield action(label="DisplayModeDescription.GetDisplayModes", read=partial(modes, user=True), act=remove, target=())
    for mode in built_in:
        description = DisplayModeDescription.GetDisplayMode(mode)
        label = f'DisplayModeDescription["{description.EnglishName}"]'
        yield row(mode, description, label, "InMenu", mode in shown)
        yield from (
            row(mode, description.DisplayAttributes, f"{label}.DisplayAttributes", member, target)
            for member, target in {key: value for modes, mapping in table if mode in modes for key, value in mapping.items()}.items()
        )
        if mode in written:
            DisplayModeDescription.UpdateDisplayMode(description, bSave=False)
    if written:
        DisplayModeDescription.SaveDisplayModes()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["curve_width", "point_width", "rows"]
