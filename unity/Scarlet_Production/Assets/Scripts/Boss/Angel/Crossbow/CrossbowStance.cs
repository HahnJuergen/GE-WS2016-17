﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossbowStance : AngelOnlyAnimationAttack {

    public TurnTowardsScarlet m_TurnCommand;

    public float m_FlyHeight;

    protected override IEnumerator WaitUntilEnd()
    {
        float t = 0;

        while((t += Time.deltaTime) < m_AnimTime)
        {
            m_Boss.transform.position = new Vector3(m_Boss.transform.position.x, 
                Mathf.Sin(t / m_AnimTime * Mathf.PI / 4) * m_FlyHeight, 
                m_Boss.transform.position.z);

            m_TurnCommand.DoTurn();
            yield return null;
        }

        m_Callback.OnAttackEnd(this);
    }

}
