using System.Security.Cryptography;
using System.Text;
using Eto.Drawing;
using Grasshopper2;
using Grasshopper2.Display;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Framework;
using Grasshopper2.Types.Colour;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.TabbedPanel;
using Rhino;

namespace Rasm.Grasshopper;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GrasshopperTheme(string Name) {
    // --- [DECLARATION]
    public Func<PropertyId, Option<Binding>> Skin { get; init; } = static _ => None;
    public Option<Func<Guises, Guises>> Preview { get; init; }
    public Seq<IO<IO<Unit>>> Overrides { get; init; }

    // --- [DEFINITION]
    public Fin<SkinDefinition> Definition {
        get {
            SkinDefinition definition = SkinDefinition.Default;
            Seq<(PropertyId Id, Binding Binding)> chosen = toSeq(SkinProperties.All).Choose(id => Skin(id).Map(binding => (id, binding))).Strict();
            _ = chosen.Iter(row => definition.Unbind(row.Id));
            Fin<Seq<Unit>> bound = chosen.Traverse(row => Try.lift(fun(() => definition.Bind(row.Id, row.Binding))).Run()
                .MapFail(error => new GrasshopperFault(SkinProperties.ToToken(row.Id), GrasshopperReason.Invalid, Inner: error))).As();
            Fin<Seq<Skin>> evaluated = Seq(PropertyId.Lit, PropertyId.Dim).Traverse(mode => Try.lift(() => definition.ToSkin(mode)).Run()
                .MapFail(error => new GrasshopperFault($"{Name} {mode}", GrasshopperReason.Invalid, Inner: error))).As();
            string text = definition.ToText();
            SkinDefinition parsed = SkinDefinition.Parse(text, out string[] errors);
            Fin<Unit> read = errors is []
                ? GrasshopperFault.Unless(string.Equals(parsed.ToText(), text, StringComparison.Ordinal), GrasshopperReason.Unreadable, Name)
                : Error.Many(toSeq(errors).Map(static error => (Error)new GrasshopperFault(error, GrasshopperReason.Unreadable)));
            return (GrasshopperFault.Unless(SkinServer.IsValidName(Name), GrasshopperReason.Invalid, Name), bound, evaluated, read)
                .Apply((_, _, _, _) => definition).As();
        }
    }

    // --- [INSTALL]
    private const string StyleExtension = ".ghstyle";
    private const string PaletteExtension = ".ghdic";

