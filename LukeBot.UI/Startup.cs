using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using LukeBot.Common;
using LukeBot.Common.OAuth;

namespace LukeBot.UI
{
    public class Startup
    {
        async Task LoadPage(string page, HttpContext context)
        {
            StreamReader reader = File.OpenText("LukeBot.UI/Pages/" + page);
            string p = reader.ReadToEnd();
            reader.Close();
            await context.Response.WriteAsync(p);
        }

        async Task HandleAPICall(string call, HttpContext context)
        {
            string responseString;

            if (call == "Users")
            {
                UsersResponse response = new UsersResponse();
                response.status = 0;
                response.users.Add(new Common.UserItem{
                    name = "lookey",
                    displayName = "Looki",
                });
                response.users.Add(new Common.UserItem{
                    name = "michakes",
                    displayName = "Michie",
                });

                responseString = JsonSerializer.Serialize(response);
            }
            else
            {
                ResponseBase response = new ResponseBase{
                    status = 1,
                };

                responseString = JsonSerializer.Serialize(response);
            }

            await context.Response.WriteAsync(responseString);
        }

        async Task HandleServiceCallback(string service, HttpContext context)
        {
            Logger.Info("Received callback for service " + service + ": " + context.Request.Path.Value);
            Logger.Info("We have " + context.Request.Query.Count + " queries:");
            foreach (var query in context.Request.Query)
            {
                Logger.Info("  -> " + query.Key + " = " + query.Value);
            }

            try
            {
                Intermediary srv = CommunicationManager.Instance.GetIntermediary(service);

                if (service == "twitch")
                {
                    UserToken token = new UserToken();
                    token.code = context.Request.Query["code"];
                    token.state = context.Request.Query["state"];
                    token.scope = new List<string>();
                    string scopes = context.Request.Query["scope"];
                    string[] scopesArray = scopes.Split(' ');
                    foreach (string str in scopesArray)
                        token.scope.Add(str);

                    srv.Fulfill(token.state, token);
                }

                await context.Response.WriteAsync(
                    "<html><body style=\"font-family: sans-serif; margin-left: 30px; margin-top: 30px;\">" +
                        "Login to " + service + " successful, you can close the window now.\n" +
                        "<script type=\"text/javascript\">$(document).ready(function() { doCountdown(); });</script>\n" +
                    "</body></html>"
                );
            }
            catch (System.Exception e)
            {
                Logger.Error("{0}", e.Message);
                await context.Response.WriteAsync(
                    "<html><body style=\"font-family: sans-serif; margin-left: 30px; margin-top: 30px;\">" +
                        "Login to " + service + " failed. Check log for details.\n" +
                        "<script type=\"text/javascript\">$(document).ready(function() { doCountdown(); });</script>\n" +
                    "</body></html>"
                );
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => {
                    await LoadPage("index.html", context);
                });
                endpoints.MapGet("{user}/{page}", async context => {
                    var user = context.Request.RouteValues["user"];
                    var page = context.Request.RouteValues["page"];
                    await LoadPage($"{page}.html", context);
                });
                endpoints.MapGet("css/{stylesheet}", async context => {
                    var stylesheet = context.Request.RouteValues["stylesheet"];
                    await LoadPage($"css/{stylesheet}", context);
                });
                endpoints.MapGet("js/{script}", async context => {
                    var script = context.Request.RouteValues["script"];
                    await LoadPage($"js/{script}", context);
                });
                endpoints.MapGet("views/{view}", async context => {
                    var view = context.Request.RouteValues["view"];
                    await LoadPage($"views/{view}", context);
                });
                endpoints.MapGet("api/{call}", async context => {
                    var call = context.Request.RouteValues["call"];
                    await HandleAPICall($"{call}", context);
                });
                endpoints.MapGet("callback/{service}", async context => {
                    var service = context.Request.RouteValues["service"];
                    await HandleServiceCallback($"{service}", context);
                });
            });
        }
    }
}
