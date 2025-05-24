using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MeuCatalogo.API.Filters
{
    public class LowercaseDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths.ToList();
            swaggerDoc.Paths.Clear();

            foreach (var path in paths)
            {
                var key = path.Key.ToLowerInvariant();
                swaggerDoc.Paths.Add(key, path.Value);
            }
        }
    }
}