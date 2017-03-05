﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WerewolfRagemodeController : BossController
{

    private BossfightCallbacks m_Callbacks;
    public CharacterHealth m_BossHealth;

    public AttackCombo m_HitCombo;
    public AttackCombo m_LeapCombo;
    public AttackCombo m_ChaseCombo;

    public Animator m_WerewolfAnimator;

    public TurnTowardsScarlet m_TurnTowardsScarlet;

    public int m_TotalAttacks = 30;
    private int m_AttackCount = 0;
    private bool m_CancelAfterComboFinishes = false;

    protected bool m_Killable = false;

    new void Start()
    {
    }

    public void LaunchPhase(BossfightCallbacks callbacks)
    {
        m_NotDeactivated = true;
        m_AttackCount = 0;

        m_BossHittable.RegisterInterject(this);

        m_Callbacks = callbacks;

        m_Combos = new AttackCombo[3];
        m_Combos[0] = m_HitCombo;
        m_Combos[1] = m_LeapCombo;
        m_Combos[2] = m_ChaseCombo;

        base.RegisterComboCallback();

        StartCoroutine(StartAfterDelay());
    }

    private void EndRageMode()
    {
        CancelComboIfActive();
        StartCoroutine(MakeKillable());
    }

    protected IEnumerator MakeKillable()
    {
        m_BossHittable.RegisterInterject(this);
        m_Killable = true;
        yield return null;

        m_BossHealth.m_CurrentHealth = 3f;
    }

    protected void Kill()
    {
        m_WerewolfAnimator.SetTrigger("DeathTrigger");
        m_NotDeactivated = false;
        m_Callbacks.PhaseEnd(this);
    }

    private new IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        m_CancelAfterComboFinishes = false;
        DecideNextCombo(null);
    }

    public override void OnComboEnd(AttackCombo combo)
    {
        IncreaseAttackCount(combo);

        m_ActiveCombo = null;

        MLog.Log(LogType.BattleLog, "On Combo End, Controller");

        if (m_AttackCount >= m_TotalAttacks)
        {
            m_CancelAfterComboFinishes = true;
        }

        if (m_CancelAfterComboFinishes)
        {
            EndRageMode();
        }
        else
        {
            DecideNextCombo(combo);
        }
    }

    private IEnumerator StartNextComboAfter(float time, AttackCombo combo)
    {
        yield return new WaitForSeconds(time);
        combo.LaunchCombo();
    }

    public override bool OnHit(Damage dmg)
    {
        if (m_Killable)
        {
            Kill();
            return false;
        }

        dmg.OnBlockDamage();
        return true;
    }

    private void IncreaseAttackCount(AttackCombo combo)
    {
        if (combo == m_HitCombo)
        {
            m_AttackCount += 2;
        }
        else if (combo == m_LeapCombo)
        {
            m_AttackCount++;
        }
        else if (combo == m_ChaseCombo)
        {
            m_AttackCount++;
        }

        m_AttackCount = Math.Min(m_AttackCount, m_TotalAttacks);

        m_BossHealth.m_CurrentHealth = Mathf.Lerp(m_BossHealth.m_MaxHealth, 0, m_AttackCount / (float) (m_TotalAttacks + 1));
    }

    private void DecideNextCombo(AttackCombo previous)
    {
        AttackCombo newCombo = m_HitCombo;

        float distance = Vector3.Distance(transform.position, m_Scarlet.transform.position);

        if (distance <= 4)
        {
            if (Mathf.Abs(m_TurnTowardsScarlet.CalculateAngleTowardsScarlet()) >= 40)
            {
                newCombo = m_ChaseCombo;
            }
        }
        else
        {
            newCombo = m_LeapCombo;
        }

        StartCoroutine(StartNextComboAfter(0.3f, newCombo));
    }

    public override void OnInterruptCombo(AttackCombo combo)
    {
        OnComboEnd(combo);
    }
}
