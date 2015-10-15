using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	private int menuShow = 0;
	private string AnimName = "menu";
	private float resetTime = 0.00F;

	// Use this for initialization
	void Start () {
	
	}
	
	public void cargarEscena(string escena){
		Application.LoadLevel (escena);
	}

	public void Salir(){
		Application.Quit();
	}

	public void showMenu(){
		Animation panelPrincipal = transform.GetComponent<Animation> ();

		if(menuShow == 0){
			panelPrincipal[AnimName].speed = 1; 
			panelPrincipal[AnimName].time = resetTime;
			panelPrincipal.Play(AnimName);
			menuShow = 1;
		}else if(menuShow == 1){
			panelPrincipal[AnimName].speed = -1; 
			resetTime =panelPrincipal[AnimName].time;
			panelPrincipal[AnimName].time = panelPrincipal[AnimName].length;
			panelPrincipal.Play(AnimName);
			menuShow = 0;
		}

	}
}
