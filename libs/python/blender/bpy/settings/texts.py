"""Typed file records persisted as native text blocks and held by the window manager."""

import bpy
from expression import Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.bpy.extension.events.handlers import PathHandler
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderReadFault, BlenderResult
from blender.bpy.settings.attachments import Attached, Attachment
from blender.imaging.faults import paired

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

    def loaded(self, session: tuple[Attached[T], bpy.types.WindowManager]) -> BlenderResult[None]:
        """Loads the file record, returning decode or native session-write refusals."""
        attached, manager = session
        held: Option[bpy.types.Text] = Option.of_optional(bpy.data.texts.get(attached.name))
        record: BlenderResult[T] = held.map(
            lambda text: catch(lambda: msgspec.json.decode(text.as_string(), type=self.form), exception=msgspec.DecodeError)().map_error(
                lambda error: (BlenderReadFault(request=repr(text), error=error),)
            )
        ).default_with(lambda: Ok(self.form()))
        return record.bind(lambda value: attached.write(manager, value))

    def saved(self, session: tuple[Attached[T], bpy.types.WindowManager]) -> BlenderResult[None]:
        """Persists actual session values in the file or returns native read faults."""
        attached, manager = session

        def stored(record: T) -> None:
            """Writes a fake-user text from the single native record read."""
            text: bpy.types.Text = Option.of_optional(bpy.data.texts.get(attached.name)).default_with(lambda: bpy.data.texts.new(attached.name))
            text.use_fake_user = True
            text.from_string(msgspec.json.encode(record, order="deterministic").decode())

        return attached.read(manager).map(stored)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Builds the attachment, current-file load, and file callbacks, or name and absent-session faults."""
        attachment = self.attachment

        def session() -> BlenderResult[tuple[Attached[T], bpy.types.WindowManager]]:
            """Binds the attachment to the live window manager holding the session record."""
            return paired(attachment.bound(identity), Option.of_optional(bpy.context.window_manager).to_result_with(lambda: (BlenderAbsentFault(owner="bpy.context", key="window_manager"),)))

        def loaded() -> BlenderResult[Option[float]]:
            """Recovers an unset session when Blender exposes the current file."""
            return session().bind(lambda pair: Ok(None) if pair[0].stored(pair[1]) else self.loaded(pair)).map(lambda _: Nothing)

        return Ok((
            attachment,
            PathHandler(handlers=bpy.app.handlers.load_post, body=lambda _: session().bind(self.loaded)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            PathHandler(handlers=bpy.app.handlers.save_pre, body=lambda _: session().bind(self.saved)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            Timer(body=loaded),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TextRecord"]
