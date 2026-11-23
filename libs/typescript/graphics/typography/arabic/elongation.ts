import { Effect, Match, Option, Schema, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Groups = Schema.Struct({
    groups: Schema.Array(Schema.String.annotate({ description: 'ICU joining group name (Beh, FarsiYeh)' })),
    except: Schema.Boolean.annotate({ description: 'Every joining group but these' }).pipe(Schema.withConstructorDefault(Effect.succeed(false))),
});
const Next = Schema.TaggedUnion({ End: {}, Letter: { groups: Schema.OptionFromOptionalKey(Groups).pipe(Schema.withConstructorDefault(Effect.succeedNone)) } }).annotate({
    description: 'Letter after the pair in its join, End when the pair closes it',
});
const Place = Schema.Struct({
    rank: Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)).annotate({ description: 'Justification rank, lower ranks taking slack first and equal ranks sharing it by capacity' }),
    stage: Schema.Literals(['text', 'line'])
        .annotate({ description: 'text elongates before line breaking so lines reflow, line fits slack after it' })
        .pipe(Schema.withConstructorDefault(Effect.succeed('line'))),
    ...Struct.Record(['before', 'after'], Schema.OptionFromOptionalKey(Groups.annotate({ description: 'Letter beside the place, any when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone))),
    next: Schema.OptionFromOptionalKey(Next).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
});
const Elongated = {
    lower: Schema.Int.check(Schema.isGreaterThanOrEqualTo(1)).annotate({ description: 'Lower elongation bound in nuqta, the length of each text-stage place and the least of each line-stage place' }),
    upper: Schema.Int.annotate({ description: 'Upper elongation bound in nuqta, each chosen place stretching at most this far' }),
    fractional: Schema.Boolean.annotate({ description: 'Elongation below one tatweel width' }).pipe(Schema.withConstructorDefault(Effect.succeed(false))),
    places: Schema.Array(Place)
        .annotate({ description: 'Kashida places, each word elongating at its best-ranked matching letter pair, the default a Naskh set' })
        .pipe(Schema.withConstructorDefault(Effect.sync(() => naskh()))),
    limit: Schema.OptionFromOptionalKey(Schema.Int.check(Schema.isGreaterThanOrEqualTo(1)).annotate({ description: 'Most kashida places per run at the text stage, per run and rank at the line stage, every fitting place when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
};
const Elongation = Schema.TaggedUnion({ None: {}, Tatweel: Elongated, Stretched: Elongated })
    .check(
        Schema.makeFilter(
            (kashida) =>
                Match.value(kashida).pipe(
                    Match.tag('None', () => true),
                    Match.orElse(({ lower, upper }) => lower <= upper),
                ),
            { description: 'lower <= upper' },
        ),
    )
    .annotate({ description: 'Arabic-script elongation by tatweel characters, or by jalt alternates then tatweels' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const naskh = () => {
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

export { Elongation };
