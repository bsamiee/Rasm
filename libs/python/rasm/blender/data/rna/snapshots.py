# mypy: disable-error-code="type-arg"
# ty: ignore[missing-type-argument]
"""RNA value trees, ordered native application, and persistent edit scopes."""

from collections.abc import Callable, Mapping
from functools import reduce
from itertools import count, takewhile
from typing import Annotated, ClassVar, Literal, overload, Protocol, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np

from rasm.blender.data.rna.descriptors import Descriptor, struct_at, Value
from rasm.blender.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult
from rasm.imaging.faults import accumulated
from rasm.imaging.parameters import Case
from rasm.rendering.edits import Owner, revert, transfer, Tree

# --- [TYPES] ----------------------------------------------------------------------------


type Scope = Scenes | ViewLayers | Selection | Objects
type NativeOwner = tuple[bpy.types.ID, str] | bpy.types.bpy_struct | bpy.types.bpy_prop_collection
type NativeValue = tuple[NativeOwner, Value | bytes | bpy.types.bpy_struct | Tree[NativeValue]]


class Scoped[O: "bpy.types.bpy_struct" = "bpy.types.bpy_struct"](Protocol):
    """Owners sharing a native struct type and a context-dependent selection."""

    @property
    def owner_type(self) -> type[O]:
        """Native type of every selected owner."""
        ...

    def owners(self, context: bpy.types.Context, /) -> BlenderResult[tuple[O, ...]]:
        """Returns selected owners in deterministic order, or every missing owner."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

_FLOAT_STORAGE = np.dtype(np.float64).newbyteorder(">")

# --- [TABLES] ---------------------------------------------------------------------------

_LEADING = tuple(
    struct.bl_rna.properties[name]
    for struct, name in (
        (bpy.types.RenderSettings, "engine"),
        (bpy.types.ImageFormatSettings, "color_management"),
        (bpy.types.ColorManagedDisplaySettings, "display_device"),
        (bpy.types.ColorManagedViewSettings, "view_transform"),
        (bpy.types.UnitSettings, "system"),
    )
)


# --- [MODELS] ---------------------------------------------------------------------------


class Applied(msgspec.Struct, frozen=True, kw_only=True):
    """Partial native writes, their outcome, and optional restoration state."""

    written: tuple[str, ...] = ()
    unchanged: tuple[str, ...] = ()
    skipped: tuple[str, ...] = ()
    outcome: BlenderResult[None] = Ok(None)
    restoration: Option[tuple[Tree[NativeValue], BlenderResult[Tree[NativeValue]]]] = Nothing

    @classmethod
    def join(cls, outcomes: tuple["Applied", ...]) -> Self:
        """Concatenates each path category in operation order."""
        return cls(
            written=tuple(path for outcome in outcomes for path in outcome.written),
            unchanged=tuple(path for outcome in outcomes for path in outcome.unchanged),
            skipped=tuple(path for outcome in outcomes for path in outcome.skipped),
            outcome=accumulated(outcome.outcome for outcome in outcomes).map(lambda _: None),
        )


class Snapshot(msgspec.Struct, frozen=True, kw_only=True):
    """Assignable values keyed by RNA paths relative to a native struct."""

    struct: str
    tree: Tree[Value]

    # --- [CAPTURE]

    @classmethod
    def of(cls, struct: "type[bpy.types.bpy_struct]", values: Mapping[str, Value]) -> Self:
        """Returns a stated value set for the native struct's members."""
        return cls(struct=struct.bl_rna.identifier, tree=Tree(roots=("",), owners={"": Owner(values=dict(values))}))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    @classmethod
    @overload
    def capture(
        cls, owner: bpy.types.bpy_struct | bpy.types.bpy_prop_collection, selection: Option[tuple[Descriptor, ...]] = Nothing, prior: "Option[Snapshot]" = Nothing
    ) -> "BlenderResult[Snapshot]": ...

    @classmethod
    @overload
    def capture(
        cls, owner: bpy.types.bpy_struct | bpy.types.bpy_prop_collection, selection: Option[tuple[Descriptor, ...]] = Nothing, prior: Option[Tree[NativeValue]] = Nothing, *, native: Literal[True]
    ) -> BlenderResult[Tree[NativeValue]]: ...

    @classmethod
    def capture(
        cls,
        owner: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection",
        selection: Option[tuple[Descriptor, ...]] = Nothing,
        prior: "Option[Snapshot] | Option[Tree[NativeValue]]" = Nothing,
        *,
        native: bool = False,
    ) -> "BlenderResult[Snapshot | Tree[NativeValue]]":
        """Captures new paths, retaining earliest priors or refusing an incompatible root."""
        try:
            definition: bpy.types.Struct = owner.bl_rna  # type: ignore[assignment, union-attr]  # ty: ignore[invalid-assignment, unresolved-attribute]
        except ReferenceError as error:
            return Error((BlenderEditFault(request=repr(owner), error=error),))
        match prior:
            case Option(tag="some", some=Snapshot() as held) if held.struct != definition.identifier:
                return Error((BlenderChoiceFault(path=repr(owner), identifier=held.struct, accepted=(definition.identifier,)),))

        def reference(struct: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection", path: str) -> NativeOwner:
            if isinstance(struct, bpy.types.bpy_struct):
                block = struct.id_data
                return (block, struct.path_from_id()) if isinstance(block, bpy.types.ID) else struct
            if isinstance(owner, bpy.types.bpy_struct):
                block = owner.id_data
                return (block, f"{owner.path_from_id()}.{path}".removeprefix(".")) if isinstance(block, bpy.types.ID) else struct
            return struct

        def children(struct: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection", key: str, prop: bpy.types.Property, *, indexed: bool = False) -> "tuple[tuple[str, bpy.types.bpy_struct], ...]":
            path = f"{key}.{prop.identifier}".removeprefix(".")
            match prop:
                case bpy.types.PointerProperty(fixed_type=bpy.types.ID()) | bpy.types.CollectionProperty(fixed_type=bpy.types.ID()):
                    return ()  # type: ignore[unreachable]
                case bpy.types.PointerProperty(is_readonly=True):
                    return tuple((path, child) for child in Option.of_optional(getattr(struct, prop.identifier)).to_list())
                case bpy.types.CollectionProperty():
                    items = tuple(getattr(struct, prop.identifier))
                    definition: bpy.types.Struct = prop.fixed_type.bl_rna  # type: ignore[assignment, union-attr]  # ty: ignore[invalid-assignment, unresolved-attribute]
                    names = Option.of_optional(definition.name_property).map(lambda named: tuple(getattr(item, named.identifier) for item in items))
                    keys = (
                        names
                        .filter(lambda found: not indexed and len(set(found)) == len(found))
                        .map(lambda found: tuple(f'["{bpy.utils.escape_identifier(name)}"]' for name in found))
                        .default_with(lambda: tuple(f"[{index}]" for index in range(len(items))))
                    )
                    return tuple((f"{path}{suffix}", item) for suffix, item in zip(keys, items))
                case _:
                    return ()

        def captured[W](
            source: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection",
            owners: dict[str, Owner[W]],
            read: "Callable[[Descriptor, bpy.types.bpy_struct | bpy.types.bpy_prop_collection], Option[W]]",
            chosen: Option[tuple[Descriptor, ...]],
            *,
            live: bool,
            roots: tuple[str, ...] = (),
            expanded: Option[Descriptor] = Nothing,
        ) -> Tree[W]:
            seen: set[tuple[int, str]] = set()

            def walked(struct: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection", path: str) -> Option[str]:
                definition: bpy.types.Struct = struct.bl_rna  # type: ignore[assignment, union-attr]  # ty: ignore[invalid-assignment, unresolved-attribute]
                if (identity := ((struct.as_pointer() if isinstance(struct, bpy.types.bpy_struct) else id(struct), definition.identifier))) in seen:
                    return Nothing
                seen.add(identity)
                target = reference(struct, path)
                key = f"{target[0].session_uid}:{target[1]}" if live and expanded.is_none() and isinstance(target, tuple) else str(identity[0]) if live and expanded.is_none() else path
                members = (*cls.members(definition), *Option.of_optional(definition.name_property).filter(lambda prop: expanded.is_some() and not prop.is_readonly).to_list())
                held = owners.setdefault(key, Owner(values={}))
                values = {
                    prop.identifier: value
                    for prop in members
                    if cls._valued(prop, native=live) and prop.identifier not in held.values and not any(name.startswith(prop.identifier + "[") for name in held.values)
                    for value in read(Descriptor(root=type(struct), owner="", prop=prop), struct).to_list()
                }
                branches = tuple(
                    (
                        prop,
                        owners[child_path].children
                        if isinstance(prop, bpy.types.CollectionProperty) and child_path in owners
                        else tuple(at for child_path, child in children(struct, path, prop, indexed=expanded.is_some()) for at in walked(child, child_path).to_list()),
                    )
                    for prop in members
                    for child_path in (f"{path}.{prop.identifier}".removeprefix("."),)
                    if not (
                        live
                        and (
                            isinstance(prop, bpy.types.CollectionProperty)
                            or (isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.CurveMapping | bpy.types.CurveProfile))
                        )
                    )
                )
                for prop, child_keys in branches:
                    if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID):
                        owners.setdefault(f"{path}.{prop.identifier}".removeprefix("."), Owner(values={}, children=child_keys))
                nested = tuple(
                    at
                    for prop, child_keys in branches
                    for at in ((f"{path}.{prop.identifier}".removeprefix("."),) if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID) else child_keys)
                )
                owners[key] = Owner(values=held.values | values, children=tuple(dict.fromkeys((*held.children, *nested))))
                for prop in members:
                    if (
                        isinstance(prop, bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty)
                        and prop.is_array
                        and any(name.startswith(prop.identifier + "[") for name in held.values)
                    ):
                        for index in range(prop.array_length):
                            picked(Descriptor(root=type(source), owner=path, prop=prop, index=Some(index)))
                return Some(key)

            def picked(descriptor: Descriptor) -> tuple[str, ...]:
                def member(struct: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection") -> tuple[str, ...]:
                    if live and expanded.is_none() and isinstance(source, bpy.types.bpy_struct) and isinstance(struct, bpy.types.bpy_struct):
                        base = source.path_from_id()
                        path = f"{struct.path_from_id()}.{descriptor.prop.identifier}".removeprefix(".")
                        for ancestor in struct.rna_ancestors():
                            definition: bpy.types.Struct = ancestor.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                            enclosing = tuple(
                                prop
                                for prop in cls.members(definition)
                                if (isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID))
                                or (isinstance(prop, bpy.types.PointerProperty) and prop.is_readonly and isinstance(prop.fixed_type, bpy.types.CurveMapping | bpy.types.CurveProfile))
                                for prefix in (ancestor.path_from_id(prop.identifier),)
                                if (not base or prefix.startswith((base + ".", base + "["))) and path.startswith((prefix + ".", prefix + "["))
                            )
                            for prop in enclosing:
                                return picked(Descriptor(root=type(source), owner=ancestor.path_from_id().removeprefix(base).removeprefix("."), prop=prop))
                    if expanded != Some(descriptor) and cls._valued(descriptor.prop, native=live):
                        target = reference(struct, descriptor.owner)
                        key = (
                            f"{target[0].session_uid}:{target[1]}"
                            if live and expanded.is_none() and isinstance(target, tuple)
                            else str(struct.as_pointer() if isinstance(struct, bpy.types.bpy_struct) else id(struct))
                            if live and expanded.is_none()
                            else descriptor.owner
                        )
                        name = descriptor.path.removeprefix(f"{descriptor.owner}." if descriptor.owner else "")
                        if live and any(
                            key.startswith((f"{at}.{name}".replace(":.", ":", 1) + ".", f"{at}.{name}".replace(":.", ":", 1) + "[")) for at, held in owners.items() for name in held.values
                        ):
                            return ()
                        if key in owners and (name in owners[key].values or descriptor.prop.identifier in owners[key].values):
                            return (key,)
                        if (
                            isinstance(descriptor.prop, bpy.types.BoolProperty | bpy.types.IntProperty | bpy.types.FloatProperty)
                            and descriptor.prop.is_array
                            and descriptor.index.is_none()
                            and key in owners
                            and any(name.startswith(descriptor.prop.identifier + "[") for name in owners[key].values)
                        ):
                            return tuple(at for index in range(descriptor.prop.array_length) for at in picked(msgspec.structs.replace(descriptor, index=Some(index))))
                        for value in read(descriptor, struct).to_list():
                            owners.setdefault(key, Owner(values={})).values[name] = value
                            return (key,)
                        return ()
                    nested = (
                        owners[descriptor.path].children
                        if isinstance(descriptor.prop, bpy.types.CollectionProperty) and descriptor.path in owners
                        else tuple(at for path, child in children(struct, descriptor.owner, descriptor.prop, indexed=expanded.is_some()) for at in walked(child, path).to_list())
                    )
                    if expanded == Some(descriptor) or isinstance(descriptor.prop, bpy.types.CollectionProperty):
                        if isinstance(descriptor.prop, bpy.types.CollectionProperty):
                            owners.setdefault(descriptor.path, Owner(values={}, children=nested))
                            nested = (descriptor.path,)
                        held = owners.setdefault(descriptor.owner, Owner(values={}))
                        owners[descriptor.owner] = Owner(values=held.values, children=tuple(dict.fromkeys((*held.children, *nested))))
                        return (descriptor.owner,)
                    return nested

                return descriptor.owner_of(source).map(member).default_value(())

            roots = (*roots, *chosen.map(lambda selected: tuple(key for descriptor in selected for key in picked(descriptor))).default_with(lambda: tuple(walked(source, "").to_list())))
            nested = frozenset(child for held in owners.values() for child in held.children)
            return Tree(roots=tuple(dict.fromkeys(key for key in roots if key not in nested)), owners=owners)

        def stored(descriptor: Descriptor, struct: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection") -> Option[Value]:
            value = descriptor.read(struct)
            return Nothing if isinstance(descriptor.prop, bpy.types.EnumProperty) and not descriptor.prop.is_enum_flag and not bool(value) else Some(value)

        def actual(descriptor: Descriptor, struct: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection") -> Option[NativeValue]:
            if isinstance(descriptor.prop, bpy.types.CollectionProperty) or (
                isinstance(descriptor.prop, bpy.types.PointerProperty) and descriptor.prop.is_readonly and isinstance(descriptor.prop.fixed_type, bpy.types.CurveMapping | bpy.types.CurveProfile)
            ):
                selected = Descriptor(root=type(struct), owner="", prop=descriptor.prop)
                return Some((reference(struct, descriptor.owner), captured(struct, {}, actual, Some((selected,)), live=True, expanded=Some(selected))))
            value = getattr(struct, descriptor.prop.identifier) if isinstance(descriptor.prop, bpy.types.PointerProperty) else descriptor.read(struct)
            return Some((reference(struct, descriptor.owner), np.asarray(value, dtype=_FLOAT_STORAGE).tobytes() if isinstance(descriptor.prop, bpy.types.FloatProperty) else value))

        try:
            match prior:
                case Option(tag="some", some=Tree() as tree):
                    return Ok(captured(owner, {key: Owner(values=dict(row.values), children=row.children) for key, row in tree.owners.items()}, actual, selection, live=True, roots=tree.roots))
                case Option(tag="some", some=Snapshot(tree=tree)):
                    return Ok(
                        Snapshot(
                            struct=definition.identifier,
                            tree=captured(owner, {key: Owner(values=dict(row.values), children=row.children) for key, row in tree.owners.items()}, stored, selection, live=False, roots=tree.roots),
                        )
                    )
                case Option():
                    return Ok(captured(owner, {}, actual, selection, live=True)) if native else Ok(Snapshot(struct=definition.identifier, tree=captured(owner, {}, stored, selection, live=False)))
        except msgspec.ValidationError as error:
            return Error((BlenderReadFault(request=repr(owner), error=error),))
        except ReferenceError as error:
            return Error((BlenderEditFault(request=repr(owner), error=error),))

    @classmethod
    def settings(cls, setting: Descriptor, compound: "tuple[type[bpy.types.bpy_struct], ...]" = ()) -> tuple[Descriptor, ...]:
        """Expands assignable leaves, retaining the caller's compound native pointers."""

        def walked(struct: bpy.types.Struct, owner: str, entered: frozenset[str]) -> tuple[Descriptor, ...]:
            def member(prop: bpy.types.Property) -> tuple[Descriptor, ...]:
                match prop:
                    case _ if cls._valued(prop) or (isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, compound)):
                        return (Descriptor(root=setting.root, owner=owner, prop=prop),)
                    case bpy.types.PointerProperty(is_readonly=True, fixed_type=pointed) if not isinstance(pointed, bpy.types.ID):
                        definition: bpy.types.Struct = pointed.bl_rna  # type: ignore[assignment, union-attr]  # ty: ignore[invalid-assignment, unresolved-attribute]
                        return walked(definition, f"{owner}.{prop.identifier}".removeprefix("."), entered | {definition.identifier}) if definition.identifier not in entered else ()
                    case _:
                        return ()

            return tuple(descriptor for prop in cls.members(struct) for descriptor in member(prop))

        match setting.prop:
            case bpy.types.PointerProperty(is_readonly=True, fixed_type=pointed) if not isinstance(pointed, (bpy.types.ID, *compound)):
                definition: bpy.types.Struct = pointed.bl_rna  # type: ignore[assignment, union-attr]  # ty: ignore[invalid-assignment, unresolved-attribute]
                return walked(definition, setting.path, frozenset({definition.identifier}))
            case _:
                return (setting,)

    @staticmethod
    def members(struct: bpy.types.Struct) -> tuple[bpy.types.Property, ...]:
        """Returns persistent settings definitions, excluding identity and registration."""
        base = bpy.types.ID.bl_rna.properties.values()
        return tuple(
            prop
            for prop in struct.properties
            if not (prop.is_hidden or prop.is_skip_save or prop.is_deprecated or prop.is_registered or prop.is_registered_optional or prop == struct.name_property or prop in base)
        )

    @staticmethod
    def _valued(prop: bpy.types.Property, *, native: bool = False) -> bool:
        """Returns stored leaves, retaining live pointers and compounds for native recovery."""
        match prop:
            case bpy.types.CollectionProperty():
                return native and not isinstance(prop.fixed_type, bpy.types.ID)
            case bpy.types.PointerProperty(is_readonly=True):
                return native and isinstance(prop.fixed_type, bpy.types.CurveMapping | bpy.types.CurveProfile)
            case bpy.types.PointerProperty():
                return native or isinstance(prop.fixed_type, bpy.types.ID)
            case _:
                return not prop.is_readonly

    # --- [APPLICATION]

    def apply(self, owner: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection", *, restoration: Option[Callable[[Tree[NativeValue]], BlenderResult[None]]] = Nothing) -> Applied:
        """Returns partial writes and faults, optionally capturing actual restoration state."""
        try:
            definition: bpy.types.Struct = owner.bl_rna  # type: ignore[assignment, union-attr]  # ty: ignore[invalid-assignment, unresolved-attribute]
        except ReferenceError as error:
            return Applied(outcome=Error((BlenderEditFault(request=repr(owner), error=error),)))
        if definition.identifier != self.struct:
            return Applied(outcome=Error((BlenderChoiceFault(path=repr(owner), identifier=self.struct, accepted=(definition.identifier,)),)))
        selected = (*(Descriptor(root=type(owner), owner="", prop=prop) for prop in self.members(definition)), *self.selection(owner)) if restoration.is_some() else ()
        prepared = restoration.map(lambda publish: self.capture(owner, Some(selected), native=True).map(lambda before: (Some(before), publish(before)))).default_value(Ok((Nothing, Ok(None))))
        match prepared:
            case Result(tag="ok", ok=(before, publication)):
                match publication:
                    case Result(tag="ok"):
                        pass
                    case Result(error=faults):
                        return Applied(outcome=Error(faults), restoration=before.map(lambda held: (held, Ok(held))))
            case Result(error=faults):
                return Applied(outcome=Error(faults))
        applied = self._applied(owner, self.tree, lambda _, value: value)
        return msgspec.structs.replace(applied, restoration=before.map(lambda held: (held, self.capture(owner, Some(selected), native=True))))

    def selection(self, owner: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection") -> tuple[Descriptor, ...]:
        """Resolves this sparse assignment's native members and compound roots."""
        return (
            *(
                descriptor
                for key in self.tree.owners
                if key
                for descriptor in Descriptor.resolve(owner, key, bpy.types.Property).to_option().to_list()
                if isinstance(descriptor.prop, bpy.types.PointerProperty | bpy.types.CollectionProperty)
            ),
            *(
                descriptor
                for key, held in self.tree.owners.items()
                for name in held.values
                for descriptor in Descriptor.resolve(owner, f"{key}.{name}".removeprefix("."), bpy.types.Property).to_option().to_list()
            ),
        )

    def projected(self, owner: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection", current: "Snapshot", locked: frozenset[str] = frozenset()) -> "BlenderResult[tuple[Snapshot, Snapshot]]":
        """Returns unlocked sparse assignments and their complete matching state."""
        try:
            definition: bpy.types.Struct = owner.bl_rna  # type: ignore[assignment, union-attr]  # ty: ignore[invalid-assignment, unresolved-attribute]
            incompatible = tuple(
                BlenderChoiceFault(path=repr(owner), identifier=snapshot.struct, accepted=(definition.identifier,)) for snapshot in (self, current) if snapshot.struct != definition.identifier
            )
            if incompatible:
                return Result[tuple[Snapshot, Snapshot], BlenderFaults](error=(incompatible[0], *incompatible[1:]))

            def within(path: str, roots: frozenset[str]) -> bool:
                return any(path == root or path.startswith((root + ".", root + "[")) for root in roots)

            free = {
                key: Owner(
                    values={name: value for name, value in held.values.items() if not within(f"{key}.{name}".removeprefix("."), locked)},
                    children=tuple(child for child in held.children if not within(child, locked)),
                )
                for key, held in self.tree.owners.items()
                if not within(key, locked)
            }
            compounds = frozenset(
                key
                for key in free
                if key
                for descriptor in Descriptor.resolve(owner, key, bpy.types.Property).to_option().to_list()
                if isinstance(descriptor.prop, bpy.types.CollectionProperty)
                or (isinstance(descriptor.prop, bpy.types.PointerProperty) and isinstance(descriptor.prop.fixed_type, bpy.types.CurveMapping | bpy.types.CurveProfile))
            )
            values = {key: held for key, held in current.tree.owners.items() if not within(key, compounds)}
            for key, held in free.items():
                previous = values.get(key, Owner(values={}))
                values[key] = held if within(key, compounds) else Owner(values=previous.values | held.values, children=tuple(dict.fromkeys((*previous.children, *held.children))))
            nested = frozenset(child for held in values.values() for child in held.children)
            sparse = Snapshot(struct=self.struct, tree=Tree(roots=tuple(key for key in self.tree.roots if key in free), owners=free))
            matching = Snapshot(struct=self.struct, tree=Tree(roots=tuple(dict.fromkeys(key for key in (*current.tree.roots, *sparse.tree.roots) if key not in nested)), owners=values))
            return Ok((sparse, matching))
        except ReferenceError as error:
            return Error((BlenderEditFault(request=repr(owner), error=error),))

    @classmethod
    def _applied[W](
        cls, owner: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection", tree: Tree[W], projected: "Callable[[Descriptor, W], Value | bytes | bpy.types.bpy_struct | Tree[NativeValue]]"
    ) -> Applied:
        """Interprets persisted or native values through the same ordered RNA mutation."""
        reports: list[Applied] = []
        completed: set[tuple[str, str]] = set()

        def member(struct: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection", key: str, descriptor: Descriptor, value: W) -> BlenderResult[Applied]:
            path = f"{key}.{descriptor.path}".removeprefix(".")
            desired = projected(descriptor, value)
            if isinstance(desired, Tree):
                report = cls._applied(struct, desired, lambda found, row: cls._value(found, row[1]))
                reports.append(report)
                return report.outcome.map(lambda _: report)
            answer = descriptor.write(struct, desired)
            match answer:
                case Result(tag="ok", ok=prior):
                    report = Applied(written=(path,)) if prior.is_some() else Applied(unchanged=(path,))
                case Result(error=faults):
                    report = Applied(outcome=Error(faults))
            reports.append(report)
            return answer.map(lambda _: report)

        def lead(prop: bpy.types.Property) -> BlenderResult[Applied]:
            steps = tuple(
                (struct, key, name, value)
                for key, held in tree.owners.items()
                for struct in struct_at(owner, key).to_list()
                for name, value in held.values.items()
                if struct.bl_rna.properties.get(name) == prop  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            )
            completed.update((key, name) for _, key, name, _ in steps)
            return accumulated(member(struct, key, Descriptor(root=type(struct), owner="", prop=prop), value) for struct, key, _, value in steps).map(Applied.join)

        def resized(struct: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection", key: str, prop: bpy.types.CollectionProperty) -> BlenderResult[Applied]:
            path = f"{key}.{prop.identifier}".removeprefix(".")
            if (
                not (keys := (tree.owners[path].children if path in tree.owners else tuple(f"{path}[{index}]" for index in takewhile(lambda index: f"{path}[{index}]" in tree.owners, count()))))
                and path not in tree.owners
            ):
                return Ok(Applied())
            items = getattr(struct, prop.identifier)
            fixed: bpy.types.bpy_struct = prop.fixed_type  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            added: Callable[[dict[str, Value | bytes | bpy.types.bpy_struct | Tree[NativeValue]]], object]
            match fixed:
                case bpy.types.CurveMapPoint():
                    added = lambda values: items.new(*np.asarray(values["location"], dtype=np.float64))
                case bpy.types.CurveProfilePoint():
                    added = lambda values: items.add(*np.asarray(values["location"], dtype=np.float64))
                case bpy.types.ColorRampElement():
                    added = lambda values: items.new(values["position"])
                case bpy.types.PropertyGroup() if prop.is_runtime:
                    added = lambda _: items.add()
                case bpy.types.bpy_struct():
                    return Ok(Applied()) if len(items) == len(keys) else Error((BlenderChoiceFault(path=f"{struct!r}.{prop.identifier}", identifier=str(len(keys)), accepted=(str(len(items)),)),))

            def changed() -> Applied:
                definition: bpy.types.Struct = fixed.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                for at in keys[len(items) :]:
                    values = {
                        name: projected(Descriptor(root=type(fixed), owner="", prop=definition.properties[name]), value)
                        for name, value in tree.owners[at].values.items()
                        if name in definition.properties
                    }
                    added(values)
                for _ in range(len(items) - len(keys)):
                    items.remove(len(items) - 1 if isinstance(fixed, bpy.types.PropertyGroup) and prop.is_runtime else items[-1])
                return Applied()

            try:
                return Ok(changed())
            except (RuntimeError, TypeError, ValueError, KeyError) as error:
                return Error((BlenderEditFault(request=f"{struct!r}.{prop.identifier}", error=error),))

        def located(key: str) -> BlenderResult[Applied]:
            def written(struct: "bpy.types.bpy_struct | bpy.types.bpy_prop_collection") -> BlenderResult[Applied]:
                definitions = tuple(struct.bl_rna.properties)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                members = tuple((name, value, Descriptor.resolve(struct, name, bpy.types.Property).to_option()) for name, value in tree.owners[key].values.items() if (key, name) not in completed)
                missing = Applied(skipped=tuple(f"{key}.{name}".removeprefix(".") for name, _, descriptor in members if descriptor.is_none()))
                reports.append(missing)
                ordered = sorted(
                    ((descriptor, value) for _, value, found in members for descriptor in found.to_list()),
                    key=lambda item: (_LEADING.index(item[0].prop) if item[0].prop in _LEADING else len(_LEADING), definitions.index(item[0].prop), item[0].index.is_some()),
                )
                values = accumulated(member(struct, key, descriptor, value) for descriptor, value in ordered).map(lambda outcomes: Applied.join((missing, *outcomes)))
                result = values.bind(
                    lambda applied: accumulated(
                        resized(struct, key, prop) for prop in definitions if isinstance(prop, bpy.types.CollectionProperty) and not isinstance(prop.fixed_type, bpy.types.ID)
                    ).map(lambda outcomes: Applied.join((applied, *outcomes)))
                ).bind(lambda applied: accumulated(located(child) for child in tree.owners[key].children).map(lambda outcomes: Applied.join((applied, *outcomes))))
                if isinstance(struct, bpy.types.CurveMapping | bpy.types.CurveProfile):
                    struct.update()
                return result

            match struct_at(owner, key):
                case Option(tag="some", some=struct):
                    return written(struct)
                case Option():
                    report = Applied(skipped=(key,))
                    reports.append(report)
                    return Ok(report)

        try:
            outcome = (
                reduce(lambda state, prop: state.bind(lambda applied: lead(prop).map(lambda outcome: Applied.join((applied, outcome)))), _LEADING, Result[Applied, BlenderFaults](ok=Applied()))
                .bind(lambda applied: accumulated(located(key) for key in tree.roots).map(lambda outcomes: Applied.join((applied, *outcomes))))
                .map(lambda _: None)
            )
        except ReferenceError as error:
            outcome = Error((BlenderEditFault(request=repr(owner), error=error),))
        return msgspec.structs.replace(Applied.join(tuple(reports)), outcome=outcome)

    @overload
    def apply_all[O: bpy.types.bpy_struct](
        self, scope: Scoped[O], context: bpy.types.Context, /, *, restoration: Option[Callable[[O, Tree[NativeValue]], BlenderResult[None]]] = Nothing
    ) -> BlenderResult[tuple[Applied, ...]]: ...

    @overload
    def apply_all[O: bpy.types.bpy_struct](
        self, scope: tuple[O, ...], /, *, restoration: Option[Callable[[O, Tree[NativeValue]], BlenderResult[None]]] = Nothing
    ) -> BlenderResult[tuple[Applied, ...]]: ...

    def apply_all[O: bpy.types.bpy_struct](
        self, scope: Scoped[O] | tuple[O, ...], /, *context: bpy.types.Context, restoration: Option[Callable[[O, Tree[NativeValue]], BlenderResult[None]]] = Nothing
    ) -> BlenderResult[tuple[Applied, ...]]:
        """Applies to every selected owner, retaining scope and independent write faults."""

        def applied(owners: tuple[O, ...]) -> tuple[Applied, ...]:
            match restoration:
                case Option(tag="some", some=publish):
                    prepared = []
                    for owner in owners:
                        definition: bpy.types.Struct = owner.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                        selected = (*(Descriptor(root=type(owner), owner="", prop=prop) for prop in self.members(definition)), *self.selection(owner))
                        captured = self.capture(owner, Some(selected), native=True)
                        prepared.append((owner, selected, captured, captured.bind(lambda before: publish(owner, before))))
                    outcomes = tuple(publication.map(lambda _: self.apply(owner)).default_with(lambda faults: Applied(outcome=Error(faults))) for owner, _, _, publication in prepared)
                    return tuple(
                        msgspec.structs.replace(outcome, restoration=captured.to_option().map(lambda before: (before, self.capture(owner, Some(selected), native=True))))
                        for (owner, selected, captured, _), outcome in zip(prepared, outcomes)
                    )
                case Option():
                    return tuple(self.apply(owner) for owner in owners)

        return Ok(applied(scope)) if isinstance(scope, tuple) else scope.owners(*context).map(applied)

    @staticmethod
    def persisted(tree: Tree[NativeValue]) -> BlenderResult[Tree[tuple[tuple[object, str], object]]]:
        """Projects native recovery to the shared ID-reference store's codec surface."""

        def member(row: NativeValue) -> BlenderResult[tuple[tuple[object, str], object]]:
            target, value = row
            if not isinstance(target, tuple):
                return Error((BlenderChoiceFault(path=repr(target), identifier=type(target).__name__, accepted=(bpy.types.ID.__name__,)),))
            if isinstance(value, Tree):
                return Snapshot.persisted(value).map(lambda held: (target, held))
            if isinstance(value, bpy.types.bpy_struct) and not isinstance(value, bpy.types.ID):
                return Error((BlenderChoiceFault(path=repr(target), identifier=type(value).__name__, accepted=(bpy.types.ID.__name__,)),))
            return Ok((target, value))

        return accumulated(
            accumulated(member(value).map(lambda row: (name, row)) for name, value in held.values.items()).map(lambda values: (key, Owner(values=dict(values), children=held.children)))
            for key, held in tree.owners.items()
        ).map(lambda owners: Tree(roots=tree.roots, owners=dict(owners)))

    @staticmethod
    def decoded(tree: Tree[tuple[tuple[object, str], object]], *, compound: bool = False) -> BlenderResult[Tree[NativeValue]]:
        """Admits stored native references and reconstructs keys from their current IDs."""
        owners: dict[str, Owner[NativeValue]] = {}

        def member(key: str, name: str, row: tuple[tuple[object, str], object]) -> BlenderResult[None]:
            (block, path), value = row
            if block is None:
                return Ok(None)
            if not isinstance(block, bpy.types.ID):
                return Error((BlenderChoiceFault(path=path, identifier=type(block).__name__, accepted=(bpy.types.ID.__name__,)),))
            descriptors = (
                Descriptor.resolve(type(block), f"{path}.{name}".removeprefix("."), bpy.types.Property).to_option()
                if compound
                else struct_at(block, path).bind(lambda struct: Descriptor.resolve(struct, name, bpy.types.Property).to_option())
            )
            for descriptor in descriptors.to_list():
                target: NativeOwner = (block, path)
                at = key if compound else f"{block.session_uid}:{path}"
                answer: BlenderResult[Value | bytes | bpy.types.bpy_struct | Tree[NativeValue]]
                if isinstance(descriptor.prop, bpy.types.CollectionProperty) or (isinstance(descriptor.prop, bpy.types.PointerProperty) and descriptor.prop.is_readonly):
                    answer = Snapshot.decoded(msgspec.convert(value, type=Tree[tuple[tuple[object, str], object]]), compound=True)
                elif isinstance(descriptor.prop, bpy.types.PointerProperty):
                    if value is not None and not isinstance(value, bpy.types.ID):
                        return Error((BlenderChoiceFault(path=f"{path}.{name}", identifier=type(value).__name__, accepted=(bpy.types.ID.__name__,)),))
                    answer = Ok(value)
                elif isinstance(descriptor.prop, bpy.types.FloatProperty):
                    size = (descriptor.prop.array_length if descriptor.prop.is_array and descriptor.index.is_none() else 1) * _FLOAT_STORAGE.itemsize
                    answer = Ok(msgspec.convert(value, Annotated[bytes, msgspec.Meta(min_length=size, max_length=size)]))
                else:
                    answer = Ok(msgspec.convert(value, Value))
                match answer:
                    case Result(tag="ok", ok=native):
                        owners.setdefault(at, Owner(values={}, children=tree.owners[key].children if compound else ())).values[name] = (target, native)
                    case Result(error=faults):
                        return Error(faults)
            return Ok(None)

        try:
            for key, held in tree.owners.items():
                if compound:
                    owners[key] = Owner(values={}, children=held.children)
            return accumulated(member(key, name, row) for key, held in tree.owners.items() for name, row in held.values.items()).map(
                lambda _: Tree(roots=tree.roots if compound else tuple(owners), owners=owners)
            )
        except (msgspec.ValidationError, TypeError) as error:
            return Error((BlenderReadFault(request=repr(tree), error=error),))
        except ReferenceError as error:
            return Error((BlenderEditFault(request=repr(tree), error=error),))

    @staticmethod
    def restore(before: Tree[NativeValue], after: Tree[NativeValue]) -> Applied:
        """Restores live values still equal to this operation's actual final state."""
        current: dict[str, Owner[NativeValue]] = {}
        reports: list[Applied] = []
        for key, held in before.owners.items():
            for name, (target, _) in held.values.items():
                if key not in after.owners or name not in after.owners[key].values:
                    try:
                        for struct in (struct_at(*target) if isinstance(target, tuple) else Some(target)).to_list():
                            reports.extend(
                                Applied(outcome=Error((BlenderAbsentFault(owner=repr(struct), key=descriptor.path),)))
                                for descriptor in Descriptor.resolve(struct, name, bpy.types.Property).to_option().to_list()
                            )
                    except ReferenceError:
                        reports.append(Applied(skipped=(f"{key}.{name}",)))
        for key, held in after.owners.items():
            values: dict[str, NativeValue] = {}
            for name, (target, _) in held.values.items():
                try:
                    for struct in (struct_at(*target) if isinstance(target, tuple) else Some(target)).to_list():
                        for descriptor in Descriptor.resolve(struct, name, bpy.types.Property).to_option().to_list():
                            match Snapshot.capture(struct, Some((descriptor,)), native=True):
                                case Result(tag="ok", ok=tree):
                                    values[name] = next(row for held in tree.owners.values() for row in held.values.values())
                                case Result(error=faults):
                                    reports.append(Applied(outcome=Error(faults)))
                except ReferenceError:
                    reports.append(Applied(skipped=(f"{key}.{name}",)))
            current[key] = Owner(values=values, children=held.children)
        edits = revert(Tree(roots=after.roots, owners=current), transfer(before, after))
        assignments: list[tuple[str, NativeOwner, Descriptor, Value | bytes | bpy.types.bpy_struct | Tree[NativeValue]]] = []
        for edit in edits:
            for name, (target, value) in edit.after.items():
                path = f"{edit.key}.{name}"
                for struct in (struct_at(*target) if isinstance(target, tuple) else Some(target)).to_list():
                    assignments.extend((path, target, descriptor, value) for descriptor in Descriptor.resolve(struct, name, bpy.types.Property).to_option().to_list())
        ordered = sorted(assignments, key=lambda item: (_LEADING.index(item[2].prop) if item[2].prop in _LEADING else len(_LEADING), item[0], item[2].index.is_some()))
        updated = {}
        for path, target, descriptor, value in ordered:
            for struct in (struct_at(*target) if isinstance(target, tuple) else Some(target)).to_list():
                if isinstance(value, Tree):
                    reports.append(Snapshot._applied(struct, value, lambda found, row: Snapshot._value(found, row[1])))
                else:
                    match Descriptor.resolve(struct, descriptor.path, bpy.types.Property).bind(lambda found: found.write(struct, Snapshot._value(found, value))):
                        case Result(tag="ok", ok=prior):
                            reports.append(Applied(written=(path,)) if prior.is_some() else Applied(unchanged=(path,)))
                        case Result(error=faults):
                            reports.append(Applied(outcome=Error(faults)))
                if isinstance(struct, bpy.types.CurveMapping | bpy.types.CurveProfile):
                    updated[struct.as_pointer()] = struct
        for struct in updated.values():
            try:
                struct.update()
            except ReferenceError as error:
                reports.append(Applied(outcome=Error((BlenderEditFault(request=repr(struct), error=error),))))
        return Applied.join(tuple(reports))

    @staticmethod
    def _value[V](descriptor: Descriptor, value: V) -> float | tuple[float, ...] | V:
        """Projects exact captured float bits through their native scalar or array metadata."""
        match descriptor.prop, value:
            case bpy.types.FloatProperty() as prop, bytes() as bits:
                values = np.frombuffer(bits, dtype=_FLOAT_STORAGE)
                return tuple(values.tolist()) if prop.is_array and descriptor.index.is_none() else float(values[0])
            case _:
                return value


class Scenes(Case, frozen=True, kw_only=True, tag=1):
    """Named scenes, or every nonruntime scene in native file order."""

    title = "Scenes"
    owner_type: ClassVar[type[bpy.types.Scene]] = bpy.types.Scene
    names: tuple[str, ...] | None = None

    def owners(self, _context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Scene, ...]]:
        """Returns named scenes or nonruntime scenes, accumulating missing names."""
        return (
            Option
            .of_optional(self.names)
            .map(lambda names: accumulated(Option.of_optional(bpy.data.scenes.get(name)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.data.scenes), key=name),)) for name in names))
            .default_with(lambda: Ok(tuple(scene for scene in bpy.data.scenes if not scene.is_runtime_data)))
        )


class ViewLayers(Case, frozen=True, kw_only=True, tag=2):
    """Every view layer belonging to the selected scenes."""

    title = "View Layers"
    owner_type: ClassVar[type[bpy.types.ViewLayer]] = bpy.types.ViewLayer
    scenes: tuple[str, ...] | None = None

    def owners(self, context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.ViewLayer, ...]]:
        """Returns view layers in scene and layer order, or all missing scene names."""
        return Scenes(names=self.scenes).owners(context).map(lambda owners: tuple(layer for scene in owners for layer in scene.view_layers))


class Selection(Case, frozen=True, kw_only=True, tag=3):
    """Objects selected in the context's view layer."""

    title = "Selection"
    owner_type: ClassVar[type[bpy.types.Object]] = bpy.types.Object

    @staticmethod
    def owners(context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Object, ...]]:
        """Returns selected objects in native view-layer order."""
        return Ok(tuple(context.view_layer.objects.selected))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


class Objects(Case, frozen=True, kw_only=True, tag=4):
    """Objects belonging to a named collection and its descendants."""

    title = "Collection Objects"
    owner_type: ClassVar[type[bpy.types.Object]] = bpy.types.Object
    collection: str

    def owners(self, _context: bpy.types.Context) -> BlenderResult[tuple[bpy.types.Object, ...]]:
        """Returns all collection objects, or the absent collection name."""
        return (
            Option
            .of_optional(bpy.data.collections.get(self.collection))
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.data.collections), key=self.collection),))
            .map(lambda collection: tuple(collection.all_objects))
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Applied", "NativeValue", "Objects", "Scenes", "Scope", "Scoped", "Selection", "Snapshot", "ViewLayers"]
