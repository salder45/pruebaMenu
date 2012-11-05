using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using OpenNI;
using NITE;

public class CubeController : MonoBehaviour {
	private Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private UserGenerator userGenerator;
	private SkeletonCapability skeletonCapability;
	private PoseDetectionCapability poseDetectionCapability;
	private string calibPose;
	private Dictionary <int, Dictionary<SkeletonJoint,SkeletonJointPosition>> joints;
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	private bool shouldRun=false;
	private const float MEDIDA_X=14f;
	private const float MEDIDA_Y=14f;
	private const float VALOR_CLICK=150f;
	//
	
	// Use this for initialization
	void Start () {
		Debug.Log("START");
		this.context=Context.CreateFromXmlFile(XML_CONFIG, out scriptNode);
		this.depth=this.context.FindExistingNode(NodeType.Depth) as  DepthGenerator;
		if(this.depth==null){
			throw new Exception("Nodo de Profundidad no encontrado");
		}
		
		this.userGenerator=new UserGenerator(this.context);
		this.skeletonCapability=this.userGenerator.SkeletonCapability;
		this.poseDetectionCapability=this.userGenerator.PoseDetectionCapability;
		this.calibPose=this.skeletonCapability.CalibrationPose;
		this.userGenerator.NewUser+=this.userGenerator_NewUser;
		this.userGenerator.LostUser+=this.userGenerator_LostUser;
		this.poseDetectionCapability.PoseDetected+=this.poseDetectionCapability_PoseDetected;
		this.skeletonCapability.CalibrationComplete+=this.skeletonCapability_CalibrationComplete;
		this.skeletonCapability.SetSkeletonProfile(SkeletonProfile.All);
		
		this.joints=new Dictionary<int,Dictionary<SkeletonJoint,SkeletonJointPosition>>();
		this.userGenerator.StartGenerating();
		this.shouldRun=true;
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("Update");
		if(this.shouldRun){
			try{
				this.context.WaitOneUpdateAll(this.depth);
			}catch(Exception){
				Debug.Log("No paso");
			}
		}
		
		int[] users=this.userGenerator.GetUsers();
		foreach(int user in users){
			if(this.skeletonCapability.IsTracking(user)){
				SkeletonJointPosition sjpTorso=skeletonCapability.GetSkeletonJointPosition(user,SkeletonJoint.Torso);
				SkeletonJointPosition sjpManoDr=skeletonCapability.GetSkeletonJointPosition(user,SkeletonJoint.LeftHand);
				
				
				//transform.Translate(new Vector3(despEnX,despEnY,transform.position.z));
			}
		}
	
	}
	
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		context.Release();
	}
	
	void userGenerator_NewUser(object sender, NewUserEventArgs e){
		Debug.Log("New User");
		if(this.skeletonCapability.DoesNeedPoseForCalibration){
			this.poseDetectionCapability.StartPoseDetection(this.calibPose,e.ID);
		}else{
			this.skeletonCapability.RequestCalibration(e.ID, true);
		}
	}
	
	void userGenerator_LostUser(object sender, UserLostEventArgs e){
		Debug.Log("Lost User");
		this.joints.Remove(e.ID);
	}
	
	void poseDetectionCapability_PoseDetected(object sender, PoseDetectedEventArgs e){
		Debug.Log("Pose Detection");
		this.poseDetectionCapability.StopPoseDetection(e.ID);
		this.skeletonCapability.RequestCalibration(e.ID,true);
	}
	
	void skeletonCapability_CalibrationComplete(object sender, CalibrationProgressEventArgs e){
		Debug.Log("Calibration Complete");
		if(e.Status==CalibrationStatus.OK){
			this.skeletonCapability.StartTracking(e.ID);
			this.joints.Add(e.ID, new Dictionary<SkeletonJoint, SkeletonJointPosition>());
		}else if(e.Status!=CalibrationStatus.ManualAbort){
			if (this.skeletonCapability.DoesNeedPoseForCalibration){
                    this.poseDetectionCapability.StartPoseDetection(calibPose, e.ID);
                }else{
                    this.skeletonCapability.RequestCalibration(e.ID, true);
                }
		}
	}
	
	float distanciaEntreDosPuntos(Point3D a,Point3D b){
		return Mathf.Sqrt(elevaCuadrado(b.X-a.X)+elevaCuadrado(b.Y-a.Y));
	}

	float elevaCuadrado(float numero){
		return (float)Math.Pow(numero,2);
	}
	
	float distanciaEnUnEje(float c, float torso,float eje){
		return Mathf.Sqrt(elevaCuadrado(c)-elevaCuadrado(torso-eje));
	}
}
