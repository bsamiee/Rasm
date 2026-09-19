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
    JpegColorSpaceEnum,
    type MasterSpread,
    MeasurementUnits,
    type PageItem,
    PNGColorSpaceEnum,
    PNGExportRangeEnum,
    PNGQualityEnum,
    RulerOrigin,
    type Spread,
} from 'adobe:indesign';
import { thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { type Bounds, dpi, pixels, type Region } from '@rasm/creative-cloud-server/images';
import type { Body, Format, Reply, Target } from '@rasm/creative-cloud-server/indesign/jobs';
import { AbsolutePath } from '@rasm/creative-cloud-server/values';
import { Array, Effect, Match, Option, Result, Schema, Struct } from 'effect';
import { close, documentFor, fileFor, swapped } from '../host.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _MAX_EXPORT_DPI = 2400;
const _FULL_REGION: Region = [0, 0, 1, 1];

// --- [GEOMETRY] ------------------------------------------------------------------------

const _spreadBox = (item: Pick<PageItem, 'resolve'>, bounds: BoundingBoxLimits, [x0, y0, x1, y1]: Region): Bounds => {
    const [[left, top]] = item.resolve([AnchorPoint.TOP_LEFT_ANCHOR, bounds], CoordinateSpaces.SPREAD_COORDINATES);
    const [[rightX, rightY]] = item.resolve([AnchorPoint.TOP_RIGHT_ANCHOR, bounds], CoordinateSpaces.SPREAD_COORDINATES);
    const [[bottomX, bottomY]] = item.resolve([AnchorPoint.BOTTOM_LEFT_ANCHOR, bounds], CoordinateSpaces.SPREAD_COORDINATES);
    const corners = Array.flatMap([x0, x1], (x) => Array.map([y0, y1], (y) => ({ x: left + x * (rightX - left) + y * (bottomX - left), y: top + x * (rightY - top) + y * (bottomY - top) })));
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

const _exported: (subject: Pick<Document, 'exportFile'>, format: Format, resolution: number, pageString: string, spread: boolean, path: AbsolutePath) => Effect.Effect<void, HostRejection> =
    Effect.fnUntraced(function* (subject, format, resolution, pageString, spread, path) {
        const file = yield* fileFor(path, true);
        const target = yield* Match.value(format).pipe(
            Match.discriminatorsExhaustive('kind')({
                jpg: () =>
                    Effect.as(
                        swapped(app.jpegExportPreferences, {
                            jpegQuality: JPEGOptionsQuality.MAXIMUM,
                            exportResolution: resolution,
                            pageString,
                            jpegExportRange: ExportRangeOrAllPages.EXPORT_RANGE,
                            exportingSpread: spread,
                            useDocumentBleeds: false,
                            jpegColorSpace: JpegColorSpaceEnum.RGB,
                            embedColorProfile: true,
                            antiAlias: true,
                            simulateOverprint: false,
                            jpegSuffix: '',
                            exportingHiddenSpread: true,
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
                            useDocumentBleeds: false,
                            transparentBackground: transparent,
                            pngColorSpace: PNGColorSpaceEnum.RGB,
                            antiAlias: true,
                            simulateOverprint: false,
                            pngSuffix: '',
                            exportingHiddenSpread: true,
                        }),
                        ExportFormat.PNG_FORMAT,
                    ),
            }),
        );
        yield* Effect.try({ try: () => subject.exportFile(target, file), catch: thrown });
    }, Effect.scoped);

const _file = (directory: string, name: string, format: Format): AbsolutePath => AbsolutePath.make(`${directory}/${name}.${format.kind}`);

const _whole = (rendered: ReturnType<typeof _full>, path: AbsolutePath): Reply<'snapshot'> => ({ kind: 'image', path, ...rendered, effectivePpi: Option.none(), isolated: false, overlaps: [] });

// --- [ISOLATION] -----------------------------------------------------------------------

const _moved = (item: PageItem, tmpSpread: Spread, targetLeft: number, targetTop: number): void => {
    const duplicate = item.duplicate(tmpSpread);
    const current = _spreadBox(duplicate, BoundingBoxLimits.OUTER_STROKE_BOUNDS, _FULL_REGION);
    duplicate.move(undefined, [targetLeft - current.left, targetTop - current.top]);
};

const _placed = (list: readonly PageItem[], rect: Bounds, tmpSpread: Spread, pageTopLeft: Bounds): Effect.Effect<void, HostRejection> =>
    Effect.forEach(
        Array.filter(
            Array.map(list, (item) => ({ item, box: _spreadBox(item, BoundingBoxLimits.OUTER_STROKE_BOUNDS, _FULL_REGION) })),
            ({ box }) => !_disjoint(box, rect),
        ),
        ({ item, box }) => Effect.sync(() => _moved(item, tmpSpread, pageTopLeft.left + (box.left - rect.left), pageTopLeft.top + (box.top - rect.top))),
        { discard: true },
    );

const _rendered = Effect.fnUntraced(function* (
    format: Format,
    path: AbsolutePath,
    widthPt: number,
    heightPt: number,
    populate: (tmpSpread: Spread, pageTopLeft: Bounds) => Effect.Effect<void, HostRejection>,
) {
    const resolution = Math.min(dpi('detail', widthPt, heightPt), _MAX_EXPORT_DPI);
    const tmp = yield* Effect.acquireRelease(
        Effect.sync(() => app.documents.add(false, { documentPreferences: { facingPages: false, pagesPerDocument: 1 } })),
        (created) => close(created).pipe(Effect.orDie),
    );
    const pages = [tmp, ...tmp.pages.everyItem().getElements(), ...Array.flatMap(tmp.masterSpreads.everyItem().getElements(), (master) => master.pages.everyItem().getElements())];
    Array.forEach(pages, (page) => Object.assign(page.marginPreferences, { top: 0, left: 0, bottom: 0, right: 0, columnCount: 1, columnGutter: 0 }));
    Object.assign(tmp.viewPreferences, { horizontalMeasurementUnits: MeasurementUnits.POINTS, verticalMeasurementUnits: MeasurementUnits.POINTS, rulerOrigin: RulerOrigin.PAGE_ORIGIN });
    Object.assign(tmp.documentPreferences, { pageWidth: widthPt, pageHeight: heightPt });
    const tmpSpread = tmp.spreads.item(0);
    const bounds = _spreadBox(tmp.pages.item(0), BoundingBoxLimits.GEOMETRIC_PATH_BOUNDS, _FULL_REGION);
    yield* populate(tmpSpread, bounds);
    yield* _exported(tmp, format, resolution, '+1', false, path);
    return { ..._full(bounds.right - bounds.left, bounds.bottom - bounds.top, resolution), contentWidthPx: pixels(widthPt, resolution), contentHeightPx: pixels(heightPt, resolution) };
}, Effect.scoped);

const _stack = (spread: Spread | MasterSpread): readonly PageItem[] => Array.reverse(Array.filter(spread.allPageItems, (item) => item.parent.id === spread.id));

const _composed = (source: Spread | MasterSpread, rect: Bounds, format: Format, path: AbsolutePath): Effect.Effect<ReturnType<typeof _full>, HostRejection> =>
    _rendered(format, path, rect.right - rect.left, rect.bottom - rect.top, (tmpSpread, pageTopLeft) => {
        const masters = Array.dedupeWith(
            Array.filter(Array.map(source.pages.everyItem().getElements(), Struct.get('appliedMaster')), Struct.get('isValid')),
            (left: MasterSpread, right: MasterSpread) => left.id === right.id,
        );
        return Effect.andThen(
            Effect.forEach(masters, (master) => _placed(_stack(master), rect, tmpSpread, pageTopLeft), { discard: true }),
            _placed(_stack(source), rect, tmpSpread, pageTopLeft),
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
    const resolution = Math.min(dpi(budget, right - left, bottom - top), _MAX_EXPORT_DPI);
    const path = _file(directory, `page-${index}`, format);
    yield* _exported(doc, format, resolution, `+${index + 1}`, false, path);
    return _whole(_full(right - left, bottom - top, resolution), path);
});

const _spread = Effect.fnUntraced(function* (doc: Document, { index, budget }: Extract<Target, { readonly kind: 'spread' }>, format: Format, directory: string) {
    yield* _indexed(index, doc.spreads.length);
    const spread = doc.spreads.item(index);
    const pages = spread.pages.everyItem().getElements();
    const boxes = Array.map(pages, (page) => _spreadBox(page, BoundingBoxLimits.GEOMETRIC_PATH_BOUNDS, _FULL_REGION));
    const width = Math.max(...Array.map(boxes, Struct.get('right'))) - Math.min(...Array.map(boxes, Struct.get('left')));
    const height = Math.max(...Array.map(boxes, Struct.get('bottom'))) - Math.min(...Array.map(boxes, Struct.get('top')));
    const resolution = Math.min(dpi(budget, width, height), _MAX_EXPORT_DPI);
    const path = _file(directory, `spread-${index}`, format);
    const first = spread.pages.firstItem().documentOffset + 1;
    const last = spread.pages.lastItem().documentOffset + 1;
    yield* _exported(doc, format, resolution, first === last ? `+${first}` : `+${first}-+${last}`, true, path);
    return _whole(_full(width, height, resolution), path);
});

const _region = Effect.fnUntraced(function* (doc: Document, { index, region }: Extract<Target, { readonly kind: 'region' }>, format: Format, directory: string) {
    yield* _indexed(index, doc.pages.length);
    const page = doc.pages.item(index);
    const rect = _spreadBox(page, BoundingBoxLimits.GEOMETRIC_PATH_BOUNDS, region);
    const path = _file(directory, `region-${index}`, format);
    return _whole(yield* _composed(page.parent, rect, format, path), path);
});

const _object = Effect.fnUntraced(function* (doc: Document, { itemId, isolate }: Extract<Target, { readonly kind: 'object' }>, format: Format, directory: string) {
    const { item, spread } = yield* Effect.fromOption(
        Array.findFirst(
            Array.flatMap(doc.spreads.everyItem().getElements(), (candidate) => Array.map(candidate.allPageItems, (entry) => ({ item: entry, spread: candidate }))),
            (entry) => entry.item.id === itemId,
        ),
        () => HostRejection.cases.itemNotFound.make({ itemId }),
    );
    const rect = _spreadBox(item, BoundingBoxLimits.OUTER_STROKE_BOUNDS, _FULL_REGION);
    const overlaps = Array.map(
        Array.filter(_stack(spread), (other) => other.id !== item.id && !_disjoint(_spreadBox(other, BoundingBoxLimits.OUTER_STROKE_BOUNDS, _FULL_REGION), rect)),
        (other) => ({ id: other.id, name: other.name, type: other.constructor.name }),
    );
    const ppi = Schema.decodeUnknownOption(Schema.NonEmptyArray(Schema.Number));
    const raster = Option.map(
        Option.firstSomeOf([Option.flatMap(Array.head(item.graphics.everyItem().getElements()), (placed) => ppi(Reflect.get(placed, 'effectivePpi'))), ppi(Reflect.get(item, 'effectivePpi'))]),
        (values) => Math.max(...values),
    );
    const path = _file(directory, `object-${itemId}`, format);
    const resolution = Math.min(dpi('detail', rect.right - rect.left, rect.bottom - rect.top), _MAX_EXPORT_DPI);
    const rendered = yield* isolate
        ? Effect.as(_exported(item, format, resolution, '+1', false, path), _full(rect.right - rect.left, rect.bottom - rect.top, resolution))
        : _composed(spread, rect, format, path);
    return { kind: 'image' as const, path, ...rendered, effectivePpi: raster, isolated: isolate, overlaps };
});

// --- [HANDLER] -------------------------------------------------------------------------

const snapshot = ({ target, format, directory, documentId }: Body<'snapshot'>): Effect.Effect<Reply<'snapshot'>, HostRejection> =>
    Effect.flatMap(documentFor(documentId), (doc) =>
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
