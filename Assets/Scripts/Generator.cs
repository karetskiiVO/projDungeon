using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Generator : MonoBehaviour {

	public int iterGen = 5;
	public int cntTile = -1;

	public Material neitralMaterial;
	public Material startMaterial;

	public bool upd;
	
	public float sideLength;

	[System.Serializable]
	public class meshSide {
		public float height;
		public float width;
		public float distanseToCenter;

		public List<int> Mask = new List<int>();
		public int bPointIndex;

		public Vector3 normal;
		public Vector3 ort;
		public Vector3 distanseToCenterPoint;
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
					Holls[i].side[j].distanseToCenterPoint = AdderNew.instance.Holls[i].side[j].distanseToCenterPoint;
                    Holls[i].side[j].vertMesh = AdderNew.instance.Holls[i].side[j].vertMesh;
					Holls[i].side[j].ort = AdderNew.instance.Holls[i].side[j].ort;
				}
				massDung.Add(Holls[i].mass);
			}
        }

		Gen(massDung, Vector3.up * 0.5f * sideLength, Vector3.forward, startMaterial);
	}
	
	bool Gen(List<int> mas, Vector3 pos, Vector3 dir, Material mat) {
		/*Ray ray = new Ray(pos - 0.1f * SideLength * dir, dir);
		RaycastHit hit;
		Physics.Raycast(Ray, SideLength * 0.5f, out hit);*/

		bool usl = true;

		int sumGen = 0;
		int index = 0;
		int massIndex;
		
		for (int i = 0; i < mas.Count; i++) {
			sumGen += mas[i];
		}
		GameObject[] dung = GameObject.FindGameObjectsWithTag("logTile");

		for (int u1 = 0; u1 < iterGen; u1++) {
			//определяем случайный индекс
			massIndex = Mathf.RoundToInt(sumGen * Random.value);
			for (int i = 0; i < mas.Count; i++) {
				if (massIndex <= mas[i]) {
					index = i;
					break;
				}
				massIndex -= mas[i];
			}
			//создаем объект
			GameObject buf = Instantiate(Holls[index].logic_tile);
			int sideIndex = -1;
			int prohIndex = -1;
			for (int u2 = 0; u2 < iterGen; u2++) {
				usl = true;

				List<int> posit = new List<int>();
				int h, w;
				int sideCnt = Holls[index].side.Count;
				sideIndex = Mathf.RoundToInt(sideCnt * Random.value) % sideCnt;
				for(int i = 0; i < Holls[index].side[sideIndex].Mask.Count; i++) {
					int materialIndex = Holls[index].side[sideIndex].Mask[i];
					if (Holls[index].logic_tile.GetComponent<Renderer>().sharedMaterials[materialIndex] == mat) {
						posit.Add(i);
                    }
                }
				if(posit.Count == 0) {
					if(u2 == iterGen - 1) {
						usl = false;
                    }
					continue;
                }
				int positIndex = Mathf.RoundToInt(posit.Count * Random.value) % posit.Count;
				prohIndex = positIndex;
				h = positIndex % Mathf.RoundToInt(Holls[index].side[sideIndex].width);
				w = (positIndex - h) / Mathf.RoundToInt(Holls[index].side[sideIndex].width);

				float ang = Vector3.Angle(dir, Holls[index].side[positIndex].normal);

				Quaternion tr = Quaternion.AngleAxis(ang, Vector3.up);
				
				Vector3 bufDir = (tr * Holls[index].side[positIndex].normal).normalized;
				if ((dir - bufDir).magnitude > 0.01f) {
					tr = Quaternion.AngleAxis(180 + ang, Vector3.up);
				}

				buf.transform.rotation = tr;

				buf.transform.position = pos - ((h + 0.5f) * sideLength * Vector3.up + (w + 0.5f) * sideLength * Holls[index].side[positIndex].ort);

				for(int i = 0; i < dung.Length; i++) {
					float dist;
					Vector3 dirBuf;
					bool k = Physics.ComputePenetration(dung[i].GetComponent<Collider>(), dung[i].transform.position, dung[i].transform.rotation, buf.GetComponent<Collider>(), buf.transform.position, buf.transform.rotation, out dirBuf, out dist);
					k |= (dist < 0.01f);
					usl &= k;
				}
                if (!usl) {
					continue;
                }
				for(int i = 0; i < Holls[index].side.Count; i++) {
					for(int j = 0; j < Holls[index].side[i].Mask.Count; j++) {
						if ((i == sideIndex) && (j == prohIndex)) {
						} else {
							int materialInd = Holls[index].side[i].Mask[j];
							Material bufMaterial = Holls[index].logic_tile.GetComponent<Renderer>().sharedMaterials[materialInd];
							if (bufMaterial == neitralMaterial) {
							} else {
								int h0, w0;
								h0 = positIndex % Mathf.RoundToInt(Holls[index].side[sideIndex].width);
								w0 = (positIndex - h0) / Mathf.RoundToInt(Holls[index].side[sideIndex].width);
								Vector3 posOut = (0.5f + h0) * sideLength * Vector3.up + (0.5f + w0) * sideLength * Holls[index].side[sideIndex].ort + buf.transform.position;
								Vector3 dirOut = (tr * Holls[index].side[sideIndex].normal).normalized;
								usl &= Gen(mas, posOut, dirOut, bufMaterial);
							}
						}
                    }
                }
                if (usl) {
					GameObject buf1 = Instantiate(Holls[index].tile3d);
					buf1.transform.rotation = buf.transform.rotation;
					buf1.transform.position = buf.transform.position;
					Destroy(buf);
					cntTile++;
					break;
                }
			}
            if (!usl) {
				Destroy(buf);
				return usl;
            }
		}
		return false;
    }
}
