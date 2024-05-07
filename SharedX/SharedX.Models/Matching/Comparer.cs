namespace SharedX.Core.Matching;
public class OrderBuyComparer : IComparer<OrderEng>
{
    public int Compare(OrderEng? x, OrderEng? y)
    {
        if (x.Price == y.Price) return 0;
        if (x.Price > y.Price) return -1;
        return 1;
    }
}


public class OrderSellComparer : IComparer<OrderEng>
{
    public int Compare(OrderEng? x, OrderEng? y)
    {
        if (x.Price == y.Price) return 0;
        if (x.Price < y.Price) return -1;
        return 1;
    }
}