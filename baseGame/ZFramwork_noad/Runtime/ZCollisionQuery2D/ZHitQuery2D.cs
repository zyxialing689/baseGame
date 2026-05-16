using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Collision2D
{
    public static class ZHitQuery2D
    {
        private static readonly List<ZCollisionBody2D> candidates = new List<ZCollisionBody2D>(128);
        private static Vector2 selectPointForSort;

        public static int HitBoxNonAlloc(
            ZCollisionBody2D attacker,
            Vector2 center,
            Vector2 size,
            ZCollisionLayer targetMask,
            ZTargetRelation relation,
            ZHitRule hitRule,
            List<ZCollisionBody2D> results,
            bool allowSelfTarget = false)
        {
            if (!PrepareResults(results)) return 0;
            Rect attackRect = ZCollisionMath2D.MakeRect(center, size);
            QueryCandidates(attackRect, targetMask);

            for (int i = 0; i < candidates.Count; i++)
            {
                ZCollisionBody2D target = candidates[i];
                if (!PassCommonHit(attacker, target, relation, allowSelfTarget)) continue;
                if (HitRuleByBox(attacker, target, hitRule, attackRect)) results.Add(target);
            }

            return results.Count;
        }

        public static int HitCircleNonAlloc(
            ZCollisionBody2D attacker,
            Vector2 center,
            float radius,
            ZCollisionLayer targetMask,
            ZTargetRelation relation,
            ZHitRule hitRule,
            List<ZCollisionBody2D> results,
            bool allowSelfTarget = false)
        {
            if (!PrepareResults(results)) return 0;
            radius = Mathf.Max(0f, radius);
            Rect queryRect = ZCollisionMath2D.MakeRect(center, Vector2.one * radius * 2f);
            QueryCandidates(queryRect, targetMask);

            for (int i = 0; i < candidates.Count; i++)
            {
                ZCollisionBody2D target = candidates[i];
                if (!PassCommonHit(attacker, target, relation, allowSelfTarget)) continue;
                if (HitRuleByCircle(attacker, target, hitRule, center, radius)) results.Add(target);
            }

            return results.Count;
        }

        public static int HitSectorNonAlloc(
            ZCollisionBody2D attacker,
            Vector2 center,
            float radius,
            float directionDegrees,
            float angleDegrees,
            ZCollisionLayer targetMask,
            ZTargetRelation relation,
            ZHitRule hitRule,
            List<ZCollisionBody2D> results,
            bool allowSelfTarget = false)
        {
            if (!PrepareResults(results)) return 0;
            radius = Mathf.Max(0f, radius);
            angleDegrees = Mathf.Clamp(angleDegrees, 0f, 360f);
            Rect queryRect = ZCollisionMath2D.MakeRect(center, Vector2.one * radius * 2f);
            QueryCandidates(queryRect, targetMask);

            for (int i = 0; i < candidates.Count; i++)
            {
                ZCollisionBody2D target = candidates[i];
                if (!PassCommonHit(attacker, target, relation, allowSelfTarget)) continue;
                if (HitRuleBySector(attacker, target, hitRule, center, radius, directionDegrees, angleDegrees)) results.Add(target);
            }

            return results.Count;
        }

        public static int ProjectileSegmentNonAlloc(
            ZCollisionBody2D attacker,
            Vector2 oldPos,
            Vector2 newPos,
            float thickness,
            ZCollisionLayer targetMask,
            ZTargetRelation relation,
            ZHitRule hitRule,
            List<ZCollisionBody2D> results,
            bool allowSelfTarget = false)
        {
            if (!PrepareResults(results)) return 0;
            thickness = Mathf.Max(0f, thickness);
            Rect queryRect = Rect.MinMaxRect(
                Mathf.Min(oldPos.x, newPos.x) - thickness,
                Mathf.Min(oldPos.y, newPos.y) - thickness,
                Mathf.Max(oldPos.x, newPos.x) + thickness,
                Mathf.Max(oldPos.y, newPos.y) + thickness);

            QueryCandidates(queryRect, targetMask);

            for (int i = 0; i < candidates.Count; i++)
            {
                ZCollisionBody2D target = candidates[i];
                if (!PassCommonHit(attacker, target, relation, allowSelfTarget)) continue;
                if (HitRuleBySegment(attacker, target, hitRule, oldPos, newPos, thickness)) results.Add(target);
            }

            return results.Count;
        }

        public static int AreaBoxNonAlloc(
            Vector2 center,
            Vector2 size,
            ZCollisionLayer targetMask,
            ZHitRule hitRule,
            List<ZCollisionBody2D> results)
        {
            return HitBoxNonAlloc(null, center, size, targetMask, ZTargetRelation.Any, hitRule, results, true);
        }

        public static int AreaCircleNonAlloc(
            Vector2 center,
            float radius,
            ZCollisionLayer targetMask,
            ZHitRule hitRule,
            List<ZCollisionBody2D> results)
        {
            return HitCircleNonAlloc(null, center, radius, targetMask, ZTargetRelation.Any, hitRule, results, true);
        }

        public static int AreaSectorNonAlloc(
            Vector2 center,
            float radius,
            float directionDegrees,
            float angleDegrees,
            ZCollisionLayer targetMask,
            ZHitRule hitRule,
            List<ZCollisionBody2D> results)
        {
            return HitSectorNonAlloc(null, center, radius, directionDegrees, angleDegrees, targetMask, ZTargetRelation.Any, hitRule, results, true);
        }

        public static int SelectPointNonAlloc(Vector2 point, ZCollisionLayer targetMask, List<ZCollisionBody2D> results)
        {
            if (!PrepareResults(results)) return 0;
            Rect queryRect = ZCollisionMath2D.MakeRect(point, Vector2.one * 0.02f);
            QueryCandidates(queryRect, targetMask);

            for (int i = 0; i < candidates.Count; i++)
            {
                ZCollisionBody2D target = candidates[i];
                if (target == null || !target.canBeSelected) continue;
                if (!target.TryGetSelectBox(out Rect selectRect)) continue;
                if (ZCollisionMath2D.RectContainsPoint(selectRect, point)) results.Add(target);
            }

            selectPointForSort = point;
            results.Sort(CompareSelectResult);
            return results.Count;
        }

        public static ZCollisionBody2D SelectBestPoint(Vector2 point, ZCollisionLayer targetMask, List<ZCollisionBody2D> tempResults)
        {
            int count = SelectPointNonAlloc(point, targetMask, tempResults);
            return count > 0 ? tempResults[0] : null;
        }

        public static int InteractNonAlloc(
            ZCollisionBody2D interactor,
            ZCollisionLayer targetMask,
            ZTargetRelation relation,
            List<ZCollisionBody2D> results,
            bool allowSelfTarget = false)
        {
            if (!PrepareResults(results)) return 0;
            if (interactor == null || !interactor.TryGetInteractBox(out Rect interactorRect)) return 0;

            QueryCandidates(interactorRect, targetMask);

            for (int i = 0; i < candidates.Count; i++)
            {
                ZCollisionBody2D target = candidates[i];
                if (target == null || !target.canBeInteracted) continue;
                if (!PassRelation(interactor, target, relation, allowSelfTarget)) continue;
                if (!target.TryGetInteractBox(out Rect targetRect)) continue;
                if (ZCollisionMath2D.RectOverlap(interactorRect, targetRect)) results.Add(target);
            }

            return results.Count;
        }

        public static ZCollisionBody2D GetNearestInteractTarget(ZCollisionBody2D interactor, List<ZCollisionBody2D> results, int count)
        {
            if (interactor == null || results == null || count <= 0)
                return null;

            Vector2 origin = interactor.Position2D;
            ZCollisionBody2D best = null;
            float bestSqr = float.MaxValue;

            int max = Mathf.Min(count, results.Count);
            for (int i = 0; i < max; i++)
            {
                ZCollisionBody2D target = results[i];
                if (target == null) continue;
                if (!target.TryGetInteractBox(out Rect rect)) continue;

                float sqr = ZCollisionMath2D.DistanceSqrPointToRect(origin, rect);
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = target;
                }
            }

            return best;
        }

        public static int FindTargetsCircleNonAlloc(
            ZCollisionBody2D requester,
            Vector2 center,
            float radius,
            ZCollisionLayer targetMask,
            ZTargetRelation relation,
            ZHitRule hitRule,
            bool requireCanBeHit,
            bool requireCanBeSelected,
            bool requireCanBeInteracted,
            List<ZCollisionBody2D> results,
            bool allowSelfTarget = false)
        {
            if (!PrepareResults(results)) return 0;
            radius = Mathf.Max(0f, radius);
            Rect queryRect = ZCollisionMath2D.MakeRect(center, Vector2.one * radius * 2f);
            QueryCandidates(queryRect, targetMask);

            for (int i = 0; i < candidates.Count; i++)
            {
                ZCollisionBody2D target = candidates[i];
                if (target == null) continue;
                if (requireCanBeHit && !target.canBeHit) continue;
                if (requireCanBeSelected && !target.canBeSelected) continue;
                if (requireCanBeInteracted && !target.canBeInteracted) continue;
                if (!PassRelation(requester, target, relation, allowSelfTarget)) continue;
                if (HitRuleByCircle(requester, target, hitRule, center, radius)) results.Add(target);
            }

            return results.Count;
        }

        private static bool PrepareResults(List<ZCollisionBody2D> results)
        {
            if (results == null)
                return false;
            results.Clear();
            return true;
        }

        private static void QueryCandidates(Rect queryRect, ZCollisionLayer targetMask)
        {
            candidates.Clear();
            if (ZCollisionWorld2D.Instance == null) return;
            ZCollisionWorld2D.Instance.QueryAabbCandidatesNonAlloc(queryRect, targetMask, candidates);
        }

        private static bool PassCommonHit(ZCollisionBody2D attacker, ZCollisionBody2D target, ZTargetRelation relation, bool allowSelfTarget)
        {
            if (target == null || !target.canBeHit)
                return false;
            return PassRelation(attacker, target, relation, allowSelfTarget);
        }

        private static bool PassRelation(ZCollisionBody2D source, ZCollisionBody2D target, ZTargetRelation relation, bool allowSelfTarget)
        {
            if (target == null)
                return false;

            if (source != null && target == source)
            {
                if (!(allowSelfTarget && relation == ZTargetRelation.Ally))
                    return false;
            }

            if (source == null || relation == ZTargetRelation.Any)
                return true;

            switch (relation)
            {
                case ZTargetRelation.Ally:
                    return target.teamId == source.teamId;
                case ZTargetRelation.Enemy:
                    return target.teamId != 0 && target.teamId != source.teamId;
                case ZTargetRelation.Neutral:
                    return target.teamId == 0;
                default:
                    return true;
            }
        }

        private static bool HitRuleByBox(ZCollisionBody2D attacker, ZCollisionBody2D target, ZHitRule rule, Rect attackRect)
        {
            switch (rule)
            {
                case ZHitRule.GroundOnly:
                    return target.TryGetWorldBox(ZBoxType.Ground, out Rect ground) && ZCollisionMath2D.RectOverlap(attackRect, ground);
                case ZHitRule.BodyOnly:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect body) && ZCollisionMath2D.RectOverlap(attackRect, body);
                case ZHitRule.SkyOnly:
                    return target.TryGetWorldBox(ZBoxType.Sky, out Rect sky) && ZCollisionMath2D.RectOverlap(attackRect, sky);
                case ZHitRule.BodyAndGroundMatch:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect tb) && ZCollisionMath2D.RectOverlap(attackRect, tb) && GroundMatch(attacker, target);
                case ZHitRule.BodyAndSkyMatch:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect tb2) && ZCollisionMath2D.RectOverlap(attackRect, tb2) && SkyMatch(attacker, target);
                case ZHitRule.AnyEnabledBox:
                    return AnyBoxOverlapBox(target, attackRect);
                default:
                    return false;
            }
        }

        private static bool HitRuleByCircle(ZCollisionBody2D attacker, ZCollisionBody2D target, ZHitRule rule, Vector2 center, float radius)
        {
            switch (rule)
            {
                case ZHitRule.GroundOnly:
                    return target.TryGetWorldBox(ZBoxType.Ground, out Rect ground) && ZCollisionMath2D.CircleOverlapRect(center, radius, ground);
                case ZHitRule.BodyOnly:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect body) && ZCollisionMath2D.CircleOverlapRect(center, radius, body);
                case ZHitRule.SkyOnly:
                    return target.TryGetWorldBox(ZBoxType.Sky, out Rect sky) && ZCollisionMath2D.CircleOverlapRect(center, radius, sky);
                case ZHitRule.BodyAndGroundMatch:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect tb) && ZCollisionMath2D.CircleOverlapRect(center, radius, tb) && GroundMatch(attacker, target);
                case ZHitRule.BodyAndSkyMatch:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect tb2) && ZCollisionMath2D.CircleOverlapRect(center, radius, tb2) && SkyMatch(attacker, target);
                case ZHitRule.AnyEnabledBox:
                    return AnyBoxOverlapCircle(target, center, radius);
                default:
                    return false;
            }
        }

        private static bool HitRuleBySector(ZCollisionBody2D attacker, ZCollisionBody2D target, ZHitRule rule, Vector2 center, float radius, float directionDegrees, float angleDegrees)
        {
            switch (rule)
            {
                case ZHitRule.GroundOnly:
                    return target.TryGetWorldBox(ZBoxType.Ground, out Rect ground) && ZCollisionMath2D.SectorOverlapRect(center, radius, directionDegrees, angleDegrees, ground);
                case ZHitRule.BodyOnly:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect body) && ZCollisionMath2D.SectorOverlapRect(center, radius, directionDegrees, angleDegrees, body);
                case ZHitRule.SkyOnly:
                    return target.TryGetWorldBox(ZBoxType.Sky, out Rect sky) && ZCollisionMath2D.SectorOverlapRect(center, radius, directionDegrees, angleDegrees, sky);
                case ZHitRule.BodyAndGroundMatch:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect tb) && ZCollisionMath2D.SectorOverlapRect(center, radius, directionDegrees, angleDegrees, tb) && GroundMatch(attacker, target);
                case ZHitRule.BodyAndSkyMatch:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect tb2) && ZCollisionMath2D.SectorOverlapRect(center, radius, directionDegrees, angleDegrees, tb2) && SkyMatch(attacker, target);
                case ZHitRule.AnyEnabledBox:
                    return AnyBoxOverlapSector(target, center, radius, directionDegrees, angleDegrees);
                default:
                    return false;
            }
        }

        private static bool HitRuleBySegment(ZCollisionBody2D attacker, ZCollisionBody2D target, ZHitRule rule, Vector2 oldPos, Vector2 newPos, float thickness)
        {
            switch (rule)
            {
                case ZHitRule.GroundOnly:
                    return target.TryGetWorldBox(ZBoxType.Ground, out Rect ground) && ZCollisionMath2D.SegmentOverlapRect(oldPos, newPos, ground, thickness);
                case ZHitRule.BodyOnly:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect body) && ZCollisionMath2D.SegmentOverlapRect(oldPos, newPos, body, thickness);
                case ZHitRule.SkyOnly:
                    return target.TryGetWorldBox(ZBoxType.Sky, out Rect sky) && ZCollisionMath2D.SegmentOverlapRect(oldPos, newPos, sky, thickness);
                case ZHitRule.BodyAndGroundMatch:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect tb) && ZCollisionMath2D.SegmentOverlapRect(oldPos, newPos, tb, thickness) && GroundMatch(attacker, target);
                case ZHitRule.BodyAndSkyMatch:
                    return target.TryGetWorldBox(ZBoxType.Body, out Rect tb2) && ZCollisionMath2D.SegmentOverlapRect(oldPos, newPos, tb2, thickness) && SkyMatch(attacker, target);
                case ZHitRule.AnyEnabledBox:
                    return AnyBoxOverlapSegment(target, oldPos, newPos, thickness);
                default:
                    return false;
            }
        }

        private static bool GroundMatch(ZCollisionBody2D attacker, ZCollisionBody2D target)
        {
            return attacker != null &&
                   attacker.TryGetWorldBox(ZBoxType.Ground, out Rect aGround) &&
                   target.TryGetWorldBox(ZBoxType.Ground, out Rect tGround) &&
                   ZCollisionMath2D.RectOverlap(aGround, tGround);
        }

        private static bool SkyMatch(ZCollisionBody2D attacker, ZCollisionBody2D target)
        {
            return attacker != null &&
                   attacker.TryGetWorldBox(ZBoxType.Sky, out Rect aSky) &&
                   target.TryGetWorldBox(ZBoxType.Sky, out Rect tSky) &&
                   ZCollisionMath2D.RectOverlap(aSky, tSky);
        }

        private static bool AnyBoxOverlapBox(ZCollisionBody2D target, Rect attackRect)
        {
            for (int i = 0; i < 5; i++)
                if (target.TryGetAnyEnabledBox(i, out Rect r) && ZCollisionMath2D.RectOverlap(attackRect, r)) return true;
            return false;
        }

        private static bool AnyBoxOverlapCircle(ZCollisionBody2D target, Vector2 center, float radius)
        {
            for (int i = 0; i < 5; i++)
                if (target.TryGetAnyEnabledBox(i, out Rect r) && ZCollisionMath2D.CircleOverlapRect(center, radius, r)) return true;
            return false;
        }

        private static bool AnyBoxOverlapSector(ZCollisionBody2D target, Vector2 center, float radius, float directionDegrees, float angleDegrees)
        {
            for (int i = 0; i < 5; i++)
                if (target.TryGetAnyEnabledBox(i, out Rect r) && ZCollisionMath2D.SectorOverlapRect(center, radius, directionDegrees, angleDegrees, r)) return true;
            return false;
        }

        private static bool AnyBoxOverlapSegment(ZCollisionBody2D target, Vector2 a, Vector2 b, float thickness)
        {
            for (int i = 0; i < 5; i++)
                if (target.TryGetAnyEnabledBox(i, out Rect r) && ZCollisionMath2D.SegmentOverlapRect(a, b, r, thickness)) return true;
            return false;
        }

        private static int CompareSelectResult(ZCollisionBody2D a, ZCollisionBody2D b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;

            float ay = a.transform.position.y;
            float by = b.transform.position.y;
            if (!Mathf.Approximately(ay, by))
                return ay < by ? -1 : 1;

            float ad = (a.Position2D - selectPointForSort).sqrMagnitude;
            float bd = (b.Position2D - selectPointForSort).sqrMagnitude;
            return ad.CompareTo(bd);
        }
    }
}
