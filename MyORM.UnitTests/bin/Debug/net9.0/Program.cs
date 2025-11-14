try
{
    var server = new HttpServer.Framework.Server.HttpServer();
    server.Start();

    Console.WriteLine("нажмите /stop для завершения.\n");

    while (true)
    {
        var stop = Console.ReadLine();
        if (stop?.Trim().ToLower() == "/stop")
        {
            server.Stop();
            break;
        }
    }
}
catch (Exception e)
{
    Console.WriteLine("Ошибка: " + e.Message);
}