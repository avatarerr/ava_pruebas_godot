using Godot;
using System;
using System.Collections.Generic;


public class Principal : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    int camaraactiva = 0;
    float MOUSE_SENSITIVITY = (float)0.5;
     List<EnemigoCajaBase> listaenemigos = new List<EnemigoCajaBase>();

     HUD_Debug hud_debug;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        Random rdd = new Random();
    
        globales.NODOJUGADOR = (Jugador)GetNode("Jugador");
        globales.NODOPRINCIPAL = this;

        inicia_cosas();
    }

    private void inicia_cosas()
    {
        try {
            Vector2 tamscreen = GetViewport().GetVisibleRect().Size;
            Vector2 posicionHUD = new Vector2(tamscreen.x / 2 - 10, tamscreen.y / 2 - 10);

            Control HUD = (Control)GetNode("HUD");
            HUD.SetPosition(posicionHUD);

            // pruebas

            hud_debug = (HUD_Debug)GetNode("HUD_Debug");
/*
            for (int i=0;i<1000;i++)
                anyadeEnemigo();
*/
        }
        catch(Exception ex) {

        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public override void _Input(InputEvent evento) {
        try {
            if (Input.IsActionJustPressed("ui_cancel")) {
                if (Input.GetMouseMode() == Input.MouseMode.Visible)
                    Input.SetMouseMode(Input.MouseMode.Captured);
                else Input.SetMouseMode(Input.MouseMode.Visible);
            }

            if (Input.IsActionJustPressed("ui_focus_next")) {
                if (camaraactiva == 0) camaraactiva = 1;
                else camaraactiva = 0;

                cambia_camara(camaraactiva);
            }

            if (Input.IsActionJustPressed("raton_clickizquierdo")) {
                dispara();
            }
            /*
            if event is InputEventMouseButton:
            if event.doubleclick:
                $Jugador.recible_haz_esquiva()
                */


            if (Input.IsActionJustPressed("pulsa_intro")) {
                // un enemigo al azar ataca
                if (listaenemigos.Count==0) return;

                RandomNumberGenerator objrand = new RandomNumberGenerator();
                objrand.Randomize();
                int indice = objrand.RandiRange(0,listaenemigos.Count-1);
                EnemigoCajaBase enemigo = listaenemigos[indice];
                enemigo.ataca();

            }

            if (Input.IsActionJustPressed("pulsa_1")) {
              //  enemigoprueba.estableceObjetivoJugador();
                anyadeEnemigo();
            }

              if (Input.IsActionJustPressed("pulsa_2")) {
                foreach(EnemigoCajaBase enemigo in listaenemigos) {
                    enemigo.poneBuscaJugador();

                }
            }  

            if (Input.IsActionJustPressed("pulsa_3")) {
                foreach(EnemigoCajaBase enemigo in listaenemigos) {
                    enemigo.poneEnMovimiento();

                }
            }    

            if (Input.IsActionJustPressed("pulsa_4")) {
                foreach(EnemigoCajaBase enemigo in listaenemigos) {
                    enemigo.poneAcelerando();

                }
            }    

             if (Input.IsActionJustPressed("pulsa_5")) {
                foreach(EnemigoCajaBase enemigo in listaenemigos) {
                    enemigo.poneEnAtaque();

                }
            }   

            if (evento is InputEventMouseMotion eventm && Input.GetMouseMode() == Input.MouseMode.Captured)
            {

                globales.NODOJUGADOR.recibe_giro_horizontal(-1 * eventm.Relative.x * MOUSE_SENSITIVITY);
                globales.NODOJUGADOR.recibe_giro_vertical(1 * eventm.Relative.y * MOUSE_SENSITIVITY);
            }
        }
        catch(Exception ex) {

        }
    }

    public override void _PhysicsProcess(float delta) {
        globales.NODOJUGADOR.desplaza(delta);

        
        float fps = Performance.GetMonitor(0);
        hud_debug.cambiaFPS(fps);
    }

    public void cambia_camara(int camara) {
        try {
        if (camara == 1)
        {
            /*
            Camera objcamara = (Camera)GetNode("CameraPruebas");
            objcamara.Translation = globales.NODOJUGADOR.Translation + new Vector3(5,5,5);
            objcamara.LookAt(globales.NODOJUGADOR.Translation,new Vector3(0,1,0));
            objcamara.Current = true;
            */

            Jugador  jugador = (Jugador)GetNode("Jugador");
            Spatial Cabeza = (Spatial)jugador.GetNode("Cabeza");
            Camera objcamera = (Camera)Cabeza.GetNode("CamaraAtras");
            objcamera.Current = true;
        }

        if (camara == 0)
        {
            Jugador  jugador = (Jugador)GetNode("Jugador");
            Spatial Cabeza = (Spatial)jugador.GetNode("Cabeza");
            Camera objcamera = (Camera)Cabeza.GetNode("Camera");
            objcamera.Current = true;

            Control hud = (Control)GetNode("HUD");
            hud.Visible = true;
        }
        }
        catch(Exception ex) {

        }
    }

    public void dispara() {
        try {
            Jugador jugador = (Jugador)GetNode("Jugador");

            // efecto del disparo
            jugador.disparaArma();

            Spatial Cabeza = (Spatial)jugador.GetNode("Cabeza");
            Camera objcamera = (Camera)Cabeza.GetNode("Camera");
            RayCast ray = (RayCast)objcamera.GetNode("RayCast");

            if (ray.Enabled && ray.IsColliding()) {
                Godot.Object objetivo = (Godot.Object)ray.GetCollider();

                Vector3 vectorimpacto = ray.GetCollisionPoint();

                // si el impacto lo recibe un objeto preparado para recibir balas
                if (objetivo.HasMethod("recibeBala")) {
                    // calculamos la direccion del impacto
                    Vector3 inicio = ray.Translation;
                    Vector3 vectordir = vectorimpacto-inicio;

                    objetivo.Call("recibeBala",vectordir);
                }

                // pintamos el impacto de la bala

                // debug con cajas en vez de impacto 

                poneCajaEscenario(vectorimpacto);

                /*
                PackedScene objeto = (PackedScene)ResourceLoader.Load("res://objetos/mini/ImpactoBalaPared.tscn");
              //  objetos.mini.ImpactoBalaPared efecto = (objetos.mini.ImpactoBalaPared)objeto.Instance();
                ImpactoBalaPared efecto = (ImpactoBalaPared)objeto.Instance();
                AddChild(efecto);            
                efecto.haceEfecto(vectorimpacto);              
                */

            
            }
        }
        catch(Exception ex) {

        }
    }

    private void anyadeEnemigo() {
        try {
            RandomNumberGenerator objrand = new RandomNumberGenerator();
            objrand.Randomize();

            Vector3 posicion = new Vector3(objrand.RandfRange(-50,30),objrand.RandfRange(5,20),objrand.RandfRange(-40,40));
            int tipo = objrand.RandiRange(1,2);

            PackedScene escena = (PackedScene)ResourceLoader.Load("res://EnemigoCajaBase.tscn");
            
            EnemigoCajaBase enemigo = (EnemigoCajaBase)escena.Instance();

            enemigo.precarga(posicion,2);
            AddChild(enemigo);

            listaenemigos.Add(enemigo);
        }
        catch(Exception ex) {

        }
    }

    public void informaMuerteCaja(EnemigoCajaBase enemigo) {
        try {
            listaenemigos.Remove(enemigo);

            PackedScene escena = (PackedScene)ResourceLoader.Load("res://MuerteCaja.tscn");

            MuerteCaja item = (MuerteCaja)escena.Instance();
            item.Translation = enemigo.Translation;
            AddChild(item);
        }
        catch(Exception ex) {

        }
    }

    // funciones debug

    public void poneCajaEscenario(Vector3 posicion) {
        try {
          PackedScene escena = (PackedScene)ResourceLoader.Load("res://EscenarioCaja.tscn");
            
            Spatial enemigo = (Spatial)escena.Instance();

            enemigo.Translation = posicion;
            AddChild(enemigo);
        }
        catch(Exception ex) {

        }
    }
}
