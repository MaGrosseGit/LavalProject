using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowPath : MonoBehaviour {

    public enum FollowType
    {
        MoveToward,
        Lerp
    }

    public enum ActionsAfterGoal
    {
        StopAfterGoal,
        ComeBackAndStop,
        Loop
    }

    public FollowType Type = FollowType.MoveToward;
    public ActionsAfterGoal ActionAfterGoal = ActionsAfterGoal.Loop;
    public PathDefinition Path;
    public float Speed = 1;
    public float MaxDistanceToGoal = .1f;
    public bool targetLookAt = true;

    public IEnumerator<Transform> _currentPoint;

	void Start () {
        if (Path == null)
        {
            Debug.LogError("Path cannot be null", gameObject);
            return;
        }

        _currentPoint = Path.GetPathEnumerator();
        _currentPoint.MoveNext();

        if (_currentPoint.Current == null)
            return;

        transform.position = _currentPoint.Current.position;
	}
	
	void Update () {
        if (_currentPoint == null || _currentPoint.Current == null)
            return;

        if (Type == FollowType.MoveToward)
            transform.position = Vector3.MoveTowards(transform.position, _currentPoint.Current.position, Time.deltaTime * Speed);
        else if (Type == FollowType.Lerp)
            transform.position = Vector3.Lerp(transform.position, _currentPoint.Current.position, Time.deltaTime * Speed);

        if (targetLookAt)
            transform.LookAt(_currentPoint.Current);

        var distanceSquared = (transform.position - _currentPoint.Current.position).sqrMagnitude;
        if (distanceSquared < Mathf.Pow(MaxDistanceToGoal, 2))
        {
            if (ActionAfterGoal == ActionsAfterGoal.StopAfterGoal && Path.AtEndPoint)
                return;
            else if (ActionAfterGoal == ActionsAfterGoal.ComeBackAndStop && Path.RestartingPoint)
                return;
            else
                _currentPoint.MoveNext();
        }
	}
}
