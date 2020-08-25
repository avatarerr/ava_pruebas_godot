using Godot;
using System;

public class globales : Node
{

    public enum TIPOIMPACTO
    {
        enemigo,
        objeto,
        proyectil,
        COUNT
    }

    public enum TIPO
    {
        cereza,
        bomba,
        bomba_explosiva,
        firstaid,
        defuser,
        COUNT
    }

    public enum TIPOENEMIGO
    {
        tanquepeque,
        mob,
        lava,
        COUNT
    }

    public enum TIPOPROYECTIL
    {
        explosion,
        fireball,
        COUNT
    }

    static public Jugador NODOJUGADOR;
    static public Principal NODOPRINCIPAL;
    
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
