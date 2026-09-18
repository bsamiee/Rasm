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
import { opened } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { type Bounds, BUDGET, DPI, dpi, pixels, points } from '@rasm/creative-cloud-server/images';
import type { Body, Format, Reply, Target } from '@rasm/creative-cloud-server/indesign/jobs';
import { AbsolutePath } from '@rasm/creative-cloud-server/values';
import { Array, Effect, Match, Number, Option, Result, Schema, Struct } from 'effect';
import { swapped } from '../host.ts';

// --- [GEOMETRY] ------------------------------------------------------------------------

const _spreadBox = (item: Pick<PageItem, 'resolve'>): Bounds => {
    const corners = Array.map([AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.TOP_RIGHT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR, AnchorPoint.BOTTOM_LEFT_ANCHOR], (anchor) => {
        const [[x, y]] = item.resolve([anchor, BoundingBoxLimits.GEOMETRIC_PATH_BOUNDS], CoordinateSpaces.SPREAD_COORDINATES);
        return { x, y };
    });
    const xs = Array.map(corners, Struct.get('x'));
    const ys = Array.map(corners, Struct.get('y'));
    return { left: Math.min(...xs), right: Math.max(...xs), top: Math.min(...ys), bottom: Math.max(...ys) };
};

const _disjoint = (bounds: Bounds, rect: Bounds): boolean => bounds.left >= rect.right || bounds.right <= rect.left || bounds.top >= rect.bottom || bounds.bottom <= rect.top;

const _full = (widthPt: number, heightPt: number, resolution: number): Pick<Reply<'snapshot'>, 'widthPx' | 'heightPx' | 'contentWidthPx' | 'contentHeightPx' | 'dpi'> => {
    const widthPx = pixels(widthPt, resolution);
    const heightPx = pixels(heightPt, resolution);
    return { widthPx, heightPx, contentWidthPx: widthPx, contentHeightPx: heightPx, dpi: resolution };
};

// --- [EXPORT] --------------------------------------------------------------------------

const _exported = (doc: Document, format: Format, resolution: number, pageString: string, spread: boolean, path: string): Effect.Effect<void, HostRejection> =>
    Effect.scoped(
        Effect.flatMap(
            Match.value(format).pipe(
                Match.discriminatorsExhaustive('kind')({
                    jpg: () =>
                        Effect.as(
                            swapped(app.jpegExportPreferences, {
                                jpegQuality: JPEGOptionsQuality.MAXIMUM,
                                exportResolution: resolution,
                                pageString,
                                jpegExportRange: ExportRangeOrAllPages.EXPORT_RANGE,
                                exportingSpread: spread,
                            }),
                            ExportFormat.JPG,
                        ),
                    png: ({ transparent }) =>
                        Effect.as(
                            swapped(app.pngExportPreferences, {
                                pngQuality: PNGQualityEnum.MAXIMUM,
                                exportResolution: resolution,
                                pageString,
                                pngExportRange: PNGExportRangeEnum.EXPORT_RANGE,
                                exportingSpread: spread,
                                transparentBackground: transparent,
                            }),
                            ExportFormat.PNG_FORMAT,
                        ),
                }),
            ),
            (target) =>
                Effect.sync(() => {
                    doc.exportFile(target, path);
                }),
        ),
    );

const _file = (directory: string, name: string, format: Format): AbsolutePath => AbsolutePath.make(`${directory}/${name}.${format.kind}`);

const _whole = (rendered: ReturnType<typeof _full>, path: AbsolutePath): Reply<'snapshot'> => ({ kind: 'image', path, ...rendered, effectivePpi: Option.none(), isolated: false, overlaps: [] });

// --- [ISOLATION] -----------------------------------------------------------------------

const _moved = (item: PageItem, tmpSpread: Spread, targetLeft: number, targetTop: number): void => {
    const duplicate = item.duplicate(tmpSpread);
    const current = _spreadBox(duplicate);
    duplicate.move(undefined, [targetLeft - current.left, targetTop - current.top]);
};

const _placed = (list: readonly PageItem[], rect: Bounds, tmpSpread: Spread, pageTopLeft: Bounds): Effect.Effect<void, HostRejection> =>
    Effect.forEach(
        Array.filter(
            Array.map(list, (item) => ({ item, box: _spreadBox(item) })),
            ({ box }) => !_disjoint(box, rect),
        ),
        ({ item, box }) => Effect.sync(() => _moved(item, tmpSpread, pageTopLeft.left + (box.left - rect.left), pageTopLeft.top + (box.top - rect.top))),
        { discard: true },
    );

