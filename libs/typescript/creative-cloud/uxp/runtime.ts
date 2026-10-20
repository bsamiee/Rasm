import type { File } from 'adobe:uxp';
import { type Cause, Context, type Effect, type Scope } from 'effect';
import type { Manifest } from './manifest.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class Uxp extends Context.Service<
    Uxp,
    {
        readonly uxp: typeof import('adobe:uxp').default;
        readonly fs: typeof import('adobe:fs');
        readonly temporary: (name: string) => Effect.Effect<File, Cause.UnknownError, Scope.Scope>;
    }
>()('@rasm/uxp/runtime/Uxp') {}
class Plugin extends Context.Service<Plugin, Manifest>()('@rasm/uxp/runtime/Plugin') {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Plugin, Uxp };
