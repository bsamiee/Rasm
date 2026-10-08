import { none, type Option, some } from '../composition.ts';
import type { Plan } from '../hooks/state.d.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _TASK = /^\d+\. /mu;

// --- [OPERATIONS] ----------------------------------------------------------------------

const touched = (plan: Plan, path: string, written: Option<string>, home: string): Option<Plan> => {
    const inside = (folder: string): boolean => path.endsWith('.md') && path.startsWith(`${folder}/`);
    if (inside(`${home}/.claude/plans`)) {
        return some({ ...plan, path: some(path) });
    }
    return written.kind === 'some' && ['/tmp', '/private/tmp'].some(inside) && _TASK.test(written.value) ? some({ ...plan, taskFile: some(path) }) : none;
};

const pointer = ({ path, taskFile }: Plan): Option<string> => {
    const named = [...(path.kind === 'some' ? [`plan ${path.value}`] : []), ...(taskFile.kind === 'some' ? [`task file ${taskFile.value}`] : [])];
    return named.length === 0 ? none : some(`Active ${named.join(', ')}. Delete each closed task from ${named.length === 1 ? 'it' : 'both'}`);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { pointer, touched };
