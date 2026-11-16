import { bridge, servePanel } from '@rasm/cep/panel';
import { Array, Schema, Struct } from 'effect';
import { Peer } from '#/peer';
import { Requests } from '#/requests';

// --- [COMPOSITION] ---------------------------------------------------------------------

servePanel({
    link: (cep) =>
        Peer.dial(
            Requests,
            Requests.toLayer(Struct.Record(Array.map(Array.fromIterable(Requests.requests.values()), Struct.get('_tag')), bridge(cep))),
            Schema.decodeEffect(Schema.URLFromString)(cep.getSystemPath('hostApplication')),
        )(cep.getExtensionId(), Schema.decodeEffect(Schema.URLFromString)(cep.getSystemPath('extension'))),
});
