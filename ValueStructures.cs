using System;

namespace EEANWorks
{
    [Serializable]
    public struct ValueSet_Float_ElementSize
    {
        public float m_value;
        public eValueTypeForElementSize m_valueType;
    }

    [Serializable]
    public struct ValueSet_Float_HorizontalPadding
    {
        public float m_value;
        public eValueTypeForHorizontalPadding m_valueType;
    }

    [Serializable]
    public struct ValueSet_Float_VerticalPadding
    {
        public float m_value;
        public eValueTypeForVerticalPadding m_valueType;
    }

    [Serializable]
    public struct ValueSet_Float_HorizontalSpacing
    {
        public float m_value;
        public eValueTypeForHorizontalSpacing m_valueType;
    }

    [Serializable]
    public struct ValueSet_Float_VerticalSpacing
    {
        public float m_value;
        public eValueTypeForVerticalSpacing m_valueType;
    }

    public enum eValueTypeForElementSize
    {
        Fixed,
        RelativeToParent,
        RelativeToTheOtherSide
    }

    public enum eValueTypeForHorizontalPadding
    {
        Fixed,
        RelativeToParent,
        RelativeToElement,
        RelativeToPaddingTop,
        RelativeToPaddingBottom
    }

    public enum eValueTypeForVerticalPadding
    {
        Fixed,
        RelativeToParent,
        RelativeToElement,
        RelativeToPaddingLeft,
        RelativeToPaddingRight
    }

    public enum eValueTypeForHorizontalSpacing
    {
        Fixed,
        RelativeToParent,
        RelativeToElement,
        RelativeToPaddingLeft,
        RelativeToPaddingRight,
        RelativeToContraryAxisSpacing
    }

    public enum eValueTypeForVerticalSpacing
    {
        Fixed,
        RelativeToParent,
        RelativeToElement,
        RelativeToPaddingTop,
        RelativeToPaddingBottom,
        RelativeToContraryAxisSpacing
    }
}