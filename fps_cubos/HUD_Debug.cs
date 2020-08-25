using Godot;
using System;

public class HUD_Debug : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public void cambiaFPS(float valor) {
        try {
            Label lbl = (Label)GetNode("lblFPS");
            lbl.Text = ""+valor;
        }
        catch(Exception ex) {

        }

    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
