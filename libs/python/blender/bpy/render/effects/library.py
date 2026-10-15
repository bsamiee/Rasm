"""Catalogued native compositor assets and publication into user libraries."""

from collections import deque
from collections.abc import Callable
from pathlib import Path
from typing import Annotated, ClassVar

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Properties
from blender.bpy.data.rna.descriptors import written
from blender.bpy.extension.events.handlers import PathHandler
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Key, Manifest
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult, BlenderWriteFault
from blender.bpy.files.assets import AssetLibrary, AssetMark, CatalogPath, published
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.graphs import Generated, NodeGroup
from blender.bpy.render.effects.stack import Context, EffectStack, Placeable
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.resources.previews import written as previewed
from blender.bpy.ui.resources.theme import Metrics
from blender.imaging.chain import Effect
from blender.imaging.compositing.blending import Entry
from blender.imaging.faults import accumulated, paired
from blender.imaging.hashing import content_hash, ContentHash

# --- [ROSTERS] --------------------------------------------------------------------------

libraries = Choices(
    lambda _identity, _owner, context: (
        (row.name, row.name, row.path)
        for preferences in Option.of_optional(context).bind(lambda held: Option.of_optional(held.preferences)).to_list()
        for row in preferences.filepaths.asset_libraries
        if isinstance(row, bpy.types.PreferencesAssetLibrary)
    )
)


# --- [MODELS] ---------------------------------------------------------------------------


class Saving(msgspec.Struct, frozen=True, kw_only=True):
    """User asset library and catalog receiving the active effect group."""

    library: Annotated[str, msgspec.Meta(title="Library"), libraries] = ""
    catalog: Annotated[CatalogPath, msgspec.Meta(title="Catalog")] = "Effects"


class Published(msgspec.Struct, frozen=True, kw_only=True):
    """Manifest, offered seed, and native generated identities of one asset file."""

    manifest: Manifest
    row: Key
    seed: Effect
    content: tuple[ContentHash, ...]
    version: tuple[int, ...]
    build: bytes


# --- [SERVICES] -------------------------------------------------------------------------


