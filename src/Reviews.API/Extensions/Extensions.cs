using FluentValidation;

namespace eShop.Reviews.API.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        
        builder.AddDefaultAuthentication();

        services.AddDbContext<ReviewsContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("reviewsdb"));
        });
        builder.EnrichNpgsqlDbContext<ReviewsContext>();

        services.AddMigration<ReviewsContext, ReviewsContextSeed>();

        builder.AddRabbitMqEventBus("eventbus");

        services.AddHttpContextAccessor();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining(typeof(Program));

            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidatorBehavior<,>));
        });

        services.AddValidatorsFromAssemblyContaining<CreateReviewCommandValidator>();

        services.AddScoped<IReviewQueries, ReviewQueries>();
    }
}
