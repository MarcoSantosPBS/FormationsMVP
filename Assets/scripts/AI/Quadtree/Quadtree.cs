using System.Collections.Generic;
using UnityEngine;

public class Quadtree<T>
{
    public int maxCapacity;
    public bool wasSubdivided;
    public Rect bound;
    public Quadtree<T> nordeste, noroeste, sudeste, sudoeste;
    public List<(Rect area, T obj)> objects;

    public Quadtree(int maxCapacity, Rect bound)
    {
        this.maxCapacity = maxCapacity;
        this.bound = bound;
        wasSubdivided = false;
        objects = new List<(Rect area, T obj)>();
    }

    public bool Insert(Rect area, T obj)
    {
        if (!bound.Overlaps(area)) { return false; }

        if (maxCapacity > objects.Count)
        {
            objects.Add((area, obj));
            return true;
        }

        if (!wasSubdivided) Subdivide();

        if (nordeste.Insert(area, obj)) { return true; }
        if (noroeste.Insert(area, obj)) { return true; }
        if (sudeste.Insert(area, obj)) { return true; }
        if (sudoeste.Insert(area, obj)) { return true; }

        return false;
    }

    public List<T> Search(Rect areaToSearch)
    {
        List<T> result = new List<T>();

        if (!bound.Overlaps(areaToSearch)) 
        {
            return result; 
        }

        foreach (var (area, obj) in objects)
        {
            if (areaToSearch.Overlaps(area))
            {
                result.Add(obj);
            }
        }

        if (!wasSubdivided) return result;

        result.AddRange(nordeste.Search(areaToSearch));
        result.AddRange(noroeste.Search(areaToSearch));
        result.AddRange(sudeste.Search(areaToSearch));
        result.AddRange(sudoeste.Search(areaToSearch));

        return result;
    }

    private void Subdivide()
    {
        float x = bound.x;
        float y = bound.y;
        float width = bound.width / 2;
        float height = bound.height / 2;

        nordeste = new Quadtree<T>(maxCapacity, new Rect(x + width, y, width, height));
        noroeste = new Quadtree<T>(maxCapacity, new Rect(x, y, width, height));
        sudeste = new Quadtree<T>(maxCapacity, new Rect(x + width, y + height, width, height));
        sudoeste = new Quadtree<T>(maxCapacity, new Rect(x, y + height, width, height));

        wasSubdivided = true;
    }
}
