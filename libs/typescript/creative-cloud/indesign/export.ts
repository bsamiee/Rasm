import type { Page, PageItem, Spread } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Cause, Effect, Match, Schema } from 'effect';
import { PNG } from 'image-size/types/png';
import { Host } from './host.ts';
import { Classes } from './objects/classes.ts';
import { resolve, Specifier } from './objects/specifiers.ts';
import { preserve } from './objects/values.ts';

// --- [MODELS] --------------------------------------------------------------------------

class Render extends Schema.Class<Render, { readonly brand: unique symbol }>('Render')(
    { target: Specifier.annotate({ identifier: undefined, description: 'Specifier of a document page, spread, or page item' }) },
    {
        description:
            "Exports a complete document page, spread, or page item through InDesign's native PNG export at maximum quality and the host's export resolution. An item shows itself; render its page or spread for surrounding context. Set app.pngExportPreferences.exportResolution for the detail needed. To see application panels, dialogs, pasteboard, or master spreads, use screenshot",
    },
) {
    static readonly Result = Schema.Struct({ image: Schema.Uint8Array, width: Schema.Int, height: Schema.Int });

    run() {
        const { target } = this;
        return Effect.gen(function* () {
            const [{ indesign, fs, uxp }, { owning }] = yield* Effect.all([Host, Classes]);
            const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = indesign;
            const [paged, spread, item] = [owning<Page>('documentOffset'), owning<Spread>('pages'), owning<PageItem>('itemLayer')];
            const found = yield* Effect.filterOrFail(
                resolve(target),
                (value): value is Page | Spread | PageItem => (paged(value) && value.parent.constructorName === 'Spread') || (spread(value) && value.constructorName === 'Spread') || item(value),
                () => Unmatched.make({ subject: 'document page, spread, or page item', query: target, candidates: [] }),
            );
            const { source, options } = Match.value(found).pipe(
                Match.when(paged, (page) => ({ source: page.parent.parent, options: { pageString: `+${page.documentOffset + 1}`, exportingSpread: false } })),
                Match.when({ constructorName: 'Spread' }, (each) => ({ source: each.parent, options: { pageString: `+${each.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
                Match.orElse((each) => ({ source: each, options: {} })),
            );
            const [folder, id] = yield* Effect.all([Effect.promise(() => uxp.storage.localFileSystem.getTemporaryFolder()), Effect.fiberId]);
            const file = yield* Effect.acquireRelease(
                Effect.promise(() => folder.createFile(`render-${id}.png`, { overwrite: true })),
                (created) => Effect.promise(created.delete.bind(created)),
            );
            yield* preserve(app.pngExportPreferences, {
                pngColorSpace: PNGColorSpaceEnum.RGB,
                pngQuality: PNGQualityEnum.MAXIMUM,
                transparentBackground: false,
                antiAlias: true,
                useDocumentBleeds: false,
                pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
                pngSuffix: '',
                exportingHiddenSpread: true,
                ...options,
            });
            const refused = (cause: unknown) => new Cause.UnknownError(cause, `export of ${target}`);
            yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, file.nativePath), catch: refused });
            const image = yield* Effect.try({ try: () => new Uint8Array(fs.readFileSync(file.url)), catch: refused });
            return { image, ...PNG.calculate(image) };
        }).pipe(Effect.scoped);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Render };
