import type { Capitalization, CharacterDirectionOptions, DigitsTypeOptions, InDesignDOMObject, KashidasOptions, ParagraphDirectionOptions, ParagraphJustificationOptions, Text, TextDefault } from 'adobe:indesign';
import { Settings } from '@rasm/typography/settings';
import { Array, Effect, Number, Option, pipe, Record, Schema, SchemaGetter, SchemaIssue, SchemaTransformation, Struct } from 'effect';
import { picked, read } from '#/dom/value';
import { Change } from '#/requests/change';

// --- [TYPES] ---------------------------------------------------------------------------

type Members<E> = Extract<keyof E, Uppercase<string>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const STYLISTIC_SETS = 20;
const STYLISTIC_TAGS = Array.makeBy(STYLISTIC_SETS, (index) => `ss${`${index + 1}`.padStart(2, '0')}`);
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
    NASKH_TATWEEL_JUSTIFICATION: { insertion: 'tatweel', fractional: false },
    NASKH_TATWEEL_JUSTIFICATION_FRAC: { insertion: 'tatweel', fractional: true },
    NASKH_KASHIDA_JUSTIFICATION: { insertion: 'stretched', fractional: false },
    NASKH_KASHIDA_JUSTIFICATION_FRAC: { insertion: 'stretched', fractional: true },
    DEFAULT_JUSTIFICATION: { insertion: 'stretched', fractional: false },
    ARABIC_JUSTIFICATION: { insertion: 'stretched', fractional: false },
    NASKH_JUSTIFICATION: { insertion: 'stretched', fractional: false },
} as const satisfies Record<Members<typeof ParagraphJustificationOptions>, { readonly insertion: 'tatweel' | 'stretched'; readonly fractional: boolean }>;
const LONGEST_KASHIDA_WIDTH = 3;
const KASHIDA_WIDTH = Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: LONGEST_KASHIDA_WIDTH })).annotate({ description: 'None, short, medium, long, a fixed elongation read as the point interval [width, width]' });
const FEATURE_FLAGS = { otfTitling: 'titl', otfContextualAlternate: 'calt', otfDiscretionaryLigature: 'dlig', otfStylisticAlternate: 'salt', otfJustificationAlternate: 'jalt' } as const;
const CAPITALIZATIONS = {
    CAP_TO_SMALL_CAP: ['c2sc', 'smcp'],
    SMALL_CAPS: ['smcp'],
    NORMAL: [],
    ALL_CAPS: [],
    LOWER_CASE: [],
} as const satisfies Record<Members<typeof Capitalization>, readonly string[]>;
const SPACING_AXES = { word: 'WordSpacing', letter: 'LetterSpacing', glyph: 'GlyphScaling' } as const;
const SPACING_BOUNDS = { min: 'minimum', desired: 'desired', max: 'maximum' } as const;
const SPACING = Record.fromIterableWith(Array.cartesian(Struct.keys(SPACING_AXES), Struct.keys(SPACING_BOUNDS)), (pair) => [pair.join(), pair]);
const spacingMember = <A extends keyof typeof SPACING_AXES, B extends keyof typeof SPACING_BOUNDS>(axis: A, bound: B) => `${SPACING_BOUNDS[bound]}${SPACING_AXES[axis]}` as const;
const { thousandths, percent } = Record.map({ thousandths: 1000, percent: 100 }, (factor) => ({ decode: (value: number) => value / factor, encode: (value: number) => value * factor }));

// --- [MODELS] --------------------------------------------------------------------------

