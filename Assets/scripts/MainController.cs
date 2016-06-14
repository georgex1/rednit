using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine.UI;
/*using System.Data;*/

public class MainController : MonoBehaviour {

	public string dbName = "haak.db";
	private string appHash = "R3dN1t!";

	//private string responseURL = "https://thepastoapps.com/proyectos/rednit/response/response.php";
	//private string responseAssets = "https://thepastoapps.com/proyectos/rednit/response/assets/images/";
	//private string updateAppLink = "https://thepastoapps.com/proyectos/rednit/response/redirect.php";

	private string responseURL = "https://haakapp.com/response/response.php";
	private string responseAssets = "https://haakapp.com/response/assets/images/";
	private string updateAppLink = "https://haakapp.com/response/redirect.php";

	private string Uid;

	private float loadTime;
	private bool closeApp;
	private bool checkUpdate;

	public dbAccess db ;

	public UserData userData;
	public UserData userChangeData;
	public AmigoData amigoData;

	public bool haveInet;
	public bool checkingCon = false;

	public string prevScene = "home";

	public string defLat = "-34.5872407";
	public string defLng = "-58.4216301";

	//Plataforma (se usa para el control de version.
	//Se envia al servicio junto con el numero dee version
	public string platform;

	//para debug
	public bool isDebug;
	public string sendDataDebug;

	//notificaciones
	public notifications notificationsScript;
	public string appName;

	//popup
	public GameObject popup;
	public GameObject popupText;
	public GameObject popupButton;

	//updatePopup
	public GameObject updatePopupObject;
	public GameObject updatePopupObjectText;

	//loading
	public GameObject loading;
	public bool actualizando;

	//personas gallery
	public int CountPersonasGal = 0;
	public bool donwloadinGallery = false;
	public int appVersion;

	void OnGUI(){
		if (isDebug) {
			GUI.skin.label.fontSize = 20;
			GUI.Label (new Rect (0, Screen.height * 0.775f, Screen.width, Screen.height * 0.05f), "DEBUG : " + sendDataDebug);
		}
	}

	void createDb(){
		db.OpenDB(dbName);

		string[] cols = new string[]{"id", "email", "nombre", "fbid", "fecha_nacimiento", "sexo", "foto", "ciudad", "lat", "lng", "busco_ciudad", "busco_sexo", "busco_edad_min", "busco_edad_max", 
			"busco_en_face", "fb_friends", "busco_cerca", "busco_distancia", "latitude", "longitude", "busco_lat", "busco_long", "descripcion", "token"};
		string[] colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("usuarios", cols, colTypes);

		cols = new string[]{"id", "nombre", "edad", "sexo", "ciudad", "foto", "visto", "fbid", "latitude", "longitude", "busco_lat", "busco_long", "descripcion"};
		colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("personas", cols, colTypes);

		cols = new string[]{"id", "usuarios_id", "personas_id", "aceptado", "nombre", "email", "edad", "sexo", "ciudad", "foto", "latitude", "longitude", "busco_distancia", "busco_lat", "busco_long", "descripcion"};
		colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("amigos_usuarios", cols, colTypes);

		cols = new string[]{"id", "usuarios_id", "amigos_id", "aceptado", "nombre", "email", "edad", "sexo", "ciudad", "foto", "chat_group", "latitude", "longitude", "busco_distancia", "busco_lat", "busco_long", "descripcion"};
		colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("amigos", cols, colTypes);

		cols = new string[]{"id", "func", "sfields", "svalues"};
		colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("sync", cols, colTypes);

		cols = new string[]{"id", "usuarios_id", "foto", "isdefault"};
		colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("fotos_usuarios", cols, colTypes);

		db.CloseDB();
	}

	// Use this for initialization
	void Start () {

		Debug.Log("inicio rutina start");
		StartCoroutine (Location());
	

		Uid = "";
		isDebug = false;
		checkUpdate = true;
		loadTime = 0;
		db = GetComponent<dbAccess>();
		createDb ();
		appName = "H";

		if (!PlayerPrefs.HasKey ("busco_completo")) {
			PlayerPrefs.SetString("busco_completo", "0");
		}

		haveInet = false;
		checkConnection ();
		checkVersion ();

		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		DontDestroyOnLoad (transform.gameObject);

		if (Uid == "") {
			if (PlayerPrefs.HasKey ("Uid")) {
				Uid = PlayerPrefs.GetString ("Uid");
			} else {
				Uid = SystemInfo.deviceUniqueIdentifier;
				PlayerPrefs.SetString ("Uid", Uid);
			}
		}

		if (!PlayerPrefs.HasKey ("config_alerts")) {
			PlayerPrefs.SetString ("config_alerts", "true");
		} else {
			string config_alerts = PlayerPrefs.GetString ("config_alerts");
			if(config_alerts == "false"){
				notificationsScript.disableNotifs();
			}
		}

		StartCoroutine (call_sync());
		StartCoroutine (get_updates ());
		StartCoroutine (checkDownloadImages());

		StartCoroutine(updateLastCon(5f));

		try_download_persona_imagen("default.png");

		//ej sync:
		/*string[] fields = {"puntos", "kilometros", "perros_id", "usuarios_id"};
		string[] values = {"100", "150", "454545" , "3"};
		insert_sync(fields, values, "perros_puntos");*/

		//PlayerPrefs.DeleteAll ();


	}
	
	void Awake () {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 20;

		DontDestroyOnLoad (transform.gameObject);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		loadTime += Time.deltaTime;

		int roundedRestSeconds = Mathf.CeilToInt (loadTime);
		int displaySeconds = roundedRestSeconds % 60;
		
		/*if (Input.GetKeyDown(KeyCode.Escape)) {
			closeApp = true;
		}*/

		if (closeApp && loadTime > 1) {
			Application.Quit();
		}
		if (loadTime > 6) {
			loadTime = 0;
			checkUpdate = true;
			checkingCon = false;
		}

		if(displaySeconds == 5 && !checkingCon){
			checkingCon = true;
			checkConnection ();
		}

	}

	private IEnumerator updateLastCon(float timeCheck){
		yield return new WaitForSeconds (timeCheck);
		if (userData.id != 0) {
			WWWForm form = new WWWForm ();
			form.AddField ("appHash", appHash);
			form.AddField ("action", "update_lastcon");
			form.AddField ("usuarios_id", userData.id.ToString ());
			form.AddField ("uToken", userData.token);
			WWW www = new WWW (responseURL, form);
			StartCoroutine (WaitForRequest (www, "update_lastcon"));
			Debug.Log ("update last con user ID: " +  userData.id.ToString ());
		} else {
			StartCoroutine(updateLastCon(5f));
		}
	}

