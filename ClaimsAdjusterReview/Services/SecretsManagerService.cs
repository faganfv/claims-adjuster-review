using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

namespace ClaimsAdjusterReview.Services;

/// <summary>
/// Service for retrieving credentials from AWS Secrets Manager
/// </summary>
public interface ISecretsManagerService
{
    Task<(string Username, string Password)> GetMqCredentialsAsync();
}

public class SecretsManagerService : ISecretsManagerService
{
    private readonly ILogger<SecretsManagerService> _logger;

    private const string AwsRegion = "us-east-1";
    private const string SecretName = "dev/ClaimsDemo/ActiveMQ"; // Update with your secret name

    public SecretsManagerService(ILogger<SecretsManagerService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves MQ credentials from AWS Secrets Manager
    /// Expected secret format: { "username": "value", "password": "value" }
    /// Uses credentials from ~/.aws/credentials
    /// </summary>
    public async Task<(string Username, string Password)> GetMqCredentialsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching MQ credentials from AWS Secrets Manager: {SecretName}", SecretName);

            using (var client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(AwsRegion)))
            {
                var request = new GetSecretValueRequest
                {
                    SecretId = SecretName
                };

                var response = await client.GetSecretValueAsync(request);

                if (!string.IsNullOrEmpty(response.SecretString))
                {
                    var secretJson = JsonSerializer.Deserialize<JsonElement>(response.SecretString);
                    
                    var username = secretJson.GetProperty("MQ_USERNAME").GetString();
                    var password = secretJson.GetProperty("MQ_PASSWORD").GetString();

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        throw new InvalidOperationException("Username or password is missing from AWS Secrets Manager");
                    }

                    _logger.LogInformation("Successfully retrieved credentials from AWS Secrets Manager");
                    return (username, password);
                }

                throw new InvalidOperationException("Secret does not contain a string value");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve credentials from AWS Secrets Manager");
            throw;
        }
    }
}
