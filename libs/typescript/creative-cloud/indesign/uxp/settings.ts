import type { Capitalization, CharacterDirectionOptions, DigitsTypeOptions, InDesignDOMObject, Justification, KashidasOptions, Leading, PageSideOptions, ParagraphDirectionOptions, ParagraphJustificationOptions, SingleWordJustification, Text, TextDefault } from 'adobe:indesign';

import { Digits, Settings } from '@rasm/typography';
import { Array, Effect, Equal, identity, Match, Number, Option, Predicate, Record, Schema, SchemaGetter, SchemaIssue, SchemaTransformation, Struct } from 'effect';
import { STYLISTIC_SETS } from '#/uxp/dictionary';
import { Model } from '#/uxp/model';

// --- [TYPES] ---------------------------------------------------------------------------

type Members<E> = Extract<keyof E, Uppercase<string>>;
type Sides = ReturnType<typeof sides>;
type Placed = InDesignDOMObject<unknown> & Pick<TextDefault, 'pointSize' | 'paragraphDirection'>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const STYLISTIC_TAGS = Array.makeBy(STYLISTIC_SETS, (index) => `ss${String(index + 1).padStart(2, '0')}`);
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
const ALIGNMENTS = {
    LEFT_ALIGN: { _tag: 'Aligned', edge: 'left' },
    CENTER_ALIGN: { _tag: 'Aligned', edge: 'center' },
    RIGHT_ALIGN: { _tag: 'Aligned', edge: 'right' },
    LEFT_JUSTIFIED: { _tag: 'Justified', edge: 'left' },
    CENTER_JUSTIFIED: { _tag: 'Justified', edge: 'center' },
    RIGHT_JUSTIFIED: { _tag: 'Justified', edge: 'right' },
    FULLY_JUSTIFIED: { _tag: 'Justified', edge: 'justified' },
    TO_BINDING_SIDE: { _tag: 'Aligned', edge: 'spine' },
    AWAY_FROM_BINDING_SIDE: { _tag: 'Aligned', edge: 'fore' },
} as const satisfies Record<Members<typeof Justification>, { readonly _tag: Settings['alignment']['_tag']; readonly edge: keyof Sides }>;
const SINGLE_WORDS = { LEFT_ALIGN: 'left', CENTER_ALIGN: 'center', RIGHT_ALIGN: 'right', FULLY_JUSTIFIED: 'justified' } as const satisfies Record<Members<typeof SingleWordJustification>, keyof Sides>;
const SPINES = { LEFT_HAND: 'right', RIGHT_HAND: 'left', SINGLE_SIDED: 'left' } as const satisfies Record<Members<typeof PageSideOptions>, 'left' | 'right'>;
const PARAGRAPH_STARTS = { LTR: 'left', RTL: 'right' } as const;
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

