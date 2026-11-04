import { Digits } from '@rasm/search';
import { Effect, Option, Predicate, Record, Schema, Struct } from 'effect';
import { Tag } from '#/internal/tag';

// --- [CONSTANTS] -----------------------------------------------------------------------

const AUTO_LEADING = 1.2;

// --- [MODELS] --------------------------------------------------------------------------

const Direction = Schema.Literals(['LTR', 'RTL']).annotate({ description: 'Paragraph direction as HarfBuzz direction key, the bidi base level' });
const Elongation = {
    lower: Schema.Int.check(Schema.isGreaterThanOrEqualTo(1)).annotate({ description: 'Lower elongation bound in nuqta, the length of each text-stage place and the least of each line-stage place' }),
    upper: Schema.Int.annotate({ description: 'Upper elongation bound in nuqta, each chosen place stretching at most this far' }),
    fractional: Schema.Boolean.annotate({ description: 'Elongation below one tatweel width' }).pipe(Schema.withConstructorDefault(Effect.succeed(false))),
};
const Kashida = Schema.TaggedUnion({ None: {}, Tatweel: Elongation, Stretched: Elongation })
    .check(Schema.makeFilter((kashida) => kashida._tag === 'None' || kashida.lower <= kashida.upper, { description: 'lower <= upper' }))
    .annotate({ description: 'Elongation by tatweel characters or by jalt alternates then tatweels' });
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
            .annotate({ description: 'Line height over the ink depth of the line above plus the ink height of the line, marks included' })
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
const Kashidas = Schema.Int.check(Schema.isGreaterThanOrEqualTo(1)).annotate({ description: 'Most kashida places per run at the text stage, per run and rank at the line stage, every fitting place when absent' });
const Groups = Schema.Struct({
    groups: Schema.Array(Schema.String.annotate({ description: 'ICU joining group name (Beh, FarsiYeh)' })),
    except: Schema.Boolean.annotate({ description: 'Every joining group but these' }).pipe(Schema.withConstructorDefault(Effect.succeed(false))),
});
const Next = Schema.TaggedUnion({ End: {}, Letter: { groups: Schema.OptionFromOptionalKey(Groups).pipe(Schema.withConstructorDefault(Effect.succeedNone)) } }).annotate({
    description: 'Letter after the pair in its join, End when the pair closes it',
});
const Place = Schema.Struct({
    rank: Rank,
    stage: Schema.Literals(['text', 'line'])
        .annotate({ description: 'text elongates before line breaking so lines reflow, line fits slack after it' })
        .pipe(Schema.withConstructorDefault(Effect.succeed('line'))),
    ...Struct.Record(['before', 'after'], Schema.OptionFromOptionalKey(Groups.annotate({ description: 'Letter beside the place, any when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone))),
    next: Schema.OptionFromOptionalKey(Next).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
});
const Ranks = Schema.Struct({
    ...Record.map({ word: 0, alternates: 1, letter: 9, glyph: 10 }, (rank) => Rank.pipe(Schema.withConstructorDefault(Effect.succeed(rank)))),
    scripts: Schema.Record(Schema.String.annotate({ description: 'ICU script name (Arabic, Latin)' }), Rank)
        .annotate({ description: 'Word space ranks by script of the letters beside the space, over word' })
        .pipe(Schema.withConstructorDefault(Effect.succeed({}))),
    places: Schema.Array(Place)
        .annotate({ description: 'Kashida places, each word elongating at its best-ranked matching letter pair' })
        .pipe(Schema.withConstructorDefault(Effect.sync(() => naskh()))),
    kashidas: Schema.OptionFromOptionalKey(Kashidas).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
}).annotate({ description: 'Justification mechanism ranks, the default places a Naskh set' });
class Settings extends Schema.Class<Settings, { readonly brand: unique symbol }>('Settings')(
    {
        direction: Schema.OptionFromOptionalKey(Direction).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        numberingSystem: Schema.OptionFromOptionalKey(Digits.System.annotate({ description: 'Digits shaped text takes, source digits when unset' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        alignment: Alignment.pipe(Schema.withConstructorDefault(Effect.sync(() => Alignment.cases.Justified.make({})))),
        leading: Leading.pipe(Schema.withConstructorDefault(Effect.sync(() => Leading.make({})))),
        kashida: Kashida.pipe(Schema.withConstructorDefault(Effect.sync(() => Kashida.cases.None.make({})))),
        justification: Ranks.pipe(Schema.withConstructorDefault(Effect.sync(() => Ranks.make({})))),
        marks: Marks.pipe(Schema.withConstructorDefault(Effect.sync(() => Marks.make({})))),
        features: Schema.Record(Tag, Feature)
            .annotate({ description: 'OpenType feature values by tag, 0 disabling a default feature, above 1 selecting that alternate' })
            .pipe(Schema.withConstructorDefault(Effect.succeed({}))),
        spacing: Spacing.pipe(Schema.withConstructorDefault(Effect.sync(() => Spacing.make({})))),
    },
    { description: 'Typesetting of one paragraph or run' },
) {
    static readonly Direction = Direction;
    static readonly Alignment = Alignment;
    static readonly Leading = Leading;
    static readonly Kashida = Kashida;
    static readonly Marks = Marks;
    static readonly Spacing = Spacing;
    static readonly Partial = Settings.mapFields((fields) =>
        Struct.map(Schema.optionalKey)({
            ...fields,
            direction: Schema.OptionFromNullOr(Direction),
            numberingSystem: Schema.OptionFromNullOr(Digits.System),
            ...Record.map({ leading: Leading, marks: Marks }, (section) => Schema.Struct(Struct.map(section.fields, Schema.optionalKey))),
            justification: Schema.Struct({ ...Struct.map(Ranks.fields, Schema.optionalKey), kashidas: Schema.optionalKey(Schema.OptionFromNullOr(Kashidas)) }),
            features: Schema.Record(Tag, Schema.NullOr(Feature)),
            spacing: Schema.Struct(Record.map(Spacing.fields, (range) => Schema.optionalKey(Schema.Struct(Struct.map(range.schema.fields, Schema.optionalKey))))),
        }),
    ).annotate({ description: 'Absent keys, section fields, spacing bounds and feature tags stay unchanged, null unsets direction, numbering system, kashida limit or a feature tag' });
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
    lineHeight(size: number, ink: { readonly descent: number; readonly ascent: number }) {
        return Height.match(this.leading.height, { Auto: ({ ratio }) => size * ratio, Fixed: Struct.get('em'), Extents: ({ ratio }) => (ink.descent + ink.ascent) * ratio });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const naskh = (): readonly (typeof Place.Type)[] => {
    const among = (...groups: readonly string[]) => Option.some(Groups.make({ groups }));
    const notYeh = Option.some(Groups.make({ groups: ['Yeh', 'FarsiYeh'], except: true }));
    const end = Option.some(Next.cases.End.make({}));
    return [
        { rank: 2, after: among('NoJoiningGroup') },
        { rank: 3, before: among('Seen') },
        { rank: 3, before: among('Sad'), after: notYeh },
        { rank: 3, before: among('Sad'), next: Option.some(Next.cases.Letter.make({})) },
        { rank: 4, after: among('TehMarbuta', 'Heh', 'Dal'), next: end },
        { rank: 5, after: among('Alef', 'Tah', 'Lam', 'Kaf', 'Gaf'), next: end },
        { rank: 6, after: among('Beh'), next: Option.some(Next.cases.Letter.make({ groups: among('Reh', 'Yeh', 'FarsiYeh') })) },
        { rank: 7, after: among('Waw', 'Ain', 'Qaf', 'Feh'), next: end },
        { rank: 8, after: notYeh, next: end },
    ].map((row) => Place.make(row));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Settings };
