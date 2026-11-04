"""Native File Output bindings and scene and routed Cryptomatte finishing."""

from contextlib import ExitStack
from itertools import islice
from pathlib import Path

import bpy
from expression import Some
import msgspec
import numpy as np

from blender.bpy.data.rna.paths import definition
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity
from blender.bpy.nodes import functions
from blender.bpy.nodes.addressing import OwnedNodes
from blender.bpy.nodes.changes import made, reconciled
from blender.bpy.nodes.desired import End, Graph, Link, MenuItem, MenuItems, Node
from blender.bpy.nodes.graphs import menu
from blender.bpy.nodes.interfaces import Interface, Socket
from blender.bpy.nodes.trees import endpoints, linked, new_group
from blender.bpy.render.outputs.formats import FileItem, FileItems, OutputFormat
from blender.bpy.render.outputs.paths import OutputPath, today
from blender.bpy.render.outputs.results import Finish, Finishing, Written
from blender.bpy.render.sessions.renders import native, RenderCall
from blender.bpy.render.view_layers.routing import RoutingRow
from blender.bpy.render.view_layers.sources import FileSplit, ids_export
from blender.imaging.files.multilayer import Matte, Multilayer
from blender.records.faults import Nothing, Ok, Outcome, Reason
from blender.rendering.outputs.naming import Output
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_native_file_items_retain_returned_names_and_socket_bindings(acquired: Acquired) -> None:
    """Repeated writes retain an automatically unique native name that cannot be requested verbatim."""
    prefix = "a" * 1019
    request = f"{prefix}_999"
    items = FileItems(items={key: FileItem(name=request, socket_type="RGBA") for key in ("first", "second")})
    with acquired(()) as identity, ExitStack() as release:
        created = new_group(bpy.types.CompositorNodeTree, "File Output bindings")
        assert created.is_ok(), created
        tree = created.ok
        release.callback(bpy.data.node_groups.remove, tree)
        output: bpy.types.CompositorNodeOutputFile = tree.nodes.new(definition(bpy.types.CompositorNodeOutputFile).identifier)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        assert items.write(output, identity) == Ok(None)
        names = {key: item.name for key, item in items.held(output, identity).items()}
        identifiers = items.sockets(output, identity)
        assert names == {"first": request, "second": f"{prefix}_1000"}
        assert tuple(len(name.encode()) for name in names.values()) == (1023, 1024)
        assert len(set(identifiers.values())) == len(items.items)
        assert items.holds(output, identity) == Ok(value=True)
        assert items.write(output, identity) == Ok(None)
        assert {key: item.name for key, item in items.held(output, identity).items()} == names
        assert items.sockets(output, identity) == identifiers
        assert items.holds(output, identity) == Ok(value=True)
        items.held(output, identity)["second"].name = "Edited"
        assert items.holds(output, identity) == Ok(value=False)
        assert items.write(output, identity) == Ok(None)
        assert {key: item.name for key, item in items.held(output, identity).items()} == names
        assert items.sockets(output, identity) == identifiers
        assert items.holds(output, identity) == Ok(value=True)


