"""Typed file records persisted as native text blocks and held by the window manager."""

import bpy
from expression import Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from rasm.blender.extension.events.handlers import PathHandler
from rasm.blender.extension.events.timers import Timer
from rasm.blender.extension.identity import Identity, Key
from rasm.blender.extension.registration import Acquisitions
from rasm.blender.faults import BlenderReadFault, BlenderResult
from rasm.blender.settings.attachments import Attached, Attachment

# --- [MODELS] ---------------------------------------------------------------------------


class TextRecord[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """File-level record held in a fake-user text and the live window manager."""

    key: Key
    form: type[T]

    # --- [STORAGE]

    @property
    def attachment(self) -> Attachment[T]:
        """Native session attachment of this file record."""
        return Attachment(key=self.key, form=self.form, owner=bpy.types.WindowManager)

    def loaded(self, attached: Attached[T]) -> BlenderResult[None]:
        """Loads the file record, returning decode or native session-write refusals."""
        held: Option[bpy.types.Text] = Option.of_optional(bpy.data.texts.get(attached.name))
        record: BlenderResult[T] = held.map(
            lambda text: catch(lambda: msgspec.json.decode(text.as_string(), type=self.form), exception=msgspec.DecodeError)().map_error(
                lambda error: (BlenderReadFault(request=repr(text), error=error),)
            )
        ).default_with(lambda: Ok(self.form()))
        return record.bind(lambda value: attached.write(bpy.context.window_manager, value))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    def saved(self, attached: Attached[T]) -> BlenderResult[None]:
        """Persists actual session values in the file or returns native read faults."""

        def stored(record: T) -> None:
            """Writes a fake-user text from the single native record read."""
            text: bpy.types.Text = Option.of_optional(bpy.data.texts.get(attached.name)).default_with(lambda: bpy.data.texts.new(attached.name))
            text.use_fake_user = True
            text.from_string(msgspec.json.encode(record, order="deterministic").decode())

        return attached.read(bpy.context.window_manager).map(stored)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Builds the attachment, current-file load, and file callbacks, or name faults."""
        attachment = self.attachment

        def loaded() -> BlenderResult[Option[float]]:
            """Recovers an unset session when Blender exposes the current file."""
            return attachment.bound(identity).bind(lambda attached: Ok(None) if attached.stored(bpy.context.window_manager) else self.loaded(attached)).map(lambda _: Nothing)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

        return Ok((
            attachment,
            PathHandler(handlers=bpy.app.handlers.load_post, body=lambda _: attachment.bound(identity).bind(self.loaded)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            PathHandler(handlers=bpy.app.handlers.save_pre, body=lambda _: attachment.bound(identity).bind(self.saved)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            *((Timer(body=loaded),) if not bpy.app.background else ()),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TextRecord"]
