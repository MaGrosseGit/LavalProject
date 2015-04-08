using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[RequireComponent(typeof(GazeAwareComponent))]
[RequireComponent(typeof(GazePointDataComponent))]
[RequireComponent(typeof(MicControlC))]
public class GazeAwareScript : MonoBehaviour {

    public Canvas radialLoader;
    public float fillSpeed = 5;
    public float gazeRadius = 5;
    [Range(0, 100)]
    public float minLoudness;
    public float dynFluidMultiplier = 14;
    public FluidDynamics dynFluid;
    public FollowPath pathToFollow;

    [HideInInspector]
    public float loudness = 0;

    bool gazing = false;
    [HideInInspector]
    public bool taskDone = false;
    bool microCheck = false;
    Image radialImg;
    Canvas radialLoaderTemp;

    GazePointDataComponent gazePoint;
    GazeAwareComponent gazeAware;

    Animator anim;

    MicControlC microphone;

	void Start () {
        anim = GetComponent<Animator>();
        gazeAware = GetComponent<GazeAwareComponent>();
        gazeAware.delayInMilliseconds = 0;
        gazePoint = GetComponent<GazePointDataComponent>();

        microphone = GetComponent<MicControlC>();
        microphone.ableToHearMic = false;
        microphone.micControl = MicControlC.micActivation.ConstantSpeak;
        microphone.enabled = false;
        GetComponent<AudioSource>().enabled = false;
        pathToFollow.enabled = false;
        transform.position = pathToFollow.Path.Points[0].transform.position;
	}

	void Update () {

        if (gazeAware.HasGaze && !gazing && !taskDone)
        {
            gazing = true;
            radialLoaderTemp = Instantiate(radialLoader, transform.position + Vector3.up - Vector3.forward, Quaternion.identity) as Canvas;
            radialImg = radialLoaderTemp.transform.GetChild(0).GetComponent<Image>();
            radialImg.fillAmount = 0;
            microphone.enabled = true;
            GetComponent<AudioSource>().enabled = true;


            //renderer.material.color = Color.yellow; //<----------------------------------------------
        }

        if (gazing)
        {
            var lastGazePoint = gazePoint.LastGazePoint;
            if (lastGazePoint.IsValid)
            {
                Vector3 distToCamV = transform.position - Camera.main.transform.position;
                float distToCamF = Vector3.Dot(distToCamV, Camera.main.transform.forward);
                Vector3 gazePos = Camera.main.ScreenToWorldPoint(new Vector3(lastGazePoint.Screen.x, lastGazePoint.Screen.y, distToCamF));
                gazePos = new Vector3(gazePos.x, gazePos.y, transform.position.z);
                if (Vector3.Distance((transform.position + (Vector3.up * (gazeRadius / 2))), gazePos) < gazeRadius)
                {
                    radialImg.fillAmount = Mathf.Lerp(radialImg.fillAmount, 1.1f, Time.deltaTime * fillSpeed);
                    loudness = microphone.loudness;
                    if (loudness >= minLoudness)
                    {
                        microCheck = true;
                        dynFluid.windMultiplier = dynFluidMultiplier;
                    }
                }
                else
                {
                    gazing = false;
                    radialImg.fillAmount = 0;
                    Destroy(radialLoaderTemp.gameObject);
                    microphone.enabled = false;
                    GetComponent<AudioSource>().enabled = false;
                    microCheck = false;

                    //renderer.material.color = Color.red; //<----------------------------------------------
                }
                if (radialImg.fillAmount >= 1 && microCheck)
                {
                    taskDone = true;
                    gazing = false;
                    Destroy(radialLoaderTemp.gameObject);
                    microphone.enabled = false;
                    GetComponent<AudioSource>().enabled = false;
                    anim.SetBool("Walk", true);
                    pathToFollow.enabled = true;
                    transform.localPosition = Vector3.zero;

                    //renderer.material.color = Color.blue; //<----------------------------------------------
                }
            }
        }
	}

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position +(Vector3.up*(gazeRadius/2)), gazeRadius);
    }
}
