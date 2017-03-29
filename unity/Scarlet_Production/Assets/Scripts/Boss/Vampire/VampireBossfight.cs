﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VampireBossfight : BossFight, BossfightCallbacks {

    public enum Phase {Tutorial, Phase1, Phase2, Phase3};
    public Phase m_StartPhase;

    public VampirePhase0TutorialController m_TutorialController;
    public VampirePhase1Controller m_Phase1Controller;
    public VampirePhase2Controller m_Phase2Controller;
    public VampirePhase3Controller m_Phase3Controller;

    public TutorialPromptController m_TutorialVisuals;

    protected bool m_HealTutorialShown = false;

    void Start()
    {
        StartBossfight();
        StartCoroutine(SaveProgressInPlayerPrefs());
    }

    public override void StartBossfight()
    {
        base.StartBossfight();
        StartCoroutine(StartAfterShortDelay());
    }

    private IEnumerator StartAfterShortDelay()
    {
        yield return new WaitForSeconds(0.2f);
        SetScarletVoice(ScarletVOPlayer.Version.City);
        ScarletVOPlayer.Instance.SetupPlayers();
        PlayMusic();

        m_HealTutorialShown = false;

        if (m_StartPhase == Phase.Tutorial)
        {
            m_TutorialController.enabled = true;
            m_TutorialController.StartPhase(this);
            SetPhaseIndicatorsEnabled(3);
            SetMusicStage(0);
        }
        else if (m_StartPhase == Phase.Phase1)
        {
            PhaseEnd(m_TutorialController);
        }
        else if (m_StartPhase == Phase.Phase2)
        {
            PhaseEnd(m_Phase1Controller);
        }
        else if (m_StartPhase == Phase.Phase3)
        {
            PhaseEnd(m_Phase2Controller);
        }
    }

    public void PhaseEnd(BossController whichPhase)
    {
        if (whichPhase == m_TutorialController)
        {
            MLog.Log(LogType.BattleLog, "Vampire: Tutorial over " + this);
            DestroyAllBullets();
            m_TutorialController.enabled = false;
            m_TutorialController.m_NotDeactivated = false;
            SetPhaseIndicatorsEnabled(3);
            m_Phase1Controller.enabled = true;
            m_Phase1Controller.m_NotDeactivated = true;

            m_Phase1Controller.StartPhase(this);
        }
        else if (whichPhase == m_Phase1Controller)
        {
            MLog.Log(LogType.BattleLog, "Vampire: Phase 1 over " + this);
            DestroyAllBullets();
            m_Phase1Controller.enabled = false;
            m_Phase1Controller.m_NotDeactivated = false;
            m_Phase2Controller.enabled = true;
            SetPhaseIndicatorsEnabled(2);
            SetMusicStage(1);
            m_Phase2Controller.m_NotDeactivated = true;
            m_Phase2Controller.StartPhase(this);

            VampireHittable hittable = FindObjectOfType<VampireHittable>();
            if (hittable != null)
                hittable.StopPlayingCriticalHPSound();

            RegenerateScarletAfterPhase();
        }
        else if (whichPhase == m_Phase2Controller)
        {
            MLog.Log(LogType.BattleLog, "Vampire: Phase 2 over " + this);
            DestroyAllBullets();
            m_Phase2Controller.enabled = false;
            m_Phase2Controller.m_NotDeactivated = false;
            m_Phase3Controller.enabled = true;
            SetPhaseIndicatorsEnabled(1);
            SetMusicStage(2);
            m_Phase3Controller.m_NotDeactivated = true;
            m_Phase3Controller.StartPhase(this);

            VampireHittable hittable = FindObjectOfType<VampireHittable>();
            if (hittable != null)
                hittable.StopPlayingCriticalHPSound();

            RegenerateScarletAfterPhase();
        }
        else if (whichPhase == m_Phase3Controller)
        {
            MLog.Log(LogType.BattleLog, "Vampire: Phase 3 over " + this);
            DestroyAllBullets();
            m_Phase3Controller.enabled = false;
            m_Phase3Controller.m_NotDeactivated = false;
            SetPhaseIndicatorsEnabled(0);
            BossfightJukebox.Instance.FadeToVolume(0f);

            VampireHittable hittable = FindObjectOfType<VampireHittable>();
            if (hittable != null)
                hittable.StopPlayingCriticalHPSound();

            ScarletVOPlayer.Instance.StopPlayingBadlyWoundedSound();

            StartCoroutine(ShowVictoryScreenAfterWaiting());
        }
    }

    private IEnumerator ShowVictoryScreenAfterWaiting()
    {
        yield return new WaitForSeconds(16f);
        ScarletVOPlayer.Instance.PlayVictorySound();
        PlayScarletVictoryAnimation();
        GetComponent<VictoryScreenController>().ShowVictoryScreen(gameObject);
    }

    protected override IEnumerator CheckScarletHealth()
    {
        PlayerHittable playerHittable = FindObjectOfType<PlayerHittable>();
        CharacterHealth scarletHealth = playerHittable.GetComponent<CharacterHealth>();

        while (true)
        {
            if (!m_HealTutorialShown && scarletHealth.m_CurrentHealth <= scarletHealth.m_MaxHealth * 0.5f)
            {
                ShowHealTutorial();
                m_HealTutorialShown = true;
            }

            if (scarletHealth.m_CurrentHealth <= 0 && s_ScarletCanDie)
            {
                OnScarletDead();
            }
            yield return null;
        }
    }

    protected override void OnScarletDead()
    {
        m_HealTutorialShown = false;

        VampireGatherLightFlyingObjectsManager vglfom = FindObjectOfType<VampireGatherLightFlyingObjectsManager>();
        if (vglfom != null)
        {
            vglfom.OnAttackEnd();
        }
        m_TutorialController.CancelAndReset();
        m_Phase1Controller.CancelAndReset();
        m_Phase2Controller.CancelAndReset();
        m_Phase3Controller.CancelAndReset();

        m_TutorialController.enabled = false;
        m_Phase1Controller.enabled = false;
        m_Phase2Controller.enabled = false;
        m_Phase3Controller.enabled = false;

        VampireHittable hittable = FindObjectOfType<VampireHittable>();
        if (hittable != null)
        {
            hittable.m_DontPlaySound = false;
            hittable.StopPlayingCriticalHPSound();
        }

        base.OnScarletDead();

        new FARQ().StartTime(154.4f).EndTime(160.527f).Location(m_TutorialController.transform).ClipName("vampire").Play();
    }

    protected void ShowHealTutorial()
    {
        StartCoroutine(HealTutorialEnumerator());
    }

    protected IEnumerator HealTutorialEnumerator()
    {
        yield return new WaitForSeconds(0.2f);

        SlowTime.Instance.StartSlowMo(m_TutorialController.m_TutorialSlowMo);
        m_TutorialVisuals.ShowTutorial("Y", "Heal", m_TutorialController.m_TutorialSlowMo);

        float t = 0;
        while ((t += Time.deltaTime) < 6 * m_TutorialController.m_TutorialSlowMo && !Input.anyKeyDown)
        {
            yield return null;
        }
        SlowTime.Instance.StopSlowMo();
        m_TutorialVisuals.HideTutorial(1f);
    }

    protected override void ResetInitialPositions()
    {
        try
        {
            m_Phase1Controller.m_VampireAnimator.SetTrigger("ForceIdleTrigger");
        } catch { }

        base.ResetInitialPositions();
    }

    public override void LoadSceneAfterBossfight()
    {
        PlayerPrefs.SetString("CurrentLevel", "post_vampire_scene");
        PlayerPrefs.Save();

        FindObjectOfType<ShowSaveSignController>().FadeInSaveSign(LoadScene);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(PlayerPrefs.GetString("CurrentLevel"));
    }

    protected override IEnumerator SaveProgressInPlayerPrefs()
    {
        yield return null;
        PlayerPrefs.SetString("CurrentLevel", "vampire_battle_dev");
        PlayerPrefs.Save();

        FindObjectOfType<ShowSaveSignController>().FadeInSaveSign();
    }
}
