using Dapper;
using Domain.Posts;
using Domain.Shared;
using Infrastructure.Persistence.ConnectionFactory;

namespace Infrastructure.Persistence.Repositories;

public sealed class SavedPostsRepository(IDbConnectionFactory dbf)
{
  public async Task<IReadOnlyList<SavedPostView>> List(
    SavedPostsCursor? cursor,
    Guid userId,
    int limit,
    CancellationToken ct)
  {
    const string query = """
                         select  
                           sp.post_id as post_id,
                           p.title as title,
                           p.kind as kind,
                           t.slug as topic_slug,
                           t.title as topic_title,
                           sp.created_at as saved_at
                         from public.saved_posts sp
                         join posts p on p.id = sp.post_id
                         join topics t on t.id = p.topic_id
                         where sp.user_id = @userId
                           and (
                             @cursorId is null
                             or (sp.created_at, sp.post_id) < (@cursorCreatedAt, @cursorId)
                           )
                         order by 
                           sp.created_at desc,
                           sp.post_id desc
                         limit @limit
                         """;

    using var db = dbf.Create();

    var parameters = new
    {
      cursorId = cursor?.Id,
      cursorCreatedAt = cursor?.CreatedAt,
      userId,
      limit,
    };

    var savedPosts = await db.QueryAsync<SavedPostView>(new CommandDefinition(query, parameters, cancellationToken: ct));

    return savedPosts.ToList();
  }

  public async Task Save(
    Guid userId,
    long postId,
    CancellationToken ct)
  {
    const string query = """
                         insert into public.saved_posts(user_id, post_id)
                         values(@userId, @postId)
                         on conflict (user_id, post_id) do nothing
                         """;

    using var db = dbf.Create();

    var parameters = new
    {
      userId,
      postId,
    };

    await db.ExecuteAsync(new CommandDefinition(query, parameters, cancellationToken: ct));
  }

  public async Task<long?> Remove(
    Guid userId,
    long postId,
    CancellationToken ct)
  {
    const string query = """
                         delete from public.saved_posts
                         where user_id = @userId and post_id = @postId
                         returning post_id
                         """;

    using var db = dbf.Create();

    var parameters = new
    {
      userId,
      postId,
    };

    var savedPostId = await db.ExecuteScalarAsync<long?>(new CommandDefinition(query, parameters, cancellationToken: ct));

    return savedPostId;
  }
}
