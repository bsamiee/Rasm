import { Completion, resource, tool } from '@rasm/mcp/link';
import { Image, Instructions, READING } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search';
import { Schema, Struct } from 'effect';
import { Tool } from 'effect/ai';
import { Rpc, RpcGroup } from 'effect/rpc';
import { InDesignError, Operations, Phase } from '../models.ts';

// --- [MODELS] --------------------------------------------------------------------------

const PATH = '/indesign';
const Peer = Schema.Struct({ application: Schema.URLFromString });
const Failure = Schema.Union([InDesignError, Unmatched]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Font = Rpc.make('font', { ...Operations.font, error: Failure });
const Enter = Rpc.make('enter', { payload: {}, success: Phase });
const Complete = Rpc.make('complete', { payload: { operation: Schema.String, parameter: Schema.String }, success: Schema.Array(Schema.String), error: Failure });
const InDesign = RpcGroup.make(
    Rpc.make('inspect', { ...Operations.inspect, error: Failure })
        .annotateMerge(READING)
        .pipe(tool),
    Rpc.make('execute', { ...Operations.execute, error: Failure }).pipe(tool),
    Rpc.make('render', { ...Operations.render, error: Failure })
        .setSuccess(Operations.render.success.mapFields(Struct.assign({ image: Image('image/png') })))
        .annotateMerge(READING)
        .pipe(tool),
    Rpc.make('find_change', { ...Operations.find_change, error: Failure })
        .annotate(Tool.OpenWorld, false)
        .pipe(tool),
    Font,
    Rpc.make('menu', { ...Operations.menu, error: Failure }).pipe(resource('indesign://menu')),
    Rpc.make('preflight', { ...Operations.preflight, error: Failure }).pipe(resource('indesign://preflight')),
    Rpc.make('queries', { ...Operations.queries, error: Failure }).pipe(resource('indesign://queries')),
    Enter,
    Complete,
)
    .annotate(Completion, (rpc, parameter, call) => call(Complete, { operation: rpc._tag, parameter }))
    .annotate(Instructions, ['InDesign is served through its panel, which links when InDesign starts. Measurements are numbers in points. Resource templates select an application; base resource URIs use the single linked application.']);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Complete, Enter, Font, InDesign, PATH, Peer };
