# Scentient Escents Unity API

## Requirements

* Unity 2020.3 and later.
* This api is designed for Android devices, such as portable VR headsets and Android phones/tablets.
* They control the Escents Peripheral over Bluetooth Low Energy. Therefore BLE must be enabled on the target device.

## Installation

For the main menu in Unity, select Window -> "Package Manager" -> "+" Button in Top Left -> "Add Package From Git Url"

The third party "Unity Android Bluetooth Low Energy" should also be installed as a dependancy.

## Android Runtime Permissions

Android has undergone a lot of changes to it's Bluetooth permissions. Here is a short summary of what permissions are required, and whether they need to request permission at runtime.

| Android OS           | < Android 6.0<br>API level 23<br>                                                                       | \>= Android 6.0<br>API level 23<br>                                                                     | \>= Android 12<br>API Level 31                                     |
| -------------------- | ------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| Permission           | android.permission.BLUETOOTH android.permission.BLUETOOTH_ADMIN android.permission.ACCESS_FINE_LOCATION | android.permission.BLUETOOTH android.permission.BLUETOOTH_ADMIN android.permission.ACCESS_FINE_LOCATION | android.permission.BLUETOOTH_CONNECT android.permission.BLUETOOTH_SCAN |
| Notes                |                                                                                                         | Must request runtime permissions                                                                        | Permissions required for BLE changed                                   |
| Example Devices ( 01/05/24 ) |                                                                                                         |                                                                                                         | OnePlus Nord, Meta Quest 2                                             |
| Target build api     |                                                                                                         |                                                                                                         |                                                                        |

## Usage

Add [ScentientDevice](@ref Scentient::ScentientDevice) component to a GameObject in your scene. 

To trigger scents via UnityEvents, or from your own scripts, try the [EmitScent](@ref Scentient::EmitScent) helper component, which lets you serialize scent name, intensity and duration in your scene.

### Sample Scene

A VR Sample Scene is included in the Package, visit the Package in the Package Manager, and import the VR Sample Scene.

Make sure you have checked the Bluettoth setup wizard ( Window -> "Android Bluetooth Low Energy Library" ), to make sure your Android Bluetooth permissions are set up correctly for the API target. This is part of the third party unity-android-bluetooth-low-energy library.

