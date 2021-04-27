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
        internal Farmer owner;
        internal Vector2 targetTile;
        internal CompanionModel model;
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
        private readonly NetInt previousDirection = new NetInt();
        private readonly NetColor color = new NetColor();
        internal readonly NetVector2 motion = new NetVector2(Vector2.Zero);
        private new readonly NetRectangle nextPosition = new NetRectangle();

        public Companion(CompanionModel model, Vector2 targetTile, GameLocation location) : this(model, null, targetTile)
        {
            this.targetTile = targetTile * 64f;
            this.currentLocation = location;
            this.SetUpCompanion();
        }

        public Companion(CompanionModel model, Farmer owner, Vector2? targetTile = null) : base(new AnimatedSprite(model.TileSheetPath, 0, model.FrameSizeWidth, model.FrameSizeHeight), (owner is null ? (Vector2)targetTile : owner.getTileLocation()) * 64f + new Vector2(model.SpawnOffsetX, model.SpawnOffsetY), 2, model.Name)
        {
            base.Breather = false;
            base.speed = model.TravelSpeed;
            base.forceUpdateTimer = 9999;
            base.collidesWithOtherCharacters.Value = (model.Type.ToUpper() == "FLYING" ? false : true);
            base.farmerPassesThrough = true;
            base.HideShadow = true;
            base.Sprite.loop = true;

            this.model = model;
            this.specialNumber.Value = Game1.random.Next(100);
            this.idleBehavior = new IdleBehavior(model.IdleBehavior);

            if (owner != null)
            {
                this.owner = owner;
                this.currentLocation = owner.currentLocation;
                this.SetUpCompanion();
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

            return true;
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
            b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(this.GetSpriteWidthForPositioning() * 4 / 2, this.GetBoundingBox().Height / 2) + ((this.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), this.Sprite.SourceRect, this.isPrismatic ? Utility.GetPrismaticColor(348 + (int)this.specialNumber, 5f) : color, this.rotation, new Vector2(this.Sprite.SpriteWidth / 2, (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, base.scale) * 4f, (base.flip || (this.Sprite.CurrentAnimation != null && this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, this.IsFlying() ? 0.991f : Math.Max(0f, base.drawOnTop ? 0.991f : ((float)base.getStandingY() / 10000f)));
        }

        private void SetUpCompanion()
        {
            // Verify the location the companion is spawning on isn't occupied (if collidesWithOtherCharacters == true)
            if (this.collidesWithOtherCharacters)
            {
                this.PlaceInEmptyTile();
            }
            this.nextPosition.Value = this.GetBoundingBox();


            // Set up the sounds to play, if any
            this.idleSound = this.model.Sounds.FirstOrDefault(s => s.WhenToPlay.ToUpper() == "IDLE");
            if (this.idleSound != null && CustomCompanions.IsSoundValid(this.idleSound.SoundName, true))
            {
                this.soundIdleTimer = this.idleSound.TimeBetweenSound;
            }

            this.movingSound = this.model.Sounds.FirstOrDefault(s => s.WhenToPlay.ToUpper() == "MOVING");
            if (this.movingSound != null && CustomCompanions.IsSoundValid(this.movingSound.SoundName, true))
            {
                this.soundMovingTimer = this.movingSound.TimeBetweenSound;
            }

            this.alwaysSound = this.model.Sounds.FirstOrDefault(s => s.WhenToPlay.ToUpper() == "ALWAYS");
            if (this.alwaysSound != null && CustomCompanions.IsSoundValid(this.alwaysSound.SoundName, true))
            {
                this.soundAlwaysTimer = this.alwaysSound.TimeBetweenSound;
            }

            // Pick a random color (Color.White if none given) or use prismatic if IsPrismatic is true
            color.Value = Color.White;
            if (this.model.Colors.Count > 0)
            {
                int randomColorIndex = Game1.random.Next(this.model.Colors.Count + (this.model.IsPrismatic ? 1 : 0));
                if (randomColorIndex > this.model.Colors.Count - 1)
                {
                    // Primsatic color has been selected
                    this.isPrismatic.Value = true;
                }
                else
                {
                    this.color.Value = CustomCompanions.GetColorFromArray(this.model.Colors[randomColorIndex]);
                }
            }

            // Set up the light to give off, if any
            if (this.model.Light != null)
            {
                this.lightPulseTimer = this.model.Light.PulseSpeed;

                this.light = new LightSource(1, new Vector2(this.position.X + this.model.Light.OffsetX, this.position.Y + this.model.Light.OffsetY), this.model.Light.Radius, CustomCompanions.GetColorFromArray(this.model.Light.Color), this.id, LightSource.LightContext.None, 0L);
                Game1.currentLightSources.Add(this.light);
            }
        }
        private void PlaceInEmptyTile()
        {
            foreach (var character in this.currentLocation.characters.Where(c => c != this))
            {
                if (character.GetBoundingBox().Intersects(this.GetBoundingBox()))
                {
                    CustomCompanions.monitor.Log("HERE", StardewModdingAPI.LogLevel.Debug);
                    base.Position = Utility.getRandomAdjacentOpenTile(this.getTileLocation(), this.currentLocation) * 64f;
                }
            }
        }

        private void AttemptMovement(GameTime time, GameLocation location)
        {
            if (owner != null || targetTile != null)
            {
                var targetDistance = Vector2.Distance(base.Position, this.GetTargetPosition());
                if (targetDistance > 640f)
                {
                    this.hasReachedPlayer.Value = false;
                    base.position.Value = this.GetTargetPosition();
                }
                else if ((targetDistance > 64f && ((owner != null && owner.isMoving()) || !this.hasReachedPlayer.Value)) || targetDistance > this.model.MaxIdleDistance)
                {
                    if (owner is null && targetDistance > this.model.MaxIdleDistance)
                    {
                        Vector2 targetPosition = this.GetTargetPosition() + new Vector2(this.model.SpawnOffsetX, this.model.SpawnOffsetY);
                        base.position.Value = Vector2.Lerp(base.position, targetPosition, this.model.TravelSpeed / 300f);
                        this.motion.Value *= -1;
                    }
                    else
                    {
                        this.hasReachedPlayer.Value = false;

                        base.Speed = model.TravelSpeed;
                        if (targetDistance > this.model.MaxIdleDistance)
                        {
                            base.Speed = model.TravelSpeed + (int)(targetDistance / 64f) - 1;
                        }

                        if (IsJumper())
                        {
                            float jumpScale = 10f;
                            float randomJumpBoostMultiplier = 2f;
                            if (this.model.IdleArguments != null && this.model.IdleArguments.Length >= 2)
                            {
                                jumpScale = this.model.IdleArguments[0];
                                randomJumpBoostMultiplier = this.model.IdleArguments[1];
                            }

                            this.PerformJumpMovement(jumpScale, randomJumpBoostMultiplier);
                        }
                        else
                        {
                            this.SetMotion(Utility.getVelocityTowardPoint(new Point((int)base.Position.X + this.model.SpawnOffsetX, (int)base.Position.Y + this.model.SpawnOffsetY), this.GetTargetPosition(), base.speed));
                        }
                    }
                }
                else
                {
                    this.hasReachedPlayer.Value = true;
                }
            }

            // Perform the position movement
            if (!this.hasReachedPlayer.Value || this.idleBehavior.PerformIdleBehavior(this, time, this.model.IdleArguments))
            {
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
            }

            // Update any animations
            if (!this.motion.Equals(Vector2.Zero))
            {
                this.previousDirection.Value = this.FacingDirection;

                if (model.UniformAnimation != null && !CustomCompanions.CompanionHasFullMovementSet(model))
                {
                    this.FacingDirection = 2;
                    this.Animate(time, false);
                }
                else if (CustomCompanions.CompanionHasFullMovementSet(model))
                {
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

                    this.Animate(time, false);
                }
            }
            else
            {
                this.Animate(time, true);
            }
        }

        private void Animate(GameTime time, bool isIdle = false)
        {
            if (isIdle)
            {
                if (this.Sprite.CurrentAnimation != null && this.previousDirection == this.FacingDirection)
                {
                    this.Sprite.animateOnce(time);
                }
                else if (this.model.UniformAnimation != null && !CustomCompanions.CompanionHasFullMovementSet(model))
                {
                    this.Sprite.CurrentAnimation = null;
                    if (this.model.UniformAnimation.IdleAnimation.ManualFrames != null)
                    {
                        this.Sprite.setCurrentAnimation(GetManualFrames(this.model.UniformAnimation.IdleAnimation.ManualFrames));
                    }
                    else
                    {
                        this.Sprite.Animate(time, model.UniformAnimation.IdleAnimation.StartingFrame, model.UniformAnimation.IdleAnimation.NumberOfFrames, model.UniformAnimation.IdleAnimation.Duration);
                    }
                }
                else
                {
                    this.Sprite.CurrentAnimation = null;
                    switch (this.FacingDirection)
                    {
                        case 0:
                            if (this.model.UpAnimation.IdleAnimation.ManualFrames != null)
                            {
                                this.Sprite.setCurrentAnimation(GetManualFrames(this.model.UpAnimation.IdleAnimation.ManualFrames));
                            }
                            else
                            {
                                this.Sprite.Animate(time, model.UpAnimation.IdleAnimation.StartingFrame, model.UpAnimation.IdleAnimation.NumberOfFrames, model.UpAnimation.IdleAnimation.Duration);
                            }
                            break;
                        case 1:
                            if (this.model.RightAnimation.IdleAnimation.ManualFrames != null)
                            {
                                this.Sprite.setCurrentAnimation(GetManualFrames(this.model.RightAnimation.IdleAnimation.ManualFrames));
                            }
                            else
                            {
                                this.Sprite.Animate(time, model.RightAnimation.IdleAnimation.StartingFrame, model.RightAnimation.IdleAnimation.NumberOfFrames, model.RightAnimation.IdleAnimation.Duration);
                            }
                            break;
                        case 2:
                            if (this.model.DownAnimation.IdleAnimation.ManualFrames != null)
                            {
                                this.Sprite.setCurrentAnimation(GetManualFrames(this.model.DownAnimation.IdleAnimation.ManualFrames));
                            }
                            else
                            {
                                this.Sprite.Animate(time, model.DownAnimation.IdleAnimation.StartingFrame, model.DownAnimation.IdleAnimation.NumberOfFrames, model.DownAnimation.IdleAnimation.Duration);
                            }
                            break;
                        case 3:
                            if (this.model.LeftAnimation.IdleAnimation.ManualFrames != null)
                            {
                                this.Sprite.setCurrentAnimation(GetManualFrames(this.model.LeftAnimation.IdleAnimation.ManualFrames));
                            }
                            else
                            {
                                this.Sprite.Animate(time, model.LeftAnimation.IdleAnimation.StartingFrame, model.LeftAnimation.IdleAnimation.NumberOfFrames, model.LeftAnimation.IdleAnimation.Duration);
                            }
                            break;
                    }
                }
            }
            else
            {
                if (this.Sprite.CurrentAnimation != null && this.previousDirection == this.FacingDirection)
                {
                    this.Sprite.animateOnce(time);
                }
                else if (this.model.UniformAnimation != null && !CustomCompanions.CompanionHasFullMovementSet(model))
                {
                    this.Sprite.CurrentAnimation = null;
                    if (this.model.UniformAnimation.ManualFrames != null)
                    {
                        this.Sprite.setCurrentAnimation(GetManualFrames(this.model.UniformAnimation.ManualFrames));
                    }
                    else
                    {
                        this.Sprite.Animate(time, model.UniformAnimation.StartingFrame, model.UniformAnimation.NumberOfFrames, model.UniformAnimation.Duration);
                    }
                }
                else
                {
                    this.Sprite.CurrentAnimation = null;
                    switch (this.FacingDirection)
                    {
                        case 0:
                            if (this.model.UpAnimation.ManualFrames != null)
                            {
                                this.Sprite.setCurrentAnimation(GetManualFrames(this.model.UpAnimation.ManualFrames));
                            }
                            else
                            {
                                this.Sprite.Animate(time, model.UpAnimation.StartingFrame, model.UpAnimation.NumberOfFrames, model.UpAnimation.Duration);
                            }
                            break;
                        case 1:
                            if (this.model.RightAnimation.ManualFrames != null)
                            {
                                this.Sprite.setCurrentAnimation(GetManualFrames(this.model.RightAnimation.ManualFrames));
                            }
                            else
                            {
                                this.Sprite.Animate(time, model.RightAnimation.StartingFrame, model.RightAnimation.NumberOfFrames, model.RightAnimation.Duration);
                            }
                            break;
                        case 2:
                            if (this.model.DownAnimation.ManualFrames != null)
                            {
                                this.Sprite.setCurrentAnimation(GetManualFrames(this.model.DownAnimation.ManualFrames));
                            }
                            else
                            {
                                this.Sprite.Animate(time, model.DownAnimation.StartingFrame, model.DownAnimation.NumberOfFrames, model.DownAnimation.Duration);
                            }
                            break;
                        case 3:
                            if (this.model.LeftAnimation.ManualFrames != null)
                            {
                                this.Sprite.setCurrentAnimation(GetManualFrames(this.model.LeftAnimation.ManualFrames));
                            }
                            else
                            {
                                this.Sprite.Animate(time, model.LeftAnimation.StartingFrame, model.LeftAnimation.NumberOfFrames, model.LeftAnimation.Duration);
                            }
                            break;
                    }
                }
            }
        }

        private List<FarmerSprite.AnimationFrame> GetManualFrames(List<ManualFrameModel> manualFrames)
        {
            var frames = new List<FarmerSprite.AnimationFrame>();
            foreach (var frame in manualFrames)
            {
                frames.Add(new FarmerSprite.AnimationFrame(frame.Frame, frame.Duration, false, flip: frame.Flip));
            }

            return frames;
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

        internal bool IsJumper()
        {
            return this.model.Type.ToUpper() == "JUMPING";
        }

        internal void PerformJumpMovement(float jumpScale, float randomJumpBoostMultiplier)
        {
            if (this.yJumpOffset == 0)
            {
                this.jumpWithoutSound();
                this.yJumpVelocity = (float)Game1.random.Next(50, 70) / jumpScale;

                if (Game1.random.NextDouble() < 0.01)
                {
                    this.yJumpVelocity *= randomJumpBoostMultiplier;
                }
            }

            Vector2 v = Utility.getAwayFromPositionTrajectory(this.GetBoundingBox(), this.GetTargetPosition());
            this.xVelocity += (0f - v.X) / 150f + ((Game1.random.NextDouble() < 0.01) ? ((float)Game1.random.Next(-50, 50) / 10f) : 0f);
            if (Math.Abs(this.xVelocity) > 5f)
            {
                this.xVelocity = Math.Sign(this.xVelocity) * 5;
            }
            this.yVelocity += (0f - v.Y) / 150f + ((Game1.random.NextDouble() < 0.01) ? ((float)Game1.random.Next(-50, 50) / 10f) : 0f);
            if (Math.Abs(this.yVelocity) > 5f)
            {
                this.yVelocity = Math.Sign(this.yVelocity) * 5;
            }

            this.SetMotion(Utility.getVelocityTowardPoint(new Point((int)this.Position.X + this.model.SpawnOffsetX, (int)this.Position.Y + this.model.SpawnOffsetY), new Vector2(this.GetTargetPosition().X, this.GetTargetPosition().Y), this.speed));
        }

        internal Vector2 GetTargetPosition()
        {
            if (owner != null && owner.currentLocation == this.currentLocation)
            {
                return owner.position;
            }

            return targetTile;
        }

        internal void SetMotion(Vector2 motion)
        {
            this.motion.Value = motion;
        }

        internal void ResetForNewLocation(GameLocation location, Vector2 position)
        {
            base.Position = position * 64f;
            this.currentLocation = location;

            if (this.collidesWithOtherCharacters)
            {
                this.PlaceInEmptyTile();
            }
        }
    }
}
