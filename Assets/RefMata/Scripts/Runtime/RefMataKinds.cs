namespace RefMata
{
    [System.Flags]
    public enum RefMataKinds
    {
        Me = 1 << 0,
        Child = 1 << 1,
        Parent = 1 << 2,
        Load = 1 << 3,
    }
}
