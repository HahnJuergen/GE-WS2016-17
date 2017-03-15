﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngelWeapons : MonoBehaviour {

    protected static AngelWeapons _Instance;

    public static AngelWeapons Instance {
        get
        {
            return _Instance;
        }
    }

    public static void SetGlow(float glow)
    {
        if (_Instance == null)
            return;

        if (_Instance.m_CurrentTip != null)
        {
            SetRendererEmission(_Instance.m_CurrentTip, glow);
        }
    }

    public static void SetRendererEmission(GameObject go, float glow)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r != null)
        {
            try
            {
                foreach (Material m in r.materials)
                {
                    Color emissionColor = new Color(glow, glow, glow, 1f);
                    m.SetColor("_EmissionColor", emissionColor);
                    m.SetColor("_EmissionColorUI", emissionColor);
                }
                DynamicGI.UpdateMaterials(r);
            }
            catch { }
        }
    }

    public enum Tips {Axe, Crosswbow, Hammer, Hellebarde, Scythe, Spear, Magic };

    public GameObject m_Axe;
    public GameObject m_Crossbow;
    public GameObject m_Hammer;
    public GameObject m_Hellebarde;
    public GameObject m_Scythe;
    public GameObject m_Spear;

    public ControlAngelVisualisation m_Effects;

    protected GameObject m_CurrentTip;

    protected IEnumerator m_WeaponChangeEnumerator;

    private void Start()
    {
        if (_Instance == null)
            _Instance = this;
    }

    public void RemoveTip()
    {
        m_CurrentTip.SetActive(false);
    }

    public void ChangeTipTo(Tips t, IEnumerator doAfterwards, MonoBehaviour callbackOwner)
    {
        if (t == Tips.Magic)
        {
            if (callbackOwner != null && doAfterwards != null)
                callbackOwner.StartCoroutine(doAfterwards);
            return;
        }

        GameObject tip = null;

        if (t == Tips.Axe)
            tip = m_Axe;
        else if (t == Tips.Crosswbow)
            tip = m_Crossbow;
        else if (t == Tips.Hammer)
            tip = m_Hammer;
        else if (t == Tips.Hellebarde)
            tip = m_Hellebarde;
        else if (t == Tips.Scythe)
            tip = m_Scythe;
        else if (t == Tips.Spear)
            tip = m_Spear;

        if (m_CurrentTip == tip)
        {
            if (callbackOwner != null && doAfterwards != null)
                callbackOwner.StartCoroutine(doAfterwards);
        }
        else
        {
            m_WeaponChangeEnumerator = ChangeTipRoutine(tip, doAfterwards, callbackOwner);
            StartCoroutine(m_WeaponChangeEnumerator);
        }
    }

    protected virtual IEnumerator ChangeTipRoutine(GameObject changeTo, IEnumerator doAfterwards, MonoBehaviour callbackOwner)
    {
        if (m_CurrentTip != null)
            m_CurrentTip.SetActive(false);

        Vector3 prevScale = changeTo.transform.localScale + new Vector3();

        changeTo.SetActive(true);
        SetRendererEmission(changeTo, 1f);
        float t = 0;
        while((t += Time.deltaTime) < 0.8f)
        {
            changeTo.transform.localScale = Vector3.Lerp(Vector3.zero, prevScale, t / 0.8f);
            yield return null;
        }
        changeTo.transform.localScale = prevScale;
        m_CurrentTip = changeTo;

        m_Effects.OnWeaponTipChanged(m_CurrentTip);

        if (callbackOwner != null && doAfterwards != null)
            callbackOwner.StartCoroutine(doAfterwards);
    }

    public void Cancel()
    {

    }

}
