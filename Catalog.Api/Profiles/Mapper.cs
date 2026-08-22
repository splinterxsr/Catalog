using Catalog.Api.Domain.Entities;
using Catalog.Api.Models;
using Riok.Mapperly.Abstractions;

namespace Catalog.Api.Profiles
{
    [Mapper]
    public partial class Mapper
    {
        [MapperIgnoreTarget(nameof(Game.Status))]
        public partial Game Map(GameRequest source);

        [MapperIgnoreSource(nameof(UserCatalog.Id))]
        public partial UserCatalogResponse Map(UserCatalog source);
        public partial IEnumerable<UserCatalogResponse> Map(IEnumerable<UserCatalog> source);
    }
}