class EffectLibrary(msgspec.Struct, frozen=True, kw_only=True):
    """Offered effect assets with an application-supplied thumbnail raster policy."""

    stack: EffectStack
    library: AssetLibrary
    thumbnail: Option[Callable[[Identity, Effect, bpy.types.Scene], BlenderResult[npt.NDArray[np.float32]]]] = Nothing
    saved: ClassVar[Key] = "saved_effect"

    def files(self, identity: Identity) -> BlenderResult[tuple[Path, ...]]:
        """Lists every published effect file in the extension's asset folder."""
        folder = self.library.located(identity)
        return catch(lambda: tuple(folder.glob("effects_*.blend")), exception=OSError)().map_error(lambda error: (BlenderReadFault(request=folder, error=error),))

    def retired(self, identity: Identity) -> BlenderResult[None]:
        """Removes published files of rows the stack no longer offers."""
        kinds = tuple(f"effects_{row.kind}." for row in self.stack.standalone)
        return self.files(identity).bind(
            lambda files: accumulated(
                catch(file.unlink, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=file, error=error),)) for file in files if not file.name.startswith(kinds)
            ).map(lambda _: None)
        )

    def published(self, identity: Identity, scene: bpy.types.Scene, row: Placeable, *, context: Context = Nothing) -> BlenderResult[Option[Path]]:
        """Publishes one row's changed native group under its complete generated-content identity."""
        folder = self.library.located(identity)
        checker = Metrics.read().checker()

        def entry(record: Entry[Effect], group: NodeGroup) -> BlenderResult[tuple[bpy.types.ID, AssetMark]]:
            mark = AssetMark(
                key=group.key, catalog=f"{identity.manifest.name}/{bpy.path.display_name(record.effect.stage.name.lower())}", description=Option.of_optional(record.effect.__doc__).default_value("")
            )

            def shown(tree: bpy.types.NodeTree, plane: npt.NDArray[np.float32]) -> BlenderResult[None]:
                return (
                    catch(lambda: previewed(tree.preview_ensure(), plane, checker), exception=RuntimeError)()  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    .map_error(lambda error: (BlenderEditFault(request=repr(tree), error=error),))
                    .map(lambda _: None)
                )

            def tiled(plane: Option[npt.NDArray[np.float32]]) -> BlenderResult[tuple[bpy.types.ID, AssetMark]]:
                return made(group, identity).bind(lambda tree: plane.map(lambda pixels: shown(tree, pixels)).default_value(Ok(None)).map(lambda _: (tree, mark)))

            return self.thumbnail.map(lambda thumbnail: thumbnail(identity, record.effect, scene).map(Some)).default_value(Ok(Nothing)).bind(tiled)

        def wrote(kind: Key, record: Entry[Effect], group: NodeGroup, config: PyOpenColorIO.Config, files: tuple[Path, ...]) -> BlenderResult[Option[Path]]:
            content = tuple(
                generated.content
                for graph in (group.graph, *(nested.graph for nested in group.graph.nested))
                for node, _ in graph.placed.values()
                for generated in node.content.to_list()
                if isinstance(generated, Generated)
            )
            facts = Published(manifest=identity.manifest, row=kind, seed=record.effect, content=content, version=bpy.app.version, build=bpy.app.build_hash)
            prefix = f"effects_{kind}."
            stem = f"{prefix}{content_hash(Published, facts, config=Some(config)).hex()}"
            target = folder / f"{stem}.blend"

            def stale(path: Path) -> BlenderResult[Option[Path]]:
                return accumulated(
                    catch(file.unlink, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=file, error=error),)) for file in files if file != path and file.name.startswith(prefix)
                ).map(lambda _: Some(path))

            return Ok(Nothing) if target in files else entry(record, group).bind(lambda asset: published(identity, folder, stem, (asset,))).bind(stale)

        def built(record: Entry[Effect], config: PyOpenColorIO.Config, files: tuple[Path, ...]) -> BlenderResult[Option[Path]]:
            return row.grouped(identity, scene, record, config, context=context).bind(lambda group: wrote(row.kind, record, group, config, files))

        return paired(row.seeded(scene), paired(self.stack.config(), self.files(identity))).bind(lambda held: built(Entry(effect=held[0]), *held[1]))

    def saving(self, call: Call[Saving]) -> BlenderResult[Done]:
        """Writes the active effect into the chosen user's native asset library."""
        record = call.record
        group = call.root(bpy.types.Scene).bind(
            lambda scene: Option.of_optional(next((entry.node_group for entry in scene.compositor_effects if entry.is_active), None)).to_result_with(
                lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key="active"),)
            )
        )
        folder = call.root(bpy.types.Preferences).bind(
            lambda preferences: Option.of_optional(
                next((Path(row.path) for row in preferences.filepaths.asset_libraries if isinstance(row, bpy.types.PreferencesAssetLibrary) and row.name == record.library), None)
            ).to_result_with(lambda: (BlenderAbsentFault(owner=repr(preferences.filepaths.asset_libraries), key=record.library),))
        )

        def saved(pair: tuple[bpy.types.NodeTree, Path]) -> BlenderResult[Done]:
            group, folder = pair
            return (
                written(group, "allow_usage_in_scene_compositor_effect", True)
                .bind(lambda _: published(call.identity, folder, bpy.path.clean_name(group.name), ((group, AssetMark(key=self.saved, catalog=record.catalog)),), kept=True))
                .map(lambda path: Done(notes=(("INFO", f"Saved {group.name} into {path}"),)))
            )

        return Result[tuple[bpy.types.NodeTree, Path], BlenderFaults].bind(paired(group, folder), saved)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Holds the library, save action, and file-load publication one row per timer tick, idle in background runs that draw no asset."""
        rows = {row.kind: row for row in self.stack.standalone}
        pending: deque[Key] = deque()

        def loaded(_path: Option[Path]) -> BlenderResult[None]:
            pending.extend(kind for kind in rows if kind not in pending)
            return self.retired(identity)

        def ticked() -> BlenderResult[Option[float]]:
            for kind in (pending.popleft(),) if pending else ():
                scene = Option.of_optional(bpy.context.scene).to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.context), key="scene"),))
                for faults in Result[bpy.types.Scene, BlenderFaults].bind(scene, lambda scene: self.published(identity, scene, rows[kind])).swap().to_option().to_list():
                    identity.reports.hold(faults)
            return Ok(Some(0.0 if pending else 1.0))

        return Ok((
            self.library,
            Operator(
                key="effect_save_asset",
                title="Save Effect Asset",
                description="Saves the active compositor effect into a user asset library",
                form=Saving,
                run=self.saving,
                invoke=Some(Properties()),
                undo=False,
            ),
            PathHandler(handlers=bpy.app.handlers.load_post, body=loaded),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            Timer(body=ticked),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EffectLibrary", "Published", "Saving", "libraries"]
