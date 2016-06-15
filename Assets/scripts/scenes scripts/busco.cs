using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Facebook.MiniJSON;
using System.Collections.Generic;

public class busco : MonoBehaviour {

	public GameObject btnDesde;
	public GameObject btnHasta;
	private MainController GMS;

	public GameObject DDDdesde;
	public GameObject DDHasta;

	public GameObject busco_ciudad;
	public string busco_sexo;

	public GameObject menuObj;
	public GameObject headerUImagen;

	// Use this for initialization
	void Start () {
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();

		headerUImagen.GetComponent<Image>().sprite = GMS.spriteSquareFromFile( GMS.userData.foto );

		if (GMS.userData.busco_sexo != "") {
			GameObject.Find (GMS.userData.busco_sexo).GetComponent<Toggle> ().isOn = true;
		} else {

			if (GMS.userData.sexo == "HOMBRE") {
				GameObject.Find ("MUJER").GetComponent<Toggle> ().isOn = true;
			} else {
				GameObject.Find ("HOMBRE").GetComponent<Toggle> ().isOn = true;
			}
		}

		btnDesde.GetComponentInChildren<Text> ().text = GMS.userData.busco_edad_min;
		btnHasta.GetComponentInChildren<Text> ().text = GMS.userData.busco_edad_max;

		//busco_ciudad.GetComponent<Text>().text = GMS.userData.ciudad;

		generarDesdeHasta ();

		if (PlayerPrefs.GetString ("busco_completo") != "1") {
			menuObj.SetActive(false);
		}

		if (GMS.userData.busco_en_face != "") {
			bool isOnFB = (GMS.userData.busco_en_face == "SI") ? true : false;
			GameObject.Find ("BuscarFB").GetComponent<Toggle> ().isOn = isOnFB;
		}

		/*if (GMS.userData.busco_cerca != "") {
			bool isOnFB = (GMS.userData.busco_cerca == "SI") ? true : false;
			GameObject.Find ("BuscarCerca").GetComponent<Toggle> ().isOn = isOnFB;
		}*/

		GMS.userChangeData = (UserData)GMS.userData.Clone();
	}
	
	// Update is called once per frame
	/*void Update () {
	
	}*/

	public void selectDesde(GameObject opcion){
		btnDesde.GetComponentInChildren<Text> ().text = opcion.GetComponentInChildren<Text> ().text;
		Debug.Log (opcion.GetComponentInChildren<Text> ().text);
		GMS.userChangeData.busco_edad_min = opcion.GetComponentInChildren<Text> ().text;
		DDDdesde.SetActive (false);
	}

	public void selectHasta(GameObject opcion){
		btnHasta.GetComponentInChildren<Text> ().text = opcion.GetComponentInChildren<Text> ().text;
		Debug.Log (opcion.GetComponentInChildren<Text> ().text);
		GMS.userChangeData.busco_edad_max = opcion.GetComponentInChildren<Text> ().text;
		DDHasta.SetActive (false);
	}

	public void generarDesdeHasta(){

		GameObject OptionDefaultDesde = DDDdesde.transform.Find("PanelMask/PanelScroll/Option").gameObject;
		GameObject OptionDefault = DDHasta.transform.Find("PanelMask/PanelScroll/Option").gameObject;

		Debug.Log ("OptionDefaultDesde name: " + OptionDefaultDesde.name);
		Debug.Log ("OptionDefault name: " + OptionDefault.name);

		for(int i = 18; i < 51; i++){
			GameObject clone = Instantiate(OptionDefaultDesde, OptionDefaultDesde.transform.position, OptionDefaultDesde.transform.rotation) as GameObject;
			clone.transform.SetParent(OptionDefaultDesde.transform.parent);
			clone.transform.localScale = new Vector3(1, 1, 1);
			clone.GetComponentInChildren<Text> ().text = i.ToString();

			GameObject clone2 = Instantiate(OptionDefault, OptionDefault.transform.position, OptionDefault.transform.rotation) as GameObject;
			clone2.transform.SetParent(OptionDefault.transform.parent);
			clone2.transform.localScale = new Vector3(1, 1, 1);
			clone2.GetComponentInChildren<Text> ().text = i.ToString();

		}
		
		Destroy (OptionDefaultDesde);
		Destroy (OptionDefault);
		
		DDDdesde.SetActive(false);
		DDHasta.SetActive(false);
	}

	public void changeSexo(string sexo_){
		busco_sexo = sexo_;
	}

	public void changeBuscarFb(GameObject option){
		if (option.GetComponent<Toggle> ().isOn) {
			GMS.userChangeData.busco_en_face = "SI";
		} else {
			GMS.userChangeData.busco_en_face = "NO";

			//obtener amigos de facebook
			/*if(FB.IsLoggedIn) {
				FB.Login("email,user_birthday,user_friends", AuthCallback);
			}*/
		}
	}

	public void changeBuscarCerca(GameObject option){
		if (option.GetComponent<Toggle> ().isOn) {
			GMS.userChangeData.busco_cerca = "SI";
			busco_ciudad.transform.parent.gameObject.SetActive(false);

		} else {
			GMS.userChangeData.busco_cerca = "NO";
			busco_ciudad.transform.parent.gameObject.SetActive(true);
			
			//obtener amigos de facebook
			/*if(FB.IsLoggedIn) {
				FB.Login("email,user_birthday,user_friends", AuthCallback);
			}*/
		}
	}

	void AuthCallback(FBResult result) {
		if(FB.IsLoggedIn) {
			FB.API("/me?fields=friends.limit(500)", Facebook.HttpMethod.GET, APICallback);
		} else {
			GMS.errorPopup("Ocurrio un error con el login de facebook, por favor intentalo nuevamente.");
			Debug.Log("User cancelled login");
		}
	}

	void APICallback(FBResult result){
		if (result.Error != null){
			// Let's just try again
			FB.API("me?fields=friends.limit(500)", Facebook.HttpMethod.GET, APICallback);
			return;
		}
		var dict = Json.Deserialize(result.Text) as Dictionary<string,object>;
		
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
	}


	public void submit(){

		if(!GMS.haveInet){
			GMS.errorPopup("Verifica tu conexion a internet");
		}else{
			
			GMS.userChangeData.busco_sexo = busco_sexo;
			GMS.userChangeData.busco_ciudad = busco_ciudad.GetComponent<Text>().text;

			GMS.showLoading(true);

			GMS.perfil_busco();

			PlayerPrefs.SetString("busco_completo", "1");
			StartCoroutine(gotoNext());
		}
		
	}

	private IEnumerator gotoNext(){
		yield return new WaitForSeconds (2);
		GMS.showLoading(false);
		Application.LoadLevel ("buscar");
	}

	public void loadMap() {
		Application.LoadLevel("mapa");
	}


}
