using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tagom.Infrastructure.Persistence;

namespace Tagom.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var cs = config.GetConnectionString("DefaultConnection")
                     ?? "Server=localhost\\SQLEXPRESS;Database=TagomCrm;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            services.AddDbContext<TagomDbContext>(opt => opt.UseSqlServer(cs));
            return services;
        }
    }
}