const _rendered = Effect.fnUntraced(function* (
    format: Format,
    path: string,
    widthPt: number,
    heightPt: number,
    cap: Option.Option<number>,
    populate: (tmpSpread: Spread, pageTopLeft: Bounds) => Effect.Effect<void, HostRejection>,
) {
    const floor = points(BUDGET.detail.longEdgePx, DPI.maximum);
    const pageWidth = Math.max(widthPt, floor);
    const pageHeight = Math.max(heightPt, floor);
    const wanted = dpi('detail', pageWidth, pageHeight);
    const resolution = Option.match(
        Option.filter(cap, (ppi) => ppi < wanted),
        { onNone: () => wanted, onSome: (ppi) => Number.clamp(Math.round(ppi), DPI) },
    );
    const tmp = yield* Effect.acquireRelease(
        Effect.sync(() => {
            const created = app.documents.add(false);
            created.documentPreferences.properties = { facingPages: false, pagesPerDocument: 1, pageWidth, pageHeight };
            return created;
        }),
        (created) =>
            Effect.sync(() => {
                created.close(SaveOptions.NO);
            }),
    );
    const tmpSpread = tmp.spreads.item(0);
    yield* populate(tmpSpread, _spreadBox(tmp.pages.item(0)));
    yield* Effect.forEach(
        Array.filter(
            [
                { needed: pageHeight > heightPt, box: { top: heightPt, left: 0, bottom: pageHeight, right: pageWidth } },
                { needed: pageWidth > widthPt, box: { top: 0, left: widthPt, bottom: pageHeight, right: pageWidth } },
            ],
            Struct.get('needed'),
        ),
        ({ box }) =>
            Effect.sync(() => {
                const mask = tmpSpread.rectangles.add();
                mask.geometricBounds = [box.top, box.left, box.bottom, box.right];
                mask.fillColor = 'Paper';
                mask.strokeColor = 'None';
            }),
        { discard: true },
    );
    yield* _exported(tmp, format, resolution, '+1', false, path);
    return { ..._full(pageWidth, pageHeight, resolution), contentWidthPx: pixels(widthPt, resolution), contentHeightPx: pixels(heightPt, resolution) };
}, Effect.scoped);

const _composed = (source: Spread | MasterSpread, rect: Bounds, cap: Option.Option<number>, format: Format, path: string): Effect.Effect<ReturnType<typeof _full>, HostRejection> =>
    _rendered(format, path, rect.right - rect.left, rect.bottom - rect.top, cap, (tmpSpread, pageTopLeft) => {
        const masters = Array.dedupeWith(
            Array.filter(Array.map(source.pages.everyItem().getElements(), Struct.get('appliedMaster')), Struct.get('isValid')),
            (left: MasterSpread, right: MasterSpread) => left.id === right.id,
        );
        return Effect.andThen(
            Effect.forEach(masters, (master) => _placed(Array.reverse(master.pageItems.everyItem().getElements()), rect, tmpSpread, pageTopLeft), { discard: true }),
            _placed(Array.reverse(source.pageItems.everyItem().getElements()), rect, tmpSpread, pageTopLeft),
        );
    });

// --- [TARGETS] -------------------------------------------------------------------------

const _indexed = (index: number, count: number): Effect.Effect<number, HostRejection> =>
    Effect.fromResult(
        Result.liftPredicate(
            index,
            (candidate) => candidate < count,
            () => HostRejection.cases.pageOutOfRange.make({ index, count }),
        ),
    );

const _page = Effect.fnUntraced(function* (doc: Document, { index, budget }: Extract<Target, { readonly kind: 'page' }>, format: Format, directory: string) {
    yield* _indexed(index, doc.pages.length);
    const [top, left, bottom, right] = doc.pages.item(index).bounds;
    const resolution = dpi(budget, right - left, bottom - top);
    const path = _file(directory, `page-${index}`, format);
    yield* _exported(doc, format, resolution, `+${index + 1}`, false, path);
    return _whole(_full(right - left, bottom - top, resolution), path);
});

