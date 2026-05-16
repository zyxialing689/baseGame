using UnityEngine;

namespace ZGame.Collision2D
{
    public static class ZCollisionMath2D
    {
        public static Rect MakeRect(Vector2 center, Vector2 size)
        {
            size.x = Mathf.Max(0f, size.x);
            size.y = Mathf.Max(0f, size.y);
            return new Rect(center - size * 0.5f, size);
        }

        public static Rect Expand(Rect rect, float amount)
        {
            amount = Mathf.Max(0f, amount);
            rect.xMin -= amount;
            rect.xMax += amount;
            rect.yMin -= amount;
            rect.yMax += amount;
            return rect;
        }

        public static bool RectOverlap(Rect a, Rect b)
        {
            return a.xMin <= b.xMax && a.xMax >= b.xMin &&
                   a.yMin <= b.yMax && a.yMax >= b.yMin;
        }

        public static bool RectContainsPoint(Rect rect, Vector2 point)
        {
            return point.x >= rect.xMin && point.x <= rect.xMax &&
                   point.y >= rect.yMin && point.y <= rect.yMax;
        }

        public static bool CircleOverlapRect(Vector2 center, float radius, Rect rect)
        {
            radius = Mathf.Max(0f, radius);
            float x = Mathf.Clamp(center.x, rect.xMin, rect.xMax);
            float y = Mathf.Clamp(center.y, rect.yMin, rect.yMax);
            float dx = center.x - x;
            float dy = center.y - y;
            return dx * dx + dy * dy <= radius * radius;
        }

        public static bool SegmentOverlapRect(Vector2 a, Vector2 b, Rect rect, float radius)
        {
            Rect expanded = Expand(rect, radius);

            if (RectContainsPoint(expanded, a) || RectContainsPoint(expanded, b))
                return true;

            Rect segBounds = Rect.MinMaxRect(
                Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y),
                Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));

            if (!RectOverlap(segBounds, expanded))
                return false;

            Vector2 p1 = new Vector2(expanded.xMin, expanded.yMin);
            Vector2 p2 = new Vector2(expanded.xMax, expanded.yMin);
            Vector2 p3 = new Vector2(expanded.xMax, expanded.yMax);
            Vector2 p4 = new Vector2(expanded.xMin, expanded.yMax);

            return LinesIntersect(a, b, p1, p2) ||
                   LinesIntersect(a, b, p2, p3) ||
                   LinesIntersect(a, b, p3, p4) ||
                   LinesIntersect(a, b, p4, p1);
        }

        public static bool SectorOverlapRect(Vector2 center, float radius, float directionDegrees, float angleDegrees, Rect rect)
        {
            if (!CircleOverlapRect(center, radius, rect))
                return false;

            if (angleDegrees >= 359.9f)
                return true;

            Vector2 c = rect.center;
            if (PointInSector(center, radius, directionDegrees, angleDegrees, c))
                return true;

            if (PointInSector(center, radius, directionDegrees, angleDegrees, new Vector2(rect.xMin, rect.yMin))) return true;
            if (PointInSector(center, radius, directionDegrees, angleDegrees, new Vector2(rect.xMin, rect.yMax))) return true;
            if (PointInSector(center, radius, directionDegrees, angleDegrees, new Vector2(rect.xMax, rect.yMin))) return true;
            if (PointInSector(center, radius, directionDegrees, angleDegrees, new Vector2(rect.xMax, rect.yMax))) return true;

            Vector2 dir = AngleToVector(directionDegrees);
            Vector2 edgeA = AngleToVector(directionDegrees - angleDegrees * 0.5f) * radius + center;
            Vector2 edgeB = AngleToVector(directionDegrees + angleDegrees * 0.5f) * radius + center;
            return SegmentOverlapRect(center, edgeA, rect, 0f) || SegmentOverlapRect(center, edgeB, rect, 0f) ||
                   SegmentOverlapRect(center, center + dir * radius, rect, 0f);
        }

        public static bool PointInSector(Vector2 center, float radius, float directionDegrees, float angleDegrees, Vector2 point)
        {
            Vector2 to = point - center;
            if (to.sqrMagnitude > radius * radius)
                return false;

            if (to.sqrMagnitude <= 0.000001f)
                return true;

            Vector2 dir = AngleToVector(directionDegrees);
            float half = angleDegrees * 0.5f;
            float angle = Vector2.Angle(dir, to);
            return angle <= half;
        }

        public static Vector2 AngleToVector(float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        public static float DistanceSqrPointToRect(Vector2 point, Rect rect)
        {
            float dx = 0f;
            if (point.x < rect.xMin) dx = rect.xMin - point.x;
            else if (point.x > rect.xMax) dx = point.x - rect.xMax;

            float dy = 0f;
            if (point.y < rect.yMin) dy = rect.yMin - point.y;
            else if (point.y > rect.yMax) dy = point.y - rect.yMax;

            return dx * dx + dy * dy;
        }

        private static bool LinesIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float o1 = Orientation(a, b, c);
            float o2 = Orientation(a, b, d);
            float o3 = Orientation(c, d, a);
            float o4 = Orientation(c, d, b);

            if (o1 * o2 < 0f && o3 * o4 < 0f)
                return true;

            const float eps = 0.000001f;
            if (Mathf.Abs(o1) < eps && OnSegment(a, c, b)) return true;
            if (Mathf.Abs(o2) < eps && OnSegment(a, d, b)) return true;
            if (Mathf.Abs(o3) < eps && OnSegment(c, a, d)) return true;
            if (Mathf.Abs(o4) < eps && OnSegment(c, b, d)) return true;

            return false;
        }

        private static float Orientation(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }

        private static bool OnSegment(Vector2 a, Vector2 p, Vector2 b)
        {
            return p.x >= Mathf.Min(a.x, b.x) && p.x <= Mathf.Max(a.x, b.x) &&
                   p.y >= Mathf.Min(a.y, b.y) && p.y <= Mathf.Max(a.y, b.y);
        }
    }
}
