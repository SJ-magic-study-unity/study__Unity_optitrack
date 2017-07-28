/************************************************************
■XML
	http://gogodiet.net/z/xml/
************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

/************************************************************
************************************************************/

public class LivePoseAnimator : MonoBehaviour
{
	/****************************************
	****************************************/
	private Avatar mocapAvatar;
	public Avatar destinationAvatar;

	public HumanPoseHandler SourcePoseHandler;
	public HumanPoseHandler HumanPoseHandler;

	public HumanPose HumanPose;
	
	private string mPacket;
	private bool mNew = false;
	
	public string Actor = "Golem";
	
	//== bone mapping look-up table ==--
	private Dictionary<string, string> mBoneDictionary = new Dictionary<string, string>();
	private Dictionary<string, string> mBoneToSkeleton = new Dictionary<string, string>();
	
	private Dictionary<string, GameObject> GhostSkelton = new Dictionary<string, GameObject>();
	

	/****************************************
	****************************************/
	/******************************
	******************************/
	void Start()
	{
		//== set up the pose handler ==--
		HumanPoseHandler = new HumanPoseHandler(destinationAvatar, this.transform);
		SourcePoseHandler = null;

		PrepareBoneDictionary();
		PrepareBoneToSkeleton();
	}

	/******************************
	******************************/
	void Update()
	{
		if(mNew){
			if( mPacket==null ) return;
			
			mNew = false;
			
			/********************
			********************/
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(mPacket);

			XmlNodeList definitions = xmlDoc.GetElementsByTagName("SkeletonDescriptions");

			if( definitions.Count>0 ){
				ParseSkeletonDefinitions( xmlDoc );
				return;
			}

			/********************
			********************/
			if (SourcePoseHandler == null){
				return;
			}
			
			/********************
			update avatar : every Frame.
			********************/
			XmlNodeList boneList = xmlDoc.GetElementsByTagName("Bone");

			for (int index = 0; index < boneList.Count; index++){
				string boneName = boneList[index].Attributes["Name"].InnerText;
				
				float x = (float)System.Convert.ToDouble(boneList[index].Attributes["x"].InnerText);
				float y = (float)System.Convert.ToDouble(boneList[index].Attributes["y"].InnerText);
				float z = (float)System.Convert.ToDouble(boneList[index].Attributes["z"].InnerText);

				float qx = (float)System.Convert.ToDouble(boneList[index].Attributes["qx"].InnerText);
				float qy = (float)System.Convert.ToDouble(boneList[index].Attributes["qy"].InnerText);
				float qz = (float)System.Convert.ToDouble(boneList[index].Attributes["qz"].InnerText);
				float qw = (float)System.Convert.ToDouble(boneList[index].Attributes["qw"].InnerText);

				//== coordinate system conversion (right to left handed) ==--
				x = -x;
				qx = -qx;
				qw = -qw;

				//== bone pose ==--
				Vector3 position = new Vector3(x, y, z);
				Quaternion orientation = new Quaternion(qx, qy, qz, qw);

				string objectName = boneName;
				
				/* */
				if(GhostSkelton.ContainsKey(objectName)){
					GameObject bone = GhostSkelton[objectName];
					if (index == 0) bone.transform.localPosition = position;
					
					bone.transform.localRotation = orientation;
				}

				/*
				GameObject bone;
				bone = GameObject.Find(objectName);
				if (bone != null){
					if (index == 0) bone.transform.localPosition = position;
					
					bone.transform.localRotation = orientation;
				}
				*/
			}

			/********************
			********************/
			SourcePoseHandler.GetHumanPose(ref HumanPose);
			HumanPoseHandler.SetHumanPose(ref HumanPose);
		}
	}

	/******************************
	******************************/
	private void ParseSkeletonDefinitions( XmlDocument xmlDoc )
	{
		XmlNodeList skeletonList = xmlDoc.GetElementsByTagName("SkeletonDescription");

		for (int index = 0; index < skeletonList.Count; index++){
			ParseSkeletonDefinition( skeletonList.Item( index ) );
		} 
	}