const Scaled = (scale: typeof thousandths) => Schema.Finite.pipe(Schema.decodeTo(Schema.Finite, SchemaTransformation.transform(scale)));
const direction = Schema.Struct({
    paragraphDirection: Schema.optionalKey(Schema.Literals(['LEFT_TO_RIGHT_DIRECTION', 'RIGHT_TO_LEFT_DIRECTION'] as const satisfies readonly Members<typeof ParagraphDirectionOptions>[]).transform(['ltr', 'rtl'])),
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
        encode: SchemaGetter.transformEffect((system) => {
            const tag = Option.getOrNull(system);
            return pipe(
                Struct.keys(NUMBERING),
                Array.findFirst((member) => NUMBERING[member] === tag),
                Option.map((digitsType) => ({ digitsType })),
                Effect.fromOption(() => new SchemaIssue.InvalidValue({ message: `No digitsType member for ${tag}` }, tag)),
            );
        }),
    }),
);
const kashida = Schema.Struct({
    kashidas: Schema.Literals(['DEFAULT_KASHIDAS', 'KASHIDAS_OFF'] satisfies Members<typeof KashidasOptions>[]),
    paragraphJustification: Schema.optionalKey(Schema.Literals(Struct.keys(JUSTIFICATIONS))),
    paragraphKashidaWidth: KASHIDA_WIDTH,
    otfJustificationAlternate: Schema.Boolean,
}).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.kashida.schema), {
        decode: SchemaGetter.transform(({ kashidas, paragraphJustification = 'DEFAULT_JUSTIFICATION', paragraphKashidaWidth: width, otfJustificationAlternate: alternates }) => {
            const { insertion, fractional } = JUSTIFICATIONS[paragraphJustification];
            return { insertion: kashidas === 'KASHIDAS_OFF' ? ('none' as const) : insertion, degree: width, upper: width, fractional, alternates };
        }),
        encode: SchemaGetter.transformEffect(({ insertion, degree, upper, fractional, alternates: otfJustificationAlternate }) =>
            pipe(
                Option.liftPredicate(degree, (width) => width === upper && Schema.is(KASHIDA_WIDTH)(width)),
                Effect.fromOption(() => new SchemaIssue.InvalidValue({ message: `No paragraphKashidaWidth for [${degree}, ${upper}]` }, { degree, upper })),
                Effect.map((paragraphKashidaWidth) => {
                    const shared = { paragraphKashidaWidth, otfJustificationAlternate };
                    return insertion === 'none' ? { kashidas: 'KASHIDAS_OFF' as const, ...shared } : { kashidas: 'DEFAULT_KASHIDAS' as const, paragraphJustification: `NASKH_${insertion === 'tatweel' ? 'TATWEEL' : 'KASHIDA'}_JUSTIFICATION${fractional ? '_FRAC' : ''}` as const, ...shared };
                }),
            ),
        ),
    }),
);
const marks = Schema.Struct(Struct.Record(['x', 'y'], Scaled(thousandths))).pipe(Schema.encodeKeys({ x: 'xOffsetDiacritic', y: 'yOffsetDiacritic' }));
const features = Schema.Struct({
    otfStylisticSets: Schema.Int,
    ...Record.map(FEATURE_FLAGS, () => Schema.Boolean),
    capitalization: Schema.Literals(Struct.keys(CAPITALIZATIONS)),
    opentypeFeatures: Schema.optionalKey(Schema.Array(Schema.Tuple([Schema.String, Schema.Finite]))),
}).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.features.schema), {
        decode: SchemaGetter.transform(({ otfStylisticSets, capitalization, opentypeFeatures = [], ...flags }) =>
            Record.fromEntries([
                ...[...STYLISTIC_TAGS.filter((_, bit) => Math.floor(otfStylisticSets / 2 ** bit) % 2 === 1), ...Object.values(Record.filter(FEATURE_FLAGS, (_, flag) => flags[flag])), ...CAPITALIZATIONS[capitalization]].map((tag) => [tag, 1] as const),
                ...opentypeFeatures.filter(([, value]) => value !== 0),
            ]),
        ),
        encode: SchemaGetter.transform((features) => {
            const flagged = (tag: string) => Option.contains(Record.get(features, tag), 1);
            const capitalization = Option.getOrElse(
                Array.findFirst(Struct.keys(CAPITALIZATIONS), (member) => Array.isReadonlyArrayNonEmpty(CAPITALIZATIONS[member]) && CAPITALIZATIONS[member].every(flagged)),
                () => 'NORMAL' as const,
            );
            const memberTags: readonly string[] = [...STYLISTIC_TAGS, ...Object.values(FEATURE_FLAGS), ...CAPITALIZATIONS[capitalization]].filter(flagged);
            return {
                otfStylisticSets: Number.sumAll(STYLISTIC_TAGS.map((tag, bit) => (flagged(tag) ? 2 ** bit : 0))),
                ...Record.map(FEATURE_FLAGS, flagged),
                capitalization,
                opentypeFeatures: Record.toEntries(Record.filter(features, (_, tag) => !memberTags.includes(tag))),
            };
        }),
    }),
);
const spacing = Schema.Struct(Record.mapEntries(SPACING, ([axis, bound]) => [spacingMember(axis, bound), Scaled(percent)])).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.spacing.schema), {
        decode: SchemaGetter.transform((members) => Record.map(SPACING_AXES, (_member, axis) => Record.map(SPACING_BOUNDS, (_prefix, bound) => members[spacingMember(axis, bound)]))),
        encode: SchemaGetter.transform((values) => Record.mapEntries(SPACING, ([axis, bound]) => [spacingMember(axis, bound), values[axis][bound]])),
    }),
);
class HostSettings extends Schema.Class<HostSettings, { readonly brand: unique symbol }>('HostSettings')({ direction, numberingSystem, kashida, marks, features, spacing }) {
    static readonly stylisticSets = STYLISTIC_TAGS;
    static readonly thousandths = thousandths.encode;
    static readonly em = thousandths.decode;
    static readonly read = (target: InDesignDOMObject<unknown>) =>
        Effect.map(
            read(HostSettings, `settings of ${target.toSpecifier()}`, () => Record.map(HostSettings.fields, (row) => picked(row.from.fields)(target))),
            Settings.make,
        );
    static readonly write = Effect.fnUntraced(function* (target: Text | TextDefault, settings: typeof Settings.Partial.Type) {
        const { features: tags, ...sections } = yield* Schema.encodeEffect(PartialHostSettings)(settings);
        const members: Record.ReadonlyRecord<string, unknown> = Object.fromEntries(Object.values(sections).flatMap(Object.entries));
        return yield* Change.write(target, { ...members, ...(tags === undefined ? {} : yield* featureMembers(target, tags)) });
    });
}
const PartialHostSettings = HostSettings.mapFields(Struct.map(Schema.optionalKey));

// --- [OPERATIONS] ----------------------------------------------------------------------

const featureMembers = (target: Text | TextDefault, { capitalization, opentypeFeatures = [], ...flags }: typeof features.Encoded) =>
    Effect.map(
        read(Schema.String, `capitalization of ${target.toSpecifier()}`, () => target.capitalization),
        (current) => ({
            ...flags,
            ...(capitalization === 'NORMAL' && Option.exists(Record.get<string, readonly string[]>(CAPITALIZATIONS, current), Array.isReadonlyArrayEmpty) ? {} : { capitalization }),
            ...(Array.isReadonlyArrayEmpty(opentypeFeatures) && !('opentypeFeatures' in target) ? {} : { opentypeFeatures }),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostSettings, KASHIDA_WIDTH, LONGEST_KASHIDA_WIDTH };
