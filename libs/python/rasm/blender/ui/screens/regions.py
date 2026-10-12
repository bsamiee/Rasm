"""Native region visibility, alignment, and editor-space coordinates."""

from typing import Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from rasm.blender.commands.calls import called
from rasm.blender.data.rna.descriptors import Descriptor
from rasm.blender.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderResult
from rasm.blender.ui.resources.theme import Metrics
from rasm.blender.ui.screens.areas import Located, ScreenSide

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Visibility(msgspec.Struct, frozen=True, kw_only=True):
    """Requested native visibility flag observed on subsequent host passes."""

    window: bpy.types.Window
    area: int
    setting: Descriptor[bpy.types.BoolProperty]
    visible: bool

    def advanced(self) -> BlenderResult[Option[Self]]:
        """Returns this request while the flag differs, or Nothing after it or removal."""
        return Ok(
            Located
            .found(self.window, self.area)
            .bind(lambda at: Option.of_optional(at.area.spaces.active))
            .filter(lambda space: isinstance(space, self.setting.root))
            .filter(lambda space: not self.setting.equal(self.setting.read(space), self.visible))
            .map(lambda _: self)
        )


class RegionView(msgspec.Struct, frozen=True, kw_only=True):
    """Region and its owning window and area, valid during one native call."""

    located: Located
    region: bpy.types.Region

    @classmethod
    def of(cls, located: Located, kind: "rna_enums.RegionTypeItems" = "WINDOW") -> BlenderResult[Self]:
        """Returns the located region of this native type, or its absence."""
        return located.region(kind).map(lambda region: cls(located=located, region=region))

    @property
    def override(self) -> "bpy.types.ContextTempOverride":
        """Native context of this exact region."""
        return bpy.context.temp_override(window=self.located.window, area=self.located.area, region=self.region)

    @property
    def scale(self) -> float:
        """Scale from editor coordinates to the node editor's scaled View2D coordinates."""
        return Metrics.read().scale if self.located.area.type == "NODE_EDITOR" else 1.0

    def shown(self, *, visible: bool) -> BlenderResult[Visibility]:
        """Starts a native visibility transition, or returns an unsafe or missing member."""
        kind = self.region.type
        suffix = "toolbar" if kind == "TOOLS" else "asset_shelf" if kind == "ASSET_SHELF_HEADER" else kind.lower()
        identifier = f"show_region_{suffix}"

        def applied(space: bpy.types.Space) -> BlenderResult[Visibility]:
            """Resolves the live space's visibility member and writes through its RNA boundary."""
            return (
                Error((
                    BlenderChoiceFault(
                        path=repr(space),
                        identifier=identifier,
                        accepted=tuple(prop.identifier for prop in space.bl_rna.properties if prop.identifier.startswith("show_region_") and prop.identifier != identifier),
                    ),
                ))
                if kind == "HUD"
                else Descriptor.resolve(type(space), identifier, bpy.types.BoolProperty).bind(
                    lambda setting: setting.write(space, visible).map(lambda _: Visibility(window=self.located.window, area=self.located.area.as_pointer(), setting=setting, visible=visible))
                )
            )

        return Option.of_optional(self.located.area.spaces.active).map(applied).default_with(lambda: Error((BlenderAbsentFault(owner=repr(self.located.area), key="spaces.active"),)))

    def flipped(self, side: ScreenSide) -> BlenderResult[None]:
        """Moves a region to the requested edge on its native axis, or returns its refusal."""
        aligned = self.region.alignment
        return (
            Ok(None)
            if aligned == side.name
            else called(bpy.ops.screen.region_flip, {}, override=Some(self.override)).map(lambda _: None)
            if aligned == side.opposite.name
            else Error((BlenderChoiceFault(path=repr(self.located.area), identifier=side.name, accepted=tuple(each.name for each in ScreenSide if aligned in {each.name, each.opposite.name})),))
        )

    def quadded(self, *, shown: bool) -> BlenderResult[None]:
        """Changes the area's quad-view state when needed, or returns its native refusal."""
        quad = any(region.alignment == "QUAD_SPLIT" for region in self.located.area.regions)
        return Ok(None) if quad == shown else called(bpy.ops.screen.region_quadview, {}, override=Some(self.override)).map(lambda _: None)

    def to_view(self, x: float, y: float) -> tuple[float, float]:
        """Returns editor coordinates from region pixels, removing native node-space scaling."""
        point: tuple[float, float] = self.region.view2d.region_to_view(x, y)  # type: ignore[assignment]
        view_x, view_y = point
        scale = self.scale
        return view_x / scale, view_y / scale

    def to_region(self, x: float, y: float) -> Option[tuple[int, int]]:
        """Returns region pixels for an editor point, or Nothing outside the clipped view."""
        v2d_is_clipped = 12000
        scale = self.scale
        point: tuple[int, int] = self.region.view2d.view_to_region(x * scale, y * scale, clip=True)  # type: ignore[assignment]
        return Nothing if v2d_is_clipped in point else Some(point)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RegionView", "Visibility"]
