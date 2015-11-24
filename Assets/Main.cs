using UnityEngine;
using System.Collections;
using Microsoft.Samples.Kinect.ControlsBasics.Embed.Common.Model;
using Microsoft.Samples.Kinect.ControlsBasics;
using UnityEngine.UI;
using winterdom_filemap;

public class Main : MonoBehaviour
{
	GameObject cube;

	MemoryMappedFilesEditor<EmbedHandDataV1> MemoryMappedReader;
	MemoryMappedFilesEditor<UnityData> MemoryMappedWriter;
	Vector2 myScreen;
	EmbedHandDataV1 hp;
	float X, Y;
	float priviousX,diffX,priviousY,diffY;
	Color testcolor;
	// Use this for initialization
	void Start ()
	{
		MemoryMappedReader = new MemoryMappedFilesEditor<EmbedHandDataV1> ("FileName");
		MemoryMappedWriter = new MemoryMappedFilesEditor<UnityData> ("Back");
		cube = GameObject.Find("Cube");
		myScreen = new Vector2 (Screen.width, Screen.height);
		priviousX = myScreen.x / 2;
		priviousY = myScreen.y / 2;

	}
	public void BackButtonOnClick(){
		Debug.Log ("Button Click");
		UnityData unityData1 = new UnityData (true);
		MemoryMappedWriter.WriteOnMemory (unityData1);
//		UnityData unityData2 = new UnityData (false);
//		MemoryMappedWriter.WriteOnMemory (unityData2);

	}

	// Update is called once per frame
	void Update ()
	{
		Rotate ();
	}
	private void Rotate(){
		hp = MemoryMappedReader.ReadFromMemory ();
		if (hp != null) {
			if (hp.isInGripInteraction) {
				diffX = priviousX - (float)hp.ScreenX;
				priviousX = (float)hp.ScreenX;
				diffY = priviousY - (float)hp.ScreenY;
				priviousY = (float)hp.ScreenY;
				GetComponent<Image> ().color = Color.blue;
				cube.transform.Rotate ((float)(diffY), (float)(-diffX), 0);
			} else {
				GetComponent<Image> ().color = Color.white;
			}
			transform.position = new Vector3 ((float)hp.ScreenX, (float)(myScreen.y - hp.ScreenY), 0);
		}
	}
}