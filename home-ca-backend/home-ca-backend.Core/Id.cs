namespace home_ca_backend.Core;

public class Id
{
    protected Id()
    { }

    protected Id(Guid guid)
    {
        Guid = guid;
    }
    
    public Guid Guid { get; } = Guid.NewGuid();

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((Id)obj);
    }

    protected bool Equals(Id other)
    {
        return Guid.Equals(other.Guid);
    }

    public override int GetHashCode()
    {
        return Guid.GetHashCode();
    }
}