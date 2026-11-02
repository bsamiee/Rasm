use core::{cmp::Reverse, iter, num::NonZero};
use std::collections::{BTreeMap, HashMap};

use aws_lc_rs::digest::{self, Digest};
use data_encoding::{BASE64_NOPAD, BASE64URL_NOPAD};
use fs_err::tokio;
use futures::{StreamExt as _, stream::FuturesUnordered};
use indexmap::IndexSet;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use petgraph::{
    graphmap::DiGraphMap,
    visit::{DfsPostOrder, Walker as _},
};
use reqwest::Client;
use url::Url;
use validated::Validated;

use crate::{IfcError, Import, Layer, Refusal};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Fetch {
    pub limit: NonZero<usize> = NonZero::<usize>::saturating_add(NonZero::<usize>::MIN, 7),
    pub mirrors: BTreeMap<Url, Url> = BTreeMap::new(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn stack(client: &Client, fetch: &Fetch, main: Url) -> Result<NEVec<Layer>, NEVec<IfcError>> {
    const ALGORITHMS: [(&str, &digest::Algorithm); 3] = [("sha256", &digest::SHA256), ("sha384", &digest::SHA384), ("sha512", &digest::SHA512)];
    let verified = |integrity: &str, digests: &[Digest]| {
        let hashes = integrity.split_whitespace().filter_map(|expression| {
            let (algorithm, hash) = expression.split_once('?').map_or(expression, |(hash, _)| hash).split_once('-')?;
            iter::zip(&ALGORITHMS, digests).find_map(|((name, _), digest)| (*name == algorithm).then_some((digest, hash.trim_end_matches('='))))
        });
        hashes.clone().map(|(digest, _)| digest.algorithm().output_len()).max().is_none_or(|strongest| {
            hashes
                .filter(|(digest, _)| digest.algorithm().output_len() == strongest)
                .any(|(digest, hash)| [BASE64_NOPAD, BASE64URL_NOPAD].iter().any(|encoding| encoding.encode(digest.as_ref()) == hash))
        })
    };
    let load = async |url: Url, integrity: Vec<Box<str>>| {
        let refused = |reason| IfcError::Refused { url: url.clone(), reason };
        let import = |source| IfcError::Import { url: url.clone(), source };
        try bikeshed Result<(Layer, _), IfcError> {
            let location = fetch
                .mirrors
                .range(..=&url)
                .rev()
                .find_map(|(prefix, mirror)| url.as_str().strip_prefix(prefix.as_str()).map(|rest| mirror.join(rest).map_err(|source| IfcError::Url { uri: rest.into(), source })))
                .transpose()?
                .unwrap_or_else(|| url.clone());
            let bytes: Vec<u8> = match location.scheme() {
                "file" => tokio::read(location.to_file_path().map_err(|()| refused(Refusal::Scheme))?).await?,
                "http" | "https" => client.get(location).send().await?.error_for_status()?.bytes().await?.into(),
                _ => Err(refused(Refusal::Scheme))?,
            };
            let digests = ALGORITHMS.map(|(_, algorithm)| digest::digest(algorithm, &bytes));
            integrity.iter().all(|metadata| verified(metadata, &digests)).ok_or_else(|| refused(Refusal::Integrity))?;
            let mut json = serde_json::Deserializer::from_slice(&bytes);
            let layer = serde_path_to_error::deserialize(&mut json).map_err(import)?;
            json.end().map_err(|source| import(serde_path_to_error::Error::new(serde_path_to_error::Track::new().path(), source)))?;
            (layer, digests)
        }
        .map_err(NEVec::new)
        .and_then(|(layer, digests)| {
            layer
                .imports
                .iter()
                .map(|Import { uri, integrity: declared }| match url.join(uri) {
                    Ok(target) if url.origin().is_tuple() && !target.origin().is_tuple() => Err(IfcError::Refused { url: target, reason: Refusal::Scheme }),
                    Ok(target) => Ok((target, declared.clone())),
                    Err(source) => Err(IfcError::Url { uri: uri.clone(), source }),
                })
                .collect::<Validated<Vec<_>, _>>()
                .ok()
                .map(|imports| (url, layer, digests, imports))
        })
    };
    let mut loaded = NEVec::new(load(main, Vec::new()).await?);
    let mut dispatched = IndexSet::from([loaded.first().0.clone()]);
    let (mut loading, mut admitted, mut launched, mut failures) = (FuturesUnordered::new(), 0, 1, Vec::new());
    loop {
        dispatched.extend(loaded.iter().skip(admitted).flat_map(|(.., imports)| imports).map(|(url, _)| url.clone()));
        admitted = loaded.len().get();
        let room = fetch.limit.get().saturating_sub(loading.len());
        loading.extend(
            dispatched
                .iter()
                .skip(launched)
                .take(room)
                .map(|url| load(url.clone(), loaded.iter().flat_map(|(.., imports)| imports).filter(|(import, _)| import == url).filter_map(|(_, integrity)| integrity.clone()).collect())),
        );
        launched = dispatched.len().min(launched.saturating_add(room));
        match loading.next().await {
            Some(Ok(record)) => loaded.push(record),
            Some(Err(errors)) => failures.extend(errors),
            None => break,
        }
    }
    let known = loaded.iter().map(|(url, _, digests, _)| (url, digests)).collect::<HashMap<_, _>>();
    let tampered = loaded
        .iter()
        .flat_map(|(.., imports)| imports)
        .filter(|(url, integrity)| integrity.as_deref().zip(known.get(url)).is_some_and(|(integrity, digests)| !verified(integrity, *digests)))
        .map(|(url, _)| url)
        .collect::<IndexSet<_>>();
    failures.extend(tampered.into_iter().map(|url| IfcError::Refused { url: url.clone(), reason: Refusal::Integrity }));
    let strength = {
        let graph = loaded.iter().flat_map(|(importer, .., imports)| imports.iter().rev().map(move |(import, _)| (importer, import))).collect::<DiGraphMap<_, ()>>();
        DfsPostOrder::new(&graph, &loaded.first().0).iter(&graph).cloned().collect::<IndexSet<_>>()
    };
    loaded.sort_by_key(|(url, ..)| Reverse(strength.get_index_of(url)));
    NEVec::try_from_vec(failures).map_or_else(|| Ok(loaded.into_nonempty_iter().map(|(_, layer, ..)| layer).collect()), Err)
}
