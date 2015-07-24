using UnityEngine;
using System.Collections;
using Image = UnityEngine.UI.Image;

public class TriggeredUI : MonoBehaviour {

	// Use this for initialization
	public bool fadeOutAuto = false;
	public float activeFor = 3f;

	private const float transitionSpeed = .2f;
	private Image img;

	private bool isActive = false;
	private float activeTime = 0f;
	private bool isTriggered = false;

	void Start () {
		img = GetComponent<Image> ();
		img.canvasRenderer.SetAlpha (0f);
	}
	
	// Update is called once per frame
	void Update () {
		if (isActive) {
			activeTime += Time.deltaTime;

			if(activeTime > activeFor) {
				Hide();
			}
		}
	}
	
	void FadeIn() {
		//img.canvasRenderer.SetAlpha (0f);
		img.CrossFadeAlpha (1f, transitionSpeed, false);
	}

	void FadeOut() {
		//img.canvasRenderer.SetAlpha (1f);
		img.CrossFadeAlpha (0f, transitionSpeed, false);
	}

	public void Show() {
		if (fadeOutAuto && isTriggered)
			return;

		if (fadeOutAuto) {
			activeTime = 0f;
			isActive = true;
		}

		isTriggered = true;
		FadeIn ();
	}

	public void Hide() {
		isActive = false;
		FadeOut ();
	}
}
