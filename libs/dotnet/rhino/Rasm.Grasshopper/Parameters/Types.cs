using System.Globalization;
using Eto.Drawing;
using Grasshopper2;
using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Parsing;
using Grasshopper2.Types.Assistant;
using Grasshopper2.Types.Conversion;
using Grasshopper2.Types.ConversionMethods;
using Grasshopper2.UI.Icon;
using Rasm.Grasshopper.UI;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Styling;
using UUIDNext;
using UnitsNet;

namespace Rasm.Grasshopper.Parameters;

// --- [SERVICES] ------------------------------------------------------------------------
internal class DataAssistant<T>(Guid parameterId, string name, DefinedGrasshopperPlugin plugin, IIcon icon, Func<T, T> copy, Func<T, string> describe)
    : TypeAssistant<T>(RowText.Localize(name, plugin).Local), GrasshopperIO.ICustomIoId {
    public Guid CustomIoId { get; } = parameterId;

    protected DefinedGrasshopperPlugin Plugin { get; } = plugin;

    public sealed override IIcon Icon { get; } = icon;

    public override T Copy(T instance) => copy(instance);

    public override bool Same(T a, T b) => EqualityComparer<T>.Default.Equals(a, b);

    public override string DescribePrimary(Pear<T> pear) => describe(pear.Item);
}

internal sealed class ImageDataAssistant(Guid parameterId, string name, DefinedGrasshopperPlugin plugin, IIcon icon)
    : DataAssistant<PixelFrame>(parameterId, name, plugin, icon, static frame => frame.Copy(),
        static frame => string.Create(CultureInfo.InvariantCulture, $"{frame.Size.Width} × {frame.Size.Height} at {frame.Origin} in {frame.Extent.Width} × {frame.Extent.Height}")) {
    public override IValueDisplay Show(Pear<PixelFrame> pear) =>
        pear is null ? base.Show(pear) : new ImageValueDisplay(pear, base.Show(pear), new(Plugin, typeof(ImageValueDisplay), nameof(IValueDisplay.Draw)));
}

internal sealed class ImageValueDisplay(Pear<PixelFrame> image, IValueDisplay text, CallbackSite site) : IValueDisplay {
    public DisplayHeight Height => DisplayHeight.Wide;

    public void Draw(Context context, Font font, Color background, ValueDisplayFrame frame) =>
        Callbacks.Run(IO.lift(() => DataTypes.Encoding(image)).Bind(encoding => encoding.Match(
            Some: transfer =>
                from tiles in PreviewTiles.Current.Of(site.Sink)
                let size = image.Item.Size
                let fit = MathF.Min(frame.Inner.Width / size.Width, frame.Inner.Height / size.Height)
                let target = RectangleF.FromCenter(frame.Inner.Center, new SizeF(size.Width * fit, size.Height * fit))
                let device = Size.Ceiling(target.Size * CanvasPainting.Scale(context))
                from extent in IO.lift(() => PixelExtent.Validated(device.Width, device.Height))
                from drawn in tiles.Paint(typeof(ImageValueDisplay), Optional(context.Control as Eto.Forms.Control), image.Item, extent, transfer,
                    bitmap => context.Graphics.DrawImage(bitmap, target))
                from deferred in drawn.Match(Some: static _ => unitIO, None: () => IO.lift(context.Invalidate))
                select unit,
            None: () => IO.lift(() => text.Draw(context, font, background, frame)))), site);
}

internal class StoredDataAssistant<T>(Guid parameterId, string name, DefinedGrasshopperPlugin plugin, IIcon icon, Func<T, T> copy, Func<T, string> describe, Func<T, string> capture, Func<string, Fin<T>> recall)
    : DataAssistant<T>(parameterId, name, plugin, icon, copy, describe) {
    public override string Format(T instance) => capture(instance);

    public override Result<T> Parse(string constructor, string text, Pear<T> existing) => Recall(text);

    public override bool Write(GrasshopperIO.IWriter writer, GrasshopperIO.Name location, T instance) {
        writer.String(location, capture(instance));
        return true;
    }

    public override bool Read(GrasshopperIO.IReader reader, GrasshopperIO.Name location, out T instance) {
        Result<T> result = Recall(reader.String(location));
        instance = result.Value;
        return result.Success;
    }

    private Result<T> Recall(string text) =>
        recall(text).Match(
            Succ: value => Result<T>.Succeed(value, text),
            Fail: error => Result<T>.Fail(error.Message, text));
}

