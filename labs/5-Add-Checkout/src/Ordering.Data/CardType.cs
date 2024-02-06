namespace eShop.Ordering.Data;

public class CardType : Enumeration
{
    public static readonly CardType Amex = new(1, nameof(Amex));
    public static readonly CardType Visa = new(2, nameof(Visa));
    public static readonly CardType MasterCard = new(3, nameof(MasterCard));

    private CardType(int id, string name) : base(id, name) { }
}
