using UnityEngine;
using System.Collections;
using System.Collections.Generic;

	public class PlayMovieOnSpace : MonoBehaviour {
		
	public List<MovieTexture> movTextures = new List<MovieTexture>();

	void Update () {
			if (Input.GetButtonDown ("Jump")) {

				Renderer r = GetComponent<Renderer>();
				MovieTexture movie = r.material.mainTexture as MovieTexture;

				if (movie.isPlaying) {
					movie.Pause();
				}
				else {
					movie.Play();
				}
			}
		}
}
