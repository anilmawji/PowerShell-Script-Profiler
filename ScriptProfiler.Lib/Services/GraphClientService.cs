﻿using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;

namespace ScriptProfiler.Lib.Services;

public sealed class GraphClientService : IGraphClientService
{
    public GraphServiceClient Client { get; set; }

    public GraphClientService(IAccessTokenProvider tokenProvider)
    {
        BaseBearerTokenAuthenticationProvider authenticationProvider = new(tokenProvider);
        Client = new GraphServiceClient(authenticationProvider);
    }

    public async Task<User?> GetUserAsync()
    {
        try
        {
            return await Client.Me.GetAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
