import { none, type Option, some } from '../composition.ts';
import type { Plan } from '../hooks/state.d.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _moved = (held: Option<string>, path: string): boolean => held.kind === 'none' || held.value !== path;
const planned = (plan: Plan, path: string): Option<Plan> => (_moved(plan.path, path) ? some({ ...plan, path: some(path) }) : none);
const tasked = (plan: Plan, path: string, temp: string): Option<Plan> => (path.endsWith('.md') && path.startsWith(`${temp}/`) && _moved(plan.taskFile, path) ? some({ ...plan, taskFile: some(path) }) : none);

const pointer = ({ path, taskFile }: Plan): Option<string> => {
    const named = [...(path.kind === 'some' ? [`plan ${path.value}`] : []), ...(taskFile.kind === 'some' ? [`task file ${taskFile.value}`] : [])];
    return named.length === 0 ? none : some(`Active ${named.join(', ')}. Delete each closed task from ${named.length === 1 ? 'it' : 'both'}`);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { planned, pointer, tasked };
