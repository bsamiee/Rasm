import fs from 'adobe:fs';
import indesign, { type Page, type PageItem, type Spread } from 'adobe:indesign';
import { storage } from 'adobe:uxp';
import { Unmatched } from '@rasm/search';
import { Effect, Match, Option } from 'effect';
import { PNG } from 'image-size/types/png';
import { InDesignError } from '../models.ts';
import { owning, preserve, resolve } from './objects.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const render = Effect.fn('render')(function* (target: string) {
    const { app, ExportFormat, PNGExportRangeEnum, PNGColorSpaceEnum, PNGQualityEnum } = indesign;
    const found = yield* Effect.filterOrFail(
        resolve(target),
        (value): value is Page | Spread | PageItem => (owning<Page>('documentOffset')(value) && value.parent.constructorName === 'Spread') || (owning<Spread>('pages')(value) && value.constructorName === 'Spread') || owning<PageItem>('itemLayer')(value),
        () => Unmatched.make({ subject: 'document page, spread, or page item', query: target, candidates: [] }),
    );
    const { source, options } = Match.value(found).pipe(
        Match.when(owning<Page>('documentOffset'), (page) => ({ source: page.parent.parent, options: { pageString: `+${page.documentOffset + 1}`, exportingSpread: false } })),
        Match.when({ constructorName: 'Spread' }, (spread) => ({ source: spread.parent, options: { pageString: `+${spread.pages.firstItem().documentOffset + 1}`, exportingSpread: true } })),
        Match.orElse((item) => ({ source: item, options: {} })),
    );
    const folder = yield* Effect.promise(() => storage.localFileSystem.getTemporaryFolder());
    const id = yield* Effect.fiberId;
    const file = yield* Effect.acquireRelease(
        Effect.promise(() => folder.createFile(`render-${id}.png`, { overwrite: true })),
        (created) => Effect.promise(() => created.delete()),
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
    yield* Effect.try({ try: () => source.exportFile(ExportFormat.PNG_FORMAT, file.nativePath), catch: InDesignError.refused(Option.some(target)) });
    const image = yield* Effect.try({ try: () => new Uint8Array(fs.readFileSync(file.url)), catch: InDesignError.refused(Option.some(target)) });
    return { image, ...PNG.calculate(image) };
}, Effect.scoped);

// --- [EXPORTS] -------------------------------------------------------------------------

export { render };
