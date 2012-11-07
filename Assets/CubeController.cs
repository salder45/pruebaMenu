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
	//Punto donde inicia el Escenario
	private Point3D pEI=Point3D.ZeroPoint;
	//punto donde terminar el escenario
	private Point3D pO=Point3D.ZeroPoint;
	//Coordenadas para punto inicial
	private float xInit=15.93f;
	private float yInit=3.85f;
	private float zInit=-50.91f;
	//Coordenadas Finales
	private float xEnd=-0.009f;
	private float yEnd=9.71f;
	private float zEnd=-34.93f;
	//distancias por eje
	private float distanciaX=0f;
	private float distanciaY=0f;
	private float distanciaZ=0f;
	//signos de las coordenadas
	private string signoXInit="*";
	private string signoYInit="*";
	private string signoZInit="*";
	private string signoXEnd="*";
	private string signoYEnd="*";
	private string signoZEnd="*";
	
	void Start () {
		//Debug.Log("START");
		//Inicializar puntos
		this.pEI=new Point3D(xInit,yInit,zInit);
		this.pO=new Point3D(xEnd,yEnd,zEnd);
		//inicia las distancias
		calculaDistanciaEjes();	
		float puntoMedioX=(distanciaX);		
		Debug.Log(puntoMedioX);
		Debug.Log((puntoMedioX+xInit));
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
	
	void calculaDistanciaEjes(){
		distanciaX=distanciaEntreDosPuntos(new Point3D(xInit,0f,0f),new Point3D(xEnd,0f,0f));
		distanciaY=distanciaEntreDosPuntos(new Point3D(0f,yInit,0f),new Point3D(0f,yEnd,0f));
		distanciaZ=distanciaEntreDosPuntos(new Point3D(0f,0f,zInit),new Point3D(0f,0f,zEnd));
	}
	
	float distanciaEntreDosPuntos(Point3D a,Point3D b){
		return Mathf.Sqrt(elevaCuadrado(b.X-a.X)+elevaCuadrado(b.Y-a.Y)+elevaCuadrado(b.Z-a.Z));
	}
	
	float elevaCuadrado(float numero){
		return Mathf.Pow(numero,2f);
	}
	
	void determinaSignos(){
		
	}
	
	string retornaSignoNumero(float numero){
		string retorno="*";
		if(isPositivo(numero)){
			retorno=POSITIVO;
		}else {
			retorno=NEGATIVO;
		}
		
		return retorno;
	}
	
	bool isPositivo(float number){
		bool retorno=true;
		if(number<0){
			retorno=false;
		}
		
		return retorno;
	}
	
}
