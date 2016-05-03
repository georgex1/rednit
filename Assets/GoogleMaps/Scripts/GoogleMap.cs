using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;
using UnityEngine.UI;
using System.Linq;

public class GoogleMap : MonoBehaviour
{
	public enum MapType
	{
		RoadMap,
		Satellite,
		Terrain,
		Hybrid
	}
	public bool loadOnStart = true;
	public bool autoLocateCenter = true;
	public GoogleMapLocation centerLocation;
	public int zoom = 13;
	public MapType mapType;
	public int size = 512;
	public bool doubleResolution = false;
	public GoogleMapMarker[] markers;
	public GoogleMapPath[] paths;
	public Text km;
	private MainController GMS;
	public Text buscoDistancia;
	public Slider slider;
	public bool newAdressFlag;



	void Start() {
		newAdressFlag = false;
		GameObject GM = GameObject.Find ("MainController");
		GMS = GM.GetComponent<MainController>();
		if(loadOnStart) Refresh();	


		Debug.Log ("Distancia guardada del usuario: " + GMS.userData.busco_distancia);
		if (GMS.userData.busco_distancia != "0" || GMS.userData.busco_distancia != "" ) {
			km.text = GMS.userData.busco_distancia;
			slider.value = float.Parse(GMS.userData.busco_distancia);
		} 


}
	public void newAddress(InputField direccion) {
		Debug.Log ("nueva direccion" + direccion.text);
		//centerLocation.address = "ruiz de montoya 119 apostoles misiones";
		centerLocation.address = direccion.text;
		newAdressFlag = true;
		Refresh();
		//Debug.Log("nueva info");
	}
	public void zoomIn() {
		zoom = zoom + 1;
		Refresh();
	}
	public void zoomOut() {
		zoom = zoom - 1;
		Refresh();
	}
	public void Refresh() {
		if(autoLocateCenter && (markers.Length == 0 && paths.Length == 0)) {
			Debug.LogError("Auto Center will only work if paths or markers are used.");	
		}
		StartCoroutine(_Refresh());
	}
	
	IEnumerator _Refresh ()
	{
		var url = "http://maps.googleapis.com/maps/api/staticmap";
		var qs = "";
		if (!autoLocateCenter) {
			if (centerLocation.address != ""){

				if(newAdressFlag == true) {
					qs += "center=" +  WWW.EscapeURL (centerLocation.address);
					newAdressFlag = false;
				} else {


				Debug.Log("Busco latlong: " + GMS.userData.busco_lat + ", " + GMS.userData.busco_long);


				//en caso que el usuario no tenga definida lat y long de busqueda cargo SU ubicacion
				if (GMS.userData.busco_lat != "" & GMS.userData.busco_long != "") {
					qs += "center=" + GMS.userData.busco_lat +","+ GMS.userData.busco_long;
				} else {
				//en caso que el usuario no tenga definida lat y long de busqueda cargo SU ubicacion
					Debug.Log(GMS.userData.latitude + ", " + GMS.userData.longitude);
					//si no tiene definida SU ubicacion traigo una ubicacion por default. REVISAR
					if (GMS.userData.latitude == "0" & GMS.userData.longitude == "0") {
						Debug.Log ("Entro en latlong == 0");
						qs += "center=" +  WWW.EscapeURL (centerLocation.address);
					} else { //si tiene definida su ubicacion cargo esa.
						Debug.Log ("Entro en el else de en latlong = 0");
						qs += "center=" + GMS.userData.latitude +","+ GMS.userData.longitude;
					}

				}

				}//newadressflag

			} else {
				//qs += "center=" + WWW.UnEscapeURL (string.Format ("{0},{1}", centerLocation.latitude, centerLocation.longitude));
			}
		
			qs += "&zoom=" + zoom.ToString ();
		}
		qs += "&size=" + WWW.UnEscapeURL (string.Format ("{0}x{0}", size));
		//qs += "&scale=" + (doubleResolution ? "2" : "1");
		qs += "&maptype=" + mapType.ToString ().ToLower ();
		qs += "&format=png";
		qs += "&visual_refresh=true";
		qs += "&markers=icon:http://thepastoapps.com/marker.png%7Cshadow:true%7C" + WWW.EscapeURL (centerLocation.address);
		qs += "&style=feature:landscape|element:labels|visibility:off&style=feature:transit|element:labels|visibility:off&style=feature:poi|element:labels|visibility:off&style=feature:water|element:labels|visibility:off&style=feature:road|element:labels.icon|visibility:off&style=saturation:-100|gamma:2.15|lightness:12|hue:0x00aaff&style=feature:road|element:labels.text.fill|visibility:on|lightness:23&style=feature:road|element:geometry|lightness:57";
		qs += "&key=AIzaSyD6EbMZzzeTQAvVc0c36WUulLFLAxG0Npo";
		var usingSensor = false;
#if UNITY_IPHONE
		usingSensor = Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running;
#endif
		qs += "&sensor=" + (usingSensor ? "true" : "false");
		
		foreach (var i in markers) {
			qs += "&markers=" + string.Format ("size:{0}|color:{1}|label:{2}", i.size.ToString ().ToLower (), i.color, i.label);
			foreach (var loc in i.locations) {
				if (loc.address != "")
					qs += "|" + WWW.UnEscapeURL (loc.address);
				else
					qs += "|" + WWW.UnEscapeURL (string.Format ("{0},{1}", loc.latitude, loc.longitude));
			}
		}
		
		foreach (var i in paths) {
			qs += "&path=" + string.Format ("weight:{0}|color:{1}", i.weight, i.color);
			if(i.fill) qs += "|fillcolor:" + i.fillColor;
			foreach (var loc in i.locations) {
				if (loc.address != "")
					qs += "|" + WWW.UnEscapeURL (loc.address);
				else
					qs += "|" + WWW.UnEscapeURL (string.Format ("{0},{1}", loc.latitude, loc.longitude));
			}
		}
		Debug.Log (url + "?" + qs);
		var req = new WWW (url + "?" + qs);
		yield return req;
		gameObject.GetComponent<RawImage> ().texture = req.texture;
	}


