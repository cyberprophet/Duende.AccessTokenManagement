﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Duende.AccessTokenManagement;

/// <summary>
/// Delegating handler that injects a client credentials access token into an outgoing request
/// </summary>
public class ClientCredentialsTokenHandler : DelegatingHandler
{
    private readonly IClientCredentialsTokenManagementService _accessTokenManagementService;
    private readonly string _tokenClientName;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="accessTokenManagementService">The Access Token Management Service</param>
    /// <param name="tokenClientName">The name of the token client configuration</param>
    public ClientCredentialsTokenHandler(
        IClientCredentialsTokenManagementService accessTokenManagementService, 
        string tokenClientName)
    {
        _accessTokenManagementService = accessTokenManagementService;
        _tokenClientName = tokenClientName;
    }

    /// <inheritdoc/>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await SetTokenAsync(request, forceRenewal: false, cancellationToken);
        var response = await base.SendAsync(request, cancellationToken);

        // retry if 401
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            response.Dispose();

            await SetTokenAsync(request, forceRenewal: true, cancellationToken);
            return await base.SendAsync(request, cancellationToken);
        }

        return response;
    }

    /// <summary>
    /// Set an access token on the HTTP request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="forceRenewal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task SetTokenAsync(HttpRequestMessage request, bool forceRenewal, CancellationToken cancellationToken)
    {
        var parameters = new TokenRequestParameters
        {
            ForceRenewal = forceRenewal
        };
            
        var token = await _accessTokenManagementService.GetAccessTokenAsync(_tokenClientName, parameters: parameters, cancellationToken: cancellationToken);

        if (!string.IsNullOrWhiteSpace(token.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        }
    }
}