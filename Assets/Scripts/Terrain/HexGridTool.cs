using System.Collections;
using System.Collections.Generic;
using System;

public class HexGrid {
    private int radius;
    public int Radius { get { return radius; } }

    HashSet<HexIndex> indexes = new HashSet<HexIndex>();

    public HexGrid(int _radius)
    {
        radius = _radius;
        Generate();
    }

    public HashSet<HexIndex> GetAll()
    {
        return indexes;
    }

    public List<HexIndex> GetCircle(HexIndex _origin, int _radius, bool _filled = false)
    {
        List<HexIndex> objects = new List<HexIndex>();

        for (int x = -_radius; x <= _radius; x++)
        {
            for (int y = Math.Max(-_radius, -x - _radius); y <= Math.Min(_radius, -x + _radius); y++)
            {
                int z = -x - y;
                if (_filled || (!_filled && (Math.Abs(x) == _radius || Math.Abs(y) == _radius || Math.Abs(z) == _radius)))
                {
                    HexIndex index = _origin + new HexIndex(x, y, z);
                    if (indexes.Contains(index))
                    {
                        objects.Add(index);
                    }
                }
            }
        }

        return objects;
    }

    public List<HexIndex> GetRandom()
    {
        List<HexIndex> objects = new List<HexIndex>();
        //System.Random rand = new System.Random(); ;

        objects.AddRange(indexes);
        objects.Shuffle();

        return objects;
    }

    public List<HexIndex> GetRing(HexIndex _origin, int _radius)
    {
        List<HexIndex> objects = new List<HexIndex>();

        if (_radius == 0) // couldn't compute with radius of 1, so add only origin
        {
            if (indexes.Contains(_origin)) // if the tile exists
            {
                objects.Add(_origin);
            }
        }
        else
        {
            HexIndex index = _origin + _radius * HexIndex.direction[4];
            for (int i = 0; i < 6; i++) // increment on sides (it's why there is the 6)
            {
                for (int j = 0; j < _radius; j++) // increment on tile on a side
                {
                    // Got to neighbor in direction i
                    // increase it before to have beautiful spiral
                    index += HexIndex.direction[i];
                    if (indexes.Contains(index)) // if the tile exists
                    {
                        objects.Add(index);
                    }
                }
            }
        }

        return objects;
    }

    public List<HexIndex> GetSpiral(HexIndex _origin, int _radius)
    {
        List<HexIndex> objects = new List<HexIndex>();

        for (int k = 0; k <= _radius; k++)
        {
            objects.AddRange(GetRing(_origin, k));
        }

        return objects;
    }

    public List<HexIndex> GetLine(HexIndex _origin, HexIndex _direction, bool _backward = true, int _distance = -1)
    {
        List<HexIndex> objects = new List<HexIndex>();
        int i = 0;
        bool loop = true;

        while (loop && (_distance < 0 || i <= _distance))
        {
            loop = false;
            if (indexes.Contains(i * _direction + _origin))
            {
                objects.Add(i * _direction + _origin);
                loop = true;
            }

            if (i != 0 && indexes.Contains(-i * _direction + _origin))
            {
                objects.Add(-i * _direction + _origin);
                loop = true;
            }

            i++;
        }

        return objects;
    }

    

    void Generate()
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    if (x + y + z == 0)
                    {
                        indexes.Add(new HexIndex(x, y, z));
                    }
                }
            }
        }
    }

}


public static class Ext
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        System.Random rnd = new System.Random();
        while (n > 1)
        {
            int k = (rnd.Next(0, n) % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}


// Structure of a 2 dimensional index.
public struct HexIndex
{
    public static HexIndex origin = new HexIndex(0, 0, 0);
    public static HexIndex[] direction = new HexIndex[6] {
        new HexIndex(+1, -1, 0),
        new HexIndex(+1, 0, -1),
        new HexIndex(0, +1, -1),
        new HexIndex(-1, +1, 0),
        new HexIndex(-1, 0, +1),
        new HexIndex(0, -1, +1)
    };


    public int x, y, z;
    public HexIndex(int _x, int _y, int _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    static public HexIndex operator +(HexIndex _t1, HexIndex _t2)
    {
        return new HexIndex(_t1.x + _t2.x, _t1.y + _t2.y, _t1.z + _t2.z);
    }

    static public HexIndex operator -(HexIndex _t1, HexIndex _t2)
    {
        return new HexIndex(_t1.x - _t2.x, _t1.y - _t2.y, _t1.z - _t2.z);
    }

    static public HexIndex operator *(int _i, HexIndex _t)
    {
        return new HexIndex(_i * _t.x, _i * _t.y, _i * _t.z);
    }

    static public bool operator ==(HexIndex _t1, HexIndex _t2)
    {
        return (_t1.x == _t2.x && _t1.y == _t2.y && _t1.z == _t2.z);
    }

    public override bool Equals(object _o)
    {
        if (!(_o is HexIndex))
            return false;

        HexIndex t = (HexIndex)_o;
        return (x == t.x && y == t.y && z == t.z);
    }

    static public bool operator !=(HexIndex _t1, HexIndex _t2)
    {
        return (_t1.x != _t2.x || _t1.y != _t2.y || _t1.z != _t2.z);
    }

    static public int Distance(HexIndex _i1, HexIndex _i2)
    {
        return Math.Max(Math.Abs(_i2.x - _i1.x), Math.Max(Math.Abs(_i2.y - _i1.y), Math.Abs(_i2.z - _i1.z)));
    }

}

