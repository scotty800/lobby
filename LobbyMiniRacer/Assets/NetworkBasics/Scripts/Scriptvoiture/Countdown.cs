using System.Collections;
using MyCarController;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    public GameObject CountDown;
    public AudioSource GetReady;
    public AudioSource GoAudio;
    public GameObject LapTimer;
    public AudioSource LevelMusic;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LapTimer.SetActive(false);
        StartCoroutine (CounStart());
    }

    IEnumerator CounStart() {
    yield return new WaitForSeconds(0.5f);
    Debug.Log("Starting Countdown...");
    CountDown.GetComponent<Text>().text = "3";

    if (!GetReady.isActiveAndEnabled)
    {
        Debug.LogWarning("GetReady AudioSource was disabled. Enabling it.");
        GetReady.enabled = true;
    }
    GetReady.Play();
    Debug.Log("Played GetReady sound for '3'");
    CountDown.SetActive(true);
    yield return new WaitForSeconds(1);

    CountDown.SetActive(false);
    CountDown.GetComponent<Text>().text = "2";
    GetReady.Play();
    Debug.Log("Played GetReady sound for '2'");
    CountDown.SetActive(true);
    yield return new WaitForSeconds(1);

    CountDown.SetActive(false);
    CountDown.GetComponent<Text>().text = "1";
    GetReady.Play();
    Debug.Log("Played GetReady sound for '1'");
    CountDown.SetActive(true);
    yield return new WaitForSeconds(1);

    CountDown.SetActive(false);
    GoAudio.Play();
    Debug.Log("Played GoAudio sound for 'GO'");
    LevelMusic.Play();
    LapTimer.SetActive(true);
    Debug.Log("LapTimer activated.");
    Debug.Log("CarController activated.");
    }
}

