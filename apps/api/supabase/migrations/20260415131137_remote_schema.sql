drop extension if exists "pg_net";

create type "public"."t_reaction_value" as enum ('like', 'dislike');

create type "public"."t_user_role" as enum ('user', 'admin');

create sequence "public"."cards_id_seq";

create sequence "public"."comments_id_seq";

create sequence "public"."jobs_id_seq";

create sequence "public"."raw_document_links_id_seq";

create sequence "public"."raw_documents_id_seq";

create sequence "public"."sources_id_seq";

create sequence "public"."topics_id_seq";


  create table "public"."comment_reactions" (
    "comment_id" bigint not null,
    "user_id" uuid not null,
    "value" public.t_reaction_value not null,
    "created_at" timestamp with time zone not null default now(),
    "updated_at" timestamp with time zone not null default now()
      );



  create table "public"."comments" (
    "id" bigint not null default nextval('public.comments_id_seq'::regclass),
    "post_id" bigint not null,
    "user_id" uuid not null,
    "parent_comment_id" bigint,
    "body" text not null,
    "created_at" timestamp with time zone not null default now(),
    "updated_at" timestamp with time zone not null default now(),
    "deleted_at" timestamp with time zone,
    "like_count" integer not null default 0,
    "dislike_count" integer not null default 0
      );



  create table "public"."jobs" (
    "id" bigint not null default nextval('public.jobs_id_seq'::regclass),
    "job_type" text not null,
    "job_key" text not null,
    "payload" jsonb not null,
    "status" text not null default 'pending'::text,
    "attempts" integer not null default 0,
    "created_at" timestamp with time zone not null default now(),
    "updated_at" timestamp with time zone not null default now(),
    "last_error" text,
    "next_attempt_at" timestamp with time zone not null default now()
      );



  create table "public"."post_reactions" (
    "post_id" bigint not null,
    "user_id" uuid not null,
    "value" public.t_reaction_value not null,
    "created_at" timestamp with time zone not null default now(),
    "updated_at" timestamp with time zone not null default now()
      );



  create table "public"."posts" (
    "id" bigint not null default nextval('public.cards_id_seq'::regclass),
    "topic_id" bigint not null,
    "raw_document_id" bigint not null,
    "lang" text not null,
    "kind" text not null,
    "title" text,
    "body" text not null,
    "position" integer not null default 0,
    "created_at" timestamp with time zone not null default now(),
    "like_count" integer not null default 0,
    "dislike_count" integer not null default 0,
    "comment_count" integer not null default 0,
    "generation_level" smallint not null default 0
      );



  create table "public"."raw_document_links" (
    "id" bigint not null default nextval('public.raw_document_links_id_seq'::regclass),
    "raw_document_id" bigint not null,
    "kind" text not null,
    "target_source_id" bigint,
    "target_lang" text,
    "target_external_ref" text,
    "url" text,
    "label" text,
    "created_at" timestamp with time zone not null default now()
      );



  create table "public"."raw_documents" (
    "id" bigint not null default nextval('public.raw_documents_id_seq'::regclass),
    "source_id" bigint not null,
    "lang" text not null,
    "external_ref" text not null,
    "title" text,
    "content" text not null,
    "fetched_at" timestamp with time zone not null default now(),
    "page_type" text,
    "popularity" double precision,
    "source_modified_at" timestamp with time zone,
    "other_locales" text[]
      );



  create table "public"."sources" (
    "id" bigint not null default nextval('public.sources_id_seq'::regclass),
    "code" text not null,
    "title" text not null
      );



  create table "public"."topic_documents" (
    "topic_id" bigint not null,
    "raw_document_id" bigint not null
      );



  create table "public"."topics" (
    "id" bigint not null default nextval('public.topics_id_seq'::regclass),
    "slug" text not null,
    "title" text not null,
    "created_at" timestamp with time zone not null default now()
      );



  create table "public"."user_roles" (
    "user_id" uuid not null,
    "role" public.t_user_role not null default 'user'::public.t_user_role
      );


