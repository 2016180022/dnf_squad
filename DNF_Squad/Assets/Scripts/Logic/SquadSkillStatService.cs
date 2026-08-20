using System.Collections.Generic;
using System.Linq;
using DnfSquad.Data;
using UnityEngine;

namespace DnfSquad.Logic
{
    /// <summary>
    /// 스쿼드 버프를 반영한 스쿼드 스킬의 최종 수치와 설명문을 계산한다.
    /// 스킬 정보 목록 / 퀵슬롯 등 스킬을 표시하는 모든 UI가 이 서비스를 공유해
    /// 표시가 어긋나지 않도록 한다.
    /// </summary>
    public static class SquadSkillStatService
    {
        public const string HighlightColor = "#00FF00";
        // BaseSkillLevel 삭제 — 스킬 레벨 가변 표기 기능 폐기로 미사용

        /// <summary>해당 스킬을 대상으로 하고, 현재 1레벨 이상 습득한 버프들의 효과를 모은다</summary>
        private static IEnumerable<(SquadBuffEffect effect, float value, SquadBuffData buff, int level)>
            CollectEffects(SquadRuntimeData data, string skillId)
        {
            foreach (var buff in data.squadBuffs)
            {
                if (buff.effects == null) continue;

                int level = data.GetBuffLevel(buff.buffId);
                if (level <= 0) continue;

                // levels[0]이 1레벨 데이터이므로 현재 레벨의 인덱스는 level - 1
                if (buff.levels == null || level - 1 >= buff.levels.Length) continue;
                var levelData = buff.levels[level - 1];

                foreach (var effect in buff.effects)
                {
                    if (effect.targetSkillId != skillId) continue;
                    if (levelData.effectValues == null) continue;
                    if (effect.effectValueIndex >= levelData.effectValues.Length) continue;

                    yield return (effect, levelData.effectValues[effect.effectValueIndex], buff, level);
                }
            }
        }

        private static float Apply(float current, float value, BuffOperation operation)
        {
            switch (operation)
            {
                case BuffOperation.Add: return current + value;
                case BuffOperation.Subtract: return current - value;
                case BuffOperation.Multiply: return current * value;
                case BuffOperation.Override: return value;
                default: return current;
            }
        }

        /// <summary>버프를 반영한 설명문 수치 배열</summary>
        public static float[] GetFinalValues(SquadRuntimeData data, string skillId)
        {
            var skill = data.GetSkill(skillId);
            if (skill?.baseValues == null) return new float[0];

            var values = (float[])skill.baseValues.Clone();

            foreach (var (effect, value, _, _) in CollectEffects(data, skillId))
            {
                if (effect.targetType != SkillTargetType.DescriptionValue) continue;
                if (effect.targetValueIndex < 0 || effect.targetValueIndex >= values.Length) continue;

                values[effect.targetValueIndex] = Apply(values[effect.targetValueIndex], value, effect.operation);
            }

            return values;
        }

        /// <summary>버프를 반영한 최종 쿨타임</summary>
        public static float GetFinalCooldown(SquadRuntimeData data, string skillId)
        {
            var skill = data.GetSkill(skillId);
            if (skill == null) return 0f;

            float cooldown = skill.cooldownSeconds;

            foreach (var (effect, value, _, _) in CollectEffects(data, skillId))
            {
                if (effect.targetType != SkillTargetType.Cooldown) continue;
                cooldown = Apply(cooldown, value, effect.operation);
            }

            return cooldown;
        }

        /// <summary>버프를 반영한 최종 사용 횟수 (0 = 무제한/미표시)</summary>
        public static int GetFinalMaxUses(SquadRuntimeData data, string skillId)
        {
            var skill = data.GetSkill(skillId);
            if (skill == null) return 0;

            float uses = skill.maxUsesPerRaid;

            foreach (var (effect, value, _, _) in CollectEffects(data, skillId))
            {
                if (effect.targetType != SkillTargetType.MaxUses) continue;
                uses = Apply(uses, value, effect.operation);
            }

            return Mathf.RoundToInt(uses);
        }

