using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using OpenNI;
using NITE;

public class CubeController : MonoBehaviour {
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	private const string WAVE="Wave";
	private Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private HandsGenerator hands;
	private GestureGenerator gesture;
	private Point3D puntoInicial=Point3D.ZeroPoint;
	private float zAnterior=0;
	// Use this for initialization
	void Start () {
		//Debug.Log("START");
		this.context=Context.CreateFromXmlFile(XML_CONFIG, out scriptNode);
		this.depth=this.context.FindExistingNode(NodeType.Depth) as DepthGenerator;
		if(depth==null){
			throw new Exception("Nodo de Profundidad no encontrado");
		}
		this.hands=this.context.FindExistingNode(NodeType.Hands) as HandsGenerator;
		if(this.hands==null){
			throw new Exception("Nodo de Manos no encontrado");
		}
		this.gesture=this.context.FindExistingNode(NodeType.Gesture) as GestureGenerator;
		if(this.gesture==null){
			throw new Exception("Nodo de Gestos no encontrado");
		}
		//handdlers
		this.hands.HandCreate+=hands_HandCreate;
		this.hands.HandUpdate+=hands_HandUpdate;
		this.hands.HandDestroy+=hands_HandDestroy;
		
		this.gesture.AddGesture(WAVE);
		this.gesture.GestureRecognized+=gesture_GestureRecognized;
		this.gesture.StartGenerating();
		
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log("Update");
		this.context.WaitOneUpdateAll(this.depth);
	
	}
	
	void OnApplicationQuit(){
		Debug.Log("Saliendo de la aplicacion");
		context.Release();
	}
	
	void gesture_GestureRecognized(object sender, GestureRecognizedEventArgs e){
		if(e.Gesture==WAVE){
			this.hands.StartTracking(e.EndPosition);
		}
	}
	
	void hands_HandCreate(object sender, HandCreateEventArgs e){
		Debug.Log("Mano Creada");
		this.puntoInicial=e.Position;
	}
	
	void hands_HandUpdate(object sender, HandUpdateEventArgs e){
		Debug.Log("Mano Update");
		Debug.Log("X "+transform.position.x+" Y "+transform.position.y+" Z "+transform.position.z);
	}
	
	void hands_HandDestroy(object sender, HandDestroyEventArgs e){
		Debug.Log("Mano destroy");
		puntoInicial=Point3D.ZeroPoint;
		zAnterior=0f;
	}
}
