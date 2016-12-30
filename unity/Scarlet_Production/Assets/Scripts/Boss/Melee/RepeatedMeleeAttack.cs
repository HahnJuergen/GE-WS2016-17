﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatedMeleeAttack : BossAttack, BossMeleeHitCommand.MeleeHitCallback, Damage.DamageCallback
{

    public int m_Repetitions = 3;
    private int m_CurrentRepetition;

    public float m_MovementSpeed = 0.5f;

    public BossMoveCommand m_BossMove;
    public BossMeleeHitCommand m_BossHit;
    public BossMeleeDamage m_Damage;

    private IEnumerator m_BetweenHitsTimer;
    public float m_TimeBetweenHits = 0.5f;

    public override void StartAttack()
    {
        m_CurrentRepetition = 0;
        m_BossHit.DoHit(this);
        m_BossMove.DoMove(m_Boss.transform.forward.x * m_MovementSpeed, m_Boss.transform.forward.z * m_MovementSpeed);
        m_Damage.m_Callback = this;
        m_Damage.m_CollisionHandler = new DefaultCollisionHandler(m_Damage);
    }

    public override void CancelAttack()
    {
        if (m_BetweenHitsTimer != null)
            StopCoroutine(m_BetweenHitsTimer);

        m_BossHit.CancelHit();
        m_Damage.m_Active = false;
    }

    public void OnMeleeHitSuccess()
    {
    }

    public void OnMeleeHitEnd()
    {
        m_BossMove.DoMove(0, 0);

        m_CurrentRepetition++;
        if (m_CurrentRepetition >= m_Repetitions)
        {
            m_Callback.OnAttackEnd(this);
        }
        else
        {
            m_BetweenHitsTimer = DoNextHitAfter(m_TimeBetweenHits);
            StartCoroutine(m_BetweenHitsTimer);
        }
    }

    private IEnumerator DoNextHitAfter(float time)
    {
        yield return new WaitForSeconds(time);

        m_BossHit.DoHit(this);
        m_BossMove.DoMove(m_Boss.transform.forward.x * m_MovementSpeed, m_Boss.transform.forward.z * m_MovementSpeed);
    }

    public void OnParryDamage()
    {
        m_Callback.OnAttackParried(this);
    }

    public void OnBlockDamage()
    {
    }

    public void OnSuccessfulHit()
    {
    }
}
