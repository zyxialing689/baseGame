using UnityEngine;

namespace ZGame.Collision2D
{
    [DisallowMultipleComponent]
    public class ZCollisionBody2D : MonoBehaviour
    {
        private static long _nextEntityId = 1;

        [Header("Identity")]
        [HideInInspector]
        public long entityId;
        public int teamId;
        public ZCollisionLayer layer = ZCollisionLayer.Hero;

        [Header("Query Flags")]
        public bool canBeHit = true;
        public bool canBeSelected = true;
        public bool canBeInteracted = true;

        [Header("Core Boxes")]
        public ZBox2D groundBox = ZBox2D.Enabled(new Vector2(0.6f, 0.25f));
        public ZBox2D bodyBox = ZBox2D.Enabled(new Vector2(0.6f, 1.0f));
        public ZBox2D skyBox = ZBox2D.Disabled(new Vector2(0.6f, 0.6f));

        [Header("Utility Boxes")]
        public ZBox2D selectBox = ZBox2D.Disabled(new Vector2(0.8f, 1.2f));
        public ZBox2D interactBox = ZBox2D.Disabled(new Vector2(1.2f, 1.2f));

        [Header("Attack Box")]
        public ZBox2D attackBox = ZBox2D.Disabled(new Vector2(1.0f, 1.0f));

        [Header("Debug Draw")]
        public ZBodyDrawMode drawMode = ZBodyDrawMode.SelectedOnly;
        public bool drawOccupiedCells;

        [HideInInspector] public bool registered;
        public ZCollisionPreset preset = ZCollisionPreset.Custom;

        internal int lastQueryId;

        public ZCollisionWorld2D World => world;

        [SerializeField, HideInInspector]
        private ZCollisionWorld2D world;

        internal void SetWorld(ZCollisionWorld2D value)
        {
            world = value;
        }

        private Rect cachedBounds;
        private bool hasCachedBounds;

        public Rect CachedBounds => cachedBounds;
        public bool HasCachedBounds => hasCachedBounds;
        public Vector2 Position2D => new Vector2(transform.position.x, transform.position.y);

        private void Reset()
        {
            if (entityId == 0)
                entityId = _nextEntityId++;
        }

        private void OnEnable()
        {
            if (entityId == 0)
                entityId = _nextEntityId++;

            if (ZCollisionWorld2D.Instance != null)
                ZCollisionWorld2D.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (world != null)
                world.Unregister(this);
        }

        private void OnValidate()
        {
            FixBox(ref groundBox);
            FixBox(ref bodyBox);
            FixBox(ref skyBox);
            FixBox(ref selectBox);
            FixBox(ref interactBox);
            FixBox(ref attackBox);

            if (registered && world != null && isActiveAndEnabled)
                SyncToWorld();
        }

        public void SyncToWorld()
        {
            UpdateCachedBounds();
            if (registered && world != null)
                world.UpdateBody(this);
            else if (ZCollisionWorld2D.Instance != null)
                ZCollisionWorld2D.Instance.Register(this);
        }

        public bool TryGetWorldBox(ZBoxType type, out Rect rect)
        {
            ZBox2D box = GetBox(type);
            if (!box.enabled)
            {
                rect = default;
                return false;
            }

            rect = GetWorldRect(box);
            return true;
        }

        public bool TryGetHitBox(ZHitRule rule, out Rect rect)
        {
            switch (rule)
            {
                case ZHitRule.GroundOnly:
                    return TryGetWorldBox(ZBoxType.Ground, out rect);
                case ZHitRule.BodyOnly:
                    return TryGetWorldBox(ZBoxType.Body, out rect);
                case ZHitRule.SkyOnly:
                    return TryGetWorldBox(ZBoxType.Sky, out rect);
                default:
                    return TryGetWorldBox(ZBoxType.Body, out rect);
            }
        }

