import { Option, Schema } from 'effect';
import { Cargo, decides } from '#/executors/cargo/executor';

// --- [MODELS] --------------------------------------------------------------------------

const Nextest = Cargo.annotate({
    title: 'Nextest',
    description: 'Runs cargo nextest commands from a workspace root, one process per command for every selected package, each task decided by the suite events of its own package and failing where a package that built holds no test',
});
const Suite = Schema.Struct({ type: Schema.Literal('suite'), event: Schema.Literals(['ok', 'failed']), nextest: Schema.Struct({ crate: Schema.String }) });
const Failed = Schema.Struct({ type: Schema.Literal('test'), event: Schema.Literal('failed'), name: Schema.String, stdout: Schema.optionalKey(Schema.String) });
const Finished = Schema.Struct({ reason: Schema.Literal('build-finished'), success: Schema.Literal(true) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const { execute, batch } = decides(({ messages, crate }) => {
    const reported = messages.filter(Schema.is(Suite)).filter(({ nextest }) => Option.contains(crate, nextest.crate));
    const failures = messages.filter(Schema.is(Failed));
    const empty = reported.length === 0 && failures.length === 0 && messages.some(Schema.is(Finished));
    const tests = failures.filter(({ name }) => Option.exists(crate, (held) => name.startsWith(`${held}::`))).map(({ name, stdout }) => (stdout === undefined ? name : `${name}\n${stdout}`));
    return { failed: empty || reported.some(({ event }) => event === 'failed'), proven: reported.length > 0, said: empty ? `${Option.getOrElse(crate, () => 'workspace')}: no tests to run\n` : tests.join('') };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { batch, execute, Nextest };
