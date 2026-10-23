import type { Page, PageItem, Spread } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Uxp } from '@rasm/uxp';
import { Cause, Effect, Match, Option, Schema, Struct } from 'effect';
import { imageSize } from 'image-size';
import { Extent } from '#/documents/extent';
import { temporary } from '#/dom/files';
import { resolve } from '#/dom/location';
import { Model } from '#/dom/model';
import { Specifier } from '#/dom/reference';
import { preserve } from '#/dom/transactions';

// --- [MODELS] --------------------------------------------------------------------------

class Render extends Schema.Class<Render, { readonly brand: unique symbol }>('Render')(
    {
        target: Specifier.annotate({ description: 'Document page, spread, or page item' }),
        pixels: Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Longest side in pixels' }),
    },
    { description: 'Renders PNG at 1 to 2400 ppi. Page items render alone, their page or spread shows context' },
) {
    static readonly Result = Schema.Struct({ image: Schema.Uint8Array, width: Schema.Int, height: Schema.Int });

    run() {
        const { target, pixels } = this;
        return Effect.gen(function* () {
            const [{ app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum, page, spread, item }, { fs }] = yield* Effect.all([Model, Uxp]);
            const found = yield* Effect.filterOrFail(
                resolve(target),
                (value): value is Page | Spread | PageItem => (page(value) && spread(value.parent)) || spread(value) || item(value),
                () => Unmatched.make({ subject: 'document page, spread, or page item', query: Option.some(target), candidates: [] }),
            );
            const { source, placed, options } = Match.value(found).pipe(
                Match.when(page, (each) => ({ source: each.parent.parent, placed: [each], options: { pageString: `+${each.documentOffset + 1}`, exportingSpread: false } })),
                Match.when(spread, (each) => ({ source: each.parent, placed: each.pages.everyItem().getElements(), options: { pageString: `+${each.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
                Match.orElse((each) => ({ source: each, placed: [each], options: {} })),
            );
            const { top, left, bottom, right } = yield* Extent.of(...placed);
            const refused = (cause: unknown) => new Cause.UnknownError(cause, `export of ${target}`);
            const pointsPerInch = 72;
            const path = yield* temporary('render.png');
            yield* preserve(app.pngExportPreferences, {
                pngColorSpace: PNGColorSpaceEnum.RGB,
                pngQuality: PNGQualityEnum.MAXIMUM,
                transparentBackground: false,
                antiAlias: true,
                useDocumentBleeds: false,
                pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
                pngSuffix: '',
                exportingHiddenSpread: true,
                exportResolution: (pixels * pointsPerInch) / Math.max(right - left, bottom - top),
                ...options,
            });
            yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, path), catch: refused });
            const image = yield* Effect.filterOrFail(
                Effect.try({ try: () => new Uint8Array(fs.readFileSync(`file:${path}`)), catch: refused }),
                (bytes) => bytes.length > 0,
                () => new Cause.UnknownError(undefined, `export of ${target} wrote no image`),
            );
            return yield* Effect.try({ try: () => ({ image, ...Struct.pick(imageSize(image), ['width', 'height']) }), catch: refused });
        }).pipe(Effect.scoped);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Render };
