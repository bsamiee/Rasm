import { Context } from 'effect';
import type { Manifest } from '#/manifest';

// --- [SERVICES] ------------------------------------------------------------------------

class Uxp extends Context.Service<Uxp, { readonly uxp: typeof import('adobe:uxp').default; readonly fs: typeof import('adobe:fs') }>()('@rasm/uxp/runtime/Uxp') {}
class Plugin extends Context.Service<Plugin, Manifest>()('@rasm/uxp/runtime/Plugin') {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Plugin, Uxp };
