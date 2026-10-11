// --- [TYPES] ---------------------------------------------------------------------------

declare module 'claude-code' {
    interface PluginState {
        'function-hooks': {
            database: Option<Option<string>>;
            rows: readonly Row[];
            notice: Option<Notice>;
            down: readonly Service[];
            edits: StateFamily<Queue>;
            plan: Plan;
            said: StateFamily<readonly string[]>;
            logged: readonly string[];
        };
    }
}

export type Option<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };
export type Row = Readonly<Record<string, string | number>>;

export interface Notice {
    readonly text: string;
    readonly at: number;
}
export interface Service {
    readonly name: string;
    readonly label: string;
}
export interface Queue {
    readonly format: readonly string[];
    readonly diagnostics: readonly string[];
}
export interface Plan {
    readonly path: Option<string>;
    readonly taskFile: Option<string>;
}
