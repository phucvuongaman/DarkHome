using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DarkHome
{
    public class OutLineController : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Material outlineMaterial;

        private Material[] baseMaterials;
        private Material[] materialsWithOutline;
        private bool isOutlineEnabled = false;
        void Start()
        {
            baseMaterials = targetRenderer.materials;

            materialsWithOutline = new Material[baseMaterials.Length + 1];
            baseMaterials.CopyTo(materialsWithOutline, 0);
            materialsWithOutline[materialsWithOutline.Length - 1] = outlineMaterial;
        }

        public void EnableOutline()
        {
            if (isOutlineEnabled) return;
            targetRenderer.materials = materialsWithOutline;
            isOutlineEnabled = true;
        }
        public void DisableOutline()
        {
            if (!isOutlineEnabled) return;
            targetRenderer.materials = baseMaterials;
            isOutlineEnabled = false;
        }
    }
}

