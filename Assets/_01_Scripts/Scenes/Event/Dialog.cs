using Game.Scenes.Core;
using System.Collections;
using TMPro;
using UnityEngine;

public class Dialog : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private string[] lines;
    [SerializeField] private float textSpeed = 0.03f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip typeSfx;
    [SerializeField] private float sfxVolume = 0.6f;
    [SerializeField] private float sfxCooldown = 0.03f; // verhindert "Machinegun"!!!
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private bool skipSpaces = true;

    private int index;
    private Coroutine typingRoutine;
    private float lastSfxTime;

    void Start()
    {
        StartDialog();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                if (typingRoutine != null)
                    StopCoroutine(typingRoutine);

                textComponent.text = lines[index];
            }
        }
    }

    private void StartDialog()
    {
        index = 0;
        textComponent.text = string.Empty;

        typingRoutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        textComponent.text = string.Empty;

        foreach (char c in lines[index])
        {
            textComponent.text += c;

            TryPlayTypeSfx(c);

            yield return new WaitForSeconds(textSpeed);
        }

        typingRoutine = null;
    }

    private void TryPlayTypeSfx(char c)
    {
        if (audioSource == null || typeSfx == null)
            return;

        if (skipSpaces && char.IsWhiteSpace(c))
            return;


        if (char.IsPunctuation(c))
            return;


        if (Time.unscaledTime - lastSfxTime < sfxCooldown)
            return;

        lastSfxTime = Time.unscaledTime;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(typeSfx, sfxVolume);
    }

    private void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            typingRoutine = StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);

            GameFlowController.Current.EventComplete();
        }
    }
}