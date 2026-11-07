using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public struct Point : IEquatable<Point>
{
    public int x;
    public int y;
    public Point(int x, int y)
    {
        // 保留原始坐标，由棋盘规则校验坐标范围
        this.x = x;
        this.y = y;
    }
    public override string ToString()
    {
        return $"({x}, {y})";
    }

    public bool Equals(Point other)
    {
        return x == other.x && y == other.y;
    }

    public override bool Equals(object obj)
    {
        return obj is Point other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (x * 397) ^ y;
        }
    }

    public static bool operator ==(Point left, Point right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Point left, Point right)
    {
        return !left.Equals(right);
    }
}