	/******************************
	******************************/
	private void ParseSkeletonDefinition( XmlNode skeleton )
	{
		/********************
		********************/
		string actorName = skeleton.Attributes["Name"].InnerText;

		if( actorName!=Actor ){
			return;
		}

		GameObject actorObject = GameObject.Find(actorName);

		if (actorObject != null){
			return;
		}

		/********************
		skelton:親子関係でつながるGameObjectの作成
		********************/
		actorObject = new GameObject();

		Vector3 actorScale = new Vector3(1,1,1);
		actorObject.transform.localScale = actorScale;
		actorObject.name = actorName;

		Dictionary<int, string> hierarchy = new Dictionary<int, string>();
		for (int i = 0; i < skeleton.ChildNodes.Count; i++){
			XmlNode bone = skeleton.ChildNodes.Item( i );

			string boneName = bone.Attributes["Name"].InnerText;

			int	ID	   = System.Convert.ToInt32(bone.Attributes["ID"].InnerText);

			hierarchy.Add( ID, boneName );
		}

		for (int i = 0; i < skeleton.ChildNodes.Count; i++){
			XmlNode bone = skeleton.ChildNodes.Item(i);

			string boneName = bone.Attributes["Name"].InnerText;

			int	parentID = System.Convert.ToInt32(bone.Attributes["ParentID"].InnerText);
			float  x		= (float)System.Convert.ToDouble(bone.Attributes["x"].InnerText);
			float  y		= (float)System.Convert.ToDouble(bone.Attributes["y"].InnerText);
			float  z		= (float)System.Convert.ToDouble(bone.Attributes["z"].InnerText);

			string objectName = boneName;
			GameObject parentObject = null;
			if( parentID==0 ){
				parentObject = actorObject;
				
			}else{
				if(hierarchy.ContainsKey(parentID)){
					string parentName = hierarchy[parentID];

					parentObject = GameObject.Find( parentName ); // 必ずparentが存在するよう、xmlの順序に気をつけてある.
				}
			}

			GameObject boneObject;

			boneObject = GameObject.Find(objectName);

			if (boneObject == null){
				boneObject = new GameObject();

				if (parentObject != null){
					boneObject.transform.parent = parentObject.transform;
				}
				Vector3 scale = new Vector3( 1,1,1 );
				boneObject.transform.localScale = scale;
				boneObject.name = objectName;

				Vector3 position = new Vector3(x, y, z);
				boneObject.transform.localPosition = position;
				
				/* */
				GhostSkelton.Add(objectName, boneObject);
			}
		}
		
		/********************
		HumanDescriptionの作成
		********************/
		HumanDescription desc = new HumanDescription();
		
		HumanBone[] humanBones = new HumanBone[ skeleton.ChildNodes.Count ]; // Macanim Bone name - Rig Bone name:mapping
			
		string[] humanName = HumanTrait.BoneName; // macanimに定義されたBone name.

		int j = 0;
		int ii = 0;
		while (ii < humanName.Length) {
			if (mBoneToSkeleton.ContainsKey(humanName[ii])){
				HumanBone humanBone = new HumanBone();
				humanBone.humanName = humanName[ii];
				humanBone.boneName = mBoneToSkeleton[humanName[ii]];
				humanBone.limit.useDefaultValues = true;
				humanBones[j++] = humanBone;
			}
			ii++;
		}

		SkeletonBone[] skeletonBones = new SkeletonBone[skeleton.ChildNodes.Count+1]; // 各Bone(GameObject)のT-pose transform設定.

		SkeletonBone sBone = new SkeletonBone();
		sBone.name = actorName;
		sBone.position = new Vector3(0, 0, 0);
		sBone.rotation = Quaternion.identity;
		sBone.scale = new Vector3(1, 1, 1);
		skeletonBones[0] = sBone;

		for (int i = 0; i < skeleton.ChildNodes.Count; i++){
			XmlNode bone = skeleton.ChildNodes.Item(i);

			string boneName = bone.Attributes["Name"].InnerText;

			float x = (float)System.Convert.ToDouble(bone.Attributes["x"].InnerText);
			float y = (float)System.Convert.ToDouble(bone.Attributes["y"].InnerText);
			float z = (float)System.Convert.ToDouble(bone.Attributes["z"].InnerText);

			SkeletonBone skeletonBone = new SkeletonBone();
			skeletonBone.name = boneName;

			skeletonBone.position = new Vector3(x, y, z);
			skeletonBone.rotation = Quaternion.identity;
			skeletonBone.scale = new Vector3( 1,1,1 );
			skeletonBones[i+1] = skeletonBone;
		}

		//set the bone arrays right
		desc.human = humanBones;
		desc.skeleton = skeletonBones;

		//set the default values for the rest of the human descriptor parameters
		desc.upperArmTwist = 0.5f;
		desc.lowerArmTwist = 0.5f;
		desc.upperLegTwist = 0.5f;
		desc.lowerLegTwist = 0.5f;
		desc.armStretch = 0.05f;
		desc.legStretch = 0.05f;
		desc.feetSpacing = 0.0f;
		desc.hasTranslationDoF = false;

		/********************
		********************/
		mocapAvatar = AvatarBuilder.BuildHumanAvatar(GameObject.Find(actorName), desc);
		SourcePoseHandler = new HumanPoseHandler(mocapAvatar, GameObject.Find( actorName ).transform);
	}

