import { Array, Clock, Config, Duration, Effect, Option, Schedule, Schema, String } from 'effect';
import type { Routes } from '#/claude/routes';
import { Host } from '#/host';

// --- [MODELS] --------------------------------------------------------------------------

const Owner = Schema.fromJsonString(Schema.Struct({ front: Schema.String, typed: Schema.Finite, fullScreen: Schema.Boolean }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const holds = Effect.gen(function* () {
    const host = yield* Host;
    const [own, { front, typed, fullScreen }] = yield* Effect.all([
        host.environment('__CFBundleIdentifier'),
        Effect.flatMap(
            host.output([
                'osascript',
                '-l',
                'JavaScript',
                '-e',
                "ObjC.import('AppKit'); JSON.stringify({ front: $.NSWorkspace.sharedWorkspace.frontmostApplication.bundleIdentifier.js, typed: $.CGEventSourceSecondsSinceLastEventType($.kCGEventSourceStateHIDSystemState, $.kCGEventKeyDown), fullScreen: ($.NSApplication.sharedApplication.currentSystemPresentationOptions & $.NSApplicationPresentationFullScreen) !== 0 })",
            ]),
            Schema.decodeEffect(Owner),
        ),
    ]);
    if (Option.contains(own, front)) {
        return [];
    }
    const busy = [...(typed < Duration.toSeconds('20 seconds') ? ['typing'] : []), ...(fullScreen ? ['a full-screen app'] : [])];
    return busy.length > 0 ? busy : [(yield* host.output(['shortcuts', 'run', 'Get Focus'])).trim()].filter(String.isNonEmpty).map((focus) => `Focus ${focus}`);
});

const paused = Effect.fnUntraced(function* ({ since, first }: Routes['paused'][0]) {
    const [enabled, now] = yield* Effect.all([Effect.orElseSucceed(Config.Boolean('FUNCTION_HOOKS_QUESTION_GATE'), () => false), Clock.currentTimeMillis]);
    const standing = enabled && now - since < Duration.toMillis('15 minutes') ? yield* Effect.repeat(holds, { schedule: Schedule.spaced('5 seconds'), times: 3, until: Array.isReadonlyArrayEmpty }) : [];
    const waited = Duration.format(Duration.millis((yield* Clock.currentTimeMillis) - since));
    return { holds: standing, notes: standing.length > 0 || first.length === 0 ? [] : [`Question waited ${waited} on ${first.join(', ')}`] };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { paused };