	void checkConnection(){

		if (!haveInet) {
			WWWForm form = new WWWForm ();
			form.AddField ("appHash", appHash);
			form.AddField ("action", "check_connection");
			WWW www = new WWW (responseURL, form);
			StartCoroutine (WaitForRequest (www, "check_connection"));
		}
	}

	void checkVersion(){
		
			WWWForm form = new WWWForm ();
			form.AddField ("appHash", appHash);
			form.AddField ("appVersion", appVersion);
			form.AddField ("platform", platform);	
			form.AddField ("action", "check_version");
			WWW www = new WWW (responseURL, form);
			StartCoroutine (WaitForRequest (www, "check_version"));
		}


	//send data inet
	public void sendData(string[] vars, string[] values, string action_, byte[] uploadImage = null){
		
		WWWForm form = new WWWForm();
		form.AddField("appHash", appHash);
		form.AddField("action", action_);
		form.AddField ("uToken", userData.token);
		
		int index=0;
		sendDataDebug = "preparando variables";

		foreach (string vars_ in vars) {
			if(vars_ != "fileUpload"){
				try{
					form.AddField(vars_, values[index]);
				}catch(Exception e){
					Debug.Log("error en variable: "+index + " | " + e);
					sendDataDebug = "error en variable: "+index;
				}
			}else{
				form.AddBinaryData("fileUpload", uploadImage);
			}
			index++;
		}

		sendDataDebug = "iniciando WWW";
		
		WWW www = new WWW(responseURL, form);
		StartCoroutine(WaitForRequest(www, action_));
		//Debug.Log(www.text);
		
	}

	private IEnumerator logoutN(){
		yield return new WaitForSeconds (1f);
		Application.Quit();
	}

