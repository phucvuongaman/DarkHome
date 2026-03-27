// Đây là file: ComponentSwapTool.cs (PHIÊN BẢN SỬA LỖI)

using UnityEngine;
using UnityEditor;
using TMPro;

public class ComponentSwapTool
{
    [MenuItem("Tools/Custom Tools/FIX: Swap Selected TextMeshPro (3D) to UI")]
    private static void SwapSelectedToTextMeshProUGUI()
    {
        GameObject selectedGO = Selection.activeGameObject;

        if (selectedGO == null)
        {
            EditorUtility.DisplayDialog("Lỗi", "Bạn phải chọn một GameObject (như Canvas_Home) trong Hierarchy trước.", "OK");
            return;
        }

        TMP_Text[] allTextComponents = selectedGO.GetComponentsInChildren<TMP_Text>(true);

        int count = 0;
        foreach (var textComponent in allTextComponents)
        {
            if (textComponent is TextMeshPro && !(textComponent is TextMeshProUGUI))
            {
                GameObject obj = textComponent.gameObject;
                TextMeshPro oldText = (TextMeshPro)textComponent;

                // --- CODE MỚI: TÌM MESH RENDERER ---
                MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
                // --- KẾT THÚC CODE MỚI ---

                // --- Sao chép (Copy) dữ liệu ---
                string text = oldText.text;
                FontStyles style = oldText.fontStyle;
                float fontSize = oldText.fontSize;
                Color color = oldText.color;
                bool richText = oldText.richText;
                TextAlignmentOptions alignment = oldText.alignment;
                TMP_FontAsset font = oldText.font;
                Material material = oldText.fontMaterial;

                Undo.RecordObject(obj, "Swap TextMeshPro Component");

                // --- CODE MỚI: XÓA CẢ HAI ---
                if (meshRenderer != null)
                {
                    Undo.DestroyObjectImmediate(meshRenderer); // Xóa MeshRenderer
                }
                Undo.DestroyObjectImmediate(oldText); // Xóa TextMeshPro (3D)
                                                      // --- KẾT THÚC CODE MỚI ---

                TextMeshProUGUI newText = Undo.AddComponent<TextMeshProUGUI>(obj);

                // --- Dán (Paste) dữ liệu ---
                newText.text = text;
                newText.fontStyle = style;
                newText.fontSize = fontSize;
                newText.color = color;
                newText.richText = richText;
                newText.alignment = alignment;
                newText.font = font;
                newText.fontMaterial = material;

                count++;
            }
        }

        EditorUtility.DisplayDialog("Hoàn tất!", $"Đã swap {count} component 'TextMeshPro' (3D) thành 'TextMeshProUGUI' (UI) VÀ dọn dẹp MeshRenderer.", "OK");
    }
}