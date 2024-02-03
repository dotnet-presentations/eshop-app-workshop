using System.ComponentModel.DataAnnotations;

namespace eShop.Ordering.Data;

public class PaymentMethod
{
    public int Id { get; set; }

    [Required]
    public string Alias { get; set; } = "Default";

    [Required]
    public required string CardNumber { get; set; }

    [Required]
    public required string SecurityNumber { get; set; }

    [Required]
    public required string CardHolderName { get; set; }

    [Required]
    public required DateTime Expiration { get; set; }

    public int BuyerId { get; set; }

    public int CardTypeId { get; set;}

    public CardType? CardType { get; set; }

    public bool IsEqualTo(int cardTypeId, string cardNumber, DateTime expiration)
    {
        return CardTypeId == cardTypeId
            && CardNumber == cardNumber
            && Expiration == expiration;
    }
}