alter sequence "public"."cards_id_seq" owned by "public"."posts"."id";

alter sequence "public"."comments_id_seq" owned by "public"."comments"."id";

alter sequence "public"."jobs_id_seq" owned by "public"."jobs"."id";

alter sequence "public"."raw_document_links_id_seq" owned by "public"."raw_document_links"."id";

alter sequence "public"."raw_documents_id_seq" owned by "public"."raw_documents"."id";

alter sequence "public"."sources_id_seq" owned by "public"."sources"."id";

alter sequence "public"."topics_id_seq" owned by "public"."topics"."id";

CREATE UNIQUE INDEX cards_pkey ON public.posts USING btree (id);

CREATE UNIQUE INDEX comment_reactions_pkey ON public.comment_reactions USING btree (comment_id, user_id);

CREATE UNIQUE INDEX comments_pkey ON public.comments USING btree (id);

CREATE INDEX ix_cards_topic_lang ON public.posts USING btree (topic_id, lang);

CREATE INDEX ix_comments_parent_created ON public.comments USING btree (parent_comment_id, created_at);

CREATE INDEX ix_comments_post_parent_created ON public.comments USING btree (post_id, parent_comment_id, created_at DESC);

CREATE INDEX ix_jobs_pending_next_attempt ON public.jobs USING btree (status, next_attempt_at, created_at);

CREATE INDEX ix_jobs_status_created ON public.jobs USING btree (status, created_at);

CREATE INDEX ix_post_votes_user_post ON public.post_reactions USING btree (user_id, post_id);

CREATE INDEX ix_raw_document_links_doc ON public.raw_document_links USING btree (raw_document_id);

CREATE INDEX ix_raw_document_links_target ON public.raw_document_links USING btree (target_source_id, target_lang, target_external_ref) WHERE (kind = 'internal'::text);

CREATE UNIQUE INDEX jobs_job_key_key ON public.jobs USING btree (job_key);

CREATE UNIQUE INDEX jobs_pkey ON public.jobs USING btree (id);

CREATE UNIQUE INDEX post_votes_pkey ON public.post_reactions USING btree (post_id, user_id);

CREATE UNIQUE INDEX raw_document_links_pkey ON public.raw_document_links USING btree (id);

CREATE UNIQUE INDEX raw_documents_pkey ON public.raw_documents USING btree (id);

CREATE UNIQUE INDEX raw_documents_source_id_lang_external_ref_key ON public.raw_documents USING btree (source_id, lang, external_ref);

CREATE UNIQUE INDEX sources_code_key ON public.sources USING btree (code);

CREATE UNIQUE INDEX sources_pkey ON public.sources USING btree (id);

CREATE UNIQUE INDEX topic_documents_pkey ON public.topic_documents USING btree (topic_id, raw_document_id);

CREATE UNIQUE INDEX topics_pkey ON public.topics USING btree (id);

CREATE UNIQUE INDEX topics_slug_key ON public.topics USING btree (slug);

CREATE UNIQUE INDEX user_roles_pkey ON public.user_roles USING btree (user_id);

CREATE UNIQUE INDEX user_roles_user_id_key ON public.user_roles USING btree (user_id);

CREATE UNIQUE INDEX ux_cards_unique ON public.posts USING btree (raw_document_id, lang, kind, "position");

CREATE UNIQUE INDEX ux_raw_document_links_external ON public.raw_document_links USING btree (raw_document_id, kind, url);

CREATE UNIQUE INDEX ux_raw_document_links_internal ON public.raw_document_links USING btree (raw_document_id, kind, target_source_id, target_lang, target_external_ref);

alter table "public"."comment_reactions" add constraint "comment_reactions_pkey" PRIMARY KEY using index "comment_reactions_pkey";

alter table "public"."comments" add constraint "comments_pkey" PRIMARY KEY using index "comments_pkey";

