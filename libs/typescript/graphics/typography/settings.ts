import { Digits } from '@rasm/search';
import { Effect, Option, Predicate, Record, Schema, SchemaTransformation, String, Struct } from 'effect';
import { Elongation } from '#/arabic/elongation.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const AUTO_LEADING = 1.2;
const TAG_LENGTH = 4;
const PRINTABLE_ASCII = /^[\u{20}-\u{7E}]+$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Tag = Schema.String.check(Schema.isBetweenLength(1, TAG_LENGTH), Schema.isPattern(PRINTABLE_ASCII)).pipe(Schema.decodeTo(Schema.String.check(Schema.isBetweenLength(TAG_LENGTH, TAG_LENGTH)), SchemaTransformation.transform({ decode: String.padEnd(TAG_LENGTH), encode: String.trimEnd })));
const Direction = Schema.Literals(['LTR', 'RTL']).annotate({ description: 'Paragraph direction as HarfBuzz direction key, the bidi base level' });
const Feature = Schema.Int.check(Schema.isGreaterThanOrEqualTo(0));
const Marks = Schema.Struct(Struct.Record(['x', 'y'], Schema.Finite.pipe(Schema.withConstructorDefault(Effect.succeed(0))))).annotate({ description: 'Mark offset in em, x right and y away from the base, below-base marks moving down' });
const Spacing = Schema.Struct(
    Record.map(
        {
            word: { value: 1, description: 'Ratios of space advance' },
            letter: { value: 0, description: 'Ratios of space advance added between letters' },
            glyph: { value: 1, description: 'Ratios of glyph width, a wdth instance where the face varies it' },
        },
        ({ value, description }) => {
            const range = Schema.Struct(Struct.Record(['min', 'desired', 'max'], Schema.Finite.pipe(Schema.withConstructorDefault(Effect.succeed(value)))))
                .check(Schema.makeFilter(({ min, desired, max }) => min <= desired && desired <= max, { description: 'min <= desired <= max' }))
                .annotate({ description });
            return range.pipe(Schema.withConstructorDefault(Effect.sync(() => range.make({}))));
        },
    ),
);
const LineAlignment = Schema.Literals(['start', 'center', 'end', 'justified']).annotate({ description: 'Line alignment, justified filling the measure' });
const Alignment = Schema.TaggedUnion({
    Aligned: { side: LineAlignment.pick(['start', 'center', 'end']) },
    Justified: {
        last: LineAlignment.annotate({ description: 'Last line alignment, justified forcing it to the measure' }).pipe(Schema.withConstructorDefault(Effect.succeed('start'))),
        single: LineAlignment.annotate({ description: 'Alignment of a justified line holding one word' }).pipe(Schema.withConstructorDefault(Effect.succeed('justified'))),
    },
}).annotate({ description: 'Paragraph alignment, Justified filling every line but the last' });
const Height = Schema.TaggedUnion({
    Auto: {
        ratio: Schema.Finite.check(Schema.isGreaterThan(0))
            .annotate({ description: `Line height over the largest point size on the line, auto leading ${AUTO_LEADING}` })
            .pipe(Schema.withConstructorDefault(Effect.succeed(AUTO_LEADING))),
    },
    Fixed: { em: Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Baseline to baseline distance in em' }) },
    Extents: {
        ratio: Schema.Finite.check(Schema.isGreaterThan(0))
            .annotate({ description: 'Line height over the glyph extents depth of the line above plus the glyph extents height of the line, marks included' })
            .pipe(Schema.withConstructorDefault(Effect.succeed(1))),
    },
}).annotate({ description: 'Line height, Auto scaling point size whatever the face metrics, Fixed holding one baseline pitch, Extents separating vocalized lines without loosening plain ones' });
const Leading = Schema.Struct({
    height: Height.pipe(Schema.withConstructorDefault(Effect.sync(() => Height.cases.Auto.make({})))),
    ...Struct.Record(
        ['before', 'after'],
        Schema.Finite.check(Schema.isGreaterThanOrEqualTo(0))
            .annotate({ description: 'Paragraph space in em, dropped at a column edge' })
            .pipe(Schema.withConstructorDefault(Effect.succeed(0))),
    ),
});
const Rank = Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)).annotate({ description: 'Lower ranks take slack first, equal ranks share it by capacity' });
const Ranks = Schema.Struct({
    ...Record.map({ word: 0, alternates: 1, letter: 9, glyph: 10 }, (rank) => Rank.pipe(Schema.withConstructorDefault(Effect.succeed(rank)))),
    scripts: Schema.Record(Schema.String.annotate({ description: 'ICU script name (Arabic, Latin)' }), Rank)
        .annotate({ description: 'Word space ranks by script of the letters beside the space, over word' })
        .pipe(Schema.withConstructorDefault(Effect.succeed({}))),
}).annotate({ description: 'Justification mechanism ranks, kashida places ranking within kashida' });
class Settings extends Schema.Class<Settings, { readonly brand: unique symbol }>('Settings')(
    {
        direction: Schema.OptionFromOptionalKey(Direction).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        numberingSystem: Schema.OptionFromOptionalKey(Digits.System.annotate({ description: 'Digits shaped text takes, source digits when unset' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        alignment: Alignment.pipe(Schema.withConstructorDefault(Effect.sync(() => Alignment.cases.Justified.make({})))),
        leading: Leading.pipe(Schema.withConstructorDefault(Effect.sync(() => Leading.make({})))),
        kashida: Elongation.pipe(Schema.withConstructorDefault(Effect.sync(() => Elongation.cases.None.make({})))),
        justification: Ranks.pipe(Schema.withConstructorDefault(Effect.sync(() => Ranks.make({})))),
        marks: Marks.pipe(Schema.withConstructorDefault(Effect.sync(() => Marks.make({})))),
        features: Schema.Record(Tag, Feature)
            .annotate({ description: 'OpenType feature values by tag, 0 disabling a default feature, above 1 selecting that alternate' })
            .pipe(Schema.withConstructorDefault(Effect.succeed({}))),
        spacing: Spacing.pipe(Schema.withConstructorDefault(Effect.sync(() => Spacing.make({})))),
    },
    { description: 'Typesetting of one paragraph or run' },
) {
    static readonly Partial = Settings.mapFields((fields) =>
        Struct.map(Schema.optionalKey)({
            ...fields,
            direction: Schema.OptionFromNullOr(Direction),
            numberingSystem: Schema.OptionFromNullOr(Digits.System),
            ...Record.map({ leading: Leading, justification: Ranks, marks: Marks }, (section) => Schema.Struct(Struct.map(section.fields, Schema.optionalKey))),
            features: Schema.Record(Tag, Schema.NullOr(Feature)),
            spacing: Schema.Struct(Record.map(Spacing.fields, (range) => Schema.optionalKey(Schema.Struct(Struct.map(range.schema.fields, Schema.optionalKey))))),
        }),
    ).annotate({ description: 'Absent keys, section fields, spacing bounds and feature tags stay unchanged, null unsets direction, numbering system or a feature tag' });
    static readonly merge = (settings: Settings, change: typeof Settings.Partial.Type) =>
        Settings.makeEffect({
            ...settings,
            ...Struct.evolve(change, {
                leading: (leading) => ({ ...settings.leading, ...leading }),
                justification: (justification) => ({ ...settings.justification, ...justification }),
                marks: (marks) => ({ ...settings.marks, ...marks }),
                features: (features) => Record.filter({ ...settings.features, ...features }, Predicate.isNotNull),
                spacing: (spacing = {}) => Record.map(settings.spacing, (range, axis) => ({ ...range, ...spacing[axis] })),
            }),
        }).pipe(Effect.mapError((issue) => new Schema.SchemaError(issue)));

    digits(text: string) {
        return Option.match(this.numberingSystem, { onNone: () => text, onSome: (digits) => digits.format(text) });
    }
    lineAlignment(line: { readonly last: boolean; readonly single: boolean }): typeof LineAlignment.Type {
        return Alignment.match(this.alignment, {
            Aligned: Struct.get('side'),
            Justified: ({ last, single }) => {
                const aligned = line.last ? last : 'justified';
                return aligned === 'justified' && line.single ? single : aligned;
            },
        });
    }
    lineHeight(size: number, extents: { readonly descent: number; readonly ascent: number }) {
        return Height.match(this.leading.height, { Auto: ({ ratio }) => size * ratio, Fixed: Struct.get('em'), Extents: ({ ratio }) => (extents.descent + extents.ascent) * ratio });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Direction, Settings, Tag };
