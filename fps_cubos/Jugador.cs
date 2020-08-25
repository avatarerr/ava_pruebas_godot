using Godot;
using System;

public class Jugador : KinematicBody
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    int speed = 10;
    int recibeordenmovimiento = 0;
    Vector3 dashdireccion = new Vector3();
    Vector3 dashdestino = new Vector3();

    private int golpeado = 0;
    private Vector3 direcciongolpeado = new Vector3();
    private Timer timergolpeado = new Timer();


    private float VELOCIDADGOLPEADO = 25;

    Texture textura1 = (Texture)GD.Load("res://art/texturas/caraenfadado.png");
    Texture textura2 = (Texture)GD.Load("res://art/texturas/caraenfadado_alt.png");

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        inicia_cosas();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    private void inicia_cosas() {
        timergolpeado.WaitTime = (float)0.5;
        timergolpeado.Connect("timeout",this, nameof(timerataque_tick));
        this.AddChild(timergolpeado);

    }
    public void recibe_haz_esquiva()
    {
        recibeordenmovimiento = 1;
    }

    public void recibe_giro_horizontal(float incremento) {

        if (dashdireccion != new Vector3())
            return;

        this.RotateObjectLocal(new Vector3(0, 1, 0), Mathf.Deg2Rad(incremento));
    }

    public void recibe_giro_vertical(float incremento) {
        try {
        if (dashdireccion != new Vector3()) {
            return;
            }


        Spatial Cabeza = (Spatial)GetNode("Cabeza");
        Camera camara = (Camera)Cabeza.GetNode("Camera");

        Vector3 degactual = camara.RotationDegrees;

        if (degactual.x > 70 && incremento < 0) {
            return;
                }

        if (degactual.x < -70 && incremento > 0) {
            return;
                    }

        camara.RotateObjectLocal(new Vector3(-1, 0, 0), Mathf.Deg2Rad(incremento));
        }
        catch(Exception ex) {
            GD.PrintS("Error:"+ex.ToString());
        }

    }


    public Spatial devuelveNodoCabeza() {
        try {
            Spatial Cabeza = (Spatial)GetNode("Cabeza");
            return Cabeza;
            
        }
        catch(Exception ex) {
            return null;

        }
    }
    // ****************** FUNCION PRINCIPAL DESPLAZAMIENTO *******

    public override void _PhysicsProcess(float delta)
    {
        try {
            if (golpeado>0) {
                Transform trans = this.GlobalTransform;
              //  Vector3 directionlocal = trans.basis.Xform(direcciongolpeado);
                Vector3 directionlocal = direcciongolpeado;
                KinematicCollision choque = MoveAndCollide(directionlocal * VELOCIDADGOLPEADO * delta);
            }


        }
        catch(Exception ex) {

        }
    }

    public void desplaza(float delta) {
        try {

            if (golpeado>0) return; // de momento no te puedes desplazar si has sido golpeado;

            Vector3 direction = new Vector3();
            float velocidad = speed;

            if (Input.IsActionPressed("ui_up"))
                direction -= new Vector3(0, 0, 1);
            if (Input.IsActionPressed("ui_down"))
                direction += new Vector3(0, 0, 1);

            if (Input.IsActionPressed("ui_left"))
                direction -= new Vector3(1, 0, 0);
            if (Input.IsActionPressed("ui_right"))
                direction += new Vector3(1, 0, 0);

            // recibe dash
            if (recibeordenmovimiento == 1) {
                recibeordenmovimiento = 0;
                dashdireccion = direction;
                dashdestino = direction * 8;
                Transform transa = this.GlobalTransform;
                Vector3 haciadonde = transa.Xform(dashdestino);
                dashdestino = haciadonde;
            }


            // efectua dash
            if (dashdireccion != new Vector3()) {
                // comprobamos si ya hemos llegado al destino del dash

                Vector3 posicionactual = this.Translation;
                Vector3 vectdistancia = posicionactual - dashdestino;
                float distvect = vectdistancia.Length();


                if (vectdistancia.Length() < 2)
                    cancela_dash();
                else {
                    direction = dashdireccion;
                    velocidad = speed * 5;
                }
            }


            if (direction == new Vector3())
                return;


            direction = direction.Normalized();
            // translate_object_local(direction*speed*delta)

            Transform trans = this.GlobalTransform;
            Vector3 directionlocal = trans.basis.Xform(direction);

            KinematicCollision choque = MoveAndCollide(directionlocal * velocidad * delta);
            if (choque != null)
                cancela_dash();
        }
        catch(Exception ex) {
            
        }
}

    // var choque = move_and_slide(direction*speed*delta, Vector3(0, 1, 0))

    public void cancela_dash() {
        dashdireccion = new Vector3();
        dashdestino = new Vector3();
    }

// recibimos daño

    public void golpeadoCaja(Vector3 direccion) {
        golpeado = 1;
        direcciongolpeado = direccion;
        timergolpeado.Stop();

        timergolpeado.Start();
        return;
    }

    private void timerataque_tick() {
        golpeado = 0;
        direcciongolpeado = new Vector3();
        timergolpeado.Stop();

    }

    public void disparaArma() {
        try {
            Particles efecto = (Particles)GetNode("Cabeza/Camera/revolver/efectofuego");
            
            efecto.SetEmitting(true);
        }
        catch(Exception ex) {

        }
    }

}
