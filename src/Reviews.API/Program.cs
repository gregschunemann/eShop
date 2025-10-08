using eShop.Reviews.API.Apis;
using eShop.Reviews.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();

var withApiVersioning = builder.Services.AddApiVersioning();

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();

app.MapDefaultEndpoints();

var reviews = app.NewVersionedApi("Reviews");

reviews.MapReviewsApiV1()
       .RequireAuthorization();

app.UseDefaultOpenApi();
app.Run();
