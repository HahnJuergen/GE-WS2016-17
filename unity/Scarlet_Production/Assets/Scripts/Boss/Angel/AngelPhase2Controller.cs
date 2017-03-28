﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngelPhase2Controller : AngelController {

    protected bool m_EndInitialized = false;

    public override void StartPhase(BossfightCallbacks callback)
    {
        base.StartPhase(callback);

        m_EndInitialized = false;
    }

    private void Update()
    {
        if (enabled && !m_EndInitialized && m_BossHittable.m_Health.m_CurrentHealth <= 0)
        {
            m_EndInitialized = true;
            CancelComboIfActive();

            StopListeningForWindupEvents();
            StopListeningToAttackEvents();
            m_NotDeactivated = false;

            m_BossHittable.RegisterInterject(null);

            m_Callback.PhaseEnd(this);
        }
    }
}
