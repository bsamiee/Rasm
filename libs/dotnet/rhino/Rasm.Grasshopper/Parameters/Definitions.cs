using System.Numerics;
using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Framework;
using Grasshopper2.Parameters;
using Grasshopper2.Types.Assistant;
using Grasshopper2.UI.Icon;
using Rasm.Rhino;
using Rasm.UI;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.Grasshopper.Parameters;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ParameterFace(ParameterText Text, string Code, Option<(string Category, Option<Color> Tint, bool Shown)> Modularity) {
    public static ParameterFace Of<TRecord>(IStateParameter<TRecord> parameter, string code) => new(parameter.Text, code, None);

    public Nomen Localized(IPlugInSink sink) => new(RowText.Localize(Text.Caption, sink), RowText.Localize(Text.Help, sink));
}

public sealed record ParameterRow(ParameterFace Face, Func<IPlugInSink, IO<IParameter>> Create) {
    // --- [DECLARE]
    public Seq<Func<IParameter, IPlugInSink, IO<Unit>>> Attach { get; init; }
    public Seq<(Type Type, Func<IGrasshopperPlugin, IO<Unit>> Register)> DataTypes { get; internal init; }

    public static ParameterRow Of(ParameterFace face, Func<Nomen, IParameter> construct) =>
        new(face, sink => IO.lift(() => construct(face.Localized(sink))).Bind(parameter => Coded(parameter, face, sink)));

    public static ParameterRow Of<TData>(ParameterFace face, Func<Nomen, Fin<Parameter<TData>>> construct, Seq<(string Caption, TData Value)> presets)
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
        from attached in Attach.TraverseM(attach => attach(parameter, sink)).As()
        from _ in IO.lift(() => Face.Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color, Requirement?>)inputs.Add : inputs.AddHidden)(
                    parameter, RowText.Localize(optional.Category, sink), optional.Tint.IfNone(Colors.Transparent), null),
            None: () => inputs.RegularAdder.Add(parameter, parameter.Requirement)))
        select parameter;

    public IO<IParameter> Output(ModularOutputAdder outputs, IPlugInSink sink) =>
        from parameter in Create(sink)
        from attached in Attach.TraverseM(attach => attach(parameter, sink)).As()
        from _ in IO.lift(() => Face.Modularity.Match(
            Some: optional =>
                (optional.Shown ? (Action<IParameter, string, Color>)outputs.Add : outputs.AddHidden)(
                    parameter, RowText.Localize(optional.Category, sink), optional.Tint.IfNone(Colors.Transparent)),
            None: () => outputs.RegularAdder.Add(parameter)))
        select parameter;

    private static IO<IParameter> Coded(IParameter parameter, ParameterFace face, IPlugInSink sink) =>
        IO.lift(() => parameter.UserName = RowText.Localize(face.Code, sink)).Map(parameter);

    internal ParameterRow Presented(GlyphSource glyph) =>
        this with {
            Attach = Attach.Add((parameter, sink) =>
                from icon in CanvasIcons.Vector(sink, glyph)
                from presented in IO.lift(() => parameter.ModifyIcon(icon, overrideExtrinsicIcon: false))
                select unit),
        };

    internal static Fin<Parameter<TData>> Construct<TData>(Nomen nomen, Grasshopper2.Parameters.Access access, Requirement requirement) =>
        (TypeAssistantServer.Find<TData>() is GrasshopperIO.ICustomIoId
            ? new DataParameter<TData>(nomen, access)
            : Refused.UnlessFound(ObjectProxies.TryEmit<Parameter<TData>>(ObjectProxies.StandardParameterIoId(typeof(TData))), nameof(ObjectProxies.TryEmit)).Map(parameter => {
                parameter.ModifyNameAndInfo(nomen.Name, nomen.Info);
                parameter.OverrideAccess(access);
                return parameter;
            }))
        .Map(parameter => {
            parameter.Requirement = requirement;
            return parameter;
        });
}

public sealed record ParameterField<TRecord>(ParameterRow Input, Func<IDataAccess, int, Fin<Func<TRecord, TRecord>>> Recall) where TRecord : IStateRecord<TRecord> {
    // --- [DECLARE]
    public static ParameterField<TRecord> Of(
        IStateParameter<TRecord> parameter, string code, GlyphSource glyph, Grasshopper2.Parameters.Access access, Requirement requirement, Option<TRecord> initial) =>
        parameter.Kind.Accept(new Mapped(parameter, ParameterFace.Of(parameter, code), glyph, initial, requirement, access));

    public static ParameterField<TRecord> Of<TData>(
        Option<TRecord> initial, Lens<TRecord, TData> lens, ParameterFace face, Func<Nomen, Fin<Parameter<TData>>> construct, Seq<(string Caption, TData Value)> presets,
        Option<Func<TData, Fin<TData>>> admit = default)
        where TData : notnull =>
        new(ParameterRow.Of(face, nomen => construct(nomen).Map(parameter => {
            _ = initial.Iter(record => parameter.Set(lens.Get(record)));
            return parameter;
        }), presets),
            (access, index) => Callbacks.Found(access.GetItem(index, out TData held), held)
                .Traverse(admit.IfNone(static value => value)).As()
                .Map(found => found.Match<Func<TRecord, TRecord>>(Some: value => record => lens.Set(value, record), None: static () => identity)));

