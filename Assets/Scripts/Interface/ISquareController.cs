using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISquareController 
{
    public void GetPoint(Point point);
    public bool TryMove(Point from, Point destination);
}
