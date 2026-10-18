#![feature(iterator_try_collect)]

use core::{hint::black_box, pin::pin};
use std::io;

use futures::{StreamExt as _, TryStreamExt as _, future, stream};
use nonempty_collections::NEVec;
use validated::Validated;

// --- [OPERATIONS] ----------------------------------------------------------------------

async fn fetch(id: u32) -> Result<u32, io::Error> {
    tokio::task::yield_now().await;
    if id > 9 { Err(io::Error::from(io::ErrorKind::NotFound)) } else { Ok(id) }
}

async fn paired() -> Result<u32, io::Error> {
    let (left, right) = future::join(fetch(1), fetch(2)).await;
    Ok(left?.strict_add(right?))
}

async fn tokio_paired() -> Result<u32, io::Error> {
    let (left, right) = tokio::join!(fetch(1), fetch(2));
    Ok(left?.strict_add(right?))
}

async fn one_fallible() -> Result<u32, io::Error> {
    let (left, right) = future::join(fetch(1), future::ready(2_u32)).await;
    Ok(left?.strict_add(right))
}

async fn accumulated() -> Result<u32, NEVec<io::Error>> {
    let (left, right) = future::join(fetch(1), fetch(2)).await;
    Validated::from(left).map2(right.into(), u32::strict_add).ok()
}

async fn raced() -> Result<u32, io::Error> {
    match future::select(pin!(fetch(1)), pin!(fetch(2))).await {
        future::Either::Left((first, _)) | future::Either::Right((first, _)) => first,
    }
}

async fn buffered(ids: Vec<u32>) -> Result<Vec<u32>, io::Error> { stream::iter(ids).map(fetch).buffered(4).try_collect().await }

async fn buffered_kept(ids: Vec<u32>) -> Result<Vec<u32>, NEVec<io::Error>> { stream::iter(ids).map(fetch).buffered(4).collect::<Vec<_>>().await.into_iter().collect::<Validated<_, _>>().ok() }

async fn joined(ids: Vec<u32>) -> Result<Vec<u32>, io::Error> { future::join_all(ids.into_iter().map(fetch)).await.into_iter().collect() }

async fn tried(ids: Vec<u32>) -> Result<Vec<u32>, io::Error> { future::join_all(ids.into_iter().map(fetch)).await.into_iter().try_collect() }

async fn joined_kept(ids: Vec<u32>) -> Result<Vec<u32>, NEVec<io::Error>> { future::join_all(ids.into_iter().map(fetch)).await.into_iter().collect::<Validated<_, _>>().ok() }

async fn kept(ids: Vec<u32>) -> Vec<Result<u32, io::Error>> { future::join_all(ids.into_iter().map(fetch)).await }

async fn listed(ids: Vec<u32>) -> Vec<Result<u32, io::Error>> { future::join_all(ids.into_iter().map(fetch)).await.into_iter().collect() }

async fn optional() -> Option<u32> {
    let (left, right) = future::join(future::ready(Some(1_u32)), future::ready(Some(2_u32))).await;
    Some(left?.strict_add(right?))
}

// --- [COMPOSITION] ---------------------------------------------------------------------

#[tokio::main]
async fn main() {
    black_box(&(
        paired().await,
        tokio_paired().await,
        one_fallible().await,
        accumulated().await,
        raced().await,
        buffered(vec![1]).await,
        buffered_kept(vec![1]).await,
        joined(vec![1]).await,
        tried(vec![1]).await,
        joined_kept(vec![1]).await,
        kept(vec![1]).await,
        listed(vec![1]).await,
        optional().await,
        buffered_collected(vec![1]).await,
        set_joined(tokio::task::JoinSet::new()).await,
    ));
}

async fn buffered_collected(ids: Vec<u32>) -> Result<Vec<u32>, io::Error> { stream::iter(ids).map(fetch).buffered(4).collect::<Vec<_>>().await.into_iter().collect() }

async fn set_joined(set: tokio::task::JoinSet<Result<u32, io::Error>>) -> Result<Vec<u32>, io::Error> { set.join_all().await.into_iter().collect() }
