import { Policy } from '@rasm/policy';
import { hooks } from '@rasm/policy/claude';
import { ghidra } from '#/rules/ghidra';
import { programs } from '#/rules/programs';
import { removal } from '#/rules/removal';
import { search } from '#/rules/search';
import { tasks } from '#/rules/tasks';
import { waits } from '#/rules/waits';

// --- [COMPOSITION] ---------------------------------------------------------------------

function register(on: Parameters<typeof hooks>[0]) {
    hooks(on, new Policy(search, removal, programs, waits, tasks, ghidra));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { register };
