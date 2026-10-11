# mypy: disable-error-code="type-arg"
# ty: ignore[missing-type-argument]
"""Native record attachments with value locks, startup seeds, and explicit storage removal."""

from collections import deque
from collections.abc import Callable, Generator, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import StrEnum
from types import MemberDescriptorType
from typing import ClassVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from rasm.blender.extension.events.handlers import PathHandler
from rasm.blender.extension.identity import Identity, Key, Name
from rasm.blender.extension.registration import Acquisitions, Band
from rasm.blender.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderReadFault, BlenderResult
from rasm.blender.settings.properties import Changed, Choices, RecordGroup
from rasm.imaging.faults import accumulated, paired

# --- [TYPES] ----------------------------------------------------------------------------

type Host = bpy.types.ID | bpy.types.Node | bpy.types.ViewLayer


class Slot(StrEnum):
    """Persisted ID-property keys outside the record's RNA field identifiers."""

    HISTORY = ".history"
    LOCKS = ".locks"
    SYNCED = ".synced"
    HELD = ".held"
    DATA = ".data"
    REFERENCES = ".references"


# --- [MODELS] ---------------------------------------------------------------------------


class Stored[T](msgspec.Struct, frozen=True, kw_only=True):
    """Typed MessagePack records and real ID references stored under one native key.

    Extension code zero is reserved for native ID references; caller codecs use other codes.
    """

    reference_code: ClassVar[int] = 0
    name: str
    decoder: msgspec.msgpack.Decoder[T]
    encoder: msgspec.msgpack.Encoder = msgspec.field(default_factory=lambda: msgspec.msgpack.Encoder(order="deterministic"))

    def read(self, host: bpy.types.bpy_struct) -> BlenderResult[Option[T]]:
        """Returns a decoded stored value, explicit absence, or native storage and codec faults."""
        try:
            held = host.get(self.name)
        except (TypeError, ReferenceError) as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))
        if held is None:
            return Ok(Nothing)

        def decoded(kind: type[object], value: object) -> object:
            """Retains native ID answers and delegates other custom types to the caller codec."""
            if issubclass(kind, bpy.types.ID):
                return value
            if self.decoder.dec_hook is None:
                raise TypeError(kind)
            return self.decoder.dec_hook(kind, value)

        try:
            decoder = msgspec.msgpack.Decoder(
                self.decoder.type,
                strict=self.decoder.strict,
                dec_hook=decoded,
                ext_hook=lambda code, data: (
                    held[Slot.REFERENCES][bytes(data).decode()]
                    if code == self.reference_code
                    else self.decoder.ext_hook(code, data)
                    if self.decoder.ext_hook is not None
                    else msgspec.msgpack.Ext(code, data)
                ),
            )
            return Ok(Some(decoder.decode(held[Slot.DATA])))
        except (msgspec.DecodeError, TypeError, KeyError, UnicodeError, NotImplementedError) as error:
            return Error((BlenderReadFault(request=f"{host!r}[{self.name!r}]", error=error),))
        except ReferenceError as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))

    def write(self, host: bpy.types.bpy_struct, record: T) -> BlenderResult[None]:
        """Encodes once and replaces one native group, returning codec or native edit faults."""
        references: dict[str, bpy.types.ID] = {}

        def encoded(value: object) -> object:
            """Stores actual IDs beside their encoded indices and delegates custom values."""
            if isinstance(value, bpy.types.ID):
                key = str(len(references))
                references[key] = value
                return msgspec.msgpack.Ext(self.reference_code, key.encode())
            if self.encoder.enc_hook is None:
                raise TypeError(type(value))
            return self.encoder.enc_hook(value)

        try:
            data = msgspec.msgpack.Encoder(enc_hook=encoded, decimal_format=self.encoder.decimal_format, uuid_format=self.encoder.uuid_format, order=self.encoder.order).encode(record)
            host[self.name] = {Slot.DATA: data, Slot.REFERENCES: references}
        except (msgspec.EncodeError, AttributeError, KeyError, TypeError, ValueError, OverflowError, ReferenceError, NotImplementedError) as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))
        return Ok(None)

    def removed(self, host: bpy.types.bpy_struct) -> BlenderResult[None]:
        """Removes the stored record and its ID references, preserving native refusals."""
        try:
            del host[self.name]
        except KeyError:
            return Ok(None)
        except (AttributeError, TypeError, ReferenceError) as error:
            return Error((BlenderEditFault(request=f"{host!r}[{self.name!r}]", error=error),))
        return Ok(None)


