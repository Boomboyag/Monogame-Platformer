using System;
using System.Diagnostics.Contracts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer;

public class Animation{

    // Animation name
    public String Name;

    // Animation texture
    private Texture2D animationTexture;

    // Offset and frame size
    private Vector2 animationFrameOffset;
    private Vector2 frameSize = new Vector2(16, 16);

    // Current and total frames
	private int currentFrame = 0;
	private int totalFrames;

    // Animation speed
	private float animationSpeed = 0.1f;
	private float animationTimer = 0f; 

    public Animation(String name_, Texture2D animationTexture_, Vector2 offset, int totalFrames_){

        // Set the name
        Name = name_;

        // Set the texture
        animationTexture = animationTexture_;

        // Set the offset and total frames
        animationFrameOffset = offset;
        totalFrames = totalFrames_;
    }

    // Update animation values
    public void Update(GameTime gameTime){

		// Increase the animation timer
		animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

		// Check if enough time has passed to progress the animation
		if(animationTimer >= animationSpeed){

			currentFrame++;
			if(currentFrame >= totalFrames) currentFrame = 0;
			animationTimer = 0f;
		}
	}

    // Reset animation values
    public void Reset(){
        currentFrame = 0;
        animationTimer = 0f;
    }

    // Create the source rectangle
    public Rectangle GetSourceRectangle(){

        // Get the size of the frame
        int frameWidth = (int)frameSize.X;
        int frameHeight = (int)frameSize.Y;

        // Return the rect
        return new Rectangle(currentFrame * frameWidth + (int)animationFrameOffset.X, (int)animationFrameOffset.Y, frameWidth, frameHeight);
    }

    // Get the animation texture
    public Texture2D GetTexture(){
        return animationTexture;
    }

    // Check if one animation equals another
    public bool Equals(Animation otherAnim){

        return (otherAnim.Name == this.Name && otherAnim.GetTexture() == this.animationTexture);
    }
}