internal sealed class NumericDataAssistant<TValue, TKey>(Guid parameterId, string name, DefinedGrasshopperPlugin plugin, IIcon icon, Func<TValue, string> describe, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key)
    : StoredDataAssistant<TValue>(parameterId, name, plugin, icon, identity, describe, value => StoredText.Capture(value, key), text => StoredText.Recall(text, create, key))
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
    [Explanation("Reads a checked numeric key within the value's range, rounding to the nearest integer, ties to even, for an integer key.")]
    public static void Inbound(double source, out TValue target) =>
        target = ((NumericDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>())
            .Create(TKey.CreateChecked(TKey.CreateTruncating(0.5d) == TKey.Zero ? double.Round(source, MidpointRounding.ToEven) : source)).ThrowIfFail();

    [Merit(Merit.Direct)]
    [Explanation("Reads a checked integer key within the value's range.")]
    public static void Inbound(int source, out TValue target) =>
        target = ((NumericDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>()).Create(TKey.CreateChecked(source)).ThrowIfFail();

    [Merit(Merit.Direct)]
    [Explanation("Writes the value's numeric key.")]
    public static void Outbound(TValue source, out double target) => target = Number(source);

    [Merit(Merit.Direct)]
    [Explanation("Rounds the value's numeric key to a host integer.")]
    public static void Outbound(TValue source, out int target) => IntegerConversions.Convert(Number(source), out target);

    internal static double Number(TValue value) => double.CreateChecked(((NumericDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>()).Key(value));
}

internal static class KeyConversions<TValue, TRaw>
    where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
    where TRaw : notnull, ISpanParsable<TRaw> {
    [Merit(Merit.Direct)]
    [Explanation("Reads the value from its key.")]
    public static void Inbound(TRaw source, out TValue target) => target = ValidationResults.Validated<TValue, TRaw>(source).ThrowIfFail();

    [Merit(Merit.Direct)]
    [Explanation("Writes the value's key.")]
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
    // --- [IMAGE_VALUES]
    private static readonly MetaName ImageEncoding = new(typeof(Transfer).ToString().Split('.'));

    public static Pear<PixelFrame> Image(PixelFrame frame, Transfer encoding) =>
        Pear<PixelFrame>.Create(frame, MetaData.Empty.Add(ImageEncoding, encoding.ToValue()));

    public static Fin<Option<Transfer>> Encoding(Pear<PixelFrame> pear) =>
        pear.Meta is { } meta && meta.Get(ImageEncoding, out string text)
            ? Transfer.Validated(text).Map(Some)
            : Fin.Succ(Option<Transfer>.None);

    // --- [ROWS]
    public static Func<DefinedGrasshopperPlugin, IO<Unit>> Of<T>(Guid parameterId, string name, IIcon icon, Func<T, T> copy, Func<T, string> describe) =>
        plugin => Registered(new DataAssistant<T>(parameterId, name, plugin, icon, copy, describe));

    public static Func<DefinedGrasshopperPlugin, IO<Unit>> Opaque<T>(IGlyph glyph, int master, SpecialColour accent, Func<T, T> copy, Func<T, string> describe) =>
        Field<T>((plugin, id, name) => CanvasIcons.Accented(plugin, glyph, master, accent).Bind(icon => Of(id, name, icon, copy, describe)(plugin))).Register;

    public static Func<DefinedGrasshopperPlugin, IO<Unit>> Image(IGlyph glyph, int master) =>
        Field<PixelFrame>((plugin, id, name) => CanvasIcons.Accented(plugin, glyph, master, SpecialColour.Input)
            .Bind(icon => Registered(new ImageDataAssistant(id, name, plugin, icon)))).Register;

    public static Func<DefinedGrasshopperPlugin, IO<Unit>> Of<T, TAssistant>(Func<DefinedGrasshopperPlugin, IO<TAssistant>> create)
        where TAssistant : TypeAssistant<T>, GrasshopperIO.ICustomIoId =>
        plugin => create(plugin).Bind(static assistant => Registered(assistant));

    public static Func<DefinedGrasshopperPlugin, IO<Unit>> Stored<T>(Guid parameterId, string name, IIcon icon, Func<T, T> copy, Func<T, string> describe, Func<T, string> capture, Func<string, Fin<T>> recall) =>
        plugin => Registered(new StoredDataAssistant<T>(parameterId, name, plugin, icon, copy, describe, capture, recall));

    public static Func<DefinedGrasshopperPlugin, IO<Unit>> Number<TValue, TKey>(Guid parameterId, string name, IIcon icon, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key)
        where TValue : notnull
        where TKey : struct, System.Numerics.INumber<TKey> =>
        Numeric(parameterId, name, icon, value => StoredText.Capture(value, key), create, key, arithmetic: !typeof(IQuantity).IsAssignableFrom(typeof(TValue)));

    internal static Func<DefinedGrasshopperPlugin, IO<Unit>> OptionalNumber<TValue, TKey>(Guid parameterId, string name, IIcon icon, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key)
        where TValue : notnull
        where TKey : struct, System.Numerics.INumber<TKey> =>
        Schema(FieldTexts.Gate(FieldTexts.Leaf(value => StoredText.Capture(value, key), text => StoredText.Recall(text, create, key))), parameterId, name, icon);

    public static Func<DefinedGrasshopperPlugin, IO<Unit>> Integer<TValue, TKey>(Guid parameterId, string name, IIcon icon, Func<TValue, string> describe)
        where TValue : IObjectFactory<TValue, TKey, ValidationError>, IConvertible<TKey>
        where TKey : struct, System.Numerics.IBinaryInteger<TKey> =>
        Numeric(parameterId, name, icon, describe, ValidationResults.Validated<TValue, TKey>, static value => value.ToValue());

    public static Func<DefinedGrasshopperPlugin, IO<Unit>> Keyed<TValue, TRaw>(Guid parameterId, string name, IIcon icon, Func<TValue, string> describe)
        where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
        where TRaw : notnull, ISpanParsable<TRaw> =>
        plugin => Registered(
            new StoredDataAssistant<TValue>(parameterId, name, plugin, icon, static value => value, describe, StoredText.Capture<TValue, TRaw>, StoredText.Recall<TValue, TRaw>),
            Seq(
                Converted<TRaw, TValue>(KeyConversions<TValue, TRaw>.Inbound),
                Converted<TValue, TRaw>(KeyConversions<TValue, TRaw>.Outbound)));

    // --- [REGISTRATION]
    private static Func<DefinedGrasshopperPlugin, IO<Unit>> Numeric<TValue, TKey>(Guid parameterId, string name, IIcon icon, Func<TValue, string> describe, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, bool arithmetic = true)
        where TValue : notnull
        where TKey : struct, System.Numerics.INumber<TKey> =>
        plugin => Registered(
            new NumericDataAssistant<TValue, TKey>(parameterId, name, plugin, icon, describe, create, key),
            Seq(
                Converted<double, TValue>(NumberConversions<TValue, TKey>.Inbound),
                Converted<TValue, double>(NumberConversions<TValue, TKey>.Outbound),
                Converted<int, TValue>(NumberConversions<TValue, TKey>.Inbound),
                Converted<TValue, int>(NumberConversions<TValue, TKey>.Outbound))
            .Concat(arithmetic ? Seq<(string, Func<bool>)>(
                (nameof(Arithmetic.AddRepository), static () => Arithmetic.AddRepository(typeof(ValueArithmetic<TValue, TKey>)) > 0),
                (nameof(Arithmetic.AddRepository), static () => Arithmetic.AddRepository(typeof(OperandArithmetic<TValue, TKey, double>)) > 0),
                (nameof(Arithmetic.AddRepository), static () => Arithmetic.AddRepository(typeof(OperandArithmetic<TValue, TKey, int>)) > 0)) : []));

    internal static (Type Type, Func<DefinedGrasshopperPlugin, IO<Unit>> Register) Field<T>(Func<DefinedGrasshopperPlugin, Guid, string, IO<Unit>> create) =>
        (typeof(T), plugin =>
            from assistant in IO.lift(static () => Optional(TypeAssistantServer.Find<T>()))
            from registered in assistant.Match(
                Some: static _ => unitIO,
                None: () => create(plugin, Uuid.NewNameBased(plugin.Id, typeof(DataParameter<T>).ToString()), typeof(T).ToString()))
            select registered);

    internal static Func<DefinedGrasshopperPlugin, IO<Unit>> Schema<TData>(
        FieldCodec<TData> codec, Guid parameterId, string name, IIcon icon)
        where TData : notnull {
        string owner = EntryKey.TypeOwner(parameterId);
        FieldCodec<TData> rooted = codec.Under(name);
        Func<TData, string> capture = value => new ValueSet(toHashMap(rooted.Capture(value).AsIterable()
            .Map(entry => (new EntryKey(owner, entry.Key), entry.Value)))).Text();
        return Stored(parameterId, name, icon, identity, capture, capture,
            text => ValueSet.Read(text).Bind(set => Try.lift(() =>
                rooted.Recall(toHashMap(set.Entries.AsIterable().Filter(entry => string.Equals(entry.Key.Owner, owner, StringComparison.Ordinal))
                    .Map(static entry => (entry.Key.Path, entry.Value)))).Run()).Run()));
    }

    private static (string Member, Func<bool> Accepted) Converted<TSource, TTarget>(ConversionDelegate<TSource, TTarget> conversion) =>
        (nameof(ConversionServer.TryAddConversion), () => ConversionServer.TryAddConversion(conversion.Method));

    private static IO<Unit> Registered<T>(TypeAssistant<T> assistant, Seq<(string Member, Func<bool> Accepted)> steps = default) {
        static Fin<Unit> Step((string Member, Func<bool> Accepted) step) => step.Accepted() ? unit : new Refused(step.Member, $"refused the data type {typeof(T)}");
        return IO.lift(() => Callbacks.Each(steps.Add((nameof(TypeAssistantServer.Register), () => TypeAssistantServer.Register(assistant))), static (step, _) => Step(step)))
            .Bind(static _ => IO.lift(static () => Step((nameof(GrasshopperIO.IO.RegisterRestorableType), static () => GrasshopperIO.IO.RegisterRestorableType(typeof(DataParameter<T>))))));
    }
}
