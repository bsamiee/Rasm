# [HOSTINGER_BILLING]

Every domain and VPS purchase consumes a catalog price `item_id` and an optional `payment_method_id`, resolving the catalog first. REST entries on `/api/billing/v1` map one-to-one onto the `hostinger` MCP `billing_*` tools. All amounts are integer minor units (cents), `1799` is `$17.99`.

## [01]-[CATALOG]

Catalog resolution is nested: a catalog item has a `category`, a string `id`, and a `prices[]` array, and each price has its own string `id`. Purchases consume the price `id`.

- Catalog-item id: `hostingercom-<category>-<sku>` (`hostingercom-vps-kvm2`, `hostingercom-domain-com`)
- Price id: `<catalog-item-id>-<currency>-<periodN><unit>` (`hostingercom-vps-kvm2-usd-1m`, `hostingercom-domain-com-usd-1y`)

```bash
# Filter by category or name, read a prices[].id for the intended term
curl -X GET "https://developers.hostinger.com/api/billing/v1/catalog?category=vps" -H "Authorization: Bearer $HOSTINGER_API_TOKEN"
```

Each price has `currency`, `price`, `first_period_price` (the promotional first-term price), `period`, and `period_unit` (`month`, `year`, `day`, `week`, `none`), and the catalog-item `metadata` shape varies by category. Orders go through the resource endpoint that owns the product (`POST /api/domains/v1/portfolio` for a domain, `POST /api/vps/v1/virtual-machines` for a VM), each taking the price `item_id` and an optional `payment_method_id`. No order goes through `/api/billing/v1` directly.

## [02]-[PAYMENT_METHODS]

Payment methods are created only in hPanel (`hpanel.hostinger.com/billing/payment-methods`), the API lists, sets the default, and deletes. Omitted `payment_method_id` on a purchase falls to the account default.

```bash
curl -X GET  "https://developers.hostinger.com/api/billing/v1/payment-methods" -H "Authorization: Bearer $HOSTINGER_API_TOKEN"
curl -X POST "https://developers.hostinger.com/api/billing/v1/payment-methods/<payment-method-id>" -H "Authorization: Bearer $HOSTINGER_API_TOKEN"   # Set default
```

`credit_card` methods can demand extra verification that leaves an order unprocessed, a non-card default clears that path. `paymentMethodId` is an integer.

## [03]-[SUBSCRIPTIONS]

Subscriptions have `status`, `total_price`, `renewal_price`, `is_auto_renewed`, `expires_at`, and `next_billing_at`. API-created orders auto-renew by default, the toggle is asymmetric (enable is `PATCH`, disable is `DELETE`), and cancellation runs through hPanel.

```bash
curl -X GET    "https://developers.hostinger.com/api/billing/v1/subscriptions" -H "Authorization: Bearer $HOSTINGER_API_TOKEN"
curl -X PATCH  "https://developers.hostinger.com/api/billing/v1/subscriptions/{id}/auto-renewal/enable"  -H "Authorization: Bearer $HOSTINGER_API_TOKEN"
curl -X DELETE "https://developers.hostinger.com/api/billing/v1/subscriptions/{id}/auto-renewal/disable" -H "Authorization: Bearer $HOSTINGER_API_TOKEN"
```

## [04]-[API_REFERENCE]

| [INDEX] | [METHOD]       | [ENDPOINT]                                          | [DESCRIPTION]                                        |
| :-----: | :------------- | :-------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `GET`          | `/api/billing/v1/catalog`                           | Catalog items and prices, `category`, `name` filters |
|  [02]   | `GET`          | `/api/billing/v1/payment-methods`                   | List payment methods                                 |
|  [03]   | `POST/DELETE`  | `/api/billing/v1/payment-methods/{id}`              | Set default, delete                                  |
|  [04]   | `GET`          | `/api/billing/v1/subscriptions`                     | List subscriptions                                   |
|  [05]   | `PATCH/DELETE` | `/api/billing/v1/subscriptions/{id}/auto-renewal/*` | Toggle auto-renewal (enable, disable)                |
