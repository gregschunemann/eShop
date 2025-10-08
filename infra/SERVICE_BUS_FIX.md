# Service Bus Namespace Deployment Fix

## Issue
The Service Bus namespace deployment failed with a "Bad Request" error with no additional details.

## Root Cause
The Service Bus namespace configuration included properties that are only valid for Premium SKU:
- `premiumMessagingPartitions: 0`
- `zoneRedundant: (environmentName == 'prod' && enableZoneRedundancy)`

However, the template was using Basic/Standard SKUs, which don't support these properties, causing the Azure Resource Manager to reject the deployment with a "Bad Request" error.

## Solution
**Removed** the incompatible properties from the Service Bus namespace configuration:

### Before (Incorrect):
```bicep
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: resourceNames.serviceBus
  location: location
  sku: {
    name: environmentName == 'prod' ? 'Standard' : 'Basic'
    tier: environmentName == 'prod' ? 'Standard' : 'Basic'
  }
  properties: {
    minimumTlsVersion: '1.2'
    premiumMessagingPartitions: 0      // ❌ Only valid for Premium
    zoneRedundant: (environmentName == 'prod' && enableZoneRedundancy)  // ❌ Only valid for Premium
  }
}
```

### After (Correct):
```bicep
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: resourceNames.serviceBus
  location: location
  sku: {
    name: environmentName == 'prod' ? 'Standard' : 'Basic'
    tier: environmentName == 'prod' ? 'Standard' : 'Basic'
  }
  properties: {
    minimumTlsVersion: '1.2'
    // Note: premiumMessagingPartitions and zoneRedundant are only valid for Premium SKU
    // Basic and Standard SKUs don't support these properties
  }
}
```

## SKU Compatibility Matrix

| Property | Basic SKU | Standard SKU | Premium SKU |
|----------|-----------|--------------|-------------|
| `minimumTlsVersion` | ✅ | ✅ | ✅ |
| `premiumMessagingPartitions` | ❌ | ❌ | ✅ |
| `zoneRedundant` | ❌ | ❌ | ✅ |

## Future Considerations
If you need zone redundancy or premium messaging partitions, consider upgrading to Premium SKU for production environments:

```bicep
sku: {
  name: environmentName == 'prod' ? 'Premium' : 'Basic'
  tier: environmentName == 'prod' ? 'Premium' : 'Basic'
  capacity: environmentName == 'prod' ? 1 : null  // Premium requires capacity
}
properties: {
  minimumTlsVersion: '1.2'
  premiumMessagingPartitions: environmentName == 'prod' ? 1 : null
  zoneRedundant: (environmentName == 'prod' && enableZoneRedundancy)
}
```

## References
- [Azure Service Bus SKU Comparison](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-premium-messaging)
- [Service Bus Namespace ARM Template Reference](https://docs.microsoft.com/en-us/azure/templates/microsoft.servicebus/namespaces)