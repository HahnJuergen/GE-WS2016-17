﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStaggerCommand : PlayerCommand {

    private Rigidbody m_ScarletBody;

    private void Start()
    {
    }

    public override void InitTrigger()
    {
        m_Trigger = new StaggerTrigger(this);
        m_ScarletBody = m_Scarlet.GetComponent<Rigidbody>();
    }

    public override void TriggerCommand()
    {
        m_Callback.OnCommandStart(m_CommandName, this);
        DoStagger();
    }

    private void DoStagger()
    {

        m_ScarletBody.velocity = new Vector3(0, 0, 0);
    }

    // Stagger trigger remains empty!! stagger is not triggered by the player.
    private class StaggerTrigger : CommandTrigger
    {
        public StaggerTrigger(PlayerCommand command) : base(command)
        {
        }

        public override void Update()
        {
        }
    }
}
