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

async fn buffered_collected(ids: Vec<u32>) -> Result<Vec<u32>, io::Error> { stream::iter(ids).map(fetch).buffered(4).collect::<Vec<_>>().await.into_iter().collect() }

async fn set_joined(set: tokio::task::JoinSet<Result<u32, io::Error>>) -> Result<Vec<u32>, io::Error> { set.join_all().await.into_iter().collect() }

async fn separate_callbacks() -> (Result<u32, u8>, Result<u32, u8>) {
    let (left, right) = future::join(future::ready(Ok::<_, u8>(1_u32)), future::ready(Ok::<_, u8>(2_u32))).await;
    ((|| Ok(left?))(), (|| Ok(right?))())
}

async fn repeated_result() -> Result<u32, u8> {
    let (left, right) = future::join(future::ready(Ok::<_, u8>(1_u32)), future::ready(Ok::<_, u8>(2_u32))).await;
    black_box(right);
    Ok(left?.strict_add(left?))
}

async fn exclusive(flag: bool) -> Result<u32, u8> {
    let (left, right) = future::join(future::ready(Ok::<_, u8>(1_u32)), future::ready(Ok::<_, u8>(2_u32))).await;
    Ok(if flag { left? } else { right? })
}

async fn nested_exclusive(outer: bool, inner: bool) -> Result<u32, u8> {
    let (left, right) = future::join(future::ready(Ok::<_, u8>(1_u32)), future::ready(Ok::<_, u8>(2_u32))).await;
    Ok(if outer { if inner { left? } else { right? } } else { 0 })
}

async fn complementary(flag: bool) -> Result<(), u8> {
    let (left, right) = future::join(future::ready(Ok::<_, u8>(1_u32)), future::ready(Ok::<_, u8>(2_u32))).await;
    if flag {
        black_box(left?);
    }
    if !flag {
        black_box(right?);
    }
    Ok(())
}

async fn grouped() -> Result<u32, u8> {
    let (first, second, handled) = future::join3(future::ready(Ok::<_, u8>(1_u32)), future::ready(Ok::<_, u8>(2_u32)), future::ready(Ok::<_, u8>(3_u32))).await;
    black_box(&handled);
    Ok(first?.strict_add(second?))
}

async fn infinite_buffered() -> Result<Vec<u32>, u8> { stream::repeat_with(|| future::ready(Err::<u32, u8>(1))).buffered(4).try_collect().await }

async fn cycling() -> Result<Vec<u32>, u8> { future::join_all([future::ready(Err::<u32, u8>(1))]).await.into_iter().cycle().collect() }

async fn optional_collect() -> Option<Vec<u32>> { future::join_all([future::ready(Some(1_u32))]).await.into_iter().try_collect() }

async fn dependent_continuation() -> Result<u32, u8> {
    let (left, right) = future::join(future::ready(Ok::<_, u8>(0_u32)), future::ready(Err::<u32, u8>(1))).await;
    if left? == 0 {
        return Ok(0);
    }
    Ok(right?)
}

async fn selected_operand() -> Result<bool, u8> {
    let (left, right) = future::join(future::ready(Ok::<_, u8>(0_u32)), future::ready(Err::<u32, u8>(1))).await;
    Ok(left? > 0 && right? > 0)
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