    public IO<Unit> Install =>
        from installed in unless(Settings.SandBox,
            from plugin in IO.lift(static () => Hooks.Session.Plugin)
            from definition in IO.lift(() => Definition)
            from described in IO.lift(() => {
                definition.Author = plugin.Name;
                definition.Description = plugin.Info;
            })
            from skin in Refreshed(SkinServer.PathOf(Name), Encoding.UTF8.GetBytes(definition.ToText()), File.ReadAllBytes, IO.lift(() => SkinServer.Save(Name, definition)))
            from styled in Preview.Map(static preview => preview(Defaults.GrasshopperDefault)).Traverse(guises => Path.Combine(Folders.DisplayStylesFolder, Name + StyleExtension) switch {
                var style => Refreshed(style, GrasshopperIO.IO.WriteStorableToByteArray(guises, "Value"), File.ReadAllBytes, IO.lift(() => GrasshopperIO.IO.WriteStorableToFile(guises, style))),
            }).As()
            let listed = fun(static (string[] lines) => Encoding.UTF8.GetBytes(string.Join('\n', lines.Skip(1))))
            let swatches = Path.Combine(Folders.ResourceFolder(ResourceFolder.Colours), Name + PaletteExtension)
            let palette = new NamedPalette(toSeq(definition.BoundProperties)
                .Filter(id => SkinProperties.KindOf(id) is PropertyType.Colour && !definition[id].IsInherited)
                .Map(id => new NamedColour($"{SkinProperties.Mode(id)} {SkinPropertyText.Title(id)}", definition.Resolve(id).ToEto(), source: Name)))
            from coloured in Refreshed(swatches, listed(NamedPalette.WriteToStrings(palette)), path => listed(File.ReadAllLines(path)), IO.lift(() => NamedPalette.WriteToFile(swatches, palette)))
            from tabs in IO.lift(static () => new SettingsFolder("ComponentTabs"))
            let tint = definition.Resolve(SkinProperties.WithMode(PropertyId.Shade | PropertyId.Apex | PropertyId.Selected, PropertyId.Lit)).ToEto().ToHex(includeAlpha: false)
            let chapters = toSeq(ObjectProxies.Proxies.Where(proxy => proxy.Plugin == plugin).Select(static proxy => proxy.Nomen.Chapter).Distinct(StringComparer.Ordinal))
            let rules = new NamedRules(Name + NamedRules.Extension, Rules.Empty.Add([Rule.CreateCiteRule(NamedRules.DefaultRules), .. chapters.Map(chapter => Rule.CreateTintRule(Selector.FromChapter(chapter), tint))]))
            from tinted in when(!chapters.IsEmpty, Refreshed(tabs.Folder + rules.Name, Encoding.UTF8.GetBytes(rules.Rules.ToString()), File.ReadAllBytes,
                IO.lift(() => GrasshopperFault.Unless(rules.Store(tabs), GrasshopperReason.Declined, rules.Name)))).As()
            select unit).As()
        from followed in Active.Changed(Active.Write)
        select unit;

    private static IO<Unit> Refreshed(string path, byte[] content, Func<string, byte[]> read, IO<Unit> write) =>
        from values in IO.lift(static () => Hooks.Session.Values)
        from present in IO.lift(() => File.Exists(path) ? Some(read(path)) : Option<byte[]>.None)
        from recorded in IO.lift(() => values.Get(path, ""))
        let stamp = Convert.ToHexString(SHA256.HashData(content))
        let same = present.Exists(bytes => bytes.AsSpan().SequenceEqual(content))
        let owned = same || present.ForAll(bytes => string.Equals(Convert.ToHexString(SHA256.HashData(bytes)), recorded, StringComparison.Ordinal))
        from written in when(owned && !same, write).As()
        from stamped in when(owned && !string.Equals(recorded, stamp, StringComparison.Ordinal), IO.lift(() => values.Set(path, stamp)) >> Hooks.Saved).As()
        select unit;

    // --- [APPLY]
    public Preference<bool> Active =>
        new(
            IO.lift(() => SkinServer.IsCurrent(Name)),
            active =>
                from held in IO.lift(() => Hooks.Themes.Find(Name))
                from changed in active
                    ? when(held.IsNone, IO.lift(() => Hooks.Themes.Add(Name, unitIO))
                        >> Apply.Bind(inverse => IO.lift(() => Hooks.Themes.SetItem(Name, inverse))).Catch(error => IO.lift(() => Hooks.Themes.Remove(Name)) >> IO.fail<Unit>(error)).As()).As()
                    : IO.lift(() => Hooks.Themes.Remove(Name))
                        >> held.IfNone(unitIO)
                        >> IO.lift(() => SkinServer.IsCurrent(Name)).Bind(static current => when(current, Preferences.Of(Settings.CanvasSkin).Write(Settings.CanvasSkin.Default)).As())
                select unit,
            Some<Func<IO<Unit>, IO<Unit>>>(static notify => Hooks.On<EventArgs>(static h => Settings.CanvasSkin.Changed += h, static h => Settings.CanvasSkin.Changed -= h, _ => notify)));

