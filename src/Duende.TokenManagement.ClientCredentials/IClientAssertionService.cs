using System.Threading.Tasks;
using IdentityModel.Client;

namespace Duende.TokenManagement.ClientCredentials;

/// <summary>
/// Service to create client assertions for back-channel clients
/// </summary>
public interface IClientAssertionService
{
    /// <summary>
    /// Creates a client assertion based on client or configuration scheme (if present)
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="endpoint"></param>
    /// <param name="configurationScheme"></param>
    /// <param name="clientId"></param>
    /// <returns></returns>
    Task<ClientAssertion?> GetClientAssertionAsync(string clientName, string clientId, string endpoint, string? configurationScheme = null);
}