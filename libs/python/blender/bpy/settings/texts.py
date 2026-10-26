"""Typed file records saved as owned text blocks and held by window manager."""

import bpy
from expression import Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import vacant
from blender.bpy.data.rna.descriptors import rooted
from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler, SAVE_PRE
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderEditFault, BlenderResult
from blender.bpy.settings.attachments import Attached, Attachment
from blender.imaging.faults import Access, ImagingFileFault, paired
from blender.imaging.parameters import heading

# --- [MODELS] ---------------------------------------------------------------------------


class TextRecord[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """File record in owned fake-user text block and window manager."""

    key: Key
    form: type[T]

    # --- [STORAGE]

    @property
    def attachment(self) -> Attachment[T]:
        """Window manager attachment."""
        return Attachment(key=self.key, form=self.form, owner=bpy.types.WindowManager)

    @property
    def ownership(self) -> Ownership[bpy.types.Text]:
        """Text block holding the file record, kept by fake user."""
        return Ownership(kind=bpy.types.Text, key=self.key, saved=Saved.ALWAYS)

    def loaded(self, identity: Identity, session: tuple[Attached[T], bpy.types.WindowManager]) -> BlenderResult[None]:
        """Loads file record from owned text block, defaults when absent."""
        attached, manager = session

        def decoded(text: bpy.types.Text) -> BlenderResult[T]:
            return catch(lambda: msgspec.json.decode(text.as_string(), type=self.form), exception=msgspec.DecodeError)().map_error(
                lambda error: (ImagingFileFault(access=Access.READ, request=repr(text), error=error),)
            )

        return self.ownership.found(identity).map(decoded).default_with(lambda: Ok(self.form())).bind(lambda value: attached.write(manager, value))

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns attachment, text ownership, load and save handlers, and startup load timer."""
        attachment, ownership = self.attachment, self.ownership
        title = f"{identity.manifest.name} {heading(self.key)}"

        def session() -> BlenderResult[tuple[Attached[T], bpy.types.WindowManager]]:
            """Binds attachment to live window manager."""
            return paired(attachment.bound(identity), rooted(bpy.types.WindowManager, bpy.context))

        def loaded() -> BlenderResult[Option[float]]:
            """Loads record once when window manager holds none."""
            return session().bind(lambda pair: Ok(None) if pair[0].stored(pair[1]) else self.loaded(identity, pair)).map(lambda _: Nothing)

        def saved(pair: tuple[Attached[T], bpy.types.WindowManager]) -> BlenderResult[None]:
            """Saves window manager record into owned text block."""
            attached, manager = pair

            def stored(record: T) -> BlenderResult[None]:
                return ownership.made(identity, lambda: BlenderEditFault.attempted(title, lambda: bpy.data.texts.new(vacant(title, bpy.data.texts.keys())))).bind(
                    lambda text: BlenderEditFault.attempted(repr(text), lambda: text.from_string(msgspec.json.encode(record, order="deterministic").decode()))
                )

            return attached.read(manager).bind(stored)

        return Ok((
            attachment,
            ownership,
            PathHandler(handlers=LOAD_POST, body=lambda _: session().bind(lambda pair: self.loaded(identity, pair))),
            PathHandler(handlers=SAVE_PRE, body=lambda _: session().bind(saved)),
            Timer(body=loaded),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TextRecord"]
