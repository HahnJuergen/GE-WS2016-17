﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LungeAttack : BossAttack, BossJumpCommand.JumpCallback, DamageCollisionHandler, Damage.DamageCallback {


    public LungeTrigger m_LungeTrigger;
    public BossCollider m_BossCollider;
    public GameObject m_Scarlet;

    public BossTurnCommand m_TurnCommand;
    public BossJumpCommand m_JumpCommand;

    public GameObject m_HitSignal;

    public Damage.BlockableType m_Blockable = Damage.BlockableType.Parry;

    public float m_TrackSpeed = 7f;

    public float m_TrackTime = 4f;
    public float m_HesitateTime = 0.1f;
    public float m_TimeAfterLand = 0.5f;
    public float m_MaxDistance = 9999f;
    public bool m_DamageInAir = false;

    private enum State {None, Aim, Jump, Land};
    private State m_State = State.None;

    private IEnumerator m_StateTimer;

    private bool m_ScarletInTargetArea;
    private DefaultCollisionHandler m_CollisionHandler;

    public LungeAttackCallbacks m_LungeAttackCallbacks;

    public override void StartAttack()
    {
        base.StartAttack();

        m_LungeTrigger.transform.position = new Vector3(m_Scarlet.transform.position.x, m_LungeTrigger.transform.position.y, m_Scarlet.transform.position.z);
        m_LungeTrigger.m_Active = false;
        m_LungeTrigger.m_Blockable = this.m_Blockable;
        m_BossCollider.m_Active = false;
        m_ScarletInTargetArea = false;

        m_Animator.SetTrigger("CrouchTrigger");

        m_StateTimer = Aim();
        StartCoroutine(m_StateTimer);

        m_CollisionHandler = new DefaultCollisionHandler(m_LungeTrigger);
        m_CollisionHandler.SetDamageBlockable(this.m_Blockable);
    }

    private IEnumerator Aim()
    {
        m_State = State.Aim;

        float time = 0;
        while ((time += Time.deltaTime) < m_TrackTime)
        {
            UpdateLungeTarget(Time.deltaTime);
            m_TurnCommand.TurnBossTowards(m_LungeTrigger.gameObject);
            yield return null;
        }

        float dist = Vector3.Distance(this.transform.position, m_Scarlet.transform.position);

        if (dist > m_MaxDistance)
        {
            m_Animator.SetTrigger("UprightTrigger");
            m_Callback.OnAttackEnd(this);
        }
        else
        {
            m_StateTimer = JumpAtTarget();
            StartCoroutine(m_StateTimer);
        }
    }

    private void UpdateLungeTarget(float deltaTime)
    {
        Vector3 posScarlet = new Vector3(m_Scarlet.transform.position.x, m_LungeTrigger.transform.position.y, m_Scarlet.transform.position.z);
        Vector3 posGoal = m_LungeTrigger.transform.position;

        if (Vector3.Distance(posScarlet, posGoal) <= deltaTime * m_TrackSpeed)
        {
            m_LungeTrigger.transform.position = posScarlet;
        }
        else
        {
            m_LungeTrigger.transform.position = Vector3.Lerp(posGoal, posScarlet, deltaTime * m_TrackSpeed);
        }
    }

    private IEnumerator JumpAtTarget()
    {
        yield return new WaitForSeconds(m_HesitateTime);

        m_State = State.Jump;
        m_JumpCommand.JumpAt(m_LungeTrigger.transform, this);

        m_CollisionHandler.SetDamageCallbacks(this);
        if (m_DamageInAir)
        {
            m_BossCollider.m_Active = true;
            m_BossCollider.m_Handler = m_CollisionHandler;
        }

        m_LungeTrigger.m_Active = true;
        m_LungeTrigger.m_Callback = this;
        m_LungeTrigger.m_CollisionHandler = this;
    }

    public override void CancelAttack()
    {
        if (m_StateTimer != null)
            StopCoroutine(m_StateTimer);

        m_LungeTrigger.m_Active = false;
        m_BossCollider.m_Active = false;

        m_State = State.None;

        if (m_HitSignal != null)
            m_HitSignal.SetActive(false);
    }

    public void OnLand()
    {
        if (m_State == State.None)
            return;

        m_State = State.Land;

        m_Animator.SetTrigger("UprightTrigger");

        m_StateTimer = WaitAfterLand();
        StartCoroutine(m_StateTimer);
    }

    private IEnumerator WaitAfterLand()
    {
        yield return new WaitForSeconds(m_TimeAfterLand);

        if (m_State == State.None)
            yield break;

        MLog.Log(LogType.BattleLog, 2, "WaitAfterLand, LungeAttack");

        m_State = State.None;

        m_LungeTrigger.m_Active = false;
        m_BossCollider.m_Active = false;
        m_Callback.OnAttackEnd(this);
    }

    public void OnStopMidAir()
    {
        if (m_HitSignal != null)
            m_HitSignal.SetActive(true);

        if (m_LungeAttackCallbacks != null)
            m_LungeAttackCallbacks.OnLungeStopInAir();
    }

    public void OnContinueMidAir()
    {
        if (m_HitSignal != null)
            m_HitSignal.SetActive(false);
    }

    public bool StopMidAir()
    {
        return m_ScarletInTargetArea;
    }

    public void HandleScarletCollision(Collider other)
    {
        m_ScarletInTargetArea = true;

        if (m_State == State.Land)
            m_CollisionHandler.HandleScarletCollision(other);
    }

    public void HandleCollision(Collider other, bool initialCollision)
    {
        m_CollisionHandler.HandleCollision(other, initialCollision);
    }

    public void HandleScarletLeave(Collider other)
    {
        m_ScarletInTargetArea = false;

        if (m_State == State.Land)
            m_CollisionHandler.HandleScarletLeave(other);
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

    public interface LungeAttackCallbacks
    {
        void OnLungeStopInAir();
    }
}
