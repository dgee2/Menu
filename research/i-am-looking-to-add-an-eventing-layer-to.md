# Eventing layer options for the Menu backend

Updated: 2026-05-27

## Executive summary

Menu is a strong fit for a transactional outbox based eventing layer: the backend is already on .NET 10, EF Core, SQL Server, and .NET Aspire, and `RecipeService.CreateRecipeAsync` already has an explicit SQL transaction boundary where an outbox write can be added safely (`C:\git\Menu\backend\MenuApi\MenuApi.csproj:33-53`, `C:\git\Menu\backend\Menu.AppHost\Menu.AppHost.csproj:12-20`, `C:\git\Menu\backend\MenuApi\Services\RecipeService.cs:39-53`).

**Best overall recommendation now:** **MassTransit + RabbitMQ + transactional outbox**. That combination best matches the current constraints:

- no licensing budget
- single service today, but likely multiple services later
- Azure is available but portability does not need to be forced
- replay/event sourcing may matter later, but is not a hard requirement now

MassTransit has a first-class transactional outbox story and strong .NET support, while RabbitMQ remains the best low-cost general-purpose broker for application events. Sources: [MassTransit transactional outbox](https://masstransit.io/documentation/patterns/transactional-outbox), [RabbitMQ docs](https://www.rabbitmq.com/docs).

**Best managed Azure alternative:** **MassTransit + Azure Service Bus**. It gives up portability and local parity versus RabbitMQ, but it improves operational burden and has strong queue/topic semantics. Source: [Azure Service Bus overview](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview).

**Important budget conclusion:** **NServiceBus is still technically excellent, but it is not feasible under the current "no licensing budget" constraint.** It should be treated as a budget-constrained option, not as the default runner-up. Source: [NServiceBus licensing](https://docs.particular.net/nservicebus/licensing/).

**If replay becomes strategically important later:** revisit **Kafka**, **NATS JetStream**, or **EventStoreDB**. Do not optimize the first cut around replay if the immediate need is reliable business events such as `RecipeCreated`.

## Scope, assumptions, and context used for scoring

This report uses the clarified context from the conversation:

- The backend may grow from one service into multiple services.
- Azure is not a hard requirement.
- There is no licensing budget right now.
- Event replay or event sourcing may matter later, but lack of it is not a deal-breaker today.
- The decision is for the **Menu backend**, not for a generic platform team.

Confirmed Menu backend context:

- .NET 10 / ASP.NET Core (`C:\git\Menu\backend\MenuApi\MenuApi.csproj:1-64`)
- EF Core + SQL Server (`C:\git\Menu\backend\MenuApi\MenuApi.csproj:33-53`)
- .NET Aspire AppHost already in use (`C:\git\Menu\backend\Menu.AppHost\Menu.AppHost.csproj:1-23`)
- Redis already provisioned in AppHost (`C:\git\Menu\backend\Menu.AppHost\Menu.AppHost.csproj:12-15`)
- Explicit DB transaction around recipe creation (`C:\git\Menu\backend\MenuApi\Services\RecipeService.cs:45-52`)

That existing transaction is the key technical hook: the safest pattern for Menu is to write the recipe row(s) and an outbox record in the same transaction, then publish asynchronously after commit.

## Rubber duck review and adjustments adopted

This scoring was reviewed with the rubber duck agent, and I adopted the key corrections:

1. **Cost is treated as a practical feasibility filter, not just another score.** That is why NServiceBus is shown as budget-constrained despite a high technical score.
2. **Apples-to-oranges categories were separated.** MediatR is treated as an in-process baseline, Dapr as a runtime/platform abstraction, EventStoreDB as an event store, and Redis Pub/Sub vs Redis Streams are scored separately.
3. **Scoring anchors were tightened.** The report now defines what 1 through 5 mean and calls out the important nuances for replay, .NET/Aspire fit, and delivery guarantees.
4. **Additional criteria were added:** idempotency/deduplication, schema/versioning support, and operational tooling/admin UX.
5. **Specific score corrections were applied:**  
   - Azure Service Bus operational complexity reduced from the earlier too-generous position  
   - SQL Server transport operational complexity reduced  
   - Amazon MQ lock-in improved because protocol portability is better than the earlier score implied  
   - Dapr saga/workflow score increased because Dapr Workflow materially changes its orchestration story
6. **AWS-specific options were added**: Amazon MQ, Amazon EventBridge, and Amazon Kinesis.

## How the scoring works

### Common 1-5 scoring anchor

| Score | Meaning |
|---|---|
| 5 | First-class fit; little glue code; strong evidence and ecosystem support |
| 4 | Strong fit; some caveats, but clearly viable |
| 3 | Workable; meaningful trade-offs or custom work required |
| 2 | Weak fit; awkward or significant custom work |
| 1 | Major gap for Menu's needs |

Important nuances:

- For **replay/event-log support**, `5` means durable replay is a core capability, `3` means partial/manual retention or replay, `1` means effectively none.
- For **.NET/Aspire fit**, `5` means mature .NET ergonomics with straightforward local wiring, `3` means usable but custom/manual integration, `1` means thin or awkward fit.
- For **delivery guarantees**, `5` means strong broker/runtime durability and reliability story, `3` means adequate but caveated, `1` means weak or non-durable.

## Evaluation criteria

### Library criteria and weights

Weighted total formula: `sum(score x weight)`.  
Maximum possible library score: `585`.

| ID | Criterion | Weight | Why it matters for Menu |
|---|---|---:|---|
| L1 | Messaging pattern fit | 8 | Need commands/events/pub-sub patterns without fighting the framework |
| L2 | EF Core + outbox fit | 10 | Most important implementation detail for Menu right now |
| L3 | Reliability and retries | 9 | Avoid lost or duplicate business events |
| L4 | Pipeline/handler ergonomics | 5 | Middleware, validation, policies, composability |
| L5 | Saga/workflow support | 6 | Future multi-step processes if the system grows |
| L6 | Replay/event-sourcing friendliness | 4 | Useful later, not decisive now |
| L7 | Multi-service growth path | 8 | Likely future direction |
| L8 | .NET/Aspire fit | 7 | Strong current-stack concern, but not over-weighted |
| L9 | Local dev/test ergonomics | 6 | Fast local iteration matters |
| L10 | Ops visibility and error handling | 6 | Dead letter, poison message, diagnostics story |
| L11 | Idempotency/dedup support | 7 | Required for reliable consumers |
| L12 | Schema/versioning support | 5 | Event contracts will evolve |
| L13 | Transport flexibility | 6 | Avoid unnecessary rework later |
| L14 | Cost/feasibility now | 10 | No license budget is a real constraint |
| L15 | Learning curve/team fit | 5 | Affects adoption speed |
| L16 | Maturity/community/docs | 8 | Reduces delivery risk |
| L17 | Vendor lock-in | 7 | Azure is available, but not mandatory |

### Transport criteria and weights

Maximum possible transport score: `500`.

| ID | Criterion | Weight | Why it matters for Menu |
|---|---|---:|---|
| T1 | Core eventing fit | 8 | How well it handles application integration events |
| T2 | Durability/guarantees | 9 | Critical for business events |
| T3 | Ordering/consumer groups | 5 | Useful for workflow correctness and scaling |
| T4 | Delayed/scheduled delivery | 4 | Handy for retries, reminders, and timed work |
| T5 | Replay/retention | 4 | Useful future option, not today's driver |
| T6 | Throughput/scale | 6 | Important, but Menu is not a massive event platform yet |
| T7 | Managed-service availability | 5 | Reduces operational burden |
| T8 | .NET/Aspire fit | 7 | Strong current-stack concern |
| T9 | Local dev parity | 6 | Easy local reproduction matters |
| T10 | Operational complexity | 7 | Hidden ops cost matters more than raw feature count |
| T11 | Idempotency/DLQ features | 7 | Reliability and recovery behavior |
| T12 | Schema/ecosystem support | 4 | Contract evolution and surrounding ecosystem |
| T13 | Cost/feasibility now | 10 | No license budget, small-team pragmatism |
| T14 | Vendor lock-in/portability | 7 | Portability is not mandatory, but it still matters |
| T15 | Observability/admin tooling | 6 | Troubleshooting and operator UX |
| T16 | Security/auth/networking fit | 5 | Production hardening |

## Categories used

To avoid misleading rankings:

- **MediatR** is a baseline in-process dispatcher, not a distributed eventing stack.
- **Dapr** is a runtime/platform abstraction, not just a library.
- **Redis Pub/Sub** and **Redis Streams** are separate options because their semantics are fundamentally different.
- **Amazon MQ (RabbitMQ)** changes ops/cost/lock-in, so it is scored separately from self-managed RabbitMQ.
- **EventStoreDB** is an event store; it is not a drop-in queue broker.
- **AWS MSK** is best understood as a managed deployment shape for **Kafka**, so its trade-offs are largely represented by the Kafka score.

## Option review: application-layer libraries

| Option | Type | Pros | Cons |
|---|---|---|---|
| MassTransit | Distributed messaging library | Excellent .NET support; first-class outbox; strong saga/routing patterns; transport portability | More concepts than the lightest libraries; replay still depends on broker choice |
| NServiceBus | Distributed messaging library | Extremely mature; strong recoverability and saga story; polished ops model | Not feasible now due licensing budget; more framework gravity |
| Wolverine | Distributed messaging library | Modern .NET-first design; good handler ergonomics; strong integration with Jasper-style workflows | Smaller ecosystem than MassTransit/NServiceBus; fewer teams will have prior experience |
| Rebus | Distributed messaging library | Lightweight, pragmatic, inexpensive, easy to understand | Outbox and advanced features are not as first-class as MassTransit/NServiceBus |
| Brighter | Command processor + messaging | Good command pipeline story; can grow toward distributed messaging | Smaller ecosystem; more assembly required for a complete eventing layer |
| CAP | Event bus / eventual consistency helper | Strong outbox flavor; pragmatic fit for integration events | Narrower pattern support and ecosystem depth than top-tier options |
| Dapr | Runtime/platform abstraction | Broker-agnostic pub/sub API; at-least-once delivery; workflow building block exists | Adds sidecar/runtime operational surface; not just a library choice; abstraction can hide broker-specific semantics |
| MediatR | In-process baseline | Very low friction; great for in-process domain events and command dispatch | Not a distributed bus; no broker, no replay, no cross-service event backbone |

Useful references: [MassTransit transactional outbox](https://masstransit.io/documentation/patterns/transactional-outbox), [NServiceBus licensing](https://docs.particular.net/nservicebus/licensing/), [Dapr pub/sub](https://docs.dapr.io/developing-applications/building-blocks/pubsub/pubsub-overview/), [Dapr Workflow](https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-overview/), [Rebus](https://github.com/rebus-org/Rebus), [Wolverine](https://wolverinefx.io/), [Brighter](https://github.com/BrighterCommand/Brighter), [CAP](https://cap.dotnetcore.xyz/), [MediatR](https://github.com/jbogard/MediatR).

## Option review: transports and brokers

| Option | Type | Pros | Cons |
|---|---|---|---|
| RabbitMQ | Broker | Mature, cheap, portable, excellent local dev story | You own broker operations unless managed; limited replay compared with log-based systems |
| Azure Service Bus | Managed broker | Strong reliability, queues/topics, scheduling, Azure ops convenience | Azure lock-in; weaker local parity; cost higher than self-hosted RabbitMQ |
| Amazon MQ (RabbitMQ) | Managed broker | RabbitMQ semantics with managed AWS operations; better portability than AWS-native-only services | Still an AWS service dependency; local parity and tooling are not as simple as plain RabbitMQ |
| Kafka | Distributed log | Excellent replay, retention, scale, and event-streaming posture | Heavier than Menu needs today; more operational and local complexity |
| NATS JetStream | Lightweight durable log/broker | Strong replay story for a lighter-weight system; good portability | .NET ecosystem fit is thinner than RabbitMQ/Azure SB; fewer off-the-shelf integrations |
| SQS + SNS | Cloud queue + pub/sub pattern | Cheap, managed, durable, simple AWS-native building blocks | Pattern composition is awkward; local parity is weak; portability is poor |
| Amazon EventBridge | Event router/integration bus | Strong AWS integrations, scheduling, routing, and serverless fit | Not a great primary application event backbone for Menu; portability is poor |
| Amazon Kinesis | Managed streaming platform | Strong replay and streaming scale | Better for high-volume streams than business workflow messaging; local parity is weak |
| Azure Event Grid | Event routing service | Good Azure event routing and push integrations | Not a primary business workflow/event bus for Menu-style application events |
| Redis Streams | Persistence-backed stream | Easy local fit if Redis is already present; consumer groups and retention exist | Reliability and tooling are weaker than top brokers; not a great long-term backbone |
| Redis Pub/Sub | Ephemeral pub/sub | Very easy and cheap for non-critical notifications | At-most-once delivery and no durable backlog make it a poor primary business-event backbone |
| SQL Server transport | Persistence-backed queue | Reuses existing infrastructure; easy local story | Operationally awkward at scale; polling/contention/growth concerns; weaker long-term shape |
| EventStoreDB | Event store | Best replay/event-sourcing posture; immutable log model | Different architectural commitment; overkill if you only need integration events today |

Useful references: [RabbitMQ docs](https://www.rabbitmq.com/docs), [Azure Service Bus overview](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview), [Amazon MQ](https://aws.amazon.com/amazon-mq/), [Amazon EventBridge](https://aws.amazon.com/eventbridge/features/), [Amazon Kinesis Data Streams](https://aws.amazon.com/kinesis/data-streams/), [NATS JetStream](https://docs.nats.io/nats-concepts/jetstream), [Redis Streams](https://redis.io/docs/latest/develop/data-types/streams/), [Redis Pub/Sub](https://redis.io/docs/latest/develop/pubsub/), [EventStoreDB pricing/features](https://www.eventstore.com/pricing).

## Weighted results: library rankings

### Summary by practical category

#### Distributed .NET messaging libraries

| Rank | Option | Weighted score | Percent | Practical note |
|---|---|---:|---:|---|
| 1 | MassTransit | 556 / 585 | 95.0% | Best overall fit |
| 2 | NServiceBus | 489 / 585 | 83.6% | Technically strong but not feasible now due licensing |
| 3 | Wolverine | 459 / 585 | 78.5% | Strong modern open-source alternative |
| 4 | Rebus | 431 / 585 | 73.7% | Good lightweight option |
| 5 | Brighter | 389 / 585 | 66.5% | Useful if you want more command-processor flavor |
| 6 | CAP | 384 / 585 | 65.6% | Good outbox/event-bus helper, narrower overall fit |

#### Runtime/platform abstraction

| Option | Weighted score | Percent | Practical note |
|---|---:|---:|---|
| Dapr | 400 / 585 | 68.4% | Viable if you want sidecar-driven portability and workflow building blocks |

#### Baseline only

| Option | Weighted score | Percent | Practical note |
|---|---:|---:|---|
| MediatR | 369 / 585 | 63.1% | Good in-process baseline, not a distributed eventing layer |

### Full library scoring matrix

`L1-L17` correspond to the weighted criteria listed above.

| Option | L1 | L2 | L3 | L4 | L5 | L6 | L7 | L8 | L9 | L10 | L11 | L12 | L13 | L14 | L15 | L16 | L17 | Weighted | % |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| MassTransit | 5 | 5 | 5 | 4 | 5 | 3 | 5 | 5 | 5 | 4 | 5 | 4 | 5 | 5 | 4 | 5 | 5 | 556 | 95.0 |
| NServiceBus | 5 | 5 | 5 | 5 | 5 | 3 | 5 | 4 | 4 | 5 | 5 | 4 | 4 | 1 | 3 | 5 | 3 | 489 | 83.6 |
| Wolverine | 4 | 4 | 4 | 4 | 4 | 3 | 4 | 4 | 4 | 3 | 4 | 3 | 3 | 5 | 3 | 4 | 5 | 459 | 78.5 |
| Rebus | 4 | 3 | 4 | 4 | 3 | 2 | 4 | 3 | 4 | 3 | 3 | 2 | 4 | 5 | 4 | 4 | 5 | 431 | 73.7 |
| Dapr | 4 | 3 | 3 | 3 | 3 | 3 | 4 | 3 | 3 | 4 | 3 | 3 | 4 | 5 | 3 | 4 | 2 | 400 | 68.4 |
| Brighter | 4 | 3 | 3 | 4 | 3 | 2 | 3 | 3 | 3 | 3 | 3 | 2 | 3 | 5 | 3 | 3 | 5 | 389 | 66.5 |
| CAP | 4 | 3 | 3 | 3 | 2 | 2 | 3 | 4 | 4 | 3 | 3 | 2 | 3 | 5 | 3 | 3 | 4 | 384 | 65.6 |
| MediatR | 2 | 4 | 2 | 4 | 1 | 1 | 1 | 5 | 5 | 2 | 2 | 2 | 1 | 5 | 5 | 5 | 5 | 369 | 63.1 |

### Why the library scores came out this way

- **MassTransit** wins because it is the strongest combination of transactional outbox fit, transport flexibility, maturity, and cost-feasibility now.
- **NServiceBus** would rank near the top technically, but the current budget constraint is real enough that it should be filtered out for now.
- **Wolverine** scores well as a modern open-source option, but it has less ecosystem depth than MassTransit.
- **Rebus** stays attractive if you want something simpler and lighter than MassTransit.
- **Dapr** improved after the rubber duck review because Dapr Workflow materially strengthens its process orchestration story. Still, Dapr is a larger platform choice than just "pick a library."
- **MediatR** should not be misread as a bus recommendation; it scores on simplicity, local fit, and cost, not on distributed eventing capability.

## Weighted results: transport rankings

### Summary by practical category

#### Best broker-first options for Menu

| Rank | Option | Weighted score | Percent | Practical note |
|---|---|---:|---:|---|
| 1 | RabbitMQ | 424 / 500 | 84.8% | Best current default |
| 2 | Azure Service Bus | 406 / 500 | 81.2% | Best managed Azure option |
| 3 | Amazon MQ (RabbitMQ) | 399 / 500 | 79.8% | Best AWS managed RabbitMQ shape |
| 4 | Kafka | 392 / 500 | 78.4% | Best if replay becomes strategically important |
| 5 | NATS JetStream | 373 / 500 | 74.6% | Strong replay-capable lightweight option |
| 6 | SQS + SNS | 348 / 500 | 69.6% | Reasonable AWS-native pattern, but awkward versus RabbitMQ/Azure SB |

#### Event-store or persistence-backed options

| Option | Weighted score | Percent | Practical note |
|---|---:|---:|---|
| EventStoreDB | 334 / 500 | 66.8% | Consider only if event sourcing/replay becomes strategic |
| Redis Streams | 330 / 500 | 66.0% | Better than Redis Pub/Sub, still not my first choice for the backbone |
| SQL Server transport | 327 / 500 | 65.4% | Acceptable stop-gap, not ideal long-term |
| Redis Pub/Sub | 282 / 500 | 56.4% | Not recommended for critical business events |

#### Cloud event-routing services

| Option | Weighted score | Percent | Practical note |
|---|---:|---:|---|
| Amazon EventBridge | 318 / 500 | 63.6% | Good AWS integration router, weak primary app event bus |
| Amazon Kinesis | 313 / 500 | 62.6% | Good stream platform, not the best business workflow bus |
| Azure Event Grid | 303 / 500 | 60.6% | Good event routing, not my primary bus recommendation |

### Full transport scoring matrix

`T1-T16` correspond to the weighted criteria listed above.

| Option | T1 | T2 | T3 | T4 | T5 | T6 | T7 | T8 | T9 | T10 | T11 | T12 | T13 | T14 | T15 | T16 | Weighted | % |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| RabbitMQ | 5 | 4 | 5 | 4 | 2 | 4 | 4 | 5 | 5 | 3 | 4 | 3 | 5 | 5 | 4 | 4 | 424 | 84.8 |
| Azure Service Bus | 5 | 5 | 4 | 5 | 2 | 4 | 5 | 5 | 3 | 4 | 4 | 3 | 4 | 2 | 4 | 5 | 406 | 81.2 |
| Amazon MQ (RabbitMQ) | 5 | 4 | 5 | 4 | 2 | 4 | 5 | 3 | 4 | 4 | 4 | 3 | 4 | 4 | 4 | 4 | 399 | 79.8 |
| Kafka | 4 | 4 | 4 | 2 | 5 | 5 | 4 | 3 | 4 | 2 | 4 | 5 | 4 | 5 | 4 | 4 | 392 | 78.4 |
| NATS JetStream | 4 | 4 | 4 | 2 | 4 | 5 | 4 | 2 | 4 | 3 | 3 | 2 | 5 | 5 | 3 | 4 | 373 | 74.6 |
| SQS + SNS | 4 | 4 | 3 | 4 | 2 | 2 | 5 | 3 | 1 | 4 | 4 | 2 | 5 | 2 | 4 | 5 | 348 | 69.6 |
| EventStoreDB | 2 | 5 | 4 | 1 | 5 | 4 | 4 | 2 | 3 | 2 | 3 | 4 | 3 | 4 | 4 | 4 | 334 | 66.8 |
| Redis Streams | 3 | 3 | 3 | 2 | 3 | 3 | 4 | 4 | 5 | 3 | 2 | 2 | 5 | 4 | 2 | 3 | 330 | 66.0 |
| SQL Server transport | 3 | 3 | 2 | 3 | 1 | 2 | 4 | 4 | 5 | 3 | 3 | 2 | 5 | 4 | 2 | 4 | 327 | 65.4 |
| Amazon EventBridge | 3 | 3 | 2 | 2 | 4 | 4 | 5 | 2 | 1 | 5 | 3 | 3 | 4 | 1 | 4 | 5 | 318 | 63.6 |
| Amazon Kinesis | 3 | 4 | 2 | 1 | 5 | 5 | 5 | 2 | 1 | 3 | 3 | 4 | 3 | 1 | 4 | 5 | 313 | 62.6 |
| Azure Event Grid | 3 | 3 | 2 | 1 | 2 | 4 | 5 | 3 | 1 | 5 | 2 | 2 | 4 | 2 | 3 | 5 | 303 | 60.6 |
| Redis Pub/Sub | 2 | 1 | 2 | 1 | 1 | 4 | 4 | 4 | 5 | 3 | 1 | 1 | 5 | 4 | 2 | 3 | 282 | 56.4 |

### Why the transport scores came out this way

- **RabbitMQ** wins because it balances cost, portability, local parity, mature tooling, and strong broker semantics better than any other option for Menu's current shape.
- **Azure Service Bus** is close behind because its managed operational model is excellent, but it gives up too much on portability and local parity to take first place.
- **Amazon MQ** scores better than the earlier draft because the lock-in penalty should not be as harsh as AWS-native-only services: RabbitMQ protocol familiarity still matters.
- **Kafka** remains excellent if replay becomes central, but its operational weight is hard to justify for the first implementation in Menu.
- **Redis Pub/Sub** falls where it should after the category cleanup: it is fine for non-critical notifications, but poor for durable business events because Redis documents it as **at-most-once** delivery. Source: [Redis Pub/Sub delivery semantics](https://redis.io/docs/latest/develop/pubsub/).

## Recommended decision for Menu

### My recommendation now

**Pick: MassTransit + RabbitMQ + transactional outbox**

Why:

1. It directly matches the existing `RecipeService.CreateRecipeAsync` transaction boundary.
2. It is fully viable with today's no-license-budget constraint.
3. It keeps the path open to multiple services later without forcing a heavy streaming platform now.
4. It gives strong local development ergonomics and low operational friction for the current stage.
5. It does not over-commit Menu to event sourcing before that need is proven.

### Best alternatives by scenario

| Scenario | Recommended shape | Why |
|---|---|---|
| You want the strongest fit now | MassTransit + RabbitMQ | Best blend of cost, fit, maturity, and portability |
| You prefer Azure-managed infrastructure | MassTransit + Azure Service Bus | Strong reliability and lower broker ops burden |
| AWS becomes the main platform later | MassTransit + Amazon MQ | Keeps RabbitMQ semantics while moving to managed AWS operations |
| Replay becomes a first-class requirement | MassTransit/Wolverine + Kafka or NATS JetStream | Better durable replay and retention story |
| Event sourcing becomes a strategic architecture choice | EventStoreDB, likely with a simpler integration bus beside it | EventStoreDB is strongest when the event log is the system of record |
| You only need in-process notifications for now | MediatR | Useful baseline, but not enough for a true eventing layer |

### What I would not choose as the primary backbone today

- **Redis Pub/Sub** for critical business events
- **Azure Event Grid** or **Amazon EventBridge** as the main internal application event backbone
- **SQL Server transport** as the intended long-term shape
- **Kafka** as the first step unless replay/streaming is already a near-term requirement
- **NServiceBus** under the current no-budget constraint

## Concrete first implementation shape for Menu

For the first slice, I would implement:

1. **A transactional outbox table** in the same SQL database used by Menu.
2. **A `RecipeCreated` integration event** emitted from the recipe creation transaction path (`C:\git\Menu\backend\MenuApi\Services\RecipeService.cs:39-53`).
3. **An outbox dispatcher** that publishes committed events to RabbitMQ through MassTransit.
4. **Idempotent consumers** from day one, even if there is only one consumer initially.
5. **Versioned event contracts** early, even if the first event shape is small.

That gives Menu a reliable eventing foundation without forcing an early move into full event sourcing.

## Additional options investigated

I also considered adjacent options that I did **not** score separately:

- **AWS MSK**: effectively a managed Kafka deployment shape, so its trade-offs are mostly represented by the Kafka score.
- **Azure Event Hubs**: better framed as a streaming/telemetry platform closer to Kafka/Kinesis than to a business workflow broker; not my first recommendation for Menu's application events.
- **Apache Pulsar**: interesting technically, but weaker .NET/Aspire fit than the stronger shortlist options above.

## Final conclusion

If I were making the decision for Menu today, I would choose **MassTransit + RabbitMQ + transactional outbox**.

It is the best fit for the current backend architecture, the no-license-budget constraint, the possibility of future service decomposition, and the fact that replay is only a potential future need rather than today's primary driver.

If managed infrastructure is worth more to the team than broker portability, **MassTransit + Azure Service Bus** is the cleanest managed alternative. If event replay becomes central later, revisit **Kafka**, **NATS JetStream**, or **EventStoreDB** based on whether the need is "durable event log" or true event sourcing.