	/******************************
	//== incoming real-time pose data ==--
	******************************/
	public void OnPacketReceived(object sender, string Packet)
	{
		mPacket = Packet;
		mNew = true;
	}

	/******************************
	******************************/
	public void OnSamplePacket(string packet)
	{
		mPacket = packet.Replace("Golem", Actor);
		mNew = true;
	}

	/******************************
	Dictionary not used
	******************************/
	public void PrepareBoneDictionary()
	{
		mBoneDictionary.Clear();
		mBoneDictionary.Add(Actor + "_Hips", "Hips");
		mBoneDictionary.Add(Actor + "_Ab", "Spine");
		mBoneDictionary.Add(Actor + "_Chest", "Spine1");
		mBoneDictionary.Add(Actor + "_Neck", "Neck");
		mBoneDictionary.Add(Actor + "_Head", "Head");
		mBoneDictionary.Add(Actor + "_LShoulder", "LeftShoulder");
		mBoneDictionary.Add(Actor + "_LUArm", "LeftArm");
		mBoneDictionary.Add(Actor + "_LFArm", "LeftForeArm");
		mBoneDictionary.Add(Actor + "_LHand", "LeftHand");
		mBoneDictionary.Add(Actor + "_RShoulder", "RightShoulder");
		mBoneDictionary.Add(Actor + "_RUArm", "RightArm");
		mBoneDictionary.Add(Actor + "_RFArm", "RightForeArm");
		mBoneDictionary.Add(Actor + "_RHand", "RightHand");
		mBoneDictionary.Add(Actor + "_LThigh", "LeftUpLeg");
		mBoneDictionary.Add(Actor + "_LShin", "LeftLeg");
		mBoneDictionary.Add(Actor + "_LFoot", "LeftFoot");
		mBoneDictionary.Add(Actor + "_RThigh", "RightUpLeg");
		mBoneDictionary.Add(Actor + "_RShin", "RightLeg");
		mBoneDictionary.Add(Actor + "_RFoot", "RightFoot");
		mBoneDictionary.Add(Actor + "_LToe", "LeftToeBase");
		mBoneDictionary.Add(Actor + "_RToe", "RightToeBase");
	}

	/******************************
	******************************/
	public void PrepareBoneToSkeleton()
	{
		mBoneToSkeleton.Clear();
		mBoneToSkeleton.Add("Hips", Actor + "_Hips");		
		mBoneToSkeleton.Add("Spine", Actor + "_Ab");		
		mBoneToSkeleton.Add("Chest", Actor + "_Chest");
		mBoneToSkeleton.Add("Neck", Actor + "_Neck");	
		mBoneToSkeleton.Add("Head", Actor + "_Head");	  
		mBoneToSkeleton.Add("LeftShoulder",Actor + "_LShoulder"); 
		mBoneToSkeleton.Add("LeftUpperArm",Actor + "_LUArm");	 
		mBoneToSkeleton.Add("LeftLowerArm",Actor + "_LFArm");	 
		mBoneToSkeleton.Add("LeftHand",Actor + "_LHand");	 
		mBoneToSkeleton.Add("RightShoulder",Actor + "_RShoulder"); 
		mBoneToSkeleton.Add("RightUpperArm",Actor + "_RUArm");	 
		mBoneToSkeleton.Add("RightLowerArm",Actor + "_RFArm");	 
		mBoneToSkeleton.Add("RightHand",Actor + "_RHand");	 
		mBoneToSkeleton.Add("LeftUpperLeg",Actor + "_LThigh");	
		mBoneToSkeleton.Add("LeftLowerLeg",Actor + "_LShin");	 
		mBoneToSkeleton.Add("LeftFoot",Actor + "_LFoot");	 
		mBoneToSkeleton.Add("RightUpperLeg",Actor + "_RThigh");	
		mBoneToSkeleton.Add("RightLowerLeg",Actor + "_RShin");	 
		mBoneToSkeleton.Add("RightFoot",Actor + "_RFoot");	 
		mBoneToSkeleton.Add("LeftToeBase",Actor + "_LToe");	  
		mBoneToSkeleton.Add("RightToeBase",Actor + "_RToe");	  
	}
}
