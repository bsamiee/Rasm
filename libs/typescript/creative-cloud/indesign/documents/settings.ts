import type { Capitalization, CharacterDirectionOptions, DigitsTypeOptions, InDesignDOMObject, KashidasOptions, ParagraphDirectionOptions, ParagraphJustificationOptions, Story, Text, TextDefault } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Settings } from '@rasm/typography/settings';
import { Array, Cause, Effect, Number, Option, Predicate, Record, Result, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { Model } from '#/dom/model';
import { Specifier } from '#/dom/specifier';
import { picked } from '#/dom/value';
import { Change } from '#/requests/change';

// --- [TYPES] ---------------------------------------------------------------------------

type Members<E> = Extract<keyof E, Uppercase<string>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const THOUSANDTHS = 1000;
const PERCENT = 100;
const STYLISTIC_SETS = 20;
const FEATURE_ON = 1;
const STYLISTIC_TAGS = Array.makeBy(STYLISTIC_SETS, (index) => `ss${`${index + 1}`.padStart(2, '0')}`);
const DIRECTIONS = { LEFT_TO_RIGHT_DIRECTION: 'ltr', RIGHT_TO_LEFT_DIRECTION: 'rtl' } as const satisfies Record<Members<typeof ParagraphDirectionOptions>, 'ltr' | 'rtl'>;
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
const SPACING = {
    word: { min: 'minimumWordSpacing', desired: 'desiredWordSpacing', max: 'maximumWordSpacing' },
    letter: { min: 'minimumLetterSpacing', desired: 'desiredLetterSpacing', max: 'maximumLetterSpacing' },
    glyph: { min: 'minimumGlyphScaling', desired: 'desiredGlyphScaling', max: 'maximumGlyphScaling' },
} as const;
const spacingMembers = <V>(value: (axis: keyof typeof SPACING, bound: 'min' | 'desired' | 'max') => V) => ({
    ...Record.mapEntries(SPACING.word, (member, bound) => [member, value('word', bound)]),
    ...Record.mapEntries(SPACING.letter, (member, bound) => [member, value('letter', bound)]),
    ...Record.mapEntries(SPACING.glyph, (member, bound) => [member, value('glyph', bound)]),
});

// --- [MODELS] --------------------------------------------------------------------------

const DirectionMembers = Schema.Struct({ paragraphDirection: Schema.optionalKey(Schema.Literals(Struct.keys(DIRECTIONS))), characterDirection: Schema.optionalKey(Schema.Literals(['DEFAULT_DIRECTION', 'LEFT_TO_RIGHT_DIRECTION', 'RIGHT_TO_LEFT_DIRECTION'] satisfies Members<typeof CharacterDirectionOptions>[])) });
const direction = DirectionMembers.pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.direction.schema), {
        decode: SchemaGetter.transform(({ paragraphDirection }) => Option.map(Option.fromUndefinedOr(paragraphDirection), (member) => DIRECTIONS[member])),
        encode: SchemaGetter.transformEffect(
            Option.match({
                onNone: () => Effect.succeed<typeof DirectionMembers.Type>({ characterDirection: 'DEFAULT_DIRECTION' }),
                onSome: (held: 'ltr' | 'rtl') =>
                    Effect.map(
                        inverse(DIRECTIONS, (value) => value === held, held),
                        (paragraphDirection) => ({ paragraphDirection }),
                    ),
            }),
        ),
    }),
);
const numberingSystem = Schema.Struct({ digitsType: Schema.Literals(Struct.keys(NUMBERING)) }).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.numberingSystem.schema), {
        decode: SchemaGetter.transform(({ digitsType }) => Option.fromNullOr(NUMBERING[digitsType])),
        encode: SchemaGetter.transformEffect((given) => {
            const held = Option.getOrNull(given);
            return Effect.map(
                inverse(NUMBERING, (value) => value === held, held),
                (digitsType) => ({ digitsType }),
            );
        }),
    }),
);
const KashidaMembers = Schema.Struct({
    kashidas: Schema.Literals(['DEFAULT_KASHIDAS', 'KASHIDAS_OFF'] satisfies Members<typeof KashidasOptions>[]),
    paragraphJustification: Schema.optionalKey(Schema.Literals(Struct.keys(JUSTIFICATIONS))),
    paragraphKashidaWidth: Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: KASHIDA_LENGTHS.length - 1 })),
    otfJustificationAlternate: Schema.Boolean,
});
const kashida = KashidaMembers.pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.kashida.schema), {
        decode: SchemaGetter.transform(({ kashidas, paragraphJustification = 'NASKH_KASHIDA_JUSTIFICATION', paragraphKashidaWidth, otfJustificationAlternate }) => {
            const { insertion, fractional } = JUSTIFICATIONS[paragraphJustification];
            return { insertion: kashidas === 'KASHIDAS_OFF' ? ('none' as const) : insertion, length: Array.getUnsafe(KASHIDA_LENGTHS, paragraphKashidaWidth), fractional, alternates: otfJustificationAlternate };
        }),
        encode: SchemaGetter.transformEffect(({ insertion, length, fractional, alternates }) => {
            const shared = { paragraphKashidaWidth: KASHIDA_LENGTHS.indexOf(length), otfJustificationAlternate: alternates };
            return insertion === 'none'
                ? Effect.succeed<typeof KashidaMembers.Type>({ kashidas: 'KASHIDAS_OFF', ...shared })
                : Effect.map(
                      inverse(JUSTIFICATIONS, (row) => row.insertion === insertion && row.fractional === fractional, insertion),
                      (paragraphJustification) => ({ kashidas: 'DEFAULT_KASHIDAS' as const, paragraphJustification, ...shared }),
                  );
        }),
    }),
);
const marks = Schema.Struct(Struct.Record(['xOffsetDiacritic', 'yOffsetDiacritic'], Schema.Finite)).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.marks.schema), {
        decode: SchemaGetter.transform(({ xOffsetDiacritic, yOffsetDiacritic }) => ({ x: xOffsetDiacritic / THOUSANDTHS, y: yOffsetDiacritic / THOUSANDTHS })),
        encode: SchemaGetter.transform(({ x, y }) => ({ xOffsetDiacritic: x * THOUSANDTHS, yOffsetDiacritic: y * THOUSANDTHS })),
    }),
);
const features = Schema.Struct({
    otfStylisticSets: Schema.Int,
    ...Record.map(FEATURE_FLAGS, () => Schema.Boolean),
    capitalization: Schema.Literals(Struct.keys(CAPITALIZATIONS)),
    opentypeFeatures: Schema.optionalKey(Schema.Array(Schema.Tuple([Schema.String, Schema.Finite]))),
}).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.features.schema), {
        decode: SchemaGetter.transform(({ otfStylisticSets, capitalization, opentypeFeatures = [], ...flags }) =>
            Array.dedupe([...STYLISTIC_TAGS.filter((_, bit) => Math.floor(otfStylisticSets / 2 ** bit) % 2 === 1), ...Record.collect(FEATURE_FLAGS, (flag, tag) => (flags[flag] ? [tag] : [])).flat(), ...CAPITALIZATIONS[capitalization], ...opentypeFeatures.flatMap(([tag, value]) => (value === 0 ? [] : [tag]))]),
        ),
        encode: SchemaGetter.transformEffect((tags) => {
            const owned: readonly string[] = [...STYLISTIC_TAGS, ...Object.values(FEATURE_FLAGS), ...Object.values(CAPITALIZATIONS).flat()];
            return Effect.map(
                inverse(CAPITALIZATIONS, (needed) => needed.every((tag) => tags.includes(tag)), tags),
                (capitalization) => ({
                    otfStylisticSets: Number.sumAll(STYLISTIC_TAGS.flatMap((tag, bit) => (tags.includes(tag) ? [2 ** bit] : []))),
                    ...Record.map(FEATURE_FLAGS, (tag) => tags.includes(tag)),
                    capitalization,
                    opentypeFeatures: tags.flatMap((tag): readonly (readonly [string, number])[] => (owned.includes(tag) ? [] : [[tag, FEATURE_ON]])),
                }),
            );
        }),
    }),
);
const spacing = Schema.Struct(spacingMembers(() => Schema.Finite)).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.spacing.schema), {
        decode: SchemaGetter.transform((host) =>
            Record.map(
                SPACING,
                Record.map((member: keyof typeof host) => host[member] / PERCENT),
            ),
        ),
        encode: SchemaGetter.transform((given) => spacingMembers((axis, bound) => given[axis][bound] * PERCENT)),
    }),
);
class HostSettings extends Schema.Class<HostSettings, { readonly brand: unique symbol }>('HostSettings')({ direction, numberingSystem, kashida, marks, features, spacing }) {
    static readonly Given = HostSettings.mapFields(Struct.map(Schema.optionalKey));
    static readonly target = Effect.fnUntraced(function* (path: string) {
        const [{ app, frame, owning, text }, specifier] = yield* Effect.all([Model, Schema.decodeUnknownEffect(Specifier.Path)(path)]);
        if (specifier.root) {
            return app.textDefaults;
        }
        const found = yield* Specifier.object(specifier);
        if (frame(found) || owning<Story>('textContainers')(found)) {
            return found.texts.item(0);
        }
        return yield* text(found) ? Effect.succeed(found) : Unmatched.make({ subject: 'text', query: Option.some(path), candidates: [] });
    });
    static readonly range = Effect.fnUntraced(function* (target: Text | TextDefault) {
        const { text } = yield* Model;
        return yield* text(target) ? Effect.succeed(target) : Unmatched.make({ subject: 'text range', query: Option.some(target.toSpecifier()), candidates: [] });
    });
    static readonly of = (target: object): Settings => Settings.make(Result.getOrThrow(Schema.decodeUnknownResult(HostSettings)(Record.map(HostSettings.fields, (row) => Record.map(picked(row.from.fields)(target), (value) => (Predicate.isObject(value) && !Array.isArray(value) ? String(value) : value))))));
    static readonly read = (target: InDesignDOMObject<unknown>) => Effect.try({ try: () => HostSettings.of(target), catch: HostError.refused(`settings of ${target.toSpecifier()}`) });
    static readonly write = Effect.fnUntraced(function* (target: Text | TextDefault, given: typeof HostSettings.Given.Type) {
        const values: Record.ReadonlyRecord<string, unknown> = Object.fromEntries(Object.values(yield* Schema.encodeEffect(HostSettings.Given)(given)).flatMap(Object.entries));
        return yield* Change.write(
            target,
            Record.filter(values, (value, member) => (member === 'capitalization' ? value !== 'NORMAL' || !Array.isReadonlyArrayEmpty(Reflect.get(CAPITALIZATIONS, String(target.capitalization))) : member in target || !Predicate.isIterable(value) || Array.isReadonlyArrayNonEmpty(Array.fromIterable(value)))),
        );
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const inverse = <K extends string, V>(table: Record.ReadonlyRecord<K, V>, holds: (value: V) => boolean, input: unknown) =>
    Effect.fromOption(
        Array.findFirst(Struct.keys(table), (key) => holds(table[key])),
        () => new SchemaIssue.InvalidValue({ message: `No host member for ${String(input)}` }, input),
    );
const accumulated = <A, B, E, R>(items: Iterable<A>, write: (item: A) => Effect.Effect<readonly B[], E, R>) =>
    Effect.validate(items, (item) => Effect.sandbox(write(item))).pipe(
        Effect.map(Array.flatten),
        Effect.catch((causes) => Effect.failCause(Cause.fromReasons(causes.flatMap(Struct.get('reasons'))))),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { accumulated, HostSettings, STYLISTIC_TAGS, THOUSANDTHS };
