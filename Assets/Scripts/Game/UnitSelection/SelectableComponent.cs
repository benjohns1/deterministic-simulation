using UnityEngine;

namespace Game.UnitSelection
{
    public class SelectableComponent : MonoBehaviour
    {
        public string CustomDisplayName = null;
        public string CustomDisplayType = null;
        public string DisplayName => string.IsNullOrWhiteSpace(CustomDisplayName) ? name : CustomDisplayName;
        public string DisplayType => string.IsNullOrWhiteSpace(CustomDisplayType) ? "Object" : CustomDisplayType;
        public bool Selected;

        public string SelectedLayerName = "Selected";
        public Color SelectColor = Color.green;

        protected SpriteRenderer SpriteRenderer;
        protected Color OriginalColor;
        protected string DeselectedLayerName;

        private void Awake()
        {
            SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            OriginalColor = SpriteRenderer.color;
            DeselectedLayerName = SpriteRenderer.sortingLayerName;
        }

        public void Select()
        {
            SetState(true, SelectColor, SelectedLayerName);
        }

        public void Deselect()
        {
            SetState(false, OriginalColor, DeselectedLayerName);
        }

        protected void SetState(bool selected, Color color, string sortingLayer)
        {
            Selected = selected;
            SpriteRenderer.color = color;
            SpriteRenderer.sortingLayerName = sortingLayer;
        }
    }
}
