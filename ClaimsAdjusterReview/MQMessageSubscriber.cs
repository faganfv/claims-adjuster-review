using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;
using ClaimsAdjusterReview.Services;

/// <summary>
/// Service for subscribing to Amazon MQ OpenWire events
/// </summary>
public class MQMessageSubscriber
{
    private readonly ILogger<MQMessageSubscriber> _logger;
    private readonly ISecretsManagerService _secretsManagerService;
    private IConnection? _connection;
    private Apache.NMS.ISession? _session;
    private IMessageConsumer? _consumer;

    private const string MqBrokerUri = "ssl://b-be37f18c-2e84-4056-b8c6-98dc2864c531-1.mq.us-east-1.amazonaws.com:61617";
    private const string TopicName = "claim.created.topic";

    public MQMessageSubscriber(ILogger<MQMessageSubscriber> logger, ISecretsManagerService secretsManagerService)
    {
        _logger = logger;
        _secretsManagerService = secretsManagerService;
    }

    public async Task StartAsync()
    {
        try
        {
            _logger.LogInformation("Initializing Amazon MQ OpenWire subscription...");

            var (mqUsername, mqPassword) = await _secretsManagerService.GetMqCredentialsAsync();

            var uri = new Uri(MqBrokerUri);
            var factory = new Apache.NMS.ActiveMQ.ConnectionFactory(uri)
            {
                UserName = mqUsername,
                Password = mqPassword,
                ClientId = "ClaimsAdjusterReview-Subscriber"
            };

            _connection = factory.CreateConnection();
            _connection.Start();

            _logger.LogInformation("Connected to Amazon MQ broker at {BrokerUri}", MqBrokerUri);

            _session = _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

            IDestination topic = new ActiveMQTopic(TopicName);

            _consumer = _session.CreateConsumer(topic);
            _consumer.Listener += OnMessageReceived;

            _logger.LogInformation("Successfully subscribed to topic: {TopicName}", TopicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MQ subscription");
            throw;
        }
    }

    private void OnMessageReceived(IMessage message)
    {
        try
        {
            if (message is ITextMessage textMessage)
            {
                _logger.LogInformation("Message received on {TopicName}: {MessageContent}", 
                    TopicName, textMessage.Text);

                ProcessClaimCreatedEvent(textMessage.Text);
            }
            else
            {
                _logger.LogWarning("Received non-text message of type {MessageType}", message.GetType().Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
        }
    }

    private void ProcessClaimCreatedEvent(string messageContent)
    {
        // TODO: Implement claim creation event processing logic
        _logger.LogInformation("Processing claim.created event: {MessageContent}", messageContent);
    }

    public void Stop()
    {
        try
        {
            _consumer?.Dispose();
            _session?.Close();
            _connection?.Close();
            _connection?.Dispose();
            _logger.LogInformation("MQ subscription stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping MQ subscription");
        }
    }
}
