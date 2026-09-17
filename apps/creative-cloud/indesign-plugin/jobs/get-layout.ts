// --- [IMPORTS] -------------------------------------------------------------------------

import { app, type Page, type PageItems } from 'adobe:indesign';
import { thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { GetLayout, type Item, Layout } from '@rasm/creative-cloud-server/indesign/jobs';
import { Array, Effect, Option, Schema } from 'effect';
import { type Handler, handler } from './handler.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Page_ = (typeof Layout)['Type']['pages'][number];

interface Box {
    readonly top: number;
    readonly left: number;
    readonly bottom: number;
    readonly right: number;
}

// --- [READS] ---------------------------------------------------------------------------

const _bounds: (input: unknown) => readonly [number, number, number, number] = Schema.decodeUnknownSync(Schema.Tuple([Schema.Number, Schema.Number, Schema.Number, Schema.Number]));

const _number: (input: unknown) => number = Schema.decodeUnknownSync(Schema.Number);

const _box = ([top, left, bottom, right]: readonly [number, number, number, number]): Box => ({ top, left, bottom, right });

const _items = (container: { readonly pageItems: PageItems }): readonly Item[] =>
    Array.flatMap(container.pageItems.everyItem().getElements(), (item) => {
        const type = item.constructor.name;
        const row: Item = { id: item.id, type, name: item.name, bounds: _box(_bounds(item.geometricBounds)), hasGraphic: item.graphics.length > 0 };
        return type === 'Group' ? [row, ..._items(item)] : [row];
    });

const _page = (page: Page, index: number, includeItems: boolean): Page_ => {
    const bounds = _box(_bounds(page.bounds));
    const width = bounds.right - bounds.left;
    const height = bounds.bottom - bounds.top;
    const preferences = page.marginPreferences;
    const margins = { top: _number(preferences.top), bottom: _number(preferences.bottom), inside: _number(preferences.left), outside: _number(preferences.right) };
    const side = String(page.side);
    const [left, right] = side === 'LEFT_HAND' ? [margins.outside, width - margins.inside] : [margins.inside, width - margins.outside];
    return {
        index,
        id: page.id,
        name: page.name,
        documentOffset: page.documentOffset,
        side,
        bounds: { top: 0, left: 0, bottom: height, right: width },
        margins,
        contentArea: { top: margins.top, left, bottom: height - margins.bottom, right },
        guides: Array.map(page.guides.everyItem().getElements(), (guide) => ({ id: guide.id, orientation: String(guide.orientation), location: _number(guide.location) })),
        items: includeItems ? Option.some(_items(page)) : Option.none(),
    };
};

const _windowed = (pages: readonly Page_[], itemCursor: number, limit: number): { readonly pages: readonly Page_[]; readonly itemCount: number; readonly itemCursor: Option.Option<number> } => {
    const counted = Array.map(pages, (page) => Option.match(page.items, { onNone: () => 0, onSome: Array.length }));
    const itemCount = Array.reduce(counted, 0, (total, count) => total + count);
    const end = Math.min(itemCount, itemCursor + limit);
    const offsets = Array.scan(counted, 0, (total, count) => total + count);
    return {
        pages: Array.map(pages, (page, position) => {
            const offset = Option.getOrElse(Array.get(offsets, position), () => 0);
            const start = Math.max(0, itemCursor - offset);
            const count = Math.max(0, end - offset) - start;
            return { ...page, items: Option.map(page.items, (items) => Array.take(Array.drop(items, start), count)) };
        }),
        itemCount,
        itemCursor: end < itemCount ? Option.some(end) : Option.none(),
    };
};

const _layout = (body: (typeof GetLayout)['Type']): (typeof Layout)['Type'] => {
    const doc = app.activeDocument;
    const pageCount = doc.pages.length;
    const end = Math.min(pageCount, body.pageCursor + body.limit);
    const pages = Array.map(
        Array.filter(Array.range(0, pageCount - 1), (index) => index >= body.pageCursor && index < end),
        (index) => _page(doc.pages.item(index), index, body.includeItems),
    );
    return {
        kind: 'layout',
        facingPages: doc.documentPreferences.facingPages,
        measurementUnits: { horizontal: String(doc.viewPreferences.horizontalMeasurementUnits), vertical: String(doc.viewPreferences.verticalMeasurementUnits) },
        ..._windowed(pages, body.itemCursor, body.limit),
        pageCount,
        pageCursor: end < pageCount ? Option.some(end) : Option.none(),
    };
};

// --- [HANDLER] -------------------------------------------------------------------------

const getLayout: Handler = handler(GetLayout, Layout, (body) =>
    app.documents.length === 0 ? Effect.fail(HostRejection.cases.noActiveDocument.make({})) : Effect.try({ try: () => _layout(body), catch: thrown }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { getLayout };
