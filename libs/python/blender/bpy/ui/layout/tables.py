"""Tables over layout rows: setting cells across scoped owners, path trees, and tree rows with one cell per column owner."""

from collections.abc import Callable
from functools import partial
from pathlib import PurePath

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block

from blender.bpy.data.rna.conditions import Scoped
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import definition, Native, RnaPath, Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.reports import alerted
from blender.bpy.ui.layout.rows import acquired, Line, marked, Member, Property, realized, ResolvedMember, RowContent, RowDraw, split
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Editor = Callable[[bpy.types.UILayout, Native, str], None]
type Branch[L: Struct, C: Struct] = tuple[int, L, tuple[Option[C], ...]]

# --- [MODELS] ---------------------------------------------------------------------------


class Matrix(Line, frozen=True, kw_only=True):
    """Setting rows across the owners of an edit scope under the owners' names, each compound pointer drawn through its host editor under the cell action, each field beside it.

    Rows a cell action draws register through their owner.
    """

    scope: Scoped
    settings: tuple[RnaPath, ...]
    compound: tuple[tuple[type[Struct], Editor], ...] = ()
    action: Option[Callable[[Identity, Descriptor], Outcome[RowDraw]]] = Nothing

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Resolves leaf settings, their cells, and the owner name property once into the table draw, each cell its compound editor under the action or its field beside the action."""
        owner_type = self.scope.owner_type
        kinds = tuple(kind for kind, _ in self.compound)

        def celled(setting: Descriptor) -> Outcome[tuple[Descriptor, RowDraw]]:
            action = self.action.map(lambda build: build(identity, setting).map(Some)).default_value(Ok(Nothing))
            match Block.of_seq(self.compound).filter(lambda pair: isinstance(setting.prop, bpy.types.PointerProperty) and isinstance(setting.prop.fixed_type, pair[0])).try_head():
                case Option(tag="some", some=(_, place)):
                    cell = action.map(lambda extra: partial(_edited, setting, place, extra))
                case _:
                    cell = paired(Property(target=Member(root=owner_type, path=setting.path), keywords={"text": ""}).drawn(identity), action).map(lambda parts: partial(_beside, *parts))
            return cell.map(lambda draw: (setting, draw))

        named = Option.of_optional(definition(owner_type).name_property).to_result_with(lambda: Fault.of(Reason.ABSENT, owner_type.__name__, member="name_property"))
        settings = accumulated(Descriptor.resolve(owner_type, path, bpy.types.Property) for path in self.settings).map(lambda found: tuple(leaf for each in found for leaf in each.leaves(kinds)))
        return paired(named, settings.bind(lambda found: accumulated(map(celled, found)))).map(lambda parts: partial(self.shown, *parts))

    def shown(self, name: bpy.types.StringProperty, held: tuple[tuple[Descriptor, RowDraw], ...], layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws owner names over one row of cells per setting across the scope's owners, labelled with the setting name or its mark while owner values differ."""
        found = self.scope.owners(context)
        alerted(layout, found)
        for owners in found.to_option().to_list():
            heads = _grid(layout, len(owners))[1]
            for owner in owners:
                heads.label(text=getattr(owner, name.identifier), translate=False)
            for setting, drawing in held:
                names, cells = _grid(layout, len(owners))
                names.alignment = "RIGHT"
                label = marked(setting, owners)
                alerted(names, label)
                mixed = label.default_value(Nothing)
                names.label(text=mixed.default_value(setting.prop.name), text_ctxt=setting.prop.translation_context, translate=mixed.is_none())
                for owner in owners:
                    drawing(cells.column(), owner, context)


class Tree(Line, frozen=True, kw_only=True):
    """Resolved paths listed once per prefix, indented by depth, files and folders under their glyphs."""

    paths: Callable[[bpy.types.Context], Outcome[tuple[PurePath, ...]]]

    def drawn(self, _identity: Identity, /) -> Outcome[RowDraw]:
        """Returns the tree draw over the paths read each redraw."""
        return Ok(self.shown)

    def shown(self, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws every path prefix once in order, indented by depth, a file glyph on full paths and a folder glyph on the rest."""
        found = self.paths(context)
        alerted(layout, found)
        for paths in found.to_option().to_list():
            files = {path.parts for path in paths}
            for node in sorted({path.parts[:end] for path in paths for end in range(1, len(path.parts) + 1)}):
                line = layout.row(align=True)
                for _ in node[1:]:
                    line.label(text="", icon="BLANK1")
                line.label(text=node[-1], icon="FILE_BLANK" if node in files else "FILE_FOLDER", translate=False)


class TreeMatrix[L: Struct, C: Struct](Line, frozen=True, kw_only=True):
    """Tree rows indented by depth under the column owners' names, each label struct's name and marks in the label cell, one cell drawn per column owner the row holds."""

    tree: Callable[[bpy.types.Context], tuple[tuple[str, ...], tuple[Branch[L, C], ...]]]
    cell: Callable[[bpy.types.UILayout, str, C, L], Outcome[None]]
    marks: tuple[RowContent, ...] = ()

    @property
    def children(self) -> tuple[RowContent, ...]:
        """Mark rows."""
        return self.marks

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the mark rows."""
        return Ok(acquired(self.marks))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the marks once into the tree draw."""
        return realized(self.marks, identity).map(lambda marks: partial(self.shown, marks))

    def shown(self, marks: RowDraw, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws column owner names, then each branch with its cells."""
        owners, branches = self.tree(context)
        heads = _grid(layout, len(owners))[1]
        for owner in owners:
            heads.label(text=owner, translate=False)
        for depth, label, cells in branches:
            names, row = _grid(layout, len(owners))
            for _ in range(depth):
                names.label(text="", icon="BLANK1")
            for named in Option.of_optional(definition(label).name_property).to_list():
                names.label(text=getattr(label, named.identifier), translate=False)
            marks(names, label, context)
            for owner, found in zip(owners, cells):
                line = row.row(align=True)
                for each in found.to_list():
                    alerted(line, self.cell(line, owner, each, label))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _grid(layout: bpy.types.UILayout, columns: int) -> tuple[bpy.types.UILayout, bpy.types.UILayout]:
    """Places the label cell beside an even grid of one value column per owner."""
    names, values = split(layout)
    return names, values.grid_flow(row_major=True, columns=columns, even_columns=True, align=True)


def _edited(setting: Descriptor, place: Editor, action: Option[RowDraw], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws a compound setting through its editor at each located owner, its action row first."""
    for owner, key in ResolvedMember(descriptor=setting).at(holder, context).to_list():
        for draw in action.to_list():
            draw(layout.row(align=True), holder, context)
        place(layout, owner, key)


def _beside(field: RowDraw, action: Option[RowDraw], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws a field with its action after it in one row."""
    line = layout.row(align=True) if action.is_some() else layout
    field(line, holder, context)
    for draw in action.to_list():
        draw(line, holder, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Branch", "Editor", "Matrix", "Tree", "TreeMatrix"]
