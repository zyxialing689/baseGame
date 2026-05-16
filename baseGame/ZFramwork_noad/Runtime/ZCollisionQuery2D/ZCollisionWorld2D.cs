using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Collision2D
{
    [DefaultExecutionOrder(-500)]
    public class ZCollisionWorld2D : MonoBehaviour
    {
        public static ZCollisionWorld2D Instance { get; private set; }

        [Header("Grid")]
        public float cellSize = 1f;

        [Header("Debug")]
        public bool drawGrid;
        public bool drawAllBodies;
        public Color gridColor = new Color(1f, 1f, 1f, 0.08f);

        public int BodyCount => bodies.Count;
        public int CellCount => cells.Count;
        public int QueryCountThisFrame { get; private set; }
        public int CandidateCountThisFrame { get; private set; }

        private readonly List<ZCollisionBody2D> bodies = new List<ZCollisionBody2D>(1024);
        private readonly Dictionary<Vector2Int, List<ZCollisionBody2D>> cells = new Dictionary<Vector2Int, List<ZCollisionBody2D>>(1024);
        private readonly Dictionary<ZCollisionBody2D, List<Vector2Int>> bodyCells = new Dictionary<ZCollisionBody2D, List<Vector2Int>>(1024);

        private int queryId;

        private void Awake()
        {
            Instance = this;
            if (cellSize <= 0.001f)
                cellSize = 1f;
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void LateUpdate()
        {
            QueryCountThisFrame = 0;
            CandidateCountThisFrame = 0;
        }

        public void Register(ZCollisionBody2D body)
        {
            if (body == null)
                return;

            if (body.registered && body.World == this)
            {
                UpdateBody(body);
                return;
            }

            if (body.World != null && body.World != this)
                body.World.Unregister(body);

            body.UpdateCachedBounds();
            body.SetWorld(this);
            body.registered = true;

            if (!bodies.Contains(body))
                bodies.Add(body);

            InsertBodyToCells(body);
        }

        public void Unregister(ZCollisionBody2D body)
        {
            if (body == null)
                return;

            RemoveBodyFromCells(body);
            bodies.Remove(body);

            body.registered = false;
            if (body.World == this)
                body.SetWorld(null);
        }

        public void UpdateBody(ZCollisionBody2D body)
        {
            if (body == null)
                return;

            if (!body.registered || body.World != this)
            {
                Register(body);
                return;
            }

            body.UpdateCachedBounds();
            RemoveBodyFromCells(body);
            InsertBodyToCells(body);
        }

        public void RebuildAll()
        {
            cells.Clear();
            bodyCells.Clear();
            for (int i = bodies.Count - 1; i >= 0; i--)
            {
                ZCollisionBody2D body = bodies[i];
                if (body == null || !body.isActiveAndEnabled)
                {
                    bodies.RemoveAt(i);
                    continue;
                }

                body.UpdateCachedBounds();
                body.registered = true;
                body.SetWorld(this);
                InsertBodyToCells(body);
            }
        }

        public void ClearAll()
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                if (bodies[i] != null)
                {
                    bodies[i].registered = false;
                    if (bodies[i].World == this)
                        bodies[i].SetWorld(null);
                }
            }
            bodies.Clear();
            cells.Clear();
            bodyCells.Clear();
        }

        public int QueryAabbCandidatesNonAlloc(Rect area, ZCollisionLayer layerMask, List<ZCollisionBody2D> results)
        {
            if (results == null)
                return 0;

            QueryCountThisFrame++;
            queryId++;
            if (queryId == int.MaxValue)
                queryId = 1;

            int before = results.Count;
            GetCellRange(area, out int minX, out int maxX, out int minY, out int maxY);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2Int key = new Vector2Int(x, y);
                    if (!cells.TryGetValue(key, out List<ZCollisionBody2D> list))
                        continue;

                    for (int i = 0; i < list.Count; i++)
                    {
                        ZCollisionBody2D body = list[i];
                        if (body == null || !body.isActiveAndEnabled)
                            continue;

                        if (body.lastQueryId == queryId)
                            continue;

                        body.lastQueryId = queryId;

                        if ((body.layer & layerMask) == 0)
                            continue;

                        if (!body.HasCachedBounds)
                            body.UpdateCachedBounds();

                        if (!ZCollisionMath2D.RectOverlap(area, body.CachedBounds))
                            continue;

                        results.Add(body);
                    }
                }
            }

            int added = results.Count - before;
            CandidateCountThisFrame += added;
            return added;
        }

        /// <summary>
        /// 近战攻击检测：attacker.attackBox overlap target.bodyBox + attacker.groundBox overlap target.groundBox
        /// </summary>
        public int MeleeAttackNonAlloc(
            ZCollisionBody2D attacker,
            ZCollisionLayer targetMask,
            ZTargetRelation relation,
            List<ZCollisionBody2D> results,
            bool allowSelfTarget = false,
            int facingSign = 1)
        {
            if (attacker == null || results == null)
                return 0;

            if (!attacker.attackBox.enabled || !attacker.groundBox.enabled)
                return 0;

            if (!attacker.TryGetAttackWorldBox(facingSign, out Rect attackRect))
                return 0;

            if (!attacker.TryGetWorldBox(ZBoxType.Ground, out Rect attackerGroundRect))
                return 0;

            QueryCountThisFrame++;
            queryId++;
            if (queryId == int.MaxValue)
                queryId = 1;

            int before = results.Count;
            GetCellRange(attackRect, out int minX, out int maxX, out int minY, out int maxY);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2Int key = new Vector2Int(x, y);
                    if (!cells.TryGetValue(key, out List<ZCollisionBody2D> list))
                        continue;

                    for (int i = 0; i < list.Count; i++)
                    {
                        ZCollisionBody2D target = list[i];
                        if (target == null || !target.isActiveAndEnabled)
                            continue;

                        if (target.lastQueryId == queryId)
                            continue;

                        target.lastQueryId = queryId;

                        if (!allowSelfTarget && target == attacker)
                            continue;

                        if ((target.layer & targetMask) == 0)
                            continue;

                        if (!target.canBeHit)
                            continue;

                        if (!target.bodyBox.enabled || !target.groundBox.enabled)
                            continue;

                        if (!target.HasCachedBounds)
                            target.UpdateCachedBounds();

                        if (!ZCollisionMath2D.RectOverlap(attackRect, target.CachedBounds))
                            continue;

                        if (!target.TryGetWorldBox(ZBoxType.Body, out Rect targetBodyRect))
                            continue;

                        if (!target.TryGetWorldBox(ZBoxType.Ground, out Rect targetGroundRect))
                            continue;

                        if (!ZCollisionMath2D.RectOverlap(attackRect, targetBodyRect))
                            continue;

                        if (!ZCollisionMath2D.RectOverlap(attackerGroundRect, targetGroundRect))
                            continue;

                        if (!CheckRelation(attacker, target, relation))
                            continue;

                        results.Add(target);
                    }
                }
            }

            int added = results.Count - before;
            CandidateCountThisFrame += added;
            return added;
        }

        private static bool CheckRelation(ZCollisionBody2D a, ZCollisionBody2D b, ZTargetRelation relation)
        {
            switch (relation)
            {
                case ZTargetRelation.Any: return true;
                case ZTargetRelation.Ally: return a.teamId == b.teamId;
                case ZTargetRelation.Enemy: return a.teamId != b.teamId;
                case ZTargetRelation.Neutral: return a.teamId == 0 || b.teamId == 0;
                default: return true;
            }
        }

        private void InsertBodyToCells(ZCollisionBody2D body)
        {
            if (!body.HasCachedBounds)
                body.UpdateCachedBounds();

            List<Vector2Int> occupied = GetOrCreateBodyCellList(body);
            occupied.Clear();

            GetCellRange(body.CachedBounds, out int minX, out int maxX, out int minY, out int maxY);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2Int key = new Vector2Int(x, y);
                    if (!cells.TryGetValue(key, out List<ZCollisionBody2D> list))
                    {
                        list = new List<ZCollisionBody2D>(8);
                        cells.Add(key, list);
                    }
                    list.Add(body);
                    occupied.Add(key);
                }
            }
        }

        private void RemoveBodyFromCells(ZCollisionBody2D body)
        {
            if (!bodyCells.TryGetValue(body, out List<Vector2Int> occupied))
                return;

            for (int i = 0; i < occupied.Count; i++)
            {
                Vector2Int key = occupied[i];
                if (!cells.TryGetValue(key, out List<ZCollisionBody2D> list))
                    continue;

                list.Remove(body);
                if (list.Count == 0)
                    cells.Remove(key);
            }
            occupied.Clear();
        }

        private List<Vector2Int> GetOrCreateBodyCellList(ZCollisionBody2D body)
        {
            if (!bodyCells.TryGetValue(body, out List<Vector2Int> list))
            {
                list = new List<Vector2Int>(4);
                bodyCells.Add(body, list);
            }
            return list;
        }

        public void GetCellRange(Rect rect, out int minX, out int maxX, out int minY, out int maxY)
        {
            float cs = Mathf.Max(0.001f, cellSize);
            minX = Mathf.FloorToInt(rect.xMin / cs);
            maxX = Mathf.FloorToInt(rect.xMax / cs);
            minY = Mathf.FloorToInt(rect.yMin / cs);
            maxY = Mathf.FloorToInt(rect.yMax / cs);
        }

        public Rect GetCellRect(Vector2Int cell)
        {
            float cs = Mathf.Max(0.001f, cellSize);
            return new Rect(cell.x * cs, cell.y * cs, cs, cs);
        }

        public bool TryGetOccupiedCells(ZCollisionBody2D body, out List<Vector2Int> occupied)
        {
            return bodyCells.TryGetValue(body, out occupied);
        }

        private void OnDrawGizmos()
        {
            if (!drawGrid && !drawAllBodies)
                return;

            if (drawGrid)
            {
                Gizmos.color = gridColor;
                foreach (Vector2Int key in cells.Keys)
                {
                    Rect r = GetCellRect(key);
                    Gizmos.DrawWireCube(r.center, r.size);
                }
            }
        }
    }
}