	IEnumerator WaitForRequest(WWW www, string response){
		yield return www;
		
		// check for errors
		if (www.error == null){
			sendDataDebug = "WWW Ok!";

			Debug.Log("response!: " + response);

			//Debug.Log("WWW Ok!: " + www.text);

			IDictionary Wresponse = (IDictionary) MiniJSON.Json.Deserialize (www.text);

			string Wcontent_ = MiniJSON.Json.Serialize(Wresponse["content"]);
			string WarrayData_ = MiniJSON.Json.Serialize(Wresponse["arrayData"]);

			//Debug.Log("WWW content: " + Wcontent_);

			IDictionary Wresponse2 = (IDictionary) MiniJSON.Json.Deserialize ( Wcontent_ );
			IDictionary Wresponse3 = (IDictionary) MiniJSON.Json.Deserialize ( WarrayData_ );

			if((string)Wresponse["status"] == "error"){

				showLoading(false);

				if( (string)Wresponse2["mgs"] == "error token" ){
					NPBinding.UI.ShowAlertDialogWithSingleButton ("Alerta!", "Ha ocurrido un error en la autentificacion. Por favor vuelve a ingresar: " + response, "Aceptar", (string _buttonPressed)=>{
						if (_buttonPressed == "Aceptar") {
							logout();
							StartCoroutine (logoutN ());
						}
					});
				}else{
					errorPopup((string)Wresponse2["mgs"], (string)Wresponse2["toclose"]);
				}

				//errorPopup((string)Wresponse2["mgs"], (string)Wresponse2["toclose"]);
			}else{

				if(response == "check_connection"){
					haveInet = true;
				}

				if(response == "update_lastcon"){
					StartCoroutine(updateLastCon(5f));
				}

				if(response == "login_facebook"){
					showLoading(true);

					sendDataDebug = "entro a login_facebook";
					Debug.Log("login facebook OK! ID: "+ (string)Wresponse3["id"]);

					userData.id = int.Parse( (string)Wresponse3["id"] );
					userData.token = (string)Wresponse3["token"];

					if( (string)Wresponse2["hasArray"] != "0" ){
						string WarrayContent_ = MiniJSON.Json.Serialize(Wresponse["arrayContent"]);
						IDictionary WresponseContent = (IDictionary) MiniJSON.Json.Deserialize ( WarrayContent_ );


						for(int i = 1; i <= int.Parse( (string)Wresponse2["hasArray"] ); i++ ){
							IDictionary reponseContent = (IDictionary) MiniJSON.Json.Deserialize ( (string)WresponseContent[i.ToString()]  );

							userData.email = (string)reponseContent["email"];
							userData.nombre = (string)reponseContent["nombre"];
							userData.fecha_nacimiento = userData.app_format_date( (string)reponseContent["fecha_nacimiento"] );
							userData.sexo = (string)reponseContent["sexo"];

							userData.busco_lat = ( (string)reponseContent["busco_lat"] != "" ) ? (string)reponseContent["busco_lat"] : defLat;
							userData.busco_long = ( (string)reponseContent["busco_long"] != "" ) ? (string)reponseContent["busco_long"] : defLng;
							userData.busco_distancia = ( (string)reponseContent["busco_distancia"] != "" ) ? (string)reponseContent["busco_distancia"] : userData.busco_distancia;
							userData.busco_en_face = ( (string)reponseContent["busco_en_face"] != "" ) ? (string)reponseContent["busco_en_face"] : userData.busco_en_face;
							userData.busco_edad_min = ( (string)reponseContent["busco_edad_min"] != "" ) ? (string)reponseContent["busco_edad_min"] : userData.busco_edad_min;
							userData.busco_edad_max = ( (string)reponseContent["busco_edad_max"] != "" ) ? (string)reponseContent["busco_edad_max"] : userData.busco_edad_max;
							userData.busco_sexo = ( (string)reponseContent["busco_sexo"] != "" ) ? (string)reponseContent["busco_sexo"] : userData.busco_sexo;

							userData.foto = (string)reponseContent["foto"];
							userData.descripcion = (string)reponseContent["descripcion"];
							//userData.token = (string)reponseContent["token"];


							try_download_persona_imagen((string)reponseContent["foto"], true);

							saveUserData(true);
							StartCoroutine(delayChangePhoto());

							//bajar galeria del usuario
							downloadUserGallery( userData.id.ToString(), true );

							//upload_user_foto();
							StartCoroutine( redirect("perfil", 3f) );
							
							download_personas();

						}
					}

				}

				if(response == "get_personas"){
					string WarrayContent_ = MiniJSON.Json.Serialize(Wresponse["arrayContent"]);
					IDictionary WresponseContent = (IDictionary) MiniJSON.Json.Deserialize ( WarrayContent_ );
					
					//Debug.Log((string)Wresponse2["hasArray"]);
					if( (string)Wresponse2["hasArray"] != "0" ){
						for(int i = 1; i <= int.Parse( (string)Wresponse2["hasArray"] ); i++ ){
							//Debug.Log("posicion: " + i);

							IDictionary reponseContent = (IDictionary) MiniJSON.Json.Deserialize ( (string)WresponseContent[i.ToString()]  );

							db.OpenDB(dbName);

							//cargar personas
							string[] cols = new string[]{"id", "nombre", "edad", "sexo", "ciudad", "foto", "visto", "fbid", "latitude", "longitude", "descripcion"};
							string[] colsVals = new string[]{ (string)reponseContent["id"], (string)reponseContent["nombre"], (string)reponseContent["edad"], 
								(string)reponseContent["sexo"], (string)reponseContent["ciudad"], (string)reponseContent["foto"], "0", (string)reponseContent["fbid"], 
								(string)reponseContent["latitude"], (string)reponseContent["longitude"], (string)reponseContent["descripcion"] };
							
							db.InsertIgnoreInto("personas", cols, colsVals, (string)reponseContent["id"]);
							db.CloseDB();

							//intentar bajar imagen de la persona
							try_download_persona_imagen((string)reponseContent["foto"]);


						}
					}

					actualizando = false;
				}

				if(response == "get_amigos"){
					string WarrayContent_ = MiniJSON.Json.Serialize(Wresponse["arrayContent"]);
					IDictionary WresponseContent = (IDictionary) MiniJSON.Json.Deserialize ( WarrayContent_ );
					
					//Debug.Log((string)Wresponse2["hasArray"]);
					if( (string)Wresponse2["hasArray"] != "0" ){
						for(int i = 1; i <= int.Parse( (string)Wresponse2["hasArray"] ); i++ ){
							//Debug.Log("posicion: " + i);
							
							IDictionary reponseContent = (IDictionary) MiniJSON.Json.Deserialize ( (string)WresponseContent[i.ToString()]  );
							
							db.OpenDB(dbName);
							
							//cargar personas
							string[] cols = new string[]{ "usuarios_id", "personas_id", "aceptado", "nombre", "email", "edad", "sexo", "ciudad", "foto", "latitude", "longitude", "descripcion" };
							string[] colsVals = new string[]{ userData.id.ToString(), (string)reponseContent["id"], "0", (string)reponseContent["nombre"], 
								(string)reponseContent["email"], (string)reponseContent["edad"], (string)reponseContent["sexo"], (string)reponseContent["ciudad"], 
								(string)reponseContent["foto"], (string)reponseContent["latitude"], (string)reponseContent["longitude"], (string)reponseContent["descripcion"] };
							
							db.InsertIgnoreInto("amigos_usuarios", cols, colsVals, (string)reponseContent["id"]);
							db.CloseDB();

							//intentar bajar imagen de la persona
							try_download_persona_imagen((string)reponseContent["foto"]);
							

						}
					}
				}

				if(response == "get_amigos_aceptados"){
					string WarrayContent_ = MiniJSON.Json.Serialize(Wresponse["arrayContent"]);
					IDictionary WresponseContent = (IDictionary) MiniJSON.Json.Deserialize ( WarrayContent_ );
					
					//Debug.Log((string)Wresponse2["hasArray"]);
					if( (string)Wresponse2["hasArray"] != "0" ){
						for(int i = 1; i <= int.Parse( (string)Wresponse2["hasArray"] ); i++ ){
							//Debug.Log("posicion: " + i);
							
							IDictionary reponseContent = (IDictionary) MiniJSON.Json.Deserialize ( (string)WresponseContent[i.ToString()]  );
							
							db.OpenDB(dbName);
							
							//cargar personas
							string[] cols = new string[]{ "usuarios_id", "amigos_id", "aceptado", "nombre", "email", "edad", "sexo", "ciudad", "foto", "chat_group", "latitude", "longitude", "descripcion"};
							string[] colsVals = new string[]{ userData.id.ToString(), (string)reponseContent["id"], "1", (string)reponseContent["nombre"], 
								(string)reponseContent["email"], (string)reponseContent["edad"], (string)reponseContent["sexo"], (string)reponseContent["ciudad"], 
								(string)reponseContent["foto"], (string)reponseContent["chat_group"], (string)reponseContent["latitude"], (string)reponseContent["longitude"], (string)reponseContent["descripcion"] };
							
							db.InsertIgnoreInto("amigos", cols, colsVals, (string)reponseContent["id"]);
							db.CloseDB();

							//intentar bajar imagen de la persona
							try_download_persona_imagen((string)reponseContent["foto"]);
							

						}
					}
				}

				if(response == "upload_perfil"){
					sendDataDebug = "imagen subida";

					db.OpenDB(dbName);

					string[] colsUsuarios = new string[]{ "foto" };
					string[] colsUsuariosValues = new string[]{ userData.foto};

					db.UpdateSingle("usuarios", "foto", userData.foto, "id" , userData.id.ToString());


					colsUsuarios = new string[]{ "ciudad", "sexo", "fecha_nacimiento", "nombre", "email", "foto", "latitude", "longitude", "busco_distancia", "busco_lat", "busco_long", "descripcion" };
					colsUsuariosValues = new string[]{ userData.ciudad, userData.sexo, userData.fecha_nacimiento, userData.nombre, userData.email, userData.foto, userData.latitude, 
						userData.longitude, userData.busco_distancia, userData.busco_lat, userData.busco_long, userData.descripcion };

					Debug.Log("Inserto valores: " + colsUsuariosValues);

					db.InsertIgnoreInto("usuarios", colsUsuarios, colsUsuariosValues, userData.id.ToString());



					db.CloseDB();
					showLoading(false);
					Application.LoadLevel ("busco");
				}

				if(response == "upload_gallery"){
					sendDataDebug = "imagen galeria subida";
					
					db.OpenDB(dbName);

					string[] colsUsuarios = new string[]{ "id", "usuarios_id", "foto", "isdefault" };
					string[] colsUsuariosValues = new string[]{ (string)Wresponse3["id"], userData.id.ToString(), (string)Wresponse3["foto"], (string)Wresponse3["isdefault"]};

					db.InsertIntoSpecific ("fotos_usuarios", colsUsuarios, colsUsuariosValues);
					db.CloseDB();

					if( (string)Wresponse3["isdefault"] == "Y" ){
						userData.temp_galleryID = (string)Wresponse3["id"];
						change_gallery_default( (string)Wresponse3["id"] );
					}

					//verificar si no tiene foto de portada agregar la cargada
					//Debug.Log("query foto: " + "select foto from fotos_usuarios where usuarios_id = '" +userData.id+ "' and isdefault = 'Y' ");

					db.OpenDB(dbName);
					ArrayList result = db.BasicQueryArray ("select foto from fotos_usuarios where usuarios_id = '" +userData.id+ "' and isdefault = 'Y' ");
					db.CloseDB();

					if (result.Count > 0) {
						if( ((string[])result [0]) [0] == "default.png" ){
							userData.temp_galleryID = (string)Wresponse3["id"];
							change_gallery_default( (string)Wresponse3["id"] );
						}
					}else{
						userData.temp_galleryID = (string)Wresponse3["id"];
						change_gallery_default( (string)Wresponse3["id"] );
					}

					showLoading(false);


				}

				if(response == "delete_gallery"){
					db.OpenDB(dbName);

					db.BasicQueryInsert("delete from fotos_usuarios where id = '"+ (string)Wresponse3["id"] +"' ");

					//verificar si era la ultima poner default
					ArrayList result = db.BasicQueryArray ("select id from fotos_usuarios where usuarios_id = '" +userData.id+ "' ");
					if (result.Count == 0) {
						db.UpdateSingle("usuarios", "foto", "default.png", "id" , userData.id.ToString());
						userData.foto = "default.png";
					}

					showLoading(false);
					db.CloseDB();
				}

				if(response == "get_gallery"){
					Debug.Log("ingrese a get gallery");
					string WarrayContent_ = MiniJSON.Json.Serialize(Wresponse["arrayContent"]);
					IDictionary WresponseContent = (IDictionary) MiniJSON.Json.Deserialize ( WarrayContent_ );

					Debug.Log("is user in get_gallery: " + (string)Wresponse3["forGallery"]);

					if((string)Wresponse3["isUser"] == "N" && (string)Wresponse3["forGallery"] == "Y"){
						CountPersonasGal = int.Parse((string)Wresponse2["hasArray"]);
						Debug.Log("CountPersonasGal en get_gallery: " + CountPersonasGal);
						donwloadinGallery = true;
					}

					//Debug.Log((string)Wresponse2["hasArray"]);
					if( (string)Wresponse2["hasArray"] != "0" ){
						Debug.Log("entro 2");
						for(int i = 1; i <= int.Parse( (string)Wresponse2["hasArray"] ); i++ ){

							
							IDictionary reponseContent = (IDictionary) MiniJSON.Json.Deserialize ( (string)WresponseContent[i.ToString()]  );
							
							db.OpenDB(dbName);

							//cargar galeria
							string[] cols = new string[]{ "id", "usuarios_id", "foto", "isdefault"};
							string isDefault = ((string)reponseContent["isdefault"] == "1") ? "Y" : "N";
							string[] colsVals = new string[]{ (string)reponseContent["id"], (string)reponseContent["usuarios_id"], (string)reponseContent["foto"], isDefault};
							db.InsertIgnoreInto("fotos_usuarios", cols, colsVals, (string)reponseContent["id"]);

							if(isDefault == "Y" && (string)Wresponse3["isUser"] == "Y"){
								userData.temp_galleryID = (string)reponseContent["id"];
							}
							db.CloseDB();

							//intentar bajar imagen de la galeria
							try_download_persona_imagen((string)reponseContent["foto"], false, true);
							

						}
					}
				}

				if(response == "get_updates"){

					//if((string)Wresponse2["mgs"] == "puntos_especiales_updated"){

					string WarrayContent_ = MiniJSON.Json.Serialize(Wresponse["arrayContent"]);
					IDictionary WresponseContent = (IDictionary) MiniJSON.Json.Deserialize ( WarrayContent_ );

					//Debug.Log((string)Wresponse2["hasArray"]);
					if( (string)Wresponse2["hasArray"] != "0" ){
						for(int i = 1; i <= int.Parse( (string)Wresponse2["hasArray"] ); i++ ){
							//Debug.Log("posicion: " + i);

							//string dada = MiniJSON.Json.Serialize(WresponseContent["1"]);
							IDictionary reponseContent = (IDictionary) MiniJSON.Json.Deserialize ( (string)WresponseContent[i.ToString()]  );

							//actualizar db de puntos especiales
							//db.OpenDB(dbName);

							//ejemplo de update:
							/*
							if((string)Wresponse2["mgs"] == "notificaciones_updated"){
								string[] colsUsuarios = new string[]{"id", "titulo", "descripcion", "plataforma", "visto", "tipo", "serverupdate"};
								string[] colsUsuariosValues = new string[]{ (string)reponseContent["id"], (string)reponseContent["titulo"], (string)reponseContent["descripcion"], (string)reponseContent["plataforma"], (string)reponseContent["visto"], (string)reponseContent["tipo"], (string)reponseContent["serverupdate"] };
								
								db.InsertIgnoreInto("notificaciones", colsUsuarios, colsUsuariosValues, (string)reponseContent["id"]);
							}*/

							//db.CloseDB();
							//Debug.Log(reponseContent["puntos"]);
						}

						Debug.Log("updated: " + (string)Wresponse2["mgs"]);
					}
					//}
				}

				if(response == "sync"){
					db.OpenDB(dbName);
					db.BasicQueryInsert("delete from sync where id = '" +(string)Wresponse3["id"]+ "' ");
					db.CloseDB();
				}

				if(response == "check_version"){
					Debug.Log ("rutina de revision de version");
					//Debug.Log((string)Wresponse3["version"]);
					Debug.Log((string)Wresponse3["update"]);
					if((string)Wresponse3["update"] == "update") {
						Debug.Log ("actualizar");
						Debug.Log ((string)Wresponse3["updateMessage"]);
						updatePopup((string)Wresponse3["updateMessage"], "cerrar");

					}
				}

			}


		} else {
			haveInet = false;
			sendDataDebug = "WWW Error: "+www.error;
			Debug.Log("WWW Error: "+ www.error);

			StartCoroutine(updateLastCon(10f));
		}
	}

