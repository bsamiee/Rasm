"""Region visibility, alignment, and editor coordinates."""

from typing import Final, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.calls import called
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.resources.theme import Metrics
from blender.bpy.ui.screens.areas import Edge, Located
from blender.records.faults import Fault, Outcome, RecordReason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

AXES: Final[tuple[tuple[Edge, Edge], ...]] = (("LEFT", "RIGHT"), ("TOP", "BOTTOM"))

# --- [MODELS] ---------------------------------------------------------------------------


class Visibility(msgspec.Struct, frozen=True, kw_only=True):
    """Region visibility request, checked on later host passes."""

    window: bpy.types.Window
    area: int
    setting: Descriptor[bpy.types.BoolProperty]
    visible: bool

    def advanced(self) -> Outcome[Option[Self]]:
        """Returns request while flag differs, Nothing once matched or area is gone."""
        return Ok(
            Located
            .found(self.window, self.area)
            .bind(lambda at: Option.of_optional(at.area.spaces.active))
            .filter(lambda space: isinstance(space, self.setting.root))
            .filter(lambda space: not self.setting.equal(self.setting.read(space), self.visible))
            .map(lambda _: self)
        )


class RegionView(msgspec.Struct, frozen=True, kw_only=True):
    """Region with window and area for one host call."""

    located: Located
    region: bpy.types.Region

    @classmethod
    def of(cls, located: Located, kind: "rna_enums.RegionTypeItems" = "WINDOW") -> Outcome[Self]:
        """Returns region of a type."""
        return located.region(kind).map(lambda region: cls(located=located, region=region))

    @property
    def override(self) -> "bpy.types.ContextTempOverride":
        """Context override of region."""
        return bpy.context.temp_override(window=self.located.window, area=self.located.area, region=self.region)

    @property
    def scale(self) -> float:
        """Editor to View2D scale, 1 outside node editor."""
        return Metrics.read().scale if self.located.area.type == "NODE_EDITOR" else 1.0

    def shown(self, *, visible: bool) -> Outcome[Visibility]:
        """Writes region visibility, refusing HUD or a missing member."""
        kind = self.region.type
        suffix = "toolbar" if kind == "TOOLS" else "asset_shelf" if kind == "ASSET_SHELF_HEADER" else kind.lower()
        identifier = f"show_region_{suffix}"

        def applied(space: bpy.types.Space) -> Outcome[Visibility]:
            """Writes active space visibility member."""
            return (
                Error((
                    Fault(
                        reason=RecordReason.OUTSIDE,
                        subject=repr(space),
                        member=", ".join(prop.identifier for prop in space.bl_rna.properties if prop.identifier.startswith("show_region_") and prop.identifier != identifier),
                        cause=identifier,
                    ),
                ))
                if kind == "HUD"
                else Descriptor.resolve(type(space), identifier, bpy.types.BoolProperty).bind(
                    lambda setting: setting.write(space, visible).map(lambda _: Visibility(window=self.located.window, area=self.located.area.as_pointer(), setting=setting, visible=visible))
                )
            )

        return (
            Option.of_optional(self.located.area.spaces.active).map(applied).default_with(lambda: Error((Fault(reason=BlenderReason.ABSENT, subject=repr(self.located.area), member="spaces.active"),)))
        )

    def flipped(self, side: Edge) -> Outcome[None]:
        """Moves region to `side` across its axis."""
        aligned = self.region.alignment
        match next((axis for axis in AXES if aligned in axis), ()):
            case _ if aligned == side:
                return Ok(None)
            case axis if side in axis:
                return called(bpy.ops.screen.region_flip, {}, override=Some(self.override)).map(lambda _: None)
            case axis:
                return Error((Fault(reason=RecordReason.OUTSIDE, subject=repr(self.located.area), member=", ".join(axis), cause=str(side)),))

    def quadded(self, *, shown: bool) -> Outcome[None]:
        """Toggles quad view when state differs."""
        quad = any(region.alignment == "QUAD_SPLIT" for region in self.located.area.regions)
        return Ok(None) if quad == shown else called(bpy.ops.screen.region_quadview, {}, override=Some(self.override)).map(lambda _: None)

    def to_view(self, x: float, y: float) -> tuple[float, float]:
        """Returns editor coordinates of region pixels."""
        point: tuple[float, float] = self.region.view2d.region_to_view(x, y)  # type: ignore[assignment]
        view_x, view_y = point
        scale = self.scale
        return view_x / scale, view_y / scale

    def to_region(self, x: float, y: float) -> Option[tuple[int, int]]:
        """Returns region pixels of editor point, Nothing outside the region."""
        scale = self.scale
        point: tuple[int, int] = self.region.view2d.view_to_region(x * scale, y * scale, clip=False)  # type: ignore[assignment]
        column, row = point
        return Some(point) if 0 <= column < self.region.width and 0 <= row < self.region.height else Nothing


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AXES", "RegionView", "Visibility"]
