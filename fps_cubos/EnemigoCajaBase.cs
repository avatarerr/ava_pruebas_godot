using Godot;
using System;
using System.Collections;

public class EnemigoCajaBase : KinematicBody
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";


/*
Funcionamiento movimiento

[[en physcis_process]]
- Si esta en herido baja el tiempo que tiene que estar herido. Si es menor que 0, cancela herido
- estado atacando=1
    lanza un rayo para comprobar si el objetivo está al alcance. Si lo está, pasa a estadoatacando=2 e inicia el timer
- estadoatacando>1
    Avanza hacia delante usando 'velocidadataque'. Si choca contra algo, llama a pararAtaque, si con lo que golpea es el jugador, 
    llama a la funcion 'golpeadoCaja' del jugador

- Si está 'enherido' gira sobre si mismo
- En los demás casos: 
    Calcula la distancia al objetivo para saber cuanta 'y' debe subir o bajar
    Avanza hacia delante segun el vector 'direccion'.
    Comprueba si choca contra algo. Si choca, llama a cambiarDireccion()

[[en cambiaDireccion()]]

Cada 3 segundos llama a cambiaDireccion().

    - Si estadoatacando>=2 no gira
    - Si esta en modobusquedaobjetivo=1 (moviendose al azar) y está a menos de 10 de la distancia del jugador, 
    se puede activar el ataque. En ese caso, cambia estadoataque=1 (acelerando), el modobusqueda=2 (buscando al jugador),
    cambia la frencuencia de cambiaDireccion a 1 e   inicia el timerAtaque

    - Si no se activa el ataque
        Si está en modo busquedaobjetivo=2 (busqueda del jugador), recalcula el objetivo para ir hacia el jugador
        si estaba en busquedaobjetivo=3 (guardar distancia con el jugador) y está a la distancia adecuada, 
        pasa a busquedaobjetivo=1 (moverse al azar)

        Si está en busquedaobjetivo=1 (moverse al azar) y ya ha llegado al objetivo (distancia < 6) cambia de objetivo al azar
        Calcula el ángulo para girar hacia el objetivo
        Pone el contadorgirador=0
        Inicia el timer para llamar al girador.




*/

enum TIPOSMOVIMIENTOS {
    NINGUNO, // no se mueve
    MOVIENDOSE, // se mueve sin buscar a nadie
    BUSCANDOJUGADOR, // intenta acercarse al jugador
    ATAQUEPREPARANDO, // gira hacia el jugador y se prepara para atacar
        ATAQUEAPUNTOLANZAR, // 
    ATAQUEACELERANDO, // se lanza hacia el jugador y va acelerando

    ATAQUECONSTANTE, // velocidad de ataque constante
    ATAQUEDESACELERANDO, // fin del ataque y desacelera
    HERIDO,
    PARADACHOQUE // cuando choca en un ataque va un poco hacia atrás antes de volver a moverse

}

    TIPOSMOVIMIENTOS tipomovimiento = TIPOSMOVIMIENTOS.NINGUNO;
    Vector3 direccion = new Vector3(0,0,-1);

   // private float angulo_destino_rad = 0;
    private float contadordeltaataque = 0; // contador delta del ataque, cuando llega a 0 cambiar de estado de ataque
   // private float contador_girador = 0;
    private float VELOCIDADGIRO = (float)0.02;
    private float VELOCIDAD = (float)6;
  //  private float VELOCIDADATAQUE = (float)12;
    //private float VELOCIDADHERIDO = 8;

    private float DISTANCIAMAXIMAJUGADOR = (float)50;
    private float DISTANCIAMINIMAJUGADOR = (float)10;

    private float ALTURAMINIMAVUELO = (float)10;
    private float ALTURAMAXIMAMAVUELO = (float)40;

    private int vida = 5;
    private bool enherido = false;
    private float tiempoherido = 0;

    private Vector3 direccionherido = new Vector3();
    private Vector3 posicionobjetivoactual = new Vector3(); // hacia donde se dirije el cubo

    private float objectivoAltura = 40; // altura a la que debe volar la caja respecto al jugador

    private int estadoatacando=0; // 0->no ataca 1->preparado para atacar 2-> inicia ataque 2->coje carrerilla 3->atacando 4->acaba ataque

    //private Timer timergirador;
    private Timer timerusogeneral = new Timer(); // timer usado para varias cosas distintas
   // private float TIEMPOPREPARAATAQUE = 4;


    //private Timer timerdireccion;
    private float velocidadatacando = (float)0;
   // private Vector3 posicionObjetivoAtacando = new Vector3();
    private int contadorciclosataque = 0;

    private Texture textura1;
    private Texture textura2;

    private Texture texturadanyo1;
    private Texture texturadanyo2;

    //private bool prueba = false;

    private int tipodisenyo = 1; // 1->negro 2-> blanco

  //  private int estadobusquedaobjetivo = 1; // 1->busqueda al azar 2->sigue al jugador, 3-> acercarse al jugador

    private Vector3 vectorgirapreparaataque = new Vector3();