	private void populateUserData(IDictionary values){
		userData.email = (string)values["email"];
		userData.nombre = (string)values["nombre"];
		userData.fecha_nacimiento = (string)values["fecha_nacimiento"];
		userData.sexo = (string)values["sexo"];

		saveUserData (false);
	}

	public void downloadUserGallery(string userId, bool isUser = false, bool forGallery = false){
		string[] colsUsuarios = new string[]{ "usuarios_id", "isUser", "forGallery", "logUserId" };
		string isUser_ = (isUser) ? "Y" : "N";
		string forGallery_ = (forGallery) ? "Y" : "N";
		string[] colsUsuariosValues = new string[]{ userId, isUser_, forGallery_, userData.id.ToString() };
		
		sendData (colsUsuarios, colsUsuariosValues, "get_gallery");
	}

	/*public void changeProfile(){
		//string[] colsUsuarios = new string[]{ "fbid", "fecha_nacimiento", "ciudad", "sexo", "nombre", "email" };
		//string[] colsUsuariosValues = new string[]{ userData.fbid, userData.fecha_nacimiento, userData.ciudad, userData.sexo, userData.nombre, userData.email };
		
		//sendData (colsUsuarios, colsUsuariosValues, "changeProfile");
		upload_user_foto ();
	}*/

