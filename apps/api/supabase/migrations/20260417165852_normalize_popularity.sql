update raw_documents
set popularity = 0
where popularity is null;

alter table raw_documents
    alter column popularity set default 0;

alter table raw_documents
    alter column popularity set not null;