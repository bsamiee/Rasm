"""Form groups saved as owned text blocks and held by window manager."""

from functools import partial

import bpy
from expression import Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler, Timer
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import Attached, Attachment
from blender.records.faults import Fault, Outcome, paired, RecordReason

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

    def loaded(self, identity: Identity, session: tuple[Attached[G], bpy.types.WindowManager]) -> Outcome[None]:
        """Applies the snapshot of the owned text block, defaults kept when absent."""
        attached, manager = session

        def decoded(text: bpy.types.Text) -> Outcome[Snapshot]:
            return catch(lambda: msgspec.json.decode(text.as_string(), type=Snapshot), exception=msgspec.DecodeError)().map_error(
                lambda error: Fault.of(RecordReason.OUTSIDE, repr(text), text.name, member=str(error))
            )

        return self.ownership.found(identity).map(lambda text: decoded(text).bind(lambda snapshot: snapshot.apply(attached.struct(manager)).outcome)).default_value(Ok(None))

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns attachment, text ownership, load and save handlers, and startup load timer."""
        attachment, ownership = self.attachment, self.ownership
        title = f"{identity.manifest.name} {bpy.path.display_name(self.key)}"

        def session() -> Outcome[tuple[Attached[G], bpy.types.WindowManager]]:
            """Binds attachment to live window manager."""
            return paired(attachment.bound(identity), rooted(bpy.types.WindowManager, bpy.context))

        def loaded() -> Outcome[Option[float]]:
            """Loads the snapshot once when window manager holds none."""
            return session().bind(lambda pair: Ok(None) if pair[0].stored(pair[1]) else self.loaded(identity, pair)).map(lambda _: Nothing)

        def saved(pair: tuple[Attached[G], bpy.types.WindowManager]) -> Outcome[None]:
            """Saves the window manager group snapshot into owned text block."""
            attached, manager = pair

            def stored(snapshot: Snapshot) -> Outcome[None]:
                return ownership.made(identity, lambda: catch(bpy.data.texts.new, exception=RuntimeError)(title).map_error(partial(Fault.of, BlenderReason.REFUSED, title))).map(
                    lambda text: text.from_string(msgspec.json.encode(snapshot, order="deterministic").decode())
                )

            return Snapshot.capture(attached.struct(manager)).bind(stored)

        return Ok((
            attachment,
            ownership,
            Handler(handlers=bpy.app.handlers.load_post, body=lambda: session().bind(lambda pair: self.loaded(identity, pair))),
            Handler(handlers=bpy.app.handlers.save_pre, body=lambda: session().bind(saved)),
            Timer(body=loaded),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TextRecord"]
