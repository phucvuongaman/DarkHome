# 🧪 Localization System - Testing Guide

> **Test Plan:** Quick verification of CSV-driven localization  
> **Duration:** ~15 phút  
> **Prerequisites:** Unity Editor đã mở DarkHome project

---

## 📋 TEST OBJECTIVES

1. ✅ Verify LocalizationManager loads CSV correctly
2. ✅ Test Vietnamese character display (dấu tiếng Việt)
3. ✅ Test language switching EN ↔ VN
4. ✅ Test GetLocalizedName() in ItemDataSO
5. ✅ Test fallback behavior (missing keys)

---

## 🛠️ SETUP (5 phút)

### **Step 1: Add LocalizationManager to Scene**

1. Mở Scene hiện tại (hoặc tạo Test scene mới)
2. Tạo Empty GameObject: `Hierarchy > Right Click > Create Empty`
3. Rename thành "LocalizationManager"
4. Add Component: `LocalizationManager`
5. Trong Inspector, set:
   - **Default Language:** English

✅ **Done!** LocalizationManager sẽ tự load EN.csv khi Play

---

### **Step 2: Create Test ItemDataSO (Optional - nếu chưa có)**

1. `Assets > DarkHome > SO > Resources > Chapter1 > Items`
2. Right Click > Create > SO > Object > ItemSO
3. Rename: "Test_Milk"
4. Fill fields:
   ```
   itemID: MILK_01
   localizationKey: ITEM_MILK
   itemName: (để trống hoặc "Fallback Name")
   description: (để trống)
   itemType: ConsumableItem
   ```

---

## 🧪 TEST CASES

### **Test Case 1: Basic CSV Loading**

**Steps:**

1. Press **Play** trong Unity
2. Mở Console (Ctrl+Shift+C)

**Expected Output:**

```
✅ Loaded 20 localization entries for English
```

**Result:** ☐ PASS / ☐ FAIL

---

### **Test Case 2: GetText() API**

**Steps:**

1. Create test script `TestLocalization.cs`:

```csharp
using UnityEngine;
using DarkHome;

public class TestLocalization : MonoBehaviour
{
    void Start()
    {
        // Test basic GetText
        string itemName = LocalizationManager.Instance.GetText("ITEM_MILK_name");
        Debug.Log($"[TEST] ITEM_MILK_name = {itemName}");

        // Test missing key
        string missing = LocalizationManager.Instance.GetText("KEY_DOES_NOT_EXIST");
        Debug.Log($"[TEST] Missing key = {missing}");
    }
}
```

2. Attach script to any GameObject
3. Play

**Expected Output:**

```
[TEST] ITEM_MILK_name = Fresh Milk
[TEST] Missing key = [KEY_DOES_NOT_EXIST]
```

**Result:** ☐ PASS / ☐ FAIL

---

### **Test Case 3: Vietnamese Characters**

**Steps:**

1. Modify `TestLocalization.cs`:

```csharp
void Start()
{
    // Switch to Vietnamese
    LocalizationManager.Instance.ChangeLanguage(SystemLanguage.Vietnamese);

    // Test Vietnamese text
    string vnName = LocalizationManager.Instance.GetText("ITEM_MILK_name");
    string vnDesc = LocalizationManager.Instance.GetText("ITEM_MILK_desc");

    Debug.Log($"[VN TEST] Name: {vnName}");
    Debug.Log($"[VN TEST] Desc: {vnDesc}");
}
```

2. Play

**Expected Output (CHECK DẤU TIẾNG VIỆT!):**

```
[VN TEST] Name: Sữa Tươi           ✅ Đúng
[VN TEST] Desc: Một chai sữa tươi. Hồi 20 HP.
```

**NOT THIS (Broken UTF-8):**

```
[VN TEST] Name: S?a T??i           ❌ SAI!
```

**Result:** ☐ PASS / ☐ FAIL

---

### **Test Case 4: ItemDataSO Integration**

**Steps:**

1. Modify test script:

```csharp
using DarkHome;

public class TestLocalization : MonoBehaviour
{
    public ItemDataSO testItem;  // Assign Test_Milk in Inspector

    void Start()
    {
        // Test EN
        string nameEN = testItem.GetLocalizedName();
        Debug.Log($"[ITEM EN] {nameEN}");

        // Switch to VN
        LocalizationManager.Instance.ChangeLanguage(SystemLanguage.Vietnamese);

        string nameVN = testItem.GetLocalizedName();
        Debug.Log($"[ITEM VN] {nameVN}");
    }
}
```

2. Assign `Test_Milk` ItemDataSO to Inspector
3. Play

**Expected Output:**

```
[ITEM EN] Fresh Milk
🌐 Language changed to: Vietnamese
✅ Loaded 20 localization entries for Vietnamese
[ITEM VN] Sữa Tươi
```

**Result:** ☐ PASS / ☐ FAIL

---

### **Test Case 5: Fallback Behavior**

**Steps:**

1. Test khi localizationKey empty:

```csharp
// In ItemDataSO Inspector:
// localizationKey: (empty)
// itemName: "Hardcoded Name"

string name = testItem.GetLocalizedName();
Debug.Log($"[FALLBACK] {name}");
// Expected: "Hardcoded Name"
```

2. Test khi LocalizationManager missing:

```csharp
// Delete LocalizationManager from scene
// Play again

string name = testItem.GetLocalizedName();
// Expected: Falls back to itemName field
```

**Result:** ☐ PASS / ☐ FAIL

---

## 🐛 COMMON ISSUES & FIXES

### **❌ Issue: "Loaded 0 localization entries"**

**Cause:** CSV file không tồn tại hoặc sai path

**Fix:**

1. Check `Assets/StreamingAssets/Localization/EN.csv` exists
2. Right-click project > Refresh (F5)
3. Build Settings > Player Settings > check StreamingAssets included

---

### **❌ Issue: Vietnamese text shows "????" or broken**

**Cause:** UTF-8 encoding không đúng

**Fix:**

1. Check đã apply Fix #2 (UTF-8 encoding) chưa
2. Mở EN.csv/VN.csv bằng Notepad++
3. Encoding > Convert to UTF-8 (no BOM)
4. Save

---

### **❌ Issue: "LocalizationManager.Instance is null"**

**Cause:** Chưa add LocalizationManager vào scene

**Fix:**

1. Xem Setup Step 1
2. Ensure GameObject có component LocalizationManager
3. Check Awake() đã chạy (Play mode)

---

## ✅ SUCCESS CRITERIA

**Hệ thống PASSED nếu:**

- ☐ All 5 test cases PASS
- ☐ Vietnamese characters hiển thị đúng (không bị ???)
- ☐ Language switching works
- ☐ No errors in Console

---

## 📸 PROOF OF WORK

**Take screenshots:**

1. Console output showing "Loaded X entries for English"
2. Console showing Vietnamese text (Sữa Tươi) correctly
3. ItemDataSO hiển thị localized name

**Save to:** `DarkHome/Testing/Screenshots/Localization_Test_[Date].png`

---

## 🎯 NEXT STEPS AFTER TESTING

✅ **If All Tests Pass:**

1. Update `walkthrough.md` với test results
2. Start populating real game content vào CSV
3. Migrate existing Items/Dialogues

⚠️ **If Any Test Fails:**

1. Check `fix_summary.md` for fixes applied
2. Verify Unity recompiled (no lint errors)
3. Ask Mentor (me!) for help 🛠️

---

**🚀 READY TO TEST!** Chúc cậu may mắn! 🎉
