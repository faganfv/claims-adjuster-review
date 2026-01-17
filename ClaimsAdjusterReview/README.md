# Claims Adjuster Review API

Amazon MQ OpenWire event subscription API with AWS Secrets Manager integration.

## Configuration

### AWS Setup

The application uses AWS Secrets Manager to retrieve MQ credentials. AWS credentials are automatically sourced from:
- `~/.aws/credentials` file
- Environment variables: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`
- IAM role (if running on EC2/Lambda)

### Secrets Manager Secret Format

The secret stored in AWS Secrets Manager should be in JSON format:

```json
{
  "username": "your-mq-username",
  "password": "your-mq-password"
}
```

### Updating Secret Name and Region

To use a different AWS region or secret name, update the constants in `Services/SecretsManagerService.cs`:

```csharp
private const string AwsRegion = "us-east-1"; // Change to your region
private const string SecretName = "mq-credentials"; // Change to your secret name
```

## Running the Application

```bash
dotnet run
```

Health check endpoint: `GET http://localhost:5000/api/health/status`

## Files

- `Program.cs` - Application startup and configuration
- `MQMessageSubscriber.cs` - Amazon MQ message subscription service
- `Controllers/HealthController.cs` - Health check endpoint
- `Services/SecretsManagerService.cs` - AWS Secrets Manager integration
- `appsettings.json` - Application configuration
- `appsettings.Development.json` - Development environment configuration
