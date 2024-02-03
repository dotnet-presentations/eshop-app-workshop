using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.Data;

public class Buyer
{
    public int Id { get; set; }

    [Required]
    public required string IdentityGuid { get; set; }

    [Required]
    public required string Name { get; set; }

    public virtual ICollection<PaymentMethod> PaymentMethods { get; set; } = [];
}
