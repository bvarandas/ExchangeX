namespace SharedX.Core.Matching;
public class OrderBuyComparer : IComparer<Order>
{
    public int Compare(Order? x, Order? y)
    {
        if (x.Price == y.Price) return 0;
        if (x.Price > y.Price) return -1;
        return 1;
    }
}


public class OrderSellComparer : IComparer<Order>
{
    public int Compare(Order? x, Order? y)
    {
        if (x.Price == y.Price) return 0;
        if (x.Price < y.Price) return -1;
        return 1;
    }
}