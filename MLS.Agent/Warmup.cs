﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using MLS.Protocol;
using MLS.Protocol.Execution;
using Newtonsoft.Json;
using Pocket;
using static Pocket.Logger<MLS.Agent.Warmup>;

namespace MLS.Agent
{
    public class Warmup : HostedService
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:4242")
        };

        protected override async Task ExecuteAsync(Budget budget)
        {
            await WarmUpRoutes();
        }

        private async Task WarmUpRoutes()
        {
            using (var operation = Log.OnEnterAndExit())
            {
                await WarmUpRoute("/workspace/run");
                await WarmUpRoute("/workspace/completion");
                await WarmUpRoute("/workspace/diagnostics");
                await WarmUpRoute("/workspace/signaturehelp");

                async Task WarmUpRoute(string relativeUri)
                {
                    const string code = "Console.WriteLine(42);";

                    var workspaceRequest = new WorkspaceRequest(
                        activeBufferId: "Program.cs",
                        workspace: new Workspace(
                            workspaceType: "console",
                            buffers: new[]
                            {
                                new Workspace.Buffer("Program.cs", code, 0)
                            }));

                    var response = await Post(relativeUri,
                                              workspaceRequest);

                    operation.Info("WarmUp response from {relativeUri} {response}", relativeUri, response);
                }
            }
        }

        private async Task<HttpResponseMessage> Post(string relativeUri, WorkspaceRequest workspaceRequest) =>
            await _httpClient.PostAsync(
                relativeUri,
                new StringContent(
                    JsonConvert.SerializeObject(workspaceRequest), Encoding.UTF8, "application/json"));
    }
}