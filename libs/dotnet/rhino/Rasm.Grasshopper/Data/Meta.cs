using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using Grasshopper2.Data.Rules;

namespace Rasm.Grasshopper.Data;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record MetaKey<T>(MetaName Name) {
    public Option<T> Read(MetaData meta) =>
        Optional(meta).Bind(Option<T> (MetaData held) => held[Name].IntendedData switch {
            T value => value,
            IPear { Item: T value } => value,
            _ => None,
        });

    public Fin<MetaData> Write(MetaData meta, T value) =>
        MetaValue.TryCreate(value, out MetaValue entry, out string failure)
            ? meta - Name + (Name, entry)
            : new GrasshopperFault(Name.ToString(), GrasshopperReason.Declined, Inner: Error.New(failure));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Trees {
    extension<T>(Tree<T> tree) {
        public Fin<Tree<T>> Matching(string siteRule) =>
            Optional(SiteRule.TryParse(siteRule, out string message))
                .ToFin(new GrasshopperFault(siteRule, GrasshopperReason.Unreadable, Inner: Optional(message).Map(static text => Error.New(text))))
                .Map(rule => toSeq(tree.Paths.Forwards).Filter(rule.IsMatch).Map(path => (Path: path, Twig: Garden.TwigFromPears(tree.Twigs[path].Pears.Where((_, index) => rule.IsMatch(index))))))
                .Map(static matched => Garden.TreeFromTwigs(new Paths(matched.Map(static row => row.Path)), matched.Map(static row => row.Twig)));
    }
}
