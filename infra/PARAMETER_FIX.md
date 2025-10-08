# Deployment Parameter Issue Fix

## Problem Analysis

The deployment failed with malformed resource names because the PowerShell script was incorrectly constructing the parameter string for Azure CLI. The errors showed resource names like:

- `eshop imageTag=latest environmentName=dev enableZoneRedundancy=False-mi-dev`
- `eshop imageTag=latest environmentName=dev enableZoneRedundancy=Falsestdevqyeocsqs5rffu`

This happened because the script was trying to pass parameters as a concatenated string instead of using the proper parameter file approach.

## Root Cause

The issue was in `deploy.ps1` at line 190:
```powershell
$paramString = ($deploymentParams.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join " "
```

This created an invalid parameter string that was being interpreted as part of the resource names.

## Solution

### 1. Fixed Parameter Passing
- **Before**: Used concatenated parameter string with incorrect syntax
- **After**: Uses the `main.bicepparam` file directly with `--parameters main.bicepparam`

### 2. Simplified Script Logic
- Removed complex parameter building logic
- Uses the existing parameter file for all configuration
- Added warning to edit `main.bicepparam` for parameter changes

### 3. Updated Commands
All Azure CLI commands now use:
```powershell
"--parameters", "main.bicepparam"
```

Instead of:
```powershell
"--parameters", $paramString
```

## Files Modified

### `deploy.ps1`
- Fixed parameter passing to use `main.bicepparam` file
- Removed complex parameter building logic
- Simplified deployment summary
- Added guidance to edit parameter file

### `main.bicep` (Previous Fix)
- Fixed Key Vault naming to stay within 24-character limit
- Uses: `${take(namePrefix, 8)}kv${take(environmentName, 3)}${take(uniqueString(resourceGroup().id), 10)}`

## Usage

The deployment script now properly uses the parameter file:

```powershell
.\deploy.ps1 -ResourceGroup "rg-eshop-dev" -Environment "dev"
```

To customize deployment parameters, edit the `main.bicepparam` file instead of passing command-line arguments.

## Benefits

1. **Correct Parameter Handling**: Azure CLI receives properly formatted parameters
2. **Simplified Script**: Reduced complexity and potential for errors
3. **Consistent Configuration**: All parameters managed in one file
4. **Better Maintainability**: Clear separation between script logic and configuration

The deployment should now succeed without malformed resource name errors.