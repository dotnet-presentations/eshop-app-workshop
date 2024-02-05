using System.ComponentModel.DataAnnotations;

namespace eShop.Catalog.Data;

public class CatalogType
{
    public int Id { get; set; }

    [Required]
    public required string Type { get; set; }
}
