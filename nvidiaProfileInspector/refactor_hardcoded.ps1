$files = Get-ChildItem -Path UI\Styles -Filter *.xaml -Recurse

$replacements = @{
    '="#15FFFFFF"' = '="{DynamicResource HighlightOverlayBrush}"'
    '="#1AFFFFFF"' = '="{DynamicResource HoverOverlayBrush}"'
    '="#10FFFFFF"' = '="{DynamicResource HighlightOverlayBrush}"'
    '="#25FFFFFF"' = '="{DynamicResource PressedOverlayBrush}"'
    '="#40FFFFFF"' = '="{DynamicResource ScrollBarThumbBrush}"'
    '="#60FFFFFF"' = '="{DynamicResource ScrollBarThumbHoverBrush}"'
    '="#80FFFFFF"' = '="{DynamicResource ScrollBarThumbPressedBrush}"'
    '="#1FFFFFFF"' = '="{DynamicResource HoverOverlayBrush}"'
    '="#11FFFFFF"' = '="{DynamicResource HighlightOverlayBrush}"'

    '="#3D2B2B"' = '="{DynamicResource ApplicationRemoveBrush}"'
    '="#154CC331"' = '="{DynamicResource ApplicationAddBrush}"'
    '="#254CC331"' = '="{DynamicResource ApplicationAddHoverBrush}"'

    '="#252525"' = '="{DynamicResource ListViewAlternatingRowBrush}"'
    '="#2A2A20"' = '="{DynamicResource ModifiedSettingBackgroundBrush}"'

    '="#1C1C1C"' = '="{DynamicResource WindowBackgroundBrush}"'
    '="#202020"' = '="{DynamicResource Layer1BackgroundBrush}"'
    '="#282828"' = '="{DynamicResource Layer2BackgroundBrush}"'
    '="#323232"' = '="{DynamicResource Layer3BackgroundBrush}"'
    '="#1A1A1A"' = '="{DynamicResource HoverOverlayBrush}"'
    '="#2A2A2A"' = '="{DynamicResource Layer2BackgroundBrush}"'
    '="#3D3D3D"' = '="{DynamicResource ControlBorderBrush}"'
    '="#444444"' = '="{DynamicResource ControlBorderHoverBrush}"'
}

foreach ($f in $files) {
    if ($f.Name -match "^(Colors|Icons)\.xaml$") { continue }
    
    $content = Get-Content $f.FullName -Raw
    
    foreach ($r in $replacements.GetEnumerator()) {
        $content = $content.Replace($r.Key, $r.Value)
    }
    
    Set-Content -Path $f.FullName -Value $content -NoNewline
}
