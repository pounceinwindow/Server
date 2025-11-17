// using Azure.Core.Pipeline;
// using HttpServer.Framework.core.Attributes;
// using HttpServer.Framework.Core.HttpResponse;
//
// namespace HttpServer.Endpoints;
//
// [Endpoint]
// public class ProductEndpoint : BaseEndpoint
// {
//     [HttpGet("/")]
//     public void Detail()
//     {
//         Context.Response.StatusCode = 200;
//         Context.Response.RedirectLocation = "/auth";
//         Context.Response.OutputStream.Close();
//     }
// }