alter table "public"."jobs" add constraint "jobs_pkey" PRIMARY KEY using index "jobs_pkey";

alter table "public"."post_reactions" add constraint "post_votes_pkey" PRIMARY KEY using index "post_votes_pkey";

alter table "public"."posts" add constraint "cards_pkey" PRIMARY KEY using index "cards_pkey";

alter table "public"."raw_document_links" add constraint "raw_document_links_pkey" PRIMARY KEY using index "raw_document_links_pkey";

alter table "public"."raw_documents" add constraint "raw_documents_pkey" PRIMARY KEY using index "raw_documents_pkey";

alter table "public"."sources" add constraint "sources_pkey" PRIMARY KEY using index "sources_pkey";

alter table "public"."topic_documents" add constraint "topic_documents_pkey" PRIMARY KEY using index "topic_documents_pkey";

alter table "public"."topics" add constraint "topics_pkey" PRIMARY KEY using index "topics_pkey";

alter table "public"."user_roles" add constraint "user_roles_pkey" PRIMARY KEY using index "user_roles_pkey";

alter table "public"."comment_reactions" add constraint "comment_reactions_comment_id_fkey" FOREIGN KEY (comment_id) REFERENCES public.comments(id) ON UPDATE CASCADE ON DELETE CASCADE not valid;

alter table "public"."comment_reactions" validate constraint "comment_reactions_comment_id_fkey";

alter table "public"."comment_reactions" add constraint "comment_reactions_user_id_fkey" FOREIGN KEY (user_id) REFERENCES auth.users(id) ON UPDATE CASCADE ON DELETE CASCADE not valid;

alter table "public"."comment_reactions" validate constraint "comment_reactions_user_id_fkey";

alter table "public"."comments" add constraint "comments_parent_comment_id_fkey" FOREIGN KEY (parent_comment_id) REFERENCES public.comments(id) ON DELETE CASCADE not valid;

alter table "public"."comments" validate constraint "comments_parent_comment_id_fkey";

alter table "public"."comments" add constraint "comments_post_id_fkey" FOREIGN KEY (post_id) REFERENCES public.posts(id) ON DELETE CASCADE not valid;

alter table "public"."comments" validate constraint "comments_post_id_fkey";

alter table "public"."jobs" add constraint "jobs_job_key_key" UNIQUE using index "jobs_job_key_key";

alter table "public"."post_reactions" add constraint "post_reactions_post_id_fkey" FOREIGN KEY (post_id) REFERENCES public.posts(id) ON UPDATE CASCADE ON DELETE CASCADE not valid;

alter table "public"."post_reactions" validate constraint "post_reactions_post_id_fkey";

alter table "public"."post_reactions" add constraint "post_reactions_user_id_fkey" FOREIGN KEY (user_id) REFERENCES auth.users(id) ON UPDATE CASCADE ON DELETE CASCADE not valid;

alter table "public"."post_reactions" validate constraint "post_reactions_user_id_fkey";

alter table "public"."posts" add constraint "cards_kind_check" CHECK ((kind = ANY (ARRAY['summary'::text, 'concept'::text, 'example'::text, 'tip'::text]))) not valid;

alter table "public"."posts" validate constraint "cards_kind_check";

alter table "public"."posts" add constraint "cards_raw_document_id_fkey" FOREIGN KEY (raw_document_id) REFERENCES public.raw_documents(id) ON DELETE CASCADE not valid;

alter table "public"."posts" validate constraint "cards_raw_document_id_fkey";

alter table "public"."posts" add constraint "cards_topic_id_fkey" FOREIGN KEY (topic_id) REFERENCES public.topics(id) ON DELETE CASCADE not valid;

alter table "public"."posts" validate constraint "cards_topic_id_fkey";

alter table "public"."raw_document_links" add constraint "raw_document_links_kind_check" CHECK ((kind = ANY (ARRAY['internal'::text, 'external'::text]))) not valid;

alter table "public"."raw_document_links" validate constraint "raw_document_links_kind_check";

