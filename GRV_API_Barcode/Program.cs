using NetBarcode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.MapGet("/", () =>
{
    return Results.Ok("Hello Mario! The princess is in another Castle.");
})
.WithName("Home")
.WithOpenApi();

app.MapGet("api/v1/barcode/{code}", (string code) =>
{
    var barcode = new Barcode(code,true);
    var value = barcode.GetBase64Image();
    return Results.Ok(value);
})
.WithName("GetBarcode")
.WithOpenApi();

app.Run();