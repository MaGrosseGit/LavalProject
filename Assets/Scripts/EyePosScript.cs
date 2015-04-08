using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GazePointDataComponent))]
[RequireComponent(typeof(EyePositionDataComponent))]
public class EyePosScript : MonoBehaviour {

    public float moveSpeed = 1000;
    public static Vector3 eyesPos;
    public static Vector2 eyesPosScreen;
    public static Vector2 realEyesPos;
    public Canvas crossHair;
    public float crossHairZval = 0;
    public int averageFramesNb = 40;
    public bool useLerp = false;
    public bool getDistance = false;
    public float distance = 0;
    public float distanceSpeed = 7f;
    public bool getRealVal = false;

    GazePointDataComponent gazePoint;
    EyePositionDataComponent eyesPosData;

    Vector3 tempEyesPos;
    Vector2 tempEyesPos2D;

    List<Vector2> eyesPos2D = new List<Vector2>();
    List<Vector3> eyesPos3D = new List<Vector3>();
    List<float> zVal = new List<float>();
    int curIndex = 0;

    GameObject distanceObj;

	void Start () {
        gazePoint = GetComponent<GazePointDataComponent>();
            eyesPosData = GetComponent<EyePositionDataComponent>();
        crossHair = Instantiate(crossHair, transform.position, Quaternion.identity) as Canvas;
        crossHair.transform.parent = Camera.main.transform;

        for (int i = 0; i < averageFramesNb; i++)
        {
            eyesPos2D.Add(Vector2.zero);
            eyesPos3D.Add(Vector3.zero);
        }
        if (getDistance)
        {
            eyesPosData.fixationDataMode = Tobii.EyeX.Framework.FixationDataMode.Sensitive;
            for (int i = 0; i < averageFramesNb; i++)
            {
                zVal.Add(0f);
            }
            distanceObj = new GameObject();
            distanceObj.transform.position = Vector3.zero;
        }
	}
	
	void Update () {
        var lastGazePoint = gazePoint.LastGazePoint;
        if (lastGazePoint.IsValid)
        {
            Vector3 distToCamV = transform.position - Camera.main.transform.position;
            float distToCamF = Vector3.Dot(distToCamV, Camera.main.transform.forward);
            tempEyesPos2D = new Vector2(lastGazePoint.Screen.x, lastGazePoint.Screen.y);
            tempEyesPos = Camera.main.ScreenToWorldPoint(new Vector3(lastGazePoint.Screen.x, lastGazePoint.Screen.y, distToCamF));
            realEyesPos = new Vector2(lastGazePoint.Screen.x, lastGazePoint.Screen.y);
            eyesPos2D[curIndex] = tempEyesPos2D;
            eyesPos3D[curIndex] = tempEyesPos;

            Vector2 avg2D = Vector2.zero;
            Vector3 avg3D = Vector3.zero;
            for (int i = 0; i < eyesPos2D.Count; i++)
            {
                avg2D += eyesPos2D[i];
                avg3D += eyesPos3D[i];
            }
            avg2D /= eyesPos2D.Count;
            avg3D /= eyesPos3D.Count;

            Vector3 crosshairPos = new Vector3(avg3D.x, avg3D.y, Camera.main.transform.position.z + crossHairZval);

            eyesPos = (useLerp)? Vector3.Lerp(crossHair.transform.position, avg3D, Time.deltaTime * moveSpeed) : avg3D;
            eyesPosScreen = (useLerp)? Vector2.Lerp(crossHair.transform.position, avg2D, Time.deltaTime * moveSpeed) : avg2D;

            crossHair.transform.position = (useLerp) ? Vector3.Lerp(crossHair.transform.position, crosshairPos, Time.deltaTime * moveSpeed) : crosshairPos;
            crosshairPos = new Vector3(tempEyesPos.x, tempEyesPos.y, Camera.main.transform.position.z + crossHairZval);
            crossHair.transform.position = (getRealVal) ? crosshairPos : crossHair.transform.position;

            curIndex++;
            if (curIndex >= eyesPos2D.Count)
                curIndex = 0;
        }

        if (getDistance)
        {
            if (eyesPosData != null)
            {
                var lastEyePosition = eyesPosData.LastEyePosition;
                if (lastEyePosition.IsValid)
                {
                    float curZ = Remap(lastEyePosition.LeftEye.Z, 100, 1000, 0, 1);
                    zVal[curIndex] = curZ;

                    float avgZ = 0;
                    for (int i = 0; i < zVal.Count; i++)
                    {
                        avgZ += zVal[i];
                    }
                    avgZ /= zVal.Count;

                    distance = Mathf.Lerp(distanceObj.transform.position.z, avgZ, Time.deltaTime * distanceSpeed);
                    distanceObj.transform.position = new Vector3(0, 0, distance);

                    curIndex++;
                    if (curIndex >= zVal.Count)
                        curIndex = 0;
                }
            }
        }
	}

    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