alter table "public"."raw_document_links" add constraint "raw_document_links_raw_document_id_fkey" FOREIGN KEY (raw_document_id) REFERENCES public.raw_documents(id) ON DELETE CASCADE not valid;

alter table "public"."raw_document_links" validate constraint "raw_document_links_raw_document_id_fkey";

alter table "public"."raw_document_links" add constraint "raw_document_links_target_source_id_fkey" FOREIGN KEY (target_source_id) REFERENCES public.sources(id) ON DELETE SET NULL not valid;

alter table "public"."raw_document_links" validate constraint "raw_document_links_target_source_id_fkey";

alter table "public"."raw_documents" add constraint "raw_documents_source_id_fkey" FOREIGN KEY (source_id) REFERENCES public.sources(id) ON DELETE CASCADE not valid;

alter table "public"."raw_documents" validate constraint "raw_documents_source_id_fkey";

alter table "public"."raw_documents" add constraint "raw_documents_source_id_lang_external_ref_key" UNIQUE using index "raw_documents_source_id_lang_external_ref_key";

alter table "public"."sources" add constraint "sources_code_key" UNIQUE using index "sources_code_key";

alter table "public"."topic_documents" add constraint "topic_documents_raw_document_id_fkey" FOREIGN KEY (raw_document_id) REFERENCES public.raw_documents(id) ON DELETE CASCADE not valid;

alter table "public"."topic_documents" validate constraint "topic_documents_raw_document_id_fkey";

alter table "public"."topic_documents" add constraint "topic_documents_topic_id_fkey" FOREIGN KEY (topic_id) REFERENCES public.topics(id) ON DELETE CASCADE not valid;

alter table "public"."topic_documents" validate constraint "topic_documents_topic_id_fkey";

alter table "public"."topics" add constraint "topics_slug_key" UNIQUE using index "topics_slug_key";

alter table "public"."user_roles" add constraint "user_roles_user_id_fkey" FOREIGN KEY (user_id) REFERENCES auth.users(id) ON DELETE CASCADE not valid;

alter table "public"."user_roles" validate constraint "user_roles_user_id_fkey";

alter table "public"."user_roles" add constraint "user_roles_user_id_key" UNIQUE using index "user_roles_user_id_key";

set check_function_bodies = off;

CREATE OR REPLACE FUNCTION public.custom_access_token_hook(event jsonb)
 RETURNS jsonb
 LANGUAGE plpgsql
AS $function$declare
    claims jsonb;
    user_role public.t_user_role;
  begin
    select role into user_role from public.user_roles where user_id = (event->>'user_id')::uuid;

    claims := event->'claims';

    if user_role is not null then
      claims := jsonb_set(claims, '{user_role}', to_jsonb(user_role));
    else
      claims := jsonb_set(claims, '{user_role}', 'null');
    end if;

    event := jsonb_set(event, '{claims}', claims);

    return event;
  end;$function$
;

CREATE OR REPLACE FUNCTION public.handle_new_user()
 RETURNS trigger
 LANGUAGE plpgsql
 SECURITY DEFINER
 SET search_path TO 'public'
AS $function$
begin
  insert into public.user_roles (user_id)
  values (new.id);

  return new;
end;
$function$
;

grant delete on table "public"."comment_reactions" to "anon";

grant insert on table "public"."comment_reactions" to "anon";

grant references on table "public"."comment_reactions" to "anon";

grant select on table "public"."comment_reactions" to "anon";

grant trigger on table "public"."comment_reactions" to "anon";

grant truncate on table "public"."comment_reactions" to "anon";

grant update on table "public"."comment_reactions" to "anon";

grant delete on table "public"."comment_reactions" to "authenticated";

grant insert on table "public"."comment_reactions" to "authenticated";

grant references on table "public"."comment_reactions" to "authenticated";

grant select on table "public"."comment_reactions" to "authenticated";

grant trigger on table "public"."comment_reactions" to "authenticated";

