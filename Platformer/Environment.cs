using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer;

public class Environment{

#region Variables

    // Collision
    private List<List<int>> colliderMap;
    private List<Rectangle> colliders = new List<Rectangle>();
    private Vector2 colliderSize;
    private bool showCollider = true;
    private Texture2D colliderTexture;

#endregion

#region Class Management

    // Class constructors
    public Environment(List<List<int>> colliderMap_){

        // Set the collision variables
        colliderMap = colliderMap_;
    }

    public Environment(List<List<int>> colliderMap_, bool showColliders_){

        // Set the collision variables
        colliderMap = colliderMap_;
        showCollider = showColliders_;
    }

#endregion

#region Game Logic

    public void Initialize(){

        // Create the collision rectangles
        CreateColliders();
    }

    public void LoadContent(ContentManager content){

        // Load the collider texture
		colliderTexture = content.Load<Texture2D>("Platform");
    }

    public void Draw(SpriteBatch spriteBatch){

        // Draw the colliders if possible
        if(showCollider) DrawColliders(spriteBatch);
    }

#endregion

#region Collision

    private void CreateColliders(){

        // Get the collider size
        colliderSize = new Vector2(512 / colliderMap[0].Count, 512 / colliderMap.Count);
        int yOffset = 0;

        for(int i = 0; i < colliderMap.Count; i++){

            // Reset the x-offset
            int xOffset = 1;

            for(int ii = 0; ii < colliderMap[i].Count; ii++){

                // Create the collider rectangle
                Rectangle collider = new Rectangle(xOffset, yOffset, (int)colliderSize.X, (int)colliderSize.Y);

                // Update the offset
                xOffset += (int)colliderSize.X;

                // Make sure a collider should be made
                if(colliderMap[i][ii] == 0) continue;

                // Add the collider to the list
                colliders.Add(collider);
            }

            // Update the y-offset
            yOffset += (int)colliderSize.Y;
        }
    }

    public List<Rectangle> GetColliders(){

        // Return the list of the colliders
        return colliders;
    }

    private void DrawColliders(SpriteBatch spriteBatch){
        
        // Loop through the collider list
        for(int i = 0; i < colliders.Count; i++){

            // Draw the collider
            spriteBatch.Draw(colliderTexture, colliders[i], Color.White);
        }
    }

#endregion

}