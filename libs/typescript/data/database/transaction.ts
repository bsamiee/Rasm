import { D1Client } from '@effect/sql-d1';
import { type Array, Effect, Option } from 'effect';
import { SqlClient, type Statement } from 'effect/sql';

// --- [OPERATIONS] ----------------------------------------------------------------------

const atomic = <A extends object>(statements: Array.NonEmptyReadonlyArray<Statement.Statement<A>>) =>
    Effect.flatMap(
        Effect.serviceOption(D1Client.D1Client),
        Option.match({
            onSome: (client) => client.batch(statements),
            onNone: () => SqlClient.SqlClient.use((sql) => sql.withTransaction(Effect.all(statements))),
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { atomic };
