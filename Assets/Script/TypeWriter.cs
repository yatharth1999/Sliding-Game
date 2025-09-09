using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI headingText;
    [TextArea] public string[] bodyTextArray; // assign your mission text
    [TextArea] public string[] headingTextArray; // assign your heading text
    public int textIndex = 0; // which text to show
    public float typingSpeed = 0.05f;   // seconds per character
    public AudioSource typeSound;       // assign typewriter click sound
    public AudioClip dingSound;         // final ding sound

    public Button continueButton; // button to show after typing
    public List<Button> answerButtons; // buttons for answers

    public static TypewriterEffect inst;
    void Start()
    {
        if (inst == null)
            inst = this;
        else
            Destroy(gameObject);

    }

    

    public IEnumerator ShowText()
    {
        continueButton.gameObject.SetActive(false);
        foreach (Button btn in answerButtons)
        {
            btn.gameObject.SetActive(false);
        }
        bodyText.text = "";
        headingText.text = headingTextArray[textIndex];
        foreach (char c in bodyTextArray[textIndex])
        {
            bodyText.text += c;
            if (typeSound && c != ' ' && c != '\n') // play only for letters
                typeSound.PlayOneShot(typeSound.clip);
            yield return new WaitForSeconds(typingSpeed);
        }


        // Play final ding
        if (dingSound && typeSound)
            typeSound.PlayOneShot(dingSound);

        // Check if all letters are typed and show the button
        if (bodyText.text.Length == bodyTextArray[textIndex].Length)
        {
            // Assuming showButton() is a method in this class or accessible to it.
            ShowButton();
        }
    }

    public void ShowButton()
    {
        if (textIndex < 5)
        {
            continueButton.gameObject.SetActive(true);
            return;
        }
        else
        {
            foreach (Button btn in answerButtons)
            {
                btn.gameObject.SetActive(true);
            }
        }

    }

    public void NextButton()
    {
        textIndex++;
        ImageSlidingPuzzle.inst.Rebuild();
        PanelSwitcher.inst.ShowPuzzleGamePanel();


    }

    public void RightAnswer()
    {
        headingText.text = "CASE CLOSED";
        StartCoroutine(ShowRightAnswerText());
        }

        IEnumerator ShowRightAnswerText()
        {
        foreach (Button btn in answerButtons)
        {
            btn.gameObject.SetActive(false);
        }
        bodyText.text = "";
        string rightAnswerText = "Finally, you found\nthe killer.\n\nWe won the case.\nThe city trusts us\nonce again.";
        foreach (char c in rightAnswerText)
        {
            bodyText.text += c;
            if (typeSound && c != ' ' && c != '\n')
            typeSound.PlayOneShot(typeSound.clip);
            yield return new WaitForSeconds(typingSpeed);
        }

        if (dingSound && typeSound)
            typeSound.PlayOneShot(dingSound);
    }
    public void WrongAnswer()
    {
        headingText.text = "CASE FAILED";
        StartCoroutine(ShowWrongAnswerText());
        }

        IEnumerator ShowWrongAnswerText()
        {
        foreach (Button btn in answerButtons)
        {
            btn.gameObject.SetActive(false);
        }
        bodyText.text = "";
        string wrongAnswerText = "Wrong suspect chosen.\nThe trail went cold.\n\nThe AI beats us.\nThe shop shutters.";
        foreach (char c in wrongAnswerText)
        {
            bodyText.text += c;
            if (typeSound && c != ' ' && c != '\n')
            typeSound.PlayOneShot(typeSound.clip);
            yield return new WaitForSeconds(typingSpeed);
        }

        if (dingSound && typeSound)
            typeSound.PlayOneShot(dingSound);
    }
}
