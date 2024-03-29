﻿using UnityEngine;

public static class RadiusCreator
{
    public static void DrawCircle(this GameObject container, float radius, float lineWidth)
    {
        var segments = 360;
        
        var line = container.GetComponent<LineRenderer>();
        if(line == null)
             line = container.AddComponent<LineRenderer>();
        
        line.useWorldSpace = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = segments + 1;
        
        var pointCount = segments + 1; // add extra point to make startpoint and endpoint the same to close the circle
                var points = new Vector3[pointCount];
        
        for (int i = 0; i < pointCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * 360f / segments);
            points[i] = new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
        }
        
        line.SetPositions(points);
    }
    
    public static void DeleteCircle(this GameObject container)
    {
        var line = container.GetComponent<LineRenderer>();
        line.Reset();
    }
    
    public static void Reset(this LineRenderer lr)
    {
        lr.positionCount = 0;
    }
}