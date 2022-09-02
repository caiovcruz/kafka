using Kafka.Consumer;
using Kafka.Producer;
using ServiceHttpEcommerce.Configurations;
using ServiceNewOrder;
using ServiceReports;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IKafkaDispatcher, KafkaDispatcher>();
builder.Services.AddScoped<IKafkaService, KafkaService>();
builder.Services.AddScoped<INewOrderService, NewOrderService>();
builder.Services.AddScoped<IOrdersDatabase, OrdersDatabase>();
builder.Services.AddScoped<IGenerateAllUsersReportsService, GenerateAllUsersReportsService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterEndpoints();

app.Run();