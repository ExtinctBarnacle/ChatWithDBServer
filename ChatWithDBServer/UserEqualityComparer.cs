namespace ChatWithDBServer {
// компаратор для класса User - чтобы корректно сравнивать объекты пользователей по имени и IP
public sealed class UserEqualityComparer : IEqualityComparer<User>
{
    public bool Equals (User x, User y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.Name, y.Name) && string.Equals(x.IP, y.IP);
    }

    public int GetHashCode(User obj)
    {
        unchecked
        {
            return ((obj.Name != null ? obj.Name.GetHashCode() : 0) * 397) ^ obj.Id;
        }
    }
}
}
