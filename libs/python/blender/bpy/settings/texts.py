"""Typed file records saved as text blocks and held by window manager."""

import bpy
from expression import Nothing, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.descriptors import rooted
from blender.bpy.extension.events.handlers import PathHandler
from blender.bpy.extension.events.timers import Timer
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReadFault, BlenderResult
from blender.bpy.settings.attachments import Attached, Attachment
from blender.imaging.faults import paired

# --- [MODELS] ---------------------------------------------------------------------------


class TextRecord[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """File record in fake-user text block and window manager."""

    key: Key
    form: type[T]

    # --- [STORAGE]

    @property
    def attachment(self) -> Attachment[T]:
        """Window manager attachment."""
        return Attachment(key=self.key, form=self.form, owner=bpy.types.WindowManager)

    def loaded(self, session: tuple[Attached[T], bpy.types.WindowManager]) -> BlenderResult[None]:
        """Loads file record from text block, defaults when absent."""
        attached, manager = session

        def decoded(text: bpy.types.Text) -> BlenderResult[T]:
            return catch(lambda: msgspec.json.decode(text.as_string(), type=self.form), exception=msgspec.DecodeError)().map_error(lambda error: (BlenderReadFault(request=repr(text), error=error),))

        return Option.of_optional(bpy.data.texts.get(attached.name)).map(decoded).default_with(lambda: Ok(self.form())).bind(lambda value: attached.write(manager, value))

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns attachment, load and save handlers, and startup load timer."""
        attachment = self.attachment

        def session() -> BlenderResult[tuple[Attached[T], bpy.types.WindowManager]]:
            """Binds attachment to live window manager."""
            return paired(attachment.bound(identity), rooted(bpy.types.WindowManager, bpy.context))

        def loaded() -> BlenderResult[Option[float]]:
            """Loads record once when window manager holds none."""
            return session().bind(lambda pair: Ok(None) if pair[0].stored(pair[1]) else self.loaded(pair)).map(lambda _: Nothing)

        def saved(pair: tuple[Attached[T], bpy.types.WindowManager]) -> BlenderResult[None]:
            """Saves window manager record into fake-user text block."""
            attached, manager = pair

            def stored(record: T) -> None:
                text: bpy.types.Text = Option.of_optional(bpy.data.texts.get(attached.name)).default_with(lambda: bpy.data.texts.new(attached.name))
                text.use_fake_user = True
                text.from_string(msgspec.json.encode(record, order="deterministic").decode())

            return attached.read(manager).map(stored)

        return Ok((
            attachment,
            PathHandler(handlers=bpy.app.handlers.load_post, body=lambda _: session().bind(self.loaded)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            PathHandler(handlers=bpy.app.handlers.save_pre, body=lambda _: session().bind(saved)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            Timer(body=loaded),
        ))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TextRecord"]
