import { Array, Effect, Function, Option, Record, Schema, SchemaGetter, Struct } from 'effect';
import type { Font } from 'fontkit';
import { type Face, Feature, type FeatureNameIds } from 'harfbuzzjs';
import { TypographyError } from '#/error';
import type { Settings } from '#/settings';
import type { Names } from '#/shaping/internal/names';
import { Lookup, type Tables } from '#/shaping/internal/tables';
import type { ShapedText } from '#/shaping/shaped-text';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHOOSABLE = /^(?:aalt|salt|cv\d{2})$/u;
const STYLISTIC = /^ss\d{2}$/u;
const VARIANT = /^cv\d{2}$/u;
const EXCLUSIVE: readonly ReadonlySet<string>[] = [new Set(['lnum', 'onum']), new Set(['pnum', 'tnum'])];
const LABELS = { name: 'uiLabelNameId', tooltip: 'uiTooltipTextNameId', sample: 'sampleTextNameId' } as const satisfies Record.ReadonlyRecord<string, keyof FeatureNameIds>;

// --- [MODELS] --------------------------------------------------------------------------

const LayoutTable = Schema.Literals(['GSUB', 'GPOS']);
const Substitution = Schema.Struct({ coverage: Schema.ReadonlySet(Schema.Int), alternates: Schema.ReadonlyMap(Schema.Int, Schema.Array(Schema.Int)) });
class Layout extends Schema.Class<Layout, { readonly brand: unique symbol }>('Layout')({
    features: Schema.Array(
        Schema.Struct({
            tag: Schema.String,
            ...Record.map(LABELS, () => Schema.OptionFromOptionalKey(Schema.String)),
            parameters: Schema.Array(Schema.String).annotate({ description: 'cvNN named parameter labels' }),
            characters: Schema.Array(Schema.String).annotate({ description: 'cvNN characters the variant serves' }),
            rivals: Schema.Array(Schema.String).annotate({ description: 'Features enabling this one unsets: the other figure form or spacing, and stylistic sets substituting a glyph this set substitutes' }),
        }),
    ).annotate({ description: 'GSUB, GPOS, AAT, and kern features, ssNN FeatureParams naming a label, cvNN a label, tooltip, sample text, parameters and characters' }),
    languageSystems: Schema.Array(Schema.Struct({ table: LayoutTable, script: Schema.String, language: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Script default when absent' })), features: Schema.Array(Schema.String) })).annotate({
        description: 'Features each GSUB and GPOS language system enables (locl, vert)',
    }),
    substitutions: Schema.ReadonlyMap(Schema.String, Substitution)
        .pipe(Schema.encodeTo(Schema.optionalKey(Schema.Never), { decode: SchemaGetter.forbidden(() => 'Encoded layout omits substitutions'), encode: SchemaGetter.omit() }))
        .annotate({ description: 'GSUB coverage and alternates of aalt, salt, jalt, cvNN and ssNN by tag' }),
}) {
    static readonly Choice = Schema.Struct({ cluster: Schema.Int, glyph: Schema.Int, feature: Schema.String }).annotate({ description: 'Alternate glyph of one shaped cluster, feature selecting it in run features' });
    static readonly of = Effect.fn('Layout.of')(function* ({ handle, sfnt, names, tables }: { readonly handle: Face; readonly sfnt: Font; readonly names: Names; readonly tables: typeof Tables.Type }) {
        const records = yield* Effect.try({ try: () => featureRecords(handle, names), catch: TypographyError.unreadable('GSUB FeatureParams') });
        const substituting = records.filter(({ table, tag }) => table === 'GSUB' && (tag === 'jalt' || CHOOSABLE.test(tag) || STYLISTIC.test(tag)));
        const indexes = Array.dedupe(substituting.flatMap(Struct.get('lookups')));
        const decoded = yield* Option.match(tables.GSUB, { onNone: () => Effect.succeed(Array.empty<typeof Lookup.Type>()), onSome: ({ lookupList }) => Effect.forEach(indexes, (index) => Schema.decodeUnknownEffect(Lookup)(lookupList.get(index))) });
        const subtables = new Map(Array.zip(indexes, decoded.map(Struct.get('subTables'))));
        const substitutions = new Map(Record.collect(Array.groupBy(substituting, Struct.get('tag')), (tag, rows) => [tag, substitutionOf(rows.flatMap(Struct.get('lookups')), subtables)] as const));
        const unlabeled = { ...Record.map(LABELS, () => Option.none<string>()), parameters: Array.empty<string>(), characters: Array.empty<string>() };
        const grouped = Array.groupBy([...records, ...sfnt.availableFeatures.map((tag) => ({ tag, labels: Option.none<typeof unlabeled>() }))], Struct.get('tag'));
        return Layout.make({
            features: Record.collect(grouped, (tag, rows) => ({
                tag,
                ...Option.getOrElse(Option.firstSomeOf(rows.map(Struct.get('labels'))), Function.constant(unlabeled)),
                rivals: rivalsOf(substitutions, Record.keys(grouped), tag),
            })),
            languageSystems: languageSystems(handle),
            substitutions,
        });
    });

    choices({ glyphs }: ShapedText): readonly (typeof Layout.Choice.Type)[] {
        const choosable = Array.fromIterable(this.substitutions.keys()).filter((tag) => CHOOSABLE.test(tag));
        return Array.cartesian(glyphs, choosable).flatMap(([{ glyph, cluster }, tag]) => this.variants(tag, glyph).map((alternate, index) => ({ cluster, glyph: alternate, feature: new Feature(tag, index + 1, cluster, cluster + 1).toString() })));
    }
    covers(tag: string, glyph: number) {
        return this.substitutions.get(tag)?.coverage.has(glyph) ?? false;
    }
    variants(tag: string, glyph: number): readonly number[] {
        return this.substitutions.get(tag)?.alternates.get(glyph) ?? [];
    }
    exclusive(features: NonNullable<(typeof Settings.Partial.Type)['features']>) {
        const rivals = Record.fromIterableWith(this.features, ({ tag, rivals: unset }) => [tag, unset] as const);
        return Record.fromEntries(Record.toEntries(features).flatMap(([tag, value]) => [...(value !== null && value > 0 ? Option.getOrElse(Record.get(rivals, tag), Array.empty<string>).map((rival) => [rival, null] as const) : []), [tag, value] as const]));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const featureRecords = (handle: Face, names: Names) => {
    const gsub = Option.map(Option.fromUndefinedOr(handle.referenceTable('GSUB')), (bytes) => new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength));
    return LayoutTable.literals.flatMap((table) => handle.getTableFeatureTags(table).map((tag, at) => featureRecord(handle, names, gsub, { table, tag, at })));
};
const featureRecord = (handle: Face, names: Names, gsub: Option.Option<DataView>, { table, tag, at }: { readonly table: typeof LayoutTable.Type; readonly tag: string; readonly at: number }) => ({
    table,
    tag,
    lookups: handle.getFeatureLookups(table, at),
    labels: Option.map(Option.fromUndefinedOr(handle.getFeatureNameIds(table, at)), (ids) => ({
        ...Record.map(LABELS, (key) => names.findById(ids[key])),
        parameters: Array.getSomes(ids.paramUiLabelNameIds.map((id) => names.findById(id))),
        characters: table === 'GSUB' && VARIANT.test(tag) ? Option.match(gsub, { onNone: Array.empty<string>, onSome: (view) => charactersAt(view, at) }) : [],
    })),
});
const charactersAt = (gsub: DataView, featureIndex: number): readonly string[] => {
    const { featureListOffset, featureCount, featureRecordSize, featureOffset, charCount, character, uint24, byteValues } = { featureListOffset: 6, featureCount: 2, featureRecordSize: 6, featureOffset: 4, charCount: 12, character: 14, uint24: 3, byteValues: 0x1_00 } as const;
    const list = gsub.getUint16(featureListOffset);
    const feature = list + gsub.getUint16(list + featureCount + featureIndex * featureRecordSize + featureOffset);
    const params = feature + gsub.getUint16(feature);
    const count = gsub.getUint16(params + charCount);
    return count === 0
        ? []
        : Array.makeBy(count, (index) => {
              const at = params + character + index * uint24;
              return String.fromCodePoint(gsub.getUint16(at) * byteValues + gsub.getUint8(at + uint24 - 1));
          });
};
const substitutionOf = (lookups: readonly number[], subtables: ReadonlyMap<number, (typeof Lookup.Type)['subTables']>): typeof Substitution.Type => {
    const read = lookups.flatMap((index) => subtables.get(index) ?? []);
    return { coverage: new Set(read.flatMap(Struct.get('coverage'))), alternates: new Map(read.flatMap(({ coverage, alternates }) => Array.zip(coverage, alternates)).toReversed()) };
};
const rivalsOf = (substitutions: ReadonlyMap<string, typeof Substitution.Type>, tags: readonly string[], tag: string) =>
    tags.filter(
        (other) =>
            other !== tag &&
            (EXCLUSIVE.some((group) => group.has(tag) && group.has(other)) || (STYLISTIC.test(tag) && STYLISTIC.test(other) && Option.exists(Option.all([Option.fromUndefinedOr(substitutions.get(tag)), Option.fromUndefinedOr(substitutions.get(other))]), ([own, rival]) => !own.coverage.isDisjointFrom(rival.coverage)))),
    );
const languageSystems = (handle: Face) => {
    const defaultLanguage = 0xff_ff;
    return LayoutTable.literals
        .flatMap((table) => handle.getTableScriptTags(table).map((script, at) => ({ table, script, at })))
        .flatMap(({ table, script, at }) =>
            [[Option.none<string>(), defaultLanguage] as const, ...handle.getScriptLanguageTags(table, at).map((language, index) => [Option.some(language), index] as const)].map(([language, index]) => ({ table, script, language, features: handle.getLanguageFeatureTags(table, at, index) })),
        );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Layout };
