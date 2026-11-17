import { Content } from '@rasm/mcp';
import { Failure, ObjectModel, Open, Rasterize } from '@rasm/object-model';
import { Schema } from 'effect';
import { Tool, Toolkit } from 'effect/ai';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ATELIER = new URL('ws://localhost:7490');

// --- [COMPOSITION] ---------------------------------------------------------------------

const Photoshop = Toolkit.make(
    Tool.make('open', { description: 'Opens document without dialogs and returns its reference', parameters: Open, success: Open.Result, failure: Failure }),
    Tool.make('rasterize', { description: 'Rasterizes document, layer, or artboard as PNG, a layer without the rest of its document', parameters: Rasterize, success: Schema.Struct({ image: Content.image('image/png'), width: Schema.Int, height: Schema.Int }), failure: Failure }).annotate(Tool.Readonly, true),
    Tool.make('object_model', { description: 'Lists class and enumeration names, or describes one declaration with its members', parameters: ObjectModel, success: ObjectModel.Result, failure: Failure }).annotate(Tool.Readonly, true),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ATELIER, Photoshop };
