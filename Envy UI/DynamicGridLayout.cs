using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace EnvyLevelLoader.UI
{
    public class DynamicGridLayout : MonoBehaviour
    {
        public ScrollRect scrollRect;  // The ScrollRect component
        public GridLayoutGroup gridLayout;  // The GridLayoutGroup attached to the content
        public RectTransform content;  // The content RectTransform where elements will be added
        public RectTransform scrollViewRect;  // The RectTransform of the ScrollView
        public int minItemsPerRow = 1;  // Minimum number of items per row

        void Start()
        {
            UpdateGrid();
        }


        void UpdateGrid()
        {
            // Get the width of the ScrollView (the visible area of the ScrollRect)
            float scrollViewWidth = scrollViewRect.rect.width;

            // Get the size of a grid item
            float itemWidth = gridLayout.cellSize.x;

            // Get the padding and spacing
            float spacing = gridLayout.spacing.x;
            float padding = gridLayout.padding.left + gridLayout.padding.right;

            // Calculate the maximum number of items that can fit on the X-axis
            int maxItemsPerRow = Mathf.Max(minItemsPerRow, Mathf.FloorToInt((scrollViewWidth - padding + spacing) / (itemWidth + spacing)));
            int displayedItemsPerRow = (int)Mathf.Ceil(scrollViewWidth / itemWidth);

            // Update the GridLayoutGroup to have a flexible constraint for columns based on calculated value
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = maxItemsPerRow;

            scrollViewRect.offsetMax = new Vector2(-262 * (displayedItemsPerRow - maxItemsPerRow), scrollViewRect.offsetMax.y); ;
        }

        // This method can be called whenever the scroll view size or content changes
        public void OnScrollViewResized()
        {
            UpdateGrid();
        }
    }

}
