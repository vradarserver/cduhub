# Legacy Namespace

These are interfaces and classes that have evolved into something else,
but the old versions have been retained so that people's stuff doesn't
break.

The classes that have been replaced rather than retired are marked with
Obsolete attributes that point off to the replacement.

Nothing has a namespace of Legacy, you need to look at each module to
see the actual namespace.



## V2 Changes

The following legacy interfaces, enums and classes were removed in V2:

* `IMcdu`
* `McduFactory`
* `ProductId`

The following V1 interfaces, enums and classes were made obsolete in V2:

| Retired            | Replaced By |
| ---                | --- |
| `CduFactory`       | `DeviceFactory` |
| `Device`           | `AircraftFamily` and `EquipmentType` |
| `DeviceIdentifier` | `UsbDevice` |
| `DeviceType`       | `AircraftFamily` and `EquipmentType` |
| `DeviceUser`       | `EquipmentLocation` |
