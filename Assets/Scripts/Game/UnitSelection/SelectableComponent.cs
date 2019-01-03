using UnityEngine;

namespace Game.UnitSelection
{
    public class SelectableComponent : MonoBehaviour
    {
        public string CustomDisplayName = null;
        public string CustomDisplayType = null;
        public string DisplayName => string.IsNullOrWhiteSpace(CustomDisplayName) ? name : CustomDisplayName;
        public string DisplayType => string.IsNullOrWhiteSpace(CustomDisplayType) ? "Object" : CustomDisplayType;

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
            SetState(SelectColor, SelectedLayerName);
        }

        public void Deselect()
        {
            SetState(OriginalColor, DeselectedLayerName);
        }

        protected void SetState(Color color, string sortingLayer)
        {
            SpriteRenderer.color = color;
            SpriteRenderer.sortingLayerName = sortingLayer;
        }
    }
}
