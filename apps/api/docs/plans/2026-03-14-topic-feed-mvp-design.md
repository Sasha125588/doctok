# Topic feed MVP design

Date: 2026-03-14

## Goal

Support a 2D swipe experience for DocTok:
- vertical swipe switches topics
- horizontal swipe switches posts inside the current topic

## Constraints

- One logical topic can have multiple `raw_documents`, one per language.
- A topic typically has about 4 posts and at most about 10.
- Entering a topic should always open the first post.
- MVP should stay simple and fit the current Postgres + Dapper backend.

## Approaches considered

### 1. Topic feed + topic detail fetch
- `GET /feed/topics` returns paginated topics for vertical swipe.
- `GET /topics/{slug}` returns all posts for one topic.
- Frontend owns the active topic index and active post index.

Pros:
- Simple backend model
- Cheap to paginate
- Fits current schema well
- Easy to extend later

Cons:
- Entering a topic requires a second request
- Best UX needs frontend prefetch

### 2. Feed window with nested topics and posts
- One endpoint returns multiple topics, each with all posts.

Pros:
- Very smooth client UX
- Fewer requests

Cons:
- Heavier payloads
- Wastes work for unseen topics
- More complicated caching

### 3. Stateful feed session
- Backend stores or controls an ordered topic session.

Pros:
- Maximum control over ranking and dedupe
- Good base for advanced recommendations

Cons:
- Overkill for MVP
- Adds server-side state and complexity

## Recommendation

Use approach 1 for MVP.

## Backend design

### Vertical feed
Add `GET /api/feed/topics`.

Behavior:
- returns topics, not posts
- filters by `lang`
- paginates with cursor
- orders by topic popularity descending and topic id descending
- includes a preview card and `postCount`

### Horizontal feed
Keep `GET /api/topics/{slug}`.

Behavior:
- returns all posts for one topic and language
- frontend starts at the first post every time
- no server-side progress tracking in MVP

### Ordering inside a topic
Use deterministic ordering:
1. `summary`
2. `example`
3. `fact`
4. `definition`
5. fallback by `position`, then `id`

### Language handling
For MVP, only return content that exists for the requested language.
No backend fallback language.

## Ranking for MVP

Use a global topic ranking based on `raw_documents.popularity` for the requested language.
Personalization is deferred until we collect topic-level interaction signals.

## Future path to V2

Later we can add:
- topic impressions and skips
- user-topic affinity
- anti-repeat logic
- batched prefetch windows
- richer ranking signals
