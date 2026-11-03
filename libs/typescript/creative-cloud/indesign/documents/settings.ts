import type { Capitalization, CharacterDirectionOptions, DigitsTypeOptions, InDesignDOMObject, KashidasOptions, ParagraphDirectionOptions, ParagraphJustificationOptions, Text, TextDefault } from 'adobe:indesign';
import { Settings } from '@rasm/typography/settings';
import { Array, Effect, identity, Number, Option, Predicate, Record, Schema, SchemaGetter, SchemaIssue, SchemaTransformation, Struct } from 'effect';
import { Model } from '#/dom/model';
import { Value } from '#/dom/value';
import { write } from '#/requests/change';

// --- [TYPES] ---------------------------------------------------------------------------

type Members<E> = Extract<keyof E, Uppercase<string>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const STYLISTIC_TAGS = Array.makeBy(20, (index) => `ss${`${index + 1}`.padStart(2, '0')}`);
const NUMBERING = {
    DEFAULT_DIGITS: null,
    NATIVE_DIGITS: null,
    ARABIC_DIGITS: 'latn',
    HINDI_DIGITS: 'arab',
    FARSI_DIGITS: 'arabext',
    FULL_FARSI_DIGITS: 'arabext',
    THAI_DIGITS: 'thai',
    LAO_DIGITS: 'laoo',
    DEVANAGARI_DIGITS: 'deva',
    BENGALI_DIGITS: 'beng',
    GURMUKHI_DIGITS: 'guru',
    GUJARATI_DIGITS: 'gujr',
    ORIYA_DIGITS: 'orya',
    TAMIL_DIGITS: 'tamldec',
    TELUGU_DIGITS: 'telu',
    KANNADA_DIGITS: 'knda',
    MALAYALAM_DIGITS: 'mlym',
    TIBETAN_DIGITS: 'tibt',
    KHMER_DIGITS: 'khmr',
    BURMESE_DIGITS: 'mymr',
} as const satisfies Record<Members<typeof DigitsTypeOptions>, string | null>;
const JUSTIFICATIONS = {
    NASKH_TATWEEL_JUSTIFICATION: { _tag: 'Tatweel', fractional: false },
    NASKH_TATWEEL_JUSTIFICATION_FRAC: { _tag: 'Tatweel', fractional: true },
    NASKH_KASHIDA_JUSTIFICATION: { _tag: 'Stretched', fractional: false },
    NASKH_KASHIDA_JUSTIFICATION_FRAC: { _tag: 'Stretched', fractional: true },
    DEFAULT_JUSTIFICATION: { _tag: 'Stretched', fractional: false },
    ARABIC_JUSTIFICATION: { _tag: 'Stretched', fractional: false },
    NASKH_JUSTIFICATION: { _tag: 'Stretched', fractional: false },
} as const satisfies Record<Members<typeof ParagraphJustificationOptions>, { readonly _tag: Exclude<Settings['kashida']['_tag'], 'None'>; readonly fractional: boolean }>;
const KASHIDA_WIDTHS = { None: 0, Short: 1, Medium: 2, Long: 3 } as const;
const ROUND_TRIPS: Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, string>> = {
    digitsType: Record.map(NUMBERING, String),
    paragraphJustification: Record.map(JUSTIFICATIONS, ({ _tag, fractional }) => `${_tag} ${fractional}`),
};
const FEATURE_FLAGS = { otfTitling: 'titl', otfContextualAlternate: 'calt', otfDiscretionaryLigature: 'dlig', otfStylisticAlternate: 'salt', otfJustificationAlternate: 'jalt' } as const;
const CAPITALIZATIONS = {
    CAP_TO_SMALL_CAP: ['c2sc', 'smcp'],
    SMALL_CAPS: ['smcp'],
    NORMAL: [],
    ALL_CAPS: [],
    LOWER_CASE: [],
} as const satisfies Record<Members<typeof Capitalization>, readonly string[]>;
const SPACING = Record.map({ word: 'WordSpacing', letter: 'LetterSpacing', glyph: 'GlyphScaling' } as const, (suffix) => Record.map({ min: 'minimum', desired: 'desired', max: 'maximum' } as const, (prefix) => `${prefix}${suffix}` as const));

// --- [MODELS] --------------------------------------------------------------------------

