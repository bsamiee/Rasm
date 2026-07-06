# [API_ETO_DRAWING]

Catalog scope: the `Eto.Drawing` 2D vector/text/image surface behind canvas painting, wire rendering, icon drawing, and tooltip painters.

[NAMESPACES]:
- `Eto.Drawing` graphics — `Graphics` (draw/fill line/lines/rectangle/ellipse/path/polygon/text/image families, clip save/reset/set + `ClipBounds`, transform save/multiply, `AntiAlias`, `ImageInterpolation`, `PixelOffsetMode`, `ScreenScale`, points-per-pixel/DPI/visibility), `Context.CreateFromContent`.
- `Eto.Drawing` geometry — `Point`/`PointF`, `Size`/`SizeF`, `Rectangle`/`RectangleF` (union/intersect/inflate/contains/from-center families), `Padding`, `LineF`, `IMatrix`.
- `Eto.Drawing` color and brushes — `Color`/`ColorHSL`/`Colors`, `OpenColor.Family`, `SolidBrush`, `LinearGradientBrush` (point-pair and rect+angle), `RadialGradientBrush`, `TextureBrush`, `ITransformBrush`, `GradientWrapMode`.
- `Eto.Drawing` pens and paths — `Pen` (`LineCap`/`LineJoin`/`MiterLimit`/`DashStyle`), `PenLineCap`/`PenLineJoin`, `DashStyle`/`DashStyles`, `GraphicsPath`/`IGraphicsPath` (move/line/arc/bezier/curve/round-rect families, figure control, bounds).
- `Eto.Drawing` text — `Font`, `SystemFont`, `FontStyle`, `FontDecoration`, `FormattedText` (wrap/alignment/trimming/measure), `TextAnchor`.
- `Eto.Drawing` images — `Bitmap` (`PixelFormat.Format32bppRgba`, PNG round trip), `Image`, `ImageFormat`.
