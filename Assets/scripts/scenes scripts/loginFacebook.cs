using UnityEngine;
using System.Collections;
using Facebook.MiniJSON;
using System.Collections.Generic;

public class loginFacebook : MonoBehaviour {
	private bool enabled = false;
	private MainController GMS;

	void Start () {
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();

		FB.Init(SetInit, OnHideUnity);
	}

	public void btnLogin(){
		FB.Login("email,user_birthday,user_friends", AuthCallback);
		//Application.LoadLevel("loader");
	}

	void AuthCallback(FBResult result) {
		if(FB.IsLoggedIn) {
			FB.API("/me?fields=id,name,first_name,last_name,email,birthday,gender,friends.limit(500)", Facebook.HttpMethod.GET, APICallback);

			//loginFacebookData (result.Text);

			/*
			GMS.FbUserId = FB.UserId;
			print (GMS.FbUserId);*/

			Debug.Log(FB.UserId);
		} else {

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
			FB.API ("/me?fields=id,name,first_name,last_name,email,birthday,gender,friends.limit(500)", Facebook.HttpMethod.GET, APICallback);
			return;
		}

		loginFacebookData (result.Text);

		//GMS.SendMessage ("loginFacebook");
	}

	private void loginFacebookData(string txt_){

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

		try{
			if (search ["birthday"] != null) {
				string fechaNac = (string)search ["birthday"];
				string[] splitFechaNac = fechaNac.Split('/');
				
				if(splitFechaNac.Length > 2){
					GMS.userData.date_year = splitFechaNac[2];
					GMS.userData.date_month = splitFechaNac[0];
					GMS.userData.date_day = splitFechaNac[1];
					GMS.userData.busco_en_face = "SI";
					GMS.userData.busco_cerca = "NO";
					GMS.userData.busco_distancia = "100";

					GMS.userData.fecha_nacimiento = GMS.userData.date_day + "/" + GMS.userData.date_month + "/" + GMS.userData.date_year;
				}
			}
		}catch(UnityException e){
			Debug.Log("error! " + e);
		}

		var dict = Json.Deserialize(txt_) as Dictionary<string,object>;
		
		object friendsH;
		var friends = new List<object>();
		string friendName;
		
		if(dict.TryGetValue ("friends", out friendsH)) {
			friends = (List<object>)(((Dictionary<string, object>)friendsH) ["data"]);
			if(friends.Count > 0) {
				foreach(object ff in friends){
					var friendDict = ((Dictionary<string,object>)(ff));
					Debug.Log((string)friendDict["id"]);
					GMS.userData.fbFriends.Add( (string)friendDict["id"] );
				}
			}
		}


		GMS.showLoading(true);
		FB.API("me/picture?type=large", Facebook.HttpMethod.GET, GetPicture);

	}

	private void GetPicture(FBResult result)
	{
		if (result.Error == null) {

			GMS.userData.temp_galleryID = GMS.generateId ().ToString ();

			GMS.userData.temp_img = GMS.userData.temp_galleryID  + ".png";
			StartCoroutine (GMS.saveTextureToFile (result.Texture, GMS.userData.temp_img, 'u'));

			StartCoroutine (loginFacebook_ ());

			/*Image img = UIFBProfilePic.GetComponent<Image>();
			img.sprite = Sprite.Create(result.Texture, new Rect(0,0, 128, 128), new Vector2());*/
		} else {
			GMS.showLoading(false);
			GMS.errorPopup("Ocurrio un error con el login de facebook, por favor intentalo nuevamente.");
		}
		
	}

	private IEnumerator loginFacebook_(){
		yield return new WaitForSeconds (3);
		//GMS.showLoading(false);
		GMS.loginFacebook ();
	}
}

