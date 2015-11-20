using System.Collections;
using System;
using System.Collections.Generic;
using Facebook.MiniJSON;

[Serializable]
public class UserData {

	public int id;
	
	public string nombre,
	email,
	password,
	fecha_nacimiento,
	reg_id,
	plataforma,
	ciudad,
	fbid,
	sexo,
	foto;

	public string busco_ciudad,
		busco_sexo,
		busco_edad_min,
		busco_edad_max,
		busco_en_face,
		busco_cerca,
		busco_distancia;

	public string date_month;
	public string date_day;
	public string date_year;

	public byte[] ImgBytes;
	public string temp_img;

	public List<string> fbFriends;
	
	//public Dictionary<string, int> ExercisesMetricas;
	
	public UserData(){
		id = 0;
		password = "";
		fecha_nacimiento = "";
		reg_id = "";
		plataforma = "";
		ciudad = "";
		fbid = "";
		sexo = "";
		foto = temp_img = "";
		busco_edad_min = "18";
		busco_edad_max = "50";
		busco_en_face = "SI";
		busco_cerca = "NO";
		busco_distancia = "100";
		fbFriends = null;
		//ExercisesMetricas = new Dictionary<string, int> ();
	}

	public void save(){

	}

	public string serializeFbFriends(){
		string fbFriendsString = "";
		if (fbFriends != null) {
			fbFriendsString =  String.Join(",", fbFriends.ToArray());
		}
		return fbFriendsString;
	}

	public void populateUser(string[] row_){
		id = int.Parse( row_ [0] );
		email = row_ [1];
		nombre = row_ [2];
		fbid = row_ [3];
		fecha_nacimiento = row_ [4];
		sexo = row_ [5];
		foto = row_ [6];
		ciudad = row_ [7];

		busco_ciudad = row_ [8];
		busco_sexo = row_ [9];
		busco_edad_min = row_ [10];
		busco_edad_max = row_ [11];
		busco_en_face = row_ [12];

		busco_cerca = row_ [14];
		busco_distancia = row_ [15];

		if (row_ [13] != "") {
			fbFriends = new List<string>(row_ [13].Split(','));
		}
	}

	public void format_month(string month_){
		int monthInt_ = 0;
		switch (month_) {
		case "ENERO": monthInt_ = 01; break;
		case "FEBRERO": monthInt_ = 02; break;
		case "MARZO": monthInt_ = 03; break;
		case "ABRIL": monthInt_ = 04; break;
		case "MAYO": monthInt_ = 05; break;
		case "JUNIO": monthInt_ = 06; break;
		case "JULIO": monthInt_ = 07; break;
		case "AGOSTO": monthInt_ = 08; break;
		case "SEPTIEMBRE": monthInt_ = 09; break;
		case "OCTUBRE": monthInt_ = 10; break;
		case "NOVIEMBRE": monthInt_ = 11; break;
		case "DICIEMBRE": monthInt_ = 12; break;
		}
		date_month = monthInt_.ToString();
	}

	public string format_month_int(string month_int){
		string month_ = "ENERO";
		switch (month_int) {
		case "01": month_ = "ENERO"; break;
		case "02": month_ = "FEBRERO"; break;
		case "03": month_ = "MARZO"; break;
		case "04": month_ = "ABRIL"; break;
		case "05": month_ = "MAYO"; break;
		case "06": month_ = "JUNIO"; break;
		case "07": month_ = "JULIO"; break;
		case "08": month_ = "AGOSTO"; break;
		case "09": month_ = "SEPTIEMBRE"; break;
		case "10": month_ = "OCTUBRE"; break;
		case "11": month_ = "NOVIEMBRE"; break;
		case "12": month_ = "DICIEMBRE"; break;
		}
		return month_;
	}

}
