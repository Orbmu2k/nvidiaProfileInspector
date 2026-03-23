$files = Get-ChildItem -Path UI -Filter *.xaml -Recurse

$brushes = @(
    'WindowBackgroundBrush', 'Layer1BackgroundBrush', 'Layer2BackgroundBrush', 'Layer3BackgroundBrush',
    'NvidiaGreenBrush', 'NvidiaGreenHoverBrush', 'NvidiaGreenPressedBrush',
    'AccentBrush', 'UserAccentBrush',
    'TextPrimaryBrush', 'TextBrush', 'TextSecondaryBrush', 'TextDisabledBrush',
    'ControlBorderBrush', 'ControlBorderHoverBrush', 'AccentBorderBrush',
    'SuccessBrush', 'ErrorBrush', 'WarningBrush', 'QuestionBrush', 'LabBrush',
    'WindowBackgroundColor', 'Layer1BackgroundColor', 'Layer2BackgroundColor', 'Layer3BackgroundColor',
    'TextPrimaryColor', 'TextSecondaryColor', 'TextDisabledColor',
    'ControlBorderColor', 'ControlBorderHoverColor'
)

foreach ($f in $files) {
    if ($f.Name -match "^(Colors|LightTheme|DarkTheme|Icons)\.xaml$") { continue }
    
    $content = Get-Content $f.FullName -Raw
    
    foreach ($b in $brushes) {
        $content = $content -replace "\{StaticResource $b\}", "{DynamicResource $b}"
    }
    
    Set-Content -Path $f.FullName -Value $content -NoNewline
}
