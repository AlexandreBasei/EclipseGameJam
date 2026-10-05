public static class TagExtensions
{
    public static int Count(this Tag tag)
    {
        int value = (int)tag;
        int count = 0;
        while (value != 0)
        {
            value &= value - 1;
            count++;
        }
        return count;
    }
}