def test_native_menu_choices_survive_normalization_rename_and_reorder(acquired: Acquired) -> None:
    """Samples distinct literal menu keys and retains their native identities through repeated reconciliation."""
    first = MenuItem(name="A", description="first")
    second = MenuItem(name="A", description="second")
    empty = MenuItem(name="", description="empty")
    cases = {first: 11.0, second: 29.0, empty: 47.0}
    columns = {"sample": np.zeros(1, dtype=np.float32)}
    output = Socket(key="value", kind=bpy.types.NodeSocketFloat)
    node_key = "choice"
    root = menu(second, cases).keyed(node_key)
    with acquired(()) as identity, ExitStack() as release:
        prior = frozenset(bpy.data.node_groups)
        release.callback(lambda: bpy.data.batch_remove(ids=tuple(group for group in bpy.data.node_groups if group not in prior)))
        for selected, expected in cases.items():
            sampled = functions.sampled(identity, columns, menu(selected, cases))
            assert sampled.is_ok(), sampled
            np.testing.assert_array_equal(sampled.ok[0], (expected,))
        defaulted = functions.sampled(identity, columns, menu(None, cases))
        assert defaulted.is_ok(), defaulted
        np.testing.assert_array_equal(defaulted.ok[0], (cases[first],))
        declared = functions.built("native_menu_binding", Interface(outputs=(output,)), {output.key: root}, bpy.types.GeometryNodeTree)
        assert declared.is_ok(), declared
        group = declared.ok
        created = made(group, identity)
        assert created.is_ok(), created
        tree = created.ok
        choice = next(node for node in tree.nodes if isinstance(node, bpy.types.GeometryNodeMenuSwitch))
        choice_outputs: bpy.types.NodeOutputs = choice.outputs  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        identifiers = tuple(socket.identifier for socket in islice(choice_outputs, 1, None))
        names = tuple(item.name for item in choice.enum_items)
        content = MenuItems(items=tuple(cases), selected=second)
        assert content.holds(choice, identity) == Ok(value=True)
        choice.enum_items[1].name = "Renamed"
        assert content.holds(choice, identity) == Ok(value=False)
        restored = reconciled(group, tree, identity)
        assert restored.is_ok(), restored
        assert tuple(item.name for item in choice.enum_items) == names
        assert tuple(socket.identifier for socket in islice(choice_outputs, 1, None)) == identifiers
        assert content.holds(choice, identity) == Ok(value=True)
        sampled = functions.sampled(identity, columns, functions.placed(group, {}, output.key))
        assert sampled.is_ok(), sampled
        np.testing.assert_array_equal(sampled.ok[0], (cases[second],))
        reordered = {empty: cases[first], second: cases[second], first: cases[empty]}
        declared = functions.built(group.key, group.interface, {output.key: menu(empty, reordered).keyed(node_key)}, group.graph.kind)
        assert declared.is_ok(), declared
        group = declared.ok
        changed = reconciled(group, tree, identity)
        assert changed.is_ok(), changed
        assert {socket.identifier for socket in islice(choice_outputs, 1, None)} == set(identifiers)
        sampled = functions.sampled(identity, columns, functions.placed(group, {}, output.key))
        assert sampled.is_ok(), sampled
        np.testing.assert_array_equal(sampled.ok[0], (reordered[empty],))
        repeated = reconciled(group, tree, identity)
        assert repeated.is_ok(), repeated
        assert MenuItems(items=tuple(reordered), selected=empty).holds(choice, identity) == Ok(value=True)
        sampled = functions.sampled(identity, columns, functions.placed(group, {}, output.key))
        assert sampled.is_ok(), sampled
        np.testing.assert_array_equal(sampled.ok[0], (reordered[empty],))


def test_native_file_items_reconnect_replaced_semantic_input(acquired: Acquired) -> None:
    """A changed filename keeps its logical connection on the newly created native socket."""
    key = "capture"
    first = FileItems(items={key: FileItem(name="first", socket_type="RGBA")})
    source = Node(kind=bpy.types.CompositorNodeBlur, inputs={"Image": (1.0, 0.0, 0.0, 1.0)})
    output = Node(kind=bpy.types.CompositorNodeOutputFile, content=Some(first))
    graph = Graph(kind=bpy.types.CompositorNodeTree, nodes={"source": source, "files": output}, links=(Link(source=End(node="source", socket="Image"), target=End(node="files", socket=key)),))
    with acquired(()) as identity, ExitStack() as release:
        created = new_group(bpy.types.CompositorNodeTree, "File Output connection")
        assert created.is_ok(), created
        tree = created.ok
        release.callback(bpy.data.node_groups.remove, tree)
        initial = reconciled(graph, tree, identity)
        assert initial.is_ok(), initial
        native_output = next(node for node in tree.nodes if isinstance(node, bpy.types.CompositorNodeOutputFile))
        identifier = first.sockets(native_output, identity)[key]
        assert endpoints(next(iter(tree.links)))[3].identifier == identifier
        second = FileItems(items={key: FileItem(name="second", socket_type="RGBA")})
        changed_graph = msgspec.structs.replace(graph, nodes={**graph.nodes, "files": msgspec.structs.replace(output, content=Some(second))})
        changed = reconciled(changed_graph, tree, identity)
        assert changed.is_ok(), changed
        replacement = second.sockets(native_output, identity)[key]
        (link,) = tuple(tree.links)
        origin, _, target, inlet = endpoints(link)
        assert isinstance(origin, bpy.types.CompositorNodeBlur)
        assert target == native_output
        assert inlet.identifier == replacement


