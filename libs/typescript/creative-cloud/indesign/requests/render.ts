import type { Page, PageItem, Spread } from 'adobe:indesign';
import { HostError, type Render } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Cache, Effect, Match, Number, Option, Path, pipe, Record, Schema, Struct } from 'effect';
import { extent } from '#/documents/document';
import { Model } from '#/dom/model';
import { Specifier } from '#/dom/specifier';
import { Value } from '#/dom/value';
import { write } from '#/requests/change';

// --- [OPERATIONS] ----------------------------------------------------------------------

const render = Effect.fn('render')(function* ({ target: written, longestSide, options, file, label }: Render) {
    const [model, target, path] = yield* Effect.all([Model, Schema.decodeUnknownEffect(Specifier.Path)(written), Path.Path]);
    const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = model;
    const found = yield* Effect.filterOrFail(
        Model.resolve(target),
        (value): value is Page | Spread | PageItem => (model.page(value) && model.spread(value.parent)) || model.spread(value) || model.item(value),
        () => Unmatched.make({ subject: 'document page, spread, or page item', query: Option.some(target.path) }),
    );
    const { source, placed, range } = Match.value(found).pipe(
        Match.when(model.page, (each) => ({ source: each.parent.parent, placed: [each], range: { pageString: `+${each.documentOffset + 1}`, exportingSpread: false } })),
        Match.when(model.spread, (each) => ({ source: each.parent, placed: each.pages.everyItem().getElements(), range: { pageString: `+${each.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
        Match.orElse((each) => ({ source: each, placed: [each], range: {} })),
    );
    const { top, left, bottom, right } = yield* extent(...placed);
    const pointsPerInch = 72;
    const preferences = app.pngExportPreferences;
    const { fields } = yield* Cache.get(model.declarations, preferences.constructorName);
    const bounds = yield* pipe(
        Record.get(fields, 'exportResolution'),
        Option.flatMap(({ minimum, maximum }) => Option.all({ minimum, maximum })),
        Effect.fromOption,
        Effect.orDie,
    );
    const defaults = {
        pngColorSpace: PNGColorSpaceEnum.RGB,
        pngQuality: PNGQualityEnum.MAXIMUM,
        transparentBackground: false,
        antiAlias: true,
        useDocumentBleeds: false,
        pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
        pngSuffix: '',
        exportingHiddenSpread: true,
        exportResolution: Number.clamp((longestSide * pointsPerInch) / Math.max(right - left, bottom - top), bounds),
        ...range,
    };
    const given = yield* Schema.decodeEffect(Value.Fields)(Option.getOrElse(options, Record.empty));
    yield* Uxp.preserve(preferences, defaults, [...Struct.keys(defaults), ...Record.keys(given)]);
    yield* write(preferences, given);
    const native = yield* path.fromFileUrl(file);
    yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, native), catch: HostError.refused(label) });
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { render };
