using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorOption : MonoBehaviour {
    public ColorPuzzle colorPuzzle;

    Image image;

	// Use this for initialization
	void Start () {
        image = GetComponent<Image>();
	}
	
    public void SendAnswer()
    {
        colorPuzzle.JudgeAnswer(image.color);
    }
}