const { thousandths, percent } = Record.map({ thousandths: 1000, percent: 100 }, (factor) => Schema.Finite.pipe(Schema.decodeTo(Schema.Finite, SchemaTransformation.transform({ decode: (value: number) => value / factor, encode: (value: number) => value * factor }))));
const direction = Schema.Struct({
    paragraphDirection: Schema.optionalKey(Schema.Literals(['LEFT_TO_RIGHT_DIRECTION', 'RIGHT_TO_LEFT_DIRECTION'] as const satisfies readonly Members<typeof ParagraphDirectionOptions>[]).transform(['LTR', 'RTL'])),
    characterDirection: Schema.optionalKey(Schema.Literals(['DEFAULT_DIRECTION', 'LEFT_TO_RIGHT_DIRECTION', 'RIGHT_TO_LEFT_DIRECTION'] satisfies Members<typeof CharacterDirectionOptions>[])),
}).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.direction.schema), {
        decode: SchemaGetter.transform(({ paragraphDirection }) => Option.fromUndefinedOr(paragraphDirection)),
        encode: SchemaGetter.transform(Option.match({ onNone: () => ({ characterDirection: 'DEFAULT_DIRECTION' as const }), onSome: (paragraphDirection) => ({ paragraphDirection }) })),
    }),
);
const numberingSystem = Schema.Struct({ digitsType: Schema.Literals(Struct.keys(NUMBERING)) }).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.numberingSystem.schema), {
        decode: SchemaGetter.transform(({ digitsType }) => Option.fromNullOr(NUMBERING[digitsType])),
        encode: SchemaGetter.transformEffect((system) =>
            Effect.fromOption(
                Array.findFirst(Struct.keys(NUMBERING), (digitsType) => (NUMBERING[digitsType] === Option.getOrNull(system) ? Option.some({ digitsType }) : Option.none())),
                () => new SchemaIssue.InvalidValue({ message: `No digitsType member for ${Option.getOrNull(system)}` }, system),
            ),
        ),
    }),
);
const kashidaWidth = Schema.Literals(Object.values(KASHIDA_WIDTHS)).annotate({ description: 'Fixed elongation read as the point interval [width, width]' });
const kashida = Schema.Struct({
    kashidas: Schema.Literals(['DEFAULT_KASHIDAS', 'KASHIDAS_OFF'] satisfies Members<typeof KashidasOptions>[]),
    paragraphJustification: Schema.optionalKey(Schema.Literals(Struct.keys(JUSTIFICATIONS))),
    paragraphKashidaWidth: kashidaWidth,
}).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.kashida.schema), {
        decode: SchemaGetter.transform(({ kashidas, paragraphJustification = 'DEFAULT_JUSTIFICATION', paragraphKashidaWidth: width }) => (kashidas === 'KASHIDAS_OFF' || width === KASHIDA_WIDTHS.None ? { _tag: 'None' as const } : { ...JUSTIFICATIONS[paragraphJustification], lower: width, upper: width })),
        encode: SchemaGetter.transformEffect(
            Effect.fnUntraced(function* (elongation: Settings['kashida']) {
                if (elongation._tag === 'None') {
                    return { kashidas: 'KASHIDAS_OFF' as const, paragraphKashidaWidth: KASHIDA_WIDTHS.None };
                }
                const width = Option.filter(Option.liftPredicate(elongation.lower, Schema.is(kashidaWidth)), (lower) => lower === elongation.upper);
                const paragraphKashidaWidth = yield* Effect.fromOption(width, () => new SchemaIssue.InvalidValue({ message: `No paragraphKashidaWidth for [${elongation.lower}, ${elongation.upper}]` }, elongation));
                const justification = Array.findFirst(Struct.keys(JUSTIFICATIONS), (member) => JUSTIFICATIONS[member]._tag === elongation._tag && JUSTIFICATIONS[member].fractional === elongation.fractional);
                const paragraphJustification = yield* Effect.fromOption(justification, () => new SchemaIssue.InvalidValue({ message: `No paragraphJustification for ${elongation._tag}${elongation.fractional ? ' fractional' : ''}` }, elongation));
                return { kashidas: 'DEFAULT_KASHIDAS' as const, paragraphJustification, paragraphKashidaWidth };
            }),
        ),
    }),
);
const marks = Schema.Struct(Struct.Record(['x', 'y'], thousandths)).pipe(Schema.encodeKeys({ x: 'xOffsetDiacritic', y: 'yOffsetDiacritic' }));
const features = Schema.Struct({
    otfStylisticSets: Schema.Int,
    ...Record.map(FEATURE_FLAGS, () => Schema.Boolean),
    capitalization: Schema.Literals(Struct.keys(CAPITALIZATIONS)),
    opentypeFeatures: Schema.optionalKey(Schema.Array(Schema.Tuple([Schema.String, Schema.Finite]))),
}).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.features.schema), {
        decode: SchemaGetter.transform(({ otfStylisticSets, capitalization, opentypeFeatures = [], ...flags }) =>
            Record.fromEntries([...[...STYLISTIC_TAGS.filter((_, bit) => Math.floor(otfStylisticSets / 2 ** bit) % 2 === 1), ...CAPITALIZATIONS[capitalization]].map((tag) => [tag, 1] as const), ...Record.collect(FEATURE_FLAGS, (flag, tag) => [tag, flags[flag] ? 1 : 0] as const), ...opentypeFeatures]),
        ),
        encode: SchemaGetter.transform((chosen) => {
            const flagged = (tag: string) => Option.exists(Record.get(chosen, tag), Number.isGreaterThan(0));
            const capitalization = Option.getOrElse(
                Array.findFirst(Struct.keys(CAPITALIZATIONS), (member) => Array.isReadonlyArrayNonEmpty(CAPITALIZATIONS[member]) && CAPITALIZATIONS[member].every(flagged)),
                () => 'NORMAL' as const,
            );
            const memberTags: readonly string[] = [...STYLISTIC_TAGS, ...Object.values(FEATURE_FLAGS), ...CAPITALIZATIONS[capitalization]];
            return {
                otfStylisticSets: Number.sumAll(STYLISTIC_TAGS.map((tag, bit) => (flagged(tag) ? 2 ** bit : 0))),
                ...Record.map(FEATURE_FLAGS, flagged),
                capitalization,
                opentypeFeatures: Record.toEntries(Record.filter(chosen, (_, tag) => !memberTags.includes(tag))),
            };
        }),
    }),
);
const spacing = Schema.Struct(Struct.Record(Array.flatMap(Record.values(SPACING), Record.values), percent)).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.spacing.schema), {
        decode: SchemaGetter.transform((members) => Record.map(SPACING, (bounds) => Record.map(bounds, (member) => members[member]))),
        encode: SchemaGetter.transform((values) => Record.collect(SPACING, (axis, bounds) => Record.mapEntries(bounds, (member, bound) => [member, values[axis][bound]])).reduce((all, each) => Record.union(all, each, identity))),
    }),
);
class HostSettings extends Schema.Class<HostSettings, { readonly brand: unique symbol }>('HostSettings')({ direction, numberingSystem, kashida, marks, features, spacing }) {
    static readonly stylisticSets = STYLISTIC_TAGS;
    static readonly kashidaWidths = KASHIDA_WIDTHS;
    static readonly Thousandths = thousandths;
    static readonly Partial = HostSettings.mapFields(Struct.map(Schema.optionalKey));
    static readonly read = (target: InDesignDOMObject<unknown>) => Model.read(HostSettings, `settings of ${target.toSpecifier()}`, () => Record.map(HostSettings.fields, (row) => Value.picked(row.from.fields)(target))).pipe(Effect.map(Settings.make));
    static readonly write = Effect.fnUntraced(function* (target: Text | TextDefault, change: typeof Settings.Partial.Type) {
        const merged = yield* Effect.flatMap(HostSettings.read(target), (settings) => Settings.merge(settings, change));
        const { features: tags, ...sections } = yield* Schema.encodeEffect(HostSettings.Partial)(Struct.pick(merged, Struct.keys(change)));
        const members: Record.ReadonlyRecord<string, unknown> = Object.fromEntries(Object.values(sections).flatMap(Object.entries));
        return yield* write(target, { ...retained(target, members), ...(tags === undefined ? {} : yield* featureMembers(target, tags)) });
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const retained = (target: Text | TextDefault, members: Record.ReadonlyRecord<string, unknown>) =>
    Record.map(members, (written, member) => {
        const held = String(Reflect.get(target, member));
        const same = Option.exists(Record.get(ROUND_TRIPS, member), (decoded) => Predicate.isString(written) && Option.exists(Record.get(decoded, held), (value) => Option.contains(Record.get(decoded, written), value)));
        return same ? held : written;
    });
const featureMembers = (target: Text | TextDefault, { capitalization, opentypeFeatures = [], ...flags }: typeof features.Encoded) =>
    Effect.map(
        Model.read(Schema.String, `capitalization of ${target.toSpecifier()}`, () => target.capitalization),
        (current) => ({
            ...flags,
            ...(capitalization === 'NORMAL' && Option.exists(Record.get<string, readonly string[]>(CAPITALIZATIONS, current), Array.isReadonlyArrayEmpty) ? {} : { capitalization }),
            ...(Array.isReadonlyArrayEmpty(opentypeFeatures) && !('opentypeFeatures' in target) ? {} : { opentypeFeatures }),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostSettings };
