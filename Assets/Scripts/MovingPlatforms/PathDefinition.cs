using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PathDefinition : MonoBehaviour {

    public GameObject pointsGroup;

    public bool AtStartingPoint { get; private set; }
    public bool RestartingPoint { get; private set; }
    public bool AtEndPoint { get; private set; }

    private bool ReachedEndPoint = false;
    private Transform[] Points;

    public void Awake(){
        Points = new Transform[pointsGroup.transform.childCount];
        for (int i = 0; i < pointsGroup.transform.childCount; i++)
        {
            Points[i] = pointsGroup.transform.GetChild(i);
        }
         AtStartingPoint = true;
        AtEndPoint = false;
        RestartingPoint = false;
    }

    public IEnumerator<Transform> GetPathEnumerator()
    {
        if (Points == null || Points.Length < 1)
            yield break;

        var direction = 1;
        var index = 0;
        while (true)
        {
            yield return Points[index];

            if (index <= 0)
                direction = 1;
            else if (index >= Points.Length - 1)
                direction = -1;

            if (index == 1 && !AtStartingPoint && ReachedEndPoint)
                RestartingPoint = true;
            else if (index == Points.Length - 2)
            {
                AtEndPoint = true;
                ReachedEndPoint = true;
            } 
            else
            {
                AtStartingPoint = false;
                AtEndPoint = false;
                RestartingPoint = false;
            }

            index += direction;
        }
    }

    public void OnDrawGizmos()
    {
        if (Points == null || Points.Length < 2)
            return;

        var points = Points.Where(t => t != null).ToList();
        if (points.Count < 2)
            return;

        for (var i = 1; i < points.Count; i++)
        {
            Gizmos.DrawLine(points[i - 1].position, points[i].position);
        }
    }
    
}
