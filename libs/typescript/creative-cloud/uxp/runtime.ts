import { Context, Schema } from 'effect';
import type { Manifest } from '#/manifest';

// --- [SERVICES] ------------------------------------------------------------------------

class Uxp extends Context.Service<Uxp, { readonly uxp: typeof import('adobe:uxp').default; readonly fs: typeof import('adobe:fs') }>()('@rasm/uxp/runtime/Uxp') {
    static readonly Entry = Schema.Struct({ isEntry: Schema.Literal(true), url: Schema.Union([Schema.String, Schema.URL]) });
}
class Plugin extends Context.Service<Plugin, Manifest>()('@rasm/uxp/runtime/Plugin') {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Plugin, Uxp };
