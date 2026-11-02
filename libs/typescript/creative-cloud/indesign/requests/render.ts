import type { Page, PageItem, Spread } from 'adobe:indesign';
import { Unmatched } from '@rasm/search/unmatched';
import { Uxp } from '@rasm/uxp/runtime';
import { Cache, Effect, Match, Number, Option, pipe, Record, Schema, Struct } from 'effect';
import { imageSize } from 'image-size';
import { Extent } from '#/documents/document';
import { Model } from '#/dom/model';
import { Specifier } from '#/dom/specifier';
import { preserve } from '#/dom/transactions';
import { InDesignError } from '#/error';

// --- [MODELS] --------------------------------------------------------------------------

class Render extends Schema.Class<Render, { readonly brand: unique symbol }>('Render')({
    target: Specifier.Path.annotate({ description: 'Document page, spread, or page item' }),
    pixels: Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Longest side in pixels' }),
}) {
    static readonly Result = Schema.Struct({ image: Schema.Uint8Array, width: Schema.Int, height: Schema.Int });

    run() {
        const { target, pixels } = this;
        return Effect.gen(function* () {
            const [model, { fs }] = yield* Effect.all([Model, Uxp]);
            const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = model;
            const found = yield* Effect.filterOrFail(
                Specifier.resolve(target),
                (value): value is Page | Spread | PageItem => (model.page(value) && model.spread(value.parent)) || model.spread(value) || model.item(value),
                () => Unmatched.make({ subject: 'document page, spread, or page item', query: Option.some(target.path), candidates: [] }),
            );
            const { source, placed, options } = Match.value(found).pipe(
                Match.when(model.page, (each) => ({ source: each.parent.parent, placed: [each], options: { pageString: `+${each.documentOffset + 1}`, exportingSpread: false } })),
                Match.when(model.spread, (each) => ({ source: each.parent, placed: each.pages.everyItem().getElements(), options: { pageString: `+${each.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
                Match.orElse((each) => ({ source: each, placed: [each], options: {} })),
            );
            const { top, left, bottom, right } = yield* Extent.of(...placed);
            const subject = `export of ${target.path}`;
            const refused = InDesignError.refused(subject);
            const pointsPerInch = 72;
            const { fields } = yield* Cache.get(model.rows, app.pngExportPreferences.constructorName);
            const range = yield* pipe(
                Record.get(fields, 'exportResolution'),
                Option.flatMap(({ minimum, maximum }) => Option.all({ minimum, maximum })),
                Effect.fromOption,
                Effect.orDie,
            );
            const exportResolution = Number.clamp((pixels * pointsPerInch) / Math.max(right - left, bottom - top), range);
            const path = yield* Model.temporary('render.png');
            yield* preserve(app.pngExportPreferences, {
                pngColorSpace: PNGColorSpaceEnum.RGB,
                pngQuality: PNGQualityEnum.MAXIMUM,
                transparentBackground: false,
                antiAlias: true,
                useDocumentBleeds: false,
                pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
                pngSuffix: '',
                exportingHiddenSpread: true,
                exportResolution,
                ...options,
            });
            yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, path), catch: refused });
            const image = yield* Effect.filterOrFail(
                Effect.try({ try: () => new Uint8Array(fs.readFileSync(`file:${path}`)), catch: refused }),
                (bytes) => bytes.length > 0,
                () => InDesignError.make({ reason: { _tag: 'Refused' }, subject: `${subject} wrote no image` }),
            );
            return yield* Effect.try({ try: () => ({ image, ...Struct.pick(imageSize(image), ['width', 'height']) }), catch: refused });
        }).pipe(Effect.scoped);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Render };
