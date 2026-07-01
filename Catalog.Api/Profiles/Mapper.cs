using Catalog.Api.Domain.Entities;
using Catalog.Api.Models;
using Riok.Mapperly.Abstractions;

namespace Catalog.Api.Profiles
{
    [Mapper]
    public partial class Mapper
    {
        public partial Game Map(GameRequest source);

        [MapperIgnoreSource(nameof(UserCatalog.Id))]
        [MapperIgnoreSource(nameof(UserCatalog.LogData))]
        public partial UserCatalogResponse Map(UserCatalog source);
        public partial IEnumerable<UserCatalogResponse> Map(IEnumerable<UserCatalog> source);
    }
}