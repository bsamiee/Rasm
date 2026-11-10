use core::num::NonZero;
#[cfg(feature = "fetch")]
use std::collections::BTreeMap;

#[cfg(feature = "fetch")]
use bytes::Bytes;
use data_encoding::{BASE64, BASE64_NOPAD, BASE64URL_NOPAD};
use futures::StreamExt as _;
use futures::stream::FuturesUnordered;
use indexmap::IndexMap;
use itertools::Itertools as _;
use nonempty_collections::{NEVec, NonEmptyIterator as _, iter};
use petgraph::graphmap::DiGraphMap;
use petgraph::visit::{DfsPostOrder, Walker as _};
#[cfg(feature = "fetch")]
use reqwest::{Client, Response};
use sha2::{Digest as _, Sha256, Sha384, Sha512};
use url::Url;

use crate::{IfcError, Import, Layer, Refusal, read};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, strum::EnumString, strum::Display)]
#[strum(serialize_all = "lowercase")]
pub enum Sri {
    Sha256,
    Sha384,
    Sha512,
}

impl Sri {
    fn digest(self, bytes: &[u8]) -> Vec<u8> {
        match self {
            Self::Sha256 => Sha256::digest(bytes).to_vec(),
            Self::Sha384 => Sha384::digest(bytes).to_vec(),
            Self::Sha512 => Sha512::digest(bytes).to_vec(),
        }
    }
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Fetch {
    pub limit: NonZero<usize> = NonZero::new(8).unwrap(),
    pub lock: Option<Sri> = None,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[cfg(feature = "fetch")]
pub async fn stack(client: &Client, fetch: &Fetch, mirrors: &BTreeMap<Url, Url>, main: Url) -> Result<NEVec<(Import, Layer)>, NEVec<IfcError>> {
    let load = async |url: &Url| -> Result<Bytes, IfcError> {
        let location = mirrors
            .range(..=url)
            .rev()
            .find_map(|(prefix, mirror)| {
                let relative = prefix.make_relative(url).filter(|relative| !relative.starts_with("../") && (relative.is_empty() || prefix.path().ends_with('/')))?;
                Some(mirror.join(&relative).map_err(|source| IfcError::Url { uri: relative.into_boxed_str(), source }))
            })
            .transpose()?
            .unwrap_or_else(|| url.clone());
        match location.scheme() {
            "file" => {
                let path = location.to_file_path().map_err(|()| IfcError::Refused { url: url.clone(), reason: Refusal::Scheme })?;
                blocking::unblock(move || fs_err::read(path)).await.map(Bytes::from).map_err(|source| IfcError::Read { url: url.clone(), source })
            }
            "http" | "https" => {
                let unreachable = |source| IfcError::Request { url: url.clone(), source };
                client.get(location).send().await.and_then(Response::error_for_status).map_err(unreachable)?.bytes().await.map_err(unreachable)
            }
            _ => Err(IfcError::Refused { url: url.clone(), reason: Refusal::Scheme }),
        }
    };
    closure(fetch, load, main, None).await
}

pub async fn closure<L, B>(fetch: &Fetch, load: L, main: Url, main_layer: Option<Layer>) -> Result<NEVec<(Import, Layer)>, NEVec<IfcError>>
where
    L: AsyncFn(&Url) -> Result<B, IfcError>,
    B: AsRef<[u8]> + Send + 'static,
{
    let imported = |importer: &Url, imports: &[Import]| -> (Vec<_>, Vec<IfcError>) {
        imports
            .iter()
            .map(|Import { uri, integrity }| match importer.join(uri) {
                Ok(target) if importer.origin().is_tuple() && !target.origin().is_tuple() => Err(IfcError::Refused { url: target, reason: Refusal::Scheme }),
                Ok(target) => Ok((target, integrity.clone())),
                Err(source) => Err(IfcError::Url { uri: uri.clone(), source }),
            })
            .partition_result::<Vec<(Url, Option<Box<str>>)>, _, _, _>()
    };
    let declare =
        |targets: &[(Url, Option<Box<str>>)], held: &mut IndexMap<Url, Vec<Box<str>>>| targets.iter().for_each(|(target, integrity)| held.entry(target.clone()).or_default().extend(integrity.clone()));
    let retrieved = async |url: Url| -> Result<(Url, Layer, B), IfcError> {
        let bytes = load(&url).await?;
        blocking::unblock(move || read(bytes.as_ref(), Some(&url)).map(|layer| (url, layer, bytes))).await
    };
    let (head, head_bytes) = match main_layer {
        Some(layer) => (layer, None),
        None => retrieved(main.clone()).await.map(|(_, layer, bytes)| (layer, Some(bytes))).map_err(NEVec::new)?,
    };
    let (head_targets, mut failures) = imported(&main, &head.imports);
    let mut declared = IndexMap::<Url, Vec<Box<str>>>::from([(main.clone(), Vec::new())]);
    declare(&head_targets, &mut declared);
    let head_imports = head_targets.into_iter().map(|(target, _)| target).collect::<Vec<_>>();
    let mut loaded = IndexMap::new();
    let mut loading = FuturesUnordered::new();
    let mut launched = 1_usize;
    loop {
        while loading.len() < fetch.limit.get()
            && let Some((url, _)) = declared.get_index(launched)
        {
            loading.push(retrieved(url.clone()));
            launched = launched.strict_add(1);
        }
        match loading.next().await {
            Some(Ok((url, layer, bytes))) => {
                let (targets, refused) = imported(&url, &layer.imports);
                failures.extend(refused);
                declare(&targets, &mut declared);
                loaded.insert(url, (layer, Some(bytes), targets.into_iter().map(|(target, _)| target).collect::<Vec<_>>()));
            }
            Some(Err(failure)) => failures.push(failure),
            None => break,
        }
    }
    let order = {
        let edges = head_imports.iter().rev().map(|import| (&main, import));
        let graph = edges.chain(loaded.iter().flat_map(|(importer, (.., imports))| imports.iter().rev().map(move |import| (importer, import)))).collect::<DiGraphMap<_, ()>>();
        DfsPostOrder::new(&graph, &main).iter(&graph).cloned().collect::<Vec<_>>()
    };
    let lock = fetch.lock;
    let verified = |metadata: &str, bytes: &[u8]| {
        let tokens = metadata.split_whitespace().filter_map(|expression| {
            let (name, hash) = expression.split_once('?').map_or(expression, |(hash, _)| hash).split_once('-')?;
            Some((name.parse::<Sri>().ok()?, hash.trim_end_matches('=')))
        });
        tokens.clone().map(|(sri, _)| sri).max().is_none_or(|strongest| {
            let digest = strongest.digest(bytes);
            [BASE64_NOPAD, BASE64URL_NOPAD].iter().any(|encoding| {
                let encoded = encoding.encode(&digest);
                tokens.clone().filter(|(sri, _)| *sri == strongest).any(|(_, hash)| hash == encoded)
            })
        })
    };
    let (first, rest) = blocking::unblock(move || {
        let resolved = |url: Url, layer: Layer, bytes: Option<B>| {
            let bytes = bytes.as_ref().map(AsRef::as_ref);
            let intact = bytes.is_none_or(|bytes| declared.get(&url).into_iter().flatten().all(|metadata| verified(metadata, bytes)));
            let integrity = bytes.zip(lock).map(|(bytes, sri)| format!("{sri}-{}", BASE64.encode(&sri.digest(bytes))).into_boxed_str());
            if intact { Ok((Import { uri: url.as_str().into(), integrity }, layer)) } else { Err(IfcError::Refused { url, reason: Refusal::Integrity }) }
        };
        let rest = order.into_iter().rev().filter_map(|url| loaded.swap_remove(&url).map(|(layer, bytes, _)| resolved(url, layer, bytes))).collect::<Vec<_>>();
        (resolved(main, head, head_bytes), rest)
    })
    .await;
    let (rest, mismatched): (Vec<_>, Vec<_>) = rest.into_iter().partition_result();
    failures.extend(mismatched);
    match first {
        Ok(first) => NEVec::try_from_vec(failures).map_or_else(|| Ok(iter::once(first).chain(rest).collect()), Err),
        Err(failure) => Err(iter::once(failure).chain(failures).collect()),
    }
}
