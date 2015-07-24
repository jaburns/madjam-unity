using UnityEngine;
using System.Collections;
using Image = UnityEngine.UI.Image;

public class UITrigger : MonoBehaviour {

	public TriggeredUI target;
	
	void Start() {
	}

	void Update() {
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		var sh = col.GetComponent<SwitchHitter>();
		if (sh == null) return;
		
		target.Show ();
	}

}