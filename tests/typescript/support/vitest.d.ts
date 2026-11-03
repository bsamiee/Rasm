import 'vitest';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'vitest' {
    interface ProvidedContext {
        readonly artifacts: string;
    }
}
