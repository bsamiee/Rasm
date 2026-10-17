#![feature(iterator_try_collect)]

use core::hint::black_box;
use std::io;

use futures::{StreamExt as _, TryStreamExt as _, future, stream};

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

async fn buffered(ids: Vec<u32>) -> Result<Vec<u32>, io::Error> { stream::iter(ids).map(fetch).buffered(4).try_collect().await }

async fn joined(ids: Vec<u32>) -> Result<Vec<u32>, io::Error> { future::join_all(ids.into_iter().map(fetch)).await.into_iter().collect() }

async fn tried(ids: Vec<u32>) -> Result<Vec<u32>, io::Error> { future::join_all(ids.into_iter().map(fetch)).await.into_iter().try_collect() }

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
        buffered(vec![1]).await,
        joined(vec![1]).await,
        tried(vec![1]).await,
        kept(vec![1]).await,
        listed(vec![1]).await,
        optional().await,
    ));
}
