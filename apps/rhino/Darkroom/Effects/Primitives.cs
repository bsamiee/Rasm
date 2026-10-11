using Rasm.Imaging.Filters.Generators;
using Rasm.Rhino.Render.Effects;

namespace Darkroom.Effects;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class PrimitiveText {
    public static ParameterText Text(ShapePrimitiveParameter parameter) =>
        parameter.Map(
            kind: ParameterText.Choice<ShapeKind, InvalidGenerator>(
                "Shape", "Figure the distance field draws", static kind => kind.Map(
                    ellipse: "Ellipse", box: "Box", polygon: "Polygon", ring: "Ring", cross: "Cross", heart: "Heart", triangle: "Triangle")),
            placement: ParameterText.Of("Placement", "Position and turn of the figure") with {
                Parameters = ParameterText.Fields<Placement, PlacementParameter, InvalidGenerator>(Text),
            },
            width: ParameterText.Of("Width", "Half width as a share of the selected frame side"),
            height: ParameterText.Of("Height", "Half height as a share of the selected frame side"),
            roundness: ParameterText.Of("Roundness", "Corner radius as a share of the shorter half extent"),
            exponent: ParameterText.Of("Exponent", "Superellipse exponent, 1 a diamond, 2 an ellipse, above 2 a squircle"),
            sides: ParameterText.Of("Sides", "Polygon side count"),
            inset: ParameterText.Of("Inset", "Star inset of a polygon, the band of a ring, the arm width of a cross"),
            apex: ParameterText.Of("Apex", "Horizontal position of the triangle's top vertex between its left and right edges"),
            fit: ParameterText.Choice<ShapeFit, InvalidGenerator>(
                "Fit", "Frame side the shape dimensions follow", static fit => fit.Map(shortSide: "Short Side", longSide: "Long Side")));

    public static ParameterText Text(PlacementParameter parameter) =>
        parameter.Map(
            centerX: ParameterText.Of("Center X", "Figure center, rightward from the frame's left edge"),
            centerY: ParameterText.Of("Center Y", "Figure center, downward from the frame's top edge"),
            rotation: ParameterText.Of("Rotation", "Turn of the figure about its center, counterclockwise"));
}
