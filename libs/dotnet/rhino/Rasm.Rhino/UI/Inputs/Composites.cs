using Eto.Forms;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PointRow(Option<Point3d> Value, Func<Edit<Point3d>, IO<Unit>> Edited) : IRow<PointRow> {
    public bool ModelUnits { get; init; } = true;

    public RowShape Shape { get; init; } = RowShape.Inline;

    public static IO<RowCells> Realize(IO<PointRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from doc in scope.DocumentIO
        from fields in Seq(0, 1, 2).TraverseM(_ =>
            from field in use(static () => Values.Fielded(new NumericUpDownWithUnitParsing { UseDistanceDisplayMode = true }))
            from measured in NumberFields.Measured(field, doc, first.ModelUnits)
            from digits in Values.Digits(field)
            select field).As()
        from shows in fields.Map(static (field, axis) => (Field: field, Axis: axis)).TraverseM(held =>
            Values.Bound(held.Field, Values.Numeric(held.Field), Values.Stepping, None, first.Value.Map(point => point[held.Axis]), edit =>
                latest.Bind(row => row.Edited(edit.Mapped(value => Placed(fields, held.Axis, value)))))).As()
        from line in RowTable.Lined(fields.Map(static field => new TableCell(field, scaleWidth: true)))
        select new RowCells(line, latest.Bind(row => shows.Map((show, axis) => show(row.Value.Map(point => point[axis]))).TraverseM(static shown => shown).As()).Map(unit));

    private static Point3d Placed(Seq<NumericUpDownWithUnitParsing> fields, int axis, double value) =>
        fields.Map((field, at) => at == axis ? value : field.Value) switch {
            var coordinates => new Point3d(coordinates[0], coordinates[1], coordinates[2]),
        };
}

public sealed record DateSpanRow(Option<DateInterval> Value, DateInterval Limits, Func<Edit<DateInterval>, IO<Unit>> Edited) : IRow<DateSpanRow> {
    public RowShape Shape { get; init; } = RowShape.Inline;

    public static IO<RowCells> Realize(IO<DateSpanRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from start in use(static () => new DateTimePicker { Mode = DateTimePickerMode.Date })
        from end in use(static () => new DateTimePicker { Mode = DateTimePickerMode.Date })
        from showStart in Values.Bound(start, Dated(start), static _ => false, None, first.Value.Map(static span => span.Start), edit =>
            latest.Bind(row => row.Edited(edit.Mapped(day => new DateInterval(day, LocalDate.FromDateTime(end.Value ?? row.Limits.End.ToDateTimeUnspecified()))))))
        from showEnd in Values.Bound(end, Dated(end), static _ => false, None, first.Value.Map(static span => span.End), edit =>
            latest.Bind(row => row.Edited(edit.Mapped(day => new DateInterval(LocalDate.FromDateTime(start.Value ?? row.Limits.Start.ToDateTimeUnspecified()), day)))))
        from line in RowTable.Lined([new TableCell(start, scaleWidth: true), new TableCell(end, scaleWidth: true)])
        select new RowCells(line,
            from row in latest
            from bounded in IO.lift(() => {
                (start.MinDate, end.MaxDate) = (row.Limits.Start.ToDateTimeUnspecified(), row.Limits.End.ToDateTimeUnspecified());
                (start.MaxDate, end.MinDate) = (row.Value.Map(static span => span.End).IfNone(row.Limits.End).ToDateTimeUnspecified(), row.Value.Map(static span => span.Start).IfNone(row.Limits.Start).ToDateTimeUnspecified());
            })
            from startShown in showStart(row.Value.Map(static span => span.Start))
            from endShown in showEnd(row.Value.Map(static span => span.End))
            select unit);

    private static BindableBinding<DateTimePicker, Option<LocalDate>> Dated(DateTimePicker picker) =>
        picker.ValueBinding.Convert(static held => Optional(held).Map(LocalDate.FromDateTime), static held => held.Map(static day => day.ToDateTimeUnspecified()).ToNullable());
}