const scaled = (factor: number) => Schema.Finite.pipe(Schema.decodeTo(Schema.Finite, SchemaTransformation.transform({ decode: (value: number) => value / factor, encode: (value: number) => value * factor })));
const { thousandths, percent } = Record.map({ thousandths: 1000, percent: 100 }, scaled);
const ParagraphDirection = Schema.Literals(['LEFT_TO_RIGHT_DIRECTION', 'RIGHT_TO_LEFT_DIRECTION'] as const satisfies readonly Members<typeof ParagraphDirectionOptions>[]).transform(['LTR', 'RTL']);
const Placement = Schema.Struct({ pointSize: Schema.Finite.check(Schema.isGreaterThan(0)), paragraphDirection: ParagraphDirection, side: Schema.optionalKey(Schema.Literals(Struct.keys(SPINES))) });
const direction = Schema.Struct({
    paragraphDirection: Schema.optionalKey(ParagraphDirection),
    characterDirection: Schema.optionalKey(Schema.Literals(['DEFAULT_DIRECTION', 'LEFT_TO_RIGHT_DIRECTION', 'RIGHT_TO_LEFT_DIRECTION'] satisfies Members<typeof CharacterDirectionOptions>[])),
}).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.direction), {
        decode: SchemaGetter.transform(({ paragraphDirection }) => Option.fromUndefinedOr(paragraphDirection)),
        encode: SchemaGetter.transform(Option.match({ onNone: () => ({ characterDirection: 'DEFAULT_DIRECTION' as const }), onSome: (paragraphDirection) => ({ paragraphDirection }) })),
    }),
);
const alignmentMembers = Schema.Struct({ justification: Schema.Literals(Struct.keys(ALIGNMENTS)), singleWordJustification: Schema.optionalKey(Schema.Literals(Struct.keys(SINGLE_WORDS))) });
const alignment = (sided: Sides) =>
    alignmentMembers.pipe(
        Schema.decodeTo(Schema.toType(Settings.fields.alignment), {
            decode: SchemaGetter.transform(({ justification, singleWordJustification = 'FULLY_JUSTIFIED' }) =>
                Match.valueTags(ALIGNMENTS[justification], {
                    Aligned: ({ _tag, edge }) => ({ _tag, side: sided[edge] }),
                    Justified: ({ _tag, edge }) => ({ _tag, last: sided[edge], single: sided[SINGLE_WORDS[singleWordJustification]] }),
                }),
            ),
            encode: SchemaGetter.transformEffect(
                Effect.fnUntraced(function* (aligned: Settings['alignment']) {
                    const side = Match.valueTags(aligned, { Aligned: Struct.get('side'), Justified: Struct.get('last') });
                    const justification = yield* inverse(ALIGNMENTS, (row) => Predicate.isTagged(row, aligned._tag) && sided[row.edge] === side, aligned, `No justification member for ${aligned._tag} ${side}`);
                    return yield* Match.valueTags(aligned, {
                        Aligned: () => Effect.succeed({ justification }),
                        Justified: ({ single }) =>
                            Effect.map(
                                inverse(SINGLE_WORDS, (edge) => sided[edge] === single, aligned, `No singleWordJustification member for ${single}`),
                                (singleWordJustification) => ({ justification, singleWordJustification }),
                            ),
                    });
                }),
            ),
        }),
    );
