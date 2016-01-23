using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class buscar : MonoBehaviour {

	private MainController GMS;

	public Text PersonaNombre;
	public Text PersonaEdad;
	public Text PersonaCiudad;
	public Image PersonaFoto;

	public Text PersonaDescripcion;

	public GameObject PanelAviso;
	public GameObject PanelAvisoBtn;

	private string actualPersona;
	private bool hasOne;

	// Use this for initialization
	void Start () {
		hasOne = false;
	
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();

		string fbFriendsString = "'0'";
		if (GMS.userData.fbFriends != null && GMS.userData.busco_en_face == "SI") {
			fbFriendsString = String.Join(",", GMS.userData.fbFriends.ToArray());
		}

		GMS.db.OpenDB(GMS.dbName);
		string buscarQuery = "select id, nombre, edad, sexo, ciudad, foto, descripcion from personas where visto = '0' AND id NOT IN " +
			"( select personas_id from amigos_usuarios where usuarios_id = '" + GMS.userData.id + "' ) AND fbid NOT IN ( " + fbFriendsString + " ) ";

		ArrayList result = GMS.db.BasicQueryArray (buscarQuery);

		/*ArrayList result = GMS.db.BasicQueryArray ("select id, nombre, edad, sexo, ciudad, foto from personas where visto = '0' ");*/
		GMS.db.CloseDB();

		Debug.Log (buscarQuery);
		Debug.Log ("total personas: " + result.Count);

		if (result.Count > 0) {

			foreach (string[] row_ in result) {

				if (GMS.checkImageExists(row_[5])) {
					hasOne = true;
					actualPersona = row_[0].ToString();
					PersonaNombre.text = row_[1];
					PersonaEdad.text = row_[2];
					PersonaCiudad.text = row_[4];
					PersonaFoto.sprite = GMS.spriteFromFile(row_[5]);

					PersonaDescripcion.text = row_[6];
				}
			}

			if(result.Count < 3){
				GMS.download_personas ();
			}

			GMS.actualizando = false;

			if(!hasOne){
				NoPersonas();
			}
		} else {
			NoPersonas();
		}
	}

	private void NoPersonas(){
		PanelAvisoBtn.SetActive (true);
		PanelAviso.SetActive(true);
		GMS.actualizando = true;
	}

	public void actualizar(){
		PanelAvisoBtn.SetActive (false);
		GMS.actualizando = true;
		GMS.download_personas ();
	}

	public void votar(string voto_){
		if (voto_ == "SI") {

			string[] fields = {"usuarios_id", "voto_usuarios_id" };
			string[] values = {GMS.userData.id.ToString(), actualPersona};
			GMS.insert_sync(fields, values, "voto_usuario");

		}

		string[] fields2 = {"usuarios_id", "usuarios_descargado_id" };
		string[] values2 = {GMS.userData.id.ToString(), actualPersona};
		GMS.insert_sync(fields2, values2, "cambiar_visto");

		GMS.db.OpenDB(GMS.dbName);
		GMS.db.UpdateSingle("personas", "visto", "1", "id" , actualPersona);
		GMS.db.CloseDB();

		Application.LoadLevel (Application.loadedLevelName);
	}

	// Update is called once per frame
	void Update () {
		if(!GMS.actualizando && PanelAviso.activeSelf){
			StartCoroutine(closeActualizarPanel());
		}
	}

	IEnumerator closeActualizarPanel(){
		yield return new WaitForSeconds (2);
		//PanelAviso.SetActive(false);
		Application.LoadLevel (Application.loadedLevelName);
	}

	public void cargarEscena(string escena){
		Application.LoadLevel (escena);
	}

}
