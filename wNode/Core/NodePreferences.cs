using UnityEngine;
using wNode.Utilities;

namespace wNode.Core
{
    [ResourcePath("References")]
    public class NodePreferences : ResourcesSingleton<NodePreferences>
    {
        [Header("Debug")]
        public bool DebugMode;

        [Header("Graph")]
        public float MinZoom = 0.5f;

        public float MaxZoom = 4;
        public float ZoomDelta = 0.1f;

        public Vector2 PortSize = new Vector2(16, 16);
        public float PortOffset;

        public float StartTanOff = 0.3f;
        public float EndTanOff = 0.3f;

        public Vector2 ConnectionLabelOffset = new Vector2(25, -10);

        [Header("Background")]
        public Color BackgroundColor = new Color(0.1f, 0.1f, 0.1f);

        public Color MainlineColor = new Color(0.7f, 0.7f, 0.7f);
        public Color SidelineColor = new Color(0.3f, 0.3f, 0.3f);

        public int LineDistanceInPixel = 80;
        public int PaddingFactor = 4;

        [Header("Inspector")]
        public Color InsepctorColor = new Color(0.2f, 0.2f, 0.2f);
        
        [Header("Node")]
        public FieldTypeColorLookup FieldTypeColor = new FieldTypeColorLookup();

        private Texture2D _insepctorTexture2D;

        public Texture2D InsepctorTexture2D
        {
            get
            {
                if (DebugMode || _insepctorTexture2D == null)
                {
                    _insepctorTexture2D = new Texture2D(1, 1);
                    _insepctorTexture2D.SetPixel(0, 0, InsepctorColor);
                    _insepctorTexture2D.Apply();
                }

                return _insepctorTexture2D;
            }
        }

        private Texture2D _backgroundTexture2D;

        public Texture2D BackgroundTexture
        {
            get
            {
                if (DebugMode || _backgroundTexture2D == null)
                {
                    _backgroundTexture2D = new Texture2D(1, 1);
                    _backgroundTexture2D.SetPixel(0, 0, BackgroundColor);
                    _backgroundTexture2D.Apply();
                }

                return _backgroundTexture2D;
            }
        }

        private readonly Color defaultMainColor = Color.black;
        private Texture2D _gridTexture;

        public Texture2D GridTexture
        {
            get
            {
                if (DebugMode || _gridTexture == null)
                {
                    var padding = LineDistanceInPixel / PaddingFactor;
                    _gridTexture = new Texture2D(LineDistanceInPixel, LineDistanceInPixel);
                    var cols = new Color[LineDistanceInPixel * LineDistanceInPixel];
                    for (var x = 0; x < LineDistanceInPixel; x++)
                    {
                        for (var y = 0; y < LineDistanceInPixel; y++)
                        {
                            // Main Color 
                            var col = defaultMainColor;

                            if (x == 0 || y == 0 ||
                                y == LineDistanceInPixel - 1 || x == LineDistanceInPixel - 1)
                            {
                                col = MainlineColor;
                            }
                            else if (y % padding == 0 || x % padding == 0)
                            {
                                col = SidelineColor;
                            }
                            else
                            {
                                col.a = 0;
                            }

                            cols[y * LineDistanceInPixel + x] = col;
                        }
                    }

                    _gridTexture.SetPixels(cols);
                    _gridTexture.wrapMode = TextureWrapMode.Repeat;
                    _gridTexture.filterMode = FilterMode.Bilinear;
                    _gridTexture.Apply();
                }

                return _gridTexture;
            }
        }
    }
}