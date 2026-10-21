using Elastic.Clients.Elasticsearch;
using Catalog.Api.Models;
using Catalog.Api.Domain.Services;

namespace Catalog.Api.Infrastructure.Services
{
    public class GameSearchService : IGameSearchService
    {
        private readonly ElasticsearchClient _elasticClient;
        private const string IndexName = "games";

        public GameSearchService(ElasticsearchClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task IndexOrUpdateGameAsync(GameSearchModel game)
        {
            // Insere se não existe, atualiza se já existe
            var response = await _elasticClient.IndexAsync(game, idx => idx.Index(IndexName).Id(game.Id));

            if (!response.IsValidResponse) throw new Exception($"Erro ao indexar o jogo no Elasticsearch: {response.DebugInformation}");
        }

        public async Task DeleteGameAsync(string id)
        {
            var response = await _elasticClient.DeleteAsync<GameSearchModel>(id, idx => idx.Index(IndexName));

            if (!response.IsValidResponse) throw new Exception($"Erro ao remover o jogo do Elasticsearch: {response.DebugInformation}");
        }

        public async Task<IEnumerable<GameSearchModel>> SearchAsync(string query, CancellationToken cancellationToken)
        {
            // Verifica se o índice já existe no Elasticsearch antes de consultar
            var indexExistsResponse = await _elasticClient.Indices.ExistsAsync(IndexName, cancellationToken);
            if (!indexExistsResponse.Exists) return [];

            // Realiza uma busca textual tolerante a erros (Fuzzy Search) nos campos Name, Description e Publisher
            var searchResponse = await _elasticClient.SearchAsync<GameSearchModel>(s => s
                .Indices(IndexName)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(query)
                        .Fields(new[] { "name", "description", "publisher" })
                        .Fuzziness(new Fuzziness("AUTO"))
                    )
                ), cancellationToken);

            if (!searchResponse.IsValidResponse) throw new Exception($"Erro ao realizar busca no Elasticsearch: {searchResponse.DebugInformation}");

            return searchResponse.Documents;
        }
    }
}