	private void saveUserData(bool isfb){

		db.OpenDB(dbName);
		
		string[] colsUsuarios = new string[]{ "id", "email", "nombre", "fbid", "fecha_nacimiento", "sexo", "foto", 
			"ciudad", "busco_sexo", "busco_ciudad", "busco_edad_min", "busco_edad_max", "busco_en_face", "fb_friends", 
			"busco_cerca", "busco_distancia", "latitude", "longitude", "busco_lat", "busco_long", "descripcion", "token"};

		ArrayList result = new ArrayList();
		if (isfb) {
			try{
				result = db.BasicQueryArray ("select fbid from usuarios where fbid = '" + userData.fbid + "' ");
			}catch(Exception e){
				sendDataDebug = "error con db";
			}
		} else {
			result = db.BasicQueryArray ("select email from usuarios where email = '"+userData.email+"' ");
		}

		string[] colsUsuariosValues = new string[]{ userData.id.ToString(), userData.email, userData.nombre, userData.fbid, userData.fecha_nacimiento, userData.sexo, userData.foto, 
			userData.ciudad, userData.busco_sexo, userData.busco_ciudad, userData.busco_edad_min, userData.busco_edad_max, userData.busco_en_face, userData.serializeFbFriends(), 
			userData.busco_cerca, userData.busco_distancia, userData.latitude, userData.longitude, userData.busco_lat, userData.busco_long, userData.descripcion, userData.token };
		
		if (result.Count == 0) {
			sendDataDebug = "count = 0 inserto usuario";
			db.InsertIntoSpecific ("usuarios", colsUsuarios, colsUsuariosValues);
		}

		db.CloseDB();
	}

	private IEnumerator delayChangePhoto(){
		yield return new WaitForSeconds (1);
		upload_foto_gallery (userData.foto, userData.temp_galleryID, "Y");
	}

	public void perfil_busco(){

		Debug.Log ("cambiado busco edad max: " + userData.busco_edad_max + " a " + userChangeData.busco_edad_max);

		//verificar si hubo cambio
		if (userData.busco_ciudad != userChangeData.busco_ciudad || userData.busco_edad_max != userChangeData.busco_edad_max || userData.busco_edad_min != userChangeData.busco_edad_min || 
		    userData.busco_en_face != userChangeData.busco_en_face || userData.busco_sexo != userChangeData.busco_sexo || userData.busco_cerca != userChangeData.busco_cerca || 
		    userData.busco_distancia != userChangeData.busco_distancia || userData.busco_lat != userChangeData.busco_lat || userData.busco_long != userChangeData.busco_long) {


			userData.busco_ciudad = userChangeData.busco_ciudad;
			userData.busco_edad_max = userChangeData.busco_edad_max;
			userData.busco_edad_min = userChangeData.busco_edad_min;
			userData.busco_en_face = userChangeData.busco_en_face;
			userData.busco_sexo = userChangeData.busco_sexo;
			userData.busco_cerca = userChangeData.busco_cerca;
			userData.busco_distancia = userChangeData.busco_distancia;
			userData.busco_lat = userChangeData.busco_lat;
			userData.busco_long = userChangeData.busco_long;
					
			Debug.Log("cambio busco");

			db.OpenDB(dbName);

			//borrar personas locales
			db.BasicQueryInsert("delete from personas where visto = '0' ");

			
			string[] colsUsuarios = new string[]{ "busco_ciudad", "busco_sexo", "busco_edad_min", "busco_edad_max", "busco_en_face", "fb_friends", "busco_distancia", "busco_lat", "busco_long" };
			string[] colsUsuariosValues = new string[]{ userData.busco_ciudad, userData.busco_sexo, userData.busco_edad_min, userData.busco_edad_max, userData.busco_en_face, userData.serializeFbFriends(), userData.busco_distancia, userData.busco_lat, userData.busco_long };
			db.InsertIgnoreInto("usuarios", colsUsuarios, colsUsuariosValues, userData.id.ToString());
			
			db.CloseDB();
			
			colsUsuarios = new string[]{ "busco_ciudad", "busco_sexo", "busco_edad_min", "busco_edad_max", "usuarios_id", "busco_en_face", "busco_distancia", "busco_lat", "busco_long" };
			colsUsuariosValues = new string[]{ userData.busco_ciudad, userData.busco_sexo, userData.busco_edad_min, userData.busco_edad_max, userData.id.ToString(), userData.busco_en_face, userData.busco_distancia, userData.busco_lat, userData.busco_long };

			sendData (colsUsuarios, colsUsuariosValues, "update_perfil_busco");
		}

	}

	public void download_personas(){
		if (userData.id != 0) {

			//buscar personas ya descargadas para no descargar de nuevo

			//buscar amigos de facebook si busco_en_face = NO

			string[] colsUsuarios = new string[]{ "usuarios_id" };
			string[] colsUsuariosValues = new string[]{ userData.id.ToString () };
		
			sendData (colsUsuarios, colsUsuariosValues, "get_personas");
		}
	}

	public void download_amigos(){
		if (userData.id != 0) {


			string amigos_list = "0";
			
			db.OpenDB(dbName);
			ArrayList result = db.BasicQueryArray ("select personas_id from amigos_usuarios where usuarios_id = '"+userData.id.ToString()+"' ");
			db.CloseDB();
			
			if (result.Count > 0) {
				foreach (string[] row_ in result) {
					amigos_list += "," + row_[0];
				}
			}


			string[] colsUsuarios = new string[]{ "usuarios_id", "lista_descargada" };
			string[] colsUsuariosValues = new string[]{ userData.id.ToString (), amigos_list };
			
			sendData (colsUsuarios, colsUsuariosValues, "get_amigos");
		}
	}

