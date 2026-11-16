use core::time::Duration;

use claims::{assert_err, assert_ok, assert_some};
use rasm_mcp_tests::{Recorder, Seen, WATCHED, everything, paired};
use rmcp::model::{GetMeta as _, ProtocolVersion, ServerNotification, SubscribeRequestParams, SubscriptionFilter, UnsubscribeRequestParams};
use rmcp::service::ClientLifecycleMode;
use tokio::time;

use crate::seen;

// --- [RESOURCES]

#[tokio::test]
async fn resource_updates_reach_subscribers_of_that_uri_alone() {
    let (shared, app) = assert_ok!(everything().await);
    let (recorder, mut events) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Initialize).await);
    let (other, mut unrelated) = Recorder::new(ProtocolVersion::V_2025_11_25);
    let (bystander, _other) = assert_ok!(paired(shared.connection(), other, ClientLifecycleMode::Initialize).await);
    assert_ok!(bystander.peer().subscribe(SubscribeRequestParams::new("test://static-text")).await);
    assert_ok!(running.peer().subscribe(SubscribeRequestParams::new(WATCHED)).await);
    app.watched.send_modify(|version| *version += 1);
    assert!(matches!(seen(&mut events).await, Seen::Updated(uri) if uri == WATCHED), "a subscriber is told its resource changed");
    assert_err!(time::timeout(Duration::from_millis(200), unrelated.recv()).await, "a subscriber of another URI is told nothing");
    assert_ok!(running.peer().unsubscribe(UnsubscribeRequestParams::new(WATCHED)).await);
    app.watched.send_modify(|version| *version += 1);
    assert_err!(time::timeout(Duration::from_millis(200), events.recv()).await, "an unsubscribed client is told nothing");
}

// --- [LISTEN]

#[tokio::test]
async fn listen_exposes_acknowledged_filter_and_tags_notifications() {
    let (shared, app) = assert_ok!(everything().await);
    let (recorder, _events) = Recorder::new(ProtocolVersion::V_2026_07_28);
    let (running, _serving) = assert_ok!(paired(shared.connection(), recorder, ClientLifecycleMode::Discover { preferred_versions: vec![ProtocolVersion::V_2026_07_28] }).await);
    let mut subscription = assert_ok!(running.peer().listen(SubscriptionFilter::builder().tools_list_changed().build()).await);
    assert_eq!(subscription.acknowledged(), &SubscriptionFilter::builder().tools_list_changed().build());
    app.routers.disable("greet");
    let notification = assert_some!(assert_ok!(assert_ok!(time::timeout(Duration::from_secs(5), subscription.next()).await)), "tool notification");
    assert!(matches!(notification, ServerNotification::ToolListChangedNotification(_)), "{notification:?}");
    assert_eq!(notification.get_meta().subscription_id(), Some(subscription.id().clone()));
}
