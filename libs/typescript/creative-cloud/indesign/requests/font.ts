import type { Font as Face } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Array, Effect, Option, Path, Predicate, Result, Schema, Struct } from 'effect';
import { Font } from '#/contract';
import { Model } from '#/dom/model';
import { Specifier } from '#/dom/specifier';
import { Value } from '#/dom/value';
import { search } from '#/requests/find-change';

// --- [OPERATIONS] ----------------------------------------------------------------------

const notFound = Schema.is(Schema.Struct({ code: Schema.Literal(-2) }));
const faceView = Effect.fn('faceView')(function* (face: Face) {
    const { app, FontStatus, OpenTypeFeature, members } = yield* Model;
    const [family, installed] = [face.fontFamily, face.status.equals(FontStatus.INSTALLED)];
    const style = installed ? Option.some(face.fontStyleName) : Option.none();
    const specifier = yield* Effect.orDie(Schema.decodeUnknownEffect(Specifier.Path)(face.toSpecifier()));
    const open = Option.toArray(yield* Effect.option(Model.current));
    const searched = yield* Effect.forEach(open, (document) =>
        search({ mode: 'Text', find: Option.some({ appliedFont: { specifier } }), options: Option.some({ includeFootnotes: true, includeHiddenLayers: true, includeLockedLayersForFind: true, includeLockedStoriesForFind: true, includeMasterPages: true }), change: Option.none() }, document),
    );
    const styled = yield* Effect.try({
        try: () => Array.flatMap(open, (document) => [...document.allParagraphStyles, ...document.allCharacterStyles].filter(({ appliedFont, fontStyle }) => Unmatched.named(`${Predicate.isString(appliedFont) ? appliedFont : appliedFont?.fontFamily}\t${fontStyle}`, face.name)).map((each) => each.toSpecifier())),
        catch: HostError.refused(`uses of ${face.name}`),
    });
    const uses = [...Array.flatMap(searched, ({ matches }) => Array.map(matches, Struct.get('specifier'))), ...styled];
    const every = app.fonts.everyItem();
    const [styles, features] = yield* Effect.try({
        try: () =>
            installed
                ? [
                      Array.dedupe(Array.filterMap(Array.zip(every.fontFamily, every.fontStyleName), ([each, named]) => (each === family ? Result.succeed(named) : Result.failVoid))),
                      members(OpenTypeFeature)
                          .filter((feature) => face.checkOpenTypeFeature(feature))
                          .map(String),
                  ]
                : [[], []],
        catch: HostError.refused(`styles and features of ${face.name}`),
    });
    const axes = Option.liftThrowable(() => Array.zipWith(face.designAxesName, face.designAxesValues, (name, value) => ({ name, value })))();
    return yield* Model.read(Font.View, face.toSpecifier(), () => ({
        _tag: 'Font',
        ...Model.referenceUnsafe(face),
        family,
        style,
        status: face.status,
        uses,
        installed: installed ? Option.some({ ...Value.picked(Font.View.Installed.fields)(face), type: face.fontType, styles, axes, features, allows: Value.picked(Font.View.Installed.fields.allows.fields)(face) }) : Option.none(),
    }));
});
const fontView = Model.view((model) => model.font, faceView);
const font = Effect.fn('font')(function* ({ name, text, file }: Font) {
    const [face, { FontStatus }, { fs }, path] = yield* Effect.all([Effect.flatMap(Model.fonts, (lookup) => lookup(name)), Model, Uxp, Path.Path]);
    const subset = Effect.as(Effect.try({ try: () => face.createSubsetFont(text, file), catch: HostError.refused(`subset of ${face.name}`) }), Option.some({ file, postscriptName: Option.none<string>() }));
    const source = face.status.equals(FontStatus.INSTALLED)
        ? yield* Effect.flatMap(path.toFileUrl(face.location), (url) => Effect.try({ try: () => fs.lstatSync(url.href), catch: HostError.refused(face.location) })).pipe(
              Effect.as(Option.some({ file: face.location, postscriptName: Option.some(face.postscriptName) })),
              Effect.catchIf(
                  (error) => !Schema.is(HostError)(error) || notFound(error.cause),
                  () => subset,
              ),
          )
        : Option.none();
    return { view: yield* faceView(face), source };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { font, fontView };
