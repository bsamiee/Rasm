using System.Globalization;
using Rhino.Display;
using Rhino.Render;
using Rhino.Render.Fields;

namespace Rasm.Rhino.Render.Content;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ParameterValue {
    public sealed record BoolValue(bool Value) : ParameterValue;

    public sealed record IntValue(int Value) : ParameterValue;

    public sealed record FloatValue(float Value) : ParameterValue;

    public sealed record DoubleValue(double Value) : ParameterValue;

    public sealed record StringValue(string Value) : ParameterValue;

    public sealed record Color4fValue(Color4f Value) : ParameterValue;

    public sealed record Vector2dValue(Vector2d Value) : ParameterValue;

    public sealed record Vector3dValue(Vector3d Value) : ParameterValue;

    public sealed record Point4dValue(Point4d Value) : ParameterValue;

    public sealed record TransformValue(Transform Value) : ParameterValue;

    public sealed record GuidValue(Guid Value) : ParameterValue;

    public sealed record DateTimeValue(DateTime Value) : ParameterValue;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentFields {
    // --- [FIELDS]
    public static IO<Unit> Set(FieldDictionary fields, string name, string text) =>
        IO.lift(() => Missing.Unless(fields.ContainsField(name), nameof(FieldDictionary.ContainsField))
            .Bind(_ => Callbacks.Thrown<InvalidOperationException, Unit>(fun(() => fields.Set(name, text)), nameof(FieldDictionary.Set))));

    // --- [PARAMETERS]
    public static IO<Option<T>> GetParameter<T>(RenderContent content, string parameter) where T : notnull =>
        use(() => Optional(content.GetParameter(parameter)), static answer => answer.Bind(static held => Optional(held as IDisposable)).Iter(static variant => variant.Dispose()))
            .Map(static answer => answer.Bind(static held => Optional(Convert.ChangeType(held, typeof(T), CultureInfo.InvariantCulture))).Map(static value => (T)value))
            .Bracket();

    public static IO<Unit> SetParameter(RenderContent content, string parameter, ParameterValue value) =>
        IO.lift(() => Callbacks.Thrown<InvalidOperationException, bool>(
                () => content.SetParameter(parameter, value.Switch<object>(
                    boolValue: static held => held.Value,
                    intValue: static held => held.Value,
                    floatValue: static held => held.Value,
                    doubleValue: static held => held.Value,
                    stringValue: static held => held.Value,
                    color4fValue: static held => held.Value,
                    vector2dValue: static held => held.Value,
                    vector3dValue: static held => held.Value,
                    point4dValue: static held => held.Value,
                    transformValue: static held => held.Value,
                    guidValue: static held => held.Value,
                    dateTimeValue: static held => held.Value)),
                nameof(RenderContent.SetParameter))
            .Bind(static accepted => Refused.Unless(accepted, nameof(RenderContent.SetParameter))));

    public static IO<Unit> BindParameterToField(RenderContent content, string parameter, Field field) =>
        IO.lift(fun(() => content.BindParameterToField(parameter, field, RenderContent.ChangeContexts.Program)));
}
