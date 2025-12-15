using HttpServer.Framework.Settings;
using MyORM;

namespace HttpServer.Infrastructure;

public sealed class OrmContextFactory : IOrmContextFactory
{
    private readonly string _cs;

    public OrmContextFactory(string cs)
    {
        _cs = cs;
    }

    public OrmContext Create()
    {
        return new OrmContext(_cs);
    }

    public static OrmContextFactory FromSettings()
    {
        return new OrmContextFactory(SettingsManager.Instance.Settings.ConnectionString!);
    }
}