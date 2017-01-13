﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingBehaviour : MonoBehaviour, HitInterject {

    public int m_TimesBlockBeforeParry;
    private int m_BlockCount;

    public float m_MaxBlockTime;

    private BossBlockCallback m_Callback;

    public Animator m_Animator;

    private bool m_Active = false;
    private IEnumerator m_BlockTimer;

    public AudioSource m_BlockAudio;

    public void Activate(BossBlockCallback callback)
    {
        // @todo block stance
        m_Callback = callback;

        m_Active = true;
        m_BlockCount = 1;

        m_BlockTimer = StopBlockingAfter();
        StartCoroutine(m_BlockTimer);
    }

    private IEnumerator StopBlockingAfter()
    {
        yield return new WaitForSeconds(m_MaxBlockTime);

        m_Animator.SetTrigger("BlockWindupTrigger");

        m_Active = false;
        m_Callback.OnBlockingOver();
    }

    public bool OnHit(Damage dmg)
    {
        if (!m_Active)
            return false;

        m_BlockCount++;

        if (m_BlockAudio != null)
            m_BlockAudio.Play();

        if (m_BlockCount > m_TimesBlockBeforeParry)
        {
            m_Animator.SetTrigger("ParryTrigger");
            m_Callback.OnBossParries();
            dmg.OnParryDamage();
        }
        else
        {
            m_Callback.OnBossBlocks();
            dmg.OnBlockDamage();
        }

        if (m_BlockTimer != null)
            StopCoroutine(m_BlockTimer);

        m_BlockTimer = StopBlockingAfter();
        StartCoroutine(m_BlockTimer);

        return true;
    }

    public interface BossBlockCallback
    {
        void OnBossBlocks();
        void OnBossParries();

        void OnBlockingOver();
    }
}
