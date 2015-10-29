using UnityEngine;
using System.Collections;
using Facebook.MiniJSON;

public class loginFacebook : MonoBehaviour {
	private bool enabled = false;

	void Start () {
		FB.Init(SetInit, OnHideUnity);
	}

	public void btnLogin(){
		FB.Login("email,user_birthday,user_friends", AuthCallback);
		//Application.LoadLevel("loader");
	}

	void AuthCallback(FBResult result) {
		if(FB.IsLoggedIn) {
			FB.API("/me?fields=id,name,first_name,last_name,email,birthday,gender", Facebook.HttpMethod.GET, APICallback);

			loginFacebookData (result.Text);

			/*GameObject GM = GameObject.FindGameObjectWithTag ("mainController");
			mainController GMS = GM.GetComponent<mainController>();
			GMS.FbUserId = FB.UserId;
			print (GMS.FbUserId);*/

			Debug.Log(FB.UserId);
		} else {
			GameObject GM = GameObject.Find ("MainController");
			MainController GMS = GM.GetComponent<MainController>();

			GMS.errorPopup("Ocurrio un error con el login de facebook, por favor intentalo nuevamente.");
			Debug.Log("User cancelled login");
		}
	}
	
	private void SetInit() {
		enabled = true; 
		print ("fb init");
		// "enabled" is a magic global; this lets us wait for FB before we start rendering
	}
	
	private void OnHideUnity(bool isGameShown) {
		/*if (!isGameShown) {
			// pause the game - we will need to hide
			Time.timeScale = 0;
		} else {
			// start the game back up - we're getting focus again
			Time.timeScale = 1;
		}*/
	}

	void APICallback(FBResult result){
		if (result.Error != null) {
			// Let's just try again
			FB.API ("/me?fields=id,name,first_name,last_name,email,birthday,gender", Facebook.HttpMethod.GET, APICallback);
			return;
		}

		loginFacebookData (result.Text);

		//GMS.SendMessage ("loginFacebook");
	}

	private void loginFacebookData(string txt_){
		GameObject GM = GameObject.Find ("MainController");
		MainController GMS = GM.GetComponent<MainController>();
		
		IDictionary search = (IDictionary) Json.Deserialize (txt_);
		
		GMS.userData.email = "";
		GMS.userData.fbid = (string)search ["id"];
		GMS.userData.nombre = (string)search ["first_name"] + ' '+ (string)search ["last_name"];
		if (search ["email"] != null) {
			GMS.userData.email = (string)search ["email"];
		}
		if ((string)search ["gender"] == "female") {
			GMS.userData.sexo = "MUJER";
		} else {
			GMS.userData.sexo = "HOMBRE";
		}
		GMS.userData.foto = "";
		GMS.userData.ciudad = "";
		GMS.userData.fecha_nacimiento = "01/01/1984";
		
		if (search ["birthday"] != null) {
			string fechaNac = (string)search ["birthday"];
			string[] splitFechaNac = fechaNac.Split('/');
			
			if(splitFechaNac.Length > 2){
				GMS.userData.date_year = splitFechaNac[2];
				GMS.userData.date_month = splitFechaNac[0];
				GMS.userData.date_day = splitFechaNac[1];
				
				GMS.userData.fecha_nacimiento = GMS.userData.date_day + "/" + GMS.userData.date_month + "/" + GMS.userData.date_year;
			}
		}
		
		GMS.loginFacebook ();
	}
}