	public void DistanceOnValueChanged(float newValue)
	{
		Debug.Log("Slider value: " + newValue);
		km.text = newValue+"";
		//km_.text = newValue + "";

		//buscoDistancia = newValue + " Km";
		//agrego el valor de distancia a una variable y lo submiteo junto a la direccion.
	}

	public void loadPrevLevel() {
		Application.LoadLevel("busco");
	}

	public void submitDistance() {
		Debug.Log("submitDistance");
		StartCoroutine (getLatLong());
	}

	IEnumerator getLatLong(){
		
		//var distanceUrl = "https://maps.googleapis.com/maps/api/geocode/json?address=" + WWW.EscapeURL (centerLocation.address) + "&key=AIzaSyBTT0TA2zlOlIabKgs3ZE4njA23yaL7wwA";  
		var distanceUrl = "http://haakapp.com/response/geolocation.php?address=" + WWW.EscapeURL (centerLocation.address);  
		Debug.Log (distanceUrl);
		var latlong = new WWW (distanceUrl);
		yield return latlong;

		Debug.Log("latlong1: " + latlong.text);

		string[] info13 = latlong.text.Split(';').Select(str => str.Trim()).ToArray();
	
		Debug.Log ("Lat: " + info13[0]);
		Debug.Log ("Long: " + info13[1]);

		if(!GMS.haveInet){
			GMS.errorPopup("Verifica tu conexion a internet");
		}else{

			GMS.userChangeData.busco_lat = info13[0];
			GMS.userChangeData.busco_long = info13[1];
			GMS.userChangeData.busco_distancia = km.text;

			//GMS.showLoading(true);
		
			GMS.perfil_busco();
			loadPrevLevel();
			//PlayerPrefs.SetString("busco_completo", "1");
			//StartCoroutine(gotoNext());
		} 

	}


}

public class GoogleGeoCodeResponse
{
	
	public string status { get; set; }
	public results[] results { get; set; }
	
}

public class results
{
	public string formatted_address { get; set; }
	public geometry geometry { get; set; }
	public string[] types { get; set; }
	public address_component[] address_components { get; set; }
	public string place_id { get; set; }
}

public class geometry
{
	public string location_type { get; set; }
	public location location { get; set; }
}

public class location
{
	public string lat { get; set; }
	public string lng { get; set; }
}

public class address_component
{
	public string long_name { get; set; }
	public string short_name { get; set; }
	public string[] types { get; set; }
}

public enum GoogleMapColor
{
	black,
	brown,
	green,
	purple,
	yellow,
	blue,
	gray,
	orange,
	red,
	white
}

[System.Serializable]
public class GoogleMapLocation
{
	public string address;
	public float latitude;
	public float longitude;
}

[System.Serializable]
public class GoogleMapMarker
{
	public enum GoogleMapMarkerSize
	{
		Tiny,
		Small,
		Mid
	}
	public GoogleMapMarkerSize size;
	public GoogleMapColor color;
	public string label;
	public GoogleMapLocation[] locations;
	
}

[System.Serializable]
public class GoogleMapPath
{
	public int weight = 5;
	public GoogleMapColor color;
	public bool fill = false;
	public GoogleMapColor fillColor;
	public GoogleMapLocation[] locations;	
}






