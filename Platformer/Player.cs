using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Platformer;

// The player states
public enum PlayerStates{
    Idle,
    Running,
    Falling,
    Jumping,
}

public class Player{

#region Variables

    // The game
    Game1 game;

    // Graphics devices
    private GraphicsDeviceManager graphicsDeviceManager;
    private GraphicsDevice graphicsDevice;

    // Time
    GameTime gameTime;
    float deltaTime;

    // Player state
    PlayerStates currentState;
    PlayerStates previousState;
    private Dictionary<PlayerStates, Action> playerStateHandlers = new Dictionary<PlayerStates, Action>();

    // Player location
    private Rectangle destinationRectangle;
    private Rectangle previousDestinationRectangle;

    // Player movement
    private int runSpeed = 200;
    private int accelerationSpeed = 20;

    // Player jumping
    private int jumpForce = 400;
    private float jumpTime;
    private float maxJumpTime = 0.5f;
    
    // Physics
    private bool hittingHead = false;
    private bool grounded = false;
    private int gravityForce = 350;
    private float fallTime = 0f;
    
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
    public Player(Game1 game_, Vector2 startingPos){

        // Assign the game variable
        game = game_;
        graphicsDeviceManager = Game1._graphics;

       // Create the destination rectangle
        destinationRectangle = new Rectangle((int)startingPos.X, (int)startingPos.Y, textureSize, textureSize);
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

        previousDestinationRectangle = destinationRectangle;
    }

    // Draw content
    public void Draw(SpriteBatch spriteBatch){

        // Draw the current animation
        Animate(spriteBatch, destinationRectangle);
    }

#endregion

#region State Management

    private void LoadPlayerStates(){

        // Add the states to the dictionary
        playerStateHandlers.Add(PlayerStates.Idle, Idle);
        playerStateHandlers.Add(PlayerStates.Running, Run);
        playerStateHandlers.Add(PlayerStates.Falling, Fall);
        playerStateHandlers.Add(PlayerStates.Jumping, Jump);
    }

    private void StateHandler(){

        // Set the previous state
        previousState = currentState;

        // Update the current state
        currentState = GetCurrentState();
        playerStateHandlers[currentState]();

        // Debug line to print the new state
        if(previousState != currentState){
            Console.WriteLine(currentState);
        }
    }

    private PlayerStates GetCurrentState(){

        // Check if the player is trying to jump
        if(CheckForJump()) return PlayerStates.Jumping;

        // Check if the player is falling
        if(!grounded) return PlayerStates.Falling;

        // Check if the player is trying to move
        if(Math.Abs(GetMovementInput().X) > 0.1f) return PlayerStates.Running;

        // Return the idle state
        return PlayerStates.Idle;
    }

#endregion

#region Player States

    private void Idle(){

        destinationRectangle.Y ++;
    }

    private void Run(){

        // Get the player's desired movement direction
        desiredMovementInput = GetMovementInput();

        // Move the player
        destinationRectangle.Y ++;
        destinationRectangle.X += (int)(desiredMovementInput.X * runSpeed * deltaTime);
    }

    private void Fall(){

        // Reset the fall timer if applicable
        if(previousState != currentState){
            fallTime = 0f;
        }

        // Update the fall timer
        fallTime += deltaTime;

        // Get the player's desired movement direction
        desiredMovementInput = GetMovementInput();

        // Add the fall force
        float _gravityForce = MathF.Min(gravityForce,  gravityForce * (fallTime / 0.25f));
        Vector2 movementForce = new Vector2(desiredMovementInput.X * (runSpeed / 1.3f), _gravityForce);

        // Move the player
        destinationRectangle.X += (int)(movementForce.X * deltaTime);
        destinationRectangle.Y += (int)(movementForce.Y * deltaTime);
    }

    private void Jump(){

        // Reset the jump timer
        if(previousState != PlayerStates.Jumping){
            jumpTime = 0;
        }

        // Update the jump time
        jumpTime += deltaTime;

        // Get the player's desired movement direction
        desiredMovementInput = GetMovementInput();

        // Add the fall force
        float _jumpForce = jumpForce * ((maxJumpTime - jumpTime)/maxJumpTime);
        Vector2 movementForce = new Vector2(desiredMovementInput.X * (runSpeed / 1.3f), -_jumpForce);

        // Move the player
        destinationRectangle.X += (int)(movementForce.X * deltaTime);
        destinationRectangle.Y += (int)(movementForce.Y * deltaTime);
    }

