import { Extension, Grant, Provider, type ProviderArgs, Role, type RoleArgs, Schema } from '@pulumi/postgresql';
import { type CustomResourceOptions, type Input, interpolate, type Output } from '@pulumi/pulumi';
import { RandomPassword } from '@pulumi/random';

// --- [OPERATIONS] ----------------------------------------------------------------------

const postgresRoles = (database: string, variable: string, server: ProviderArgs, endpoint: Input<string>, protect: boolean, options: CustomResourceOptions): Readonly<Record<string, Output<string>>> => {
    const provider = new Provider(database, { ...server, database: 'postgres', username: 'postgres', sslmode: 'disable' }, options);
    const login = (role: string, settings: RoleArgs) => {
        const key = new RandomPassword(`${database}-${role}`, { length: 40, special: false }).result;
        return { name: new Role(`${database}-${role}`, { name: `${database}-${role}`, login: true, password: key, ...settings }, { provider }).name, key };
    };
    const migrator = login('migrator', { idleInTransactionSessionTimeout: 60_000 });
    const schema = new Schema(database, { name: migrator.name, owner: migrator.name, database: 'postgres' }, { provider, protect });
    const runtime = login('runtime', { statementTimeout: 15_000, idleInTransactionSessionTimeout: 30_000, searchPaths: [schema.name] });
    const statistics = new Extension(`${database}-pg-stat-statements`, { name: 'pg_stat_statements', database: 'postgres' }, { provider });
    const url = (user: Input<string>, key: Input<string>) => interpolate`postgres://${user}:${key}@${endpoint}/postgres`;
    return {
        [variable]: url(new Grant(`${database}-runtime`, { database: 'postgres', role: runtime.name, objectType: 'schema', schema: schema.name, privileges: ['USAGE'] }, { provider }).role, runtime.key),
        [`${variable}_MIGRATOR`]: url(new Grant(`${database}-migrator`, { database: 'postgres', role: schema.owner, objectType: 'database', privileges: ['CREATE'] }, { provider, dependsOn: statistics }).role, migrator.key),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { postgresRoles };
