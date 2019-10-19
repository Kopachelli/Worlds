using ProtoBuf;

[ProtoContract]
public struct WorldPosition
{
    public static WorldPosition NoPosition = new WorldPosition(-1, -1);

    [ProtoMember(1)]
    public int Longitude;
    [ProtoMember(2)]
    public int Latitude;

    public WorldPosition(int longitude, int latitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }

    public override string ToString()
    {
        return string.Format("[" + Longitude + "," + Latitude + "]");
    }

    public bool Equals(int longitude, int latitude)
    {
        return (Longitude == longitude) && (Latitude == latitude);
    }

    public bool Equals(WorldPosition p)
    {
        return Equals(p.Longitude, p.Latitude);
    }

    public override bool Equals(object p)
    {
        if (p is WorldPosition position)
            return Equals(position);

        return false;
    }

    public override int GetHashCode()
    {
        int hash = 91 + Longitude.GetHashCode();
        hash = (hash * 7) + Latitude.GetHashCode();

        return hash;
    }

    public static bool operator ==(WorldPosition p1, WorldPosition p2)
    {
        return p1.Equals(p2);
    }

    public static bool operator !=(WorldPosition p1, WorldPosition p2)
    {
        return !p1.Equals(p2);
    }
}