grant truncate on table "public"."comment_reactions" to "authenticated";

grant update on table "public"."comment_reactions" to "authenticated";

grant delete on table "public"."comment_reactions" to "service_role";

grant insert on table "public"."comment_reactions" to "service_role";

grant references on table "public"."comment_reactions" to "service_role";

grant select on table "public"."comment_reactions" to "service_role";

grant trigger on table "public"."comment_reactions" to "service_role";

grant truncate on table "public"."comment_reactions" to "service_role";

grant update on table "public"."comment_reactions" to "service_role";

grant delete on table "public"."comments" to "anon";

grant insert on table "public"."comments" to "anon";

grant references on table "public"."comments" to "anon";

grant select on table "public"."comments" to "anon";

grant trigger on table "public"."comments" to "anon";

grant truncate on table "public"."comments" to "anon";

grant update on table "public"."comments" to "anon";

grant delete on table "public"."comments" to "authenticated";

grant insert on table "public"."comments" to "authenticated";

grant references on table "public"."comments" to "authenticated";

grant select on table "public"."comments" to "authenticated";

grant trigger on table "public"."comments" to "authenticated";

grant truncate on table "public"."comments" to "authenticated";

grant update on table "public"."comments" to "authenticated";

grant delete on table "public"."comments" to "service_role";

grant insert on table "public"."comments" to "service_role";

grant references on table "public"."comments" to "service_role";

grant select on table "public"."comments" to "service_role";

grant trigger on table "public"."comments" to "service_role";

grant truncate on table "public"."comments" to "service_role";

grant update on table "public"."comments" to "service_role";

grant delete on table "public"."jobs" to "anon";

grant insert on table "public"."jobs" to "anon";

grant references on table "public"."jobs" to "anon";

grant select on table "public"."jobs" to "anon";

grant trigger on table "public"."jobs" to "anon";

grant truncate on table "public"."jobs" to "anon";

grant update on table "public"."jobs" to "anon";

grant delete on table "public"."jobs" to "authenticated";

grant insert on table "public"."jobs" to "authenticated";

grant references on table "public"."jobs" to "authenticated";

grant select on table "public"."jobs" to "authenticated";

grant trigger on table "public"."jobs" to "authenticated";

grant truncate on table "public"."jobs" to "authenticated";

grant update on table "public"."jobs" to "authenticated";

grant delete on table "public"."jobs" to "service_role";

grant insert on table "public"."jobs" to "service_role";

grant references on table "public"."jobs" to "service_role";

grant select on table "public"."jobs" to "service_role";

grant trigger on table "public"."jobs" to "service_role";

grant truncate on table "public"."jobs" to "service_role";

grant update on table "public"."jobs" to "service_role";

grant delete on table "public"."post_reactions" to "anon";

grant insert on table "public"."post_reactions" to "anon";

grant references on table "public"."post_reactions" to "anon";

grant select on table "public"."post_reactions" to "anon";

grant trigger on table "public"."post_reactions" to "anon";

grant truncate on table "public"."post_reactions" to "anon";

grant update on table "public"."post_reactions" to "anon";

grant delete on table "public"."post_reactions" to "authenticated";

grant insert on table "public"."post_reactions" to "authenticated";

grant references on table "public"."post_reactions" to "authenticated";

grant select on table "public"."post_reactions" to "authenticated";

grant trigger on table "public"."post_reactions" to "authenticated";

grant truncate on table "public"."post_reactions" to "authenticated";

grant update on table "public"."post_reactions" to "authenticated";

grant delete on table "public"."post_reactions" to "service_role";

grant insert on table "public"."post_reactions" to "service_role";

grant references on table "public"."post_reactions" to "service_role";

grant select on table "public"."post_reactions" to "service_role";

grant trigger on table "public"."post_reactions" to "service_role";

grant truncate on table "public"."post_reactions" to "service_role";

grant update on table "public"."post_reactions" to "service_role";

