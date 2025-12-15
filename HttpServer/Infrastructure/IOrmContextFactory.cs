using MyORM;

namespace HttpServer.Infrastructure;

public interface IOrmContextFactory
{
    OrmContext Create();
}