class Attached[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """One registered native pointer and the record binding over its actual target class."""

    name: str
    owner: type[Host]
    group: RecordGroup[T]

    # --- [VALUES]

    def struct(self, host: Host) -> bpy.types.PropertyGroup:
        """Returns the attached native root group of a host."""
        return getattr(host, self.name)  # type: ignore[no-any-return]

    def read(self, host: Host) -> BlenderResult[T]:
        """Reads the attached record, returning its host conversion faults on refusal."""
        return self.group.read(self.struct(host))

    def write(self, host: Host, record: T) -> BlenderResult[None]:
        """Writes independent record leaves, returning every native write refusal."""
        return self.group.write(self.struct(host), record)

    # --- [LOCKS]

    def locks(self, host: Host) -> frozenset[str]:
        """Returns the stored locks belonging to fields of the current record."""
        return Option.of_optional(self.struct(host).get(Slot.LOCKS)).map(lambda held: frozenset(held.keys())).default_value(frozenset()) & frozenset(self.group.record.__struct_fields__)

    def toggled(self, host: Host, key: str) -> None:
        """Flips the lock of one field in native group storage."""
        self.struct(host)[Slot.LOCKS] = dict.fromkeys(self.locks(host) ^ {key}, True)

    @staticmethod
    def kept(locks: frozenset[str], held: T, record: T) -> T:
        """Returns the desired record with each locked field taken from its held value."""
        return msgspec.structs.replace(record, **{key: getattr(held, key) for key in locks})

    def applied(self, host: Host, record: T) -> BlenderResult[None]:
        """Applies a lock-kept preset, returning read and native write refusals."""
        locks = self.locks(host)
        return self.read(host).bind(lambda held: self.write(host, self.kept(locks, held, record)))

    # --- [HOSTS]

    def host(self, group: bpy.types.PropertyGroup) -> Host:
        """Returns the actual host of an attached root group."""
        return next(parent for parent in reversed(group.rna_ancestors()) if isinstance(parent, self.owner))

    def hosts(self) -> tuple[Host, ...]:
        """Returns current file hosts, including embedded IDs, tree nodes, and view layers."""
        pending = deque(block for prop in bpy.types.BlendData.bl_rna.properties if isinstance(prop, bpy.types.CollectionProperty) for block in getattr(bpy.data, prop.identifier))
        blocks: dict[int, bpy.types.ID] = {}
        while pending:
            if (key := (block := (pending.popleft())).as_pointer()) not in blocks:
                blocks[key] = block
                pending.extend(
                    child
                    for prop in block.bl_rna.properties
                    if isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.ID)
                    for child in Option.of_optional(getattr(block, prop.identifier)).to_list()
                    if child.is_embedded_data
                )
        match self.owner:
            case owner if issubclass(owner, bpy.types.ViewLayer):
                return tuple(layer for scene in bpy.data.scenes for layer in scene.view_layers if isinstance(layer, owner))
            case owner if issubclass(owner, bpy.types.Node):
                return tuple(node for tree in blocks.values() if isinstance(tree, bpy.types.NodeTree) for node in tree.nodes if isinstance(node, owner))
            case _:
                return tuple(block for block in blocks.values() if isinstance(block, self.owner))

    # --- [PERSISTENCE]

    def stored(self, host: Host) -> bool:
        """Returns whether the native system-properties store contains this attachment."""
        return host.is_property_set(self.name, ghost=False)

    def seeded(self, record: T) -> BlenderResult[None]:
        """Seeds unstored hosts, returning every native write refusal."""
        return accumulated(self.write(host, record) for host in self.hosts() if not self.stored(host)).map(lambda _: None)

    def purged(self) -> int:
        """Removes persisted values explicitly and returns the number of hosts cleared."""
        hosts = tuple(host for host in self.hosts() if self.stored(host))
        for host in hosts:
            host.property_unset(self.name)
        return len(hosts)


