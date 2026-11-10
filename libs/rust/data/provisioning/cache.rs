use core::future::ready;
use std::collections::{BTreeMap, BTreeSet};

use redis::acl::Rule;
use redis::aio::{ConnectionManager, ConnectionManagerConfig};
use redis::{AsyncCommands as _, Client, FieldExistenceCheck, HashFieldExpirationOptions, RedisError, Script, Value};
use secrecy::ExposeSecret as _;

use crate::stores::{Create, Login, Server, decoded, encoded, named};
use crate::{Kind, Label, Operation, ProvisioningError, Refusal, Removal, StoreId};

// --- [CONSTANTS] -----------------------------------------------------------------------

const LABELS: &str = concat!(env!("CARGO_PKG_NAME"), ":labels");
const NUMBERS: &str = concat!(env!("CARGO_PKG_NAME"), ":numbers");

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Numbered {
    pub login: Login,
    pub database: u32,
}

// --- [SERVICES] ------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Cache {
    admin: ConnectionManager,
    home: i64,
    databases: u32,
    claim: Script,
}

impl Cache {
    pub async fn connect(client: &Client, config: ConnectionManagerConfig) -> Result<Self, RedisError> {
        const CLAIM: &str = "local taken = {} \
                             for _, database in ipairs(redis.call('HVALS', KEYS[1])) do taken[tonumber(database)] = true end \
                             for database = 0, tonumber(ARGV[1]) - 1 do \
                             if database ~= tonumber(ARGV[2]) and not taken[database] then \
                             redis.call('HSET', KEYS[1], ARGV[3], database) \
                             redis.call('HSET', KEYS[2], ARGV[3], ARGV[4]) \
                             redis.call('SELECT', database) \
                             redis.call('FLUSHDB') \
                             return database \
                             end \
                             end \
                             return false";
        let mut admin = client.get_connection_manager_with_config(config).await?;
        let stated: BTreeMap<String, u32> = redis::cmd("CONFIG").arg("GET").arg("databases").query_async(&mut admin).await?;
        Ok(Self { admin, home: client.get_connection_info().redis_settings().db(), databases: stated.into_values().sum(), claim: Script::new(CLAIM) })
    }
}

impl Server for Cache {
    fn kind(&self) -> Kind { Kind::Cache }

    fn limit(&self) -> impl Future<Output = Result<Option<u32>, ProvisioningError>> + Send { ready(Ok(None)) }

    async fn stores(&self) -> Result<Vec<(StoreId, Option<Label>)>, ProvisioningError> {
        let listed: Result<(BTreeMap<String, String>, Vec<String>, Vec<String>), RedisError> = redis::pipe().hgetall(LABELS).hkeys(NUMBERS).acl_users().query_async(&mut self.admin.clone()).await;
        let (labels, numbered, users) = listed.map_err(|source| ProvisioningError::Cache { operation: Operation::List, source })?;
        let ids = BTreeSet::from_iter(labels.keys().chain(&numbered).chain(&users).filter_map(|name| named(name)));
        Ok(ids.into_iter().map(|id| (id, labels.get(&id.to_string()).and_then(|label| decoded(id, label.as_bytes())))).collect())
    }

    async fn label(&self, id: StoreId, label: &Label) -> Result<(), ProvisioningError> {
        let held = HashFieldExpirationOptions::default().set_existence_check(FieldExistenceCheck::FXX);
        let labeled: Result<bool, RedisError> = self.admin.clone().hset_ex(LABELS, &held, &[(id.to_string(), encoded(id, label)?)]).await;
        labeled.map_err(|source| ProvisioningError::Cache { operation: Operation::Label(id), source })?.ok_or(ProvisioningError::Store { id, refusal: Refusal::Missing })
    }

    async fn remove(&self, id: StoreId, removal: Removal) -> Result<(), ProvisioningError> {
        const SELECT: &str = "SELECT";
        let (mut admin, name, refused) = (self.admin.clone(), id.to_string(), |source| ProvisioningError::Cache { operation: Operation::Remove(id), source });
        let (user, number): (Value, Option<u32>) = redis::pipe().cmd("ACL").arg("GETUSER").arg(&name).hget(NUMBERS, &name).query_async(&mut admin).await.map_err(refused)?;
        let open = match (removal, user) {
            (Removal::Forced, _) | (Removal::Idle, Value::Nil) => String::new(),
            (Removal::Idle, _) => redis::cmd("CLIENT").arg("LIST").arg("USER").arg(&name).query_async(&mut admin).await.map_err(refused)?,
        };
        open.is_empty().ok_or(ProvisioningError::Store { id, refusal: Refusal::InUse })?;
        let mut removed = redis::pipe();
        let flushed = number
            .into_iter()
            .fold(removed.atomic().acl_deluser(&[&name]).ignore(), |removing, database| removing.cmd(SELECT).arg(database).ignore().flushdb().ignore().cmd(SELECT).arg(self.home).ignore());
        flushed.hdel(NUMBERS, &name).ignore().hdel(LABELS, &name).ignore().query_async(&mut admin).await.map_err(refused)
    }
}

impl Create for Cache {
    type Access = Numbered;

    async fn create(&self, id: StoreId, label: &Label) -> Result<Numbered, ProvisioningError> {
        let (mut admin, name, granted, refused) = (self.admin.clone(), id.to_string(), Login::new(id), |source| ProvisioningError::Cache { operation: Operation::Create(id), source });
        let claimed: Option<u32> = self.claim.key(NUMBERS).key(LABELS).arg(self.databases).arg(self.home).arg(&name).arg(encoded(id, label)?).invoke_async(&mut admin).await.map_err(refused)?;
        let database = claimed.ok_or(ProvisioningError::Exhausted { kind: Kind::Cache })?;
        let rules = [
            Rule::On,
            Rule::AddPass(granted.password.expose_secret().to_owned()),
            Rule::Other(format!("db={database}")),
            Rule::AllKeys,
            Rule::Channel(format!("{name}*")),
            Rule::AllCommands,
            Rule::RemoveCategory("admin".to_owned()),
            Rule::RemoveCommand("flushall".to_owned()),
        ];
        admin.acl_setuser_rules(&name, &rules).await.map(|()| Numbered { login: granted, database }).map_err(refused)
    }
}
