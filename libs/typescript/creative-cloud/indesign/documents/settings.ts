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
const KASHIDA_LENGTHS = Settings.fields.kashida.schema.fields.length.schema.literals;
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
const { thousandths, percent } = Record.map({ thousandths: 1000, percent: 100 }, (factor) => ({ decode: (held: number) => held / factor, encode: (given: number) => given * factor }));

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
        encode: SchemaGetter.transformEffect((given) => {
            const held = Option.getOrNull(given);
            return pipe(
                Struct.keys(NUMBERING),
                Array.findFirst((member) => NUMBERING[member] === held),
                Option.map((digitsType) => ({ digitsType })),
                Effect.fromOption(() => new SchemaIssue.InvalidValue({ message: `No digitsType member for ${held}` }, held)),
            );
        }),
    }),
);
const kashida = Schema.Struct({
    kashidas: Schema.Literals(['DEFAULT_KASHIDAS', 'KASHIDAS_OFF'] satisfies Members<typeof KashidasOptions>[]),
    paragraphJustification: Schema.optionalKey(Schema.Literals(Struct.keys(JUSTIFICATIONS))),
    paragraphKashidaWidth: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: KASHIDA_LENGTHS.length - 1 })),
    otfJustificationAlternate: Schema.Boolean,
}).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.kashida.schema), {
        decode: SchemaGetter.transform(({ kashidas, paragraphJustification = 'DEFAULT_JUSTIFICATION', paragraphKashidaWidth, otfJustificationAlternate: alternates }) => {
            const { insertion, fractional } = JUSTIFICATIONS[paragraphJustification];
            return { insertion: kashidas === 'KASHIDAS_OFF' ? ('none' as const) : insertion, length: Array.getUnsafe(KASHIDA_LENGTHS, paragraphKashidaWidth), fractional, alternates };
        }),
        encode: SchemaGetter.transform(({ insertion, length, fractional, alternates: otfJustificationAlternate }) => {
            const shared = { paragraphKashidaWidth: KASHIDA_LENGTHS.indexOf(length), otfJustificationAlternate };
            return insertion === 'none' ? { kashidas: 'KASHIDAS_OFF' as const, ...shared } : { kashidas: 'DEFAULT_KASHIDAS' as const, paragraphJustification: `NASKH_${insertion === 'tatweel' ? 'TATWEEL' : 'KASHIDA'}_JUSTIFICATION${fractional ? '_FRAC' : ''}` as const, ...shared };
        }),
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
            Array.dedupe([...STYLISTIC_TAGS.filter((_, bit) => Math.floor(otfStylisticSets / 2 ** bit) % 2 === 1), ...Object.values(Record.filter(FEATURE_FLAGS, (_, flag) => flags[flag])), ...CAPITALIZATIONS[capitalization], ...opentypeFeatures.flatMap(([tag, value]) => (value === 0 ? [] : [tag]))]),
        ),
        encode: SchemaGetter.transform((tags) => {
            const capitalization = Option.getOrElse(
                Array.findFirst(Struct.keys(CAPITALIZATIONS), (member) => Array.isReadonlyArrayNonEmpty(CAPITALIZATIONS[member]) && Array.isReadonlyArrayEmpty(Array.difference(CAPITALIZATIONS[member], tags))),
                () => 'NORMAL' as const,
            );
            const owned: readonly string[] = [...STYLISTIC_TAGS, ...Object.values(FEATURE_FLAGS), ...CAPITALIZATIONS[capitalization]];
            return {
                otfStylisticSets: Number.sumAll(STYLISTIC_TAGS.flatMap((tag, bit) => (tags.includes(tag) ? [2 ** bit] : []))),
                ...Record.map(FEATURE_FLAGS, (tag) => tags.includes(tag)),
                capitalization,
                opentypeFeatures: Array.map(Array.difference(tags, owned), (tag) => [tag, 1] as const),
            };
        }),
    }),
);
const spacing = Schema.Struct(Record.mapEntries(SPACING, ([axis, bound]) => [spacingMember(axis, bound), Scaled(percent)])).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.spacing.schema), {
        decode: SchemaGetter.transform((host) => Record.map(SPACING_AXES, (_member, axis) => Record.map(SPACING_BOUNDS, (_prefix, bound) => host[spacingMember(axis, bound)]))),
        encode: SchemaGetter.transform((given) => Record.mapEntries(SPACING, ([axis, bound]) => [spacingMember(axis, bound), given[axis][bound]])),
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
    static readonly write = Effect.fnUntraced(function* (target: Text | TextDefault, given: typeof Settings.Partial.Type) {
        const { features: tags, ...members } = yield* Schema.encodeEffect(Given)(given);
        const rest: Record.ReadonlyRecord<string, unknown> = Object.fromEntries(Object.values(members).flatMap(Object.entries));
        return yield* Change.write(target, { ...rest, ...(tags === undefined ? {} : yield* featureMembers(target, tags)) });
    });
}
const Given = HostSettings.mapFields(Struct.map(Schema.optionalKey));

// --- [OPERATIONS] ----------------------------------------------------------------------

const featureMembers = (target: Text | TextDefault, { capitalization, opentypeFeatures = [], ...flags }: typeof features.Encoded) =>
    Effect.map(
        read(Schema.String, `capitalization of ${target.toSpecifier()}`, () => target.capitalization),
        (held) => ({
            ...flags,
            ...(capitalization === 'NORMAL' && Option.exists(Record.get<string, readonly string[]>(CAPITALIZATIONS, held), Array.isReadonlyArrayEmpty) ? {} : { capitalization }),
            ...(Array.isReadonlyArrayEmpty(opentypeFeatures) && !('opentypeFeatures' in target) ? {} : { opentypeFeatures }),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostSettings };
