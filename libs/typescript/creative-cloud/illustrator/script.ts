import type { preserve } from '@rasm/extendscript/host';

// --- [TYPES] ---------------------------------------------------------------------------

interface Scripting {
    readonly app: object;
    readonly UserInteractionLevel: { readonly DONTDISPLAYALERTS: unknown };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const suppressAlerts = <A>({ app, UserInteractionLevel }: Scripting, body: () => A, host: { readonly preserve: typeof preserve }): A => host.preserve(app, { userInteractionLevel: UserInteractionLevel.DONTDISPLAYALERTS }, body);
const HostScript = { around: String(suppressAlerts) } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { HostScript };
