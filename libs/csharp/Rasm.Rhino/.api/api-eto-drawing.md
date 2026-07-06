# [API_ETO_DRAWING]

Catalog scope: the `Eto.Drawing` 2D vector/text/image surface — the full custom-drawing primitive set behind `Drawable` hosts and pipeline-neutral paint rails.

[NAMESPACES]:
- `Eto.Drawing` graphics — `Graphics` (fill/draw rectangle/line/lines/ellipse/polygon/path/image/text, `MeasureString`, clip save/reset/set, transform save/translate/scale/rotate/multiply, `AntiAlias`, `ImageInterpolation`, `PixelOffsetMode`).
- `Eto.Drawing` geometry — `Point`/`PointF`, `Size`/`SizeF`, `Rectangle`/`RectangleF` (from-center/inflate/union/intersect/contains families).
- `Eto.Drawing` color and brushes — `Color`/`Colors`/`SystemColors`, `SolidBrush`, `LinearGradientBrush`, `RadialGradientBrush`, `TextureBrush`, `GradientWrapMode`.
- `Eto.Drawing` pens and paths — `Pen` (`LineCap`, `LineJoin`, `MiterLimit`, `DashStyle`), `PenLineCap`/`PenLineJoin`, `DashStyle`/`DashStyles`, `GraphicsPath`/`IGraphicsPath` (move/line/arc/bezier/curve/rectangle/round-rect, figure open/close, fill/stroke hit tests, bounds).
- `Eto.Drawing` text — `Font`, `FontStyle`, `FontDecoration`, `SystemFonts` roster, `FormattedText` (wrap/alignment/trimming/measure).
- `Eto.Drawing` images — `Bitmap` (pixel formats, byte-array round trip), `Image`, `ImageFormat`, `IMatrix`, `Context`.
