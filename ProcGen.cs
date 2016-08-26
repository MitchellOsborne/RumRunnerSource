using UnityEngine;
using System.Collections.Generic;

public class ProcGen : MonoBehaviour
{
	public enum CornerDirection
	{
		LEFT,
		RIGHT
	}

	public CornerDirection cornerDirection;
	public int TrackDirection;
	public float PieceLength = 50;
	//public GameObject CornerBlock; //This is to check if the block is a corner, and to start layering the track around the corner
	private List<GameObject> SectionList = new List<GameObject>();
	public List<GameObject> EasySections = new List<GameObject>();
	public List<GameObject> MedSections = new List<GameObject>();
	public List<GameObject> HardSections = new List<GameObject>();
	public int CurrDifficulty;
	public int MedDifficultyScale;
	public int MaxDifficultyScale; //How many sections the player must complete before reaching max difficult scale
	public GameObject Corner, StartPiece, ShipObj;
	public int ShipSpawnChance;
	public GameObject CannonObj;
	public int CannonSpawnChance;
	public bool RunTutorial;
	public GameObject TutorialSection;

	private CornerDirection prevCornerDirection;
	private List<GameObject> SectionQueue = new List<GameObject> ();
	private List<GameObject> objList = new List<GameObject>();
	private Vector3 TrackPos = new Vector3(0,0,0);
	

	void Start()
	{
		SectionList.AddRange (EasySections);
		Random.seed = (int)System.DateTime.Now.Ticks;
		if (RunTutorial) {
			SectionQueue.Add (TutorialSection);
			SectionQueue.Add (SectionList [Random.Range (0, SectionList.Count - 1)]);
		} else {
			for (int i = 0; i < 2; ++i) {
				SectionQueue.Add (SectionList [Random.Range (0, SectionList.Count - 1)]);
			}
		}
		RebuildMap ();
	}

	void Update()
	{

	}

	//Adds a piece to the track which the player spawns on at the start.
	private void AddStartPiece()
	{
		GameObject obj = (GameObject)GameObject.Instantiate (StartPiece, TrackPos, Quaternion.Euler (0, TrackDirection, 0));
		obj.GetComponent<Transform> ().rotation.Set (0, TrackDirection, 0, 0);
		obj.transform.SetParent (gameObject.transform);
		objList.Add (obj);
		TrackPos.z += PieceLength;
	}

	private void AddCorner(bool forceLeft)
	{
		cornerDirection = prevCornerDirection;

		Random.seed = (int)System.DateTime.Now.Ticks;
		int i = Random.Range (0, 99);
		if (forceLeft) {
			i = 0;
		}
		GameObject obj;
		if (i <= 49) {
			prevCornerDirection = CornerDirection.LEFT;
			obj = (GameObject)GameObject.Instantiate (Corner, TrackPos, Quaternion.Euler (0, TrackDirection, 0)); // Left corner
			obj.GetComponent<Transform> ().rotation.Set (0, TrackDirection, 0, 0);
			obj.transform.SetParent (gameObject.transform);
			objList.Add (obj);
			TrackDirection -= 90;
			if (TrackDirection <= -90) {
				TrackDirection = 270;
			}
		} else {
			prevCornerDirection = CornerDirection.RIGHT;
			obj = (GameObject)GameObject.Instantiate (Corner, TrackPos, Quaternion.Euler (0, TrackDirection - 90.0f, 0)); // Right corner
			obj.GetComponent<Transform> ().rotation.Set (0, TrackDirection, 0, 0);
			obj.transform.SetParent (gameObject.transform);
			objList.Add (obj);
			TrackDirection += 90;
			if (TrackDirection >= 360) {
				TrackDirection = 0;
			}
		}
	}

	//Checks the current difficult and repopulates the available sections accordingly
	private void OrganiseSections()
	{
		SectionList.Clear ();
		if (CurrDifficulty >= MedDifficultyScale) {
			SectionList.AddRange (MedSections);
			if (CurrDifficulty >= MaxDifficultyScale) {
				SectionList.AddRange (HardSections);
			} else {
				SectionList.AddRange (EasySections);
			}
		}
	}

