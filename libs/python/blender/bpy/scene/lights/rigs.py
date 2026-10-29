"""Light rig feature: registration, the rig and reference sections, and the place, catalog pick, arrange, and remove operators."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from functools import partial

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.ids.images import lighting
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.events import Handler, updated
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.scene.lights.placement import COLLECTION, DATA, EMPTY, LIGHTS, REFERENCE, replaced, resolved, rig_removed, rigged, RigSetup, RigState, served, SETUP, states, WORLD
from blender.bpy.scene.lights.references import rig_reference_apply_command, rig_reference_draw_command, rig_reference_gel_command, rig_reference_pick_command
from blender.bpy.scene.objects import focused
from blender.bpy.settings.attachments import Attached, Stored
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import FUNCTIONS, OperatorButton, Property
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.records.faults import Fault, Outcome, paired
from blender.records.fields import Retention
from blender.rendering.lighting.rigs import RIGS

# --- [MODELS] ---------------------------------------------------------------------------


class RigPick(bpy.types.PropertyGroup):
    """Catalog rig name."""

    name: bpy.props.EnumProperty(name="Rig", description="Catalog rig", items=choices((name, name, "") for name in RIGS))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LightRig(msgspec.Struct, frozen=True, kw_only=True):
    """Rig setup attachment with owned lights, collection, world, and reference image, and the rig and reference sections."""

    retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields attachment, ownerships, the handler placing rigs again when their reference image changes, and both sections.

        Args:
            identity: Identity of extension registering row.

        Yields:
            Acquisitions with `Band` pairs.
        """
        with identity.reports.deferred(replaced) as deferred:

            def referenced(_: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                touched = frozenset(block.original for update in updates if (block := update.id) is not None)

                def placed(attached: Attached[RigSetup]) -> None:
                    for rig, _ in EMPTY.held(identity):
                        if attached.struct(rig).reference in touched:
                            deferred(Handle.of(rig), identity)

                return SETUP.bound(identity).map(placed)

            yield Ok((
                SETUP,
                EMPTY,
                LIGHTS,
                DATA,
                COLLECTION,
                WORLD,
                REFERENCE,
                Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.Image,), referenced)),
                self.section(),
                self.references(),
            ))

    def section(self) -> Section[RigSetup]:
        """Returns active rig setup with the place, pick, arrange, and remove buttons in its header."""
        return Section(
            key="light_rig",
            title="Light Rig",
            header=tuple(OperatorButton(target=command) for command in (rig_place_command, rig_pick_command, rig_arrange_command, rig_remove_command)),
            store=Some(
                Store(
                    attachment=SETUP,
                    host=Some(Member(root=bpy.types.LayerObjects, path=("active",))),
                    retention=self.retention,
                    fields={"reference": lambda member: Property(target=member, function=FUNCTIONS["template_ID_preview"], keywords={"open": "image.open"})},
                )
            ),
        )

    @staticmethod
    def references() -> Section:
        """Returns the reference pick, draw, gel, and apply buttons."""
        commands = (rig_reference_pick_command, rig_reference_draw_command, rig_reference_gel_command, rig_reference_apply_command)
        return Section(key="rig_reference", title="Rig Reference", rows=tuple(OperatorButton(target=command) for command in commands))


# --- [OPERATIONS] -----------------------------------------------------------------------


def rig_place(call: Call[Empty]) -> Outcome[Done]:
    """Places the rig about the active object, or about the target of the active rig empty."""
    return (
        rooted(bpy.types.ViewLayer, call.context)
        .bind(focused)
        .bind(lambda item: served(call.identity, item))
        .bind(lambda holder: rigged(holder, EMPTY.found(call.identity, of=Some(holder)), call.context, call.identity))
        .map(lambda lights: Done(notes=(("INFO", f"Placed {len(lights)} lights"),)))
    )


def rig_pick(call: Call[RigPick]) -> Outcome[Done]:
    """Sets the resolved rig's layout from the catalog."""
    return (
        rooted(bpy.types.ViewLayer, call.context)
        .bind(lambda layer: resolved(layer, call.identity))
        .bind(lambda rig: states(call.identity, rig).bind(lambda pair: pair[0].write(rig, msgspec.structs.replace(pair[1], rig=RIGS[call.properties.name]))))
        .map(lambda _: Done())
    )


def rig_arrange(call: Call[Empty]) -> Outcome[Done]:
    """Converts the resolved rig's matched lights to the arranged lights its reference reading derives."""
    config = PyOpenColorIO.GetCurrentConfig()

    def converted(rig: bpy.types.Object, setup: RigSetup, store: Stored[RigState], state: RigState) -> Outcome[None]:
        return (
            lighting(setup.reference, config)
            .bind(state.rig.arranged)
            .bind(lambda arranged: arranged.to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(rig), member="reference")))
            .bind(lambda arranged: store.write(rig, msgspec.structs.replace(state, rig=msgspec.structs.replace(state.rig, layout=arranged))))
        )

    return (
        rooted(bpy.types.ViewLayer, call.context)
        .bind(lambda layer: resolved(layer, call.identity))
        .bind(lambda rig: paired(SETUP.read(call.identity, rig), states(call.identity, rig)).bind(lambda held: converted(rig, held[0], *held[1])))
        .map(lambda _: Done())
    )


def rig_remove(call: Call[Empty]) -> Outcome[Done]:
    """Removes the resolved rig's lights, empty, and collection."""
    return (
        paired(
            rooted(bpy.types.ViewLayer, call.context)
            .bind(lambda layer: resolved(layer, call.identity))
            .filter_with(lambda rig: EMPTY.marked(call.identity, rig).is_some(), lambda rig: Fault.of(BlenderReason.ABSENT, repr(rig), member=EMPTY.key)),
            rooted(bpy.types.Scene, call.context),
        )
        .bind(lambda pair: rig_removed(pair[0], call.identity, pair[1]))
        .map(lambda count: Done(notes=(("INFO", f"Removed {count} rig data-blocks"),)))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

rig_place_command: Operator[Empty] = Operator(form=Empty, run=rig_place)
rig_pick_command: Operator[RigPick] = Operator(form=RigPick, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), primary=Some("name"), run=rig_pick)
rig_arrange_command: Operator[Empty] = Operator(form=Empty, run=rig_arrange)
rig_remove_command: Operator[Empty] = Operator(form=Empty, run=rig_remove)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LightRig", "RigPick", "rig_arrange_command", "rig_pick_command", "rig_place_command", "rig_remove_command"]
