﻿using SequencedActionCreator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject[] MenuItems;
    public TrackingBehaviour menuCamera;
    public GameObject Menu;
    public bool m_InCombatScene = false;

    public bool isShowing = false;

    private int m_Continue = 0;
    private int m_NewGame = 1;
    private int m_SelectChapter = 2;
    private int m_Options = 3;
    private int m_Credits = 4;
    private int m_LeaveGame = 5;



    private int selected;
    private CameraTracking cameraTracking;
    private TrackingBehaviour previousTracking;
    private TrackingBehaviour m_NormalTracking;
    private bool m_FirstStartWhileSceneOpen = true;

    void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("CurrentVolume", 1);
        if(!m_InCombatScene)
        {
            cameraTracking = Camera.main.GetComponent<CameraTracking>();
        }

        if (isShowing && PlayerPrefs.GetInt("IsStarted") == 0)
            {
            if (!m_InCombatScene)
            {
                previousTracking = cameraTracking.m_TrackingBehaviour;
                m_NormalTracking = previousTracking;
                cameraTracking.m_TrackingBehaviour = menuCamera;
            }
            selected = m_Continue;
            SelectItem(selected);
            Menu.SetActive(true);
            SetScarletControlsEnabled(false);
        } 
         else if(PlayerPrefs.GetInt("IsStarted") == 1)
        {
            StartNewGame();
        } else if(!isShowing && FindObjectOfType<AreaEnterTextController>() != null)
        {
            FindObjectOfType<AreaEnterTextController>().StartFadeIn();
        }
    }

    void Update()
    {

        if (isShowing)
        {
            if (!m_InCombatScene)
            {
                if (cameraTracking.m_TrackingBehaviour != menuCamera)
                {
                    cameraTracking.m_TrackingBehaviour = menuCamera;
                }
            }
            if (Input.GetButtonDown("Vertical") || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (Input.GetAxis("Vertical") < 0 || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (selected == MenuItems.Length - 1)
                    {
                        selected = m_Continue;
                    }
                    else
                    {
                        selected += 1;
                    }
                }
                else
                {
                    if (selected == m_Continue)
                    {
                        selected = MenuItems.Length - 1;
                    }
                    else
                    {
                        selected -= 1;
                    }
                }
                SelectItem(selected);
            }

            if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Attack"))
            {
                if (selected == m_NewGame)
                {
                    StartNewGame();
                }
                else if (selected == m_Continue)
                {
                    LoadGame();
                }
                else if (selected == m_Options)
                {
                    OpenOptions();
                }
                else if (selected == m_SelectChapter)
                {
                    OpenSelectChapter();
                }
                else if (selected == m_Credits)
                {
                    OpenCredits();
                }
                else if (selected == m_LeaveGame)
                {
                    CloseGame();
                }
            }
        }
    }


    private void SetScarletControlsEnabled(bool enabled)
    {
        PlayerControls controls = FindObjectOfType<PlayerControls>();
        PlayerCommand.s_IsMenuActive = !enabled;
        if (controls != null)
        {
            if (enabled)
                StartCoroutine(EnableCommandsInNextFrame(controls));
            else
                controls.DisableAllCommands();
        }
    }

    private IEnumerator EnableCommandsInNextFrame(PlayerControls controls)
    {
        yield return null;
        controls.EnableAllCommands();
    }


    public void SelectItem(int itemNumber)
    {
        if (selected != itemNumber)
        {
            selected = itemNumber;
        }
        for (int i = 0; i < MenuItems.Length; i++)
        {
            Image background = MenuItems[i].GetComponentInChildren<Image>();
            if (itemNumber == i)
            {
                background.sprite = Resources.Load<Sprite>("BackgroundPanelSelected");
            }
            else
            {
                background.sprite = Resources.Load<Sprite>("BackgroundPanel");
            }
        }
    }

    private void ZoomToScarlet()
    {
        isShowing = false;
        if (!m_InCombatScene)
        {
            if (m_NormalTracking != null)
            {
                cameraTracking.m_TrackingBehaviour = m_NormalTracking;
            }
            else
            {
                cameraTracking.m_TrackingBehaviour = previousTracking;
            }
        }
        SetScarletControlsEnabled(true);
    }

    public void StartNewGame()
    {
        // Reset Player Prefs except volume
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetFloat("CurrentVolume", AudioListener.volume);
        if (SceneManager.GetActiveScene().name.Equals("city_exploration_level") && m_FirstStartWhileSceneOpen)
        {
            m_FirstStartWhileSceneOpen = false;
            ZoomToScarlet();
            Menu.SetActive(false);
            GetComponent<MainMenuController>().enabled = false;
            GetComponentInParent<IngameMenuController>().enabled = true;

            ActivateScarletControls();
            SequencedActionController.Instance.PlayCutscene("Opening");
        } else
        {
            PlayerPrefs.SetInt("IsStarted", 1);
            SceneManager.LoadScene("city_exploration_level");
        }
    }

    public void LoadGame()
    {
        if (selected == m_Continue)
        {
            ////TODO: Check load game with checkpoints
            var currentScene = SceneManager.GetActiveScene().name;
            if (!PlayerPrefs.GetString("CurrentLevel", "city_exploration_level").Equals(currentScene))
            {
                SceneManager.LoadScene(PlayerPrefs.GetString("CurrentLevel", "city_exploration_level"));
            }
            else
            {
                ZoomToScarlet();
                Menu.SetActive(false);
                GetComponent<MainMenuController>().enabled = false;
                GetComponentInParent<IngameMenuController>().enabled = true;

                ActivateScarletControls();
                //if (currentScene.Equals("city_exploration_level"))
                //{
                //    SequencedActionController.Instance.PlayCutscene("Opening");
                //}
            }
        }
    }

    public void OpenOptions()
    {
        if (selected == m_Options)
        {
            Menu.SetActive(false);
            GetComponent<OptionsMenuController>().enabled = true;
            GetComponent<OptionsMenuController>().Activate();
            GetComponent<MainMenuController>().enabled = false;
        }
    }

    public void OpenCredits()
    {
        if (selected == m_Credits)
        {
            SceneManager.LoadScene("credits_without_cutscene");
        }
    }

    public void OpenSelectChapter()
    {
        if (selected == m_SelectChapter)
        {
            Menu.SetActive(false);
            GetComponent<LevelSelectMenuController>().enabled = true;
            GetComponent<LevelSelectMenuController>().Activate();
            GetComponent<MainMenuController>().enabled = false;
        }
    }

    public void Activate(TrackingBehaviour normalTracking)
    {
        isShowing = true;
        if (!m_InCombatScene)
        {
            previousTracking = cameraTracking.m_TrackingBehaviour;
            if (normalTracking != null) m_NormalTracking = normalTracking;
            cameraTracking.m_TrackingBehaviour = menuCamera;
        }
        selected = m_Continue;
        SelectItem(selected);
        Menu.SetActive(true);
        SetScarletControlsEnabled(false);
    }

    private void ActivateScarletControls()
    {
        ControlsActivator activator = FindObjectOfType<ControlsActivator>();
        if (activator != null)
            activator.ActivateControls();
    }

    public void CloseGame()
    {
        if (selected == m_LeaveGame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

}
