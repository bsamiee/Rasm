using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Eto.Forms;
using Grasshopper2;
using Grasshopper2.Data;
using Grasshopper2.Display;
using Grasshopper2.Parameters;
using Grasshopper2.Parsing;
using Grasshopper2.Types.Assistant;
using Grasshopper2.Types.Conversion;
using Grasshopper2.Types.ConversionMethods;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Icon.Vector;
using Grasshopper2.UI.Slider;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using UUIDNext;

namespace Rasm.Grasshopper.Data;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IGeometric<TSelf> where TSelf : IGeometric<TSelf> {
    public static abstract Seq<GeometryBase> Geometry(TSelf value);

    public static abstract TSelf Transform(TSelf value, Transform transform);

    public static abstract TSelf Deform(TSelf value, SpaceMorph morph);
}

public interface IValueControl<T> {
    public Option<Control> CreateValueControl(ValueParameter<T> parameter);

    public Seq<Preset<T>> Presets => [];
}

// --- [SERVICES] ------------------------------------------------------------------------
public class ValueParameter<T> : Parameter<T>, GrasshopperIO.ICustomIoId {
    // --- [CONSTRUCTION]
    public ValueParameter() : base(new Nomen(typeof(T).Name, "")) { }

    public ValueParameter(string name, string userName, string info, Access access = Access.Item) : base(name, userName, info, access) { }

    public ValueParameter(GrasshopperIO.IReader reader) : base(reader) { }

    // --- [IDENTITY]
    public Guid CustomIoId => DataTypes.IoId(GetType());

    // --- [PRESENTATION]
    public override bool DisplayCapable => TypeAssistant.ZupportsDraw;

    public override IPresets PresetsWeak {
        get {
            if (Presets.Count is 0 && TypeAssistant is IValueControl<T> owner)
                _ = owner.Presets.Iter(preset => Presets.Add(preset));
            return Presets;
        }
    }

    public override Control CreateValueControl() =>
        (Presets.Count is 0 && TypeAssistant is IValueControl<T> owner ? owner.CreateValueControl(this).ValueUnsafe() : null) ?? base.CreateValueControl();

    protected override IIcon? IconInternal => TypeAssistant?.Icon;

    protected override void CreateDisplayFromTree(Tree<T> tree, DisplayBag bag, CancellationToken token) =>
        toSeq(tree.NonNullPears).TakeWhile(_ => !token.IsCancellationRequested).Iter(pear => TypeAssistant.Draw(pear, bag));
}

internal class ValueAssistant<TValue, TKey, TError>(ITypeAssistant<TKey> key, Option<Func<ValueParameter<TValue>, Control>> control = default, Seq<Preset<TValue>> presets = default)
    : TypeAssistant<TValue>($"{char.ToLowerInvariant(typeof(TValue).Name[0])}{typeof(TValue).Name[1..]}"), IValueControl<TValue>
    where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>
    where TKey : notnull
    where TError : class, IValidationError<TError> {
    // --- [IDENTITY]
    public override IIcon Icon => DataTypes.Icon(typeof(TValue).Assembly, typeof(TValue).Name) ?? key.Icon;

    public override IEnumerable<string> Constructors => [Name];

    // --- [VALUES]
    public override TValue Copy(TValue instance) => instance;

    public override string DescribePrimary(Pear<TValue> pear) => key.DescribePrimary(Garden.Pear(pear.Item.ToValue(), pear.Meta));

    // --- [TEXT]
    public override string Format(TValue instance) => key.Format(instance.ToValue());

    public override Result<TValue> Parse(string constructor, string text, Pear<TValue> existing) =>
        key.Parse(constructor: null, text, existing: null) switch {
            { Success: false } failed => Result<TValue>.Fail(failed.Error, text, failed.Start, failed.After),
            var parsed => Conversions<TValue, TKey, TError>.Validated(parsed.Value).Match(
                Succ: value => parsed.WithValue(value),
                Fail: error => Result<TValue>.Fail(error.Message, text, parsed.Start, parsed.After)),
        };

    // --- [PERSISTENCE]
    public override bool Write(GrasshopperIO.IWriter writer, GrasshopperIO.Name location, TValue instance) => key.Write(writer, location, instance.ToValue());

    public override bool Read(GrasshopperIO.IReader reader, GrasshopperIO.Name location, out TValue instance) {
        Option<TValue> read = key.Read(reader, location, out TKey stored) ? Conversions<TValue, TKey, TError>.Validated(stored).ToOption() : None;
        instance = read.ValueUnsafe()!;
        return read.IsSome;
    }

    // --- [CONTROLS]
    public Option<Control> CreateValueControl(ValueParameter<TValue> parameter) => control.Map(create => create(parameter));

    public Seq<Preset<TValue>> Presets => presets;
}

