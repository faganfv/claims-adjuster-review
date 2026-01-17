using ClaimsAdjusterReview.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<ISecretsManagerService, SecretsManagerService>();
builder.Services.AddSingleton<MQMessageSubscriber>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var subscriber = app.Services.GetRequiredService<MQMessageSubscriber>();
await subscriber.StartAsync();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    subscriber.Stop();
});

app.Run();