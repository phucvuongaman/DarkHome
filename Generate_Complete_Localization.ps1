# Generate_Complete_Localization.ps1
# Auto-generates complete Localization_VN.csv from game CSVs

param(
    [string]$DataPath = "c:\Users\abcde\Desktop\Unity test\DarkHome\Assets\DarkHome\Data\CSV"
)

Write-Host "🔧 LOCALIZATION GENERATOR - DarkHome" -ForegroundColor Cyan
Write-Host "Generating complete localization from CSVs..." -ForegroundColor Yellow

# Output file
$outputFile = Join-Path $DataPath "Localization_VN_COMPLETE.csv"

# Header
"Key,Vietnamese,English" | Out-File $outputFile -Encoding UTF8

Write-Host "`n📖 Processing Dialogues.csv..." -ForegroundColor Green

# Load Dialogues CSV
$dialogues = Import-Csv (Join-Path $DataPath "Dialogues.csv")

# Extract unique TextKeys
$dialogueKeys = $dialogues.TextKey | Where-Object { $_ -ne '' } | Select-Object -Unique | Sort-Object

Write-Host "Found $($dialogueKeys.Count) dialogue keys"

# Generate dialogue translations
foreach ($key in $dialogueKeys) {
    # Smart translation based on key patterns
    $vn = ""
    $en = ""
    
    # Parse key structure
    if ($key -match "DIALOGUE_(\w+)_(.+)") {
        $speaker = $matches[1]
        $context = $matches[2]
        
        # Context-based translation
        switch -Regex ($context) {
            "DAY\d+_MORNING" { 
                $vn = "Sáng ngày $($context.Substring(3,1)). Mika nhìn em."
                $en = "Day $($context.Substring(3,1)) morning. Mika looks at you."
            }
            "IDLE" {
                $vn = "..."
                $en = "..."
            }
            "ENDING" {
                $vn = "Đã đến lúc quyết định..."
                $en = "Time to decide..."
            }
            default {
                $vn = "[TODO: $key]"
                $en = "[TODO: $key]"
            }
        }
    }
    else {
        $vn = "[TODO: $key]"
        $en = "[TODO: $key]"
    }
    
    # Escape commas
    if ($vn -match ",") { $vn = "`"$vn`"" }
    if ($en -match ",") { $en = "`"$en`"" }
    
    "$key,$vn,$en" | Out-File $outputFile -Append -Encoding UTF8
}

Write-Host "`n🎯 Processing Quests.csv..." -ForegroundColor Green

# Load Quests
$quests = Import-Csv (Join-Path $DataPath "Quests.csv")

foreach ($quest in $quests) {
    if ($quest.QuestNameKey -ne '') {
        # Quest name
        $nameKey = $quest.QuestNameKey
        $vn = $quest.QuestID -replace "QUEST_", "" -replace "_", " "
        $en = $vn
        "$nameKey,`"$vn`",`"$en`"" | Out-File $outputFile -Append -Encoding UTF8
        
        # Quest description  
        if ($quest.DescriptionKey -ne '') {
            $descKey = $quest.DescriptionKey
            "$descKey,`"[Mô tả quest]`",`"[Quest description]`"" | Out-File $outputFile -Append -Encoding UTF8
        }
    }
}

Write-Host "`n✅ Generation complete!" -ForegroundColor Green
Write-Host "Output: $outputFile" -ForegroundColor Cyan
Write-Host "`n⚠️  NEXT STEPS:" -ForegroundColor Yellow
Write-Host "1. Review generated file"
Write-Host "2. Replace [TODO] entries with proper translations"
Write-Host "3. Rename to Localization_VN.csv when ready"

$lineCount = (Get-Content $outputFile | Measure-Object -Line).Lines
Write-Host "`nTotal entries: $lineCount" -ForegroundColor Magenta