        // GetFinalSkillLevel / BuildSkillLevelText 삭제 — 스킬 레벨 가변 표기 기능 폐기
        // (스킬 기본 레벨 1과 버프 기본 레벨 0이 맞지 않아 표기가 혼란스러움)

        /// <summary>
        /// 최종 설명문. 기본값과 달라진 수치는 강조색으로 표시하고,
        /// 현재 레벨까지 달성한 버프의 부가 설명을 뒤에 이어붙인다.
        /// </summary>
        public static string BuildSkillDescription(SquadRuntimeData data, string skillId)
        {
            var skill = data.GetSkill(skillId);
            if (skill == null || string.IsNullOrEmpty(skill.descriptionTemplate)) return string.Empty;

            var baseValues = skill.baseValues ?? new float[0];
            var finalValues = GetFinalValues(data, skillId);

            var args = new object[finalValues.Length];
            for (int i = 0; i < finalValues.Length; i++)
            {
                string text = finalValues[i].ToString("0.##");
                bool changed = i >= baseValues.Length || !Mathf.Approximately(baseValues[i], finalValues[i]);

                args[i] = changed ? $"<color={HighlightColor}>{text}</color>" : text;
            }

            var result = string.Format(skill.descriptionTemplate, args);
            result += BuildSuffixLine(data, skill);
            result += BuildBonusLines(data, skillId);

            return result;
        }

        /// <summary>
        /// 설명문 뒤에 붙는 괄호 문구를 조립한다.
        /// 순서는 쿨타임 → 사용 횟수 → 사용 조건으로 고정하며,
        /// 값이 없는 항목은 건너뛰고 셋 다 없으면 괄호를 붙이지 않는다.
        /// </summary>
        private static string BuildSuffixLine(SquadRuntimeData data, SquadSkillData skill)
        {
            var parts = new List<string>();

            float finalCooldown = GetFinalCooldown(data, skill.skillId);
            if (finalCooldown > 0f)
            {
                string text = finalCooldown.ToString("0.##");
                if (!Mathf.Approximately(skill.cooldownSeconds, finalCooldown))
                {
                    text = $"<color={HighlightColor}>{text}</color>"; // 버프로 변경됨
                }
                parts.Add($"쿨타임 {text}초");
            }

            int finalUses = GetFinalMaxUses(data, skill.skillId);
            if (finalUses > 0)
            {
                string text = finalUses.ToString();
                if (skill.maxUsesPerRaid != finalUses)
                {
                    text = $"<color={HighlightColor}>{text}</color>"; // 버프로 변경됨
                }
                parts.Add($"최대 {text}회 사용 가능");
            }

            if (!string.IsNullOrEmpty(skill.usableConditionNote))
            {
                parts.Add(skill.usableConditionNote);
            }

            return parts.Count == 0 ? string.Empty : $"\n({string.Join(", ", parts)})";
        }

        /// <summary>
        /// 달성한 레벨까지의 부가 설명만 모아 반환.
        /// 버프 Desc와 달리 스킬 Desc에서는 이미 달성한 것만 노출한다.
        /// </summary>
        private static string BuildBonusLines(SquadRuntimeData data, string skillId)
        {
            var lines = new List<string>();

            foreach (var buff in data.squadBuffs)
            {
                if (buff.effects == null || buff.levels == null) continue;
                if (!buff.effects.Any(e => e.targetSkillId == skillId)) continue;

                int level = data.GetBuffLevel(buff.buffId);

                for (int i = 0; i < level && i < buff.levels.Length; i++)
                {
                    var bonus = buff.levels[i].bonusDescriptionTemplate;
                    if (string.IsNullOrEmpty(bonus)) continue;

                    lines.Add($"<color={HighlightColor}>{bonus}</color>");
                }
            }

            return lines.Count == 0 ? string.Empty : "\n" + string.Join("\n", lines);
        }
    }
}
