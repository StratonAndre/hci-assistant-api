using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using HCI.AIAssistant.API.Managers;
using HCI.AIAssistant.API.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CORS",
    policy =>
    {
        policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin();
    });
});


// Replace appsettings.json values with Key Vault values
var keyVaultName = builder.Configuration[$"AppConfigurations{ConfigurationPath.KeyDelimiter}KeyVaultName"];
var secretsPrefix = builder.Configuration[$"AppConfigurations{ConfigurationPath.KeyDelimiter}SecretsPrefix"];

if (string.IsNullOrWhiteSpace(keyVaultName))
{
    throw new ArgumentNullException("KeyVaultName", "KeyVaultName is missing.");
}

if (string.IsNullOrWhiteSpace(secretsPrefix))
{
    throw new ArgumentNullException("SecretsPrefix", "SecretsPrefix is missing.");
}

var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

builder.Configuration.AddAzureKeyVault(
    keyVaultUri,
    new DefaultAzureCredential(),
    new CustomSecretManager(secretsPrefix)
);

// Configure values based on appsettings.json
builder.Services.Configure<SecretsService>(builder.Configuration.GetSection("Secrets"));
builder.Services.Configure<AppConfigurationsService>(builder.Configuration.GetSection("AppConfigurations"));

// Add services to the container.
builder.Services.AddSingleton<ISecretsService>(
    provider => provider.GetRequiredService<IOptions<SecretsService>>().Value
);

builder.Services.AddSingleton<IAppConfigurationsService>(
    provider => provider.GetRequiredService<IOptions<AppConfigurationsService>>().Value
);

builder.Services.AddSingleton<IParametricFunctions, ParametricFunctions>();

// TODO: Add AIAssistantService at step 29 after creating the service classes
// builder.Services.AddSingleton<IAIAssistantService, AIAssistantService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("CORS");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Test output to verify secrets loading
Console.WriteLine("=== Testing Secret Loading ===");
Console.WriteLine($"AI Assistant Endpoint: {app.Services.GetService<ISecretsService>()?.AIAssistantSecrets?.EndPoint}");
Console.WriteLine($"AI Assistant Key: {app.Services.GetService<ISecretsService>()?.AIAssistantSecrets?.Key}");
Console.WriteLine($"AI Assistant Id: {app.Services.GetService<ISecretsService>()?.AIAssistantSecrets?.Id}");
Console.WriteLine($"IoT Hub Connection String: {app.Services.GetService<ISecretsService>()?.IoTHubSecrets?.ConnectionString}");
Console.WriteLine($"Key Vault Name: {app.Services.GetService<IAppConfigurationsService>()?.KeyVaultName}");
Console.WriteLine($"Secrets Prefix: {app.Services.GetService<IAppConfigurationsService>()?.SecretsPrefix}");
Console.WriteLine($"IoT Device Name: {app.Services.GetService<IAppConfigurationsService>()?.IoTDeviceName}");
Console.WriteLine("=== End Testing ===");

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();