import type { preserve, within } from '@rasm/extendscript/host';

// --- [TYPES] ---------------------------------------------------------------------------

interface Scripting {
    readonly app: object;
    readonly UserInteractionLevel: { readonly DONTDISPLAYALERTS: unknown };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const interactionless = <A>({ app, UserInteractionLevel }: Scripting, body: () => A, runtime: { readonly preserve: typeof preserve; readonly within: typeof within }): A => runtime.within(() => runtime.preserve(app, { userInteractionLevel: UserInteractionLevel.DONTDISPLAYALERTS }), body);
const ScriptHost = { around: String(interactionless) } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { ScriptHost };