grant delete on table "public"."posts" to "anon";

grant insert on table "public"."posts" to "anon";

grant references on table "public"."posts" to "anon";

grant select on table "public"."posts" to "anon";

grant trigger on table "public"."posts" to "anon";

grant truncate on table "public"."posts" to "anon";

grant update on table "public"."posts" to "anon";

grant delete on table "public"."posts" to "authenticated";

grant insert on table "public"."posts" to "authenticated";

grant references on table "public"."posts" to "authenticated";

grant select on table "public"."posts" to "authenticated";

grant trigger on table "public"."posts" to "authenticated";

grant truncate on table "public"."posts" to "authenticated";

grant update on table "public"."posts" to "authenticated";

grant delete on table "public"."posts" to "service_role";

grant insert on table "public"."posts" to "service_role";

grant references on table "public"."posts" to "service_role";

grant select on table "public"."posts" to "service_role";

grant trigger on table "public"."posts" to "service_role";

grant truncate on table "public"."posts" to "service_role";

grant update on table "public"."posts" to "service_role";

grant delete on table "public"."raw_document_links" to "anon";

grant insert on table "public"."raw_document_links" to "anon";

grant references on table "public"."raw_document_links" to "anon";

grant select on table "public"."raw_document_links" to "anon";

grant trigger on table "public"."raw_document_links" to "anon";

grant truncate on table "public"."raw_document_links" to "anon";

grant update on table "public"."raw_document_links" to "anon";

grant delete on table "public"."raw_document_links" to "authenticated";

grant insert on table "public"."raw_document_links" to "authenticated";

grant references on table "public"."raw_document_links" to "authenticated";

grant select on table "public"."raw_document_links" to "authenticated";

grant trigger on table "public"."raw_document_links" to "authenticated";

grant truncate on table "public"."raw_document_links" to "authenticated";

grant update on table "public"."raw_document_links" to "authenticated";

grant delete on table "public"."raw_document_links" to "service_role";

grant insert on table "public"."raw_document_links" to "service_role";

grant references on table "public"."raw_document_links" to "service_role";

grant select on table "public"."raw_document_links" to "service_role";

grant trigger on table "public"."raw_document_links" to "service_role";

grant truncate on table "public"."raw_document_links" to "service_role";

grant update on table "public"."raw_document_links" to "service_role";

grant delete on table "public"."raw_documents" to "anon";

grant insert on table "public"."raw_documents" to "anon";

grant references on table "public"."raw_documents" to "anon";

grant select on table "public"."raw_documents" to "anon";

grant trigger on table "public"."raw_documents" to "anon";

grant truncate on table "public"."raw_documents" to "anon";

grant update on table "public"."raw_documents" to "anon";

grant delete on table "public"."raw_documents" to "authenticated";

grant insert on table "public"."raw_documents" to "authenticated";

grant references on table "public"."raw_documents" to "authenticated";

grant select on table "public"."raw_documents" to "authenticated";

grant trigger on table "public"."raw_documents" to "authenticated";

grant truncate on table "public"."raw_documents" to "authenticated";

grant update on table "public"."raw_documents" to "authenticated";

grant delete on table "public"."raw_documents" to "service_role";

grant insert on table "public"."raw_documents" to "service_role";

grant references on table "public"."raw_documents" to "service_role";

grant select on table "public"."raw_documents" to "service_role";

grant trigger on table "public"."raw_documents" to "service_role";

grant truncate on table "public"."raw_documents" to "service_role";

grant update on table "public"."raw_documents" to "service_role";

grant delete on table "public"."sources" to "anon";

grant insert on table "public"."sources" to "anon";

grant references on table "public"."sources" to "anon";

grant select on table "public"."sources" to "anon";

grant trigger on table "public"."sources" to "anon";

grant truncate on table "public"."sources" to "anon";

grant update on table "public"."sources" to "anon";

grant delete on table "public"."sources" to "authenticated";

