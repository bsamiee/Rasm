using System.Numerics;
using Rasm.UI;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.Rhino.UI.Rows;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MenuRows {
    // --- [OPTIONS]
    public static Func<RowScope, IO<Seq<Seq<Command<RowRule, Invoke>>>>> Options<TRecord>(RowSource<TRecord> source, Seq<IStateParameter<TRecord>> parameters) where TRecord : notnull =>
        parameters.Choose(parameter => parameter.Kind.Accept(new Listed<TRecord>(source, parameter))).Strict() switch {
            var listed => _ => IO.pure(Seq(listed)),
        };

    // --- [VISITORS]
    private sealed class Listed<TRecord>(RowSource<TRecord> source, IStateParameter<TRecord> parameter) : IStateParameterVisitor<TRecord, Option<Command<RowRule, Invoke>>>
        where TRecord : notnull {
        private CommandFace Face => new(parameter.Key, parameter.Text.Caption, Some(parameter.Text.Help), None);

        public Option<Command<RowRule, Invoke>> Numeric<TValue, TKey>(Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            new Command<RowRule, Invoke>.Run(Face, None, new Invoke.Effect(scope =>
                from held in scope.Read(source)
                from entered in HostDialogs.ShowNumberBox(RowText.Localize(parameter.Text.Caption, scope.Sink), RowText.Localize(parameter.Text.Help, scope.Sink), domain, lens.Get(held))
                from written in Written(scope, record => lens.Set(entered, record))
                select written));

        public Option<Command<RowRule, Invoke>> OptionalNumeric<TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            None;

        public Option<Command<RowRule, Invoke>> Choice<TValue>(Lens<TRecord, TValue> lens) where TValue : ISmartEnum<string, TValue, ValidationError> =>
            Radio(toSeq(TValue.Items).Map(static item => (Name: item.ToValue(), Caption: Captions.Of(item), Value: item)), lens);

        public Option<Command<RowRule, Invoke>> OptionalChoice<TValue>(Lens<TRecord, Option<TValue>> lens, string absent) where TValue : ISmartEnum<string, TValue, ValidationError> =>
            Radio((Name: "absent", Caption: absent, Value: Option<TValue>.None).Cons(toSeq(TValue.Items).Map(static item => (Name: item.ToValue(), Caption: Captions.Of(item), Value: Some(item)))), lens);

        public Option<Command<RowRule, Invoke>> Variant<TValue>(Lens<TRecord, TValue> lens) where TValue : class => None;

        public Option<Command<RowRule, Invoke>> Enumerated<TEnum>(Lens<TRecord, TEnum> lens) where TEnum : struct, Enum =>
            Radio(toSeq(Enum.GetValues<TEnum>()).Map(static member => (Name: member.ToString(), Caption: Captions.Of(member.ToString()), Value: member)), lens);

        public Option<Command<RowRule, Invoke>> Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested> => None;

        public Option<Command<RowRule, Invoke>> OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) where TNested : IStateRecord<TNested> => None;

        public Option<Command<RowRule, Invoke>> Toggle(Lens<TRecord, bool> lens) =>
            CommandRows.Toggle(Face, None, RowRule.When(source, lens.Get, parameter), (scope, on) => Written(scope, record => lens.Set(on, record)));

        public Option<Command<RowRule, Invoke>> Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> => None;

        public Option<Command<RowRule, Invoke>> OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> => None;

        public Option<Command<RowRule, Invoke>> Color(Lens<TRecord, SKColorF> lens) => None;

        public Option<Command<RowRule, Invoke>> OptionalColor(Lens<TRecord, Gated<SKColorF>> lens) => None;

        public Option<Command<RowRule, Invoke>> Gradient(Lens<TRecord, ColorRamp> lens) => None;

        public Option<Command<RowRule, Invoke>> Colors<TValue>(Lens<TRecord, TValue> lens) where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>> => None;

        public Option<Command<RowRule, Invoke>> Keyed<TValue, TRaw>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            None;

        public Option<Command<RowRule, Invoke>> OptionalKeyed<TValue, TRaw>(Lens<TRecord, Option<TValue>> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            None;

        public Option<Command<RowRule, Invoke>> Loaded<TValue, TKey, TRaw>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
            where TKey : IObjectFactory<TKey, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            None;

        public Option<Command<RowRule, Invoke>> Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull => None;

        private Command<RowRule, Invoke> Radio<TValue>(Seq<(string Name, string Caption, TValue Value)> members, Lens<TRecord, TValue> lens) where TValue : notnull =>
            new Command<RowRule, Invoke>.Menu(Face, None, [[new Command<RowRule, Invoke>.Choice(Face, None, members.Map(member =>
                new Command<RowRule, Invoke>.Run(new CommandFace($"{parameter.Key}-{member.Name}", member.Caption, None, None), None,
                    new Invoke.Effect(scope => Written(scope, record => lens.Set(member.Value, record)))) {
                    Marked = RowRule.When(source, record => EqualityComparer<TValue>.Default.Equals(lens.Get(record), member.Value), parameter),
                }))]]);

        private IO<Unit> Written(RowScope scope, Func<TRecord, TRecord> change) => source.Commit(scope, change, [parameter]).Map(static _ => unit);
    }
}
