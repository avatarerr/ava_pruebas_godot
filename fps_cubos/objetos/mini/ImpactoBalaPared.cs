using Godot;
using System;

public class ImpactoBalaPared : Spatial
{
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

    public void haceEfecto(Vector3 posicion) {
        Particles particula = (Particles)GetNode("Particles");
        particula.Translation = posicion;
        particula.Emitting = true;
        Timer timautodestruye = (Timer)GetNode("Autodestruye");
        timautodestruye.Start();
    }
    private void autodestruye() {
        QueueFree();
    }
}
