/************************************************************
************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System;

/************************************************************
************************************************************/
[RequireComponent(typeof(LivePoseAnimator))]

/************************************************************
************************************************************/
public class SampleMocapData : MonoBehaviour
{
	public string dataPath;
	public int maxLength = 3600;

	private int currentFrame = 1;
	private bool first = true;

	private ArrayList motionData;


	void Start()
	{
		string fn = Application.dataPath + "/" + dataPath;
		readData(fn);
	}

	void Update()
	{
		if (first){
			createOptitrackAvater();
			first = false;
			
		}else{
			if(currentFrame == motionData.Count) currentFrame = 1;
			
			this.gameObject.GetComponent<LivePoseAnimator>().OnSamplePacket((string)motionData[currentFrame]);
			currentFrame += 1;
		}
	}

	private void createOptitrackAvater()
	{
		this.gameObject.GetComponent<LivePoseAnimator>().OnSamplePacket("<?xml version=\"1.0\" ?><SkeletonDescriptions><SkeletonDescription ID=\"7\" Name=\"Golem\" BoneCount=\"21\"><BoneDefs ID=\"1\" ParentID=\"0\" Name=\"Golem_Hips\" x=\"0.000000\" y=\"0.858212\" z=\"0.000000\" /><BoneDefs ID=\"2\" ParentID=\"1\" Name=\"Golem_Ab\" x=\"0.000000\" y=\"0.076079\" z=\"0.000000\" /><BoneDefs ID=\"3\" ParentID=\"2\" Name=\"Golem_Chest\" x=\"0.000000\" y=\"0.203848\" z=\"0.000000\" /><BoneDefs ID=\"4\" ParentID=\"3\" Name=\"Golem_Neck\" x=\"0.000000\" y=\"0.203011\" z=\"0.018456\" /><BoneDefs ID=\"5\" ParentID=\"4\" Name=\"Golem_Head\" x=\"0.000000\" y=\"0.129892\" z=\"-0.018556\" /><BoneDefs ID=\"6\" ParentID=\"3\" Name=\"Golem_LShoulder\" x=\"-0.037239\" y=\"0.180833\" z=\"-0.001992\" /><BoneDefs ID=\"7\" ParentID=\"6\" Name=\"Golem_LUArm\" x=\"-0.118945\" y=\"0.000000\" z=\"0.000000\" /><BoneDefs ID=\"8\" ParentID=\"7\" Name=\"Golem_LFArm\" x=\"-0.286052\" y=\"0.000000\" z=\"0.000000\" /><BoneDefs ID=\"9\" ParentID=\"8\" Name=\"Golem_LHand\" x=\"-0.228390\" y=\"0.000000\" z=\"0.000000\" /><BoneDefs ID=\"10\" ParentID=\"3\" Name=\"Golem_RShoulder\" x=\"0.036927\" y=\"0.180833\" z=\"-0.001992\" /><BoneDefs ID=\"11\" ParentID=\"10\" Name=\"Golem_RUArm\" x=\"0.118945\" y=\"0.000000\" z=\"0.000000\" /><BoneDefs ID=\"12\" ParentID=\"11\" Name=\"Golem_RFArm\" x=\"0.286052\" y=\"0.000000\" z=\"0.000000\" /><BoneDefs ID=\"13\" ParentID=\"12\" Name=\"Golem_RHand\" x=\"0.228390\" y=\"0.000000\" z=\"0.000000\" /><BoneDefs ID=\"14\" ParentID=\"1\" Name=\"Golem_LThigh\" x=\"-0.092780\" y=\"0.000000\" z=\"0.000000\" /><BoneDefs ID=\"15\" ParentID=\"14\" Name=\"Golem_LShin\" x=\"0.000000\" y=\"-0.407353\" z=\"0.000000\" /><BoneDefs ID=\"16\" ParentID=\"15\" Name=\"Golem_LFoot\" x=\"0.000000\" y=\"-0.421415\" z=\"0.000000\" /><BoneDefs ID=\"18\" ParentID=\"1\" Name=\"Golem_RThigh\" x=\"0.092780\" y=\"0.000000\" z=\"0.000000\" /><BoneDefs ID=\"19\" ParentID=\"18\" Name=\"Golem_RShin\" x=\"0.000000\" y=\"-0.407353\" z=\"0.000000\" /><BoneDefs ID=\"20\" ParentID=\"19\" Name=\"Golem_RFoot\" x=\"0.000000\" y=\"-0.421415\" z=\"0.000000\" /><BoneDefs ID=\"17\" ParentID=\"16\" Name=\"Golem_LToe\" x=\"0.000000\" y=\"-0.060307\" z=\"0.139170\" /><BoneDefs ID=\"21\" ParentID=\"20\" Name=\"Golem_RToe\" x=\"0.000000\" y=\"-0.060307\" z=\"0.139170\" /></SkeletonDescription></SkeletonDescriptions>");
	}

	void readData(string fn)
	{
		int count = 0;
		motionData = new ArrayList();

		string line;
		StreamReader file = new StreamReader(fn);

		while ((line = file.ReadLine()) != null){
		
			// 120Hz => 60Hz
			if (count % 2 == 0) motionData.Add(line);

			count += 1;

			if (count > maxLength) break;
		}
	}
	
	Void OnGUI()
	{
		// GUI.color = Color.black;
		GUI.color = new Color(0f, 0f, 1.0f, 0.9f);
		
		string label = string.Format("{0,10}", currentFrame);
		GUI.Label(new Rect(0, 15, 100, 30), label);
	}
	
	

}