        public bool TryGetSelectBox(out Rect rect)
        {
            if (TryGetWorldBox(ZBoxType.Select, out rect)) return true;
            if (TryGetWorldBox(ZBoxType.Body, out rect)) return true;
            if (TryGetWorldBox(ZBoxType.Ground, out rect)) return true;
            if (TryGetWorldBox(ZBoxType.Sky, out rect)) return true;
            return false;
        }

        public bool TryGetInteractBox(out Rect rect)
        {
            if (TryGetWorldBox(ZBoxType.Interact, out rect)) return true;
            if (TryGetWorldBox(ZBoxType.Body, out rect)) return true;
            if (TryGetWorldBox(ZBoxType.Ground, out rect)) return true;
            if (TryGetWorldBox(ZBoxType.Sky, out rect)) return true;
            return false;
        }

        /// <summary>
        /// 获取攻击框的世界坐标（支持 facing 翻转）
        /// </summary>
        public bool TryGetAttackWorldBox(int facingSign, out Rect rect)
        {
            if (!attackBox.enabled)
            {
                rect = default;
                return false;
            }

            Vector3 scale3 = transform.lossyScale;
            Vector2 scale = new Vector2(Mathf.Abs(scale3.x), Mathf.Abs(scale3.y));
            float offsetX = attackBox.offset.x * (facingSign >= 0 ? 1f : -1f);
            Vector2 center = Position2D + new Vector2(offsetX * scale.x, attackBox.offset.y * scale.y);
            Vector2 size = new Vector2(Mathf.Abs(attackBox.size.x * scale.x), Mathf.Abs(attackBox.size.y * scale.y));
            rect = ZCollisionMath2D.MakeRect(center, size);
            return true;
        }

        public bool TryGetAnyEnabledBox(int index, out Rect rect)
        {
            switch (index)
            {
                case 0: return TryGetWorldBox(ZBoxType.Ground, out rect);
                case 1: return TryGetWorldBox(ZBoxType.Body, out rect);
                case 2: return TryGetWorldBox(ZBoxType.Sky, out rect);
                case 3: return TryGetWorldBox(ZBoxType.Select, out rect);
                case 4: return TryGetWorldBox(ZBoxType.Interact, out rect);
                case 5: return TryGetWorldBox(ZBoxType.Attack, out rect);
                default:
                    rect = default;
                    return false;
            }
        }

        public ZBox2D GetBox(ZBoxType type)
        {
            switch (type)
            {
                case ZBoxType.Ground: return groundBox;
                case ZBoxType.Body: return bodyBox;
                case ZBoxType.Sky: return skyBox;
                case ZBoxType.Select: return selectBox;
                case ZBoxType.Interact: return interactBox;
                case ZBoxType.Attack: return attackBox;
                default: return bodyBox;
            }
        }

        public void SetBox(ZBoxType type, ZBox2D box)
        {
            FixBox(ref box);
            switch (type)
            {
                case ZBoxType.Ground: groundBox = box; break;
                case ZBoxType.Body: bodyBox = box; break;
                case ZBoxType.Sky: skyBox = box; break;
                case ZBoxType.Select: selectBox = box; break;
                case ZBoxType.Interact: interactBox = box; break;
                case ZBoxType.Attack: attackBox = box; break;
            }
            SyncToWorld();
        }

