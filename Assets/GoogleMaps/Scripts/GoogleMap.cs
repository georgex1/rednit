using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;
using UnityEngine.UI;




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


	void Start() {
		if(loadOnStart) Refresh();	
}
	public void newAddress(InputField direccion) {
		Debug.Log ("nueva direccion" + direccion.text);
		//centerLocation.address = "ruiz de montoya 119 apostoles misiones";
		centerLocation.address = direccion.text;
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
			if (centerLocation.address != "")
				//qs += "center=" + WWW.UnEscapeURL (centerLocation.address);
				qs += "center=" +  WWW.EscapeURL (centerLocation.address);
			else {
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
		
		
	/*	var req = new HTTP.Request ("GET", url + "?" + qs, true);
		req.Send ();
		while (!req.isDone)
			yield return null;
		if (req.exception == null) {
			var tex = new Texture2D (size, size);
			tex.LoadImage (req.response.Bytes);
			renderer.material.mainTexture = tex;
		}*/
		Debug.Log (url + "?" + qs);
		var req = new WWW (url + "?" + qs);
		//Debug.Log (req.texture);
		yield return req;
		//renderer.material.mainTexture = req.texture;
		//renderer.material.SetTexture = req.texture;
		//gameobject.GetComponent<RawImage> ().texture = texture;
		gameObject.GetComponent<RawImage> ().texture = req.texture;
	}


	public void DistanceOnValueChanged(float newValue)
	{
		Debug.Log("Slider value: " + newValue);
		km.text = newValue + " Km";
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
		
		var distanceUrl = "https://maps.googleapis.com/maps/api/geocode/json?address=" + WWW.EscapeURL (centerLocation.address) + "&key=AIzaSyBTT0TA2zlOlIabKgs3ZE4njA23yaL7wwA";  
		Debug.Log (distanceUrl);
		var latlong = new WWW (distanceUrl);
		yield return latlong;
		Debug.Log("latlong1: " + latlong.text);


		//Dictionary<string,object> scoreData = scores[0] as Dictionary<string,object>;
		
	//	object score = scoreData["score"];


		//IDictionary search = (IDictionary) Json.Deserialize (latlong.text);
		//var search = Json.Deserialize(latlong.text) as Dictionary<string,object>;
		//Debug.Log("search['string']: " + ((List<object>) search["results"])[0]);

		//Debug.Log(List<object>)(((Dictionary<string, object>)latlong) ["data"]);
		//Debug.Log("Lat: " + (double) search["location"]); // floats come out as doubles
		//Debug.Log (latlong.ToString ());
		//GMS.userData.busco_lat = (string)search ["location"];
		//GMS.userData.busco_long = (string)search ["lng"];

		//Debug.Log ("GMS.userData.busco_lat: " + GMS.userData.busco_lat);
		//Debug.Log ("GMS.userData.busco_long: " + GMS.userData.busco_long);
		//Debug.Log("latlong");
		
		
		
		/*	if(!GMS.haveInet){
			GMS.errorPopup("Verifica tu conexion a internet");
		}else{
			
			GMS.userChangeData.busco_lat = latlong;
			GMS.userChangeData.busco_distancia = buscoDistancia;
			
			GMS.showLoading(true);
			
			GMS.perfil_busco();
			
			//PlayerPrefs.SetString("busco_completo", "1");
			//StartCoroutine(gotoNext());
		} */
		
	}




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






