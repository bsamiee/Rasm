import { DateTime, Effect, Layer, type Scope } from 'effect';
import { Execution, type Kind } from '#/runtime/execution';

// --- [COMPOSITION] ---------------------------------------------------------------------

const scheduled = <E, R>(job: (controller: ScheduledController) => Effect.Effect<unknown, E, R | Execution | Scope.Scope>): Kind<R> => ({
    layer: Layer.empty,
    handlers: (ready) => ({
        scheduled: (controller, _, execution) =>
            ready().then((isolate) => Execution.run(isolate, execution)(Effect.asVoid(Effect.withSpan(job(controller), `scheduled ${controller.cron}`, { attributes: { 'faas.trigger': 'timer', 'faas.cron': controller.cron, 'faas.time': DateTime.formatIso(DateTime.makeUnsafe(controller.scheduledTime)) } })))),
    }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { scheduled };
