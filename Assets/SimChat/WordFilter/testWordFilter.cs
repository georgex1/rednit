using UnityEngine;
using System.Collections;

public class testWordFilter : MonoBehaviour {
	//string to filter
	string dirty = "";
	string clean = "";
	//initialize the WordFilter
	TWordFilter wf = new TWordFilter();
	//GUI Variables
	bool hasLoadedWords = false;
	bool isloadingFile = false;
	bool isFilteringString = false;
	bool replacements = true;
	float startTime = 0f;
	
	void Start (){
		//set the finish function
		wf.setFinishedFunc(stringDone);
	}
	
	
	//function to call after filtering the string in a separate thread
	void stringDone(string s){
		clean = s;
		isFilteringString = false;
	}
	
	void OnGUI(){
		GUILayout.BeginArea(new Rect(10,10,Screen.width-20,Screen.height-20));
		GUILayout.BeginHorizontal("box");
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		if(hasLoadedWords){
			if(isFilteringString){
				GUILayout.Label("Filtering "+dots(startTime,4));
			}else{
				if(replacements){
					GUILayout.Label(clean);
					dirty = GUILayout.TextArea(dirty,GUILayout.MinWidth(Screen.width/2));
					if(GUILayout.Button("Filter")){
						clean = wf.cleanString(dirty);
					}if(GUILayout.Button("Filter in Separate Thread")){
						isFilteringString = true;
						wf.startCleanString(dirty);
					}
				}else{ //if replacements are not used, actively replace the input text
					dirty = GUILayout.TextArea(dirty,GUILayout.MinWidth(Screen.width/2));
					wf.cleanString(ref dirty);
					GUILayout.Label("Words are automatically replaced");
				}
			}
		}else{
			if(isloadingFile){
				GUILayout.Label("Downloading File "+dots(startTime,4));
			}else{
				//determines if the replacement words will be used
				replacements = GUILayout.Toggle(replacements,"Use Replacement Words");
				if(GUILayout.Button("Read Resource File")){
					getRescourceFile(replacements);
				}
				if(GUILayout.Button("Download Word File")){
					startTime = Time.time;
					isloadingFile = true;
					downloadWords(replacements);
				}
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
	
	//load the word list from the resources folder
	void getRescourceFile(bool withReplacements){
		TextAsset s = Resources.Load("words"+(withReplacements?"":"Stars"), typeof(TextAsset)) as TextAsset;
		if(s==null)
			UnityEngine.Debug.LogWarning("No File Found");	
		else
			finishedLoadingWords(s.text);
	}
	//download word list from server
	void downloadWords(bool withReplacements){
		WWW w = new WWW("http://simple-chat-pro.appspot.com/words.php"+((withReplacements)?"?rep=true":""));
		StartCoroutine(waitForWords(w));
	}
	//function to wait for the word list to download
	IEnumerator waitForWords(WWW www){
		yield return www;
		if(www.error == null)
			finishedLoadingWords(www.text);
		else
			Debug.Log(www.error);
	}
	//function to call when word list has been read
	void finishedLoadingWords(string wordFileText){
		hasLoadedWords = true;
		isloadingFile = false;
		wf.parseString(wordFileText);
	}
	//function which returns a string of '.' characters based on the time and number of dots
	string dots(float startTime,int n){
		System.Text.StringBuilder output = new System.Text.StringBuilder();
		output.Append('.',Mathf.RoundToInt(Time.time-startTime) % n);
		return output.ToString();
	}
}
