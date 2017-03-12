﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTargetRing : MonoBehaviour {

    public Image m_Loadingmage;

    Quaternion rotation;

    void Awake()
    {
        rotation = transform.rotation;
    }

    void LateUpdate()
    {
        transform.rotation = rotation;
    }

    public void UpdateIndicator(float floatcurrentTime)
    {
        m_Loadingmage.fillAmount = Mathf.Lerp(0.0f, 1.0f, floatcurrentTime);
    }
}
