using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer;

// The player states
public enum PlayerStates{
    Idle,
    Running
}

public class Player{

#region Variables

    // Graphics devices
    private GraphicsDeviceManager graphicsDeviceManager;
    private GraphicsDevice graphicsDevice;

    // Time
    GameTime gameTime;
    float deltaTime;

    // Player state
    PlayerStates currentState;
    private Dictionary<PlayerStates, Action> playerStateHandlers = new Dictionary<PlayerStates, Action>();

    // Player movement
    private Vector2 position;
    private int speed = 200;
    private int accelerationSpeed = 20;
    
    // Player input
    private Vector2 desiredMovementInput;
    private KeyboardState keyboardState;

    // Player graphics
    private Texture2D playerTexture;
    private int textureSize = 32;

    // Player animation
    private Animation currentAnimation;
    private SpriteEffects animationEffect;
    private Dictionary<PlayerStates, Animation> stateAnimations = new Dictionary<PlayerStates, Animation>();

#endregion

#region Class Management

    // Class creation
    public Player(Vector2 startingPos){

        // Assign the game variable
        graphicsDeviceManager = Game1._graphics;

        // Set the starting position
        position = startingPos;
    }

#endregion

#region Game Logic

    // Load content
    public void LoadContent(ContentManager content){

        // Get the graphics device
        graphicsDevice = graphicsDeviceManager.GraphicsDevice;

        // Load the graphics
		playerTexture = content.Load<Texture2D>("PlayerSheet");

        // Load the player states
        LoadPlayerStates();

        // Load the animations
        LoadAnimations();
    }

    // Update logic
    public void Update(GameTime gameTime_){

        // The game time
        gameTime = gameTime_;
        deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Keyboard state
        keyboardState = Keyboard.GetState();

        // Manage player states
        StateHandler();

        // Manage player collisions
        Collision();

        // Manage player animations
        Animation();
    }

    // Draw content
    public void Draw(SpriteBatch spriteBatch){

        // Create the destination rectangle
        Rectangle destinationRect = new Rectangle((int)position.X, (int)position.Y, textureSize, textureSize);
        Rectangle sourceRect = currentAnimation.GetSourceRectangle();

        // Draw the player
        spriteBatch.Draw(currentAnimation.GetTexture(), destinationRect, sourceRect, Color.White, 0f, Vector2.Zero, animationEffect, 1f);
    }

#endregion

#region State Management

    private void LoadPlayerStates(){

        // Add the states to the dictionary
        playerStateHandlers.Add(PlayerStates.Idle, Idle);
        playerStateHandlers.Add(PlayerStates.Running, Run);
    }

    private void StateHandler(){

        // Update the current state
        currentState = GetCurrentState();
        playerStateHandlers[currentState]();
    }

    private PlayerStates GetCurrentState(){

        // Check if the player is trying to move
        if(Math.Abs(GetMovementInput().X) > 0.1f) return PlayerStates.Running;

        // Return the idle state
        return PlayerStates.Idle;
    }

    private void Idle(){

        // Set the movement to zero
        desiredMovementInput = Vector2.Zero;
    }

    private void Run(){

        // Get the player's desired movement direction
        desiredMovementInput = GetMovementInput();

        // Move the player
        position += desiredMovementInput * speed * deltaTime;
    }

    private Vector2 GetMovementInput(){

        Vector2 moveDir = new Vector2(0, 0);

        // Get the player input
		if(keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
			moveDir.X = 1;
		if(keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
			moveDir.X = -1;

        // Return the movement direction (lerped to make it smoother)
        return Vector2.Lerp(desiredMovementInput, moveDir, deltaTime * accelerationSpeed);
    }

#endregion

#region Collision

    private void Collision(){

        // Ensure the player doesn't go off-screen
        ScreenBoundsCollision();
    }

    private void ScreenBoundsCollision(){

        // Get the player position
        int x = (int)position.X;
        int y = (int)position.Y;

        // Get the viewport bounds
        Rectangle viewportBounds = graphicsDevice.Viewport.Bounds;

        // Check the zeroes
		if(y < 0)
			position.Y = 0;

		if(x < 0)
			position.X = 0;

        // Check the outer edges
        if(y + textureSize > viewportBounds.Bottom)
			position.Y = viewportBounds.Bottom - textureSize;

		if(x + textureSize > viewportBounds.Right)
			position.X = viewportBounds.Right - textureSize;
    }

#endregion

#region Animation

    private void LoadAnimations(){

        // Load the animations
        Animation idle = new Animation("Idle", playerTexture, new Vector2(16, 16), 4);
        Animation run = new Animation("Run", playerTexture, new Vector2(16, 32), 6);

        // Add the animations to their respective spot in the Enum dictionary
        stateAnimations.Add(PlayerStates.Idle, idle);
        stateAnimations.Add(PlayerStates.Running, run);
    }

    private void Animation(){

        // Check if the animation should be flipped
        FlipAnimation();

        // Find the current animation
        currentAnimation = GetCurrentAnimation();

        // Update the current animation
        currentAnimation.Update(gameTime);
    }

    private Animation GetCurrentAnimation(){

        // Find the animation related to the current state
        Animation newAnim = stateAnimations[currentState];

        // Check if the animation matches the current animation and return it if so
        if(newAnim == currentAnimation) return currentAnimation;

        // Otherwise, stop the current animation and play the new one
        if(currentAnimation != null) currentAnimation.Reset();
        return newAnim;
    }

    private void FlipAnimation(){

        // Check if the animation should be flipped
        if(desiredMovementInput.X < 0f){
            animationEffect = SpriteEffects.FlipHorizontally;
            return;
        }

        // Check if the animation should not be flipped
        if(desiredMovementInput.X > 0f){
            animationEffect = SpriteEffects.None;
            return;
        }
    }

#endregion

}