def test_native_file_items_report_removed_raw_input(acquired: Acquired) -> None:
    """A removed raw native identifier reaches the native absence result after item replacement."""
    first = FileItems(items={key: FileItem(name=key, socket_type="RGBA") for key in ("capture", "retained")})
    output = Node(kind=bpy.types.CompositorNodeOutputFile, content=Some(first))
    origin = End(node="source", socket="Image")
    graph = Graph(
        kind=bpy.types.CompositorNodeTree,
        nodes={"source": Node(kind=bpy.types.CompositorNodeBlur), "files": output},
        links=tuple(Link(source=origin, target=End(node="files", socket=key)) for key in first.items),
    )
    with acquired(()) as identity, ExitStack() as release:
        created = new_group(bpy.types.CompositorNodeTree, "Removed raw input")
        assert created.is_ok(), created
        tree = created.ok
        release.callback(bpy.data.node_groups.remove, tree)
        initial = reconciled(graph, tree, identity)
        assert initial.is_ok(), initial
        native_output = next(node for node in tree.nodes if isinstance(node, bpy.types.CompositorNodeOutputFile))
        identifier = first.sockets(native_output, identity)["capture"]
        second = msgspec.structs.replace(first, items={**first.items, "capture": FileItem(name="replacement", socket_type="RGBA")})
        changed_graph = msgspec.structs.replace(
            graph, nodes={**graph.nodes, "files": msgspec.structs.replace(output, content=Some(second))}, links=(Link(source=origin, target=End(node="files", socket=identifier)), graph.links[1])
        )
        missing = reconciled(changed_graph, tree, identity)
        assert missing.is_error(), missing
        assert any(fault.reason is Reason.ABSENT and fault.member == identifier for fault in missing.error)


def test_native_menu_reorder_retains_multi_input_links(acquired: Acquired) -> None:
    """Reordering menu items preserves surviving links and their slots among an unowned source."""
    first, second = MenuItem(name="A"), MenuItem(name="B")
    menu_spec = Node(kind=bpy.types.GeometryNodeMenuSwitch, members={"data_type": "GEOMETRY"}, content=Some(MenuItems(items=(first, second))))
    graph = Graph(
        kind=bpy.types.GeometryNodeTree,
        nodes={"menu": menu_spec, "joined": Node(kind=bpy.types.GeometryNodeJoinGeometry)},
        links=(Link(source=End(node="menu", socket="Output"), target=End(node="joined", socket="Geometry")),),
    )
    with acquired(()) as identity, ExitStack() as release:
        created = new_group(bpy.types.GeometryNodeTree, "Retained multi-input links")
        assert created.is_ok(), created
        tree = created.ok
        release.callback(bpy.data.node_groups.remove, tree)
        initial = reconciled(graph, tree, identity)
        assert initial.is_ok(), initial
        joined = OwnedNodes.of(tree, identity, graph.contents).nodes["joined"]
        unowned = tree.nodes.new(definition(bpy.types.GeometryNodeMeshCube).identifier)
        added = linked(tree, unowned.outputs[0], joined.inputs["Geometry"], verify_limits=False)
        assert added.is_ok(), added
        before = tuple((link.as_pointer(), link.multi_input_sort_id) for link in tree.links)
        changed_graph = msgspec.structs.replace(graph, nodes={**graph.nodes, "menu": msgspec.structs.replace(menu_spec, content=Some(MenuItems(items=(second, first))))})
        changed = reconciled(changed_graph, tree, identity)
        assert changed.is_ok(), changed
        assert tuple((link.as_pointer(), link.multi_input_sort_id) for link in tree.links) == before


