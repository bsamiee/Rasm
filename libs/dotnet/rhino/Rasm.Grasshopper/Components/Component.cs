using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Eto.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Doc;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.Types.Assistant;
using Grasshopper2.Types.Linguistic;
using Grasshopper2.UI;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.Toolbar;
using Rasm.Grasshopper.Data;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IComponentDefinition<TSelf> where TSelf : Component<TSelf>, IComponentDefinition<TSelf> {
    public static abstract Nomen Face { get; }

    public static abstract Func<IDataAccess, Fin<Unit>> Define(Ports<TSelf> ports);
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class Component<TSelf> : ModularComponent, GrasshopperIO.ICustomIoId, IPinCushion
    where TSelf : Component<TSelf>, IComponentDefinition<TSelf> {
    // --- [IDENTITY]
    private Option<(Ports<TSelf> Ports, Func<IDataAccess, Fin<Unit>> Solve)> declared;

    protected Component()
        : base(TSelf.Face switch {
            { Chapter: var chapter } face when string.IsNullOrWhiteSpace(chapter) => face.WithChapter(typeof(TSelf).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title),
            var face => face,
        }) { }

    protected Component(GrasshopperIO.IReader reader) : base(reader) {
        AddOutputs(new OutputAdder(this));
        _ = declared.Iter(held => held.Ports.Restore(reader));
    }

    public Guid CustomIoId => DataTypes.IoId(typeof(TSelf));

    public IEnumerable<Guid> SupportedPins => declared.ToSeq().Bind(static held => toSeq(held.Ports.SupportedPins));

    protected override IIcon? IconInternal => DataTypes.Icon(typeof(TSelf).Assembly, typeof(TSelf).Name);

    // --- [TOPOLOGY]
    protected sealed override void AddInputs(ModularInputAdder inputs) => declared = Declared();

    protected sealed override void AddOutputs(ModularOutputAdder outputs) { }

    public sealed override void VariableParameterMaintenance() => declared = declared.IsSome ? declared : Declared();

    private Option<(Ports<TSelf> Ports, Func<IDataAccess, Fin<Unit>> Solve)> Declared() =>
        new Ports<TSelf>((TSelf)this) switch {
            var ports => Some((ports, ports.Define())),
        };

    // --- [SOLVE]
    protected sealed override void Process(IDataAccess access) => declared.Iter(held => ignore(held.Solve(access).IfFail(error => Reported(access, error))));

    private static Unit Reported(IDataAccess access, Error error) {
        Seq<Error> leaves = toSeq(error.AsIterable()).Filter(static leaf => !leaf.Is(Errors.Cancelled)).Strict();
        _ = leaves.Iter(leaf => access.AddError(leaf.Message, leaf.Inner.Map(static cause => cause.Message).ValueUnsafe()));
        return leaves.Filter(static leaf => leaf.IsExceptional).Iter(static leaf => Hooks.Run(IO.fail<Unit>(leaf), unit));
    }

    // --- [PANEL]
    public override void AppendToInputPanel(InputPanel panel) {
        base.AppendToInputPanel(panel);
        Seq<BarItem> rows = declared.ToSeq().Bind(static held => held.Ports.Rows).Choose(static row => row()).Strict();
        if (!rows.IsEmpty) {
            using IDisposable category = panel.BeginCategory(Nomen.Name);
            _ = rows.Iter(row => panel.AddBar(drawCategoryLabels: false, row));
        }
    }
}

public sealed class Ports<TSelf> where TSelf : Component<TSelf>, IComponentDefinition<TSelf> {
    // --- [STATE]
    private const int Percent = 100;
    private const byte RoundTripDigits = 17;
    private readonly ModularInputAdder inputs;
    private readonly ModularOutputAdder outputs;
    private readonly (Seq<IParameter> Inputs, Seq<IParameter> Outputs) saved;
    private Seq<IParameter> restored;
    private Seq<Func<TSelf, IO<Unit>>> attachments;
    private Option<EnvIO> attachment;

    internal Ports(TSelf owner) {
        Owner = owner;
        inputs = new ModularInputAdder(new InputAdder(owner));
        outputs = new ModularOutputAdder(new OutputAdder(owner));
        saved = (toSeq(owner.Parameters).Filter(owner.Parameters.IsInput).Strict(), toSeq(owner.Parameters).Filter(owner.Parameters.IsOutput).Strict());
        restored = saved.Inputs + saved.Outputs;
        owner.DocumentChanged += (_, args) => Reattached(Optional(args.NewDocument));
    }

    public TSelf Owner { get; }

    internal LanguageExt.HashSet<Guid> SupportedPins { get; private set; } = HashSet<Guid>();

    internal Seq<Func<Option<BarItem>>> Rows { get; private set; }

    // --- [TOPOLOGY]
    internal Func<IDataAccess, Fin<Unit>> Define() {
        Func<IDataAccess, Fin<Unit>> solve = TSelf.Define(this);
        _ = restored.Iter(orphan => {
            if (Owner.Parameters.IsInput(orphan)) Owner.Parameters.RemoveInput(orphan, undo: null);
            else Owner.Parameters.RemoveOutput(orphan, undo: null);
        });
        restored = [];
        return solve;
    }

    internal Unit Restore(GrasshopperIO.IReader reader) =>
        Seq((List: Owner.ModularInputs, Key: "InputVisibleList", Parameters: saved.Inputs), (List: Owner.ModularOutputs, Key: "OutputVisibleList", Parameters: saved.Outputs))
            .Filter(side => reader.HasItem(side.Key))
            .Iter(side => side.Parameters.Zip(toSeq(reader.BooleanArray(side.Key))).Iter(pair => {
                int index = side.List.IndexOf(pair.First);
                if (index < 0) return;
                if (pair.Second) side.List.Show(index);
                else side.List.Hide(index);
            }));

    // --- [INPUTS]
    public Func<IDataAccess, Fin<T>> Item<T>(Parameter<T> parameter, Option<T> initial = default, Option<(string Category, Color Tint, bool Hidden)> modular = default) where T : notnull {
        (Parameter<T> bound, Func<IDataAccess, Option<Pear<T>>> read) = Read(parameter, Requirement.MustExist, initial, modular);
        return access => read(access).Bind(static pear => Optional(pear.Item)).ToFin(new GrasshopperFault(bound.UserName, GrasshopperReason.Missing));
    }

    public Func<IDataAccess, Fin<Pear<T>>> Pear<T>(Parameter<T> parameter, Option<T> initial = default, Option<(string Category, Color Tint, bool Hidden)> modular = default) where T : notnull {
        (Parameter<T> bound, Func<IDataAccess, Option<Pear<T>>> read) = Read(parameter, Requirement.MustExist, initial, modular);
        return access => read(access).ToFin(new GrasshopperFault(bound.UserName, GrasshopperReason.Missing));
    }

    public Func<IDataAccess, Fin<Option<T>>> Maybe<T>(Parameter<T> parameter, Option<T> initial = default, Option<(string Category, Color Tint, bool Hidden)> modular = default) where T : notnull {
        Func<IDataAccess, Option<Pear<T>>> read = Read(parameter, Requirement.MayBeMissing, initial, modular).Read;
        return access => read(access).Bind(static pear => Optional(pear.Item));
    }

    public Func<IDataAccess, Fin<TEnum>> Choice<TEnum>(Parameter<int> parameter, TEnum initial, Option<(string Category, Color Tint, bool Hidden)> modular = default) where TEnum : struct, Enum {
        (Parameter<int> bound, Func<IDataAccess, Option<Pear<int>>> read) = Read(parameter, Requirement.MustExist, Some(Convert.ToInt32(initial, CultureInfo.InvariantCulture)), modular);
        _ = bound.Presets.AddEnum<TEnum>();
        return access =>
            from pear in read(access).ToFin(new GrasshopperFault(bound.UserName, GrasshopperReason.Missing))
            let choice = (TEnum)Enum.ToObject(typeof(TEnum), pear.Item)
            from defined in GrasshopperFault.Unless(Enum.IsDefined(choice), GrasshopperReason.Invalid, choice)
            select choice;
    }

    public Func<IDataAccess, Fin<Twig<T>>> Twig<T>(Parameter<T> parameter, Option<(string Category, Color Tint, bool Hidden)> modular = default) where T : notnull {
        Parameter<T> bound = Input(parameter, Access.Twig, parameter.Requirement, None, modular);
        return access => access.GetTwig(Owner.Parameters.IndexOfInput(bound), out Twig<T> twig) ? twig : Garden.TwigEmpty<T>();
    }

    public Func<IDataAccess, Fin<Tree<T>>> Tree<T>(Parameter<T> parameter, Option<(string Category, Color Tint, bool Hidden)> modular = default) where T : notnull {
        Parameter<T> bound = Input(parameter, Access.Tree, parameter.Requirement, None, modular);
        return access => access.GetTree(Owner.Parameters.IndexOfInput(bound), out Tree<T> tree) ? tree : Garden.TreeEmpty<T>();
    }

    public Func<IDataAccess, Fin<Length>> Length(Parameter<double> parameter, Option<double> initial = default, Option<(string Category, Color Tint, bool Hidden)> modular = default) {
        Func<IDataAccess, Fin<double>> read = Item(parameter, initial, modular);
        Func<IDataAccess, double> scale = Units(Rhino.UnitSystem.Meters);
        return access => read(access).Map(value => UnitsNet.Length.FromMeters(UnitsNet.QuantityValue.FromDoubleRounded(value / scale(access), RoundTripDigits)));
    }

    private (Parameter<T> Bound, Func<IDataAccess, Option<Pear<T>>> Read) Read<T>(Parameter<T> parameter, Requirement requirement, Option<T> initial, Option<(string Category, Color Tint, bool Hidden)> modular) where T : notnull {
        Parameter<T> bound = Input(parameter, Access.Item, requirement, initial, modular);
        return (bound, access => access.GetPear(Owner.Parameters.IndexOfInput(bound), out Pear<T> pear) ? Optional(pear) : None);
    }

    // --- [OUTPUTS]
    public Func<IDataAccess, T, Unit> Output<T>(Parameter<T> parameter, Option<(string Category, Color Tint, bool Hidden)> modular = default) where T : notnull {
        Parameter<T> bound = Bound(parameter, Side.Output, Access.Item, parameter.Requirement, added => Appended(added, modular));
        return fun((IDataAccess access, T value) => access.SetItem(Owner.Parameters.IndexOfOutput(bound), value));
    }

    public Func<IDataAccess, Pear<T>, Unit> OutputPear<T>(Parameter<T> parameter, Option<(string Category, Color Tint, bool Hidden)> modular = default) where T : notnull {
        Parameter<T> bound = Bound(parameter, Side.Output, Access.Item, parameter.Requirement, added => Appended(added, modular));
        return fun((IDataAccess access, Pear<T> pear) => access.SetPear(Owner.Parameters.IndexOfOutput(bound), pear));
    }

    public Func<IDataAccess, Seq<T>, Unit> Outputs<T>(Parameter<T> parameter, Option<(string Category, Color Tint, bool Hidden)> modular = default) where T : notnull {
        Parameter<T> bound = Bound(parameter, Side.Output, Access.Twig, parameter.Requirement, added => Appended(added, modular));
        return fun((IDataAccess access, Seq<T> values) => access.SetTwig(Owner.Parameters.IndexOfOutput(bound), values.ToArray()));
    }

    public Func<IDataAccess, Tree<T>, Unit> OutputTree<T>(Parameter<T> parameter, Option<(string Category, Color Tint, bool Hidden)> modular = default) where T : notnull {
        Parameter<T> bound = Bound(parameter, Side.Output, Access.Tree, parameter.Requirement, added => Appended(added, modular));
        return fun((IDataAccess access, Tree<T> tree) => access.SetTree(Owner.Parameters.IndexOfOutput(bound), tree));
    }

    public Func<IDataAccess, Length, Unit> OutputLength(Parameter<double> parameter, Option<(string Category, Color Tint, bool Hidden)> modular = default) {
        Func<IDataAccess, double, Unit> write = Output(parameter, modular);
        Func<IDataAccess, double> scale = Units(Rhino.UnitSystem.Meters);
        return (access, length) => write(access, length.Meters.ToDouble() * scale(access));
    }

    // --- [CONTEXT]
    public Func<IDataAccess, double> Units(Rhino.UnitSystem basis) {
        SupportedPins = SupportedPins.AddOrUpdate(UnitSystemPin.Id);
        return access => {
            _ = access.GetUnitScaling(basis, out double scale);
            return scale;
        };
    }

    public Func<IDataAccess, (double Absolute, double Relative, Angle Angular)> Tolerance() {
        SupportedPins = SupportedPins.AddOrUpdate(AbsoluteTolerancePin.Id).AddOrUpdate(RelativeTolerancePin.Id).AddOrUpdate(AngularTolerancePin.Id);
        return static access => {
            _ = access.GetTolerance(out double absolute, out double relative);
            _ = access.GetTolerance(out Angle angular);
            return (absolute, relative, angular);
        };
    }

    public Func<IDataAccess, Language> Language() {
        SupportedPins = SupportedPins.AddOrUpdate(LinguisticCulturePin.Id);
        return static access => {
            _ = access.GetLanguage(out Language language);
            return language;
        };
    }

    public Action<IDataAccess, double> Progress() =>
        (access, fraction) => {
            if (access.Iterations is 1 || Owner.Threading is not ThreadingState.MultiThreaded)
                Owner.SetProgressPercentage((long)double.Round((access.Index + fraction) * Percent, MidpointRounding.ToEven), (long)access.Iterations * Percent);
        };

    // --- [SETTINGS]
    public Func<T> Setting<T>(GrasshopperIO.Name key, T fallback, Nomen face, Seq<T> choices = default) where T : notnull {
        Preference<T> preference = Preferences.Object(Owner, key, fallback, face);
        Rows = Rows.Add(() => Row(preference, fallback, face, choices));
        return () => Hooks.Run(preference.Read, fallback);
    }

    private static Option<BarItem> Row<T>(Preference<T> preference, T fallback, Nomen face, Seq<T> choices) where T : notnull =>
        (Hooks.Run(preference.Read, fallback), choices) switch {
            (bool state, _) => Some<BarItem>(new RadioToggle(icon: null, face, state, value => Written(preference, (T)(object)value))),
            (var current, { IsEmpty: false }) => Some<BarItem>(Choice(preference, current, face, choices)),
            (var current, _) when typeof(T).IsEnum => Some<BarItem>(Choice(preference, current, face, toSeq(Enum.GetValues(typeof(T)).Cast<T>()))),
            (var current, _) => Typed(preference, current, face)
                || Hooks.Run(IO.fail<Option<BarItem>>(new GrasshopperFault(typeof(T).ToString(), GrasshopperReason.Declined)), None),
        };

    private static DropDown<T> Choice<T>(Preference<T> preference, T current, Nomen face, Seq<T> choices) where T : notnull {
        DropDown<T> choice = new(face, value => Written(preference, value));
        _ = choices.Iter(item => choice.AddOption(item, new Nomen(item.ToString() ?? "", face.Info)));
        _ = choice.SetValue(current);
        return choice;
    }

    private static Option<BarItem> Typed<T>(Preference<T> preference, T current, Nomen face) where T : notnull =>
        Optional(TypeAssistantServer.Find<T>()).Filter(static assistant => assistant.ZupportsRoundTripping).Map(assistant => {
            Pear<T> existing = Garden.Pear(current);
            TextField field = new(icon: null, face, assistant.Format(current)) { Validator = text => assistant.Parse(constructor: null, text, existing).Success };
            field.TextChanged += (_, text) => Some(assistant.Parse(constructor: null, text, existing)).Filter(static parsed => parsed.Success).Iter(parsed => Written(preference, parsed.Value));
            return (BarItem)field;
        });

    private static Unit Written<T>(Preference<T> preference, T value) where T : notnull => Hooks.Run(preference.Write(value), unit);

    // --- [ATTRIBUTES]
    public Unit Body(SizeF minimum, SizeF maximum, Action<Context, Skin, Shade, RectangleF> paint, Option<IResponsive> widget = default) {
        _ = Owner.OverrideAttributes(new BodyAttributes(Owner, minimum, maximum, paint, widget));
        return unit;
    }

    // --- [LIFETIME]
    public Unit Attached(Func<TSelf, IO<Unit>> acquire) {
        attachments = attachments.Add(acquire);
        return unit;
    }

    private Unit Reattached(Option<Document> document) {
        _ = attachment.Iter(static scope => scope.Dispose());
        attachment = attachments.IsEmpty ? None : document.Map(static _ => EnvIO.New(token: CancellationToken.None));
        return attachment.Iter(scope => Hooks.Run(IO.liftVAsync(_ => attachments.TraverseM(acquire => acquire(Owner)).As().Map(static _ => unit).RunAsync(scope)), unit));
    }

    // --- [BINDING]
    private Parameter<T> Input<T>(Parameter<T> declared, Access access, Requirement requirement, Option<T> initial, Option<(string Category, Color Tint, bool Hidden)> modular) where T : notnull =>
        Bound(declared, Side.Input, access, requirement, added => Inserted(
            initial.Fold(added, static (held, value) => held.Set(value)),
            requirement,
            modular.Filter(_ => requirement is Requirement.MayBeMissing || initial.IsSome)));

    private Unit Inserted(IParameter added, Requirement requirement, Option<(string Category, Color Tint, bool Hidden)> modular) =>
        modular.Match(
            Some: placed => {
                if (placed.Hidden)
                    inputs.AddHidden(added, placed.Category, placed.Tint, requirement);
                else
                    inputs.Add(added, placed.Category, placed.Tint, requirement);
                return unit;
            },
            None: () => {
                inputs.RegularAdder.Add(added, requirement);
                return unit;
            });

    private Unit Appended(IParameter added, Option<(string Category, Color Tint, bool Hidden)> modular) =>
        modular.Match(
            Some: placed => {
                if (placed.Hidden)
                    outputs.AddHidden(added, placed.Category, placed.Tint);
                else
                    outputs.Add(added, placed.Category, placed.Tint);
                return unit;
            },
            None: () => {
                outputs.RegularAdder.Add(added);
                return unit;
            });

    private Parameter<T> Bound<T>(Parameter<T> declared, Side side, Access access, Requirement requirement, Func<Parameter<T>, Unit> add) where T : notnull {
        Seq<Parameter<T>> candidates = toSeq(restored.Filter(held => Owner.Parameters.IsInput(held) == (side is Side.Input)).OfType<Parameter<T>>())
            .Filter(held => held.GetType() == declared.GetType())
            .Strict();
        Option<Parameter<T>> matched = candidates.Find(held => string.Equals(held.UserName, declared.UserName, StringComparison.Ordinal))
            || candidates.Find(held => string.Equals(held.Nomen.Name, declared.Nomen.Name, StringComparison.Ordinal));
        restored = restored.Filter(held => !matched.Exists(claimed => ReferenceEquals(claimed, held))).Strict();
        Parameter<T> bound = matched.IfNone(() => {
            _ = add(declared);
            return declared;
        });
        bound.OverrideAccess(access, expire: false);
        bound.Requirement = requirement;
        return bound;
    }
}
