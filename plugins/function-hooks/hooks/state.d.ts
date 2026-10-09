// --- [TYPES] ---------------------------------------------------------------------------

declare module 'claude-code' {
    interface PluginState {
        'function-hooks': {
            database: Option<Option<string>>;
            notice: Option<Notice>;
            down: readonly Service[];
            edits: StateFamily<{ readonly format: readonly string[]; readonly diagnostics: readonly string[] }>;
            capture: StateFamily<Option<Capture>>;
            plan: Plan;
            said: StateFamily<readonly string[]>;
        };
    }
}

export type Option<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };

export interface Notice {
    readonly text: string;
    readonly at: number;
}
export interface Service {
    readonly name: string;
    readonly port: string;
}
export interface Plan {
    readonly path: Option<string>;
    readonly taskFile: Option<string>;
}
export interface Comparison {
    readonly changed: number;
    readonly diff: string;
    readonly outside: boolean;
}
export type Recorded = { readonly kind: 'rhino'; readonly view: string; readonly mode: Option<string>; readonly changed: Option<number> } | { readonly kind: 'blender'; readonly view: string; readonly size: readonly [number, number]; readonly comparison: Option<Comparison> };
export interface Capture {
    readonly path: string;
    readonly record: Option<Recorded>;
    readonly generation: number;
}
