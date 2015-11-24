using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine.UI;
/*using System.Data;*/

public class MainController : MonoBehaviour {

	public string dbName = "rednit.db";
	private string appHash = "R3dN1t!";

	private string responseURL = "http://thepastoapps.com/proyectos/rednit/response/response.php";
	private string responseAssets = "http://thepastoapps.com/proyectos/rednit/response/assets/images/";

	//private string responseURL = "http://localhost/betterpixel/rednit/response/response.php";
	//private string responseAssets = "http://localhost/betterpixel/rednit/response/assets/images/";
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

	//loading
	public GameObject loading;

	public bool actualizando;

	void OnGUI(){
		if (isDebug) {
			GUI.skin.label.fontSize = 20;
			GUI.Label (new Rect (0, Screen.height * 0.775f, Screen.width, Screen.height * 0.05f), "DEBUG : " + sendDataDebug);
		}
	}

	void createDb(){
		db.OpenDB(dbName);

		string[] cols = new string[]{"id", "email", "nombre", "fbid", "fecha_nacimiento", "sexo", "foto", "ciudad", "lat", "lng", "busco_ciudad", "busco_sexo", "busco_edad_min", "busco_edad_max", "busco_en_face", "fb_friends", "busco_cerca", "busco_distancia"};
		string[] colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("usuarios", cols, colTypes);

		cols = new string[]{"id", "nombre", "edad", "sexo", "ciudad", "foto", "visto", "fbid"};
		colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("personas", cols, colTypes);

		cols = new string[]{"id", "usuarios_id", "personas_id", "aceptado", "nombre", "email", "edad", "sexo", "ciudad", "foto"};
		colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("amigos_usuarios", cols, colTypes);

		cols = new string[]{"id", "usuarios_id", "amigos_id", "aceptado", "nombre", "email", "edad", "sexo", "ciudad", "foto", "chat_group"};
		colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("amigos", cols, colTypes);

		cols = new string[]{"id", "func", "sfields", "svalues"};
		colTypes = new string[]{"INT", "TEXT", "TEXT", "TEXT"};
		db.CreateTable ("sync", cols, colTypes);

		db.CloseDB();
	}

	// Use this for initialization
	void Start () {

		Uid = "";
		isDebug = false;
		checkUpdate = true;
		loadTime = 0;
		db = GetComponent<dbAccess>();
		createDb ();
		appName = "Rednit";

		if (!PlayerPrefs.HasKey ("busco_completo")) {
			PlayerPrefs.SetString("busco_completo", "0");
		}

		haveInet = false;
		checkConnection ();

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

		//ej sync:
		/*string[] fields = {"puntos", "kilometros", "perros_id", "usuarios_id"};
		string[] values = {"100", "150", "454545" , "3"};
		insert_sync(fields, values, "perros_puntos");*/

		//PlayerPrefs.DeleteAll ();


	}
	
