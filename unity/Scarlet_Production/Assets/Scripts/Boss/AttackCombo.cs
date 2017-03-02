﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCombo : MonoBehaviour, BossAttack.AttackCallback {

    public GameObject m_Boss;
    public Animator m_Animator;
    public TurnTowardsScarlet m_FullTurnCommand;
    public BossStaggerCommand m_BossStagger;

    public BossMoveCommand m_MoveCommand;

    public BossAttack[] m_Attacks;

    public float m_TimeAfterCombo;

    public ComboCallback m_Callback;
    protected BossAttack m_CurrentAttack;
    protected int m_CurrentAttackIndex;

    protected bool m_BetweenAttacks;
    protected IEnumerator m_AttackTimer;

    protected IEnumerator m_ParriedTimer;

    protected bool m_Cancelled;
   
	protected virtual void Start ()
    {
		foreach(BossAttack attack in m_Attacks)
        {
            SetupAttack(attack);
        }
    }

    protected virtual void SetupAttack(BossAttack attack)
    {
        attack.m_Boss = m_Boss;
        attack.m_Animator = m_Animator;
        attack.m_Callback = this;
        attack.m_FullTurnCommand = m_FullTurnCommand;
    }

    public virtual void LaunchCombo()
    {
        m_Cancelled = false;

        MLog.Log(LogType.BattleLog, 1, "Launching Combo, Combo, " + this);

        m_Callback.OnComboStart(this);

        if (!m_Cancelled)
        {
            m_CurrentAttackIndex = 0;
            m_Attacks[m_CurrentAttackIndex].StartAttack();
        }
    }

    public virtual void OnAttackStart(BossAttack attack)
    {
        MLog.Log(LogType.BattleLog, 1, "Attack Start, Combo, " + this + " " + attack);

        if (m_CurrentAttack != null || m_Cancelled)
            attack.CancelAttack();
        else
        {
            m_BetweenAttacks = false;
            m_CurrentAttack = attack;
        }
    }

    public virtual void OnAttackEnd(BossAttack attack)
    {
        MLog.Log(LogType.BattleLog, 1, "Attack End, Combo, " + this + " " + attack);

        m_BetweenAttacks = true;
        m_CurrentAttack = null;

        m_CurrentAttackIndex++;
        if (m_CurrentAttackIndex >= m_Attacks.Length)
        {
            m_Callback.OnComboEnd(this);
        }
        else if (!m_Cancelled)
        {
            m_AttackTimer = StartNextAttackAfter(m_Attacks[m_CurrentAttackIndex - 1].m_TimeAfterAttack);
            StartCoroutine(m_AttackTimer);
        }
    }

    protected virtual IEnumerator StartNextAttackAfter(float time)
    {
        yield return new WaitForSeconds(time);

        if (!m_Cancelled)
        {
            if (m_CurrentAttackIndex < m_Attacks.Length)
                m_Attacks[m_CurrentAttackIndex].StartAttack();
        }
    }

    public virtual void OnAttackEndUnsuccessfully(BossAttack attack)
    {
        OnAttackEnd(attack);
    }

    public virtual void OnAttackParried(BossAttack attack)
    {
        MLog.Log(LogType.BattleLog, 1, "Attack was Parried, Combo, " + this);

        m_Callback.OnComboParried(this);

        if (m_CurrentAttack != null)
        {
            m_CurrentAttack.CancelAttack();
        }
        m_BossStagger.DoStagger();
        m_ParriedTimer = WaitAfterParried();
        StartCoroutine(m_ParriedTimer);
    }

    public virtual void CancelCombo()
    {
        MLog.Log(LogType.BattleLog, 1, "Cancelling Combo, Combo, " + this);

        m_Cancelled = true;

        if (m_CurrentAttack != null)
        {
            m_CurrentAttack.CancelAttack();
            m_CurrentAttack.StopAllCoroutines();
        }

        StopAllCoroutines();
    }

    protected virtual IEnumerator WaitAfterParried()
    {
        yield return new WaitForSeconds(1f);

        m_Callback.OnInterruptCombo(this);
    }

    public virtual void OnAttackInterrupted(BossAttack attack)
    {
        MLog.Log(LogType.BattleLog, 1, "Attack was Interrupted, Combo, " + this);

        if (m_CurrentAttack != null)
            m_CurrentAttack.CancelAttack();

        m_Callback.OnInterruptCombo(this);
    }

    public interface ComboCallback
    {
        void OnComboStart(AttackCombo combo);
        void OnComboEnd(AttackCombo combo);

        /// <summary>
        ///  Caution! OnInterruptCombo will most likely be called afterwards, the combo should take care of staggering!
        ///  Only if an immediate notification is required!!
        /// </summary>
        /// <param name="combo"></param>
        void OnComboParried(AttackCombo combo);

        void OnInterruptCombo(AttackCombo combo);
    }

}