    public IO<Unit> Scoped(Canvas canvas) =>
        from definition in IO.lift(() => Definition)
        from restore in use(
            Preferences.Property<(Skin? Lit, Skin? Dim)>(
                () => (ReferenceEquals(canvas.SkinLit, SkinServer.Lit) ? null : canvas.SkinLit, ReferenceEquals(canvas.SkinDim, SkinServer.Dim) ? null : canvas.SkinDim),
                skins => {
                    canvas.SkinLit = skins.Lit;
                    canvas.SkinDim = skins.Dim;
                    canvas.Invalidate();
                }).Swap((definition.ToSkin(PropertyId.Lit), definition.ToSkin(PropertyId.Dim))),
            static restore => restore)
        select unit;

    private IO<IO<Unit>> Apply =>
        from installed in IO.lift(() => GrasshopperFault.Unless(SkinServer.Exists(Name), GrasshopperReason.Missing, Name))
        from document in IO.lift(static () => Optional(RhinoDoc.ActiveDoc))
        from restores in Preferences.Of(Settings.CanvasSkin).Swap(Name).Map(restore => IO.lift(() => SkinServer.IsCurrent(Name)).Bind(current => when(current, restore).As()))
            .Cons(document.Bind(_ => Preview).Map(static preview => Preferences.Property(
                    static () => ReferenceEquals(Defaults.UserDefault, Defaults.GrasshopperDefault) ? null : Defaults.UserDefault,
                    static guises => Defaults.UserDefault = guises)
                .Swap(preview(Defaults.GrasshopperDefault))).ToSeq()
                .Concat(Overrides))
            .TraverseM(static swap => use(swap, static restore => restore)).As().BracketFail()
        select (from outcome in restores.Rev().Map<K<IO, Unit>>(static restore => restore).PartitionFallible().As()
                from restored in guard(outcome.Fails.IsEmpty, Error.Many(outcome.Fails))
                select unit);

    // --- [SIGNAL]
    public static IO<Unit> Changed(Func<Skin, IO<Unit>> react) =>
        from armed in IO.lift(static () => SkinServer.Lit)
        let current = IO.lift(static () => SkinServer.CurrentSkin).Bind(react)
        from followed in Seq(
                Hooks.On(static h => SkinServer.CurrentChanged += h, static h => SkinServer.CurrentChanged -= h, current),
                Preferences.Of(Settings.DarkMode).Changed(_ => current),
                Preferences.Of(Settings.TypefaceSans).Changed(_ => current),
                Preferences.Of(Settings.TypefaceSerif).Changed(_ => current),
                Preferences.Of(Settings.TypefaceMono).Changed(_ => current),
                Preferences.Of(Settings.TypefaceScript).Changed(_ => current),
                Preferences.Of(Settings.Animations).Changed(_ => current),
                Hooks.On(static h => Layout.LayoutConstantsChanged += h, static h => Layout.LayoutConstantsChanged -= h, current))
            .TraverseM(static subscription => subscription).As()
        select unit;

    // --- [DERIVATION]
    public static Func<PropertyId, Option<Binding>> Tinted(OpenColor.Family neutral, OpenColor.Family accent) =>
        SkinDefinition.Default switch {
            var host => id => id is var (_, family, _, variant) && family is not (PropertyId.Messaging or PropertyId.Icons) && !variant.HasFlag(PropertyId.Tentative)
                ? Optional(host[id])
                    .Filter(static binding => !binding.IsInherited && binding.Kind is PropertyType.Colour)
                    .Map(_ => host.Resolve(id).ToEto() switch {
                        var source => Binding.Literal(SkinValue.From(new Color(OpenColor.Approximate(variant.HasFlag(PropertyId.Selected) ? accent : neutral, source), source.A))),
                    })
                : None,
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SkinKeys {
    extension(PropertyId id) {
        public void Deconstruct(out PropertyId mode, out PropertyId family, out PropertyId role, out PropertyId variant) =>
            (mode, family, role, variant) = (SkinProperties.Mode(id), SkinProperties.Family(id), SkinProperties.Role(id), SkinProperties.Variant(id));
    }
}
