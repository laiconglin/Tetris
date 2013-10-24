using UnityEngine;
using System.Collections;

public class StartMenu : MonoBehaviour
{
	
	public GUISkin menuskin = null;
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnGUI ()
	{   
		GUI.skin = menuskin;
		var sw = Screen.width;
		var sh = Screen.height;

		if (GUI.Button (new Rect (50, sh / 2, sw - 100, sh / 4), "TOUCH TO START")) {
			Application.LoadLevel ("TetrisPro");
		}   
	}
}
