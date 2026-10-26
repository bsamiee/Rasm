using System.Numerics;
using Grasshopper2;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.Parsing;
using Grasshopper2.Types.Assistant;
using Grasshopper2.Types.Conversion;
using Grasshopper2.Types.ConversionMethods;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Rasm.Rhino;
using UUIDNext;
using UnitsNet;

namespace Rasm.Grasshopper.Parameters;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DataTexts<TData>(Func<TData, string> Capture, Func<string, Fin<TData>> Recall) where TData : notnull;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DataParameter<T> : Parameter<T>, ICustomIoId {
    // --- [CONSTRUCTION]
    public DataParameter() : base(Nomen.Empty) { }

    public DataParameter(Nomen nomen, Access access = Access.Tree) : base(nomen, access) { }

    public DataParameter(IReader reader) : base(reader) { }

    // --- [PERSISTENCE]
    Guid ICustomIoId.CustomIoId => ((ICustomIoId)TypeAssistant).CustomIoId;

    protected override IIcon IconInternal => TypeAssistant.Icon;
}

internal class DataAssistant<T>(Guid parameterId, string name, IGrasshopperPlugin plugin, IIcon icon, Func<T, T> copy, Func<T, string> describe)
    : TypeAssistant<T>(RowText.Localize(name, plugin).Local), ICustomIoId {
    public Guid CustomIoId { get; } = parameterId;

    public sealed override IIcon Icon { get; } = icon;

    public override T Copy(T instance) => copy(instance);

    public override bool Same(T a, T b) => EqualityComparer<T>.Default.Equals(a, b);

    public override string DescribePrimary(Pear<T> pear) => describe(pear.Item);
}

internal class StoredDataAssistant<T>(Guid parameterId, string name, IGrasshopperPlugin plugin, IIcon icon, DataTexts<T> texts)
    : DataAssistant<T>(parameterId, name, plugin, icon, identity, texts.Capture) where T : notnull {
    public override string Format(T instance) => texts.Capture(instance);

    public override Result<T> Parse(string constructor, string text, Pear<T> existing) => Recall(text);

    public override bool Write(IWriter writer, Name location, T instance) {
        writer.String(location, texts.Capture(instance));
        return true;
    }

    public override bool Read(IReader reader, Name location, out T instance) {
        Result<T> result = Recall(reader.String(location));
        instance = result.Value;
        return result.Success;
    }

    private Result<T> Recall(string text) =>
        texts.Recall(text).Match(
            Succ: value => Result<T>.Succeed(value, text),
            Fail: error => Result<T>.Fail(error.Message, text));
}

