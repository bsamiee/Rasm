"""Form groups saved as owned text blocks and held by window manager."""

from functools import partial
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler, Timer
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import Attached, Attachment
from blender.records.faults import Fault, Outcome, paired, RecordReason

# --- [CONSTANTS] ------------------------------------------------------------------------

_SNAPSHOT: Final = msgspec.json.Decoder(Snapshot)

# --- [MODELS] ---------------------------------------------------------------------------


class TextRecord[G: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Window manager form group saved as a snapshot in an owned fake-user text block."""

    key: Key
    form: type[G]

    # --- [STORAGE]

    @property
    def attachment(self) -> Attachment[G]:
        """Window manager attachment."""
        return Attachment(key=self.key, form=self.form, owner=bpy.types.WindowManager)

    @property
    def ownership(self) -> Ownership[bpy.types.Text]:
        """Text block holding the snapshot, kept by fake user."""
        return Ownership(kind=bpy.types.Text, key=self.key, saved=Saved.ALWAYS)

    def loaded(self, identity: Identity, attached: Attached[G], manager: bpy.types.WindowManager) -> Outcome[None]:
        """Applies the snapshot of the owned text block, defaults kept when absent."""

        def decoded(text: bpy.types.Text) -> Outcome[Snapshot]:
            return catch(_SNAPSHOT.decode, exception=msgspec.DecodeError)(text.as_string()).map_error(lambda error: Fault.of(RecordReason.OUTSIDE, repr(text), text.name, member=str(error)))

        return self.ownership.found(identity).map(lambda text: decoded(text).bind(lambda snapshot: snapshot.apply(attached.struct(manager)).outcome)).default_value(Ok(None))

    def saved(self, identity: Identity, attached: Attached[G], manager: bpy.types.WindowManager) -> Outcome[None]:
        """Saves the window manager group snapshot into the owned text block, created on first save."""
        title = f"{identity.manifest.name} {bpy.path.display_name(self.key)}"

        def stored(snapshot: Snapshot) -> Outcome[None]:
            return self.ownership.made(identity, lambda: catch(bpy.data.texts.new, exception=RuntimeError)(title).map_error(partial(Fault.of, BlenderReason.REFUSED, title))).map(
                lambda text: text.from_string(msgspec.json.encode(snapshot, order="deterministic").decode())
            )

        return Snapshot.capture(attached.struct(manager)).bind(stored)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns attachment, text ownership, load and save handlers, and a startup load for a window manager holding none."""

        def session() -> Outcome[tuple[Attached[G], bpy.types.WindowManager]]:
            return paired(self.attachment.bound(identity), rooted(bpy.types.WindowManager, bpy.context))

        def started() -> Outcome[Option[float]]:
            return (
                session().bind(lambda pair: Some(pair).filter(lambda held: not held[0].stored(held[1])).map(lambda held: self.loaded(identity, *held)).default_value(Ok(None))).map(lambda _: Nothing)
            )

        return Ok((
            self.attachment,
            self.ownership,
            Handler(handlers=bpy.app.handlers.load_post, body=lambda: session().bind(lambda pair: self.loaded(identity, *pair))),
            Handler(handlers=bpy.app.handlers.save_pre, body=lambda: session().bind(lambda pair: self.saved(identity, *pair))),
            Timer(body=started),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TextRecord"]
