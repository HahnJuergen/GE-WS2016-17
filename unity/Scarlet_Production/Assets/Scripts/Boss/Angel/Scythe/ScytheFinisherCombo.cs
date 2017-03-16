﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScytheFinisherCombo : AngelCombo {

    public float m_ScytheStanceTime;
    
    public override void LaunchCombo()
    {
        base.LaunchCombo();
    }

    public override void OnAttackStart(BossAttack attack)
    {
        if (attack is AngelOnlyAnimationAttack)
        {
            AngelSoundPlayer.PlayMiscStanceSound();
        }
        base.OnAttackStart(attack);
    }

    /* protected virtual IEnumerator WaitForAnimationThenLaunch()
     {
         yield return new WaitForSeconds(m_ScytheStanceTime);

         if (!m_Cancelled)
             base.LaunchCombo();
     } */

    public override void OnAttackEnd(BossAttack attack)
    {
        if (attack is AngelOnlyAnimationAttack)
        {
            m_Animator.SetInteger("StanceMoveDirection", 0);
            m_Animator.SetTrigger("StanceMoveTrigger");

            base.OnAttackEnd(attack);
        }
        else if (attack is ChaseAttack)
        {
            OnFindFinish(attack);
        }
        else
        {
            base.OnAttackEnd(attack);
        }
    }

    protected override void OnFindFinish(BossAttack findAttack)
    {
        if (m_Success < 0)
        {
            m_Animator.SetTrigger("IdleTrigger");
            m_Callback.OnComboEnd(this);
        }
        else
        {
            base.OnAttackEnd(findAttack);
        }
    }

}