	public void download_amigos_aceptados(){
		if (userData.id != 0) {
			string amigos_list = "0";

			db.OpenDB(dbName);
			ArrayList result = db.BasicQueryArray ("select amigos_id from amigos where usuarios_id = '"+userData.id.ToString()+"' ");
			db.CloseDB();

			if (result.Count > 0) {
				foreach (string[] row_ in result) {
					amigos_list += "," + row_[0];
				}
			}

			string[] colsUsuarios = new string[]{ "usuarios_id", "lista_descargada" };
			string[] colsUsuariosValues = new string[]{ userData.id.ToString (), amigos_list };
			
			sendData (colsUsuarios, colsUsuariosValues, "get_amigos_aceptados");
		}
	}

	public void try_download_persona_imagen(string foto_, bool isUser = false, bool forGallery = false){
		string filepath = Application.persistentDataPath + "/" + foto_;
		//Debug.Log ("data path: " + Application.persistentDataPath + "/" + foto_);
		if (!File.Exists (filepath)) {
			/*Debug.Log ("!fileexist");
			Debug.Log ("foto_: " + foto_);
			Debug.Log ("isUser: " + isUser);
			Debug.Log ("forGallery: " + forGallery);*/
			StartCoroutine (downloadImg (foto_, isUser, forGallery));
		} else {
			if(!isUser && forGallery){
				CountPersonasGal--;
				//Debug.Log("CountPersonasGal en try_download_persona_imagen: " + CountPersonasGal);
			}
		}
	}

	private IEnumerator checkDownloadImages(){
		yield return new WaitForSeconds (3);
		db.OpenDB (dbName);
		
		ArrayList result = db.BasicQueryArray ("select foto from personas ");
		db.CloseDB ();

		if (result.Count > 0) {
			foreach (string[] row_ in result) {
				try_download_persona_imagen(row_[0]);
			}
		}

		db.OpenDB (dbName);
		result = db.BasicQueryArray ("select foto from amigos_usuarios ");
		db.CloseDB ();

		if (result.Count > 0) {
			foreach (string[] row_ in result) {
				try_download_persona_imagen(row_[0]);
			}
		}

		db.OpenDB (dbName);
		result = db.BasicQueryArray ("select foto from amigos ");
		db.CloseDB ();

		if (result.Count > 0) {
			foreach (string[] row_ in result) {
				try_download_persona_imagen(row_[0]);
			}
		}


		StartCoroutine (checkDownloadImages());
	}
	
	IEnumerator downloadImg (string image_name, bool isUser = false, bool forGallery = false){
		if (image_name != "") {
			Texture2D texture = new Texture2D (1, 1);
			Debug.Log ("try download image: " + responseAssets + image_name);
			WWW www = new WWW (responseAssets + image_name);
			yield return www;
			www.LoadImageIntoTexture (texture);
		
			byte[] ImgBytes = texture.EncodeToPNG ();
		
			File.WriteAllBytes (Application.persistentDataPath + "/" + image_name, ImgBytes);
			Debug.Log ("Ruta local de la foto: " + Application.persistentDataPath + "/" + image_name);
			Debug.Log ("isuser: " + isUser);
			if(isUser){
				StartCoroutine(delayChangePhoto());
			}else if(forGallery){
				CountPersonasGal--;
				Debug.Log("CountPersonasGal en downloadImg: " + CountPersonasGal);
			}
		}
	}

	public void upload_foto_gallery(string newPhoto, string newPhotoId, string isDefault = "N"){
		byte[] fileData = File.ReadAllBytes (Application.persistentDataPath + "/" + newPhoto);

		string[] cols2 = new string[] {
			"usuarios_id",
			"fileUpload",
			"usuario_foto",
			"id",
			"isDefault"
		};
		string[] data2 = new string[] {
			userData.id.ToString (),
			"imagen_usuario",
			newPhoto,
			newPhotoId,
			isDefault
		};

		Debug.Log ("data upload gallery: " + userData.id.ToString () + " - " +
		           "imagen_usuario" + " - " +
		           newPhoto + " - " +
		           newPhotoId + " - " + isDefault );

		try {
			sendData (cols2, data2, "upload_gallery", fileData);
		} catch (IOException e) {
			Debug.Log (e);
		}
	}

	public void delete_foto_gallery(string gallery_foto){

		//verificar si la borrada es default
		db.OpenDB (dbName);
		ArrayList result = db.BasicQueryArray ("select id from fotos_usuarios where id = '" +gallery_foto+ "' and isdefault = 'Y' ");
		db.CloseDB();

		string newDefault = "0";

		if (result.Count > 0) {
			//agregar nueva default
			db.OpenDB (dbName);
			result = db.BasicQueryArray ("select id from fotos_usuarios where usuarios_id = '" +userData.id+ "' and isdefault = 'N' ");
			if (result.Count > 0) {
				newDefault = ((string[])result [0]) [0];
				change_gallery_default(newDefault);
			}
			db.CloseDB();
		}

		string[] cols2 = new string[] {
			"usuarios_id",
			"id",
			"newDefault"
		};
		string[] data2 = new string[] {
			userData.id.ToString (),
			gallery_foto,
			newDefault
		};
		
		try {
			sendData (cols2, data2, "delete_gallery");
		} catch (IOException e) {
			Debug.Log (e);
		}
	}

	public void change_gallery_default(string imageId){
		db.OpenDB (dbName);
		db.UpdateSingle("fotos_usuarios", "isdefault", "N", "usuarios_id" , userData.id.ToString());
		db.UpdateSingle("fotos_usuarios", "isdefault", "Y", "id" , imageId);


		//set default image in user table
		userData.temp_galleryID = imageId;
		db.UpdateSingle("usuarios", "foto", userData.getGalleryPhoto(), "id" , userData.id.ToString());
		userData.foto = userData.getGalleryPhoto ();
		db.CloseDB ();

		string[] cols2 = new string[] {
			"foto_id",
			"usuario_id",
		};
		string[] data2 = new string[] {
			imageId,
			userData.id.ToString()
		};
		
		try {
			sendData (cols2, data2, "default_gallery");
		} catch (IOException e) {
			Debug.Log (e);
		}
	}
	