const _spread = Effect.fnUntraced(function* (doc: Document, { index, budget }: Extract<Target, { readonly kind: 'spread' }>, format: Format, directory: string) {
    yield* _indexed(index, doc.spreads.length);
    const before = Array.reduce(Array.take(doc.spreads.everyItem().getElements(), index), 0, (total, spread) => total + spread.pages.length);
    const pages = doc.spreads.item(index).pages.everyItem().getElements();
    const sizes = Array.map(pages, (page) => {
        const [top, left, bottom, right] = page.bounds;
        return { width: right - left, height: bottom - top };
    });
    const width = Array.reduce(sizes, 0, (total, size) => total + size.width);
    const height = Math.max(0, ...Array.map(sizes, Struct.get('height')));
    const resolution = dpi(budget, width, height);
    const path = _file(directory, `spread-${index}`, format);
    yield* _exported(doc, format, resolution, pages.length === 1 ? `+${before + 1}` : `+${before + 1}-+${before + pages.length}`, true, path);
    return _whole(_full(width, height, resolution), path);
});

const _region = Effect.fnUntraced(function* (doc: Document, { index, region: [x0, y0, x1, y1] }: Extract<Target, { readonly kind: 'region' }>, format: Format, directory: string) {
    yield* _indexed(index, doc.pages.length);
    const page = doc.pages.item(index);
    const bounds = _spreadBox(page);
    const width = bounds.right - bounds.left;
    const height = bounds.bottom - bounds.top;
    const rect = { left: bounds.left + x0 * width, top: bounds.top + y0 * height, right: bounds.left + x1 * width, bottom: bounds.top + y1 * height };
    const path = _file(directory, `region-${index}`, format);
    return _whole(yield* _composed(page.parent, rect, Option.none(), format, path), path);
});

const _object = Effect.fnUntraced(function* (doc: Document, { itemId, isolate }: Extract<Target, { readonly kind: 'object' }>, format: Format, directory: string) {
    const { item, spread } = yield* Effect.fromOption(
        Array.findFirst(
            Array.flatMap(doc.spreads.everyItem().getElements(), (candidate) => Array.map(candidate.allPageItems, (entry) => ({ item: entry, spread: candidate }))),
            (entry) => entry.item.id === itemId,
        ),
        () => HostRejection.cases.itemNotFound.make({ itemId }),
    );
    const rect = _spreadBox(item);
    const overlaps = Array.map(
        Array.filter(spread.pageItems.everyItem().getElements(), (other) => other.id !== item.id && !_disjoint(_spreadBox(other), rect)),
        (other) => ({ id: other.id, name: other.name, type: other.constructor.name }),
    );
    const ppi = Schema.decodeUnknownOption(Schema.NonEmptyArray(Schema.Number));
    const raster = Option.map(
        Option.firstSomeOf([Option.flatMap(Array.head(item.graphics.everyItem().getElements()), (placed) => ppi(Reflect.get(placed, 'effectivePpi'))), ppi(Reflect.get(item, 'effectivePpi'))]),
        (values) => Math.max(...values),
    );
    const cap = Option.zipRight(
        Option.liftPredicate(overlaps, (others) => isolate || Array.isReadonlyArrayEmpty(others)),
        raster,
    );
    const path = _file(directory, `object-${itemId}`, format);
    const rendered = yield* isolate
        ? _rendered(format, path, rect.right - rect.left, rect.bottom - rect.top, cap, (tmpSpread, pageTopLeft) => Effect.sync(() => _moved(item, tmpSpread, pageTopLeft.left, pageTopLeft.top)))
        : _composed(spread, rect, cap, format, path);
    return { kind: 'image' as const, path, ...rendered, effectivePpi: raster, isolated: isolate, overlaps };
});

// --- [HANDLER] -------------------------------------------------------------------------

const snapshot = ({ target, format, directory }: Body<'snapshot'>): Effect.Effect<Reply<'snapshot'>, HostRejection> =>
    Effect.flatMap(opened(app), (doc) =>
        Match.value(target).pipe(
            Match.discriminatorsExhaustive('kind')({
                page: (page) => _page(doc, page, format, directory),
                spread: (spread) => _spread(doc, spread, format, directory),
                region: (region) => _region(doc, region, format, directory),
                object: (object) => _object(doc, object, format, directory),
            }),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { snapshot };
