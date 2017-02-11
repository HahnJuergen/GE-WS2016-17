﻿using UnityEngine;
using System.Collections;

public class EnviroAudioSource : MonoBehaviour {

	public enum AudioSourceFunction
	{
		Weather1,
		Weather2,
		Ambient,
		Thunder
	}

	public AudioSourceFunction myFunction;
    public AudioSource audiosrc;

	public bool isFadingIn = false;
	public bool isFadingOut = false;

	public float fadingSpeed = 0.1f;

    private float audioSourceMaxVol;

	void Start ()
	{
        if (audiosrc == null)
        {
            audiosrc = GetComponent<AudioSource>();
        }

        audioSourceMaxVol = audiosrc.volume;

        if (myFunction == AudioSourceFunction.Weather1 || myFunction == AudioSourceFunction.Weather2) 
		{
			audiosrc.loop = true;
			audiosrc.volume = 0f;
		}
	}


	public void FadeOut () 
	{
		isFadingOut = true;
		isFadingIn = false;
	}



	public void FadeIn (AudioClip clip) 
	{
		isFadingIn = true;
		isFadingOut = false;
		audiosrc.clip = clip;
		audiosrc.Play ();
	}


	void Update ()
	{
		if (isFadingIn && audiosrc.volume < audioSourceMaxVol)
		{
			audiosrc.volume += fadingSpeed * Time.deltaTime;
		}
		else if (isFadingIn && audiosrc.volume >= 0.999f)
		{
			isFadingIn = false;
		}

		if (isFadingOut && audiosrc.volume > 0f)
		{
			audiosrc.volume -= fadingSpeed * Time.deltaTime;
		} 
		else if (isFadingOut && audiosrc.volume == 0f)
		{
			isFadingOut = false;
			audiosrc.Stop();
		}
	}
}
