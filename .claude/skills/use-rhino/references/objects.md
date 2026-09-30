# [OBJECTS]

Layers, objects, selection, and commands go through `document.py` entry points inside `run_python`, RhinoCommon for an operation no entry point holds.

## [01]-[SCRIPTS]

Scripts hold model values in the document's units and reach RhinoCommon through pythonnet:
- Globals reset per call, a later call reaches objects through the ids a record printed or through `find`
- Record ids are strings, RhinoCommon tables take `Guid("<id>")`
- Lengths are model units, `doc.ModelAbsoluteTolerance` and `doc.ModelAngleToleranceRadians` give tolerances
- `RhinoMath.UnitScale(UnitSystem.<from>, doc.ModelUnitSystem)` scales a length given in another unit
- `Rhino.UI.Localization.FormatNumber(<length>, doc.ModelUnitSystem, Rhino.UI.DistanceDisplayMode.FeetInches, <p>, False)` states a length to 1/2^p in
- `doc.AdjustModelUnitSystem(UnitSystem.<unit>, False)` sets the model unit alone, tolerance, distance display, page units, style, and grid numbers stay
- Out parameters take no argument and follow the return value in a tuple, `TryGetBool(key)` reads `(found, value)`
- .NET arrays reach a collection overload through `method.Overloads[IEnumerable[T]](array)`
- Enum parameters take a member or `<Enum>(<int>)`, members named `None` read as `NONE`
- Camera changes pair `PushViewProjection` and `PopViewProjection` inside one call
- Work another UI-thread task finishes (a Grasshopper 2 solve) reads in the next call
