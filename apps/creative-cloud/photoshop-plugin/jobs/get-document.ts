// --- [IMPORTS] -------------------------------------------------------------------------

import { app, type Document, type Layer } from 'adobe:photoshop';
import { type Handler, handler, thrown } from '@rasm/creative-cloud-server/client';
import { HostRejection } from '@rasm/creative-cloud-server/errors';
import { type Active, DocumentState, GetDocument, type LayerRow } from '@rasm/creative-cloud-server/photoshop/jobs';
import { Array, Effect, Option } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Placed {
    readonly layer: Layer;
    readonly depth: number;
    readonly parentId: Option.Option<number>;
}

// --- [LAYERS] --------------------------------------------------------------------------

const _children = (layer: Layer): readonly Layer[] => Option.getOrElse(Option.fromNullishOr(layer.layers), () => []);

const flatten = (candidates: Iterable<Layer>, depth: number, parentId: Option.Option<number>): readonly Placed[] =>
    Array.flatMap(Array.fromIterable(candidates), (layer) => [{ layer, depth, parentId }, ...flatten(_children(layer), depth + 1, Option.some(layer.id))]);

const _row = ({ layer, depth, parentId }: Placed): LayerRow => ({ id: layer.id, name: layer.name, kind: layer.kind, visible: layer.visible, depth, parentId, children: _children(layer).length });

// --- [DOCUMENTS] -----------------------------------------------------------------------

const _summary = (open: Document): { readonly id: number; readonly name: string; readonly path: string; readonly saved: boolean } => ({
    id: open.id,
    name: open.name,
    path: open.path,
    saved: open.saved,
});

const _active = (): Option.Option<Document> => (app.documents.length === 0 ? Option.none() : Option.some(app.activeDocument));

const document: (documentId: Option.Option<number>) => Effect.Effect<Document, HostRejection> = Option.match({
    onNone: () => Effect.fromOption(() => HostRejection.cases.noActiveDocument.make({}))(_active()),
    onSome: (id: number) => Effect.fromOption(() => HostRejection.cases.documentNotFound.make({ documentId: id }))(Array.findFirst(app.documents, (open) => open.id === id)),
});

const _state = (open: Document, limit: number, cursor: number, depth: number): (typeof Active)['Type'] => {
    const rows = Array.map(
        Array.filter(flatten(open.layers, 0, Option.none()), (placed) => placed.depth <= depth),
        _row,
    );
    return {
        id: open.id,
        mode: open.mode,
        bitsPerChannel: open.bitsPerChannel,
        colorProfileName: open.colorProfileName,
        width: open.width,
        height: open.height,
        resolution: open.resolution,
        layers: Array.take(Array.drop(rows, cursor), limit),
        layerCount: rows.length,
        layerCursor: cursor + limit < rows.length ? Option.some(cursor + limit) : Option.none(),
    };
};

// --- [HANDLER] -------------------------------------------------------------------------

const getDocument: Handler = handler(GetDocument, DocumentState, ({ documentId, limit, cursor, depth }) =>
    Effect.gen(function* () {
        const documents = yield* Effect.try({ try: () => Array.map(Array.fromIterable(app.documents), _summary), catch: thrown });
        const active = yield* Option.match(documentId, { onNone: () => Effect.succeed(_active()), onSome: (id) => Effect.map(document(Option.some(id)), Option.some) });
        return { kind: 'document' as const, documents, active: Option.map(active, (open) => _state(open, limit, cursor, depth)) };
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { document, flatten, getDocument };
