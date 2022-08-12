﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Generator : MonoBehaviour {

	public int iterGen = 5;
	public int cntTile = -1;

	public Material neitralMaterial;
	public Material startMaterial;
	System.Random rnd = new System.Random();
	public bool upd;
	
	public float sideLength;

	[System.Serializable]
	public class meshSide {
		public float height;
		public float width;
		public float distanseToCenter;

		public int[,] Mask;
		public int bPointIndex;

		public Vector3 normal;
		public Vector3 ort;
		public Vector3 zeroVert;
		public List<Vector3> vertMesh = new List<Vector3>();
	}

	[System.Serializable]
	public class tile {
		public GameObject logic_tile;
		public GameObject tile3d;
		public Vector3 scale;
		public int mass = 1;
		public List<meshSide> side = new List<meshSide>();
	}

	public List<tile> Holls = new List<tile>();
	public List<int> massDung = new List<int>();

	void Start () {
		sideLength = AdderNew.instance.SideLength;

		if (upd) {
			AdderNew.instance.AdderNewStart();
			for (int i = 0; i < AdderNew.instance.Holls.Count; i++) {
				Holls.Add(new tile());
				Holls[i].logic_tile = AdderNew.instance.Holls[i].logic_tile;
				Holls[i].tile3d = AdderNew.instance.Holls[i].tile3d;
                Holls[i].mass = AdderNew.instance.Holls[i].mass;
                Holls[i].scale = AdderNew.instance.Holls[i].scale;
				for(int j = 0; j < AdderNew.instance.Holls[i].side.Count; j++) {
                    Holls[i].side.Add(new meshSide());
                    Holls[i].side[j].normal = AdderNew.instance.Holls[i].side[j].normal;
					Holls[i].side[j].height = AdderNew.instance.Holls[i].side[j].height;
					Holls[i].side[j].width = AdderNew.instance.Holls[i].side[j].width;
                    Holls[i].side[j].Mask = AdderNew.instance.Holls[i].side[j].Mask;
                    Holls[i].side[j].distanseToCenter = AdderNew.instance.Holls[i].side[j].distanseToCenter;
					Holls[i].side[j].zeroVert = AdderNew.instance.Holls[i].side[j].zeroVert * Holls[i].scale.x;
                    Holls[i].side[j].vertMesh = AdderNew.instance.Holls[i].side[j].vertMesh;
					Holls[i].side[j].ort = AdderNew.instance.Holls[i].side[j].ort;
				}
				massDung.Add(Holls[i].mass);
			}
        }

		Gen(massDung, Vector3.up * sideLength, Vector3.forward, null);
	}

	void Gen(List<int> mas, Vector3 pos, Vector3 dir, Material mat) {
		int tileInd = rnd.Next(Holls.Count);
		int sideInd = rnd.Next(Holls[tileInd].side.Count);
		int h0 = Mathf.RoundToInt(UnityEngine.Random.value * Holls[tileInd].side[sideInd].height) % Mathf.RoundToInt(Holls[tileInd].side[sideInd].height);
		int w0 = Mathf.RoundToInt(UnityEngine.Random.value * Holls[tileInd].side[sideInd].width) % Mathf.RoundToInt(Holls[tileInd].side[sideInd].width);

		GameObject buf = Instantiate(Holls[tileInd].logic_tile);

		float ang = Vector3.Angle(dir, Holls[tileInd].side[sideInd].normal);

		Quaternion tr = Quaternion.AngleAxis(ang, Vector3.up);
		
		if ((dir + (tr * Holls[tileInd].side[sideInd].normal).normalized).magnitude > 0.001f) {
			tr = Quaternion.AngleAxis(ang + 180f, Vector3.up);
		}

		buf.transform.rotation = tr;
		//buf.transform.position = pos - ((h0 + 0.5f) * sideLength * Vector3.up + (w0 + 0.5f) * sideLength * (tr * Holls[tileInd].side[sideInd].ort) + (tr * Holls[tileInd].side[sideInd].zeroVert));
		buf.transform.position = pos + dir * sideLength * 0.5f;
		if (mat != null) {
			return;
        }

		for (int i = 0; i < Holls[tileInd].side.Count; i++) {
			for (int h = 0; h < Mathf.RoundToInt(Holls[tileInd].side[i].height); h++) {
				for (int w = 0; w < Mathf.RoundToInt(Holls[tileInd].side[i].width); w++) {
					//Vector3 posOut = ((float)h + 0.5f) * sideLength * Vector3.up + ((float)w + 0.5f) * sideLength * (tr * Holls[tileInd].side[i].ort) + tr * Holls[tileInd].side[i].zeroVert + buf.transform.position/*+ tr * Holls[tileInd].side[i].normal*/;
					Vector3 posOut = buf.transform.position + tr * Holls[tileInd].side[i].normal*sideLength*0.5f;
					Vector3 dirOut = tr * Holls[tileInd].side[i].normal;

					int matInd = Holls[tileInd].side[i].Mask[h, w];

					Material matOut = Holls[tileInd].logic_tile.GetComponent<Renderer>().sharedMaterials[matInd];

					Gen(mas, posOut, dirOut, matOut);
				}
			}
		}
		/*if (mat == null) {
			Destroy(buf);
		}*/
	}
	

}
