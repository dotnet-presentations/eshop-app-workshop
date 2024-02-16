using System.ComponentModel.DataAnnotations;

namespace eShop.Catalog.Data;

public class CatalogItem
{
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public required string Description { get; set; }

    public decimal Price { get; set; }

    public required string PictureFileName { get; set; }

    public string? PictureUri { get; set; }

    public int CatalogTypeId { get; set; }

    public virtual CatalogType? CatalogType { get; set; }

    public int CatalogBrandId { get; set; }

    public virtual CatalogBrand? CatalogBrand { get; set; }

    /// <summary>
    /// Quantity in stock
    /// </summary>
    public int AvailableStock { get; set; }

    /// <summary>
    /// Available stock at which we should reorder
    /// </summary>
    public int RestockThreshold { get; set; }

    /// <summary>
    /// Maximum number of units that can be in-stock at any time (due to physical/logistical constraints in warehouses)
    /// </summary>
    public int MaxStockThreshold { get; set; }

    /// <summary>
    /// True if item is on reorder
    /// </summary>
    public bool OnReorder { get; set; }
}
