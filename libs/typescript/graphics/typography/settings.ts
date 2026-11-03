import { Digits } from '@rasm/search/digits';
import { Effect, Schema, Struct } from 'effect';
import { Tag } from '#/tags';

// --- [MODELS] --------------------------------------------------------------------------

const Range = (value: number, description: string) => {
    const range = Schema.Struct(Struct.Record(['min', 'desired', 'max'], Schema.Finite.pipe(Schema.withConstructorDefault(Effect.succeed(value)))))
        .check(Schema.makeFilter(({ min, desired, max }) => min <= desired && desired <= max, { description: 'min <= desired <= max' }))
        .annotate({ description });
    return range.pipe(Schema.withConstructorDefault(Effect.sync(() => range.make({}))));
};
const Direction = Schema.Literals(['rtl', 'ltr']).annotate({ description: 'Paragraph direction, script direction when unset' });
const Kashida = Schema.Struct({
    insertion: Schema.Literals(['none', 'tatweel', 'stretched'])
        .annotate({ description: 'Elongation by tatweel characters or stretched glyphs' })
        .pipe(Schema.withConstructorDefault(Effect.succeed('none'))),
    degree: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0))
        .annotate({ description: 'Elongation degree i, each chosen place stretching by 0 or by [i, upper] nuqta, 0 none' })
        .pipe(Schema.withConstructorDefault(Effect.succeed(0))),
    upper: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0))
        .annotate({ description: 'Style upper bound u of one elongation in nuqta' })
        .pipe(Schema.withConstructorDefault(Effect.succeed(0))),
    fractional: Schema.Boolean.annotate({ description: 'Elongation below one tatweel width' }).pipe(Schema.withConstructorDefault(Effect.succeed(false))),
    alternates: Schema.Boolean.annotate({ description: 'Justification alternates before elongation' }).pipe(Schema.withConstructorDefault(Effect.succeed(false))),
}).check(Schema.makeFilter(({ degree, upper }) => degree <= upper, { description: 'degree <= upper' }));
const Marks = Schema.Struct(Struct.Record(['x', 'y'], Schema.Finite.pipe(Schema.withConstructorDefault(Effect.succeed(0))))).annotate({ description: 'Mark offset in em, y up' });
const Spacing = Schema.Struct({
    word: Range(1, 'Ratios of space advance'),
    letter: Range(0, 'Ratios of space advance added between letters'),
    glyph: Range(1, 'Ratios of glyph width'),
});
class Settings extends Schema.Class<Settings, { readonly brand: unique symbol }>('Settings')(
    {
        direction: Schema.OptionFromOptionalKey(Direction).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        numberingSystem: Schema.OptionFromOptionalKey(Digits.System.annotate({ description: 'Decimal numbering system id, source digits when unset' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
        kashida: Kashida.pipe(Schema.withConstructorDefault(Effect.sync(() => Kashida.make({})))),
        marks: Marks.pipe(Schema.withConstructorDefault(Effect.sync(() => Marks.make({})))),
        features: Schema.Record(Tag, Schema.Int.check(Schema.isGreaterThan(0)))
            .annotate({ description: 'Enabled OpenType features by tag, a value above 1 selecting that alternate' })
            .pipe(Schema.withConstructorDefault(Effect.succeed({}))),
        spacing: Spacing.pipe(Schema.withConstructorDefault(Effect.sync(() => Spacing.make({})))),
    },
    { description: 'Typesetting of one paragraph or run' },
) {
    static readonly Partial = Settings.mapFields((fields) => Struct.map(Schema.optionalKey)({ ...fields, direction: Schema.OptionFromNullOr(Direction), numberingSystem: Schema.OptionFromNullOr(Digits.System) })).annotate({
        description: 'Absent keys stay unchanged, null unsets direction or numbering system',
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Settings };
