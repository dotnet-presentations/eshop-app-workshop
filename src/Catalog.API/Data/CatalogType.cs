using System.ComponentModel.DataAnnotations;

namespace eShop.Catalog.API.Data;

public class CatalogType
{
    public int Id { get; set; }

    [Required]
    public required string Type { get; set; }
}
