using System.Diagnostics;
using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Framework;
using Grasshopper2.Parameters;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI.Icon;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Styling;

namespace Rasm.Grasshopper.Parameters;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ParameterFace(ParameterText Text, string Code, Option<(string Category, Option<Color> Tint, bool Shown)> Modularity) {
    public Nomen Localized(IPlugInSink sink) => new(RowText.Localize(Text.Caption, sink), RowText.Localize(Text.Help, sink));
}

public sealed record ParameterRow(ParameterFace Face, Func<IPlugInSink, IO<IParameter>> Create) {
    // --- [DECLARE]
    public Func<IParameter, IPlugInSink, IO<Unit>> Attach { get; init; } = static (_, _) => unitIO;
    public Seq<(Type Type, Func<DefinedGrasshopperPlugin, IO<Unit>> Register)> DataTypes { get; internal init; }

    public static ParameterRow Of(ParameterFace face, Func<Nomen, IParameter> construct) =>
        new(face, sink => IO.lift(() => construct(face.Localized(sink))).Bind(parameter => Coded(parameter, face, sink)));

    public static ParameterRow Of<TData>(ParameterFace face, Func<Nomen, Parameter<TData>> construct, Seq<(string Caption, TData Value)> presets)
        where TData : notnull =>
        new(face, sink =>
            from parameter in IO.lift(() => construct(face.Localized(sink)))
            from _ in IO.lift(() => Callbacks.Each(
                presets,
                preset => parameter.Presets.Add(RowText.Localize(preset.Caption, sink), "", preset.Value),
                nameof(Presets<>.Add)))
            from coded in Coded(parameter, face, sink)
            select coded);

    // --- [REALIZE]
    public IO<IParameter> Input(ModularInputAdder inputs, IPlugInSink sink) =>
        from parameter in Create(sink)
        from attached in Attach(parameter, sink)
        from _ in IO.lift(() => Face.Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color, Requirement?>)inputs.Add : inputs.AddHidden)(
                    parameter, RowText.Localize(optional.Category, sink), optional.Tint.IfNone(Colors.Transparent), null),
            None: () => inputs.RegularAdder.Add(parameter, parameter.Requirement)))
        select parameter;

    public IO<IParameter> Output(ModularOutputAdder outputs, IPlugInSink sink) =>
        from parameter in Create(sink)
        from attached in Attach(parameter, sink)
        from _ in IO.lift(() => Face.Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color>)outputs.Add : outputs.AddHidden)(
                    parameter, RowText.Localize(optional.Category, sink), optional.Tint.IfNone(Colors.Transparent)),
            None: () => outputs.RegularAdder.Add(parameter)))
        select parameter;

    private static IO<IParameter> Coded(IParameter parameter, ParameterFace face, IPlugInSink sink) =>
        IO.lift(() => parameter.UserName = RowText.Localize(face.Code, sink)).Map(parameter);

    internal ParameterRow Presented(IGlyph glyph, int master, SpecialColour accent) =>
        this with {
            Attach = (parameter, sink) =>
                from attached in Attach(parameter, sink)
                from icon in CanvasIcons.Accented(sink, glyph, master, accent)
                from presented in IO.lift(() => parameter.ModifyIcon(icon, overrideExtrinsicIcon: false))
                select unit,
        };

    internal static Parameter<TData> Construct<TData>(Nomen nomen, Access access, Requirement requirement) {
        Parameter<TData> parameter = TypeAssistantServer.Find<TData>() is GrasshopperIO.ICustomIoId
            ? new DataParameter<TData>(nomen, access)
            : ObjectProxies.FindById(ObjectProxies.StandardParameterIoId(typeof(TData))) switch {
                { Type: var type } when type.IsAssignableTo(typeof(Parameter<TData>)) =>
                    (Parameter<TData>)Activator.CreateInstance(type, nomen, access)!,
                _ => throw Error.New($"no parameter type matches {typeof(TData)}").ToException(),
            };
        parameter.Requirement = requirement;
        return parameter;
    }
}

