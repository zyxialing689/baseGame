using System;
using UnityEngine;

namespace ZGame.Collision2D
{
    [Flags]
    public enum ZCollisionLayer
    {
        None       = 0,
        Hero       = 1 << 0,
        Monster    = 1 << 1,
        NPC        = 1 << 2,
        Building   = 1 << 3,
        Resource   = 1 << 4,
        DropItem   = 1 << 5,
        Projectile = 1 << 6,
        SkillArea  = 1 << 7,
        Custom1    = 1 << 8,
        Custom2    = 1 << 9,
        All        = ~0
    }

    public enum ZTargetRelation
    {
        Any,
        Ally,
        Enemy,
        Neutral
    }

    public enum ZHitRule
    {
        GroundOnly,
        BodyOnly,
        SkyOnly,
        BodyAndGroundMatch,
        BodyAndSkyMatch,
        AnyEnabledBox
    }

    public enum ZBoxType
    {
        Ground,
        Body,
        Sky,
        Select,
        Interact,
        Attack
    }

    public enum ZBodyDrawMode
    {
        None,
        SelectedOnly,
        Always
    }

    public enum ZCollisionPreset
    {
        Custom,
        GroundOnly,
        BodyOnly,
        SkyOnly,
        GroundBody,
        BodySky,
        GroundBodySky,
        Melee
    }

    [Serializable]
    public struct ZBox2D
    {
        public bool enabled;
        public Vector2 offset;
        public Vector2 size;

        public static ZBox2D Disabled(Vector2 size)
        {
            return new ZBox2D
            {
                enabled = false,
                offset = Vector2.zero,
                size = size
            };
        }

        public static ZBox2D Enabled(Vector2 size)
        {
            return new ZBox2D
            {
                enabled = true,
                offset = Vector2.zero,
                size = size
            };
        }
    }
}
