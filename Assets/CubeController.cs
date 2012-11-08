using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using OpenNI;
using NITE;

public class CubeController : MonoBehaviour {
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	private const string WAVE="Wave";
	private const string POSITIVO="+";
	private const string NEGATIVO="-";
	private Context context;
	private ScriptNode scriptNode;
	private DepthGenerator depth;
	private HandsGenerator hands;
	private GestureGenerator gesture;
	//ultimo punto de la mano
	private Point3D puntoInicial;
	private Point3D puntoFinal;
	//puntos inicial y final
	private float xInit=15.93f;
	private float yInit=3.85f;
	private float zInit=-50.91f;
	private float xEnd=-0.009f;
	private float yEnd=9.71f;
	private float zEnd=-34.93f;
	//distancias entre los puntos
	private float distanciaX=0f;
	private float distanciaY=0f;
	private float distanciaZ=0f;
	//puntos medios
	private Point3D puntoMitad;
	private float xMedia=0f;
	private float yMedia=0f;
	private float zMedia=0f;
	
	
	void Start () {
		//Debug.Log("START");
		determinaCentro();
		
		Debug.Log("X "+xMedia+" Y "+yMedia+" Z "+zMedia);
		transform.position=new Vector3(xMedia,yMedia,zMedia);
		
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
		//Debug.Log("Distancias X: "+distanciaX+" Y: "+distanciaY+" Z: "+distanciaZ);
	
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
	}
	
	void hands_HandUpdate(object sender, HandUpdateEventArgs e){
		Debug.Log("Mano Update");
	}
	
	void hands_HandDestroy(object sender, HandDestroyEventArgs e){
		Debug.Log("Mano destroy");
	}
	
	
	void inicializaPuntos(){
		this.puntoInicial=new Point3D(xInit,yInit,zInit);
		this.puntoFinal=new Point3D(xEnd,yEnd,zEnd);
	}
	
	float distanciaEntreDosPuntos(Point3D a,Point3D b){
		return Mathf.Sqrt(elevaCuadrado(a.X-b.X)+elevaCuadrado(a.Y-b.Y)+elevaCuadrado(a.Z-b.Z));
	}
	
	float elevaCuadrado(float numero){
		return Mathf.Pow(numero,2f);
	}
	
	void calculaDistanciaEje(){
		distanciaX=distanciaEntreDosPuntos(new Point3D(xInit,0f,0f),new Point3D(xEnd,0f,0f));
		distanciaY=distanciaEntreDosPuntos(new Point3D(0f,yInit,0f),new Point3D(0f,yEnd,0f));
		distanciaZ=distanciaEntreDosPuntos(new Point3D(0f,0f,zInit),new Point3D(0f,0f,zEnd));
		
	}
	
	void calculaPuntoMedio(){
		xMedia=xInit-(distanciaX/2);
		yMedia=yInit+(distanciaY/2);
		zMedia=zInit+(distanciaZ/2);
	}
	
	void determinaCentro(){
		inicializaPuntos();
		calculaDistanciaEje();
		calculaPuntoMedio();
	}
}