class Attachment[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Native pointer declaration, its fresh class acquisition, and optional startup seed."""

    key: Key
    form: type[T]
    owner: type[Host]
    seed: Option[Callable[[Identity], BlenderResult[T]]] = Nothing
    changed: Option[Callable[[Identity], AbstractContextManager[Changed]]] = Nothing
    bindings: Option[Mapping[MemberDescriptorType, Choices]] = Nothing

    # --- [BINDING]

    def bound(self, identity: Identity) -> BlenderResult[Attached[T]]:
        """Binds to the registered native pointer target, returning absent or name faults."""
        return identity.name(Name.ATTACHMENT, self.key).bind(
            lambda name: (
                Option
                .of_optional(self.owner.bl_rna.properties.get(name))
                .map(lambda prop: type(prop.fixed_type))
                .to_result_with(lambda: (BlenderAbsentFault(owner=self.owner.__name__, key=name),))
                .map(lambda struct: Attached(name=name, owner=self.owner, group=RecordGroup(record=self.form, struct=struct)))
            )
        )

    # --- [REGISTRATION]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds the change callback while yielding fresh native declarations or name faults."""
        with ExitStack() as stack:
            changed = self.changed.map(lambda build: stack.enter_context(build(identity)))
            yield (
                paired(identity.name(Name.ATTACHMENT, self.key), RecordGroup.bound(identity, f"{self.owner.__name__.lower()}_{self.key}", self.form, changed, bindings=self.bindings))
                .map(lambda pair: Attached(name=pair[0], owner=self.owner, group=pair[1]))
                .map(
                    lambda attached: (
                        *((Band.GROUPS, group) for group in (attached.group.struct, *attached.group.nested)),
                        (Band.ATTACHMENTS, pointed(self.owner, attached.name, attached.group.pointer)),
                        *self.seed.map(lambda seed: (PathHandler(handlers=bpy.app.handlers.load_post, body=lambda _: seed(identity).bind(attached.seeded)),)).default_value(()),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                    )
                )
            )


# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def pointed(owner: type[bpy.types.bpy_struct], name: str, definition: bpy.props._PropertyDeferred) -> Generator[BlenderResult[None]]:
    """Acquires one unoccupied native property or returns its collision or declaration refusal."""
    request = f"{owner.__name__}.{name}"
    if name in owner.bl_rna.properties or name in owner.bl_rna.functions or hasattr(owner, name):  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        yield Result[None, BlenderFaults](error=(BlenderCollisionFault(request=request, error=owner.__name__),))
        return
    try:
        setattr(owner, name, definition)
    except (AttributeError, TypeError, ValueError) as error:
        yield Result[None, BlenderFaults](error=(BlenderEditFault(request=request, error=error),))
        return
    try:
        yield Result[None, BlenderFaults](ok=None)
    finally:
        delattr(owner, name)


def preferred[T](identity: Identity, preferences: bpy.types.Preferences, write: Callable[[bpy.types.AddonPreferences], BlenderResult[T]]) -> BlenderResult[T]:
    """Marks attempted preference writes dirty and returns their actual native outcome."""
    match Option.of_optional(preferences.addons.get(identity.package)).bind(lambda addon: Option.of_optional(addon.preferences)):
        case Option(tag="some", some=holder):
            answer = write(holder)  # type: ignore[unreachable]
            preferences.is_dirty = True
            return answer
        case _:
            return Error((BlenderAbsentFault(owner=repr(preferences), key=identity.package),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Attached", "Attachment", "Host", "Slot", "Stored", "pointed", "preferred"]
