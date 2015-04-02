param($installPath, $toolsPath, $package, $project)

$platformNames = "x86", "x64"
$propertyName = "CopyToOutputDirectory"

foreach($platformName in $platformNames) {
  $folder = $project.ProjectItems.Item($platformName)

  if ($folder -eq $null) {
    continue
  }

  $fileName = "nanomsg.dll"
  $item = $folder.ProjectItems.Item($fileName)
  if ($item -eq $null) {
    continue
  }
  $property = $item.Properties.Item($propertyName)
  if ($property -eq $null) {
    continue
  }
  $property.Value = 1
  
  $fileName = "libnanomsg.so"
  $item = $folder.ProjectItems.Item($fileName)
  if ($item -eq $null) {
    continue
  }
  $property = $item.Properties.Item($propertyName)
  if ($property -eq $null) {
    continue
  }
  $property.Value = 1
}
