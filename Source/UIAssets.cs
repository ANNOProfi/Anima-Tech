using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AnimaTech
{
    [StaticConstructorOnStartup]
    public static class UIAssets
    {
        public static readonly List<Material> tableProjectionmaterials;

        static UIAssets()
        {
            tableProjectionmaterials = new List<Material>();
            InitializeTableProjections();
        }

        private static void InitializeTableProjections()
        {
            IEnumerable<Texture2D> enumerable = from x in ContentFinder<Texture2D>.GetAllInFolder("Things/Buildings/RunesOverlay")
                where !x.name.EndsWith(Graphic_Single.MaskSuffix)
                orderby x.name
                select x;
            tableProjectionmaterials.Clear();
            foreach (Texture2D item2 in enumerable)
            {
                Material item = MaterialPool.MatFrom(new MaterialRequest(item2, ShaderDatabase.Transparent));
                tableProjectionmaterials.Add(item);
            }
        }

        public static Material GetTableProjectionMaterial(ref int index, bool randomize = false)
        {
            if (randomize || index < 0)
            {
                index = Rand.Range(0, tableProjectionmaterials.Count);
            }
            else
            {
                index = Mathf.Clamp(index, 0, tableProjectionmaterials.Count);
            }
            return tableProjectionmaterials[index];
        }
    }
}