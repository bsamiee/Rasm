using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Eto.Forms;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rhino.Display;
using UnitsNet;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class PreferenceRows {

    // --- [SOURCES]

    extension<TControl, T>(ValueRow<TControl, T> row)
        where TControl : Control
        where T : notnull {
        public ValueRow<TControl, T> Over(Preference<T> preference) =>
            Bound(row, preference.Read.Map(Some), preference.Write, preference.Swap, Signal(preference));

        public ValueRow<TControl, T> Over(Preference<Option<T>> preference) =>
            Bound(row, preference.Read, held => preference.Write(held), held => preference.Swap(held), Signal(preference));

        public ValueRow<TControl, T> Over(Seq<Preference<T>> targets) =>
            row with {
                Current = targets.TraverseM(static target => target.Read).As().Map(static values => values.Head.Filter(first => values.ForAll(value => EqualityComparer<T>.Default.Equals(value, first)))),
                Changes = row.Changes + targets.Map(static target => Signal(target)),
                Edited = edit => edit.Applied.Traverse(value => targets.TraverseM(target => target.Write(value)).As()).As().Map(unit),
            };
    }

    // --- [ROWS]

    extension(Row) {
        public static Row Of(string label, Preference<bool> preference) => Row.Of(label, Values.Check(None, Ignored).Over(preference));

        public static Row Of(string label, Preference<double> preference, NumberRange range) => Row.Of(label, Values.Number(None, range, Ignored).Over(preference));

        public static Row Of(string label, Preference<Length> preference, Length low, Length high) => Row.Of(label, Values.Distance(None, low, high, modelUnits: true, Ignored).Over(preference));

        public static Row Of(string label, Preference<Angle> preference) => Row.Of(label, Values.Turn(None, Ignored).Over(preference));

        public static Row Of(string label, Preference<Color4f> preference) => Row.Of(label, Values.Swatch(None, Ignored).Over(preference));

        public static Row Of(string label, Preference<string> preference) => Row.Of(label, Values.Line(None, Ignored).Over(preference));

        public static Row Of<T>(string label, Preference<T> preference, Seq<ChoiceItem<T>> items) where T : notnull => Row.Of(label, Values.Choice(None, items, Ignored).Over(preference));

        public static Row Of<T>(string label, Preference<Option<T>> preference, Seq<ChoiceItem<T>> items) where T : notnull => Row.Of(label, Values.Choice(None, items, Ignored).Over(preference));

        public static Row Of<T>(string label, Preference<T> preference) where T : struct, Enum => Row.Of(label, Values.Enumerated<T>(None, Ignored).Over(preference));

        public static Row Of<T>(string label, Preference<T> preference, Func<T, string> caption) where T : class, ISmartEnum<string, T, ValidationError> =>
            Row.Of(label, Values.Choice(None, toSeq(T.Items).Map(item => new ChoiceItem<T>(item, caption(item))), Ignored).Over(preference));
    }

    // --- [EDITS]

    internal static HostEvent<Unit> Signal<T>(Preference<T> preference, [CallerArgumentExpression(nameof(preference))] string label = "") => new(label, deliver => preference.Changed(_ => deliver(unit)));

    private static ValueRow<TControl, T> Bound<TControl, T>(ValueRow<TControl, T> row, IO<Option<T>> current, Func<T, IO<Unit>> write, Func<T, IO<IO<Unit>>> swap, HostEvent<Unit> signal)
        where TControl : Control
        where T : notnull =>
        Ref(Option<IO<Unit>>.None) switch {
            var restore => row with {
                Current = current,
                Changes = row.Changes.Add(signal),
                Edited = edit => edit.Switch(
                    (Restore: restore, Write: write, Swap: swap),
                    changed: static (state, held) => held.Applies
                        ? Taken(state.Restore).Bind(_ => state.Write(held.Value))
                        : atomicIO(() => state.Restore.Value).Bind(prior => prior.IsSome
                            ? state.Write(held.Value)
                            : state.Swap(held.Value).Bind(back => atomicIO(() => state.Restore.Value = Some(back))).Map(unit)),
                    cancel: static (state, _) => Taken(state.Restore).Bind(static prior => prior.IfNone(IO.pure(unit)))),
            },
        };

    private static IO<Option<IO<Unit>>> Taken(Ref<Option<IO<Unit>>> restore) =>
        atomicIO(() => {
            Option<IO<Unit>> held = restore.Value;
            restore.Value = None;
            return held;
        });

    private static IO<Unit> Ignored<T>(Edit<T> _) where T : notnull => IO.pure(unit);
}