// partes
    MeshInstance Cara;
    MeshInstance Cuerpo;
    CollisionShape Colision;
    

    RandomNumberGenerator objrand;

    // datos de precarga
    private Vector3 precargaPosicionInicial;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        inicia();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public void precarga(Vector3 posicioninicial) {
        precarga(posicioninicial,0);
    }

    public void precarga(Vector3 posicioninicial,int tipodisenyo) {
        try {
        this.precargaPosicionInicial = posicioninicial;
        this.tipodisenyo = tipodisenyo;
        }
        catch(Exception ex) {

        }
    }
    public void inicia()
    {
        try {


            objrand = new RandomNumberGenerator();
            objrand.Randomize();

            int tipodisenyo = 0;
            textura1 = (Texture)GD.Load("res://art/texturas/caraenfadado.png");
            textura2 = (Texture)GD.Load("res://art/texturas/caraenfadado_alt.png");
            texturadanyo1 = (Texture)GD.Load("res://art/texturas/danyo/danyo_caja1.png");

            Cara = (MeshInstance)GetNode("Cara");
            Cuerpo = (MeshInstance)GetNode("Cuerpo");
            Colision = (CollisionShape)GetNode("CollisionShape");

            this.Translation = precargaPosicionInicial;

            if (tipodisenyo==0) {
                RandomNumberGenerator objrand = new RandomNumberGenerator();
                objrand.Randomize();
                tipodisenyo = objrand.RandiRange(1,2);
            }

            Color colorcuerpo = Color.Color8(0,0,0,0);

            if (tipodisenyo==2) colorcuerpo = Color.Color8(255,255,255,0);
            
            SpatialMaterial matcuerpo = (SpatialMaterial)Cuerpo.GetSurfaceMaterial(0);
            matcuerpo.AlbedoColor = colorcuerpo;
            Cuerpo.MaterialOverride = matcuerpo;

            cambiaTipoMovimiento(TIPOSMOVIMIENTOS.MOVIENDOSE,0);
            //cambiaTipoMovimiento(TIPOSMOVIMIENTOS.NINGUNO,0);


          //  timergirador = (Timer)GetNode("TimerGirador");     
    
            //inicia_timer_direccion();

            cambiaObjetivoAzar();
            rehaceObjetivoAltura();

            this.AddChild(timerusogeneral);

           // timerusogeneral.WaitTime = TIEMPOPREPARAATAQUE;
            timerusogeneral.Connect("timeout",this, nameof(ciclo_timer_general));


            activaFuegoYHumo(false);

           // timerdireccion = (Timer)GetNode("TimerCambioDireccion");

       //     quitaObjetivoJugador();
        }
        catch(Exception ex) {

        }
        
    }


    private void rehaceObjetivoAltura() {
            RandomNumberGenerator objrand = new RandomNumberGenerator();
            objrand.Randomize();
            float valor = objrand.RandfRange(1, 2);

            objectivoAltura = valor;
    }


    public override void _PhysicsProcess(float delta) {
        try {

            // ****** No se mueve
            if (tipomovimiento == TIPOSMOVIMIENTOS.NINGUNO)  {
                /*
                if (direccion != new Vector3())
                {


                    // ascender/descender

                    
                    Vector3 direcciontrans = new Vector3(direccion.x, 0,direccion.z);

                    Transform trans = this.GlobalTransform;
                    Vector3 directionlocal = trans.basis.Xform(direcciontrans);
                    //   Vector3 directionlocal = direccion;
                    KinematicCollision choque = MoveAndCollide(directionlocal * VELOCIDAD * delta);


                    // actuamos si chocamos
                }
            */
                return;
            }
                

            // se mueve al azar por el escenario
            if (tipomovimiento == TIPOSMOVIMIENTOS.MOVIENDOSE) {
                
                // el movimiento normal es siempre hacia el frente, si está mirando hacia arriba o abajo
                // hacemos que mire al frente

                Vector3 giroactual = Rotation;
                if (giroactual.x!=0) {
                     
                     if (Math.Abs(giroactual.x)<VELOCIDADGIRO) giroactual.x = 0;
                     else if (giroactual.x>VELOCIDADGIRO) giroactual.x -= VELOCIDADGIRO;
                     else giroactual.x += VELOCIDADGIRO;


                    this.Rotation = giroactual;
                }

                // comprobamos si hay que rotar


                Vector3 posicionyo = this.Translation;
                Vector2 diferencia2d = new Vector2(posicionobjetivoactual.x, posicionobjetivoactual.z) - new Vector2(posicionyo.x, posicionyo.z);
                Vector3 zfact = -this.GlobalTransform.basis.z;
                Vector2 zfact2d = new Vector2(zfact.x, zfact.z);
                Vector2 diferencia2dnorm = diferencia2d.Normalized();
                Vector2 zfact2dnorm = zfact2d.Normalized();
                float anguloangle = zfact2dnorm.AngleTo(diferencia2dnorm);

                if (anguloangle>(float)VELOCIDADGIRO)  this.RotateY(-VELOCIDADGIRO);
                else if (anguloangle<(float)VELOCIDADGIRO) this.RotateY(VELOCIDADGIRO);

                // avanzamos

                Vector3 posicionactual = Translation;

                if (direccion != new Vector3())
                {
                    Vector3 vectordistancia = posicionobjetivoactual - posicionyo;
                    float distanciafloat = vectordistancia.Length();

                    float lay = 0;

                    // ascender/descender
                    
                    if (vectordistancia.y>0) lay= (float)0.02;
                    if (vectordistancia.y<0) lay= (float)-0.02; 
                    
                    Vector3 direcciontrans = new Vector3(direccion.x, lay,direccion.z);

                    Transform trans = this.GlobalTransform;
                    Vector3 directionlocal = trans.basis.Xform(direcciontrans);
                    //   Vector3 directionlocal = direccion;
                    KinematicCollision choque = MoveAndCollide(directionlocal * VELOCIDAD * delta);


                    // actuamos si chocamos

                    if (choque!=null || distanciafloat<10)
                        cambiaObjetivoAzar();

                    
                    if (decideAtacarJugador()) 
                        cambiaTipoMovimiento(TIPOSMOVIMIENTOS.ATAQUEPREPARANDO,0);
                }


             
            }

            // se desplaza como siempre, pero buscando como objetivo al jugador
            if (tipomovimiento == TIPOSMOVIMIENTOS.BUSCANDOJUGADOR) {

                // funciona igual que el movimiento normal, pero el objetivo es siempre el jugador

                posicionobjetivoactual = globales.NODOJUGADOR.Translation;

                // comprobamos si hay que rotar


            
                Vector3 posicionyo = this.Translation;
                Vector2 diferencia2d = new Vector2(posicionobjetivoactual.x, posicionobjetivoactual.z) - new Vector2(posicionyo.x, posicionyo.z);
                Vector3 zfact = -this.GlobalTransform.basis.z;
                Vector2 zfact2d = new Vector2(zfact.x, zfact.z);
                Vector2 diferencia2dnorm = diferencia2d.Normalized();
                Vector2 zfact2dnorm = zfact2d.Normalized();
                float anguloangle = zfact2dnorm.AngleTo(diferencia2dnorm);

                if (anguloangle>(float)VELOCIDADGIRO)  this.RotateY(-VELOCIDADGIRO);
                else if (anguloangle<(float)VELOCIDADGIRO) this.RotateY(VELOCIDADGIRO);

                // avanzamos

                Vector3 posicionactual = Translation;

                if (direccion != new Vector3())
                {
                   Vector3 vectordistancia = posicionobjetivoactual - posicionyo;
                    float distanciafloat = vectordistancia.Length();

                    float lay = 0;

                    // ascender/descender
                    
                    if (vectordistancia.y>0) lay= (float)0.02;
                    if (vectordistancia.y<0) lay= (float)-0.02; 
                    
                    Vector3 direcciontrans = new Vector3(direccion.x, lay,direccion.z);

                    Transform trans = this.GlobalTransform;
                    Vector3 directionlocal = trans.basis.Xform(direcciontrans);
                    //   Vector3 directionlocal = direccion;
                    KinematicCollision choque = MoveAndCollide(directionlocal * VELOCIDAD * delta);


                    // actuamos si chocamos

                    if (choque!=null)
                        cambiaObjetivoAzar();

                    if (decideAtacarJugador()) 
                        cambiaTipoMovimiento(TIPOSMOVIMIENTOS.ATAQUEPREPARANDO,0);  
                }
            }

            // deja de moverse y gira para encarar al jugador antes de atacar
            if (tipomovimiento == TIPOSMOVIMIENTOS.ATAQUEPREPARANDO) {
                Vector3 giroactual = Rotation;
                Vector3 girodestino = vectorgirapreparaataque;
                
                if (giroactual.y<girodestino.y && (girodestino.y-giroactual.y)>(VELOCIDADGIRO*1.5)) {
              //      RotateY(CANTIDADGIROAT);
                    giroactual.y += VELOCIDADGIRO*(float)1.5;
                }
                if (giroactual.y>girodestino.y && (giroactual.y-girodestino.y)>(VELOCIDADGIRO*1.5)) {
                   // RotateY(-CANTIDADGIROAT);
                   giroactual.y -= VELOCIDADGIRO*(float)1.5;
                }

                if (giroactual.x<girodestino.x && (girodestino.x-giroactual.x)>(VELOCIDADGIRO*1.5)) {
              //      RotateY(CANTIDADGIROAT);
                    giroactual.x += VELOCIDADGIRO*(float)1.5;
                }
                if (giroactual.x>girodestino.x && (giroactual.x-girodestino.x)>(VELOCIDADGIRO*1.5)) {
                   // RotateY(-CANTIDADGIROAT);
                   giroactual.x -= VELOCIDADGIRO*(float)1.5;
                }                              

                this.Rotation = giroactual;


                // si alcanza el giro destino, se lanza al ataque despues de 1 segundo

                if ((girodestino.y-giroactual.y)<(VELOCIDADGIRO*2) && (giroactual.x-girodestino.x)<(VELOCIDADGIRO*1.5)) {
                    cambiaTipoMovimiento(TIPOSMOVIMIENTOS.ATAQUEAPUNTOLANZAR,0);
                }

            }

  // deja de moverse y gira para encarar al jugador antes de atacar
            if (tipomovimiento == TIPOSMOVIMIENTOS.ATAQUEAPUNTOLANZAR) {
                contadordeltaataque -= delta;

                this.Rotation = vectorgirapreparaataque;
                this.RotateZ(objrand.RandfRange(-(float)0.2,(float)0.2));

                if (contadordeltaataque<=0) lanzaAtaque();

            }

            // avanza en linea recta acelerando hasta alcanzar una velocidad maxima
            if (tipomovimiento == TIPOSMOVIMIENTOS.ATAQUEACELERANDO) {

             
                // avanzamos

                Vector3 posicionactual = Translation;

                if (direccion != new Vector3())
                {
                    Vector3 direcciontrans = new Vector3(direccion.x, 0,direccion.z);

                    Transform trans = this.GlobalTransform;
                    Vector3 directionlocal = trans.basis.Xform(direcciontrans);
                    //   Vector3 directionlocal = direccion;
                    contadordeltaataque -= delta;
                    
                    KinematicCollision choque = MoveAndCollide(directionlocal * velocidadatacando * delta);

                    if (choque!=null)
                        choca(choque,directionlocal);
                    else
                        if (contadordeltaataque<=0) cambiaTipoMovimiento(TIPOSMOVIMIENTOS.ATAQUECONSTANTE,0);


                    // actuamos si chocamos



                        
                }
            }

            if (tipomovimiento == TIPOSMOVIMIENTOS.ATAQUECONSTANTE) {

             
                // avanzamos

                Vector3 posicionactual = Translation;

                if (direccion != new Vector3())
                {
                    Vector3 direcciontrans = new Vector3(direccion.x, 0,direccion.z);

                    Transform trans = this.GlobalTransform;
                    Vector3 directionlocal = trans.basis.Xform(direcciontrans);
                    //   Vector3 directionlocal = direccion;
                    contadordeltaataque -= delta;
                    
                    KinematicCollision choque = MoveAndCollide(directionlocal * velocidadatacando * delta);

                    if (choque!=null)
                        choca(choque,directionlocal);
                    else
                        if (contadordeltaataque<=0) cambiaTipoMovimiento(TIPOSMOVIMIENTOS.ATAQUEDESACELERANDO,0);


                    // actuamos si chocamos



                        
                }
            }

            if (tipomovimiento == TIPOSMOVIMIENTOS.ATAQUEDESACELERANDO) {

             
                // avanzamos

                Vector3 posicionactual = Translation;

                if (direccion != new Vector3())
                {
                    Vector3 direcciontrans = new Vector3(direccion.x, 0,direccion.z);

                    Transform trans = this.GlobalTransform;
                    Vector3 directionlocal = trans.basis.Xform(direcciontrans);
                    //   Vector3 directionlocal = direccion;
                    contadordeltaataque -= delta;
                    
                    KinematicCollision choque = MoveAndCollide(directionlocal * velocidadatacando * delta);

                    if (choque!=null)
                       choca(choque,directionlocal);
                    else
                        if (contadordeltaataque<=0) cambiaTipoMovimiento(TIPOSMOVIMIENTOS.MOVIENDOSE,0);


                    // actuamos si chocamos



                        
                }
            }

            // se mueve un poco hacia atras al chocar despues de un ataque

           if (tipomovimiento == TIPOSMOVIMIENTOS.PARADACHOQUE) {

             
                // avanzamos

                Vector3 posicionactual = Translation;

                if (direccion != new Vector3())
                {
                    // hacemos que se vaya poniendo derecho

                     Vector3 giroactual = Rotation;
                    if (giroactual.x!=0) {
                        
                        if (Math.Abs(giroactual.x)<VELOCIDADGIRO) giroactual.x = 0;
                        else if (giroactual.x>VELOCIDADGIRO) giroactual.x -= VELOCIDADGIRO;
                        else giroactual.x += VELOCIDADGIRO;


                        this.Rotation = giroactual;
                    }


                    Vector3 direcciontrans = new Vector3(direccion.x, 0,-direccion.z);

                    Transform trans = this.GlobalTransform;
                    Vector3 directionlocal = trans.basis.Xform(direcciontrans);
                    //   Vector3 directionlocal = direccion;
                    contadordeltaataque -= delta;
                    
                    KinematicCollision choque = MoveAndCollide(directionlocal * VELOCIDAD*2 * delta);


                    if (contadordeltaataque<=0) cambiaTipoMovimiento(TIPOSMOVIMIENTOS.MOVIENDOSE,0);


                    // actuamos si chocamos



                        
                }
            } 

        }
        catch(Exception ex) {

        }
    }


    private void cambiaTipoMovimiento(TIPOSMOVIMIENTOS tipo, float flag) {
        try {
            switch(tipo) {
                case(TIPOSMOVIMIENTOS.NINGUNO):
                    tipomovimiento = TIPOSMOVIMIENTOS.NINGUNO;
                    break;
                case(TIPOSMOVIMIENTOS.MOVIENDOSE):
                    activaFuegoYHumo(false);
                    tipomovimiento = TIPOSMOVIMIENTOS.MOVIENDOSE;
                    break;
                case(TIPOSMOVIMIENTOS.BUSCANDOJUGADOR):
                    tipomovimiento = TIPOSMOVIMIENTOS.BUSCANDOJUGADOR;
                    break;
                case(TIPOSMOVIMIENTOS.HERIDO):
                    tipomovimiento = TIPOSMOVIMIENTOS.HERIDO;
                    break;
                 case(TIPOSMOVIMIENTOS.ATAQUEPREPARANDO):
                    calculaPosicionPreparaAtaque();
                    activaSoloHumo();
                    tipomovimiento = TIPOSMOVIMIENTOS.ATAQUEPREPARANDO;
                    break;
                 case(TIPOSMOVIMIENTOS.ATAQUEAPUNTOLANZAR):                    
                    contadordeltaataque = (float)1;
                    vectorgirapreparaataque = this.Rotation;
                    tipomovimiento = TIPOSMOVIMIENTOS.ATAQUEAPUNTOLANZAR;
                    break;                    
                case(TIPOSMOVIMIENTOS.ATAQUEACELERANDO):                     
                    activaFuegoYHumo(true);
                    contadordeltaataque = (float)1;
                    tipomovimiento = TIPOSMOVIMIENTOS.ATAQUEACELERANDO;
                    velocidadatacando = VELOCIDAD;
                    timerusogeneral.Stop();
                    timerusogeneral.WaitTime = (float)0.25; // se usará para ir aumentado la velocidad
                    timerusogeneral.Start();
                break;
                case(TIPOSMOVIMIENTOS.ATAQUECONSTANTE): 
                    contadordeltaataque = (float)2.3;
                    tipomovimiento = TIPOSMOVIMIENTOS.ATAQUECONSTANTE;
                    timerusogeneral.Stop();
                break;
                case(TIPOSMOVIMIENTOS.ATAQUEDESACELERANDO): 
                    contadordeltaataque = (float)1;
                    tipomovimiento = TIPOSMOVIMIENTOS.ATAQUEDESACELERANDO;
                    timerusogeneral.Stop();
                    timerusogeneral.WaitTime = (float)0.25; // se usará para ir disminuyendo la velocidad
                    timerusogeneral.Start();
                break;
                 case(TIPOSMOVIMIENTOS.PARADACHOQUE): 
                    contadordeltaataque = (float)0.5;
                    tipomovimiento = TIPOSMOVIMIENTOS.PARADACHOQUE;
                    timerusogeneral.Stop();
                break;              
            }
        }
        catch(Exception ex) {

        }
    }


    private bool decideAtacarJugador() {
        try {


            int numero = objrand.RandiRange(-100,100);
            if (numero==0) {
                return true;
            }

            return false;
            
        }
        catch(Exception ex) {
            return false;
        }
    }
    // acciones a realizar cuando choca contra algo mientras atacaba
    private void choca(KinematicCollision choque, Vector3 directionlocal) {
    //    if (tipomovimiento==TIPOSMOVIMIENTOS.ATAQUEPREPARANDO || tipomovimiento==TIPOSMOVIMIENTOS.ATAQUEDESACELERANDO)
      //      timerusogeneral.Stop();

        Godot.Object golpeado =  choque.Collider;
        ulong idcollider = choque.ColliderId;
        ulong idjugador = globales.NODOJUGADOR.GetInstanceId();

        if (idjugador==idcollider) {
            globales.NODOJUGADOR.golpeadoCaja(directionlocal);
        }

        cambiaTipoMovimiento(TIPOSMOVIMIENTOS.PARADACHOQUE,0);

    }


    public void recibeBala(Vector3 direccionimpacto) {

        Vector3 posicionjugador = globales.NODOJUGADOR.Translation;
        enherido = true;
        tiempoherido = (float)1;
        Transform trans = this.GlobalTransform;
        Vector3 posicionenemigo = this.Translation;

        Vector3 vectorlinea = posicionenemigo-posicionjugador;
        //direccionherido = trans.basis.Xform(new Vector3(0,0,-1));
        //direccionherido = direccionimpacto.Normalized();
        direccionherido = vectorlinea.Normalized();

        vida--;


        if (vida <=0) muere();

        rehaceObjetivoAltura();
    }

    public void muere() {
        globales.NODOPRINCIPAL.informaMuerteCaja(this);
        this.QueueFree();
    }

    public void ataca() {
        try {
            if (estadoatacando==2) {
                contadorciclosataque = 0;
                estadoatacando = 3;
                velocidadatacando = VELOCIDAD;
                timerusogeneral.Stop();
                timerusogeneral.WaitTime = (float)0.5;
                timerusogeneral.Start();

              //  on_TimerAtaque_timeout();

                MeshInstance Cara = (MeshInstance)GetNode("Cara");
                SpatialMaterial mat = (SpatialMaterial)Cara.GetSurfaceMaterial(0);
               
               
                mat.AlbedoTexture =textura2;               
                

            }
        }
        catch(Exception ex) {

        }
    }

    public void lanzaAtaque() {

        this.Rotation = vectorgirapreparaataque;
        cambiaTipoMovimiento(TIPOSMOVIMIENTOS.ATAQUEACELERANDO,0);
    }

    public void ciclo_timer_general() {
        try {

            if (tipomovimiento == TIPOSMOVIMIENTOS.ATAQUEACELERANDO) {
                velocidadatacando += 6;
            }

            if (tipomovimiento == TIPOSMOVIMIENTOS.ATAQUECONSTANTE) {

            }


            if (tipomovimiento == TIPOSMOVIMIENTOS.ATAQUEDESACELERANDO) {
                velocidadatacando -= 6;
            }

        
    

        }
        catch(Exception ex) {

        }
    }

    public void estableceObjetivoJugador() {
        //  Vector3 posjugador = globales.NODOJUGADOR.Translation;
        //posjugador.y = posjugador.y+objectivoAltura;
        try {
            //estadobusquedaobjetivo = 2;
            Vector3 posjugador = globales.NODOJUGADOR.Translation;
            posjugador.y = posjugador.y+objectivoAltura;

            posicionobjetivoactual = posjugador;

            if (posicionobjetivoactual.y<2) 
                posicionobjetivoactual.y = 2;

         //   timerdireccion.WaitTime = (float)1;
        }
        catch(Exception ex) {

        }
    }


