using CustomCompanions.Framework.Models.Companion;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCompanions.Framework.Companions
{
    public class Companion : NPC
    {
        private Farmer owner;
        private CompanionModel model;
        private IdleBehavior idleBehavior;

        private float lightPulseTimer;

        private int? soundIdleTimer = null;
        private int? soundMovingTimer = null;
        private int? soundAlwaysTimer = null;

        private SoundModel idleSound;
        private SoundModel movingSound;
        private SoundModel alwaysSound;

        private LightSource light;

        private readonly NetBool hasReachedPlayer = new NetBool();
        private readonly NetInt specialNumber = new NetInt();
        private readonly NetBool isPrismatic = new NetBool();
        private readonly NetColor color = new NetColor();
        private readonly NetVector2 motion = new NetVector2(Vector2.Zero);
        private new readonly NetRectangle nextPosition = new NetRectangle();

        public Companion(CompanionModel model, Farmer owner) : base(new AnimatedSprite(model.TileSheetPath, 0, model.FrameSizeWidth, model.FrameSizeHeight), owner.getTileLocation() * 64f + new Vector2(model.SpawnOffsetX, model.SpawnOffsetY), 2, model.Name)
        {
            base.Breather = false;
            base.speed = model.TravelSpeed;
            base.forceUpdateTimer = 9999;
            base.collidesWithOtherCharacters.Value = (model.Type.ToUpper() == "FLYING" ? false : true);
            base.farmerPassesThrough = true;
            base.HideShadow = true;

            // Verify the location the companion is spawning on isn't occupied (if collidesWithOtherCharacters == true)
            if (collidesWithOtherCharacters)
            {
                foreach (var character in owner.currentLocation.characters.Where(c => c != this))
                {
                    if (character.GetBoundingBox().Intersects(this.GetBoundingBox()))
                    {
                        base.Position = Utility.getRandomAdjacentOpenTile(owner.getTileLocation(), owner.currentLocation) * 64f;
                    }
                }
            }

            this.owner = owner;
            this.model = model;
            this.specialNumber.Value = Game1.random.Next(100);
            this.nextPosition.Value = this.GetBoundingBox();
            this.idleBehavior = new IdleBehavior(model.IdleBehavior);

            // Set up the sounds to play, if any
            idleSound = model.Sounds.FirstOrDefault(s => s.WhenToPlay.ToUpper() == "IDLE");
            if (idleSound != null && CustomCompanions.IsSoundValid(idleSound.SoundName, true))
            {
                this.soundIdleTimer = idleSound.TimeBetweenSound;
            }

            movingSound = model.Sounds.FirstOrDefault(s => s.WhenToPlay.ToUpper() == "MOVING");
            if (movingSound != null && CustomCompanions.IsSoundValid(movingSound.SoundName, true))
            {
                this.soundMovingTimer = movingSound.TimeBetweenSound;
            }

            alwaysSound = model.Sounds.FirstOrDefault(s => s.WhenToPlay.ToUpper() == "ALWAYS");
            if (alwaysSound != null && CustomCompanions.IsSoundValid(alwaysSound.SoundName, true))
            {
                this.soundAlwaysTimer = alwaysSound.TimeBetweenSound;
            }

            // Pick a random color (Color.White if none given) or use prismatic if IsPrismatic is true
            color.Value = Color.White;
            if (model.Colors.Count > 0)
            {
                int randomColorIndex = Game1.random.Next(model.Colors.Count + (model.IsPrismatic ? 1 : 0));
                if (randomColorIndex > model.Colors.Count - 1)
                {
                    // Primsatic color has been selected
                    isPrismatic.Value = true;
                }
                else
                {
                    color.Value = CustomCompanions.GetColorFromArray(model.Colors[randomColorIndex]);
                }
            }

            // Set up the light to give off, if any
            if (model.Light != null)
            {
                this.lightPulseTimer = model.Light.PulseSpeed;

                this.light = new LightSource(1, new Vector2(this.position.X + model.Light.OffsetX, this.position.Y + model.Light.OffsetY), model.Light.Radius, CustomCompanions.GetColorFromArray(model.Light.Color), this.id, LightSource.LightContext.None, 0L);
                Game1.currentLightSources.Add(this.light);
            }
        }

        public override void update(GameTime time, GameLocation location)
        {
            base.currentLocation = location;
            base.update(time, location);
            base.forceUpdateTimer = 99999;

            // Handle any movement
            this.AttemptMovement(time, location);

            // Update light location, if applicable
            this.UpdateLight(time);

            // Play any sound(s) that are required
            this.PlayRequiredSounds(time);
        }

        public override bool isMoving()
        {
            return !this.motion.Equals(Vector2.Zero);
        }


        public override bool shouldCollideWithBuildingLayer(GameLocation location)
        {
            if (IsFlying())
            {
                return false;
            }

            return base.shouldCollideWithBuildingLayer(location);
        }

        public override bool collideWith(StardewValley.Object o)
        {
            if (IsFlying())
            {
                return false;
            }

            return base.collideWith(o);
        }

        public override bool isColliding(GameLocation l, Vector2 tile)
        {
            if (IsFlying())
            {
                return false;
            }

            return base.isColliding(l, tile);
        }

        public override void draw(SpriteBatch b, float alpha = 1f)
        {
            b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(this.GetSpriteWidthForPositioning() * 4 / 2, this.GetBoundingBox().Height / 2) + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), this.Sprite.SourceRect, this.isPrismatic ? Utility.GetPrismaticColor(348 + (int)this.specialNumber, 5f) : color, this.rotation, new Vector2(this.Sprite.SpriteWidth / 2, (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, base.scale) * 4f, (base.flip || (this.Sprite.CurrentAnimation != null && this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)base.getStandingY() / 10000f)));
        }

        private void AttemptMovement(GameTime time, GameLocation location)
        {

            Farmer f = owner is null ? Utility.isThereAFarmerWithinDistance(base.getTileLocation(), 10, base.currentLocation) : owner;
            if (f != null)
            {
                var targetDistance = Vector2.Distance(base.Position, f.Position);
                if (targetDistance > 640f)
                {
                    this.hasReachedPlayer.Value = false;
                    base.position.Value = f.position;
                }
                else if ((targetDistance > 64f && !owner.isMoving()) || targetDistance > 128f)
                {
                    this.hasReachedPlayer.Value = false;

                    base.Speed = model.TravelSpeed;
                    if (targetDistance > 128f)
                    {
                        base.Speed = model.TravelSpeed + (int)(targetDistance / 64f) - 1;
                    }

                    this.SetMotion(Utility.getVelocityTowardPlayer(new Point((int)base.Position.X + this.model.SpawnOffsetX, (int)base.Position.Y + this.model.SpawnOffsetY), base.speed, f));
                }
                else
                {
                    this.hasReachedPlayer.Value = true;
                    this.motion.Value = this.idleBehavior.ApplyMotionModifications(this.motion.Value, time);
                }
            }

            // Perform the position movement
            this.nextPosition.Value = this.GetBoundingBox();
            this.nextPosition.X += (int)this.motion.X;
            if (!location.isCollidingPosition(this.nextPosition, Game1.viewport, this) || IsFlying())
            {
                base.position.X += (int)this.motion.X;
            }
            this.nextPosition.X -= (int)this.motion.X;
            this.nextPosition.Y += (int)this.motion.Y;
            if (!location.isCollidingPosition(this.nextPosition, Game1.viewport, this) || IsFlying())
            {
                base.position.Y += (int)this.motion.Y;
            }

            // Update any animations
            if (!this.motion.Equals(Vector2.Zero))
            {
                if (model.UniformAnimation != null && !CustomCompanions.CompanionHasFullMovementSet(model))
                {
                    this.Sprite.Animate(time, model.UniformAnimation.StartingFrame, model.UniformAnimation.NumberOfFrames, model.UniformAnimation.Duration);
                    this.FacingDirection = 2;
                }
                else if (CustomCompanions.CompanionHasFullMovementSet(model))
                {
                    int oldDirection = this.FacingDirection;

                    if (Math.Abs(this.motion.Y) > Math.Abs(this.motion.X) && this.motion.Y < 0f)
                    {
                        this.FacingDirection = 0;
                    }
                    else if (Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X > 0f)
                    {
                        this.FacingDirection = 1;
                    }
                    else if (Math.Abs(this.motion.Y) > Math.Abs(this.motion.X) && this.motion.Y > 0f)
                    {
                        this.FacingDirection = 2;
                    }
                    else if (Math.Abs(this.motion.X) > Math.Abs(this.motion.Y) && this.motion.X < 0f)
                    {
                        this.FacingDirection = 3;
                    }

                    switch (this.FacingDirection)
                    {
                        case 0:
                            this.Sprite.Animate(time, model.UpAnimation.StartingFrame, model.UpAnimation.NumberOfFrames, model.UpAnimation.Duration);
                            break;
                        case 1:
                            this.Sprite.Animate(time, model.RightAnimation.StartingFrame, model.RightAnimation.NumberOfFrames, model.RightAnimation.Duration);
                            break;
                        case 2:
                            this.Sprite.Animate(time, model.DownAnimation.StartingFrame, model.DownAnimation.NumberOfFrames, model.DownAnimation.Duration);
                            break;
                        case 3:
                            this.Sprite.Animate(time, model.LeftAnimation.StartingFrame, model.LeftAnimation.NumberOfFrames, model.LeftAnimation.Duration);
                            break;
                    }
                }
            }
            else
            {
                if (model.UniformAnimation != null && !CustomCompanions.CompanionHasFullMovementSet(model))
                {
                    this.Sprite.Animate(time, model.UniformAnimation.IdleAnimation.StartingFrame, model.UniformAnimation.IdleAnimation.NumberOfFrames, model.UniformAnimation.IdleAnimation.Duration);
                }
                else
                {
                    switch (this.FacingDirection)
                    {
                        case 0:
                            this.Sprite.Animate(time, model.UpAnimation.IdleAnimation.StartingFrame, model.UpAnimation.IdleAnimation.NumberOfFrames, model.UpAnimation.IdleAnimation.Duration);
                            break;
                        case 1:
                            this.Sprite.Animate(time, model.RightAnimation.IdleAnimation.StartingFrame, model.RightAnimation.IdleAnimation.NumberOfFrames, model.RightAnimation.IdleAnimation.Duration);
                            break;
                        case 2:
                            this.Sprite.Animate(time, model.DownAnimation.IdleAnimation.StartingFrame, model.DownAnimation.IdleAnimation.NumberOfFrames, model.DownAnimation.IdleAnimation.Duration);
                            break;
                        case 3:
                            this.Sprite.Animate(time, model.LeftAnimation.IdleAnimation.StartingFrame, model.LeftAnimation.IdleAnimation.NumberOfFrames, model.LeftAnimation.IdleAnimation.Duration);
                            break;
                    }
                }
            }
        }

        private void UpdateLight(GameTime time)
        {
            if (light != null)
            {
                if (this.model.Light.PulseSpeed != 0)
                {
                    this.light.radius.Value = this.model.Light.PulseMinRadius + (0.5f * (this.model.Light.Radius - this.model.Light.PulseMinRadius) * (1 + (float)Math.Sin(2 * Math.PI * lightPulseTimer)));//(this.model.Light.Radius / 2) * (1 + (float)Math.Sin(2 * Math.PI * lightPulseTimer));
                    this.lightPulseTimer = (this.lightPulseTimer + (float)time.ElapsedGameTime.TotalMilliseconds / this.model.Light.PulseSpeed) % 1;
                }

                this.light.position.Value = new Vector2(this.position.X + this.model.Light.OffsetX, this.position.Y + this.model.Light.OffsetY);
            }
        }

        private void PlayRequiredSounds(GameTime time)
        {
            if (this.soundAlwaysTimer != null)
            {
                this.soundAlwaysTimer = Math.Max(0, (int)this.soundAlwaysTimer - time.ElapsedGameTime.Milliseconds);
                if (soundAlwaysTimer <= 0 && Game1.random.NextDouble() <= alwaysSound.ChanceOfPlaying)
                {
                    Game1.playSound(alwaysSound.SoundName);
                    soundAlwaysTimer = alwaysSound.TimeBetweenSound;
                }
            }

            if (this.isMoving() && this.soundMovingTimer != null)
            {
                this.soundMovingTimer = Math.Max(0, (int)this.soundMovingTimer - time.ElapsedGameTime.Milliseconds);
                if (soundMovingTimer <= 0 && Game1.random.NextDouble() <= movingSound.ChanceOfPlaying)
                {
                    Game1.playSound(movingSound.SoundName);
                    soundMovingTimer = movingSound.TimeBetweenSound;
                }
            }

            if (!this.isMoving() && this.soundIdleTimer != null)
            {
                this.soundIdleTimer = Math.Max(0, (int)this.soundIdleTimer - time.ElapsedGameTime.Milliseconds);
                if (soundIdleTimer <= 0 && Game1.random.NextDouble() <= idleSound.ChanceOfPlaying)
                {
                    Game1.playSound(idleSound.SoundName);
                    soundIdleTimer = idleSound.TimeBetweenSound;
                }
            }
        }

        internal bool IsFlying()
        {
            return this.model.Type.ToUpper() == "FLYING";
        }

        internal void SetMotion(Vector2 motion)
        {
            this.motion.Value = motion;
        }

        internal void ResetForNewLocation(Vector2 position)
        {
            base.Position = position * 64f;
        }
    }
}
