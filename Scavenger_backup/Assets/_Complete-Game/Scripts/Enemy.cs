﻿using UnityEngine;
using System.Collections;

namespace Completed
{
	//Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
	public abstract class Enemy : MovingObject
	{
		public int playerDamage; 							//The amount of food points to subtract from the player when attacking.
		public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.
		public AudioClip attackSound2;						//Second of two audio clips to play when attacking the player.
		
		
		private Animator animator;							//Variable of type Animator to store a reference to the enemy's Animator component.
		private bool skipMove = false;								//Boolean to determine whether or not enemy should skip a turn or move this turn.
        private Color visitedColor = new Color(0.15f, 0.15f, 0.15f);
        private Color invisibility = new Color(0.15f, 0.15f, 0.15f, 0f);
        protected Transform target;                         //Transform to attempt to move toward each turn.
        protected BoardManager board;
        protected SpriteRenderer spriteRenderer;

        //Start overrides the virtual Start function of the base class.
        protected override void Start ()
		{
			//Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
			//This allows the GameManager to issue movement commands.
			GameManager.instance.AddEnemyToList (this);
            board = GameManager.instance.GetBoard();
			animator = GetComponent<Animator> ();
            spriteRenderer = GetComponent<SpriteRenderer>();

            base.Start();
		}

        protected void Update()
        {
            spriteRenderer.color = board.GetTile((int)transform.position.x, (int)transform.position.y).GetColor();
            if (spriteRenderer.color == visitedColor)
                spriteRenderer.color = invisibility;
        }

        //Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
        //See comments in MovingObject for more on how base AttemptMove function works.
        protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//Check if skipMove is true, if so set it to false and skip this turn.
			if(skipMove)
			{
				skipMove = false;
				return;			
			}
			
			//Call the AttemptMove function from MovingObject.
			base.AttemptMove <T> (xDir, yDir);

            if (GameManager.instance.slowedEnemies)
                skipMove = true;
		}


        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
        public abstract void MoveEnemy();

		
		//OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
		//and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
		protected override void OnCantMove <T> (T component)
		{
			//Declare hitPlayer and set it to equal the encountered component.
			Player hitPlayer = component as Player;
			
			//Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
			hitPlayer.LoseFood (playerDamage);
			
			//Set the attack trigger of animator to trigger Enemy attack animation.
			animator.SetTrigger ("enemyAttack");
			
			//Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
		}

        protected override void OnTakeOverPosition<T>(T component)
        {
            Player player = component as Player;
            GameManager.instance.SetEnemyToRemove(this);
            player.LoseFood(playerDamage);
            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);           
        }
    }
}