def test_native_cryptomatte_finishing(acquired: Acquired, tmp_path: Path) -> None:
    """Renders both native matte namespace forms and verifies every finishing callback's new output."""
    expected: dict[str, dict[str, tuple[str, tuple[float, float, float]]]] = {}
    seen: list[tuple[str, Written]] = []
    shape, frame = (8, 16), 7
    eyes = {"left": "_eye_A", "right": "_eye_B"}
    mode = ""

    def accepted[T](answer: Outcome[T]) -> T:
        """Requires success from a native operation and returns its retained value."""
        assert answer.is_ok(), answer
        return answer.ok

    def checked(_scene: bpy.types.Scene, file: Written, _identity: Identity) -> Outcome[None]:
        """Reads each native source and its newly finished files before releasing derived test output."""
        original = accepted(Multilayer.read(Path(file.path)))
        mattes = accepted(Matte.found(original, header=next(iter(original.parts.values()))))
        assert file.format.file_format == "OPEN_EXR_MULTILAYER"
        if file.output is Output.PREVIEW:
            assert file.layer == Nothing
            assert file.view == Nothing
            owners = tuple(expected)
        else:
            assert file.layer.is_some()
            owner = file.layer.some
            assert owner in expected
            owners = (owner,)
            if mode == "stereo":
                assert file.view.is_some(), file
                assert Path(file.path).stem.endswith(eyes[file.view.some])
                assert all(matte.view == Nothing for matte in mattes)
            else:
                assert file.view == Nothing
        namespaces = {
            kind if file.layer.is_some() and mode == "bare" else f"{layer}.{kind}": (layer, index, kind, name, color)
            for index, (layer, kinds) in enumerate(expected.items(), 1)
            if layer in owners
            for kind, (name, color) in kinds.items()
        }
        assert {matte.name for matte in mattes} == namespaces.keys()
        if mode == "stereo" and file.output is Output.PREVIEW:
            assert {matte.view.some for matte in mattes} == eyes.keys()
            assert any(0 not in matte.file.parts for matte in mattes)
        assert len(mattes) == len(namespaces) * (len(eyes) if mode == "stereo" and file.output is Output.PREVIEW else 1)
        declared_names = {held for kinds in expected.values() for held, _ in kinds.values()}
        for matte in mattes:
            layer, index, kind, name, color = namespaces[matte.name]
            assert set(matte.ids) & declared_names == {name}
            np.testing.assert_array_equal(accepted(matte.coverage(frozenset({name}))), np.ones(shape, dtype=np.float32))
            eye = matte.view.or_else(file.view).default_value("")
            target = tmp_path / mode / (layer if mode == "bare" else str(index)) / kind / f"{frame:04d}{eyes[eye] if eye else ''}.exr"
            exported = accepted(Multilayer.read(target))
            assert tuple(held.name for held in exported.layers) == (name,)
            pixels = accepted(exported.plane(exported.layers[0], ("R", "G", "B", "A")))
            np.testing.assert_array_equal(pixels, np.broadcast_to(np.asarray((*color, 1.0), dtype=np.float32), (*shape, 4)))
            target.unlink()
        seen.append((mode, file))
        return Ok(None)

    row = RoutingRow()
    finishing = Finishing(rows=(ids_export, Finish(run=checked, outputs=frozenset({Output.PREVIEW, Output.MATTE}), formats=frozenset({"OPEN_EXR_MULTILAYER"}))), files=row.written)
    with acquired((*RoutingRow.prerequisites, finishing)) as identity, ExitStack() as scope:
        preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        scope.callback(setattr, preferences, "is_dirty", preferences.is_dirty)
        scope.callback(setattr, preferences.filepaths, "render_output_directory", preferences.filepaths.render_output_directory)
        preferences.filepaths.render_output_directory = str(tmp_path)
        prior_groups = frozenset(bpy.data.node_groups)
        scope.callback(lambda: bpy.data.batch_remove(ids=tuple(group for group in bpy.data.node_groups if group not in prior_groups)))
        created: list[bpy.types.ID] = []
        scope.callback(lambda: bpy.data.batch_remove(ids=tuple(created)))
        scene = bpy.data.scenes.new("Finishing Cryptomatte")
        created.append(scene)
        accepted(assigned(scene, {("render", "engine"): "CYCLES"}).bind(lambda _: assigned(scene, {("cycles", "device"): "CPU", ("cycles", "samples"): 1, ("cycles", "use_denoising"): False})))
        scene.render.resolution_y, scene.render.resolution_x = shape
        scene.render.resolution_percentage = 100
        scene.render.use_single_layer = False
        scene.render.use_multiview = False
        scene.render.use_compositing = True
        scene.render.compositor_precision = "FULL"
        scene.render.film_transparent = True
        scene.frame_set(frame)
        for view in scene.render.views:
            view.file_suffix = eyes[view.name]
        accepted(accepted(OutputFormat.exr(Output.MATTE, layered=True)).applied(scene.render.image_settings))
        camera_data = bpy.data.cameras.new("Finishing Camera")
        camera = bpy.data.objects.new(camera_data.name, camera_data)
        created.extend((camera, camera_data))
        camera_data.type = "ORTHO"
        camera_data.ortho_scale = 2
        camera.location = (0, 0, 3)
        camera.hide_render = True
        scene.collection.objects.link(camera)
        scene.camera = camera
        collections: dict[str, bpy.types.Collection] = {}
        for layer_name, object_color, material_color in (("A.CryptoObject", (1.0, 0.0, 0.0), (0.0, 0.0, 1.0)), ("A", (0.0, 1.0, 0.0), (1.0, 1.0, 0.0))):
            collection = bpy.data.collections.new(f"Finishing {layer_name}")
            mesh = bpy.data.meshes.new(f"Finishing Mesh {layer_name}")
            material = bpy.data.materials.new(f"Finishing Material {layer_name}")
            item = bpy.data.objects.new(f"Finishing Object {layer_name}", mesh)
            created.extend((collection, mesh, material, item))
            scene.collection.children.link(collection)
            collection.objects.link(item)
            mesh.from_pydata(((-2, -2, 0), (2, -2, 0), (2, 2, 0), (-2, 2, 0)), (), ((0, 1, 2, 3),))
            mesh.materials.append(material)
            item.color = (*object_color, 1.0)  # type: ignore[assignment]
            material.diffuse_color = (*material_color, 1.0)  # type: ignore[assignment]
            collections[layer_name] = collection
            expected[layer_name] = {"CryptoObject": (item.name, object_color), "CryptoMaterial": (material.name, material_color)}
        scene.view_layers[0].name = "A.CryptoObject"
        scene.view_layers.new("A")
        for layer in scene.view_layers:
            layer.use = True
            layer.use_pass_cryptomatte_object = True
            layer.use_pass_cryptomatte_material = True
            layer.pass_cryptomatte_depth = 2
            for name, collection in collections.items():
                layer.layer_collection.children[collection.name].exclude = name != layer.name

        routing = accepted(RoutingRow.bound(identity))
        for layer in scene.view_layers:
            settings = routing.routing.struct(layer)
            settings.split = FileSplit.CLASSES.name
            settings.nuke = False
            for kind in expected[layer.name]:
                item = settings.files.add()
                item.name = f"{kind}00"
                item.output = Output.MATTE.name
        effect = None
        for mode, template in (("bare", "bare/{layer}/{pass}/{frame:4}"), ("prefixed", "prefixed/{order:1}/{pass}/{frame:4}"), ("stereo", "stereo/{order:1}/{pass}/{frame:4}")):
            scene.render.use_multiview = mode == "stereo"
            scene.render.views_format = "STEREO_3D"
            scene.render.image_settings.views_format = "MULTIVIEW"
            outputs = routing.pathed.attached.struct(scene)
            outputs.template = template
            outputs.use_variable = False
            path = accepted(OutputPath.of(outputs, scene, (str(tmp_path), tmp_path), Nothing, today()))
            accepted(routing.pathed.stored.write(scene, path))
            with bpy.context.temp_override(scene=scene):
                accepted(routing.reconciled(scene, bpy.context, Ok(path)))
            held = routing.held(scene)
            assert held.is_some(), held
            tree = held.some
            for node in tree.nodes:
                if isinstance(node, bpy.types.CompositorNodeOutputFile):
                    accepted(assigned(node, {("format", "views_format"): "INDIVIDUAL"}))
            if effect is None:
                effect = scene.compositor_effects.new(name=tree.name)
                effect.node_group = tree
                effect.enable_for_render = True
            scene.render.filepath = str(tmp_path / f"scene_{mode}.exr")
            rendered = accepted(native(identity, scene, RenderCall(write_still=True), files=Some(lambda produced: Ok(Written.scene(produced, ((Path(produced.render.filepath), Nothing),))))))
            assert rendered == frozenset({"FINISHED"})
            observed = tuple(file for observed_mode, file in seen if observed_mode == mode)
            assert sum(file.output is Output.PREVIEW and file.layer == Nothing for file in observed) == 1, seen
            assert sorted(file.layer.some for file in observed if file.output is Output.MATTE) == sorted(tuple(expected) * (len(eyes) if mode == "stereo" else 1)), seen
        removed = scene.view_layers.new("Removed Route")
        removed_name = removed.name
        scene.view_layers.remove(removed)
        export_only = Finishing(rows=(ids_export,), files=row.written)
        for file in (file for written_mode, file in seen if written_mode == "bare" and file.output is Output.MATTE):
            unowned = export_only.finished(scene, (msgspec.structs.replace(file, layer=Nothing),), identity)
            assert unowned.is_error(), unowned
            assert all((fault.reason, fault.subject) == (Reason.ABSENT, file.path) for fault in unowned.error)
            stale = export_only.finished(scene, (msgspec.structs.replace(file, layer=Some(removed_name)),), identity)
            assert stale.is_error(), stale
            assert all((fault.reason, fault.subject, fault.member) == (Reason.ABSENT, repr(scene.view_layers), removed_name) for fault in stale.error)
