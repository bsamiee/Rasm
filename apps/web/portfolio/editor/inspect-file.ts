import { Inspection } from '@rasm/media/inspection';
import { Array, Effect, Schema, String, Struct } from 'effect';
import { Asset, ImageAsset } from '#/model/asset.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const inspectFile = Effect.fn('inspectFile')(function* (file: File, id: string) {
    const { mime } = ImageAsset.Rendition.fields;
    const inspection = yield* Inspection.read(file, { minimumWidth: 256, ratio: 2, concurrency: 8, type: Schema.is(mime)(file.type) ? file.type : Array.lastNonEmpty(mime.literals) });
    const { _tag, renditions, ...measured } = Inspection.$is('Image')(inspection) ? inspection : { ...inspection, renditions: [] };
    return {
        asset: yield* Schema.decodeUnknownEffect(Asset)({
            ...measured,
            kind: String.uncapitalize(_tag),
            id,
            name: file.name,
            size: file.size,
            mime: file.type,
            ...(Array.isReadonlyArrayNonEmpty(renditions) && { renditions: renditions.map((rendition) => ({ width: rendition.width, size: rendition.file.size, mime: rendition.file.type })) }),
        }),
        renditions: Array.map(renditions, Struct.get('file')),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { inspectFile };
