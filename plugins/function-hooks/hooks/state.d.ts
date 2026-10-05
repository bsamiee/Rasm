// --- [TYPES] ---------------------------------------------------------------------------

declare module 'claude-code' {
    interface PluginState {
        'function-hooks': { spawned: Readonly<Record<string, Spawned>> };
    }
}

export interface Lineage {
    readonly main: string;
    readonly worktree: string;
    readonly branch: string;
}
export interface Judging {
    readonly kind: 'range';
    readonly agent: string;
    readonly lineage: Lineage;
    readonly from: number;
    readonly to: number;
}
export interface Building {
    readonly kind: 'category';
    readonly agent: string;
    readonly lineage: Lineage;
    readonly session: string;
    readonly category: string;
}
export type Spawned = Judging | Building;
