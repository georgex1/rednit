using UnityEngine;
using System.Collections;
using System.IO;

public class login : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject GM = GameObject.Find ("MainController");
		MainController GMS = GM.GetComponent<MainController>();

		GMS.showLoading (true);

		//auto login usuario
		GMS.db.OpenDB(GMS.dbName);

		ArrayList result = GMS.db.BasicQueryArray ("select id, email, nombre, fbid, fecha_nacimiento, sexo, foto, ciudad, busco_ciudad, busco_sexo, busco_edad_min, busco_edad_max, busco_en_face, fb_friends, busco_cerca, busco_distancia, latitude, longitude, busco_lat, busco_long, descripcion, token from usuarios limit 1");
		GMS.db.CloseDB();

		if (result.Count > 0) {

			GMS.userData.populateUser (((string[])result [0]));

			if (GMS.userData.foto == "") {
				Debug.Log ("no photo");
				Application.LoadLevel ("perfil");
				return;
			} else {
				string filepath = Application.persistentDataPath + "/" + GMS.userData.foto;
				Debug.Log ("usuario foto: " + Application.persistentDataPath + "/" + GMS.userData.foto);
				if (!File.Exists (filepath)) {
					GMS.showLoading (false);
					Application.LoadLevel ("perfil");
					return;
				} else {
					if (PlayerPrefs.GetString ("busco_completo") == "1") {
						GMS.showLoading (false);
						Application.LoadLevel ("buscar");
						return;
					} else {
						GMS.showLoading (false);
						Application.LoadLevel ("busco");
						return;
					}
				}
			}
		} else {
			GMS.showLoading (false);
			Application.LoadLevel ("login");
		}


	}

}
