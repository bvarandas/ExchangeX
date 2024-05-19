namespace SharedX.Core.Matching;
public class OrderBuyComparer : IComparer<Book>
{
    public int Compare(Book? x, Book? y)
    {
        if (x.Price == y.Price) return 0;
        if (x.Price > y.Price) return -1;
        return 1;
    }
}

public class OrderSellComparer : IComparer<Book>
{
    public int Compare(Book? x, Book? y)
    {
        if (x.Price == y.Price) return 0;
        if (x.Price < y.Price) return -1;
        return 1;
    }
}