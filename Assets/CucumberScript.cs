using System.Collections;
using UnityEngine;
using System.Text.RegularExpressions;

public class CucumberScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;
    public KMSelectable cucumberSel;
    public SpriteRenderer cucumberRend;
    public Material[] cucumberMats;
    public Sprite[] cucumberSprites;

    int remainingBites = 5;
    int timer;
    bool contest;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        cucumberSel.OnInteract += delegate () { PressButton(cucumberSel); return false; };
    }

    void Start()
    {
        contest = (int)bomb.GetTime() < 120;
        if (!contest)
            Debug.LogFormat("[Cucumber #{0}] Enjoy your cucumber!", moduleId);
        else
            Debug.LogFormat("[Cucumber #{0}] Looks like you are in a cucumber eating contest!", moduleId);
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true)
        {
            pressed.AddInteractionPunch();
            if (!contest)
            {
                audio.PlaySoundAtTransform("eat" + Random.Range(1, 3), transform);
                Debug.LogFormat("[Cucumber #{0}] You took a bite out of the cucumber.", moduleId);
                if (timer > 0)
                {
                    audio.PlaySoundAtTransform("choke", transform);
                    GetComponent<KMBombModule>().HandleStrike();
                    Debug.LogFormat("[Cucumber #{0}] You did not wait 30 seconds between the last bite and choked! Strike!", moduleId);
                }
                remainingBites--;
            }
            else
            {
                Debug.LogFormat("[Cucumber #{0}] You shoved the cucumber down your throat.", moduleId);
                remainingBites = 0;
                audio.PlaySoundAtTransform("swallow", transform);
                StartCoroutine(DelayAnnounce());
            }
            if (remainingBites == 0)
            {
                moduleSolved = true;
                GetComponent<KMBombModule>().HandlePass();
                cucumberRend.material = null;
                cucumberRend.sprite = null;
                Debug.LogFormat("[Cucumber #{0}] Cucumber successfully eaten (solved)!", moduleId);
                return;
            }
            cucumberRend.sprite = cucumberSprites[remainingBites - 1];
            cucumberRend.material = cucumberMats[remainingBites - 1];
            StopAllCoroutines();
            StartCoroutine(ChokeHazard());
        }
    }

    IEnumerator ChokeHazard()
    {
        timer = 30;
        while (timer > 0)
        {
            int prev = (int)bomb.GetTime() % 10;
            while (prev == (int)bomb.GetTime() % 10) yield return null;
            timer--;
        }
    }

    IEnumerator DelayAnnounce()
    {
        yield return new WaitForSecondsRealtime(1f);
        audio.PlaySoundAtTransform("contest", transform);
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press [Presses the cucumber]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            cucumberSel.OnInteract();
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (remainingBites > 0)
        {
            while (timer > 0) yield return true;
            cucumberSel.OnInteract();
        }
    }
}