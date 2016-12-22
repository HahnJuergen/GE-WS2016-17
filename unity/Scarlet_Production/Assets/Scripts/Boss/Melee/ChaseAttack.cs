﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseAttack : BossAttack, BossMeleeHitCommand.MeleeHitCallback, DamageCollisionHandler {

    public BossMoveCommand m_BossMove;
    public TurnTowardsScarlet m_BossTurn;
    public BossMeleeHitCommand m_BossHit;
    public BossMeleeDamage m_RangeTrigger;

    public GameObject m_Target;

    public float m_MaxChaseTime = 7f;
    private float m_CurrentChaseTime;

    private float m_MaxTurnAngleChaseState = 120f;
    private float m_MaxTurnAngleTurnState = 180f;

    private enum AttackState {None, Chase, Turn, Attack};
    private AttackState m_State = AttackState.None;

    private bool m_ScarletInRange = false;

    public override void StartAttack()
    {
        m_State = AttackState.Chase;
        m_BossTurn.m_TurnSpeed = m_MaxTurnAngleTurnState;
        m_RangeTrigger.m_CollisionHandler = this;
        m_RangeTrigger.m_Active = true;

        m_ScarletInRange = false;
        m_CurrentChaseTime = 0f;

        m_Callback.OnAttackStart(this);
    }

    void Update ()
    {
		if (m_State == AttackState.Chase)
        {
            Chase();
        }
        else if (m_State == AttackState.Turn)
        {
            Turn();
        }
	}

    void Chase()
    {
        Vector3 distance = m_Target.transform.position - m_Boss.transform.position;

        m_BossTurn.DoTurn();

        if (Mathf.Abs(m_BossTurn.m_AngleTowardsScarlet) <= 45)
        {
            m_BossMove.DoMove(m_Boss.transform.forward.x, m_Boss.transform.forward.z);
            m_BossTurn.m_TurnSpeed = m_MaxTurnAngleChaseState;
        }
        else
        {
            m_State = AttackState.Turn;
            m_BossMove.StopMoving();
            m_BossTurn.m_TurnSpeed = m_MaxTurnAngleTurnState;
        }

        CheckRange();
   }

    void Turn()
    {
        m_BossTurn.DoTurn();

        if (Mathf.Abs(m_BossTurn.m_AngleTowardsScarlet) <= 15)
        {
            m_State = AttackState.Chase;
        }

        CheckRange();
    }

    private void CheckRange()
    {
        if (m_ScarletInRange)
        {
            StartHit();
        }
        else
        {
            CheckChaseTime();
        }

    }

    private void CheckChaseTime()
    {
        m_CurrentChaseTime += Time.deltaTime;

        if (m_CurrentChaseTime >= m_MaxChaseTime)
        {
            m_State = AttackState.None;
            CancelAttack();
            m_Callback.OnAttackInterrupted(this);
        }
    }

    void StartHit()
    {
        m_State = AttackState.Attack;
        m_BossHit.DoHit(this);
        m_BossMove.StopMoving();
    }
    
    public override void CancelAttack()
    {
        m_State = AttackState.None;
        m_BossHit.CancelHit();
    }
    
    public void OnMeleeHitEnd()
    {
        m_Callback.OnAttackEnd(this);    
    }

    public void OnMeleeHitSuccess()
    {
        m_Callback.OnAttackEnd(this);
    }

    public void HandleScarletCollision(Collider other)
    {
        Hittable hittable = other.GetComponentInChildren<Hittable>();
        if (hittable != null && hittable is PlayerHittable)
        {
            m_ScarletInRange = true;
        }
    }

    public void HandleCollision(Collider other, bool initialCollision)
    {
    }
}
