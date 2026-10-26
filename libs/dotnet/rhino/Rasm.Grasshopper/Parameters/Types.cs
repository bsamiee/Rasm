using System.Globalization;
using Eto.Drawing;
using Grasshopper2;
using Grasshopper2.Data;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Parameters;
using Grasshopper2.Parsing;
using Grasshopper2.Types.Assistant;
using Grasshopper2.Types.Conversion;
using Grasshopper2.Types.ConversionMethods;
using Grasshopper2.UI.Icon;
using Rasm.Grasshopper.UI.Surfaces;
using Rasm.Rhino;
using Rasm.Rhino.Persistence.Stores;
using SkiaSharp;
using UUIDNext;
using UnitsNet;

namespace Rasm.Grasshopper.Parameters;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DataTexts<TData>(Func<TData, string> Capture, Func<string, Fin<TData>> Recall) where TData : notnull {
    private const string Root = "value";

    public static DataTexts<TData> Of<TRecord>(Rasm.UI.IStateParameter<TRecord> parameter, TRecord seed, Lens<TRecord, TData> lens) where TRecord : notnull {
        FieldCodec<TRecord> codec = new ValueKey<TRecord>(Root, parameter.Kind, parameter.Text, seed).Codec;
        string owner = typeof(TData).ToString();
        return codec.Entries.Count is 1
            ? new(
                data => codec.Entries[0].Capture(lens.Set(data, seed)).IfNone(""),
                text => codec.Entries[0].Recall(text).Bind(step => step.Map(set => lens.Get(set(seed))).RunSafe()))
            : new(
                data => ValueSet.Fields(owner, lens.Set(data, seed), codec.Parameters).Text(),
                text => ValueSet.Read(text).Bind(set => codec.Recalled(owner, set.Texts(owner)).Map(lens.Get).RunSafe()));
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DataParameter<T> : Parameter<T>, GrasshopperIO.ICustomIoId {
    // --- [CONSTRUCTION]
    public DataParameter() : base(Nomen.Empty) { }

    public DataParameter(Nomen nomen, Access access = Access.Tree) : base(nomen, access) { }

    public DataParameter(GrasshopperIO.IReader reader) : base(reader) { }

    // --- [PERSISTENCE]
    Guid GrasshopperIO.ICustomIoId.CustomIoId => ((GrasshopperIO.ICustomIoId)TypeAssistant).CustomIoId;

    protected override IIcon IconInternal => TypeAssistant.Icon;
}

internal class DataAssistant<T>(Guid parameterId, string name, IGrasshopperPlugin plugin, IIcon icon, Func<T, T> copy, Func<T, string> describe)
    : TypeAssistant<T>(RowText.Localize(name, plugin).Local), GrasshopperIO.ICustomIoId {
    public Guid CustomIoId { get; } = parameterId;

    public sealed override IIcon Icon { get; } = icon;

    public override T Copy(T instance) => copy(instance);

    public override bool Same(T a, T b) => EqualityComparer<T>.Default.Equals(a, b);

    public override string DescribePrimary(Pear<T> pear) => describe(pear.Item);
}

internal sealed class ImageDataAssistant(Guid parameterId, string name, IGrasshopperPlugin plugin, IIcon icon)
    : DataAssistant<SKImage>(parameterId, name, plugin, icon, identity,
        static image => string.Create(CultureInfo.InvariantCulture, $"{image.Width} × {image.Height} {image.ColorType} {((image.ColorSpace ?? SKColorSpace.CreateSrgb()).GammaIsLinear ? "linear" : "encoded")}")) {
    private readonly CallbackSite site = new(plugin, typeof(ImageValueDisplay), nameof(IValueDisplay.Draw));

    public override IValueDisplay Show(Pear<SKImage> pear) => pear is null ? base.Show(pear) : new ImageValueDisplay(pear.Item, site);
}

internal sealed class ImageValueDisplay(SKImage image, CallbackSite site) : IValueDisplay {
    private readonly Rasm.UI.Node view = new Rasm.UI.Node.Image(image) { Fit = Rasm.UI.ImageFit.Contain, Width = Rasm.UI.Sizing.Grow.Unit, Height = Rasm.UI.Sizing.Grow.Unit };

    public DisplayHeight Height => DisplayHeight.Wide;

    public void Draw(Context context, Font font, Color background, ValueDisplayFrame frame) =>
        Callbacks.Run(
            DisposalOps.Within(() => new Guest(site.Sink, IO.lift(context.Invalidate)), guest =>
                from device in IO.lift(() => Guest.Device(context))
                from painted in guest.Paint(context.Graphics, device, frame.Inner, Rasm.UI.Visibility.Shown, view)
                select unit),
            site);
}

internal class StoredDataAssistant<T>(Guid parameterId, string name, IGrasshopperPlugin plugin, IIcon icon, DataTexts<T> texts)
    : DataAssistant<T>(parameterId, name, plugin, icon, identity, texts.Capture) where T : notnull {
    public override string Format(T instance) => texts.Capture(instance);

    public override Result<T> Parse(string constructor, string text, Pear<T> existing) => Recall(text);

    public override bool Write(GrasshopperIO.IWriter writer, GrasshopperIO.Name location, T instance) {
        writer.String(location, texts.Capture(instance));
        return true;
    }

    public override bool Read(GrasshopperIO.IReader reader, GrasshopperIO.Name location, out T instance) {
        Result<T> result = Recall(reader.String(location));
        instance = result.Value;
        return result.Success;
    }

    private Result<T> Recall(string text) =>
        texts.Recall(text).Match(
            Succ: value => Result<T>.Succeed(value, text),
            Fail: error => Result<T>.Fail(error.Message, text));
}

internal sealed class NumericDataAssistant<TValue, TKey>(Guid parameterId, string name, IGrasshopperPlugin plugin, IIcon icon, DataTexts<TValue> texts, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key)
    : StoredDataAssistant<TValue>(parameterId, name, plugin, icon, texts)
    where TValue : notnull
    where TKey : struct, System.Numerics.INumber<TKey> {
    public Func<TKey, Fin<TValue>> Create { get; } = create;
    public Func<TValue, TKey> Key { get; } = key;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class NumberConversions<TValue, TKey>
    where TValue : notnull
    where TKey : struct, System.Numerics.INumber<TKey> {
    [Merit(Merit.Direct)]
    [Explanation("Reads number as key within value range, integer keys round half to even")]
    public static void Inbound(double source, out TValue target) =>
        target = ((NumericDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>())
            .Create(TKey.CreateChecked(TKey.CreateTruncating(0.5d) == TKey.Zero ? double.Round(source, MidpointRounding.ToEven) : source)).ThrowIfFail();

    [Merit(Merit.Direct)]
    [Explanation("Reads integer as key within value range")]
    public static void Inbound(int source, out TValue target) =>
        target = ((NumericDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>()).Create(TKey.CreateChecked(source)).ThrowIfFail();

    [Merit(Merit.Direct)]
    [Explanation("Writes value key as number")]
    public static void Outbound(TValue source, out double target) => target = Number(source);

    [Merit(Merit.Direct)]
    [Explanation("Rounds value key to integer")]
    public static void Outbound(TValue source, out int target) => IntegerConversions.Convert(Number(source), out target);

    internal static double Number(TValue value) => double.CreateChecked(((NumericDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>()).Key(value));
}

internal static class KeyConversions<TValue, TRaw>
    where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
    where TRaw : notnull, ISpanParsable<TRaw> {
    [Merit(Merit.Direct)]
    [Explanation("Reads value from key")]
    public static void Inbound(TRaw source, out TValue target) => target = ValidationResults.Validated<TValue, TRaw>(source).ThrowIfFail();

    [Merit(Merit.Direct)]
    [Explanation("Writes value key")]
    public static void Outbound(TValue source, out TRaw target) => target = source.ToValue();
}

internal abstract class ValueArithmetic<TValue, TKey> : ArithmeticRepository
    where TValue : notnull
    where TKey : struct, System.Numerics.INumber<TKey> {
    [Arithmetic.Operator(Arithmetic.Operation.Add)]
    public static double Add(TValue a, TValue b) => NumberConversions<TValue, TKey>.Number(a) + NumberConversions<TValue, TKey>.Number(b);

    [Arithmetic.Operator(Arithmetic.Operation.Sub)]
    public static double Subtract(TValue a, TValue b) => NumberConversions<TValue, TKey>.Number(a) - NumberConversions<TValue, TKey>.Number(b);

    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static double Multiply(TValue a, TValue b) => NumberConversions<TValue, TKey>.Number(a) * NumberConversions<TValue, TKey>.Number(b);

    [Arithmetic.Operator(Arithmetic.Operation.Div)]
    public static double Divide(TValue a, TValue b) => NumberConversions<TValue, TKey>.Number(a) / NumberConversions<TValue, TKey>.Number(b);
}

internal abstract class OperandArithmetic<TValue, TKey, TOperand> : ArithmeticRepository
    where TValue : notnull
    where TKey : struct, System.Numerics.INumber<TKey>
    where TOperand : struct, System.Numerics.INumber<TOperand> {
    [Arithmetic.Operator(Arithmetic.Operation.Add)]
    public static double Add(TValue a, TOperand b) => NumberConversions<TValue, TKey>.Number(a) + double.CreateChecked(b);

    [Arithmetic.Operator(Arithmetic.Operation.Add)]
    public static double Add(TOperand a, TValue b) => double.CreateChecked(a) + NumberConversions<TValue, TKey>.Number(b);

    [Arithmetic.Operator(Arithmetic.Operation.Sub)]
    public static double Subtract(TValue a, TOperand b) => NumberConversions<TValue, TKey>.Number(a) - double.CreateChecked(b);

    [Arithmetic.Operator(Arithmetic.Operation.Sub)]
    public static double Subtract(TOperand a, TValue b) => double.CreateChecked(a) - NumberConversions<TValue, TKey>.Number(b);

    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static double Multiply(TValue a, TOperand b) => NumberConversions<TValue, TKey>.Number(a) * double.CreateChecked(b);

    [Arithmetic.Operator(Arithmetic.Operation.Mul)]
    public static double Multiply(TOperand a, TValue b) => double.CreateChecked(a) * NumberConversions<TValue, TKey>.Number(b);

    [Arithmetic.Operator(Arithmetic.Operation.Div)]
    public static double Divide(TValue a, TOperand b) => NumberConversions<TValue, TKey>.Number(a) / double.CreateChecked(b);

    [Arithmetic.Operator(Arithmetic.Operation.Div)]
    public static double Divide(TOperand a, TValue b) => double.CreateChecked(a) / NumberConversions<TValue, TKey>.Number(b);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class DataTypes {
    // --- [ROWS]
    public static Func<IGrasshopperPlugin, IO<Unit>> Of<T>(Guid parameterId, string name, IIcon icon, Func<T, T> copy, Func<T, string> describe) =>
        plugin => Registered(new DataAssistant<T>(parameterId, name, plugin, icon, copy, describe));

    public static Func<IGrasshopperPlugin, IO<Unit>> Opaque<T>(string caption, Rasm.UI.GlyphSource glyph, Func<T, T> copy, Func<T, string> describe) =>
        plugin => CanvasIcons.Vector(plugin, glyph).Bind(icon => Of(ParameterId<T>(plugin), caption, icon, copy, describe)(plugin));

    public static Func<IGrasshopperPlugin, IO<Unit>> Image(string caption, Rasm.UI.GlyphSource glyph) =>
        plugin => CanvasIcons.Vector(plugin, glyph).Bind(icon => Registered(new ImageDataAssistant(ParameterId<SKImage>(plugin), caption, plugin, icon)));

    public static Func<IGrasshopperPlugin, IO<Unit>> Of<T, TAssistant>(Func<IGrasshopperPlugin, IO<TAssistant>> create)
        where TAssistant : TypeAssistant<T>, GrasshopperIO.ICustomIoId =>
        plugin => create(plugin).Bind(static assistant => Registered(assistant));

    public static Func<IGrasshopperPlugin, IO<Unit>> Stored<T>(Guid parameterId, string name, IIcon icon, DataTexts<T> texts) where T : notnull =>
        plugin => Registered(new StoredDataAssistant<T>(parameterId, name, plugin, icon, texts));

    public static Func<IGrasshopperPlugin, IO<Unit>> Number<TValue, TKey>(
        Guid parameterId, string name, IIcon icon, DataTexts<TValue> texts, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key)
        where TValue : notnull
        where TKey : struct, System.Numerics.INumber<TKey> =>
        plugin => Registered(
            new NumericDataAssistant<TValue, TKey>(parameterId, name, plugin, icon, texts, create, key),
            [
                Converted<double, TValue>(NumberConversions<TValue, TKey>.Inbound),
                Converted<TValue, double>(NumberConversions<TValue, TKey>.Outbound),
                Converted<int, TValue>(NumberConversions<TValue, TKey>.Inbound),
                Converted<TValue, int>(NumberConversions<TValue, TKey>.Outbound),
                .. typeof(IQuantity).IsAssignableFrom(typeof(TValue))
                    ? []
                    : Seq(typeof(ValueArithmetic<TValue, TKey>), typeof(OperandArithmetic<TValue, TKey, double>), typeof(OperandArithmetic<TValue, TKey, int>))
                        .Map<(string, Func<bool>)>(static repository => (nameof(Arithmetic.AddRepository), () => Arithmetic.AddRepository(repository) > 0)),
            ]);

    public static Func<IGrasshopperPlugin, IO<Unit>> Keyed<TValue, TRaw>(Guid parameterId, string name, IIcon icon, DataTexts<TValue> texts)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        plugin => Registered(
            new StoredDataAssistant<TValue>(parameterId, name, plugin, icon, texts),
            Seq(
                Converted<TRaw, TValue>(KeyConversions<TValue, TRaw>.Inbound),
                Converted<TValue, TRaw>(KeyConversions<TValue, TRaw>.Outbound)));

    // --- [REGISTRATION]
    internal static (Type Type, Func<IGrasshopperPlugin, IO<Unit>> Register) Field<T>(Func<Guid, string, IIcon, Func<IGrasshopperPlugin, IO<Unit>>> create) =>
        (typeof(T), plugin =>
            from assistant in IO.lift(static () => Optional(TypeAssistantServer.Find<T>()))
            from registered in assistant.Match(
                Some: static _ => unitIO,
                None: () => CanvasIcons.PlugIn(plugin).Bind(icon => create(ParameterId<T>(plugin), typeof(T).Name, icon.IfNone(StandardIcons.Parameters.Unknown))(plugin)))
            select registered);

    private static Guid ParameterId<T>(IGrasshopperPlugin plugin) => Uuid.NewNameBased(plugin.Plugin.Id, typeof(DataParameter<T>).ToString());

    private static (string Member, Func<bool> Accepted) Converted<TSource, TTarget>(ConversionDelegate<TSource, TTarget> conversion) =>
        (nameof(ConversionServer.TryAddConversion), () => ConversionServer.TryAddConversion(conversion.Method));

    private static IO<Unit> Registered<T>(TypeAssistant<T> assistant, Seq<(string Member, Func<bool> Accepted)> steps = default) {
        static Fin<Unit> Step((string Member, Func<bool> Accepted) step) => step.Accepted() ? unit : new Refused(step.Member, $"refused data type {typeof(T)}");
        return IO.lift(() => Callbacks.Each(steps.Add((nameof(TypeAssistantServer.Register), () => TypeAssistantServer.Register(assistant))), static (step, _) => Step(step)))
            .Bind(static _ => IO.lift(static () => Step((nameof(GrasshopperIO.IO.RegisterRestorableType), static () => GrasshopperIO.IO.RegisterRestorableType(typeof(DataParameter<T>))))));
    }
}
