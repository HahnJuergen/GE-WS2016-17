﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackPattern : MonoBehaviour, AttackCallbacks
{

    public Attack[] m_Attacks;

    public Attack m_CurrentAttack;

    public GameObject m_PizzaSetupPrefab;
    public GameObject m_PizzaAttackPrefab;

    public GameObject m_BeamPrefab;
    public GameObject m_Boss;

    public GameObject m_ConeSetupPrefab;
    public GameObject m_ConeAttackPrefab;

    public GameObject m_TargetSetupPrefab;
    public GameObject m_TargetAttackPrefab;

    public Material m_HitMaterial;
    public Material m_HighlightMaterial;

    public bool m_CancelOnHit = true;

    private IEnumerator m_ColorChangeEnumerator;
    private IDictionary<Renderer, Material> m_OriginalMaterialDictionary;

    /*
     * Index of m_Attacks that is currently active; only stored so as not to use the same attack twice in a row.
     */
    private int m_CurrentAttackIndex;

    // Use this for initialization
    void Start()
    {
        m_Attacks = new Attack[6];

        m_CurrentAttackIndex = 0;
        StartCoroutine(StartNextAttackAfter(2f));

        InitOriginalColorDictionary();
    }

    void SetupAttack(int index)
    {
        switch(index)
        {
            case 0:
                m_Attacks[index] = new ChaseAttack(this, m_HighlightMaterial);
                break;
            case 1:
                m_Attacks[index] = new ConeAttackSeries(this, m_ConeSetupPrefab, m_ConeAttackPrefab);
                break;
            case 2:
                m_Attacks[index] = new PizzaAttackSeries(this, m_PizzaSetupPrefab, m_PizzaAttackPrefab);
                break;
            case 3:
                m_Attacks[index] = new TargetAttackSeries(this, m_TargetSetupPrefab, m_TargetAttackPrefab);
                break;
            case 4:
                m_Attacks[index] = new BeamAttackSeries(this, m_BeamPrefab, m_Boss);
                break;
            case 5:
                m_Attacks[index] = new DoubleBeamAttackSeries(this, m_BeamPrefab, m_Boss);
                break;
        }

        m_Attacks[index].m_Callbacks = this;
    }

    private IEnumerator StartNextAttackAfter(float time)
    {
        yield return new WaitForSeconds(time);

        SetupAttack(m_CurrentAttackIndex);

        m_Attacks[m_CurrentAttackIndex].StartAttack();
        m_CurrentAttack = m_Attacks[m_CurrentAttackIndex];
    }

    // Update is called once per frame
    void Update()
    {
        if (m_CurrentAttack != null)
        {
            m_CurrentAttack.WhileActive();
        }
    }

    public void OnBossHit()
    {
        if (m_CancelOnHit && m_CurrentAttack != null && m_CurrentAttackIndex != 0) // = any AE attack
        {
            m_Boss.GetComponentInChildren<Animator>().SetTrigger("StaggerTrigger");
            m_CurrentAttack.CancelAttack();
        }

        ColorBossRed();
    }

    void AttackCallbacks.OnAttackStart(Attack a)
    {
        m_CurrentAttack = a;
    }

    void AttackCallbacks.OnAttackEnd(Attack a)
    {
        m_CurrentAttack = null;
        
        if (m_CurrentAttackIndex == 0)
        {
            m_CurrentAttackIndex = (int) Random.Range(1, m_Attacks.Length);
        }
        else
        {
            m_CurrentAttackIndex = 0;
        }

        // m_CurrentAttackIndex = 4;

        StartCoroutine(StartNextAttackAfter(2f));
    }

    void AttackCallbacks.OnAttackParried(Attack a)
    {
        m_Boss.GetComponentInChildren<Animator>().SetTrigger("IdleTrigger");
    }

    void AttackCallbacks.OnAttackCancelled(Attack a)
    {
        if (m_CurrentAttack != null)
        {
            m_CurrentAttack = null;
            StartCoroutine(StaggerThenContinueAttacking(1.5f, a));
        }
    }

    private IEnumerator StaggerThenContinueAttacking(float seconds, Attack a)
    {
        yield return new WaitForSeconds(seconds);

        (this as AttackCallbacks).OnAttackEnd(a);
    }

    private void InitOriginalColorDictionary()
    {
        m_OriginalMaterialDictionary = new System.Collections.Generic.Dictionary<Renderer, Material>();
        foreach (Renderer r in m_Boss.GetComponentsInChildren<Renderer>())
        {
            m_OriginalMaterialDictionary.Add(r, r.material);
        }
    }

    private void ColorBossRed()
    {
        foreach (Renderer r in m_Boss.GetComponentsInChildren<Renderer>())
        {
            r.material = m_HitMaterial;
        }

        if (m_ColorChangeEnumerator != null)
        {
            StopCoroutine(m_ColorChangeEnumerator);
        }
        StartCoroutine(RestoreColors());
    }

    public void HighlightBoss()
    {
        foreach(Renderer r in m_Boss.GetComponentsInChildren<Renderer>())
        {
            r.material = m_HighlightMaterial;
        }
    }

    public IEnumerator RestoreColors()
    {
        yield return new WaitForSeconds(0.3f);

        foreach (Renderer r in m_Boss.GetComponentsInChildren<Renderer>())
        {
            r.material = m_OriginalMaterialDictionary[r];
        }
    } 
}
