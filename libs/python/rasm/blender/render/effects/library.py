"""Catalogued native compositor assets and publication into user libraries."""

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

from rasm.blender.commands.operators import Call, Done, Operator, Properties
from rasm.blender.data.rna.descriptors import written
from rasm.blender.extension.events.handlers import PathHandler
from rasm.blender.extension.identity import Identity, Key, Manifest
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult, BlenderWriteFault
from rasm.blender.files.assets import AssetLibrary, AssetMark, CatalogPath, published
from rasm.blender.nodes.graphs import Generated, NodeGroup
from rasm.blender.render.effects.stack import Context, EffectStack
from rasm.blender.settings.properties import Choices
from rasm.blender.ui.resources.previews import written as previewed
from rasm.blender.ui.resources.theme import Metrics
from rasm.imaging.chain import Effect
from rasm.imaging.compositing.blending import Entry
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.hashing import content_hash, ContentHash

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

    def published(self, identity: Identity, scene: bpy.types.Scene, *, context: Context = Nothing) -> BlenderResult[tuple[Path, ...]]:
        """Publishes changed native groups using their complete generated-content identities."""
        rows, folder = self.stack.standalone, self.library.located(identity)
        checker = Metrics.read().checker()
        files: BlenderResult[tuple[Path, ...]] = catch(lambda: tuple(folder.glob("effects_*.blend")), exception=OSError)().map_error(
            lambda error: (BlenderReadFault(request=str(folder), error=error),)
        )

        def entry(record: Entry[Effect], group: NodeGroup) -> BlenderResult[tuple[bpy.types.ID, AssetMark]]:
            def tiled(tree: bpy.types.NodeTree) -> BlenderResult[tuple[bpy.types.ID, AssetMark]]:
                mark = AssetMark(key=group.key, catalog=f"{identity.manifest.name}/{bpy.path.display_name(record.effect.stage.name.lower())}", description=record.effect.__doc__ or "")

                def shown(plane: npt.NDArray[np.float32]) -> BlenderResult[None]:
                    return (
                        catch(lambda: previewed(tree.preview_ensure(), plane, checker), exception=RuntimeError)()  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        .map_error(lambda error: (BlenderEditFault(request=repr(tree), error=error),))
                        .map(lambda _: None)
                    )

                return self.thumbnail.map(lambda thumbnail: thumbnail(identity, record.effect, scene).bind(shown)).default_value(Ok(None)).map(lambda _: (tree, mark))

            return group.made(identity).bind(tiled)

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
                    catch(file.unlink, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=str(file), error=error),))
                    for file in files
                    if file != path and file.name.startswith(prefix)
                ).map(lambda _: Some(path))

            return Ok(Nothing) if target in files else entry(record, group).bind(lambda asset: published(identity, folder, stem, (asset,))).bind(stale)

        def refreshed(pair: tuple[PyOpenColorIO.Config, tuple[Path, ...]]) -> BlenderResult[tuple[Path, ...]]:
            config, files = pair
            return accumulated(
                row.grouped(identity, scene, record, config, context=context).bind(lambda group: wrote(row.kind, record, group, config, files))
                for row in rows
                for record in (Entry(effect=row.seeded(scene)),)
            ).bind(
                lambda paths: accumulated(
                    catch(file.unlink, exception=OSError)().map_error(lambda error: (BlenderWriteFault(request=str(file), error=error),))
                    for file in files
                    if not any(file.name.startswith(f"effects_{row.kind}.") for row in rows)
                ).map(lambda _: tuple(path for held in paths for path in held.to_list()))
            )

        return Result[tuple[PyOpenColorIO.Config, tuple[Path, ...]], BlenderFaults].bind(paired(self.stack.config(), files), refreshed)

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
        """Returns the library, save action, and startup/file-load publication declaration."""

        def loaded(_path: Option[Path]) -> BlenderResult[None]:
            return Result[bpy.types.Scene, BlenderFaults].bind(
                Option.of_optional(bpy.context.scene).to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.context), key="scene"),)),
                lambda scene: self.published(identity, scene).map(lambda _: None),
            )

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
        ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EffectLibrary", "Published", "Saving", "libraries"]
