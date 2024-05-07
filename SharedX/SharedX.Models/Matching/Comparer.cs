namespace SharedX.Core.Matching;
public class OrderBuyComparer : IComparer<OrderEngine.OrderEngine>
{
    public int Compare(OrderEngine.OrderEngine? x, OrderEngine.OrderEngine? y)
    {
        if (x.Price == y.Price) return 0;
        if (x.Price > y.Price) return -1;
        return 1;
    }
}


public class OrderSellComparer : IComparer<OrderEngine.OrderEngine>
{
    public int Compare(OrderEngine.OrderEngine? x, OrderEngine.OrderEngine? y)
    {
        if (x.Price == y.Price) return 0;
        if (x.Price < y.Price) return -1;
        return 1;
    }
}