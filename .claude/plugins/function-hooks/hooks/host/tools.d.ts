// Input of the close tool tool-call.ts serves and findings.ts decides, cleaned stamps a guidance part and null resets the stamp,
// and the two ops the 2.1.263 load line lists that its OpEventOf and OpValueOf omit (anthropics/claude-code#92469)
declare module 'claude-code' {
    interface McpToolInputs {
        'mcp__function-hooks__close': {
            batchId: string;
            rows?: readonly { id: string; verdict: string; landedFile: string; lines: string; proof: string }[];
            cleaned?: Readonly<Record<string, string | null>>;
        };
    }

    // The credential $.session.authorize() mints for the auth field of $.http.fetch, kind from the first-party header the build reads
    interface SessionAuthorization {
        readonly handle: unknown;
        readonly kind: 'bearer' | 'api-key';
    }

    // OpEventOf is a type alias no interface merges into, so the two ops sit beside it until a release declares them
    interface UndeclaredOpEventOf {
        // $.session.authorize(), the probe logged e as {}
        'session.authorize': NoArgs;
        // $.flag.value(name, fallback), the runtime check refuses an empty name or an absent fallback
        'flag.value': {
            name: string;
            fallback: unknown;
        };
    }

    interface UndeclaredOpValueOf {
        // null under the build constant of 2.1.263, the probe's value, or with no first-party credential, else the credential
        'session.authorize': SessionAuthorization | null;
        // The flag's value or the fallback, the build has no flag table and withholds the flag noun from $
        'flag.value': unknown;
    }
}