	public void upload_user_foto(){

		if (userData.foto != userData.temp_img) {
			userData.foto = userData.temp_img;
			byte[] fileData = File.ReadAllBytes (Application.persistentDataPath + "/" + userData.foto);

			string[] cols2 = new string[] {
				"usuarios_id",
				"fbid",
				"fileUpload",
				"usuario_foto",
				"fecha_nacimiento",
				"ciudad",
				"sexo",
				"nombre",
				"email",
				"latitude",
				"longitude",
				"descripcion",
				"isnewfoto"
			};
			string[] data2 = new string[] {
				userData.id.ToString (),
				userData.fbid,
				"imagen_usuario",
				userData.foto,
				userData.fecha_nacimiento,
				userData.ciudad,
				userData.sexo,
				userData.nombre,
				userData.email,
				userData.latitude,
				userData.longitude,
				userData.descripcion,
				"1"
			};

			try {
				sendData (cols2, data2, "upload_perfil", fileData);
			} catch (IOException e) {
				Debug.Log (e);
			}
		} else {
			
			string[] cols2 = new string[] {
				"usuarios_id",
				"fbid",
				"usuario_foto",
				"fecha_nacimiento",
				"ciudad",
				"sexo",
				"nombre",
				"email",
				"latitude",
				"longitude",
				"descripcion"
			};
			string[] data2 = new string[] {
				userData.id.ToString (),
				userData.fbid,
				userData.foto,
				userData.fecha_nacimiento,
				userData.ciudad,
				userData.sexo,
				userData.nombre,
				userData.email,
				userData.latitude,
				userData.longitude,
				userData.descripcion,
			};
			
			try {
				sendData (cols2, data2, "upload_perfil");
			} catch (IOException e) {
				Debug.Log (e);
			}
		}


	}

