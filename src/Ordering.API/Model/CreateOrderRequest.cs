using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.API.Model;

public class CreateOrderRequest
{
    [Required]
    public required string UserName { get; set; }

    [Required]
    public required string City { get; set; }

    [Required]
    public required string Street { get; set; }

    [Required]
    public required string State { get; set; }

    [Required]
    public required string Country { get; set; }

    [Required]
    public required string ZipCode { get; set; }

    [Required]
    public required string CardNumber { get; set; }

    [Required]
    public required string CardHolderName { get; set; }

    [Required]
    public DateTime CardExpiration { get; set; }

    [Required]
    public required string CardSecurityNumber { get; set; }

    [Required]
    public int CardTypeId { get; set; }

    [Required]
    public required IReadOnlyCollection<BasketItem> Items { get; set; }
}
