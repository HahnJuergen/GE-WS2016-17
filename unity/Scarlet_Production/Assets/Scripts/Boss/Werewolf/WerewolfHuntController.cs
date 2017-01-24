﻿using System;
using System.Collections;
using UnityEngine;

public class WerewolfHuntController : BossController, AttackCombo.ComboCallback, LungeAttack.LungeAttackCallbacks {

    public AttackCombo m_JumpCombo;
    public LungeAttack m_LungeAttack;

    public Transform m_HuntCenter;
    public float m_HuntDistance;

    public int m_NumParryRequired = 4;
    public PlayerAttackCommand m_PlayerAttackCommand;
    private float m_AttackDmgAmountBefore;
    private float m_RiposteDmgAmountBefore;
    private float m_FinalDmgAmountBefore;

    public PlayerParryCommand m_PlayerParryCommand;
    private float m_BlockTimeBefore;

    public float m_MinHuntTime = 5f;
    public float m_MaxHuntTime = 9f;
    private bool m_JumpSoon;

    public CharacterHealth m_BossHealth;

    public BossTurnCommand m_BossTurn;
    public BossMoveCommand m_BossMove;

    public TutorialPromptController m_Tutorial;

    public float m_SlowMoAmount;
    public float m_SlowMoTime;

    private bool m_Active;
    private BossfightCallbacks m_Callbacks;

    private IEnumerator m_HuntTimer;

    private IEnumerator m_SlowMoTimer;

    /// <summary>
    /// as opposed to "jumping": hunting = running around
    /// </summary>
    private bool m_Hunting;

    public bool m_SuccessfullyRiposted;

    protected new void Start()
    {
    }

    private void Update()
    {
        if (m_Active)
        {
            if (m_BossHealth.m_CurrentHealth == 0)
            {
                m_Active = false;

                m_PlayerAttackCommand.m_RegularHitDamage = m_AttackDmgAmountBefore;
                m_PlayerAttackCommand.m_FinalHitDamage = m_FinalDmgAmountBefore;
                m_PlayerAttackCommand.m_RiposteDamage = m_RiposteDmgAmountBefore;

                m_PlayerParryCommand.m_PerfectParryTime = m_PlayerParryCommand.m_PerfectParryTime - m_BlockTimeBefore;
                m_PlayerParryCommand.m_OkParryTime = m_BlockTimeBefore;

                m_JumpCombo.CancelCombo();
                m_LungeAttack.m_LungeAttackCallbacks = null;

                if (m_Callbacks != null)
                    m_Callbacks.PhaseEnd(this);
            }

            if (m_BossHealth.m_CurrentHealth < m_BossHealth.m_HealthOld)
            {
                m_SuccessfullyRiposted = true;
                EventManager.StopListening("user_parry", OnUserFirstParry);
            }
        }

        if (m_Active && m_Hunting)
        {
            HuntPhaseRoutine();
        }
    }

    public void StartHuntPhase(BossfightCallbacks callbacks)
    {
        m_BossHittable.RegisterInterject(this);
        m_SuccessfullyRiposted = false;

        m_Callbacks = callbacks;
        m_Active = true;
        m_JumpSoon = false;
        m_LungeAttack.m_LungeAttackCallbacks = this;

        m_RiposteDmgAmountBefore = m_PlayerAttackCommand.m_RiposteDamage;
        m_AttackDmgAmountBefore = m_PlayerAttackCommand.m_RegularHitDamage;
        m_FinalDmgAmountBefore = m_PlayerAttackCommand.m_FinalHitDamage;

        m_PlayerAttackCommand.m_RegularHitDamage = 0;
        m_PlayerAttackCommand.m_FinalHitDamage = 0;
        m_PlayerAttackCommand.m_RiposteDamage = m_BossHealth.m_MaxHealth / m_NumParryRequired;

        m_BlockTimeBefore = m_PlayerParryCommand.m_OkParryTime;
        m_PlayerParryCommand.m_OkParryTime = 0;
        m_PlayerParryCommand.m_PerfectParryTime = m_PlayerParryCommand.m_PerfectParryTime + m_BlockTimeBefore;

        EventManager.StartListening("user_parry", OnUserFirstParry);

        LaunchSingleHuntIteration();
    }

    private IEnumerator EndHuntAfter(float time)
    {
        yield return new WaitForSeconds(time);
        m_JumpSoon = true;

    }

    private void HuntPhaseRoutine()
    {
        if (m_JumpSoon)
        {
            if (CheckJump())
            {
                LungeOut();
                return;
            }
        }

        float distanceToCenter = Vector3.Distance(transform.position, m_HuntCenter.position);

        if (distanceToCenter >= 1.1 * m_HuntDistance || distanceToCenter <= 0.9 * m_HuntDistance)
        {
            ReachHuntDistance(distanceToCenter);
        }
        else
        {
            RunInCircle(distanceToCenter);
        }
    }

