using Azure;
using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Cooking.Receipes
{
    public partial class CookingReceipesClient
    {
        private readonly HttpPipeline _pipeline;

        public CookingReceipesClient(CookingReceipesClientOptions options = default)
        {
            if (options == null) options = new CookingReceipesClientOptions();
            _pipeline = HttpPipelineBuilder.Build(options);
        }
    }
}
