use core::num::NonZero;
use std::collections::BTreeMap;

use aws_lc_rs::digest;
use bytes::Bytes;
use data_encoding::{BASE64, BASE64_NOPAD, BASE64URL_NOPAD};
use fs_err::tokio as fs;
use futures::{StreamExt as _, stream::FuturesUnordered};
use indexmap::IndexMap;
use itertools::Itertools as _;
use nonempty_collections::{NEVec, NonEmptyIterator as _, iter};
use petgraph::{
    graphmap::DiGraphMap,
    visit::{DfsPostOrder, Walker as _},
};
use reqwest::{Client, Response};
use tokio::task;
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

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Fetch {
    pub limit: NonZero<usize> = NonZero::new(8).unwrap(),
    pub lock: Option<Sri> = None,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct Stack {
    pub layers: NEVec<Layer>,
    pub lock: NEVec<Import>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub async fn stack(client: &Client, fetch: &Fetch, mirrors: &BTreeMap<Url, Url>, main: Url) -> Result<Stack, NEVec<IfcError>> {
    let load = async |url: &Url| -> Result<Bytes, IfcError> {
        let refused = |reason| IfcError::Refused { url: url.clone(), reason };
        let location = mirrors
            .range(..=url)
            .rev()
            .find_map(|(prefix, mirror)| {
                let rest = url.as_str().strip_prefix(prefix.as_str()).filter(|rest| prefix.as_str().ends_with('/') || rest.is_empty() || rest.starts_with('/'))?;
                Some(if rest.is_empty() {
                    Ok(mirror.clone())
                } else {
                    let mirrored = format!("{}/{}", mirror.as_str().trim_end_matches('/'), rest.trim_start_matches('/'));
                    Url::parse(&mirrored).map_err(|source| IfcError::Url { uri: mirrored.into_boxed_str(), source })
                })
            })
            .transpose()?
            .unwrap_or_else(|| url.clone());
        match location.scheme() {
            "file" => fs::read(location.to_file_path().map_err(|()| refused(Refusal::Scheme))?).await.map(Bytes::from).map_err(|source| IfcError::Read { url: url.clone(), source }),
            "http" | "https" => {
                let unreachable = |source| IfcError::Request { url: url.clone(), source };
                client.get(location).send().await.and_then(Response::error_for_status).map_err(unreachable)?.bytes().await.map_err(unreachable)
            }
            _ => Err(refused(Refusal::Scheme)),
        }
    };
    closure(fetch, load, main, None).await
}

pub async fn closure<L, B>(fetch: &Fetch, load: L, main: Url, main_layer: Option<Layer>) -> Result<Stack, NEVec<IfcError>>
where
    L: AsyncFn(&Url) -> Result<B, IfcError>,
    B: AsRef<[u8]> + Send + 'static,
{
    const fn algorithm(sri: Sri) -> &'static digest::Algorithm {
        match sri {
            Sri::Sha256 => &digest::SHA256,
            Sri::Sha384 => &digest::SHA384,
            Sri::Sha512 => &digest::SHA512,
        }
    }
    let verified = |metadata: &str, bytes: &[u8]| {
        let tokens = metadata.split_whitespace().filter_map(|expression| {
            let (name, hash) = expression.split_once('?').map_or(expression, |(hash, _)| hash).split_once('-')?;
            Some((name.parse::<Sri>().ok()?, hash.trim_end_matches('=')))
        });
        tokens.clone().map(|(sri, _)| sri).max().is_none_or(|strongest| {
            let digest = digest::digest(algorithm(strongest), bytes);
            let mut storage = [0; digest::MAX_OUTPUT_LEN.div_ceil(3).strict_mul(4)];
            let encoded = &mut storage[..BASE64_NOPAD.encode_len(digest.as_ref().len())];
            [BASE64_NOPAD, BASE64URL_NOPAD].iter().any(|encoding| {
                encoding.encode_mut(digest.as_ref(), encoded);
                tokens.clone().filter(|(sri, _)| *sri == strongest).any(|(_, hash)| hash.as_bytes() == &*encoded)
            })
        })
    };
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
    let retrieved = async |url: Url, tokens: Vec<Box<str>>| -> Result<_, IfcError> {
        let bytes = load(&url).await?;
        let checked = tokens.len();
        task::spawn_blocking(move || {
            tokens.iter().all(|metadata| verified(metadata, bytes.as_ref())).ok_or_else(|| IfcError::Refused { url: url.clone(), reason: Refusal::Integrity })?;
            read(bytes.as_ref(), Some(&url)).map(|layer| (url, layer, Some(bytes), checked))
        })
        .await?
    };
    let (_, head, head_bytes, _) = match main_layer {
        Some(layer) => (main.clone(), layer, None, 0),
        None => retrieved(main.clone(), Vec::new()).await.map_err(NEVec::new)?,
    };
    let (head_targets, mut failures) = imported(&main, &head.imports);
    let mut declared = IndexMap::<Url, Vec<Box<str>>>::from([(main.clone(), Vec::new())]);
    let declare =
        |targets: &[(Url, Option<Box<str>>)], held: &mut IndexMap<Url, Vec<Box<str>>>| targets.iter().for_each(|(target, integrity)| held.entry(target.clone()).or_default().extend(integrity.clone()));
    declare(&head_targets, &mut declared);
    let head_imports = head_targets.into_iter().map(|(target, _)| target).collect::<Vec<_>>();
    let mut loaded = IndexMap::new();
    let mut loading = FuturesUnordered::new();
    let mut launched = 1_usize;
    loop {
        while loading.len() < fetch.limit.get()
            && let Some((url, tokens)) = declared.get_index(launched)
        {
            loading.push(retrieved(url.clone(), tokens.clone()));
            launched = launched.strict_add(1);
        }
        match loading.next().await {
            Some(Ok((url, layer, bytes, checked))) => {
                let (targets, refused) = imported(&url, &layer.imports);
                failures.extend(refused);
                declare(&targets, &mut declared);
                loaded.insert(url, (layer, bytes, checked, targets.into_iter().map(|(target, _)| target).collect::<Vec<_>>()));
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
    let (first, rest) = task::spawn_blocking(move || {
        let resolved = |url: Url, layer: Layer, bytes: Option<B>, checked: usize| {
            let mut late = declared.get(&url).into_iter().flatten().skip(checked);
            let bytes = bytes.as_ref().map(AsRef::as_ref);
            let intact = bytes.is_none_or(|bytes| late.all(|metadata| verified(metadata, bytes)));
            let integrity = bytes.zip(lock).map(|(bytes, sri)| format!("{sri}-{}", BASE64.encode_display(digest::digest(algorithm(sri), bytes).as_ref())).into_boxed_str());
            if intact { Ok((Import { uri: url.as_str().into(), integrity }, layer)) } else { Err(IfcError::Refused { url, reason: Refusal::Integrity }) }
        };
        let rest = order.into_iter().rev().filter_map(|url| loaded.swap_remove(&url).map(|(layer, bytes, checked, _)| resolved(url, layer, bytes, checked))).collect::<Vec<_>>();
        (resolved(main, head, head_bytes, 0), rest)
    })
    .await
    .map_err(|source| NEVec::new(IfcError::from(source)))?;
    let (rest, mismatched): (Vec<_>, Vec<_>) = rest.into_iter().partition_result();
    failures.extend(mismatched);
    match first {
        Ok((import, layer)) => NEVec::try_from_vec(failures).map_or_else(
            || {
                let (imports, layers): (Vec<_>, Vec<_>) = rest.into_iter().unzip();
                Ok(Stack { layers: iter::once(layer).chain(layers).collect(), lock: iter::once(import).chain(imports).collect() })
            },
            Err,
        ),
        Err(failure) => Err(iter::once(failure).chain(failures).collect()),
    }
}