    private bool CheckJump()
    {
        float turnAngle = this.transform.rotation.eulerAngles.y;
        return (Mathf.Abs(((int) turnAngle) % 90) <= 4);
    }

    private void LungeOut()
    {
        m_JumpSoon = false;
        m_Hunting = false;
        m_JumpCombo.m_Callback = this;

        m_BossMove.StopMoving();
        m_JumpCombo.LaunchCombo();
    }

    private void ReachHuntDistance(float distanceToCenter)
    {
        float turnAngle;
        if (distanceToCenter < m_HuntDistance)
        { 
            turnAngle = BossTurnCommand.CalculateAngleTowards(this.transform, m_HuntCenter);
            turnAngle += 180;
        }
        else
        {
            turnAngle = BossTurnCommand.CalculateAngleTowards(this.transform, m_HuntCenter);
        }

        m_BossTurn.TurnBossBy(turnAngle);

        m_BossMove.DoMove(transform.forward.x, transform.forward.z);
    }

    private void RunInCircle(float distanceToCenter)
    {
        m_BossTurn.TurnBossBy(BossTurnCommand.CalculateAngleTowards(this.transform, m_HuntCenter) + 90);
        m_BossMove.DoMove(transform.forward.x, transform.forward.z);
    }

    public new void OnInterruptCombo(AttackCombo combo)
    {
        if (combo == m_JumpCombo)
        {
            if (m_SlowMoTimer != null)
            {
                MLog.Log(LogType.BattleLog, 0, "Hunt Controller: Cancelling Jump Slow Mo");
                StopCoroutine(m_SlowMoTimer);
            }
        }

        if (m_Hunting)
            return;

        if (m_HuntTimer == null)
            StopCoroutine(m_HuntTimer);

        m_HuntTimer = WaitToStandUp();
        StartCoroutine(m_HuntTimer);
    }

    public new void OnComboStart(AttackCombo combo)
    {
    }

    public new void OnComboEnd(AttackCombo combo)
    {
        StartCoroutine(WaitToStandUp());

//        LaunchSingleHuntIteration();
    }

    private IEnumerator WaitToStandUp()
    {
        yield return new WaitForSeconds(0.5f);
        LaunchSingleHuntIteration();
    }

    private void LaunchSingleHuntIteration()
    {
        m_Hunting = true;

        m_JumpSoon = false;
        float time = m_MinHuntTime + UnityEngine.Random.value * (m_MaxHuntTime - m_MinHuntTime);
        m_HuntTimer = EndHuntAfter(time);
        StartCoroutine(m_HuntTimer);
    }

    // doesn't matter at all (JumpAttack will always take care of that)
    public new bool OnHit(Damage dmg)
    {
        dmg.OnBlockDamage();
        return true;
    }

    public void OnLungeStopInAir()
    {
        if (!m_SuccessfullyRiposted)
        {
            SlowTime.Instance.StartSlowMo(m_SlowMoAmount);

            m_SlowMoTimer = SlowMoTimer();
            StartCoroutine(m_SlowMoTimer);

            m_Tutorial.ShowTutorial("X", "Parry", m_SlowMoAmount);
        }
    }

    public override void OnComboParried(AttackCombo combo)
    {
        base.OnComboParried(combo);
        if (!m_SuccessfullyRiposted)
        {
            SlowTime.Instance.StartSlowMo(m_SlowMoAmount);

            m_SlowMoTimer = SlowMoTimer();
            StartCoroutine(m_SlowMoTimer);

            m_Tutorial.ShowTutorial("Y", "Riposte", m_SlowMoAmount);
        }

        if (m_HuntTimer == null)
            StopCoroutine(m_HuntTimer);
    }

    public override void OnComboRiposted(AttackCombo combo)
    {
        base.OnComboRiposted(combo);
        if (!m_SuccessfullyRiposted)
        {
            if (m_SlowMoTimer != null)
            {
                StopCoroutine(m_SlowMoTimer);
                SlowTime.Instance.StopSlowMo();

                m_Tutorial.HideTutorial(m_SlowMoAmount);
            }
        }

        if (m_HuntTimer == null)
            StopCoroutine(m_HuntTimer);
    }

    private void OnUserFirstParry()
    {
        if (m_SlowMoTimer != null)
        {
            StopCoroutine(m_SlowMoTimer);
            SlowTime.Instance.StopSlowMo();
            m_Tutorial.HideTutorial();
        }
    }

    private IEnumerator SlowMoTimer()
    {
        yield return new WaitForSeconds(m_SlowMoTime * m_SlowMoAmount);
        SlowTime.Instance.StopSlowMo();

        m_Tutorial.HideTutorial(m_SlowMoAmount);
    }
}
