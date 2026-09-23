# ty: ignore[unresolved-import, unresolved-attribute, not-iterable, invalid-argument-type]
# mypy: disable-error-code="attr-defined, arg-type, unreachable"
# /// script
# requires-python = ">=3.13"
# dependencies = ["msgspec", "rhino3dm"]
# ///
"""Describe `.3dm` files on disk through openNURBS, with no Rhino running and no document opened, one JSON line per file."""

from collections import Counter
from datetime import datetime
from functools import reduce
from pathlib import Path
import sys

import msgspec
from records import Fault, LayerRecord, MaterialRecord, Properties, Record
from rhino3dm import ActiveSpace, BoundingBox, File3dm, ObjectMode, ObjectType, UnitSystem

# --- [MODELS] ---------------------------------------------------------------------------


class FileRecord(Record, frozen=True):
    """One `.3dm` file's archive version, last editor, lock holder, units, tolerances, model objects, layers, materials, blocks, and views."""

    path: str
    version: int
    edited_by: str
    edited: datetime
    open_by: tuple[str, ...] | None
    units: UnitSystem
    tolerance: float
    angle_tolerance: float
    types: dict[ObjectType, int]
    hidden: int
    locked: int
    min: tuple[float, float, float] | None
    max: tuple[float, float, float] | None
    invalid: dict[str, str]
    layers: tuple[LayerRecord, ...]
    materials: tuple[MaterialRecord, ...]
    blocks: tuple[str, ...]
    views: tuple[str, ...]
    named_views: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def describe(path: Path) -> FileRecord | Fault:
    """Read one `.3dm` file's contents as the record `describe(doc)` gives an open document, model-space objects outside block definitions counted."""
    model = File3dm.Read(str(path))
    if model is None:
        return Fault(File3dm, str(path))
    objects = [item for item in model.Objects if not item.Attributes.IsInstanceDefinitionObject and item.Attributes.ActiveSpace == ActiveSpace.ModelSpace]
    materials = list(model.Materials)
    per_layer = Counter(item.Attributes.LayerIndex for item in objects)
    modes = Counter(item.Attributes.Mode for item in objects)
    box = reduce(BoundingBox.Union, (item.Geometry.GetBoundingBox() for item in objects)) if objects else None
    html = "#{:02X}{:02X}{:02X}".format
    lock = Path(f"{path}.rhl")
    return FileRecord(
        path=str(path),
        version=model.ArchiveVersion,
        edited_by=model.LastEditedBy,
        edited=model.LastEdited,
        open_by=tuple(lock.read_text(encoding="utf-8-sig").splitlines()) if lock.exists() else None,
        units=model.Settings.ModelUnitSystem,
        tolerance=model.Settings.ModelAbsoluteTolerance,
        angle_tolerance=model.Settings.ModelAngleToleranceDegrees,
        types=dict(Counter(item.Geometry.ObjectType for item in objects)),
        hidden=modes[ObjectMode.Hidden],
        locked=modes[ObjectMode.Locked],
        min=None if box is None else (box.Min.X, box.Min.Y, box.Min.Z),
        max=None if box is None else (box.Max.X, box.Max.Y, box.Max.Z),
        invalid={str(item.Attributes.Id): item.Geometry.IsValidWithLog[1] for item in objects if not item.Geometry.IsValid},
        layers=tuple(
            LayerRecord(
                layer.FullPath,
                Properties(
                    color=html(*layer.Color[:3]),
                    print_color=html(*layer.PlotColor[:3]),
                    print_width=layer.PlotWeight,
                    material=None if layer.RenderMaterialIndex < 0 else materials[layer.RenderMaterialIndex].Name,
                    visible=layer.Visible,
                    locked=layer.Locked,
                    strings=dict(layer.GetUserStrings()) or None,
                ),
                per_layer[layer.Index],
            )
            for layer in model.Layers
        ),
        materials=tuple(
            {
                material.RenderMaterialInstanceId: MaterialRecord(
                    material.Name, html(*(round(channel * 255) for channel in surface.BaseColor[:3])), surface.Roughness, surface.Metallic, surface.Opacity
                )
                for material in materials
                for surface in (material.PhysicallyBased,)
                if surface.Supported
            }.values()
        ),
        blocks=tuple(definition.Name for definition in model.InstanceDefinitions),
        views=tuple(view.Name for view in model.Views),
        named_views=tuple(view.Name for view in model.NamedViews),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> None:
    """Print one JSON line per `.3dm` file each argument names, a folder naming every `.3dm` under it, and exit 1 when any file fails to read."""
    results = [describe(path.resolve()) for argument in map(Path, sys.argv[1:]) for path in (sorted(argument.rglob("*.3dm")) if argument.is_dir() else (argument,))]
    sys.stdout.buffer.write(msgspec.json.Encoder(enc_hook=lambda value: value.__name__ if isinstance(value, type) else value.name).encode_lines(results))
    sys.exit(any(isinstance(result, Fault) for result in results))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FileRecord", "describe", "main"]

if __name__ == "__main__":
    main()
