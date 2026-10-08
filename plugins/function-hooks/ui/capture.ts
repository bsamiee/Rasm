import { bind, decoded, fromUndefined, map, none, type Option, ok, type Result, some } from '../composition.ts';
import type { Capture, Comparison, Recorded } from '../hooks/state.d.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Printed = { readonly view: string; readonly mode: string | null; readonly changed?: number } | { readonly view: string; readonly camera: { readonly size: readonly [number, number] }; readonly comparison: Comparison | null };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NAMED = /\.artifacts\/[\w-]+\/[\w.-]+\.png/gu;
const _PNG = /\.png$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [NAMING]

const capturePath = (tool: string, text: string): Option<string> =>
    fromUndefined(['mcp__rhino-mcp-platform__run_python', 'mcp__blender__execute_blender_code', 'mcp__mcp-for-blender__execute_blender_code'].includes(tool) ? [...text.matchAll(_NAMED)].map(([path]) => path).findLast((path) => !path.endsWith('-diff.png')) : undefined);

// --- [RECORD]

const recorded = (printed: Result<string>): Result<Option<Recorded>> =>
    bind(decoded<readonly { readonly Capture?: string }[]>('exiftool', printed), (tags) => {
        const text = tags[0]?.Capture;
        return text === undefined
            ? ok(none)
            : map(decoded<Printed>('Capture', ok(text)), (stored) =>
                  some<Recorded>(
                      'camera' in stored ? { kind: 'blender', view: stored.view, size: stored.camera.size, comparison: stored.comparison === null ? none : some(stored.comparison) } : { kind: 'rhino', view: stored.view, mode: stored.mode === null ? none : some(stored.mode), changed: fromUndefined(stored.changed) },
                  ),
              );
    });

// --- [CAPTION]

const diff = ({ path, record }: Capture): Option<string> => {
    if (record.kind === 'none') {
        return none;
    }
    if (record.value.kind === 'blender') {
        return record.value.comparison.kind === 'some' ? some(record.value.comparison.value.diff) : none;
    }
    return record.value.changed.kind === 'some' ? some(path.replace(_PNG, '-diff.png')) : none;
};

const caption = (head: string, record: Option<Recorded>): string => {
    const facts = (stored: Recorded): readonly string[] =>
        stored.kind === 'blender'
            ? [`view ${stored.view}`, `camera ${stored.size.join('×')}`, ...(stored.comparison.kind === 'some' ? [`changed ${stored.comparison.value.changed}`, ...(stored.comparison.value.outside ? ['outside frame'] : [])] : [])]
            : [`view ${stored.view}`, ...(stored.mode.kind === 'some' ? [`mode ${stored.mode.value}`] : []), ...(stored.changed.kind === 'some' ? [`changed ${stored.changed.value}`] : [])];
    return [head, ...(record.kind === 'some' ? facts(record.value) : [])].join(' · ');
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { caption, capturePath, diff, recorded };
