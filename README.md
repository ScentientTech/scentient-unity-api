# Scentient Peripheral Unity Android API

## Requirements

## Installation

## Runtime Permissions

Android has undergone a lot of changes to it's Bluetooth permissions. Here is a short summary of what permissions are required, and whether they need to request permission at runtime.

| Android OS           | < Android 6.0<br>API level 23<br>                                                                       | \>= Android 6.0<br>API level 23<br>                                                                     | \>= Android 12<br>API Level 31                                     |
| -------------------- | ------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| Permission           | android.permission.BLUETOOTH android.permission.BLUETOOTH_ADMIN android.permission.ACCESS_FINE_LOCATION | android.permission.BLUETOOTH android.permission.BLUETOOTH_ADMIN android.permission.ACCESS_FINE_LOCATION | android.permission.BLUETOOTH_CONNECT android.permission.BLUETOOTH_SCAN |
| Notes                |                                                                                                         | Must request runtime permissions                                                                        | Permissions required for BLE changed                                   |
| Devices ( 01/05/24 ) |                                                                                                         |                                                                                                         | OnePlus Nord, Meta Quest 2                                             |
| Target build api     |                                                                                                         |                                                                                                         |                                                                        |
| < API level 23       |                                                                                                         |                                                                                                         |                                                                        |
| \>=API level 23      |                                                                                                         |                                                                                                         |                                                                        |
| \>=API level 31      |                                                                                                         |                                                                                                         |                                                                        |


## Usage


