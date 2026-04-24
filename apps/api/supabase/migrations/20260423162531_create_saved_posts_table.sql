create table saved_posts (
    user_id uuid not null references auth.users(id) on delete cascade,
    post_id bigint not null references posts(id) on delete cascade,
    created_at timestamptz not null default now(),
        
    primary key(user_id, post_id)
);

create index saved_posts_user_id_created_at_idx 
    on saved_posts (user_id, created_at desc);