    // --- [SOLVE]
    public static IO<Func<TRecord, TRecord>> Read(Seq<ParameterField<TRecord>> fields, int first, IDataAccess access) =>
        Callbacks.Each(fields.Map((field, position) => IO.lift(() => field.Recall(access, first + position))))
            .Map(static sets => fun((TRecord seed) => sets.Fold(seed, static (record, set) => set(record))));

    // --- [MAPPING]
    private sealed class Mapped(
        IStateParameter<TRecord> parameter, ParameterFace face, GlyphSource glyph, Option<TRecord> initial, Requirement requirement, Grasshopper2.Parameters.Access access)
        : IStateParameterVisitor<TRecord, ParameterField<TRecord>> {
        public ParameterField<TRecord> Numeric<TValue, TKey>(Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            Typed(lens, [], (id, name, icon) => DataTypes.Number(id, name, icon, Texts(lens), domain.Create, domain.Key), Some<Func<TValue, Fin<TValue>>>(domain.Checked));

        public ParameterField<TRecord> OptionalNumeric<TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            Stored(lens, [], Some<Func<Gated<TValue>, Fin<Gated<TValue>>>>(gate => domain.Checked(gate.Value).Map(_ => gate)));

        public ParameterField<TRecord> Choice<TValue>(Lens<TRecord, TValue> lens)
            where TValue : ISmartEnum<string, TValue, ValidationError> =>
            Typed(lens, toSeq(TValue.Items).Map(static item => (Captions.Of(item), item)), (id, name, icon) => DataTypes.Keyed<TValue, string>(id, name, icon, Texts(lens)));

        public ParameterField<TRecord> OptionalChoice<TValue>(Lens<TRecord, Option<TValue>> lens, string absent)
            where TValue : ISmartEnum<string, TValue, ValidationError> =>
            Stored(lens, Seq((Caption: absent, Value: Option<TValue>.None)) + toSeq(TValue.Items).Map<(string Caption, Option<TValue> Value)>(static item => (Captions.Of(item), Some(item))));

        public ParameterField<TRecord> Variant<TValue>(Lens<TRecord, TValue> lens) where TValue : class =>
            Stored(lens, StateCase<TValue>.Items.Map(static item => (item.Caption, item.Default)));

        public ParameterField<TRecord> Enumerated<TEnum>(Lens<TRecord, TEnum> lens) where TEnum : struct, Enum =>
            Stored(lens, toSeq(Enum.GetValues<TEnum>()).Map(static item => (Captions.Of(item.ToString()), item)));

        public ParameterField<TRecord> Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested> => Stored(lens, []);

        public ParameterField<TRecord> OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) where TNested : IStateRecord<TNested> => Stored(lens, []);

        public ParameterField<TRecord> Toggle(Lens<TRecord, bool> lens) => Stored(lens, []);

        public ParameterField<TRecord> Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> => Stored(lens, []);

        public ParameterField<TRecord> OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> => Stored(lens, []);

        public ParameterField<TRecord> Color(Lens<TRecord, SKColorF> lens) => Stored(lens, []);

        public ParameterField<TRecord> OptionalColor(Lens<TRecord, Gated<SKColorF>> lens) => Stored(lens, []);

        public ParameterField<TRecord> Gradient(Lens<TRecord, ColorRamp> lens) => Stored(lens, []);

        public ParameterField<TRecord> Colors<TValue>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>> =>
            Stored(lens, []);

        public ParameterField<TRecord> Keyed<TValue, TRaw>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            Typed(lens, [], (id, name, icon) => DataTypes.Keyed<TValue, TRaw>(id, name, icon, Texts(lens)));

        public ParameterField<TRecord> OptionalKeyed<TValue, TRaw>(Lens<TRecord, Option<TValue>> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> => Stored(lens, []);

        public ParameterField<TRecord> Loaded<TValue, TKey, TRaw>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
            where TKey : IObjectFactory<TKey, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> => Stored(lens, []);

        public ParameterField<TRecord> Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull =>
            Typed(lens, [], static (id, name, icon) => DataTypes.Of<TValue>(id, name, icon, identity, static value => value.ToString()!));

        private DataTexts<TData> Texts<TData>(Lens<TRecord, TData> lens) where TData : notnull =>
            DataTexts<TData>.Of(parameter, TRecord.Default, lens);

        private ParameterField<TRecord> Stored<TData>(Lens<TRecord, TData> lens, Seq<(string Caption, TData Value)> presets, Option<Func<TData, Fin<TData>>> admit = default)
            where TData : notnull =>
            Typed(lens, presets, (id, name, icon) => DataTypes.Stored(id, name, icon, Texts(lens)), admit);

        private ParameterField<TRecord> Typed<TData>(Lens<TRecord, TData> lens, Seq<(string Caption, TData Value)> presets,
            Func<Guid, string, IIcon, Func<IGrasshopperPlugin, IO<Unit>>> create, Option<Func<TData, Fin<TData>>> admit = default)
            where TData : notnull {
            (Type Type, Func<IGrasshopperPlugin, IO<Unit>> Register) registration = DataTypes.Field<TData>(create);
            ParameterField<TRecord> field = Of(initial, lens, face, nomen => ParameterRow.Construct<TData>(nomen, access, requirement), presets, admit);
            return field with { Input = field.Input.Presented(glyph) with { DataTypes = [registration] } };
        }
    }
}
