using Catalog.Api.Domain.Entities;
using Catalog.Api.Models;
using Riok.Mapperly.Abstractions;

namespace Catalog.Api.Profiles
{
    [Mapper]
    public partial class Mapper
    {
        public partial Game Map(GameRequest source);
    }
}