	//Occurs when the player reaches a corner, destroys the section the player just completed
	//and spawns the next section (ahead of the section the player is up to)
	public void TransitionSections()
	{
		CurrDifficulty += 1;

		if (CurrDifficulty == MedDifficultyScale || CurrDifficulty == MaxDifficultyScale) {
			OrganiseSections ();
		}
		Random.seed = (int)System.DateTime.Now.Ticks;

		SectionQueue.Add (SectionList [Random.Range (0, SectionList.Count - 1)]);
		BuildSection(SectionQueue [SectionQueue.Count - 1]);
		for (int i = 0; i < SectionQueue [0].GetComponent<SectionGen> ().PieceList.Count + 1; ++i) {
			Destroy (objList [i]);
		}
		objList.RemoveRange (0, SectionQueue[0].GetComponent<SectionGen> ().PieceList.Count + 1);
		SectionQueue.RemoveAt (0);

	}

	//Builds the next section of track. Pieces are stored in a list in the SectionGen script
	public void BuildSection(GameObject Section)
	{
		Section.GetComponent<SectionGen> ().GenerateSection ();
		for (int x = 0; x < Section.GetComponent<SectionGen> ().PieceList.Count; ++x) 
		{
			//Creates the new Map Piece at TrackPos Position with rot Rotation
			GameObject obj = (GameObject)GameObject.Instantiate (Section.GetComponent<SectionGen> ().PieceList[x], TrackPos, Quaternion.Euler (0, TrackDirection, 0));
			obj.GetComponent<Transform> ().rotation.Set (0, TrackDirection, 0, 0);
			obj.transform.SetParent (gameObject.transform);
			objList.Add (obj);

			//Spawns scenic ships next to the track, 
			//or spawns a cannon next to the track which fires on the player when they get close
			if (Random.Range (1, 100) <= ShipSpawnChance) {
				float offset = 10;
				float Rot = 1;
				if (Random.Range (0, 100) > 50) {
					offset = -offset;
				}
				if (Random.Range (0, 100) > 50) {
					Rot = -1;
				}
				GameObject ship = (GameObject)GameObject.Instantiate (ShipObj, obj.transform.position + offset *obj.transform.right,Quaternion.Euler(0, (TrackDirection -90) * Rot, 0));
				ship.transform.position -= new Vector3 (0, 3, 0);
				ship.transform.parent = obj.transform;
			}else if (Random.Range (1, 100) <= CannonSpawnChance) {
				float offset = -10;
				float Rot = 1;
				GameObject cannon = (GameObject)GameObject.Instantiate (CannonObj, obj.transform.position + offset *obj.transform.right,Quaternion.Euler(0, TrackDirection, 0));
				cannon.transform.parent = obj.transform;
			}

			//Adjusts the Position of the next block position based on what direction we're heading in
			switch (TrackDirection) 
			{
				case 0:
					TrackPos.z += PieceLength;
					break;
				case 90:
					TrackPos.x += PieceLength;
					break;
				case 180:
					TrackPos.z -= PieceLength;
					break;
				case 270:
					TrackPos.x -= PieceLength;
					break;
				default:
					break;
			}

		}
		
		if (Section == TutorialSection) {
			AddCorner (true);
		} else {
			AddCorner (false);
		}
		
		//Adds PieceLength units to TrackPos to account for corner piece
		switch (TrackDirection) 
		{
		case 0:
			TrackPos.z += PieceLength;
			break;
		case 90:
			TrackPos.x += PieceLength;
			break;
		case 180:
			TrackPos.z -= PieceLength;
			break;
		case 270:
			TrackPos.x -= PieceLength;
			break;
		default:
			break;
		}
	}

	//Destroys and rebuilds the map at world origin, used when the player dies
	public void RebuildMap()
	{
		foreach (GameObject obj in objList) {
			Destroy (obj);
		}
		objList.Clear ();
		TrackPos = new Vector3 (0, 0, 0);
		TrackDirection = 0;
		AddStartPiece ();
		for (int i = 0; i < SectionQueue.Count; i += 1) {
			BuildSection (SectionQueue [i]);
		}
	}

}
