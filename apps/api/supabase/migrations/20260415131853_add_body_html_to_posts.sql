-- Pre-rendered HTML version of the markdown body.
-- Computed once on post creation; served directly to the frontend via v-html.
ALTER TABLE public.posts
    ADD COLUMN IF NOT EXISTS body_html text NOT NULL DEFAULT '';
