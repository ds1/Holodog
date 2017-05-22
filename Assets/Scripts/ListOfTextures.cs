using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListOfTextures : MonoBehaviour {

	public List<MovieTexture> movTextures = new List<MovieTexture>();

	// Use this for initialization
	void Start () {
	


	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("A")) {
			Debug.Log ("a was pressed");
		}
	}
}
