use core::{cmp::Reverse, num::NonZero};
use std::collections::BTreeMap;

use aws_lc_rs::digest;
use data_encoding::BASE64;
use fs_err::tokio;
use futures::{StreamExt as _, stream};
use indexmap::IndexSet;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use petgraph::{
    graphmap::DiGraphMap,
    visit::{DfsPostOrder, Walker as _},
};
use reqwest::Client;
use url::Url;
use validated::Validated;

use crate::{IfcError, Layer, Refusal};

// --- [TYPES] ---------------------------------------------------------------------------

type Loaded = (Url, Layer, Vec<(Url, Option<Box<str>>)>);

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Fetch {
    pub limit: NonZero<usize> = NonZero::new(8).unwrap(),
    pub mirrors: BTreeMap<Url, Url> = BTreeMap::new(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn stack(client: &Client, fetch: &Fetch, main: Url) -> Result<NEVec<Layer>, NEVec<IfcError>> {
    let mut loaded = closure(client, fetch, NEVec::new(load(client, fetch, main, []).await.ok()?)).await?;
    let strength = {
        let imports = loaded.iter().flat_map(|(importer, _, imports)| imports.iter().rev().map(move |(import, _)| (importer, import))).collect::<DiGraphMap<_, ()>>();
        DfsPostOrder::new(&imports, &loaded.first().0).iter(&imports).cloned().collect::<IndexSet<_>>()
    };
    loaded.sort_by_key(|(url, ..)| Reverse(strength.get_index_of(url)));
    Ok(loaded.into_nonempty_iter().map(|(_, layer, _)| layer).collect())
}

async fn closure(client: &Client, fetch: &Fetch, mut loaded: NEVec<Loaded>) -> Result<NEVec<Loaded>, NEVec<IfcError>> {
    let declared = || loaded.iter().flat_map(|(_, _, imports)| imports);
    let level = declared().map(|(url, _)| url).filter(|url| loaded.iter().all(|(known, ..)| known != *url)).cloned().collect::<IndexSet<_>>();
    if level.is_empty() {
        Ok(loaded)
    } else {
        let fetched = stream::iter(level)
            .map(|url| {
                let integrity = declared().filter(|(import, _)| *import == url).filter_map(|(_, integrity)| integrity.as_deref()).collect::<Vec<_>>();
                load(client, fetch, url, integrity)
            })
            .buffer_unordered(fetch.limit.get())
            .collect::<Vec<_>>()
            .await
            .into_iter()
            .collect::<Validated<Vec<_>, _>>()
            .ok()?;
        loaded.extend(fetched);
        Box::pin(closure(client, fetch, loaded)).await
    }
}

async fn load(client: &Client, fetch: &Fetch, url: Url, integrity: impl IntoIterator<Item = &str>) -> Validated<Loaded, IfcError> {
    let refused = |reason| IfcError::Refused { url: url.clone(), reason };
    let import = |source| IfcError::Import { url: url.clone(), source };
    let decoded = try bikeshed Result<Layer, IfcError> {
        let location = fetch
            .mirrors
            .iter()
            .rev()
            .find_map(|(prefix, mirror)| url.as_str().strip_prefix(prefix.as_str()).map(|rest| mirror.join(rest).map_err(|source| IfcError::Url { uri: rest.into(), source })))
            .transpose()?
            .unwrap_or_else(|| url.clone());
        let bytes = match location.scheme() {
            "file" => tokio::read(location.to_file_path().map_err(|()| refused(Refusal::Scheme))?).await?,
            "http" | "https" => client.get(location).send().await?.error_for_status()?.bytes().await?.into(),
            _ => Err(refused(Refusal::Scheme))?,
        };
        integrity.into_iter().all(|value| verified(value, &bytes)).ok_or_else(|| refused(Refusal::Integrity))?;
        let mut json = serde_json::Deserializer::from_slice(&bytes);
        let layer = serde_path_to_error::deserialize(&mut json).map_err(import)?;
        json.end().map_err(|source| import(serde_path_to_error::Error::new(serde_path_to_error::Track::new().path(), source)))?;
        layer
    };
    decoded.map_or_else(Validated::fail, |layer| {
        layer
            .imports
            .iter()
            .map(|declared| url.join(&declared.uri).map(|target| (target, declared.integrity.clone())).map_err(|source| IfcError::Url { uri: declared.uri.clone(), source }))
            .collect::<Validated<Vec<_>, _>>()
            .map(|imports| (url, layer, imports))
    })
}

fn verified(integrity: &str, bytes: &[u8]) -> bool {
    let hashes = integrity.split_whitespace().filter_map(|expression| match expression.split_once('?').map_or(expression, |(hash, _)| hash).split_once('-')? {
        ("sha256", hash) => Some((&digest::SHA256, hash)),
        ("sha384", hash) => Some((&digest::SHA384, hash)),
        ("sha512", hash) => Some((&digest::SHA512, hash)),
        _ => None,
    });
    hashes.clone().map(|(algorithm, _)| algorithm).max_by_key(|algorithm| algorithm.output_len()).is_some_and(|strongest| {
        let computed = digest::digest(strongest, bytes);
        hashes.filter(|(algorithm, _)| *algorithm == strongest).any(|(_, hash)| BASE64.decode(hash.as_bytes()).is_ok_and(|expected| expected == computed.as_ref()))
    })
}
