using System.Collections;
using System;

[Serializable]
public class AmigoData {
	
	public string id, 
	nombre,
	aceptado,
	email,
	edad,
	sexo,
	ciudad,
	foto, 
	chat_group, 
	latitude, 
	longitude, 
	descripcion;
	
	//public Dictionary<string, int> ExercisesMetricas;
	
	public AmigoData(){
		id = "0";
		nombre = "";
		aceptado = "0";
		email = "";
		edad = "";
		sexo = "";
		ciudad = "";
		sexo = "";
		foto = "";
		descripcion = "";
		chat_group = "";
	}
	
	public void save(){
		
	}
	
	public void populateUser(string[] row_){
		id = row_ [1];
		aceptado = row_ [2];
		nombre = row_ [3];
		email = row_ [4];
		edad = row_ [5];
		sexo = row_ [6];
		ciudad = row_ [7];
		foto = row_ [8];
		descripcion = row_ [9];
	}
	
}
