using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using OpenNI;
using NITE;

public class CubeController : MonoBehaviour {
	private readonly string XML_CONFIG=@".//OpenNI.xml";
	private const string WAVE="Wave";
	private const float MEDIDA_MANO_X=400f;
	private const float MEDIDA_MANO_Y=300f;
	private const float MEDIDA_MANO_Z=300f;
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
	//puntos mano
	private Point3D puntoMano;
	private float kinectManoX=0f;
	private float kinectManoY=0f;
	private float kinectManoZ=0f;
	//punto de referencia para el kinect
	private float kinectRefX=0f;
	private float kinectRefY=0f;
	private float kinectRefZ=0f;
	//distancias reales kinect
	private float kinectDistanciaX=0f;
	private float kinectDistanciaY=0f;
	private float kinectDistanciaZ=0f;
	
	
	void Start () {
		//Debug.Log("START");
		determinaCentro();
		
		//Debug.Log("X "+xMedia+" Y "+yMedia+" Z "+zMedia);
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
		this.puntoMano=e.Position;
		setPuntosManoActuales(this.puntoMano);
		iniciaPuntoReferencia(this.puntoMano);
		//Debug.Log("X "+puntoMano.X+" Y "+puntoMano.Y+" Z "+puntoMano.Z);
		//Debug.Log("**** X "+kinectRefX+" Y "+kinectRefY+" Z "+kinectRefZ);
		
	}
	
	void hands_HandUpdate(object sender, HandUpdateEventArgs e){
		//Debug.Log("Mano Update");
		setPuntosManoActuales(e.Position);
		distanciaKinectReales();
		calculaMovimiento();
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
		this.distanciaX=distanciaEntreDosPuntos(new Point3D(xInit,0f,0f),new Point3D(xEnd,0f,0f));
		this.distanciaY=distanciaEntreDosPuntos(new Point3D(0f,yInit,0f),new Point3D(0f,yEnd,0f));
		this.distanciaZ=distanciaEntreDosPuntos(new Point3D(0f,0f,zInit),new Point3D(0f,0f,zEnd));
		
	}
	
	void calculaPuntoMedio(){
		this.xMedia=xInit-(distanciaX/2);
		this.yMedia=yInit+(distanciaY/2);
		this.zMedia=zInit+(distanciaZ/2);
	}
	
	void determinaCentro(){
		inicializaPuntos();
		calculaDistanciaEje();
		calculaPuntoMedio();
	}
	
	void iniciaPuntoReferencia(Point3D puntoCentral){
		this.kinectRefX=puntoCentral.X+(MEDIDA_MANO_X/2f);
		this.kinectRefY=puntoCentral.Y-(MEDIDA_MANO_Y/2f);
		this.kinectRefZ=puntoCentral.Z+(MEDIDA_MANO_Z/2f);
	}
	
	void setPuntosManoActuales(Point3D a){
		this.kinectManoX=a.X;
		this.kinectManoY=a.Y;
		this.kinectManoZ=a.Z;
	}
	
	void distanciaKinectReales(){
		this.kinectDistanciaX=distanciaEntreDosPuntos(new Point3D(kinectRefX,0f,0f),new Point3D(kinectManoX,0f,0f));
		this.kinectDistanciaY=distanciaEntreDosPuntos(new Point3D(0f,kinectRefY,0f),new Point3D(0f,kinectManoY,0f));
		this.kinectDistanciaZ=distanciaEntreDosPuntos(new Point3D(0f,0f,kinectRefZ),new Point3D(0f,0f,kinectManoZ));
	}
	
	void calculaMovimiento(){
		//Debug.Log("Calcula Movimiento");
		float xNormal=((kinectDistanciaX/10f)*100f)/(MEDIDA_MANO_X/10f);
		float yNormal=((kinectDistanciaY/10f)*100f)/(MEDIDA_MANO_Y/10f);
		float zNormal=((kinectDistanciaZ/10f)*100f)/(MEDIDA_MANO_Z/10f);
		float x=(xNormal*distanciaX)/100;
		float y=(yNormal*distanciaY)/100;
		float z=(zNormal*distanciaZ)/100;
		Debug.Log(xNormal);
		
		transform.position=new Vector3(x,y,transform.position.z);
	}

}
