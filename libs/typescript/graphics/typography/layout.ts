import { Array, Option, Record, Schema, Struct } from 'effect';
import type { Font } from 'fontkit';
import type { Face } from 'harfbuzzjs';

// --- [MODELS] --------------------------------------------------------------------------

const LayoutTable = Schema.Literals(['GSUB', 'GPOS']);
class Layout extends Schema.Class<Layout, { readonly brand: unique symbol }>('Layout')({
    features: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String) })).annotate({
        description: 'GSUB, GPOS, AAT, and kern features, name labels ssNN and cvNN',
    }),
    languageSystems: Schema.Array(Schema.Struct({ table: LayoutTable, script: Schema.String, language: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Script default when absent' })), features: Schema.Array(Schema.String) })).annotate({
        description: 'Features each GSUB and GPOS language system enables (locl, vert)',
    }),
}) {
    static readonly of = ({ handle, parsed, named }: { readonly handle: Face; readonly parsed: Font; readonly named: (id: number | undefined) => Option.Option<string> }) => {
        const defaultLanguageSystem = 0xff_ff;
        return Layout.make({
            features: Record.toEntries(
                Array.groupBy([...LayoutTable.literals.flatMap((table) => handle.getTableFeatureTags(table).map((tag, at) => ({ tag, name: named(handle.getFeatureNameIds(table, at)?.uiLabelNameId) }))), ...parsed.availableFeatures.map((tag) => ({ tag, name: Option.none<string>() }))], Struct.get('tag')),
            ).map(([tag, records]) => ({ tag, name: Option.firstSomeOf(records.map(Struct.get('name'))) })),
            languageSystems: LayoutTable.literals
                .flatMap((table) => handle.getTableScriptTags(table).map((script, at) => ({ table, script, at })))
                .flatMap(({ table, script, at }) => [
                    { table, script, language: Option.none<string>(), features: handle.getLanguageFeatureTags(table, at, defaultLanguageSystem) },
                    ...handle.getScriptLanguageTags(table, at).map((language, index) => ({ table, script, language: Option.some(language), features: handle.getLanguageFeatureTags(table, at, index) })),
                ]),
        });
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Layout, LayoutTable };
