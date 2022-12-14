using EEANWorks.Games.TBSG._01.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace EEANWorks.Games.Unity.Engine.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class DynamicGridLayoutGroup : MonoBehaviour
    {

        #region Serialized Fields
        [SerializeField]
        private bool m_executeScriptAlways = true;

        [SerializeField]
        private int m_scriptExecutionPriorityLevel = 1; // Lower values have higher priority

        [SerializeField]
        private bool m_fixedNumOfElementsPerRow = default;
        [SerializeField]
        private bool m_fixedNumOfElementsPerColumn = default;

        [SerializeField]
        private int m_elementsPerRow = 1;
        [SerializeField]
        private int m_elementsPerColumn = 1;

        [SerializeField]
        private bool m_horizontallyExpansive = default;
        [SerializeField]
        private bool m_verticallyExpansive = default;

        [SerializeField]
        private bool m_automaticElementWidth = default;
        [SerializeField]
        private bool m_automaticElementHeight = default;

        [SerializeField]
        private ValueSet_Float_ElementSize m_elementSizeX = default;
        [SerializeField]
        private ValueSet_Float_ElementSize m_elementSizeY = default;

        [SerializeField]
        private ValueSet_Float_HorizontalPadding m_paddingLeft = default;
        [SerializeField]
        private ValueSet_Float_HorizontalPadding m_paddingRight = default;
        [SerializeField]
        private ValueSet_Float_VerticalPadding m_paddingBottom = default;
        [SerializeField]
        private ValueSet_Float_VerticalPadding m_paddingTop = default;

        [SerializeField]
        private bool m_automaticSpacingX = default;
        [SerializeField]
        private bool m_automaticSpacingY = default;

        [SerializeField]
        private ValueSet_Float_HorizontalSpacing m_spacingX = default;
        [SerializeField]
        private ValueSet_Float_VerticalSpacing m_spacingY = default;

        [SerializeField]
        private Axis m_startAxis = default;
        [SerializeField]
        private int m_horizontalDirection = default; // { "Left To Right", "Right To Left" }
        [SerializeField]
        private int m_verticalDirection = default; // { "Top To Bottom", "Bottom To Top" }
        [SerializeField]
        private eHorizontalAlignment m_horizontalAlignment = default;
        [SerializeField]
        private eVerticalAlignment m_verticalAlignment = default;
        #endregion

        #region Properties
        public int ScriptExecutionPriorityLevel
        {
            get { return m_scriptExecutionPriorityLevel; }
            set
            {
                if (value > 0)
                {
                    m_scriptExecutionPriorityLevel = value;
                }
            }
        }

        public bool FixedNumOfElementsPerRow { get { return m_fixedNumOfElementsPerRow; } set { if (SetPropertyUtility.SetStruct(ref m_fixedNumOfElementsPerRow, value)) { RefreshGridLayout(); } } }
        public bool FixedNumOfElementsPerColumn { get { return m_fixedNumOfElementsPerColumn; } set { if (SetPropertyUtility.SetStruct(ref m_fixedNumOfElementsPerColumn, value)) { RefreshGridLayout(); } } }

        public int ElementsPerRow
        {
            get { return m_elementsPerRow; }
            set
            {
                if (value != m_elementsPerRow && value > 0)
                {
                    m_elementsPerRow = value;
                    RefreshGridLayout();
                }
            }
        }
        public int ElementsPerColumn
        {
            get { return m_elementsPerColumn; }
            set
            {
                if (value != m_elementsPerColumn && value > 0)
                {
                    m_elementsPerColumn = value;
                    RefreshGridLayout();
                }
            }
        }

        public bool HorizontallyExpansive { get { return m_horizontallyExpansive; } set { if (SetPropertyUtility.SetStruct(ref m_horizontallyExpansive, value)) { RefreshGridLayout(); } } }
        public bool VerticallyExpansive { get { return m_verticallyExpansive; } set { if (SetPropertyUtility.SetStruct(ref m_verticallyExpansive, value)) { RefreshGridLayout(); } } }

        public bool AutomaticElementWidth { get { return m_automaticElementWidth; } set { if (SetPropertyUtility.SetStruct(ref m_automaticElementWidth, value)) { RefreshGridLayout(); } } }
        public bool AutomaticElementHeight { get { return m_automaticElementHeight; } set { if (SetPropertyUtility.SetStruct(ref m_automaticElementHeight, value)) { RefreshGridLayout(); } } }

        public ValueSet_Float_ElementSize ElementSizeX
        {
            get { return m_elementSizeX; }
            set
            {
                if (value.m_value != m_elementSizeX.m_value
                    && value.m_valueType != m_elementSizeX.m_valueType
                    && value.m_value > 0)
                {
                    m_elementSizeX = value;
                    RefreshGridLayout();
                }
            }
        }
        public ValueSet_Float_ElementSize ElementSizeY
        {
            get { return m_elementSizeY; }
            set
            {
                if (value.m_value != m_elementSizeY.m_value
                    && value.m_valueType != m_elementSizeY.m_valueType
                    && value.m_value > 0)
                {
                    m_elementSizeY = value;
                    RefreshGridLayout();
                }
            }
        }

        public ValueSet_Float_HorizontalPadding PaddingLeft
        {
            get { return m_paddingLeft; }
            set
            {
                if (value.m_value != m_paddingLeft.m_value
                    && value.m_valueType != m_paddingLeft.m_valueType
                    && value.m_value > 0)
                {
                    m_paddingLeft = value;
                    RefreshGridLayout();
                }
            }
        }
        public ValueSet_Float_HorizontalPadding PaddingRight
        {
            get { return m_paddingRight; }
            set
            {
                if (value.m_value != m_paddingRight.m_value
                    && value.m_valueType != m_paddingRight.m_valueType
                    && value.m_value > 0)
                {
                    m_paddingRight = value;
                    RefreshGridLayout();
                }
            }
        }
        public ValueSet_Float_VerticalPadding PaddingBottom
        {
            get { return m_paddingBottom; }
            set
            {
                if (value.m_value != m_paddingBottom.m_value
                    && value.m_valueType != m_paddingBottom.m_valueType
                    && value.m_value > 0)
                {
                    m_paddingBottom = value;
                    RefreshGridLayout();
                }
            }
        }
        public ValueSet_Float_VerticalPadding PaddingTop
        {
            get { return m_paddingTop; }
            set
            {
                if (value.m_value != m_paddingTop.m_value
                    && value.m_valueType != m_paddingTop.m_valueType
                    && value.m_value > 0)
                {
                    m_paddingTop = value;
                    RefreshGridLayout();
                }
            }
        }

        public bool AutomaticSpacingX { get { return m_automaticSpacingX; } set { if (SetPropertyUtility.SetStruct(ref m_automaticSpacingX, value)) { RefreshGridLayout(); } } }
        public bool AutomaticSpacingY { get { return m_automaticSpacingY; } set { if (SetPropertyUtility.SetStruct(ref m_automaticSpacingY, value)) { RefreshGridLayout(); } } }

        public ValueSet_Float_HorizontalSpacing SpacingX
        {
            get { return m_spacingX; }
            set
            {
                if (value.m_value != m_spacingX.m_value
                    && value.m_valueType != m_spacingX.m_valueType
                    && value.m_value > 0)
                {
                    m_spacingX = value;
                    RefreshGridLayout();
                }
            }
        }
        public ValueSet_Float_VerticalSpacing SpacingY
        {
            get { return m_spacingY; }
            set
            {
                if (value.m_value != m_spacingY.m_value
                    && value.m_valueType != m_spacingY.m_valueType
                    && value.m_value > 0)
                {
                    m_spacingY = value;
                    RefreshGridLayout();
                }
            }
        }

        public Axis StartAxis { get { return m_startAxis; } set { if (SetPropertyUtility.SetStruct(ref m_startAxis, value)) { RefreshGridLayout(); } } }
        public int HorizontalDirection { get { return m_horizontalDirection; } set { if (SetPropertyUtility.SetStruct(ref m_horizontalDirection, value)) { RefreshGridLayout(); } } }
        public int VerticalDirection { get { return m_verticalDirection; } set { if (SetPropertyUtility.SetStruct(ref m_verticalDirection, value)) { RefreshGridLayout(); } } }
        public eHorizontalAlignment HorizontalAlignment { get { return m_horizontalAlignment; } set { if (SetPropertyUtility.SetStruct(ref m_horizontalAlignment, value)) { RefreshGridLayout(); } } }
        public eVerticalAlignment VerticalAlignment { get { return m_verticalAlignment; } set { if (SetPropertyUtility.SetStruct(ref m_verticalAlignment, value)) { RefreshGridLayout(); } } }

        public static bool AreAllInitialized
        {
            get
            {
                int uninitializedCount = 0;
                foreach (var entry in m_executionLevel_numberOfScriptsToInitialize)
                {
                    uninitializedCount += entry.Value;
                }

                return uninitializedCount == 0;
            }
        }

        private Vector2 TotalPadding { get { return new Vector2(m_padding.xMin + m_padding.xMax, m_padding.yMin + m_padding.yMax); } }

        private Vector2 TotalSpacing { get { return new Vector2(m_spacing.x * (m_elementsPerRow - 1), m_spacing.y * (m_elementsPerColumn - 1)); } }

        private float WidthAvailableForElements
        {
            get
            {
                float widthAvailableForElements = 1 - (TotalPadding.x + TotalSpacing.x);
                if (widthAvailableForElements < 0)
                    widthAvailableForElements = 0;
                else if (widthAvailableForElements > 1)
                    widthAvailableForElements = 1;

                return widthAvailableForElements;
            }
        }

        private float HeightAvailableForElements
        {
            get
            {
                float heightAvailableForElements = 1 - (TotalPadding.y + TotalSpacing.y);
                if (heightAvailableForElements < 0)
                    heightAvailableForElements = 0;
                else if (heightAvailableForElements > 1)
                    heightAvailableForElements = 1;

                return heightAvailableForElements;
            }
        }

        private float WidthAvailableForSpacing
        {
            get
            {
                float widthAvailableForSpacing = 1 - (m_elementWidth * m_elementsPerRow + TotalPadding.x);
                if (widthAvailableForSpacing < 0)
                    widthAvailableForSpacing = 0;
                else if (widthAvailableForSpacing > 1)
                    widthAvailableForSpacing = 1;

                return widthAvailableForSpacing;
            }
        }

        private float HeightAvailableForSpacing
        {
            get
            {
                float heightAvailableForSpacing = 1 - (m_elementHeight * m_elementsPerColumn + TotalPadding.y);
                if (heightAvailableForSpacing < 0)
                    heightAvailableForSpacing = 0;
                else if (heightAvailableForSpacing > 1)
                    heightAvailableForSpacing = 1;

                return heightAvailableForSpacing;
            }
        }

        private float TotalWidthUsed { get { return m_elementWidth * m_elementsPerRow + TotalPadding.x + TotalSpacing.x; } }

        private float TotalHeightUsed { get { return m_elementHeight * m_elementsPerColumn + TotalPadding.y + TotalSpacing.y; } }

        public bool RefreshLayoutOnTransformChildrenChanged { get; set; }
        #endregion

        #region Private Fields
        private int m_previousChildCount;

        private RectTransform m_rectTransform;
        private float m_rectTransformWidth;
        private float m_rectTransformHeight;

        //Values of the six variables below are Relative To Parent Rect Transform
        private Rect m_padding;
        private Vector2 m_spacing;
        private float m_elementWidth;
        private float m_elementHeight;

        private bool m_elementWidthSet;
        private bool m_elementHeightSet;
        private bool m_paddingLeftSet;
        private bool m_paddingRightSet;
        private bool m_paddingTopSet;
        private bool m_paddingBottomSet;
        private bool m_spacingXSet;
        private bool m_spacingYSet;

        private int m_numberOfTimesExpanded;

        private bool m_isInitialized;

        private static int m_currentExecutionLevel = 1; //Used to identify whether this script can be executed
        private static Dictionary<int, int> m_executionLevel_numberOfScriptsToInitialize = new Dictionary<int, int>();
        private static int m_sceneChangeCount = 0;
        #endregion

        private void OnValidate()
        {
            if (m_isInitialized)
                RefreshGridLayout();
        }

        // Awake is called before Start and Update for the first frame
        void Awake()
        {
            m_rectTransform = this.GetComponent<RectTransform>();

            m_padding = new Rect();

            m_numberOfTimesExpanded = 0;

            RefreshLayoutOnTransformChildrenChanged = true;
            m_isInitialized = false;

            if (SceneConnector.SceneChangeCount != m_sceneChangeCount) // Scene has changed
            {
                m_sceneChangeCount = SceneConnector.SceneChangeCount; // Update

                m_currentExecutionLevel = 1; // Initialize
                m_executionLevel_numberOfScriptsToInitialize.Clear(); // Initialize
            }

            if (!m_executionLevel_numberOfScriptsToInitialize.ContainsKey(m_scriptExecutionPriorityLevel))
                m_executionLevel_numberOfScriptsToInitialize.Add(m_scriptExecutionPriorityLevel, 1);
            else
                m_executionLevel_numberOfScriptsToInitialize[m_scriptExecutionPriorityLevel]++;
        }

        // Update is called once per frame
        void Update()
        {
            if (!Application.isPlaying && !m_executeScriptAlways)
                return;

            if (!Application.isPlaying)
                RefreshGridLayout();
            else
            {
                if (m_currentExecutionLevel >= m_scriptExecutionPriorityLevel)
                {
                    if (!m_isInitialized)
                    {
                        //Debug.Log(this.gameObject.name + " -> Initialization: Calling RefreshGridLayout()");
                        bool succeeded = (m_executionLevel_numberOfScriptsToInitialize.ContainsKey(m_scriptExecutionPriorityLevel)) ? RefreshGridLayout() : false;
                        if (succeeded)
                        {
                            m_executionLevel_numberOfScriptsToInitialize[m_scriptExecutionPriorityLevel]--;
                            m_previousChildCount = this.transform.childCount;
                            m_isInitialized = true;

                            if (m_executionLevel_numberOfScriptsToInitialize.ContainsKey(m_currentExecutionLevel)
                                && m_executionLevel_numberOfScriptsToInitialize[m_currentExecutionLevel] == 0)
                            {
                                m_currentExecutionLevel++;
                            }
                        }
                    }
                }
            }
        }

        private void OnTransformChildrenChanged()
        {
            if (m_isInitialized && RefreshLayoutOnTransformChildrenChanged)
            {
                if (this.transform.childCount != m_previousChildCount) //Limit the number of refresh to one, when the number of child objects has changed
                {
                    //Debug.Log(this.gameObject.name + "-> OnTransformChildrenChanged(): ChildCount changed from " + m_previousChildCount.ToString() + " to " + this.transform.childCount.ToString() + ". Calling RefreshGridLayout()");
                    m_previousChildCount = this.transform.childCount;
                    RefreshGridLayout();
                }
            }
        }

        private bool RefreshGridLayout() { return RefreshGridLayout(-1); }
        private bool RefreshGridLayout(int _numberOfElementsToConsider)
        {
            if (Application.isPlaying)
            {
                if (m_scriptExecutionPriorityLevel > m_currentExecutionLevel)
                    return false;
            }

            //Debug.Log("Refreshing Grid Layout!");

            try
            {
                //Prevent undesirable movement of the rect transform
                m_rectTransform.SetPosition(0, 0, 0, 0);

                //Getting the size of the rectangle of the gameobject.
                m_rectTransformWidth = m_rectTransform.rect.width;
                m_rectTransformHeight = m_rectTransform.rect.height;

                if (m_rectTransformWidth <= 0 || m_rectTransformHeight <= 0)
                    return false;

                //Debug.Log(this.gameObject.name + "'s size is (" + m_rectTransformWidth.ToString() + ", " + m_rectTransformHeight.ToString() + ")");

                //----------------------------------------------------------------------------------
                // Initialize value set status
                //----------------------------------------------------------------------------------
                m_elementWidthSet = false;
                m_elementHeightSet = false;
                m_paddingLeftSet = false;
                m_paddingRightSet = false;
                m_paddingTopSet = false;
                m_paddingBottomSet = false;
                m_spacingXSet = false;
                m_spacingYSet = false;

                m_elementWidth = 0;
                m_elementHeight = 0;
                m_padding.Set(0, 0, 0, 0);
                m_spacing = Vector2.zero;

                //----------------------------------------------------------------------------------
                // Start calculation
                //----------------------------------------------------------------------------------
                CalculateElementSizeIfReferenceless(); // Element size
                CalculatePaddingIfReferenceless(); // Padding
                CalculateSpacingIfReferenceless(); // Spacing

                CalculateElementSizeIfRelativeToTheOtherSide(); // This method will be called again if the referencing element side is to be set automatically by referencing padding and spacing. In such case, this method will actually do nothing.

                CalculatePaddingIfRelativeToElement(); // Padding based on element size
                CalculateSpacingIfRelativeToElement(); // Spacing based on element size

                while (!(m_paddingLeftSet && m_paddingRightSet && m_paddingTopSet && m_paddingBottomSet))
                {
                    CalculatePaddingIfRelativeToContraryAxisPadding(); // Padding based on a contrary axis' padding
                }

                CalculateSpacingIfRelativeToPadding(); // Spacing based on a contrary axis' padding

                CalculateSpacingIfRelativeToContraryAxisSpacing(); // Spacing based the other spacing

                CalculateElementSizeIfAutomaticSizing(); // Element size based on padding and spacing

                CalculateElementSizeIfRelativeToTheOtherSide(); // Corrects the element size if the other element side was sized automatically

                CalculateSpacingIfAutomaticSizing(); // Spacing based on element size and padding

                //------------------------------------------------------------------------------------------------------
                // Set the number of elements per axis in case the number of elements towards the axis is not specified
                //------------------------------------------------------------------------------------------------------
                if (!m_fixedNumOfElementsPerRow)
                    m_elementsPerRow = Convert.ToInt32(Mathf.Floor(WidthAvailableForElements / m_elementWidth)); // Set to the max number of elements that fit horizontally

                if (!m_fixedNumOfElementsPerColumn)
                    m_elementsPerColumn = Convert.ToInt32(Mathf.Floor(HeightAvailableForElements / m_elementHeight)); // Set to the max number of elements that fit vertically

                //----------------------------------------------------------------------------------
                // Adjust element size, padding, and spacing if the total exceeds the available space
                //----------------------------------------------------------------------------------
                if (m_fixedNumOfElementsPerRow || !m_horizontallyExpansive)
                {
                    float totalWidthUsed = TotalWidthUsed;
                    if (totalWidthUsed > 1 && !Mathf.Approximately(totalWidthUsed, 1))
                    {
                        // Divide by totalWidthUsed so that all values are reduced to have a total of 1
                        m_elementWidth /= totalWidthUsed;
                        m_padding.xMin /= totalWidthUsed;
                        m_padding.xMax /= totalWidthUsed;
                        m_spacing.x /= totalWidthUsed;
                    }
                }

                if (m_fixedNumOfElementsPerColumn || !m_verticallyExpansive)
                {
                    float totalHeightUsed = TotalHeightUsed;
                    if (totalHeightUsed > 1 && !Mathf.Approximately(totalHeightUsed, 1))
                    {
                        // Divide by totalHeightUsed so that all values are reduced to have a total of 1
                        m_elementHeight /= totalHeightUsed;
                        m_padding.yMin /= totalHeightUsed;
                        m_padding.yMax /= totalHeightUsed;
                        m_spacing.y /= totalHeightUsed;
                    }
                }

                //----------------------------------------------------------------------------------
                // Set the acutal layout
                //----------------------------------------------------------------------------------

                int childCount = this.transform.childCount;

                int numOfElementsToConsider;
                if (_numberOfElementsToConsider < 0 || _numberOfElementsToConsider > childCount)
                    numOfElementsToConsider = childCount;
                else
                    numOfElementsToConsider = _numberOfElementsToConsider;

                //Set size and position of each cell within the grid
                //Do not modify Rect Information
                for (int i = 0; i < numOfElementsToConsider; i++) //...instead, modify anchor
                {
                    RectTransform rt = this.transform.GetChild(i).GetComponent<RectTransform>();

                    // Action in case the element cannot be placed within the available area
                    if (i >= m_elementsPerRow * m_elementsPerColumn)//...if there are more elements than what can fit in the given area
                    {
                        if (!m_horizontallyExpansive && !m_verticallyExpansive)//...if the area is not expansive, destroy the element
                        {
                            if (!Application.isPlaying)
                            {
                                Debug.Log("The extra object will be destroyed only on play!");
                                break;
                            }
                            else
                                Destroy(rt.gameObject);
                        }
                        else
                        {
                            Vector2 expansionVector;

                            if (m_horizontallyExpansive)
                            {
                                m_numberOfTimesExpanded++;

                                expansionVector = new Vector2(m_numberOfTimesExpanded, 0);
                                if (m_horizontalDirection == 0) // Left to Right
                                    m_rectTransform.anchorMax += expansionVector; // Expand to the right
                                else // Right to Left
                                    m_rectTransform.anchorMin -= expansionVector; // Expand to the left
                            }
                            else if (m_verticallyExpansive)
                            {
                                m_numberOfTimesExpanded++;

                                expansionVector = new Vector2(0, m_numberOfTimesExpanded);
                                if (m_verticalDirection == 0) // Top to Bottom
                                    m_rectTransform.anchorMin -= expansionVector; // Expand to the bottom
                                else // Bottom to Top
                                    m_rectTransform.anchorMax += expansionVector; // Expand to the top
                            }

                            //Debug.Log(this.gameObject.name + "-> Expanded the rect transform: Calling RefreshGridLayout()");
                            RefreshGridLayout();
                            return false; //Terminate the function
                        }
                    }

                    int cellRow = 0;
                    int cellColumn = 0;
                    int numOfElementsInRow = 0;
                    int numOfElementsInColumn = 0;

                    if (m_startAxis == Axis.Horizontal)
                    {
                        cellRow = i / m_elementsPerRow;
                        cellColumn = i % m_elementsPerRow;

                        numOfElementsInRow = (m_elementsPerRow * (cellRow + 1) > numOfElementsToConsider) ? numOfElementsToConsider % m_elementsPerRow : m_elementsPerRow;
                        int numberOfRowsUsed = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(numOfElementsToConsider) / m_elementsPerRow));
                        numOfElementsInColumn = numberOfRowsUsed;
                    }
                    else
                    {
                        cellRow = i % m_elementsPerColumn;
                        cellColumn = i / m_elementsPerColumn;

                        numOfElementsInColumn = (m_elementsPerColumn * (cellColumn + 1) > numOfElementsToConsider) ? numOfElementsToConsider % m_elementsPerColumn : m_elementsPerColumn;
                        int numberOfColumnsUsed = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(numOfElementsToConsider) / m_elementsPerColumn));
                        numOfElementsInRow = numberOfColumnsUsed;
                    }

                    // Adjust padding value based on alignment
                    float totalElementWidthForRow = m_elementWidth * numOfElementsInRow;
                    float totalElementHeightForColumn = m_elementHeight * numOfElementsInColumn;
                    Vector2 currentTotalSpacing = new Vector2(m_spacing.x * (numOfElementsInRow - 1), m_spacing.y * (numOfElementsInColumn - 1));
                    float availableWidth = 1 - (totalElementWidthForRow + TotalPadding.x + currentTotalSpacing.x);
                    float availableHeight = 1 - (totalElementHeightForColumn + TotalPadding.y + currentTotalSpacing.y);

                    switch (m_horizontalAlignment)
                    {
                        default: // case eHorizontalAlignment.Left
                            m_padding.xMax += availableWidth;
                            break;
                        case eHorizontalAlignment.Center:
                            {
                                float halfWidth = availableWidth / 2;
                                m_padding.xMin += halfWidth;
                                m_padding.xMax += halfWidth;
                            }
                            break;
                        case eHorizontalAlignment.Right:
                            m_padding.xMin += availableWidth;
                            break;
                    }

                    switch (m_verticalAlignment)
                    {
                        default: // case eVerticalAlignment.Top
                            m_padding.yMin += availableHeight;
                            break;
                        case eVerticalAlignment.Center:
                            {
                                float halfHeight = availableHeight / 2;
                                m_padding.yMin += halfHeight;
                                m_padding.yMax += halfHeight;
                            }
                            break;
                        case eVerticalAlignment.Bottom:
                            m_padding.yMax += availableHeight;
                            break;
                    }

                    // Set the position of the element
                    float minX;
                    float minY;

                    if (m_horizontalDirection == 0 && m_verticalDirection == 0) // UpperLeft to LowerRight
                    {
                        minX = m_padding.xMin + cellColumn * (m_elementWidth + m_spacing.x);
                        minY = 1 - ((m_padding.yMax) + m_elementHeight + cellRow * (m_elementHeight + m_spacing.y));
                    }
                    else if (m_horizontalDirection == 1 && m_verticalDirection == 1) // LowerRight to UpperLeft
                    {
                        minX = 1 - ((m_padding.xMax) + m_elementWidth + cellColumn * (m_elementWidth + m_spacing.x));
                        minY = m_padding.yMin + cellRow * (m_elementHeight + m_spacing.y);
                    }
                    else if (m_horizontalDirection == 0) // LowerLeft to UpperRight
                    {
                        minX = m_padding.xMin + cellColumn * (m_elementWidth + m_spacing.x);
                        minY = m_padding.yMin + cellRow * (m_elementHeight + m_spacing.y);
                    }
                    else // UpperRight to LowerLeft
                    {
                        minX = 1 - ((m_padding.xMax) + m_elementWidth + cellColumn * (m_elementWidth + m_spacing.x));
                        minY = 1 - ((m_padding.yMax) + m_elementHeight + cellRow * (m_elementHeight + m_spacing.y));
                    }

                    float maxX = minX + m_elementWidth;
                    float maxY = minY + m_elementHeight;

                    // Due to the fact that float does not return an exact representation of a number,
                    // the values can become slightly out of range (0 <= value <= 1).
                    // Hence, adjust the value if out of range.
                    if (minX < 0)
                        minX = 0;

                    if (minY < 0)
                        minY = 0;

                    if (maxX > 1)
                        maxX = 1;

                    if (maxY > 1)
                        maxY = 1;

                    // Set element position
                    rt.anchorMin = new Vector2(minX, minY);
                    rt.anchorMax = new Vector2(maxX, maxY);

                    //Prevent undesired movement of the rect transform
                    rt.SetPosition(0, 0, 0, 0);
                }

                // Send elements that must not be considered out of the screen
                Vector2 outOfScreen = new Vector2(-1, -1);
                for (int i = numOfElementsToConsider; i < childCount; i++)
                {
                    RectTransform rt_element = this.transform.GetChild(i).GetComponent<RectTransform>();
                    rt_element.anchorMin = outOfScreen;
                    rt_element.anchorMax = outOfScreen;
                }

                return true;
            }
            catch (Exception ex)
            {
                //Debug.Log("DynamicGridLayoutGroup.RefreshGridLayout() : " + ex.Message + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Calculate element size for cases where no reference to other values is required
        /// </summary>
        private void CalculateElementSizeIfReferenceless()
        {
            if (!m_automaticElementWidth)
            {
                switch (m_elementSizeX.m_valueType)
                {
                    case eValueTypeForElementSize.Fixed:
                        m_elementWidth = m_elementSizeX.m_value / m_rectTransformWidth;
                        m_elementWidthSet = true;
                        break;
                    case eValueTypeForElementSize.RelativeToParent:
                        {
                            m_elementWidth = m_elementSizeX.m_value;
                            if (m_horizontallyExpansive)
                                m_elementWidth /= 1 + m_numberOfTimesExpanded;

                            m_elementWidthSet = true;
                        }
                        break;
                    default: // case eValueTypeForElementSize.RelativeToTheOtherSide
                             // Calculation requires m_elementHeight obtained afterwards
                        break;
                }
            }

            if (!m_automaticElementHeight)
            {
                switch (m_elementSizeY.m_valueType)
                {
                    case eValueTypeForElementSize.Fixed:
                        m_elementHeight = m_elementSizeY.m_value / m_rectTransformHeight;
                        m_elementHeightSet = true;
                        break;
                    case eValueTypeForElementSize.RelativeToParent:
                        {
                            m_elementHeight = m_elementSizeY.m_value;
                            if (m_verticallyExpansive)
                                m_elementHeight /= 1 + m_numberOfTimesExpanded;

                            m_elementHeightSet = true;
                        }
                        break;
                    default: // case eValueTypeForElementSize.RelativeToTheOtherSide
                             // Calculation requires m_elementWidth obtained afterwards
                        break;
                }
            }
        }

        /// <summary>
        /// Calculate element size for cases where reference to the other side is required
        /// </summary>
        private void CalculateElementSizeIfRelativeToTheOtherSide()
        {
            if (m_elementSizeX.m_valueType == eValueTypeForElementSize.RelativeToTheOtherSide && m_elementHeightSet)
            {
                m_elementWidth = ((m_elementHeight * m_rectTransformHeight) * m_elementSizeX.m_value) / m_rectTransformWidth;
                m_elementWidthSet = true;
            }

            if (m_elementSizeY.m_valueType == eValueTypeForElementSize.RelativeToTheOtherSide && m_elementWidthSet)
            {
                m_elementHeight = ((m_elementWidth * m_rectTransformWidth) * m_elementSizeY.m_value) / m_rectTransformHeight;
                m_elementHeightSet = true;
            }
        }

        /// <summary>
        /// Calculate element size for cases where it is to be set based on padding and spacing
        /// </summary>
        private void CalculateElementSizeIfAutomaticSizing()
        {
            if (m_automaticElementWidth && m_paddingLeftSet && m_paddingRightSet && m_spacingXSet)
            {
                m_elementWidth = WidthAvailableForElements / m_elementsPerRow;
                m_elementWidthSet = true;
            }

            if (m_automaticElementHeight && m_paddingTopSet && m_paddingBottomSet && m_spacingYSet)
            {
                m_elementHeight = HeightAvailableForElements / m_elementsPerColumn;
                m_elementHeightSet = true;
            }
        }

        /// <summary>
        /// Calculate padding for cases where no reference to other values is required
        /// </summary>
        private void CalculatePaddingIfReferenceless()
        {
            switch (m_paddingLeft.m_valueType)
            {
                case eValueTypeForHorizontalPadding.Fixed:
                    m_padding.xMin = m_paddingLeft.m_value / m_rectTransformWidth;
                    m_paddingLeftSet = true;
                    break;
                case eValueTypeForHorizontalPadding.RelativeToParent:
                    {
                        m_padding.xMin = m_paddingLeft.m_value;
                        if (m_horizontallyExpansive)
                            m_padding.xMin /= 1 + m_numberOfTimesExpanded;

                        m_paddingLeftSet = true;
                    }
                    break;
                default:
                    // Calculation requires values obtained afterwards
                    break;
            }

            switch (m_paddingRight.m_valueType)
            {
                case eValueTypeForHorizontalPadding.Fixed:
                    m_padding.xMax = m_paddingRight.m_value / m_rectTransformWidth;
                    m_paddingRightSet = true;
                    break;
                case eValueTypeForHorizontalPadding.RelativeToParent:
                    {
                        m_padding.xMax = m_paddingRight.m_value;
                        if (m_horizontallyExpansive)
                            m_padding.xMax /= 1 + m_numberOfTimesExpanded;

                        m_paddingRightSet = true;
                    }
                    break;
                default:
                    // Calculation requires values obtained afterwards
                    break;
            }

            switch (m_paddingTop.m_valueType)
            {
                case eValueTypeForVerticalPadding.Fixed:
                    m_padding.yMax = m_paddingTop.m_value / m_rectTransformHeight;
                    m_paddingTopSet = true;
                    break;
                case eValueTypeForVerticalPadding.RelativeToParent:
                    {
                        m_padding.yMax = m_paddingTop.m_value;
                        if (m_verticallyExpansive)
                            m_padding.yMax /= 1 + m_numberOfTimesExpanded;

                        m_paddingTopSet = true;
                    }
                    break;
                default:
                    // Calculation requires values obtained afterwards
                    break;
            }

            switch (m_paddingBottom.m_valueType)
            {
                case eValueTypeForVerticalPadding.Fixed:
                    m_padding.yMin = m_paddingBottom.m_value / m_rectTransformHeight;
                    m_paddingBottomSet = true;
                    break;
                case eValueTypeForVerticalPadding.RelativeToParent:
                    {
                        m_padding.yMin = m_paddingBottom.m_value;
                        if (m_verticallyExpansive)
                            m_padding.yMin /= 1 + m_numberOfTimesExpanded;

                        m_paddingBottomSet = true;
                    }
                    break;
                default:
                    // Calculation requires values obtained afterwards
                    break;
            }
        }

        /// <summary>
        /// Calculate padding for cases where reference to element size is required
        /// </summary>
        private void CalculatePaddingIfRelativeToElement()
        {
            if (m_paddingLeft.m_valueType == eValueTypeForHorizontalPadding.RelativeToElement && m_elementWidthSet)
            {
                m_padding.xMin = m_paddingLeft.m_value * m_elementWidth;
                m_paddingLeftSet = true;
            }

            if (m_paddingRight.m_valueType == eValueTypeForHorizontalPadding.RelativeToElement && m_elementWidthSet)
            {
                m_padding.xMax = m_paddingRight.m_value * m_elementWidth;
                m_paddingRightSet = true;
            }

            if (m_paddingTop.m_valueType == eValueTypeForVerticalPadding.RelativeToElement && m_elementHeightSet)
            {
                m_padding.yMax = m_paddingTop.m_value * m_elementHeight;
                m_paddingTopSet = true;
            }

            if (m_paddingBottom.m_valueType == eValueTypeForVerticalPadding.RelativeToElement && m_elementHeightSet)
            {
                m_padding.yMin = m_paddingBottom.m_value * m_elementHeight;
                m_paddingBottomSet = true;
            }
        }

        /// <summary>
        /// Calculate padding for cases where reference to the other axis' value is required
        /// </summary>
        private void CalculatePaddingIfRelativeToContraryAxisPadding()
        {
            if (!m_paddingLeftSet && m_paddingLeft.m_valueType == eValueTypeForHorizontalPadding.RelativeToPaddingTop && m_paddingTopSet)
            {
                m_padding.xMin = ((m_padding.yMax * m_rectTransformHeight) * m_paddingLeft.m_value) / m_rectTransformWidth;
                m_paddingLeftSet = true;
            }

            if (!m_paddingLeftSet && m_paddingLeft.m_valueType == eValueTypeForHorizontalPadding.RelativeToPaddingBottom && m_paddingBottomSet)
            {
                m_padding.xMin = ((m_padding.yMin * m_rectTransformHeight) * m_paddingLeft.m_value) / m_rectTransformWidth;
                m_paddingLeftSet = true;
            }

            if (!m_paddingRightSet && m_paddingRight.m_valueType == eValueTypeForHorizontalPadding.RelativeToPaddingTop && m_paddingTopSet)
            {
                m_padding.xMax = ((m_padding.yMax * m_rectTransformHeight) * m_paddingRight.m_value) / m_rectTransformWidth;
                m_paddingRightSet = true;
            }

            if (!m_paddingRightSet && m_paddingRight.m_valueType == eValueTypeForHorizontalPadding.RelativeToPaddingBottom && m_paddingBottomSet)
            {
                m_padding.xMax = ((m_padding.yMin * m_rectTransformHeight) * m_paddingRight.m_value) / m_rectTransformWidth;
                m_paddingRightSet = true;
            }

            if (!m_paddingTopSet && m_paddingTop.m_valueType == eValueTypeForVerticalPadding.RelativeToPaddingLeft && m_paddingLeftSet)
            {
                m_padding.yMax = ((m_padding.xMin * m_rectTransformWidth) * m_paddingTop.m_value) / m_rectTransformHeight;
                m_paddingTopSet = true;
            }

            if (!m_paddingTopSet && m_paddingTop.m_valueType == eValueTypeForVerticalPadding.RelativeToPaddingRight && m_paddingRightSet)
            {
                m_padding.yMax = ((m_padding.xMax * m_rectTransformWidth) * m_paddingTop.m_value) / m_rectTransformHeight;
                m_paddingTopSet = true;
            }

            if (!m_paddingBottomSet && m_paddingBottom.m_valueType == eValueTypeForVerticalPadding.RelativeToPaddingLeft && m_paddingLeftSet)
            {
                m_padding.yMin = ((m_padding.xMin * m_rectTransformWidth) * m_paddingBottom.m_value) / m_rectTransformHeight;
                m_paddingBottomSet = true;
            }

            if (!m_paddingBottomSet && m_paddingBottom.m_valueType == eValueTypeForVerticalPadding.RelativeToPaddingRight && m_paddingRightSet)
            {
                m_padding.yMin = ((m_padding.xMax * m_rectTransformWidth) * m_paddingBottom.m_value) / m_rectTransformHeight;
                m_paddingBottomSet = true;
            }
        }

        /// <summary>
        /// Calculate spacing for cases where no reference to other values is required
        /// </summary>
        private void CalculateSpacingIfReferenceless()
        {
            switch (m_spacingX.m_valueType)
            {
                case eValueTypeForHorizontalSpacing.Fixed:
                    m_spacing.x = m_spacingX.m_value / m_rectTransformWidth;
                    m_spacingXSet = true;
                    break;
                case eValueTypeForHorizontalSpacing.RelativeToParent:
                    {
                        m_spacing.x = m_spacingX.m_value;
                        if (m_horizontallyExpansive)
                            m_spacing.x /= 1 + m_numberOfTimesExpanded;

                        m_spacingXSet = true;
                    }
                    break;
                default:
                    // Calculation requires values obtained afterwards
                    break;
            }

            switch (m_spacingY.m_valueType)
            {
                case eValueTypeForVerticalSpacing.Fixed:
                    m_spacing.y = m_spacingY.m_value / m_rectTransformHeight;
                    m_spacingYSet = true;
                    break;
                case eValueTypeForVerticalSpacing.RelativeToParent:
                    {
                        m_spacing.y = m_spacingY.m_value;
                        if (m_verticallyExpansive)
                            m_spacing.y /= 1 + m_numberOfTimesExpanded;

                        m_spacingYSet = true;
                    }
                    break;
                default:
                    // Calculation requires values obtained afterwards
                    break;
            }
        }

        /// <summary>
        /// Calculate spacing for cases where reference to element size is required
        /// </summary>
        private void CalculateSpacingIfRelativeToElement()
        {
            if (m_spacingX.m_valueType == eValueTypeForHorizontalSpacing.RelativeToElement && m_elementWidthSet)
            {
                m_spacing.x = m_spacingX.m_value * m_elementWidth;
                m_spacingXSet = true;
            }

            if (m_spacingY.m_valueType == eValueTypeForVerticalSpacing.RelativeToElement && m_elementHeightSet)
            {
                m_spacing.y = m_spacingY.m_value * m_elementHeight;
                m_spacingYSet = true;
            }
        }

        /// <summary>
        /// Calculate spacing for cases where reference to the same axis' padding value is required
        /// </summary>
        private void CalculateSpacingIfRelativeToPadding()
        {
            if (m_spacingX.m_valueType == eValueTypeForHorizontalSpacing.RelativeToPaddingLeft && m_paddingLeftSet)
            {
                m_spacing.x = m_spacingX.m_value * m_padding.xMin;
                m_spacingXSet = true;
            }

            if (m_spacingX.m_valueType == eValueTypeForHorizontalSpacing.RelativeToPaddingRight && m_paddingRightSet)
            {
                m_spacing.x = m_spacingX.m_value * m_padding.xMax;
                m_spacingXSet = true;
            }

            if (m_spacingY.m_valueType == eValueTypeForVerticalSpacing.RelativeToPaddingTop && m_paddingTopSet)
            {
                m_spacing.y = m_spacingY.m_value * m_padding.yMax;
                m_spacingYSet = true;
            }

            if (m_spacingY.m_valueType == eValueTypeForVerticalSpacing.RelativeToPaddingBottom && m_paddingBottomSet)
            {
                m_spacing.y = m_spacingY.m_value * m_padding.yMin;
                m_spacingYSet = true;
            }
        }

        /// <summary>
        /// Calculate spacing for cases where reference to the other axis' spacing value is required
        /// </summary>
        private void CalculateSpacingIfRelativeToContraryAxisSpacing()
        {
            if (m_spacingX.m_valueType == eValueTypeForHorizontalSpacing.RelativeToContraryAxisSpacing && m_spacingYSet)
            {
                m_spacing.x = ((m_spacing.y * m_rectTransformHeight) * m_spacingX.m_value) / m_rectTransformWidth;
                m_spacingXSet = true;
            }

            if (m_spacingY.m_valueType == eValueTypeForVerticalSpacing.RelativeToContraryAxisSpacing && m_spacingXSet)
            {
                m_spacing.y = ((m_spacing.x * m_rectTransformWidth) * m_spacingY.m_value) / m_rectTransformHeight;
                m_spacingYSet = true;
            }
        }

        /// <summary>
        /// Calculate spacing for cases where it is to be set based on element size and padding
        /// </summary>
        private void CalculateSpacingIfAutomaticSizing()
        {
            if (m_automaticSpacingX && m_elementWidthSet && m_paddingLeftSet && m_paddingRightSet)
            {
                m_spacing.x = WidthAvailableForSpacing / (m_elementsPerRow - 1);
                m_spacingXSet = true;
            }

            if (m_automaticSpacingY && m_elementHeightSet && m_paddingTopSet && m_paddingBottomSet)
            {
                m_spacing.y = HeightAvailableForSpacing / (m_elementsPerColumn - 1);
                m_spacingYSet = true;
            }
        }

        #region Public Methods
        public void RefreshLayout(int _numberOfElementsToConsider) { if (m_isInitialized) { RefreshGridLayout(_numberOfElementsToConsider); } }
        #endregion

        public enum eHorizontalAlignment
        {
            Left,
            Center,
            Right
        }

        public enum eVerticalAlignment
        {
            Top,
            Center,
            Bottom
        }
    }
}