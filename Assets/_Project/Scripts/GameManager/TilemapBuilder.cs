using UnityEngine;
using FoodTruckKiller.Core.DataLoader;

namespace FoodTruckKiller.GameManager
{
    /// <summary>
    /// tilemap 背景铺贴：按 <see cref="SceneLayoutData.regions"/> 把街道/砖墙/暗巷/餐车地板
    /// 等区域铺成大块背景。
    /// <para>坐标尺度：使用 scene_layout.json 原值（1.875×1.25 世界单位范围），按 tile
    /// 平铺填充。sortingOrder=-10 永远在所有动态对象下方。</para>
    /// </summary>
    public class TilemapBuilder : MonoBehaviour
    {
        [Tooltip("铺贴间距（世界单位）。")]
        [SerializeField] private float tileStep = 0.4f;

        [Tooltip("背景块缩放。")]
        [SerializeField] private float tileScale = 1.5f;

        [Tooltip("铺贴范围：左下角 (-xMax,-yMax) 到右上角 (+xMax,+yMax)。覆盖摄像机视野。")]
        [SerializeField] private float xMax = 2.5f;
        [SerializeField] private float yMax = 1.7f;

        public void Build()
        {
            var layout = JsonDataLoader.SceneLayout;
            if (layout == null)
            {
                Debug.LogWarning("[TilemapBuilder] SceneLayout not loaded, skip.");
                return;
            }

            // 父节点
            var parent = new GameObject("Background_Tiles").transform;
            parent.SetParent(transform, false);

            // 大背景底色（街道）
            BuildLayer(parent, "base_street", "Sprites/Tilesets/street", -xMax, -yMax, xMax, yMax, 0);

            // 按 regions 叠加：暗巷/侧巷/餐车地板/砖墙
            if (layout.regions != null)
            {
                // 区域范围（JSON 原值）极小（0.1~1.8），需放大到 5×3 视野范围
                float scaleToViewX = (xMax * 2f) / 1.875f;  // ~2.67
                float scaleToViewY = (yMax * 2f) / 1.25f;   // ~2.72
                foreach (var region in layout.regions)
                {
                    var sprite = LoadRegionSprite(region.tileType);
                    if (sprite == null) continue;
                    // 区域中心映射到视野
                    float cx = region.x * scaleToViewX;
                    float cy = region.y * scaleToViewY;
                    float w  = Mathf.Max(region.w * scaleToViewX, 0.3f);
                    float h  = Mathf.Max(region.h * scaleToViewY, 0.3f);
                    BuildLayer(parent, $"_region_{region.name}", sprite,
                               cx - w * 0.5f, cy - h * 0.5f, w, h, 1);
                }
            }

            Debug.Log($"[TilemapBuilder] Built base + {layout.regions?.Count ?? 0} regions.");
        }

        private static Sprite LoadRegionSprite(string tileType)
        {
            string path = tileType switch
            {
                "Truck"      => "Sprites/Tilesets/truck_floor",
                "MainRoad"   => "Sprites/Tilesets/street",
                "DarkAlley"  => "Sprites/Tilesets/alley",
                "SideAlley"  => "Sprites/Tilesets/alley",
                "Sidewalk"   => "Sprites/Tilesets/sidewalk",
                "Wall"       => "Sprites/Tilesets/wall",
                _            => "Sprites/Tilesets/sidewalk",
            };
            return SceneBootstrapper.LoadSprite(path);
        }

        private void BuildLayer(Transform parent, string name, string spritePath,
            float x0, float y0, float x1, float y1, int sortingOrder)
        {
            var sprite = SceneBootstrapper.LoadSprite(spritePath);
            if (sprite == null) return;
            BuildLayer(parent, name, sprite, x0, y0, x1 - x0, y1 - y0, sortingOrder);
        }

        private void BuildLayer(Transform parent, string name, Sprite sprite,
            float x, float y, float w, float h, int sortingOrder)
        {
            int countX = Mathf.Max(1, Mathf.CeilToInt(w / tileStep));
            int countY = Mathf.Max(1, Mathf.CeilToInt(h / tileStep));

            for (int ix = 0; ix < countX; ix++)
            {
                for (int iy = 0; iy < countY; iy++)
                {
                    float wx = x + ix * tileStep + tileStep * 0.5f;
                    float wy = y + iy * tileStep + tileStep * 0.5f;
                    var go = new GameObject($"{name}_{ix}_{iy}");
                    go.transform.SetParent(parent, false);
                    go.transform.position = new Vector3(wx, wy, 0f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.color = Color.white;
                    sr.sortingOrder = sortingOrder - 10;
                    go.transform.localScale = new Vector3(tileScale, tileScale, 1f);
                }
            }
        }
    }
}
