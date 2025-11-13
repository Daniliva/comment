using Microsoft.Extensions.DependencyInjection;
using Nest;
using Microsoft.Extensions.Configuration;


namespace Comments.Infrastructure.Extensions
    {

        public static class ElasticsearchExtensions
        {
            public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration config)
            {
                var url = config["Elasticsearch:Url"];
                var index = config["Elasticsearch:Index"];
                var settings = new ConnectionSettings(new Uri(url))
                    .DefaultIndex(index)
                    .EnableApiVersioningHeader();
                var client = new ElasticClient(settings);
                services.AddSingleton<IElasticClient>(client);
                return services;
            }
        }
    }