// elije un objetivo al azar (una posicion en el espacio) para moverse hacia allí
    private void cambiaObjetivoAzar() {
        try {

            Vector3 posactual = this.Translation;

            bool aceptado = false;

          Vector3 nuevapos = new Vector3();

            int contadorintentos = 0;
            while (!aceptado) {
              
                nuevapos = posactual;

                float rangox = objrand.RandfRange(-15,15);
                float rangoz = objrand.RandfRange(-15,15);
                nuevapos.x += rangox;
                nuevapos.z += rangoz;
                nuevapos.y += objrand.RandfRange(-2,2);
            
                // comprobamos que la nueva direccion esta en el rango que queremos para el jugador


                // posible mejora: Hacer esto solo con los Vector2 usando x,z
                float distanciajugadoractual = posactual.DistanceTo(globales.NODOJUGADOR.Translation);
                float distanciajugador = nuevapos.DistanceTo(globales.NODOJUGADOR.Translation);
/*
                if (distanciajugador>DISTANCIAMAXIMAJUGADOR && distanciajugador>distanciajugadoractual) { // nos estamos alejando demasiado, nos acercamos al jugador
                    nuevapos = globales.NODOJUGADOR.Translation;
                    //estadobusquedaobjetivo = 3;
                }

                if (distanciajugador<DISTANCIAMINIMAJUGADOR && distanciajugador<distanciajugadoractual) { //demasiado cerca, nos vamos hacia el lado contrario
                    nuevapos = posactual;
                    nuevapos.x += (-1)*rangox;
                    nuevapos.z += (-1)*rangoz;

                }
*/
                if (nuevapos.y<ALTURAMINIMAVUELO) nuevapos.y = ALTURAMINIMAVUELO;
                if (nuevapos.y>ALTURAMAXIMAMAVUELO) nuevapos.y = ALTURAMAXIMAMAVUELO;


                // comprobamos si la caja puede llegar al objetivo



                PhysicsDirectSpaceState estado =  GetWorld().DirectSpaceState;
                Godot.Collections.Dictionary choques =  estado.IntersectRay(nuevapos, Translation);

                contadorintentos++;


                if (choques.Count==0 || contadorintentos>3) {                    
                    aceptado = true;
                }
                else {
                    continue;
                }


            }

            posicionobjetivoactual = nuevapos;


        }
        catch(Exception ex) {

        }
        

    }


        // calcula la posicion hasta donde debe girar para encararse al jugador antes de lanzarse
        public void calculaPosicionPreparaAtaque() {
         //   Spatial cabezajugador = globales.NODOJUGADOR.devuelveNodoCabeza();
         //   Vector3 jugadortrans =  cabezajugador.Translation;
         Vector3 jugadortrans = globales.NODOJUGADOR.Translation;
            jugadortrans.y += (float)6;

            Transform antigua = this.GlobalTransform;
            this.LookAt(jugadortrans,new Vector3(0,1,0));
        //    this.RotateY((float)3.14);
            vectorgirapreparaataque = this.Rotation;
            Transform nueva = this.GlobalTransform;
            this.GlobalTransform = antigua;
            
            return;

        }



        // ****** funciones debug  ******

    public void gira_en_X(float cuanto) {
        try {
            RotateX(cuanto);
        }
        catch(Exception ex) {

        }
    }




        public float calcula_angulo_jugador() {
            try {
                Vector3 posicion_enemigo = this.Translation;
                Vector3 posicion_jugador = globales.NODOJUGADOR.Translation;

                Vector3 linea_enemigo_jugador = posicion_enemigo - posicion_jugador;
                Vector3 linea_enemigo_jugador_norm = linea_enemigo_jugador.Normalized();

                // calculamos el vector al frente del enemigo
                Vector3 linea_enemigo_frente = GlobalTransform.basis.z;
                Vector3 linea_enemigo_frente_norm = linea_enemigo_frente.Normalized();

                // sacamos la version 2D de todo

                Vector2 linea_enemigo_frente2D = new Vector2(linea_enemigo_frente_norm.z, linea_enemigo_frente_norm.y);
                Vector2 linea_enemigo_jugador2D = new Vector2(linea_enemigo_jugador_norm.z, linea_enemigo_jugador_norm.y);
                
                float angulo = linea_enemigo_frente2D.AngleTo(linea_enemigo_jugador2D);
                float angulo_alt = linea_enemigo_frente.AngleTo(linea_enemigo_jugador);

                angulo = Mathf.Rad2Deg(angulo);
                angulo_alt = Mathf.Rad2Deg(angulo_alt);

                LookAt(posicion_jugador,new Vector3(0,1,0));
              //  RotateY((float)3.14);

               // globales.NODOPRINCIPAL.poneCajaEscenario(new Vector3(posicion_enemigo.x, posicion_jugador.y+2, posicion_enemigo.z));
                return angulo;

            }
            catch(Exception ex) {
                return 0;
            }
        }

        public void poneBuscaJugador() {
            cambiaTipoMovimiento(TIPOSMOVIMIENTOS.BUSCANDOJUGADOR,0);
        }

        public void poneEnMovimiento() {
            cambiaTipoMovimiento(TIPOSMOVIMIENTOS.MOVIENDOSE,0);
        }

        public void poneAcelerando() {
            cambiaTipoMovimiento(TIPOSMOVIMIENTOS.ATAQUEACELERANDO,0);
        }

        public void poneEnAtaque() {
            cambiaTipoMovimiento(TIPOSMOVIMIENTOS.ATAQUEPREPARANDO,0);
        }

        public void activaSoloHumo() {
            try {
                Particles partfuego = (Particles)GetNode("particulasEnfadado/particulasFuego");
                partfuego.Visible = false;

                Particles parthumo = (Particles)GetNode("particulasEnfadado/particulasHumo");
                parthumo.Visible = true;

            }
            catch(Exception ex) {

            }
        }

        public void activaFuegoYHumo(bool activa) {
            try {
                Particles partfuego = (Particles)GetNode("particulasEnfadado/particulasFuego");
                partfuego.Visible = activa;

                Particles parthumo = (Particles)GetNode("particulasEnfadado/particulasHumo");
                parthumo.Visible = activa;

            }
            catch(Exception ex) {

            }
        }
}
