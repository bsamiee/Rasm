using System.Numerics;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Numeric;
using Rasm.UI;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.Rhino.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ParameterRows<TRecord>(RowSource<TRecord> Source, Func<IStateParameter<TRecord>, RowRules> Rules) where TRecord : notnull {
    public Option<IO<Option<SKColorF>>> Pick { get; init; }
    public Func<IStateParameter<TRecord>, Option<Seq<ControlRow>>> Replaced { get; init; } = static _ => None;

    public Seq<ControlRow> Of<TParameter>(IEnumerable<TParameter> parameters) where TParameter : IStateParameter<TRecord> =>
        toSeq(parameters).Bind(parameter => Replaced(parameter).IfNone(() => parameter.Kind.Accept(new Mapped(this, parameter, Rules(parameter)))));

    private Seq<ControlRow> Nested<TNested>(Seq<string> path, Lens<TRecord, TNested> lens, RowRules rules) where TNested : IStateRecord<TNested> =>
        new ParameterRows<TNested>(Source.Within(path, lens, TNested.Parameters), _ => rules) { Pick = Pick }.Of(TNested.Parameters);

    private RowRules Showing(RowRules rules, IStateParameter<TRecord> parameter, Func<TRecord, bool> holds) =>
        RowRule.When(Source, holds, parameter) switch {
            var shown => rules with { Visible = Some(rules.Visible.Map(visible => visible & shown).IfNone(shown)) },
        };

    // --- [VISITORS]
    private sealed class Mapped(ParameterRows<TRecord> owner, IStateParameter<TRecord> parameter, RowRules rules) : IStateParameterVisitor<TRecord, Seq<ControlRow>> {
        private RowSource<TRecord> Source => owner.Source;

        public Seq<ControlRow> Numeric<TValue, TKey>(Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            [Sliders.Numeric(Source, parameter, lens, domain, rules)];

        public Seq<ControlRow> OptionalNumeric<TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            [Sliders.OptionalNumeric(Source, parameter, lens, domain, rules)];

        public Seq<ControlRow> Choice<TValue>(Lens<TRecord, TValue> lens) where TValue : ISmartEnum<string, TValue, ValidationError> =>
            [ChoiceRows.Choice(Source, parameter, lens, static scope => IO.pure(ChoiceRows.Items<TValue>(None, scope.Sink)), rules)];

        public Seq<ControlRow> OptionalChoice<TValue>(Lens<TRecord, Option<TValue>> lens, string absent) where TValue : ISmartEnum<string, TValue, ValidationError> =>
            [ChoiceRows.Choice(Source, parameter, lens, scope => IO.pure(ChoiceRows.Optional(absent, ChoiceRows.Items<TValue>(None, scope.Sink), scope.Sink)), rules)];

        public Seq<ControlRow> Variant<TValue>(Lens<TRecord, TValue> lens) where TValue : class =>
            ChoiceRows.Choice(
                    Source, parameter,
                    Lens<TRecord, StateCase<TValue>>.New(
                        current => StateCase<TValue>.Items.First(item => item.Holds(lens.Get(current))),
                        picked => current => picked.Holds(lens.Get(current)) ? current : lens.Set(picked.Default, current)),
                    static scope => IO.pure(StateCase<TValue>.Items.Map(item => new ChoiceItem<StateCase<TValue>>(item, RowText.Localize(item.Caption, scope.Sink).Local, None))), rules)
                .Cons(StateCase<TValue>.Items.Bind(item => item.Accept(new Cased<TValue>(owner, parameter, lens, item.Key, rules))));

        public Seq<ControlRow> Enumerated<TEnum>(Lens<TRecord, TEnum> lens) where TEnum : struct, Enum =>
            [ChoiceRows.Choice(Source, parameter, lens, static scope => IO.pure(ChoiceRows.Members<TEnum>(scope.Sink)), rules)];

        public Seq<ControlRow> Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested> =>
            new ControlRow.Group(parameter.Text, rules).Cons(owner.Nested([parameter.Key], lens, rules));

        public Seq<ControlRow> OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) where TNested : IStateRecord<TNested> =>
            ToggleRows.Check(Source, parameter, Prelude.lens(lens, Gated<TNested>.EnabledEntry.Lens), rules).Cons(owner.Nested(
                [parameter.Key, Gated<TNested>.ValueEntry.Key], Prelude.lens(lens, Gated<TNested>.ValueEntry.Lens),
                owner.Showing(rules, parameter, current => lens.Get(current).Enabled)));

        public Seq<ControlRow> Toggle(Lens<TRecord, bool> lens) => [ToggleRows.Check(Source, parameter, lens, rules)];

        public Seq<ControlRow> Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> => Entry;

        public Seq<ControlRow> OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> => Entry;

        public Seq<ControlRow> Color(Lens<TRecord, SKColorF> lens) => [ColorRows.Swatch(Source, parameter, lens, owner.Pick, rules)];

        public Seq<ControlRow> OptionalColor(Lens<TRecord, Gated<SKColorF>> lens) => [ColorRows.Swatch(Source, parameter, lens, owner.Pick, rules)];

        public Seq<ControlRow> Gradient(Lens<TRecord, ColorRamp> lens) => [ColorRows.Gradient(Source, parameter, lens, rules)];

        public Seq<ControlRow> Colors<TValue>(Lens<TRecord, TValue> lens) where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>> =>
            [ColorRows.Swatches(Source, parameter, lens, owner.Pick, rules)];

        public Seq<ControlRow> Keyed<TValue, TRaw>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> => Entry;

        public Seq<ControlRow> OptionalKeyed<TValue, TRaw>(Lens<TRecord, Option<TValue>> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> => Entry;

        public Seq<ControlRow> Loaded<TValue, TKey, TRaw>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
            where TKey : IObjectFactory<TKey, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            [TextRows.Field(Source, parameter, new EntryForm.SingleLine(), Some((Eto.FileAction.OpenFile, Seq<FileTypeRow>())), rules)];

        public Seq<ControlRow> Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull => [];

        private Seq<ControlRow> Entry => [TextRows.Field(Source, parameter, new EntryForm.SingleLine(), None, rules)];
    }

    private sealed class Cased<TValue>(ParameterRows<TRecord> owner, IStateParameter<TRecord> parameter, Lens<TRecord, TValue> lens, string key, RowRules rules)
        : IStateCaseVisitor<TValue, Seq<ControlRow>> where TValue : class {
        public Seq<ControlRow> Case<TChild>() where TChild : class, TValue, IStateRecord<TChild> =>
            owner.Nested(
                [parameter.Key, key],
                Lens<TRecord, TChild>.New(current => lens.Get(current) as TChild ?? TChild.Default, child => current => lens.Set(child, current)),
                owner.Showing(rules, parameter, current => lens.Get(current) is TChild));
    }
}
