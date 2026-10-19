import { single } from '@rasm/search';
import { Effect, Schema, Struct } from 'effect';
import { Helper } from './helper.ts';

// --- [MODELS] --------------------------------------------------------------------------

class Face extends Schema.Class<Face, { readonly brand: unique symbol }>('Face')({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, fullName: Schema.String, file: Schema.String }) {
    static readonly installed = Effect.flatMap(Helper, (client) => client.faces());
    static readonly named = (name: string) => Effect.flatMap(Face.installed, (faces) => single('installed face', name)(faces, { names: (each) => [each.postscriptName, each.fullName, ...(each.style === 'Regular' ? [each.family] : [])], label: Struct.get('postscriptName') }));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Face };
