// --- [IMPORTS] -------------------------------------------------------------------------

import {
    AnchorPoint,
    app,
    BoundingBoxLimits,
    CoordinateSpaces,
    type Document,
    ExportFormat,
    ExportRangeOrAllPages,
    JPEGOptionsQuality,
    type MasterSpread,
    type PageItem,
    PNGExportRangeEnum,
    PNGQualityEnum,
    SaveOptions,
    type Spread,
} from 'adobe:indesign';
import { thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { DPI, dpi, pixels } from '@rasm/creative-cloud-server/images';
import { type Format, Image, Snapshot, type Target } from '@rasm/creative-cloud-server/indesign/jobs';
import { AbsolutePath } from '@rasm/creative-cloud-server/values';
import { Array, Effect, Match, Number, Option, Schema, type Scope, Struct } from 'effect';
import { type Handler, handler } from './handler.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Rendered = Omit<(typeof Image)['Type'], 'kind' | 'path' | 'isolated' | 'overlaps' | 'effectivePpi'>;

interface Box {
    readonly top: number;
    readonly left: number;
    readonly bottom: number;
    readonly right: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _MIN_PAGE_PT = 216;
const _CORNERS = [AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.TOP_RIGHT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR, AnchorPoint.BOTTOM_LEFT_ANCHOR];
const _CONTAINERS = ['Spread', 'MasterSpread'];
const _JPEG = ['jpegQuality', 'exportResolution', 'pageString', 'jpegExportRange', 'exportingSpread'] as const;
const _PNG = ['pngQuality', 'exportResolution', 'pageString', 'pngExportRange', 'exportingSpread', 'transparentBackground'] as const;

// --- [GEOMETRY] ------------------------------------------------------------------------

const _bounds: (input: unknown) => readonly [number, number, number, number] = Schema.decodeUnknownSync(Schema.Tuple([Schema.Number, Schema.Number, Schema.Number, Schema.Number]));

const _points: (input: unknown) => Array.NonEmptyReadonlyArray<readonly [number, number]> = Schema.decodeUnknownSync(Schema.NonEmptyArray(Schema.Tuple([Schema.Number, Schema.Number])));

const _ppi: (input: unknown) => Option.Option<Array.NonEmptyReadonlyArray<number>> = Schema.decodeUnknownOption(Schema.NonEmptyArray(Schema.Number));

const _spreadBox = (item: Pick<PageItem, 'resolve'>): Box => {
    const corners = Array.map(_CORNERS, (anchor) => Array.headNonEmpty(_points(item.resolve([anchor, BoundingBoxLimits.GEOMETRIC_PATH_BOUNDS], CoordinateSpaces.SPREAD_COORDINATES))));
    const xs = Array.map(corners, ([x]) => x);
    const ys = Array.map(corners, ([, y]) => y);
    return { left: Math.min(...xs), right: Math.max(...xs), top: Math.min(...ys), bottom: Math.max(...ys) };
};

const _disjoint = (box: Box, rect: Box): boolean => box.left >= rect.right || box.right <= rect.left || box.top >= rect.bottom || box.bottom <= rect.top;

const _backToFront = (container: Pick<Spread, 'allPageItems'>): readonly PageItem[] =>
    Array.filter(Array.reverse(container.allPageItems), (item) => Array.contains(_CONTAINERS, String(item.parent.constructor.name)));

const _full = (widthPt: number, heightPt: number, resolution: number): Rendered => {
    const widthPx = pixels(widthPt, resolution);
    const heightPx = pixels(heightPt, resolution);
    return { widthPx, heightPx, contentWidthPx: widthPx, contentHeightPx: heightPx, dpi: resolution };
};

// --- [EXPORT] --------------------------------------------------------------------------

const _restored = <T extends object, K extends keyof T>(target: T, keys: readonly K[], values: Partial<T>): Effect.Effect<Pick<T, K>, HostRejection, Scope.Scope> =>
    Effect.acquireRelease(
        Effect.try({
            try: () => {
                const saved = Struct.pick(target, keys);
                Object.assign(target, values);
                return saved;
            },
            catch: thrown,
        }),
        (saved) =>
            Effect.sync(() => {
                Object.assign(target, saved);
            }),
    );

const _exported = (doc: Document, format: Format, resolution: number, pageString: string, spread: boolean, path: string): Effect.Effect<void, HostRejection> =>
    Effect.scoped(
        Effect.andThen(
            Match.value(format).pipe(
                Match.discriminatorsExhaustive('kind')({
                    jpg: () =>
                        _restored(app.jpegExportPreferences, _JPEG, {
                            jpegQuality: JPEGOptionsQuality.MAXIMUM,
                            exportResolution: resolution,
                            pageString,
                            jpegExportRange: ExportRangeOrAllPages.EXPORT_RANGE,
                            exportingSpread: spread,
                        }),
                    png: ({ transparent }) =>
                        _restored(app.pngExportPreferences, _PNG, {
                            pngQuality: PNGQualityEnum.MAXIMUM,
                            exportResolution: resolution,
                            pageString,
                            pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
                            exportingSpread: spread,
                            transparentBackground: transparent,
                        }),
                }),
            ),
            Effect.try({ try: () => doc.exportFile(format.kind === 'jpg' ? ExportFormat.JPG : ExportFormat.PNG_FORMAT, path), catch: thrown }),
        ),
    );

const _file = (directory: string, name: string, format: Format): AbsolutePath => AbsolutePath.make(`${directory}/${name}.${format.kind}`);

const _whole = (rendered: Rendered, path: AbsolutePath): (typeof Image)['Type'] => ({ kind: 'image', path, ...rendered, effectivePpi: Option.none(), isolated: false, overlaps: [] });

// --- [ISOLATION] -----------------------------------------------------------------------

const _moved = (item: PageItem, tmpSpread: Spread, targetLeft: number, targetTop: number): void => {
    const duplicate = item.duplicate(tmpSpread);
    const current = _spreadBox(duplicate);
    duplicate.move(undefined, [`${targetLeft - current.left}pt`, `${targetTop - current.top}pt`]);
};

const _placed = (list: readonly PageItem[], rect: Box, tmpSpread: Spread, pageTopLeft: Box): Effect.Effect<void, HostRejection> =>
    Effect.forEach(
        Array.filter(
            Array.map(list, (item) => ({ item, box: _spreadBox(item) })),
            ({ box }) => !_disjoint(box, rect),
        ),
        ({ item, box }) => Effect.try({ try: () => _moved(item, tmpSpread, pageTopLeft.left + (box.left - rect.left), pageTopLeft.top + (box.top - rect.top)), catch: thrown }),
        { discard: true },
    );

const _masked = (tmpSpread: Spread, box: Box): void => {
    const mask = tmpSpread.rectangles.add();
    mask.geometricBounds = [`${box.top}pt`, `${box.left}pt`, `${box.bottom}pt`, `${box.right}pt`];
    mask.fillColor = 'Paper';
    mask.strokeColor = 'None';
};

const _rendered = Effect.fnUntraced(function* (
    format: Format,
    path: string,
    widthPt: number,
    heightPt: number,
    cap: Option.Option<number>,
    populate: (tmpSpread: Spread, pageTopLeft: Box) => Effect.Effect<void, HostRejection>,
) {
    const pageWidth = Math.max(widthPt, _MIN_PAGE_PT);
    const pageHeight = Math.max(heightPt, _MIN_PAGE_PT);
    const wanted = dpi('detail', pageWidth, pageHeight);
    const resolution = Option.match(
        Option.filter(cap, (ppi) => ppi < wanted),
        { onNone: () => wanted, onSome: (ppi) => Number.clamp(Math.round(ppi), DPI) },
    );
    const tmp = yield* Effect.acquireRelease(
        Effect.try({
            try: () => {
                const opened = app.documents.add(false);
                while (opened.pages.length > 1) {
                    opened.pages.item(opened.pages.length - 1).remove();
                }
                opened.documentPreferences.facingPages = false;
                Object.assign(opened.pages.item(0).marginPreferences, { top: '0pt', bottom: '0pt', left: '0pt', right: '0pt' });
                opened.documentPreferences.pageWidth = `${pageWidth}pt`;
                opened.documentPreferences.pageHeight = `${pageHeight}pt`;
                return opened;
            },
            catch: thrown,
        }),
        (opened) =>
            Effect.sync(() => {
                opened.close(SaveOptions.NO);
            }),
    );
    const tmpSpread = tmp.spreads.item(0);
    yield* populate(tmpSpread, _spreadBox(tmp.pages.item(0)));
    yield* Effect.try({
        try: () => {
            if (pageHeight > heightPt) {
                _masked(tmpSpread, { top: heightPt, left: 0, bottom: pageHeight, right: pageWidth });
            }
            if (pageWidth > widthPt) {
                _masked(tmpSpread, { top: 0, left: widthPt, bottom: pageHeight, right: pageWidth });
            }
        },
        catch: thrown,
    });
    yield* _exported(tmp, format, resolution, '+1', false, path);
    return { ..._full(pageWidth, pageHeight, resolution), contentWidthPx: pixels(widthPt, resolution), contentHeightPx: pixels(heightPt, resolution) };
}, Effect.scoped);

const _composed = (source: Spread | MasterSpread, rect: Box, cap: Option.Option<number>, format: Format, path: string): Effect.Effect<Rendered, HostRejection> =>
    _rendered(format, path, rect.right - rect.left, rect.bottom - rect.top, cap, (tmpSpread, pageTopLeft) => {
        const applied: readonly MasterSpread[] = Array.map(source.pages.everyItem().getElements(), Struct.get('appliedMaster'));
        const masters = Array.dedupeWith(Array.filter(applied, Struct.get('isValid')), (left: MasterSpread, right: MasterSpread) => left.id === right.id);
        return Effect.andThen(
            Effect.forEach(masters, (master) => _placed(_backToFront(master), rect, tmpSpread, pageTopLeft), { discard: true }),
            _placed(_backToFront(source), rect, tmpSpread, pageTopLeft),
        );
    });

// --- [TARGETS] -------------------------------------------------------------------------

const _indexed = (index: number, count: number): Effect.Effect<void, HostRejection> => (index < count ? Effect.void : Effect.fail(HostRejection.cases.pageOutOfRange.make({ index, count })));

const _page = Effect.fnUntraced(function* (doc: Document, { index, budget }: Extract<Target, { readonly kind: 'page' }>, format: Format, directory: string) {
    yield* _indexed(index, doc.pages.length);
    const [top, left, bottom, right] = _bounds(doc.pages.item(index).bounds);
    const resolution = dpi(budget, right - left, bottom - top);
    const path = _file(directory, `page-${index}`, format);
    yield* _exported(doc, format, resolution, `+${index + 1}`, false, path);
    return _whole(_full(right - left, bottom - top, resolution), path);
});

const _spread = Effect.fnUntraced(function* (doc: Document, { index, budget }: Extract<Target, { readonly kind: 'spread' }>, format: Format, directory: string) {
    yield* _indexed(index, doc.spreads.length);
    const before = Array.reduce(Array.take(doc.spreads.everyItem().getElements(), index), 0, (total, spread) => total + spread.pages.length);
    const pages = doc.spreads.item(index).pages.everyItem().getElements();
    const boxes = Array.map(pages, (page) => _bounds(page.bounds));
    const width = Array.reduce(boxes, 0, (total, [, l, , r]) => total + (r - l));
    const height = Array.reduce(boxes, 0, (tallest, [t, , b]) => Math.max(tallest, b - t));
    const resolution = dpi(budget, width, height);
    const path = _file(directory, `spread-${index}`, format);
    yield* _exported(doc, format, resolution, pages.length === 1 ? `+${before + 1}` : `+${before + 1}-+${before + pages.length}`, true, path);
    return _whole(_full(width, height, resolution), path);
});

const _region = Effect.fnUntraced(function* (doc: Document, { index, region: [x0, y0, x1, y1] }: Extract<Target, { readonly kind: 'region' }>, format: Format, directory: string) {
    yield* _indexed(index, doc.pages.length);
    const page = doc.pages.item(index);
    const box = _spreadBox(page);
    const width = box.right - box.left;
    const height = box.bottom - box.top;
    const rect = { left: box.left + x0 * width, top: box.top + y0 * height, right: box.left + x1 * width, bottom: box.top + y1 * height };
    const path = _file(directory, `region-${index}`, format);
    return _whole(yield* _composed(page.parent, rect, Option.none(), format, path), path);
});

const _object = Effect.fnUntraced(function* (doc: Document, { itemId, isolate }: Extract<Target, { readonly kind: 'object' }>, format: Format, directory: string) {
    const located = Array.head(
        Array.getSomes(
            Array.map(doc.spreads.everyItem().getElements(), (candidate) => {
                const direct = candidate.pageItems.itemByID(itemId);
                const found = Option.orElse(direct.isValid ? Array.head(direct.getElements()) : Option.none(), () => Array.findFirst(candidate.allPageItems, (entry) => entry.id === itemId));
                return Option.map(found, (hit) => ({ item: hit, spread: candidate }));
            }),
        ),
    );
    const { item, spread } = yield* Effect.fromOption(located, () => HostRejection.cases.itemNotFound.make({ itemId }));
    const rect = _spreadBox(item);
    const overlaps = Array.map(
        Array.filter(spread.pageItems.everyItem().getElements(), (other) => other.id !== item.id && !_disjoint(_spreadBox(other), rect)),
        (other) => ({ id: other.id, name: other.name, type: String(other.constructor.name) }),
    );
    const graphic = item.graphics.length > 0 ? Array.head(item.graphics.item(0).getElements()) : Option.none();
    const raster = Option.map(
        Option.orElse(
            Option.flatMap(graphic, (placed) => _ppi(Reflect.get(placed, 'effectivePpi'))),
            () => _ppi(Reflect.get(item, 'effectivePpi')),
        ),
        (values) => Math.max(...values),
    );
    const cap = isolate || overlaps.length === 0 ? raster : Option.none();
    const path = _file(directory, `object-${itemId}`, format);
    const rendered = yield* isolate
        ? _rendered(format, path, rect.right - rect.left, rect.bottom - rect.top, cap, (tmpSpread, pageTopLeft) =>
              Effect.try({ try: () => _moved(item, tmpSpread, pageTopLeft.left, pageTopLeft.top), catch: thrown }),
          )
        : _composed(spread, rect, cap, format, path);
    return { kind: 'image' as const, path, ...rendered, effectivePpi: raster, isolated: isolate, overlaps };
});

// --- [HANDLER] -------------------------------------------------------------------------

const snapshot: Handler = handler(Snapshot, Image, ({ target, format, directory }) =>
    app.documents.length === 0
        ? Effect.fail(HostRejection.cases.noActiveDocument.make({}))
        : Match.value(target).pipe(
              Match.discriminatorsExhaustive('kind')({
                  page: (page) => _page(app.activeDocument, page, format, directory),
                  spread: (spread) => _spread(app.activeDocument, spread, format, directory),
                  region: (region) => _region(app.activeDocument, region, format, directory),
                  object: (object) => _object(app.activeDocument, object, format, directory),
              }),
          ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { snapshot };
