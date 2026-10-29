using System.Drawing;
using System.Numerics;
using Rasm.Drafting;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Annotation.Styles;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HatchPatterns {
    // --- [STANDARD]
    public static HatchPattern Hatching(LineGroup lines) {
        HatchPattern pattern = new() { FillType = HatchPatternFillType.Lines, PatternUnitSystem = UnitSystem.Millimeters };
        using HatchLine line = new() { Angle = lines.Hatching.Angle.Radians.ToDouble(), Offset = new Vector2d(0d, lines.Hatching.Spacing.Millimeters.ToDouble()) };
        _ = pattern.AddHatchLine(line);
        return pattern;
    }

    // --- [COMPARISON]
    public static IO<bool> Unchanged(HatchPattern live, HatchPattern staged) =>
        (from held in Products.Many(() => live.HatchLines, required: false)
         from wanted in Products.Many(() => staged.HatchLines, required: false)
         select (live.FillType, live.PatternUnitSystem, live.AlwaysModelDistances, live.Description) == (staged.FillType, staged.PatternUnitSystem, staged.AlwaysModelDistances, staged.Description)
             && UserStrings.Of(live.GetUserStrings()) == UserStrings.Of(staged.GetUserStrings())
             && held.Count == wanted.Count
             && held.Zip(wanted).ForAll(static pair =>
                 (pair.First.Angle, pair.First.BasePoint, pair.First.Offset) == (pair.Second.Angle, pair.Second.BasePoint, pair.Second.Offset)
                 && pair.First.GetDashes.SequenceEqual(pair.Second.GetDashes))).Bracket();

    // --- [PREVIEW]
    public static IO<Seq<(Vector2 From, Vector2 To)>> Preview(HatchPattern pattern, UnitsNet.Angle angle, Size pixels) =>
        IO.lift(() => toSeq(pattern.CreatePreviewGeometry(pixels.Width, pixels.Height, angle.Radians.ToDouble()))
            .Map(static line => (From: new Vector2((float)line.FromX, (float)-line.FromY), To: new Vector2((float)line.ToX, (float)-line.ToY)))
            .Strict());

    // --- [DEFAULTS]
    public static IO<int> FromDefaults(RhinoDoc doc, string name) =>
        Rows.Find(doc.HatchPatterns, name)
            .Map(static live => live.Index)
            .Catch(static error => error.Filter<RhinoFault>().Head is RhinoFault fault && fault.Reason == RhinoReason.Missing, error =>
                (from defaults in Products.Many(HatchPattern.GetDefaultHatchPatterns)
                 from held in IO.lift(() => defaults.Find(row => Rows.Names<HatchPattern>().Equals(row.Name, name)).ToFin(error))
                 from index in IO.lift(() => Conversions.Required(Conversions.Present(TableKinds.HatchPatterns.Add(doc, held))))
                 select index).Bracket());

    // --- [CURRENT]
    public static IO<Unit> SetCurrent(RhinoDoc doc, ComponentRef<HatchPattern> address) =>
        from live in Rows.Find(doc.HatchPatterns, address)
        from changed in unless(doc.HatchPatterns.CurrentHatchPatternIndex == live.Index, IO.lift(() => { doc.HatchPatterns.CurrentHatchPatternIndex = live.Index; })).As()
        select changed;
}
