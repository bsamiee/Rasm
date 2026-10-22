import { Automation, type Installation } from '@rasm/automation';
import { Contract } from '@rasm/indesign';
import { Context, Effect, Path } from 'effect';
import { RpcClient } from 'effect/rpc';

// --- [SERVICES] ------------------------------------------------------------------------

class InDesignScript extends Context.Service<InDesignScript>()('@rasm/atelier/indesign-script/InDesignScript', {
    make: Effect.fnUntraced(function* (executable: string) {
        const [automation, path] = yield* Effect.all([Automation, Path.Path]);
        const script = yield* path.toFileUrl(path.join(path.dirname(path.dirname(executable)), 'Resources', 'main.idjs'));
        return {
            hosts: Effect.map(automation.applications({ toOpen: script }), (urls) => (row: Installation) => urls.some((url) => url.href === row.url.href)),
            client: (pid: number) =>
                RpcClient.make(Contract, { flatten: true }).pipe(Effect.provide(Automation.protocol(pid, (message) => ({ command: 'do script', arguments: { direct: { _tag: 'file', url: script.href }, language: 'uxpscript', 'with arguments': [message] } }))), Effect.provideService(Automation, automation)),
        };
    }),
}) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { InDesignScript };
