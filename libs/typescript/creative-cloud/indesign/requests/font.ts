import { Source } from '@rasm/typography';
import { Uxp } from '@rasm/uxp';
import { Cause, Effect, Option, Schema } from 'effect';
import { fontByName } from '#/dom/decoding';
import { temporary } from '#/dom/files';
import { Model } from '#/dom/model';
import { FontView } from '#/font-view';

// --- [MODELS] --------------------------------------------------------------------------

class Font extends Schema.Class<Font, { readonly brand: unique symbol }>('Font')(
    {
        name: Schema.String.annotate({ description: 'Font, PostScript, or full name, family name selects Regular face' }),
        text: Schema.String.annotate({ description: 'Subset characters for unreadable font files' }),
    },
    { description: 'Reads font with face bytes' },
) {
    static readonly Result = Schema.Struct({
        view: FontView,
        source: Schema.OptionFromOptionalKey(Source.annotate({ description: 'Installed face bytes' })),
    });
    run() {
        const { name, text } = this;
        return Effect.gen(function* () {
            const [face, { FontStatus }, { fs }] = yield* Effect.all([fontByName(name), Model, Uxp]);
            const bytes = (path: string) => Effect.try({ try: () => new Uint8Array(fs.readFileSync(path)), catch: (cause) => new Cause.UnknownError(cause, path) });
            const subset = temporary('font.otf').pipe(
                Effect.tap((path) => Effect.try({ try: () => face.createSubsetFont(text, path), catch: (cause) => new Cause.UnknownError(cause, `subset of ${face.name}`) })),
                Effect.flatMap((path) => bytes(`file:${path}`)),
                Effect.map((data) => Option.some({ data, postscriptName: Option.none<string>() })),
            );
            const source = face.status.equals(FontStatus.INSTALLED)
                ? yield* bytes(`file:${face.location}`).pipe(
                      Effect.map((data) => Option.some({ data, postscriptName: Option.some(face.postscriptName) })),
                      Effect.catchIf(
                          ({ cause }) => Schema.is(Schema.Struct({ code: Schema.Literal(-2) }))(cause),
                          () => subset,
                      ),
                  )
                : Option.none();
            return { view: yield* FontView.of(face), source };
        }).pipe(Effect.scoped);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Font };
