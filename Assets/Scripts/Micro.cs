using UnityEngine;
using System.Collections;  

public class Micro : MonoBehaviour 
{
	public float sensitivity = 100;
	public float loudness = 0;
	void Start() 
	{
			audio.clip = Microphone.Start(null, true,1, 44100);
			audio.loop = true;
			audio.mute = true;
			audio.Play();
		
	}
	void Update() 
	{
		if(audio.isPlaying)
		{
			loudness = GetAveragedVolume() * sensitivity;
		}
	}
	float GetAveragedVolume()
	{ 
		float[] data = new float[256];
		float a = 0;
		audio.GetOutputData(data,0);
		foreach(float s in data)
		{
			a += Mathf.Abs(s);
		}
		return a*0.000390625f;
	}
}