        public void ApplyPreset(ZCollisionPreset preset)
        {
            switch (preset)
            {
                case ZCollisionPreset.GroundOnly:
                    groundBox = ZBox2D.Enabled(new Vector2(0.8f, 0.4f));
                    bodyBox = ZBox2D.Disabled(new Vector2(0.8f, 1f));
                    skyBox = ZBox2D.Disabled(new Vector2(0.8f, 0.8f));
                    break;
                case ZCollisionPreset.BodyOnly:
                    groundBox = ZBox2D.Disabled(new Vector2(0.6f, 0.25f));
                    bodyBox = ZBox2D.Enabled(new Vector2(0.8f, 1.2f));
                    skyBox = ZBox2D.Disabled(new Vector2(0.8f, 0.8f));
                    break;
                case ZCollisionPreset.SkyOnly:
                    groundBox = ZBox2D.Disabled(new Vector2(0.6f, 0.25f));
                    bodyBox = ZBox2D.Disabled(new Vector2(0.8f, 1f));
                    skyBox = ZBox2D.Enabled(new Vector2(0.9f, 0.9f));
                    break;
                case ZCollisionPreset.GroundBody:
                    groundBox = ZBox2D.Enabled(new Vector2(0.6f, 0.25f));
                    bodyBox = ZBox2D.Enabled(new Vector2(0.8f, 1.2f));
                    skyBox = ZBox2D.Disabled(new Vector2(0.8f, 0.8f));
                    break;
                case ZCollisionPreset.BodySky:
                    groundBox = ZBox2D.Disabled(new Vector2(0.6f, 0.25f));
                    bodyBox = ZBox2D.Enabled(new Vector2(0.8f, 1.0f));
                    skyBox = ZBox2D.Enabled(new Vector2(0.9f, 0.9f));
                    break;
                case ZCollisionPreset.GroundBodySky:
                    groundBox = ZBox2D.Enabled(new Vector2(0.6f, 0.25f));
                    bodyBox = ZBox2D.Enabled(new Vector2(0.8f, 1.2f));
                    skyBox = ZBox2D.Enabled(new Vector2(0.8f, 0.8f));
                    break;
                case ZCollisionPreset.Melee:
                    groundBox = ZBox2D.Enabled(new Vector2(0.6f, 0.25f));
                    bodyBox = ZBox2D.Enabled(new Vector2(0.8f, 1.2f));
                    attackBox = ZBox2D.Enabled(new Vector2(1.2f, 1.2f));
                    skyBox = ZBox2D.Disabled(new Vector2(0.8f, 0.8f));
                    selectBox = ZBox2D.Disabled(new Vector2(0.8f, 1.2f));
                    interactBox = ZBox2D.Disabled(new Vector2(1.2f, 1.2f));
                    break;
            }
            SyncToWorld();
        }

        public void UpdateCachedBounds()
        {
            bool hasAny = false;
            Rect total = default;
            AddToBounds(ref hasAny, ref total, groundBox);
            AddToBounds(ref hasAny, ref total, bodyBox);
            AddToBounds(ref hasAny, ref total, skyBox);
            AddToBounds(ref hasAny, ref total, selectBox);
            AddToBounds(ref hasAny, ref total, interactBox);
            AddToBounds(ref hasAny, ref total, attackBox);

            if (!hasAny)
                total = ZCollisionMath2D.MakeRect(Position2D, Vector2.one * 0.01f);

            cachedBounds = total;
            hasCachedBounds = true;
        }

        private void AddToBounds(ref bool hasAny, ref Rect total, ZBox2D box)
        {
            if (!box.enabled)
                return;

            Rect rect = GetWorldRect(box);
            if (!hasAny)
            {
                total = rect;
                hasAny = true;
                return;
            }

            total.xMin = Mathf.Min(total.xMin, rect.xMin);
            total.xMax = Mathf.Max(total.xMax, rect.xMax);
            total.yMin = Mathf.Min(total.yMin, rect.yMin);
            total.yMax = Mathf.Max(total.yMax, rect.yMax);
        }

        private Rect GetWorldRect(ZBox2D box)
        {
            Vector3 scale3 = transform.lossyScale;
            Vector2 scale = new Vector2(Mathf.Abs(scale3.x), Mathf.Abs(scale3.y));
            Vector2 center = Position2D + new Vector2(box.offset.x * scale.x, box.offset.y * scale.y);
            Vector2 size = new Vector2(Mathf.Abs(box.size.x * scale.x), Mathf.Abs(box.size.y * scale.y));
            return ZCollisionMath2D.MakeRect(center, size);
        }

        private static void FixBox(ref ZBox2D box)
        {
            box.size.x = Mathf.Max(0.001f, Mathf.Abs(box.size.x));
            box.size.y = Mathf.Max(0.001f, Mathf.Abs(box.size.y));
        }
    }
}
