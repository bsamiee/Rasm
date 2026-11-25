import { Policy } from '@rasm/policy';
import { files } from '#/rules/files';
import { ghidra } from '#/rules/ghidra';
import { tasks } from '#/rules/tasks';
import { waits } from '#/rules/waits';

// --- [COMPOSITION] ---------------------------------------------------------------------

const policy = new Policy(files, waits, tasks, ghidra);

// --- [EXPORTS] -------------------------------------------------------------------------

export { policy };
