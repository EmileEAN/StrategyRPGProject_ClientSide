using EEANWorks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace EEANWorks.Games.TBSG._01
{
    public sealed class Tag
    {
        private Tag(string _string, Tag _parent = null)
        {
            String = _string.CoalesceNullAndReturnCopyOptionally(true);

            m_childrenTags = new List<Tag>();

            ParentTag = _parent;
        }

        #region Properties
        public Tag ParentTag { get; }

        public string String { get; }

        public IList<Tag> ChildrenTags { get { return m_childrenTags.AsReadOnly(); } }

        public string HierarchyString { get { return HierarchizeString(); } } //Method Wrapper

        public static Tag Zero { get; } = NewTag("<#0/>");
        public static Tag One { get; } = NewTag("<#1/>");
        #endregion

        #region Private Read-only Fields
        private readonly List<Tag> m_childrenTags;
        #endregion

        #region Public Methods
        public static Tag NewTag(string _tagString)
        {
            if (_tagString == null)
                return new Tag(null);

            List<string> tagStrings = DivideIntoTagStrings(_tagString);

            if (tagStrings.Count < 1)
                return new Tag(null);

            Tag t = new Tag(tagStrings[0]); //First tag will always be the root tag

            Tag currentTag = t;

            for (int i = 2; i <= tagStrings.Count; i++)
            {
                if (!tagStrings[i - 1].StartsWith("</"))
                {
                    if (tagStrings[i - 1].EndsWith("/>"))
                        currentTag.m_childrenTags.Add(new Tag(tagStrings[i - 1].Substring(0, tagStrings[i - 1].Length - 2) + ">", currentTag)); //Remove the "/"
                    else
                        currentTag.m_childrenTags.Add(new Tag(tagStrings[i - 1], currentTag));

                    if (!tagStrings[i - 1].EndsWith("/>"))
                        currentTag = currentTag.m_childrenTags.Last();
                }
                else
                    currentTag = currentTag.ParentTag;
            }

            return t;
        }

        public T ToValue<T>(BattleSystemCore _system, UnitInstance _effectHolder = null, StatusEffect _statusEffect = null, UnitInstance _actor = null, Skill _skill = null, Effect _effect = null, List<_2DCoord> _effectRange = null, List<object> _targets = null, object _target = null, List<object> _secondaryTargetsForComplexTargetSelectionEffect = null, int _targetPreviousHP = 0, int _targetPreviousLocationTileIndex = 0, List<StatusEffect> _statusEffects = null, UnitInstance _effectHolderOfActivatedEffect = null, StatusEffect _statusEffectActivated = null, eTileType _previousTileType = default)
        {
            try
            {
                object result;

                if (m_childrenTags.Count != 0)
                    result = TranslateFunctionTagToValue(_system);
                else
                    result = TranslateSimpleValueTagToValue(_effectHolder, _statusEffect, _actor, _skill, _effect, _effectRange, _targets, _target, _secondaryTargetsForComplexTargetSelectionEffect, _targetPreviousHP, _targetPreviousLocationTileIndex, _statusEffects, _effectHolderOfActivatedEffect, _statusEffectActivated, _previousTileType);

                if (result == null)
                    return default;

                if (result is T)
                    return (T)result;
                else if (result.GetType().IsClass)
                    return (T)(result as object);
                else
                    return (T)Convert.ChangeType(result, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Debug.Log("At Tag.ToValue<T>(): " + ex.Message);
                return default;
            }
        }

        public string ToFormattedString(int _level)
        {
            try
            {
                return (m_childrenTags.Count != 0) ? TranslateFunctionTagToFormattedString(_level) : TranslateSimpleValueTagToFormattedString(_level);
            }
            catch (Exception ex)
            {
                Debug.Log("At Tag.ToFormattedString(): " + ex.Message);
                return "";
            }
        }
        #endregion

        #region Private Methods
        private static List<string> DivideIntoTagStrings(string _tagString)
        {
            if (_tagString == null)
                return new List<string>();

            string tagStringCopy = string.Copy(_tagString); //copy value to not accidentally modify the original string

            List<string> tagStrings = new List<string>();

            while (tagStringCopy != "")
            {
                int countCharsInTag = 1; //the first character '<'

                for (int i = 2; i <= tagStringCopy.Length; i++)
                {
                    //ignore the first character in commandCopy which should be '<'
                    if (tagStringCopy[i - 1] != '<')
                        countCharsInTag++;
                    else
                        break;
                }

                tagStrings.Add(tagStringCopy.Substring(0, countCharsInTag));
                tagStringCopy = tagStringCopy.Substring(countCharsInTag);
            }

            return tagStrings;
        }

        private object TranslateFunctionTagToValue(BattleSystemCore _system)
        {
            List<object> tagValues = new List<object>();

            foreach (Tag tag in m_childrenTags)
            {
                tagValues.Add(tag.ToValue<object>(_system));
            }

            object result;

            try
            {
                switch (String)
                {
                    #region return object
                    case "<$GetFirstOrDefault>":
                        {
                            if (tagValues[0] is IEnumerable)
                                result = ((IEnumerable)tagValues[0]).Cast<object>().FirstOrDefault();
                            else
                                result = null;
                        }
                        break;
                    case "<$MergeListsWithoutDuplicates>":
                        {
                            List<object> list = new List<object>();
                            foreach (object tagValue in tagValues)
                            {
                                if (!(tagValue is IEnumerable))
                                {
                                    list.Clear();
                                    break;
                                }

                                list.AddRange(((IEnumerable)tagValues[0]).Cast<object>());
                            }

                            result = list.Distinct().ToList();
                        }
                        break;
                    #endregion

                    #region return decimal
                    case "<$Sum>":
                        {
                            result = tagValues[0];
                            for (int i = 1; i < tagValues.Count; i++)
                            {
                                result = CoreFunctions.Sum(result, tagValues[i]);
                            }
                            result = Convert.ToDecimal(result);
                        }
                        break;
                    case "<$Subtract>":
                        {
                            result = tagValues[0];
                            for (int i = 1; i < tagValues.Count; i++)
                            {
                                result = CoreFunctions.Subtract(result, tagValues[i]);
                            }
                            result = Convert.ToDecimal(result);
                        }
                        break;
                    case "<$Multiply>":
                        {
                            result = tagValues[0];
                            for (int i = 1; i < tagValues.Count; i++)
                            {
                                result = CoreFunctions.Multiply(result, tagValues[i]);
                            }
                            result = Convert.ToDecimal(result);
                        }
                        break;
                    case "<$Divide>":
                        {
                            result = tagValues[0];
                            for (int i = 1; i < tagValues.Count; i++)
                            {
                                result = CoreFunctions.Divide(result, tagValues[i]);
                            }
                            result = Convert.ToDecimal(result);
                        }
                        break;
                    #endregion

                    #region return int
                    case "<$Count>":
                        result = (tagValues[0] as List<object>).Count;
                        break;
                    case "<$GetLevel>":
                        result = Calculator.Level(tagValues[0] as Unit);
                        break;
                    case "<$GetMaxHP>":
                        result = Calculator.MaxHP(tagValues[0] as Unit);
                        break;
                    case "<$GetPhyStr>":
                        result = Calculator.PhysicalStrength(tagValues[0] as Unit);
                        break;
                    case "<$GetPhyRes>":
                        result = Calculator.PhysicalResistance(tagValues[0] as Unit);
                        break;
                    case "<$GetMagStr>":
                        result = Calculator.MagicalStrength(tagValues[0] as Unit);
                        break;
                    case "<$GetMagRes>":
                        result = Calculator.MagicalResistance(tagValues[0] as Unit);
                        break;
                    case "<$GetVit>":
                        result = Calculator.Vitality(tagValues[0] as Unit);
                        break;
                    #endregion

                    #region return bool
                    case "<$ContainsElement>":
                        {
                            if (tagValues.Count < 2)
                                result = false;
                            else
                                result = (tagValues[0] as Unit).BaseInfo.Elements.Contains((eElement)tagValues[1]);
                        }
                        break;
                    #endregion

                    #region return List<UnitInstance>
                    case "<$FindUnitsByName>":
                        {
                            if (tagValues.Count < 3)
                                result = null;
                            else if (tagValues.Count == 3)
                                result = _system.FindUnitsByName(tagValues[0] as string, (bool)tagValues[1], (eStringMatchType)tagValues[2]);
                            else
                                result = _system.FindUnitsByName(tagValues[0] as string, (bool)tagValues[1], (eStringMatchType)tagValues[2], tagValues[3] as List<UnitInstance>);
                        }
                        break;
                    case "<$FindUnitsByLabel>":
                        {
                            if (tagValues.Count < 2)
                                result = null;
                            else if (tagValues.Count == 2)
                                result = _system.FindUnitsByLabel(tagValues[0] as string, (bool)tagValues[1]);
                            else
                                result = _system.FindUnitsByLabel(tagValues[0] as string, (bool)tagValues[1], tagValues[2] as List<UnitInstance>);
                        }
                        break;
                    case "<$FindUnitsByGender>":
                        {
                            if (tagValues.Count < 3)
                                result = null;
                            else if (tagValues.Count == 3)
                                result = _system.FindUnitsByGender((eGender)tagValues[0], (bool)tagValues[1]);
                            else
                                result = _system.FindUnitsByGender((eGender)tagValues[0], (bool)tagValues[1], tagValues[2] as List<UnitInstance>);
                        }
                        break;
                    case "<$FindUnitsByElement>":
                        {
                            if (tagValues.Count < 2)
                                result = null;
                            else if (tagValues.Count == 2)
                                result = _system.FindUnitsByElement((eElement)tagValues[0], (bool)tagValues[1]);
                            else if (tagValues.Count == 3)
                                result = _system.FindUnitsByElement((eElement)tagValues[0], (bool)tagValues[1], (eElement)tagValues[2]);
                            else
                                result = _system.FindUnitsByElement((eElement)tagValues[0], (bool)tagValues[1], (eElement)tagValues[2], tagValues[3] as List<UnitInstance>);
                        }
                        break;
                    case "<$FindUnitsByStatusValue>":
                        {
                            if (tagValues.Count < 3)
                                result = null;
                            else if (tagValues.Count == 3)
                                result = _system.FindUnitsByStatusValue((eUnitStatusType)tagValues[0], (eRelationType)tagValues[1], (int)tagValues[2]);
                            else
                                result = _system.FindUnitsByStatusValue((eUnitStatusType)tagValues[0], (eRelationType)tagValues[1], (int)tagValues[2], tagValues[3] as List<UnitInstance>);
                        }
                        break;
                    case "<$FindUnitsByStatusValueRanking>":
                        {
                            if (tagValues.Count < 3)
                                result = null;
                            else if (tagValues.Count == 3)
                                result = _system.FindUnitsByStatusValueRanking((eUnitStatusType)tagValues[0], (eSortType)tagValues[1], (int)tagValues[2]);
                            else
                                result = _system.FindUnitsByStatusValueRanking((eUnitStatusType)tagValues[0], (eSortType)tagValues[1], (int)tagValues[2], tagValues[3] as List<UnitInstance>);
                        }
                        break;
                    case "<$FindUnitsByTargetClassification>":
                        {
                            if (tagValues.Count < 2)
                                result = null;
                            else if (tagValues.Count == 2)
                                result = _system.FindUnitsByTargetClassification(tagValues[0] as UnitInstance, (eTargetUnitClassification)tagValues[1]);
                            else
                                result = _system.FindUnitsByTargetClassification(tagValues[0] as UnitInstance, (eTargetUnitClassification)tagValues[1], tagValues[2] as List<_2DCoord>);
                        }
                        break;
                    #endregion

                    default:
                        result = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return result;
        }

        private object TranslateSimpleValueTagToValue(UnitInstance _effectHolder, StatusEffect _statusEffect, UnitInstance _actor, Skill _skill, Effect _effect, List<_2DCoord> _effectRange, List<object> _targets, object _target, List<object> _secondaryTargetsForComplexTargetSelectionEffect, int _targetPreviousHP, int _targetPreviousLocationTileIndex, List<StatusEffect> _statusEffects, UnitInstance _effectHolderOfActivatedEffect, StatusEffect _statusEffectActivated, eTileType _previousTileType)
        {
            try
            {
                if (String.StartsWith("<#")) // Number
                {
                    string tagStringCopy = string.Copy(String);

                    tagStringCopy = tagStringCopy.Substring(2, tagStringCopy.Length - 4); // Remove "<#" and "/>"

                    return Convert.ToDecimal(tagStringCopy);
                }
                else if (String.StartsWith("<S=")) // String
                {
                    string tagStringCopy = string.Copy(String);

                    tagStringCopy = tagStringCopy.Substring(3, tagStringCopy.Length - 5); // Remove "<S=" and "/>"

                    return tagStringCopy;
                }
                else if (String.StartsWith("<E=")) // Enum
                {
                    string tagStringCopy = string.Copy(String);

                    tagStringCopy = tagStringCopy.Substring(3, tagStringCopy.Length - 5); // Remove "<E=" and "/>"

                    string[] enumTypeAndValueString = tagStringCopy.Split('.'); // Will be separated into enum type and enum value.

                    return enumTypeAndValueString[1].ToCorrespondingEnumValue(enumTypeAndValueString[0]);
                }
                else if (String == "<True/>") return true;
                else if (String == "<False/>") return false;
                else if (String == "<EffectHolder/>") return _effectHolder;
                else if (String == "<EffectUser/>") return _actor;
                else if (String == "<StatusEffectOriginSkillLevel/>") return _statusEffects[0].OriginSkillLevel;
                else if (String == "<StatusEffectEquipmentLevel/>") return _statusEffects[0].EquipmentLevel;
                else if (String == "<Skill/>") return _skill;
                else if (String == "<SkillName/>") return _skill.BaseInfo.Name;
                else if (String == "<SkillLevel/>") return _skill.Level;
                else if (String == "<Effect/>") return _effect;
                else if (String == "<EffectRange/>") return _effectRange;
                else if (String == "<Target/>") return _target;
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string HierarchizeString(string _s = "", int _counter = 0)
        {
            if (_counter != 0)
            {
                _s += "\n";
                for (int i = 0; i < _counter; i++)
                {
                    _s += "\t";
                }
            }

            _s += this.String;

            _counter++;
            foreach (Tag t in this.ChildrenTags) { _s += HierarchizeString(_s, _counter); }

            return _s;
        }

        private string TranslateFunctionTagToFormattedString(int _level)
        {
            List<string> tagStrings = new List<string>();

            foreach (Tag tag in m_childrenTags)
            {
                tagStrings.Add(tag.ToFormattedString(_level));
            }

            string result = string.Empty;

            try
            {
                switch (String)
                {
                    #region object string
                    case "<$GetFirstOrDefault>":
                        result = "Get first or default from " + tagStrings[0];
                        break;
                    case "<$MergeListsWithoutDuplicates>":
                        {
                            result = "Following lists merged without duplicates ";
                            foreach (string tagString in tagStrings)
                            {
                                result += "[" + tagString + "]";
                            }
                        }
                        break;
                    #endregion

                    #region decimal string
                    case "<$Sum>":
                        for (int i = 0; i < tagStrings.Count; i++)
                        {
                            if (i != 0)
                                result += " + ";

                            result += tagStrings[i];
                        }
                        break;
                    case "<$Subtract>":
                        for (int i = 0; i < tagStrings.Count; i++)
                        {
                            if (i != 0)
                                result += " - ";

                            result += tagStrings[i];
                        }
                        break;
                    case "<$Multiply>":
                        for (int i = 0; i < tagStrings.Count; i++)
                        {
                            if (i != 0)
                                result += " x ";

                            result += tagStrings[i];
                        }
                        break;
                    case "<$Divide>":
                        for (int i = 0; i < tagStrings.Count; i++)
                        {
                            if (i != 0)
                                result += " / ";

                            result += tagStrings[i];
                        }
                        break;
                    #endregion

                    #region int string
                    case "<$Count>":
                        result = "Number of elements in " + tagStrings[0];
                        break;
                    case "<$GetLevel>":
                        result = "Level of " + tagStrings[0];
                        break;
                    case "<$GetMaxHP>":
                        result = "Max HP of " + tagStrings[0];
                        break;
                    case "<$GetPhyStr>":
                        result = "Physical Strength of " + tagStrings[0];
                        break;
                    case "<$GetPhyRes>":
                        result = "Physical Resistance of " + tagStrings[0];
                        break;
                    case "<$GetMagStr>":
                        result = "Magical Strength of " + tagStrings[0];
                        break;
                    case "<$GetMagRes>":
                        result = "Magical Resistance of " + tagStrings[0];
                        break;
                    case "<$GetVit>":
                        result = "Vitality of " + tagStrings[0];
                        break;
                    #endregion

                    #region bool string
                    case "<$ContainsElement>":
                        result = "If " + tagStrings[0] + "'s element is " + tagStrings[1];
                        break;
                    #endregion

                    #region List<UnitInstance> string
                    case "<$FindUnitsByName>":
                        if (tagStrings.Count >= 3)
                        {
                            bool negateStringMatchType = Convert.ToBoolean(tagStrings[1]);

                            string listSpecifyingString = (tagStrings.Count > 3) ? "within [" + tagStrings[3] + "]" : "";

                            switch (tagStrings[2])
                            {
                                case "ExactMatch":
                                    result = "Units" + listSpecifyingString + " that are " + (negateStringMatchType ? "not " : "") + "named <b>⟪" + tagStrings[0] + "⟫</b>";
                                    break;
                                case "Contains":
                                    result = "Units" + listSpecifyingString + " that " + (negateStringMatchType ? "do not " : "") + "contain <b>⟪" + tagStrings[0] + "⟫</b> in its name";
                                    break;
                                case "StartsWith":
                                    result = "Units" + listSpecifyingString + " which name " + (negateStringMatchType ? "does not start " : "starts ") + "with <b>⟪" + tagStrings[0] + "⟫</b>";
                                    break;
                                case "EndsWith":
                                    result = "Units" + listSpecifyingString + " which name " + (negateStringMatchType ? "does not end " : "ends ") + "with <b>⟪" + tagStrings[0] + "⟫</b>";
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case "<$FindUnitsByLabel>":
                        if (tagStrings.Count >= 2)
                        {
                            bool excludeLabel = Convert.ToBoolean(tagStrings[1]);

                            string listSpecifyingString = (tagStrings.Count > 2) ? "within [" + tagStrings[2] + "]" : "";

                            result = "Units" + listSpecifyingString + " that are " + (excludeLabel ? "not " : "") + tagStrings[0];
                        }
                        break;
                    case "<$FindUnitsByGender>":
                        if (tagStrings.Count >= 2)
                        {
                            bool excludeGender = Convert.ToBoolean(tagStrings[1]);

                            string listSpecifyingString = (tagStrings.Count > 2) ? "within [" + tagStrings[2] + "]" : "";

                            result = "Units" + listSpecifyingString + " that are " + (excludeGender ? "not " : "") + tagStrings[0];
                        }
                        break;
                    case "<$FindUnitsByElement>":
                        if (tagStrings.Count >= 2)
                        {
                            bool excludeElement = Convert.ToBoolean(tagStrings[1]);

                            string secondElementString = (tagStrings.Count > 2) ? " and " + tagStrings[2] : "";

                            string listSpecifyingString = (tagStrings.Count > 3) ? "within [" + tagStrings[3] + "]" : "";

                            result = (excludeElement ? "Non-" : "") + "< " + tagStrings[0] + secondElementString + "> Units" + listSpecifyingString;
                        }
                        break;
                    case "<$FindUnitsByStatusValue>":
                        if (tagStrings.Count >= 3)
                        {
                            string relationString = string.Empty;
                            switch (tagStrings[1])
                            {
                                case "EqualTo":
                                    relationString = " is ";
                                    break;
                                case "NotEqualTo":
                                    relationString = " is not ";
                                    break;
                                case "GreaterThan":
                                    relationString = " > ";
                                    break;
                                case "LessThan":
                                    relationString = " < ";
                                    break;
                                case "GreaterThanOrEqualTo":
                                    relationString = " ≧ ";
                                    break;
                                case "LessThanOrEqualTo":
                                    relationString = " ≦ ";
                                    break;
                                default:
                                    break;
                            }

                            string listSpecifyingString = (tagStrings.Count > 3) ? "within [" + tagStrings[3] + "]" : "";

                            result = "Units " + listSpecifyingString + " which " + tagStrings[0] + relationString + tagStrings[2];
                        }
                        break;
                    case "<$FindUnitsByStatusValueRanking>":
                        if (tagStrings.Count >= 3)
                        {
                            string rankingTypeString = (tagStrings[1] == "Descending") ? " highest " : " lowest ";

                            string listSpecifyingString = (tagStrings.Count > 3) ? "within [" + tagStrings[3] + "]" : "";

                            result = "Unit(s) " + listSpecifyingString + " with the " + Convert.ToInt32(tagStrings[2]).ToOrdinal() + rankingTypeString + tagStrings[0];
                        }
                        break;
                    case "<$FindUnitsByTargetClassification>":
                        if (tagStrings.Count >= 2)
                        {
                            result = tagStrings[1];
                        }
                        break;
                    #endregion

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                return "";
            }

            return result;
        }

        private string TranslateSimpleValueTagToFormattedString(int _level)
        {
            try
            {
                if (String.StartsWith("<#")) // Number
                {
                    string tagStringCopy = string.Copy(String);

                    tagStringCopy = tagStringCopy.Substring(2, tagStringCopy.Length - 4); // Remove "<#" and "/>"

                    return tagStringCopy;
                }
                else if (String.StartsWith("<S=")) // String
                {
                    string tagStringCopy = string.Copy(String);

                    tagStringCopy = tagStringCopy.Substring(3, tagStringCopy.Length - 5); // Remove "<S=" and "/>"

                    return tagStringCopy;
                }
                else if (String.StartsWith("<E=")) // Enum
                {
                    string tagStringCopy = string.Copy(String);

                    tagStringCopy = tagStringCopy.Substring(3, tagStringCopy.Length - 5); // Remove "<E=" and "/>"

                    string[] enumTypeAndValueString = tagStringCopy.Split('.'); // Will be separated into enum type and enum value.

                    return "<b><" + enumTypeAndValueString[1] + "></b>";
                }
                else if (String == "<StatusEffectOriginSkillLevel/>" || String == "<SkillLevel/>")
                    return (_level > 0) ? _level.ToString() : "<Skill Level>";
                else
                    return String.Substring(1, String.Length - 3); // Remove "<" and "/>"
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion
    }
}
