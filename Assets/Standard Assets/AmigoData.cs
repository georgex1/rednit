﻿using System.Collections;
using System;

[Serializable]
public class AmigoData {
	
	public string id, nombre,
	aceptado,
	email,
	edad,
	sexo,
	ciudad,
	foto;
	
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
	}
	
	public void save(){
		
	}
	
	public void populateUser(string[] row_){
		id = row_ [2];
		aceptado = row_ [3];
		nombre = row_ [4];
		email = row_ [5];
		edad = row_ [6];
		sexo = row_ [7];
		ciudad = row_ [8];
		foto = row_ [9];
	}
	
}
