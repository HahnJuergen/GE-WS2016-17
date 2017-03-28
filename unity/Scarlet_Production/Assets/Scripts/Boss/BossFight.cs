﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BossFight : MonoBehaviour {

    public static bool s_ScarletCanDie = true;
    protected IEnumerator m_ScarletHealthEnumerator;
    protected IEnumerator m_ResetEnumerator;

    protected Vector3 m_ScarletPositionStart;
    protected Quaternion m_ScarletRotationStart;

    protected Vector3 m_BossPositionStart;
    protected Quaternion m_BossRotationStart;

    public float m_MaxScarletRegenerationAfterPhase = 100f;
    protected IEnumerator m_ScarletRegenerationEnumerator;

    protected IEnumerator m_GodModeEnumerator;

    public GameObject[] m_PhaseIndicators;

    public Image m_FadeOutImage;
    protected float m_FadeOutSpeed = 2f;

    protected bool m_VolumeReset = false;

    public virtual void StartBossfight()
    {
        StartCoroutine(StartAfterFirstFrame());
        StartCoroutine(GodModeEnumerator());
    }

    public virtual void RestartBossfight()
    {
        PlayerHittable playerHittable = FindObjectOfType<PlayerHittable>();
        CharacterHealth scarletHealth = playerHittable.GetComponent<CharacterHealth>();
        scarletHealth.m_CurrentHealth = scarletHealth.m_HealthStart;

        BossHittable bossHittable = FindObjectOfType<BossHittable>();
        CharacterHealth bossHealth = bossHittable.GetComponent<CharacterHealth>();
        bossHealth.m_CurrentHealth = bossHealth.m_HealthStart;

        StartBossfight();
        PlayerControls controls = FindObjectOfType<PlayerControls>();
        controls.EnableAllCommands();
    }

    protected virtual void StoreInitialState()
    {
        // this kind of abuses the fact that hittables always have to be where the colliders are, that is, they're definitely at the correct transform
        PlayerHittable playerHittable = FindObjectOfType<PlayerHittable>();
        BossHittable bossHittable = FindObjectOfType<BossHittable>();

        m_ScarletPositionStart = playerHittable.transform.position + Vector3.zero;
        m_ScarletRotationStart = Quaternion.Euler(playerHittable.transform.rotation.eulerAngles);

        m_BossPositionStart = bossHittable.transform.position + Vector3.zero;
        m_BossRotationStart = Quaternion.Euler(bossHittable.transform.rotation.eulerAngles);
    }

    protected virtual IEnumerator StartAfterFirstFrame()
    {
        yield return null;

        StoreInitialState();
        m_ScarletHealthEnumerator = CheckScarletHealth();
        StartCoroutine(m_ScarletHealthEnumerator);
    }

    public abstract void LoadSceneAfterBossfight();

    protected virtual IEnumerator CheckScarletHealth()
    {
        PlayerHittable playerHittable = FindObjectOfType<PlayerHittable>();
        CharacterHealth scarletHealth = playerHittable.GetComponent<CharacterHealth>();

        while (true)
        {
            if (scarletHealth.m_CurrentHealth <= 0 && s_ScarletCanDie)
            {
                OnScarletDead();
            }
            yield return null;
        }
    }

    protected virtual void OnScarletDead()
    {
        StopAllCoroutines();

        PlayerControls controls = FindObjectOfType<PlayerControls>();
        controls.DisableAllCommands();
        controls.StopMoving();
        controls.GetComponent<Animator>().SetTrigger("DeathTrigger");

        FancyAudio.Instance.StopAll();
        ScarletVOPlayer.Instance.PlayDeathSound();

        SlowTime.Instance.m_PreventChanges = false;
        SlowTime.Instance.StopAllCoroutines();
        SlowTime.Instance.StopSlowMo();

        m_ResetEnumerator = ResetRoutine();
        StartCoroutine(m_ResetEnumerator);
    }

    protected virtual IEnumerator ResetRoutine()
    {
        GetComponent<DeathScreenController>().ShowVictoryScreen(gameObject);
        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(FadeOutBackground());

        PlayerHittable playerHittable = FindObjectOfType<PlayerHittable>();
        playerHittable.GetComponent<Animator>().SetTrigger("IdleTrigger");

        yield return new WaitForSeconds(1f);

        PlayerHealCommand healCommand = FindObjectOfType<PlayerHealCommand>();
        healCommand.ResetPotions();

        ResetInitialPositions();
        DestroyAllBullets();

        yield return StartCoroutine(FadeInBackground());
        yield return null;
        RestartBossfight();
    }

    protected virtual void ResetInitialPositions()
    {
        PlayerHittable playerHittable = FindObjectOfType<PlayerHittable>();
        BossHittable bossHittable = FindObjectOfType<BossHittable>();

        playerHittable.transform.position = m_ScarletPositionStart + new Vector3();
        playerHittable.transform.rotation = Quaternion.Euler(m_ScarletRotationStart.eulerAngles);
        playerHittable.GetComponent<Animator>().SetTrigger("IdleTrigger");

        bossHittable.transform.position = m_BossPositionStart + new Vector3();
        bossHittable.transform.rotation = Quaternion.Euler(m_BossRotationStart.eulerAngles);
    }

    protected virtual void DestroyAllBullets()
    {
        List<GameObject> toDestroy = new List<GameObject>();

        foreach(BulletBehaviour b in FindObjectsOfType<Bullet>())
        {
            b.StopAllCoroutines();
            if (b is Bullet)
                toDestroy.Add(b.gameObject);
        }

        for(int i = 0; i < toDestroy.Count; i++)
        {
            if (!toDestroy[i].activeInHierarchy)
                continue;

            try
            {
                Destroy(toDestroy[i]);
            }
            catch { }
        }
    }

    protected virtual void RegenerateScarletAfterPhase()
    {
        PlayerHittable playerHittable = FindObjectOfType<PlayerHittable>();
        CharacterHealth scarletHealth = playerHittable.GetComponent<CharacterHealth>();

        PlayerHealCommand healCommand = FindObjectOfType<PlayerHealCommand>();
        healCommand.m_NumHealthPotions++;

        m_ScarletRegenerationEnumerator = ScarletRegenerationRoutine(scarletHealth);
        StartCoroutine(m_ScarletRegenerationEnumerator);
    }

    protected virtual IEnumerator ScarletRegenerationRoutine(CharacterHealth health)
    {
        float newHealth = Mathf.Min(health.m_MaxHealth, health.m_CurrentHealth + m_MaxScarletRegenerationAfterPhase * health.m_MaxHealth);
        float healthGain = newHealth - health.m_CurrentHealth;

        float regTime = 0.5f;
        float healedAmount = 0;
        float t = 0;
        while((t += Time.deltaTime) < regTime)
        {
            float healStep = Time.deltaTime * (healthGain / regTime);
            health.m_CurrentHealth += healStep;
            healedAmount += healStep;
            yield return null;
        }

        health.m_CurrentHealth += healthGain - healedAmount;
    }

    protected virtual void StopPlayerMove()
    {
        PlayerMoveCommand moveCommand = FindObjectOfType<PlayerMoveCommand>();
        if (moveCommand != null)
            moveCommand.StopMoving();
    }

    protected virtual void SetScarletVoice(ScarletVOPlayer.Version version)
    {
        ScarletVOPlayer voPlayer = FindObjectOfType<ScarletVOPlayer>();
        if (voPlayer != null)
        {
            voPlayer.m_Version = version;
            voPlayer.SetupPlayers();
        }
    }

    protected virtual void PlayScarletVictoryAnimation()
    {
        PlayerHittable playerHittable = FindObjectOfType<PlayerHittable>();
        playerHittable.GetComponent<Animator>().SetTrigger("VictoryTrigger");
    }

    protected IEnumerator GodModeEnumerator()
    {
        while(true)
        {
            if (Input.anyKeyDown && Input.GetKeyDown(KeyCode.Alpha0))
            {
                if (s_ScarletCanDie)
                {
                    s_ScarletCanDie = false;
                    try
                    {
                        GetComponent<GodModeActivatedBoxController>().ShowGodModeAcitvated("God mode enabled.", "Filthy Casual!");
                    } catch { }
                    print("God mode enabled.");
                }
                else
                {
                    s_ScarletCanDie = true;
                    try
                    {
                        GetComponent<GodModeActivatedBoxController>().ShowGodModeAcitvated("God mode disabled.", "");
                    } catch { }
                    print("God mode disabled.");
                }
            }

            yield return null;
        }
    }

    protected abstract IEnumerator SaveProgressInPlayerPrefs();

    protected virtual void PlayMusic()
    {
        BossfightJukebox musicPlayer = BossfightJukebox.Instance;
        if (musicPlayer != null)
        {
            float volume = musicPlayer.m_Source1.volume;
            if (!m_VolumeReset)
            {
                BossfightJukebox.SetVolume(0f);
                m_VolumeReset = true;
            }

            musicPlayer.FadeToVolume(volume);
            musicPlayer.StartPlayingUnlessPlaying();
        }
    }

    protected virtual void SetMusicStage(int stage)
    {
        BossfightJukebox musicPlayer = BossfightJukebox.Instance;
        if (musicPlayer != null)
        {
            musicPlayer.m_CurrentClipOffset = stage;
        }
    }

    public virtual void SetPhaseIndicatorsEnabled(int howMany)
    {
        for(int i = 0; i < m_PhaseIndicators.Length; i++)
        {
            if (i < howMany)
            {
                m_PhaseIndicators[m_PhaseIndicators.Length - i - 1].SetActive(true);
            } else
            {
                m_PhaseIndicators[m_PhaseIndicators.Length - i - 1].SetActive(false);
            }
        }
    }

    protected virtual IEnumerator FadeOutBackground()
    {
        if (m_FadeOutImage == null)
            yield break;

        Color blackColor = Color.black;
        Color transparentColor = new Color(0, 0, 0, 0);

        float t = 0;
        while((t += Time.deltaTime) < m_FadeOutSpeed)
        {
            m_FadeOutImage.color = Color.Lerp(transparentColor, blackColor, t / m_FadeOutSpeed);
            yield return null;
        }
        m_FadeOutImage.color = blackColor;
    }

    protected virtual IEnumerator FadeInBackground()
    {
        if (m_FadeOutImage == null)
            yield break;

        Color blackColor = Color.black;
        Color transparentColor = new Color(0, 0, 0, 0);

        float t = 0;
        while ((t += Time.deltaTime) < m_FadeOutSpeed)
        {
            m_FadeOutImage.color = Color.Lerp(blackColor, transparentColor, t / m_FadeOutSpeed);
            yield return null;
        }
        m_FadeOutImage.color = transparentColor;
    }
}

public interface BossfightCallbacks
{
    void PhaseEnd(BossController whichPhase);
    void SetPhaseIndicatorsEnabled(int howMany);
}