    private Vector2 GetMovementInput(){

        // Create a blank vector2
        Vector2 moveDir = Vector2.Zero;

        // Get the player input
		if(keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
			moveDir.X = 1;
		if(keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
			moveDir.X = -1;

        // Return the movement direction (lerped to make it smoother)
        return Vector2.Lerp(desiredMovementInput, moveDir, deltaTime * accelerationSpeed);
    }

    private bool CheckForJump(){

        // Make sure the player isn't hitting their head
        if(hittingHead) return false;

        // Make sure the player isn't already jumping
        if(currentState == PlayerStates.Jumping && jumpTime < maxJumpTime) return true;

        // Make sure the player is grounded
        if(!grounded) return false;

        // Check if the player is trying to jump
        if(keyboardState.IsKeyDown(Keys.Space)){
            return true;
        }

        // Return false if all else fails
        return false;
    }

#endregion

#region Collision

    private void Collision(){

        // Set the grounded variable to false
        grounded = false;
        hittingHead = false;

        // Ensure the player doesn't go off-screen
        ScreenBoundsCollision();

        // Ensure the player doesn't collide with the enviornment
        GetEnviornmentCollision();
    }

    private void ScreenBoundsCollision(){

        // Get the player position
        int x = (int)destinationRectangle.X;
        int y = (int)destinationRectangle.Y;

        // Get the viewport bounds
        Rectangle viewportBounds = graphicsDevice.Viewport.Bounds;

        // Check the zeroes
		if(y < 0)
			destinationRectangle.Y = 0;

		if(x < 0)
			destinationRectangle.X = 0;

        // Check the outer edges
        if(y + textureSize > viewportBounds.Bottom)
			destinationRectangle.Y = viewportBounds.Bottom - textureSize;

		if(x + textureSize > viewportBounds.Right)
			destinationRectangle.X = viewportBounds.Right - textureSize;
    }

    private void GetEnviornmentCollision(){

        // Get the list of collision rects from the game class
        List<Rectangle> gameCollision = game.GetEnviornmentCollision();

        // Loop through the provided list
        for(int i = 0; i < gameCollision.Count; i++){

            // Handle the collision if applicable
            if(destinationRectangle.Intersects(gameCollision[i])) HandleEnviornmentCollision(gameCollision[i]);
        }
    }

    private void HandleEnviornmentCollision(Rectangle collisionObject){

        #region Bottom

        // Make sure this is the first frame the colliders are overlapping on the y-axis
        bool wasNotIntersectingBottom = previousDestinationRectangle.Bottom <= collisionObject.Top;

        // Calculate the overlap
        int bottom = destinationRectangle.Bottom - collisionObject.Top;

        // Move the player back
        if(bottom >= 0 && wasNotIntersectingBottom){
            destinationRectangle.Y -= bottom;
            grounded = true;
            return;
        }
        #endregion

        #region Left

        // Make sure this is the first frame the colliders are overlapping on the x-axis
        bool wasNotIntersectingLeft = previousDestinationRectangle.Left >= collisionObject.Right;

        // Calculate the overlap
        int left = destinationRectangle.Left - collisionObject.Right;

        // Move the player back
        if(left <= 0 && wasNotIntersectingLeft){
            destinationRectangle.X -= left;
            return;
        }
        #endregion

        #region Right

        // Make sure this is the first frame the colliders are overlapping on the x-axis
        bool wasNotIntersectingRight = previousDestinationRectangle.Right <= collisionObject.Left;

        // Calculate the overlap
        int right = destinationRectangle.Right - collisionObject.Left;

        // Move the player back
        if(right >= 0 && wasNotIntersectingRight){
            destinationRectangle.X -= right;
            return;
        }
        #endregion
    
        #region Top

        // Make sure this is the first frame the colliders are overlapping on the y-axis
        bool wasNotIntersectingTop = previousDestinationRectangle.Top >= collisionObject.Bottom;

        // Calculate the overlap
        int top = destinationRectangle.Top - collisionObject.Bottom;

        // Move the player back
        if(top <= 0 && wasNotIntersectingTop){
            destinationRectangle.Y -= top;
            hittingHead = true;
            return;
        }

        #endregion
    }

#endregion

#region Animation

    private void LoadAnimations(){

        // Create the animations
        Animation idle = new Animation("Idle", playerTexture, new Vector2(16, 16), 4);
        Animation run = new Animation("Run", playerTexture, new Vector2(16, 32), 6);
        Animation fall = new Animation("Fall", playerTexture, new Vector2(16, 48), 1);

        // Add the animations to their respective spot in the Enum dictionary
        stateAnimations.Add(PlayerStates.Idle, idle);
        stateAnimations.Add(PlayerStates.Running, run);
        stateAnimations.Add(PlayerStates.Falling, fall);
        stateAnimations.Add(PlayerStates.Jumping, fall);
    }

    private void Animation(){

        // Check if the animation should be flipped
        FlipAnimation();

        // Find the current animation
        currentAnimation = GetCurrentAnimation();

        // Update the current animation
        currentAnimation.Update(gameTime);
    }

    private void Animate(SpriteBatch spriteBatch, Rectangle destinationRect){

        // Create the source rectangle
        Rectangle sourceRect = currentAnimation.GetSourceRectangle();

        // Draw the player
        spriteBatch.Draw(currentAnimation.GetTexture(), destinationRect, sourceRect, Color.White, 0f, Vector2.Zero, animationEffect, 1f);
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