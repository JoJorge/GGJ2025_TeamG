using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPuzzle : MonoBehaviour {
    public Text roundTimeText;
    public Image circle;
    public Image cross;
    public Image[] colorPalette;
    public Image[] colorOption;
    public Color[] possibleMixedColor;
    public float roundTimer = 0.0f;
    public float roundTime = 4.0f;

    public Color answerColor;
    bool isTimerStarted;
    bool isSolvingQuiz;

	// Use this for initialization
	void Start () {
        roundTimer = 0.0f;
        roundTimeText.text = roundTimer.ToString("0.0");
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            /* difference
            Color colorDiff; 
            float hm, sm, vm;

            mixedColor.color = possibleMixedColor[round++];
            Color.RGBToHSV(mixedColor.color, out hm, out sm, out vm);
            colorPalette1.color = Random.ColorHSV(0.0f, 1.0f, sm, sm, vm, vm);
            colorDiff = mixedColor.color - colorPalette1.color;
            colorPalette2.color = new Color(colorDiff.r, colorDiff.g, colorDiff.b, 1.0f);*/
            GetQuiz();
        }

        if (isTimerStarted) {
            roundTimer += Time.deltaTime;
            roundTimeText.text = (roundTime - roundTimer).ToString("0.0");
            if (isSolvingQuiz && roundTimer >= roundTime)
            {
                isTimerStarted = false;
                cross.enabled = true;
            }
        }

    }

    void GetQuiz()
    {
        if (!isSolvingQuiz)
        {
            Color startColor = Random.ColorHSV(0.0f, 1.0f, 0.7f, 1.0f, 0.7f, 1.0f);
            Color endColor = Random.ColorHSV(0.0f, 1.0f, 0.7f, 1.0f, 0.7f, 1.0f);

            isSolvingQuiz = true;
            circle.enabled = false;
            cross.enabled = false;
            // Fill gradient.
            for (int i = 0; i < colorPalette.Length; i++)
                colorPalette[i].color = Color.Lerp(startColor, endColor, (float)i / (colorPalette.Length - 1));
            answerColor = colorPalette[Mathf.CeilToInt((float)colorPalette.Length / 2) - 1].color;
            colorPalette[Mathf.CeilToInt((float)colorPalette.Length / 2) - 1].color = Color.white;

            // Set up options
            for (int i = 0; i < colorOption.Length; i++)
                colorOption[i].color = Random.ColorHSV(0.0f, 1.0f, 0.7f, 1.0f, 0.7f, 1.0f);
            colorOption[Random.Range(0, colorOption.Length)].color = answerColor;

            roundTimer = 0.0f;
            isTimerStarted = true;
        }
    }

    public void JudgeAnswer(Color playerAnswer)
    {
        if (isSolvingQuiz)
        {
            circle.enabled = false;
            cross.enabled = false;
            colorPalette[Mathf.CeilToInt((float)colorPalette.Length / 2) - 1].color = playerAnswer;
            if (playerAnswer == answerColor)
            {
                circle.enabled = true;
            }
            else
            {
                cross.enabled = true;
            }

            isTimerStarted = false;
            isSolvingQuiz = false;
        }
    }
}