	void Awake () {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 15;

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

	void checkConnection(){

		if (!haveInet) {
			WWWForm form = new WWWForm ();
			form.AddField ("appHash", appHash);
			form.AddField ("action", "check_connection");
			WWW www = new WWW (responseURL, form);
			StartCoroutine (WaitForRequest (www, "check_connection"));
		}
	}

	//send data inet
	public void sendData(string[] vars, string[] values, string action_, byte[] uploadImage = null){
		
		WWWForm form = new WWWForm();
		form.AddField("appHash", appHash);
		form.AddField("action", action_);
		
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

	IEnumerator WaitForRequest(WWW www, string response){
		yield return www;
		
		// check for errors
		if (www.error == null){
			sendDataDebug = "WWW Ok!";
			Debug.Log("WWW Ok!: " + www.text);

			IDictionary Wresponse = (IDictionary) MiniJSON.Json.Deserialize (www.text);

			string Wcontent_ = MiniJSON.Json.Serialize(Wresponse["content"]);
			string WarrayData_ = MiniJSON.Json.Serialize(Wresponse["arrayData"]);

			//Debug.Log("WWW content: " + Wcontent_);

			IDictionary Wresponse2 = (IDictionary) MiniJSON.Json.Deserialize ( Wcontent_ );
			IDictionary Wresponse3 = (IDictionary) MiniJSON.Json.Deserialize ( WarrayData_ );

			if((string)Wresponse["status"] == "error"){

				errorPopup((string)Wresponse2["mgs"], (string)Wresponse2["toclose"]);
			}else{

				if(response == "check_connection"){
					haveInet = true;
				}

				if(response == "login_facebook"){
					sendDataDebug = "entro a login_facebook";
					Debug.Log("login facebook OK! ID: "+ (string)Wresponse3["id"]);

					userData.id = int.Parse( (string)Wresponse3["id"] );

					saveUserData(true);

					//upload_user_foto();
					Application.LoadLevel ("perfil");

					download_personas();
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
							string[] cols = new string[]{"id", "nombre", "edad", "sexo", "ciudad", "foto", "visto", "fbid"};
							string[] colsVals = new string[]{ (string)reponseContent["id"], (string)reponseContent["nombre"], (string)reponseContent["edad"], (string)reponseContent["sexo"], (string)reponseContent["ciudad"], (string)reponseContent["foto"], "0", (string)reponseContent["fbid"] };
							
							db.InsertIgnoreInto("personas", cols, colsVals, (string)reponseContent["id"]);

							//intentar bajar imagen de la persona
							try_download_persona_imagen((string)reponseContent["foto"]);

							db.CloseDB();
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
							string[] cols = new string[]{ "usuarios_id", "personas_id", "aceptado", "nombre", "email", "edad", "sexo", "ciudad", "foto" };
							string[] colsVals = new string[]{ userData.id.ToString(), (string)reponseContent["id"], "0", (string)reponseContent["nombre"], (string)reponseContent["email"], (string)reponseContent["edad"], (string)reponseContent["sexo"], (string)reponseContent["ciudad"], (string)reponseContent["foto"] };
							
							db.InsertIgnoreInto("amigos_usuarios", cols, colsVals, (string)reponseContent["id"]);
							
							//intentar bajar imagen de la persona
							try_download_persona_imagen((string)reponseContent["foto"]);
							
							db.CloseDB();
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
							string[] cols = new string[]{ "usuarios_id", "amigos_id", "aceptado", "nombre", "email", "edad", "sexo", "ciudad", "foto", "chat_group"};
							string[] colsVals = new string[]{ userData.id.ToString(), (string)reponseContent["id"], "1", (string)reponseContent["nombre"], (string)reponseContent["email"], (string)reponseContent["edad"], (string)reponseContent["sexo"], (string)reponseContent["ciudad"], (string)reponseContent["foto"], (string)reponseContent["chat_group"] };
							
							db.InsertIgnoreInto("amigos", cols, colsVals, (string)reponseContent["id"]);
							
							//intentar bajar imagen de la persona
							try_download_persona_imagen((string)reponseContent["foto"]);
							
							db.CloseDB();
						}
					}
				}

				if(response == "upload_perfil"){
					sendDataDebug = "imagen subida";

					db.OpenDB(dbName);

					string[] colsUsuarios = new string[]{ "foto" };
					string[] colsUsuariosValues = new string[]{ userData.foto};

					db.UpdateSingle("usuarios", "foto", userData.foto, "id" , userData.id.ToString());


					colsUsuarios = new string[]{ "ciudad", "sexo", "fecha_nacimiento", "nombre", "email", "foto" };
					colsUsuariosValues = new string[]{ userData.ciudad, userData.sexo, userData.fecha_nacimiento, userData.nombre, userData.email, userData.foto };
					db.InsertIgnoreInto("usuarios", colsUsuarios, colsUsuariosValues, userData.id.ToString());

					db.CloseDB();
					showLoading(false);
					Application.LoadLevel ("busco");
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
							db.OpenDB(dbName);

							//ejemplo de update:
							/*
							if((string)Wresponse2["mgs"] == "notificaciones_updated"){
								string[] colsUsuarios = new string[]{"id", "titulo", "descripcion", "plataforma", "visto", "tipo", "serverupdate"};
								string[] colsUsuariosValues = new string[]{ (string)reponseContent["id"], (string)reponseContent["titulo"], (string)reponseContent["descripcion"], (string)reponseContent["plataforma"], (string)reponseContent["visto"], (string)reponseContent["tipo"], (string)reponseContent["serverupdate"] };
								
								db.InsertIgnoreInto("notificaciones", colsUsuarios, colsUsuariosValues, (string)reponseContent["id"]);
							}*/

							db.CloseDB();
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
			}


		} else {
			haveInet = false;
			sendDataDebug = "WWW Error: "+www.error;
			Debug.Log("WWW Error: "+ www.error);
		}
	}

	private void populateUserData(IDictionary values){
		userData.email = (string)values["email"];
		userData.nombre = (string)values["nombre"];
		userData.fecha_nacimiento = (string)values["fecha_nacimiento"];
		userData.sexo = (string)values["sexo"];

		saveUserData (false);
	}

	/*public void changeProfile(){
		//string[] colsUsuarios = new string[]{ "fbid", "fecha_nacimiento", "ciudad", "sexo", "nombre", "email" };
		//string[] colsUsuariosValues = new string[]{ userData.fbid, userData.fecha_nacimiento, userData.ciudad, userData.sexo, userData.nombre, userData.email };
		
		//sendData (colsUsuarios, colsUsuariosValues, "changeProfile");
		upload_user_foto ();
	}*/

	private void saveUserData(bool isfb){

		db.OpenDB(dbName);
		
		string[] colsUsuarios = new string[]{ "id", "email", "nombre", "fbid", "fecha_nacimiento", "sexo", "foto", "ciudad", "busco_sexo", "busco_ciudad", "busco_edad_min", "busco_edad_max", "busco_en_face", "fb_friends", "busco_cerca", "busco_distancia"};

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

		string[] colsUsuariosValues = new string[]{ userData.id.ToString(), userData.email, userData.nombre, userData.fbid, userData.fecha_nacimiento, userData.sexo, userData.foto, userData.ciudad, userData.busco_sexo, userData.busco_ciudad, userData.busco_edad_min, userData.busco_edad_max, userData.busco_en_face, userData.serializeFbFriends(), userData.busco_cerca, userData.busco_distancia };
		
		if (result.Count == 0) {
			sendDataDebug = "count = 0 inserto usuario";
			db.InsertIntoSpecific ("usuarios", colsUsuarios, colsUsuariosValues);
		}

		db.CloseDB();
	}

	public void perfil_busco(){

		Debug.Log ("cambiado busco edad max: " + userData.busco_edad_max + " a " + userChangeData.busco_edad_max);

		//verificar si hubo cambio
		if (userData.busco_ciudad != userChangeData.busco_ciudad || userData.busco_edad_max != userChangeData.busco_edad_max || userData.busco_edad_min != userChangeData.busco_edad_min || 
		    userData.busco_en_face != userChangeData.busco_en_face || userData.busco_sexo != userChangeData.busco_sexo || userData.busco_cerca != userChangeData.busco_cerca || 
		    userData.busco_distancia != userChangeData.busco_distancia) {


			userData.busco_ciudad = userChangeData.busco_ciudad;
			userData.busco_edad_max = userChangeData.busco_edad_max;
			userData.busco_edad_min = userChangeData.busco_edad_min;
			userData.busco_en_face = userChangeData.busco_en_face;
			userData.busco_sexo = userChangeData.busco_sexo;
			userData.busco_cerca = userChangeData.busco_cerca;
			userData.busco_distancia = userChangeData.busco_distancia;
					
			Debug.Log("cambio busco");

			db.OpenDB(dbName);

			//borrar personas locales
			db.BasicQueryInsert("delete from personas where visto = '0' ");

			
			string[] colsUsuarios = new string[]{ "busco_ciudad", "busco_sexo", "busco_edad_min", "busco_edad_max", "busco_en_face", "fb_friends" };
			string[] colsUsuariosValues = new string[]{ userData.busco_ciudad, userData.busco_sexo, userData.busco_edad_min, userData.busco_edad_max, userData.busco_en_face, userData.serializeFbFriends() };
			db.InsertIgnoreInto("usuarios", colsUsuarios, colsUsuariosValues, userData.id.ToString());
			
			db.CloseDB();
			
			colsUsuarios = new string[]{ "busco_ciudad", "busco_sexo", "busco_edad_min", "busco_edad_max", "usuarios_id", "busco_en_face" };
			colsUsuariosValues = new string[]{ userData.busco_ciudad, userData.busco_sexo, userData.busco_edad_min, userData.busco_edad_max, userData.id.ToString(), userData.busco_en_face };

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
			string[] colsUsuarios = new string[]{ "usuarios_id" };
			string[] colsUsuariosValues = new string[]{ userData.id.ToString () };
			
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

	private void try_download_persona_imagen(string foto_){
		string filepath = Application.persistentDataPath + "/" + foto_;
		if (!File.Exists (filepath)) {
			StartCoroutine( downloadImg(foto_) );
		}
	}

	private IEnumerator checkDownloadImages(){
		yield return new WaitForSeconds (3);
		db.OpenDB (dbName);
		
		ArrayList result = db.BasicQueryArray ("select foto from personas ");
		if (result.Count > 0) {
			foreach (string[] row_ in result) {
				try_download_persona_imagen(row_[0]);
			}
		}

		result = db.BasicQueryArray ("select foto from amigos_usuarios ");
		if (result.Count > 0) {
			foreach (string[] row_ in result) {
				try_download_persona_imagen(row_[0]);
			}
		}

		result = db.BasicQueryArray ("select foto from amigos ");
		if (result.Count > 0) {
			foreach (string[] row_ in result) {
				try_download_persona_imagen(row_[0]);
			}
		}
		db.CloseDB ();

		StartCoroutine (checkDownloadImages());
	}
	
	IEnumerator downloadImg (string image_name){
		if (image_name != "") {
			Texture2D texture = new Texture2D (1, 1);
			Debug.Log ("try download image: " + responseAssets + image_name);
			WWW www = new WWW (responseAssets + image_name);
			yield return www;
			www.LoadImageIntoTexture (texture);
		
			byte[] ImgBytes = texture.EncodeToPNG ();
		
			File.WriteAllBytes (Application.persistentDataPath + "/" + image_name, ImgBytes);
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
				"email"
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
				userData.email
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
				"email"
			};
			string[] data2 = new string[] {
				userData.id.ToString (),
				userData.fbid,
				userData.foto,
				userData.fecha_nacimiento,
				userData.ciudad,
				userData.sexo,
				userData.nombre,
				userData.email
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
		
		string[] colsUsuarios = new string[]{ "email", "nombre", "fbid", "fecha_nacimiento", "sexo", "plataforma", "regid", "usuario_foto", "fileUpload"};
		string[] colsUsuariosValues = new string[]{ userData.email, userData.nombre, userData.fbid, userData.fecha_nacimiento, userData.sexo, userData.plataforma, userData.reg_id, userData.foto, "imagen_usuario" };
		
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

	public void sync(){
		Debug.Log ("sync....");
		//sync foto perro
		db.OpenDB(dbName);

		ArrayList result = db.BasicQueryArray ("select id, func, sfields, svalues from sync order by id ASC limit 1");
		if (result.Count > 0) {
			if(haveInet){
				string[] cols = new string[]{ "id", "func", "fields", "values"};
				string[] values = new string[]{ ((string[])result [0])[0] , ((string[])result [0])[1], ((string[])result [0])[2], ((string[])result [0])[3]};
				sendData (cols, values, "sync");
			}
			//((string[])result [0])[0];
		}

		db.CloseDB();
		StartCoroutine (call_sync ());
	}

	public void insert_sync(string[] fields, string[] values, string sync_func){

		db.OpenDB (dbName);

		string fields_json = MiniJSON.Json.Serialize(fields);
		string values_json = MiniJSON.Json.Serialize(values);

		//Debug.Log ("insertar en sync fields: " + fields_json + "values: " + values_json + " func: " + sync_func);

		string[] colsF = new string[]{ "id", "func", "sfields", "svalues"};
		string[] colsV = new string[]{ generateId().ToString(), sync_func, fields_json, values_json };
		
		db.InsertIntoSpecific("sync", colsF, colsV);

		db.CloseDB ();
	}

	public bool validEmail(string emailaddress){
		return System.Text.RegularExpressions.Regex.IsMatch(emailaddress, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
	}

	public void errorPopup(string error = "Error", string toclose = ""){

		popup.SetActive (true);
		popupText.GetComponent<Text> ().text = error;
		if (toclose == "1") {
			popupButton.SetActive (false);
		} else {
			popupButton.SetActive (true);
		}
	}

	public void showLoading(bool show = true){
		loading.SetActive (show);
	}

	public void closePopup(){
		popup.SetActive (false);
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
		Debug.Log ("spriteFromFile: " + image_);
		Sprite sprite = new Sprite ();
		if (image_ != "") {
			string filepath = Application.persistentDataPath + "/" + image_;
			if (File.Exists (filepath)) {

				byte[] fileData = File.ReadAllBytes (filepath);
				Texture2D tex = new Texture2D (2, 2);
				tex.LoadImage (fileData); //..this will auto-resize the texture dimensions.

				Debug.Log (tex.width + "x" + tex.height);
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

	public IEnumerator saveTextureToFile(Texture2D /*savedTexture */loadTexture, string fileName, char tosave){
		yield return new WaitForSeconds(0.5f);

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

}
