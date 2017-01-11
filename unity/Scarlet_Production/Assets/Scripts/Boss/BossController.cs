﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour, AttackCombo.ComboCallback, BlockingBehaviour.BossBlockCallback, OnHitBehaviour.BossHitCallbacks, HitInterject {

    public GameObject m_Scarlet;

    /// <summary>
    /// 
    /// </summary>
    public AttackCombo[] m_Combos;
    public WerewolfHittable m_BossHittable;

    protected int m_CurrentComboIndex;

    protected IEnumerator m_NextComboTimer;

    protected AttackCombo m_ActiveCombo;

    public BlockingBehaviour m_Blocking;
    public OnHitBehaviour m_TimeWindowManager;

    // Use this for initialization
    protected void Start ()
    {
/*        RegisterComboCallback();

        m_CurrentComboIndex = 0;

        StartCoroutine(StartAfterDelay()); */
    }

    protected void RegisterComboCallback()
    {
        foreach (AttackCombo combo in m_Combos)
        {
            combo.m_Callback = this;
        }
    }

    protected IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (m_Combos.Length > 0)
        {
            m_Combos[0].LaunchCombo();
        }
    }

    public virtual void OnComboStart(AttackCombo combo)
    {
        if (m_ActiveCombo != null)
            combo.CancelCombo();
        else
            m_ActiveCombo = combo;
    }

    public virtual void OnComboEnd(AttackCombo combo)
    {
        m_ActiveCombo = null;

        m_BossHittable.RegisterInterject(this);

        if (m_NextComboTimer != null)
            StopCoroutine(m_NextComboTimer);

        m_NextComboTimer = StartNextComboAfter(combo.m_TimeAfterCombo);
        StartCoroutine(m_NextComboTimer);
    }

    private IEnumerator StartNextComboAfter(float time)
    {
        yield return new WaitForSeconds(time);
        StartNextCombo();
    }

    private void StartNextCombo()
    {
        m_CurrentComboIndex++;
        if (m_CurrentComboIndex >= m_Combos.Length)
            m_CurrentComboIndex = 0;

        m_Combos[m_CurrentComboIndex].LaunchCombo();
    }

    public virtual void OnActivateBlock(AttackCombo combo)
    {
        m_Blocking.Activate(this);
        m_BossHittable.RegisterInterject(m_Blocking);
    }

    public virtual void OnActivateParry(AttackCombo combo)
    {
        // @todo maybe wait a little longer or do something special?
        OnComboEnd(combo);
    }

    public virtual void OnInterruptCombo(AttackCombo combo)
    {
        if (m_NextComboTimer != null)
            StopCoroutine(m_NextComboTimer);

        m_NextComboTimer = StartNextComboAfter(combo.m_TimeAfterCombo);
        StartCoroutine(m_NextComboTimer);
    }

    public void OnBossBlocks()
    {
    }

    public void OnBossParries()
    {
        StartNextCombo();
    }

    public void OnBlockingOver()
    {
        StartNextCombo();
    }

    public bool OnHit(Damage dmg)
    {
        if (m_TimeWindowManager != null)
        {
            m_TimeWindowManager.Activate(this);
            m_BossHittable.RegisterInterject(m_TimeWindowManager);
        }

        if (m_NextComboTimer != null)
            StopCoroutine(m_NextComboTimer);

        return false;
    }

    public void OnBossTakesDamage()
    {
    }

    public void OnBossStaggered()
    {

    }

    public void OnTimeWindowClosed()
    {
        StartNextCombo();
    }

    public void OnBossStaggerOver()
    {
        StartNextCombo();
    }
}
