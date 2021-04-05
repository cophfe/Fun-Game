﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlib;
using Raylib;
using static Raylib.Raylib;

namespace Project2D
{
	class Eye : GameObject
	{
		Vector2 targetPosition;
		float yOffset = 0;
		Vector2 flipMultiplier = Vector2.One;
		Vector2 centeredPosition;
		public Vector2 middlePoint;
		float maxDistance;
		bool isMain = true;
		Eye main = null;
		bool isLasering = false;

		public Eye(Vector2 offset, float maxDistance, float size) : base(TextureName.Pupil)
		{
			centeredPosition = offset;
			position = offset;
			LocalScale = new Vector2(size, size);
			this.maxDistance = maxDistance;
			spriteManager.SetTint(new Colour(0, 0, 0, 255));
		}

		public Eye(Vector2 offset, Eye main) : base(TextureName.Pupil)
		{
			main.InitiateSecond(out maxDistance, out scale);
			isMain = false;
			this.main = main;
			centeredPosition = offset;
			spriteManager.SetTint(new Colour(0, 0, 0, 255));
			main.InitiateMiddlePoint(offset);
		}

		public override void Update()
		{
			if (isMain)
			{
				Vector2 mousePos = Game.GetCurrentScene().GetCamera().GetMouseWorldPosition();
				targetPosition = mousePos - (middlePoint * flipMultiplier + parent.GlobalPosition);
				targetPosition = targetPosition.MagnitudeSquared() > maxDistance * maxDistance ? targetPosition.Normalised() * maxDistance : targetPosition;
			}
			else
			{
				Colour c;
				main.CopyValues(out flipMultiplier, out yOffset, out targetPosition, out isLasering, out c);
				spriteManager.SetTint(c);
			}
			position = centeredPosition * flipMultiplier + targetPosition;
			position.y += yOffset;
			base.Update();

		}

		public void InitiateMiddlePoint(Vector2 offset)
		{
			middlePoint = new Vector2(Trig.Lerp(centeredPosition.x, offset.x, 0.5f), Trig.Lerp(centeredPosition.y, offset.y, 0.5f));
		}
		public void InitiateSecond(out float dist, out Vector2 scale)
		{
			dist = maxDistance;
			scale = this.scale;
		}

		public void CopyValues(out Vector2 flip, out float yOffset, out Vector2 local, out bool laser, out Colour tint)
		{
			flip = flipMultiplier;
			yOffset = this.yOffset;
			local = targetPosition;
			laser = isLasering;
			tint = spriteManager.GetTint();
		}

		public Vector2 GetTarget()
		{
			return targetPosition;
		}
		public override void Draw()
		{
			if (isLasering)
			{
				Ray ray = new Ray(GlobalPosition, targetPosition.Normalised());
				Hit hit;
				if (Game.GetCurrentScene().GetCollisionManager().RayCast(ray, out hit, CollisionLayer.Player))
				{
					DrawLineEx(ray.position, ray.direction * hit.distanceAlongRay + ray.position, 10, RLColor.RED);
					if (hit.objectHit.GetCollider().GetLayer() == CollisionLayer.Enemy)
					{
						(hit.objectHit as Chicken).cookedValue += Game.deltaTime * 2;
					}
					else
					{
						hit.objectHit.AddImpulseAtPosition(ray.direction * 2000, (ray.direction * hit.distanceAlongRay + ray.position) - hit.objectHit.GetCollider().GetCentrePoint());

					}
				}
				else
					DrawLineEx(GlobalPosition, (GlobalPosition + ray.direction * 1000), 10, RLColor.RED);
			}

			base.Draw();
		}

		public void SetLaser(bool isLasering)
		{
			this.isLasering = isLasering;
		}

		public void SetOffsetY(float y)
		{
			yOffset = y;
		}

		public void FlipY(bool isFlipped)
		{
			flipMultiplier.y = isFlipped ? -1 : 1;
		}

		public void FlipX(bool isFlipped)
		{
			flipMultiplier.x = isFlipped ? -1 : 1;
		}
	}
}
