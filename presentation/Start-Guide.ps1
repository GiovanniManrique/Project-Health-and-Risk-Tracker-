param([int]$Port = 8765)
$ErrorActionPreference = 'Stop'
$guideUrl = "http://127.0.0.1:$Port"
try {
    $existingGuide = Invoke-RestMethod -Uri "$guideUrl/health" -TimeoutSec 2
    if ($existingGuide.app -eq 'ProjectHealthTracker teaching guide' -and $existingGuide.version -eq 3) {
        Write-Output "Guide already running: $guideUrl/"
        return
    }
    throw "Port $Port is serving something else. Choose a different -Port value."
} catch {
    if ($_.Exception.Message -like 'Port * is serving something else*') { throw }
}

$bundledPython = Join-Path $env:USERPROFILE '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
if (Test-Path -LiteralPath $bundledPython) {
    $guidePython = $bundledPython
} else {
    $pythonCommand = Get-Command python -ErrorAction SilentlyContinue
    if (-not $pythonCommand) { throw 'Python was not found. Install Python 3 or use an available runtime to run serve_guide.py.' }
    $guidePython = $pythonCommand.Source
}

$serverScript = Join-Path $PSScriptRoot 'serve_guide.py'
$logsDirectory = Join-Path (Split-Path -Parent $PSScriptRoot) 'artifacts'
New-Item -ItemType Directory -Path $logsDirectory -Force | Out-Null
$serverArgs = @('-u', ('"{0}"' -f $serverScript), '--port', [string]$Port)
$serverProcess = Start-Process -FilePath $guidePython -ArgumentList $serverArgs -WindowStyle Hidden -PassThru `
    -RedirectStandardOutput (Join-Path $logsDirectory "guide-$Port.stdout.log") `
    -RedirectStandardError (Join-Path $logsDirectory "guide-$Port.stderr.log")
for ($attempt = 0; $attempt -lt 15; $attempt++) {
    Start-Sleep -Milliseconds 200
    try {
        $guideHealth = Invoke-RestMethod -Uri "$guideUrl/health" -TimeoutSec 1
        if ($guideHealth.app -eq 'ProjectHealthTracker teaching guide' -and $guideHealth.version -eq 3) {
            Write-Output "Guide running: $guideUrl/ (process $($serverProcess.Id))"
            return
        }
    } catch { }
    if ($serverProcess.HasExited) { break }
}
throw "The guide did not start on port $Port. Read artifacts\guide-$Port.stderr.log or choose a different -Port value."
