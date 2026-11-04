import type { Source } from '@rasm/typography';
import { Effect, FileSystem, Option, Path, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

class AvailableFace extends Schema.Class<AvailableFace, { readonly brand: unique symbol }>('AvailableFace')({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, displayName: Schema.String, file: Schema.URL }) {
    static readonly Name = Schema.String.annotate({ description: 'PostScript name, full name, or family for default face, any case' });
    static readonly source = Effect.fnUntraced(function* ({ file, postscriptName }: AvailableFace) {
        const [files, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        return { data: yield* files.readFile(yield* path.fromFileUrl(file)), postscriptName: Option.some(postscriptName) } satisfies typeof Source.Type;
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { AvailableFace };