	public int generateId(){
		int timestamp = (int)Math.Truncate((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
		return timestamp;
	}

	public void loginFacebook(){

		userData.foto = userData.temp_img;
		byte[] fileData = File.ReadAllBytes (Application.persistentDataPath + "/" + userData.foto);
		
		string[] colsUsuarios = new string[]{ "email", "nombre", "fbid", "fecha_nacimiento", "sexo", "plataforma", "regid", "usuario_foto", "fileUpload", "latitude", "longitude"};
		string[] colsUsuariosValues = new string[]{ userData.email, userData.nombre, userData.fbid, userData.fecha_nacimiento, userData.sexo, userData.plataforma, userData.reg_id, userData.foto, "imagen_usuario", userData.latitude, userData.longitude };
		
		sendData (colsUsuarios, colsUsuariosValues, "login_facebook", fileData);


	}

	private IEnumerator get_updates(){
		yield return new WaitForSeconds (10);

		//ej de call updates
		//call_updates ("puntos_especiales");
		download_personas();
		download_amigos ();
		download_amigos_aceptados ();

		StartCoroutine (get_updates ());
	}

	public void call_updates( string table ){
		if (haveInet) {
			db.OpenDB (dbName);

			ArrayList result = db.BasicQueryArray ("select serverupdate from " + table + " order by serverupdate DESC limit 1");
			string serverUpdate = "2015-01-01";
			if (result.Count > 0) {
				serverUpdate = ((string[])result [0]) [0];
			}
			db.CloseDB ();
			string[] cols = new string[]{ "table", "serverupdate"};
			string[] values = new string[]{ table, serverUpdate};
			sendData (cols, values, "get_updates");
		}
	}

	private IEnumerator call_sync(){
		yield return new WaitForSeconds (10);
		sync ();
	}

	private IEnumerator redirect(string escene_, float seconds){
		yield return new WaitForSeconds (seconds);

		Debug.Log ("redirect escene: " + escene_ + " en " + seconds);
		showLoading(false);
		Application.LoadLevel (escene_);
	}

	public void sync(){
		Debug.Log ("sync....");
		//sync foto perro
		db.OpenDB(dbName);

		ArrayList result = db.BasicQueryArray ("select id, func, sfields, svalues from sync order by id ASC limit 1");
		db.CloseDB();

		if (result.Count > 0) {
			if(haveInet){
				string[] cols = new string[]{ "id", "func", "fields", "values", "usuarios_id"};
				string[] values = new string[]{ ((string[])result [0])[0] , ((string[])result [0])[1], ((string[])result [0])[2], ((string[])result [0])[3], userData.id.ToString() };
				sendData (cols, values, "sync");
			}
			//((string[])result [0])[0];
		}


		StartCoroutine (call_sync ());
	}

	public void insert_sync(string[] fields, string[] values, string sync_func){

		db.OpenDB (dbName);

		string fields_json = MiniJSON.Json.Serialize(fields);
		string values_json = MiniJSON.Json.Serialize(values);

		//Debug.Log ("insertar en sync fields: " + fields_json + "values: " + values_json + " func: " + sync_func);
		string newSyncId = getSyncNewId ();

		string[] colsF = new string[]{ "id", "func", "sfields", "svalues"};
		string[] colsV = new string[]{ newSyncId, sync_func, fields_json, values_json };
		
		db.InsertIntoSpecific("sync", colsF, colsV);

		db.CloseDB ();
	}

	private string getSyncNewId(){
		db.OpenDB("millasperrunas.db");
		ArrayList result = db.BasicQueryArray ("select id from sync order by id DESC limit 1");
		db.CloseDB();
		
		string newId = "1";
		
		if (result.Count > 0) {
			newId = ((string[])result [0]) [0];
			int newIdInt = int.Parse(newId)+1;
			newId = newIdInt.ToString();
		}
		
		return newId;
	}

	public bool validEmail(string emailaddress){
		return System.Text.RegularExpressions.Regex.IsMatch(emailaddress, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
	}

	/*public void errorPopup(string error = "Error", string toclose = ""){

		popup.SetActive (true);
		popupText.GetComponent<Text> ().text = error;
		if (toclose == "1") {
			popupButton.SetActive (false);
		} else {
			popupButton.SetActive (true);
		}
	}*/
	private string errorChrs = "";
	public void errorPopup(string error = "Error", string toclose = ""){
		
		string btnText = "Aceptar";
		/*if (toclose != "" && toclose != null) {
			btnText = "Entiendo";
			errorChrs = error;
		}*/
		errorChrs = error;
		NPBinding.UI.ShowAlertDialogWithSingleButton ("Alerta!", error, btnText, (string _buttonPressed)=>{
			if (_buttonPressed == "Aceptar") {
				Debug.Log("aceptado");
			}
			if (_buttonPressed == "Entiendo") {
				errorPopup(errorChrs, "1");
			}
		}); 
	}

	public void updatePopup(string error = "Error", string toclose = ""){
		
		updatePopupObject.SetActive (true);
		updatePopupObjectText.GetComponent<Text> ().text = error;
		if (toclose == "1") {
			updatePopupObject.SetActive (false);
		} else {
			updatePopupObject.SetActive (true);
		}
	}

	public void showLoading(bool show = true){
		loading.SetActive (show);
	}

	public void closePopup(){
		popup.SetActive (false);
		updatePopupObject.SetActive (false);
	}
	public void updateApp(){
		Application.OpenURL(updateAppLink);
	}

	public bool checkImageExists(string image_){
		string filepath = Application.persistentDataPath + "/" + image_;
		if (File.Exists (filepath)) {
			return true;
		} else {
			return false;
		}
	}

	public Sprite spriteFromFile(string image_){
		//Debug.Log ("spriteFromFile: " + image_);
		Sprite sprite = new Sprite ();
		if (image_ != "") {
			string filepath = Application.persistentDataPath + "/" + image_;
			if (File.Exists (filepath)) {

				byte[] fileData = File.ReadAllBytes (filepath);
				Texture2D tex = new Texture2D (2, 2);
				tex.LoadImage (fileData); //..this will auto-resize the texture dimensions.

				//Debug.Log (tex.width + "x" + tex.height);
				sprite = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0f, 0f));
			}else{
				Texture2D tex = Resources.Load("default") as Texture2D;
				sprite = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0f, 0f));
			}
		} else {
			Texture2D tex = Resources.Load("default") as Texture2D;
			sprite = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0f, 0f));
		}
		return sprite;
	}

	public Sprite spriteSquareFromFile(string image_){
		Debug.Log ("spriteFromFile: " + image_);
		Sprite sprite = new Sprite ();
		if (image_ != "") {
			
			byte[] fileData = File.ReadAllBytes (Application.persistentDataPath + "/" + image_);
			Texture2D tex = new Texture2D (2, 2);
			tex.LoadImage (fileData); //..this will auto-resize the texture dimensions.
			
			//convertirla en cuadrado
			Texture2D texSq = new Texture2D(2, 2, TextureFormat.ARGB32, false);;
			
			if(tex.width > tex.height){
				
				int restText = tex.width - tex.height;
				int restText2 =  restText/2 ;
				texSq = new Texture2D(tex.height, tex.height, TextureFormat.ARGB32, false);
				
				int xi = 1;
				for (var y = 1; y <= texSq.height; y++) {
					xi = 1;
					for (var x = restText2; x < ( tex.width - restText2 ); x++) {
						texSq.SetPixel (xi, y, tex.GetPixel (x, y));
						xi ++;
					}
				}
			}
			
			if(tex.height > tex.width){
				
				int restText = tex.height - tex.width;
				int restText2 = restText/2 ;
				
				texSq = new Texture2D(tex.width, tex.width, TextureFormat.ARGB32, false);
				
				int yi = 1;
				for (var x = 1; x <= texSq.width; x++) {
					yi = 1;
					for (var y = restText2; y < ( tex.height - restText2 ); y++) {
						texSq.SetPixel (x, yi, tex.GetPixel (x, y));
						yi ++;
					}
				}
			}
			
			texSq.Apply();
			
			sprite = Sprite.Create (texSq, new Rect (0, 0, texSq.width, texSq.height), new Vector2 (0f, 0f));
			
		} else {
			Texture2D tex = Resources.Load("default (2)") as Texture2D;
			sprite = Sprite.Create (tex, new Rect (0, 0, tex.width, tex.height), new Vector2 (0f, 0f));
		}
		return sprite;
	}

	public IEnumerator saveTextureToFile(Texture2D /*savedTexture */loadTexture, string fileName, char tosave){
		yield return new WaitForSeconds(0.5f);

		Debug.Log ("texture inicial: " + fileName + " " + loadTexture.width + "x" + loadTexture.height);

		int newWidth = 300;
		int newHeigth =  (newWidth * loadTexture.height / loadTexture.width) ;

		Texture2D savedTexture = ScaleTexture (loadTexture, newWidth, newHeigth);

		Debug.Log ("guardar textura en imagen: " + fileName + " " + savedTexture.width + "x" + savedTexture.height);

		Texture2D newTexture = new Texture2D(savedTexture.width, savedTexture.height, TextureFormat.ARGB32, false);
		
		newTexture.SetPixels(0,0, savedTexture.width, savedTexture.height, savedTexture.GetPixels());
		newTexture.Apply();
		if(tosave == 'u'){
			userData.ImgBytes = newTexture.EncodeToPNG ();
			userData.temp_img = fileName;
			
			File.WriteAllBytes (Application.persistentDataPath + "/" + userData.temp_img, userData.ImgBytes);
			Debug.Log (Application.persistentDataPath + "/" + userData.temp_img);
		}

		if (tosave == 'g') {
			File.WriteAllBytes (Application.persistentDataPath + "/" + fileName, newTexture.EncodeToPNG ());
			Debug.Log (Application.persistentDataPath + "/" + fileName);
		}
	}

	public string getActualDate(){
		return DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss");
	}

	public string getHour(){
		return DateTime.Now.ToString ("HH");
	}


	private Texture2D ScaleTexture(Texture2D source,int targetWidth,int targetHeight) {
		Texture2D result=new Texture2D(targetWidth,targetHeight,source.format,false);
		float incX=(1.0f / (float)targetWidth);
		float incY=(1.0f / (float)targetHeight);
		for (int i = 0; i < result.height; ++i) {
			for (int j = 0; j < result.width; ++j) {
				Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
				result.SetPixel(j, i, newColor);
			}
		}
		result.Apply();
		return result;
	}

	//agrego esto, no se si es el lugar correcto. 
	// Busco la lat y long del usuario para asignarle

	public IEnumerator Location()
	{
		Debug.Log ("inicio el proceso de deteccion de ubicacion");
		// First, check if user has location service enabled
		if (!Input.location.isEnabledByUser)
			yield break;
		
		// Start service before querying location
		Input.location.Start();
		
		// Wait until service initializes
		int maxWait = 20;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			yield return new WaitForSeconds(1);
			maxWait--;
		}
		
		// Service didn't initialize in 20 seconds
		if (maxWait < 1)
		{
			print("Timed out");
			yield break;
		}
		
		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed)
		{
			print("Unable to determine device location");
			yield break;
		}
		else
		{
			// Access granted and location value could be retrieved
			print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

			userData.latitude = Input.location.lastData.latitude.ToString();
			userData.longitude = Input.location.lastData.longitude.ToString();

			if(Input.location.lastData.latitude == 0f){
				userData.latitude = defLat;
				userData.longitude = defLng;
			}
		}
		
		// Stop service if there is no need to query location updates continuously
		Input.location.Stop();
	}

	public void logout(){
		db.OpenDB(dbName);

		db.BasicQueryInsert("DROP TABLE usuarios");
		db.BasicQueryInsert("DROP TABLE personas");
		db.BasicQueryInsert("DROP TABLE amigos_usuarios");
		db.BasicQueryInsert("DROP TABLE amigos");
		db.BasicQueryInsert("DROP TABLE fotos_usuarios");

		db.CloseDB ();
	}

}
