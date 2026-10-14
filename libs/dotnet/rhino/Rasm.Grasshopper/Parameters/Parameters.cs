using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Framework;
using Grasshopper2.Parameters;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI.Icon;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;

namespace Rasm.Grasshopper.Parameters;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ParameterFace(ParameterText Text, string Code, Option<(string Category, Option<Color> Tint, bool Shown)> Modularity) {
    public Nomen Localized(IPlugInSink sink) => Some<object>(sink) switch {
        var table => new(RowText.Localize(Text.Caption, table: table), RowText.Localize(Text.Help, table: table)),
    };
}

public sealed record ParameterRow(ParameterFace Face, Func<IPlugInSink, IO<IParameter>> Create) {
    // --- [DECLARE]
    public Option<Func<IParameter, IPlugInSink, IO<Unit>>> Attach { get; init; }
    public Seq<(Type Type, Func<DefinedGrasshopperPlugin, IO<Unit>> Register)> DataTypes { get; internal init; }

    public static ParameterRow Of(ParameterFace face, Func<Nomen, IParameter> construct) =>
        new(face, sink => IO.lift(() => construct(face.Localized(sink))).Bind(parameter => Coded(parameter, face, sink)));

    public static ParameterRow Of<TData>(ParameterFace face, Func<Nomen, Parameter<TData>> construct, Seq<(string Caption, TData Value)> presets)
        where TData : notnull =>
        new(face, sink =>
            from parameter in IO.lift(() => construct(face.Localized(sink)))
            from _ in IO.lift(() => Callbacks.Each(
                presets,
                preset => parameter.Presets.Add(RowText.Localize(preset.Caption, table: Some<object>(sink)), Conversions.Unset(Option<string>.None), preset.Value),
                nameof(Presets<>.Add)))
            from coded in Coded(parameter, face, sink)
            select coded);

    public static ParameterRow Opaque<TData>(
        ParameterFace face, IGlyph glyph, int master, SpecialColour accent, Access access, Requirement requirement, Seq<(string Caption, TData Value)> presets = default)
        where TData : notnull =>
        Of(face, nomen => Construct<TData>(nomen, access, requirement), presets).Presented(glyph, master, accent) with {
            DataTypes = [Parameters.DataTypes.Field<TData>(static (plugin, id, name) =>
                Parameters.DataTypes.Of<TData>(id, name, plugin.Icon, identity, static value => value.ToString()!)(plugin))],
        };

    public static ParameterRow Of<TData>(
        StateParameter<TData> kind, ParameterFace face, IGlyph glyph, int master, SpecialColour accent, Access access, Requirement requirement, Option<TData> initial)
        where TData : notnull =>
        kind.Accept(new ParameterField.Crossing<TData>(face, glyph, master, accent, initial, requirement, access)).Input;

    // --- [REALIZE]
    public IO<IParameter> Input(ModularInputAdder inputs, IPlugInSink sink) =>
        from parameter in Create(sink)
        from attached in Attach.Match(Some: attach => attach(parameter, sink), None: static () => unitIO)
        from _ in IO.lift(() => Face.Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color, Requirement?>)inputs.Add : inputs.AddHidden)(
                    parameter, RowText.Localize(optional.Category, table: Some<object>(sink)), optional.Tint.IfNone(Colors.Transparent), null),
            None: () => inputs.RegularAdder.Add(parameter, parameter.Requirement)))
        select parameter;

    public IO<IParameter> Output(ModularOutputAdder outputs, IPlugInSink sink) =>
        from parameter in Create(sink)
        from attached in Attach.Match(Some: attach => attach(parameter, sink), None: static () => unitIO)
        from _ in IO.lift(() => Face.Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color>)outputs.Add : outputs.AddHidden)(
                    parameter, RowText.Localize(optional.Category, table: Some<object>(sink)), optional.Tint.IfNone(Colors.Transparent)),
            None: () => outputs.RegularAdder.Add(parameter)))
        select parameter;

    private static IO<IParameter> Coded(IParameter parameter, ParameterFace face, IPlugInSink sink) =>
        IO.lift(() => parameter.UserName = RowText.Localize(face.Code, table: Some<object>(sink))).Map(_ => parameter);

    internal ParameterRow Presented(IGlyph glyph, int master, SpecialColour accent) =>
        this with {
            Attach = Some<Func<IParameter, IPlugInSink, IO<Unit>>>((parameter, sink) =>
            from attached in Attach.Match(Some: attach => attach(parameter, sink), None: static () => unitIO)
            from icon in CanvasIcons.Accented(sink, glyph, master, accent)
            from presented in IO.lift(() => parameter.ModifyIcon(icon, overrideExtrinsicIcon: false))
            select unit),
        };

    internal static Parameter<TData> Construct<TData>(Nomen nomen, Access access, Requirement requirement) {
        Parameter<TData> parameter = TypeAssistantServer.Find<TData>() is GrasshopperIO.ICustomIoId
            ? new DataParameter<TData>(nomen, access)
            : ObjectProxies.FindById(ObjectProxies.StandardParameterIoId(typeof(TData))) switch {
                { Type: var type } when type.IsAssignableTo(typeof(Parameter<TData>)) =>
                    (Parameter<TData>)Activator.CreateInstance(type, nomen, access)!,
                _ => throw new RefusedDataType(typeof(TData), nameof(ObjectProxies.StandardParameterIoId)).ToException(),
            };
        parameter.Requirement = requirement;
        return parameter;
    }
}