internal sealed class GeometricAssistant<TValue, TKey, TError>(ITypeAssistant<TKey> key) : ValueAssistant<TValue, TKey, TError>(key)
    where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IGeometric<TValue>
    where TKey : notnull
    where TError : class, IValidationError<TError> {
    // --- [PREVIEW]
    public override void Draw(Pear<TValue> pear, DisplayBag bag) =>
        TValue.Geometry(pear.Item).Iter(shape => Optional(TypeAssistantServer.FindByValue(shape)).Iter(assistant => assistant.Draw(Garden.IPear(shape, pear.Meta), bag)));

    public override IEnumerable<GeometryBase> ToGeometry(Pear<TValue> pear) => TValue.Geometry(pear.Item);

    // --- [SPACE]
    public override BoundingBox BoundingBox(TValue instance) =>
        TValue.Geometry(instance).Fold(Rhino.Geometry.BoundingBox.Unset, static (box, shape) => Rhino.Geometry.BoundingBox.Union(box, shape.GetBoundingBox(accurate: true)));

    public override object Transform(TValue instance, Transform matrix) => TValue.Transform(instance, matrix);

    public override object Deform(TValue instance, SpaceMorph morph) => TValue.Deform(instance, morph);

    public override bool ClosestPoint(TValue instance, Point3d locus, out Point3d site) {
        Option<Point3d> nearest = TValue.Geometry(instance)
            .Choose(shape => Optional(TypeAssistantServer.FindByValue(shape)).Bind(assistant => assistant.ClosestPoint(shape, locus, out Point3d found) ? Some(found) : None))
            .Fold(Option<Point3d>.None, (best, found) => best.Exists(held => held.DistanceToSquared(locus) <= found.DistanceToSquared(locus)) ? best : Some(found));
        site = nearest.IfNone(locus);
        return nearest.IsSome;
    }

    // --- [MEASURES]
    public override bool Length(TValue instance, out double length) {
        Option<double> measured = Measured(instance, static (assistant, shape) => assistant.Length(shape, out double value) ? Some(value) : None);
        length = measured.IfNone(0d);
        return measured.IsSome;
    }

    public override bool Area(TValue instance, out double area) {
        Option<double> measured = Measured(instance, static (assistant, shape) => assistant.Area(shape, out double value) ? Some(value) : None);
        area = measured.IfNone(0d);
        return measured.IsSome;
    }

    public override bool Volume(TValue instance, out double volume) {
        Option<double> measured = Measured(instance, static (assistant, shape) => assistant.Volume(shape, out double value) ? Some(value) : None);
        volume = measured.IfNone(0d);
        return measured.IsSome;
    }

    private static Option<double> Measured(TValue instance, Func<ITypeAssistant, GeometryBase, Option<double>> measure) =>
        TValue.Geometry(instance).Choose(shape => Optional(TypeAssistantServer.FindByValue(shape)).Bind(assistant => measure(assistant, shape))).Strict() switch {
            { IsEmpty: true } => None,
            var parts => parts.Fold(0d, static (sum, part) => sum + part),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Conversions<TValue, TKey, TError>
    where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>
    where TKey : notnull
    where TError : class, IValidationError<TError> {
    // --- [VALIDATION]
    public static Fin<TValue> Validated(TKey key) =>
        TValue.Validate(key, CultureInfo.InvariantCulture, out TValue? value) switch {
            null => value!,
            Error error => error,
            var error => Error.New(string.Create(CultureInfo.InvariantCulture, $"{error}")),
        };

    // --- [INBOUND]
    [Merit(Merit.Direct)]
    [Explanation("Validates the key as the value.")]
    public static void Inbound(TKey source, out TValue target) =>
        target = Validated(source).IfFail(static error => throw ConversionRepository.CustomError(typeof(TKey), typeof(TValue), error.Message));

    // --- [OUTBOUND]
    [Merit(Merit.Direct)]
    [Explanation("Takes the key of the value.")]
    public static void Outbound(TValue source, out TKey target) => target = source.ToValue();

    // --- [ROWS]
    public static Seq<IO<Unit>> Rows { get; } = typeof(TKey) == typeof(string) ? [] : [Row<TKey, TValue>(Inbound), Row<TValue, TKey>(Outbound)];

    public static IO<Unit> Row<TSource, TTarget>(ConversionDelegate<TSource, TTarget> convert) =>
        IO.lift(() => GrasshopperFault.Unless(
            ConversionServer.TryAddConversion(convert.Method) || ConversionServer.FindConversion<TSource, TTarget>() is not null, GrasshopperReason.Declined, convert.Method));
}

internal static class NumberConversions<TValue, TKey, TError>
    where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>
    where TKey : struct, INumber<TKey>
    where TError : class, IValidationError<TError> {
    // --- [KEYS]
    public static bool Integral { get; } = TKey.IsInteger(TKey.CreateTruncating(0.5d));

    // --- [INBOUND]
    [Merit(Merit.Direct)]
    [Explanation("Validates the number as the key, rounded first when the key is an integer.")]
    public static void Inbound(double source, out TValue target) =>
        Conversions<TValue, TKey, TError>.Inbound(TKey.CreateChecked(Integral ? double.Round(source, MidpointRounding.AwayFromZero) : source), out target);

    [Merit(Merit.Direct)]
    [Explanation("Validates the integer as the key.")]
    public static void Inbound(int source, out TValue target) => Conversions<TValue, TKey, TError>.Inbound(TKey.CreateChecked(source), out target);

    [Merit(Merit.Direct)]
    [Explanation("Validates the big integer as the key.")]
    public static void Inbound(BigInteger source, out TValue target) => Conversions<TValue, TKey, TError>.Inbound(TKey.CreateChecked(source), out target);

    // --- [OUTBOUND]
    [Merit(Merit.Direct)]
    [Explanation("Takes the key of the value as a number.")]
    public static void Outbound(TValue source, out double target) => target = double.CreateChecked(source.ToValue());

    [Merit(Merit.Direct)]
    [Explanation("Rounds the key of the value to an integer.")]
    public static void Outbound(TValue source, out int target) => IntegerConversions.Convert(double.CreateChecked(source.ToValue()), out target);

    // --- [ROWS]
    public static Seq<IO<Unit>> Rows { get; } = [
        Conversions<TValue, TKey, TError>.Row<double, TValue>(Inbound),
        Conversions<TValue, TKey, TError>.Row<int, TValue>(Inbound),
        Conversions<TValue, TKey, TError>.Row<BigInteger, TValue>(Inbound),
        Conversions<TValue, TKey, TError>.Row<TValue, double>(Outbound),
        Conversions<TValue, TKey, TError>.Row<TValue, int>(Outbound),
        .. Conversions<TValue, TKey, TError>.Rows,
    ];
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class DataTypes {
    // --- [ROWS]
    public static IO<Unit> Value<TValue, TKey, TError>()
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>
        where TKey : notnull
        where TError : class, IValidationError<TError> =>
        Keyed<TValue, TKey>(static key => new ValueAssistant<TValue, TKey, TError>(key), Conversions<TValue, TKey, TError>.Rows);

    public static IO<Unit> Choice<TValue, TKey, TError>()
        where TValue : ISmartEnum<TKey, TValue, TError>
        where TKey : notnull
        where TError : class, IValidationError<TError> =>
        Keyed<TValue, TKey>(
            static key => new ValueAssistant<TValue, TKey, TError>(key, presets: toSeq(TValue.Items).Map(static item => new Preset<TValue>(string.Create(CultureInfo.InvariantCulture, $"{item.ToValue()}"), item))),
            Conversions<TValue, TKey, TError>.Rows);

    public static IO<Unit> Number<TValue, TKey, TError>()
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>
        where TKey : struct, INumber<TKey>
        where TError : class, IValidationError<TError> =>
        Keyed<TValue, TKey>(static key => new ValueAssistant<TValue, TKey, TError>(key), NumberConversions<TValue, TKey, TError>.Rows);

    public static IO<Unit> Bounded<TValue, TKey, TError>()
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : class, IValidationError<TError> =>
        Keyed<TValue, TKey>(
            static key => new ValueAssistant<TValue, TKey, TError>(key, Some(static Control (ValueParameter<TValue> parameter) => {
                Guid runtime = Guid.NewGuid();
                UiNumber standard = UiNumber.Standard(decimal.CreateSaturating(TValue.MinValue.ToValue()), decimal.CreateSaturating(TValue.MaxValue.ToValue()));
                UiNumber limits = NumberConversions<TValue, TKey, TError>.Integral ? standard.WithDecimals(0) : standard;
                Seq<decimal> current = toSeq(parameter.PersistentData.NonNullItems).Map(static value => decimal.CreateSaturating(value.ToValue()));
                SliderControl slider = new(limits) { Width = 200, Height = 24 };
                slider.Values.Assign(current.IsEmpty ? Seq(limits.Value) : current);
                slider.SliderValueChanged += (_, _) => {
                    Seq<TValue> picked = toSeq(slider.Values.AllValues).Choose(static number => Conversions<TValue, TKey, TError>.Validated(TKey.CreateChecked(number)).ToOption());
                    if (picked.IsEmpty) return;
                    parameter.AddUndoRecord(new VerbNoun("Set", parameter.TypeAssistant.Name), new PersistentDataAction(parameter, runtime));
                    parameter.PersistentData = Garden.TreeFromPears(
                        (parameter.PersistentData.PathCount > 0 ? parameter.PersistentData.Paths : Paths.Zero).First, picked.Map(static value => Garden.Pear(value)));
                    parameter.Document?.Solution.DelayedExpire(parameter);
                };
                return slider;
            })),
            NumberConversions<TValue, TKey, TError>.Rows);

    public static IO<Unit> Geometric<TValue, TKey, TError>()
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IGeometric<TValue>
        where TKey : notnull
        where TError : class, IValidationError<TError> =>
        Keyed<TValue, TKey>(static key => new GeometricAssistant<TValue, TKey, TError>(key), Conversions<TValue, TKey, TError>.Rows);

    public static IO<Unit> Assistant<T>() =>
        IO.lift(static () => GrasshopperFault.Unless(
            GrasshopperIO.IO.TryGetIoId(typeof(ValueParameter<T>), out Guid id) && (GrasshopperIO.IO.RegisterRestorableType(typeof(ValueParameter<T>), id) || GrasshopperIO.IO.ResolveType(id) is not null),
            GrasshopperReason.Declined,
            typeof(ValueParameter<T>)));

    private static IO<Unit> Keyed<TValue, TKey>(Func<ITypeAssistant<TKey>, TypeAssistant<TValue>> assistant, Seq<IO<Unit>> conversions) =>
        from key in IO.lift(static () => Optional(TypeAssistantServer.Find<TKey>()).ToFin(new GrasshopperFault(typeof(TKey).ToString(), GrasshopperReason.Missing)))
        from registered in IO.lift(() => GrasshopperFault.Unless(TypeAssistantServer.Find<TValue>() is not null || TypeAssistantServer.Register(assistant(key)), GrasshopperReason.Declined, typeof(TValue)))
        from converted in conversions.TraverseM(static row => row).As()
        from restorable in Assistant<TValue>()
        select unit;

    // --- [IDENTITY]
    private static readonly Func<(Assembly Assembly, string Name), Option<IIcon>> Icons = memoUnsafe(static ((Assembly Assembly, string Name) source) =>
        Optional(AbstractIcon.FromResource(source.Name, source.Assembly))
        || Optional(Folders.TryFindResourceUri(source.Assembly, source.Name + ".svg"))
            .Bind(resource => Optional(Folders.GetResourceAsString(source.Assembly, resource)))
            .Map(static IIcon (svg) => SvgIcon.FromXml(svg)));

    internal static Guid IoId(Type type) =>
        Uuid.NewNameBased(
            Optional(type.Assembly.GetCustomAttribute<GuidAttribute>()).Bind(static declared => parseGuid(declared.Value)).IfNone(static () => Hooks.Session.Plugin.Id),
            type.ToString());

    internal static IIcon? Icon(Assembly assembly, string name) => Icons((assembly, name)).ValueUnsafe();
}