internal sealed class KeyedDataAssistant<TValue, TKey>(Guid parameterId, string name, IGrasshopperPlugin plugin, IIcon icon, DataTexts<TValue> texts, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key)
    : StoredDataAssistant<TValue>(parameterId, name, plugin, icon, texts)
    where TValue : notnull
    where TKey : notnull {
    public Func<TKey, Fin<TValue>> Create { get; } = create;
    public Func<TValue, TKey> Key { get; } = key;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class NumberConversions<TValue, TKey>
    where TValue : notnull
    where TKey : struct, INumber<TKey> {
    [Merit(Merit.Direct)]
    [Explanation("Reads number as key within value range, integer keys round half to even")]
    public static void Inbound(double source, out TValue target) =>
        target = ((KeyedDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>())
            .Create(TKey.CreateChecked(TKey.CreateTruncating(0.5d) == TKey.Zero ? double.Round(source, MidpointRounding.ToEven) : source)).ThrowIfFail();

    [Merit(Merit.Direct)]
    [Explanation("Reads integer as key within value range")]
    public static void Inbound(int source, out TValue target) =>
        target = ((KeyedDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>()).Create(TKey.CreateChecked(source)).ThrowIfFail();

    [Merit(Merit.Direct)]
    [Explanation("Writes value key as number")]
    public static void Outbound(TValue source, out double target) => target = Number(source);

    [Merit(Merit.Direct)]
    [Explanation("Rounds value key to integer")]
    public static void Outbound(TValue source, out int target) => IntegerConversions.Convert(Number(source), out target);

    internal static double Number(TValue value) => double.CreateChecked(((KeyedDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>()).Key(value));
}

internal static class KeyConversions<TValue, TKey>
    where TValue : notnull
    where TKey : notnull {
    [Merit(Merit.Direct)]
    [Explanation("Reads value from key")]
    public static void Inbound(TKey source, out TValue target) =>
        target = ((KeyedDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>()).Create(source).ThrowIfFail();

    [Merit(Merit.Direct)]
    [Explanation("Writes value key")]
    public static void Outbound(TValue source, out TKey target) => target = ((KeyedDataAssistant<TValue, TKey>)TypeAssistantServer.Find<TValue>()).Key(source);
}

internal abstract class ValueArithmetic<TValue, TKey> : ArithmeticRepository
    where TValue : notnull
    where TKey : struct, INumber<TKey> {
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
    where TKey : struct, INumber<TKey>
    where TOperand : struct, INumber<TOperand> {
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

    public static Func<IGrasshopperPlugin, IO<Unit>> Opaque<T>(string caption, IO<IIcon> icon, Func<T, T> copy, Func<T, string> describe) =>
        plugin => icon.Bind(held => Of(ParameterId<T>(plugin), caption, held, copy, describe)(plugin));

    public static Func<IGrasshopperPlugin, IO<Unit>> Of<T, TAssistant>(Func<IGrasshopperPlugin, IO<TAssistant>> create)
        where TAssistant : TypeAssistant<T>, ICustomIoId =>
        plugin => create(plugin).Bind(static assistant => Registered(assistant));

    public static Func<IGrasshopperPlugin, IO<Unit>> Stored<T>(Guid parameterId, string name, IIcon icon, DataTexts<T> texts) where T : notnull =>
        plugin => Registered(new StoredDataAssistant<T>(parameterId, name, plugin, icon, texts));

    public static Func<IGrasshopperPlugin, IO<Unit>> Number<TValue, TKey>(
        Guid parameterId, string name, IIcon icon, DataTexts<TValue> texts, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        plugin => Registered(
            new KeyedDataAssistant<TValue, TKey>(parameterId, name, plugin, icon, texts, create, key),
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

    public static Func<IGrasshopperPlugin, IO<Unit>> Keyed<TValue, TKey>(
        Guid parameterId, string name, IIcon icon, DataTexts<TValue> texts, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key)
        where TValue : notnull
        where TKey : notnull =>
        plugin => Registered(
            new KeyedDataAssistant<TValue, TKey>(parameterId, name, plugin, icon, texts, create, key),
            Seq(
                Converted<TKey, TValue>(KeyConversions<TValue, TKey>.Inbound),
                Converted<TValue, TKey>(KeyConversions<TValue, TKey>.Outbound)));

    // --- [REGISTRATION]
    private static Guid ParameterId<T>(IGrasshopperPlugin plugin) => Uuid.NewNameBased(plugin.Plugin.Id, typeof(DataParameter<T>).ToString());

    private static (string Member, Func<bool> Accepted) Converted<TSource, TTarget>(ConversionDelegate<TSource, TTarget> conversion) =>
        (nameof(ConversionServer.TryAddConversion), () => ConversionServer.TryAddConversion(conversion.Method));

    private static IO<Unit> Registered<T>(TypeAssistant<T> assistant, Seq<(string Member, Func<bool> Accepted)> steps = default) {
        static Fin<Unit> Step((string Member, Func<bool> Accepted) step) => step.Accepted() ? unit : new Refused(step.Member, $"refused data type {typeof(T)}");
        return LanguageExt.IO.lift(() => Callbacks.Each(
                steps.Add((nameof(TypeAssistantServer.Register), () => TypeAssistantServer.Register(assistant)))
                    .Add((nameof(GrasshopperIO.IO.RegisterRestorableType), static () => GrasshopperIO.IO.RegisterRestorableType(typeof(DataParameter<T>)))),
                static (step, _) => Step(step)))
            .Map(static _ => unit);
    }
}