public sealed record ParameterField<TRecord>(ParameterRow Input, Func<IDataAccess, int, Fin<Func<TRecord, TRecord>>> Recall)
    where TRecord : notnull;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DataParameter<T> : Parameter<T>, GrasshopperIO.ICustomIoId {
    // --- [CONSTRUCTION]
    public DataParameter() : base(Nomen.Empty) { }

    public DataParameter(Nomen nomen, Access access = Access.Tree) : base(nomen, access) { }

    public DataParameter(GrasshopperIO.IReader reader) : base(reader) { }

    // --- [PERSISTENCE]
    Guid GrasshopperIO.ICustomIoId.CustomIoId => TypeAssistant switch {
        GrasshopperIO.ICustomIoId assistant => assistant.CustomIoId,
        _ => throw new UnreachableException(),
    };

    protected override IIcon IconInternal => TypeAssistant.Icon;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ParameterField {
    // --- [DECLARE]
    public static ParameterField<TRecord> Of<TRecord>(
        StateParameter<TRecord> kind, ParameterFace face, IGlyph glyph, int master, SpecialColour accent, Access access, Requirement requirement, Option<TRecord> initial)
        where TRecord : notnull =>
        kind.Accept(new Crossing<TRecord>(face, glyph, master, accent, initial, requirement, access));

    public static ParameterField<TRecord> Of<TRecord, TData>(
        Option<TRecord> initial, Lens<TRecord, TData> lens, ParameterFace face, Func<Nomen, Parameter<TData>> construct, Seq<(string Caption, TData Value)> presets,
        Option<Func<TData, Fin<TData>>> admit = default)
        where TRecord : notnull
        where TData : notnull =>
        new(ParameterRow.Of(face, nomen => {
            Parameter<TData> parameter = construct(nomen);
            _ = initial.Iter(record => parameter.Set(lens.Get(record)));
            return parameter;
        }, presets),
            (access, index) => Callbacks.Found(access.GetItem(index, out TData held), held)
                .Traverse(value => admit.Match(Some: accept => accept(value), None: () => Fin.Succ(value))).As()
                .Map(found => found.Match<Func<TRecord, TRecord>>(Some: value => record => lens.Set(value, record), None: static () => identity)));

    // --- [SOLVE]
    public static IO<Func<TRecord, TRecord>> Read<TRecord>(Seq<ParameterField<TRecord>> fields, int first, IDataAccess access)
        where TRecord : notnull =>
        Callbacks.Each(fields.Map((field, position) => IO.lift(() => field.Recall(access, first + position))))
            .Map(static sets => fun((TRecord seed) => sets.Fold(seed, static (record, set) => set(record))));

    internal sealed class Crossing<TRecord>(
        ParameterFace face, IGlyph glyph, int master, SpecialColour accent, Option<TRecord> initial, Requirement requirement, Access access)
        : IStateParameterVisitor<TRecord, ParameterField<TRecord>> where TRecord : notnull {

        public ParameterField<TRecord> Numeric<TValue, TKey>(Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, System.Numerics.INumber<TKey> =>
            Typed(lens, [], (plugin, id, name) =>
                DataTypes.Number(id, name, plugin.Icon, domain.Create, domain.Key)(plugin), Some(domain.Admit));

        public ParameterField<TRecord> OptionalNumeric<TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, System.Numerics.INumber<TKey> =>
            Typed(lens, [], (plugin, id, name) => DataTypes.OptionalNumber(id, name, plugin.Icon, domain.Create, domain.Key)(plugin),
                Some<Func<Gated<TValue>, Fin<Gated<TValue>>>>(gate => domain.Admit(gate.Value).Map(_ => gate)));

        public ParameterField<TRecord> Choice<TValue>(Lens<TRecord, TValue> lens)
            where TValue : ISmartEnum<string, TValue, ValidationError>, ICaptioned =>
            Typed(lens, toSeq(TValue.Items).Map(static item => (item.Caption, item)),
                static (plugin, id, name) => DataTypes.Keyed<TValue, string>(id, name, plugin.Icon, StoredText.Capture<TValue, string>)(plugin));

        public ParameterField<TRecord> OptionalChoice<TValue>(Lens<TRecord, Option<TValue>> lens, string absent)
            where TValue : ISmartEnum<string, TValue, ValidationError>, ICaptioned =>
            Typed(lens, Seq((Caption: absent, Value: Option<TValue>.None))
                + toSeq(TValue.Items).Map<(string Caption, Option<TValue> Value)>(static item => (item.Caption, Some(item))),
                static (plugin, id, name) => DataTypes.Schema(FieldTexts.Optional(StoredText.Capture<TValue, string>, StoredText.Recall<TValue, string>), id, name, plugin.Icon)(plugin));

        public ParameterField<TRecord> Variant<TValue>(Lens<TRecord, TValue> lens) where TValue : class =>
            Archived(lens, StateCase<TValue>.Items.Map(static item => (item.Caption, item.Default)), FieldTexts.Variant<TValue>());

        public ParameterField<TRecord> Enumerated<TEnum>(Lens<TRecord, TEnum> lens, Func<TEnum, string> caption) where TEnum : struct, Enum =>
            Archived(lens, toSeq(Enum.GetValues<TEnum>()).Map(item => (caption(item), item)), FieldTexts.Leaf(StoredText.Format, StoredText.Member<TEnum>));

        public ParameterField<TRecord> Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested> =>
            Archived(lens, [], FieldTexts.Record<TNested>());

        public ParameterField<TRecord> OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) where TNested : IStateRecord<TNested> => Archived(lens, [], FieldTexts.Gate(FieldTexts.Record<TNested>()));

        public ParameterField<TRecord> Toggle(Lens<TRecord, bool> lens) => Archived(lens, [], FieldTexts.Leaf(StoredText.Format, StoredText.Parse<bool>));

        public ParameterField<TRecord> Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> => Archived(lens, [], FieldTexts.Leaf(StoredText.Format, StoredText.Parse<TRaw>));

        public ParameterField<TRecord> OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> => Archived(lens, [], FieldTexts.Optional(StoredText.Format, StoredText.Parse<TRaw>));

        public ParameterField<TRecord> Color(Lens<TRecord, Swatch> lens) => Archived(lens, [], FieldTexts.Leaf(StoredText.Capture<Swatch, int>, StoredText.Recall<Swatch, int>));

        public ParameterField<TRecord> OptionalColor(Lens<TRecord, Gated<Swatch>> lens) => Archived(lens, [], FieldTexts.Gate(FieldTexts.Leaf(StoredText.Capture<Swatch, int>, StoredText.Recall<Swatch, int>)));

        public ParameterField<TRecord> Gradient(Lens<TRecord, Ramp> lens) => Archived(lens, [], FieldTexts.Leaf(StoredText.Capture<Ramp, string>, StoredText.Recall<Ramp, string>));

        public ParameterField<TRecord> Swatches<TValue>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, Seq<Swatch>, ValidationError>, IConvertible<Seq<Swatch>>, IObjectFactory<TValue, string, ValidationError>, IConvertible<string> =>
            Typed(lens, [], static (plugin, id, name) => DataTypes.Keyed<TValue, string>(id, name, plugin.Icon, StoredText.Capture<TValue, string>)(plugin));

        public ParameterField<TRecord> Keyed<TValue, TRaw>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            Typed(lens, [], static (plugin, id, name) => DataTypes.Keyed<TValue, TRaw>(id, name, plugin.Icon, StoredText.Capture<TValue, TRaw>)(plugin));

        public ParameterField<TRecord> OptionalKeyed<TValue, TRaw>(Lens<TRecord, Option<TValue>> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> => Archived(lens, [], FieldTexts.Optional(StoredText.Capture<TValue, TRaw>, StoredText.Recall<TValue, TRaw>));

        public ParameterField<TRecord> Loaded<TValue, TKey, TRaw>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
            where TKey : IObjectFactory<TKey, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> {
            FieldCodec<Option<TKey>> keys = FieldTexts.Optional(StoredText.Capture<TKey, TRaw>, StoredText.Recall<TKey, TRaw>);
            return Archived(lens, [], new FieldCodec<Option<TValue>>(value => keys.Capture(value.Map(key)), entries => keys.Recall(entries).Bind(found => found.Traverse(load).As())));
        }

        public ParameterField<TRecord> Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull =>
            Typed(lens, [], static (plugin, id, name) => DataTypes.Of<TValue>(id, name, plugin.Icon, identity, static value => value.ToString()!)(plugin));

        private ParameterField<TRecord> Archived<TData>(Lens<TRecord, TData> lens, Seq<(string Caption, TData Value)> presets, FieldCodec<TData> codec) where TData : notnull =>
            Typed(lens, presets, (plugin, id, name) => DataTypes.Schema(codec, id, name, plugin.Icon)(plugin));

        private ParameterField<TRecord> Typed<TData>(Lens<TRecord, TData> lens, Seq<(string Caption, TData Value)> presets,
            Func<DefinedGrasshopperPlugin, Guid, string, IO<Unit>> create, Option<Func<TData, Fin<TData>>> admit = default)
            where TData : notnull {
            (Type Type, Func<DefinedGrasshopperPlugin, IO<Unit>> Register) registration = DataTypes.Field<TData>(create);
            ParameterField<TRecord> field = Of(initial, lens, face, nomen => ParameterRow.Construct<TData>(nomen, access, requirement), presets, admit);
            return field with { Input = field.Input.Presented(glyph, master, accent) with { DataTypes = [registration] } };
        }
    }
}