public sealed record ParameterField<TRecord>(
    ParameterRow Input, ParameterRow Output, Func<IDataAccess, int, Fin<Func<TRecord, TRecord>>> Recall, Func<TRecord, Fin<object>> Capture)
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
        _ => throw new RefusedDataType(typeof(T), nameof(TypeAssistantServer.Find)).ToException(),
    };

    protected override IIcon IconInternal => TypeAssistant.Icon;
}

public sealed class ValueParameterVisitor(string owner, IGlyph glyph, int master, SpecialColour accent, Option<(string Category, Option<Color> Tint, bool Shown)> modularity) : IValueKeyVisitor<ParameterField<ValueSet>> {
    public ParameterField<ValueSet> Visit<TValue, TRaw>(ValueKey<TValue, TRaw> key) where TValue : notnull where TRaw : notnull {
        EntryKey entry = new(owner, Seq(key.Name));
        ParameterField<TValue> field = ParameterField.Of(key.Kind, new ParameterFace(ParameterText.Of(key.Caption, key.Help), key.Name, modularity), glyph, master, accent, None, Requirement.MayBeMissing);
        return new(field.Input, field.Output,
            (access, index) => Fin.Succ(Callbacks.Found(access.GetItem(index, out TValue held), held).Match<Func<ValueSet, ValueSet>>(
                Some: value => values => new ValueSet(values.Entries.AddOrUpdate(entry, key.Text(value))), None: static () => identity)),
            values => values.Entries.Find(entry).Match(Some: text => entry.Read(text, key.Read).Map<object>(static value => value), None: () => Fin.Succ<object>(key.Default)));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Hints {
    public static UiNumber Of<TKey>(Presentation<TKey> presentation, TKey value, (TKey Low, TKey High) bounds)
        where TKey : struct, System.Numerics.INumber<TKey> =>
        presentation.Track(value, bounds) switch {
            var range => new(int.Min(presentation.Decimals.IfNone(UiNumber.MaxDecimals), UiNumber.MaxDecimals), decimal.CreateSaturating(value),
                decimal.CreateSaturating(range.Low), decimal.CreateSaturating(range.High)),
        };
}

public static class ParameterField {
    // --- [DECLARE]
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
            ParameterRow.Of(face, construct, presets),
            (access, index) => Callbacks.Found(access.GetItem(index, out TData held), held)
                .Traverse(value => admit.Match(Some: accept => accept(value), None: () => Fin.Succ(value))).As()
                .Map(found => found.Match<Func<TRecord, TRecord>>(Some: value => record => lens.Set(value, record), None: static () => identity)),
            record => Fin.Succ<object>(lens.Get(record)));

    public static ParameterField<TRecord> Of<TRecord>(
        StateParameter<TRecord> kind, ParameterFace face, IGlyph glyph, int master, SpecialColour accent, Option<TRecord> initial, Requirement requirement)
        where TRecord : notnull =>
        kind.Accept(new Crossing<TRecord>(face, glyph, master, accent, initial, requirement, Access.Item));

    public static Seq<ParameterField<TRecord>> Join<TRecord, TParameter>(
        Seq<(TParameter Parameter, ParameterText Text)> texts, Func<TParameter, (string Code, Option<(string Category, Option<Color> Tint, bool Shown)> Modularity)> placement, IGlyph glyph, int master, SpecialColour accent)
        where TRecord : notnull
        where TParameter : IStateParameter<TRecord> =>
        texts.Map(pair => placement(pair.Parameter) switch {
            var placed => Of(pair.Parameter.Kind, new ParameterFace(pair.Text, placed.Code, placed.Modularity), glyph, master, accent, None, Requirement.MayBeMissing),
        }).Strict();

    // --- [SOLVE]
    public static IO<Func<TRecord, TRecord>> Read<TRecord>(Seq<ParameterField<TRecord>> fields, int first, IDataAccess access)
        where TRecord : notnull =>
        from parts in fields.Map<K<IO, Func<TRecord, TRecord>>>((field, position) =>
            IO.lift(() => field.Recall(access, first + position))).PartitionFallible().As()
        from _ in unless(parts.Fails.IsEmpty, IO.fail<Unit>(Error.Many(parts.Fails))).As()
        select fun((TRecord seed) => parts.Succs.Fold(seed, static (record, set) => set(record)));

    public static IO<Unit> Write<TRecord>(Seq<ParameterField<TRecord>> fields, int first, TRecord record, IDataAccess access)
        where TRecord : notnull =>
        Callbacks.Each(fields.Map((field, position) => IO.lift(() => field.Capture(record).Map(value => (Index: first + position, Value: value)))))
            .Bind(placed => IO.lift(() => placed.Iter(value => access.SetItem(value.Index, value.Value))));

    internal sealed class Crossing<TRecord>(
        ParameterFace face, IGlyph glyph, int master, SpecialColour accent, Option<TRecord> initial, Requirement requirement, Access access)
        : IStateParameterVisitor<TRecord, ParameterField<TRecord>> where TRecord : notnull {

        public ParameterField<TRecord> Numeric<TValue, TKey>(Lens<TRecord, TValue> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation)
            where TValue : notnull
            where TKey : struct, System.Numerics.INumber<TKey> =>
            Typed(lens, [], (plugin, id, name) =>
                DataTypes.Number(id, name, plugin.Icon, create, key)(plugin), Some<Func<TValue, Fin<TValue>>>(value => presentation.Admit(value, bounds)));

        public ParameterField<TRecord> OptionalNumeric<TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation)
            where TValue : notnull
            where TKey : struct, System.Numerics.INumber<TKey> =>
            Typed(lens, [], (plugin, id, name) => DataTypes.OptionalNumber(id, name, plugin.Icon, create, key)(plugin),
                Some<Func<Gated<TValue>, Fin<Gated<TValue>>>>(gate => presentation.Admit(gate.Value, bounds).Map(_ => gate)));

        public ParameterField<TRecord> Choice<TValue, TError>(Lens<TRecord, TValue> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> =>
            Typed(lens, toSeq(TValue.Items).Map(item => (face.Text.Items[item.ToValue()].Caption, item)),
                static (plugin, id, name) => DataTypes.Keyed<TValue, string, TError>(id, name, plugin.Icon, StoredText.Capture<TValue, string>)(plugin));

        public ParameterField<TRecord> OptionalChoice<TValue, TError>(Lens<TRecord, Option<TValue>> lens)
            where TValue : ISmartEnum<string, TValue, TError>
            where TError : Error, IValidationError<TError> =>
            Typed(lens, face.Text.Absent.Map(static caption => (Caption: caption, Value: Option<TValue>.None)).ToSeq()
                + toSeq(TValue.Items).Map<(string Caption, Option<TValue> Value)>(item => (face.Text.Items[item.ToValue()].Caption, Some(item))),
                static (plugin, id, name) => DataTypes.Schema(FieldTexts.Optional(StoredText.Capture<TValue, string>, static text => IO.lift(StoredText.Recall<TValue, string, TError>(text))), id, name, plugin.Icon)(plugin));

        public ParameterField<TRecord> Variant<TValue>(Lens<TRecord, TValue> lens) where TValue : class =>
            Archived(lens, StateCase<TValue>.Items.Map(item => (face.Text.Items[item.Key].Caption, item.Default)), FieldTexts.Variant<TValue>());

        public ParameterField<TRecord> Enumerated<TEnum>(Lens<TRecord, TEnum> lens) where TEnum : struct, Enum =>
            Archived(lens, toSeq(Enum.GetValues<TEnum>()).Map(item => (face.Text.Items[StoredText.Format(item)].Caption, item)), FieldTexts.Leaf(StoredText.Format, static text => IO.lift(StoredText.Member<TEnum>(text))));

        public ParameterField<TRecord> Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested> =>
            Archived(lens, [], FieldTexts.Record<TNested>());

        public ParameterField<TRecord> OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) where TNested : IStateRecord<TNested> => Archived(lens, [], FieldTexts.Gate(FieldTexts.Record<TNested>()));

        public ParameterField<TRecord> Toggle(Lens<TRecord, bool> lens) => Archived(lens, [], FieldTexts.Leaf(StoredText.Format, static text => IO.lift(StoredText.Parse<bool>(text))));

        public ParameterField<TRecord> Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> => Archived(lens, [], FieldTexts.Leaf(StoredText.Format, static text => IO.lift(StoredText.Parse<TRaw>(text))));

        public ParameterField<TRecord> OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> => Archived(lens, [], FieldTexts.Optional(StoredText.Format, static text => IO.lift(StoredText.Parse<TRaw>(text))));

        public ParameterField<TRecord> Color(Lens<TRecord, Swatch> lens) => Archived(lens, [], FieldTexts.Leaf(StoredText.Capture<Swatch, int>, static text => IO.lift(StoredText.Recall<Swatch, int, InvalidPixelValue>(text))));

        public ParameterField<TRecord> OptionalColor(Lens<TRecord, Gated<Swatch>> lens) => Archived(lens, [], FieldTexts.Gate(FieldTexts.Leaf(StoredText.Capture<Swatch, int>, static text => IO.lift(StoredText.Recall<Swatch, int, InvalidPixelValue>(text)))));

        public ParameterField<TRecord> Gradient(Lens<TRecord, Ramp> lens) => Archived(lens, [], FieldTexts.Leaf(StoredText.Capture<Ramp, string>, static text => IO.lift(StoredText.Recall<Ramp, string, InvalidPixelValue>(text))));

        public ParameterField<TRecord> Swatches<TValue, TError>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, Seq<Swatch>, TError>, IConvertible<Seq<Swatch>>, IObjectFactory<TValue, string, TError>, IConvertible<string>
            where TError : Error, IValidationError<TError> =>
            Typed(lens, [], static (plugin, id, name) => DataTypes.Keyed<TValue, string, TError>(id, name, plugin.Icon, StoredText.Capture<TValue, string>)(plugin));

        public ParameterField<TRecord> Keyed<TValue, TRaw, TError>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> =>
            Typed(lens, [], static (plugin, id, name) => DataTypes.Keyed<TValue, TRaw, TError>(id, name, plugin.Icon, StoredText.Capture<TValue, TRaw>)(plugin));

        public ParameterField<TRecord> OptionalKeyed<TValue, TRaw, TError>(Lens<TRecord, Option<TValue>> lens)
            where TValue : IObjectFactory<TValue, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> => Archived(lens, [], FieldTexts.Optional(StoredText.Capture<TValue, TRaw>, static text => IO.lift(StoredText.Recall<TValue, TRaw, TError>(text))));

        public ParameterField<TRecord> Loaded<TValue, TKey, TRaw, TError>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
            where TKey : IObjectFactory<TKey, TRaw, TError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw>
            where TError : Error, IValidationError<TError> => Archived(lens, [], FieldTexts.Optional(value => StoredText.Capture<TKey, TRaw>(key(value)), text => IO.lift(StoredText.Recall<TKey, TRaw, TError>(text)).Bind(load)));

        public ParameterField<TRecord> Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull =>
            Typed(lens, [], static (plugin, id, name) => DataTypes.Of<TValue>(id, name, plugin.Icon, identity, static value => value.ToString()!)(plugin));

        private ParameterField<TRecord> Archived<TData>(Lens<TRecord, TData> lens, Seq<(string Caption, TData Value)> presets, FieldCodec<TData> codec) where TData : notnull =>
            Typed(lens, presets, (plugin, id, name) => DataTypes.Schema(codec, id, name, plugin.Icon)(plugin));

        private ParameterField<TRecord> Typed<TData>(Lens<TRecord, TData> lens, Seq<(string Caption, TData Value)> presets,
            Func<DefinedGrasshopperPlugin, Guid, string, IO<Unit>> create, Option<Func<TData, Fin<TData>>> admit = default)
            where TData : notnull {
            (Type Type, Func<DefinedGrasshopperPlugin, IO<Unit>> Register) registration = DataTypes.Field<TData>(create);
            ParameterField<TRecord> field = Of(initial, lens, face, nomen => ParameterRow.Construct<TData>(nomen, access, requirement), presets, admit);
            return field with {
                Input = field.Input.Presented(glyph, master, accent) with { DataTypes = [registration] },
                Output = field.Output.Presented(glyph, master, accent) with { DataTypes = [registration] },
            };
        }
    }
}