grant insert on table "public"."sources" to "authenticated";

grant references on table "public"."sources" to "authenticated";

grant select on table "public"."sources" to "authenticated";

grant trigger on table "public"."sources" to "authenticated";

grant truncate on table "public"."sources" to "authenticated";

grant update on table "public"."sources" to "authenticated";

grant delete on table "public"."sources" to "service_role";

grant insert on table "public"."sources" to "service_role";

grant references on table "public"."sources" to "service_role";

grant select on table "public"."sources" to "service_role";

grant trigger on table "public"."sources" to "service_role";

grant truncate on table "public"."sources" to "service_role";

grant update on table "public"."sources" to "service_role";

grant delete on table "public"."topic_documents" to "anon";

grant insert on table "public"."topic_documents" to "anon";

grant references on table "public"."topic_documents" to "anon";

grant select on table "public"."topic_documents" to "anon";

grant trigger on table "public"."topic_documents" to "anon";

grant truncate on table "public"."topic_documents" to "anon";

grant update on table "public"."topic_documents" to "anon";

grant delete on table "public"."topic_documents" to "authenticated";

grant insert on table "public"."topic_documents" to "authenticated";

grant references on table "public"."topic_documents" to "authenticated";

grant select on table "public"."topic_documents" to "authenticated";

grant trigger on table "public"."topic_documents" to "authenticated";

grant truncate on table "public"."topic_documents" to "authenticated";

grant update on table "public"."topic_documents" to "authenticated";

grant delete on table "public"."topic_documents" to "service_role";

grant insert on table "public"."topic_documents" to "service_role";

grant references on table "public"."topic_documents" to "service_role";

grant select on table "public"."topic_documents" to "service_role";

grant trigger on table "public"."topic_documents" to "service_role";

grant truncate on table "public"."topic_documents" to "service_role";

grant update on table "public"."topic_documents" to "service_role";

grant delete on table "public"."topics" to "anon";

grant insert on table "public"."topics" to "anon";

grant references on table "public"."topics" to "anon";

grant select on table "public"."topics" to "anon";

grant trigger on table "public"."topics" to "anon";

grant truncate on table "public"."topics" to "anon";

grant update on table "public"."topics" to "anon";

grant delete on table "public"."topics" to "authenticated";

grant insert on table "public"."topics" to "authenticated";

grant references on table "public"."topics" to "authenticated";

grant select on table "public"."topics" to "authenticated";

grant trigger on table "public"."topics" to "authenticated";

grant truncate on table "public"."topics" to "authenticated";

grant update on table "public"."topics" to "authenticated";

grant delete on table "public"."topics" to "service_role";

grant insert on table "public"."topics" to "service_role";

grant references on table "public"."topics" to "service_role";

grant select on table "public"."topics" to "service_role";

grant trigger on table "public"."topics" to "service_role";

grant truncate on table "public"."topics" to "service_role";

grant update on table "public"."topics" to "service_role";

grant delete on table "public"."user_roles" to "service_role";

grant insert on table "public"."user_roles" to "service_role";

grant references on table "public"."user_roles" to "service_role";

grant select on table "public"."user_roles" to "service_role";

grant trigger on table "public"."user_roles" to "service_role";

grant truncate on table "public"."user_roles" to "service_role";

grant update on table "public"."user_roles" to "service_role";

grant delete on table "public"."user_roles" to "supabase_auth_admin";

grant insert on table "public"."user_roles" to "supabase_auth_admin";

grant references on table "public"."user_roles" to "supabase_auth_admin";

grant select on table "public"."user_roles" to "supabase_auth_admin";

grant trigger on table "public"."user_roles" to "supabase_auth_admin";

grant truncate on table "public"."user_roles" to "supabase_auth_admin";

grant update on table "public"."user_roles" to "supabase_auth_admin";

CREATE TRIGGER on_auth_user_created AFTER INSERT ON auth.users FOR EACH ROW EXECUTE FUNCTION public.handle_new_user();


