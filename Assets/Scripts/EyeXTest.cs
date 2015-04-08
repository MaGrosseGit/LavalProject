using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tobii.EyeX.Framework;

public class EyeXTest : MonoBehaviour {

    public GameObject target;

    FixationDataComponent fixation;

	void Start () {
        fixation = GetComponent<FixationDataComponent>();
        for (int i = 0; i < 100; i++)
        {
            zVal.Add(0f);
        }
	}

    //public float lowestVal = 1000;
    //public float highestVal = 0;

    List<float> zVal = new List<float>();
    int curIndex = 0;
	
	void Update () {
        //if (GetComponent<ActivatableComponent>().HasActivationFocus)
        //{
        //    Debug.Log("The game object has activation focus.");
        //}
        //if (GetComponent<ActivatableComponent>().IsActivated)
        //{
        //    Debug.Log("The game object has been activated.");
        //}

	    // Get the last fixation point.
        //var lastFixation = fixation.LastFixation;
        //if (lastFixation.IsValid)
        //{
        //    if (lastFixation.EventType == FixationDataEventType.Begin)
        //    {
        //        Debug.Log("A fixation started.");
        //    }
        //    else if (lastFixation.EventType == FixationDataEventType.Data)
        //    {
        //        Debug.Log("Fixation data is being retrieved.");
        //    }
        //    else if (lastFixation.EventType == FixationDataEventType.End)
        //    {
        //        Debug.Log("The fixation ended.");
        //    }
        //    // Convert the fixation data to screen space.
        //    var screenSpace = lastFixation.GazePoint.Screen;
        //}

        //Debug.Log(Input.mousePosition);

        // Get the last gaze point.
        var lastGazePoint = GetComponent<GazePointDataComponent>().LastGazePoint;
        if (lastGazePoint.IsValid)
        {
            // Convert the fixation data to screen space.
            //Debug.Log(lastGazePoint.Screen);
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(lastGazePoint.Screen.x, lastGazePoint.Screen.y, 10f));
            var screenSpace = lastGazePoint.Screen;
        }

        // Get the last eye position.
        //var lastEyePosition = GetComponent<EyePositionDataComponent>().LastEyePosition;
        //if (lastEyePosition.IsValid)
        //{
        //    // Get the position of the left eye.
        //    float testZ = Remap(lastEyePosition.LeftEye.Z, 100, 1000, 0, 1);
        //    zVal[curIndex] = testZ * 5;

        //    float avgZ = 0;
        //    for (int i = 0; i < zVal.Count; i++)
        //    {
        //        avgZ += zVal[i];
        //    }
        //    avgZ /= zVal.Count;

        //    var leftEyePosition = new Vector3(0, 0, Mathf.Lerp(target.transform.position.z, avgZ, Time.deltaTime * 7f));
        //    //var leftEyePosition = new Vector3(0, 0, lastEyePosition.LeftEye.Z);
        //    //if (lastEyePosition.LeftEye.Z > highestVal)
        //    //    highestVal = lastEyePosition.LeftEye.Z;
        //    //if (lastEyePosition.LeftEye.Z < lowestVal && lastEyePosition.LeftEye.Z != 0)
        //    //    lowestVal = lastEyePosition.LeftEye.Z;
        //    //Debug.Log(leftEyePosition);
        //    target.transform.position = leftEyePosition;
        //    curIndex++;
        //    if (curIndex >= zVal.Count)
        //        curIndex = 0;
        //}

	}


    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