const leading = (size: number) => {
    const spaces = Struct.Record(['spaceBefore', 'spaceAfter'], scaled(size));
    return Schema.Union([Schema.Struct({ leading: Schema.Literal('AUTO' satisfies Members<typeof Leading>), autoLeading: percent, ...spaces }), Schema.Struct({ leading: scaled(size), ...spaces })]).pipe(
        Schema.decodeTo(Schema.toType(Settings.fields.leading), {
            decode: SchemaGetter.transform((line) => ({ height: line.leading === 'AUTO' ? { _tag: 'Auto' as const, ratio: line.autoLeading } : { _tag: 'Fixed' as const, em: line.leading }, before: line.spaceBefore, after: line.spaceAfter })),
            encode: SchemaGetter.transformEffect(({ height, before, after }) =>
                Effect.map(
                    Match.valueTags(height, {
                        Auto: ({ ratio }) => Effect.succeed({ leading: 'AUTO' as const, autoLeading: ratio }),
                        Fixed: ({ em }) => Effect.succeed({ leading: em }),
                        Extents: (extents) => Effect.fail(new SchemaIssue.InvalidValue({ message: `No leading member for ${extents._tag} ${extents.ratio}` }, extents)),
                    }),
                    (line) => ({ ...line, spaceBefore: before, spaceAfter: after }),
                ),
            ),
        }),
    );
};
const numberingSystem = Schema.Struct({ digitsType: Schema.Literals(Struct.keys(NUMBERING)) }).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.numberingSystem), {
        decode: SchemaGetter.transform(({ digitsType }) => Option.flatMap(Option.fromNullOr(NUMBERING[digitsType]), Digits.of)),
        encode: SchemaGetter.transformEffect((system) =>
            Effect.map(
                inverse(NUMBERING, (digits) => digits === Option.getOrNull(Option.map(system, Struct.get('system'))), system, `No digitsType member for ${Option.getOrNull(Option.map(system, Struct.get('system')))}`),
                (digitsType) => ({ digitsType }),
            ),
        ),
    }),
);
const kashidaWidth = Schema.Literals(Object.values(KASHIDA_WIDTHS)).annotate({ description: 'Fixed elongation read as the point interval [width, width]' });
const elongated = Effect.fnUntraced(function* (elongation: Exclude<Settings['kashida'], { readonly _tag: 'None' }>) {
    const width = Option.filter(Option.liftPredicate(elongation.lower, Schema.is(kashidaWidth)), (lower) => lower === elongation.upper);
    const paragraphKashidaWidth = yield* Effect.fromOption(width, () => new SchemaIssue.InvalidValue({ message: `No paragraphKashidaWidth for [${elongation.lower}, ${elongation.upper}]` }, elongation));
    const paragraphJustification = yield* inverse(JUSTIFICATIONS, (row) => Predicate.isTagged(row, elongation._tag) && row.fractional === elongation.fractional, elongation, elongation.fractional ? `No paragraphJustification for ${elongation._tag} fractional` : `No paragraphJustification for ${elongation._tag}`);
    return { kashidas: 'DEFAULT_KASHIDAS' as const, paragraphJustification, paragraphKashidaWidth };
});
const kashida = Schema.Struct({
    kashidas: Schema.Literals(['DEFAULT_KASHIDAS', 'KASHIDAS_OFF'] satisfies Members<typeof KashidasOptions>[]),
    paragraphJustification: Schema.optionalKey(Schema.Literals(Struct.keys(JUSTIFICATIONS))),
    paragraphKashidaWidth: kashidaWidth,
}).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.kashida), {
        decode: SchemaGetter.transform(({ kashidas, paragraphJustification = 'DEFAULT_JUSTIFICATION', paragraphKashidaWidth }) =>
            kashidas === 'KASHIDAS_OFF' || paragraphKashidaWidth === KASHIDA_WIDTHS.None ? { _tag: 'None' as const } : { ...JUSTIFICATIONS[paragraphJustification], lower: paragraphKashidaWidth, upper: paragraphKashidaWidth },
        ),
        encode: SchemaGetter.transformEffect(
            Match.valueTags({
                None: () => Effect.succeed({ kashidas: 'KASHIDAS_OFF' as const, paragraphKashidaWidth: KASHIDA_WIDTHS.None }),
                Tatweel: elongated,
                Stretched: elongated,
            }),
        ),
    }),
);
const marks = Schema.Struct(Struct.Record(['x', 'y'], thousandths)).pipe(Schema.encodeKeys({ x: 'xOffsetDiacritic', y: 'yOffsetDiacritic' }));
const featureValues = Schema.Array(Schema.Tuple([Schema.String, Schema.Finite]));
const heldFeatures = Schema.Struct({ capitalization: Schema.String, existing: featureValues });
const features = Schema.Struct({
    otfStylisticSets: Schema.Int,
    ...Record.map(FEATURE_FLAGS, () => Schema.Boolean),
    capitalization: Schema.Literals(Struct.keys(CAPITALIZATIONS)),
    opentypeFeatures: Schema.optionalKey(featureValues),
}).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.features), {
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
const spacing = Schema.Struct(Struct.Record(Array.flatMap(Object.values(SPACING), Record.values), percent)).pipe(
    Schema.decodeTo(Schema.toType(Settings.fields.spacing), {
        decode: SchemaGetter.transform((members) =>
            Record.map(
                SPACING,
                Record.map((member) => members[member]),
            ),
        ),
        encode: SchemaGetter.transform((values) => Record.collect(SPACING, (axis, bounds) => Struct.renameKeys(values[axis], bounds)).reduce((all, each) => Record.union(all, each, identity))),
    }),
);
class InDesignSettings extends Schema.Class<InDesignSettings, { readonly brand: unique symbol }>('InDesignSettings')({ direction, numberingSystem, kashida, marks, features, spacing }) {
    static readonly stylisticSets = STYLISTIC_TAGS;
    static readonly kashidaWidths = KASHIDA_WIDTHS;
    static readonly Thousandths = thousandths;
    static readonly read = (target: Placed) => Effect.flatMap(placement(target), (placed) => held(target, placed));
    static readonly write = Effect.fnUntraced(function* (target: Text | TextDefault, change: typeof Settings.Partial.Type) {
        const placed = yield* placement(target);
        const merged = yield* Effect.flatMap(held(target, placed), (settings) => Settings.merge(settings, change));
        const { trips, schema } = sections({ ...placed, paragraphDirection: Option.getOrElse(merged.direction, () => placed.paragraphDirection) });
        const written = Struct.pick(merged, [...Struct.keys(change), ...('direction' in change ? (['alignment'] as const) : [])]);
        const encoded = yield* Schema.encodeEffect(schema.mapFields(Struct.map(Schema.optionalKey)))(written);
        const members: Record.ReadonlyRecord<string, unknown> = Object.fromEntries(Object.values(Struct.omit(encoded, ['features'])).flatMap(Object.entries));
        const featured = yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(encoded.features), (tags) => featureMembers(target, tags)));
        return yield* Model.write(target, { ...retained(target, members, trips), ...Option.getOrElse(featured, Record.empty) });
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const inverse = <K extends string, V>(table: { readonly [key in K]: V }, wanted: (row: V) => boolean, actual: unknown, message: string) =>
    Effect.fromOption(
        Array.findFirst(Struct.keys(table), (key) => wanted(table[key])),
        () => new SchemaIssue.InvalidValue({ message }, actual),
    );
const sides = (paragraph: (typeof Placement.Type)['paragraphDirection'], spine: (typeof SPINES)[keyof typeof SPINES]) => {
    const toward = (edge: 'left' | 'right'): Exclude<ReturnType<Settings['lineAlignment']>, 'center' | 'justified'> => (edge === PARAGRAPH_STARTS[paragraph] ? 'start' : 'end');
    return { left: toward('left'), center: 'center', right: toward('right'), justified: 'justified', spine: toward(spine), fore: toward(spine === 'left' ? 'right' : 'left') } as const;
};
const sections = ({ pointSize, paragraphDirection, side = 'SINGLE_SIDED' }: typeof Placement.Type) => {
    const sided = sides(paragraphDirection, SPINES[side]);
    const trips = { ...ROUND_TRIPS, justification: Record.map(ALIGNMENTS, ({ _tag, edge }) => `${_tag} ${sided[edge]}`) };
    return { trips, schema: InDesignSettings.mapFields(Struct.assign({ alignment: alignment(sided), leading: leading(pointSize) })) };
};
const placement = Effect.fnUntraced(function* (target: Placed) {
    const { text, item } = yield* Model;
    const page = () => Option.flatMap(text(target) ? Array.head(target.parentTextFrames) : Option.none(), (holder) => Option.fromNullishOr(item(holder) ? holder.parentPage : holder.parent.parentPage));
    return yield* Model.picked(Placement, target, () => Record.getSomes({ side: Option.map(page(), Struct.get('side')) }));
});
const held = Effect.fnUntraced(function* (target: Placed, placed: typeof Placement.Type) {
    const { picked } = yield* Model;
    const { schema } = sections(placed);
    const pick = (row: Schema.Top) => picked(row, target);
    return Settings.make(yield* Model.read(schema, `settings of ${target.toSpecifier()}`, () => Record.map(schema.fields, pick)));
});
const retained = (target: Text | TextDefault, members: Record.ReadonlyRecord<string, unknown>, trips: typeof ROUND_TRIPS) =>
    Record.map(members, (written, member) => {
        const current = String(Reflect.get(target, member));
        const decoded = Option.flatMap(Record.get(trips, member), Record.get(current));
        return Predicate.isString(written) && Option.isSome(decoded) && Equal.equals(decoded, Option.flatMap(Record.get(trips, member), Record.get(written))) ? current : written;
    });
const featureMembers = (target: Text | TextDefault, { capitalization, opentypeFeatures = [], ...flags }: typeof features.Encoded) =>
    Effect.map(
        Model.read(heldFeatures, `features of ${target.toSpecifier()}`, () => ({ capitalization: target.capitalization, existing: 'opentypeFeatures' in target ? target.opentypeFeatures : [] })),
        ({ capitalization: current, existing }) => {
            const replaced = Record.toEntries({ ...Record.map(Record.fromEntries(existing), () => 0), ...Record.fromEntries(opentypeFeatures) });
            return {
                ...flags,
                ...(capitalization === 'NORMAL' && Option.exists(Record.get<string, readonly string[]>(CAPITALIZATIONS, current), Array.isReadonlyArrayEmpty) ? {} : { capitalization }),
                ...(Array.isReadonlyArrayEmpty(replaced) ? {} : { opentypeFeatures: replaced }),
            };
        },
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { InDesignSettings };
