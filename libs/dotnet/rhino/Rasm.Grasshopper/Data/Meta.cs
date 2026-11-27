using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using Grasshopper2.Data.Rules;
using Rasm.Rhino;

namespace Rasm.Grasshopper.Data;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Metadata {
    extension(MetaName name) {
        public Option<T> Read<T>(MetaData meta) =>
            Optional(meta).Bind(Option<T> (MetaData held) => held[name].IntendedData switch {
                T value => value,
                IPear { Item: T value } => value,
                _ => None,
            });

        public Fin<MetaData> Write<T>(MetaData meta, T value) =>
            MetaValue.TryCreate(value, out MetaValue entry, out string failure)
                ? MetaData.Merge(MergeBehaviour.Overwrite, meta, new MetaData((name, entry)))
                : Fault.Of(RhinoReason.Declined, name.ToString(), inner: Error.New(failure));
    }
}

public static class Trees {
    extension<T>(Tree<T> tree) {
        public Tree<T> Matching(SiteRule rule) =>
            toSeq(tree.Paths.Forwards)
                .Filter(rule.IsMatch)
                .Map(path => (Path: path, Twig: Garden.TwigFromPears(tree.Twigs[path].Pears.Where((_, index) => rule.IsMatch(index))))) switch {
                    var matched => Garden.TreeFromTwigs(new Paths(matched.Map(static row => row.Path)), matched.Map(static row => row.Twig)),
                };

        public Fin<Tree<T>> Matching(string siteRule) =>
            Optional(SiteRule.TryParse(siteRule, out string message))
                .ToFin(Fault.Of(RhinoReason.Unreadable, siteRule, inner: Optional(message).Map(static text => Error.New(text))))
                .Map(tree.Matching);
    }
}
