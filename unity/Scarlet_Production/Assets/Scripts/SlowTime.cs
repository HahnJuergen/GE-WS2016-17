﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTime : MonoBehaviour {

    private static SlowTime _instance;

    public static SlowTime Instance
    {
        get
        {
            return _instance;
        }
    }

    public float m_SlowAmount;
    float currentSlowMo = 0f;
    
    void Start () {
        _instance = this;
	}
	
    public void StartSlowMo()
    {
        Time.timeScale = m_SlowAmount;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public void StopSlowMo()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}
