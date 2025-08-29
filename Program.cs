using System.Reflection;
using Amazon.SQS;
using AmazonSQS.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddScoped<IMessageQueueService, MessageQueueService>();

builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var awsAccessKey = Environment.GetEnvironmentVariable("AWS__AccessKey") ?? "test";
    var awsSecretKey = Environment.GetEnvironmentVariable("AWS__SecretKey") ?? "test";
    var serviceUrl = Environment.GetEnvironmentVariable("AWS__ServiceURL") ?? "http://localstack:4566";

    var config = new AmazonSQSConfig
    {
        ServiceURL = serviceUrl,
        UseHttp = true
    };

    return new AmazonSQSClient(awsAccessKey, awsSecretKey, config);
});

builder.Services.AddHostedService<SqsConsumerService>();

builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Message Queue API V1");
});

app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthorization();
app.MapControllers();

app.Run();