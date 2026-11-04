#!/usr/bin/env node
import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { serve } from '@rasm/mcp/node';
import { Duration, Effect, Layer } from 'effect';
import plugin from '#/.codex-plugin/plugin.json' with { type: 'json' };
import servers from '#/.lsp.json' with { type: 'json' };
import { registerLanguageServerTools } from '#/tools';

// --- [COMPOSITION] ---------------------------------------------------------------------

serve(plugin, { retention: Duration.hours(1), instructions: [] }, Layer.effectDiscard(registerLanguageServerTools({ servers, idle: '30 minutes' }))).pipe(Effect.provide(layer), runMain({ teardown: serve.teardown }));
